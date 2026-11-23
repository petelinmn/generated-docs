---
title: "Другие workload-ы: продвинутая теория"
lang: ru
track: kubernetes
kind: module
doc: advanced
slug: 12-workloads
part: 2
tags: [statefulset, daemonset, job, cronjob, partition, podmanagementpolicy, pvc-retention, toleration, backofflimit, concurrency-policy, ttl]
versions: "Kubernetes 1.37.0 (kind v0.34), kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Другие workload-ы: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md): манифесты четырёх контроллеров и их базовое поведение не повторяются.

## StatefulSet: идентичность переживает пересоздание

Запишем файл в том реплики `web-1` и удалим под:

```text
$ kubectl -n learn-12 exec web-1 -- sh -c 'echo custom-data > /usr/share/nginx/html/custom.txt'
$ kubectl -n learn-12 delete pod web-1 --now
ip before=10.244.0.31 after=10.244.0.35  uid changed=yes
$ kubectl -n learn-12 exec web-1 -- cat /usr/share/nginx/html/custom.txt
custom-data
$ kubectl -n learn-12 get pvc data-web-1
data-web-1 Bound
```

Под получил то же имя `web-1` и тот же PVC (`data-web-1`) с данными, а вот IP и UID сменились. Поэтому обращаться нужно по имени (`web-1.web`), а не по IP ([модуль 06](../06-services/basics.ru.md#стабильный-адрес)).

### Масштабирование

```text
$ kubectl -n learn-12 scale sts/web --replicas=1
$ kubectl -n learn-12 get pods,pvc --no-headers | awk '{print $1,$2}'
web-0 1/1
data-web-0 Bound
data-web-1 Bound
data-web-2 Bound
$ kubectl -n learn-12 scale sts/web --replicas=3
$ kubectl -n learn-12 exec web-1 -- cat /usr/share/nginx/html/custom.txt
custom-data
```

При уменьшении удаляются поды `web-2` и `web-1` (старший номер первым), но их PVC остаются. При увеличении `web-1` снова получает свой PVC, и данные на месте. При удалении в обратном порядке:

```text
web-2 Terminating
web-1 Terminating
web-0 Terminating
```

### Политика PVC

Что будет с PVC при масштабировании вниз и удалении набора, определяет `persistentVolumeClaimRetentionPolicy`:

```text
$ kubectl -n learn-12 get sts web -o jsonpath='{.spec.persistentVolumeClaimRetentionPolicy}'
{"whenDeleted":"Retain","whenScaled":"Retain"}
```

По умолчанию оба события `Retain`: после `delete sts web` PVC (`data-web-0`, `data-web-1`, `data-web-2`) остались. С `whenDeleted: Delete` и `whenScaled: Delete`:

```text
$ kubectl -n learn-12 patch sts web -p '{"spec":{"persistentVolumeClaimRetentionPolicy":{"whenDeleted":"Delete","whenScaled":"Delete"}}}'
$ kubectl -n learn-12 scale sts/web --replicas=2
$ kubectl -n learn-12 get pvc --no-headers | awk '{print $1}'
data-web-0 data-web-1
$ kubectl -n learn-12 delete sts web
$ kubectl -n learn-12 get pvc
No resources found in learn-12 namespace.
```

При уменьшении исчез `data-web-2`, при удалении набора — все остальные. Выбирайте `Delete` только осознанно: данные пропадут.

### podManagementPolicy: порядок или параллельно

По умолчанию `OrderedReady`: следующий под создаётся только после готовности предыдущего. Если `web-0` не становится готовым, остальные не создаются. Опыт: два набора по 3 реплики с readiness-пробой, которая никогда не проходит:

```text
OrderedReady:  stuck-0 0/1 Running
Parallel:      par-0 0/1 Running   par-1 0/1 Running   par-2 0/1 Running
```

`OrderedReady` остановился на `stuck-0`, а `Parallel` (`podManagementPolicy: Parallel`) создал все три сразу. Параллельный режим ускоряет запуск, если приложению не важен порядок.

### Обновление с partition

`updateStrategy.rollingUpdate.partition` задаёт: обновляются только поды с номером **не меньше** `partition`. Это канарейка и поэтапная выкатка. Меняем образ с `partition: 2`:

```text
$ kubectl -n learn-12 patch sts web -p '{"spec":{"updateStrategy":{"type":"RollingUpdate","rollingUpdate":{"partition":2}}}}'
$ kubectl -n learn-12 set image sts/web nginx=nginx:1.28
web-0 image: nginx:1.29
web-1 image: nginx:1.29
web-2 image: nginx:1.28
updated=1 current=2 revisions: cur=web-796ff6cbd7 upd=web-84b47bb8f8
$ kubectl -n learn-12 patch sts web -p '{"spec":{"updateStrategy":{"type":"RollingUpdate","rollingUpdate":{"partition":0}}}}'
web-0: nginx:1.28
web-1: nginx:1.28
web-2: nginx:1.28
```

Обновился только `web-2`; когда он проверен, `partition` снижают до 0, и обновление идёт в обратном порядке номеров: 2, 1, 0. Статус показывает две ревизии: `currentRevision` и `updateRevision`. Сообщение `kubectl rollout status` при `partition > 0` вводит в заблуждение: оно уже писало `partitioned roll out complete: 3 new pods have been updated`, хотя обновлён был лишь один под.

Значения по умолчанию, подставленные API: `podManagementPolicy: OrderedReady`, `updateStrategy: {rollingUpdate: {maxUnavailable: 1, partition: 0}, type: RollingUpdate}`.

> ⚠️ Проверить: `serviceName` в этой версии не обязателен: StatefulSet без него создал под `nosvc-0`, но без headless Service нет стабильных DNS-имён; поведение `updateStrategy.type: OnDelete` и `maxUnavailable` не проверялось.

## DaemonSet: ноды, toleration и селектор

Тот же DaemonSet с toleration для taint control plane:

```yaml
spec:
  template:
    spec:
      tolerations:
        - {key: node-role.kubernetes.io/control-plane, operator: Exists, effect: NoSchedule}
```

```text
$ kubectl -n learn-12 get ds agent2
NAME     DESIRED   CURRENT   READY   UP-TO-DATE   AVAILABLE
agent2   3         3         3       3            3
agent2-4hngv Running learn-multi-worker2
agent2-q8vwh Running learn-multi-worker
agent2-qs2bd Running learn-multi-control-plane
```

С toleration под появился и на ноде control plane (3 из 3).

Удалённый под возвращается на ту же ноду:

```text
$ kubectl -n learn-12 delete pod agent-29n99 --now     # был на learn-multi-worker2
agent-56hbr Running learn-multi-worker
agent-5kbq8 Running learn-multi-worker2
```

Ограничить DaemonSet подмножеством нод можно `nodeSelector`:

```text
$ kubectl label node learn-multi-worker disk=ssd
$ kubectl -n learn-12 patch ds agent -p '{"spec":{"template":{"spec":{"nodeSelector":{"disk":"ssd"}}}}}'
agent-56hbr Running learn-multi-worker
agent-5kbq8 Terminating learn-multi-worker2
desired=1 current=1 ready=1
```

Под остался только на ноде с меткой (модуль 18 разбирает `nodeSelector`, affinity и taints). По умолчанию у DaemonSet обновление `RollingUpdate` с `maxUnavailable: 1`:

```text
{"rollingUpdate":{"maxSurge":0,"maxUnavailable":1},"type":"RollingUpdate"}
```

Новая нода получает под автоматически.

> ⚠️ Проверить: автоматическое появление пода на добавленной ноде и стратегия `OnDelete` у DaemonSet не запускались.

## Job: повторы, индексы, очистка

### backoffLimit

При ошибках Job создаёт новые поды до `backoffLimit` повторов (по умолчанию 6), затем помечается неуспешной:

```text
$ kubectl -n learn-12 get job failing        # backoffLimit: 2, команда exit 1
NAME      STATUS   COMPLETIONS   DURATION   AGE
failing   Failed   0/1           69s        69s
$ kubectl -n learn-12 get pods -l job-name=failing
failing-gt5sd 0/1 Error
failing-qwf2r 0/1 Error
failing-rtzsv 0/1 Error
$ kubectl -n learn-12 get job failing -o jsonpath='{range .status.conditions[*]}{.type}={.status} ({.reason}): {.message}{"\n"}{end}'
FailureTarget=True (BackoffLimitExceeded): Job has reached the specified backoff limit
Failed=True (BackoffLimitExceeded): Job has reached the specified backoff limit
```

Три пода: первая попытка и два повтора (`backoffLimit: 2`), с нарастающей задержкой между попытками (все три заняли 69 секунд). Условия `FailureTarget` и `Failed` с причиной `BackoffLimitExceeded`.

### Параллелизм

В Job `parallel` (`completions: 6`, `parallelism: 2`) наблюдение каждые две секунды показало пары:

```text
t=3  running=0 completed=2
t=5  running=2 completed=2
t=7  running=0 completed=4
t=9  running=2 completed=4
t=11 running=0 completed=6
$ kubectl -n learn-12 get job parallel -o jsonpath='succeeded={.status.succeeded} completedIndexes={.status.completedIndexes}'
succeeded=6 completedIndexes=0-5
```

Одновременно работали не больше двух подов; всего выполнено шесть.

### Автоочистка: ttlSecondsAfterFinished

Завершённые Job и их поды остаются, пока их не удалят. Поле `ttlSecondsAfterFinished` удаляет Job автоматически:

```text
$ kubectl -n learn-12 get job ttl          # ttlSecondsAfterFinished: 15, через ~90 с
Error from server (NotFound): jobs.batch "ttl" not found
```

### Другие поля Job

`activeDeadlineSeconds` — максимальное время всего Job; `suspend` — приостановить; `backoffLimitPerIndex`, `podFailurePolicy`, `successPolicy` — тонкая настройка повторов и успеха (см. [экспертную теорию](expert.ru.md#справочник-полей)).

## CronJob: параллелизм, пауза, история

### concurrencyPolicy

Что делать, если предыдущий запуск ещё идёт:

| Политика | Поведение |
|---|---|
| `Allow` (по умолчанию) | запускать параллельно |
| `Forbid` | пропустить новый запуск |
| `Replace` | остановить старый и запустить новый |

С `Forbid` и задачей на 100 секунд при расписании каждую минуту Job один, а событие объясняет пропуски:

```text
JobAlreadyActive   Not starting job because prior execution is running and concurrency policy is Forbid
```

Значения по умолчанию в нашем кластере: `concurrencyPolicy=Allow`, `suspend=false`, `startingDeadlineSeconds` и `timeZone` не заданы (`<none>`).

### История и пауза

`successfulJobsHistoryLimit` и `failedJobsHistoryLimit` ограничивают число хранимых завершённых Job. С `successfulJobsHistoryLimit: 2` после нескольких запусков остались два завершённых и один выполняющийся (`tick-…831`, `…832` — `Complete`, `…833` — `Running`). Пауза:

```text
$ kubectl -n learn-12 patch cronjob tick -p '{"spec":{"suspend":true}}'
jobs before=3 after=2
```

При `suspend: true` новые Job не создаются (число Job за минуту не выросло).

> ⚠️ Проверить: точный смысл `startingDeadlineSeconds`, поведение `Replace` и работа `timeZone` не запускались в этом материале; по умолчанию расписание считается по времени контроллера (в опыте `lastScheduleTime` в UTC: `2026-11-23T10:33:00Z`).

## Лучшие практики

- Для StatefulSet всегда создавайте headless Service; обращайтесь к репликам по именам.
- Решайте судьбу PVC заранее (`Retain` или `Delete`) и делайте резервные копии.
- Для DaemonSet задавайте toleration для нужных taint и ресурсы (агент не должен съедать ноду, [модуль 11](../11-probes-and-resources/overview.ru.md)).
- Job и CronJob делайте идемпотентными: повторы и пропуски неизбежны.
- Ставьте `ttlSecondsAfterFinished`, `activeDeadlineSeconds` и лимиты истории CronJob, чтобы не копить объекты.
- Для CronJob выбирайте `concurrencyPolicy` осознанно; для долгих задач — `Forbid`.

## Частые ошибки

- ❌ Обращаться к реплике StatefulSet по IP → ✅ по имени: IP меняется при пересоздании (в опыте `.31` → `.35`).
- ❌ Ждать, что `scale` вниз удалит данные → ✅ PVC остаются (`Retain`).
- ❌ Судить о завершении обновления по `rollout status` при `partition > 0` → ✅ смотреть `updateRevision` и образы подов.
- ❌ Оставлять Job без `backoffLimit` и без очистки → ✅ ограничить повторы и включить TTL.
- ❌ Расписание CronJob вида «every minute» → ✅ ровно 5 полей cron.

## Сравнение с другими треками

- [Linux](../../../linux/overview.ru.md): CronJob — это cron на уровне кластера (те же 5 полей), DaemonSet — аналог службы systemd на каждом хосте.
- [C#](../../../csharp/recipes/background-jobs/overview.ru.md): `BackgroundService` с `PeriodicTimer` внутри приложения и CronJob снаружи решают разные задачи: в первом случае процесс живёт постоянно, во втором — запускается на время работы.

## Связанные материалы

- Предыдущий уровень: [базовая теория](basics.ru.md)
- Дальше: [экспертная теория](expert.ru.md)
- Хранилище: [08 · Хранилище](../08-storage/advanced.ru.md#политика-освобождения-reclaimpolicy)
- Выкатка и откат: [05 · Deployment и ReplicaSet](../05-deployments/advanced.ru.md#rolling-update-по-шагам)

## Источники

- [StatefulSet: partitioned rolling updates](https://kubernetes.io/docs/concepts/workloads/controllers/statefulset/#partitions)
- [StatefulSet: PersistentVolumeClaim retention](https://kubernetes.io/docs/concepts/workloads/controllers/statefulset/#persistentvolumeclaim-retention)
- [StatefulSet: pod management policies](https://kubernetes.io/docs/concepts/workloads/controllers/statefulset/#pod-management-policies)
- [DaemonSet: how daemon pods are scheduled](https://kubernetes.io/docs/concepts/workloads/controllers/daemonset/#how-daemon-pods-are-scheduled)
- [Jobs: handling pod and container failures](https://kubernetes.io/docs/concepts/workloads/controllers/job/#pod-backoff-failure-policy)
- [Automatic cleanup for finished Jobs](https://kubernetes.io/docs/concepts/workloads/controllers/job/#ttl-mechanism-for-finished-jobs)
- [CronJob: concurrency policy](https://kubernetes.io/docs/concepts/workloads/controllers/cron-jobs/#concurrency-policy)
