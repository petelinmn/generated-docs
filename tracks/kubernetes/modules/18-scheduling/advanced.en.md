---
title: "Pod scheduling: advanced"
lang: en
track: kubernetes
kind: module
doc: advanced
slug: 18-scheduling
part: 3
tags: [pod-anti-affinity, topology-spread, noexecute, tolerationseconds, priorityclass, preemption, cordon, drain, pdb]
versions: "Kubernetes 1.37.0 (kind v0.34, 3 nodes), kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Pod scheduling: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on [basic theory](basics.en.md). Experiments were run in kind on three nodes, namespace `learn-18`.

## Pod affinity and anti-affinity

The rules "next to whom" and "away from whom" look at the labels of **other pods**, and the scope is set by `topologyKey` — a node label: `kubernetes.io/hostname` means "one node", `topology.kubernetes.io/zone` means "one zone".

```yaml
spec:
  replicas: 3
  template:
    metadata: {labels: {app: anti}}
    spec:
      affinity:
        podAntiAffinity:
          requiredDuringSchedulingIgnoredDuringExecution:
          - labelSelector: {matchLabels: {app: anti}}
            topologyKey: kubernetes.io/hostname
```

Meaning: "do not place the pod next to another `app=anti` pod on the same node". There are two worker nodes and three replicas:

```text
$ kubectl -n learn-18 get pods -o wide
anti-dc6868b9c-7dh4h   Running   learn-worker
anti-dc6868b9c-9pg79   Pending   <none>
anti-dc6868b9c-jznkq   Running   learn-worker2
$ kubectl -n learn-18 get events --field-selector reason=FailedScheduling
0/3 nodes are available: 1 node(s) had untolerated taint(s), 2 node(s) didn't match pod anti-affinity rules. preemption: 0/3 nodes are available: 1 Preemption is not helpful for scheduling, 2 No preemption victims found for incoming pod.
```

The third replica waits: the hard rule outweighs the wish to run all replicas. `podAffinity` works the other way round: "I want to be on the same node or in the same zone as pod X" (for example, an application next to its cache). For a soft variant there is `preferredDuring…` with a weight.

## topologySpreadConstraints

Anti-affinity says "at most one per node", but often you need something else: "spread as evenly as possible, a skew of at most one pod is fine". That is what `topologySpreadConstraints` is for:

```yaml
spec:
  topologySpreadConstraints:
  - maxSkew: 1
    topologyKey: zone
    whenUnsatisfiable: DoNotSchedule
    labelSelector: {matchLabels: {app: spread}}
```

- `maxSkew` — the maximum allowed difference in the pod count between the most and the least loaded areas (domains).
- `whenUnsatisfiable`: `DoNotSchedule` is hard, `ScheduleAnyway` is soft.

In the experiment, zones `a` and `b` are the two worker nodes:

```text
replicas=4: learn-worker 2, learn-worker2 2
replicas=5: learn-worker 3, learn-worker2 2
```

A skew of 1 is allowed; the sixth and seventh replicas will even out the distribution again. This is better than anti-affinity for services whose replica count exceeds the node count.

> ⚠️ Verify: the `ScheduleAnyway` mode and the fields `minDomains`, `matchLabelKeys`, `nodeAffinityPolicy` and `nodeTaintsPolicy` are taken from the documentation and were not run.

## NoExecute and tolerationSeconds

A taint with the `NoExecute` effect also acts on **already running** pods without a toleration — they are evicted. Experiment: two "bare" pods (no controller) ran on `learn-worker2`, then `kubectl taint node learn-worker2 maint=true:NoExecute`:

```text
$ kubectl -n learn-18 get pods
No resources found in learn-18 namespace.
```

The pods are gone and will not return: nobody watches them. That is why "bare" pods in production are a bad idea (see [module 04](../04-pods/basics.en.md)).

`tolerationSeconds` sets how long a pod endures a taint before being evicted:

```yaml
tolerations:
- {key: maint, operator: Exists, effect: NoExecute, tolerationSeconds: 20}
```

