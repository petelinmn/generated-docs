---
title: "Pod scheduling: basics"
lang: en
track: kubernetes
kind: module
doc: basics
slug: 18-scheduling
part: 3
tags: [scheduler, nodeselector, nodeaffinity, taints, tolerations, pending]
versions: "Kubernetes 1.37.0 (kind v0.34, 3 nodes), kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Pod scheduling: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [04 · Pod](../04-pods/overview.en.md), [05 · Deployment](../05-deployments/overview.en.md) · **Time:** ~2 h

All examples were run in kind (Kubernetes 1.37.0) on three nodes: `learn-control-plane`, `learn-worker`, `learn-worker2`; namespace `learn-18`. The nodes got labels with `kubectl label node learn-worker disk=ssd zone=a` and `kubectl label node learn-worker2 zone=b`.

## Why it matters

By default a pod can end up on any node with enough resources. Real tasks are harder: a database needs a fast disk, an agent needs a GPU, and replicas of one service should not be stacked on one node. Scheduling rules express these requirements in the manifest.

## How the scheduler chooses a node

`kube-scheduler` watches for pods without `spec.nodeName` and for each of them:

1. **Filters** the nodes: are there enough free `requests` (CPU, memory), do labels match, does a taint get in the way, and so on.
2. **Scores** the remaining ones and picks the best.
3. Writes the choice to `spec.nodeName` (binding). After that the kubelet of that node starts the pod.

If no node is left after filtering, the pod stays `Pending`, and the scheduler writes a `FailedScheduling` event with a per-node breakdown of reasons. This is the main diagnostic tool:

```text
$ kubectl -n learn-18 describe pod sel-bad
  Warning  FailedScheduling  15s   default-scheduler  0/3 nodes are available: 1 node(s) had untolerated taint(s), 2 node(s) didn't match Pod's node affinity/selector. preemption: 0/3 nodes are available: 3 Preemption is not helpful for scheduling.
```

Read it like this: of the three nodes one was rejected because of a taint (control-plane), two because the labels did not match.

## nodeSelector

The simplest rule: the pod goes only to nodes that have **all** the listed labels.

```yaml
apiVersion: v1
kind: Pod
metadata: {name: sel-ok}
spec:
  nodeSelector: {disk: ssd}
  containers: [{name: c, image: nginx:1.29}]
```

```text
$ kubectl -n learn-18 get pod sel-ok -o wide
NAME     READY   STATUS    NODE
sel-ok   1/1     Running   learn-worker
```

Look at node labels with `kubectl get nodes --show-labels`, set them with `kubectl label node <name> key=value`. If there are no suitable nodes (`nodeSelector: {disk: nvme}`), the pod stays `Pending` (see the example in the [overview](overview.en.md#minimal-example)).

## nodeAffinity

`nodeAffinity` does the same as `nodeSelector` but is more expressive: it has operators (`In`, `NotIn`, `Exists`, `DoesNotExist`, `Gt`, `Lt`) and two modes.

> ⚠️ Verify: the operators `NotIn`, `Exists`, `DoesNotExist`, `Gt`, `Lt` are taken from the documentation; only `In` was used in the experiments.

```yaml
affinity:
  nodeAffinity:
    requiredDuringSchedulingIgnoredDuringExecution:   # hard rule
      nodeSelectorTerms:
      - matchExpressions: [{key: zone, operator: In, values: [b]}]
    preferredDuringSchedulingIgnoredDuringExecution:  # a wish
    - weight: 50
      preference:
        matchExpressions: [{key: disk, operator: In, values: [nvme]}]
```

- **`required…`** — like `nodeSelector`: no suitable node means `Pending`. In the experiment the pod with `zone In [b]` landed on `learn-worker2`.
- **`preferred…`** — the scheduler adds weight to the score of matching nodes but does not forbid the others. In the experiment the pod with the wish `disk=nvme` (no such node) started calmly on `learn-worker`.
- The long name reads: "the requirement applies during scheduling and is **ignored during execution**". If the node labels change after the start, an already running pod does not move.

## Taints and tolerations

A **taint** is put on a node and repels pods that have no matching **toleration**.

```text
$ kubectl taint node learn-worker2 dedicated=gpu:NoSchedule
node/learn-worker2 tainted
```

Format: `key=value:effect`. The `NoSchedule` effect forbids **new** pods without a toleration from using the node. A pod with a toleration:

```yaml
spec:
  tolerations:
  - {key: dedicated, operator: Equal, value: gpu, effect: NoSchedule}
```

Experiment: two pods asked for the node `zone=b` (`learn-worker2`):

```text
$ kubectl -n learn-18 get pods -o wide
NAME       READY   STATUS    NODE
no-tol     0/1     Pending   <none>
with-tol   1/1     Running   learn-worker2
$ kubectl -n learn-18 describe pod no-tol
  Warning  FailedScheduling  … 0/3 nodes are available: 1 node(s) didn't match Pod's node affinity/selector, 2 node(s) had untolerated taint(s). …
```

Important: **a toleration only permits, it does not attract.** A pod with a toleration can land on any suitable node, including one without a taint. To pin a pod to a dedicated node, combine a taint (repels outsiders) and `nodeSelector` or affinity (attracts your own).

To remove a taint, use the same key with a minus: `kubectl taint node learn-worker2 dedicated=gpu:NoSchedule-`.

### The control-plane taint

The control-plane node in kind already has a taint:

```text
$ kubectl get node learn-control-plane -o jsonpath='{.spec.taints}'
[{"effect":"NoSchedule","key":"node-role.kubernetes.io/control-plane"}]
```

That is why ordinary pods do not land there, while system pods (kube-proxy, kindnet) and a [DaemonSet](../12-workloads/overview.en.md) with the right toleration do.

## Beginner mistakes

- ❌ Expecting a toleration to "attract" a pod to a tainted node → ✅ add `nodeSelector` or affinity.
- ❌ Not reading `FailedScheduling` and guessing → ✅ `kubectl describe pod`: the event lists reasons per node.
- ❌ A typo in a label value (`ssd ` or `SSD`) → ✅ labels are case-sensitive; compare with `kubectl get nodes --show-labels`.
- ❌ Hard rules on all pods → ✅ `preferred…` where a wish is enough.
- ❌ Assuming pods will move after node labels change → ✅ `IgnoredDuringExecution`: running pods stay in place.

## Related materials

- Next: [advanced theory](advanced.en.md) — antiAffinity, topology spread, priorities, drain
- Pod and its lifecycle: [04 · Pod](../04-pods/overview.en.md)
- Requests as scheduler input: [11 · Probes and resources](../11-probes-and-resources/overview.en.md)
- DaemonSet and tolerations: [12 · Other workloads](../12-workloads/overview.en.md)

## Sources

- [Assigning Pods to Nodes](https://kubernetes.io/docs/concepts/scheduling-eviction/assign-pod-node/)
- [Taints and Tolerations](https://kubernetes.io/docs/concepts/scheduling-eviction/taint-and-toleration/)
- [Kubernetes Scheduler](https://kubernetes.io/docs/concepts/scheduling-eviction/kube-scheduler/)
