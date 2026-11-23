---
title: "Namespace и RBAC: экспертная теория"
lang: ru
track: kubernetes
kind: module
doc: expert
slug: 10-namespaces-and-rbac
part: 2
tags: [rbac, authorization, escalation-prevention, serviceaccount, bound-tokens, aggregated-clusterroles, impersonation, resourcequota, system-masters]
versions: "Kubernetes 1.37.0 (kind v0.34), kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Namespace и RBAC: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

## Под капотом

### Как принимается решение «можно или нет»

Запрос проходит этапы [аутентификация → авторизация → admission](../02-architecture/advanced.ru.md#путь-запроса-через-api-server). Режим авторизации в нашем кластере — `Node,RBAC` (флаг `--authorization-mode` у `kube-apiserver`): модули проверяются по очереди, и запрос проходит, если один из них его разрешил. Модуль `Node` отвечает за kubelet-ы, `RBAC` — за всё остальное.

RBAC вычисляет разрешение так: собирает все привязки, где субъект запроса (пользователь, его группы, ServiceAccount) встречается среди `subjects`, берёт роли этих привязок и ищет в их `rules` правило, совпадающее с запросом по `apiGroups`, `resources` (и подресурсам), `verbs`, а при необходимости `resourceNames`. Совпало — разрешено, нет — отказ (`Forbidden`). Запрещающих правил нет, поэтому порядок и «приоритет» ролей не важны: итог — объединение всех выданных прав.

### Идентичность: пользователь, группы, ServiceAccount

- **Пользователей и групп как объектов нет.** Их задаёт способ аутентификации: клиентский сертификат (CN — имя, O — группа), токен внешнего провайдера или токен ServiceAccount.
- **ServiceAccount** имеет имя вида `system:serviceaccount:<namespace>:<имя>` и автоматически входит в группы `system:serviceaccounts` и `system:serviceaccounts:<namespace>` (видно в сообщении о повышении прав ниже).
- Kind-админ из `kubectl auth whoami`: `kubernetes-admin` в группе `kubeadm:cluster-admins`; встроенные группы `system:authenticated`, `system:masters` и др.

### Защита от повышения прав

RBAC не позволяет выдать больше, чем есть у самого выдающего. Субъекту с правом создавать RoleBinding (но не имеющему прав `edit`) нельзя привязать `edit`:

```text
$ kubectl -n learn-10a create rolebinding evil --clusterrole=edit --serviceaccount=learn-10a:rbadmin \
    --as=system:serviceaccount:learn-10a:rbadmin
error: failed to create rolebinding: rolebindings.rbac.authorization.k8s.io "evil" is forbidden: user "system:serviceaccount:learn-10a:rbadmin" (groups=["system:serviceaccounts" "system:serviceaccounts:learn-10a" "system:authenticated"]) is attempting to grant RBAC permissions not currently held:
{APIGroups:[""], Resources:["bindings"], Verbs:["get" "list" "watch"]}
```

Сервер сравнил права роли `edit` с правами `rbadmin` и отклонил привязку на первом недостающем правиле. Обойти это можно только правами `escalate` (на роли) и `bind` (на привязки) — их выдают крайне осторожно.

> ⚠️ Проверить: глаголы `escalate` и `bind` и их роль в обходе защиты даны по документации и не запускались в этом материале.

### Кто имеет cluster-admin

```text
$ kubectl get clusterrolebinding -o json | … (привязки с roleRef cluster-admin)
cluster-admin           ['Group:system:masters']
kubeadm:cluster-admins  ['Group:kubeadm:cluster-admins']
```

В нашем кластере `cluster-admin` привязан к группам `system:masters` и `kubeadm:cluster-admins`. Всякий, кто попал в эти группы (например, через клиентский сертификат с `O=system:masters`), имеет полный доступ ко всему кластеру.

### Токены ServiceAccount

Современные токены — **привязанные** (bound): подписанный JWT с утверждениями `aud`, `iss`, `sub`, `exp`, `iat` и привязкой к объектам (`kubernetes.io`: `pod`, `node`, `serviceaccount`). Kubelet запрашивает его у API server (TokenRequest), монтирует проекцией и обновляет. Старые бессрочные токены в Secret больше не создаются автоматически: в `learn-10a` Secret нет. Токен для внешнего использования выдаёт `kubectl create token` (срок задаётся флагом `--duration`).

## Справочник

### Поля правила Role/ClusterRole

| Поле | Смысл |
|---|---|
| `apiGroups` | группы API: `""` — core, `apps`, `batch`, `rbac.authorization.k8s.io` и др.; `*` — все |
| `resources` | типы объектов и подресурсы (`pods`, `pods/log`, `deployments/scale`); `*` — все |
| `verbs` | `get`, `list`, `watch`, `create`, `update`, `patch`, `delete`, `deletecollection`; дополнительно `escalate`, `bind`, `impersonate` |
| `resourceNames` | ограничение конкретными именами объектов |
| `nonResourceURLs` | URL вне ресурсов (`/healthz`, `/metrics`), только в ClusterRole |

> ⚠️ Проверить: `resourceNames` и `nonResourceURLs` в пользовательских ролях не запускались; `nonResourceURLs` виден в правилах `cluster-admin` (`"*"`).

### Привязки

| Объект | Поля |
|---|---|
| RoleBinding | `roleRef` (Role или ClusterRole, неизменяемый), `subjects`, `metadata.namespace` |
| ClusterRoleBinding | `roleRef` (ClusterRole), `subjects` |
| `subjects[].kind` | `User`, `Group`, `ServiceAccount` |

`roleRef` после создания изменить нельзя: привязку пересоздают.

### Команды

| Команда | Что делает |
|---|---|
| `kubectl auth can-i <глагол> <ресурс> [--as=… --as-group=… -n …]` | проверка права |
| `kubectl auth can-i --list` | все права субъекта в namespace |
| `kubectl auth whoami` | кто вы с точки зрения сервера |
| `kubectl create role/clusterrole … --verb=… --resource=…` | создать роль |
| `kubectl create rolebinding/clusterrolebinding … --serviceaccount=ns:sa` | привязать |
| `kubectl create token <sa> --duration=…` | выпустить токен |

### Объекты namespace, которые стоит помнить

| Объект | Назначение | Проверено |
|---|---|---|
| ResourceQuota | суммарные лимиты ресурсов и числа объектов | да |
| LimitRange | значения по умолчанию и границы для контейнера | да |
| ServiceAccount | идентичность пода | да |
| Role, RoleBinding | права внутри namespace | да |
| NetworkPolicy | сетевая изоляция (модуль 17) | нет |

## Подвохи

### Подвох: namespace не изолирует сеть и права сами по себе

Поды из разных namespace по умолчанию общаются друг с другом по сети (в [модуле 06](../06-services/expert.ru.md#подвох-короткое-имя-работает-только-в-своём-namespace) под из `learn-06b` дозвонился до Service в `learn-06`). Права тоже не появляются сами: разделить доступ позволяют только RBAC и NetworkPolicy.

### Подвох: нельзя искать объект по имени во всех namespace

`kubectl get cm cfg -A` → `error: a resource cannot be retrieved by name across all namespaces`.

### Подвох: квота без LimitRange отклоняет поды без ресурсов

`failed quota: compute: must specify limits.cpu for: …; requests.memory for: …`. После LimitRange значения подставляются сами.

### Подвох: Deployment упёрся в квоту и молчит

`0/2` и `ReplicaFailure=True (FailedCreate)`: ошибка в условии и событиях, а не в выводе `kubectl apply`.

### Подвох: квота делает «последний под» непредсказуемым

Под на 400m поместился, а следующие на 100m уже нет (предел 500m): проверка идёт при создании, а не в планировщике, и не зависит от нод.

### Подвох: `--as` не проверяет существование субъекта

`kubectl auth can-i list pods --as=jane` отвечает `no`, хотя пользователя `jane` нигде нет: имперсонация не требует объекта пользователя.

### Подвох: можно выдать права на несуществующий ресурс или группу

Привязка к группе `devs` создалась без ошибок (в Kubernetes групп-объектов нет). Опечатка в имени группы или `apiGroups` даёт молчаливо бесполезную привязку.

### Подвох: ClusterRole по RoleBinding действует только в namespace

`view` через RoleBinding разрешил `list deployments` в `learn-10a`, но не в `default`.

### Подвох: `create pods` = доступ к Secret

Право создавать поды позволяет смонтировать любой Secret этого namespace ([модуль 07](../07-config-and-secrets/expert.ru.md#доступ-rbac)).

### Подвох: токен есть у каждого пода по умолчанию

Без `automountServiceAccountToken: false` в каждый под монтируется токен `default` ServiceAccount (в опыте `ca.crt`, `namespace`, `token`).

## Компромиссы и альтернативы

| Подход | Плюсы | Минусы |
|---|---|---|
| Namespace на команду | простые права и квоты | много объектов, общий кластер |
| Отдельный кластер на команду | сильная изоляция | цена и сложность эксплуатации |
| Виртуальные кластеры (vcluster) | изоляция между сильной и слабой | дополнительный компонент |
| RBAC | встроен, декларативен | нет запретов, сложно аудировать |
| ABAC, внешняя авторизация (webhook) | гибкость | сложность, зависимость от внешней системы |
| OPA Gatekeeper / Kyverno | политики сверх RBAC (что разрешено создавать) | отдельный компонент (модуль 17) |

> ⚠️ Проверить: vcluster, OPA Gatekeeper, Kyverno и webhook-авторизация описаны по документации и не запускались.

## Вопросы с собеседований

**1. Что такое namespace и что он изолирует?**

<details><summary>Ответ</summary>

Логическое пространство имён внутри кластера: объекты уникальны по имени только внутри него, на него навешиваются квоты и права. Сеть и безопасность он не изолирует сам по себе: поды разных namespace по умолчанию видят друг друга.

</details>

**2. Какие ресурсы не принадлежат namespace?**

<details><summary>Ответ</summary>

Кластерные: ноды, PersistentVolume, StorageClass, сами namespace, CRD, ClusterRole и др. В нашем кластере 34 типа namespaced и 37 кластерных.

</details>

**3. Чем Role отличается от ClusterRole и RoleBinding от ClusterRoleBinding?**

<details><summary>Ответ</summary>

Role и RoleBinding действуют в одном namespace, ClusterRole и ClusterRoleBinding — на весь кластер (включая кластерные ресурсы). ClusterRole, привязанную RoleBinding-ом, ограничивают одним namespace.

</details>

**4. (С подвохом) Можно ли в RBAC запретить действие?**

<details><summary>Ответ</summary>

Нет: правила только разрешающие. Всё, что не выдано, запрещено. Итоговые права — объединение всех привязок субъекта.

</details>

**5. Что такое ServiceAccount и как под получает его токен?**

<details><summary>Ответ</summary>

Идентичность пода в namespace. Kubelet проецирует в под короткоживущий токен, привязанный к поду, в `/var/run/secrets/kubernetes.io/serviceaccount` вместе с `ca.crt` и `namespace`. Можно отказаться: `automountServiceAccountToken: false`.

</details>

**6. (С подвохом) Субъект может создавать RoleBinding. Может ли он выдать себе `cluster-admin`?**

<details><summary>Ответ</summary>

Нет: API отклоняет выдачу прав, которых нет у выдающего (`attempting to grant RBAC permissions not currently held`). Обход возможен лишь через права `escalate` и `bind`.

</details>

**7. Как проверить, что может ServiceAccount, не имея его токена?**

<details><summary>Ответ</summary>

`kubectl auth can-i <действие> --as=system:serviceaccount:<ns>:<sa>`, список — `--list`. Имперсонация требует права `impersonate` у проверяющего.

</details>

**8. Чем ResourceQuota отличается от LimitRange?**

<details><summary>Ответ</summary>

ResourceQuota ограничивает суммарное потребление namespace (CPU, память, число объектов). LimitRange задаёт значения по умолчанию и границы для отдельного контейнера или пода. Вместе: квота на CPU требует явных значений, LimitRange их подставляет.

</details>

**9. (С подвохом) Deployment показывает `0/2`, ошибок при `apply` не было. Что проверить?**

<details><summary>Ответ</summary>

Условие `ReplicaFailure` у Deployment и события `FailedCreate` у ReplicaSet: причина может быть в исчерпанной квоте (`exceeded quota: compute, requested: requests.cpu=100m, used: … limited: …`) или в нарушении LimitRange.

</details>

**10. Как ограничить, какие namespace может читать приложение?**

<details><summary>Ответ</summary>

Выдать Role в нужном namespace и привязать её RoleBinding-ом к ServiceAccount приложения; не выдавать ClusterRoleBinding. Проверка: `auth can-i list pods -n другой --as=…` вернёт `no`.

</details>

**11. Что даёт `view` и можно ли с ним читать Secret?**

<details><summary>Ответ</summary>

Чтение большинства объектов namespace; Secret не входят (в опыте `auth can-i get secrets` — `no`). `view` собирается агрегированием ClusterRole с меткой `aggregate-to-view`.

</details>

**12. Почему право `create pods` опасно?**

<details><summary>Ответ</summary>

Под может смонтировать любой Secret своего namespace и использовать любой ServiceAccount, поэтому создающий поды фактически получает доступ к секретам и правам этих учётных записей. Нужны Pod Security и ограничения RBAC.

</details>

## Шпаргалка

```bash
kubectl create ns NS
kubectl -n NS create serviceaccount SA
kubectl -n NS create role R --verb=get,list,watch --resource=pods,pods/log
kubectl -n NS create rolebinding RB --role=R --serviceaccount=NS:SA
kubectl create clusterrole CR --verb=get,list --resource=nodes
kubectl create clusterrolebinding CRB --clusterrole=CR --serviceaccount=NS:SA
kubectl -n NS create rolebinding RB2 --clusterrole=view --group=devs
kubectl -n NS auth can-i list pods --as=system:serviceaccount:NS:SA
kubectl -n NS auth can-i --list --as=system:serviceaccount:NS:SA
kubectl auth whoami
kubectl -n NS create token SA --duration=10m
kubectl -n NS describe quota
```

## Связанные материалы

- Предыдущие уровни: [базовая](basics.ru.md), [продвинутая](advanced.ru.md)
- Предыдущие модули: [07 · ConfigMap и Secret](../07-config-and-secrets/overview.ru.md), [02 · Архитектура](../02-architecture/overview.ru.md)
- Фундамент: [трек Linux](../../../linux/overview.ru.md)

## Источники

- [Using RBAC Authorization](https://kubernetes.io/docs/reference/access-authn-authz/rbac/)
- [Privilege escalation prevention and bootstrapping](https://kubernetes.io/docs/reference/access-authn-authz/rbac/#privilege-escalation-prevention-and-bootstrapping)
- [Service Accounts](https://kubernetes.io/docs/concepts/security/service-accounts/)
- [Authenticating](https://kubernetes.io/docs/reference/access-authn-authz/authentication/)
- [Resource Quotas](https://kubernetes.io/docs/concepts/policy/resource-quotas/)
- [Limit Ranges](https://kubernetes.io/docs/concepts/policy/limit-range/)
