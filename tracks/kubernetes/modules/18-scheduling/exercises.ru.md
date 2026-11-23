---
title: "Упражнения: планирование подов"
lang: ru
track: kubernetes
kind: module
doc: exercises
slug: 18-scheduling
part: 3
tags: [scheduler, taints, affinity, priorityclass, drain]
status: draft
updated: 2026-11-23
---

# Упражнения: планирование подов

🌐 **Русский** · [English](exercises.en.md)

> Решайте самостоятельно, подсказки и решения под спойлерами. Нужен кластер kind минимум с двумя рабочими нодами (`kind create cluster --name learn --config` с конфигом из `control-plane` и двух `worker`) и namespace: `kubectl create namespace learn-18`. После работы снимите метки, taints и удалите namespace: `kubectl delete namespace learn-18`.

## 1. 🟢 nodeSelector и `Pending`

**Задача:** пометьте `learn-worker` меткой `disk=ssd`. Создайте под с `nodeSelector: {disk: ssd}` и под с `nodeSelector: {disk: nvme}`. Объясните, почему второй в `Pending`.

**Ожидаемый результат:** первый под на `learn-worker`; во втором событии `FailedScheduling` с `didn't match Pod's node affinity/selector`.

<details><summary>Подсказка</summary>

`kubectl label node learn-worker disk=ssd`; причина — в `kubectl describe pod`.

</details>

<details><summary>Решение</summary>

```text
sel-ok   Running   learn-worker
sel-bad  Pending   <none>
  Warning  FailedScheduling  0/3 nodes are available: 1 node(s) had untolerated taint(s), 2 node(s) didn't match Pod's node affinity/selector. …
```

</details>

## 2. 🟢 Taint и toleration

**Задача:** повесьте на `learn-worker2` taint `dedicated=gpu:NoSchedule`. Создайте два пода, привязанных к этой ноде меткой (`zone=b`): с toleration и без. Сравните.

**Ожидаемый результат:** под с toleration `Running` на `learn-worker2`, без — `Pending` с `2 node(s) had untolerated taint(s)`.

<details><summary>Подсказка</summary>

`kubectl taint node learn-worker2 dedicated=gpu:NoSchedule`; `nodeSelector: {zone: b}` нужен, чтобы оба пода «хотели» эту ноду.

</details>

<details><summary>Решение</summary>

```text
with-tol   Running   learn-worker2
no-tol     Pending   <none>
  … 1 node(s) didn't match Pod's node affinity/selector, 2 node(s) had untolerated taint(s). …
```

Снять taint: `kubectl taint node learn-worker2 dedicated=gpu:NoSchedule-`.

</details>

## 3. 🟡 Разнести реплики

**Задача:** Deployment из трёх реплик с `required` podAntiAffinity по `kubernetes.io/hostname`. Посмотрите, где окажутся поды. Затем замените правило на `topologySpreadConstraints` (`maxSkew: 1`, `DoNotSchedule`) и пять реплик.

**Ожидаемый результат:** при anti-affinity два пода `Running`, один `Pending` (`didn't match pod anti-affinity rules`); при topology spread все пять `Running`, распределение 3 и 2.

<details><summary>Подсказка</summary>

`topologyKey` для spread — метка нод (`zone`), которую вы поставили; `labelSelector` совпадает с метками подов.

</details>

<details><summary>Решение</summary>

```text
anti-…-7dh4h   Running   learn-worker
anti-…-9pg79   Pending   <none>
anti-…-jznkq   Running   learn-worker2
spread, replicas=5: learn-worker 3, learn-worker2 2
```

</details>

## 4. 🟡 Приоритет и вытеснение

**Задача:** создайте PriorityClass `low` (100) и `high` (100000). Запустите Deployment из двух подов с `requests: cpu: 10` и классом `low`. Затем один под с тем же запросом и классом `high`. Найдите события вытеснения. Повторите с классом, у которого `preemptionPolicy: Never`.

**Ожидаемый результат:** `Preempted by pod …`; во втором случае `preemption: not eligible due to preemptionPolicy=Never`.

<details><summary>Подсказка</summary>

CPU на нодах `12`: `kubectl get nodes -o custom-columns=N:.metadata.name,CPU:.status.allocatable.cpu`.

</details>

<details><summary>Решение</summary>

```text
Warning  FailedScheduling  pod/vip  … preemption: found a potential placement for pod on node learn-worker2, preempting 1 victims
Normal   Preempted         pod/low-…  Preempted by pod … on node learn-worker2
Warning  FailedScheduling  pod/vip-nopre  … preemption: not eligible due to preemptionPolicy=Never.
```

</details>

## 5. 🔴 `drain` и PDB

**Задача:** Deployment `web` из четырёх реплик. Создайте `PodDisruptionBudget` с `--min-available=4`. Выполните `kubectl drain` ноды, где стоят поды (с `--timeout=15s`). Затем исправьте PDB так, чтобы `drain` прошёл.

**Ожидаемый результат:** `ALLOWED DISRUPTIONS 0` и ошибка `global timeout reached`; после `--min-available=3` (или `maxUnavailable: 1`) — выселение по одному.

<details><summary>Подсказка</summary>

`kubectl get pdb`; не забудьте `kubectl uncordon <нода>`.

</details>

<details><summary>Решение</summary>

```text
NAME      MIN AVAILABLE   MAX UNAVAILABLE   ALLOWED DISRUPTIONS
web-pdb   4               N/A               0
error when evicting pods/"web-…" -n "learn-18": global timeout reached: 15s
```

После `kubectl delete pdb web-pdb` и `kubectl create pdb web-pdb --selector=app=web --min-available=3` выселяется по одному поду; если замены некуда запуститься (нода одна), они остаются в `Pending`.

> ⚠️ Проверить: успешный `drain` после исправления PDB в опыте не воспроизводился; в опыте проверена только блокировка.

</details>
