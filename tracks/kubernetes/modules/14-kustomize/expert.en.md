---
title: "Kustomize: expert theory"
lang: en
track: kubernetes
kind: module
doc: expert
slug: 14-kustomize
part: 2
tags: [kustomize, kustomization, patches, generators, replacements, components, load-restrictor]
versions: "Kustomize 5.8.1 (bundled with kubectl 1.36.1), Kubernetes 1.37.0 (kind v0.34)"
status: draft
updated: 2026-11-23
---

# Kustomize: expert theory

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on [basic](basics.en.md) and [advanced](advanced.en.md) theory. What was verified by running in Kustomize 5.8.1 is marked "in the experiment".

## `kustomization.yaml` field reference

| Field | Meaning | Status in this material |
|---|---|---|
| `resources` | files, folders and URLs to build | verified (files, a folder with `kustomization.yaml`) |
| `components` | `Component`s to plug in | verified |
| `namespace` | namespace for all objects | verified |
| `namePrefix`, `nameSuffix` | name prefix and suffix | `namePrefix` verified |
| `labels` | labels with `pairs` and `includeSelectors` | verified |
| `commonLabels` | deprecated: adds labels to selectors as well | the warning `'commonLabels' is deprecated. Please use 'labels' instead` |
| `images` | `name`, `newName`, `newTag`, `digest` | `newTag` verified |
| `replicas` | `name` and `count` | verified |
| `patches` | `path` or `patch` and `target` | verified |
| `configMapGenerator`, `secretGenerator` | `literals`, `files`, `envs`, `behavior` | `literals` verified |
| `generatorOptions` | `disableNameSuffixHash`, `labels`, `annotations` | `disableNameSuffixHash` verified |
| `replacements` | copying values between fields | verified |
| `helmCharts` | plugging in Helm charts (needs `--enable-helm`) | not tested |
| `commonAnnotations`, `crds`, `configurations`, `openapi`, `sortOptions`, `buildMetadata`, `vars` | per the documentation | not tested |

> ⚠️ Verify: the purpose and syntax of the fields marked "not tested" are taken from the Kustomize documentation; no experiments were run on them. The field `vars` and the separate `patchesStrategicMerge`/`patchesJson6902` are marked in the documentation as deprecated in favor of `replacements` and `patches`.

### kubectl commands

| Command | Purpose |
|---|---|
| `kubectl kustomize <dir>` | print the build result |
| `kubectl apply -k <dir>` | apply the result |
| `kubectl diff -k <dir>` | show the difference from the cluster |
| `kubectl get -k <dir>`, `kubectl delete -k <dir>` | objects from the result |

Flags of `kubectl kustomize` (from help): `--enable-helm` (default `false`), `--enable-alpha-plugins` (`false`), `--load-restrictor` (default `LoadRestrictionsRootOnly`), `--as-current-user`.

## Internals

- **A build is a pure function.** `kubectl kustomize` reads files, applies transformations and prints YAML; no cluster is needed. The source files do not change.
- **Resource accumulation.** `resources` are gathered into a common set where each object has an identifier (group, version, kind, name, namespace). A duplicate is forbidden: `may not add resource with an already registered id: Deployment.v1.apps/web.[noNs]`.
- **References between objects (name reference).** After `namePrefix` and generator hashing Kustomize updates references: the ConfigMap name in `envFrom` became `prod-app-config-dfk4bdbtkk` (in the experiment).
- **Content hash.** The suffix is computed from the ConfigMap data: `LOG_LEVEL=warn` → `dfk4bdbtkk`, `debug` → `47668c6k28`. A data change — a new name — a new pod template — a rolling update.
- **Output order.** In the experiment objects came in the order ConfigMap, Secret, Service, Deployment (not in the file order of `resources`): Kustomize sorts by kind.
- **`apply -k` is an ordinary `apply`.** It creates and updates objects but does not delete those that are no longer in the result (the old ConfigMap stayed). Deleting obsolete objects is done by prune or GitOps ([module 20 of the roadmap](../../overview.en.md)).
- **Patch by `target`.** The target is chosen by `kind`, `name` and other `target` fields; with a `target` present, the name in the body of a strategic-merge patch does not determine the target (in the experiment `ignored`).

## Pitfalls

### 1. `commonLabels` changes selectors

In the experiment `commonLabels: {team: a}` gave five occurrences of the label: in object labels, in the Service `selector`, in `selector.matchLabels` and in the Deployment's template labels. A Deployment selector is immutable, so `commonLabels` cannot be added to a live object. Use `labels` without `includeSelectors`.

### 2. The hash does not protect from forgotten ConfigMaps

After `apply -k` with a new configuration both ConfigMaps remain (`dfk4bdbtkk` and `47668c6k28`); after `delete -k` the old version stayed: it was not in the build.

### 3. A patch is not silently skipped — it fails

