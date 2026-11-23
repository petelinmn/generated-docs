---
title: "Kustomize: advanced theory"
lang: en
track: kubernetes
kind: module
doc: advanced
slug: 14-kustomize
part: 2
tags: [kustomize, patches, json6902, strategic-merge, configmap-generator, replacements, components]
versions: "Kustomize 5.8.1 (bundled with kubectl 1.36.1), Kubernetes 1.37.0 (kind v0.34)"
status: draft
updated: 2026-11-23
---

# Kustomize: advanced theory

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on [basic theory](basics.en.md): the base and overlay structure is not repeated.

## Patches

The `patches` field changes part of an object. There are two formats.

### Strategic merge: a piece of a manifest

In prod we add resource limits, describing only the needed part of the Deployment:

```yaml
# overlays/prod/resources-patch.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: web
spec:
  template:
    spec:
      containers:
        - name: web
          resources:
            limits: {cpu: 200m, memory: 128Mi}
```

```yaml
# overlays/prod/kustomization.yaml (fragment)
patches:
  - path: resources-patch.yaml
```

The container is found by `name: web`, and its other fields are kept: the result still has `image: nginx:1.29` and `ports`, and `resources` were added.

### JSON 6902: a list of operations

```yaml
patches:
  - target: {kind: Deployment, name: web}
    patch: |-
      - op: replace
        path: /spec/replicas
        value: 5
      - op: add
        path: /spec/template/spec/containers/0/env
        value: [{name: MODE, value: json6902}]
```

The result: `replicas: 5` and `env: [{name: MODE, value: json6902}]`. The path carries the container index (`/containers/0`).

### Choosing the target

A patch is applied to the object chosen in `target` (by `kind`, `name`, a label, etc.), not to the one written in the patch's `metadata.name`:

```yaml
patches:
  - target: {kind: Service}
    patch: |-
      apiVersion: v1
      kind: Service
      metadata:
        name: ignored
      spec:
        type: NodePort
```

In the experiment the Service `web` got `type: NodePort` although the patch had the name `ignored`.

### Patch errors

```text
$ kubectl kustomize .          # patches: [{path: nope.yaml}]
error: trouble configuring builtin PatchTransformer with config: `
path: nope.yaml
`: failed to get the patch file from path(nope.yaml): … The system cannot find the file specified.
$ kubectl kustomize .          # a patch for Deployment/ghost, which does not exist
error: no resource matches strategic merge patch "Deployment.v1.apps/ghost.[noNs]": no matches for Id Deployment.v1.apps/ghost.[noNs]; failed to find unique target for patch Deployment.v1.apps/ghost.[noNs]
```

## ConfigMap and Secret generators

```yaml
configMapGenerator:
  - name: app-config
    literals: [LOG_LEVEL=warn]
```

A content hash is appended to the name: `prod-app-config-dfk4bdbtkk`. Let us plug the ConfigMap into the Deployment with a patch (`envFrom` with `name: app-config`):

```yaml
spec:
  template:
    spec:
      containers:
        - name: web
          envFrom:
            - configMapRef: {name: app-config}
```

Kustomize replaces the reference with the real name:

```text
$ kubectl -n learn-14-prod get deploy prod-web -o jsonpath='{.spec.template.spec.containers[0].envFrom}'
[{"configMapRef":{"name":"prod-app-config-dfk4bdbtkk"}}]
```

Change the value (`warn` → `debug`) and look at `kubectl diff -k`:

```text
-            name: prod-app-config-dfk4bdbtkk
+            name: prod-app-config-47668c6k28
+  LOG_LEVEL: debug
+  name: prod-app-config-47668c6k28
```

The new content means a **new name**, so the pod template changed and a rolling update went through:

```text
$ kubectl apply -k overlays/prod
configmap/prod-app-config-47668c6k28 created
service/prod-web unchanged
deployment.apps/prod-web configured
$ kubectl -n learn-14-prod get rs
prod-web-5944d4c54    3 3 3
prod-web-6fc9fb97d8   0 0 0
$ kubectl -n learn-14-prod exec deploy/prod-web -- printenv LOG_LEVEL
debug
```

