---
title: "Планирование подов: продвинутая теория"
lang: ru
track: kubernetes
kind: module
doc: advanced
slug: 18-scheduling
part: 3
tags: [pod-anti-affinity, topology-spread, noexecute, tolerationseconds, priorityclass, preemption, cordon, drain, pdb]
versions: "Kubernetes 1.37.0 (kind v0.34, 3 ноды), kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Планирование подов: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md). Опыты — в kind на трёх нодах, namespace `learn-18`.

## Pod affinity и anti-affinity

Правила «рядом с кем» и «подальше от кого» смотрят на метки **других подов**, а область определяет `topologyKey` — метка ноды: `kubernetes.io/hostname` — «одна нода», `topology.kubernetes.io/zone` — «одна зона».

```yaml
spec:
  replicas: 3
  template:
    metadata: {labels: {app: anti}}
    spec:
      affinity:
        podAntiAffinity:
          requiredDuringSchedulingIgnoredDuringExecution:
          - labelSelector: {matchLabels: {app: anti}}
            topologyKey: kubernetes.io/hostname
```

Смысл: «не ставить под рядом с другим подом `app=anti` на одной ноде». Рабочих нод две, реплик три:

```text
$ kubectl -n learn-18 get pods -o wide
anti-dc6868b9c-7dh4h   Running   learn-worker
anti-dc6868b9c-9pg79   Pending   <none>
anti-dc6868b9c-jznkq   Running   learn-worker2
$ kubectl -n learn-18 get events --field-selector reason=FailedScheduling
0/3 nodes are available: 1 node(s) had untolerated taint(s), 2 node(s) didn't match pod anti-affinity rules. preemption: 0/3 nodes are available: 1 Preemption is not helpful for scheduling, 2 No preemption victims found for incoming pod.
```

Третья реплика ждёт: жёсткое правило сильнее желания «запустить все реплики». `podAffinity` работает зеркально: «хочу быть на одной ноде или в одной зоне с подом X» (например, приложение рядом с кэшем). Для мягкого варианта есть `preferredDuring…` с весом.

## topologySpreadConstraints

Anti-affinity говорит «не больше одного на ноду», а нужно часто другое: «распределить как можно ровнее, допустим перекос не больше одного пода». Для этого есть `topologySpreadConstraints`:

```yaml
spec:
  topologySpreadConstraints:
  - maxSkew: 1
    topologyKey: zone
    whenUnsatisfiable: DoNotSchedule
    labelSelector: {matchLabels: {app: spread}}
```

- `maxSkew` — максимально допустимая разница числа подов между самыми загруженной и свободной областями (доменами).
- `whenUnsatisfiable`: `DoNotSchedule` — жёстко, `ScheduleAnyway` — мягко.

В опыте зоны `a` и `b` — две рабочие ноды:

```text
replicas=4: learn-worker 2, learn-worker2 2
replicas=5: learn-worker 3, learn-worker2 2
```

Перекос 1 допустим; шестая и седьмая реплики снова выровняют распределение. Это лучше anti-affinity для сервисов с числом реплик больше числа нод.

> ⚠️ Проверить: режим `ScheduleAnyway`, поля `minDomains`, `matchLabelKeys`, `nodeAffinityPolicy` и `nodeTaintsPolicy` взяты из документации и не запускались.

## NoExecute и tolerationSeconds

Taint с эффектом `NoExecute` действует и на **уже работающие** поды без toleration — их выселяют. Опыт: на `learn-worker2` стояли два «голых» пода (без контроллера), затем `kubectl taint node learn-worker2 maint=true:NoExecute`:

```text
$ kubectl -n learn-18 get pods
No resources found in learn-18 namespace.
```

Поды удалены и не возродятся: за ними никто не следит. Поэтому «голые» поды в production — плохая идея (см. [модуль 04](../04-pods/basics.ru.md)).

`tolerationSeconds` задаёт, сколько под терпит taint, прежде чем будет выселен:

```yaml
tolerations:
- {key: maint, operator: Exists, effect: NoExecute, tolerationSeconds: 20}
```

Опыт с Deployment из двух реплик, `kubectl taint node learn-worker maint=true:NoExecute`:

