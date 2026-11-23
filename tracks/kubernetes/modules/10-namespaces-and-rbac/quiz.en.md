---
title: "Quiz: Namespaces and RBAC"
lang: en
track: kubernetes
kind: module
doc: quiz
slug: 10-namespaces-and-rbac
part: 2
tags: [namespace, rbac, role, serviceaccount, resourcequota, limitrange]
status: draft
updated: 2026-11-23
---

# Quiz: Namespaces and RBAC

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What happens to objects when a namespace is deleted?

- A) They move to `default`
- B) They are deleted with the namespace
- C) They remain without a namespace
- D) They freeze

<details><summary>Answer</summary>

**B.** Deleting a namespace deletes everything inside (in the experiment the `tmp` pod vanished with `learn-10b`).
→ [Basics](basics.en.md#deleting-a-namespace)

</details>

## 2. 🟢 Which resource does not belong to a namespace?

- A) Pod
- B) ConfigMap
- C) PersistentVolume
- D) ServiceAccount

<details><summary>Answer</summary>

**C.** PVs, nodes, StorageClasses, the namespaces themselves and CRDs are cluster-scoped. A Pod, ConfigMap and ServiceAccount live in a namespace.
→ [Basics](basics.en.md#not-everything-lives-in-a-namespace)

</details>

## 3. 🟢 What does `kubectl auth can-i list pods --as=system:serviceaccount:learn-10a:reader` do?

- A) Grants the ServiceAccount a right
- B) Checks whether this subject can perform the action
- C) Creates a ServiceAccount
- D) Deletes a role

<details><summary>Answer</summary>

**B.** It is a rights check on behalf of another subject (impersonation); it changes nothing.
→ [Basics](basics.en.md#checking-kubectl-auth-can-i)

</details>

## 4. 🟢 A role allows `get/list/watch` on `pods`. Can pod logs be read?

- A) Yes, logs are part of `pods`
- B) No, the subresource `pods/log` is needed
- C) Yes, but only `get`
- D) No, logs are read only by an administrator

<details><summary>Answer</summary>

**B.** `pods/log` is a separate subresource and must be listed in `resources`.
→ [Basics](basics.en.md#role-and-rolebinding)

</details>

## 5. 🟡 A namespace has a ResourceQuota on CPU and memory but no LimitRange. What happens to `kubectl run` without resources?

- A) The pod is created
- B) A refusal: `must specify limits.cpu … requests.memory …`
- C) The pod is created and immediately deleted
- D) A default quota is applied

<details><summary>Answer</summary>

**B.** `failed quota: compute: must specify limits.cpu for: …`. A LimitRange with `default` and `defaultRequest` would substitute the values.
→ [Advanced](advanced.en.md#a-pod-without-requests-and-limits-is-rejected)

</details>

## 6. 🟡 A ServiceAccount got `view` through a RoleBinding in `learn-10a`. What is true?

- A) It sees Deployments in all namespaces
- B) It sees Deployments in `learn-10a` but not in `default`, and does not read Secrets
- C) It can change Deployments in `learn-10a`
- D) The rights apply only to pods

<details><summary>Answer</summary>

**B.** A ClusterRole via a RoleBinding acts in one namespace; `view` does not include Secrets (in the experiment `get secrets` — `no`).
→ [Advanced](advanced.en.md#clusterrole-and-bindings)

</details>

## 7. 🟡 Select all correct statements

- [ ] A) In RBAC you can create a deny rule
- [ ] B) With `automountServiceAccountToken: false` the pod has no token directory
- [ ] C) `--as=jane` gets the rights of the group `devs` only together with `--as-group=devs`
- [ ] D) A Deployment reports quota exhaustion with an error in `kubectl apply`

<details><summary>Answer</summary>

**B, C.** A is wrong: rules are only granting. D is wrong: the Deployment is created, and the cause is visible in the `ReplicaFailure` condition and the `FailedCreate` events.
→ [Advanced](advanced.en.md)

</details>

## 8. 🟡 Where do you look to see why a Deployment is `0/2` with no errors on `apply`?

- A) In the kube-proxy logs
- B) In the Deployment's `ReplicaFailure` condition and the `FailedCreate` events
- C) In `kubectl top`
- D) In `kubectl get nodes`

<details><summary>Answer</summary>

**B.** The message: `exceeded quota: compute, requested: requests.cpu=100m, used: requests.cpu=500m, limited: requests.cpu=500m`.
→ [Advanced](advanced.en.md#a-deployment-that-hits-the-quota-does-not-fail-but-hangs)

</details>

## 9. 🔴 Find the flaw

> "A developer with the right to create RoleBindings will give themselves any rights, since binding the role `cluster-admin` is a single command."

<details><summary>Answer</summary>

RBAC protects from privilege escalation: you cannot grant what the grantor does not have (`attempting to grant RBAC permissions not currently held`). In the experiment an attempt to bind `edit` for `rbadmin` was rejected on the first missing rule. It can be bypassed only with the rights `escalate` and `bind` — these must be granted with great care.
→ [Expert](expert.en.md#privilege-escalation-prevention)

</details>

## 10. 🔴 Open question

An application must read ConfigMaps and pod logs only in its own namespace. How do you set this up by the principle of least privilege and how do you check that a foreign namespace is closed?

<details><summary>Key points of a good answer</summary>

- A separate ServiceAccount for the application (not `default`).
- A Role with `resources: [configmaps, pods, pods/log]` and `verbs: [get, list, watch]` in its namespace.
- A RoleBinding to this ServiceAccount (not a ClusterRoleBinding).
- Check: `auth can-i list pods -n learn-10a --as=…` → `yes`; `-n default` → `no`; `list secrets` → `no`; a request from a pod with the token: `200` and `403`.
- If API access is not needed at all — `automountServiceAccountToken: false`.

</details>

→ [Basics](basics.en.md#a-serviceaccount-token-in-a-pod)
