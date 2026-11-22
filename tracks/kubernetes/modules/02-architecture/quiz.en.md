---
title: "Quiz: Kubernetes architecture"
lang: en
track: kubernetes
kind: module
doc: quiz
slug: 02-architecture
part: 1
tags: [kubernetes, architecture, control-plane, etcd, scheduler, controller]
status: draft
updated: 2026-11-22
---

# Quiz: Kubernetes architecture

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 Which component is the only one that talks to etcd?

- A) kube-scheduler
- B) kubelet
- C) kube-apiserver
- D) kube-controller-manager

<details><summary>Answer</summary>

**C.** All other components and clients work through the API server, which validates the request and reads or writes etcd. Direct access to etcd bypasses authorization and validation.
→ [Basics](basics.en.md#what-a-cluster-consists-of)

</details>

## 2. 🟢 Who starts the pod's containers on a node?

- A) kube-scheduler
- B) the kubelet through a container runtime
- C) kube-proxy
- D) etcd

<details><summary>Answer</summary>

**B.** The scheduler only writes the chosen node into the pod, kube-proxy sets up network rules for Services, etcd stores the state. The containers are created by the kubelet, which calls the runtime (containerd in our cluster) over the CRI interface.
→ [Basics](basics.en.md#what-happens-when-a-deployment-is-created)

</details>

## 3. 🟢 You deleted one pod of a 2-replica Deployment. What happens?

- A) Nothing, one pod remains
- B) The ReplicaSet creates a new pod
- C) The Deployment is deleted
- D) The kubelet restarts the deleted pod

<details><summary>Answer</summary>

**B.** The ReplicaSet compares the desired number of replicas (2) with the actual (1) and creates the missing pod. In the experiment the new pod appeared with an age of 3 seconds. The kubelet does not restore a deleted pod (D is wrong), the Deployment is not deleted (C is wrong).
→ [Basics](basics.en.md#self-healing)

</details>

## 4. 🟢 Where do the control plane components run in a kind cluster?

- A) As ordinary processes on your laptop
- B) As static pods on the node, from manifests in `/etc/kubernetes/manifests`
- C) In the cloud
- D) In a separate VirtualBox virtual machine

<details><summary>Answer</summary>

**B.** In kind a node is a Docker container; the kubelet inside it starts `etcd`, `kube-apiserver`, `kube-controller-manager` and `kube-scheduler` as static pods from the four yaml files of this folder.
→ [Basics](basics.en.md#what-is-visible-in-kind)

</details>

## 5. 🟡 The scheduler is stopped and you raised `replicas` from 2 to 3. What do you see?

- A) Three `Running` pods
- B) Two `Running` pods and one `Pending`, with no `FailedScheduling` events
- C) An error from `kubectl scale`
- D) Two `Running` pods and one `CrashLoopBackOff`

<details><summary>Answer</summary>

**B.** Controllers create the pod, but nobody assigns a node. `FailedScheduling` events are emitted by the scheduler itself, so there are none. After the scheduler returned, the pod became `Running`. `kubectl scale` succeeds: the API server and etcd work.
→ [Advanced](advanced.en.md#what-happens-when-a-component-is-unavailable)

</details>

## 6. 🟡 What does this command print if the object changed after reading?

```bash
kubectl -n learn-02 get deploy web -o json \
  | sed 's/"resourceVersion": "[0-9]*"/"resourceVersion": "1"/' \
  | kubectl replace -f -
```

- A) The object is replaced without errors
- B) `Error from server (Conflict): … the object has been modified; please apply your changes to the latest version and try again`
- C) `NotFound`
- D) `Forbidden`

<details><summary>Answer</summary>

**B.** The version `resourceVersion: "1"` is stale and the server rejects the update: this is how optimistic locking works. The client must re-read the object and repeat.
→ [Advanced](advanced.en.md#optimistic-locking)

</details>

## 7. 🟡 Select all correct statements

- [ ] A) The controller-manager is stopped: self-healing does not work, a Deployment's `READY` may be stale
- [ ] B) If the API server is stopped, already running containers stop immediately
- [ ] C) The owner chain of a pod: Deployment → ReplicaSet → Pod
- [ ] D) A request passes the stages: authentication, authorization, admission, validation, writing to etcd

<details><summary>Answer</summary>

**A, C, D.** A is confirmed by the experiment (`3/3` with two pods). C is shown through `ownerReferences`. D is the request path. B is wrong: containers are run by the kubelet and the runtime and keep running; without the API server you cannot make changes.
→ [Advanced](advanced.en.md)

</details>

## 8. 🟡 What does `kubectl -n kube-system get lease` show?

- A) A list of certificates
- B) `Lease` objects, including who is the current leader for `kube-controller-manager` and `kube-scheduler`
- C) A list of nodes
- D) The audit log

<details><summary>Answer</summary>

**B.** The copies of the controller-manager and scheduler compete for a `Lease`; `HOLDER` is the current leader. The `apiserver-…` row is a record of an API server instance.
→ [Advanced](advanced.en.md#leader-election)

</details>

## 9. 🔴 Find the flaw in the reasoning

> "I set `spec.nodeName: worker-1` in the pod, so the scheduler checked that `worker-1` has enough resources and no conflicts with taints."

<details><summary>Answer</summary>

The reasoning is wrong: with `nodeName` set the pod bypasses the scheduler. In the experiment such a pod has no `Scheduled` event from `default-scheduler`, only kubelet events. Resource, taint and affinity checks are not performed, and the kubelet starts the pod at once. To choose a node use `nodeSelector`, affinity and tolerations (module 18 "Pod scheduling").
→ [Expert](expert.en.md#gotcha-nodename-bypasses-the-scheduler)

</details>

## 10. 🔴 Open question

A ConfigMap is deleted with `kubectl delete`, the command answers "deleted", but `kubectl get` still shows the object with a non-empty `deletionTimestamp`. Explain what is happening and how to act.

<details><summary>Key points of a good answer</summary>

- `delete` set `deletionTimestamp`, but `metadata.finalizers` has an entry: the object stays `Terminating` until the list is empty.
- A finalizer is usually removed by a controller after cleaning up external resources; first find and fix it.
- If there is no controller or it was deleted, you can do it by hand: `kubectl patch … -p '{"metadata":{"finalizers":null}}'`, but only after making sure external resources need no cleanup.
- In the experiment the object vanished right after the finalizer was removed.

</details>

→ [Expert](expert.en.md#gotcha-an-object-is-stuck-in-terminating)
