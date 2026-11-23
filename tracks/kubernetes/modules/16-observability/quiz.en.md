---
title: "Quiz: observability"
lang: en
track: kubernetes
kind: module
doc: quiz
slug: 16-observability
part: 3
tags: [logs, events, prometheus, promql]
status: draft
updated: 2026-11-23
---

# Quiz: observability

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What does `kubectl logs multi` do for a two-container pod without `-c`?

- A) Shows the logs of both containers
- B) Shows one container and reports `Defaulted container`
- C) Returns an error
- D) Shows events

<details><summary>Answer</summary>

**B.** In the experiment: `Defaulted container "app" out of: app, side`. For both use `--all-containers`.
→ [Basics](basics.en.md#logs-kubectl-logs)

</details>

## 2. 🟢 How do you see the logs of all containers of a pod, with the source marked?

- A) `kubectl logs P --all-containers --prefix`
- B) `kubectl logs P -f`
- C) `kubectl describe pod P`
- D) `kubectl events`

<details><summary>Answer</summary>

**A.** A prefix like `[pod/multi/app]`.
→ [Basics](basics.en.md#logs-kubectl-logs)

</details>

## 3. 🟢 Where does the kubelet keep container logs on a node?

- A) In etcd
- B) In `/var/log/pods/<ns>_<pod>_<uid>/<container>/`
- C) In a ConfigMap
- D) In the kubelet's memory

<details><summary>Answer</summary>

**B.** Links to these files lie in `/var/log/containers`.
→ [Basics](basics.en.md#where-logs-are-on-a-node)

</details>

## 4. 🟢 Where is the cause of `FailedScheduling` or `BackOff`?

- A) In `kubectl logs`
- B) In the object's events: `kubectl describe`/`kubectl events`
- C) In `kubectl top`
- D) In the API server logs

<details><summary>Answer</summary>

**B.** There may be no container at all, and the event explains the cause.
→ [Basics](basics.en.md#events)

</details>

## 5. 🟡 Why do you need a node log collector (a DaemonSet) if there is `kubectl logs`?

- A) `kubectl logs` does not work without an agent
- B) To keep history: logs are deleted with the pod and rotated
- C) To speed up `kubectl logs`
- D) To replace events

<details><summary>Answer</summary>

**B.** The files lie on the node and are deleted with the pod; the agent reads them and sends them to storage.
→ [Advanced](advanced.en.md#log-collection-with-a-node-daemon)

</details>

## 6. 🟡 A log agent DaemonSet shows `DESIRED 2` on a three-node cluster. What is most likely?

- A) One node is unavailable
- B) The control-plane has a taint and the DaemonSet has no toleration
- C) A pod limit
- D) An image configuration error

<details><summary>Answer</summary>

**B.** That is what happened in the experiment: `fb 2 2 2`.
→ [Advanced](advanced.en.md#log-collection-with-a-node-daemon)

</details>

## 7. 🟡 What is `container_cpu_usage_seconds_total` and how do you read it?

- A) A gauge, the value is read as is
- B) A counter, read with `rate()`
- C) An event
- D) A label

<details><summary>Answer</summary>

**B.** A counter: total CPU time; its growth rate is cores per second.
→ [Advanced](advanced.en.md#promql-examples)

</details>

## 8. 🟡 The scrape interval is 1 minute and `rate(...[1m])` is empty. What do you fix?

- A) Increase the window, for example to `[5m]`
- B) Restart Prometheus
- C) Delete the metric
- D) Raise the interval to 1 hour

<details><summary>Answer</summary>

**A.** The window needs at least two samples; in the experiment `[5m]` returned data.
→ [Advanced](advanced.en.md#the-rate-window-and-the-scrape-interval)

</details>

## 9. 🟡 Select all correct statements

- [ ] A) Prometheus polls targets itself (pull)
- [ ] B) kube-state-metrics measures container CPU
- [ ] C) cAdvisor is built into the kubelet
- [ ] D) Prometheus 3 rejects a target without `Content-Type`

<details><summary>Answer</summary>

**A, C, D.** B is wrong: consumption is served by cAdvisor, while kube-state-metrics serves object state.
→ [Advanced](advanced.en.md#prometheus-internals-and-installation)

</details>

## 10. 🔴 Find the flaw

> "The container crashed, so `kubectl logs --previous` will always show the cause."

<details><summary>Answer</summary>

The kubelet removes earlier instances; for a pod in `CrashLoopBackOff` the command returned `unable to retrieve container logs` in the experiment. You need events, ordinary `kubectl logs` and external log storage.
→ [Gotcha 1](expert.en.md#1---previous-does-not-always-return-logs)

</details>

## 11. 🔴 A Prometheus target is `down`: `non-compliant scrape target sending blank Content-Type`. What do you do?

- A) Disable Prometheus
- B) Make the application send a `Content-Type` (for example, `text/plain`)
- C) Increase `scrape_interval`
- D) Add a toleration

<details><summary>Answer</summary>

**B.** In the experiment, after renaming the file to `metrics.txt`, the server started sending `text/plain` and the metric appeared.
→ [Gotcha 3](expert.en.md#3-prometheus-3-and-content-type)

</details>

## 12. 🔴 Open question

How would you build observability for an application in Kubernetes: what to collect and with what?

<details><summary>Key points of a good answer</summary>

- Logs: stdout/stderr → a DaemonSet agent → storage (search, history); `kubectl logs` for quick checks.
- Events: `describe`/`events` for first diagnosis; not an archive.
- Metrics: Prometheus (pull), kube-state-metrics, cAdvisor, the application's `/metrics`; Grafana for dashboards.
- Alerts on symptoms; `rate()` with a sufficient window; volumes for storage.

</details>

→ [Expert](expert.en.md#trade-offs-and-alternatives)
