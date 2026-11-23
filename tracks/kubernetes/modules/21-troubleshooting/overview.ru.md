---
title: "Диагностика: Pending, CrashLoopBackOff, сеть, ноды"
lang: ru
track: kubernetes
kind: module
doc: overview
slug: 21-troubleshooting
part: 3
levels: [1, 2, 3, 4]
tags: [troubleshooting, debugging, crashloopbackoff, imagepullbackoff, oomkilled, pending, endpoints, dns, kubectl-debug, events, finalizer]
versions: "Kubernetes 1.37.0 (kind v0.34, 3 ноды), kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Диагностика: Pending, CrashLoopBackOff, сеть, ноды

🌐 **Русский** · [English](overview.en.md)

## Коротко

Когда что-то не работает, помогает один порядок действий: **статус объекта → события → логи → вход внутрь**. Статус (`kubectl get`) подсказывает, на каком этапе остановился под (`Pending`, `ImagePullBackOff`, `CrashLoopBackOff`, `CreateContainerConfigError`, `OOMKilled`). События (`kubectl describe`) называют причину. Логи показывают, что делало приложение. Если нужно заглянуть внутрь, есть `kubectl exec`, временный контейнер `kubectl debug` и отладочный под на ноде. Отдельно проверяют сеть (Service без endpoints, DNS) и состояние ноды.

## Ключевые идеи

- **Читайте статус, затем `describe`:** причина почти всегда в разделе `Events` или в `state`/`lastState` контейнера.
- **`Pending` — планировщик или том, а не приложение:** нет ноды (ресурсы, taints, селекторы) или не привязан PVC.
- **`ImagePullBackOff` — образ, тег или реестр;** `CreateContainerConfigError` — отсутствует ConfigMap или Secret; `RunContainerError` — не запускается команда.
- **`CrashLoopBackOff` — приложение падает;** код выхода и логи говорят почему; `137` и `OOMKilled` — нехватка памяти.
- **Service без endpoints — это неверный селектор, неготовые поды или не тот порт.**
- **`kubectl debug` добавляет временный контейнер к работающему поду** (даже когда в образе нет оболочки) и открывает файловую систему ноды.
- **Объект в `Terminating` держит финализатор;** `Deployment` с `ReplicaFailure` — квота или admission.
- **Нода `NotReady` получает taints,** поды на ней не уходят сразу, а ждут `tolerationSeconds`.

## Минимальный пример

```text
$ kubectl -n learn-21 get pods
NAME     READY   STATUS                       RESTARTS      AGE
badimg   0/1     ImagePullBackOff             0             45s
nocfg    0/1     CreateContainerConfigError   0             45s
oom      0/1     OOMKilled                    2 (24s ago)   25s
$ kubectl -n learn-21 describe pod nocfg | tail -2
  Warning  Failed  12s (x4 over 37s)  kubelet  spec.containers{c}: Error: configmap "missing-cm" not found
```

## Когда применять, а когда нет

| Симптом | Куда смотреть |
|---|---|
| Под не запускается | `get pod`, `describe pod` (события) |
| Под падает | `logs`, `logs --previous`, `lastState` |
| Нет доступа к сервису | `get endpoints`, DNS, `port-forward` |
| Нужно «заглянуть» в контейнер без shell | `kubectl debug` |
| Нода не готова | `get nodes`, `describe node`, taints |
| Не удаляется объект | `deletionTimestamp`, `finalizers` |

- ❌ Не перезапускайте под «на удачу»: вы потеряете состояние, по которому ищут причину.
- ❌ Не удаляйте финализаторы без понимания: внешние ресурсы останутся без очистки.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | метод диагностики, типовые статусы подов и их причины |
| [2. Продвинутая теория](advanced.ru.md) | Service и endpoints, DNS, `kubectl debug`, квота, `Terminating`, нода `NotReady` |
| [3. Экспертная теория](expert.ru.md) | справочник статусов и команд, алгоритм, устройство, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | ещё не создан — `/learn kubernetes 21-troubleshooting level 5` |
| 6. Тесты | ещё не создан — `/learn kubernetes 21-troubleshooting level 6` |

## Связанные материалы

- Пререквизиты: [04 · Pod](../04-pods/overview.ru.md), [06 · Service и сеть](../06-services/overview.ru.md), [11 · Probes и ресурсы](../11-probes-and-resources/overview.ru.md)
- Логи, события, метрики: [16 · Наблюдаемость](../16-observability/overview.ru.md)
- Размещение подов и `Pending`: [18 · Планирование подов](../18-scheduling/overview.ru.md)
- Хранилище и PVC: [08 · Хранилище](../08-storage/overview.ru.md)
- Квоты и RBAC: [10 · Namespaces и RBAC](../10-namespaces-and-rbac/overview.ru.md)
- Предыдущий модуль: [20 · GitOps](../20-gitops/overview.ru.md)
