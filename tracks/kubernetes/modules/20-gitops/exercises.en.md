---
title: "Exercises: GitOps and Argo CD"
lang: en
track: kubernetes
kind: module
doc: exercises
slug: 20-gitops
part: 3
tags: [gitops, argocd, sync, self-heal, helm]
status: draft
updated: 2026-11-23
---

# Exercises: GitOps and Argo CD

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself; hints and solutions are under spoilers. You need a kind cluster and internet access (installing Argo CD and cloning the public repository `argocd-example-apps`). Afterwards: `kubectl delete namespace argocd learn-20` and delete the `*.argoproj.io` CRDs.

## 1. 🟢 Installing Argo CD

**Task:** install Argo CD into the namespace `argocd` with the `install.yaml` manifest (with `--server-side`) and wait until it is ready. List the pods and CRDs.

**Expected result:** seven components `Running` (application-controller, applicationset-controller, dex, notifications, redis, repo-server, server) and the CRDs `applications`, `applicationsets`, `appprojects`.

<details><summary>Hint</summary>

`kubectl -n argocd wait --for=condition=Available deploy --all`.

</details>

<details><summary>Solution</summary>

The commands are in the [basic theory](basics.en.md#installing-argo-cd).

</details>

## 2. 🟢 The first Application

**Task:** create an `Application` for `guestbook` and look at the statuses before the sync.

**Expected result:** `OutOfSync` / `Missing`; the resources `Service/guestbook-ui` and `Deployment/guestbook-ui` are `OutOfSync`.

<details><summary>Hint</summary>

`kubectl -n argocd get app guestbook -o jsonpath='{range .status.resources[*]}{.kind}/{.name} {.status}{"\n"}{end}'`.

</details>

<details><summary>Solution</summary>

```text
guestbook   OutOfSync   Missing
Service/guestbook-ui OutOfSync
Deployment/guestbook-ui OutOfSync
```

</details>

## 3. 🟡 Sync

**Task:** run a sync by writing to `operation`. First without `syncOptions`, then with `CreateNamespace=true`. Explain the difference.

**Expected result:** first `Failed … namespaces "learn-20" not found`; then `Succeeded`, `Synced` / `Progressing` → `Healthy`.

<details><summary>Hint</summary>

`kubectl -n argocd patch app guestbook --type=merge -p '{"operation":{"sync":{}}}'`.

</details>

<details><summary>Solution</summary>

```text
Failed | one or more objects failed to apply, reason: namespaces "learn-20" not found
… with syncOptions: Succeeded
```

</details>

## 4. 🟡 Drift and self-heal

**Task:** without auto-sync, `kubectl scale` `guestbook-ui` to 3 replicas and look at the status. Enable `selfHeal` and wait for the return.

**Expected result:** `OutOfSync` / `Healthy` and `replicas=3`; after `selfHeal` — `Synced` / `Healthy` and `replicas=1`.

<details><summary>Hint</summary>

`kubectl -n argocd patch app guestbook --type=merge -p '{"spec":{"syncPolicy":{"automated":{"selfHeal":true}}}}'`.

</details>

<details><summary>Solution</summary>

```text
guestbook   OutOfSync   Healthy     (replicas 3)
guestbook   Synced      Healthy     (replicas=1)
```

</details>

## 5. 🔴 A Helm source and deletion

**Task:** create an `Application` `helm-guestbook` (path `helm-guestbook`, parameter `replicaCount=2`, `automated` with `prune` and `selfHeal`). Then delete it without a finalizer and add a finalizer to `guestbook` before deleting it. What is left in `learn-20`?

**Expected result:** `sourceType = Helm`, `helm-guestbook 2/2`; after deletion `helm-guestbook` stays (no finalizer), `guestbook-ui` is deleted (with a finalizer).

<details><summary>Hint</summary>

The finalizer: `resources-finalizer.argocd.argoproj.io`.

</details>

<details><summary>Solution</summary>

The manifest and the result are in the [advanced theory](advanced.en.md#helm-and-kustomize-as-a-source) and [here](advanced.en.md#deleting-an-application).

</details>
