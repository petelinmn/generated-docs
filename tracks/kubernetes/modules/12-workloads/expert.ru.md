---
title: "Другие workload-ы: экспертная теория"
lang: ru
track: kubernetes
kind: module
doc: expert
slug: 12-workloads
part: 2
tags: [statefulset, daemonset, job, cronjob, partition, podmanagementpolicy, pvc-retention, backofflimit, concurrency-policy, indexed-job]
versions: "Kubernetes 1.37.0 (kind v0.34), kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Другие workload-ы: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

Опирается на [базовую](basics.ru.md) и [продвинутую](advanced.ru.md) теорию. Все значения ниже получены на Kubernetes 1.37.0 (`kubectl explain`, опыты в kind).

## Справочник полей

Список полей `spec` получен командой `kubectl explain <kind>.spec`.

### StatefulSet

| Поле | Смысл | Значение по умолчанию (наблюдалось) |
|---|---|---|
| `replicas` | число подов | 1 |
| `selector` | селектор подов (обязателен, совпадает с метками шаблона) | — |
| `serviceName` | имя headless Service для DNS | не обязателен (набор без него создался) |
| `template` | шаблон пода | — |
| `volumeClaimTemplates` | шаблоны PVC, по одному PVC на реплику | пусто |
| `podManagementPolicy` | `OrderedReady` или `Parallel` | `OrderedReady` |
| `updateStrategy` | `RollingUpdate` (с `partition`, `maxUnavailable`) или `OnDelete` | `RollingUpdate`, `partition: 0`, `maxUnavailable: 1` |
| `persistentVolumeClaimRetentionPolicy` | `whenDeleted`, `whenScaled`: `Retain`/`Delete` | оба `Retain` |
| `minReadySeconds` | сколько под должен быть готов, чтобы считаться доступным | 0 |
| `revisionHistoryLimit` | сколько ревизий хранить | — |
| `ordinals` | начальный номер реплик (`ordinals.start`) | — |

### DaemonSet

| Поле | Смысл | По умолчанию |
|---|---|---|
| `selector`, `template` | как у Deployment | — |
| `updateStrategy` | `RollingUpdate` (`maxUnavailable`, `maxSurge`) или `OnDelete` | `{"rollingUpdate":{"maxSurge":0,"maxUnavailable":1},"type":"RollingUpdate"}` |
| `minReadySeconds` | пауза готовности | 0 |
| `revisionHistoryLimit` | число ревизий | — |

Поля `replicas` у DaemonSet нет.

### Job

| Поле | Смысл |
|---|---|
| `completions` | сколько успешных подов нужно |
| `parallelism` | сколько подов одновременно |
| `completionMode` | `NonIndexed` или `Indexed` (индекс в `JOB_COMPLETION_INDEX`) |
| `backoffLimit` | число повторов до отказа |
| `backoffLimitPerIndex`, `maxFailedIndexes` | повторы по индексам (для `Indexed`) |
| `activeDeadlineSeconds` | предельное время всего Job |
| `ttlSecondsAfterFinished` | автоудаление после завершения |
| `suspend` | приостановить |
| `podFailurePolicy` | правила реакции на причины отказа пода |
| `podReplacementPolicy` | когда создавать замену (например, после полного завершения пода) |
| `successPolicy` | условие успеха без ожидания всех индексов |
| `selector`, `manualSelector` | ручной селектор (обычно не нужен) |
| `managedBy` | внешний контроллер, ведущий Job |
| `scheduling` | изменение параметров планирования пока Job приостановлен |
| `template` | шаблон пода (`restartPolicy` только `Never` или `OnFailure`) |

### CronJob

| Поле | Смысл | По умолчанию (наблюдалось) |
|---|---|---|
| `schedule` | cron из 5 полей | обязателен |
| `jobTemplate` | шаблон Job | обязателен |
| `concurrencyPolicy` | `Allow`/`Forbid`/`Replace` | `Allow` |
| `suspend` | пауза | `false` |
| `startingDeadlineSeconds` | насколько запоздавший запуск ещё допустим | не задано |
| `timeZone` | часовой пояс расписания | не задано |
| `successfulJobsHistoryLimit`, `failedJobsHistoryLimit` | сколько завершённых Job хранить | — |

