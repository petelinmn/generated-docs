---
title: "Exercises: observability"
lang: en
track: kubernetes
kind: module
doc: exercises
slug: 16-observability
part: 3
tags: [logs, events, prometheus, fluent-bit]
status: draft
updated: 2026-11-23
---

# Exercises: observability

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself; hints and solutions are under spoilers. You need a kind cluster and a namespace: `kubectl create namespace learn-16`. Exercises 4 and 5 need Helm and internet access. Afterwards: `helm uninstall` the releases and `kubectl delete namespace learn-16`.

## 1. 🟢 Two containers in one pod

**Task:** create a pod `multi` with containers `app` (prints `app line N` to stdout once a second) and `side` (prints `side-log` to stderr once every two seconds). Get: the logs of `app`, the logs of both with a prefix, the last 5 seconds with the time.

**Expected result:** `Defaulted container "app" out of: app, side` without `-c`; lines with the prefixes `[pod/multi/app]` and `[pod/multi/side]`.

<details><summary>Hint</summary>

`-c`, `--all-containers --prefix`, `--since=5s --timestamps`.

</details>

<details><summary>Solution</summary>

```text
$ kubectl -n learn-16 logs multi --all-containers --prefix --tail=2
[pod/multi/app] app line 19
[pod/multi/app] app line 20
[pod/multi/side] side-log
[pod/multi/side] side-log
$ kubectl -n learn-16 logs multi -c app --since=5s --timestamps
2026-11-23T13:16:47.040690397Z app line 16
```

</details>

## 2. 🟢 A crashed container

**Task:** create a pod `crash2` that runs for 8 seconds, prints `start-<time>` and `dying` and exits with code 1. Wait for a restart and compare `kubectl logs` and `kubectl logs --previous`. Look at the events.

**Expected result:** the current log has a new `start-…`, `--previous` has the earlier `start-…` and `dying`; the events show `BackOff`/`Created`/`Started`.

<details><summary>Hint</summary>

`kubectl get pod crash2 -w`; `kubectl events --for pod/crash2`.

</details>

<details><summary>Solution</summary>

```text
$ kubectl -n learn-16 logs crash2
start-1795439848
$ kubectl -n learn-16 logs crash2 --previous
start-1795439840
dying
```

If a container crashes instantly and has already restarted several times, `--previous` may return `unable to retrieve container logs` — see [basic theory](basics.en.md#logs-of-a-crashed-container---previous).

</details>

## 3. 🟡 Log files and health

**Task:** find the log file of the pod `multi` on its node and the link to it in `/var/log/containers`. Then print `readyz?verbose` and `livez` of the API server and a few lines of the metric `apiserver_request_total`.

**Expected result:** a line like `<time> stdout F app line N`; `readyz check passed`; `ok`.

<details><summary>Hint</summary>

`docker exec <node> ls /var/log/pods`; `kubectl get --raw '/readyz?verbose'`; `kubectl get --raw /metrics | grep apiserver_request_total`.

</details>

<details><summary>Solution</summary>

```text
2026-11-23T13:16:58.055797529Z stdout F app line 28
[+]poststarthook/apiservice-openapi-controller ok
readyz check passed
ok
apiserver_request_total{code="200",…,subresource="/healthz",verb="GET",…} 2
```

</details>

## 4. 🟡 Prometheus and an application metric

**Task:** install the chart `prometheus-community/prometheus` (without alertmanager, pushgateway and a volume). Run a pod `metricsapp` with `prometheus.io/*` annotations that serves `demo_orders_total{shop="a"} 42` on the path `/metrics.txt`. Find the metric with a PromQL query. Then get the `rate()` of container CPU with the windows `[1m]` and `[5m]`.

**Expected result:** `demo_orders_total{job="kubernetes-pods",…} 42`; the window `[1m]` is empty, `[5m]` returns data.

<details><summary>Hint</summary>

Query through the server pod: `kubectl exec deploy/prom-prometheus-server -c prometheus-server -- wget -qO- 'http://localhost:9090/api/v1/query?query=…'`.

</details>

<details><summary>Solution</summary>

```text
$ helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
$ helm install prom prometheus-community/prometheus -n learn-16 --set alertmanager.enabled=false --set prometheus-pushgateway.enabled=false --set server.persistentVolume.enabled=false --wait
{job="kubernetes-pods", pod="metricsapp", shop="a"}   42
```

More in the [advanced theory](advanced.en.md#prometheus-internals-and-installation).

</details>

## 5. 🔴 A log collector and Content-Type

**Task:** (a) run a fluent-bit DaemonSet that reads `/var/log/containers/multi_learn-16_*.log` and show that the `app line N` lines are visible in its output; explain why there are 2 pods and 3 nodes. (b) Make the application serve metrics on the path `/metrics` without an extension (`busybox httpd`) and find the target error in Prometheus.

**Expected result:** (a) `fb 2 2 2`, JSON lines with `"log":"app line 1"`; (b) `non-compliant scrape target sending blank Content-Type…`.

<details><summary>Hint</summary>

(b) `Status → Targets` in the UI or `/api/v1/targets` and the `lastError` field.

</details>

<details><summary>Solution</summary>

The manifests are in the [advanced theory](advanced.en.md#log-collection-with-a-node-daemon).

```text
{"date":1795439792.020621,"stream":"stdout","logtag":"F","log":"app line 1"}
non-compliant scrape target sending blank Content-Type and no fallback_scrape_protocol specified for target
```

There are two pods because the control-plane has a taint and the DaemonSet has no toleration.

</details>
