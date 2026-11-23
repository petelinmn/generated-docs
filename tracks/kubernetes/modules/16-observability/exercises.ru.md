---
title: "Упражнения: наблюдаемость"
lang: ru
track: kubernetes
kind: module
doc: exercises
slug: 16-observability
part: 3
tags: [logs, events, prometheus, fluent-bit]
status: draft
updated: 2026-11-23
---

# Упражнения: наблюдаемость

🌐 **Русский** · [English](exercises.en.md)

> Решайте самостоятельно, подсказки и решения под спойлерами. Нужен кластер kind и namespace: `kubectl create namespace learn-16`. Упражнениям 4 и 5 нужен Helm и доступ в интернет. После работы: `helm uninstall` релизов и `kubectl delete namespace learn-16`.

## 1. 🟢 Два контейнера в одном поде

**Задача:** создайте под `multi` с контейнерами `app` (печатает в stdout `app line N` раз в секунду) и `side` (печатает в stderr `side-log` раз в две секунды). Получите: логи `app`, логи обоих с префиксом, последние 5 секунд с временем.

**Ожидаемый результат:** `Defaulted container "app" out of: app, side` без `-c`; строки с префиксами `[pod/multi/app]` и `[pod/multi/side]`.

<details><summary>Подсказка</summary>

`-c`, `--all-containers --prefix`, `--since=5s --timestamps`.

</details>

<details><summary>Решение</summary>

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

## 2. 🟢 Упавший контейнер

**Задача:** создайте под `crash2`, который работает 8 секунд, печатает `start-<время>` и `dying` и завершается с кодом 1. Дождитесь перезапуска и сравните `kubectl logs` и `kubectl logs --previous`. Посмотрите события.

**Ожидаемый результат:** текущий лог содержит новый `start-…`, `--previous` — прежний `start-…` и `dying`; в событиях `BackOff`/`Created`/`Started`.

<details><summary>Подсказка</summary>

`kubectl get pod crash2 -w`; `kubectl events --for pod/crash2`.

</details>

<details><summary>Решение</summary>

```text
$ kubectl -n learn-16 logs crash2
start-1795439848
$ kubectl -n learn-16 logs crash2 --previous
start-1795439840
dying
```

Если контейнер падает мгновенно и уже несколько раз перезапустился, `--previous` может вернуть `unable to retrieve container logs` — см. [базовую теорию](basics.ru.md#логи-упавшего-контейнера---previous).

</details>

## 3. 🟡 Файлы логов и health

**Задача:** найдите на ноде файл лога пода `multi` и ссылку на него в `/var/log/containers`. Затем выведите `readyz?verbose`, `livez` API-сервера и несколько строк метрики `apiserver_request_total`.

**Ожидаемый результат:** строка вида `<время> stdout F app line N`; `readyz check passed`; `ok`.

<details><summary>Подсказка</summary>

`docker exec <нода> ls /var/log/pods`; `kubectl get --raw '/readyz?verbose'`; `kubectl get --raw /metrics | grep apiserver_request_total`.

</details>

<details><summary>Решение</summary>

```text
2026-11-23T13:16:58.055797529Z stdout F app line 28
[+]poststarthook/apiservice-openapi-controller ok
readyz check passed
ok
apiserver_request_total{code="200",…,subresource="/healthz",verb="GET",…} 2
```

</details>

## 4. 🟡 Prometheus и метрика приложения

**Задача:** установите чарт `prometheus-community/prometheus` (без alertmanager, pushgateway и тома). Запустите под `metricsapp` с аннотациями `prometheus.io/*`, отдающий `demo_orders_total{shop="a"} 42` по пути `/metrics.txt`. Найдите метрику запросом PromQL. Затем получите `rate()` CPU контейнеров с окном `[1m]` и `[5m]`.

**Ожидаемый результат:** `demo_orders_total{job="kubernetes-pods",…} 42`; окно `[1m]` пусто, `[5m]` — данные.

<details><summary>Подсказка</summary>

Запрос через под сервера: `kubectl exec deploy/prom-prometheus-server -c prometheus-server -- wget -qO- 'http://localhost:9090/api/v1/query?query=…'`.

</details>

<details><summary>Решение</summary>

```text
$ helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
$ helm install prom prometheus-community/prometheus -n learn-16 --set alertmanager.enabled=false --set prometheus-pushgateway.enabled=false --set server.persistentVolume.enabled=false --wait
{job="kubernetes-pods", pod="metricsapp", shop="a"}   42
```

Подробнее — в [продвинутой теории](advanced.ru.md#prometheus-устройство-и-установка).

</details>

## 5. 🔴 Сборщик логов и Content-Type

**Задача:** (а) запустите fluent-bit DaemonSet, читающий `/var/log/containers/multi_learn-16_*.log`, и покажите, что строки `app line N` видны в его выводе; объясните, почему подов 2, а нод 3. (б) Сделайте так, чтобы приложение отдавало метрики по пути `/metrics` без расширения (`busybox httpd`) и найдите ошибку цели в Prometheus.

**Ожидаемый результат:** (а) `fb 2 2 2`, строки JSON с `"log":"app line 1"`; (б) `non-compliant scrape target sending blank Content-Type…`.

<details><summary>Подсказка</summary>

(б) `Status → Targets` в интерфейсе или `/api/v1/targets` и поле `lastError`.

</details>

<details><summary>Решение</summary>

Манифесты — в [продвинутой теории](advanced.ru.md#сбор-логов-демоном-на-ноде).

```text
{"date":1795439792.020621,"stream":"stdout","logtag":"F","log":"app line 1"}
non-compliant scrape target sending blank Content-Type and no fallback_scrape_protocol specified for target
```

Подов два, потому что на control-plane стоит taint без toleration у DaemonSet.

</details>