> ⚠️ Проверить: числовые умолчания `revisionHistoryLimit`, `successfulJobsHistoryLimit`, `failedJobsHistoryLimit`, `minReadySeconds` у StatefulSet/DaemonSet/CronJob здесь не снимались (в опыте для CronJob лимит истории задавался явно: 2).

## Внутреннее устройство

- **Контроллеры.** За каждый объект отвечает свой контроллер в kube-controller-manager: он наблюдает за объектом и подами по watch ([модуль 02](../02-architecture/advanced.ru.md)) и приводит факт к желаемому. Имена и идентичность подов StatefulSet выводятся из номера: под `web-1` — это «ячейка номер 1», а не конкретный экземпляр. Поэтому после удаления появляется новый под с тем же именем, новым UID и новым IP (опыт: `10.244.0.31` → `10.244.0.35`).
- **PVC привязан к имени.** Контроллер ищет PVC по шаблону `<имя шаблона>-<имя набора>-<номер>` (`data-web-1`). Если он есть, новый под использует его — так сохраняются данные.
- **DaemonSet и планировщик.** Под DaemonSet создаётся с привязкой к конкретной ноде; чтобы он попал на ноду с taint, в шаблоне нужен toleration (опыт: без него 2 пода из 3 нод, с ним 3).
- **Job следит за условиями.** При отказе Job выставляет `FailureTarget`, затем `Failed` с причиной (`BackoffLimitExceeded`); успех — условие `Complete`. Статус хранит `succeeded`, `failed`, `active`, а для `Indexed` — `completedIndexes`.
- **CronJob создаёт Job.** Имя Job составляется из имени CronJob и времени запуска в минутах от начала эпохи (`tick-29923833`); владелец — CronJob (`ownerReferences`), поэтому удаление CronJob удаляет его Job и поды. `lastScheduleTime` — время последнего запуска в UTC.

## Подвохи

### 1. `rollout status` при partition говорит «complete»

```text
$ kubectl -n learn-12 rollout status sts/web
partitioned roll out complete: 3 new pods have been updated
```

(при `partition: 2` на деле обновлён один под). Проверяйте `status.updateRevision` против `currentRevision` и образы подов.

### 2. `scale` вниз не освобождает PVC

После `scale --replicas=1` остались `data-web-0`, `data-web-1`, `data-web-2`. Место на диске продолжает тратиться; чистить нужно вручную либо задавать `whenScaled: Delete`.

### 3. Под с тем же именем — это другой объект

После `delete pod web-1` появляется под с тем же именем, но новым UID и новым IP (`10.244.0.31` → `10.244.0.35`). Кэшировать IP реплики нельзя; обращайтесь по DNS-имени `web-1.web`.

### 4. OrderedReady блокируется первой репликой

Если `stuck-0` не проходит readiness, `stuck-1` и `stuck-2` не появятся. На `Parallel` все три создаются сразу. Поэтому сломанный образ в StatefulSet выглядит как «один под в `Running 0/1` и больше ничего».

### 5. Job создаёт поды, даже если они падают

Три пода `Error` при `backoffLimit: 2` и условие `BackoffLimitExceeded`. Если `restartPolicy: OnFailure`, перезапуск идёт внутри пода (растёт `RESTARTS`), а не создаётся новый.

### 6. CronJob пропускает запуск при `Forbid`

Событие `JobAlreadyActive` — не ошибка, а штатный пропуск. История и `lastScheduleTime` при этом не растут для пропущенных запусков.

### 7. Удаление CronJob удаляет историю

Job принадлежат CronJob; после удаления CronJob логи завершённых Job теряются, если поды не собраны в хранилище логов.

### 8. TTL удаляет и логи

С `ttlSecondsAfterFinished: 15` Job и его поды исчезли примерно за полторы минуты; снимайте логи заранее или отправляйте их во внешнее хранилище.

> ⚠️ Проверить: реакция CronJob на пропущенные из-за простоя запуски (`startingDeadlineSeconds`, «more than 100 missed start times») и поведение `podFailurePolicy`, `successPolicy`, `backoffLimitPerIndex`, `managedBy` в этих материалах не запускались.