Experiment with a two-replica Deployment, `kubectl taint node learn-worker maint=true:NoExecute`:

```text
t=14  tolsec-…-n6j47 (worker2)   tolsec-…-vbksb (worker)
t=21  tolsec-…-d6bdt (worker)    tolsec-…-n6j47 (worker2)   ← vbksb evicted after ~20 s
t=42  tolsec-…-n6j47 (worker2)   tolsec-…-plz74 (worker)    ← d6bdt evicted after another ~20 s
```

The catch: the replacement has the same toleration, so the `ReplicaSet` creates it, the scheduler puts it on the same node **again** (the taint does not stop it), and after 20 seconds it is evicted again. The cycle goes on until the taint is removed. To really "empty a node" you need `drain` and suitable rules (see below).

Your pods also carry such tolerations although you did not write them: the API server adds them.

```text
$ kubectl -n learn-18 get pod web-… -o jsonpath='{.spec.tolerations}'
[{"effect":"NoExecute","key":"node.kubernetes.io/not-ready","operator":"Exists","tolerationSeconds":300},
 {"effect":"NoExecute","key":"node.kubernetes.io/unreachable","operator":"Exists","tolerationSeconds":300}]
```

So when a node is lost, pods are evicted after 5 minutes.

## Priorities and preemption

A **PriorityClass** is a cluster-wide object: a name and a number. The higher the number, the more important the pod. When a high-priority pod lacks space, the scheduler may **preempt** pods with lower priority.

```yaml
apiVersion: scheduling.k8s.io/v1
kind: PriorityClass
metadata: {name: high-18}
value: 100000
```

A pod picks a class like this: `spec.priorityClassName: high-18`. Without a class the priority is 0 and the policy is `PreemptLowerPriority`:

```text
$ kubectl get priorityclass
NAME                      VALUE        GLOBAL-DEFAULT   AGE     PREEMPTIONPOLICY
high-18                   100000       false            2m10s   PreemptLowerPriority
low-18                    100          false            2m10s   PreemptLowerPriority
system-cluster-critical   2000000000   false            11m     PreemptLowerPriority
system-node-critical      2000001000   false            11m     PreemptLowerPriority
```

Experiment: each worker node has 12 CPUs. A `low` Deployment of two pods with `requests: cpu: 10` and priority 100 occupied both nodes. Then a `vip` pod with the same request and priority 100000:

```text
$ kubectl -n learn-18 get pods -o wide
low-7d849d9689-ddq7j   Pending   <none>
low-7d849d9689-vq4t4   Running   learn-worker
vip                    Running   learn-worker2
$ kubectl -n learn-18 get events | grep -iE "preempt"
Warning  FailedScheduling  pod/vip  0/3 nodes are available: 1 node(s) had untolerated taint(s), 2 Insufficient cpu. preemption: found a potential placement for pod on node learn-worker2, preempting 1 victims
Normal   Preempted         pod/low-7d849d9689-fx5vg  Preempted by pod 12d4bc6c-… on node learn-worker2
```

One `low` pod was preempted; its replacement is now `Pending` (no resources left). If preemption is undesirable, a class has `preemptionPolicy: Never`: the pod still gets a high priority in the queue but does not preempt anyone:

```text
$ kubectl -n learn-18 describe pod vip-nopre
  Warning  FailedScheduling  8s  default-scheduler  0/3 nodes are available: 1 node(s) had untolerated taint(s), 2 Insufficient cpu. preemption: not eligible due to preemptionPolicy=Never.
```

## Node maintenance: cordon, drain and PDB

- `kubectl cordon <node>` — mark the node as unavailable for new pods. This is the taint `node.kubernetes.io/unschedulable:NoSchedule`, and the status is `Ready,SchedulingDisabled`. Running pods stay.
- `kubectl drain <node> --ignore-daemonsets --delete-emptydir-data` — `cordon` followed by eviction of the pods. DaemonSet pods are not evicted (they would return at once), pods with `emptyDir` lose their data — hence the flag.
- `kubectl uncordon <node>` — return the node to work.

