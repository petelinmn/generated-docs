---
title: "Namespace и RBAC: продвинутая теория"
lang: ru
track: kubernetes
kind: module
doc: advanced
slug: 10-namespaces-and-rbac
part: 2
tags: [resourcequota, limitrange, clusterrole, clusterrolebinding, builtin-roles, serviceaccount-token, impersonation, rbac]
versions: "Kubernetes 1.37.0 (kind v0.34), kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Namespace и RBAC: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md): создание namespace, Role, RoleBinding и `auth can-i` не повторяются.

## ResourceQuota: лимиты namespace

ResourceQuota ограничивает суммарное потребление namespace:

```yaml
apiVersion: v1
kind: ResourceQuota
metadata: {name: compute}
spec:
  hard:
    pods: "3"
    requests.cpu: 500m
    requests.memory: 256Mi
    limits.cpu: "1"
    limits.memory: 512Mi
    configmaps: "5"
```

```text
$ kubectl -n learn-10a describe quota compute
Resource         Used  Hard
--------         ----  ----
configmaps       2     5
limits.cpu       0     1
limits.memory    0     512Mi
pods             0     3
requests.cpu     0     500m
requests.memory  0     256Mi
```

`configmaps: 2` уже занято: в namespace по умолчанию есть служебный `kube-root-ca.crt` и наш `cfg`.

### Под без requests и limits отклоняется

Если квота считает CPU и память, каждый контейнер обязан указать `requests` и `limits`:

```text
$ kubectl -n learn-10a run nolimits --image=alpine:3.22 --restart=Never -- sleep 3600
Error from server (Forbidden): pods "nolimits" is forbidden: failed quota: compute: must specify limits.cpu for: nolimits; limits.memory for: nolimits; requests.cpu for: nolimits; requests.memory for: nolimits
```

Чтобы разработчикам не приходилось писать значения в каждом манифесте, задают LimitRange.

## LimitRange: значения по умолчанию и границы

```yaml
apiVersion: v1
kind: LimitRange
metadata: {name: defaults}
spec:
  limits:
    - type: Container
      default: {cpu: 200m, memory: 128Mi}
      defaultRequest: {cpu: 100m, memory: 64Mi}
      max: {cpu: 400m, memory: 200Mi}
      min: {cpu: 50m, memory: 32Mi}
```

- `default` — подставляемые `limits`; `defaultRequest` — подставляемые `requests`;
- `max` и `min` — границы, которые нельзя нарушить.

Под без ресурсов теперь создаётся с подставленными значениями:

```text
$ kubectl -n learn-10a run p1 --image=alpine:3.22 --restart=Never -- sleep 3600
$ kubectl -n learn-10a get pod p1 -o jsonpath='{.spec.containers[0].resources}'
{"limits":{"cpu":"200m","memory":"128Mi"},"requests":{"cpu":"100m","memory":"64Mi"}}
$ kubectl -n learn-10a get pod p1 -o jsonpath='qos={.status.qosClass}'
qos=Burstable
```

Использование квоты обновилось:

```text
limits.cpu       200m   1
limits.memory    128Mi  512Mi
pods             1      3
requests.cpu     100m   500m
requests.memory  64Mi   256Mi
```

Значение выше `max` отклоняется:

```text
$ kubectl -n learn-10a apply -f toobig.yaml     # limits.cpu: "1"
Error from server (Forbidden): error when creating "STDIN": pods "toobig" is forbidden: maximum cpu usage per Container is 400m, but limit is 1
```

### Исчерпание квоты

Квота проверяется при создании объекта. Под с `requests.cpu: 400m` поместился (100m + 400m = 500m, предел), следующие отклоняются:

```text
$ kubectl -n learn-10a run q2 --image=alpine:3.22 --restart=Never -- sleep 3600
Error from server (Forbidden): pods "q2" is forbidden: exceeded quota: compute, requested: requests.cpu=100m, used: requests.cpu=500m, limited: requests.cpu=500m
```

Ошибка названа точно: что запрошено, сколько занято и каков предел.

### Деплой, упёршийся в квоту, не падает, а «зависает»

Deployment сам не может создать под: ошибку получает ReplicaSet, и Deployment лишь показывает `0/2`:

