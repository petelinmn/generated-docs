---
title: "Наблюдаемость: продвинутая теория"
lang: ru
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

# Наблюдаемость: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md). Опыты — в kind, namespace `learn-16`. Prometheus и Grafana поставлены [Helm](../13-helm/overview.ru.md)-чартами из внешних репозиториев.

## Сбор логов демоном на ноде

Файлы с логами лежат на каждой ноде, поэтому агент, который их читает, запускают **DaemonSet'ом** ([модуль 12](../12-workloads/overview.ru.md)) с монтированием `hostPath: /var/log`. Пример: fluent-bit читает логи пода `multi` и печатает их в stdout в формате JSON (в реальной системе вывод идёт в хранилище: Loki, Elasticsearch и т. д.).

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

Подов два, а нод три: на control-plane стоит taint, а у DaemonSet нет toleration (см. [модуль 18](../18-scheduling/basics.ru.md#taint-на-control-plane)). Логи подов с control-plane такой агент не соберёт; для них нужен `tolerations`.

Агент читает те же файлы, что и kubelet; при ротации он продолжает работу. В production к строке добавляют метаданные Kubernetes (имя пода, namespace, метки) — для этого в fluent-bit есть фильтр `kubernetes`.

> ⚠️ Проверить: фильтр `kubernetes` и отправка в Loki/Elasticsearch не запускались; опыт проверял чтение файлов и разбор формата CRI.

## Prometheus: устройство и установка

Prometheus сам опрашивает (**pull**) endpoint'ы `/metrics`, где метрики лежат в текстовом формате. Цели он находит через **service discovery** Kubernetes (по Pod, Endpoints, Node) и фильтрует по меткам и аннотациям (`relabel_configs`).

Установка чартом (удалённый репозиторий Helm):

```text
$ helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
$ helm repo update
$ helm install prom prometheus-community/prometheus -n learn-16 \
    --set alertmanager.enabled=false --set prometheus-pushgateway.enabled=false \
    --set server.persistentVolume.enabled=false --wait
STATUS: deployed
$ kubectl -n learn-16 get pods
prom-kube-state-metrics-7f665f86b5-p4vf5   1/1   Running
prom-prometheus-node-exporter-mj4tj        1/1   Running   (×3 — по одному на ноду)
prom-prometheus-server-5677897ff9-d6lrg    2/2   Running
```

Чарт `prometheus` 30.0.0 принёс Prometheus v3.16.0, kube-state-metrics 2.20.0 и node-exporter. Данные не сохраняются (`persistentVolume.enabled=false`): для опыта это допустимо, в production нужен том.

Цели (jobs) по умолчанию:

```text
$ sum by (job)(up)
kubernetes-service-endpoints   7
kubernetes-nodes-cadvisor      3
kubernetes-nodes               3
kubernetes-api-servers         1
prometheus                     1
kubernetes-pods                1   ← после запуска пода metricsapp с аннотациями
```

Источники метрик, которые собирает этот чарт:

| Источник | Что отдаёт |
|---|---|
| kubelet `/metrics/cadvisor` | потребление контейнеров (`container_cpu_usage_seconds_total`, память, сеть) |
| kube-state-metrics | состояние объектов Kubernetes (`kube_pod_status_phase`, `kube_pod_container_status_restarts_total`, `kube_node_status_condition`) |
| node-exporter | метрики ОС ноды |
| kube-apiserver `/metrics` | запросы к API и т. д. |
| ваши поды с аннотациями `prometheus.io/scrape` | метрики приложений |

## PromQL: примеры

Запросы выполнены через HTTP API Prometheus (`/api/v1/query`) из пода сервера:

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

- `container_cpu_usage_seconds_total` — **счётчик** (counter): растёт всегда; смысл имеет скорость роста — `rate(…[окно])` (ядра CPU в секунду).
- `kube_…_restarts_total` — тоже счётчик; для «были ли перезапуски недавно» используют `increase(…[окно])`.
- `sum by (pod)` — агрегация по метке.

### Окно `rate()` и интервал опроса

В чарте `scrape_interval` равен 1 минуте. Запрос с окном `[1m]` в опыте вернул **пустой результат** (`n = 0`), а с окном `[5m]` — данные:

```text
rate(container_cpu_usage_seconds_total{…,pod="multi",container="app"}[1m])   → пусто
sum by (pod)(rate(container_cpu_usage_seconds_total{…}[5m]))                  → 7 рядов
```

`rate()` нужно как минимум два сэмпла в окне. Практика: окно не меньше четырёх интервалов опроса.

## Метрики приложения

Подключить своё приложение можно аннотациями пода (так настроен `kubernetes-pods` в этом чарте):

```yaml
metadata:
  name: metricsapp
  annotations:
    prometheus.io/scrape: "true"
    prometheus.io/port: "8080"
    prometheus.io/path: "/metrics.txt"
```

Приложение отдаёт текстовый формат Prometheus:

```text
# HELP demo_orders_total Orders processed.
# TYPE demo_orders_total counter
demo_orders_total{shop="a"} 42
```

В опыте (простой `busybox httpd`) сначала файл назывался `/metrics`, и цель оказалась `down`:

```text
non-compliant scrape target sending blank Content-Type and no fallback_scrape_protocol specified for target
```

Prometheus 3 требует, чтобы ответ имел заголовок `Content-Type` (у `httpd` для файла без расширения его нет). После переименования в `metrics.txt` (`Content-type: text/plain`) метрика появилась:

```text
$ demo_orders_total
{job="kubernetes-pods", pod="metricsapp", shop="a"}   42
```

> ⚠️ Проверить: аннотации `prometheus.io/*` — соглашение конфигурации этого чарта, а не часть Kubernetes. Для реальных приложений чаще используют `ServiceMonitor`/`PodMonitor` оператора Prometheus (не запускался).

## Grafana

Grafana рисует дашборды по данным Prometheus. Установка чартом из репозитория `grafana-community` со значениями (источник данных подключается при старте):

```yaml
adminPassword: learn-only        # только для опыта
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

Через Grafana тот же запрос вернул то же значение — 42. Пароль в значениях в открытом виде допустим только для одноразового кластера; в production используйте Secret ([модуль 07](../07-config-and-secrets/overview.ru.md)). Дашборды можно импортировать готовые (по идентификатору из каталога) или хранить как код.

> ⚠️ Проверить: построение дашбордов и алерты Grafana/Alertmanager в опытах не запускались.

## Лучшие практики

- Логи — в stdout/stderr, по одному событию в строке (лучше JSON).
- Один сборщик логов на ноду (DaemonSet) с tolerations для всех нод, откуда нужны логи.
- Метрики: имена вида `<что>_<единица>_total` для счётчиков; не помещайте в метки значения с высокой кардинальностью (ID пользователей).
- Окно `rate()` — не меньше четырёх интервалов опроса.
- Алерты — по симптомам (ошибки, задержки), а не по каждой причине.
- Для хранения метрик и логов — тома и политика хранения.

## Частые ошибки

- ❌ `rate(...[1m])` при интервале опроса 1 минута → ✅ окно 5 минут и более.
- ❌ Целевой `/metrics` без `Content-Type` → ✅ корректный заголовок (в опыте хватило `text/plain`).
- ❌ Аннотации `prometheus.io/*` без соответствующей конфигурации сбора → ✅ проверить `Status → Targets`.
- ❌ Агент логов без toleration → ✅ добавить tolerations, если нужны control-plane и выделенные ноды.
- ❌ Пароль админа Grafana в значениях Helm в Git → ✅ Secret.

## Связанные материалы

- Дальше: [экспертная теория](expert.ru.md)
- Метрики для HPA и `kubectl top`: [15 · Автомасштабирование](../15-autoscaling/basics.ru.md#metrics-server-и-kubectl-top)
- Helm: [13 · Helm](../13-helm/basics.ru.md)
- DaemonSet: [12 · Другие workloads](../12-workloads/basics.ru.md)
- Секреты: [07 · ConfigMap и Secret](../07-config-and-secrets/overview.ru.md)

## Источники

- [Prometheus: Configuration](https://prometheus.io/docs/prometheus/latest/configuration/configuration/)
- [Prometheus: Querying basics](https://prometheus.io/docs/prometheus/latest/querying/basics/)
- [prometheus-community/helm-charts: prometheus](https://github.com/prometheus-community/helm-charts/tree/main/charts/prometheus)
- [kube-state-metrics](https://github.com/kubernetes/kube-state-metrics)
- [Fluent Bit: Tail input](https://docs.fluentbit.io/manual/data-pipeline/inputs/tail)
- [Grafana: Provision data sources](https://grafana.com/docs/grafana/latest/administration/provisioning/#data-sources)
