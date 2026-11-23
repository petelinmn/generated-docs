---
title: "Namespaces and RBAC: advanced"
lang: en
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

# Namespaces and RBAC: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on [basics](basics.en.md): creating namespaces, Roles, RoleBindings and `auth can-i` are not repeated.

## ResourceQuota: namespace limits

A ResourceQuota limits the total consumption of a namespace:

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

`configmaps: 2` is already used: a namespace has the service `kube-root-ca.crt` by default, plus our `cfg`.

### A pod without requests and limits is rejected

If a quota counts CPU and memory, every container must specify `requests` and `limits`:

```text
$ kubectl -n learn-10a run nolimits --image=alpine:3.22 --restart=Never -- sleep 3600
Error from server (Forbidden): pods "nolimits" is forbidden: failed quota: compute: must specify limits.cpu for: nolimits; limits.memory for: nolimits; requests.cpu for: nolimits; requests.memory for: nolimits
```

So that developers do not have to write values in every manifest, a LimitRange is set.

## LimitRange: defaults and bounds

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

- `default` — the substituted `limits`; `defaultRequest` — the substituted `requests`;
- `max` and `min` — bounds that must not be violated.

A pod without resources is now created with the substituted values:

```text
$ kubectl -n learn-10a run p1 --image=alpine:3.22 --restart=Never -- sleep 3600
$ kubectl -n learn-10a get pod p1 -o jsonpath='{.spec.containers[0].resources}'
{"limits":{"cpu":"200m","memory":"128Mi"},"requests":{"cpu":"100m","memory":"64Mi"}}
$ kubectl -n learn-10a get pod p1 -o jsonpath='qos={.status.qosClass}'
qos=Burstable
```

The quota usage was updated:

```text
limits.cpu       200m   1
limits.memory    128Mi  512Mi
pods             1      3
requests.cpu     100m   500m
requests.memory  64Mi   256Mi
```

A value above `max` is rejected:

```text
$ kubectl -n learn-10a apply -f toobig.yaml     # limits.cpu: "1"
Error from server (Forbidden): error when creating "STDIN": pods "toobig" is forbidden: maximum cpu usage per Container is 400m, but limit is 1
```

### Quota exhaustion

The quota is checked when an object is created. A pod with `requests.cpu: 400m` fit (100m + 400m = 500m, the limit), the following ones are rejected:

```text
$ kubectl -n learn-10a run q2 --image=alpine:3.22 --restart=Never -- sleep 3600
Error from server (Forbidden): pods "q2" is forbidden: exceeded quota: compute, requested: requests.cpu=100m, used: requests.cpu=500m, limited: requests.cpu=500m
```

The error is named exactly: what was requested, how much is used and what the limit is.

### A Deployment that hits the quota does not fail but "hangs"

A Deployment cannot create a pod itself: the ReplicaSet gets the error, and the Deployment only shows `0/2`:

```text
$ kubectl -n learn-10a get deploy dep
dep   0/2   0     0     5s
$ kubectl -n learn-10a get deploy dep -o jsonpath='{range .status.conditions[*]}{.type}={.status} ({.reason}): {.message}{"\n"}{end}'
Progressing=True (NewReplicaSetCreated): Created new replica set "dep-64f8b8bfd7"
Available=False (MinimumReplicasUnavailable): Deployment does not have minimum availability.
ReplicaFailure=True (FailedCreate): pods "dep-64f8b8bfd7-fftch" is forbidden: exceeded quota: compute, requested: requests.cpu=100m, used: requests.cpu=500m, limited: requests.cpu=500m
```

