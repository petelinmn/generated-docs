---
title: "Планирование подов: экспертная теория"
lang: ru
track: kubernetes
kind: module
doc: expert
slug: 18-scheduling
part: 3
tags: [scheduler, scheduling-framework, taints, tolerations, affinity, topology-spread, preemption, pdb]
versions: "Kubernetes 1.37.0 (kind v0.34, 3 ноды), kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Планирование подов: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

Опирается на [базовую](basics.ru.md) и [продвинутую](advanced.ru.md) теорию. Что подтверждено опытом в kind (Kubernetes 1.37.0) — помечено «в опыте».

## Справочник

### Поля пода, влияющие на размещение

| Поле | Смысл |
|---|---|
| `spec.nodeName` | прямое назначение ноды, планировщик не участвует (в опыте обошёл taint control-plane) |
| `spec.nodeSelector` | карта меток; нужны все |
| `spec.affinity.nodeAffinity` | `required…` (`nodeSelectorTerms`) и `preferred…` (`weight` + `preference`) |
| `spec.affinity.podAffinity` / `podAntiAffinity` | `required…` (`labelSelector`, `topologyKey`, опционально `namespaces`/`namespaceSelector`) и `preferred…` (`weight` + `podAffinityTerm`) |
| `spec.tolerations[]` | `key`, `operator` (`Equal`/`Exists`), `value`, `effect`, `tolerationSeconds` |
| `spec.topologySpreadConstraints[]` | `maxSkew`, `topologyKey`, `whenUnsatisfiable`, `labelSelector` (и необязательные `minDomains`, `matchLabelKeys`, `nodeAffinityPolicy`, `nodeTaintsPolicy`) |
| `spec.priorityClassName` → `spec.priority` | приоритет подставляется из PriorityClass (у подов без класса — 0) |
| `spec.preemptionPolicy` | `PreemptLowerPriority` (по умолчанию) или `Never` |
| `spec.schedulerName` | какой планировщик занимается подом (по умолчанию `default-scheduler`) |

> ⚠️ Проверить: в опытах использованы `nodeSelector`, `nodeAffinity` с оператором `In`, `podAntiAffinity` (required, `kubernetes.io/hostname`), `topologySpreadConstraints` (`DoNotSchedule`), tolerations `Equal`/`Exists`, PriorityClass и `preemptionPolicy: Never`. Остальные поля и операторы — по документации.

### Эффекты taint

| Эффект | Действие |
|---|---|
| `NoSchedule` | новые поды без toleration не планируются; работающие остаются |
| `PreferNoSchedule` | планировщик старается избегать, но не запрещает |
| `NoExecute` | новые не планируются, **работающие без toleration выселяются** (в опыте) |

Toleration подходит к taint, если совпали `key` и `effect` (пустой `effect` — любой), а значение: при `Equal` равно `value`, при `Exists` не проверяется. Пустой `key` с `Exists` терпит всё.

### Taints, которые ставит кластер

| Taint | Когда |
|---|---|
| `node-role.kubernetes.io/control-plane:NoSchedule` | на control-plane в kind (в опыте) |
| `node.kubernetes.io/unschedulable:NoSchedule` | `kubectl cordon` (в опыте, с `timeAdded`) |
| `node.kubernetes.io/not-ready`, `node.kubernetes.io/unreachable` (`NoExecute`) | нода не готова или недоступна; у подов автоматически есть toleration на 300 с (в опыте) |
| `memory-pressure`, `disk-pressure`, `pid-pressure`, `network-unavailable` | давление на ресурсы ноды |

> ⚠️ Проверить: последняя строка таблицы взята из документации и не воспроизводилась.

### Команды

| Команда | Назначение |
|---|---|
| `kubectl label node N k=v` / `k-` | поставить и снять метку |
| `kubectl taint node N k=v:Effect` / `k=v:Effect-` | поставить и снять taint |
| `kubectl cordon / uncordon / drain N` | закрыть ноду, вернуть, освободить |
| `kubectl get pdb` | допустимые нарушения (`ALLOWED DISRUPTIONS`) |
| `kubectl get priorityclass` | значения и политики приоритетов |
| `kubectl get events --field-selector reason=FailedScheduling` | причины `Pending` |

## Внутреннее устройство

