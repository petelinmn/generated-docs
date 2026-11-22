---
title: "Exercises: storage"
lang: en
track: kubernetes
kind: module
doc: exercises
slug: 08-storage
part: 1
tags: [persistentvolume, persistentvolumeclaim, storageclass, reclaim-policy, access-modes, emptydir]
status: draft
updated: 2026-11-22
---

# Exercises: storage

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. Solutions are under spoilers. You need a kind cluster and a namespace: `kubectl create namespace learn-08`. Clean up afterwards: `kubectl delete namespace learn-08`, then delete the remaining PVs and classes created in the exercises.

## 1. 🟢 Data survives the pod

**Task:** create a 1 GiB PVC and a pod that writes a file to the mounted volume. Check that the PVC is `Pending` before the pod is created. Delete the pod, start another with the same PVC and read the file.

**Expected result:** `Pending` → `Bound`; the second pod shows the same file.

<details><summary>Hint</summary>

The default class (`standard`) waits for the first consumer. Write the file with `echo … > /data/file.txt`.

</details>

<details><summary>Solution</summary>

```yaml
apiVersion: v1
kind: PersistentVolumeClaim
metadata: {name: data}
spec:
  accessModes: [ReadWriteOnce]
  resources: {requests: {storage: 1Gi}}
---
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

```bash
kubectl -n learn-08 apply -f pvc.yaml            # PVC only: Pending
kubectl -n learn-08 apply -f writer.yaml         # now Bound
kubectl -n learn-08 exec writer -- cat /data/file.txt
kubectl -n learn-08 delete pod writer --now
kubectl -n learn-08 apply -f reader.yaml         # the same PVC, without writing
kubectl -n learn-08 exec reader -- cat /data/file.txt
```

```text
data   Pending   …   standard
data   Bound     pvc-eac81e8c-…   1Gi   RWO   standard
persisted-1795372305
persisted-1795372305
```

The file lies in the PV, not in the pod: it survived the pod replacement.

</details>

## 2. 🟢 Where the data lies and what happens when a PVC is deleted

**Task:** find the PV data directory on the kind node (`kubectl get pv … -o jsonpath='{.spec.hostPath.path}'`), read the file right on the node. Then delete the pod and the PVC and check that the PV and the directory disappeared. Why?

**Expected result:** before deletion the file is visible on the node; after — `No resources found`, the directory is empty.

<details><summary>Hint</summary>

Open the node through `docker exec learn-control-plane`. The reclaim policy is visible in `kubectl get pv`.

</details>

<details><summary>Solution</summary>

```bash
kubectl get pv -o custom-columns=NAME:.metadata.name,RECLAIM:.spec.persistentVolumeReclaimPolicy
docker exec learn-control-plane sh -c "cat /var/local-path-provisioner/*/file.txt"
kubectl -n learn-08 delete pvc data --wait=false     # Terminating: the pod still uses the PVC
kubectl -n learn-08 delete pod reader --now
kubectl -n learn-08 get pvc; kubectl get pv
docker exec learn-control-plane sh -c "ls /var/local-path-provisioner/"
```

```text
persisted-1795372305
["kubernetes.io/pvc-protection"]      # the finalizer on the PVC while the pod is alive
No resources found in learn-08 namespace.
No resources found
                                      # the directory is empty
```

A dynamically provisioned PV has the `Delete` policy: after the PVC is deleted the provisioner removes both the PV and the directory. While the pod used the PVC, the finalizer `kubernetes.io/pvc-protection` kept it in `Terminating`.

</details>

## 3. 🟡 Retain and returning a PV

**Task:** create a StorageClass `retain-sc` (provisioner `rancher.io/local-path`, `reclaimPolicy: Retain`, `WaitForFirstConsumer`), write data through a pod, delete the pod and the PVC. Show the PV status and that the data is intact, then return the PV to the `Available` state.

**Expected result:** the PV `Retain` / `Released`, the file `keep-me` on the node; after editing `claimRef` — `Available`.

<details><summary>Hint</summary>

`kubectl patch pv <name> --type=json -p '[{"op":"remove","path":"/spec/claimRef"}]'`.

</details>

<details><summary>Solution</summary>

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
$ kubectl patch pv pvc-2184b427-… --type=json -p '[{"op":"remove","path":"/spec/claimRef"}]'
$ kubectl get pv pvc-2184b427-…
pvc-2184b427-…   Available
```

`Retain` keeps the PV and data after the PVC is deleted; the `claimRef` reference does not let a new PVC bind until it is removed. The data is not cleaned in the process.

</details>

## 4. 🟡 A static PV and binding

**Task:** create a PV `static-pv` of 500 MiB from the node directory `/mnt/static-data` (`hostPath`, `storageClassName: ""`, `Retain`) and a 200 MiB PVC with `storageClassName: ""`. Check how much the PVC got. Then create a 5 GiB PVC and explain its state.

**Expected result:** the PVC `Bound`, `500Mi`; the second PVC `Pending` with `FailedBinding`.

<details><summary>Hint</summary>

Create the node directory with `docker exec learn-control-plane mkdir -p /mnt/static-data`. Events — `kubectl describe pvc`.

</details>

<details><summary>Solution</summary>

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
```

```text
static-claim Bound static-pv 500Mi
too-big Pending
  Normal  FailedBinding  persistentvolume-controller  no persistent volumes available for this claim and no storage class is set
```

The 200 MiB PVC got the whole PV (500 MiB): binding is whole. For 5 GiB there is no suitable PV, and no class is set, so there is no dynamic provisioning either: the PVC stays `Pending`.

</details>

## 5. 🔴 RWOP, expansion and hostPath

**Task:** (a) create a PVC with `ReadWriteOncePod` and two pods that use it; explain the state of the second. (b) Try to grow a bound PVC of the `standard` class. (c) Run a pod with `hostPath: /etc/kubernetes` (`readOnly`) and show what it sees, plus an `emptyDir` with `medium: Memory`.

**Expected result:** (a) the second pod `Pending` (`FailedScheduling`); (b) `Forbidden`; (c) `admin.conf` and a 16 MiB `tmpfs`.

<details><summary>Hint</summary>

(a) The access mode is `ReadWriteOncePod`. (b) `kubectl patch pvc … requests.storage`. (c) `emptyDir: {medium: Memory, sizeLimit: 16Mi}`.

</details>

<details><summary>Solution</summary>

```text
$ kubectl -n learn-08 get pods first second
first    1/1   Running
second   0/1   Pending
  Warning  FailedScheduling  default-scheduler  0/1 nodes are available: 1 node(s) unavailable due to PersistentVolumeClaim with ReadWriteOncePod access mode already in-use by another pod. …
$ kubectl -n learn-08 patch pvc keep -p '{"spec":{"resources":{"requests":{"storage":"200Mi"}}}}'
Error from server (Forbidden): persistentvolumeclaims "keep" is forbidden: only dynamically provisioned pvc can be resized and the storageclass that provisions the pvc must support resize
$ kubectl -n learn-08 exec vols -- sh -c 'mount | grep " /mem "; ls /host | head -3'
tmpfs on /mem type tmpfs (rw,relatime,size=16384k,noswap)
admin.conf
controller-manager.conf
kubelet.conf
```

`ReadWriteOncePod` allows one pod, so the second is not scheduled. `standard` has `allowVolumeExpansion: false`, expansion is forbidden. `hostPath` gives access to the node's files, including the cluster's `admin.conf`: use it only for system agents and experiments.

</details>
