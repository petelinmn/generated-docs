---
title: "Диагностика: базовая теория"
lang: ru
track: kubernetes
kind: module
doc: basics
slug: 21-troubleshooting
part: 3
tags: [troubleshooting, describe, events, imagepullbackoff, crashloopbackoff, oomkilled, pending, createcontainerconfigerror]
versions: "Kubernetes 1.37.0 (kind v0.34, 3 ноды), kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Диагностика: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [04 · Pod](../04-pods/overview.ru.md), [16 · Наблюдаемость](../16-observability/basics.ru.md) · **Время:** ~2 ч

Все примеры выполнены в kind (Kubernetes 1.37.0), namespace `learn-21`: для каждой типовой поломки создан под с нарочно неверной настройкой.

## Зачем это нужно

Большая часть времени с Kubernetes уходит не на написание манифестов, а на вопрос «почему не работает». Ответ почти всегда уже есть в кластере: нужно уметь его найти и прочитать.

## Метод: от статуса к причине

1. **Статус:** `kubectl get pods` — колонка `STATUS` называет этап.
2. **Описание и события:** `kubectl describe pod <под>` — раздел `Events` внизу объясняет причину.
3. **Логи:** `kubectl logs <под>` (и `--previous` для перезапущенного контейнера, см. [модуль 16](../16-observability/basics.ru.md#логи-упавшего-контейнера---previous)).
4. **Вход внутрь:** `kubectl exec` или `kubectl debug` ([продвинутая теория](advanced.ru.md#kubectl-debug-заглянуть-внутрь)).
5. **Связанные объекты:** Service, PVC, нода, namespace (квота).

Состояние контейнера лежит в самом поде: `status.containerStatuses[].state` (сейчас) и `lastState` (предыдущий запуск) с полями `reason`, `message` и `exitCode`.

## Типовые поломки

Все шесть подов созданы одновременно:

```text
$ kubectl -n learn-21 get pods
NAME     READY   STATUS                       RESTARTS      AGE
badcmd   0/1     RunContainerError            2 (20s ago)   45s
badimg   0/1     ImagePullBackOff             0             45s
nocfg    0/1     CreateContainerConfigError   0             45s
noreg    0/1     ErrImagePull                 0             45s
oom      0/1     Error                        2 (29s ago)   45s
pvcpod   0/1     Pending                      0             45s
```

### ImagePullBackOff и ErrImagePull

Образ не скачивается: нет такого тега или недоступен реестр. `ErrImagePull` — сама ошибка, `ImagePullBackOff` — пауза перед очередной попыткой.

```text
$ kubectl -n learn-21 get pod badimg -o jsonpath='{.status.containerStatuses[0].state}'
{"waiting":{"message":"Back-off pulling image \"nginx:no-such-tag\": ErrImagePull: rpc error: code = NotFound desc = failed to pull and unpack image \"docker.io/library/nginx:no-such-tag\": … docker.io/library/nginx:no-such-tag: not found","reason":"ImagePullBackOff"}}
```

Под `noreg` с образом `registry.invalid/app:1` получил тот же статус из-за недоступного реестра. Проверьте имя, тег, права (`imagePullSecrets`) и сеть до реестра.

### CreateContainerConfigError

Под ссылается на ConfigMap или Secret, которого нет:

```text
$ kubectl -n learn-21 get pod nocfg -o jsonpath='{.status.containerStatuses[0].state}'
{"waiting":{"message":"configmap \"missing-cm\" not found","reason":"CreateContainerConfigError"}}
```

Образ скачан (`Successfully pulled image`), но контейнер создать не удалось. Создайте недостающий объект ([модуль 07](../07-config-and-secrets/overview.ru.md)); под запустится сам при следующей попытке.

### RunContainerError

Контейнер создан, но процесс не стартует (команда `/no/such/binary`):

```text
Error: failed to create containerd task: failed to create shim task: OCI runtime create failed: runc create failed: unable to start container process: error during container init: exec: "/no/such/binary": stat /no/such/binary: no such file or directory
```

Затем под уходит в `BackOff`. Проверьте `command` и `args`, путь в образе и права на исполнение.

### OOMKilled

Контейнер превысил лимит памяти (`limits.memory: 50Mi`) и был убит ядром:

```text
$ kubectl -n learn-21 get pod oom -o jsonpath='{.status.containerStatuses[0].lastState.terminated}'
{"exitCode":137,"finishedAt":"2026-11-23T13:55:12Z","reason":"OOMKilled","startedAt":"2026-11-23T13:55:12Z"}
$ kubectl -n learn-21 get pod oom
NAME   READY   STATUS      RESTARTS      AGE
oom    0/1     OOMKilled   2 (24s ago)   25s
```

Код `137` равен 128 + 9, то есть сигнал `SIGKILL`. Увеличьте лимит или найдите утечку ([модуль 11](../11-probes-and-resources/basics.ru.md)).

В одном из опытов под, который пытался занять память командой `x=$(head -c 200000000 /dev/zero | tr …)`, завершился с кодом `255` и причиной `Error`, а не `OOMKilled`. Поэтому не делайте вывод о нехватке памяти по одному коду выхода, а смотрите `lastState.reason`.

### CrashLoopBackOff

Контейнер запускается и сразу завершается с ошибкой (`exit 1`); kubelet перезапускает его с растущей паузой, а статус показывает `Error` между попытками (в опыте модуля 16) и `CrashLoopBackOff` во время паузы (по документации). Причину ищите в логах (`logs`, `logs --previous`) и коде выхода (`lastState.terminated.exitCode`). Пример и тонкости `--previous` — в [модуле 16](../16-observability/basics.ru.md#логи-упавшего-контейнера---previous).

### Pending

Под принят, но не размещён. Причина — в событии пода:

```text
$ kubectl -n learn-21 describe pod pvcpod | tail -3
  Warning  FailedScheduling  46s   default-scheduler  0/3 nodes are available: pod has unbound immediate PersistentVolumeClaims. not found
$ kubectl -n learn-21 describe pvc nopv | tail -3
  Warning  ProvisioningFailed  14s (x4 over 51s)  persistentvolume-controller  storageclass.storage.k8s.io "no-such-class" not found
```

Под ждёт PVC, а у PVC указан несуществующий StorageClass. Вывод: смотреть нужно связанный объект (PVC), а не только сам под. Другие причины `Pending` (ресурсы, taints, селекторы) разобраны в [модуле 18](../18-scheduling/basics.ru.md#как-планировщик-выбирает-ноду), а PVC — в [модуле 08](../08-storage/overview.ru.md).

## Таблица «статус → первая проверка»

| Статус | Первая проверка |
|---|---|
| `Pending` | события пода, PVC, ресурсы нод, taints |
| `ImagePullBackOff`, `ErrImagePull` | имя и тег образа, реестр, `imagePullSecrets` |
| `CreateContainerConfigError` | ConfigMap и Secret, на которые есть ссылки |
| `RunContainerError` | `command`, `args`, путь в образе |
| `CrashLoopBackOff`, `Error` | `logs`, `logs --previous`, код выхода |
| `OOMKilled` (`137`) | `limits.memory`, утечки памяти |
| `0/1 Running` | readiness-проба, `describe` ([продвинутая теория](advanced.ru.md#service-без-endpoints)) |

## Ошибки новичка

- ❌ Смотреть только `logs`, когда контейнер ещё не создан → ✅ `describe` и события.
- ❌ Удалять и создавать под заново, не прочитав события → ✅ сначала собрать факты.
- ❌ Путать `ErrImagePull` и `ImagePullBackOff` → ✅ первая — ошибка, вторая — пауза между попытками.
- ❌ Считать код `137` всегда OOM → ✅ проверить `lastState.reason`.
- ❌ Искать причину `Pending` в самом приложении → ✅ планировщик и тома.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — Service, DNS, `kubectl debug`, квота, `Terminating`, нода
- Логи и события: [16 · Наблюдаемость](../16-observability/overview.ru.md)
- Ресурсы и пробы: [11 · Probes и ресурсы](../11-probes-and-resources/overview.ru.md)
- Хранилище: [08 · Хранилище](../08-storage/overview.ru.md)

## Источники

- [Debug Pods](https://kubernetes.io/docs/tasks/debug/debug-application/debug-pods/)
- [Troubleshoot Applications](https://kubernetes.io/docs/tasks/debug/debug-application/)
- [Determine the Reason for Pod Failure](https://kubernetes.io/docs/tasks/debug/debug-application/determine-reason-pod-failure/)
- [Images (pull policy and ImagePullBackOff)](https://kubernetes.io/docs/concepts/containers/images/)
