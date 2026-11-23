---
title: "Namespaces and RBAC: basics"
lang: en
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

# Namespaces and RBAC: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [06 · Service and networking](../06-services/overview.en.md) · **Time:** ~3 h

All examples were run in kind (Kubernetes 1.37.0), in the namespaces `learn-10a` and `learn-10b`.

## Why it matters

Different applications and teams live in one cluster. Without separation names clash and anyone can delete someone else's things. A namespace gives a separate name space and a point for limits, and RBAC determines who may perform which actions.

## Namespace

### What is there out of the box

```text
$ kubectl get ns
default  kube-node-lease  kube-public  kube-system  local-path-storage
```

- `default` — for objects without an explicit namespace;
- `kube-system` — cluster components ([module 02](../02-architecture/basics.en.md#what-is-visible-in-kind));
- `kube-public`, `kube-node-lease` — service ones (public cluster data and node heartbeat leases);
- `local-path-storage` — kind's storage provisioner ([module 08](../08-storage/basics.en.md)).

Your own namespace is created with one command; each one automatically has a label with its name:

```text
$ kubectl create ns learn-10a
$ kubectl get ns learn-10a --show-labels
learn-10a   Active   11s   kubernetes.io/metadata.name=learn-10a
```

### The name scope

An object name is unique only inside a namespace. Equal `ConfigMap`s in two namespaces are different objects:

```text
$ kubectl -n learn-10a create configmap cfg --from-literal=ns=a
$ kubectl -n learn-10b create configmap cfg --from-literal=ns=b
$ kubectl get cm cfg -A
error: a resource cannot be retrieved by name across all namespaces
```

You cannot search by name across all namespaces at once: you need `-n` or a list (`kubectl get cm -A`). A DNS call between namespaces needs `name.namespace` ([module 06](../06-services/expert.en.md#gotcha-the-short-name-works-only-in-its-own-namespace)).

### Not everything lives in a namespace

In our cluster 34 resource types belong to a namespace and 37 are cluster-scoped:

```text
$ kubectl api-resources --namespaced=false --no-headers | awk '{print $1}' | head -14
componentstatuses namespaces nodes persistentvolumes mutatingadmissionpolicies mutatingadmissionpolicybindings mutatingwebhookconfigurations validatingadmissionpolicies validatingadmissionpolicybindings validatingwebhookconfigurations customresourcedefinitions apiservices selfsubjectreviews tokenreviews
```

Nodes, PersistentVolumes ([module 08](../08-storage/basics.en.md#a-pod-that-uses-a-pvc)), the namespaces themselves, CRDs and others are not bound to a namespace.

### Deleting a namespace

Deleting a namespace deletes everything inside it:

```text
$ kubectl -n learn-10b run tmp --image=alpine:3.22 --restart=Never -- sleep 3600
$ kubectl delete ns learn-10b --wait=true
namespace "learn-10b" deleted
$ kubectl get ns learn-10b
Error from server (NotFound): namespaces "learn-10b" not found
```

That is why in the track's exercises everything goes into `learn-NN` and is removed with one command.

## RBAC: who may do what

### Subjects, roles, bindings

| Concept | What it is |
|---|---|
| Subject | a user, a group or a ServiceAccount |
| Role | a set of permissions in **one namespace** (a verb + a resource) |
| RoleBinding | binds a Role to subjects in a namespace |

Permissions are only granting: everything not granted is forbidden.

### ServiceAccount

Kubernetes users are an external entity (a certificate, a provider token), while an application in a pod runs as a **ServiceAccount**: an object in a namespace. Each namespace already has `default`:

```text
$ kubectl -n learn-10a get sa
NAME      AGE
default   47s
$ kubectl -n learn-10a create serviceaccount reader
```

### Role and RoleBinding

Let us allow reading pods and their logs:

```yaml
apiVersion: rbac.authorization.k8s.io/v1
kind: Role
metadata: {name: pod-reader}
rules:
  - apiGroups: [""]
    resources: ["pods", "pods/log"]
    verbs: ["get", "list", "watch"]
```

- `apiGroups: [""]` — the core API group (pods, services, ConfigMaps). A Deployment needs the group `apps`.
- `resources` — object types; `pods/log` is a subresource.
- `verbs` — actions (`get`, `list`, `watch`, `create`, `update`, `patch`, `delete`).

```bash
kubectl -n learn-10a create rolebinding read-pods --role=pod-reader --serviceaccount=learn-10a:reader
```

### Checking: kubectl auth can-i

`auth can-i` answers whether an action is allowed; `--as` performs the check on behalf of another subject:

```text
$ SA=system:serviceaccount:learn-10a:reader
$ kubectl -n learn-10a auth can-i list pods --as=$SA          # before the binding
no
$ kubectl -n learn-10a create rolebinding read-pods --role=pod-reader --serviceaccount=learn-10a:reader
$ kubectl -n learn-10a auth can-i list pods --as=$SA          # after the binding
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

The role works only in `learn-10a` (`list pods -n default` — `no`) and only for the listed resources and verbs. The list of all of a subject's rights: `kubectl auth can-i --list --as=$SA`.

On a denial the message says exactly what is forbidden:

```text
$ kubectl get pods --as=$SA -n default
Error from server (Forbidden): pods is forbidden: User "system:serviceaccount:learn-10a:reader" cannot list resource "pods" in API group "" in the namespace "default"
```

### A ServiceAccount token in a pod

A pod started with `serviceAccountName: reader` gets a token and the cluster certificate in the directory `/var/run/secrets/kubernetes.io/serviceaccount` and uses them to call the API server:

```text
$ kubectl -n learn-10a exec apiclient -- ls /var/run/secrets/kubernetes.io/serviceaccount
ca.crt
namespace
token
-- requests from the pod with the ServiceAccount token reader --
list pods in own ns: 200
list pods in default: 403
list secrets own ns: 403
```

Pods in its own namespace are readable (`200`), a foreign namespace and secrets give `403`: the rights are exactly per the Role. By default every pod gets the token of its ServiceAccount (`default` if another is not set); how to opt out is in [advanced](advanced.en.md#serviceaccount-tokens).

## Beginner mistakes

- ❌ Considering a namespace a security boundary → ✅ it is a boundary of names and policies; networking needs NetworkPolicy, running pods needs Pod Security.
- ❌ Searching for an object by name across all namespaces at once → ✅ `-n` or `-A` with a list.
- ❌ Granting rights on `pods` and being surprised logs are unavailable → ✅ `pods/log` is a separate subresource.
- ❌ Forgetting `apiGroups` → ✅ `deployments` need the group `apps`, `pods` the empty `""`.
- ❌ Running applications as the `default` ServiceAccount with extended rights → ✅ a separate ServiceAccount and a narrow role.

## Related materials

- Next: [advanced](advanced.en.md) — quotas, ClusterRole, built-in roles, tokens
- Previous module: [09 · Ingress and Gateway API](../09-ingress-and-gateway-api/overview.en.md)
- Authorization in a request: [02 · Kubernetes architecture](../02-architecture/advanced.en.md#the-request-path-through-the-api-server)
- Foundation: the [Linux track](../../../linux/overview.en.md)

## Sources

- [Namespaces](https://kubernetes.io/docs/concepts/overview/working-with-objects/namespaces/)
- [Using RBAC Authorization](https://kubernetes.io/docs/reference/access-authn-authz/rbac/)
- [Configure Service Accounts for Pods](https://kubernetes.io/docs/tasks/configure-pod-container/configure-service-account/)
- [Authorization overview](https://kubernetes.io/docs/reference/access-authn-authz/authorization/)
- [kubectl auth can-i](https://kubernetes.io/docs/reference/kubectl/generated/kubectl_auth/kubectl_auth_can-i/)
