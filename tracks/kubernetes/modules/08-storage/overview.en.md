---
title: "Storage"
lang: en
track: kubernetes
kind: module
doc: overview
slug: 08-storage
part: 1
levels: [1, 2, 3, 4]
tags: [volume, persistentvolume, persistentvolumeclaim, storageclass, emptydir, hostpath, dynamic-provisioning, reclaim-policy, csi]
versions: "Kubernetes 1.37.0 (kind v0.34, local-path-provisioner), kubectl 1.36.1"
status: draft
updated: 2026-11-22
---

# Storage

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

A container's file system is ephemeral: when a container restarts or a pod is replaced, the data disappears. For data that must live longer Kubernetes gives **volumes**. Simple volumes (`emptyDir`, `hostPath`) live as long as a pod or a node. For durable data three objects are used: a **PersistentVolumeClaim** (an application's request for space), a **PersistentVolume** (the storage itself) and a **StorageClass** (a way to create storage automatically on request).

## Key ideas

- **A volume belongs to a pod, not a container:** it survives a container restart, but an `emptyDir` is deleted with the pod.
- **A PVC is a request, a PV is a resource.** An application asks for space in a PVC and does not know where it is physically; the cluster finds or creates a suitable PV and binds it to the PVC.
- **A StorageClass enables dynamic provisioning.** Without a pre-created PV the cluster creates one itself on a PVC request (in kind — the default class `standard`).
- **`WaitForFirstConsumer`:** a PVC stays `Pending` until a pod uses it; the storage is created next to the pod's node.
- **`reclaimPolicy`** decides the data's fate after the PVC is deleted: `Delete` (the PV and data are deleted) or `Retain` (the PV goes to `Released`, the data stays).
- **Access modes:** `ReadWriteOnce`, `ReadWriteOncePod` and others: they limit how many nodes or pods may write.
- **PVC protection:** while a pod uses a PVC it cannot be deleted (the finalizer `kubernetes.io/pvc-protection`).

## Minimal example

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
      command: ["sh", "-c", "echo persisted > /data/file.txt; sleep 3600"]
      volumeMounts: [{name: d, mountPath: /data}]
  volumes:
    - name: d
      persistentVolumeClaim: {claimName: data}
```

```text
$ kubectl -n learn-08 get pvc
NAME   STATUS   VOLUME                                     CAPACITY   ACCESS MODES   STORAGECLASS
data   Bound    pvc-eac81e8c-2cc1-4f1c-b4ef-888b1005a130   1Gi        RWO            standard
```

## When to use and when not

- ✅ Databases, queues, user uploads — any data that must outlive a pod.
- ✅ `emptyDir` — a cache and temporary files, a shared directory between a pod's containers.
- ❌ Do not keep state in the container file system or in `emptyDir` if it must not be lost.
- ❌ `hostPath` on production clusters is a dangerous node binding and access to the node's files; acceptable for system agents and experiments.
- ❌ An application with state and replicas should run as a StatefulSet with `volumeClaimTemplates` (module 12), not as a Deployment with one PVC.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | volume types, PVC and dynamic provisioning, `WaitForFirstConsumer`, data durability, deleting a PVC and protection |
| [2. Advanced](advanced.en.md) | static PVs and binding rules, `reclaimPolicy`, StorageClass, access modes, in-memory `emptyDir`, `hostPath`, volume expansion |
| [3. Expert](expert.en.md) | the PV lifecycle, binding and CSI, a reference of fields, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) with solutions |
| 5. Runnable examples | not generated yet — `/learn kubernetes 08-storage level 5` |
| 6. Tests | not generated yet — `/learn kubernetes 08-storage level 6` |

## Related materials

- Prerequisites: [04 · Pod](../04-pods/overview.en.md), [07 · ConfigMap and Secret](../07-config-and-secrets/overview.en.md)
- Next: [09 · Ingress and Gateway API](../09-ingress-and-gateway-api/overview.en.md)
- Foundation: the [Linux track](../../../linux/overview.en.md) — file systems, mounting, permissions
- The same idea in other tracks: [Testcontainers for C#](../../../csharp/libraries/testcontainers/overview.en.md) — volumes and container state in tests
- StatefulSet and a PVC per replica: [12 · Other workloads](../12-workloads/overview.en.md)
- Troubleshooting: [21 · Troubleshooting](../21-troubleshooting/overview.en.md)
- [Linux 02 · Filesystem](../../../linux/modules/02-filesystem/overview.en.md)
