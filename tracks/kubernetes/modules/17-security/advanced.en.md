---
title: "Pod and network security: advanced"
lang: en
track: kubernetes
kind: module
doc: advanced
slug: 17-security
part: 3
tags: [networkpolicy, namespaceselector, egress, pod-security-admission, capabilities, serviceaccount, validatingadmissionpolicy]
versions: "Kubernetes 1.37.0 (kind v0.34, kindnet), kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Pod and network security: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on [basic theory](basics.en.md). Experiments were run in kind (Kubernetes 1.37.0, kindnet).

## NetworkPolicy: from, namespaceSelector and AND versus OR

The `from` list consists of elements; an element may contain `podSelector`, `namespaceSelector` or both. This is easy to get wrong: the way you write it changes the meaning.

**One element with two selectors — AND:** a pod from a namespace labeled `team=other` **and** labeled `role=frontend`:

```yaml
ingress:
- from:
  - namespaceSelector: {matchLabels: {team: other}}
    podSelector: {matchLabels: {role: frontend}}
```

**Two elements — OR:** any pod from a namespace `team=other` **or** a `role=frontend` pod from the policy's own namespace:

```yaml
ingress:
- from:
  - namespaceSelector: {matchLabels: {team: other}}
  - podSelector: {matchLabels: {role: frontend}}
```

Experiment (namespace `learn-17-other` labeled `team=other`, pod `outsider` labeled `role=frontend`):

```text
== AND (one element)
client-ok -> BLOCKED    ← role=frontend, but not from team=other
outsider -> OK
== OR (two elements)
client-ok -> OK
client-bad -> BLOCKED
outsider -> OK
```

The difference is one indent (`-` before `podSelector`), so review such policies carefully.

## Egress and DNS

`policyTypes: [Egress]` limits the **outgoing** traffic of the selected pods. A policy without rules forbids everything, including DNS queries to CoreDNS — and the application can no longer find services by name:

```yaml
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata: {name: client-egress-deny}
spec:
  podSelector: {matchLabels: {role: frontend}}
  policyTypes: [Egress]
```

```text
$ kubectl -n learn-17 exec client-ok -- wget -q -T 3 -O /dev/null http://server.learn-17.svc
BLOCKED
$ kubectl -n learn-17 exec client-ok -- nslookup server.learn-17.svc
;; connection timed out; no servers could be reached
```

Allow DNS and the needed server:

```yaml
spec:
  podSelector: {matchLabels: {role: frontend}}
  policyTypes: [Egress]
  egress:
  - to:
    - namespaceSelector: {matchLabels: {kubernetes.io/metadata.name: kube-system}}
      podSelector: {matchLabels: {k8s-app: kube-dns}}
    ports: [{protocol: UDP, port: 53}, {protocol: TCP, port: 53}]
  - to:
    - podSelector: {matchLabels: {app: server}}
    ports: [{protocol: TCP, port: 80}]
```

```text
client-ok -> OK
```

Kubernetes puts the label `kubernetes.io/metadata.name` on every namespace itself, so you can select by namespace name without manual labels. Note that responses to allowed connections return without separate rules.

> ⚠️ Verify: `ipBlock` (including `except`) and the `endPort` port range are taken from the documentation and were not run.

## Pod Security Admission: modes in practice

The namespace `learn-17-psa` had `enforce=baseline`, `warn=restricted`, `audit=restricted`.

**A warning (`warn`) when a controller is created.** The Deployment `dep` was created, and `kubectl` showed which fields violate `restricted`: PSA checks the pod template already when the Deployment is created.

**A rejection (`enforce`) applies to pods, not controllers.** After `kubectl label ns learn-17-psa pod-security.kubernetes.io/enforce=restricted --overwrite`:

```text
Warning: existing pods in namespace "learn-17-psa" violate the new PodSecurity enforce level "restricted:latest"
Warning: dep-5d9b469677-mhvf9 (and 1 other pod): allowPrivilegeEscalation != false, unrestricted capabilities, runAsNonRoot != true, seccompProfile
namespace/learn-17-psa labeled
```

The existing pods stayed, but a new rollout did not pass:

```text
$ kubectl -n learn-17-psa rollout restart deploy/dep
$ kubectl -n learn-17-psa get rs
NAME             DESIRED   CURRENT   READY   AGE
dep-576fd7f57b   1         0         0       6s      ← new ReplicaSet, no pods
dep-5d9b469677   1         1         1       17s
$ kubectl -n learn-17-psa get events --field-selector reason=FailedCreate
Error creating: pods "dep-576fd7f57b-bgw6p" is forbidden: violates PodSecurity "restricted:latest": allowPrivilegeEscalation != false (…)
```

The Deployment accepted the change and so did the ReplicaSet, but it could not create pods (`ReplicaFailure=True`, `FailedCreate`). So after enabling `enforce`, look not only at the Deployment but at the ReplicaSet events.

**A check without applying.** To find out what would happen, add `--dry-run=server`:

```text
$ kubectl label --dry-run=server --overwrite ns learn-17 pod-security.kubernetes.io/enforce=restricted
Warning: existing pods in namespace "learn-17" violate the new PodSecurity enforce level "restricted:latest"
Warning: client-bad (and 2 other pods): allowPrivilegeEscalation != false, unrestricted capabilities, runAsNonRoot != true, seccompProfile
namespace/learn-17 labeled (server dry run)
```

A typical rollout path: `warn` and `audit` at the target level → fix the manifests → `enforce`.

What `baseline` forbids is visible from the messages in the experiment: `privileged` and `hostNetwork` (`host namespaces (hostNetwork=true)`).

## Capabilities and privileges

