---
title: "Quiz: pod and network security"
lang: en
track: kubernetes
kind: module
doc: quiz
slug: 17-security
part: 3
tags: [networkpolicy, pod-security-admission, securitycontext]
status: draft
updated: 2026-11-23
---

# Quiz: pod and network security

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 A namespace has no NetworkPolicy. What can you say about the network?

- A) All incoming traffic is closed
- B) All traffic is allowed
- C) Only traffic inside the namespace is allowed
- D) Only DNS is allowed

<details><summary>Answer</summary>

**B.** In the experiment all three clients reached the server. The network is closed with policies.
→ [Basics](basics.en.md#networkpolicy-limiting-the-network)

</details>

## 2. 🟢 How do you make a default deny for incoming traffic in a namespace?

- A) `podSelector: {}` and `policyTypes: [Ingress]` with no `ingress` rules
- B) `podSelector: {}` with `ingress: [{}]`
- C) Delete the Service
- D) Put a label on the namespace

<details><summary>Answer</summary>

**A.** An empty selector selects all pods, and no rules means "nothing is allowed". Option B, on the contrary, allows everything.
→ [Basics](basics.en.md#default-deny)

</details>

## 3. 🟢 What does `runAsNonRoot: true` mean for an image that starts as root when `runAsUser` is not set?

- A) Kubernetes changes the user itself
- B) The container does not start: `CreateContainerConfigError`
- C) Nothing, the field is optional
- D) The start proceeds with a warning

<details><summary>Answer</summary>

**B.** In the experiment: `container has runAsNonRoot and image will run as root`.
→ [Basics](basics.en.md#securitycontext-process-rights)

</details>

## 4. 🟢 `readOnlyRootFilesystem: true`, but the application needs `/tmp`. What do you do?

- A) Disable the field
- B) Mount an `emptyDir` on `/tmp`
- C) Run the pod as root
- D) Nothing: `/tmp` is exempt

<details><summary>Answer</summary>

**B.** Writing stays possible only in mounted volumes (in the experiment `/x` failed, `/tmp` on an `emptyDir` worked).
→ [Basics](basics.en.md#securitycontext-process-rights)

</details>

## 5. 🟡 `from` has one element with `namespaceSelector` and `podSelector` together. What does it mean?

- A) Any pod from the selected namespaces "or" any pod with the selected labels matches
- B) A pod that satisfies both selectors matches
- C) A validation error
- D) The selectors are ignored

<details><summary>Answer</summary>

**B.** One element is AND. Two separate elements are OR. In the experiment `client-ok` (not from the needed namespace) was blocked in the first variant and not in the second.
→ [Advanced](advanced.en.md#networkpolicy-from-namespaceselector-and-and-versus-or)

</details>

## 6. 🟡 An egress policy without rules selected a pod. What happens on `wget http://server.learn-17.svc`?

- A) The request passes
- B) Even name resolution fails (DNS is blocked)
- C) Only port 80 is blocked
- D) The block applies only to external addresses

<details><summary>Answer</summary>

**B.** In the experiment `nslookup` gave `connection timed out; no servers could be reached`. DNS has to be allowed explicitly.
→ [Advanced](advanced.en.md#egress-and-dns)

</details>

## 7. 🟡 A namespace has `enforce=restricted`, and you run `rollout restart` of a Deployment with a "violating" template. What happens?

- A) The command returns an error
- B) A new ReplicaSet without pods appears, the events show `FailedCreate`; the old pods stay
- C) All pods restart and pass the check
- D) The namespace stops accepting any objects

<details><summary>Answer</summary>

**B.** In the experiment: `dep-576fd7f57b 1 0 0`, and the events show `violates PodSecurity "restricted:latest"`.
→ [Advanced](advanced.en.md#pod-security-admission-modes-in-practice)

</details>

## 8. 🟡 Select all correct statements

- [ ] A) Changing the `enforce` label deletes existing pods
- [ ] B) `kubectl label --dry-run=server` shows which existing pods would violate the level
- [ ] C) `warn` shows a warning to the client
- [ ] D) `baseline` rejects `privileged: true`

<details><summary>Answer</summary>

**B, C, D.** A is wrong: existing pods stay, only a warning appears.
→ [Advanced](advanced.en.md#pod-security-admission-modes-in-practice)

</details>

## 9. 🟡 What does `capabilities: {drop: [NET_RAW]}` do?

- A) Changes nothing
- B) Removes the right to raw sockets; `ping` stops working
- C) Closes port 80
- D) Forbids writing to the filesystem

<details><summary>Answer</summary>

**B.** In the experiment: `ping: permission denied (are you root?)`.
→ [Advanced](advanced.en.md#capabilities-and-privileges)

</details>

## 10. 🔴 Find the flaw

> "We enabled `pod-security.kubernetes.io/enforce=restricted`, so all pods running in the namespace are now checked and safe."

<details><summary>Answer</summary>

PSA checks the creation and update of pods and does not touch running pods: when the label changes, only a warning is shown. The violators will be rejected the next time they are created, for example when pods are re-created.
→ [Gotcha 3](expert.en.md#3-enforce-does-not-touch-running-pods)

</details>

## 11. 🔴 Two NetworkPolicies select one pod: the first allows ingress from `role=frontend`, the second from `role=admin`. Who gets through?

- A) Only `frontend`
- B) Only `admin`
- C) Both `frontend` and `admin`
- D) Nobody: the policies conflict

<details><summary>Answer</summary>

**C.** Policies only allow and are additive; there are no conflicts.
→ [Gotcha 7](expert.en.md#7-policies-only-allow)

</details>

## 12. 🔴 Open question

How would you protect a namespace with an application: what would you enable at the network, pod and admission levels?

<details><summary>Key points of a good answer</summary>

- Network: `default-deny` for ingress (and egress with DNS if needed), then targeted allowances.
- Pods: non-root, `allowPrivilegeEscalation: false`, `drop: [ALL]`, seccomp `RuntimeDefault`, read-only rootfs.
- Admission: PSA through `warn`/`audit`, then `enforce: restricted`; ValidatingAdmissionPolicy for your own rules.
- API access: a dedicated ServiceAccount with minimal rights, the token disabled where it is not needed.

</details>

→ [Expert](expert.en.md#trade-offs-and-alternatives)