This is how the problem ["a ConfigMap change does not restart pods"](../07-config-and-secrets/advanced.en.md#a-configmap-change-does-not-restart-pods) is solved. But the old ConfigMap **stays** in the cluster: `kubectl apply -k` deletes nothing (after `delete -k` the namespace still had `prod-app-config-dfk4bdbtkk`).

To turn the hash off, use `generatorOptions`:

```yaml
generatorOptions:
  disableNameSuffixHash: true
secretGenerator:
  - name: creds
    literals: [password=s3cret]
```

Then the name stays `plain`/`creds`, but a data change does not restart pods. A Secret from `secretGenerator` holds base64 (`password: czNjcmV0`): this is not encryption ([module 07](../07-config-and-secrets/overview.en.md)); do not keep values in `kustomization.yaml` in Git.

## Replacements

`replacements` copy a value from one field to another. The Service port goes into a Deployment variable:

```yaml
replacements:
  - source: {kind: Service, name: web, fieldPath: spec.ports.0.port}
    targets:
      - select: {kind: Deployment, name: web}
        fieldPaths: [spec.template.spec.containers.0.env.0.value]
```

```text
        - name: SVC_PORT
          value: "80"
```

The placeholder `value: placeholder` in `env[0]` was replaced with `80`. With an index (`env.0`) the field must exist.

## Components

A component is a reusable piece not tied to a single base:

```yaml
# comp-metrics/kustomization.yaml
apiVersion: kustomize.config.k8s.io/v1alpha1
kind: Component
patches:
  - target: {kind: Deployment}
    patch: |-
      - op: add
        path: /metadata/annotations
        value: {prometheus.io/scrape: "true"}
```

```yaml
# app/kustomization.yaml
resources: [deployment.yaml, service.yaml]
components: [../comp-metrics]
```

```text
$ kubectl kustomize app | grep -B1 -A2 annotations
metadata:
  annotations:
    prometheus.io/scrape: "true"
  name: web
```

## Load restrictions

By default Kustomize does not read files outside the folder with the `kustomization.yaml`:

```text
error: accumulating resources: accumulation err='accumulating resources from '../../app/base/service.yaml': security; file '…\app\base\service.yaml' is not in or below '…\t4\a'
```

Reference folders that have their own `kustomization.yaml` (like `../../base`); the `--load-restrictor` flag of `kubectl kustomize` turns the check off, but doing so is not recommended.

> ⚠️ Verify: the full error text is truncated in the experiment; the behavior of `--load-restrictor=LoadRestrictionsNone` was not run.

## Helm inside Kustomize

`kubectl kustomize` has the flag `--enable-helm` (default `false`): it lets you plug charts into `kustomization.yaml`.

> ⚠️ Verify: the `helmCharts` field and the work of `--enable-helm` were not run (Helm in `PATH` and access to a repository are needed).

## Best practices

- The base is a working manifest that can be applied on its own; an overlay has only the differences.
- First `kubectl kustomize` or `kubectl diff -k`, then `apply -k`.
- Hashed generators are for configuration that must restart pods; clean up old ConfigMaps (or use GitOps with prune).
- Use `labels` instead of `commonLabels`; enable `includeSelectors` deliberately: Deployment selectors are immutable.
- Do not keep secrets in `kustomization.yaml`.
- Move shared pieces into components.

## Common mistakes

- ❌ A patch for a non-existent object → ✅ the name and `kind` must match (`no resource matches strategic merge patch`).
- ❌ Listing the same resource twice → ✅ `may not add resource with an already registered id`.
- ❌ Referencing a file outside the folder → ✅ move it to a folder with its own `kustomization.yaml`.
- ❌ Changing a selector through `labels` on a live Deployment → ✅ `includeSelectors` only at the start.

## Comparison with other tracks

- [Helm](../13-helm/advanced.en.md): parameters and history instead of patches over ready YAML.
- [C#](../../../csharp/recipes/background-jobs/overview.en.md): an application's configuration comes from ConfigMaps/Secrets ([07](../07-config-and-secrets/overview.en.md)); Kustomize only manages their versions.

## Related materials

- Previous level: [basic theory](basics.en.md)
- Next: [expert theory](expert.en.md)
- Rolling update: [05 · Deployment](../05-deployments/advanced.en.md#rolling-update-step-by-step)
- Secrets: [07 · ConfigMap and Secret](../07-config-and-secrets/advanced.en.md)

## Sources

- [Kustomize: patches](https://kubectl.docs.kubernetes.io/references/kustomize/kustomization/patches/)
- [Kustomize: configMapGenerator](https://kubectl.docs.kubernetes.io/references/kustomize/kustomization/configmapgenerator/)
- [Kustomize: replacements](https://kubectl.docs.kubernetes.io/references/kustomize/kustomization/replacements/)
- [Kustomize: components](https://kubectl.docs.kubernetes.io/references/kustomize/kustomization/components/)
- [Kustomize: generatorOptions](https://kubectl.docs.kubernetes.io/references/kustomize/kustomization/generatoroptions/)
