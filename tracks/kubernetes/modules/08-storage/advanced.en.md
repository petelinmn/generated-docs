---
title: "Storage: advanced"
lang: en
track: kubernetes
kind: module
doc: advanced
slug: 08-storage
part: 1
tags: [persistentvolume, storageclass, reclaim-policy, retain, access-modes, readwriteoncepod, emptydir, hostpath, volume-expansion, static-provisioning]
versions: "Kubernetes 1.37.0 (kind v0.34, local-path-provisioner), kubectl 1.36.1"
status: draft
updated: 2026-11-22
---

# Storage: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on [basics](basics.en.md): PVC, dynamic provisioning, `WaitForFirstConsumer` and deleting a PVC are not repeated.

## Static PVs and binding rules

Besides dynamic provisioning, an administrator can create a PV in advance. An example on a node directory:

```yaml
apiVersion: v1
kind: PersistentVolume
metadata: {name: static-pv}
spec:
  capacity: {storage: 500Mi}
  accessModes: [ReadWriteOnce]
  persistentVolumeReclaimPolicy: Retain
  storageClassName: ""
  hostPath: {path: /mnt/static-data}
---
apiVersion: v1
kind: PersistentVolumeClaim
metadata: {name: static-claim}
spec:
  storageClassName: ""
  accessModes: [ReadWriteOnce]
  resources: {requests: {storage: 200Mi}}
```

```text
$ kubectl -n learn-08 get pvc static-claim
static-claim Bound static-pv 500Mi
$ kubectl get pv static-pv
static-pv 500Mi Bound learn-08/static-claim
```

The empty string `storageClassName: ""` means "no dynamic provisioning: look for a ready PV". The PVC asked for 200 MiB and got the whole 500 MiB PV: a PVC is bound to a PV as a whole, the remainder is not split.

The binding conditions: the StorageClass matches, the access modes fit, the PV capacity is not less than the request. If there is no suitable PV, the PVC stays `Pending`:

```text
$ kubectl -n learn-08 get pvc too-big          # a 5Gi request with a 500Mi PV
too-big Pending
$ kubectl -n learn-08 describe pvc too-big
  Normal  FailedBinding  persistentvolume-controller  no persistent volumes available for this claim and no storage class is set
```

A PVC can be bound to a specific PV with the `volumeName` field.

## Reclaim policy (reclaimPolicy)

What to do with a PV when its PVC is deleted:

| Policy | What happens |
|---|---|
| `Delete` | the PV and data are deleted (the default for dynamically provisioned ones; in the experiment the node directory was removed) |
| `Retain` | the PV stays, status `Released`, the data is kept |

Let us create a StorageClass with `Retain`, write data, delete the pod and the PVC:

```yaml
apiVersion: storage.k8s.io/v1
kind: StorageClass
metadata: {name: retain-sc}
provisioner: rancher.io/local-path
reclaimPolicy: Retain
volumeBindingMode: WaitForFirstConsumer
```

```text
$ kubectl delete pvc keep -n learn-08
$ kubectl get pv pvc-2184b427-… -o custom-columns=NAME:.metadata.name,RECLAIM:.spec.persistentVolumeReclaimPolicy,STATUS:.status.phase,CLAIM:.spec.claimRef.name
NAME               RECLAIM   STATUS     CLAIM
pvc-2184b427-…     Retain    Released   keep
$ docker exec learn-control-plane sh -c "cat /var/local-path-provisioner/pvc-2184b427-…_learn-08_keep/important.txt"
keep-me
```

The PV is `Released`, the data is in place. A new PVC will not bind to such a PV automatically: it still holds a reference to the old PVC (`claimRef`). To put the PV back into service, remove the reference:

```text
$ kubectl patch pv pvc-2184b427-… --type=json -p '[{"op":"remove","path":"/spec/claimRef"}]'
$ kubectl get pv pvc-2184b427-…
NAME               STATUS
pvc-2184b427-…     Available
```

The data is not cleaned in the process: the next PVC will see the old files. The policy of an existing PV is changed by patching `persistentVolumeReclaimPolicy`.

## StorageClass

A StorageClass describes a "kind" of storage and how to create it:

| Field | Meaning |
|---|---|
| `provisioner` | who creates volumes (`rancher.io/local-path`, a cloud driver, a CSI driver) |
| `parameters` | parameters for the provisioner (disk type, zone and so on) |
| `reclaimPolicy` | `Delete` (default) or `Retain` |
| `volumeBindingMode` | `Immediate` or `WaitForFirstConsumer` |
| `allowVolumeExpansion` | whether a PVC size may be increased |
| `mountOptions` | mount options |
| `allowedTopologies` | a restriction by zones and nodes |

The default class is marked with an annotation; in kind it is `standard (default)`. The StorageClass fields were obtained from `kubectl explain storageclass`.

### Immediate and WaitForFirstConsumer

`Immediate` creates a volume at once when the PVC is created, before a pod exists. The `local-path` provisioner in kind cannot work that way: it chooses a directory on the pod's node, and there is no pod yet.

```text
$ kubectl -n learn-08 get pvc imm
imm Pending
$ kubectl -n learn-08 get events --field-selector involvedObject.name=imm
ProvisioningFailed  failed to provision volume with StorageClass "imm-sc": configuration error, no node was specified
```

With `WaitForFirstConsumer` (the `standard` class mode) everything works. In cloud clusters `WaitForFirstConsumer` matters for zonal disks: the volume is created in the same zone where the pod will be.

### Volume expansion

The size of an existing PVC can be increased only if the class allows it (`allowVolumeExpansion: true`) and the volume was provisioned dynamically:

