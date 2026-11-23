---
title: "Observability: logs, events, metrics"
lang: en
track: kubernetes
kind: module
doc: overview
slug: 16-observability
part: 3
levels: [1, 2, 3, 4]
tags: [observability, logs, events, metrics, prometheus, grafana, kube-state-metrics, cadvisor, fluent-bit, promql]
versions: "Kubernetes 1.37.0 (kind v0.34), kubectl 1.36.1, Prometheus v3.16.0 (chart 30.0.0), Grafana 13.3.1 (chart 14.5.0), fluent-bit 4.1, Helm 4.3"
status: draft
updated: 2026-11-23
---

# Observability: logs, events, metrics

🌐 [Русский](overview.ru.md) · **English**

## In short

To understand what is happening in a cluster there are three sources. **Logs** — what containers write to stdout and stderr (`kubectl logs`). **Events** — Kubernetes records of what happened to an object: assigned to a node, image pull failed, restarted. **Metrics** — numeric series over time: load, restart counts, latencies. For logs and metrics "out of the box" you only get the current state; history and charts need separate components: a log collector (for example, fluent-bit), **Prometheus** for metrics and **Grafana** for dashboards.

## Key ideas

- **`kubectl logs` reads files on the node.** The kubelet keeps container output on disk (`/var/log/pods/…`) with rotation (10Mi and 5 files by default in the experiment).
- **For a multi-container pod you must name the container:** `-c`, `--all-containers`, `--prefix`.
- **Logs of a crashed container — `--previous`,** but it does not always work: the kubelet keeps only the last finished container.
- **Events are short-lived** and tied to an object: `kubectl describe`, `kubectl events`, `--field-selector`.
- **Metrics are served by `/metrics` endpoints:** kube-apiserver, the kubelet and cAdvisor, kube-state-metrics, your applications.
- **Prometheus pulls metrics itself** through Kubernetes service discovery; `rate()` needs a window of at least several scrape intervals.
- **Prometheus 3 requires a correct `Content-Type`** on `/metrics`.
- **Cluster-level logs are collected by a DaemonSet** — an agent on each node reads the same files.

## Minimal example

```text
$ kubectl -n learn-16 logs multi --all-containers --prefix --tail=2
[pod/multi/app] app line 19
[pod/multi/app] app line 20
[pod/multi/side] side-log
[pod/multi/side] side-log
$ kubectl -n learn-16 events --for pod/crash --types=Warning
LAST SEEN           TYPE      REASON    OBJECT      MESSAGE
48s (x3 over 86s)   Warning   BackOff   Pod/crash   Back-off restarting failed container c in pod crash_learn-16(…)
```

## When to use and when not

| Question | Source |
|---|---|
| What the application prints right now | `kubectl logs` |
| Why a pod does not start or keeps restarting | events (`describe`, `events`) |
| How the load changed over the last hours | Prometheus + Grafana |
| What was in the logs yesterday, after the pod was deleted | a log collector with storage |
| Number of pods, restarts, node status | kube-state-metrics |

- ❌ Do not keep log history inside pods: after a pod is deleted `kubectl logs` will not help.
- ❌ Do not print secrets to logs: they get copied into aggregators.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | `kubectl logs` and its flags, events, `--previous`, where logs live on a node |
| [2. Advanced theory](advanced.en.md) | log collection with a DaemonSet, Prometheus: targets, PromQL, application metrics, Grafana, mistakes |
| [3. Expert theory](expert.en.md) | reference of metric sources and queries, internals, pitfalls, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn kubernetes 16-observability level 5` |
| 6. Tests | not created yet — `/learn kubernetes 16-observability level 6` |

## Related materials

- Prerequisites: [04 · Pod](../04-pods/overview.en.md), [11 · Probes and resources](../11-probes-and-resources/overview.en.md) (`kubectl top`, requests)
- Metrics for autoscaling: [15 · Autoscaling](../15-autoscaling/overview.en.md)
- Installation with Helm: [13 · Helm](../13-helm/overview.en.md)
- DaemonSet: [12 · Other workloads](../12-workloads/overview.en.md)
- Next: [17 · Security](../17-security/overview.en.md)
- Diagnosis by events and logs: module 21 · Troubleshooting (in the [track](../../overview.en.md) roadmap)
