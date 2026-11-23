---
title: "Pod and network security: expert"
lang: en
track: kubernetes
kind: module
doc: expert
slug: 17-security
part: 3
tags: [networkpolicy, pod-security-admission, pod-security-standards, securitycontext, validatingadmissionpolicy, cel, seccomp]
versions: "Kubernetes 1.37.0 (kind v0.34, kindnet), kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Pod and network security: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on [basic](basics.en.md) and [advanced](advanced.en.md) theory. What was confirmed by an experiment in kind (Kubernetes 1.37.0) is marked "in the experiment".

## Reference

### `securityContext` fields

| Field | Where | Meaning |
|---|---|---|
| `runAsUser`, `runAsGroup` | pod, container | numeric uid and gid of the process (in the experiment `uid=10001 gid=10001`) |
| `runAsNonRoot` | pod, container | the kubelet refuses to start a container as root (in the experiment `CreateContainerConfigError`) |
| `fsGroup` | pod | the owner group of volumes |
| `seccompProfile` | pod, container | `RuntimeDefault`, `Localhost`, `Unconfined` |
| `allowPrivilegeEscalation` | container | forbids gaining more rights than the parent process (in the experiment `NoNewPrivs: 1`) |
| `readOnlyRootFilesystem` | container | a read-only root filesystem (in the experiment `Read-only file system`) |
| `capabilities.add/drop` | container | add and remove capabilities (in the experiment `CapEff`) |
| `privileged` | container | privileged mode (in the experiment rejected by the `baseline` level) |

> ⚠️ Verify: the fields `supplementalGroups`, `sysctls`, `seLinuxOptions`, `procMount`, `fsGroupChangePolicy` and the seccomp profile values `Localhost`/`Unconfined` are taken from the documentation and were not run.

### Pod Security Admission labels

| Namespace label | Value |
|---|---|
| `pod-security.kubernetes.io/enforce` | the level whose violation rejects the pod |
| `pod-security.kubernetes.io/warn` | the level whose violation produces a warning |
| `pod-security.kubernetes.io/audit` | the level whose violations are written to the audit log |
| `…/<mode>-version` | the standard version (`latest` by default, in the experiment `baseline:latest`) |

Levels: `privileged` → `baseline` → `restricted` (from lenient to strict).

In the experiment `baseline` rejected `privileged: true` and `hostNetwork`. `restricted` required: `allowPrivilegeEscalation=false`, `capabilities.drop=["ALL"]`, `runAsNonRoot=true`, `seccompProfile.type` equal to `RuntimeDefault` or `Localhost`.

> ⚠️ Verify: the full list of controls of each level (hostPath, hostPort, allowed capabilities, volume types, sysctls and so on), the version labels and the cluster-wide exemption setup (`AdmissionConfiguration`) were not run.

### NetworkPolicy fields

| Field | Meaning |
|---|---|
| `spec.podSelector` | which pods the policy selects (empty — all in the namespace) |
| `spec.policyTypes` | `Ingress`, `Egress` or both; if omitted, it is derived from the sections present |
| `ingress[].from[]` / `egress[].to[]` | `podSelector`, `namespaceSelector`, `ipBlock` |
| `ingress[].ports[]` / `egress[].ports[]` | `protocol`, `port`, optional `endPort` |

The experiment covered `podSelector`, `namespaceSelector` (in one element and in two), `ports` (TCP, UDP). A `NetworkPolicy` object is namespaced.

### Commands

| Command | Purpose |
|---|---|
| `kubectl get netpol` | the namespace policies (`POD-SELECTOR`, `AGE`) |
| `kubectl label ns N pod-security.kubernetes.io/enforce=restricted` | enable PSA |
| `kubectl label --dry-run=server --overwrite ns N …` | check which existing pods would violate the level |
| `kubectl exec P -- grep CapEff /proc/1/status` | check the actual process rights (also `NoNewPrivs`, `Seccomp`) |
| `kubectl get validatingadmissionpolicy,validatingadmissionpolicybinding` | CEL policies and bindings |

## Internals

