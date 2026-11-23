---
title: "Наблюдаемость: базовая теория"
lang: ru
track: kubernetes
kind: module
doc: basics
slug: 16-observability
part: 3
tags: [logs, kubectl-logs, events, previous-logs, kubelet, observability]
versions: "Kubernetes 1.37.0 (kind v0.34), kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Наблюдаемость: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [04 · Pod](../04-pods/overview.ru.md) · **Время:** ~1,5 ч

Все примеры выполнены в kind (Kubernetes 1.37.0, три ноды), namespace `learn-16`. Два пода для опытов:

- `multi` — два контейнера: `app` пишет в stdout строки `app line N` раз в секунду, `side` — в stderr строку `side-log` раз в две секунды;
- `crash` — контейнер печатает `boot-<время>` и завершается с кодом 1.

## Зачем это нужно

Приложение в кластере нельзя «открыть и посмотреть». Три вопроса возникают постоянно: что оно пишет, что с ним происходит (перезапуски, ошибки загрузки образа) и как оно себя чувствует во времени. Для первых двух хватает `kubectl`; для третьего нужны метрики (см. [продвинутую теорию](advanced.ru.md)).

## Логи: `kubectl logs`

Контейнер пишет в stdout и stderr, а kubelet сохраняет этот вывод. Если в поде несколько контейнеров, `kubectl` выберет первый и скажет об этом:

```text
$ kubectl -n learn-16 logs multi
Defaulted container "app" out of: app, side
app line 1
app line 2
…
```

Основные флаги:

```text
$ kubectl -n learn-16 logs multi -c app --tail=2
app line 19
app line 20

$ kubectl -n learn-16 logs multi --all-containers --prefix --tail=2
[pod/multi/app] app line 19
[pod/multi/app] app line 20
[pod/multi/side] side-log
[pod/multi/side] side-log

$ kubectl -n learn-16 logs -l app=logdemo --tail=1 --prefix
[pod/crash/c] boot-1795439802
[pod/multi/app] app line 20

$ kubectl -n learn-16 logs multi -c app --since=5s --timestamps
2026-11-23T13:16:47.040690397Z app line 16
2026-11-23T13:16:48.042243973Z app line 17
```

| Флаг | Что делает |
|---|---|
| `-c <контейнер>` | выбрать контейнер |
| `--all-containers` | вывод всех контейнеров пода |
| `--prefix` | добавить `[pod/контейнер]` перед строкой |
| `-l <селектор>` | логи всех подов по меткам |
| `--tail=N` | последние N строк |
| `--since=5s` | только за последние 5 секунд |
| `--timestamps` | время каждой строки |
| `-f` | следить за выводом |
| `-p` (`--previous`) | предыдущий экземпляр контейнера |

Флаги `-f` и `--limit-bytes` в опытах не запускались.

### Логи упавшего контейнера: `--previous`

Если контейнер перезапустился, `kubectl logs` показывает текущий экземпляр, а `--previous` — предыдущий. Опыт с подом `crash2` (работает 8 секунд, печатает `start-<время>` и `dying`, завершается с кодом 1):

```text
$ kubectl -n learn-16 get pod crash2 --no-headers
crash2   1/1   Running   1 (0s ago)   9s
$ kubectl -n learn-16 logs crash2
start-1795439848
$ kubectl -n learn-16 logs crash2 --previous
start-1795439840
dying
```

Пока контейнер снова работает, `--previous` показывает последние строки предыдущего запуска. Но с подом `crash`, который падает мгновенно и ждёт в `CrashLoopBackOff`, `--previous` после нескольких перезапусков не сработал:

```text
$ kubectl -n learn-16 get pod crash --no-headers
crash   0/1   Error   2 (36s ago)   37s
$ kubectl -n learn-16 logs crash --previous
unable to retrieve container logs for containerd://46ae9ad0…
```

На ноде остался только файл последнего запуска (`2.log`), предыдущие экземпляры kubelet уже удалил. Поэтому для упавших подов смотрите ещё и обычный `kubectl logs` (в опыте он вернул `boot-1795439802`) и события.

## События

События — записи о том, что произошло с объектом. Их выводит `kubectl describe` (раздел `Events`), а также отдельные команды:

