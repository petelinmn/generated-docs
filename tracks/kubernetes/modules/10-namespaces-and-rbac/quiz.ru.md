---
title: "Квиз: Namespace и RBAC"
lang: ru
track: kubernetes
kind: module
doc: quiz
slug: 10-namespaces-and-rbac
part: 2
tags: [namespace, rbac, role, serviceaccount, resourcequota, limitrange]
status: draft
updated: 2026-11-23
---

# Квиз: Namespace и RBAC

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает, из какого уровня вопрос: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что произойдёт с объектами при удалении namespace?

- A) Перейдут в `default`
- B) Удалятся вместе с namespace
- C) Останутся без namespace
- D) Заморозятся

<details><summary>Ответ</summary>

**B.** Удаление namespace удаляет всё внутри (в опыте под `tmp` исчез вместе с `learn-10b`).
→ [Базовая теория](basics.ru.md#удаление-namespace)

</details>

## 2. 🟢 Какой ресурс не принадлежит namespace?

- A) Pod
- B) ConfigMap
- C) PersistentVolume
- D) ServiceAccount

<details><summary>Ответ</summary>

**C.** PV, ноды, StorageClass, сами namespace и CRD — кластерные. Pod, ConfigMap и ServiceAccount живут в namespace.
→ [Базовая теория](basics.ru.md#не-всё-живёт-в-namespace)

</details>

## 3. 🟢 Что делает `kubectl auth can-i list pods --as=system:serviceaccount:learn-10a:reader`?

- A) Выдаёт право ServiceAccount
- B) Проверяет, может ли этот субъект выполнить действие
- C) Создаёт ServiceAccount
- D) Удаляет роль

<details><summary>Ответ</summary>

**B.** Это проверка прав от имени другого субъекта (имперсонация); она ничего не меняет.
→ [Базовая теория](basics.ru.md#проверка-kubectl-auth-can-i)

</details>

## 4. 🟢 Роль разрешает `get/list/watch` на `pods`. Можно ли читать логи подов?

- A) Да, логи входят в `pods`
- B) Нет, нужен подресурс `pods/log`
- C) Да, но только `get`
- D) Нет, логи читаются только администратором

<details><summary>Ответ</summary>

**B.** `pods/log` — отдельный подресурс, его нужно указать в `resources`.
→ [Базовая теория](basics.ru.md#role-и-rolebinding)

</details>

## 5. 🟡 В namespace есть ResourceQuota на CPU и память, но нет LimitRange. Что будет с `kubectl run` без ресурсов?

- A) Под создастся
- B) Отказ: `must specify limits.cpu … requests.memory …`
- C) Под создастся и сразу удалится
- D) Применится квота по умолчанию

<details><summary>Ответ</summary>

**B.** `failed quota: compute: must specify limits.cpu for: …`. LimitRange с `default` и `defaultRequest` подставил бы значения.
→ [Продвинутая теория](advanced.ru.md#под-без-requests-и-limits-отклоняется)

</details>

## 6. 🟡 ServiceAccount получил `view` через RoleBinding в `learn-10a`. Что верно?

- A) Он видит Deployment во всех namespace
- B) Он видит Deployment в `learn-10a`, но не в `default`, и не читает Secret
- C) Он может изменять Deployment в `learn-10a`
- D) Права действуют только на поды

<details><summary>Ответ</summary>

**B.** ClusterRole по RoleBinding действует в одном namespace; `view` не включает Secret (в опыте `get secrets` — `no`).
→ [Продвинутая теория](advanced.ru.md#clusterrole-и-привязки)

</details>

## 7. 🟡 Выберите все верные утверждения

- [ ] A) В RBAC можно создать запретительное правило
- [ ] B) У `automountServiceAccountToken: false` в поде нет каталога с токеном
- [ ] C) Права группы `devs` получает `--as=jane` только вместе с `--as-group=devs`
- [ ] D) Deployment сообщает об исчерпании квоты ошибкой в `kubectl apply`

<details><summary>Ответ</summary>

**B, C.** A неверно: правила только разрешающие. D неверно: Deployment создаётся, а причина видна в условии `ReplicaFailure` и событиях `FailedCreate`.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Где посмотреть, почему Deployment `0/2` и ошибок при `apply` не было?

- A) В логах kube-proxy
- B) В условии `ReplicaFailure` Deployment и событиях `FailedCreate`
- C) В `kubectl top`
- D) В `kubectl get nodes`

<details><summary>Ответ</summary>

**B.** Сообщение: `exceeded quota: compute, requested: requests.cpu=100m, used: requests.cpu=500m, limited: requests.cpu=500m`.
→ [Продвинутая теория](advanced.ru.md#деплой-упёршийся-в-квоту-не-падает-а-зависает)

</details>

## 9. 🔴 Найди ошибку

> «Разработчик с правом создавать RoleBinding выдаст себе любые права, ведь привязка роли `cluster-admin` — всего одна команда».

<details><summary>Ответ</summary>

RBAC защищает от повышения прав: нельзя выдать то, чего нет у выдающего (`attempting to grant RBAC permissions not currently held`). В опыте попытка привязать `edit` для `rbadmin` была отклонена на первом недостающем правиле. Обойти это можно лишь правами `escalate` и `bind` — их нужно выдавать крайне осторожно.
→ [Экспертная теория](expert.ru.md#защита-от-повышения-прав)

</details>

## 10. 🔴 Открытый вопрос

Приложению нужно читать ConfigMap и логи подов только в своём namespace. Как это настроить по принципу наименьших привилегий и как проверить, что чужой namespace закрыт?

<details><summary>Ключевые пункты ответа</summary>

- Отдельный ServiceAccount для приложения (не `default`).
- Role с `resources: [configmaps, pods, pods/log]` и `verbs: [get, list, watch]` в его namespace.
- RoleBinding на этот ServiceAccount (не ClusterRoleBinding).
- Проверка: `auth can-i list pods -n learn-10a --as=…` → `yes`; `-n default` → `no`; `list secrets` → `no`; запрос из пода с токеном: `200` и `403`.
- Если доступ к API не нужен вообще — `automountServiceAccountToken: false`.

</details>

→ [Базовая теория](basics.ru.md#токен-serviceaccount-в-поде)
