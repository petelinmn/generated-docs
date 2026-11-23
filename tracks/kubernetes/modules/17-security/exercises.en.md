---
title: "Exercises: pod and network security"
lang: en
track: kubernetes
kind: module
doc: exercises
slug: 17-security
part: 3
tags: [networkpolicy, pod-security-admission, securitycontext, validatingadmissionpolicy]
status: draft
updated: 2026-11-23
---

# Exercises: pod and network security

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself; hints and solutions are under spoilers. You need a kind cluster (kindnet enforces NetworkPolicy) and a namespace: `kubectl create namespace learn-17`. Afterwards: `kubectl delete namespace learn-17 learn-17-psa`.

## 1. 🟢 default deny and an allowance

**Task:** run `server` (nginx, label `app=server`) with a Service, clients `client-ok` (`role=frontend`) and `client-bad` (`role=other`). Check access, then apply `default-deny-ingress` and an allowance for `role=frontend`.

**Expected result:** first both clients get a response; after deny both are blocked; after the allowance only `client-ok` gets through.

<details><summary>Hint</summary>

The check: `kubectl exec client-ok -- wget -q -T 3 -O /dev/null http://server.learn-17.svc`.

</details>

<details><summary>Solution</summary>

```yaml
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata: {name: allow-frontend}
spec:
  podSelector: {matchLabels: {app: server}}
  policyTypes: [Ingress]
  ingress:
  - from: [{podSelector: {matchLabels: {role: frontend}}}]
    ports: [{protocol: TCP, port: 80}]
```

```text
client-ok -> OK
client-bad -> wget: download timed out   (BLOCKED)
```

</details>

## 2. 🟢 A hardened pod

**Task:** create a pod `secure` from `busybox:1.37` with uid 10001, `runAsNonRoot`, `readOnlyRootFilesystem`, `drop: [ALL]`, `allowPrivilegeEscalation: false` and an `emptyDir` on `/tmp`. Show that writing to `/` is impossible and to `/tmp` is possible.

**Expected result:** `uid=10001`, `touch: /x: Read-only file system`, `tmp-writable`, `CapEff: 0000000000000000`.

<details><summary>Hint</summary>

`kubectl exec secure -- grep -E "CapEff|NoNewPrivs" /proc/1/status`.

</details>

<details><summary>Solution</summary>

The manifest is in the [basic theory](basics.en.md#securitycontext-process-rights).

```text
uid=10001 gid=10001 groups=10001
touch: /x: Read-only file system
tmp-writable
CapEff:	0000000000000000
NoNewPrivs:	1
```

</details>

## 3. 🟡 AND and OR

**Task:** create a namespace `learn-17-other` labeled `team=other` and in it a pod `outsider` labeled `role=frontend`. Write an ingress policy with `namespaceSelector` and `podSelector` in **one** element, then in **two**. Compare access for `client-ok` and `outsider`.

**Expected result:** with one element `client-ok` is blocked and `outsider` gets through; with two elements both get through.

<details><summary>Hint</summary>

The difference is the presence of a second `-` in the `from` list.

</details>

<details><summary>Solution</summary>

```text
== one element (AND)
client-ok -> BLOCKED
outsider -> OK
== two elements (OR)
client-ok -> OK
client-bad -> BLOCKED
outsider -> OK
```

</details>

## 4. 🟡 Egress and DNS

**Task:** forbid all outgoing traffic for `role=frontend` and make sure the name `server.learn-17.svc` does not resolve. Then allow DNS (to `kube-dns` in `kube-system`) and access to `app=server`.

**Expected result:** first `nslookup` gives `connection timed out`, after the rule `client-ok -> OK`.

<details><summary>Hint</summary>

A `namespaceSelector` with the label `kubernetes.io/metadata.name: kube-system` and a `podSelector` with `k8s-app: kube-dns`; ports 53 UDP and TCP.

</details>

<details><summary>Solution</summary>

The manifest is in the [advanced theory](advanced.en.md#egress-and-dns).

```text
;; connection timed out; no servers could be reached
client-ok -> OK
```

</details>

## 5. 🔴 PSA and ValidatingAdmissionPolicy

**Task:** (a) create a namespace `learn-17-psa` with `enforce=baseline`, `warn=restricted`; try to create a privileged pod and an ordinary one. (b) Switch `enforce` to `restricted`, restart a Deployment and find the cause in the ReplicaSet events. (c) Create a ValidatingAdmissionPolicy that forbids `:latest`.

**Expected result:** (a) the rejection `violates PodSecurity "baseline:latest": privileged`, a `restricted` warning for the ordinary pod; (b) `FailedCreate`; (c) `image tag :latest is not allowed`.

<details><summary>Hint</summary>

(b) `kubectl get rs`, `kubectl get events --field-selector reason=FailedCreate`; (c) a policy and a binding with `validationActions: [Deny]`.

</details>

<details><summary>Solution</summary>

```text
Error from server (Forbidden): pods "priv" is forbidden: violates PodSecurity "baseline:latest": privileged (container "priv" must not set securityContext.privileged=true)
Warning: would violate PodSecurity "restricted:latest": allowPrivilegeEscalation != false (…), …
dep-576fd7f57b   1   0   0   6s
Error creating: pods "dep-576fd7f57b-bgw6p" is forbidden: violates PodSecurity "restricted:latest": …
The pods "lat" is invalid: : ValidatingAdmissionPolicy 'no-latest-17' with binding 'no-latest-17' denied request: image tag :latest is not allowed
```

The policy manifests are in the [advanced theory](advanced.en.md#validatingadmissionpolicy-your-own-rules).

</details>
