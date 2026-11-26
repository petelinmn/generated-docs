---
title: "Autoscaling"
lang: en
track: kubernetes
kind: module
doc: overview
slug: 15-autoscaling
part: 3
levels: [1, 2, 3, 4]
tags: [hpa, autoscaling, metrics-server, vpa, cluster-autoscaler, requests, kubectl-top]
versions: "Kubernetes 1.37.0 (kind v0.34), metrics-server v0.9.1, kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Autoscaling

🌐 [Русский](overview.ru.md) · **English**

## In short

You do not have to pick the replica count by hand: the **HorizontalPodAutoscaler (HPA)** changes `replicas` of a Deployment (or another scalable object) by itself based on load metrics. CPU and memory metrics need **metrics-server**, which serves the Metrics API (`kubectl top` runs on it too). Other kinds of autoscaling — **VPA** (choosing requests/limits) and **Cluster Autoscaler** (the number of nodes) — are installed separately.

## Key ideas

- **HPA compares the current metric with the target** and computes the needed replica count: `ceil(current replicas × current / target)`. In the experiment 1 pod at 250% against a target of 50% → 5 replicas.
- **CPU utilization is computed from `requests`.** HPA does not work without `requests`: `missing request for cpu`.
- **metrics-server is required.** Without it `kubectl top` and HPA on CPU/memory get no data.
- **`minReplicas` cannot be 0** for an HPA with Resource metrics only; `maxReplicas` caps growth (`TooManyReplicas`).
- **`behavior` controls the speed:** by default up fast (up to +4 pods or +100% per 15 s), down slowly: a 5-minute stabilization window.
- **`Pods`, `Object`, `External` metrics need an adapter** (custom/external metrics API); without it `ScalingActive=False`.
- **One Deployment — one HPA:** two HPAs on one object give `AmbiguousSelector`.

## Minimal example

```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata: {name: php-apache, namespace: learn-15}
spec:
  scaleTargetRef: {apiVersion: apps/v1, kind: Deployment, name: php-apache}
  minReplicas: 1
  maxReplicas: 5
  metrics:
    - type: Resource
      resource:
        name: cpu
        target: {type: Utilization, averageUtilization: 50}
```

```text
$ kubectl -n learn-15 get hpa php-apache
NAME         REFERENCE               TARGETS       MINPODS   MAXPODS   REPLICAS
php-apache   Deployment/php-apache   cpu: 250%/50%   1       5         1
… 15 s later
php-apache   Deployment/php-apache   cpu: 250%/50%   1       5         5
```

## When to use and when not

| Situation | Solution |
|---|---|
| A stateless service with variable load | HPA |
| We do not know the right requests | VPA (in recommendation mode) |
| Not enough nodes | Cluster Autoscaler |
| A StatefulSet with a strict order | carefully: scaling also goes in order |

- ❌ Do not enable HPA without `requests`: the metric will be `<unknown>`.
- ❌ Do not set `replicas` in a manifest that is applied together with an HPA on every deploy: you will reset the replica count.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | metrics-server, `kubectl top`, the first HPA, how replicas are computed |
| [2. Advanced theory](advanced.en.md) | `behavior`, stabilization windows, several metrics, custom/external metrics, errors, VPA and Cluster Autoscaler |
| [3. Expert theory](expert.en.md) | HPA field and condition reference, the algorithm, pitfalls, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn kubernetes 15-autoscaling level 5` |
| 6. Tests | not created yet — `/learn kubernetes 15-autoscaling level 6` |

## Related materials

- Prerequisites: [05 · Deployment](../05-deployments/overview.en.md), [11 · Probes and resources](../11-probes-and-resources/overview.en.md) (requests and limits)
- Previous module: [14 · Kustomize](../14-kustomize/overview.en.md)
- Next: [16 · Observability](../16-observability/overview.en.md)
- Manual scaling: [05 · Deployment](../05-deployments/advanced.en.md)
- Pod placement on nodes: [18 · Pod scheduling](../18-scheduling/overview.en.md)
- [Limits of horizontal scaling](../../../system-design/modules/02-scalability-and-load-balancing/overview.en.md)
