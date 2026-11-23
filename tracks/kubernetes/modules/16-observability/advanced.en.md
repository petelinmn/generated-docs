---
title: "Observability: advanced"
lang: en
track: kubernetes
kind: module
doc: advanced
slug: 16-observability
part: 3
tags: [prometheus, promql, grafana, fluent-bit, kube-state-metrics, cadvisor, scrape, daemonset, helm]
versions: "Kubernetes 1.37.0 (kind v0.34), Prometheus v3.16.0 (chart prometheus 30.0.0), kube-state-metrics 2.20.0, Grafana 13.3.1 (chart grafana-community/grafana 14.5.0), fluent-bit 4.1, Helm 4.3"
status: draft
updated: 2026-11-23
---

# Observability: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on [basic theory](basics.en.md). Experiments were run in kind, namespace `learn-16`. Prometheus and Grafana were installed with [Helm](../13-helm/overview.en.md) charts from external repositories.

## Log collection with a node daemon

Log files lie on every node, so the agent that reads them is run as a **DaemonSet** ([module 12](../12-workloads/overview.en.md)) mounting `hostPath: /var/log`. Example: fluent-bit reads the logs of the pod `multi` and prints them to stdout as JSON (in a real system the output goes to storage: Loki, Elasticsearch and so on).

```yaml
apiVersion: v1
kind: ConfigMap
metadata: {name: fb-config}
data:
  fluent-bit.conf: |
    [SERVICE]
        Flush 1
        Parsers_File parsers.conf
    [INPUT]
        Name tail
        Path /var/log/containers/multi_learn-16_*.log
        Parser cri
        Tag kube.*
        Read_from_Head true
    [OUTPUT]
        Name stdout
        Match *
        Format json_lines
  parsers.conf: |
    [PARSER]
        Name cri
        Format regex
        Regex ^(?<time>[^ ]+) (?<stream>stdout|stderr) (?<logtag>[^ ]*) (?<log>.*)$
        Time_Key time
        Time_Format %Y-%m-%dT%H:%M:%S.%L%z
---
apiVersion: apps/v1
kind: DaemonSet
metadata: {name: fb}
spec:
  selector: {matchLabels: {app: fb}}
  template:
    metadata: {labels: {app: fb}}
    spec:
      containers:
      - name: fb
        image: fluent/fluent-bit:4.1
        args: ["-c", "/cfg/fluent-bit.conf"]
        volumeMounts:
        - {name: cfg, mountPath: /cfg}
        - {name: varlog, mountPath: /var/log, readOnly: true}
      volumes:
      - {name: cfg, configMap: {name: fb-config}}
      - {name: varlog, hostPath: {path: /var/log}}
```

```text
$ kubectl -n learn-16 get ds fb
NAME   DESIRED   CURRENT   READY   UP-TO-DATE   AVAILABLE
fb     2         2         2       2            2
$ kubectl -n learn-16 logs fb-c287w | grep "app line" | head -2
{"date":1795439792.020621,"stream":"stdout","logtag":"F","log":"app line 1"}
{"date":1795439793.022395,"stream":"stdout","logtag":"F","log":"app line 2"}
```

