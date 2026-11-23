---
title: "Exercises: autoscaling"
lang: en
track: kubernetes
kind: module
doc: exercises
slug: 15-autoscaling
part: 3
tags: [hpa, autoscaling, metrics-server, requests]
status: draft
updated: 2026-11-23
---

# Exercises: autoscaling

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself; hints and solutions are under spoilers. You need a kind cluster and a namespace: `kubectl create namespace learn-15`. Clean up afterwards: `kubectl delete namespace learn-15`. The exercises need metrics-server (installation is in [basic theory](basics.en.md#metrics-server-and-kubectl-top)).

## 1. 🟢 metrics-server and `kubectl top`

**Task:** install metrics-server (in kind add `--kubelet-insecure-tls`) and make sure `kubectl top nodes` and `kubectl top pod` return data.

**Expected result:** the APIService `v1beta1.metrics.k8s.io` is `AVAILABLE True`, `kubectl top nodes` shows CPU and memory.

<details><summary>Hint</summary>

`kubectl get apiservice v1beta1.metrics.k8s.io`; data appears about a minute after the start.

</details>

<details><summary>Solution</summary>

```text
$ kubectl get apiservice v1beta1.metrics.k8s.io
NAME                     SERVICE                      AVAILABLE   AGE
v1beta1.metrics.k8s.io   kube-system/metrics-server   True        59s
$ kubectl top nodes
NAME                  CPU(cores)   CPU(%)   MEMORY(bytes)   MEMORY(%)
learn-control-plane   127m         1%       713Mi           2%
```

</details>

## 2. 🟢 The first HPA under load

**Task:** deploy `php-apache` with `requests.cpu: 200m`, create an HPA (`cpu 50%`, 1–5 replicas) and a load. Show the replicas grow, then remove the load.

**Expected result:** `cpu: 250%/50%` and 5 replicas under load; about a minute after it is removed, 1 replica (with a `scaleDown` stabilization window of 30 s).

<details><summary>Hint</summary>

Load: `kubectl run load --image=busybox:1.37 --restart=Never -- sh -c "while sleep 0.01; do wget -q -O- http://php-apache >/dev/null; done"`. Observation: `kubectl get hpa php-apache` every 15 seconds.

</details>

<details><summary>Solution</summary>

```text
t=45  cpu 250%/50%  replicas=1
t=60  cpu 250%/50%  replicas=5
t=75  cpu 76%/50%   replicas=5
… after kubectl delete pod load:
t=60 replicas=2
t=75 replicas=1
```

</details>

## 3. 🟡 Reading `describe hpa`

**Task:** for the HPA from exercise 2 find in `kubectl describe hpa` the final `behavior` policies, the `ScalingLimited` condition and the `SuccessfulRescale` event. Compute by the formula how many replicas were needed at 71% and 5 pods.

**Expected result:** `ScalingLimited=True (TooManyReplicas)`; the calculation `ceil(5 × 71 / 50) = 8`, limited to 5.

<details><summary>Hint</summary>

The `Conditions` and `Events` sections.

</details>

<details><summary>Solution</summary>

```text
ScalingLimited  True  TooManyReplicas  the desired replica count is more than the maximum replica count
Normal  SuccessfulRescale  New size: 5; reason: cpu resource utilization (percentage of request) above target
```

</details>

## 4. 🟡 No `requests` and `minReplicas: 0`

**Task:** create a Deployment `nor` without `requests` and an HPA on it. Find the cause of `<unknown>`. Then try `minReplicas: 0`.

**Expected result:** `missing request for cpu in container nginx of Pod …`; the error `must specify at least one Object or External metric to support scaling to zero replicas`.

<details><summary>Hint</summary>

`kubectl autoscale deploy nor --min=1 --max=3 --cpu=50%`; `kubectl patch hpa nor --type merge -p '{"spec":{"minReplicas":0}}'`.

</details>

<details><summary>Solution</summary>

```text
nor    Deployment/nor   cpu: <unknown>/50%   1   3   1
  ScalingActive  False   FailedGetResourceMetric  … missing request for cpu in container nginx of Pod nor-b475dd579-hwg4c
The HorizontalPodAutoscaler "nor" is invalid: spec.metrics: Forbidden: must specify at least one Object or External metric to support scaling to zero replicas
```

</details>

## 5. 🔴 Two metrics, a duplicate HPA and a custom metric

**Task:** (a) create an HPA with the metrics cpu 50% and memory `100Mi`. (b) Create a second HPA on the same Deployment and find `AmbiguousSelector`. (c) Create an HPA with a `Pods` metric `http_requests_per_second` and find the cause of `FailedGetPodsMetric`.

**Expected result:** (a) `cpu: 0%/50%, memory: 12276Ki/100Mi`; (b) `AmbiguousSelector`; (c) `no custom metrics API (custom.metrics.k8s.io) registered`.

<details><summary>Hint</summary>

(b) two HPAs with one `scaleTargetRef`; (c) `type: Pods`, `pods.metric.name`, `target.type: AverageValue`.

</details>

<details><summary>Solution</summary>

```text
multi   Deployment/php-apache   cpu: 0%/50%, memory: 12276Ki/100Mi   1   4   1
ScalingActive  False  AmbiguousSelector  pods by selector app=d2 are controlled by more than one HPA (e.g. [learn-15/custom learn-15/d2])
ScalingActive  False  FailedGetPodsMetric  … unable to fetch metrics from custom metrics API: no custom metrics API (custom.metrics.k8s.io) registered
```

</details>