A patch for a non-existent object is not ignored: `no resource matches strategic merge patch "Deployment.v1.apps/ghost.[noNs]"`. The message names the object identifier.

### 4. The namespace is not created

`namespace: learn-14-missing` without an existing namespace — `namespaces "learn-14-missing" not found` on every object.

### 5. Leaving the folder

The file `../../app/base/service.yaml` in `resources` is rejected (`security; file … is not in or below …`): reference folders, not files in foreign ones.

### 6. `disableNameSuffixHash` turns the restart off

Without the hash a ConfigMap change does not change the pod template (the name is the same), and pods do not restart ([module 07](../07-config-and-secrets/advanced.en.md#a-configmap-change-does-not-restart-pods)).

### 7. A generated Secret is base64

`password: czNjcmV0` decodes trivially; the value sits in `kustomization.yaml`.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| Kustomize | plain YAML, built into kubectl, no templates | no releases, history, packages; complex cases go through patches |
| [Helm](../13-helm/overview.en.md) | parameters, history, ready charts | templates, harder to read |
| Manifest copies | simple | environments drift apart |
| Helm + Kustomize | changes to foreign charts without a fork | two tools (`--enable-helm`) |

## Interview questions

<details><summary>1. What are a base and an overlay?</summary>

The base is a shared set of manifests; an overlay is a folder with a `kustomization.yaml` that plugs in the base and applies the environment's differences.

→ [Basic theory](basics.en.md#overlay)

</details>

<details><summary>2. How does <code>kubectl kustomize</code> differ from <code>kubectl apply -k</code>?</summary>

The first prints the result without contacting the cluster; the second applies it.

→ [Reference](#kubectl-commands)

</details>

<details><summary>3. Why does a generated ConfigMap get a hash suffix?</summary>

A data change gives a new name, hence a new pod template and a rolling update; without the hash pods do not restart.

→ [Advanced theory](advanced.en.md#configmap-and-secret-generators)

</details>

<details><summary>4. What happens to the old ConfigMap after a data change and <code>apply -k</code>?</summary>

It stays: `apply -k` does not delete objects that disappeared from the result.

→ [Pitfall 2](#2-the-hash-does-not-protect-from-forgotten-configmaps)

</details>

<details><summary>5. Which two patch formats does Kustomize support?</summary>

Strategic merge (a manifest fragment) and JSON 6902 (a list of `op/path/value` operations).

→ [Advanced theory](advanced.en.md#patches)

</details>

<details><summary>6. How does a patch choose an object?</summary>

By `target` (`kind`, `name`, …); without `target` — by `kind` and the name in the body of a strategic-merge patch. No match — an error.

→ [Advanced theory](advanced.en.md#patch-errors)

</details>

<details><summary>7. Trap: why must you not add <code>commonLabels</code> to an existing Deployment?</summary>

They also go into the selector, and a Deployment's `spec.selector` is immutable.

→ [Pitfall 1](#1-commonlabels-changes-selectors)

</details>

<details><summary>8. Does Kustomize create a namespace from the <code>namespace</code> field?</summary>

No, it only fills it into the objects; the namespace must be created (or its manifest included in `resources`).

→ [Pitfall 4](#4-the-namespace-is-not-created)

</details>

<details><summary>9. What is a component and how does it differ from a base?</summary>

A `Component` is a reusable transformation (`apiVersion …/v1alpha1`) plugged in through `components`, not a set of resources to build.

→ [Advanced theory](advanced.en.md#components)

</details>

<details><summary>10. What does <code>replacements</code> do?</summary>

It copies a value from a field of one object into fields of others (in the experiment a Service port → a Deployment variable).

→ [Advanced theory](advanced.en.md#replacements)

</details>

<details><summary>11. Why does Kustomize not read a file outside its folder?</summary>

The `LoadRestrictionsRootOnly` protection (the default of the `--load-restrictor` flag).

→ [Pitfall 5](#5-leaving-the-folder)

</details>

<details><summary>12. When to choose Kustomize and when Helm?</summary>

Kustomize — for your own manifests with small environment differences; Helm — for parameterization, releases and third-party charts.

→ [Trade-offs](#trade-offs-and-alternatives)

</details>

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Self-check: [quiz](quiz.en.md), [exercises](exercises.en.md)
- Helm: [13 · Helm](../13-helm/expert.en.md)
- Configuration: [07 · ConfigMap and Secret](../07-config-and-secrets/expert.en.md)

## Sources

- [Kustomize: Reference](https://kubectl.docs.kubernetes.io/references/kustomize/)
- [Kustomize: kustomization.yaml](https://kubectl.docs.kubernetes.io/references/kustomize/kustomization/)
- [Kubernetes: Declarative Management using Kustomize](https://kubernetes.io/docs/tasks/manage-kubernetes-objects/kustomization/)
- [kubectl: kustomize](https://kubernetes.io/docs/reference/kubectl/generated/kubectl_kustomize/)
