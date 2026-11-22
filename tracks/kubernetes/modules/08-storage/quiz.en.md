---
title: "Quiz: storage"
lang: en
track: kubernetes
kind: module
doc: quiz
slug: 08-storage
part: 1
tags: [persistentvolume, persistentvolumeclaim, storageclass, reclaim-policy, access-modes]
status: draft
updated: 2026-11-22
---

# Quiz: storage

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What happens to data in an `emptyDir` when a pod is replaced?

- A) It is kept
- B) It is deleted with the pod
- C) It is copied to the new pod
- D) It moves into a PV

<details><summary>Answer</summary>

**B.** An `emptyDir` is created with the pod and deleted with it (it survives only a container restart). For durable data you need a PVC.
→ [Basics](basics.en.md#simple-volumes)

</details>

## 2. 🟢 A PVC has just been created and is `Pending`, no errors. Why?

- A) Out of space
- B) The StorageClass has `WaitForFirstConsumer`: the PVC waits for a pod that uses it
- C) The PV was forgotten
- D) No permissions

<details><summary>Answer</summary>

**B.** The event: `waiting for first consumer to be created before binding`. After the pod was created the PVC became `Bound`, and the PV was created automatically.
→ [Basics](basics.en.md#the-pvc-waits-for-the-first-pod)

</details>

## 3. 🟢 Which object does not belong to a namespace?

- A) PVC
- B) Pod
- C) PV
- D) ConfigMap

<details><summary>Answer</summary>

**C.** A PersistentVolume is a cluster-level object (like a StorageClass). A PVC, Pod and ConfigMap live in a namespace.
→ [Basics](basics.en.md#a-pod-that-uses-a-pvc)

</details>

## 4. 🟢 What happens to the data if you delete the PVC of a dynamically provisioned volume with the default policy?

- A) It is kept
- B) The PV and data are deleted (`reclaimPolicy: Delete`)
- C) The data stays, but the PV becomes `Released`
- D) The data goes to an archive

<details><summary>Answer</summary>

**B.** In the experiment the node directory (`/var/local-path-provisioner/…`) was deleted with the PV. `Retain` lets you keep the data.
→ [Basics](basics.en.md#deleting-a-pvc)

</details>

## 5. 🟡 A 200 MiB PVC is bound to a 500 MiB static PV. How much does it get?

- A) 200 MiB, the rest is free for others
- B) 500 MiB: binding is whole
- C) An error
- D) 700 MiB

<details><summary>Answer</summary>

**B.** The PVC's `CAPACITY` is `500Mi`. The remainder is not given to other PVCs.
→ [Advanced](advanced.en.md#static-pvs-and-binding-rules)

</details>

## 6. 🟡 What status does a PV with `Retain` have after its PVC is deleted?

- A) `Available`
- B) `Bound`
- C) `Released`
- D) `Deleted`

<details><summary>Answer</summary>

**C.** The data stays. The PV keeps a `claimRef` to the deleted PVC; to make the PV `Available`, the reference is removed.
→ [Advanced](advanced.en.md#reclaim-policy-reclaimpolicy)

</details>

## 7. 🟡 Select all correct statements

- [ ] A) While a pod uses a PVC, deleting it leaves the PVC in `Terminating`
- [ ] B) `ReadWriteOnce` forbids several pods on one node from using the volume
- [ ] C) `ReadWriteOncePod` allows only one pod
- [ ] D) `standard` in kind allows PVC expansion

<details><summary>Answer</summary>

**A, C.** B is wrong: RWO limits nodes, not pods. D is wrong: `allowVolumeExpansion: false`, expansion gives `Forbidden`.
→ [Advanced](advanced.en.md)

</details>

## 8. 🟡 What does `describe pod second` show if two pods use one PVC with `ReadWriteOncePod`?

- A) Both pods work
- B) `FailedScheduling … PersistentVolumeClaim with ReadWriteOncePod access mode already in-use by another pod`
- C) `ImagePullBackOff`
- D) `CrashLoopBackOff`

<details><summary>Answer</summary>

**B.** The second pod stays `Pending`: the scheduler does not allow a second consumer.
→ [Advanced](advanced.en.md#access-modes)

</details>

## 9. 🔴 Find the flaw

> "We ran the database as a 3-replica Deployment with one PVC (`ReadWriteOnce`), so the data is now shared and reliable."

<details><summary>Answer</summary>

RWO allows writing from one node, so replicas on other nodes cannot mount the volume (and on one node several writers to one directory would corrupt the database's data). Stateful applications need a StatefulSet with `volumeClaimTemplates`: a PVC per replica and stable names, while data replication is done by the database itself. Also check `reclaimPolicy`: under `Delete` deleting the PVC wipes the data.
→ [Expert](expert.en.md#gotcha-readwriteonce-is-about-the-node-not-the-pod)

</details>

## 10. 🔴 Open question

Describe what happens between creating a PVC and a file appearing in the container with dynamic provisioning and `WaitForFirstConsumer`. Which events and objects will you see?

<details><summary>Key points of a good answer</summary>

- The PVC is created: `Pending`, the event `waiting for first consumer to be created before binding`.
- A pod with the PVC is created: the scheduler picks a node with the storage in mind.
- The provisioner (`local-path` in kind) creates a directory on the node and a PV `pvc-<uid>` with `nodeAffinity`; the events `Provisioning`, `ExternalProvisioning`.
- The PV controller links the PV and PVC: `Bound`, `claimRef` and `volumeName`.
- The kubelet mounts the volume and starts the container; the file is also visible on the node in `/var/local-path-provisioner/…`.
- On an error — the event `ProvisioningFailed` on the PVC.

</details>

→ [Expert](expert.en.md#the-path-from-a-pvc-to-a-file-in-the-container)