The cause is visible in the `ReplicaFailure` condition and in the `FailedCreate` events. The same condition was mentioned in [module 05](../05-deployments/expert.en.md#the-deployment-status). Quotas also count objects: `pods`, `configmaps`, `persistentvolumeclaims`, `count/<resource>`.

> ⚠️ Verify: quota scopes (`scopes`, for example `BestEffort`) and the `count/<resource>` syntax were not run in this material.

## ClusterRole and bindings

| Object | Scope |
|---|---|
| Role + RoleBinding | one namespace |
| ClusterRole + ClusterRoleBinding | the whole cluster, including cluster-scoped resources (nodes) and non-resource URLs |
| ClusterRole + RoleBinding | the ClusterRole's rights, but only in the binding's namespace |

The cluster-scoped resource `nodes` could not be read by the ServiceAccount until a ClusterRole and ClusterRoleBinding were granted:

```text
$ kubectl auth can-i list nodes --as=$SA
no
$ kubectl create clusterrole node-reader --verb=get,list --resource=nodes
$ kubectl create clusterrolebinding reader-nodes --clusterrole=node-reader --serviceaccount=learn-10a:reader
$ kubectl auth can-i list nodes --as=$SA
yes
```

The third variant is useful for reuse: the built-in ClusterRole `view` is bound with a RoleBinding, and the rights act only in one namespace:

```text
$ kubectl create rolebinding view-here --clusterrole=view --serviceaccount=learn-10a:reader -n learn-10a
$ kubectl -n learn-10a auth can-i list deployments --as=$SA
yes
$ kubectl -n default auth can-i list deployments --as=$SA
no
$ kubectl -n learn-10a auth can-i get secrets --as=$SA
no
```

`view` allows reading Deployments in `learn-10a`, but not in `default`, and does not allow reading Secrets.

## Built-in ClusterRoles

The cluster has ready-made roles (verified: `cluster-admin`, `admin`, `edit`, `view`):

| Role | Rights |
|---|---|
| `cluster-admin` | everything: `apiGroups: *`, `resources: *`, `verbs: *`, and non-resource URLs |
| `admin` | almost everything in a namespace, including roles and bindings inside it |
| `edit` | creating and changing most objects, without managing rights |
| `view` | read only (without Secrets) |

```text
$ kubectl get clusterrole cluster-admin -o jsonpath='{.rules}'
[{"apiGroups":["*"],"resources":["*"],"verbs":["*"]},{"nonResourceURLs":["*"],"verbs":["*"]}]
$ kubectl get clusterrole view -o jsonpath='{.aggregationRule}'
{"clusterRoleSelectors":[{"matchLabels":{"rbac.authorization.k8s.io/aggregate-to-view":"true"}}]}
```

`view` has an `aggregationRule`: the role is assembled from all ClusterRoles labeled `aggregate-to-view: "true"`. This is how CRDs add their rights to the standard roles.

## Groups and impersonation

There are no users as objects in Kubernetes: the name and groups come from a certificate, a token or an external provider. A subject in a binding can be given as a group:

```text
$ kubectl -n learn-10a auth can-i list pods --as=jane --as-group=devs
no
$ kubectl -n learn-10a create rolebinding devs-view --clusterrole=view --group=devs
$ kubectl -n learn-10a auth can-i list pods --as=jane --as-group=devs
yes
$ kubectl -n learn-10a auth can-i list pods --as=jane
no
```

The rights go to the group `devs`: `jane` has them only together with the group. The flags `--as` and `--as-group` are impersonation: an administrator checks rights without obtaining someone else's credentials. Who you are yourself:

```text
$ kubectl auth whoami
Username                                            kubernetes-admin
Groups                                              [kubeadm:cluster-admins system:authenticated]
```

The kind administrator logs in as `kubernetes-admin` from the group `kubeadm:cluster-admins`, to which `cluster-admin` is bound.

## ServiceAccount tokens

The token in a pod is issued by the kubelet as a **projection** (a temporary token bound to the pod). The token content from the `apiclient` pod:

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

The token is bound to a specific pod and node (the claims show `pod` and `node`), `exp` in this cluster is about a year from `iat`, and the kubelet refreshes it in advance (`warnafter` — in an hour). There is no Secret object with the token in the namespace: `kubectl get secrets` is empty.

> ⚠️ Verify: that the token stops being valid after the pod is deleted, and the exact rights of the built-in roles `admin` and `edit`, are given from documentation and were not verified by running.

A short-lived token for external clients is issued by the command:

```text
$ kubectl -n learn-10a create token reader --duration=10m
eyJhbGciOiJSUzI1NiIsImtpZCI6Il…
```

In the token `exp − iat = 600` seconds (in the experiment 1795424540 − 1795423940).

### Opting out of the token

A pod that does not need API access does not need a token:

```yaml
spec:
  serviceAccountName: reader
  automountServiceAccountToken: false
```

```text
$ kubectl -n learn-10a exec noauto -- ls /var/run/secrets/kubernetes.io/serviceaccount
ls: /var/run/secrets/kubernetes.io/serviceaccount: No such file or directory
```

There is no token directory: compromising such a pod does not give API access.

## Best practices

- A namespace per team or environment; each with a ResourceQuota and LimitRange.
- The principle of least privilege: a separate ServiceAccount per application, a narrow Role, no `*` in `verbs` and `resources`.
- Groups instead of listing users; describe roles in Git.
- `automountServiceAccountToken: false` for pods that do not work with the API.
- Regularly check `kubectl auth can-i --list --as=…` and bindings to `cluster-admin`.
- Remember: the right `create pods` is equivalent to access to all Secrets of the namespace (module 07): a pod can mount any Secret.

## Common mistakes

- ❌ Giving `cluster-admin` to an application "to make it work" → ✅ find the missing verb and resource from the `Forbidden` message.
- ❌ Setting a CPU quota without a LimitRange → ✅ pods without explicit values will be rejected.
- ❌ Expecting a Deployment to report quota exhaustion with an error → ✅ look at `ReplicaFailure` and the `FailedCreate` events.
- ❌ Using a ClusterRoleBinding where a RoleBinding is enough → ✅ the minimal scope.
- ❌ Thinking `view` shows Secrets → ✅ `view` is without Secrets (verified: `get secrets` — `no`).

## Comparison with other tracks

- [Linux](../../../linux/overview.en.md): RBAC is an analog of groups and `sudoers`, but for API objects; the principle of least privilege is the same.
- [C#](../../../csharp/recipes/jwt-auth/overview.en.md): a ServiceAccount token is also a JWT with claims (`sub`, `aud`, `exp`) that the server verifies.

## Related materials

- Previous level: [basics](basics.en.md)
- Next: [expert](expert.en.md)
- Previous modules: [07 · ConfigMap and Secret](../07-config-and-secrets/overview.en.md), [05 · Deployment](../05-deployments/overview.en.md)

## Sources

- [Resource Quotas](https://kubernetes.io/docs/concepts/policy/resource-quotas/)
- [Limit Ranges](https://kubernetes.io/docs/concepts/policy/limit-range/)
- [Using RBAC Authorization: ClusterRole and bindings](https://kubernetes.io/docs/reference/access-authn-authz/rbac/#role-and-clusterrole)
- [Default roles and role bindings](https://kubernetes.io/docs/reference/access-authn-authz/rbac/#default-roles-and-role-bindings)
- [Managing Service Accounts](https://kubernetes.io/docs/reference/access-authn-authz/service-accounts-admin/)
- [User impersonation](https://kubernetes.io/docs/reference/access-authn-authz/authentication/#user-impersonation)
