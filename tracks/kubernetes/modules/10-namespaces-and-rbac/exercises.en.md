---
title: "Exercises: Namespaces and RBAC"
lang: en
track: kubernetes
kind: module
doc: exercises
slug: 10-namespaces-and-rbac
part: 2
tags: [namespace, rbac, role, rolebinding, serviceaccount, resourcequota, limitrange]
status: draft
updated: 2026-11-23
---

# Exercises: Namespaces and RBAC

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. Solutions are under spoilers. You need a kind cluster. Clean up afterwards: `kubectl delete ns learn-10a learn-10b` and the cluster-scoped objects (`clusterrole node-reader`, `clusterrolebinding reader-nodes`).

## 1. 🟢 Namespaces and the name scope

**Task:** create the namespaces `learn-10a` and `learn-10b`, a ConfigMap `cfg` with a different value in each. Show that the objects are different and explain the error on `kubectl get cm cfg -A`. Delete `learn-10b` and make sure its ConfigMap is gone too.

**Expected result:** `error: a resource cannot be retrieved by name across all namespaces`; after the namespace is deleted its objects are gone.

<details><summary>Hint</summary>

Search by name in a specific namespace (`-n`). The list of all — `kubectl get cm -A`.

</details>

<details><summary>Solution</summary>

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

A name is unique inside a namespace; the API does not support searching by name across all namespaces at once, you need a list (`-A`) or `-n`. Deleting a namespace deletes everything inside it.

</details>

## 2. 🟢 Role, RoleBinding and checking rights

**Task:** create a ServiceAccount `reader`, a Role `pod-reader` (get, list, watch on `pods` and `pods/log`) and a RoleBinding. Check `can-i` before and after the binding for: `list pods`, `get pods/log`, `delete pods`, `list secrets`, `list pods -n default`.

**Expected result:** before the binding `no`; after — `yes`, `yes`, `no`, `no`, `no`.

<details><summary>Hint</summary>

The subject for `--as`: `system:serviceaccount:learn-10a:reader`.

</details>

<details><summary>Solution</summary>

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

The role works only in its own namespace and only on the granted resources and verbs; everything else is forbidden by default.

</details>

## 3. 🟡 A ServiceAccount token in a pod

**Task:** start a pod `apiclient` with `serviceAccountName: reader` (image `curlimages/curl`) and call the API with the token from the pod: list pods in `learn-10a`, in `default` and list Secrets in `learn-10a`. Then start a pod with `automountServiceAccountToken: false` and show there is no token.

**Expected result:** `200`, `403`, `403`; the second pod has no token directory.

<details><summary>Hint</summary>

The token is in `/var/run/secrets/kubernetes.io/serviceaccount/token`, the CA in `ca.crt`; the API address is `https://kubernetes.default.svc`.

</details>

<details><summary>Solution</summary>

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

The token gives rights exactly per the Role; with `automountServiceAccountToken: false` there is no token directory.

</details>

## 4. 🟡 ResourceQuota and LimitRange

**Task:** create a ResourceQuota (3 pods, `requests.cpu: 500m`, `requests.memory: 256Mi`, `limits.cpu: "1"`, `limits.memory: 512Mi`). Try `kubectl run` without resources. Add a LimitRange (`default` 200m/128Mi, `defaultRequest` 100m/64Mi, `max` 400m/200Mi) and repeat. Then create a pod with `limits.cpu: "1"`.

**Expected result:** a refusal with `must specify limits.cpu …`; after the LimitRange the pod is created with the substituted values; `maximum cpu usage per Container is 400m, but limit is 1`.

<details><summary>Hint</summary>

The substituted values are visible in `kubectl get pod … -o jsonpath='{.spec.containers[0].resources}'`; quota usage — `kubectl describe quota`.

</details>

<details><summary>Solution</summary>

```text
Error from server (Forbidden): pods "nolimits" is forbidden: failed quota: compute: must specify limits.cpu for: nolimits; limits.memory for: nolimits; requests.cpu for: nolimits; requests.memory for: nolimits
{"limits":{"cpu":"200m","memory":"128Mi"},"requests":{"cpu":"100m","memory":"64Mi"}}
Error from server (Forbidden): error when creating "STDIN": pods "toobig" is forbidden: maximum cpu usage per Container is 400m, but limit is 1
```

A quota with CPU and memory requires explicit values; a LimitRange substitutes them by default and sets bounds.

</details>

## 5. 🔴 Quota exhaustion, ClusterRole, groups and escalation prevention

**Task:** (a) bring the `requests.cpu` quota to its limit and show that a 2-replica Deployment stays `0/2`, find the cause in the conditions. (b) Give a ServiceAccount read access to nodes through a ClusterRole and ClusterRoleBinding. (c) Bind `view` to the group `devs` and check `--as=jane` with and without the group. (d) Give the ServiceAccount `rbadmin` the right to create RoleBindings and show that it cannot bind `edit`.

**Expected result:** (a) `ReplicaFailure=True (FailedCreate): … exceeded quota`; (b) `no` → `yes`; (c) `yes` with the group, `no` without; (d) `attempting to grant RBAC permissions not currently held`.

<details><summary>Hint</summary>

(a) `kubectl get deploy -o jsonpath` on `.status.conditions`. (c) `--as-group=devs`. (d) A Role with the verb `create` on `rolebindings` in the group `rbac.authorization.k8s.io`.

</details>

<details><summary>Solution</summary>

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

The quota is checked when a pod is created: the Deployment is created, and the ReplicaSet gets the error. A binding by group gives rights only to those in that group. You cannot grant rights the grantor lacks: the API rejects the binding on the first missing rule.

</details>
