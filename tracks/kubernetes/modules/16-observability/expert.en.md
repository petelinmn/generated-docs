---
title: "Observability: expert"
lang: en
track: kubernetes
kind: module
doc: expert
slug: 16-observability
part: 3
tags: [observability, prometheus, cadvisor, kube-state-metrics, kubelet, cri-logs, events, promql]
versions: "Kubernetes 1.37.0 (kind v0.34), Prometheus v3.16.0 (chart prometheus 30.0.0), kube-state-metrics 2.20.0, Grafana 13.3.1, fluent-bit 4.1, kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Observability: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on [basic](basics.en.md) and [advanced](advanced.en.md) theory. What was confirmed by an experiment in kind (Kubernetes 1.37.0) is marked "in the experiment".

## Reference

### Where to look

| What you need | Source | How to get it |
|---|---|---|
| Container output | kubelet | `kubectl logs …` |
| What happened to an object | API events | `kubectl describe`, `kubectl events`, `kubectl get events` |
| API server health | `/livez`, `/readyz`, `/healthz` | `kubectl get --raw /readyz?verbose` (in the experiment `readyz check passed`, `ok`) |
| API server metrics | `/metrics` | `kubectl get --raw /metrics` (in the experiment 303 `# TYPE` lines) |
| Container consumption | kubelet: `/metrics/cadvisor`, `/metrics/resource` | `kubectl get --raw /api/v1/nodes/<node>/proxy/metrics/cadvisor` |
| Kubelet configuration | `/configz` | `kubectl get --raw /api/v1/nodes/<node>/proxy/configz` |
| Object state as metrics | kube-state-metrics | Prometheus |
| Node OS metrics | node-exporter | Prometheus |
| Application metrics | the application's `/metrics` | Prometheus |

### Flags and commands

| Command | Purpose |
|---|---|
| `kubectl logs P -c C --tail=N --since=5s --timestamps` | log selection |
| `kubectl logs --all-containers --prefix` / `-l selector` | several containers and pods |
| `kubectl logs --previous` | the previous instance of the container (in the experiment unreliable for `CrashLoopBackOff`) |
| `kubectl events --for pod/P --types=Warning` | events of one object |
| `kubectl get events --field-selector involvedObject.name=P,type=Normal --sort-by=.lastTimestamp` | filter and sort |

> ⚠️ Verify: the flags `-f`, `--since-time`, `--limit-bytes` and `-w` for events are taken from the documentation and were not run.

### Prometheus settings in the chart (in the experiment)

| Parameter | Value |
|---|---|
| `scrape_interval` | 1m |
| `evaluation_interval` | 1m |
| `scrape_interval` in "slow" jobs | 5m |
| Jobs | `kubernetes-api-servers`, `kubernetes-nodes`, `kubernetes-nodes-cadvisor`, `kubernetes-pods`, `kubernetes-pods-slow`, `kubernetes-service-endpoints`, `kubernetes-service-endpoints-slow`, `kubernetes-services`, `prometheus`, `prometheus-pushgateway` (disabled) |

### Prometheus metric types

| Type | Example | How to read it |
|---|---|---|
| counter | `container_cpu_usage_seconds_total`, `demo_orders_total` | `rate()` / `increase()` |
| gauge | `kube_pod_status_phase`, `kube_node_status_condition` | the value as is |
| histogram, summary | request latencies | `histogram_quantile()` |

> ⚠️ Verify: histogram and summary, `histogram_quantile()`, recording and alerting rules were not run in the experiments.

## Internals

