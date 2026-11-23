---
title: "Exercises: Kustomize"
lang: en
track: kubernetes
kind: module
doc: exercises
slug: 14-kustomize
part: 2
tags: [kustomize, overlay, patch, configmap-generator]
status: draft
updated: 2026-11-23
---

# Exercises: Kustomize

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself; hints and solutions are under spoilers. You need a kind cluster; create the namespace beforehand: `kubectl create namespace learn-14-prod`. Clean up afterwards: `kubectl delete namespace learn-14-prod`. The base and overlay structure is from [basic theory](basics.en.md#structure).

## 1. 🟢 A base and two overlays

**Task:** create a base (a Deployment `web` with `nginx:1.28` and a Service) and two overlays: dev (prefix `dev-`, label `env: dev`) and prod (prefix `prod-`, 3 replicas, `nginx:1.29`). Look at the `kubectl kustomize` result.

**Expected result:** in dev the names are `dev-web`, the selector contains `env: dev`; in prod `replicas: 3`, `image: nginx:1.29`.

<details><summary>Hint</summary>

`labels: [{pairs: {env: dev}, includeSelectors: true}]`, `replicas: [{name: web, count: 3}]`, `images: [{name: nginx, newTag: "1.29"}]`.

</details>

<details><summary>Solution</summary>

The manifests are from [basic theory](basics.en.md#overlay).

```text
$ kubectl kustomize overlays/prod | grep -E "replicas|image:"
  replicas: 3
      - image: nginx:1.29
```

</details>

## 2. 🟢 Apply and delete

**Task:** apply the prod overlay and then delete it, using `-k`.

**Expected result:** `configmap/…`, `service/prod-web`, `deployment.apps/prod-web` are created and then deleted.

<details><summary>Hint</summary>

`kubectl apply -k overlays/prod`, `kubectl delete -k overlays/prod`.

</details>

<details><summary>Solution</summary>

```text
$ kubectl apply -k overlays/prod
service/prod-web created
deployment.apps/prod-web created
$ kubectl delete -k overlays/prod
service "prod-web" deleted from learn-14-prod namespace
deployment.apps "prod-web" deleted from learn-14-prod namespace
```

</details>

## 3. 🟡 A limits patch and JSON 6902

**Task:** add the limits `cpu: 200m`, `memory: 128Mi` to prod with a strategic merge patch, and in a separate overlay change the replica count to 5 with a JSON 6902 patch.

**Expected result:** `resources.limits` in the Deployment; `replicas: 5`.

<details><summary>Hint</summary>

The container is found by `name: web`. For JSON 6902: `target: {kind: Deployment, name: web}`, `op: replace`, `path: /spec/replicas`.

</details>

<details><summary>Solution</summary>

The patches are from [advanced theory](advanced.en.md#patches).

```text
        resources:
          limits:
            cpu: 200m
            memory: 128Mi
  replicas: 5
```

</details>

## 4. 🟡 A hashed ConfigMap and a restart

**Task:** add a `configMapGenerator` with `LOG_LEVEL=warn` and plug it into the Deployment via `envFrom`. Apply, then change it to `debug` and apply again. Look at the ConfigMap and ReplicaSet names.

**Expected result:** two ConfigMaps with different hashes, two ReplicaSets (old 0, new 3), `printenv LOG_LEVEL` gives `debug`.

<details><summary>Hint</summary>

`kubectl diff -k overlays/prod` shows the name change; `kubectl get cm,rs`.

</details>

<details><summary>Solution</summary>

```text
configmap/prod-app-config-47668c6k28 created
deployment.apps/prod-web configured
prod-web-5944d4c54    3 3 3
prod-web-6fc9fb97d8   0 0 0
debug
```

The old ConfigMap stayed: `apply -k` does not delete objects.

</details>

## 5. 🔴 Replacements, a component and restrictions

**Task:** (a) with `replacements` pass the Service port into the Deployment variable `SVC_PORT`. (b) Move adding the annotation `prometheus.io/scrape: "true"` into a `Component` and plug it in. (c) Put a file from a neighboring folder into `resources` and look at the error.

**Expected result:** (a) `SVC_PORT: "80"`; (b) the annotation in the Deployment; (c) the error `security; file … is not in or below …`.

<details><summary>Hint</summary>

(a) `source: {kind: Service, name: web, fieldPath: spec.ports.0.port}`; (b) `kind: Component`, `components: [../comp-metrics]`; (c) `resources: [../../app/base/service.yaml]`.

</details>

<details><summary>Solution</summary>

```text
        - name: SVC_PORT
          value: "80"
  annotations:
    prometheus.io/scrape: "true"
error: accumulating resources: accumulation err='accumulating resources from '../../app/base/service.yaml': security; file '…' is not in or below '…'
```

</details>
