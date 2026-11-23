---
title: "Quiz: Probes and resources"
lang: en
track: kubernetes
kind: module
doc: quiz
slug: 11-probes-and-resources
part: 2
tags: [probes, liveness-probe, readiness-probe, startup-probe, resources, qos]
status: draft
updated: 2026-11-23
---

# Quiz: Probes and resources

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What does the kubelet do on a liveness probe failure?

- A) Removes the pod from endpoints
- B) Restarts the container
- C) Deletes the pod
- D) Nothing, only writes an event

<details><summary>Answer</summary>

**B.** In the experiment: `Killing … failed liveness probe, will be restarted`, `RESTARTS` grew. Removing a pod from endpoints is the reaction to a readiness failure.
→ [Basics](basics.en.md#liveness-restarting-a-hung-one)

</details>

## 2. 🟢 A pod is `Running` but `0/1`, and no traffic reaches it. What happened?

- A) The readiness probe failed
- B) The liveness probe failed
- C) Memory ran out
- D) The pod is `BestEffort`

<details><summary>Answer</summary>

**A.** The condition `Ready=False`: the pod is excluded from endpoints but not restarted (in the experiment after the file `/tmp/ready` was removed).
→ [Basics](basics.en.md#readiness-ready-to-accept-traffic)

</details>

## 3. 🟢 What are the defaults of `periodSeconds` and `failureThreshold`?

- A) 1 and 1
- B) 10 and 3
- C) 30 and 5
- D) 5 and 10

<details><summary>Answer</summary>

**B.** Also `timeoutSeconds: 1`, `successThreshold: 1`, `initialDelaySeconds: 0`. Visible in `kubectl describe pod`.
→ [Basics](basics.en.md#parameters-and-defaults)

</details>

## 4. 🟢 A pod asks for `requests.cpu: "100"` on a 12-core node. What happens?

- A) The pod starts and is limited
- B) The pod stays `Pending`, `FailedScheduling: Insufficient cpu`
- C) The pod crashes with `OOMKilled`
- D) The scheduler reduces the claim

<details><summary>Answer</summary>

**B.** The scheduler compares requests with the node's allocatable, not with actual load.
→ [Basics](basics.en.md#scheduling-goes-by-requests)

</details>

## 5. 🟡 An application starts for 25 seconds. Liveness: `periodSeconds: 3`, `failureThreshold: 2`. What happens without a startup probe?

- A) The application manages to start
- B) Liveness kills the container before the start ends, the cycle repeats
- C) Kubernetes increases the delay itself
- D) The pod is `Pending`

<details><summary>Answer</summary>

**B.** In the experiment `slow-nostartup` was restarted (`r=1`); with a startup probe (`15 × 3 s`) the start passed without restarts.
→ [Advanced](advanced.en.md#a-slow-start-and-the-startup-probe)

</details>

## 6. 🟡 A pod with a CPU limit of `100m` burns CPU for 20 seconds on an almost idle node. How much CPU does it get?

- A) All 20 seconds: the node is idle
- B) About 2 seconds (10% of a core), throttled in every period
- C) 100 seconds
- D) The pod is killed

<details><summary>Answer</summary>

**B.** `usage_usec 2023496`, `nr_throttled 202` of `nr_periods 202`: the limit works regardless of node load. The pod without a limit got all 20 seconds.
→ [Advanced](advanced.en.md#cpu-throttling)

</details>

## 7. 🟡 Select all correct statements

- [ ] A) Without a readiness probe a container is `Ready` right after start
- [ ] B) `requests.cpu` limits the container's CPU consumption
- [ ] C) `limits.cpu` below `requests.cpu` is rejected on creation
- [ ] D) A `tcpSocket` probe passes if the port accepts a connection

<details><summary>Answer</summary>

**A, C, D.** B is wrong: it is limited by `limits`, while `requests` sets the weight and scheduling.
→ [Advanced](advanced.en.md)

</details>

## 8. 🟡 What QoS class does a pod with only `requests` (no `limits`) have?

- A) `Guaranteed`
- B) `Burstable`
- C) `BestEffort`
- D) There is no class

<details><summary>Answer</summary>

**B.** `Guaranteed` requires requests = limits on all containers for CPU and memory; `BestEffort` — no resources. In the experiment the pod `onlyreq` got `Burstable`.
→ [Basics](basics.en.md#qos-classes)

</details>

## 9. 🔴 Find the flaw

> "Our service's liveness probe checks the database connection: that way we quickly see problems and restart the service."

<details><summary>Answer</summary>

When the database fails all pods fail liveness at once and are restarted, and the service becomes unavailable for long although a restart will not help. Dependencies are checked by readiness (the pod is excluded from traffic but not killed); liveness must answer only "is the application hopelessly hung".
→ [Advanced](advanced.en.md#how-to-design-probes)

</details>

## 10. 🔴 Open question

Explain how `requests` and `limits` turn into kernel settings, what happens when the CPU and memory limits are exceeded and how this relates to QoS.

<details><summary>Key points of a good answer</summary>

- `limits.cpu` → `cpu.max` (the quota: `10000 100000` for `100m`); `requests.cpu` → `cpu.weight` (the weight under contention: 17 for `100m`, 1 for `BestEffort`); `limits.memory` → `memory.max`.
- CPU is compressible: exceeding the limit is throttling (in the experiment `nr_throttled 202`); memory is incompressible: exceeding is OOMKilled, code 137.
- The QoS class is computed from requests and limits and is reflected in `oom_score_adj` (`Guaranteed` −997, `Burstable` 998–999, `BestEffort` 1000): when a node runs short of memory the processes with the larger value are killed first.
- Scheduling goes by requests (`Insufficient cpu`), not by actual consumption.

</details>

→ [Expert](expert.en.md#qos-and-oom_score_adj)
