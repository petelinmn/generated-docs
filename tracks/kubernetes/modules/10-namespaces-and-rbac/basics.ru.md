---
title: "Namespace и RBAC: базовая теория"
lang: ru
track: kubernetes
kind: module
doc: basics
slug: 10-namespaces-and-rbac
part: 2
tags: [namespace, rbac, role, rolebinding, serviceaccount, authorization, can-i]
versions: "Kubernetes 1.37.0 (kind v0.34), kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Namespace и RBAC: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [06 · Service и сеть](../06-services/overview.ru.md) · **Время:** ~3 ч

Все примеры выполнены в kind (Kubernetes 1.37.0), в namespace `learn-10a` и `learn-10b`.

## Зачем это нужно

В одном кластере живут разные приложения и команды. Без разделения имена конфликтуют, а любой может удалить чужое. Namespace даёт отдельное пространство имён и точку для лимитов, а RBAC определяет, кто и какие действия может выполнять.

## Namespace

### Что есть из коробки

```text
$ kubectl get ns
default  kube-node-lease  kube-public  kube-system  local-path-storage
```

- `default` — для объектов без явного namespace;
- `kube-system` — компоненты кластера ([модуль 02](../02-architecture/basics.ru.md#что-видно-в-kind));
- `kube-public`, `kube-node-lease` — служебные (публичные данные кластера и аренды heartbeat нод);
- `local-path-storage` — провайдер хранилища kind ([модуль 08](../08-storage/basics.ru.md)).

Свой namespace создаётся одной командой; у каждого автоматически есть метка с его именем:

```text
$ kubectl create ns learn-10a
$ kubectl get ns learn-10a --show-labels
learn-10a   Active   11s   kubernetes.io/metadata.name=learn-10a
```

### Область имён

Имя объекта уникально только внутри namespace. Одинаковые `ConfigMap` в двух namespace — разные объекты:

```text
$ kubectl -n learn-10a create configmap cfg --from-literal=ns=a
$ kubectl -n learn-10b create configmap cfg --from-literal=ns=b
$ kubectl get cm cfg -A
error: a resource cannot be retrieved by name across all namespaces
```

Искать по имени сразу во всех namespace нельзя: нужен `-n` или список (`kubectl get cm -A`). Вызов по DNS между namespace требует `имя.namespace` ([модуль 06](../06-services/expert.ru.md#подвох-короткое-имя-работает-только-в-своём-namespace)).

### Не всё живёт в namespace

В нашем кластере 34 типа ресурсов принадлежат namespace и 37 — кластерные:

```text
$ kubectl api-resources --namespaced=false --no-headers | awk '{print $1}' | head -14
componentstatuses namespaces nodes persistentvolumes mutatingadmissionpolicies mutatingadmissionpolicybindings mutatingwebhookconfigurations validatingadmissionpolicies validatingadmissionpolicybindings validatingwebhookconfigurations customresourcedefinitions apiservices selfsubjectreviews tokenreviews
```

Ноды, PersistentVolume ([модуль 08](../08-storage/basics.ru.md#под-который-использует-pvc)), сами namespace, CRD и другие не привязаны к namespace.

### Удаление namespace

Удаление namespace удаляет всё внутри него:

```text
$ kubectl -n learn-10b run tmp --image=alpine:3.22 --restart=Never -- sleep 3600
$ kubectl delete ns learn-10b --wait=true
namespace "learn-10b" deleted
$ kubectl get ns learn-10b
Error from server (NotFound): namespaces "learn-10b" not found
```

Именно поэтому в упражнениях трека всё складывается в `learn-NN` и убирается одной командой.

## RBAC: кто что может

### Субъекты, роли, привязки

| Понятие | Что это |
|---|---|
| Субъект | пользователь, группа или ServiceAccount |
| Role | набор разрешений в **одном namespace** (глагол + ресурс) |
| RoleBinding | привязывает Role к субъектам в namespace |

Права только разрешающие: всё, что не выдано, запрещено.

### ServiceAccount

Пользователи Kubernetes — внешняя сущность (сертификат, токен провайдера), а приложение в поде работает от **ServiceAccount**: объекта в namespace. В каждом namespace уже есть `default`:

```text
$ kubectl -n learn-10a get sa
NAME      AGE
default   47s
$ kubectl -n learn-10a create serviceaccount reader
```

### Role и RoleBinding

Разрешим читать поды и их логи:

```yaml
apiVersion: rbac.authorization.k8s.io/v1
kind: Role
metadata: {name: pod-reader}
rules:
  - apiGroups: [""]
    resources: ["pods", "pods/log"]
    verbs: ["get", "list", "watch"]
```

- `apiGroups: [""]` — основная (core) группа API (поды, сервисы, ConfigMap). Для Deployment нужна группа `apps`.
- `resources` — типы объектов; `pods/log` — подресурс.
- `verbs` — действия (`get`, `list`, `watch`, `create`, `update`, `patch`, `delete`).

```bash
kubectl -n learn-10a create rolebinding read-pods --role=pod-reader --serviceaccount=learn-10a:reader
```

### Проверка: kubectl auth can-i

`auth can-i` отвечает, разрешено ли действие; `--as` выполняет проверку от имени другого субъекта:

```text
$ SA=system:serviceaccount:learn-10a:reader
$ kubectl -n learn-10a auth can-i list pods --as=$SA          # до привязки
no
$ kubectl -n learn-10a create rolebinding read-pods --role=pod-reader --serviceaccount=learn-10a:reader
$ kubectl -n learn-10a auth can-i list pods --as=$SA          # после привязки
yes
$ kubectl -n learn-10a auth can-i get pods/log --as=$SA
yes
$ kubectl -n learn-10a auth can-i delete pods --as=$SA
no
$ kubectl -n learn-10a auth can-i list secrets --as=$SA
no
$ kubectl -n learn-10a auth can-i list pods -n default --as=$SA
no
```

Роль действует только в `learn-10a` (`list pods -n default` — `no`) и только на перечисленные ресурсы и глаголы. Список всех прав субъекта: `kubectl auth can-i --list --as=$SA`.

При отказе сообщение указывает, что именно запрещено:

```text
$ kubectl get pods --as=$SA -n default
Error from server (Forbidden): pods is forbidden: User "system:serviceaccount:learn-10a:reader" cannot list resource "pods" in API group "" in the namespace "default"
```

### Токен ServiceAccount в поде

Под, запущенный с `serviceAccountName: reader`, получает токен и сертификат кластера в каталоге `/var/run/secrets/kubernetes.io/serviceaccount` и с их помощью обращается к API server:

```text
$ kubectl -n learn-10a exec apiclient -- ls /var/run/secrets/kubernetes.io/serviceaccount
ca.crt
namespace
token
-- запросы из пода с токеном ServiceAccount reader --
list pods in own ns: 200
list pods in default: 403
list secrets own ns: 403
```

Поды в своём namespace читаются (`200`), чужой namespace и секреты — `403`: права ровно по Role. По умолчанию все поды получают токен своего ServiceAccount (`default`, если не указан другой); как отказаться — в [продвинутой теории](advanced.ru.md#токены-serviceaccount).

## Ошибки новичка

- ❌ Считать namespace границей безопасности → ✅ это граница имён и политик; для сети нужны NetworkPolicy, для запуска — Pod Security.
- ❌ Искать объект по имени сразу во всех namespace → ✅ `-n` или `-A` со списком.
- ❌ Выдавать права на `pods` и удивляться, что логи недоступны → ✅ `pods/log` — отдельный подресурс.
- ❌ Забывать `apiGroups` → ✅ для `deployments` нужна группа `apps`, для `pods` — пустая `""`.
- ❌ Запускать приложения от `default` ServiceAccount с расширенными правами → ✅ отдельный ServiceAccount и узкая роль.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — квоты, ClusterRole, встроенные роли, токены
- Предыдущий модуль: [09 · Ingress и Gateway API](../09-ingress-and-gateway-api/overview.ru.md)
- Авторизация в запросе: [02 · Архитектура Kubernetes](../02-architecture/advanced.ru.md#путь-запроса-через-api-server)
- Фундамент: [трек Linux](../../../linux/overview.ru.md)

## Источники

- [Namespaces](https://kubernetes.io/docs/concepts/overview/working-with-objects/namespaces/)
- [Using RBAC Authorization](https://kubernetes.io/docs/reference/access-authn-authz/rbac/)
- [Configure Service Accounts for Pods](https://kubernetes.io/docs/tasks/configure-pod-container/configure-service-account/)
- [Authorization overview](https://kubernetes.io/docs/reference/access-authn-authz/authorization/)
- [kubectl auth can-i](https://kubernetes.io/docs/reference/kubectl/generated/kubectl_auth/kubectl_auth_can-i/)
