---
title: "GitOps and Argo CD: basics"
lang: en
track: kubernetes
kind: module
doc: basics
slug: 20-gitops
part: 3
tags: [gitops, argocd, application, sync, declarative, drift]
versions: "Kubernetes 1.37.0 (kind v0.34), Argo CD v3.6.2, kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# GitOps and Argo CD: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [05 · Deployment](../05-deployments/overview.en.md), [19 · CRDs and operators](../19-crd-and-operators/overview.en.md) · **Time:** ~2 h

All examples were run in kind (Kubernetes 1.37.0), Argo CD v3.6.2. The manifest source is the public repository `argoproj/argocd-example-apps` (the `guestbook` application), so you do not need your own repository for the experiments.

## Why it matters

Without GitOps a rollout looks like this: someone runs `kubectl apply` or a CI script. Later it is unclear what exactly is in the cluster now, who changed it and when, and how to get the previous state back. GitOps answers: "what is in Git is the truth; the cluster must match it". From that follow audit (commit history), rollback (revert), reproducibility (the same repository creates the same cluster) and self-healing.

## Principles

1. **Declarative:** the desired state is described by manifests, not commands.
2. **Versioned:** manifests live in Git, every change has an author and history.
3. **Automatic pull:** a controller in the cluster fetches changes itself.
4. **Continuous reconciliation:** the controller keeps comparing the actual state with the desired one and removes divergences.

## Installing Argo CD

```text
$ kubectl create namespace argocd
$ kubectl apply -n argocd --server-side \
    -f https://raw.githubusercontent.com/argoproj/argo-cd/stable/manifests/install.yaml
$ kubectl -n argocd get pods
argocd-application-controller-0                    1/1   Running
argocd-applicationset-controller-76c9bbbc9f-kkk82  1/1   Running
argocd-dex-server-c5c68fc58-mtbvs                  1/1   Running
argocd-notifications-controller-67bf66d585-78dmn   1/1   Running
argocd-redis-665bf4467d-wb62t                      1/1   Running
argocd-repo-server-7fb4dfb7fb-s92mr                1/1   Running
argocd-server-7c54fc4fd9-qhrxs                     1/1   Running
$ kubectl get crd | grep argoproj
applications.argoproj.io   applicationsets.argoproj.io   appprojects.argoproj.io
```

The manifest was taken from the `stable` branch, and in the experiment it is the image `quay.io/argoproj/argocd:v3.6.2`. For reproducibility in production pin a version, not a branch. Seven components and three CRDs were installed; their roles are in the [expert theory](expert.en.md#internals).

## The first Application

```yaml
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata: {name: guestbook, namespace: argocd}
spec:
  project: default
  source:
    repoURL: https://github.com/argoproj/argocd-example-apps.git
    targetRevision: HEAD
    path: guestbook
  destination: {server: https://kubernetes.default.svc, namespace: learn-20}
  syncPolicy:
    syncOptions: [CreateNamespace=true]
```

- `source` — where the manifests are (repository, revision, path).
- `destination` — where to apply: `https://kubernetes.default.svc` means "the cluster where Argo CD runs".
- `project: default` — an Argo CD project (boundaries: which repositories and clusters are allowed).
- `syncOptions: [CreateNamespace=true]` — create the destination namespace if it does not exist.

An Application is an object in the `argocd` namespace, not in the application's namespace.

## Statuses: Sync and Health

Right after creation:

```text
$ kubectl -n argocd get app guestbook --no-headers
guestbook   OutOfSync   Missing
$ kubectl -n argocd get app guestbook -o jsonpath='{range .status.resources[*]}{.kind}/{.name} {.status}{"\n"}{end}'
Service/guestbook-ui OutOfSync
Deployment/guestbook-ui OutOfSync
```

- **Sync status:** `Synced` — the cluster matches Git, `OutOfSync` — it does not.
- **Health status:** `Healthy`, `Progressing`, `Degraded`, `Missing` (the object is not in the cluster) and others.

Argo CD read Git, saw two objects, but they are not in the cluster: `OutOfSync` and `Missing`. There is no automatic sync by default, so nothing has been applied yet.

## Sync

A sync applies the manifests from Git. It is started with the command `argocd app sync` (the CLI) or in the UI. Without the CLI, the same is done by writing the `operation` field of the application:

```text
$ kubectl -n argocd patch app guestbook --type=merge \
    -p '{"operation":{"sync":{"syncOptions":["CreateNamespace=true"]}}}'
$ kubectl -n argocd get app guestbook --no-headers
guestbook   Synced   Progressing
$ kubectl -n learn-20 get deploy,svc --no-headers
deployment.apps/guestbook-ui   0/1
service/guestbook-ui           ClusterIP
$ kubectl -n argocd get app guestbook -o jsonpath='{.status.operationState.phase}'
Succeeded
```

`Synced` — the manifests are applied; `Progressing` — the Deployment is still starting a pod; then `Healthy`.

The first sync attempt in the experiment ended with an error:

```text
one or more objects failed to apply, reason: namespaces "learn-20" not found
```

The cause: in a manual operation the option `CreateNamespace=true` must be given explicitly (the application's policy is not inherited in the operation), more in the [advanced theory](advanced.en.md#sync-options-and-operations).

## Beginner mistakes

- ❌ Expecting an `Application` to apply the manifests itself → ✅ a sync is needed (manual or `automated`).
- ❌ Confusing `OutOfSync` (divergence from Git) with `Degraded` (the application is unhealthy).
- ❌ Taking manifests from `HEAD` without pinning in production → ✅ a tag or commit in `targetRevision`.
- ❌ Creating an `Application` outside `argocd` → ✅ by default Argo CD watches its own namespace.
- ❌ Editing objects in the cluster by hand → ✅ change Git.

## Related materials

- Next: [advanced theory](advanced.en.md) — auto-sync, self-heal, Helm, deletion
- CRDs and operators: [19 · CRDs and operators](../19-crd-and-operators/overview.en.md)
- Deployment: [05 · Deployment](../05-deployments/overview.en.md)

## Sources

- [Argo CD: Getting Started](https://argo-cd.readthedocs.io/en/stable/getting_started/)
- [Argo CD: Declarative Setup](https://argo-cd.readthedocs.io/en/stable/operator-manual/declarative-setup/)
- [Argo CD: Automated Sync Policy](https://argo-cd.readthedocs.io/en/stable/user-guide/auto_sync/)
- [argocd-example-apps](https://github.com/argoproj/argocd-example-apps)
- [OpenGitOps: principles](https://opengitops.dev/)
