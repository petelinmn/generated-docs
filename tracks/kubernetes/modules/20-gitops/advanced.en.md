---
title: "GitOps and Argo CD: advanced"
lang: en
track: kubernetes
kind: module
doc: advanced
slug: 20-gitops
part: 3
tags: [argocd, auto-sync, self-heal, drift, sync-options, helm, finalizer, prune]
versions: "Kubernetes 1.37.0 (kind v0.34), Argo CD v3.6.2, kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# GitOps and Argo CD: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on [basic theory](basics.en.md). Experiments were run in kind, the source is the repository `argoproj/argocd-example-apps`.

## Automatic sync

Auto-sync is enabled in `syncPolicy`:

```yaml
syncPolicy:
  syncOptions: [CreateNamespace=true]
  automated:
    prune: false       # delete what disappeared from Git
    selfHeal: false    # revert manual edits in the cluster
```

With `automated` Argo CD applies a new revision from Git without a command. What the flags do:

- `prune` — delete from the cluster objects that are no longer in Git. Off by default: deletion is dangerous.
- `selfHeal` — restore the state from Git if an object was changed in the cluster.

> ⚠️ Verify: `prune` and the repository polling interval (about three minutes per the documentation) were not checked in the experiments: a public repository has no commits that you could change.

## Drift and self-heal

**Drift** is a divergence between the cluster and Git. Experiment: after a sync `guestbook-ui` has 1 replica in Git. Auto-sync was turned off and the Deployment was scaled by hand:

```text
$ kubectl -n learn-20 scale deploy guestbook-ui --replicas=3
… after 30 seconds …
$ kubectl -n argocd get app guestbook --no-headers
guestbook   OutOfSync   Healthy
$ kubectl -n argocd get app guestbook -o jsonpath='{range .status.resources[?(@.status=="OutOfSync")]}{.kind}/{.name}{"\n"}{end}'
Deployment/guestbook-ui
$ kubectl -n learn-20 get deploy guestbook-ui -o jsonpath='{.spec.replicas}'
3
```

The application is healthy (`Healthy`) but does not match Git (`OutOfSync`), and Argo CD shows which object differs. Now we enable `selfHeal`:

```text
$ kubectl -n argocd patch app guestbook --type=merge \
    -p '{"spec":{"syncPolicy":{"automated":{"selfHeal":true}}}}'
… after 5–10 seconds …
guestbook   Synced   Healthy   replicas=1
```

Argo CD returned 1 replica by itself. This is the main difference between GitOps and "applied once": you cannot change the cluster by hand for long.

## Sync options and operations

`syncPolicy.syncOptions` change the sync behavior; the experiment used `CreateNamespace=true` (create the destination namespace).

A catch with a manual operation. The first sync through writing `operation: {sync: {}}` ended like this:

```text
$ kubectl -n argocd get app guestbook -o jsonpath='{.status.operationState.phase}{" | "}{.status.operationState.message}'
Failed | one or more objects failed to apply, reason: namespaces "learn-20" not found
```

The option `CreateNamespace=true` was in the application's `syncPolicy`, but an operation created by hand uses **its own** `syncOptions`. Then `automated` was enabled, and the status still stayed `Failed … OutOfSync Missing`: auto-sync does not retry a failed operation for the same revision. What helped was to give the option explicitly in the operation:

```text
$ kubectl -n argocd patch app guestbook --type=merge \
    -p '{"operation":{"sync":{"syncOptions":["CreateNamespace=true"]}}}'
… operationState.phase = Succeeded
```