```text
t=14  tolsec-…-n6j47 (worker2)   tolsec-…-vbksb (worker)
t=21  tolsec-…-d6bdt (worker)    tolsec-…-n6j47 (worker2)   ← vbksb выселен через ~20 с
t=42  tolsec-…-n6j47 (worker2)   tolsec-…-plz74 (worker)    ← d6bdt выселен ещё через ~20 с
```

Подвох: замена тоже имеет эту toleration, поэтому `ReplicaSet` создаёт её, планировщик **снова** ставит её на ту же ноду (taint ему не помеха), а через 20 секунд её выселяют снова. Цикл продолжается, пока taint не снят. Для настоящего «слить ноду» нужны `drain` и подходящие правила (см. ниже).

Такие toleration есть и у ваших подов, хотя вы их не писали: их добавляет API-сервер.

```text
$ kubectl -n learn-18 get pod web-… -o jsonpath='{.spec.tolerations}'
[{"effect":"NoExecute","key":"node.kubernetes.io/not-ready","operator":"Exists","tolerationSeconds":300},
 {"effect":"NoExecute","key":"node.kubernetes.io/unreachable","operator":"Exists","tolerationSeconds":300}]
```

То есть при потере связи с нодой поды выселяются через 5 минут.

## Приоритеты и вытеснение

**PriorityClass** — кластерный объект: имя и число. Чем больше число, тем важнее под. Когда поду с высоким приоритетом не хватает места, планировщик может **вытеснить (preempt)** поды с меньшим приоритетом.

```yaml
apiVersion: scheduling.k8s.io/v1
kind: PriorityClass
metadata: {name: high-18}
value: 100000
```

Под выбирает класс так: `spec.priorityClassName: high-18`. Без класса приоритет равен 0, политика — `PreemptLowerPriority`:

```text
$ kubectl get priorityclass
NAME                      VALUE        GLOBAL-DEFAULT   AGE     PREEMPTIONPOLICY
high-18                   100000       false            2m10s   PreemptLowerPriority
low-18                    100          false            2m10s   PreemptLowerPriority
system-cluster-critical   2000000000   false            11m     PreemptLowerPriority
system-node-critical      2000001000   false            11m     PreemptLowerPriority
```

Опыт: на каждой рабочей ноде 12 CPU. Deployment `low` из двух подов с `requests: cpu: 10` и приоритетом 100 занял обе ноды. Затем под `vip` с таким же запросом и приоритетом 100000:

```text
$ kubectl -n learn-18 get pods -o wide
low-7d849d9689-ddq7j   Pending   <none>
low-7d849d9689-vq4t4   Running   learn-worker
vip                    Running   learn-worker2
$ kubectl -n learn-18 get events | grep -iE "preempt"
Warning  FailedScheduling  pod/vip  0/3 nodes are available: 1 node(s) had untolerated taint(s), 2 Insufficient cpu. preemption: found a potential placement for pod on node learn-worker2, preempting 1 victims
Normal   Preempted         pod/low-7d849d9689-fx5vg  Preempted by pod 12d4bc6c-… on node learn-worker2
```

Вытеснен один под `low`; его замена теперь `Pending` (ресурсов не осталось). Если вытеснение нежелательно, у класса есть `preemptionPolicy: Never`: под всё равно получает высокий приоритет в очереди, но никого не вытесняет:

```text
$ kubectl -n learn-18 describe pod vip-nopre
  Warning  FailedScheduling  8s  default-scheduler  0/3 nodes are available: 1 node(s) had untolerated taint(s), 2 Insufficient cpu. preemption: not eligible due to preemptionPolicy=Never.
```

## Обслуживание ноды: cordon, drain и PDB

- `kubectl cordon <нода>` — пометить ноду как недоступную для новых подов. Это taint `node.kubernetes.io/unschedulable:NoSchedule`, а статус — `Ready,SchedulingDisabled`. Уже работающие поды остаются.
- `kubectl drain <нода> --ignore-daemonsets --delete-emptydir-data` — `cordon` и затем выселение (eviction) подов. DaemonSet-поды не выселяются (иначе они вернутся сразу же), поды с `emptyDir` теряют данные — отсюда флаг.
- `kubectl uncordon <нода>` — вернуть ноду в работу.