```text
$ kubectl -n learn-10a get deploy dep
dep   0/2   0     0     5s
$ kubectl -n learn-10a get deploy dep -o jsonpath='{range .status.conditions[*]}{.type}={.status} ({.reason}): {.message}{"\n"}{end}'
Progressing=True (NewReplicaSetCreated): Created new replica set "dep-64f8b8bfd7"
Available=False (MinimumReplicasUnavailable): Deployment does not have minimum availability.
ReplicaFailure=True (FailedCreate): pods "dep-64f8b8bfd7-fftch" is forbidden: exceeded quota: compute, requested: requests.cpu=100m, used: requests.cpu=500m, limited: requests.cpu=500m
```

Причина видна в условии `ReplicaFailure` и в событиях `FailedCreate`. Это же условие упоминалось в [модуле 05](../05-deployments/expert.ru.md#статус-deployment). Квоты также считают объекты: `pods`, `configmaps`, `persistentvolumeclaims`, `count/<ресурс>`.

> ⚠️ Проверить: области действия квоты (`scopes`, например `BestEffort`) и синтаксис `count/<ресурс>` не запускались в этом материале.

## ClusterRole и привязки

| Объект | Область |
|---|---|
| Role + RoleBinding | один namespace |
| ClusterRole + ClusterRoleBinding | весь кластер, включая кластерные ресурсы (ноды) и нересурсные URL |
| ClusterRole + RoleBinding | права ClusterRole, но только в namespace привязки |

Кластерный ресурс `nodes` ServiceAccount прочитать не мог, пока не выдали ClusterRole и ClusterRoleBinding:

```text
$ kubectl auth can-i list nodes --as=$SA
no
$ kubectl create clusterrole node-reader --verb=get,list --resource=nodes
$ kubectl create clusterrolebinding reader-nodes --clusterrole=node-reader --serviceaccount=learn-10a:reader
$ kubectl auth can-i list nodes --as=$SA
yes
```

Третий вариант полезен для переиспользования: встроенная ClusterRole `view` привязана RoleBinding-ом, и права действуют только в одном namespace:

```text
$ kubectl create rolebinding view-here --clusterrole=view --serviceaccount=learn-10a:reader -n learn-10a
$ kubectl -n learn-10a auth can-i list deployments --as=$SA
yes
$ kubectl -n default auth can-i list deployments --as=$SA
no
$ kubectl -n learn-10a auth can-i get secrets --as=$SA
no
```

`view` разрешает читать Deployment в `learn-10a`, но не в `default` и не даёт читать Secret.

## Встроенные ClusterRole

В кластере есть готовые роли (проверено: `cluster-admin`, `admin`, `edit`, `view`):

| Роль | Права |
|---|---|
| `cluster-admin` | всё: `apiGroups: *`, `resources: *`, `verbs: *`, и нересурсные URL |
| `admin` | почти всё в namespace, включая роли и привязки внутри него |
| `edit` | создание и изменение большинства объектов, без управления правами |
| `view` | только чтение (без Secret) |

```text
$ kubectl get clusterrole cluster-admin -o jsonpath='{.rules}'
[{"apiGroups":["*"],"resources":["*"],"verbs":["*"]},{"nonResourceURLs":["*"],"verbs":["*"]}]
$ kubectl get clusterrole view -o jsonpath='{.aggregationRule}'
{"clusterRoleSelectors":[{"matchLabels":{"rbac.authorization.k8s.io/aggregate-to-view":"true"}}]}
```

У `view` есть `aggregationRule`: роль собирается из всех ClusterRole с меткой `aggregate-to-view: "true"`. Так CRD добавляют свои права в стандартные роли.

## Группы и имперсонация

Пользователей как объектов в Kubernetes нет: имя и группы приходят из сертификата, токена или внешнего провайдера. Субъект в привязке можно задать группой:

```text
$ kubectl -n learn-10a auth can-i list pods --as=jane --as-group=devs
no
$ kubectl -n learn-10a create rolebinding devs-view --clusterrole=view --group=devs
$ kubectl -n learn-10a auth can-i list pods --as=jane --as-group=devs
yes
$ kubectl -n learn-10a auth can-i list pods --as=jane
no
```

Права получает группа `devs`: `jane` их имеет только вместе с группой. Флаги `--as` и `--as-group` — имперсонация: администратор проверяет права, не получая чужие учётные данные. Кто вы сами:

```text
$ kubectl auth whoami
Username                                            kubernetes-admin
Groups                                              [kubeadm:cluster-admins system:authenticated]
```

Администратор kind входит как `kubernetes-admin` из группы `kubeadm:cluster-admins`, которой привязан `cluster-admin`.

## Токены ServiceAccount

Токен в поде выдаёт kubelet как **проекция** (временный токен, привязанный к поду). Содержимое токена из пода `apiclient`:

```json
{"aud":["https://kubernetes.default.svc.cluster.local"],
 "exp":1826959948,"iat":1795423948,
 "iss":"https://kubernetes.default.svc.cluster.local",
 "kubernetes.io":{"namespace":"learn-10a",
   "node":{"name":"learn-control-plane"},
   "pod":{"name":"apiclient"},
   "serviceaccount":{"name":"reader"},"warnafter":1795427555},
 "sub":"system:serviceaccount:learn-10a:reader"}
```

Токен привязан к конкретному поду и ноде (в утверждениях видны `pod` и `node`), срок `exp` в этом кластере около года от `iat`, а kubelet обновляет его заранее (`warnafter` — через час). Объекта Secret с токеном в namespace нет: `kubectl get secrets` пуст.

> ⚠️ Проверить: что токен перестаёт действовать после удаления пода, и точные права встроенных ролей `admin` и `edit` даны по документации и не проверялись запуском.

Короткоживущий токен для внешних клиентов выдаёт команда:

```text
$ kubectl -n learn-10a create token reader --duration=10m
eyJhbGciOiJSUzI1NiIsImtpZCI6Il…
```

В токене `exp − iat = 600` секунд (в опыте 1795424540 − 1795423940).

### Отказ от токена

Поду, которому доступ к API не нужен, токен не нужен:

```yaml
spec:
  serviceAccountName: reader
  automountServiceAccountToken: false
```

```text
$ kubectl -n learn-10a exec noauto -- ls /var/run/secrets/kubernetes.io/serviceaccount
ls: /var/run/secrets/kubernetes.io/serviceaccount: No such file or directory
```

Каталога с токеном нет: компрометация такого пода не даёт доступа к API.

## Лучшие практики

- Namespace на команду или окружение; на каждый — ResourceQuota и LimitRange.
- Принцип наименьших привилегий: отдельный ServiceAccount на приложение, узкая Role, никаких `*` в `verbs` и `resources`.
- Группы вместо перечисления пользователей; роли описывайте в Git.
- `automountServiceAccountToken: false` для подов, не работающих с API.
- Регулярно проверяйте `kubectl auth can-i --list --as=…` и привязки к `cluster-admin`.
- Помните: право `create pods` равносильно доступу ко всем Secret namespace (модуль 07): под можно смонтировать любой Secret.

## Частые ошибки

- ❌ Выдавать `cluster-admin` приложению «чтобы заработало» → ✅ найти недостающий глагол и ресурс по сообщению `Forbidden`.
- ❌ Ставить квоту на CPU без LimitRange → ✅ поды без явных значений будут отклоняться.
- ❌ Ждать, что Deployment сообщит об исчерпании квоты ошибкой → ✅ смотреть `ReplicaFailure` и события `FailedCreate`.
- ❌ Привязывать ClusterRoleBinding там, где достаточно RoleBinding → ✅ минимальная область.
- ❌ Считать, что `view` показывает Secret → ✅ `view` без Secret (проверено: `get secrets` — `no`).

## Сравнение с другими треками

- [Linux](../../../linux/overview.ru.md): RBAC — аналог групп и `sudoers`, но для объектов API; принцип наименьших привилегий тот же.
- [C#](../../../csharp/recipes/jwt-auth/overview.ru.md): токен ServiceAccount — тоже JWT с утверждениями (`sub`, `aud`, `exp`), которые проверяет сервер.

## Связанные материалы

- Предыдущий уровень: [базовая теория](basics.ru.md)
- Дальше: [экспертная теория](expert.ru.md)
- Предыдущие модули: [07 · ConfigMap и Secret](../07-config-and-secrets/overview.ru.md), [05 · Deployment](../05-deployments/overview.ru.md)

## Источники

- [Resource Quotas](https://kubernetes.io/docs/concepts/policy/resource-quotas/)
- [Limit Ranges](https://kubernetes.io/docs/concepts/policy/limit-range/)
- [Using RBAC Authorization: ClusterRole и привязки](https://kubernetes.io/docs/reference/access-authn-authz/rbac/#role-and-clusterrole)
- [Default roles and role bindings](https://kubernetes.io/docs/reference/access-authn-authz/rbac/#default-roles-and-role-bindings)
- [Managing Service Accounts](https://kubernetes.io/docs/reference/access-authn-authz/service-accounts-admin/)
- [User impersonation](https://kubernetes.io/docs/reference/access-authn-authz/authentication/#user-impersonation)