```text
$ kubectl -n learn-08 patch pvc keep -p '{"spec":{"resources":{"requests":{"storage":"200Mi"}}}}'
Error from server (Forbidden): persistentvolumeclaims "keep" is forbidden: only dynamically provisioned pvc can be resized and the storageclass that provisions the pvc must support resize
```

`standard` in kind has `allowVolumeExpansion: false`, so the request was rejected. A PVC cannot be shrunk. Before binding a PVC is almost immutable:

```text
spec is immutable after creation except resources.requests and volumeAttributesClassName for bound claims
```

## Access modes

| Mode | Short | Meaning |
|---|---|---|
| `ReadWriteOnce` | RWO | read and write from one node (several pods on that node may share the volume) |
| `ReadOnlyMany` | ROX | read from many nodes |
| `ReadWriteMany` | RWX | read and write from many nodes |
| `ReadWriteOncePod` | RWOP | read and write from one pod only |

A mode is a requirement on the storage, not a guarantee of enforcement: actual support is determined by the driver. `ReadWriteOncePod` is checked by the scheduler. A second pod with the same PVC does not start:

```text
$ kubectl -n learn-08 get pods first second
first    1/1   Running
second   0/1   Pending
$ kubectl -n learn-08 describe pod second
  Warning  FailedScheduling  default-scheduler  0/1 nodes are available: 1 node(s) unavailable due to PersistentVolumeClaim with ReadWriteOncePod access mode already in-use by another pod. …
```

> ⚠️ Verify: the behavior of `ReadWriteMany` and `ReadOnlyMany` on real drivers (NFS, cloud file systems) was not run in this material.

## Block volumes

`volumeMode: Block` gives a pod a raw block device (`volumeDevices` instead of `volumeMounts`). The `local-path` provisioner does not support block volumes:

```text
ProvisioningFailed  rancher.io/local-path does not support block volume provisioning
```

## emptyDir in memory

An `emptyDir` can be placed in `tmpfs`; its size is limited by `sizeLimit`:

```yaml
volumes:
  - name: mem
    emptyDir: {medium: Memory, sizeLimit: 16Mi}
```

```text
$ kubectl -n learn-08 exec vols -- sh -c 'mount | grep " /mem "; df -h /mem | tail -1'
tmpfs on /mem type tmpfs (rw,relatime,size=16384k,noswap)
tmpfs                    16.0M         0     16.0M   0% /mem
```

Data in memory is faster but counts toward the pod's memory consumption (take it into account when choosing `limits`, module 11) and is lost when the pod is deleted.

## hostPath

`hostPath` mounts a node directory into a container:

```text
$ kubectl -n learn-08 exec vols -- ls /host        # hostPath: /etc/kubernetes
admin.conf
controller-manager.conf
kubelet.conf
```

Any pod with `hostPath` can read the node's files: here it is the kind cluster's admin `admin.conf`. It also gives a binding to a specific node and different behavior on different nodes. It is acceptable for experiments and system agents; for applications use a PVC (or `local` volumes with `nodeAffinity`). The use of `hostPath` is restricted by security policies (module 17).

## Best practices

- For important data use a StorageClass with `Retain` and regular backups: deleting a PVC under `Delete` wipes the data.
- Choose a StorageClass with `allowVolumeExpansion: true` if data growth is likely.
- For zonal disks use `WaitForFirstConsumer`.
- Do not use `hostPath` in applications; restrict it with policies.
- For stateful applications with replicas — a StatefulSet and `volumeClaimTemplates` (module 12).
- Watch for `Released` PVs: under `Retain` they are not freed by themselves.

## Common mistakes

- ❌ Expecting a 200 MiB PVC to take exactly 200 MiB of a static PV → ✅ a PVC is bound to a PV as a whole (it got 500 MiB).
- ❌ Expecting a `Released` PV to become available itself → ✅ you must remove `claimRef` (and clean the data if needed).
- ❌ Trying to shrink or grow a PVC without `allowVolumeExpansion` → ✅ `Forbidden`; you need a class that allows it.
- ❌ Using `Immediate` with a provisioner bound to a node → ✅ `WaitForFirstConsumer`.
- ❌ Mounting `hostPath` "for simplicity" → ✅ a PVC.

## Comparison with other tracks

- [Linux](../../../linux/overview.en.md): a volume is a mount (`mount`); `tmpfs` and `ext4` in the output are ordinary kernel file systems.
- [Docker](../01-containers/basics.en.md#data-the-writable-layer-and-volumes): `docker volume` and a bind mount correspond to a PVC and `hostPath`; Kubernetes adds storage classes and dynamic provisioning.

## Related materials

- Previous level: [basics](basics.en.md)
- Next: [expert](expert.en.md)
- Previous module: [07 · ConfigMap and Secret](../07-config-and-secrets/overview.en.md)

## Sources

- [Persistent Volumes: reclaiming](https://kubernetes.io/docs/concepts/storage/persistent-volumes/#reclaiming)
- [Persistent Volumes: access modes](https://kubernetes.io/docs/concepts/storage/persistent-volumes/#access-modes)
- [Storage Classes](https://kubernetes.io/docs/concepts/storage/storage-classes/)
- [Resizing Persistent Volumes](https://kubernetes.io/docs/concepts/storage/persistent-volumes/#expanding-persistent-volumes-claims)
- [Volumes: emptyDir](https://kubernetes.io/docs/concepts/storage/volumes/#emptydir)
- [Volumes: hostPath](https://kubernetes.io/docs/concepts/storage/volumes/#hostpath)
- [Change the Reclaim Policy of a PersistentVolume](https://kubernetes.io/docs/tasks/administer-cluster/change-pv-reclaim-policy/)
