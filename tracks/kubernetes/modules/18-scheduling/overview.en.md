---
title: "Pod scheduling"
lang: en
track: kubernetes
kind: module
doc: overview
slug: 18-scheduling
part: 3
levels: [1, 2, 3, 4]
tags: [scheduler, nodeselector, affinity, taints, tolerations, topology-spread, priorityclass, preemption, drain, pdb]
versions: "Kubernetes 1.37.0 (kind v0.34, 3 nodes), kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Pod scheduling

🌐 [Русский](overview.ru.md) · **English**

## In short

The **scheduler** chooses a node for every new pod: it first discards unsuitable nodes (filters), then scores the rest and writes the choice to `spec.nodeName`. You can influence the choice from the manifest: **nodeSelector and nodeAffinity** (where it is allowed), **taints and tolerations** (whom a node rejects), **podAffinity/podAntiAffinity and topologySpreadConstraints** (next to whom, or spread out), **priority** (who is more important when space runs out). If there is no suitable node, the pod stays `Pending`, and the reason is in the `FailedScheduling` event.

## Key ideas

- **The scheduler assigns only new pods.** It does not move a running pod; a move is a deletion and re-creation by a controller.
- **`nodeSelector` is the simplest form:** the pod goes only to nodes with the required labels. No match — `Pending`.
- **Affinity is more flexible:** `required…` is a hard rule, `preferred…` is a wish with a weight.
- **A taint repels, a toleration permits.** A taint does not attract: a pod with a toleration still has to pick that node through other rules.
- **The `NoExecute` effect evicts already running pods,** and `tolerationSeconds` limits how long they "endure".
- **Anti-affinity and topology spread spread replicas** across nodes and zones; a hard rule with too few nodes leaves extra replicas `Pending`.
- **Priority allows preemption:** a high-priority pod that lacks space deletes low-priority pods. `preemptionPolicy: Never` disables preemption.
- **`cordon` and `drain` prepare a node for maintenance;** a PodDisruptionBudget can block `drain`.

## Minimal example

```yaml
apiVersion: v1
kind: Pod
metadata: {name: sel-bad}
spec:
  nodeSelector: {disk: nvme}
  containers: [{name: c, image: nginx:1.29}]
```

```text
$ kubectl -n learn-18 get pod sel-bad
NAME      READY   STATUS    RESTARTS   AGE
sel-bad   0/1     Pending   0          15s
$ kubectl -n learn-18 describe pod sel-bad
  Warning  FailedScheduling  15s   default-scheduler  0/3 nodes are available: 1 node(s) had untolerated taint(s), 2 node(s) didn't match Pod's node affinity/selector. preemption: 0/3 nodes are available: 3 Preemption is not helpful for scheduling.
```

No node has the label `disk=nvme`, and the control-plane has a taint.

## When to use and when not

| Task | Tool |
|---|---|
| You need nodes of a certain type (SSD, GPU) | nodeSelector or nodeAffinity |
| A node is reserved for special pods | taint on the node + toleration in pods (+ nodeSelector) |
| Replicas must not end up on one node | podAntiAffinity or topologySpreadConstraints |
| Important pods must get space first | PriorityClass |
| Node maintenance | `cordon`, `drain`, PDB |

- ❌ Do not set hard rules without need: each narrows the choice, and a pod easily gets stuck in `Pending`.
- ❌ Do not use `nodeName` instead of scheduling rules: it bypasses both the scheduler and taints.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | how the scheduler chooses a node, nodeSelector, nodeAffinity, taints and tolerations, reading `FailedScheduling` |
| [2. Advanced theory](advanced.en.md) | podAffinity and antiAffinity, topology spread, `NoExecute` and `tolerationSeconds`, priorities and preemption, cordon/drain and PDB |
| [3. Expert theory](expert.en.md) | reference of fields and operators, built-in taints, scheduler internals, pitfalls, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn kubernetes 18-scheduling level 5` |
| 6. Tests | not created yet — `/learn kubernetes 18-scheduling level 6` |

## Related materials

- Prerequisites: [04 · Pod](../04-pods/overview.en.md), [05 · Deployment](../05-deployments/overview.en.md), [11 · Probes and resources](../11-probes-and-resources/overview.en.md) (requests affect placement)
- Previous modules: [16 · Observability](../16-observability/overview.en.md), [17 · Security](../17-security/overview.en.md)
- DaemonSet and tolerations: [12 · Other workloads](../12-workloads/overview.en.md)
- Autoscaling and a lack of nodes: [15 · Autoscaling](../15-autoscaling/overview.en.md)
- Next: module 19 · CRDs and operators (in the [track](../../overview.en.md) roadmap)