> ⚠️ Verify: how `argocd app sync` and the UI deal with this (per the documentation they take the application's policy into account) was not checked in the experiment: the CLI was not used.

> ⚠️ Verify: the other options (`Prune=false`, `ServerSideApply=true`, `Replace=true`, `ApplyOutOfSyncOnly=true`), `retry` and `syncWaves`/hooks are taken from the documentation; only `CreateNamespace=true` was verified by running.

## Helm and Kustomize as a source

Argo CD detects the source type from the directory contents. For a directory with a Helm chart you set values in `source.helm`:

```yaml
source:
  repoURL: https://github.com/argoproj/argocd-example-apps.git
  targetRevision: HEAD
  path: helm-guestbook
  helm:
    parameters: [{name: replicaCount, value: "2"}]
destination: {server: https://kubernetes.default.svc, namespace: learn-20}
syncPolicy:
  automated: {prune: true, selfHeal: true}
```

```text
$ kubectl -n argocd get app helm-guestbook -o jsonpath='{.status.sourceType}'
Helm
$ kubectl -n learn-20 get deploy
guestbook-ui     1/1
helm-guestbook   2/2
```

The parameter `replicaCount=2` reached the Deployment. Argo CD ran `helm template` itself and applied the result: there are no Helm releases in the cluster ([module 13](../13-helm/advanced.en.md) shows what a Helm release looks like).

> ⚠️ Verify: a Kustomize source and several sources (`sources`) were not run.

## Deleting an Application

What happens to the resources when an `Application` is deleted depends on the finalizer:

```text
$ kubectl -n argocd delete app helm-guestbook      # without a finalizer
$ kubectl -n learn-20 get deploy
guestbook-ui     1/1
helm-guestbook   2/2      ← the resources stayed

$ kubectl -n argocd patch app guestbook --type=merge \
    -p '{"metadata":{"finalizers":["resources-finalizer.argocd.argoproj.io"]}}'
$ kubectl -n argocd delete app guestbook           # with a finalizer
$ kubectl -n learn-20 get deploy
helm-guestbook   2/2      ← guestbook-ui was deleted with the application
```

Without a finalizer the `Application` is deleted while the resources stay "orphaned". The finalizer `resources-finalizer.argocd.argoproj.io` turns on cascading deletion. When it was added, Kubernetes issued a warning (`prefer a domain-qualified finalizer name including a path`): it is a suggestion, not an error.

## Best practices

- One repository (or directory) per environment; shared manifests through Kustomize or Helm.
- Pin the revision (`targetRevision`) with a tag or commit in production.
- Enable `selfHeal` deliberately: urgent manual fixes will stop working.
- Enable `prune` only when you are sure of the Git contents.
- Secrets — through encryption (Sealed Secrets, SOPS) or external stores; not in plain text.
- Argo CD must not be the only point of access: keep a backup of its configuration.

> ⚠️ Verify: Sealed Secrets, SOPS, External Secrets, ApplicationSet and "app of apps" were not run (the CRD `applicationsets.argoproj.io` exists and was installed).

## Common mistakes

- ❌ A manual operation without the needed `syncOptions` → ✅ give them explicitly or use the CLI.
- ❌ Waiting for a failed auto-sync to retry → ✅ fix the cause and start the sync again.
- ❌ Deleting an `Application` and expecting the resources to vanish → ✅ the finalizer.
- ❌ Editing a resource in the cluster with `selfHeal` on → ✅ edit Git.
- ❌ Applying `HEAD` without pinning → ✅ a tag or a commit hash.

## Related materials

- Next: [expert theory](expert.en.md)
- Helm: [13 · Helm](../13-helm/overview.en.md)
- Kustomize: [14 · Kustomize](../14-kustomize/overview.en.md)
- Deletion and finalizers: [19 · CRDs and operators](../19-crd-and-operators/expert.en.md#internals)

## Sources

- [Argo CD: Automated Sync Policy](https://argo-cd.readthedocs.io/en/stable/user-guide/auto_sync/)
- [Argo CD: Sync Options](https://argo-cd.readthedocs.io/en/stable/user-guide/sync-options/)
- [Argo CD: App Deletion](https://argo-cd.readthedocs.io/en/stable/user-guide/app_deletion/)
- [Argo CD: Helm](https://argo-cd.readthedocs.io/en/stable/user-guide/helm/)
- [Argo CD: Application Specification](https://argo-cd.readthedocs.io/en/stable/operator-manual/application.yaml)
