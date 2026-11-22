---
title: "Deployment и ReplicaSet"
lang: ru
track: kubernetes
kind: module
doc: overview
slug: 05-deployments
part: 1
levels: [1, 2, 3, 4]
tags: [deployment, replicaset, rolling-update, rollback, scaling, rollout, strategy, revision]
versions: "Kubernetes 1.37.0 (kind v0.34), kubectl 1.36.1"
status: draft
updated: 2026-11-22
---

# Deployment и ReplicaSet

🌐 **Русский** · [English](overview.en.md)

## Коротко

Deployment описывает желаемое состояние приложения: образ, число реплик, стратегию обновления. ReplicaSet — объект, который его реализует: он следит, чтобы запущено было ровно нужное число одинаковых подов. Вы редактируете Deployment, а контроллер плавно создаёт новый ReplicaSet и переносит на него трафик (rolling update), сохраняя историю для отката. Это основной способ запускать stateless-приложения.

## Ключевые идеи

- **Deployment → ReplicaSet → Pod.** Deployment управляет ReplicaSet-ами, ReplicaSet — подами; при смене шаблона пода создаётся новый ReplicaSet.
- **Rolling update по умолчанию:** `maxSurge: 25%`, `maxUnavailable: 25%`; обновление идёт малыми шагами без простоя.
- **История ревизий.** Каждый старый ReplicaSet хранится (по умолчанию 10 ревизий) и позволяет откатиться: `kubectl rollout undo`.
- **Обновление запускает только изменение шаблона пода** (образ, переменные, метки шаблона); изменение `replicas` ничего не пересоздаёт.
- **Деплой может застрять,** например при неверном образе: старые поды продолжают работать, а Deployment ждёт `progressDeadlineSeconds` (600 с по умолчанию) и затем помечается `ProgressDeadlineExceeded`.
- **`selector` неизменяем** и должен совпадать с метками шаблона пода.
- **Стратегия `Recreate`** сначала останавливает все старые поды, потом запускает новые: простой неизбежен.

## Минимальный пример

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: web
spec:
  replicas: 3
  selector:
    matchLabels:
      app: web
  template:
    metadata:
      labels:
        app: web
    spec:
      containers:
        - name: nginx
          image: nginx:1.28
          ports:
            - containerPort: 80
```

```text
$ kubectl -n learn-05 apply -f web.yaml
$ kubectl -n learn-05 set image deploy/web nginx=nginx:1.29
$ kubectl -n learn-05 rollout status deploy/web
deployment "web" successfully rolled out
```

## Когда применять, а когда нет

- ✅ Stateless-приложения: веб-серверы, API, воркеры.
- ✅ Нужны плавные обновления и быстрый откат.
- ❌ Приложения с состоянием и стабильными именами и дисками (БД): для них StatefulSet (модуль 12).
- ❌ Разовые и периодические задачи: Job и CronJob (модуль 12).
- ❌ Агент на каждой ноде: DaemonSet (модуль 12).

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | манифест Deployment, ReplicaSet и `pod-template-hash`, масштабирование, обновление образа, `rollout status` и откат |
| [2. Продвинутая теория](advanced.ru.md) | как идёт rolling update по шагам, `maxSurge` и `maxUnavailable`, `Recreate`, история и `change-cause`, застрявший деплой и `progressDeadlineSeconds`, pause/resume, `rollout restart` |
| [3. Экспертная теория](expert.ru.md) | устройство контроллера и хэш шаблона, справочник полей Deployment, условия, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) с решениями |
| 5. Запускаемые примеры | ещё не создан — `/learn kubernetes 05-deployments level 5` |
| 6. Тесты | ещё не создан — `/learn kubernetes 05-deployments level 6` |

## Связанные материалы

- Пререквизиты: [04 · Pod](../04-pods/overview.ru.md), [03 · kubectl](../03-local-cluster-and-kubectl/overview.ru.md)
- Дальше: [06 · Service и сеть](../06-services/overview.ru.md)
- Цикл согласования: [02 · Архитектура Kubernetes](../02-architecture/overview.ru.md)
- То же в других треках: [Фоновые задачи в C#](../../../csharp/recipes/background-jobs/overview.ru.md) — graceful shutdown при выкатке
