---
title: "Autoscaling: advanced theory"
lang: en
track: kubernetes
kind: module
doc: advanced
slug: 15-autoscaling
part: 3
tags: [hpa, behavior, stabilization-window, custom-metrics, vpa, cluster-autoscaler, autoscaling]
versions: "Kubernetes 1.37.0 (kind v0.34), metrics-server v0.9.1, kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Autoscaling: advanced theory

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on [basic theory](basics.en.md): the metrics-server installation and the first HPA are not repeated.

## Speed: `behavior`

The scale-up and scale-down behavior is set by the `behavior` block. In the experiment the HPA was created with one parameter:

```yaml
behavior:
  scaleDown: {stabilizationWindowSeconds: 30}
```

`kubectl describe hpa` shows the final values completed with defaults:

```text
Behavior:
  Scale Up:
    Stabilization Window: 0 seconds
    Select Policy: Max
    Policies:
      - Type: Pods     Value: 4    Period: 15 seconds
      - Type: Percent  Value: 100  Period: 15 seconds
  Scale Down:
    Stabilization Window: 30 seconds
    Select Policy: Max
    Policies:
      - Type: Percent  Value: 100  Period: 15 seconds
```

Up by default: a stabilization window of 0, up to 4 pods **or** 100% can be added in 15 seconds (the policy with the larger effect is chosen: `Select Policy: Max`). That is why in the experiment the replicas grew from 1 to 5 in one step (+4). Down: shrinking up to 100% per 15 seconds is allowed, and the stabilization window is what you set (30 s).

### The scale-down stabilization window

The window keeps the replica count from dropping while the window held higher recommendations. To see the default, three replicas were set by hand on the Deployment `d2` with an HPA (`--min=1 --max=3`):

```text
$ kubectl -n learn-15 scale deploy d2 --replicas=3
t=+95s  d2 replicas=3
t=+205s d2 replicas=3
t=+260s d2 replicas=3
t=+315s d2 replicas=1
```

There is no load, but HPA kept 3 replicas for about five minutes and only then dropped to `minReplicas`. This is the default stabilization window (300 seconds).

> ⚠️ Verify: the exact default (300 s) and the absence of a window for `scaleUp` are taken from the Kubernetes documentation and agree with the observation (the pods shrank between 260 and 315 seconds); the `scaleDown` parameter was not printed separately by describe without `behavior`.

## Several metrics

An HPA can take several metrics into account: it computes the desired replica count for each and takes the **largest**.

```yaml
metrics:
  - type: Resource
    resource: {name: cpu, target: {type: Utilization, averageUtilization: 50}}
  - type: Resource
    resource: {name: memory, target: {type: AverageValue, averageValue: 100Mi}}
```

```text
$ kubectl -n learn-15 get hpa multi
NAME    REFERENCE               TARGETS                              MINPODS   MAXPODS   REPLICAS
multi   Deployment/php-apache   cpu: 0%/50%, memory: 12276Ki/100Mi   1         4         1
```

Target types: `Utilization` (a percentage of `requests`, only for Resource) and `AverageValue` (an absolute value per pod).

## Metrics not from metrics-server

The metric types `Pods`, `Object`, `External` come not from the Metrics API but from the custom/external metrics APIs provided by an adapter (Prometheus Adapter, KEDA, etc.). Without an adapter HPA does not work:

```yaml
metrics:
  - type: Pods
    pods:
      metric: {name: http_requests_per_second}
      target: {type: AverageValue, averageValue: "10"}
```

```text
$ kubectl -n learn-15 get hpa custom
NAME     REFERENCE       TARGETS         MINPODS   MAXPODS   REPLICAS
custom   Deployment/d2   <unknown>/10    1         3         1
  ScalingActive  False   FailedGetPodsMetric  the HPA was unable to compute the replica count: unable to get metric http_requests_per_second: unable to fetch metrics from custom metrics API: no custom metrics API (custom.metrics.k8s.io) registered
```

> ⚠️ Verify: installing an adapter and HPA working on custom/external metrics were not run (the cluster had no adapter).

## Errors and limitations

