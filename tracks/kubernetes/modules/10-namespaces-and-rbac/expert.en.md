---
title: "Namespaces and RBAC: expert"
lang: en
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

# Namespaces and RBAC: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## Under the hood

### How the "allowed or not" decision is made

A request passes the stages [authentication → authorization → admission](../02-architecture/advanced.en.md#the-request-path-through-the-api-server). The authorization mode in our cluster is `Node,RBAC` (the `--authorization-mode` flag of `kube-apiserver`): the modules are checked in turn, and the request passes if one of them allowed it. The `Node` module handles kubelets, `RBAC` everything else.

RBAC computes the permission like this: it collects all bindings in which the request's subject (the user, their groups, a ServiceAccount) appears among `subjects`, takes the roles of those bindings and looks in their `rules` for a rule matching the request by `apiGroups`, `resources` (and subresources), `verbs` and, if needed, `resourceNames`. A match — allowed, none — a refusal (`Forbidden`). There are no deny rules, so the order and "priority" of roles do not matter: the result is the union of all granted rights.

### Identity: a user, groups, a ServiceAccount

- **There are no users and groups as objects.** They are set by the authentication method: a client certificate (CN is the name, O is a group), an external provider's token or a ServiceAccount token.
- A **ServiceAccount** has a name like `system:serviceaccount:<namespace>:<name>` and automatically belongs to the groups `system:serviceaccounts` and `system:serviceaccounts:<namespace>` (visible in the escalation message below).
- The kind admin from `kubectl auth whoami`: `kubernetes-admin` in the group `kubeadm:cluster-admins`; the built-in groups `system:authenticated`, `system:masters` and others.

### Privilege escalation prevention

RBAC does not let you grant more than the grantor has. A subject with the right to create RoleBindings (but without `edit` rights) cannot bind `edit`:

```text
$ kubectl -n learn-10a create rolebinding evil --clusterrole=edit --serviceaccount=learn-10a:rbadmin \
    --as=system:serviceaccount:learn-10a:rbadmin
error: failed to create rolebinding: rolebindings.rbac.authorization.k8s.io "evil" is forbidden: user "system:serviceaccount:learn-10a:rbadmin" (groups=["system:serviceaccounts" "system:serviceaccounts:learn-10a" "system:authenticated"]) is attempting to grant RBAC permissions not currently held:
{APIGroups:[""], Resources:["bindings"], Verbs:["get" "list" "watch"]}
```

The server compared the rights of the `edit` role with those of `rbadmin` and rejected the binding on the first missing rule. It can be bypassed only with the rights `escalate` (on roles) and `bind` (on bindings) — these are granted with great care.

> ⚠️ Verify: the verbs `escalate` and `bind` and their role in bypassing the protection are given from documentation and were not run in this material.

### Who has cluster-admin

```text
$ kubectl get clusterrolebinding -o json | … (bindings with roleRef cluster-admin)
cluster-admin           ['Group:system:masters']
kubeadm:cluster-admins  ['Group:kubeadm:cluster-admins']
```

In our cluster `cluster-admin` is bound to the groups `system:masters` and `kubeadm:cluster-admins`. Anyone who gets into these groups (for example, through a client certificate with `O=system:masters`) has full access to the whole cluster.

### ServiceAccount tokens

Modern tokens are **bound**: a signed JWT with the claims `aud`, `iss`, `sub`, `exp`, `iat` and a binding to objects (`kubernetes.io`: `pod`, `node`, `serviceaccount`). The kubelet requests it from the API server (TokenRequest), mounts it as a projection and refreshes it. Old non-expiring tokens in a Secret are no longer created automatically: there is no Secret in `learn-10a`. A token for external use is issued by `kubectl create token` (the lifetime is set with the `--duration` flag).

## Reference

### Role/ClusterRole rule fields

| Field | Meaning |
|---|---|
| `apiGroups` | API groups: `""` — core, `apps`, `batch`, `rbac.authorization.k8s.io` and others; `*` — all |
| `resources` | object types and subresources (`pods`, `pods/log`, `deployments/scale`); `*` — all |
| `verbs` | `get`, `list`, `watch`, `create`, `update`, `patch`, `delete`, `deletecollection`; additionally `escalate`, `bind`, `impersonate` |
| `resourceNames` | restriction to specific object names |
| `nonResourceURLs` | URLs outside resources (`/healthz`, `/metrics`), only in a ClusterRole |

> ⚠️ Verify: `resourceNames` and `nonResourceURLs` in custom roles were not run; `nonResourceURLs` is visible in the `cluster-admin` rules (`"*"`).

### Bindings

| Object | Fields |
|---|---|
| RoleBinding | `roleRef` (a Role or ClusterRole, immutable), `subjects`, `metadata.namespace` |
| ClusterRoleBinding | `roleRef` (a ClusterRole), `subjects` |
| `subjects[].kind` | `User`, `Group`, `ServiceAccount` |

`roleRef` cannot be changed after creation: the binding is recreated.

### Commands

| Command | What it does |
|---|---|
| `kubectl auth can-i <verb> <resource> [--as=… --as-group=… -n …]` | check a right |
| `kubectl auth can-i --list` | all of a subject's rights in a namespace |
| `kubectl auth whoami` | who you are from the server's point of view |
| `kubectl create role/clusterrole … --verb=… --resource=…` | create a role |
| `kubectl create rolebinding/clusterrolebinding … --serviceaccount=ns:sa` | bind |
| `kubectl create token <sa> --duration=…` | issue a token |

### Namespace objects worth remembering

| Object | Purpose | Verified |
|---|---|---|
| ResourceQuota | total limits of resources and object counts | yes |
| LimitRange | defaults and bounds for a container | yes |
| ServiceAccount | a pod's identity | yes |
| Role, RoleBinding | rights inside a namespace | yes |
| NetworkPolicy | network isolation (module 17) | no |

## Gotchas

### Gotcha: a namespace does not isolate the network and rights by itself

Pods of different namespaces talk to each other over the network by default (in [module 06](../06-services/expert.en.md#gotcha-the-short-name-works-only-in-its-own-namespace) a pod from `learn-06b` reached a Service in `learn-06`). Rights do not appear on their own either: access can be separated only by RBAC and NetworkPolicy.

### Gotcha: you cannot search for an object by name in all namespaces

`kubectl get cm cfg -A` → `error: a resource cannot be retrieved by name across all namespaces`.

### Gotcha: a quota without a LimitRange rejects pods without resources

`failed quota: compute: must specify limits.cpu for: …; requests.memory for: …`. After a LimitRange the values are substituted automatically.

### Gotcha: a Deployment hit the quota and stays silent

`0/2` and `ReplicaFailure=True (FailedCreate)`: the error is in the condition and events, not in the `kubectl apply` output.

### Gotcha: a quota makes the "last pod" unpredictable

A 400m pod fit, but the next 100m ones no longer did (the limit is 500m): the check happens at creation, not in the scheduler, and does not depend on nodes.

### Gotcha: `--as` does not check the subject's existence

`kubectl auth can-i list pods --as=jane` answers `no` although there is no user `jane` anywhere: impersonation does not require a user object.

### Gotcha: you can grant rights to a non-existent resource or group

A binding to the group `devs` was created without errors (there are no group objects in Kubernetes). A typo in a group name or `apiGroups` gives a silently useless binding.

### Gotcha: a ClusterRole via a RoleBinding acts only in the namespace

`view` through a RoleBinding allowed `list deployments` in `learn-10a`, but not in `default`.

### Gotcha: `create pods` = access to Secrets

The right to create pods lets you mount any Secret of that namespace ([module 07](../07-config-and-secrets/expert.en.md#access-rbac)).

### Gotcha: every pod has a token by default

Without `automountServiceAccountToken: false` the `default` ServiceAccount's token is mounted into every pod (in the experiment `ca.crt`, `namespace`, `token`).

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| A namespace per team | simple rights and quotas | many objects, a shared cluster |
| A separate cluster per team | strong isolation | cost and operational complexity |
| Virtual clusters (vcluster) | isolation between strong and weak | an extra component |
| RBAC | built in, declarative | no denials, hard to audit |
| ABAC, external authorization (webhook) | flexibility | complexity, dependence on an external system |
| OPA Gatekeeper / Kyverno | policies on top of RBAC (what may be created) | a separate component (module 17) |

> ⚠️ Verify: vcluster, OPA Gatekeeper, Kyverno and webhook authorization are described from documentation and were not run.

## Interview questions

**1. What is a namespace and what does it isolate?**

<details><summary>Answer</summary>

A logical name space inside a cluster: objects are unique by name only inside it, and quotas and rights are attached to it. It does not isolate the network and security by itself: pods of different namespaces see each other by default.

</details>

**2. Which resources do not belong to a namespace?**

<details><summary>Answer</summary>

Cluster-scoped ones: nodes, PersistentVolume, StorageClass, the namespaces themselves, CRDs, ClusterRole and others. In our cluster there are 34 namespaced types and 37 cluster-scoped.

</details>

**3. How does a Role differ from a ClusterRole and a RoleBinding from a ClusterRoleBinding?**

<details><summary>Answer</summary>

A Role and RoleBinding act in one namespace, a ClusterRole and ClusterRoleBinding across the whole cluster (including cluster-scoped resources). A ClusterRole bound with a RoleBinding is limited to one namespace.

</details>

**4. (Trick) Can you forbid an action in RBAC?**

<details><summary>Answer</summary>

No: the rules are only granting. Everything not granted is forbidden. The resulting rights are the union of all of a subject's bindings.

</details>

**5. What is a ServiceAccount and how does a pod get its token?**

<details><summary>Answer</summary>

A pod's identity in a namespace. The kubelet projects into the pod a short-lived token bound to the pod, in `/var/run/secrets/kubernetes.io/serviceaccount` together with `ca.crt` and `namespace`. You can opt out: `automountServiceAccountToken: false`.

</details>

**6. (Trick) A subject can create RoleBindings. Can they give themselves `cluster-admin`?**

<details><summary>Answer</summary>

No: the API rejects granting rights the grantor does not have (`attempting to grant RBAC permissions not currently held`). A bypass is possible only through the rights `escalate` and `bind`.

</details>

**7. How do you check what a ServiceAccount can do without having its token?**

<details><summary>Answer</summary>

`kubectl auth can-i <action> --as=system:serviceaccount:<ns>:<sa>`, the list — `--list`. Impersonation requires the `impersonate` right from the checker.

</details>

**8. How does a ResourceQuota differ from a LimitRange?**

<details><summary>Answer</summary>

A ResourceQuota limits the total consumption of a namespace (CPU, memory, the number of objects). A LimitRange sets defaults and bounds for an individual container or pod. Together: a CPU quota requires explicit values, a LimitRange substitutes them.

</details>

**9. (Trick) A Deployment shows `0/2`, and there were no errors on `apply`. What do you check?**

<details><summary>Answer</summary>

The `ReplicaFailure` condition of the Deployment and the `FailedCreate` events of the ReplicaSet: the cause may be an exhausted quota (`exceeded quota: compute, requested: requests.cpu=100m, used: … limited: …`) or a LimitRange violation.

</details>

**10. How do you restrict which namespaces an application can read?**

<details><summary>Answer</summary>

Grant a Role in the needed namespace and bind it with a RoleBinding to the application's ServiceAccount; do not grant a ClusterRoleBinding. Check: `auth can-i list pods -n other --as=…` returns `no`.

</details>

**11. What does `view` give and can you read Secrets with it?**

<details><summary>Answer</summary>

Reading most objects of a namespace; Secrets are not included (in the experiment `auth can-i get secrets` — `no`). `view` is assembled by aggregating ClusterRoles labeled `aggregate-to-view`.

</details>

**12. Why is the right `create pods` dangerous?**

<details><summary>Answer</summary>

A pod can mount any Secret of its namespace and use any ServiceAccount, so whoever creates pods effectively gets access to the secrets and rights of these accounts. Pod Security and RBAC restrictions are needed.

</details>

## Cheat sheet

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

## Related materials

- Previous levels: [basics](basics.en.md), [advanced](advanced.en.md)
- Previous modules: [07 · ConfigMap and Secret](../07-config-and-secrets/overview.en.md), [02 · Architecture](../02-architecture/overview.en.md)
- Foundation: the [Linux track](../../../linux/overview.en.md)

## Sources

- [Using RBAC Authorization](https://kubernetes.io/docs/reference/access-authn-authz/rbac/)
- [Privilege escalation prevention and bootstrapping](https://kubernetes.io/docs/reference/access-authn-authz/rbac/#privilege-escalation-prevention-and-bootstrapping)
- [Service Accounts](https://kubernetes.io/docs/concepts/security/service-accounts/)
- [Authenticating](https://kubernetes.io/docs/reference/access-authn-authz/authentication/)
- [Resource Quotas](https://kubernetes.io/docs/concepts/policy/resource-quotas/)
- [Limit Ranges](https://kubernetes.io/docs/concepts/policy/limit-range/)
