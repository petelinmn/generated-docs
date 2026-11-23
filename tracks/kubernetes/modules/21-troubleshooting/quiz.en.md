---
title: "Quiz: troubleshooting"
lang: en
track: kubernetes
kind: module
doc: quiz
slug: 21-troubleshooting
part: 3
tags: [troubleshooting, events, endpoints, kubectl-debug]
status: draft
updated: 2026-11-23
---

# Quiz: troubleshooting

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 A pod is in `ImagePullBackOff`. Where do you look for the cause?

- A) In the container logs
- B) In the container's `state.message` and the events of `describe pod`
- C) In `kubectl top`
- D) In the CoreDNS logs

<details><summary>Answer</summary>

**B.** There is no container, so no logs; in the experiment the `message` said `… nginx:no-such-tag: not found`.
→ [Basics](basics.en.md#imagepullbackoff-and-errimagepull)

</details>

## 2. 🟢 A pod is in `CreateContainerConfigError`: `configmap "missing-cm" not found`. What do you do?

- A) Rebuild the image
- B) Create the ConfigMap `missing-cm` (or fix the reference)
- C) Raise the memory limit
- D) Add a toleration

<details><summary>Answer</summary>

**B.** The image was pulled, but the container's configuration could not be assembled.
→ [Basics](basics.en.md#createcontainerconfigerror)

</details>

## 3. 🟢 A container ended with `exitCode: 137` and `reason: OOMKilled`. The cause?

- A) A wrong command
- B) The memory limit was exceeded
- C) No image
- D) A DNS error

<details><summary>Answer</summary>

**B.** `137 = 128 + 9` (`SIGKILL`), the cause is confirmed by `reason`.
→ [Basics](basics.en.md#oomkilled)

</details>

## 4. 🟢 A pod is `Pending` with `pod has unbound immediate PersistentVolumeClaims`. Where do you continue the search?

- A) In the pod logs
- B) In the PVC events (`describe pvc`)
- C) In `kubectl top`
- D) In the image

<details><summary>Answer</summary>

**B.** In the experiment: `storageclass … "no-such-class" not found`.
→ [Basics](basics.en.md#pending)

</details>

## 5. 🟡 `kubectl get endpoints web-typo` shows `<none>`. The likely cause?

- A) The pods have no `requests`
- B) The Service selector does not match the pod labels
- C) The pod limit is exhausted
- D) DNS is off

<details><summary>Answer</summary>

**B.** In the experiment the selector was `app: wbe` instead of `app: web`.
→ [Advanced](advanced.en.md#a-service-without-endpoints)

</details>

## 6. 🟡 A Service has endpoints `…:8080`, but requests give `Connection refused`. The cause?

- A) A wrong DNS
- B) `targetPort` does not match the port the container listens on
- C) The node is `NotReady`
- D) No quota

<details><summary>Answer</summary>

**B.** The pod addresses exist, but nobody listens on `8080`.
→ [Gotcha 4](expert.en.md#4-the-right-targetport-is-the-container-port)

</details>

## 7. 🟡 After changing the readiness probe to a nonexistent path, the rollout does not finish. What happens to traffic?

- A) It stops
- B) The old pods keep serving; the new pod is `0/1` and not in the endpoints
- C) The new pod gets all the traffic
- D) The Deployment rolls back automatically

<details><summary>Answer</summary>

**B.** In the experiment the old pods stayed `1/1`, `rollout status` ended with a timeout, and the events had `statuscode: 404`.
→ [Advanced](advanced.en.md#unready-pods-do-not-get-into-endpoints)

</details>

## 8. 🟡 What does `kubectl debug pod/P --image=busybox --target=nginx` do?

- A) Restarts the pod
- B) Adds a temporary container sharing the process namespace with `nginx`
- C) Copies the pod to another node
- D) Changes the image of the `nginx` container

<details><summary>Answer</summary>

**B.** In the experiment `ps` showed the process `nginx: master process`, and the filesystem was reachable through `/proc/1/root`.
→ [Advanced](advanced.en.md#kubectl-debug-looking-inside)

</details>

## 9. 🟡 Select all correct statements

- [ ] A) An object with a finalizer gets `deletionTimestamp` after `delete` and stays
- [ ] B) `ReplicaFailure` can be caused by an exceeded quota
- [ ] C) A `NotReady` node immediately evicts all pods
- [ ] D) `kubectl debug node/…` leaves a debugging pod in the namespace

<details><summary>Answer</summary>

**A, B, D.** C is wrong: eviction happens by `tolerationSeconds` (300 s by default), in the experiment the pods stayed `Running`.
→ [Advanced](advanced.en.md#a-notready-node)

</details>

## 10. 🔴 Find the flaw

> "Exit code 137 always means OOM, so we raise the memory."

<details><summary>Answer</summary>

`137` is `SIGKILL`; the cause is confirmed by `lastState.reason`. In the experiment a failing command gave `255` and `Error`, not `OOMKilled`.
→ [Gotcha 1](expert.en.md#1-137-is-not-always-oom)

</details>

## 11. 🔴 A Deployment is `0/5`, there are no errors in the Deployment itself and no pod events. Where is the cause?

- A) In the kubelet
- B) In `ReplicaFailure` and the ReplicaSet events (for example, the quota `exceeded quota`)
- C) In CoreDNS
- D) In Ingress

<details><summary>Answer</summary>

**B.** The pods were not created at all. In the experiment: `FailedCreate … exceeded quota: q, requested: pods=1, used: pods=4, limited: pods=4`.
→ [Advanced](advanced.en.md#quota-replicafailure)

</details>

## 12. 🔴 Open question

Describe the order of diagnosis when "the application is unavailable".

<details><summary>Key points of a good answer</summary>

- Pod status (`get`), events (`describe`), logs (including `--previous`).
- If the pod is `Running`: readiness, the Service endpoints, `targetPort`, DNS, NetworkPolicy.
- Check the node, taints, quotas and admission.
- If needed — `kubectl debug` (pod and node); finalizers for `Terminating`.

</details>

→ [Expert](expert.en.md#internals)
