---
title: "Pod scheduling: expert"
lang: en
track: kubernetes
kind: module
doc: expert
slug: 18-scheduling
part: 3
tags: [scheduler, scheduling-framework, taints, tolerations, affinity, topology-spread, preemption, pdb]
versions: "Kubernetes 1.37.0 (kind v0.34, 3 nodes), kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Pod scheduling: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on [basic](basics.en.md) and [advanced](advanced.en.md) theory. What was confirmed by an experiment in kind (Kubernetes 1.37.0) is marked "in the experiment".

## Reference

### Pod fields that affect placement

| Field | Meaning |
|---|---|
| `spec.nodeName` | direct node assignment, the scheduler is not involved (in the experiment it bypassed the control-plane taint) |
| `spec.nodeSelector` | a map of labels; all are required |
| `spec.affinity.nodeAffinity` | `required…` (`nodeSelectorTerms`) and `preferred…` (`weight` + `preference`) |
| `spec.affinity.podAffinity` / `podAntiAffinity` | `required…` (`labelSelector`, `topologyKey`, optionally `namespaces`/`namespaceSelector`) and `preferred…` (`weight` + `podAffinityTerm`) |
| `spec.tolerations[]` | `key`, `operator` (`Equal`/`Exists`), `value`, `effect`, `tolerationSeconds` |
| `spec.topologySpreadConstraints[]` | `maxSkew`, `topologyKey`, `whenUnsatisfiable`, `labelSelector` (and optional `minDomains`, `matchLabelKeys`, `nodeAffinityPolicy`, `nodeTaintsPolicy`) |
| `spec.priorityClassName` → `spec.priority` | the priority is filled in from the PriorityClass (0 for pods without a class) |
| `spec.preemptionPolicy` | `PreemptLowerPriority` (default) or `Never` |
| `spec.schedulerName` | which scheduler handles the pod (`default-scheduler` by default) |

> ⚠️ Verify: the experiments used `nodeSelector`, `nodeAffinity` with the `In` operator, `podAntiAffinity` (required, `kubernetes.io/hostname`), `topologySpreadConstraints` (`DoNotSchedule`), `Equal`/`Exists` tolerations, PriorityClass and `preemptionPolicy: Never`. The other fields and operators come from the documentation.

### Taint effects

| Effect | Action |
|---|---|
| `NoSchedule` | new pods without a toleration are not scheduled; running pods stay |
| `PreferNoSchedule` | the scheduler tries to avoid it but does not forbid it |
| `NoExecute` | new pods are not scheduled, **running pods without a toleration are evicted** (in the experiment) |

A toleration matches a taint if `key` and `effect` match (an empty `effect` means any), and the value: with `Equal` it equals `value`, with `Exists` it is not checked. An empty `key` with `Exists` tolerates everything.

### Taints set by the cluster

| Taint | When |
|---|---|
| `node-role.kubernetes.io/control-plane:NoSchedule` | on the control-plane in kind (in the experiment) |
| `node.kubernetes.io/unschedulable:NoSchedule` | `kubectl cordon` (in the experiment, with `timeAdded`) |
| `node.kubernetes.io/not-ready`, `node.kubernetes.io/unreachable` (`NoExecute`) | the node is not ready or unreachable; pods automatically have a 300 s toleration (in the experiment) |
| `memory-pressure`, `disk-pressure`, `pid-pressure`, `network-unavailable` | pressure on node resources |

> ⚠️ Verify: the last row of the table is taken from the documentation and was not reproduced.

### Commands

| Command | Purpose |
|---|---|
| `kubectl label node N k=v` / `k-` | set and remove a label |
| `kubectl taint node N k=v:Effect` / `k=v:Effect-` | set and remove a taint |
| `kubectl cordon / uncordon / drain N` | close a node, return it, free it |
| `kubectl get pdb` | allowed disruptions (`ALLOWED DISRUPTIONS`) |
| `kubectl get priorityclass` | priority values and policies |
| `kubectl get events --field-selector reason=FailedScheduling` | reasons for `Pending` |

## Internals

