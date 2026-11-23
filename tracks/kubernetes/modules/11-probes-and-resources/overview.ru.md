---
title: "Probes и ресурсы"
lang: ru
track: kubernetes
kind: module
doc: overview
slug: 11-probes-and-resources
part: 2
levels: [1, 2, 3, 4]
tags: [probes, liveness-probe, readiness-probe, startup-probe, resources, requests, limits, qos, cgroups, cpu-throttling, oomkilled]
versions: "Kubernetes 1.37.0 (kind v0.34, cgroup v2), kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Probes и ресурсы

🌐 **Русский** · [English](overview.en.md)

## Коротко

Kubernetes не знает, здорово ли приложение внутри контейнера и сколько ему нужно ресурсов, пока вы ему об этом не скажете. **Probes (пробы)** — проверки, которые kubelet выполняет сам: живо ли приложение (liveness), готово ли принимать трафик (readiness), закончился ли старт (startup). **Requests и limits** — заявка на ресурсы и потолок: по заявке планировщик выбирает ноду, а потолок ядро Linux (cgroups) обеспечивает на нужной ноде.

## Ключевые идеи

- **Три пробы — три разные реакции.** Провал liveness — kubelet перезапускает контейнер. Провал readiness — под убирается из endpoints Service, но не перезапускается. Startup защищает медленный старт: пока она не прошла, остальные не работают.
- **Без readiness контейнер «готов» сразу после запуска,** даже если приложение ещё не слушает порт.
- **Механизмы проб:** `httpGet`, `tcpSocket`, `exec`, `grpc`. Значения по умолчанию: `timeoutSeconds: 1`, `periodSeconds: 10`, `failureThreshold: 3`, `successThreshold: 1`.
- **Requests влияют на планирование,** limits — на исполнение. Планировщик складывает requests, а не фактическое потребление.
- **CPU сжимаемый, память — нет.** Превышение CPU-лимита замедляет контейнер (throttling), превышение лимита памяти убивает его (`OOMKilled`).
- **QoS-класс** (`Guaranteed`, `Burstable`, `BestEffort`) вычисляется из requests и limits и определяет, кого вытеснят первым.
- **Лимит меньше запроса — ошибка:** такой под API не принимает.

## Минимальный пример

```yaml
apiVersion: v1
kind: Pod
metadata: {name: web}
spec:
  containers:
    - name: c
      image: nginx:1.29
      readinessProbe:
        httpGet: {path: /, port: 80}
      livenessProbe:
        httpGet: {path: /, port: 80}
        periodSeconds: 10
      resources:
        requests: {cpu: 100m, memory: 64Mi}
        limits: {memory: 128Mi}
```

```text
$ kubectl -n learn-11 get pod web
NAME   READY   STATUS    RESTARTS   AGE
web    1/1     Running   0          20s
```

## Когда применять, а когда нет

- ✅ Readiness — для любого приложения за Service: пока оно не готово, трафик не идёт.
- ✅ Liveness — для приложений, которые могут зависнуть, но не упасть (deadlock).
- ✅ Startup — для медленно стартующих приложений вместо длинного `initialDelaySeconds`.
- ✅ Requests — всегда; limits по памяти — почти всегда.
- ❌ Liveness-проба, зависящая от внешних сервисов (БД, очередь): падение зависимости перезапустит все поды разом.
- ❌ CPU-лимиты «на всякий случай» для чувствительных к задержкам сервисов: throttling даёт скачки задержек даже на свободной ноде.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | liveness, readiness, механизмы и параметры, requests и limits, QoS-классы, `Insufficient cpu` |
| [2. Продвинутая теория](advanced.ru.md) | startup-проба и медленный старт, как проектировать пробы, cgroups изнутри, CPU throttling, OOM, ёмкость ноды |
| [3. Экспертная теория](expert.ru.md) | работа kubelet с пробами, QoS и `oom_score_adj`, справочник полей, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) с решениями |
| 5. Запускаемые примеры | ещё не создан — `/learn kubernetes 11-probes-and-resources level 5` |
| 6. Тесты | ещё не создан — `/learn kubernetes 11-probes-and-resources level 6` |

## Связанные материалы

- Пререквизиты: [04 · Pod](../04-pods/overview.ru.md), [06 · Service и сеть](../06-services/overview.ru.md) (endpoints и готовность)
- Дальше: [12 · Другие workload-ы](../12-workloads/overview.ru.md)
- Квоты и значения по умолчанию: [10 · Namespace и RBAC](../10-namespaces-and-rbac/overview.ru.md)
- Выкатка и готовность: [05 · Deployment и ReplicaSet](../05-deployments/overview.ru.md)
- То же в других треках: [Linux](../../../linux/overview.ru.md) — cgroups, из которых строятся лимиты
- Автомасштабирование по requests: [15 · Автомасштабирование](../15-autoscaling/overview.ru.md)
- Логи, события и метрики: [16 · Наблюдаемость](../16-observability/overview.ru.md)
- Размещение подов на нодах: [18 · Планирование подов](../18-scheduling/overview.ru.md)
- Диагностика: [21 · Диагностика](../21-troubleshooting/overview.ru.md)
