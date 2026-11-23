---
title: "Autoscaling: expert theory"
lang: en
track: kubernetes
kind: module
doc: expert
slug: 15-autoscaling
part: 3
tags: [hpa, autoscaling, metrics-server, metrics-api, hpa-algorithm, scale-subresource]
versions: "Kubernetes 1.37.0 (kind v0.34), metrics-server v0.9.1, kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Autoscaling: expert theory

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on [basic](basics.en.md) and [advanced](advanced.en.md) theory. What was confirmed by an experiment in kind (Kubernetes 1.37.0) is marked "in the experiment".

## Reference

### Fields of the `autoscaling/v2` HorizontalPodAutoscaler

| Field | Meaning |
|---|---|
| `spec.scaleTargetRef` | the object to scale (`apiVersion`, `kind`, `name`): it needs the `scale` subresource |
| `spec.minReplicas` | the minimum (1 by default); 0 only with Object/External metrics (in the experiment rejected for Resource) |
| `spec.maxReplicas` | the maximum (required) |
| `spec.metrics[]` | the list of metrics; type `Resource`, `ContainerResource`, `Pods`, `Object`, `External` |
| `spec.behavior` | `scaleUp` and `scaleDown`: `stabilizationWindowSeconds`, `selectPolicy`, `policies[]` (`type: Pods` or `Percent`, `value`, `periodSeconds`) |
| `status.currentReplicas`, `desiredReplicas` | the current and desired replica count |
| `status.currentMetrics` | the latest metric values |
| `status.conditions` | `AbleToScale`, `ScalingActive`, `ScalingLimited` (and `ScaledToZero` in the experiment) |

> ⚠️ Verify: the default of `minReplicas`, support for `ContainerResource`, `Object`, `External` and `selectPolicy: Min/Disabled` are taken from the documentation and were not run (the experiment used `Resource` and `Pods`, `Max`).

### Metric target types

| `target.type` | Field | Where applicable |
|---|---|---|
| `Utilization` | `averageUtilization` (percent of `requests`) | Resource, ContainerResource |
| `AverageValue` | `averageValue` | Resource, Pods, Object, External |
| `Value` | `value` | Object, External |

The experiment verified `Utilization` (cpu 50%) and `AverageValue` (memory `100Mi`, pods `10`).

### HPA conditions (in the experiment)

| Condition | Example reason |
|---|---|
| `AbleToScale` | `ReadyForNewScale`, `SucceededGetScale` |
| `ScalingActive` | `ValidMetricFound`; `FailedGetResourceMetric` (no requests or metrics-server); `FailedGetPodsMetric` (no custom API); `AmbiguousSelector` |
| `ScalingLimited` | `TooManyReplicas` (hit `maxReplicas`) |
| `ScaledToZero` | `NotScaledToZero` |

### Commands

| Command | Purpose |
|---|---|
| `kubectl get hpa` | a table: targets, bounds, replicas |
| `kubectl describe hpa` | metrics, behavior, conditions, events |
| `kubectl autoscale deploy X --min --max --cpu` | create an HPA |
| `kubectl top nodes/pods [--containers]` | Metrics API data |
| `kubectl get --raw /apis/metrics.k8s.io/v1beta1/...` | the raw Metrics API response (`PodMetricsList`) |

## Internals

- **The controller loop.** The HPA controller in kube-controller-manager periodically (every 15 seconds per the documentation) reads the metrics, computes the desired replica count and writes it through the `scale` subresource of the target object. The Deployment controller then creates the pods ([module 05](../05-deployments/expert.en.md)).
- **The formula.** `desired = ceil(current × current metric / target)`. In the experiment: 1 pod, 250% at a target of 50% → `ceil(1 × 250 / 50) = 5`; 5 pods, 71% → `ceil(5 × 71 / 50) = 8`, but the `maxReplicas: 5` limit applies (`TooManyReplicas`).
- **A percentage of `requests`.** For `Utilization` the metric is the ratio of consumption to `requests`: 143m with requests of 200m is 71%. So without requests the percentage is undefined.
- **The data source.** metrics-server polls the kubelet and keeps only the latest snapshot in memory; the window in the response is `15.903s`. History and charts are a monitoring task ([module 16 of the roadmap](../../overview.en.md)).
- **Several metrics.** HPA computes the desired count for each and takes the maximum.
- **`behavior` policies.** Upward and downward recommendations are limited by policies (`Pods` and `Percent` per period) and the stabilization window; with `Select Policy: Max` the policy allowing the larger change is chosen (in the experiment +4 pods per 15 s).
- **The owner of `replicas`.** HPA writes to `scale`, not to the manifest: on a repeated `kubectl apply` of a manifest with `replicas` the count is reset to the manifest value until the next HPA cycle.

> ⚠️ Verify: the 15-second polling interval and the 10% tolerance when comparing with the target are taken from the documentation; the tolerance was not verified by running. The behavior on a repeated apply with `replicas` is inferred from the documentation.

## Pitfalls

### 1. `<unknown>` in `TARGETS`

There are two causes: no `requests` (`missing request for cpu in container nginx of Pod …`) or no data from metrics-server (in the first minutes `no metrics returned from resource metrics API`, `did not receive metrics for targeted pods (pods might be unready)`).