**Two HPAs on one target.** A second HPA on the same Deployment breaks both:

```text
ScalingActive  False  AmbiguousSelector  pods by selector app=d2 are controlled by more than one HPA (e.g. [learn-15/custom learn-15/d2])
```

After the extra HPA was deleted the condition went away.

**`minReplicas: 0`.** For an HPA with Resource metrics only it is rejected:

```text
The HorizontalPodAutoscaler "nor" is invalid: spec.metrics: Forbidden: must specify at least one Object or External metric to support scaling to zero replicas
```

The message says that scaling to zero requires at least one `Object` or `External` metric.

> ⚠️ Verify: the conditions for enabling scale-to-zero (feature gate and version) were not checked.

**No `requests`.** The metric is `<unknown>`, the condition `ScalingActive=False (FailedGetResourceMetric)`: `missing request for cpu in container nginx of Pod …`.

**Manual `scale`.** HPA does not cancel a manually set replica count at once: it was held until the end of the stabilization window (see above), and then HPA returned the count within `minReplicas`/`maxReplicas`.

## VPA and Cluster Autoscaler

| Component | What it does | How it is installed |
|---|---|---|
| HPA | the replica count | built into Kubernetes |
| VPA (Vertical Pod Autoscaler) | picks pod `requests` and `limits` | separately (CRDs and controllers) |
| Cluster Autoscaler | adds and removes nodes when pods do not fit | separately, with cloud integration |

> ⚠️ Verify: VPA and Cluster Autoscaler were not run in this material (kind has no cloud nodes); the descriptions are from the documentation. The documentation does not recommend using HPA and VPA together on the same metric (CPU).

## Best practices

- Always set `requests` on the containers you scale by.
- Choose a target that leaves headroom for a spike: 50–70% utilization is a typical start.
- `maxReplicas` is insurance for the budget and downstream systems.
- Do not set `replicas` in a manifest you apply repeatedly: every deploy will reset the replica count.
- For sharp upward spikes — loosen the `scaleUp` policies; to protect against a "sawtooth" — the downward stabilization window.
- Check `kubectl describe hpa`: the conditions and events explain why HPA does not scale.

## Common mistakes

- ❌ Two HPAs on one Deployment → ✅ one HPA with several metrics.
- ❌ An HPA on `Pods`/`External` without an adapter → ✅ install the adapter first and check the API.
- ❌ A CPU target of 100% → ✅ keep headroom for spikes.
- ❌ A manual `scale` instead of changing the HPA bounds → ✅ change `minReplicas`/`maxReplicas`.

## Comparison with other tracks

- [Linux](../../../linux/overview.en.md): CPU `limits` turn into `cpu.max` and throttling ([module 11](../11-probes-and-resources/advanced.en.md#cpu-throttling)); HPA computes a percentage of `requests`, not of `limits`.
- [C#](../../../csharp/recipes/background-jobs/overview.en.md): queue workers are often scaled by queue length (an external metric, for example KEDA), not by CPU.

## Related materials

- Previous level: [basic theory](basics.en.md)
- Next: [expert theory](expert.en.md)
- Requests, limits and QoS: [11 · Probes and resources](../11-probes-and-resources/advanced.en.md)
- Rolling update when the replica count changes: [05 · Deployment](../05-deployments/advanced.en.md#rolling-update-step-by-step)

## Sources

- [HPA: algorithm details](https://kubernetes.io/docs/tasks/run-application/horizontal-pod-autoscale/#algorithm-details)
- [HPA: configurable scaling behavior](https://kubernetes.io/docs/tasks/run-application/horizontal-pod-autoscale/#configurable-scaling-behavior)
- [HPA: support for multiple metrics](https://kubernetes.io/docs/tasks/run-application/horizontal-pod-autoscale-walkthrough/#autoscaling-on-multiple-metrics-and-custom-metrics)
- [Vertical Pod Autoscaling](https://kubernetes.io/docs/concepts/workloads/autoscaling/vertical-pod-autoscale/)
- [Node autoscaling](https://kubernetes.io/docs/concepts/cluster-administration/node-autoscaling/)
