---
title: "Quiz: autoscaling"
lang: en
track: kubernetes
kind: module
doc: quiz
slug: 15-autoscaling
part: 3
tags: [hpa, autoscaling, metrics-server, requests]
status: draft
updated: 2026-11-23
---

# Quiz: autoscaling

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What does the HorizontalPodAutoscaler change?

- A) The containers' `requests`
- B) The replica count of the target object
- C) The number of nodes
- D) The container image

<details><summary>Answer</summary>

**B.** `requests` are picked by VPA, nodes are added by Cluster Autoscaler.
→ [Overview](overview.en.md)

</details>

## 2. 🟢 What is needed for an HPA on CPU?

- A) Only the HPA manifest
- B) metrics-server and CPU `requests` on the containers
- C) Prometheus
- D) Ingress

<details><summary>Answer</summary>

**B.** Without `requests` `TARGETS` will show `<unknown>` (`missing request for cpu`).
→ [Basics](basics.en.md#beginner-mistakes)

</details>

## 3. 🟢 1 pod, utilization 250%, target 50%, `maxReplicas: 5`. How many replicas will HPA want?

- A) 2
- B) 3
- C) 5
- D) 10

<details><summary>Answer</summary>

**C.** `ceil(1 × 250 / 50) = 5`; the limit of 5 does not interfere. In the experiment the replicas grew from 1 to 5.
→ [Basics](basics.en.md#load-and-scaling)

</details>

## 4. 🟢 What is the CPU utilization percentage in HPA computed from?

- A) From `limits`
- B) From `requests`
- C) From the number of node cores
- D) From the cluster average

<details><summary>Answer</summary>

**B.** In the experiment 143m with `requests: 200m` gave 71%.
→ [Basics](basics.en.md#the-first-hpa)

</details>

## 5. 🟡 5 pods, utilization 71%, target 50%, `maxReplicas: 5`. What will HPA show?

- A) Scales to 8
- B) Stays at 5, the condition `ScalingLimited=True (TooManyReplicas)`
- C) An error
- D) Scales down

<details><summary>Answer</summary>

**B.** The formula gives `ceil(5 × 71 / 50) = 8`, but the ceiling is 5.
→ [Basics](basics.en.md#load-and-scaling)

</details>

## 6. 🟡 You manually set `scale deploy d2 --replicas=3` with an HPA `min=1 max=3` and zero load. What happens?

- A) 1 replica returns at once
- B) The replicas stay at 3 for about five minutes (the stabilization window), then become 1
- C) The replicas stay at 3 forever
- D) The HPA is deleted

<details><summary>Answer</summary>

**B.** In the experiment 3 replicas held until ~260 s, and at ~315 s there was 1.
→ [Advanced](advanced.en.md#the-scale-down-stabilization-window)

</details>

## 7. 🟡 How will HPA behave with cpu and memory metrics at once?

- A) Uses only the first
- B) Computes replicas for each and takes the largest
- C) Takes the average
- D) Rejects such an HPA

<details><summary>Answer</summary>

**B.** In the experiment the table showed both targets: `cpu: 0%/50%, memory: 12276Ki/100Mi`.
→ [Advanced](advanced.en.md#several-metrics)

</details>

## 8. 🟡 Select all correct statements

- [ ] A) `minReplicas: 0` is rejected for an HPA with Resource metrics only
- [ ] B) Two HPAs on one Deployment give `AmbiguousSelector`
- [ ] C) A `Pods` metric works without an adapter
- [ ] D) `kubectl autoscale` creates an HPA

<details><summary>Answer</summary>

**A, B, D.** C is wrong: a custom metrics API adapter is needed (`no custom metrics API (custom.metrics.k8s.io) registered`).
→ [Advanced](advanced.en.md#errors-and-limitations)

</details>

## 9. 🔴 Find the flaw

> "We removed `requests` so that pods are not limited, and HPA will keep working on CPU."

<details><summary>Answer</summary>

Without `requests` the utilization percentage is undefined: `FailedGetResourceMetric … missing request for cpu`, `TARGETS` is `<unknown>`, and there is no scaling. `requests` is the denominator of the metric, not a limit.
→ [Pitfall 1](expert.en.md#1-unknown-in-targets)

</details>

## 10. 🔴 What happens on a repeated `kubectl apply` of a Deployment manifest with `replicas: 1` if HPA raised 5 replicas?

- A) Nothing
- B) The replica count returns to 1 until the next HPA cycle, then HPA raises it again
- C) The HPA is deleted
- D) The apply is rejected

<details><summary>Answer</summary>

**B.** The `replicas` field in the manifest overrides HPA's decision, which is why it is removed from the manifest (inferred from the documentation; not reproduced in the experiment).
→ [Internals](expert.en.md#internals)

</details>

## 11. 🔴 Open question

Explain how HPA works and describe how you would configure it for a loaded service.

<details><summary>Key points of a good answer</summary>

- The controller reads metrics each cycle and computes `ceil(current × metric / target)`, then limits it by `minReplicas`/`maxReplicas` and the `behavior` policies.
- metrics-server and `requests` are needed; the percentage is computed from `requests`.
- A target of 50–70%, `maxReplicas` to fit the budget, a downward stabilization window against a "sawtooth", `scaleUp` for spikes.
- For business metrics an adapter (custom/external); VPA and Cluster Autoscaler separately.

</details>

→ [Expert](expert.en.md#trade-offs-and-alternatives)
