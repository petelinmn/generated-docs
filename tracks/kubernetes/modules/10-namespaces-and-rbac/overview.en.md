---
title: "Namespaces and RBAC"
lang: en
track: kubernetes
kind: module
doc: overview
slug: 10-namespaces-and-rbac
part: 2
levels: [1, 2, 3, 4]
tags: [namespace, rbac, role, rolebinding, clusterrole, serviceaccount, resourcequota, limitrange, authorization]
versions: "Kubernetes 1.37.0 (kind v0.34), kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Namespaces and RBAC

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

One cluster is usually shared between teams and environments. A **namespace** separates objects into "folders": names are unique only inside a folder, and limits can be attached to each folder. **RBAC** (Role-Based Access Control) answers the question "who may do what": permissions are described by roles and granted by bindings to users, groups and ServiceAccounts — accounts of applications in the cluster.

## Key ideas

- **A namespace is a boundary of names and policies,** but not hard network or hardware isolation: pods of different namespaces see each other over the network by default.
- **Some resources do not belong to a namespace** (nodes, PersistentVolume, StorageClass, namespaces themselves): they are "cluster-scoped".
- **RBAC works on the principle "everything not allowed is forbidden".** Permissions are only added; there are no deny rules.
- **A Role + RoleBinding act in one namespace,** a ClusterRole + ClusterRoleBinding — across the cluster. A ClusterRole can be bound with a RoleBinding and limited to one namespace.
- **A ServiceAccount is a pod's identity.** A ServiceAccount token is mounted into the pod automatically, and the application calls the API server with it.
- **A ResourceQuota limits the total consumption of a namespace,** a LimitRange sets defaults and bounds for a single container.
- **A quota without requests and limits rejects pods:** a pod without explicit values is rejected until a LimitRange substitutes them by default.

## Minimal example

```bash
kubectl create serviceaccount reader -n learn-10a
kubectl create role pod-reader --verb=get,list,watch --resource=pods -n learn-10a
kubectl create rolebinding read-pods --role=pod-reader --serviceaccount=learn-10a:reader -n learn-10a
```

```text
$ kubectl -n learn-10a auth can-i list pods --as=system:serviceaccount:learn-10a:reader
yes
$ kubectl -n learn-10a auth can-i delete pods --as=system:serviceaccount:learn-10a:reader
no
```

## When to use and when not

- ✅ A namespace per team, environment or application; permissions and quotas at the namespace level.
- ✅ A separate ServiceAccount per application with the minimum necessary rights.
- ❌ A namespace does not replace security isolation: you need NetworkPolicy for networking and Pod Security for running pods (module 17).
- ❌ Do not give `cluster-admin` to applications and people "just in case".
- ❌ Do not use the `default` ServiceAccount for applications that need API access: give them a separate one and a narrow role.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | namespaces: creation, scope, deletion; Role, RoleBinding, ServiceAccount, `auth can-i`, an API request from a pod |
| [2. Advanced](advanced.en.md) | ResourceQuota and LimitRange, ClusterRole and bindings, built-in roles, groups, ServiceAccount tokens |
| [3. Expert](expert.en.md) | how authorization works, a reference of RBAC objects, privilege escalation prevention, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) with solutions |
| 5. Runnable examples | not generated yet — `/learn kubernetes 10-namespaces-and-rbac level 5` |
| 6. Tests | not generated yet — `/learn kubernetes 10-namespaces-and-rbac level 6` |

## Related materials

- Prerequisites: [06 · Service and networking](../06-services/overview.en.md), [07 · ConfigMap and Secret](../07-config-and-secrets/overview.en.md) (access to Secrets)
- Next: [11 · Probes and resources](../11-probes-and-resources/overview.en.md)
- The request path and authorization: [02 · Kubernetes architecture](../02-architecture/advanced.en.md#the-request-path-through-the-api-server)
- Foundation: the [Linux track](../../../linux/overview.en.md) — users, groups, permissions
- Pod and network security: [17 · Security](../17-security/overview.en.md)
- CRDs and operators: [19 · CRDs and operators](../19-crd-and-operators/overview.en.md)
- Troubleshooting: [21 · Troubleshooting](../21-troubleshooting/overview.en.md)