- **The scheduler is an ordinary controller.** In kind it is the static pod `kube-scheduler` in `kube-system` (image `registry.k8s.io/kube-scheduler:v1.37.0` in the experiment). It watches pods without `spec.nodeName`, picks a node and creates a binding.
- **The scheduling cycle** consists of stages (an extensible "scheduling framework"): filters discard nodes, scores rank the rest, then binding. Messages like `Insufficient cpu`, `didn't match Pod's node affinity/selector` and `had untolerated taint(s)` are results of different filters.
- **The queue.** Pods that could not be placed go to a queue of "unschedulable" pods and are reconsidered when something changes (a new node, space freed, labels changed).
- **The decision is based on `requests`, not actual consumption.** In the experiment two `low` pods with `requests: cpu: 10` barely used CPU but occupied the nodes entirely: `vip` got `Insufficient cpu`.
- **Preemption.** If a pod fits nowhere, the scheduler looks for a node where space can be freed by deleting pods of lower priority. The message `preemption: found a potential placement … preempting 1 victims` (in the experiment) means a decision was made, and the `Preempted` event records the victim. A preempted pod, if it has a controller, will be re-created and queued.
- **The taint controller.** Eviction by `NoExecute` and `tolerationSeconds` is handled by the node lifecycle controller. It simply deletes pods without a controller (in the experiment).
- **Eviction API.** `kubectl drain` evicts through eviction, not `delete`: that is how the PDB is taken into account.

> ⚠️ Verify: the queue details (backoff/unschedulable), the list of framework stages and the `percentageOfNodesToScore` parameter are taken from the documentation and were not checked. Scheduler profiles (`KubeSchedulerConfiguration`, several schedulers) were not run.

## Gotchas

### 1. A toleration does not attract

A pod with a toleration for `dedicated=gpu` can land on an ordinary node. Pin pods with a combination of taint + `nodeSelector`/affinity.

### 2. `tolerationSeconds` and replacements

In the experiment pods were evicted after ~20 s, and the replacement came right back to the same node and was evicted again. A taint does not "drive pods away for good" while it exists and the template has a toleration.

### 3. The scheduler does not look at actual consumption

A node can be idle yet "busy" by `requests`: `0/3 nodes are available: … 2 Insufficient cpu`. And vice versa: pods without `requests` are placed "for free" and overload the node.

### 4. `required` anti-affinity with `replicas` above the node count

In the experiment the third replica stayed `Pending` with `didn't match pod anti-affinity rules`. For scalable services use topology spread.

### 5. `IgnoredDuringExecution`

Changing node labels or a `NoSchedule` taint does not affect running pods. Only `NoExecute` evicts.

### 6. A PDB blocks `drain`

`minAvailable` equal to the replica count gives `ALLOWED DISRUPTIONS 0`, and `drain` hangs until the timeout (in the experiment `global timeout reached: 15s`).

### 7. The replacement of a preempted pod also stays `Pending`

In the experiment preemption freed space for `vip`, while the victim's replacement was left without space. Preemption moves the resource shortage onto less important pods but does not solve it. It is solved by the Cluster Autoscaler or more nodes ([module 15](../15-autoscaling/advanced.en.md#vpa-and-cluster-autoscaler)).

### 8. `nodeName` bypasses everything

A pod with `nodeName` landed on the control-plane although it has a `NoSchedule` taint (in the experiment). The scheduler, and therefore the filters by `requests`, do not take part on this path.

### 9. The message `Preemption is not helpful`

In `FailedScheduling` the phrase `preemption: 0/3 nodes are available: 3 Preemption is not helpful for scheduling` means that preemption would not help (for example, the cause is a label mismatch, not a lack of resources). It is not an error.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| `nodeSelector` | simple | equality only |
| nodeAffinity | operators, soft rules | verbose |
| taints + tolerations | protection of dedicated nodes | do not attract by themselves |
| podAntiAffinity | hard spread across nodes | `Pending` with too few nodes, costlier on a large cluster |
| topology spread | even distribution, soft mode | harder to configure |
| PriorityClass | guarantees space for important pods | preempts, needs `requests` |
| Descheduler (external project) | rebalances already running pods | a separate installation; not run |

## Interview questions

<details><summary>1. How does the scheduler choose a node?</summary>

It filters nodes (resources by `requests`, labels, taints, affinity), scores the rest and writes the choice to `spec.nodeName`.

