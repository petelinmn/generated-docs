---
title: "Storage: basics"
lang: en
track: kubernetes
kind: module
doc: basics
slug: 08-storage
part: 1
tags: [volume, persistentvolume, persistentvolumeclaim, storageclass, dynamic-provisioning, waitforfirstconsumer, pvc-protection]
versions: "Kubernetes 1.37.0 (kind v0.34, local-path-provisioner), kubectl 1.36.1"
status: draft
updated: 2026-11-22
---

# Storage: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [04 · Pod](../04-pods/overview.en.md) · **Time:** ~2 h

All examples were run in kind (Kubernetes 1.37.0), namespace `learn-08`: `kubectl create namespace learn-08`. kind has a default StorageClass `standard` with the provisioner `rancher.io/local-path`: disks are created as directories on the node.

## Why it matters

Data inside a container disappears when it is recreated: the image is immutable, and the writable layer is deleted with the container ([module 01](../01-containers/basics.en.md#data-the-writable-layer-and-volumes)). Applications with state (databases, user uploads) need a place that outlives the pod.

## Simple volumes

A volume is declared in the pod's `spec.volumes` and mounted into a container through `volumeMounts`. The simplest types:

| Type | What it is | Lifetime |
|---|---|---|
| `emptyDir` | an empty directory, created with the pod | until the pod is deleted |
| `hostPath` | a directory on the node | while the node and directory exist |
| `configMap`, `secret` | configuration ([module 07](../07-config-and-secrets/overview.en.md)) | while the object exists |
| `persistentVolumeClaim` | durable storage | independent of the pod |

`emptyDir` was already seen in [module 04](../04-pods/advanced.en.md#several-containers-in-a-pod) as a shared directory of a pod's containers. It survives a container restart but not a pod replacement.

## PersistentVolumeClaim

A request for space is described by a PVC object:

```yaml
apiVersion: v1
kind: PersistentVolumeClaim
metadata: {name: data}
spec:
  accessModes: [ReadWriteOnce]
  resources: {requests: {storage: 1Gi}}
```

- `accessModes` — how the volume will be used (`ReadWriteOnce` — read and write from one node).
- `resources.requests.storage` — the needed size.
- `storageClassName` is not set: the default StorageClass (`standard`) is used.

### The PVC waits for the first pod

Right after creation the PVC stays `Pending`:

```text
$ kubectl -n learn-08 get pvc
NAME   STATUS    VOLUME   CAPACITY   ACCESS MODES   STORAGECLASS   VOLUMEATTRIBUTESCLASS   AGE
data   Pending                                      standard       <unset>                 3s
$ kubectl -n learn-08 describe pvc data
Status:        Pending
Events:
  Normal  WaitForFirstConsumer  0s (x2 over 3s)  persistentvolume-controller  waiting for first consumer to be created before binding
```

This is normal: the `standard` StorageClass has `volumeBindingMode: WaitForFirstConsumer`. The cluster does not create the storage until a pod uses it, because the place must be chosen with the node the pod will land on in mind. There is no PV yet either (`No resources found`).

## A pod that uses a PVC

```yaml
apiVersion: v1
kind: Pod
metadata: {name: writer}
spec:
  containers:
    - name: c
      image: alpine:3.22
      command: ["sh", "-c", "echo persisted-$(date +%s) > /data/file.txt; sleep 3600"]
      volumeMounts: [{name: d, mountPath: /data}]
  volumes:
    - name: d
      persistentVolumeClaim: {claimName: data}
```

After the pod starts, the cluster creates a PV and binds it to the PVC:

```text
$ kubectl -n learn-08 get pvc
NAME   STATUS   VOLUME                                     CAPACITY   ACCESS MODES   STORAGECLASS
data   Bound    pvc-eac81e8c-2cc1-4f1c-b4ef-888b1005a130   1Gi        RWO            standard
$ kubectl get pv -o custom-columns=NAME:.metadata.name,CAP:.spec.capacity.storage,MODE:.spec.accessModes,RECLAIM:.spec.persistentVolumeReclaimPolicy,SC:.spec.storageClassName,STATUS:.status.phase,CLAIM:.spec.claimRef.name
NAME                                       CAP   MODE              RECLAIM   SC         STATUS   CLAIM
pvc-eac81e8c-2cc1-4f1c-b4ef-888b1005a130   1Gi   [ReadWriteOnce]   Delete    standard   Bound    data
```

The PVC is `Bound`, and it got a PV with an automatically generated name `pvc-<uid>`. A PV is a cluster-level object (no namespace), while a PVC belongs to a namespace.

Where the data lives in kind is visible from the PV:

```text
path=/var/local-path-provisioner/pvc-eac81e8c-2cc1-4f1c-b4ef-888b1005a130_learn-08_data
affinity=kubernetes.io/hostname
```

This is a directory on the node; the PV has a `nodeAffinity` by node name: a pod with this volume will run only on it.

## Data survives the pod

Let us delete the pod and start another with the same PVC (`reader` reads the file):

```text
$ kubectl -n learn-08 exec writer -- cat /data/file.txt
persisted-1795372305
$ kubectl -n learn-08 delete pod writer --now
$ kubectl -n learn-08 apply -f reader.yaml
$ kubectl -n learn-08 exec reader -- cat /data/file.txt
persisted-1795372305
```

The file is in place. It can also be seen on the node:

```text
$ docker exec learn-control-plane sh -c "ls /var/local-path-provisioner/*/; cat /var/local-path-provisioner/*/file.txt"
file.txt
persisted-1795372305
```

## Deleting a PVC

What happens if you delete a PVC while a pod uses it?

```text
$ kubectl -n learn-08 delete pvc data --wait=false
$ kubectl -n learn-08 get pvc data
NAME   STATUS        VOLUME                                     CAPACITY   ACCESS MODES   STORAGECLASS
data   Terminating   pvc-eac81e8c-…                             1Gi        RWO            standard
$ kubectl -n learn-08 get pvc data -o jsonpath='{.metadata.finalizers}'
["kubernetes.io/pvc-protection"]
```

The PVC stays `Terminating`: the finalizer `kubernetes.io/pvc-protection` does not let it be deleted while a pod uses it. After the pod is deleted the PVC disappears, and with it, under the `Delete` policy, the PV and data disappear:

```text
$ kubectl -n learn-08 delete pod reader --now
$ kubectl -n learn-08 get pvc
No resources found in learn-08 namespace.
$ kubectl get pv
No resources found
$ docker exec learn-control-plane sh -c "ls /var/local-path-provisioner/"
                                     # empty: the data directory is deleted
```

> ⚠️ The `Delete` policy (the default for dynamically provisioned volumes) wipes the data together with the PVC. For important data use `Retain` ([advanced](advanced.en.md#reclaim-policy-reclaimpolicy)).

## Beginner mistakes

- ❌ Keeping data in `emptyDir` and expecting it to outlive the pod → ✅ a PVC.
- ❌ Being surprised that a PVC is `Pending` while there is no pod → ✅ it is `WaitForFirstConsumer`; create a pod.
- ❌ Deleting the PVC of important data under the `Delete` policy → ✅ check `reclaimPolicy` and take a backup before deleting.
- ❌ Looking for a PV in a namespace → ✅ a PV is a cluster-level object, it has no namespace.
- ❌ Running a stateful application as a Deployment of several replicas with one PVC → ✅ a StatefulSet (module 12).

## Related materials

- Next: [advanced](advanced.en.md) — static PVs, `Retain`, StorageClass, access modes
- Previous module: [07 · ConfigMap and Secret](../07-config-and-secrets/overview.en.md)
- Pod volumes: [04 · Pod](../04-pods/overview.en.md)
- Foundation: the [Linux track](../../../linux/overview.en.md)

## Sources

- [Volumes](https://kubernetes.io/docs/concepts/storage/volumes/)
- [Persistent Volumes](https://kubernetes.io/docs/concepts/storage/persistent-volumes/)
- [Storage Classes](https://kubernetes.io/docs/concepts/storage/storage-classes/)
- [Dynamic Volume Provisioning](https://kubernetes.io/docs/concepts/storage/dynamic-provisioning/)
- [Configure a Pod to Use a PersistentVolume for Storage](https://kubernetes.io/docs/tasks/configure-pod-container/configure-persistent-volume-storage/)
