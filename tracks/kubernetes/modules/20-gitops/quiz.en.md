---
title: "Quiz: GitOps and Argo CD"
lang: en
track: kubernetes
kind: module
doc: quiz
slug: 20-gitops
part: 3
tags: [gitops, argocd, sync, self-heal]
status: draft
updated: 2026-11-23
---

# Quiz: GitOps and Argo CD

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What is the source of truth in GitOps?

- A) The cluster state
- B) The Git repository
- C) The container image
- D) The Argo CD UI

<details><summary>Answer</summary>

**B.** The cluster must match Git.
→ [Basics](basics.en.md#principles)

</details>

## 2. 🟢 What does an `Application` describe?

- A) Only a container image
- B) A source (repository, revision, path) and a destination (cluster, namespace)
- C) Only access rights
- D) Only Helm values

<details><summary>Answer</summary>

**B.** Plus the sync policy.
→ [Basics](basics.en.md#the-first-application)

</details>

## 3. 🟢 An application has just been created, no auto-sync. Which statuses?

- A) `Synced` / `Healthy`
- B) `OutOfSync` / `Missing`
- C) `Degraded` / `Unknown`
- D) `Failed`

<details><summary>Answer</summary>

**B.** The manifests are read, but they are not in the cluster.
→ [Basics](basics.en.md#statuses-sync-and-health)

</details>

## 4. 🟢 What does `Progressing` mean in Health status?

- A) A divergence from Git
- B) The resources are applied but not ready yet (for example, a pod is starting)
- C) An apply error
- D) The application is being deleted

<details><summary>Answer</summary>

**B.** It is health, not a match with Git.
→ [Basics](basics.en.md#sync)

</details>

## 5. 🟡 You ran `kubectl scale` by hand on an application without auto-sync. What does Argo CD show?

- A) `Synced` / `Healthy`
- B) `OutOfSync` / `Healthy`, the `Deployment` differs from Git
- C) `Degraded`
- D) Nothing

<details><summary>Answer</summary>

**B.** That is what happened in the experiment.
→ [Advanced](advanced.en.md#drift-and-self-heal)

</details>

## 6. 🟡 After `selfHeal` was turned on, the replicas returned to the Git value. This is…

- A) An Argo CD bug
- B) Expected behavior: a manual edit is considered drift and reverted
- C) HPA at work
- D) The kubelet at work

<details><summary>Answer</summary>

**B.** For urgent changes edit Git.
→ [Advanced](advanced.en.md#drift-and-self-heal)

</details>

## 7. 🟡 A manual `sync` operation failed: `namespaces "learn-20" not found`, although `syncPolicy` has `CreateNamespace=true`. Why?

- A) The option is not supported
- B) The operation uses its own `syncOptions`, they must be given explicitly
- C) The namespace is closed by RBAC
- D) The repository is unreachable

<details><summary>Answer</summary>

**B.** In the experiment with `syncOptions` in the operation the sync passed.
→ [Advanced](advanced.en.md#sync-options-and-operations)

</details>

## 8. 🟡 You deleted an `Application` without a finalizer. What about the resources?

- A) Deleted
- B) Left in the cluster
- C) Archived
- D) Frozen

<details><summary>Answer</summary>

**B.** A cascade is turned on by `resources-finalizer.argocd.argoproj.io`.
→ [Advanced](advanced.en.md#deleting-an-application)

</details>

## 9. 🟡 Select all correct statements

- [ ] A) Argo CD can render a Helm chart and apply the result
- [ ] B) With a Helm source a Helm release is created in the cluster
- [ ] C) `OutOfSync` and `Healthy` can be true at the same time
- [ ] D) Auto-sync retries a failed operation for the same revision

<details><summary>Answer</summary>

**A, C.** B is wrong: Argo CD applies the result of `helm template`, there are no release objects. D is wrong: in the experiment the status stayed `Failed`.
→ [Advanced](advanced.en.md#helm-and-kustomize-as-a-source)

</details>

## 10. 🔴 Find the flaw

> "We use GitOps, so secrets happily live in Git as Secret manifests."

<details><summary>Answer</summary>

A Secret holds values in base64 — that is encoding, not encryption; Git keeps history forever. You need encryption (Sealed Secrets, SOPS) or an external store.
→ [Gotcha 7](expert.en.md#7-secrets-in-git)

</details>

## 11. 🔴 Which Argo CD component renders Helm and Kustomize?

- A) argocd-server
- B) argocd-repo-server
- C) argocd-redis
- D) argocd-dex-server

<details><summary>Answer</summary>

**B.** It clones repositories and renders manifests; the controller compares the result with the cluster.
→ [Internals](expert.en.md#internals)

</details>

## 12. 🔴 Open question

Compare a push deploy from CI with GitOps using Argo CD.

<details><summary>Key points of a good answer</summary>

- Push: CI applies changes once, needs cluster access, does not see drift.
- GitOps: a controller in the cluster pulls Git, keeps reconciling, shows drift, reverts manual edits; history and audit live in Git.
- The price of GitOps: an extra component, permissions, secret handling.

</details>

→ [Expert](expert.en.md#trade-offs-and-alternatives)