- **A NetworkPolicy is only an API object.** The API server itself does not filter traffic. The network plugin (CNI) reads the policies and turns them into rules on the nodes. In kind it is kindnet — in the experiment it enforced them. With a plugin lacking this feature, policies are created but have no effect.
- **Semantics.** A pod not selected by any policy is open. A pod selected by at least one policy of a given direction gets "everything is forbidden except what the union of the rules allows". There are no "deny" policies: only allowing and "empty" ones (default deny).
- **PSA is a built-in admission controller.** It reads the namespace labels and checks pods at creation (and update). For Deployments, Jobs and other controllers the modes `warn` and `audit` fire on the pod template, while `enforce` fires on the real pod created by the ReplicaSet. That is why the rejection is visible in ReplicaSet events (in the experiment `FailedCreate … violates PodSecurity "restricted:latest"`).
- **ValidatingAdmissionPolicy runs inside the API server** without an external webhook: the CEL expression gets the object as `object` and returns `true` (allowed) or `false` (rejection with `message`). A `ValidatingAdmissionPolicyBinding` sets the scope (`namespaceSelector`, `objectSelector`) and the actions (`validationActions`: `Deny`, `Warn`, `Audit`). In the experiment `Deny` was verified.
- **The ServiceAccount token** is a projected volume with a token bound to the pod; `automountServiceAccountToken: false` removes the volume (in the experiment).
- **The process and the kernel.** `securityContext` is turned into container runtime parameters: uid/gid, the set of capabilities, the `no_new_privs` flag, the seccomp profile. The result can be checked in `/proc/1/status` of the container (`CapEff`, `NoNewPrivs`, `Seccomp`). More about capabilities and seccomp is in the [Linux track](../../../linux/overview.en.md).

> ⚠️ Verify: the behavior of NetworkPolicy in a cluster with a CNI lacking policy support was not checked. The `Warn` and `Audit` actions of a ValidatingAdmissionPolicyBinding are taken from the documentation.

## Gotchas

### 1. AND and OR in `from`

One element with `namespaceSelector` and `podSelector` is an intersection; two elements are a union. In the experiment the difference between the variants changed the result for `client-ok` and `outsider`.

### 2. Egress without DNS

`policyTypes: [Egress]` without rules blocks DNS too: `nslookup` gives `connection timed out; no servers could be reached`. Add a rule to `kube-dns` (UDP/TCP 53) to the policy.

### 3. `enforce` does not touch running pods

Changing the label shows `Warning: existing pods … violate`, but the pods stay. The violation shows up at the next creation (a Deployment restart, rescheduling).

### 4. The cause of a rejection is in the ReplicaSet, not the Deployment

`kubectl rollout status` hangs, `kubectl get deploy` shows `0/1`, and the cause is in the ReplicaSet events. Look for `FailedCreate` and `violates PodSecurity`.

### 5. `runAsNonRoot` and a root image

`runAsNonRoot: true` is checked by the kubelet: an image without `USER` starts as root, and the container does not run (`CreateContainerConfigError`: `container has runAsNonRoot and image will run as root`). The fix is `runAsUser` or another image.

### 6. `readOnlyRootFilesystem` and write paths

An application writing to temporary directories will fail. In the experiment `touch /x` gave `Read-only file system`, while `/tmp` on an `emptyDir` worked.

### 7. Policies only allow

You cannot "allow everything except one thing". You have to list what is allowed. The exception is `ipBlock.except` within one block.

### 8. A namespace without PSA labels is like `privileged`

`learn-17` had no labels, and pods ran as root with the full set of capabilities (in the experiment). Protection is enabled only by labels (or cluster configuration).

### 9. The violation message contains a version

`baseline:latest` and `restricted:latest` in the messages mean the latest version of the standard for the given Kubernetes version is used. After a cluster upgrade the requirements may change, so in production the version is often pinned (`…-version`).

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| NetworkPolicy | a standard API, portable | depends on the CNI; L3/L4; no explicit deny |
| CNI extensions (Calico, Cilium and others) | L7, global policies, explicit deny | tied to the plugin; not run |
| PSA | built in, simple levels | three levels, no arbitrary rules |
| ValidatingAdmissionPolicy | your own rules without a webhook | CEL has to be learned |
| OPA Gatekeeper, Kyverno | rich policies, mutations, reports | a separate installation; not run |
| Service mesh (mTLS) | encryption and service identity | complexity; not run |

## Interview questions

<details><summary>1. What happens without a single NetworkPolicy?</summary>

All traffic is allowed: any pod can reach any other. The network is closed with policies.

