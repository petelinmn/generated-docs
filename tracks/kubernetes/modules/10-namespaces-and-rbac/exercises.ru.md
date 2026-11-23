---
title: "Упражнения: Namespace и RBAC"
lang: ru
track: kubernetes
kind: module
doc: exercises
slug: 10-namespaces-and-rbac
part: 2
tags: [namespace, rbac, role, rolebinding, serviceaccount, resourcequota, limitrange]
status: draft
updated: 2026-11-23
---

# Упражнения: Namespace и RBAC

🌐 **Русский** · [English](exercises.en.md)

> Реши сам, прежде чем открывать подсказки. Решения — под спойлерами. Нужен кластер kind. Убрать за собой: `kubectl delete ns learn-10a learn-10b` и кластерные объекты (`clusterrole node-reader`, `clusterrolebinding reader-nodes`).

## 1. 🟢 Namespace и область имён

**Задача:** создай namespace `learn-10a` и `learn-10b`, в каждом ConfigMap `cfg` с разным значением. Покажи, что объекты разные, и объясни ошибку при `kubectl get cm cfg -A`. Удали `learn-10b` и убедись, что исчез и его ConfigMap.

**Ожидаемый результат:** `error: a resource cannot be retrieved by name across all namespaces`; после удаления namespace его объекты исчезли.

<details><summary>Подсказка</summary>

Искать по имени нужно в конкретном namespace (`-n`). Список по всем — `kubectl get cm -A`.

</details>

<details><summary>Решение</summary>

```bash
kubectl create ns learn-10a; kubectl create ns learn-10b
kubectl -n learn-10a create configmap cfg --from-literal=ns=a
kubectl -n learn-10b create configmap cfg --from-literal=ns=b
kubectl get cm cfg -A
kubectl get cm -A --field-selector metadata.name=cfg
kubectl delete ns learn-10b
```

```text
error: a resource cannot be retrieved by name across all namespaces
```

Имя уникально внутри namespace; поиск по имени сразу во всех namespace API не поддерживает, нужен список (`-A`) или `-n`. Удаление namespace удаляет всё внутри него.

</details>

## 2. 🟢 Role, RoleBinding и проверка прав

**Задача:** создай ServiceAccount `reader`, Role `pod-reader` (get, list, watch на `pods` и `pods/log`) и RoleBinding. Проверь `can-i` до и после привязки для: `list pods`, `get pods/log`, `delete pods`, `list secrets`, `list pods -n default`.

**Ожидаемый результат:** до привязки `no`; после — `yes`, `yes`, `no`, `no`, `no`.

<details><summary>Подсказка</summary>

Субъект для `--as`: `system:serviceaccount:learn-10a:reader`.

</details>

<details><summary>Решение</summary>

```bash
kubectl -n learn-10a create serviceaccount reader
kubectl -n learn-10a create role pod-reader --verb=get,list,watch --resource=pods,pods/log
kubectl -n learn-10a create rolebinding read-pods --role=pod-reader --serviceaccount=learn-10a:reader
SA=system:serviceaccount:learn-10a:reader
for v in "list pods" "get pods/log" "delete pods" "list secrets" "list pods -n default"; do
  printf "%s -> " "$v"; kubectl -n learn-10a auth can-i $v --as=$SA
done
```

```text
list pods -> yes
get pods/log -> yes
delete pods -> no
list secrets -> no
list pods -n default -> no
```

Роль действует только в своём namespace и только на выданные ресурсы и глаголы; остальное запрещено по умолчанию.

</details>

## 3. 🟡 Токен ServiceAccount в поде

**Задача:** запусти под `apiclient` с `serviceAccountName: reader` (образ `curlimages/curl`) и вызови API с токеном из пода: список подов в `learn-10a`, в `default` и список Secret в `learn-10a`. Затем запусти под с `automountServiceAccountToken: false` и покажи, что токена нет.

**Ожидаемый результат:** `200`, `403`, `403`; у второго пода нет каталога токена.

<details><summary>Подсказка</summary>

Токен лежит в `/var/run/secrets/kubernetes.io/serviceaccount/token`, CA — в `ca.crt`; адрес API — `https://kubernetes.default.svc`.

</details>

<details><summary>Решение</summary>

```bash
CALL='T=$(cat /var/run/secrets/kubernetes.io/serviceaccount/token); CA=/var/run/secrets/kubernetes.io/serviceaccount/ca.crt; curl -s --cacert $CA -H "Authorization: Bearer $T" -o /dev/null -w "%{http_code}\n"'
kubectl -n learn-10a exec apiclient -- sh -c "$CALL https://kubernetes.default.svc/api/v1/namespaces/learn-10a/pods"
kubectl -n learn-10a exec apiclient -- sh -c "$CALL https://kubernetes.default.svc/api/v1/namespaces/default/pods"
kubectl -n learn-10a exec apiclient -- sh -c "$CALL https://kubernetes.default.svc/api/v1/namespaces/learn-10a/secrets"
kubectl -n learn-10a exec noauto -- ls /var/run/secrets/kubernetes.io/serviceaccount
```