Опыт: `learn-worker` в состоянии cordon, все 4 пода `web` стоят на `learn-worker2`. `kubectl drain learn-worker2 --ignore-daemonsets --delete-emptydir-data` вытеснил их, а Deployment создал замены — и все четыре повисли в `Pending`: единственная рабочая нода закрыта, на control-plane taint. После `uncordon` они запустились. Выселение учитывает **PodDisruptionBudget (PDB)** — ограничение на добровольные нарушения:

```text
$ kubectl -n learn-18 create pdb web-pdb --selector=app=web --min-available=4
$ kubectl -n learn-18 get pdb
NAME      MIN AVAILABLE   MAX UNAVAILABLE   ALLOWED DISRUPTIONS   AGE
web-pdb   4               N/A               0                     2s
$ kubectl drain learn-worker --ignore-daemonsets --delete-emptydir-data --timeout=15s
There are pending nodes to be drained:
 learn-worker
error when evicting pods/"web-689b9bdbdd-lf4jv" -n "learn-18": global timeout reached: 15s
```

`ALLOWED DISRUPTIONS = 0`: из четырёх реплик нужно, чтобы работали все четыре, и `drain` не выселил ни одну. PDB защищает от случайного обнуления сервиса, но неудачный PDB (`minAvailable` равен числу реплик) блокирует обслуживание нод.

## nodeName: прямое назначение

Если задать `spec.nodeName` сразу, планировщик не участвует вообще:

```text
$ kubectl -n learn-18 get pod byname -o wide
NAME     READY   STATUS              NODE
byname   0/1     ContainerCreating   learn-control-plane
```

Под попал на control-plane, хотя там `NoSchedule` taint: проверку taint для `NoSchedule` проводит планировщик, а он обойден. Это отладочный приём, не инструмент эксплуатации.

## Лучшие практики

- Для распределения реплик предпочитайте `topologySpreadConstraints`; жёсткий `podAntiAffinity` — только если число реплик заведомо не больше числа нод.
- Выделенные ноды: taint + toleration + `nodeSelector` в одном наборе.
- Всегда задавайте `requests`: без них планировщик не знает, сколько места нужно (см. [модуль 11](../11-probes-and-resources/overview.ru.md)).
- Заводите немного PriorityClass (критичное, обычное, фоновое), а не десятки.
- Для критичных сервисов задавайте PDB, но с запасом (`minAvailable` меньше числа реплик).
- Перед `drain` проверяйте, что подов без контроллера на ноде нет: их не пересоздаст никто.

## Частые ошибки

- ❌ `requiredDuringScheduling` anti-affinity при числе реплик больше числа нод → ✅ topology spread или `preferred…`.
- ❌ Приоритеты без `requests`: вытеснение считает ресурсы, и «вытеснить нечего».
- ❌ PDB `minAvailable` = числу реплик → ✅ оставить запас для обслуживания.
- ❌ Ждать, что taint `NoExecute` «переселит» нужные поды → ✅ нужен контроллер (Deployment и т. д.).
- ❌ Использовать `nodeName` как постоянное решение → ✅ nodeSelector или affinity.

## Связанные материалы

- Дальше: [экспертная теория](expert.ru.md)
- DaemonSet и tolerations по умолчанию: [12 · Другие workloads](../12-workloads/basics.ru.md)
- Автомасштабирование и нехватка нод: [15 · Автомасштабирование](../15-autoscaling/advanced.ru.md#vpa-и-cluster-autoscaler)
- Probes и requests: [11 · Probes и ресурсы](../11-probes-and-resources/overview.ru.md)

## Источники

- [Pod Topology Spread Constraints](https://kubernetes.io/docs/concepts/scheduling-eviction/topology-spread-constraints/)
- [Pod Priority and Preemption](https://kubernetes.io/docs/concepts/scheduling-eviction/pod-priority-preemption/)
- [Node-pressure Eviction and API-initiated Eviction](https://kubernetes.io/docs/concepts/scheduling-eviction/api-eviction/)
- [Safely Drain a Node](https://kubernetes.io/docs/tasks/administer-cluster/safely-drain-node/)
- [Disruptions (PodDisruptionBudget)](https://kubernetes.io/docs/concepts/workloads/pods/disruptions/)
