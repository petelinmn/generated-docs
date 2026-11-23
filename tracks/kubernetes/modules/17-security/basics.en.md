---
title: "Pod and network security: basics"
lang: en
track: kubernetes
kind: module
doc: basics
slug: 17-security
part: 3
tags: [networkpolicy, default-deny, securitycontext, pod-security-admission, runasnonroot]
versions: "Kubernetes 1.37.0 (kind v0.34, kindnet), kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Pod and network security: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [04 · Pod](../04-pods/overview.en.md), [06 · Service and networking](../06-services/overview.en.md) · **Time:** ~2 h

All examples were run in kind (Kubernetes 1.37.0, network plugin kindnet), namespaces `learn-17`, `learn-17-other` and `learn-17-psa`.

## Why it matters

A cluster is a shared environment. If one pod is compromised, by default it can reach any other pod, and the process inside runs as root with a wide set of privileges. Basic security hygiene means limiting the network, the process rights and which pods are admitted to a namespace at all.

## NetworkPolicy: limiting the network

Setup: `server` (nginx with the label `app=server` and a Service), two clients in the same namespace — `client-ok` (label `role=frontend`) and `client-bad` (`role=other`), and `outsider` in another namespace `learn-17-other`. Access is checked with `wget` and a 3-second timeout.

**Without policies** all three clients reached the server:

```text
client-ok -> OK
client-bad -> OK
outsider -> OK
```

### default deny

A policy selects pods through `podSelector`. An empty selector selects all pods of the namespace; without `ingress` rules it forbids all incoming traffic:

```yaml
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata: {name: default-deny-ingress}
spec:
  podSelector: {}
  policyTypes: [Ingress]
```

```text
client-ok -> wget: download timed out   (BLOCKED)
client-bad -> wget: download timed out  (BLOCKED)
outsider -> wget: download timed out    (BLOCKED)
```

The rule: **once a pod is selected by at least one policy of type `Ingress`, only the incoming traffic listed in the policies is allowed.**

### Allow what is needed

```yaml
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata: {name: allow-frontend}
spec:
  podSelector: {matchLabels: {app: server}}
  policyTypes: [Ingress]
  ingress:
  - from:
    - podSelector: {matchLabels: {role: frontend}}
    ports: [{protocol: TCP, port: 80}]
```

```text
client-ok -> OK
client-bad -> BLOCKED
outsider -> BLOCKED
```

Policies are **additive**: what at least one of them allows is allowed. You cannot forbid on top of an allowance.

> ⚠️ Verify: policies are enforced by the network plugin (CNI); in kind it is kindnet, and it applied them. The behavior in a cluster whose plugin lacks NetworkPolicy support was not checked in the experiments.

## securityContext: process rights

By default a container runs as root:

```text
$ kubectl -n learn-17 exec client-ok -- sh -c 'id; grep CapEff /proc/1/status; touch /x && echo rootfs-writable'
uid=0(root) gid=0(root) groups=0(root),10(wheel)
CapEff:	00000000a80425fb
rootfs-writable
```

A hardened variant:

```yaml
apiVersion: v1
kind: Pod
metadata: {name: secure}
spec:
  securityContext:                     # pod level
    runAsNonRoot: true
    runAsUser: 10001
    runAsGroup: 10001
    fsGroup: 10001
    seccompProfile: {type: RuntimeDefault}
  containers:
  - name: c
    image: busybox:1.37
    command: ["sleep","3600"]
    securityContext:                   # container level
      allowPrivilegeEscalation: false
      readOnlyRootFilesystem: true
      capabilities: {drop: [ALL]}
    volumeMounts: [{name: tmp, mountPath: /tmp}]
  volumes: [{name: tmp, emptyDir: {}}]
```

