---
title: "Наблюдаемость: экспертная теория"
lang: ru
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

# Наблюдаемость: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

Опирается на [базовую](basics.ru.md) и [продвинутую](advanced.ru.md) теорию. Что подтверждено опытом в kind (Kubernetes 1.37.0) — помечено «в опыте».

## Справочник

### Где что смотреть

| Что нужно | Источник | Как получить |
|---|---|---|
| Вывод контейнера | kubelet | `kubectl logs …` |
| Что случилось с объектом | события API | `kubectl describe`, `kubectl events`, `kubectl get events` |
| Здоровье API-сервера | `/livez`, `/readyz`, `/healthz` | `kubectl get --raw /readyz?verbose` (в опыте `readyz check passed`, `ok`) |
| Метрики API-сервера | `/metrics` | `kubectl get --raw /metrics` (в опыте 303 строки `# TYPE`) |
| Потребление контейнеров | kubelet: `/metrics/cadvisor`, `/metrics/resource` | `kubectl get --raw /api/v1/nodes/<нода>/proxy/metrics/cadvisor` |
| Конфигурация kubelet | `/configz` | `kubectl get --raw /api/v1/nodes/<нода>/proxy/configz` |
| Состояние объектов в виде метрик | kube-state-metrics | Prometheus |
| Метрики ОС ноды | node-exporter | Prometheus |
| Метрики приложения | `/metrics` приложения | Prometheus |

### Флаги и команды

| Команда | Назначение |
|---|---|
| `kubectl logs P -c C --tail=N --since=5s --timestamps` | выборка логов |
| `kubectl logs --all-containers --prefix` / `-l селектор` | несколько контейнеров и подов |
| `kubectl logs --previous` | предыдущий экземпляр контейнера (в опыте нестабилен для `CrashLoopBackOff`) |
| `kubectl events --for pod/P --types=Warning` | события одного объекта |
| `kubectl get events --field-selector involvedObject.name=P,type=Normal --sort-by=.lastTimestamp` | фильтр и сортировка |

> ⚠️ Проверить: флаги `-f`, `--since-time`, `--limit-bytes`, `-w` для событий взяты из документации и не запускались.

### Настройки Prometheus в чарте (в опыте)

| Параметр | Значение |
|---|---|
| `scrape_interval` | 1m |
| `evaluation_interval` | 1m |
| `scrape_interval` в «медленных» jobs | 5m |
| Jobs | `kubernetes-api-servers`, `kubernetes-nodes`, `kubernetes-nodes-cadvisor`, `kubernetes-pods`, `kubernetes-pods-slow`, `kubernetes-service-endpoints`, `kubernetes-service-endpoints-slow`, `kubernetes-services`, `prometheus`, `prometheus-pushgateway` (отключён) |

### Типы метрик Prometheus

| Тип | Пример | Как читать |
|---|---|---|
| counter | `container_cpu_usage_seconds_total`, `demo_orders_total` | `rate()` / `increase()` |
| gauge | `kube_pod_status_phase`, `kube_node_status_condition` | значение как есть |
| histogram, summary | задержки запросов | `histogram_quantile()` |

> ⚠️ Проверить: histogram и summary, `histogram_quantile()`, recording и alerting rules в опытах не запускались.

## Внутреннее устройство

- **Путь `kubectl logs`.** Клиент обращается к API-серверу, тот — к kubelet нужной ноды, а kubelet читает файл `/var/log/pods/<ns>_<под>_<uid>/<контейнер>/N.log` (в опыте). Поэтому логи недоступны, если нода недоступна, и пропадают после удаления пода.
- **Формат файла — CRI.** Строка: время RFC3339 с наносекундами, поток, метка `F`/`P`, текст (в опыте `2026-11-23T13:16:58.05… stdout F app line 28`). Агент сбора логов обязан его разобрать (в опыте для fluent-bit понадобился regex-парсер `cri`).
- **Ротация.** Kubelet ограничивает размер и число файлов контейнера: `containerLogMaxSize: 10Mi`, `containerLogMaxFiles: 5` (в опыте из `/configz`).
- **Старые контейнеры.** После перезапуска kubelet оставляет только ограниченное число завершённых контейнеров; в опыте на ноде был только `2.log` пода `crash`, и `--previous` вернул ошибку.
- **События — объекты API** с полями `type`, `reason`, `message`, `count`, ссылкой на объект. Их создают контроллеры, kubelet и планировщик; повторяющиеся события сворачиваются (`x3 over 86s`). Срок хранения ограничен (по документации — час; флаг `--event-ttl` в опыте не задан).
- **Модель pull.** Prometheus по расписанию делает HTTP GET на `/metrics` каждой цели. Цели ему выдаёт service discovery Kubernetes, `relabel_configs` превращают метки и аннотации пода в метки метрик (`job`, `pod`, `namespace`).
- **cAdvisor** встроен в kubelet и отдаёт потребление контейнеров; в опыте он опрашивался через API-сервер (`kubernetes-nodes-cadvisor`).
- **kube-state-metrics** не измеряет нагрузку, а превращает состояние объектов API в метрики (в опыте `kube_pod_container_status_restarts_total{pod="crash2"} = 4`).
- **metrics-server и Prometheus** решают разные задачи: первый хранит последний срез для `kubectl top` и HPA ([модуль 15](../15-autoscaling/expert.ru.md)), второй — историю.

