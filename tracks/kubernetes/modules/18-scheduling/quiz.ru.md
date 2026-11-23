---
title: "Квиз: планирование подов"
lang: ru
track: kubernetes
kind: module
doc: quiz
slug: 18-scheduling
part: 3
tags: [scheduler, taints, affinity, priorityclass]
status: draft
updated: 2026-11-23
---

# Квиз: планирование подов

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, потом откройте ответ. Метка показывает уровень вопроса: 🟢 [базовая](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Под в `Pending` с `nodeSelector: {disk: nvme}`, а такой метки нет ни у одной ноды. Где смотреть причину?

- A) `kubectl logs`
- B) События пода: `FailedScheduling` в `kubectl describe pod`
- C) Логи kubelet
- D) `kubectl top`

<details><summary>Ответ</summary>

**B.** Контейнера ещё нет, логов нет; причину перечисляет планировщик в событии (`0/3 nodes are available: … didn't match Pod's node affinity/selector`).
→ [Базовая теория](basics.ru.md#как-планировщик-выбирает-ноду)

</details>

## 2. 🟢 Чем `preferredDuringSchedulingIgnoredDuringExecution` отличается от `required…`?

- A) Ничем
- B) `preferred` — пожелание с весом, под запустится и без подходящей ноды
- C) `preferred` действует только на работающие поды
- D) `preferred` работает только с taints

<details><summary>Ответ</summary>

**B.** В опыте под с пожеланием `disk=nvme` (такой ноды нет) запустился на `learn-worker`.
→ [Базовая теория](basics.ru.md#nodeaffinity)

</details>

## 3. 🟢 Что делает taint `dedicated=gpu:NoSchedule`?

- A) Притягивает GPU-поды на ноду
- B) Не пускает на ноду новые поды без toleration
- C) Выселяет все работающие поды
- D) Запрещает kubelet запускать контейнеры

<details><summary>Ответ</summary>

**B.** Работающие поды остаются (их выселяет только `NoExecute`), притягивает не taint, а affinity/nodeSelector.
→ [Базовая теория](basics.ru.md#taints-и-tolerations)

</details>

## 4. 🟢 Под имеет toleration на `dedicated=gpu`. Попадёт ли он обязательно на GPU-ноду?

- A) Да
- B) Нет, toleration лишь разрешает такую ноду
- C) Да, если ноды остальные заняты
- D) Нет, toleration бесполезны

<details><summary>Ответ</summary>

**B.** Закрепление — это taint + `nodeSelector`/affinity.
→ [Базовая теория](basics.ru.md#taints-и-tolerations)

</details>

## 5. 🟡 Три реплики с `required` podAntiAffinity по `kubernetes.io/hostname` и две рабочие ноды. Что будет?

- A) Все три запустятся, две на одной ноде
- B) Две запустятся, третья останется в `Pending`
- C) Deployment откажется создаваться
- D) Третья уйдёт на control-plane

<details><summary>Ответ</summary>

**B.** В опыте событие: `2 node(s) didn't match pod anti-affinity rules`. На control-plane мешает taint.
→ [Продвинутая теория](advanced.ru.md#pod-affinity-и-anti-affinity)

</details>

## 6. 🟡 Что делает taint `NoExecute` с «голым» подом без toleration, уже работающим на ноде?

- A) Ничего
- B) Выселяет, и он не возвращается
- C) Выселяет, и он перезапускается на другой ноде
- D) Переводит в `Pending`

<details><summary>Ответ</summary>

**B.** В опыте оба «голых» пода исчезли (`No resources found`); перезапустить их некому, контроллера нет.
→ [Продвинутая теория](advanced.ru.md#noexecute-и-tolerationseconds)

</details>

## 7. 🟡 Deployment терпит `maint=true:NoExecute` 20 секунд. Что будет после `kubectl taint node learn-worker maint=true:NoExecute`?

- A) Поды остаются навсегда
- B) Поды на ноде выселяются через ~20 с, замены возвращаются на ту же ноду и выселяются снова
- C) Поды переезжают один раз и остаются на другой ноде
- D) Deployment удаляется

<details><summary>Ответ</summary>

**B.** В опыте имя пода на `learn-worker` менялось дважды (t≈21 с и t≈42 с). У замены та же toleration, а taint не мешает планировщику.
→ [Продвинутая теория](advanced.ru.md#noexecute-и-tolerationseconds)

</details>

## 8. 🟡 Выберите все верные утверждения

- [ ] A) `kubectl drain` включает `cordon`
- [ ] B) PDB может заблокировать `drain`
- [ ] C) `drain` по умолчанию выселяет DaemonSet-поды
- [ ] D) `uncordon` возвращает ноду в планирование

<details><summary>Ответ</summary>

**A, B, D.** C неверно: DaemonSet-поды игнорируются флагом `--ignore-daemonsets` (иначе `drain` отказывается работать).
→ [Продвинутая теория](advanced.ru.md#обслуживание-ноды-cordon-drain-и-pdb)

</details>

## 9. 🟡 Под `vip` (приоритет 100000) не помещается, на нодах стоят поды `low` (приоритет 100). Что произойдёт?

- A) `vip` будет ждать
- B) Планировщик вытеснит под `low`, а замена вытесненного будет ждать места
- C) Все `low` удалятся навсегда
- D) Ошибка валидации

<details><summary>Ответ</summary>

**B.** В опыте `Preempted by pod …`, `vip` запустился, а новый под `low` — `Pending`.
→ [Продвинутая теория](advanced.ru.md#приоритеты-и-вытеснение)

</details>

## 10. 🔴 Найдите ошибку

> «Нода `learn-worker` почти не нагружена по CPU, значит, поду с `requests: cpu: 10` место найдётся».

<details><summary>Ответ</summary>

Планировщик суммирует `requests` уже размещённых подов, а не их реальное потребление. Если по `requests` нода занята, будет `Insufficient cpu`, даже когда она простаивает.
→ [Подвох 3](expert.ru.md#3-планировщик-не-смотрит-на-реальное-потребление)

</details>

## 11. 🔴 `drain` завис, а `kubectl get pdb` показывает `ALLOWED DISRUPTIONS 0`. Причина?

- A) Нода сломана
- B) `minAvailable` равен текущему числу реплик, выселять нельзя ни одну
- C) У подов нет `requests`
- D) PDB применяется только к `Job`

<details><summary>Ответ</summary>

**B.** В опыте `--min-available=4` при четырёх репликах: `global timeout reached: 15s`.
→ [Подвох 6](expert.ru.md#6-pdb-блокирует-drain)

</details>

## 12. 🔴 Как закрепить поды за выделенными GPU-нодами и не пустить на них чужих?

<details><summary>Ключевые мысли хорошего ответа</summary>

- Taint на GPU-нодах отталкивает всех без toleration.
- В подах GPU-нагрузки toleration разрешает им эти ноды, а `nodeSelector` или nodeAffinity притягивает именно к ним.
- Нужны оба правила: taint без selector не притянет, selector без taint не защитит ноды от чужих.

</details>

→ [Базовая теория](basics.ru.md#taints-и-tolerations)