- **The path of `kubectl logs`.** The client contacts the API server, which contacts the kubelet of the right node, and the kubelet reads the file `/var/log/pods/<ns>_<pod>_<uid>/<container>/N.log` (in the experiment). That is why logs are unavailable when the node is unreachable and vanish after the pod is deleted.
- **The file format is CRI.** A line: an RFC3339 time with nanoseconds, a stream, the `F`/`P` tag, the text (in the experiment `2026-11-23T13:16:58.05… stdout F app line 28`). A log collection agent has to parse it (in the experiment fluent-bit needed the `cri` regex parser).
- **Rotation.** The kubelet limits the size and number of container files: `containerLogMaxSize: 10Mi`, `containerLogMaxFiles: 5` (in the experiment from `/configz`).
- **Old containers.** After a restart the kubelet keeps only a limited number of finished containers; in the experiment the node had only `2.log` for the pod `crash`, and `--previous` returned an error.
- **Events are API objects** with the fields `type`, `reason`, `message`, `count` and a reference to the object. They are created by controllers, the kubelet and the scheduler; repeated events are folded together (`x3 over 86s`). Retention is limited (an hour per the documentation; the `--event-ttl` flag is not set in the experiment).
- **The pull model.** Prometheus makes an HTTP GET to the `/metrics` of each target on a schedule. Targets come from Kubernetes service discovery, and `relabel_configs` turn pod labels and annotations into metric labels (`job`, `pod`, `namespace`).
- **cAdvisor** is built into the kubelet and serves container consumption; in the experiment it was polled through the API server (`kubernetes-nodes-cadvisor`).
- **kube-state-metrics** does not measure load but turns the state of API objects into metrics (in the experiment `kube_pod_container_status_restarts_total{pod="crash2"} = 4`).
- **metrics-server and Prometheus** solve different problems: the first keeps the latest snapshot for `kubectl top` and HPA ([module 15](../15-autoscaling/expert.en.md)), the second keeps history.

> ⚠️ Verify: the detail that metrics-server reads `/metrics/resource` is taken from the documentation. Prometheus data storage (TSDB, retention) and behavior on volume loss were not run.

## Gotchas

### 1. `--previous` does not always return logs

In the experiment for a pod in `CrashLoopBackOff` the command returned `unable to retrieve container logs for containerd://…`: the earlier files were no longer on the node. For reliability send logs to external storage.

### 2. `rate()` with a window below two scrape intervals

A query with the window `[1m]` and a 1-minute scrape interval returned an empty result. The window should be at least four intervals.

### 3. Prometheus 3 and `Content-Type`

A target without the `Content-Type` header is marked `down` with the error `non-compliant scrape target sending blank Content-Type and no fallback_scrape_protocol specified for target`. Simple HTTP servers do not send this header for a static file without an extension.

### 4. Events are short-lived

Do not use events as an archive: they are kept for a limited time and folded together.

### 5. A log agent does not see the control-plane

A DaemonSet without a toleration started on 2 of 3 nodes (in the experiment). The logs of control-plane system pods remain uncollected.

### 6. Logs vanish with the pod

After a pod is deleted its directory in `/var/log/pods` is removed; `kubectl logs` does not work for a deleted pod.

### 7. Prometheus without a volume loses data

In the experiment the installation used `server.persistentVolume.enabled=false`, that is, no persistent volume. That is acceptable for a disposable cluster and dangerous in production.

### 8. `Defaulted container`

In a multi-container pod `kubectl logs` without `-c` shows only one container (in the experiment the first, `app`) and prints the line `Defaulted container "app" out of: app, side`, which is easy to miss.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| `kubectl logs`/events | nothing to install | no history, no search |
| A DaemonSet agent (fluent-bit) | history and search, a standard pattern | needs storage, tolerations, metadata |
| A sidecar agent in every pod | flexible processing per application | uses pod resources; not run |
| Prometheus (pull) | a mature ecosystem, PromQL | local storage, needs a volume, cardinality |
| The Prometheus operator (ServiceMonitor) | declarative targets | a separate component; not run |
| Managed cloud solutions | less operations work | provider lock-in; not run |
| OpenTelemetry | one standard for traces, metrics, logs | added complexity; not run |

## Interview questions

<details><summary>1. How does <code>kubectl logs</code> work?</summary>

Through the API server to the kubelet of the node, which reads the container's log file.