## Компромиссы и альтернативы

| Задача | Вариант | Компромисс |
|---|---|---|
| База данных в Kubernetes | StatefulSet вручную | вы сами отвечаете за репликацию, бэкапы, переключение |
| База данных в Kubernetes | оператор (CRD) | автоматизация, но сложнее и зависит от оператора |
| База данных | управляемая БД вне кластера | проще эксплуатация, но стоимость и сеть |
| Агент на ноде | DaemonSet | по поду на ноду, ресурсы на каждой ноде |
| Периодическая задача | CronJob | кластерный планировщик, не точный до секунд |
| Периодическая задача в приложении | `BackgroundService`/таймер | проще, но привязана к живому процессу ([рецепт](../../../csharp/recipes/background-jobs/overview.ru.md)) |
| Очередь задач | Job на элемент / worker Deployment | зависит от нагрузки и порядка |

## Вопросы с собеседований

<details><summary>1. Чем StatefulSet отличается от Deployment?</summary>

Стабильные имена подов (`web-0`, `web-1`), собственный PVC на реплику (`volumeClaimTemplates`), стабильный DNS через headless Service и упорядоченные создание, удаление и обновление. Deployment создаёт взаимозаменяемые поды со случайными именами.

→ [Базовая теория](basics.ru.md#statefulset)

</details>

<details><summary>2. Что произойдёт с данными, если удалить под StatefulSet?</summary>

Новый под получит то же имя и тот же PVC, данные сохранятся (опыт: файл `custom.txt` остался); меняются IP и UID.

→ [Продвинутая теория](advanced.ru.md#statefulset-идентичность-переживает-пересоздание)

</details>

<details><summary>3. Что с PVC после <code>scale</code> вниз и после удаления StatefulSet?</summary>

По умолчанию `Retain`: PVC остаются в обоих случаях. С `persistentVolumeClaimRetentionPolicy` в `Delete` они удаляются при уменьшении (старший номер) и при удалении набора.

→ [Продвинутая теория](advanced.ru.md#политика-pvc)

</details>

<details><summary>4. Зачем StatefulSet нужен headless Service?</summary>

Чтобы у каждого пода была своя DNS-запись (`web-1.web.<ns>.svc`), а клиент мог обратиться к конкретной реплике, а не к случайной.

→ [Базовая теория](basics.ru.md#что-даёт-statefulset)

</details>

<details><summary>5. Что делает <code>partition</code> и в каком порядке идёт обновление?</summary>

Обновляются только поды с номером не меньше `partition`, в порядке от старшего к младшему. Это канарейка: сначала `partition: 2`, затем снижаем до 0.

→ [Продвинутая теория](advanced.ru.md#обновление-с-partition)

</details>

<details><summary>6. Ловушка: <code>rollout status</code> написал «partitioned roll out complete». Всё ли обновилось?</summary>

Нет: при `partition > 0` обновлена только часть подов (в опыте один из трёх). Сверяйте `currentRevision` и `updateRevision`, образы подов.

→ [Подвох 1](#1-rollout-status-при-partition-говорит-complete)

</details>

<details><summary>7. Почему DaemonSet запустил 2 пода на 3-нодном кластере?</summary>

На ноде control plane стоит taint `node-role.kubernetes.io/control-plane:NoSchedule`; без toleration под туда не попадает. С toleration — 3 пода.

→ [Продвинутая теория](advanced.ru.md#daemonset-ноды-toleration-и-селектор)

</details>

<details><summary>8. Можно ли задать <code>replicas</code> у DaemonSet?</summary>

Нет: поля нет, число подов определяется числом подходящих нод (с учётом `nodeSelector` и toleration).

→ [Справочник](#справочник-полей)

</details>

<details><summary>9. Какие значения <code>restartPolicy</code> допустимы в Job и что будет с <code>Always</code>?</summary>

`Never` и `OnFailure`. С `Always` объект отвергается: `Required value: valid values: "OnFailure", "Never"`.

→ [Базовая теория](basics.ru.md#job)

</details>

<details><summary>10. Чем <code>OnFailure</code> отличается от <code>Never</code> при отказе?</summary>

С `OnFailure` kubelet перезапускает контейнер в том же поде (растёт `RESTARTS`). С `Never` под остаётся в `Error`, а Job создаёт новый под, пока не исчерпан `backoffLimit`.

→ [Подвох 5](#5-job-создаёт-поды-даже-если-они-падают)

</details>

<details><summary>11. Что значит <code>backoffLimit: 2</code>, сколько подов будет при вечном отказе?</summary>

Три пода: первая попытка и два повтора (в опыте три пода `Error`, условия `FailureTarget` и `Failed` с причиной `BackoffLimitExceeded`).

→ [Продвинутая теория](advanced.ru.md#backofflimit)

</details>

<details><summary>12. Как работает <code>Indexed</code> Job?</summary>

Каждому поду выдаётся индекс в `JOB_COMPLETION_INDEX` (и в имени пода), статус хранит `completedIndexes` (`0-5`). `completions: 6` и `parallelism: 2` дают шесть запусков парами.

→ [Базовая теория](basics.ru.md#несколько-запусков-completions-и-parallelism)

</details>

<details><summary>13. Как предотвратить накопление завершённых Job?</summary>

`ttlSecondsAfterFinished` у Job и `successfulJobsHistoryLimit`/`failedJobsHistoryLimit` у CronJob.

→ [Продвинутая теория](advanced.ru.md#автоочистка-ttlsecondsafterfinished)

</details>

<details><summary>14. Что делает <code>concurrencyPolicy: Forbid</code>?</summary>

Пока предыдущий запуск работает, новый пропускается; событие `JobAlreadyActive … concurrency policy is Forbid`.

→ [Продвинутая теория](advanced.ru.md#concurrencypolicy)

</details>

<details><summary>15. Гарантирует ли CronJob ровно один запуск в срок?</summary>

Нет: запуски могут быть пропущены, задержаны или дублированы, поэтому задачи должны быть идемпотентными.

→ [Ошибки новичка](basics.ru.md#ошибки-новичка)

</details>

<details><summary>16. Как запустить CronJob вручную?</summary>

`kubectl create job manual-run --from=cronjob/tick`: Job создаётся из шаблона, владелец — CronJob.

→ [Базовая теория](basics.ru.md#cronjob)

</details>

<details><summary>17. Ловушка: расписание <code>every minute</code>. Что произойдёт?</summary>

Создание отвергнется: `expected exactly 5 fields, found 2`. Нужно `* * * * *`.

→ [Базовая теория](basics.ru.md#cronjob)

</details>

<details><summary>18. Чем <code>OrderedReady</code> и <code>Parallel</code> отличаются на практике?</summary>

При `OrderedReady` пока `stuck-0` не готов, остальные поды не создаются; при `Parallel` все три создаются сразу. Порядок удаления при `scale 0` — от старшего номера (`web-2`, `web-1`, `web-0`).

→ [Продвинутая теория](advanced.ru.md#podmanagementpolicy-порядок-или-параллельно)

</details>

## Связанные материалы

- Предыдущий уровень: [продвинутая теория](advanced.ru.md)
- Самопроверка: [квиз](quiz.ru.md), [упражнения](exercises.ru.md)
- Архитектура и watch: [02 · Архитектура](../02-architecture/advanced.ru.md)
- Хранилище: [08 · Хранилище](../08-storage/expert.ru.md)

## Источники

- [StatefulSet basics](https://kubernetes.io/docs/tutorials/stateful-application/basic-stateful-set/)
- [StatefulSet API reference](https://kubernetes.io/docs/reference/kubernetes-api/workload-resources/stateful-set-v1/)
- [DaemonSet API reference](https://kubernetes.io/docs/reference/kubernetes-api/workload-resources/daemon-set-v1/)
- [Job API reference](https://kubernetes.io/docs/reference/kubernetes-api/workload-resources/job-v1/)
- [CronJob API reference](https://kubernetes.io/docs/reference/kubernetes-api/workload-resources/cron-job-v1/)
- [Pod failure policy](https://kubernetes.io/docs/concepts/workloads/controllers/job/#pod-failure-policy)