There are two pods and three nodes: the control-plane has a taint, and the DaemonSet has no toleration (see [module 18](../18-scheduling/basics.en.md#the-control-plane-taint)). Such an agent will not collect the logs of control-plane pods; for them you need `tolerations`.

The agent reads the same files as the kubelet; on rotation it keeps working. In production the line is enriched with Kubernetes metadata (pod name, namespace, labels) — fluent-bit has the `kubernetes` filter for that.

> ⚠️ Verify: the `kubernetes` filter and sending to Loki/Elasticsearch were not run; the experiment verified reading the files and parsing the CRI format.

## Prometheus: internals and installation

Prometheus polls (**pull**) the `/metrics` endpoints itself, where metrics are in a text format. It finds targets through Kubernetes **service discovery** (by Pod, Endpoints, Node) and filters them by labels and annotations (`relabel_configs`).

Installing with a chart (a remote Helm repository):

```text
$ helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
$ helm repo update
$ helm install prom prometheus-community/prometheus -n learn-16 \
    --set alertmanager.enabled=false --set prometheus-pushgateway.enabled=false \
    --set server.persistentVolume.enabled=false --wait
STATUS: deployed
$ kubectl -n learn-16 get pods
prom-kube-state-metrics-7f665f86b5-p4vf5   1/1   Running
prom-prometheus-node-exporter-mj4tj        1/1   Running   (×3 — one per node)
prom-prometheus-server-5677897ff9-d6lrg    2/2   Running
```

The `prometheus` chart 30.0.0 brought Prometheus v3.16.0, kube-state-metrics 2.20.0 and node-exporter. Data is not persisted (`persistentVolume.enabled=false`): acceptable for an experiment, a volume is needed in production.

Default targets (jobs):

```text
$ sum by (job)(up)
kubernetes-service-endpoints   7
kubernetes-nodes-cadvisor      3
kubernetes-nodes               3
kubernetes-api-servers         1
prometheus                     1
kubernetes-pods                1   ← after the pod metricsapp with annotations started
```

The metric sources this chart collects:

| Source | What it provides |
|---|---|
| kubelet `/metrics/cadvisor` | container consumption (`container_cpu_usage_seconds_total`, memory, network) |
| kube-state-metrics | the state of Kubernetes objects (`kube_pod_status_phase`, `kube_pod_container_status_restarts_total`, `kube_node_status_condition`) |
| node-exporter | node OS metrics |
| kube-apiserver `/metrics` | API requests and so on |
| your pods with `prometheus.io/scrape` annotations | application metrics |

## PromQL: examples

The queries were run through the Prometheus HTTP API (`/api/v1/query`) from the server pod:

```text
$ count(kube_node_status_condition{condition="Ready",status="true"} == 1)
3

$ kube_pod_container_status_restarts_total{namespace="learn-16",pod="crash2"}
{container="c", pod="crash2"}   4

$ sum by (pod)(rate(container_cpu_usage_seconds_total{namespace="learn-16",container!=""}[5m]))
{pod="multi"}                              0.001224
{pod="prom-prometheus-server-…"}           0.003557
{pod="metricsapp"}                         0.000005
…
```

- `container_cpu_usage_seconds_total` is a **counter**: it only grows; what matters is the rate of growth — `rate(…[window])` (CPU cores per second).
- `kube_…_restarts_total` is also a counter; to ask "were there restarts recently" use `increase(…[window])`.
- `sum by (pod)` aggregates by a label.

### The `rate()` window and the scrape interval

In the chart `scrape_interval` is 1 minute. A query with the window `[1m]` returned an **empty result** in the experiment (`n = 0`), and with `[5m]` it returned data:

```text
rate(container_cpu_usage_seconds_total{…,pod="multi",container="app"}[1m])   → empty
sum by (pod)(rate(container_cpu_usage_seconds_total{…}[5m]))                  → 7 series
```

`rate()` needs at least two samples in the window. Practice: a window of at least four scrape intervals.

## Application metrics

You can connect your own application with pod annotations (this is how `kubernetes-pods` is set up in this chart):

```yaml
metadata:
  name: metricsapp
  annotations:
    prometheus.io/scrape: "true"
    prometheus.io/port: "8080"
    prometheus.io/path: "/metrics.txt"
```

The application serves the Prometheus text format:

```text
# HELP demo_orders_total Orders processed.
# TYPE demo_orders_total counter
demo_orders_total{shop="a"} 42
```

In the experiment (a simple `busybox httpd`) the file was first named `/metrics`, and the target was `down`:

```text
non-compliant scrape target sending blank Content-Type and no fallback_scrape_protocol specified for target
```

Prometheus 3 requires the response to have a `Content-Type` header (`httpd` does not send one for a file without an extension). After renaming to `metrics.txt` (`Content-type: text/plain`) the metric appeared:

```text
$ demo_orders_total
{job="kubernetes-pods", pod="metricsapp", shop="a"}   42
```

> ⚠️ Verify: the `prometheus.io/*` annotations are a configuration convention of this chart, not part of Kubernetes. For real applications `ServiceMonitor`/`PodMonitor` of the Prometheus operator are used more often (not run).

## Grafana

Grafana draws dashboards from Prometheus data. Installation with a chart from the `grafana-community` repository with values (the data source is connected at startup):

```yaml
adminPassword: learn-only        # for the experiment only
persistence: {enabled: false}
datasources:
  datasources.yaml:
    apiVersion: 1
    datasources:
    - name: Prometheus
      type: prometheus
      url: http://prom-prometheus-server.learn-16.svc
      access: proxy
      isDefault: true
```

```text
$ helm install graf grafana-community/grafana -n learn-16 -f gvals.yaml --wait
$ kubectl -n learn-16 port-forward svc/graf-grafana 3300:80 &
$ curl -s localhost:3300/api/health
{ "database": "ok", "version": "13.3.1", … }
$ curl -s -u admin:learn-only localhost:3300/api/datasources
Prometheus prometheus http://prom-prometheus-server.learn-16.svc True
$ curl -s -u admin:learn-only "localhost:3300/api/datasources/proxy/uid/<uid>/api/v1/query?query=demo_orders_total"
1 42
```

Through Grafana the same query returned the same value — 42. A plain-text password in values is acceptable only for a disposable cluster; in production use a Secret ([module 07](../07-config-and-secrets/overview.en.md)). You can import ready-made dashboards (by an identifier from the catalog) or keep them as code.

> ⚠️ Verify: building dashboards and Grafana/Alertmanager alerts were not run in the experiments.

## Best practices

- Logs go to stdout/stderr, one event per line (JSON is better).
- One log collector per node (DaemonSet) with tolerations for all nodes whose logs you need.
- Metrics: names like `<what>_<unit>_total` for counters; do not put high-cardinality values (user IDs) into labels.
- The `rate()` window — at least four scrape intervals.
- Alerts on symptoms (errors, latency), not on every cause.
- For storing metrics and logs — volumes and a retention policy.

## Common mistakes

- ❌ `rate(...[1m])` with a 1-minute scrape interval → ✅ a window of 5 minutes or more.
- ❌ A `/metrics` target without `Content-Type` → ✅ a correct header (`text/plain` was enough in the experiment).
- ❌ `prometheus.io/*` annotations without the matching scrape configuration → ✅ check `Status → Targets`.
- ❌ A log agent without a toleration → ✅ add tolerations if control-plane and dedicated nodes are needed.
- ❌ The Grafana admin password in Helm values in Git → ✅ a Secret.

## Related materials

- Next: [expert theory](expert.en.md)
- Metrics for HPA and `kubectl top`: [15 · Autoscaling](../15-autoscaling/basics.en.md#metrics-server-and-kubectl-top)
- Helm: [13 · Helm](../13-helm/basics.en.md)
- DaemonSet: [12 · Other workloads](../12-workloads/basics.en.md)
- Secrets: [07 · ConfigMap and Secret](../07-config-and-secrets/overview.en.md)

## Sources

- [Prometheus: Configuration](https://prometheus.io/docs/prometheus/latest/configuration/configuration/)
- [Prometheus: Querying basics](https://prometheus.io/docs/prometheus/latest/querying/basics/)
- [prometheus-community/helm-charts: prometheus](https://github.com/prometheus-community/helm-charts/tree/main/charts/prometheus)
- [kube-state-metrics](https://github.com/kubernetes/kube-state-metrics)
- [Fluent Bit: Tail input](https://docs.fluentbit.io/manual/data-pipeline/inputs/tail)
- [Grafana: Provision data sources](https://grafana.com/docs/grafana/latest/administration/provisioning/#data-sources)
