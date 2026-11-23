---
title: "Probes и ресурсы: базовая теория"
lang: ru
track: kubernetes
kind: module
doc: basics
slug: 11-probes-and-resources
part: 2
tags: [probes, liveness-probe, readiness-probe, resources, requests, limits, qos]
versions: "Kubernetes 1.37.0 (kind v0.34, cgroup v2), kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Probes и ресурсы: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [04 · Pod](../04-pods/overview.ru.md) · **Время:** ~3 ч

Все примеры выполнены в kind (Kubernetes 1.37.0), namespace `learn-11`.

## Зачем это нужно

Контейнер может быть «запущен», но не работать: приложение зависло, ещё не прогрелось или потеряло связь с базой. Без подсказок Kubernetes видит лишь, что процесс жив. Пробы позволяют kubelet проверять само приложение. Второй вопрос — ресурсы: без заявки планировщик не знает, сколько места нужно поду, а без потолка один под может съесть всю ноду.

## Пробы

### Три вида

| Проба | Вопрос | Реакция при провале |
|---|---|---|
| `livenessProbe` | приложение живо? | kubelet **перезапускает** контейнер |
| `readinessProbe` | приложение готово принимать трафик? | под убирается из endpoints Service, **без перезапуска** |
| `startupProbe` | приложение закончило запуск? | пока не пройдена, liveness и readiness не выполняются; при провале — перезапуск |

### Механизмы

| Механизм | Как проверяет | Успех |
|---|---|---|
| `httpGet` | HTTP GET на адрес пода | код 200–399 |
| `tcpSocket` | попытка открыть TCP-соединение | порт принимает соединение |
| `exec` | команда в контейнере | код выхода 0 |
| `grpc` | gRPC health-запрос | статус `SERVING` |