```text
$ kubectl -n learn-16 events --for pod/crash --types=Warning
LAST SEEN           TYPE      REASON    OBJECT      MESSAGE
48s (x3 over 86s)   Warning   BackOff   Pod/crash   Back-off restarting failed container c in pod crash_learn-16(…)

$ kubectl -n learn-16 get events --field-selector involvedObject.name=crash2,type=Normal \
    --sort-by=.lastTimestamp -o custom-columns=REASON:.reason,COUNT:.count
REASON      COUNT
Scheduled   1
Pulled      3
Created     3
Started     3
```

Типичный порядок событий пода: `Scheduled` → `Pulled` → `Created` → `Started`; при проблемах — `BackOff`, `Failed`, `FailedScheduling` (см. [модуль 18](../18-scheduling/basics.ru.md#как-планировщик-выбирает-ноду)).

События хранятся в API ограниченное время. В опыте у API-сервера нет флага `--event-ttl`; по документации срок жизни события по умолчанию — один час.

> ⚠️ Проверить: срок хранения событий по умолчанию (1 час) взят из документации, в опыте проверено только отсутствие флага `--event-ttl` в манифесте kube-apiserver.

## Где логи на ноде

Kubelet записывает вывод контейнера в файлы на ноде. В kind это можно посмотреть через `docker exec`:

```text
$ docker exec learn-worker ls -l /var/log/pods/learn-16_multi_<uid>/app/
-rw-r----- 1 root root 1445 Nov 23 13:16 0.log
$ docker exec learn-worker tail -2 /var/log/pods/learn-16_multi_<uid>/app/0.log
2026-11-23T13:16:58.055797529Z stdout F app line 27
2026-11-23T13:16:59.056976392Z stdout F app line 28
$ docker exec learn-worker ls -l /var/log/containers | grep multi
multi_learn-16_app-a8dc….log -> /var/log/pods/learn-16_multi_<uid>/app/0.log
```

- Каталог `/var/log/pods/<namespace>_<под>_<uid>/<контейнер>/` содержит файлы по номеру перезапуска (`0.log`, `1.log`, …).
- Каталог `/var/log/containers/` содержит символические ссылки на них, удобные для агентов сбора логов.
- Формат строки — CRI: время, поток (`stdout` или `stderr`), метка (`F` — полная строка, `P` — часть), текст.

Размер файла и число файлов ограничены настройками kubelet:

```text
$ kubectl get --raw /api/v1/nodes/learn-worker/proxy/configz   # поля kubeletconfig
{'containerLogMaxSize': '10Mi', 'containerLogMaxFiles': 5, 'containerLogMaxWorkers': 1, 'containerLogMonitorInterval': '10s'}
```

После удаления пода его файлы удаляются с ноды: история логов нужна во внешнем хранилище (см. [продвинутую теорию](advanced.ru.md#сбор-логов-демоном-на-ноде)).

## Ошибки новичка

- ❌ Забыть `-c` в поде с несколькими контейнерами и не заметить `Defaulted container` → ✅ явно указывать контейнер или `--all-containers`.
- ❌ Ждать, что `--previous` всегда сработает → ✅ дополнять событиями и обычными логами; важные логи — во внешнем хранилище.
- ❌ Искать причину сбоя только в логах → ✅ сначала `kubectl describe pod` и события.
- ❌ Рассчитывать на события как на архив → ✅ они недолговечны.
- ❌ Писать логи в файлы внутри контейнера → ✅ в stdout/stderr.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — сбор логов, Prometheus, Grafana
- Pod и жизненный цикл: [04 · Pod](../04-pods/overview.ru.md)
- Probes и перезапуски: [11 · Probes и ресурсы](../11-probes-and-resources/overview.ru.md)
- `kubectl top`: [11 · Probes и ресурсы](../11-probes-and-resources/basics.ru.md#фактическое-потребление-kubectl-top)

## Источники

- [Logging Architecture](https://kubernetes.io/docs/concepts/cluster-administration/logging/)
- [kubectl logs](https://kubernetes.io/docs/reference/kubectl/generated/kubectl_logs/)
- [kubectl events](https://kubernetes.io/docs/reference/kubectl/generated/kubectl_events/)
- [Debug Running Pods](https://kubernetes.io/docs/tasks/debug/debug-application/debug-running-pod/)