```text
200
403
403
ls: /var/run/secrets/kubernetes.io/serviceaccount: No such file or directory
```

Токен даёт права ровно по Role; при `automountServiceAccountToken: false` каталога с токеном нет.

</details>

## 4. 🟡 ResourceQuota и LimitRange

**Задача:** создай ResourceQuota (3 пода, `requests.cpu: 500m`, `requests.memory: 256Mi`, `limits.cpu: "1"`, `limits.memory: 512Mi`). Попробуй `kubectl run` без ресурсов. Добавь LimitRange (`default` 200m/128Mi, `defaultRequest` 100m/64Mi, `max` 400m/200Mi) и повтори. Затем создай под с `limits.cpu: "1"`.

**Ожидаемый результат:** отказ с `must specify limits.cpu …`; после LimitRange под создан с подставленными значениями; `maximum cpu usage per Container is 400m, but limit is 1`.

<details><summary>Подсказка</summary>

Подставленные значения видно в `kubectl get pod … -o jsonpath='{.spec.containers[0].resources}'`; использование квоты — `kubectl describe quota`.

</details>

<details><summary>Решение</summary>

```text
Error from server (Forbidden): pods "nolimits" is forbidden: failed quota: compute: must specify limits.cpu for: nolimits; limits.memory for: nolimits; requests.cpu for: nolimits; requests.memory for: nolimits
{"limits":{"cpu":"200m","memory":"128Mi"},"requests":{"cpu":"100m","memory":"64Mi"}}
Error from server (Forbidden): error when creating "STDIN": pods "toobig" is forbidden: maximum cpu usage per Container is 400m, but limit is 1
```

Квота с CPU и памятью требует явных значений; LimitRange подставляет их по умолчанию и задаёт границы.

</details>

## 5. 🔴 Исчерпание квоты, ClusterRole, группы и защита от эскалации

**Задача:** (а) доведи квоту по `requests.cpu` до предела и покажи, что Deployment на 2 реплики остаётся `0/2`, найди причину в условиях. (б) Дай ServiceAccount чтение нод через ClusterRole и ClusterRoleBinding. (в) Привяжи `view` группе `devs` и проверь `--as=jane` с группой и без. (г) Дай ServiceAccount `rbadmin` право создавать RoleBinding и покажи, что он не может привязать `edit`.

**Ожидаемый результат:** (а) `ReplicaFailure=True (FailedCreate): … exceeded quota`; (б) `no` → `yes`; (в) `yes` с группой, `no` без; (г) `attempting to grant RBAC permissions not currently held`.

<details><summary>Подсказка</summary>

(а) `kubectl get deploy -o jsonpath` по `.status.conditions`. (в) `--as-group=devs`. (г) Role с глаголом `create` на `rolebindings` в группе `rbac.authorization.k8s.io`.

</details>

<details><summary>Решение</summary>

```text
ReplicaFailure=True (FailedCreate): pods "dep-64f8b8bfd7-fftch" is forbidden: exceeded quota: compute, requested: requests.cpu=100m, used: requests.cpu=500m, limited: requests.cpu=500m

$ kubectl auth can-i list nodes --as=$SA
no
$ kubectl create clusterrole node-reader --verb=get,list --resource=nodes
$ kubectl create clusterrolebinding reader-nodes --clusterrole=node-reader --serviceaccount=learn-10a:reader
$ kubectl auth can-i list nodes --as=$SA
yes

$ kubectl -n learn-10a create rolebinding devs-view --clusterrole=view --group=devs
$ kubectl -n learn-10a auth can-i list pods --as=jane --as-group=devs
yes
$ kubectl -n learn-10a auth can-i list pods --as=jane
no

$ kubectl -n learn-10a create rolebinding evil --clusterrole=edit --serviceaccount=learn-10a:rbadmin --as=system:serviceaccount:learn-10a:rbadmin
error: failed to create rolebinding: … is attempting to grant RBAC permissions not currently held:
{APIGroups:[""], Resources:["bindings"], Verbs:["get" "list" "watch"]}
```

Квота проверяется при создании пода: Deployment создаётся, а ошибку получает ReplicaSet. Привязка по группе даёт права только тем, кто в этой группе. Нельзя выдать права, которых нет у выдающего: API отклоняет привязку на первом недостающем правиле.

</details>
