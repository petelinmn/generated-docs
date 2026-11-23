---
title: "Другие workload-ы: StatefulSet, DaemonSet, Job, CronJob"
lang: ru
track: kubernetes
kind: module
doc: overview
slug: 12-workloads
part: 2
levels: [1, 2, 3, 4]
tags: [statefulset, daemonset, job, cronjob, workload, headless-service, volumeclaimtemplates, batch, backofflimit, concurrency-policy]
versions: "Kubernetes 1.37.0 (kind v0.34), kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Другие workload-ы: StatefulSet, DaemonSet, Job, CronJob

🌐 **Русский** · [English](overview.en.md)

## Коротко

[Deployment](../05-deployments/overview.ru.md) подходит для stateless-приложений, где все реплики взаимозаменяемы. Для остальных задач есть другие контроллеры: **StatefulSet** — приложения с состоянием и стабильными именами (базы данных), **DaemonSet** — по одному поду на каждую ноду (агенты), **Job** — задача, которая выполняется до успешного завершения, **CronJob** — Job по расписанию. Все они создают поды по тому же шаблону, но по-разному решают, сколько подов, где и когда запускать.

## Ключевые идеи

- **StatefulSet даёт подам стабильную идентичность:** имена `web-0`, `web-1`, … стабильные DNS-имена и собственный PVC на каждую реплику, который переживает пересоздание пода.
- **Поды StatefulSet создаются по порядку** (`web-0`, затем `web-1`) и удаляются в обратном; режим `Parallel` отключает ожидание.
- **PVC StatefulSet по умолчанию не удаляются** при масштабировании вниз и удалении набора; это управляется `persistentVolumeClaimRetentionPolicy`.
- **DaemonSet запускает под на каждой подходящей ноде,** включая новые; на ноды с taint (например, control plane) нужен toleration.
- **Job выполняет задачу до успеха:** `completions`, `parallelism`, `backoffLimit`; поды перезапускаются только с `restartPolicy: Never` или `OnFailure`.
- **CronJob порождает Job по расписанию (cron, 5 полей)** с политикой параллелизма, паузой и историей.
- **Выбор контроллера — это выбор гарантий:** взаимозаменяемость, идентичность, «по одному на ноду», «один раз до конца».

## Минимальный пример

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

## Когда применять, а когда нет

| Задача | Контроллер |
|---|---|
| Stateless-сервис, реплики взаимозаменяемы | Deployment |
| База данных, брокер, кластер с идентичностью реплик | StatefulSet |
| Агент на каждой ноде (логи, метрики, сеть) | DaemonSet |
| Разовая пакетная задача, миграция | Job |
| Периодическая задача (бэкап, отчёт) | CronJob |

- ❌ Не используйте Deployment с одним PVC для базы с репликами (модуль 08): нужен StatefulSet.
- ❌ Не запускайте «разовые» задачи как Deployment или голые поды: при завершении их будут перезапускать.
- ❌ CronJob не гарантирует точность до секунды и ровно одного запуска: задачи должны быть идемпотентными.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | StatefulSet и headless Service, DaemonSet, Job, CronJob: манифесты и базовое поведение |
| [2. Продвинутая теория](advanced.ru.md) | сохранность данных и порядок в StatefulSet, обновление с `partition`, политика PVC, toleration у DaemonSet, повторы Job и индексы, CronJob: параллелизм, пауза, история |
| [3. Экспертная теория](expert.ru.md) | справочник полей четырёх контроллеров, условия Job, подвохи, компромиссы, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) с решениями |
| 5. Запускаемые примеры | ещё не создан — `/learn kubernetes 12-workloads level 5` |
| 6. Тесты | ещё не создан — `/learn kubernetes 12-workloads level 6` |

## Связанные материалы

- Пререквизиты: [05 · Deployment и ReplicaSet](../05-deployments/overview.ru.md), [08 · Хранилище](../08-storage/overview.ru.md) (PVC), [06 · Service и сеть](../06-services/overview.ru.md) (headless Service)
- Дальше: [13 · Helm](../13-helm/overview.ru.md)
- Остановка и перезапуск подов: [04 · Pod](../04-pods/overview.ru.md)
- В других треках: [фоновые задачи в C#](../../../csharp/recipes/background-jobs/overview.ru.md) — то же «периодическое и разовое» вне Kubernetes
