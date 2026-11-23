---
title: "Autoscaling: basic theory"
lang: en
track: kubernetes
kind: module
doc: basics
slug: 15-autoscaling
part: 3
tags: [hpa, autoscaling, metrics-server, kubectl-top, requests]
versions: "Kubernetes 1.37.0 (kind v0.34), metrics-server v0.9.1, kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Autoscaling: basic theory

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [05 · Deployment](../05-deployments/overview.en.md), [11 · Probes and resources](../11-probes-and-resources/overview.en.md) · **Time:** ~2 h

All examples were run in kind (Kubernetes 1.37.0), namespace `learn-15`. The load is created by a pod that sends requests to the application in a loop.

## Why this is needed

Load changes: more requests by day, fewer at night. Keeping the maximum replicas all the time is expensive, keeping the minimum is risky. HPA watches the metrics and picks the replica count between `minReplicas` and `maxReplicas` itself.

## metrics-server and `kubectl top`

Pod CPU and memory are collected by **metrics-server**: it polls the kubelet on every node and serves the data through the Metrics API (`metrics.k8s.io`). It is not installed in kind ([module 11](../11-probes-and-resources/basics.en.md#actual-consumption-kubectl-top) shows `Metrics API not available`). Installation with the manifest from a release:

```text
$ kubectl apply -f https://github.com/kubernetes-sigs/metrics-server/releases/latest/download/components.yaml
```

The experiment used metrics-server v0.9.1; in kind the container argument `--kubelet-insecure-tls` was added so that it accepts the kubelet's self-signed certificate (you should not do this in production).

```text
$ kubectl -n kube-system rollout status deploy/metrics-server
deployment "metrics-server" successfully rolled out
$ kubectl get apiservice v1beta1.metrics.k8s.io
NAME                     SERVICE                      AVAILABLE   AGE
v1beta1.metrics.k8s.io   kube-system/metrics-server   True        59s
$ kubectl top nodes
NAME                  CPU(cores)   CPU(%)   MEMORY(bytes)   MEMORY(%)
learn-control-plane   127m         1%       713Mi           2%
$ kubectl -n learn-15 top pod --containers
POD                           NAME         CPU(cores)   MEMORY(bytes)
php-apache-5899f79df5-f9jkx   php-apache   1m           11Mi
```

Metrics do not appear at once: after the start it takes about a minute. The data itself is a `PodMetrics` object:

```text
$ kubectl get --raw /apis/metrics.k8s.io/v1beta1/namespaces/learn-15/pods
PodMetricsList d2-76fb4597fc-n22fg 15.903s {'cpu': '0', 'memory': '10560Ki'}
```

(`15.903s` is the measurement window.)

> ⚠️ Verify: the work of metrics-server in kind without `--kubelet-insecure-tls` was not tested.

## The application for the experiment

The Deployment `php-apache` with mandatory `requests` (without them HPA cannot compute a percentage):

```yaml
spec:
  template:
    spec:
      containers:
        - name: php-apache
          image: registry.k8s.io/hpa-example
          resources:
            limits: {cpu: 500m}
            requests: {cpu: 200m}
```

And a Service `php-apache` on port 80.

## The first HPA

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

The quick form: `kubectl autoscale deploy d2 --min=1 --max=3 --cpu=60%` creates the same object (`averageUtilization: 60`).

Without load HPA shows:

```text
$ kubectl -n learn-15 get hpa php-apache
NAME         REFERENCE               TARGETS       MINPODS   MAXPODS   REPLICAS   AGE
php-apache   Deployment/php-apache   cpu: 0%/50%   1         5         1          40s
```

`TARGETS` is the current value against the target: CPU utilization is computed from `requests` (200m), so 100m is 50%.

## Load and scaling

Start the load:

```text
$ kubectl -n learn-15 run load --image=busybox:1.37 --restart=Never -- sh -c "while sleep 0.01; do wget -q -O- http://php-apache >/dev/null; done"
```

Observation every 15 seconds:

```text
t=15  cpu 28%/50%   replicas=1
t=45  cpu 250%/50%  replicas=1
t=60  cpu 250%/50%  replicas=5
t=75  cpu 76%/50%   replicas=5
t=90  cpu 57%/50%   replicas=5
t=105 cpu 67%/50%   replicas=5
```

When HPA saw 250% against a target of 50% it raised the replicas from 1 to 5 (the maximum) in one step. The formula: `desired = ceil(current replicas × current metric / target)`. For one pod this is `ceil(1 × 250 / 50) = 5`. Once the load was spread over five pods utilization dropped to 57–76%; it is still above 50%, but the replica count had already hit `maxReplicas`:

```text
$ kubectl -n learn-15 describe hpa php-apache
Metrics:                                               ( current / target )
  resource cpu on pods  (as a percentage of request):  71% (143m) / 50%
Min replicas:                                          1
Max replicas:                                          5
Deployment pods:       5 current / 5 desired
Conditions:
  ScalingLimited  True    TooManyReplicas  the desired replica count is more than the maximum replica count
Events:
  Normal   SuccessfulRescale   New size: 5; reason: cpu resource utilization (percentage of request) above target
```

At `71%` and 5 pods the formula would give `ceil(5 × 71 / 50) = 8`, but the ceiling is 5 (`TooManyReplicas`).

Remove the load (`kubectl delete pod load`):

```text
t=45 replicas=5
t=60 replicas=2
t=75 replicas=1
```

The replicas returned to 1 in about a minute (in this experiment the stabilization window was shortened to 30 seconds — more in [advanced theory](advanced.en.md#speed-behavior)). By default scaling down is noticeably slower.

## Beginner mistakes

- ❌ An HPA on a Deployment without `requests`:

```text
$ kubectl -n learn-15 get hpa nor
NAME   REFERENCE        TARGETS              MINPODS   MAXPODS   REPLICAS
nor    Deployment/nor   cpu: <unknown>/50%   1         3         1
  ScalingActive  False   FailedGetResourceMetric  … missing request for cpu in container nginx of Pod nor-b475dd579-hwg4c
```

  ✅ Always set `requests` on the containers you scale by.
- ❌ An HPA without metrics-server → ✅ install it first and check `kubectl top`.
- ❌ Expecting an instant reaction: in the first minutes the events `FailedGetResourceMetric … no metrics returned` are normal until metrics-server has gathered data.
- ❌ Setting `minReplicas: 0` → ✅ it is rejected (see [advanced theory](advanced.en.md#errors-and-limitations)).

## Related materials

- Next: [advanced theory](advanced.en.md) — `behavior`, several metrics, custom metrics, VPA and Cluster Autoscaler
- Previous module: [14 · Kustomize](../14-kustomize/overview.en.md)
- Requests and limits: [11 · Probes and resources](../11-probes-and-resources/overview.en.md)
- Deployment and manual scaling: [05 · Deployment](../05-deployments/basics.en.md#scaling)

## Sources

- [Horizontal Pod Autoscaling](https://kubernetes.io/docs/tasks/run-application/horizontal-pod-autoscale/)
- [HorizontalPodAutoscaler Walkthrough](https://kubernetes.io/docs/tasks/run-application/horizontal-pod-autoscale-walkthrough/)
- [Resource metrics pipeline](https://kubernetes.io/docs/tasks/debug/debug-cluster/resource-metrics-pipeline/)
- [metrics-server](https://github.com/kubernetes-sigs/metrics-server#readme)
- [kubectl autoscale](https://kubernetes.io/docs/reference/kubectl/generated/kubectl_autoscale/)