→ [Basic theory](basics.en.md#where-logs-are-on-a-node)

</details>

<details><summary>2. How do you see the logs of a specific container and of all containers?</summary>

`-c <container>`; `--all-containers` (+ `--prefix`).

→ [Basic theory](basics.en.md#logs-kubectl-logs)

</details>

<details><summary>3. Trick: <code>logs --previous</code> returned an error. Why?</summary>

The kubelet had already removed the earlier container instances; for a pod in `CrashLoopBackOff` this is a normal situation.

→ [Gotcha 1](#1---previous-does-not-always-return-logs)

</details>

<details><summary>4. Where do you find the cause of <code>ImagePullBackOff</code> or <code>FailedScheduling</code>?</summary>

In the pod's events: `kubectl describe pod` or `kubectl events --for`.

→ [Basic theory](basics.en.md#events)

</details>

<details><summary>5. Why do logs vanish after a pod is deleted and what do you do about it?</summary>

The files lie on the node and are removed with the pod. You need an agent (a DaemonSet) that ships logs to external storage.

→ [Gotcha 6](#6-logs-vanish-with-the-pod)

</details>

<details><summary>6. How does Prometheus differ from metrics-server?</summary>

metrics-server keeps only the latest snapshot for `kubectl top` and HPA; Prometheus collects history and answers PromQL queries.

→ [Internals](#internals)

</details>

<details><summary>7. What does kube-state-metrics do?</summary>

It turns the state of Kubernetes objects (pods, nodes, Deployments) into metrics: phases, restart counts, readiness.

→ [Advanced theory](advanced.en.md#prometheus-internals-and-installation)

</details>

<details><summary>8. Trick: <code>rate(container_cpu_usage_seconds_total[1m])</code> is empty although the metric exists. Why?</summary>

There are fewer than two samples in the window: the scrape interval is 1 minute. You need a window of several intervals.

→ [Gotcha 2](#2-rate-with-a-window-below-two-scrape-intervals)

</details>

<details><summary>9. Trick: a Prometheus target is <code>down</code> with an error about <code>Content-Type</code>. The cause?</summary>

The application does not send a `Content-Type` header; Prometheus 3 requires it.

→ [Gotcha 3](#3-prometheus-3-and-content-type)

</details>

<details><summary>10. How do you connect your application's metrics to Prometheus?</summary>

Serve `/metrics` in the text format and describe the target: with pod annotations (if the scrape is configured that way) or `ServiceMonitor`/`PodMonitor` of the operator.

→ [Advanced theory](advanced.en.md#application-metrics)

</details>

<details><summary>11. How does a counter differ from a gauge and how do you read a counter?</summary>

A counter only grows (it resets on restart), and you read it with `rate()`/`increase()`; a gauge is a "now" value.

→ [Reference](#prometheus-metric-types)

</details>

<details><summary>12. Why did a DaemonSet with a log agent not collect logs from the control-plane?</summary>

The control-plane has a taint; without a toleration the pod is not scheduled. You need `tolerations`.

→ [Gotcha 5](#5-a-log-agent-does-not-see-the-control-plane)

</details>

## Cheat sheet

```text
kubectl logs P [-c C] [--all-containers --prefix] [--tail N] [--since 5s] [--previous] [-l sel]
kubectl events --for pod/P --types=Warning
kubectl get --raw /readyz?verbose
kubectl get --raw /api/v1/nodes/N/proxy/metrics/cadvisor
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
```

- stdout/stderr → files on the node → an agent → storage.
- Events are quick hints but short-lived.
- Prometheus: pull, `rate()` with a window ≥ 4 intervals, a correct `Content-Type`.
- A DaemonSet agent: tolerations for all the nodes you need.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Metrics and HPA: [15 · Autoscaling](../15-autoscaling/expert.en.md)
- DaemonSet: [12 · Other workloads](../12-workloads/expert.en.md)
- Probes and restarts: [11 · Probes and resources](../11-probes-and-resources/expert.en.md)
- Scheduling and taints: [18 · Pod scheduling](../18-scheduling/overview.en.md)

## Sources

- [Logging Architecture](https://kubernetes.io/docs/concepts/cluster-administration/logging/)
- [Metrics For Kubernetes System Components](https://kubernetes.io/docs/concepts/cluster-administration/system-metrics/)
- [Kubernetes API health endpoints](https://kubernetes.io/docs/reference/using-api/health-checks/)
- [Prometheus: Metric types](https://prometheus.io/docs/concepts/metric_types/)
- [Prometheus: rate()](https://prometheus.io/docs/prometheus/latest/querying/functions/#rate)
- [kube-state-metrics: pod metrics](https://github.com/kubernetes/kube-state-metrics/blob/main/docs/metrics/workload/pod-metrics.md)
