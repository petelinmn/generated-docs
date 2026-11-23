---
title: "Планирование подов: базовая теория"
lang: ru
track: kubernetes
kind: module
doc: basics
slug: 18-scheduling
part: 3
tags: [scheduler, nodeselector, nodeaffinity, taints, tolerations, pending]
versions: "Kubernetes 1.37.0 (kind v0.34, 3 ноды), kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Планирование подов: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [04 · Pod](../04-pods/overview.ru.md), [05 · Deployment](../05-deployments/overview.ru.md) · **Время:** ~2 ч

Все примеры выполнены в kind (Kubernetes 1.37.0) на трёх нодах: `learn-control-plane`, `learn-worker`, `learn-worker2`; namespace `learn-18`. Ноды получили метки командами `kubectl label node learn-worker disk=ssd zone=a` и `kubectl label node learn-worker2 zone=b`.

## Зачем это нужно

По умолчанию под может оказаться на любой ноде, где хватает ресурсов. Реальные задачи сложнее: базе данных нужен быстрый диск, агенту GPU, а реплики одного сервиса лучше не складывать на одну ноду. Правила планирования выражают эти требования в манифесте.

## Как планировщик выбирает ноду

`kube-scheduler` следит за подами без `spec.nodeName` и для каждого:

1. **Фильтрует** ноды: хватает ли свободных `requests` (CPU, память), совпадают ли метки, не мешает ли taint и так далее.
2. **Оценивает** оставшиеся и выбирает лучшую.
3. Записывает выбор в `spec.nodeName` (привязка, binding). Дальше под запускает kubelet этой ноды.

Если после фильтрации не осталось ни одной ноды, под остаётся в `Pending`, а планировщик пишет событие `FailedScheduling` с разбором причин по нодам. Это главный инструмент диагностики:

```text
$ kubectl -n learn-18 describe pod sel-bad
  Warning  FailedScheduling  15s   default-scheduler  0/3 nodes are available: 1 node(s) had untolerated taint(s), 2 node(s) didn't match Pod's node affinity/selector. preemption: 0/3 nodes are available: 3 Preemption is not helpful for scheduling.
```

Читать так: из трёх нод одна отброшена из-за taint (control-plane), две — потому что метки не подошли.

## nodeSelector

Самое простое правило: под идёт только на ноды, у которых есть **все** перечисленные метки.

```yaml
apiVersion: v1
kind: Pod
metadata: {name: sel-ok}
spec:
  nodeSelector: {disk: ssd}
  containers: [{name: c, image: nginx:1.29}]
```

```text
$ kubectl -n learn-18 get pod sel-ok -o wide
NAME     READY   STATUS    NODE
sel-ok   1/1     Running   learn-worker
```

Метки нод смотрим так: `kubectl get nodes --show-labels`, задаём — `kubectl label node <имя> ключ=значение`. Если подходящих нод нет (`nodeSelector: {disk: nvme}`), под остаётся `Pending` (см. пример в [обзоре](overview.ru.md#минимальный-пример)).

## nodeAffinity

`nodeAffinity` делает то же, что `nodeSelector`, но выразительнее: есть операторы (`In`, `NotIn`, `Exists`, `DoesNotExist`, `Gt`, `Lt`) и два режима.

> ⚠️ Проверить: операторы `NotIn`, `Exists`, `DoesNotExist`, `Gt`, `Lt` взяты из документации; в опытах использован только `In`.

```yaml
affinity:
  nodeAffinity:
    requiredDuringSchedulingIgnoredDuringExecution:   # жёсткое правило
      nodeSelectorTerms:
      - matchExpressions: [{key: zone, operator: In, values: [b]}]
    preferredDuringSchedulingIgnoredDuringExecution:  # пожелание
    - weight: 50
      preference:
        matchExpressions: [{key: disk, operator: In, values: [nvme]}]
```

- **`required…`** — как `nodeSelector`: нет подходящей ноды — `Pending`. В опыте под с `zone In [b]` оказался на `learn-worker2`.
- **`preferred…`** — планировщик добавляет вес к оценке подходящих нод, но не запрещает остальные. В опыте под с пожеланием `disk=nvme` (такой ноды нет) спокойно запустился на `learn-worker`.
- Длинное название читается так: «требование действует при планировании, **игнорируется при работе**». Если метки ноды изменятся после запуска, уже работающий под не переедет.

## Taints и tolerations

**Taint** вешается на ноду и отталкивает поды, у которых нет соответствующей **toleration**.

```text
$ kubectl taint node learn-worker2 dedicated=gpu:NoSchedule
node/learn-worker2 tainted
```

Формат: `ключ=значение:эффект`. Эффект `NoSchedule` запрещает **новым** подам без toleration занимать ноду. Под с toleration:

```yaml
spec:
  tolerations:
  - {key: dedicated, operator: Equal, value: gpu, effect: NoSchedule}
```

Опыт: два пода просили ноду `zone=b` (`learn-worker2`):

```text
$ kubectl -n learn-18 get pods -o wide
NAME       READY   STATUS    NODE
no-tol     0/1     Pending   <none>
with-tol   1/1     Running   learn-worker2
$ kubectl -n learn-18 describe pod no-tol
  Warning  FailedScheduling  … 0/3 nodes are available: 1 node(s) didn't match Pod's node affinity/selector, 2 node(s) had untolerated taint(s). …
```

Важно: **toleration только разрешает, а не притягивает.** Под с toleration может попасть на любую подходящую ноду, в том числе без taint. Чтобы закрепить под за выделенной нодой, комбинируют taint (отталкивает чужих) и `nodeSelector` или affinity (притягивает своих).

Снять taint — тот же ключ со знаком минус: `kubectl taint node learn-worker2 dedicated=gpu:NoSchedule-`.

### Taint на control-plane

На ноде control-plane в kind уже висит taint:

```text
$ kubectl get node learn-control-plane -o jsonpath='{.spec.taints}'
[{"effect":"NoSchedule","key":"node-role.kubernetes.io/control-plane"}]
```

Поэтому обычные поды туда не попадают, а системные (kube-proxy, kindnet) и [DaemonSet](../12-workloads/overview.ru.md) с нужной toleration — попадают.

## Ошибки новичка

- ❌ Ждать, что toleration «притянет» под на ноду с taint → ✅ добавить `nodeSelector` или affinity.
- ❌ Не читать `FailedScheduling` и гадать → ✅ `kubectl describe pod`: в событии перечислены причины по нодам.
- ❌ Опечатка в значении метки (`ssd ` или `SSD`) → ✅ метки чувствительны к регистру; сверять с `kubectl get nodes --show-labels`.
- ❌ Жёсткие правила на все поды → ✅ `preferred…` там, где достаточно пожелания.
- ❌ Считать, что после смены меток нод поды переедут → ✅ `IgnoredDuringExecution`: уже работающие поды остаются на месте.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — antiAffinity, topology spread, приоритеты, drain
- Pod и его жизненный цикл: [04 · Pod](../04-pods/overview.ru.md)
- Requests как входные данные планировщика: [11 · Probes и ресурсы](../11-probes-and-resources/overview.ru.md)
- DaemonSet и tolerations: [12 · Другие workloads](../12-workloads/overview.ru.md)

## Источники

- [Assigning Pods to Nodes](https://kubernetes.io/docs/concepts/scheduling-eviction/assign-pod-node/)
- [Taints and Tolerations](https://kubernetes.io/docs/concepts/scheduling-eviction/taint-and-toleration/)
- [Kubernetes Scheduler](https://kubernetes.io/docs/concepts/scheduling-eviction/kube-scheduler/)
