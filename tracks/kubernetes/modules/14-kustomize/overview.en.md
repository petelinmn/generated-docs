---
title: "Kustomize"
lang: en
track: kubernetes
kind: module
doc: overview
slug: 14-kustomize
part: 2
levels: [1, 2, 3, 4]
tags: [kustomize, overlay, patch, configmap-generator, base, environments]
versions: "Kustomize 5.8.1 (bundled with kubectl 1.36.1), Kubernetes 1.37.0 (kind v0.34)"
status: draft
updated: 2026-11-23
---

# Kustomize

🌐 [Русский](overview.ru.md) · **English**

## In short

Kustomize customizes plain manifests without templates: there is a **base** (shared YAML) and **overlays** (environments) that change the base with patches, name prefixes, labels, images and ConfigMap generators. It is built into kubectl: `kubectl kustomize <dir>` prints the result, `kubectl apply -k <dir>` applies it. Unlike [Helm](../13-helm/overview.en.md) there are no releases and no history, but the source files remain valid YAML.

## Key ideas

- **base + overlays.** The shared set of manifests lives in `base/`, the differences between environments in `overlays/dev`, `overlays/prod`.
- **A `kustomization.yaml` file** lists `resources` and transformations; a build does not change the source files.
- **Transformers:** `namespace`, `namePrefix`, `labels`, `images`, `replicas`, `patches`, `replacements`.
- **ConfigMap/Secret generators add a hash suffix** to the name and rewrite references to it: a configuration change triggers a rolling update.
- **Two kinds of patches:** strategic merge (a piece of a manifest) and JSON 6902 (a list of operations); the target is chosen with `target`.
- **Components (`Component`)** are reusable pieces plugged into different overlays.
- **`kubectl apply -k` does not delete previous ConfigMaps:** old versions with another hash stay.

## Minimal example

```text
$ cat overlays/prod/kustomization.yaml
resources: [../../base]
namespace: learn-14-prod
namePrefix: prod-
replicas: [{name: web, count: 3}]
images: [{name: nginx, newTag: "1.29"}]
$ kubectl kustomize overlays/prod        # look at the result
$ kubectl apply -k overlays/prod         # apply
configmap/prod-app-config-dfk4bdbtkk created
service/prod-web created
deployment.apps/prod-web created
```

## When to use and when not

| Situation | Solution |
|---|---|
| Your own service in dev/stage/prod with small differences | Kustomize |
| You need releases, rollbacks, hooks, third-party charts | [Helm](../13-helm/overview.en.md) |
| Tweaking someone else's Helm chart without a fork | Helm + Kustomize |

- ❌ Do not turn overlays into copies of the base: change only the differences.
- ❌ Do not count on history and rollback: they are provided by Git or GitOps.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | base and overlay, `kustomization.yaml`, namespace, prefix, labels, images, replicas, `apply -k` |
| [2. Advanced theory](advanced.en.md) | patches, hashed generators, replacements, components, error diagnosis |
| [3. Expert theory](expert.en.md) | field reference, output order, load restrictions, pitfalls, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn kubernetes 14-kustomize level 5` |
| 6. Tests | not created yet — `/learn kubernetes 14-kustomize level 6` |

## Related materials

- Prerequisites: [05 · Deployment](../05-deployments/overview.en.md), [07 · ConfigMap and Secret](../07-config-and-secrets/overview.en.md)
- Previous module: [13 · Helm](../13-helm/overview.en.md)
- Next: [15 · Autoscaling](../15-autoscaling/overview.en.md)
- Rollout on a configuration change: [07 · ConfigMap and Secret](../07-config-and-secrets/advanced.en.md#a-configmap-change-does-not-restart-pods)
- GitOps and Argo CD: [20 · GitOps](../20-gitops/overview.en.md)
