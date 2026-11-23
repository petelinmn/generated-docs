---
title: "Kustomize: basic theory"
lang: en
track: kubernetes
kind: module
doc: basics
slug: 14-kustomize
part: 2
tags: [kustomize, overlay, base, namespace, name-prefix, images, replicas]
versions: "Kustomize 5.8.1 (bundled with kubectl 1.36.1), Kubernetes 1.37.0 (kind v0.34)"
status: draft
updated: 2026-11-23
---

# Kustomize: basic theory

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [05 · Deployment](../05-deployments/overview.en.md), [03 · kubectl](../03-local-cluster-and-kubectl/overview.en.md) · **Time:** ~2 h

All examples were run in kind (Kubernetes 1.37.0) with the commands `kubectl kustomize` and `kubectl apply -k` (Kustomize 5.8.1 bundled with kubectl 1.36.1). Nothing separate needs to be installed.

## Why this is needed

One service is deployed to dev and prod: prod has more replicas, a fresh image, limits and a different configuration. Copying manifests is risky: every fix must be made everywhere. Kustomize keeps the shared part once (the **base**) and describes the environment differences separately (**overlays**), leaving the source files as plain YAML.

## Structure

```text
app/
├── base/
│   ├── deployment.yaml
│   ├── service.yaml
│   └── kustomization.yaml
└── overlays/
    ├── dev/kustomization.yaml
    └── prod/
        ├── kustomization.yaml
        └── resources-patch.yaml
```

`base/kustomization.yaml` simply lists the files:

```yaml
apiVersion: kustomize.config.k8s.io/v1beta1
kind: Kustomization
resources:
  - deployment.yaml
  - service.yaml
```

`base/deployment.yaml` is an ordinary Deployment (`web`, `nginx:1.28`, 1 replica), `service.yaml` is a Service on the same `app: web`.

## Overlay

An overlay plugs in the base through `resources` and adds transformations. For dev:

```yaml
# overlays/dev/kustomization.yaml
apiVersion: kustomize.config.k8s.io/v1beta1
kind: Kustomization
namespace: learn-14-dev
namePrefix: dev-
labels:
  - pairs: {env: dev}
    includeSelectors: true
resources:
  - ../../base
```

```text
$ kubectl kustomize overlays/dev
apiVersion: v1
kind: Service
metadata:
  labels:
    env: dev
  name: dev-web
  namespace: learn-14-dev
spec:
  ports:
  - port: 80
  selector:
    app: web
    env: dev
---
apiVersion: apps/v1
kind: Deployment
metadata:
  labels:
    env: dev
  name: dev-web
  namespace: learn-14-dev
spec:
  replicas: 1
  selector:
    matchLabels:
      app: web
      env: dev
…
```

The result: names with the `dev-` prefix, the namespace filled in, the label `env: dev` added to the `selector` as well (because of `includeSelectors: true`). The base files did not change.

For prod we change the replicas and the image:

```yaml
# overlays/prod/kustomization.yaml
apiVersion: kustomize.config.k8s.io/v1beta1
kind: Kustomization
namespace: learn-14-prod
namePrefix: prod-
labels:
  - pairs: {env: prod}
resources:
  - ../../base
replicas:
  - name: web
    count: 3
images:
  - name: nginx
    newTag: "1.29"
```

```text
$ kubectl kustomize overlays/prod | grep -E "replicas|image:"
  replicas: 3
      - image: nginx:1.29
```

Here `labels` has no `includeSelectors`: the label `env: prod` landed on the objects but not in the selectors (the Service `selector` stayed `app: web`).

## Applying

The commands are the same as for ordinary manifests, but with the `-k` flag and a directory:

```text
$ kubectl apply -k overlays/prod
configmap/prod-app-config-dfk4bdbtkk created
service/prod-web created
deployment.apps/prod-web created
$ kubectl get -k overlays/prod
NAME                                   DATA   AGE
configmap/prod-app-config-47668c6k28   1      35s
…
$ kubectl delete -k overlays/prod
configmap "prod-app-config-47668c6k28" deleted from learn-14-prod namespace
service "prod-web" deleted from learn-14-prod namespace
deployment.apps "prod-web" deleted from learn-14-prod namespace
```

The overlay's namespace must exist beforehand: Kustomize does not create it.

```text
$ kubectl apply -k overlays/nons          # namespace: learn-14-missing
Error from server (NotFound): error when creating "overlays/nons": namespaces "learn-14-missing" not found
```

To create the namespace together with the application, add a Namespace manifest to `resources`.

## Main transformers

| Field | What it does |
|---|---|
| `namespace` | sets the namespace on all objects |
| `namePrefix`, `nameSuffix` | adds a prefix or suffix to names and updates references |
| `labels` | adds labels (with `includeSelectors: true` — to selectors too) |
| `images` | changes an image (`newName`, `newTag`) by name |
| `replicas` | sets the replica count by resource name |
| `patches` | edits fields (more in [advanced theory](advanced.en.md#patches)) |
| `configMapGenerator`, `secretGenerator` | create ConfigMaps and Secrets |

## Beginner mistakes

- ❌ Copying the base into every overlay → ✅ put only the differences in an overlay.
- ❌ Applying the directory with `kubectl apply -f` → ✅ `kubectl apply -k`.
- ❌ Expecting the namespace to be created by itself → ✅ create it beforehand or include a Namespace in `resources`.
- ❌ Using the deprecated `commonLabels` → ✅ `labels` (Kustomize warns `'commonLabels' is deprecated`).
- ❌ Not looking at the result before applying → ✅ `kubectl kustomize` or `kubectl diff -k`.

## Related materials

- Next: [advanced theory](advanced.en.md) — patches, generators, replacements, components
- Previous module: [13 · Helm](../13-helm/overview.en.md)
- Deployment and images: [05 · Deployment](../05-deployments/overview.en.md)
- Configuration: [07 · ConfigMap and Secret](../07-config-and-secrets/overview.en.md)

## Sources

- [Kubernetes: Declarative Management using Kustomize](https://kubernetes.io/docs/tasks/manage-kubernetes-objects/kustomization/)
- [Kustomize: kustomization.yaml reference](https://kubectl.docs.kubernetes.io/references/kustomize/kustomization/)
- [Kustomize: labels](https://kubectl.docs.kubernetes.io/references/kustomize/kustomization/labels/)
- [Kustomize: images](https://kubectl.docs.kubernetes.io/references/kustomize/kustomization/images/)
- [Kustomize: replicas](https://kubectl.docs.kubernetes.io/references/kustomize/kustomization/replicas/)
