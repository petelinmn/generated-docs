---
title: "Pod"
lang: ru
track: kubernetes
kind: module
doc: overview
slug: 04-pods
part: 1
levels: [1, 2, 3, 4]
tags: [pod, container, init-container, sidecar, lifecycle, restart-policy, termination, crashloopbackoff]
versions: "Kubernetes 1.37.0 (kind v0.34), kubectl 1.36.1"
status: draft
updated: 2026-11-22
---

# Pod

🌐 **Русский** · [English](overview.en.md)

## Коротко

Pod — наименьшая единица, которую запускает Kubernetes: один или несколько контейнеров с общим IP-адресом, общим `localhost` и общими томами. Под эфемерен: его не «чинят», а заменяют. Напрямую поды создают редко, обычно их порождает контроллер, например [Deployment](../05-deployments/overview.ru.md). Но понимать жизненный цикл пода обязательно: от него зависят все диагностические статусы вроде `Pending`, `CrashLoopBackOff` и `Init:Error`.

## Ключевые идеи

- **Контейнеры одного пода живут вместе:** запускаются на одной ноде, делят сетевое пространство (один IP, общий `localhost`) и могут делить тома.
- **Фаза пода** (`Pending`, `Running`, `Succeeded`, `Failed`, `Unknown`) — грубое состояние; столбец `STATUS` в `kubectl get` показывает более подробные причины (`ContainerCreating`, `Completed`, `Error`, `CrashLoopBackOff`).
- **`restartPolicy`** (`Always` по умолчанию, `OnFailure`, `Never`) решает, перезапускает ли kubelet завершившийся контейнер.
- **Init-контейнеры** выполняются по очереди до основных контейнеров; пока они не завершились успешно, под не `Running`.
- **Sidecar-контейнер** работает рядом с основным (логирование, прокси); в современных версиях это init-контейнер с `restartPolicy: Always`.
- **Остановка пода:** `SIGTERM`, ожидание `terminationGracePeriodSeconds` (30 с по умолчанию), затем `SIGKILL`.
- **Почти все поля пода неизменяемы:** образ меняется на месте, остальное требует пересоздания.

## Минимальный пример

```yaml
apiVersion: v1
kind: Pod
metadata:
  name: web
  labels:
    app: web
spec:
  containers:
    - name: nginx
      image: nginx:1.29
      ports:
        - containerPort: 80
```

```text
$ kubectl -n learn-04 apply -f web.yaml
$ kubectl -n learn-04 get pod web
NAME   READY   STATUS    RESTARTS   AGE
web    1/1     Running   0          20s
```

## Когда применять, а когда нет

- ✅ Разовые задачи и отладка: `kubectl run`, поды с `restartPolicy: Never`.
- ✅ Несколько тесно связанных контейнеров, которые должны делить сеть и тома (приложение + sidecar).
- ❌ Приложение «просто в поде» без контроллера: при падении ноды или удалении оно не вернётся. Используйте Deployment.
- ❌ Несвязанные части приложения в одном поде: их нельзя масштабировать независимо. Делайте отдельные поды.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | манифест пода, фазы и условия, `restartPolicy`, `Completed`/`Error`/`CrashLoopBackOff`, логи, exec, события |
| [2. Продвинутая теория](advanced.ru.md) | несколько контейнеров, общий `localhost` и тома, переменные из `fieldRef`, init-контейнеры, нативный sidecar, остановка пода и `preStop` |
| [3. Экспертная теория](expert.ru.md) | устройство пода (pause-контейнер и namespaces), справочник полей `spec`, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) с решениями |
| 5. Запускаемые примеры | ещё не создан — `/learn kubernetes 04-pods level 5` |
| 6. Тесты | ещё не создан — `/learn kubernetes 04-pods level 6` |

## Связанные материалы

- Пререквизиты: [03 · Локальный кластер и kubectl](../03-local-cluster-and-kubectl/overview.ru.md), [02 · Архитектура Kubernetes](../02-architecture/overview.ru.md)
- Дальше: [05 · Deployment и ReplicaSet](../05-deployments/overview.ru.md)
- Фундамент: [трек Linux](../../../linux/overview.ru.md) — процессы и сигналы
- Контейнеры: [01 · Контейнеры и Docker](../01-containers/overview.ru.md) — `PID 1` и `SIGTERM`
- Завершение и перезапуск подов в Job: [12 · Другие workload-ы](../12-workloads/overview.ru.md)
- Логи, события и метрики: [16 · Наблюдаемость](../16-observability/overview.ru.md)
- Безопасность подов и сети: [17 · Безопасность](../17-security/overview.ru.md)
- Размещение подов на нодах: [18 · Планирование подов](../18-scheduling/overview.ru.md)
