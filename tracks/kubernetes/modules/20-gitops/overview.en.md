---
title: "GitOps and Argo CD"
lang: en
track: kubernetes
kind: module
doc: overview
slug: 20-gitops
part: 3
levels: [1, 2, 3, 4]
tags: [gitops, argocd, application, sync, self-heal, drift, declarative, continuous-delivery]
versions: "Kubernetes 1.37.0 (kind v0.34), Argo CD v3.6.2, kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# GitOps and Argo CD

🌐 [Русский](overview.ru.md) · **English**

## In short

**GitOps** is a way of delivering to a cluster where the only source of truth is a Git repository. It holds manifests of the desired state, and a dedicated controller inside the cluster continuously compares them with the actual state and brings the cluster in line (the pull model). A change is made by a commit, a rollback is a revert of a commit, and manual edits in the cluster count as drift, which is visible and can be fixed automatically. We cover **Argo CD**: it is installed in the cluster and describes deployments with `Application` objects.

## Key ideas

- **Git is the source of truth.** The desired state is described declaratively and versioned.
- **The controller pulls changes itself** rather than receiving them from CI. The cluster needs no inbound access from CI.
- **An `Application` in Argo CD links a source and a target:** a repository, path and revision → a cluster and a namespace.
- **Two statuses:** `Sync` (does the cluster match Git: `Synced`, `OutOfSync`) and `Health` (is the result healthy: `Healthy`, `Progressing`, `Missing`).
- **Automatic sync (`automated`)** applies changes from Git by itself; **`selfHeal`** reverts manual edits in the cluster.
- **A source can be plain manifests, Helm or Kustomize.**
- **Deleting an `Application` does not delete the resources by default:** a cascade needs a finalizer.
- **Argo CD is itself an operator:** it consists of controllers and CRDs (`Application`, `AppProject`, `ApplicationSet`).

## Minimal example

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
    automated: {selfHeal: true}
```

```text
$ kubectl -n argocd get app guestbook
NAME        SYNC STATUS   HEALTH STATUS
guestbook   Synced        Healthy
```

The example uses the public examples repository of the Argo CD project.

## When to use and when not

| Situation | Solution |
|---|---|
| You need audit, rollback and reproducible environments | GitOps |
| Many clusters or environments with shared manifests | GitOps + Helm/Kustomize |
| A one-off experiment on a local cluster | `kubectl apply` |
| Secrets in plain text | must not go to Git (needs encryption or an external store) |

- ❌ Do not edit the application with `kubectl edit`: with `selfHeal` the edit disappears, without it you get a divergence.
- ❌ Do not put secrets into Git unencrypted.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | GitOps principles, installing Argo CD, the first `Application`, sync |
| [2. Advanced theory](advanced.en.md) | auto-sync, self-heal and drift, sync options, Helm as a source, deleting an application |
| [3. Expert theory](expert.en.md) | reference of fields and statuses, Argo CD internals, pitfalls, Flux and other approaches, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn kubernetes 20-gitops level 5` |
| 6. Tests | not created yet — `/learn kubernetes 20-gitops level 6` |

## Related materials

- Prerequisites: [05 · Deployment](../05-deployments/overview.en.md), [13 · Helm](../13-helm/overview.en.md), [14 · Kustomize](../14-kustomize/overview.en.md), [19 · CRDs and operators](../19-crd-and-operators/overview.en.md)
- Rights and namespaces: [10 · Namespaces and RBAC](../10-namespaces-and-rbac/overview.en.md)
- Previous module: [19 · CRDs and operators](../19-crd-and-operators/overview.en.md)
- Next: [21 · Troubleshooting](../21-troubleshooting/overview.en.md)
- Secrets: [07 · ConfigMap and Secret](../07-config-and-secrets/overview.en.md)
- [System design: multi-region and recovery](../../../system-design/modules/16-multi-region-and-dr/overview.en.md)
