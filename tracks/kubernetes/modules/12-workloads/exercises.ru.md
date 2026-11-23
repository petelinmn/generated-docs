---
title: "Упражнения: другие workload-ы"
lang: ru
track: kubernetes
kind: module
doc: exercises
slug: 12-workloads
part: 2
tags: [statefulset, daemonset, job, cronjob, workload]
status: draft
updated: 2026-11-23
---

# Упражнения: другие workload-ы

🌐 **Русский** · [English](exercises.en.md)

> Решайте самостоятельно, подсказки и решения под спойлерами. Нужен кластер kind и namespace: `kubectl create namespace learn-12`. После работы: `kubectl delete namespace learn-12`. Для упражнения 3 нужен кластер из нескольких нод.

## 1. 🟢 StatefulSet и стабильное имя

**Задача:** создайте headless Service `web` и StatefulSet `web` из 3 реплик с `volumeClaimTemplates` на 100Mi. Запишите файл в том `web-1`, удалите под и убедитесь, что файл остался, а IP сменился.

**Ожидаемый результат:** три пода `web-0..2`, три PVC `data-web-0..2`, файл на месте после пересоздания.

<details><summary>Подсказка</summary>

`kubectl exec web-1 -- sh -c 'echo custom-data > /usr/share/nginx/html/custom.txt'`, затем `kubectl delete pod web-1 --now`, затем `kubectl exec web-1 -- cat …`.

</details>

<details><summary>Решение</summary>

Манифест — из [базовой теории](basics.ru.md#statefulset).

```text
$ kubectl -n learn-12 exec web-1 -- cat /usr/share/nginx/html/custom.txt
custom-data
ip before=10.244.0.31 after=10.244.0.35  uid changed=yes
```

Имя и PVC постоянны, IP и UID — новые.

</details>

## 2. 🟢 Job с индексами

**Задача:** создайте Indexed Job с `completions: 6`, `parallelism: 2`, каждый под печатает `index=$JOB_COMPLETION_INDEX`. Покажите `completedIndexes`.

**Ожидаемый результат:** `succeeded=6 completedIndexes=0-5`, одновременно работают не более двух подов.

<details><summary>Подсказка</summary>

`completionMode: Indexed`, `restartPolicy: Never`, команда `echo index=$JOB_COMPLETION_INDEX`.

</details>

<details><summary>Решение</summary>

```yaml
apiVersion: batch/v1
kind: Job
metadata: {name: parallel}
spec:
  completions: 6
  parallelism: 2
  completionMode: Indexed
  template:
    spec:
      restartPolicy: Never
      containers: [{name: c, image: alpine:3.22, command: ["sh","-c","echo index=$JOB_COMPLETION_INDEX"]}]
```

```text
$ kubectl -n learn-12 get job parallel -o jsonpath='succeeded={.status.succeeded} completedIndexes={.status.completedIndexes}'
succeeded=6 completedIndexes=0-5
```

</details>

## 3. 🟡 DaemonSet, taint и nodeSelector

**Задача:** на кластере из 3 нод (1 control plane + 2 worker) запустите DaemonSet. Сколько подов? Добавьте toleration и сравните. Затем ограничьте набор нод меткой `disk=ssd`.

**Ожидаемый результат:** 2 пода без toleration, 3 с toleration, 1 с `nodeSelector` на одной размеченной ноде.

<details><summary>Подсказка</summary>

Toleration: `{key: node-role.kubernetes.io/control-plane, operator: Exists, effect: NoSchedule}`. Метка: `kubectl label node <node> disk=ssd`. Кластер: `kind create cluster --config` с одним `control-plane` и двумя `worker` ([модуль 03](../03-local-cluster-and-kubectl/advanced.ru.md#многоузловой-kind)).

</details>

<details><summary>Решение</summary>

```text
agent   DESIRED 2   (без toleration)
agent2  DESIRED 3   (с toleration, под и на control-plane)
$ kubectl label node learn-multi-worker disk=ssd
$ kubectl -n learn-12 patch ds agent -p '{"spec":{"template":{"spec":{"nodeSelector":{"disk":"ssd"}}}}}'
desired=1 current=1 ready=1
```

</details>

## 4. 🟡 Политика PVC

**Задача:** для StatefulSet из упражнения 1 включите `whenScaled: Delete` и `whenDeleted: Delete`. Уменьшите до 2 реплик, затем удалите набор. Что с PVC?

**Ожидаемый результат:** после scale исчез `data-web-2`; после удаления набора PVC нет.

<details><summary>Подсказка</summary>

`kubectl patch sts web -p '{"spec":{"persistentVolumeClaimRetentionPolicy":{"whenDeleted":"Delete","whenScaled":"Delete"}}}'`.

</details>

<details><summary>Решение</summary>

```text
$ kubectl -n learn-12 scale sts/web --replicas=2
$ kubectl -n learn-12 get pvc --no-headers | awk '{print $1}'
data-web-0 data-web-1
$ kubectl -n learn-12 delete sts web
$ kubectl -n learn-12 get pvc
No resources found in learn-12 namespace.
```

</details>

## 5. 🔴 Повторы Job и CronJob без пересечений

**Задача:** (а) создайте Job с командой `exit 1` и `backoffLimit: 2`; найдите условия и число подов. (б) Создайте CronJob «каждую минуту» с `concurrencyPolicy: Forbid`, задачей `sleep 100` и найдите в событиях причину пропусков. (в) Попробуйте расписание `every minute`.

**Ожидаемый результат:** (а) 3 пода `Error`, условия `FailureTarget`/`Failed` с причиной `BackoffLimitExceeded`; (б) событие `JobAlreadyActive`; (в) ошибка `expected exactly 5 fields, found 2`.

<details><summary>Подсказка</summary>

(а) `kubectl get job failing -o jsonpath='{range .status.conditions[*]}{.type}={.status} ({.reason}){"\n"}{end}'`. (б) `kubectl describe cronjob`, раздел Events.

</details>

<details><summary>Решение</summary>

```text
failing-gt5sd 0/1 Error
failing-qwf2r 0/1 Error
failing-rtzsv 0/1 Error
FailureTarget=True (BackoffLimitExceeded)
Failed=True (BackoffLimitExceeded)
JobAlreadyActive   Not starting job because prior execution is running and concurrency policy is Forbid
The CronJob "badsched" is invalid: spec.schedule: Invalid value: "every minute": expected exactly 5 fields, found 2: [every minute]
```

Три пода — первая попытка и два повтора; `Forbid` пропускает запуск, пока идёт прежний.

</details>