Experiment: `learn-worker` is cordoned, all 4 `web` pods are on `learn-worker2`. `kubectl drain learn-worker2 --ignore-daemonsets --delete-emptydir-data` evicted them, the Deployment created replacements — and all four hung in `Pending`: the only worker node is closed, and the control-plane has a taint. After `uncordon` they started. Eviction takes a **PodDisruptionBudget (PDB)** into account — a limit on voluntary disruptions:

```text
$ kubectl -n learn-18 create pdb web-pdb --selector=app=web --min-available=4
$ kubectl -n learn-18 get pdb
NAME      MIN AVAILABLE   MAX UNAVAILABLE   ALLOWED DISRUPTIONS   AGE
web-pdb   4               N/A               0                     2s
$ kubectl drain learn-worker --ignore-daemonsets --delete-emptydir-data --timeout=15s
There are pending nodes to be drained:
 learn-worker
error when evicting pods/"web-689b9bdbdd-lf4jv" -n "learn-18": global timeout reached: 15s
```

`ALLOWED DISRUPTIONS = 0`: all four of four replicas must stay up, so `drain` evicted none. A PDB protects against accidentally zeroing a service, but a bad PDB (`minAvailable` equal to the replica count) blocks node maintenance.

## nodeName: direct assignment

If you set `spec.nodeName` right away, the scheduler is not involved at all:

```text
$ kubectl -n learn-18 get pod byname -o wide
NAME     READY   STATUS              NODE
byname   0/1     ContainerCreating   learn-control-plane
```

The pod landed on the control-plane although it has a `NoSchedule` taint: the `NoSchedule` taint check is done by the scheduler, which was bypassed. This is a debugging trick, not an operations tool.

## Best practices

- To spread replicas prefer `topologySpreadConstraints`; a hard `podAntiAffinity` only if the replica count is certainly not above the node count.
- Dedicated nodes: taint + toleration + `nodeSelector` as one set.
- Always set `requests`: without them the scheduler does not know how much space is needed (see [module 11](../11-probes-and-resources/overview.en.md)).
- Create a few PriorityClasses (critical, normal, background), not dozens.
- For critical services set a PDB, but with headroom (`minAvailable` below the replica count).
- Before `drain` check that there are no pods without a controller on the node: nobody will re-create them.

## Common mistakes

- ❌ A `requiredDuringScheduling` anti-affinity with more replicas than nodes → ✅ topology spread or `preferred…`.
- ❌ Priorities without `requests`: preemption computes resources, and "nothing to preempt".
- ❌ A PDB `minAvailable` equal to the replica count → ✅ leave headroom for maintenance.
- ❌ Expecting a `NoExecute` taint to "relocate" the right pods → ✅ a controller (Deployment and so on) is needed.
- ❌ Using `nodeName` as a permanent solution → ✅ nodeSelector or affinity.

## Related materials

- Next: [expert theory](expert.en.md)
- DaemonSet and default tolerations: [12 · Other workloads](../12-workloads/basics.en.md)
- Autoscaling and a lack of nodes: [15 · Autoscaling](../15-autoscaling/advanced.en.md#vpa-and-cluster-autoscaler)
- Probes and requests: [11 · Probes and resources](../11-probes-and-resources/overview.en.md)

## Sources

- [Pod Topology Spread Constraints](https://kubernetes.io/docs/concepts/scheduling-eviction/topology-spread-constraints/)
- [Pod Priority and Preemption](https://kubernetes.io/docs/concepts/scheduling-eviction/pod-priority-preemption/)
- [Node-pressure Eviction and API-initiated Eviction](https://kubernetes.io/docs/concepts/scheduling-eviction/api-eviction/)
- [Safely Drain a Node](https://kubernetes.io/docs/tasks/administer-cluster/safely-drain-node/)
- [Disruptions (PodDisruptionBudget)](https://kubernetes.io/docs/concepts/workloads/pods/disruptions/)
