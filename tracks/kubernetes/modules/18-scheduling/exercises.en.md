---
title: "Exercises: pod scheduling"
lang: en
track: kubernetes
kind: module
doc: exercises
slug: 18-scheduling
part: 3
tags: [scheduler, taints, affinity, priorityclass, drain]
status: draft
updated: 2026-11-23
---

# Exercises: pod scheduling

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself; hints and solutions are under spoilers. You need a kind cluster with at least two worker nodes (`kind create cluster --name learn --config` with a config of one `control-plane` and two `worker` nodes) and a namespace: `kubectl create namespace learn-18`. Afterwards remove the labels and taints and delete the namespace: `kubectl delete namespace learn-18`.

## 1. 🟢 nodeSelector and `Pending`

**Task:** label `learn-worker` with `disk=ssd`. Create a pod with `nodeSelector: {disk: ssd}` and a pod with `nodeSelector: {disk: nvme}`. Explain why the second is `Pending`.

**Expected result:** the first pod on `learn-worker`; the second has a `FailedScheduling` event with `didn't match Pod's node affinity/selector`.

<details><summary>Hint</summary>

`kubectl label node learn-worker disk=ssd`; the reason is in `kubectl describe pod`.

</details>

<details><summary>Solution</summary>

```text
sel-ok   Running   learn-worker
sel-bad  Pending   <none>
  Warning  FailedScheduling  0/3 nodes are available: 1 node(s) had untolerated taint(s), 2 node(s) didn't match Pod's node affinity/selector. …
```

</details>

## 2. 🟢 Taint and toleration

**Task:** put the taint `dedicated=gpu:NoSchedule` on `learn-worker2`. Create two pods tied to this node by a label (`zone=b`): one with a toleration and one without. Compare.

**Expected result:** the pod with the toleration is `Running` on `learn-worker2`, the one without is `Pending` with `2 node(s) had untolerated taint(s)`.

<details><summary>Hint</summary>

`kubectl taint node learn-worker2 dedicated=gpu:NoSchedule`; `nodeSelector: {zone: b}` is needed so that both pods "want" this node.

</details>

<details><summary>Solution</summary>

```text
with-tol   Running   learn-worker2
no-tol     Pending   <none>
  … 1 node(s) didn't match Pod's node affinity/selector, 2 node(s) had untolerated taint(s). …
```

Remove the taint: `kubectl taint node learn-worker2 dedicated=gpu:NoSchedule-`.

</details>

## 3. 🟡 Spread the replicas

**Task:** a Deployment of three replicas with a `required` podAntiAffinity on `kubernetes.io/hostname`. See where the pods land. Then replace the rule with `topologySpreadConstraints` (`maxSkew: 1`, `DoNotSchedule`) and five replicas.

**Expected result:** with anti-affinity two pods are `Running`, one is `Pending` (`didn't match pod anti-affinity rules`); with topology spread all five are `Running`, distributed 3 and 2.

<details><summary>Hint</summary>

The `topologyKey` for spread is a node label (`zone`) that you set; the `labelSelector` matches the pod labels.

</details>

<details><summary>Solution</summary>

```text
anti-…-7dh4h   Running   learn-worker
anti-…-9pg79   Pending   <none>
anti-…-jznkq   Running   learn-worker2
spread, replicas=5: learn-worker 3, learn-worker2 2
```

</details>

## 4. 🟡 Priority and preemption

**Task:** create PriorityClasses `low` (100) and `high` (100000). Run a Deployment of two pods with `requests: cpu: 10` and class `low`. Then one pod with the same request and class `high`. Find the preemption events. Repeat with a class that has `preemptionPolicy: Never`.

**Expected result:** `Preempted by pod …`; in the second case `preemption: not eligible due to preemptionPolicy=Never`.

<details><summary>Hint</summary>

CPU on the nodes is `12`: `kubectl get nodes -o custom-columns=N:.metadata.name,CPU:.status.allocatable.cpu`.

</details>

<details><summary>Solution</summary>

```text
Warning  FailedScheduling  pod/vip  … preemption: found a potential placement for pod on node learn-worker2, preempting 1 victims
Normal   Preempted         pod/low-…  Preempted by pod … on node learn-worker2
Warning  FailedScheduling  pod/vip-nopre  … preemption: not eligible due to preemptionPolicy=Never.
```

</details>

## 5. 🔴 `drain` and PDB

**Task:** a Deployment `web` of four replicas. Create a `PodDisruptionBudget` with `--min-available=4`. Run `kubectl drain` on the node where the pods are (with `--timeout=15s`). Then fix the PDB so that `drain` can proceed.

**Expected result:** `ALLOWED DISRUPTIONS 0` and the error `global timeout reached`; after `--min-available=3` (or `maxUnavailable: 1`) pods are evicted one at a time.

<details><summary>Hint</summary>

`kubectl get pdb`; do not forget `kubectl uncordon <node>`.

</details>

<details><summary>Solution</summary>

```text
NAME      MIN AVAILABLE   MAX UNAVAILABLE   ALLOWED DISRUPTIONS
web-pdb   4               N/A               0
error when evicting pods/"web-…" -n "learn-18": global timeout reached: 15s
```

After `kubectl delete pdb web-pdb` and `kubectl create pdb web-pdb --selector=app=web --min-available=3` pods are evicted one at a time; if the replacements have nowhere to start (only one node), they stay `Pending`.

> ⚠️ Verify: a successful `drain` after fixing the PDB was not reproduced in the experiment; only the blocking was verified.

</details>