> ⚠️ Проверить: детали того, что metrics-server читает `/metrics/resource`, взяты из документации. Хранение данных Prometheus (TSDB, retention) и поведение при потере тома не запускались.

## Подвохи

### 1. `--previous` не всегда возвращает логи

В опыте для пода в `CrashLoopBackOff` команда вернула `unable to retrieve container logs for containerd://…`: предыдущих файлов на ноде уже не было. Для надёжности логи нужно отправлять во внешнее хранилище.

### 2. `rate()` с окном меньше двух интервалов опроса

Запрос с окном `[1m]` при интервале опроса 1 минута вернул пустой результат. Окно — минимум четыре интервала.

### 3. Prometheus 3 и `Content-Type`

Цель без заголовка `Content-Type` помечается `down` с ошибкой `non-compliant scrape target sending blank Content-Type and no fallback_scrape_protocol specified for target`. Простые HTTP-серверы для статического файла без расширения этот заголовок не отправляют.

### 4. События недолговечны

Нельзя использовать события как архив: они хранятся ограниченное время и сворачиваются.

### 5. Агент логов не видит control-plane

DaemonSet без toleration запустился на 2 нодах из 3 (в опыте). Логи системных подов control-plane останутся несобранными.

### 6. Логи исчезают вместе с подом

После удаления пода его каталог в `/var/log/pods` удаляется; `kubectl logs` для удалённого пода не работает.

### 7. Prometheus без тома теряет данные

В опыте установка шла с `server.persistentVolume.enabled=false`, то есть без постоянного тома. Это допустимо для одноразового кластера и опасно в production.

### 8. `Defaulted container`

В поде с несколькими контейнерами `kubectl logs` без `-c` показывает только один контейнер (в опыте первый, `app`) и пишет об этом строку `Defaulted container "app" out of: app, side`, которую легко не заметить.

## Компромиссы и альтернативы

| Подход | Плюсы | Минусы |
|---|---|---|
| `kubectl logs`/events | ничего ставить не надо | нет истории, нет поиска |
| DaemonSet-агент (fluent-bit) | история и поиск, стандартный шаблон | нужны хранилище, tolerations, метаданные |
| Sidecar-агент в каждом поде | гибкая обработка для приложения | расходует ресурсы пода; не запускался |
| Prometheus (pull) | зрелая экосистема, PromQL | локальное хранилище, нужен том, кардинальность |
| Оператор Prometheus (ServiceMonitor) | декларативные цели | отдельный компонент; не запускался |
| Управляемые решения облака | меньше эксплуатации | привязка к провайдеру; не запускались |
| OpenTelemetry | единый стандарт трейсов, метрик, логов | усложнение; не запускался |

## Вопросы с собеседований

<details><summary>1. Как работает <code>kubectl logs</code>?</summary>

Через API-сервер к kubelet ноды, который читает файл лога контейнера.