→ [Basic theory](basics.en.md#networkpolicy-limiting-the-network)

</details>

<details><summary>2. How do you make a default deny for incoming traffic?</summary>

A policy with an empty `podSelector` and `policyTypes: [Ingress]` without `ingress` rules.

→ [Basic theory](basics.en.md#default-deny)

</details>

<details><summary>3. Trick: two policies allow different things. What is allowed?</summary>

The union of both allowances: policies only add permissions.

→ [Gotcha 7](#7-policies-only-allow)

</details>

<details><summary>4. How do the two ways of writing <code>from</code> with <code>namespaceSelector</code> and <code>podSelector</code> differ?</summary>

In one element it is AND, in two separate elements it is OR.

→ [Advanced theory](advanced.en.md#networkpolicy-from-namespaceselector-and-and-versus-or)

</details>

<details><summary>5. Why did name resolution stop after an egress <code>default-deny</code>?</summary>

DNS requests (CoreDNS) are blocked. You must allow UDP/TCP 53 to `kube-dns`.

→ [Gotcha 2](#2-egress-without-dns)

</details>

<details><summary>6. How do <code>enforce</code>, <code>warn</code> and <code>audit</code> differ in PSA?</summary>

`enforce` rejects the pod, `warn` shows the client a warning, `audit` writes an event to the audit log.

→ [Basic theory](basics.en.md#pod-security-admission-control-at-the-entrance)

</details>

<details><summary>7. Trick: <code>enforce: restricted</code> is on, yet a Deployment is created without errors. Why?</summary>

Pods are rejected, not the Deployment. The ReplicaSet cannot create pods (`FailedCreate`); the Deployment itself stays "created".

→ [Gotcha 4](#4-the-cause-of-a-rejection-is-in-the-replicaset-not-the-deployment)

</details>

<details><summary>8. What happens to already running pods after <code>enforce</code> is enabled?</summary>

Nothing: creation is checked. A warning on the label change lists the violators.

→ [Gotcha 3](#3-enforce-does-not-touch-running-pods)

</details>

<details><summary>9. Which fields make a pod "restricted"?</summary>

`allowPrivilegeEscalation: false`, `capabilities.drop: [ALL]`, `runAsNonRoot: true`, `seccompProfile: RuntimeDefault` (listed in the message in the experiment).

→ [Reference](#pod-security-admission-labels)

</details>

<details><summary>10. Why <code>readOnlyRootFilesystem</code> and what should an application do with it?</summary>

It reduces damage after a compromise: you cannot write files into the image. For writing, mount volumes (`emptyDir`).

→ [Gotcha 6](#6-readonlyrootfilesystem-and-write-paths)

</details>

<details><summary>11. How do you disable the ServiceAccount token of a pod?</summary>

`automountServiceAccountToken: false` in the pod or in the ServiceAccount.

→ [Advanced theory](advanced.en.md#the-serviceaccount-token)

</details>

<details><summary>12. How do you forbid images with the <code>latest</code> tag without external components?</summary>

A ValidatingAdmissionPolicy with a CEL expression and a binding to the namespace.

→ [Advanced theory](advanced.en.md#validatingadmissionpolicy-your-own-rules)

</details>

<details><summary>13. Trick: a NetworkPolicy is created but blocks nothing. Why?</summary>

Possible causes: the policy selects no pod (`podSelector`), the needed direction is not in `policyTypes`, or the network plugin does not support policies.

→ [Internals](#internals)

</details>

## Cheat sheet

```text
kubectl get netpol
kubectl label ns N pod-security.kubernetes.io/{enforce,warn,audit}=baseline|restricted
kubectl label --dry-run=server --overwrite ns N pod-security.kubernetes.io/enforce=restricted
kubectl exec P -- grep -E "CapEff|NoNewPrivs|Seccomp" /proc/1/status
```

- The network is open until a pod is selected by a policy; policies only allow.
- In `from`: one element is AND, two are OR.
- Egress → do not forget DNS.
- PSA checks pod creation; look at ReplicaSet events.
- In a pod: non-root, no escalation, `drop: [ALL]`, seccomp, read-only rootfs.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- RBAC: [10 · Namespaces and RBAC](../10-namespaces-and-rbac/expert.en.md)
- Linux: [Linux track](../../../linux/overview.en.md)
- Scheduling: [18 · Pod scheduling](../18-scheduling/overview.en.md)

## Sources

- [Network Policies](https://kubernetes.io/docs/concepts/services-networking/network-policies/)
- [Pod Security Standards](https://kubernetes.io/docs/concepts/security/pod-security-standards/)
- [Pod Security Admission](https://kubernetes.io/docs/concepts/security/pod-security-admission/)
- [Validating Admission Policy](https://kubernetes.io/docs/reference/access-authn-authz/validating-admission-policy/)
- [Security Context](https://kubernetes.io/docs/tasks/configure-pod-container/security-context/)
- [Service Accounts](https://kubernetes.io/docs/concepts/security/service-accounts/)