```text
$ kubectl -n learn-17-psa exec secure -- id
uid=10001 gid=10001 groups=10001
$ kubectl -n learn-17-psa exec secure -- sh -c 'touch /x; touch /tmp/ok && echo tmp-writable'
touch: /x: Read-only file system
tmp-writable
$ kubectl -n learn-17-psa exec secure -- sh -c 'grep -E "CapEff|NoNewPrivs" /proc/1/status'
CapEff:	0000000000000000
NoNewPrivs:	1
```

- `runAsNonRoot` + `runAsUser` — the process is not root.
- `readOnlyRootFilesystem` — you can write only to mounted volumes (here an `emptyDir` on `/tmp`).
- `capabilities.drop: [ALL]` — zero capabilities (`CapEff: 0`) instead of the default 14 privileges.
- `allowPrivilegeEscalation: false` — the process cannot gain more rights than it has (`NoNewPrivs: 1`).

If you set `runAsNonRoot: true` for an image that starts as root, the container does not start:

```text
$ kubectl -n learn-17 get pod nonroot-fail
NAME           READY   STATUS                       RESTARTS   AGE
nonroot-fail   0/1     CreateContainerConfigError   0          6s
  Warning  Failed  kubelet  Error: container has runAsNonRoot and image will run as root (pod: "nonroot-fail_learn-17(…)", container: c)
```

## Pod Security Admission: control at the entrance

PSA is a built-in admission controller. It is enabled with **namespace labels**:

```text
$ kubectl label ns learn-17-psa \
    pod-security.kubernetes.io/enforce=baseline \
    pod-security.kubernetes.io/warn=restricted \
    pod-security.kubernetes.io/audit=restricted
```

- **Levels** (Pod Security Standards): `privileged` (no restrictions), `baseline` (forbids the obviously dangerous), `restricted` (strict practice).
- **Modes:** `enforce` — reject the pod, `warn` — a warning to the user, `audit` — a record in the audit log.

What happened in the experiment:

```text
$ kubectl -n learn-17-psa run priv --image=nginx:1.29 --overrides='{…"securityContext":{"privileged":true}…}'
Error from server (Forbidden): pods "priv" is forbidden: violates PodSecurity "baseline:latest": privileged (container "priv" must not set securityContext.privileged=true)

$ kubectl -n learn-17-psa run plain --image=nginx:1.29
Warning: would violate PodSecurity "restricted:latest": allowPrivilegeEscalation != false (…), unrestricted capabilities (…), runAsNonRoot != true (…), seccompProfile (…)
pod/plain created
```

The privileged pod was rejected (`baseline` — `enforce`), the ordinary one was created but with a `restricted` warning (`warn`). The warning shows exactly what to fix.

## Beginner mistakes

- ❌ Assuming the network is "closed by default" without policies → ✅ it is open; `default-deny` closes it.
- ❌ Creating `default-deny` and forgetting to allow the needed traffic → ✅ first a map of connections, then policies.
- ❌ `readOnlyRootFilesystem: true` without an `emptyDir` for `/tmp` → ✅ mount a writable volume.
- ❌ `runAsNonRoot: true` for an image that runs as root → ✅ set `runAsUser` or take an image without root.
- ❌ Enabling `enforce: restricted` without checking existing applications → ✅ `warn`/`audit` first.

## Related materials

- Next: [advanced theory](advanced.en.md) — namespaceSelector, egress, PSA modes, ValidatingAdmissionPolicy
- Networking and Service: [06 · Service and networking](../06-services/overview.en.md)
- API access rights: [10 · Namespaces and RBAC](../10-namespaces-and-rbac/overview.en.md)
- Pod and containers: [04 · Pod](../04-pods/overview.en.md)

## Sources

- [Network Policies](https://kubernetes.io/docs/concepts/services-networking/network-policies/)
- [Configure a Security Context for a Pod or Container](https://kubernetes.io/docs/tasks/configure-pod-container/security-context/)
- [Pod Security Admission](https://kubernetes.io/docs/concepts/security/pod-security-admission/)
- [Pod Security Standards](https://kubernetes.io/docs/concepts/security/pod-security-standards/)