- **Планировщик — обычный контроллер.** В kind это статический под `kube-scheduler` в `kube-system` (образ `registry.k8s.io/kube-scheduler:v1.37.0` в опыте). Он смотрит на поды без `spec.nodeName`, выбирает ноду и создаёт binding.
- **Цикл планирования** состоит из этапов (расширяемая «scheduling framework»): фильтры отбрасывают ноды, оценки (score) ранжируют остальные, затем привязка. Сообщения вроде `Insufficient cpu`, `didn't match Pod's node affinity/selector` и `had untolerated taint(s)` — результаты разных фильтров.
- **Очередь.** Неразмещённые поды уходят в очередь «неразмещаемых» и возвращаются в рассмотрение, когда что-то меняется (новая нода, освободилось место, изменились метки).
- **Решение по `requests`, а не по реальному потреблению.** В опыте два пода `low` с `requests: cpu: 10` почти не использовали CPU, но заняли ноды целиком: у `vip` — `Insufficient cpu`.
- **Вытеснение (preemption).** Если под нигде не помещается, планировщик ищет ноду, где можно освободить место, удалив поды с меньшим приоритетом. Сообщение `preemption: found a potential placement … preempting 1 victims` (в опыте) означает, что решение принято, а событие `Preempted` фиксирует жертву. Вытесненный под, если у него есть контроллер, будет пересоздан и встанет в очередь.
- **Taint-контроллер.** За выселением по `NoExecute` и `tolerationSeconds` следит контроллер ноды (node lifecycle). Поды без контроллера он просто удаляет (в опыте).
- **Eviction API.** `kubectl drain` выселяет через eviction, а не через `delete`: так учитывается PDB.

> ⚠️ Проверить: детали очередей (backoff/unschedulable), список этапов framework и параметр `percentageOfNodesToScore` взяты из документации и не проверялись. Профили планировщика (`KubeSchedulerConfiguration`, несколько планировщиков) не запускались.

## Подвохи

### 1. Toleration не притягивает

Под с toleration на `dedicated=gpu` может оказаться на обычной ноде. Закрепляйте комбинацией taint + `nodeSelector`/affinity.

### 2. `tolerationSeconds` и замены

В опыте поды выселялись через ~20 с, а замена тут же возвращалась на ту же ноду и выселялась снова. Taint не «выгоняет навсегда», пока он есть, а у шаблона есть toleration.

### 3. Планировщик не смотрит на реальное потребление

Нода может простаивать, но быть «занятой» по `requests`: `0/3 nodes are available: … 2 Insufficient cpu`. И наоборот: поды без `requests` ставятся «бесплатно» и перегружают ноду.

### 4. `required` anti-affinity при `replicas` больше числа нод

В опыте третья реплика осталась в `Pending` с `didn't match pod anti-affinity rules`. Для масштабируемых сервисов используйте topology spread.

### 5. `IgnoredDuringExecution`

Смена меток ноды или taint `NoSchedule` не затрагивает работающие поды. Выселяет только `NoExecute`.

### 6. PDB блокирует `drain`

`minAvailable`, равный числу реплик, даёт `ALLOWED DISRUPTIONS 0`, и `drain` зависает до таймаута (в опыте `global timeout reached: 15s`).

### 7. Замена вытесненного пода тоже остаётся в `Pending`