→ [Базовая теория](basics.ru.md#где-логи-на-ноде)

</details>

<details><summary>2. Как посмотреть логи конкретного контейнера и всех сразу?</summary>

`-c <контейнер>`; `--all-containers` (+ `--prefix`).

→ [Базовая теория](basics.ru.md#логи-kubectl-logs)

</details>

<details><summary>3. Ловушка: <code>logs --previous</code> вернул ошибку. Почему?</summary>

Kubelet уже удалил предыдущие экземпляры контейнера; для пода в `CrashLoopBackOff` это обычная ситуация.

→ [Подвох 1](#1---previous-не-всегда-возвращает-логи)

</details>

<details><summary>4. Где найти причину <code>ImagePullBackOff</code> или <code>FailedScheduling</code>?</summary>

В событиях пода: `kubectl describe pod` или `kubectl events --for`.

→ [Базовая теория](basics.ru.md#события)

</details>

<details><summary>5. Почему логи пропадают после удаления пода и что с этим делать?</summary>

Файлы лежат на ноде и удаляются вместе с подом. Нужен агент (DaemonSet), отправляющий логи во внешнее хранилище.

→ [Подвох 6](#6-логи-исчезают-вместе-с-подом)

</details>

<details><summary>6. Чем Prometheus отличается от metrics-server?</summary>

metrics-server хранит только последний срез для `kubectl top` и HPA; Prometheus собирает историю и отвечает на запросы PromQL.

→ [Внутреннее устройство](#внутреннее-устройство)

</details>

<details><summary>7. Что делает kube-state-metrics?</summary>

Превращает состояние объектов Kubernetes (поды, ноды, Deployments) в метрики: фазы, число перезапусков, готовность.

→ [Продвинутая теория](advanced.ru.md#prometheus-устройство-и-установка)

</details>

<details><summary>8. Ловушка: <code>rate(container_cpu_usage_seconds_total[1m])</code> пустой, хотя метрика есть. Почему?</summary>

В окне меньше двух сэмплов: интервал опроса 1 минута. Нужно окно в несколько интервалов.

→ [Подвох 2](#2-rate-с-окном-меньше-двух-интервалов-опроса)

</details>

<details><summary>9. Ловушка: цель в Prometheus <code>down</code> с ошибкой про <code>Content-Type</code>. Причина?</summary>

Приложение не отправляет заголовок `Content-Type`; Prometheus 3 требует его.

→ [Подвох 3](#3-prometheus-3-и-content-type)

</details>

<details><summary>10. Как подключить метрики своего приложения к Prometheus?</summary>

Отдать `/metrics` в текстовом формате и описать цель: аннотациями пода (если так настроен сбор) или `ServiceMonitor`/`PodMonitor` оператора.

→ [Продвинутая теория](advanced.ru.md#метрики-приложения)

</details>

<details><summary>11. Чем counter отличается от gauge и как читать counter?</summary>

Counter только растёт (сбрасывается при перезапуске), читают его через `rate()`/`increase()`; gauge — значение «сейчас».

→ [Справочник](#типы-метрик-prometheus)

</details>

<details><summary>12. Почему DaemonSet с агентом логов не собрал логи с control-plane?</summary>

На control-plane стоит taint; без toleration под не планируется. Нужны `tolerations`.

→ [Подвох 5](#5-агент-логов-не-видит-control-plane)

</details>

## Шпаргалка

```text
kubectl logs P [-c C] [--all-containers --prefix] [--tail N] [--since 5s] [--previous] [-l sel]
kubectl events --for pod/P --types=Warning
kubectl get --raw /readyz?verbose
kubectl get --raw /api/v1/nodes/N/proxy/metrics/cadvisor
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
```

- stdout/stderr → файлы на ноде → агент → хранилище.
- События — быстрые подсказки, но недолговечные.
- Prometheus: pull, `rate()` с окном ≥ 4 интервалов, корректный `Content-Type`.
- DaemonSet-агент: tolerations для всех нужных нод.

## Связанные материалы

- Предыдущий уровень: [продвинутая теория](advanced.ru.md)
- Метрики и HPA: [15 · Автомасштабирование](../15-autoscaling/expert.ru.md)
- DaemonSet: [12 · Другие workloads](../12-workloads/expert.ru.md)
- Probes и перезапуски: [11 · Probes и ресурсы](../11-probes-and-resources/expert.ru.md)
- Планирование и taints: [18 · Планирование подов](../18-scheduling/overview.ru.md)

## Источники

- [Logging Architecture](https://kubernetes.io/docs/concepts/cluster-administration/logging/)
- [Metrics For Kubernetes System Components](https://kubernetes.io/docs/concepts/cluster-administration/system-metrics/)
- [Kubernetes API health endpoints](https://kubernetes.io/docs/reference/using-api/health-checks/)
- [Prometheus: Metric types](https://prometheus.io/docs/concepts/metric_types/)
- [Prometheus: rate()](https://prometheus.io/docs/prometheus/latest/querying/functions/#rate)
- [kube-state-metrics: pod metrics](https://github.com/kubernetes/kube-state-metrics/blob/main/docs/metrics/workload/pod-metrics.md)
