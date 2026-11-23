---
title: "GitOps and Argo CD: expert"
lang: en
track: kubernetes
kind: module
doc: expert
slug: 20-gitops
part: 3
tags: [gitops, argocd, application-controller, repo-server, appproject, applicationset, flux, drift]
versions: "Kubernetes 1.37.0 (kind v0.34), Argo CD v3.6.2, kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# GitOps and Argo CD: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on [basic](basics.en.md) and [advanced](advanced.en.md) theory. What was confirmed by an experiment in kind with Argo CD v3.6.2 is marked "in the experiment".

## Reference

### Application fields

| Field | Meaning |
|---|---|
| `spec.project` | the Argo CD project (`default`) |
| `spec.source` | `repoURL`, `targetRevision`, `path`; `helm.parameters`; possibly `chart` and `kustomize` |
| `spec.destination` | `server` (or `name`) and `namespace` |
| `spec.syncPolicy.automated` | `prune`, `selfHeal` (in the experiment); `allowEmpty` |
| `spec.syncPolicy.syncOptions` | a list of options (in the experiment `CreateNamespace=true`) |
| `operation.sync` | a sync request; its own `syncOptions` (in the experiment) |
| `metadata.finalizers` | `resources-finalizer.argocd.argoproj.io` — cascading deletion (in the experiment) |

> ⚠️ Verify: `spec.sources` (several sources), `ignoreDifferences`, `syncPolicy.retry`, `revisionHistoryLimit`, `info`, `source.kustomize` and `destination.name` are taken from the documentation and were not run.

### Statuses

| Field | Values (seen in the experiment in bold) |
|---|---|
| `status.sync.status` | **`Synced`**, **`OutOfSync`**, `Unknown` |
| `status.health.status` | **`Healthy`**, **`Progressing`**, **`Missing`**, `Degraded`, `Suspended`, `Unknown` |
| `status.operationState.phase` | **`Succeeded`**, **`Failed`**, `Running` |
| `status.resources[]` | `kind`, `name`, `status`, `health` — for each object |
| `status.sourceType` | **`Helm`**, `Kustomize`, `Directory` |
| `status.sync.revision` | the Git commit compared against (in the experiment a SHA) |

### `kubectl` commands

| Command | Purpose |
|---|---|
| `kubectl -n argocd get app` | the list of applications: `SYNC STATUS`, `HEALTH STATUS` |
| `kubectl -n argocd get app A -o jsonpath='{.status.resources}'` | the status of objects |
| `kubectl -n argocd patch app A --type=merge -p '{"operation":{"sync":{…}}}'` | start a sync |
| `kubectl -n argocd patch app A --type=merge -p '{"spec":{"syncPolicy":…}}'` | change the policy |

## Internals

The installation consists of seven components (in the experiment):

| Component | Role |
|---|---|
| `argocd-application-controller` (StatefulSet) | compares desired (Git) and actual (cluster), performs the sync |
| `argocd-repo-server` | clones repositories and **renders** manifests (Helm, Kustomize, a directory) |
| `argocd-server` | the API and UI |
| `argocd-redis` | a cache |
| `argocd-dex-server` | single sign-on (SSO) |
| `argocd-applicationset-controller` | generates `Application`s from templates (`ApplicationSet`) |
| `argocd-notifications-controller` | notifications about application events |

- **Comparison.** The controller takes rendered manifests from the repo-server, compares them with live objects and updates `status.sync` and `status.resources`. A divergence is `OutOfSync` (in the experiment it was the object `Deployment/guestbook-ui` after `scale`).
- **Sync.** `operation.sync` triggers applying the manifests (by default `kubectl apply`). The result is written to `status.operationState`. Auto-sync only creates such an operation by itself.
- **Argo CD is an operator.** Its objects are the CRDs `applications`, `appprojects`, `applicationsets` ([module 19](../19-crd-and-operators/overview.en.md)); the state is stored in Kubernetes itself, and Git is the only external source.
- **Rendering without a Helm release.** For a Helm source Argo CD runs `helm template` itself; there are no Helm release objects in the cluster (in the experiment there was only the Deployment `helm-guestbook`).
- **Permissions.** In a basic installation the controller gets wide cluster rights. Projects (`AppProject`) limit the repositories, clusters and namespaces applications may use.

> ⚠️ Verify: the claims about controller permissions in a basic installation, `kubectl apply` as the default way of applying, the Git polling interval (about 3 minutes per the documentation) and webhooks, `AppProject` with restrictions, `ApplicationSet`, health checks and resource hooks (`PreSync`, `PostSync`), credential rotation and Argo CD RBAC were not run.

## Gotchas

### 1. Auto-sync does not retry a failed operation

After `Failed` for the same revision auto-sync is silent (in the experiment the status stayed `Failed`, `OutOfSync`, `Missing`). Fix the cause and start the sync again.

### 2. An operation does not inherit `syncOptions`

A manual `operation.sync` uses only its own `syncOptions` (in the experiment: without `CreateNamespace=true` the namespace was not created).

### 3. Deleting an application without a finalizer leaves the resources

A cascade is turned on by the finalizer (in the experiment: `helm-guestbook` stayed, `guestbook-ui` was deleted).

### 4. `selfHeal` undoes urgent manual fixes

A manual `scale` returned to the Git value within seconds (in the experiment). Make urgent changes with a commit or temporarily turn auto-sync off.

### 5. `OutOfSync` does not mean "broken"

In the experiment after a manual `scale` the application was `OutOfSync` and `Healthy` at the same time. These are independent axes.

### 6. The `stable` branch changes

