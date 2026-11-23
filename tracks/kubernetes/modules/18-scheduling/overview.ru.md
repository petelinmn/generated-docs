---
title: "Планирование подов"
lang: ru
track: kubernetes
kind: module
doc: overview
slug: 18-scheduling
part: 3
levels: [1, 2, 3, 4]
tags: [scheduler, nodeselector, affinity, taints, tolerations, topology-spread, priorityclass, preemption, drain, pdb]
versions: "Kubernetes 1.37.0 (kind v0.34, 3 ноды), kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Планирование подов

🌐 **Русский** · [English](overview.en.md)

## Коротко

**Планировщик (scheduler)** выбирает для каждого нового пода ноду: сначала отбрасывает неподходящие (фильтры), затем оценивает оставшиеся (очки) и записывает выбор в `spec.nodeName`. Повлиять на выбор можно из манифеста: **nodeSelector и nodeAffinity** (куда можно), **taints и tolerations** (кого нода не пускает), **podAffinity/podAntiAffinity и topologySpreadConstraints** (рядом с кем или вразброс), **priority** (кто важнее при нехватке места). Если подходящей ноды нет, под остаётся в `Pending`, а причина — в событии `FailedScheduling`.

## Ключевые идеи

- **Планировщик назначает только новые поды.** Уже запущенный под он не переносит; перенос — это удаление и повторное создание контроллером.
- **`nodeSelector` — простейшая форма:** под идёт только на ноды с нужными метками. Нет подходящей — `Pending`.
- **Affinity гибче:** `required…` — жёсткое правило, `preferred…` — пожелание с весом.
- **Taint отталкивает, toleration разрешает.** Taint не притягивает: поду с toleration всё равно нужно выбрать эту ноду другими правилами.
- **Эффект `NoExecute` выселяет уже работающие поды**, а `tolerationSeconds` ограничивает время «терпения».
- **Anti-affinity и topology spread разносят реплики** по нодам и зонам; жёсткое правило при нехватке нод оставляет лишние реплики в `Pending`.
- **Приоритет позволяет вытеснять (preemption):** под с высоким приоритетом, которому не хватило места, удаляет поды с низким. `preemptionPolicy: Never` отключает вытеснение.
- **`cordon` и `drain` готовят ноду к обслуживанию;** PodDisruptionBudget может заблокировать `drain`.

## Минимальный пример

```yaml
apiVersion: v1
kind: Pod
metadata: {name: sel-bad}
spec:
  nodeSelector: {disk: nvme}
  containers: [{name: c, image: nginx:1.29}]
```

```text
$ kubectl -n learn-18 get pod sel-bad
NAME      READY   STATUS    RESTARTS   AGE
sel-bad   0/1     Pending   0          15s
$ kubectl -n learn-18 describe pod sel-bad
  Warning  FailedScheduling  15s   default-scheduler  0/3 nodes are available: 1 node(s) had untolerated taint(s), 2 node(s) didn't match Pod's node affinity/selector. preemption: 0/3 nodes are available: 3 Preemption is not helpful for scheduling.
```

Ни у одной ноды нет метки `disk=nvme`, а на control-plane стоит taint.

## Когда применять, а когда нет

| Задача | Инструмент |
|---|---|
| Нужны ноды определённого типа (SSD, GPU) | nodeSelector или nodeAffinity |
| Нода зарезервирована под особые поды | taint на ноде + toleration в подах (+ nodeSelector) |
| Реплики не должны оказаться на одной ноде | podAntiAffinity или topologySpreadConstraints |
| Важные поды должны получить место первыми | PriorityClass |
| Обслуживание ноды | `cordon`, `drain`, PDB |

- ❌ Не задавайте жёсткие правила без необходимости: каждое сужает выбор, и под легко застревает в `Pending`.
- ❌ Не используйте `nodeName` вместо правил планирования: он обходит и планировщик, и taints.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | как планировщик выбирает ноду, nodeSelector, nodeAffinity, taints и tolerations, чтение `FailedScheduling` |
| [2. Продвинутая теория](advanced.ru.md) | podAffinity и antiAffinity, topology spread, `NoExecute` и `tolerationSeconds`, приоритеты и вытеснение, cordon/drain и PDB |
| [3. Экспертная теория](expert.ru.md) | справочник полей и операторов, встроенные taints, устройство планировщика, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | ещё не создан — `/learn kubernetes 18-scheduling level 5` |
| 6. Тесты | ещё не создан — `/learn kubernetes 18-scheduling level 6` |

## Связанные материалы

- Пререквизиты: [04 · Pod](../04-pods/overview.ru.md), [05 · Deployment](../05-deployments/overview.ru.md), [11 · Probes и ресурсы](../11-probes-and-resources/overview.ru.md) (requests влияют на размещение)
- Предыдущие модули: [16 · Наблюдаемость](../16-observability/overview.ru.md), [17 · Безопасность](../17-security/overview.ru.md)
- DaemonSet и tolerations: [12 · Другие workloads](../12-workloads/overview.ru.md)
- Автомасштабирование и нехватка нод: [15 · Автомасштабирование](../15-autoscaling/overview.ru.md)
- Дальше: [19 · CRD и операторы](../19-crd-and-operators/overview.ru.md)
- Диагностика: [21 · Диагностика](../21-troubleshooting/overview.ru.md)