> ⚠️ Проверить: диапазон кодов 200–399 для `httpGet` и статус `SERVING` для `grpc` даны по документации; запуском проверены провал `httpGet` (код 404), `tcpSocket`, `exec` и провал `grpc` на закрытом порту (подробнее — в [продвинутой теории](advanced.ru.md#как-проектировать-пробы)).

### Параметры и значения по умолчанию

Значения, которые Kubernetes подставил сам (из `kubectl describe pod`):

```text
Liveness:   tcp-socket :80 delay=0s timeout=1s period=10s #success=1 #failure=3
Readiness:  http-get http://:80/ delay=0s timeout=1s period=10s #success=1 #failure=3
```

| Поле | Смысл | По умолчанию |
|---|---|---|
| `initialDelaySeconds` | задержка перед первой проверкой | 0 |
| `periodSeconds` | интервал между проверками | 10 |
| `timeoutSeconds` | сколько ждать ответ | 1 |
| `successThreshold` | подряд успешных для «успех» | 1 |
| `failureThreshold` | подряд неудачных для «провал» | 3 |

## Liveness: перезапуск зависшего

Под, у которого liveness-проба ходит на несуществующий путь `/healthz` (nginx отвечает `404`):

```yaml
apiVersion: v1
kind: Pod
metadata: {name: live-http}
spec:
  containers:
    - name: c
      image: nginx:1.29
      livenessProbe:
        httpGet: {path: /healthz, port: 80}
        periodSeconds: 3
        failureThreshold: 2
```

```text
$ kubectl -n learn-11 get pod live-http
NAME        READY   STATUS    RESTARTS     AGE
live-http   1/1     Running   2 (1s ago)   25s
$ kubectl -n learn-11 describe pod live-http
    Last State:     Terminated
      Reason:       Completed
      Exit Code:    0
    Restart Count:  2
    Liveness:       http-get http://:80/healthz delay=0s timeout=1s period=3s #success=1 #failure=2
  Warning  Unhealthy  1s (x4 over 10s)  kubelet  Liveness probe failed: HTTP probe failed with statuscode: 404
  Normal   Killing    1s (x2 over 7s)   kubelet  Container c failed liveness probe, will be restarted
```

После двух подряд неудач (`#failure=2`) kubelet убил контейнер (`Killing … will be restarted`), счётчик `RESTARTS` вырос. Контейнер завершился с кодом `0` и причиной `Completed`: nginx штатно обработал `SIGTERM` ([модуль 04](../04-pods/advanced.ru.md#остановка-пода)). Это не авария приложения, а решение kubelet.

## Readiness: готов ли принимать трафик

Проба `exec` читает файл `/tmp/ready`; пока файл есть, под готов:

```yaml
spec:
  containers:
    - name: c
      image: nginx:1.29
      command: ["sh", "-c", "touch /tmp/ready; exec nginx -g 'daemon off;'"]
      readinessProbe:
        exec: {command: ["cat", "/tmp/ready"]}
        periodSeconds: 2
```

```text
$ kubectl -n learn-11 get pod ready-flip
ready-flip: 1/1
$ kubectl -n learn-11 exec ready-flip -- rm /tmp/ready
$ kubectl -n learn-11 get pod ready-flip          # через ~6 с
after rm: 0/1 Running
$ kubectl -n learn-11 get pod ready-flip -o jsonpath='{range .status.conditions[*]}{.type}={.status} {end}'
PodReadyToStartContainers=True Initialized=True Ready=False ContainersReady=False PodScheduled=True
$ kubectl -n learn-11 exec ready-flip -- touch /tmp/ready
after touch: 1/1 Running
```

Под остаётся `Running`, но `Ready=False`: kubelet не перезапускал контейнер, а Service перестал отправлять ему трафик (адрес исчезает из EndpointSlice, [модуль 06](../06-services/advanced.ru.md#готовность-подов-и-endpoints)). Когда проба снова проходит, под возвращается в Service сам.

Readiness работает и при выкатке: [Deployment](../05-deployments/advanced.ru.md#застрявший-деплой) считает новый под готовым к замене старого только после успешной readiness.

## Ресурсы: requests и limits

```yaml
resources:
  requests: {cpu: 100m, memory: 64Mi}
  limits: {cpu: 200m, memory: 128Mi}
```

- **`requests`** — сколько ресурсов гарантированно нужно: планировщик ищет ноду, где свободно не меньше.
- **`limits`** — потолок: превысить его контейнер не может.
- **Единицы:** CPU — ядра (`1`) или милли-ядра (`100m` = 0.1 ядра); память — байты с суффиксами `Mi`, `Gi` (`64Mi`).

### Планирование идёт по requests

Под, просящий больше, чем есть на ноде, не планируется:

```text
$ kubectl -n learn-11 get pod huge
0/1 Pending
$ kubectl -n learn-11 describe pod huge
  Warning  FailedScheduling  default-scheduler  0/1 nodes are available: 1 Insufficient cpu. preemption: 0/1 nodes are available: 1 Preemption is not helpful for scheduling.
```

Это под с `requests.cpu: "100"` на ноде с 12 ядрами. Событие `Insufficient cpu` — самая частая причина `Pending` ([модуль 04](../04-pods/basics.ru.md#фазы-и-статусы)). Сколько ресурсов уже занято заявками, видно в `kubectl describe node`:

```text
Allocated resources:
  Resource           Requests     Limits
  cpu                1200m (10%)  100m (0%)
  memory             450Mi (1%)   532Mi (1%)
```

Это суммы заявок и потолков всех подов на ноде, а не фактическое потребление.

### Лимит не может быть меньше запроса

```text
The Pod "badlimits" is invalid: spec.containers[0].resources.requests: Invalid value: "500m": must be less than or equal to cpu limit of 100m
```

## QoS-классы

Класс вычисляется из заданных requests и limits:

| Класс | Условие | Пример |
|---|---|---|
| `Guaranteed` | у каждого контейнера и CPU, и память: requests = limits | `requests` = `limits` = `100m`/`64Mi` |
| `Burstable` | что-то задано, но не как у `Guaranteed` | лимит только по памяти, или только requests |
| `BestEffort` | ничего не задано | под без ресурсов |

```text
$ for p in guaranteed burstable besteffort onlyreq; do printf "%s -> " $p; kubectl -n learn-11 get pod $p -o jsonpath='{.status.qosClass}{"\n"}'; done
guaranteed -> Guaranteed       # requests = limits для CPU и памяти
burstable -> Burstable         # requests 50m/32Mi, лимит только по памяти 128Mi
besteffort -> BestEffort       # без resources
onlyreq -> Burstable           # только requests
```

QoS нужен при нехватке ресурсов на ноде: первыми вытесняют `BestEffort`, затем `Burstable`, последними `Guaranteed` (подробнее — в [экспертной теории](expert.ru.md#qos-и-oom_score_adj)).

## Фактическое потребление: kubectl top

`kubectl top` показывает реальное потребление, но ему нужен metrics-server. В нашем kind его нет:

```text
$ kubectl top pods -n learn-11
error: Metrics API not available
```

Установка metrics-server и мониторинг — в модулях 15 и 16.

## Ошибки новичка

- ❌ Не задавать readiness и считать под готовым сразу → ✅ без readiness контейнер `Ready` сразу после запуска, даже если приложение ещё не слушает порт.
- ❌ Делать liveness-пробу той же, что readiness, с коротким таймаутом → ✅ liveness должна быть мягче: лишний перезапуск хуже лишней задержки.
- ❌ Не задавать requests → ✅ без заявки планировщик не знает, сколько нужно, и под получает класс `BestEffort`.
- ❌ Путать requests и фактическое потребление → ✅ планировщик и `describe node` считают заявки, а не нагрузку.
- ❌ Ставить `limits` меньше `requests` → ✅ такой под отклоняется.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — startup-проба, cgroups, throttling
- Следующий модуль: модуль 12 · Другие workload-ы (в дорожной карте [трека](../../overview.ru.md))
- Предыдущие модули: [06 · Service и сеть](../06-services/overview.ru.md), [04 · Pod](../04-pods/overview.ru.md)
- Квоты и LimitRange: [10 · Namespace и RBAC](../10-namespaces-and-rbac/advanced.ru.md#resourcequota-лимиты-namespace)
- Фундамент: [трек Linux](../../../linux/overview.ru.md)

## Источники

- [Configure Liveness, Readiness and Startup Probes](https://kubernetes.io/docs/tasks/configure-pod-container/configure-liveness-readiness-startup-probes/)
- [Liveness, Readiness, and Startup Probes](https://kubernetes.io/docs/concepts/configuration/liveness-readiness-startup-probes/)
- [Resource Management for Pods and Containers](https://kubernetes.io/docs/concepts/configuration/manage-resources-containers/)
- [Pod Quality of Service Classes](https://kubernetes.io/docs/concepts/workloads/pods/pod-qos/)
- [Assign CPU Resources to Containers and Pods](https://kubernetes.io/docs/tasks/configure-pod-container/assign-cpu-resource/)