Linux splits root's privileges into **capabilities**. A container gets 14 of them by default (`CapEff: 00000000a80425fb`). You can remove the extra ones and add the needed ones:

```yaml
securityContext:
  capabilities: {drop: [NET_RAW], add: [NET_ADMIN]}
```

```text
$ kubectl -n learn-17 exec capsdemo -- sh -c 'grep CapEff /proc/1/status; ping -c1 -W1 127.0.0.1'
CapEff:	00000000a80415fb
PING 127.0.0.1 (127.0.0.1): 56 data bytes
ping: permission denied (are you root?)
$ kubectl -n learn-17 exec client-ok -- ping -c1 -W1 127.0.0.1
64 bytes from 127.0.0.1: seq=0 ttl=64 time=0.078 ms
```

Without `NET_RAW` `ping` (raw sockets) does not work; with `NET_ADMIN` the process got an extra privilege (the bit was added). Practice: `drop: [ALL]` and add only what is needed. What `privileged: true` does is a separate matter: all capabilities and access to node devices; `baseline` already forbids such a pod.

> ⚠️ Verify: `seccompProfile`. In the experiment an ordinary container in kind already ran under a seccomp filter (`Seccomp: 2` in `/proc/1/status`), and with `RuntimeDefault` the value is the same. The default for other environments was not checked.

## The ServiceAccount token

A ServiceAccount token is mounted into every pod by default:

```text
$ kubectl -n learn-17 exec client-ok -- ls /var/run/secrets/kubernetes.io/serviceaccount
ca.crt  namespace  token
```

If a pod does not need API access, remove it: `automountServiceAccountToken: false` in the pod or in the ServiceAccount. In the experiment the pod `secure` had no `/var/run/secrets/kubernetes.io/serviceaccount` directory. The token's rights are set by [RBAC](../10-namespaces-and-rbac/overview.en.md).

## ValidatingAdmissionPolicy: your own rules

When the standard checks are not enough, the API server can check objects itself with an expression in CEL (Common Expression Language). Example: forbid images with the tag `:latest` in one namespace.

```yaml
apiVersion: admissionregistration.k8s.io/v1
kind: ValidatingAdmissionPolicy
metadata: {name: no-latest-17}
spec:
  failurePolicy: Fail
  matchConstraints:
    resourceRules:
    - apiGroups: [""]
      apiVersions: ["v1"]
      operations: ["CREATE","UPDATE"]
      resources: ["pods"]
  validations:
  - expression: "object.spec.containers.all(c, !c.image.endsWith(':latest'))"
    message: "image tag :latest is not allowed"
---
apiVersion: admissionregistration.k8s.io/v1
kind: ValidatingAdmissionPolicyBinding
metadata: {name: no-latest-17}
spec:
  policyName: no-latest-17
  validationActions: [Deny]
  matchResources:
    namespaceSelector: {matchLabels: {kubernetes.io/metadata.name: learn-17}}
```

```text
$ kubectl -n learn-17 run lat --image=nginx:latest
The pods "lat" is invalid: : ValidatingAdmissionPolicy 'no-latest-17' with binding 'no-latest-17' denied request: image tag :latest is not allowed
$ kubectl -n learn-17 run notlat --image=nginx:1.29
pod/notlat created
```

The policy (`Policy`) describes the rule, the binding (`Binding`) says where it applies and what to do (`Deny`, `Warn`, `Audit`). More in [expert theory](expert.en.md).

## Best practices

- In every namespace with applications start with `default-deny` for incoming traffic, then add allowances.
- For outgoing traffic do not forget DNS.
- Roll out `restricted` through `warn`/`audit`, then `enforce`.
- In pods: `runAsNonRoot`, `allowPrivilegeEscalation: false`, `capabilities.drop: [ALL]`, `seccompProfile: RuntimeDefault`, `readOnlyRootFilesystem` with a writable volume.
- Disable the ServiceAccount token where it is not needed.
- Images: fixed tags or digests, scanning in CI (tools like Trivy).

> ⚠️ Verify: image scanning (Trivy, Grype and the like) and image signatures were not run in the experiments — these are recommendations from the documentation.

## Common mistakes

- ❌ Mixing up AND and OR in `from` → ✅ test the policies with requests from different pods.
- ❌ An egress `default-deny` without DNS → ✅ allow UDP/TCP 53 to CoreDNS.
- ❌ Expecting `enforce` to stop already running pods → ✅ it checks creation only.
- ❌ Looking for the cause in the Deployment → ✅ check `kubectl get events` for the ReplicaSet (`FailedCreate`).
- ❌ Granting capabilities "just in case" → ✅ `drop: [ALL]` and a targeted `add`.

## Related materials

- Next: [expert theory](expert.en.md)
- RBAC and ServiceAccount: [10 · Namespaces and RBAC](../10-namespaces-and-rbac/advanced.en.md)
- Secrets: [07 · ConfigMap and Secret](../07-config-and-secrets/overview.en.md)
- Pod placement (taints, affinity): [18 · Pod scheduling](../18-scheduling/overview.en.md)

## Sources

- [Network Policies](https://kubernetes.io/docs/concepts/services-networking/network-policies/)
- [Pod Security Admission](https://kubernetes.io/docs/concepts/security/pod-security-admission/)
- [Enforce Pod Security Standards with Namespace Labels](https://kubernetes.io/docs/tasks/configure-pod-container/enforce-standards-namespace-labels/)
- [Validating Admission Policy](https://kubernetes.io/docs/reference/access-authn-authz/validating-admission-policy/)
- [Configure Service Accounts for Pods](https://kubernetes.io/docs/tasks/configure-pod-container/configure-service-account/)
