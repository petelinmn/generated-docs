---
title: "Другие workload-ы: базовая теория"
lang: ru
track: kubernetes
kind: module
doc: basics
slug: 12-workloads
part: 2
tags: [statefulset, daemonset, job, cronjob, headless-service, volumeclaimtemplates, workload]
versions: "Kubernetes 1.37.0 (kind v0.34), kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Другие workload-ы: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [05 · Deployment и ReplicaSet](../05-deployments/overview.ru.md), [08 · Хранилище](../08-storage/overview.ru.md) · **Время:** ~3 ч

Все примеры выполнены в kind (Kubernetes 1.37.0), namespace `learn-12`. DaemonSet показан на кластере из 3 нод (1 control plane + 2 worker).

## Зачем это нужно

У Deployment все реплики одинаковы и заменяемы. Но база данных должна знать, кто из реплик первичный, а у агента логов должна быть ровно одна копия на ноду. Нужны контроллеры с другими гарантиями.

## StatefulSet

StatefulSet запускает поды с **стабильной идентичностью**. Ему нужен headless Service ([модуль 06](../06-services/advanced.ru.md#headless-service)) и шаблон PVC:

```yaml
apiVersion: v1
kind: Service
metadata: {name: web}
spec:
  clusterIP: None
  selector: {app: web}
  ports: [{port: 80, name: http}]
---
apiVersion: apps/v1
kind: StatefulSet
metadata: {name: web}
spec:
  serviceName: web
  replicas: 3
  selector: {matchLabels: {app: web}}
  template:
    metadata: {labels: {app: web}}
    spec:
      containers:
        - name: nginx
          image: nginx:1.29
          volumeMounts: [{name: data, mountPath: /usr/share/nginx/html}]
  volumeClaimTemplates:
    - metadata: {name: data}
      spec:
        accessModes: [ReadWriteOnce]
        resources: {requests: {storage: 100Mi}}
```

### Что даёт StatefulSet

**Порядок и имена.** Поды создаются по одному и по порядку, у них предсказуемые имена `<имя набора>-<номер>`:

```text
web-0 0/1 Pending → ContainerCreating → 1/1 Running
web-1 0/1 Pending → ContainerCreating → 1/1 Running       # только после готовности web-0
web-2 0/1 Pending → ContainerCreating → 1/1 Running
```

**Свой PVC на каждую реплику.** По шаблону `volumeClaimTemplates` создаются `data-web-0`, `data-web-1`, `data-web-2`:

```text
$ kubectl -n learn-12 get pods,pvc --no-headers | awk '{print $1,$2,$3}'
web-0 1/1 Running
web-1 1/1 Running
web-2 1/1 Running
data-web-0 Bound pvc-54252831-…
data-web-1 Bound pvc-8c9baee0-…
data-web-2 Bound pvc-373d6e26-…
```

**Стабильные имена и DNS.** Hostname пода равен его имени, а у каждого пода своя DNS-запись через headless Service (`<под>.<сервис>.<namespace>.svc`):

```text
$ for i in 0 1 2; do kubectl -n learn-12 exec web-$i -- hostname; done
web-0
web-1
web-2
$ kubectl -n learn-12 exec dns -- nslookup web-1.web.learn-12.svc.cluster.local
Name:	web-1.web.learn-12.svc.cluster.local
Address: 10.244.0.31
$ kubectl -n learn-12 exec dns -- wget -qO- http://web-2.web
hello from web-2
```

Клиент может обратиться к конкретной реплике (например, к первичной базе `web-0`), а не к случайной.

## DaemonSet

DaemonSet запускает ровно один под на каждой подходящей ноде. Типичные задачи: сбор логов, метрики ноды, сетевые плагины.

```yaml
apiVersion: apps/v1
kind: DaemonSet
metadata: {name: agent}
spec:
  selector: {matchLabels: {app: agent}}
  template:
    metadata: {labels: {app: agent}}
    spec:
      containers:
        - name: c
          image: alpine:3.22
          command: ["sh","-c","echo agent on $NODE; sleep 3600"]
          env: [{name: NODE, valueFrom: {fieldRef: {fieldPath: spec.nodeName}}}]
```

На кластере из 3 нод (control plane с taint и два worker):

```text
$ kubectl -n learn-12 get ds agent
NAME    DESIRED   CURRENT   READY   UP-TO-DATE   AVAILABLE   NODE SELECTOR   AGE
agent   2         2         2       2            2           <none>          25s
$ kubectl -n learn-12 get pods -o wide
agent-29n99 Running learn-multi-worker2
agent-56hbr Running learn-multi-worker
```

Подов два, а не три: на ноде control plane стоит taint, и под без toleration туда не попадает ([модуль 03](../03-local-cluster-and-kubectl/advanced.ru.md#многоузловой-kind)). Поведение с toleration — в [продвинутой теории](advanced.ru.md#daemonset-ноды-toleration-и-селектор). Реплик в DaemonSet нет: число подов определяется числом нод.

## Job

Job запускает под(ы) до **успешного завершения**: в отличие от Deployment, завершившийся под не перезапускается без нужды.

```yaml
apiVersion: batch/v1
kind: Job
metadata: {name: simple}
spec:
  template:
    spec:
      restartPolicy: Never
      containers: [{name: c, image: alpine:3.22, command: ["sh","-c","echo job-done; exit 0"]}]
```

```text
$ kubectl -n learn-12 wait --for=condition=complete job/simple --timeout=90s
job.batch/simple condition met
$ kubectl -n learn-12 logs job/simple
job-done
```

`restartPolicy` у Job только `Never` или `OnFailure`; `Always` отклоняется:

```text
The Job "always" is invalid: spec.template.spec.restartPolicy: Required value: valid values: "OnFailure", "Never"
```

### Несколько запусков: completions и parallelism

```yaml
spec:
  completions: 6        # сколько успешных подов нужно
  parallelism: 2        # сколько одновременно
  completionMode: Indexed
```

В режиме `Indexed` каждому поду выдаётся номер в переменной `JOB_COMPLETION_INDEX`:

```text
$ kubectl -n learn-12 logs -l job-name=parallel,batch.kubernetes.io/job-completion-index=3 --tail=1
index=3
$ kubectl -n learn-12 get job parallel -o jsonpath='succeeded={.status.succeeded} completedIndexes={.status.completedIndexes}'
succeeded=6 completedIndexes=0-5
```

Шесть подов выполнены по два одновременно: каждые несколько секунд `running=2`, затем следующая пара. Имена подов содержат индекс (`parallel-0-tx4fs`).

## CronJob

CronJob создаёт Job по расписанию в формате cron из 5 полей (`минута час день месяц день_недели`):

```yaml
apiVersion: batch/v1
kind: CronJob
metadata: {name: tick}
spec:
  schedule: "* * * * *"          # каждую минуту
  jobTemplate:
    spec:
      template:
        spec:
          restartPolicy: OnFailure
          containers: [{name: c, image: alpine:3.22, command: ["sh","-c","date +%T; echo tick"]}]
```

```text
$ kubectl -n learn-12 get cronjob tick
NAME   SCHEDULE    TIMEZONE   SUSPEND   ACTIVE   LAST SCHEDULE   AGE
tick   * * * * *   <none>     False     1        2s              2m34s
$ kubectl -n learn-12 get jobs
tick-29923831 Complete
tick-29923832 Complete
tick-29923833 Running
```

Имя Job — имя CronJob и число (время запуска в минутах от начала эпохи). Запустить задачу вручную, не дожидаясь расписания:

```text
$ kubectl -n learn-12 create job manual-run --from=cronjob/tick
$ kubectl -n learn-12 get job manual-run -o jsonpath='owner={.metadata.ownerReferences[0].kind}/{.metadata.ownerReferences[0].name}'
owner=CronJob/tick
```

Неверное расписание отвергается сразу:

```text
The CronJob "badsched" is invalid: spec.schedule: Invalid value: "every minute": expected exactly 5 fields, found 2: [every minute]
```

## Ошибки новичка

- ❌ Запускать базу данных Deployment-ом → ✅ StatefulSet: стабильные имена и отдельный PVC на реплику.
- ❌ Считать, что DaemonSet запустится на всех нодах → ✅ для нод с taint нужен toleration (в опыте 2 пода вместо 3).
- ❌ Ставить `restartPolicy: Always` в Job → ✅ только `Never` или `OnFailure`.
- ❌ Ждать от CronJob точного момента и ровно одного запуска → ✅ делать задачи идемпотентными.
- ❌ Удалять StatefulSet и ждать, что данные удалятся → ✅ PVC остаются (по умолчанию).

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — идентичность, `partition`, политика PVC, повторы Job, параллелизм CronJob
- Предыдущий модуль: [11 · Probes и ресурсы](../11-probes-and-resources/overview.ru.md)
- Хранилище для StatefulSet: [08 · Хранилище](../08-storage/overview.ru.md)
- Остановка подов: [04 · Pod](../04-pods/advanced.ru.md#остановка-пода)

## Источники

- [StatefulSets](https://kubernetes.io/docs/concepts/workloads/controllers/statefulset/)
- [DaemonSet](https://kubernetes.io/docs/concepts/workloads/controllers/daemonset/)
- [Jobs](https://kubernetes.io/docs/concepts/workloads/controllers/job/)
- [CronJob](https://kubernetes.io/docs/concepts/workloads/controllers/cron-jobs/)
- [Indexed Job for Parallel Processing](https://kubernetes.io/docs/tasks/job/indexed-parallel-processing-static/)