→ [Basic theory](basics.en.md#how-the-scheduler-chooses-a-node)

</details>

<details><summary>2. Why is a pod <code>Pending</code> and how do you find the reason?</summary>

There is no suitable node. The per-node reasons are in the `FailedScheduling` event (`kubectl describe pod`).

→ [Basic theory](basics.en.md#how-the-scheduler-chooses-a-node)

</details>

<details><summary>3. How does nodeSelector differ from nodeAffinity?</summary>

nodeSelector is label equality only. nodeAffinity adds operators and a soft mode (`preferred…`).

→ [Basic theory](basics.en.md#nodeaffinity)

</details>

<details><summary>4. Trick: does a toleration for a <code>gpu</code> taint guarantee the pod lands on a GPU node?</summary>

No. A toleration only permits. To pin the pod you also need nodeSelector or affinity.

→ [Gotcha 1](#1-a-toleration-does-not-attract)

</details>

<details><summary>5. How does <code>NoSchedule</code> differ from <code>NoExecute</code>?</summary>

`NoSchedule` acts only on new pods, `NoExecute` also evicts running pods without a toleration.

→ [Advanced theory](advanced.en.md#noexecute-and-tolerationseconds)

</details>

<details><summary>6. What does <code>tolerationSeconds</code> do?</summary>

The time a pod endures a `NoExecute` taint before eviction. By default for `not-ready` and `unreachable` it is 300 s.

→ [Advanced theory](advanced.en.md#noexecute-and-tolerationseconds)

</details>

<details><summary>7. How do you spread replicas across nodes and when is topology spread better?</summary>

`podAntiAffinity` with `kubernetes.io/hostname` is hard "one per node". Topology spread allows a `maxSkew` and fits when there are more replicas than nodes.

→ [Advanced theory](advanced.en.md#topologyspreadconstraints)

</details>

<details><summary>8. What happens if a high-priority pod does not fit?</summary>

The scheduler looks for a node where lower-priority pods can be preempted; victims get `Preempted`. With `preemptionPolicy: Never` there is no preemption.

→ [Advanced theory](advanced.en.md#priorities-and-preemption)

</details>

<details><summary>9. Trick: a node is idle on CPU but a pod is <code>Pending</code> with <code>Insufficient cpu</code>. Why?</summary>

The scheduler sums `requests`, not actual consumption.

→ [Gotcha 3](#3-the-scheduler-does-not-look-at-actual-consumption)

</details>

<details><summary>10. How does <code>cordon</code> differ from <code>drain</code>?</summary>

`cordon` only closes the node to new pods (the `unschedulable` taint), `drain` additionally evicts pods.

→ [Advanced theory](advanced.en.md#node-maintenance-cordon-drain-and-pdb)

</details>

<details><summary>11. Why did <code>drain</code> hang?</summary>

Often because of a PDB with `ALLOWED DISRUPTIONS 0`, or because of pods without a controller and DaemonSets (they need `--ignore-daemonsets` and deliberate deletion).

→ [Gotcha 6](#6-a-pdb-blocks-drain)

</details>

<details><summary>12. What is <code>IgnoredDuringExecution</code> in a rule's name?</summary>

The rule is checked only at scheduling time: if labels change later, a running pod does not move.

→ [Gotcha 5](#5-ignoredduringexecution)

</details>

<details><summary>13. What does <code>nodeName</code> do and why is it dangerous?</summary>

It assigns a node directly, bypassing the scheduler and its filters. A pod can land on a node with a taint or without free resources.

→ [Gotcha 8](#8-nodename-bypasses-everything)

</details>

## Cheat sheet

```text
kubectl label node N k=v          node label
kubectl taint node N k=v:NoSchedule[-]
kubectl cordon N | uncordon N | drain N --ignore-daemonsets --delete-emptydir-data
kubectl get events --field-selector reason=FailedScheduling
kubectl get pdb ; kubectl get priorityclass
```

- Pending → `describe pod` → events.
- A taint repels, a toleration permits, affinity/selector attracts.
- Spread: topology spread (soft) or antiAffinity (hard).
- Priority preempts; `preemptionPolicy: Never` does not.
- Before `drain` — a PDB with headroom.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Autoscaling: [15 · Autoscaling](../15-autoscaling/expert.en.md)
- DaemonSet: [12 · Other workloads](../12-workloads/expert.en.md)
- Security and PSA: [17 · Security](../17-security/overview.en.md)

## Sources

- [Kubernetes Scheduler](https://kubernetes.io/docs/concepts/scheduling-eviction/kube-scheduler/)
- [Scheduling Framework](https://kubernetes.io/docs/concepts/scheduling-eviction/scheduling-framework/)
- [Taints and Tolerations](https://kubernetes.io/docs/concepts/scheduling-eviction/taint-and-toleration/)
- [Well-Known Labels, Annotations and Taints](https://kubernetes.io/docs/reference/labels-annotations-taints/)
- [Pod Priority and Preemption](https://kubernetes.io/docs/concepts/scheduling-eviction/pod-priority-preemption/)