The installation manifest from `stable` changes over time. For reproducibility pin a version (a release tag).

### 7. Secrets in Git

A Secret manifest only encodes values in base64 (see [module 07](../07-config-and-secrets/overview.en.md)); it must not go into Git unencrypted.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| Argo CD | a UI, a clear comparison, `ApplicationSet`, projects | a heavy installation (7 components), wide permissions |
| Flux | a compact set of controllers, native CRDs | no built-in UI; not run |
| CI with `kubectl apply`/`helm upgrade` (push) | simple, no extra components | no continuous reconcile, CI needs cluster access |
| Helm without GitOps | release history, rollback ([module 13](../13-helm/overview.en.md)) | no self-healing |
| Kustomize + Git + CI | plain YAML | the same limitation: no reconcile |

## Interview questions

<details><summary>1. What is GitOps and how does it differ from CI/CD with <code>kubectl apply</code>?</summary>

Git is the source of truth, a controller in the cluster pulls changes and keeps reconciling the state. In the push model CI applies changes once and does not watch for drift.

→ [Basic theory](basics.en.md#principles)

</details>

<details><summary>2. What does an <code>Application</code> object describe?</summary>

A source (repository, revision, path), a destination (cluster, namespace) and a sync policy.

→ [Basic theory](basics.en.md#the-first-application)

</details>

<details><summary>3. How do Sync status and Health status differ?</summary>

Sync — does the cluster match Git; Health — is the result healthy (pods ready and so on). Any combinations are possible (in the experiment `OutOfSync` + `Healthy`).

→ [Gotcha 5](#5-outofsync-does-not-mean-broken)

</details>

<details><summary>4. What does <code>selfHeal</code> do?</summary>

It restores the state from Git if an object was changed in the cluster by hand.

→ [Advanced theory](advanced.en.md#drift-and-self-heal)

</details>

<details><summary>5. Trick: an application was created but nothing was applied. Why?</summary>

There is no auto-sync, a manual sync is needed: the status is `OutOfSync`/`Missing`.

→ [Basic theory](basics.en.md#statuses-sync-and-health)

</details>

<details><summary>6. Trick: a sync failed with <code>namespaces "…" not found</code> although the application has <code>CreateNamespace=true</code>. Why?</summary>

A manual operation uses its own `syncOptions`: the option must be given in it too.

→ [Gotcha 2](#2-an-operation-does-not-inherit-syncoptions)

</details>

<details><summary>7. What happens to the resources when an <code>Application</code> is deleted?</summary>

Without a finalizer they stay; with `resources-finalizer.argocd.argoproj.io` they are deleted.

→ [Advanced theory](advanced.en.md#deleting-an-application)

</details>

<details><summary>8. How does Argo CD work with Helm charts?</summary>

The repo-server runs `helm template` with the given parameters, the controller applies the result; a Helm release is not created in the cluster.

→ [Advanced theory](advanced.en.md#helm-and-kustomize-as-a-source)

</details>

<details><summary>9. What components does Argo CD have and what is the repo-server for?</summary>

The application controller, repo-server, server, redis, dex, applicationset and notifications. The repo-server clones repositories and renders manifests.

→ [Internals](#internals)

</details>

<details><summary>10. Why can a Secret not be kept in Git "as is"?</summary>

Secret values are only base64-encoded, and Git keeps history forever. You need encryption (Sealed Secrets, SOPS) or external stores.

→ [Gotcha 7](#7-secrets-in-git)

</details>

<details><summary>11. What is drift and how is it visible?</summary>

A divergence between the cluster and Git. The application becomes `OutOfSync`, and `status.resources` shows which objects differ.

→ [Advanced theory](advanced.en.md#drift-and-self-heal)

</details>

<details><summary>12. How does Argo CD differ from Flux?</summary>

Both pull Git into the cluster; Argo CD offers a UI and `Application`/`ApplicationSet`, Flux a set of small controllers. See the documentation for more (Flux was not run in this material).

→ [Trade-offs and alternatives](#trade-offs-and-alternatives)

</details>

## Cheat sheet

```text
kubectl -n argocd get app
kubectl -n argocd patch app A --type=merge -p '{"operation":{"sync":{"syncOptions":["CreateNamespace=true"]}}}'
kubectl -n argocd patch app A --type=merge -p '{"spec":{"syncPolicy":{"automated":{"selfHeal":true}}}}'
kubectl -n argocd get app A -o jsonpath='{.status.operationState.message}'
```

- Git is the truth; the controller in the cluster pulls and reconciles.
- Sync and Health are independent axes.
- `automated`: `prune` (deletion), `selfHeal` (revert manual edits).
- Cascading deletion — a finalizer; secrets — not in plain text.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- CRDs and operators: [19 · CRDs and operators](../19-crd-and-operators/expert.en.md)
- Troubleshooting: [21 · Troubleshooting](../21-troubleshooting/overview.en.md)
- Helm and Kustomize: [13 · Helm](../13-helm/expert.en.md), [14 · Kustomize](../14-kustomize/expert.en.md)

## Sources

- [Argo CD: Architectural Overview](https://argo-cd.readthedocs.io/en/stable/operator-manual/architecture/)
- [Argo CD: Resource Health](https://argo-cd.readthedocs.io/en/stable/operator-manual/health/)
- [Argo CD: Projects](https://argo-cd.readthedocs.io/en/stable/user-guide/projects/)
- [Argo CD: ApplicationSet](https://argo-cd.readthedocs.io/en/stable/operator-manual/applicationset/)
- [Flux: Core Concepts](https://fluxcd.io/flux/concepts/)