### 2. The percentage is computed from `requests`, not `limits`

A pod with `limits: 500m`, `requests: 200m` consuming 143m is 71% (in the experiment). Understated `requests` give an overstated percentage and early scaling.

### 3. Scaling down is slow

Three replicas raised by hand held for about five minutes without load (in the experiment) — the default stabilization window. For fast shrinking set `behavior.scaleDown`.

### 4. Two HPAs on one object

`AmbiguousSelector`: both HPAs stop working.

### 5. The first scaling goes straight to the maximum

The upward policy `Pods: 4` and `Percent: 100` per 15 seconds allowed the path `1 → 5` in one step; for sharp metric jumps this may be too aggressive.

### 6. `minReplicas: 0`

Does not work for Resource metrics: `must specify at least one Object or External metric to support scaling to zero replicas`.

### 7. A `Pods` metric without an adapter

`FailedGetPodsMetric … no custom metrics API (custom.metrics.k8s.io) registered`: the adapter is a separate installation.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| HPA on CPU | simple, built in | CPU does not always reflect load; `requests` needed |
| HPA on custom/external metrics | reacts to business metrics (RPS, queue) | an adapter is needed (Prometheus Adapter, KEDA) |
| VPA | picks resources | pod restarts, does not combine with HPA on the same resource |
| Cluster Autoscaler | the number of nodes | cloud integration, delay |
| A fixed replica count | predictable | overpaying or shortage at a peak |

## Interview questions

<details><summary>1. How does HPA compute the desired replica count?</summary>

`ceil(current replica count × current metric / target)`; the result is limited by `minReplicas` and `maxReplicas`.

→ [Basic theory](basics.en.md#load-and-scaling)

</details>

<details><summary>2. What is needed for HPA on CPU?</summary>

metrics-server (the Metrics API) and CPU `requests` on the containers of the target pods.

→ [Basic theory](basics.en.md#metrics-server-and-kubectl-top)

</details>

<details><summary>3. Trap: <code>TARGETS</code> shows <code>&lt;unknown&gt;</code>. Why?</summary>

No `requests`, no metrics-server, or the metrics have not been collected yet (`FailedGetResourceMetric`).

→ [Pitfall 1](#1-unknown-in-targets)

</details>

<details><summary>4. What is the CPU utilization percentage computed from?</summary>

From the containers' `requests`, not from `limits`.

→ [Pitfall 2](#2-the-percentage-is-computed-from-requests-not-limits)

</details>

<details><summary>5. How fast does HPA scale up and down by default?</summary>

Up: a stabilization window of 0, up to +4 pods or +100% per 15 seconds. Down: a stabilization window of about 5 minutes (~300 s in the experiment).

→ [Advanced theory](advanced.en.md#speed-behavior)

</details>

<details><summary>6. How does HPA behave with several metrics?</summary>

It computes the desired replica count for each and takes the largest.

→ [Advanced theory](advanced.en.md#several-metrics)

</details>

<details><summary>7. What happens if you create two HPAs on one Deployment?</summary>

`AmbiguousSelector`: both HPAs refuse to work.

→ [Pitfall 4](#4-two-hpas-on-one-object)

</details>

<details><summary>8. Can you set <code>minReplicas: 0</code>?</summary>

Not for an HPA with Resource metrics only (a validation error); scale-to-zero needs Object/External metrics.

→ [Pitfall 6](#6-minreplicas-0)

</details>

<details><summary>9. How does HPA differ from VPA and Cluster Autoscaler?</summary>

HPA changes the replica count, VPA — requests/limits, Cluster Autoscaler — the number of nodes.

→ [Advanced theory](advanced.en.md#vpa-and-cluster-autoscaler)

</details>

<details><summary>10. What is a metrics adapter for?</summary>

For `Pods`, `Object`, `External` metrics: HPA takes them from the custom/external metrics API, not from metrics-server.

→ [Pitfall 7](#7-a-pods-metric-without-an-adapter)

</details>

<details><summary>11. Why should you not set <code>replicas</code> in the manifest with an HPA?</summary>

A repeated apply returns the set number, overriding HPA's decision.

→ [Internals](#internals)

</details>

<details><summary>12. How do you see why HPA does not scale?</summary>

`kubectl describe hpa`: the conditions (`ScalingActive`, `ScalingLimited`) and events; then `kubectl top` and a check of `requests`.

→ [HPA conditions](#hpa-conditions-in-the-experiment)

</details>

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Self-check: [quiz](quiz.en.md), [exercises](exercises.en.md)
- Requests and QoS: [11 · Probes and resources](../11-probes-and-resources/expert.en.md)
- Deployment: [05 · Deployment](../05-deployments/expert.en.md)

## Sources

- [HorizontalPodAutoscaler API (autoscaling/v2)](https://kubernetes.io/docs/reference/kubernetes-api/workload-resources/horizontal-pod-autoscaler-v2/)
- [HPA: algorithm details](https://kubernetes.io/docs/tasks/run-application/horizontal-pod-autoscale/#algorithm-details)
- [Resource metrics pipeline](https://kubernetes.io/docs/tasks/debug/debug-cluster/resource-metrics-pipeline/)
- [Metrics APIs](https://kubernetes.io/docs/tasks/debug/debug-cluster/resource-metrics-pipeline/#metrics-api)