В опыте вытеснение освободило место `vip`, а замена жертвы осталась без места. Вытеснение переносит проблему нехватки ресурсов на менее важные поды, но не решает её. Решает её Cluster Autoscaler или больше нод ([модуль 15](../15-autoscaling/advanced.ru.md#vpa-и-cluster-autoscaler)).

### 8. `nodeName` обходит всё

Под с `nodeName` попал на control-plane, хотя там `NoSchedule` taint (в опыте). Планировщик, а значит и фильтры по `requests`, в этом пути не участвует.

### 9. Сообщение `Preemption is not helpful`

В `FailedScheduling` фраза `preemption: 0/3 nodes are available: 3 Preemption is not helpful for scheduling` означает, что вытеснение ничего не даст (например, причина — несовпадение меток, а не нехватка ресурсов). Это не ошибка.

## Компромиссы и альтернативы

| Подход | Плюсы | Минусы |
|---|---|---|
| `nodeSelector` | прост | только равенство |
| nodeAffinity | операторы, мягкие правила | многословно |
| taints + tolerations | защита выделенных нод | сами не притягивают |
| podAntiAffinity | жёсткий разброс по нодам | `Pending` при нехватке нод, дороже при большом кластере |
| topology spread | ровное распределение, мягкий режим | сложнее настройка |
| PriorityClass | гарантирует место важным подам | вытесняет, нужны `requests` |
| Descheduler (внешний проект) | перераспределяет уже запущенные поды | отдельная установка; не запускался |

## Вопросы с собеседований

<details><summary>1. Как планировщик выбирает ноду?</summary>

Фильтрует ноды (ресурсы по `requests`, метки, taints, affinity), оценивает оставшиеся и записывает выбор в `spec.nodeName`.

→ [Базовая теория](basics.ru.md#как-планировщик-выбирает-ноду)

</details>

<details><summary>2. Почему под в <code>Pending</code> и как узнать причину?</summary>

Нет подходящей ноды. Причины по нодам — в событии `FailedScheduling` (`kubectl describe pod`).

→ [Базовая теория](basics.ru.md#как-планировщик-выбирает-ноду)

</details>

<details><summary>3. Чем nodeSelector отличается от nodeAffinity?</summary>

nodeSelector — только равенство меток. nodeAffinity добавляет операторы и мягкий режим (`preferred…`).

→ [Базовая теория](basics.ru.md#nodeaffinity)

</details>

<details><summary>4. Ловушка: toleration на taint <code>gpu</code> гарантирует, что под попадёт на GPU-ноду?</summary>

Нет. Toleration только разрешает. Чтобы закрепить, нужен ещё nodeSelector или affinity.

→ [Подвох 1](#1-toleration-не-притягивает)

</details>

<details><summary>5. Чем <code>NoSchedule</code> отличается от <code>NoExecute</code>?</summary>

`NoSchedule` действует только на новые поды, `NoExecute` выселяет и работающие без toleration.

→ [Продвинутая теория](advanced.ru.md#noexecute-и-tolerationseconds)

</details>

<details><summary>6. Что делает <code>tolerationSeconds</code>?</summary>

Время, которое под терпит `NoExecute`-taint до выселения. По умолчанию для `not-ready` и `unreachable` — 300 с.

→ [Продвинутая теория](advanced.ru.md#noexecute-и-tolerationseconds)

</details>

<details><summary>7. Как разнести реплики по нодам и когда лучше topology spread?</summary>

`podAntiAffinity` с `kubernetes.io/hostname` — жёстко «по одной на ноду». Topology spread допускает перекос `maxSkew` и годится, когда реплик больше, чем нод.

→ [Продвинутая теория](advanced.ru.md#topologyspreadconstraints)

</details>

<details><summary>8. Что происходит, если под с высоким приоритетом не помещается?</summary>

Планировщик ищет ноду, где можно вытеснить поды с меньшим приоритетом; жертвы получают `Preempted`. При `preemptionPolicy: Never` вытеснения нет.

→ [Продвинутая теория](advanced.ru.md#приоритеты-и-вытеснение)

</details>

<details><summary>9. Ловушка: нода простаивает по CPU, но под в <code>Pending</code> с <code>Insufficient cpu</code>. Почему?</summary>

Планировщик считает сумму `requests`, а не фактическое потребление.

→ [Подвох 3](#3-планировщик-не-смотрит-на-реальное-потребление)

</details>

<details><summary>10. Чем <code>cordon</code> отличается от <code>drain</code>?</summary>

`cordon` лишь закрывает ноду для новых подов (taint `unschedulable`), `drain` дополнительно выселяет поды.

→ [Продвинутая теория](advanced.ru.md#обслуживание-ноды-cordon-drain-и-pdb)

</details>

<details><summary>11. Почему <code>drain</code> завис?</summary>

Часто из-за PDB с `ALLOWED DISRUPTIONS 0`, либо из-за подов без контроллера и DaemonSet (нужны `--ignore-daemonsets` и осознанное удаление).

→ [Подвох 6](#6-pdb-блокирует-drain)

</details>

<details><summary>12. Что такое <code>IgnoredDuringExecution</code> в названии правила?</summary>

Правило проверяется только при планировании: если метки изменились позже, работающий под не переедет.

→ [Подвох 5](#5-ignoredduringexecution)

</details>

<details><summary>13. Что делает <code>nodeName</code> и почему это опасно?</summary>

Назначает ноду напрямую, обходя планировщик и его фильтры. Под может попасть на ноду с taint или без свободных ресурсов.

→ [Подвох 8](#8-nodename-обходит-всё)

</details>

## Шпаргалка

```text
kubectl label node N k=v          метка ноды
kubectl taint node N k=v:NoSchedule[-]
kubectl cordon N | uncordon N | drain N --ignore-daemonsets --delete-emptydir-data
kubectl get events --field-selector reason=FailedScheduling
kubectl get pdb ; kubectl get priorityclass
```

- Pending → `describe pod` → события.
- Taint отталкивает, toleration разрешает, affinity/selector притягивает.
- Разброс: topology spread (мягко) или antiAffinity (жёстко).
- Приоритет вытесняет; `preemptionPolicy: Never` не вытесняет.
- Перед `drain` — PDB с запасом.

## Связанные материалы

- Предыдущий уровень: [продвинутая теория](advanced.ru.md)
- Автомасштабирование: [15 · Автомасштабирование](../15-autoscaling/expert.ru.md)
- DaemonSet: [12 · Другие workloads](../12-workloads/expert.ru.md)
- Безопасность и PSA: [17 · Безопасность](../17-security/overview.ru.md)

## Источники

- [Kubernetes Scheduler](https://kubernetes.io/docs/concepts/scheduling-eviction/kube-scheduler/)
- [Scheduling Framework](https://kubernetes.io/docs/concepts/scheduling-eviction/scheduling-framework/)
- [Taints and Tolerations](https://kubernetes.io/docs/concepts/scheduling-eviction/taint-and-toleration/)
- [Well-Known Labels, Annotations and Taints](https://kubernetes.io/docs/reference/labels-annotations-taints/)
- [Pod Priority and Preemption](https://kubernetes.io/docs/concepts/scheduling-eviction/pod-priority-preemption/)
