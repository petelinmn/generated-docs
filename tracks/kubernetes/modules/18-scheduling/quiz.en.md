---
title: "Quiz: pod scheduling"
lang: en
track: kubernetes
kind: module
doc: quiz
slug: 18-scheduling
part: 3
tags: [scheduler, taints, affinity, priorityclass]
status: draft
updated: 2026-11-23
---

# Quiz: pod scheduling

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 A pod with `nodeSelector: {disk: nvme}` is `Pending`, and no node has that label. Where do you look for the reason?

- A) `kubectl logs`
- B) The pod's events: `FailedScheduling` in `kubectl describe pod`
- C) The kubelet logs
- D) `kubectl top`

<details><summary>Answer</summary>

**B.** There is no container yet, so no logs; the scheduler lists the reason in an event (`0/3 nodes are available: … didn't match Pod's node affinity/selector`).
→ [Basics](basics.en.md#how-the-scheduler-chooses-a-node)

</details>

## 2. 🟢 How does `preferredDuringSchedulingIgnoredDuringExecution` differ from `required…`?

- A) It does not
- B) `preferred` is a wish with a weight; the pod starts even without a suitable node
- C) `preferred` acts only on running pods
- D) `preferred` works only with taints

<details><summary>Answer</summary>

**B.** In the experiment the pod with the wish `disk=nvme` (no such node) started on `learn-worker`.
→ [Basics](basics.en.md#nodeaffinity)

</details>

## 3. 🟢 What does the taint `dedicated=gpu:NoSchedule` do?

- A) Attracts GPU pods to the node
- B) Keeps new pods without a toleration off the node
- C) Evicts all running pods
- D) Forbids the kubelet from starting containers

<details><summary>Answer</summary>

**B.** Running pods stay (only `NoExecute` evicts them); a taint does not attract, affinity/nodeSelector does.
→ [Basics](basics.en.md#taints-and-tolerations)

</details>

## 4. 🟢 A pod has a toleration for `dedicated=gpu`. Will it necessarily land on the GPU node?

- A) Yes
- B) No, a toleration only permits such a node
- C) Yes, if the other nodes are busy
- D) No, tolerations are useless

<details><summary>Answer</summary>

**B.** Pinning is taint + `nodeSelector`/affinity.
→ [Basics](basics.en.md#taints-and-tolerations)

</details>

## 5. 🟡 Three replicas with a `required` podAntiAffinity on `kubernetes.io/hostname` and two worker nodes. What happens?

- A) All three start, two on one node
- B) Two start, the third stays `Pending`
- C) The Deployment cannot be created
- D) The third goes to the control-plane

<details><summary>Answer</summary>

**B.** In the experiment the event says `2 node(s) didn't match pod anti-affinity rules`. On the control-plane the taint gets in the way.
→ [Advanced](advanced.en.md#pod-affinity-and-anti-affinity)

</details>

## 6. 🟡 What does a `NoExecute` taint do to a "bare" pod without a toleration that already runs on the node?

- A) Nothing
- B) Evicts it, and it does not come back
- C) Evicts it, and it restarts on another node
- D) Moves it to `Pending`

<details><summary>Answer</summary>

**B.** In the experiment both "bare" pods vanished (`No resources found`); nobody can restart them, there is no controller.
→ [Advanced](advanced.en.md#noexecute-and-tolerationseconds)

</details>

## 7. 🟡 A Deployment tolerates `maint=true:NoExecute` for 20 seconds. What happens after `kubectl taint node learn-worker maint=true:NoExecute`?

- A) The pods stay forever
- B) Pods on the node are evicted after ~20 s, the replacements return to the same node and are evicted again
- C) The pods move once and stay on another node
- D) The Deployment is deleted

<details><summary>Answer</summary>

**B.** In the experiment the pod name on `learn-worker` changed twice (t≈21 s and t≈42 s). The replacement has the same toleration, and the taint does not stop the scheduler.
→ [Advanced](advanced.en.md#noexecute-and-tolerationseconds)

</details>

## 8. 🟡 Select all correct statements

- [ ] A) `kubectl drain` includes `cordon`
- [ ] B) A PDB can block `drain`
- [ ] C) `drain` evicts DaemonSet pods by default
- [ ] D) `uncordon` returns the node to scheduling

<details><summary>Answer</summary>

**A, B, D.** C is wrong: DaemonSet pods are ignored with the `--ignore-daemonsets` flag (otherwise `drain` refuses to work).
→ [Advanced](advanced.en.md#node-maintenance-cordon-drain-and-pdb)

</details>

## 9. 🟡 Pod `vip` (priority 100000) does not fit, and `low` pods (priority 100) occupy the nodes. What happens?

- A) `vip` will wait
- B) The scheduler preempts a `low` pod, and the victim's replacement waits for space
- C) All `low` pods are deleted for good
- D) A validation error

<details><summary>Answer</summary>

**B.** In the experiment: `Preempted by pod …`, `vip` started, and the new `low` pod is `Pending`.
→ [Advanced](advanced.en.md#priorities-and-preemption)

</details>

## 10. 🔴 Find the flaw

> "Node `learn-worker` is almost idle on CPU, so a pod with `requests: cpu: 10` will find room."

<details><summary>Answer</summary>

The scheduler sums the `requests` of already placed pods, not their actual consumption. If the node is full by `requests`, you get `Insufficient cpu` even when it is idle.
→ [Gotcha 3](expert.en.md#3-the-scheduler-does-not-look-at-actual-consumption)

</details>

## 11. 🔴 `drain` hangs, and `kubectl get pdb` shows `ALLOWED DISRUPTIONS 0`. The cause?

- A) The node is broken
- B) `minAvailable` equals the current replica count, so none can be evicted
- C) The pods have no `requests`
- D) A PDB applies only to `Job`

<details><summary>Answer</summary>

**B.** In the experiment `--min-available=4` with four replicas: `global timeout reached: 15s`.
→ [Gotcha 6](expert.en.md#6-a-pdb-blocks-drain)

</details>

## 12. 🔴 How do you pin pods to dedicated GPU nodes and keep outsiders off them?

<details><summary>Key points of a good answer</summary>

- A taint on the GPU nodes repels everyone without a toleration.
- In the GPU workload pods a toleration permits those nodes, and a `nodeSelector` or nodeAffinity attracts them to exactly those nodes.
- Both rules are needed: a taint without a selector does not attract, a selector without a taint does not protect the nodes from outsiders.

</details>

→ [Basics](basics.en.md#taints-and-tolerations)
