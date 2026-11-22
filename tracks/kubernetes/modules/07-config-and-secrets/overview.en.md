---
title: "ConfigMap and Secret"
lang: en
track: kubernetes
kind: module
doc: overview
slug: 07-config-and-secrets
part: 1
levels: [1, 2, 3, 4]
tags: [configmap, secret, environment-variables, volume, base64, immutable, configuration, encryption-at-rest]
versions: "Kubernetes 1.37.0 (kind v0.34), kubectl 1.36.1"
status: draft
updated: 2026-11-22
---

# ConfigMap and Secret

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

Configuration must not live inside the image: the same image is run in different environments with different settings. Kubernetes has two objects for this: a **ConfigMap** stores ordinary settings, a **Secret** stores sensitive data (passwords, tokens, certificates). Both are attached to a pod as environment variables or as files. Importantly, a Secret is by default only base64-encoded, not encrypted.

## Key ideas

- **ConfigMap and Secret are "key → value" pairs** created separately from the pod; one object can be attached to many pods.
- **Two ways to attach:** environment variables (`env`, `envFrom`) and volumes (files). Variables are fixed at container start, files in a volume are updated without a restart.
- **base64 is not encryption.** A Secret value is decoded with one command, and in etcd it lies as plain text by default: protection is built on RBAC and etcd encryption.
- **Changing a ConfigMap does not restart pods:** the new values are picked up only by pods recreated after the change (or by a volume, but not `subPath`).
- **A missing ConfigMap or key blocks startup:** the status is `CreateContainerConfigError` unless the reference is marked `optional: true`.
- **`immutable: true`** forbids changing the object: protection from accidental edits and relief for the API server.
- **The size limit is 1 MiB** per object; key names are `[-._a-zA-Z0-9]+`.

## Minimal example

```bash
kubectl -n learn-07 create configmap app-config --from-literal=MODE=prod --from-literal=TIMEOUT=30
kubectl -n learn-07 create secret generic db --from-literal=username=admin --from-literal=password='S3cr3t!'
```

```yaml
env:
  - name: APP_MODE
    valueFrom: {configMapKeyRef: {name: app-config, key: MODE}}
  - name: DB_PASSWORD
    valueFrom: {secretKeyRef: {name: db, key: password}}
```

```text
$ kubectl -n learn-07 exec app -- env | grep -E "^(APP_MODE|DB_PASSWORD)="
APP_MODE=prod
DB_PASSWORD=S3cr3t!
```

## When to use and when not

- ✅ Any environment settings: service addresses, log level, flags, configuration files.
- ✅ A Secret — for passwords, tokens and keys (with RBAC and, preferably, etcd encryption).
- ❌ Do not store secrets in the image, in Git in plain text or in a ConfigMap.
- ❌ Do not assume a Secret is "protected" by itself: base64 is easily decoded.
- ❌ Large files (over 1 MiB) need volumes and storage (module 08), not a ConfigMap.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | creating a ConfigMap and Secret, base64, attaching through `env`, `envFrom` and volumes |
| [2. Advanced](advanced.en.md) | updating values, `subPath`, `immutable`, missing references, file permissions, Secret types, restarting on change |
| [3. Expert](expert.en.md) | how a Secret is stored in etcd, encryption, RBAC, a reference of types and fields, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) with solutions |
| 5. Runnable examples | not generated yet — `/learn kubernetes 07-config-and-secrets level 5` |
| 6. Tests | not generated yet — `/learn kubernetes 07-config-and-secrets level 6` |

## Related materials

- Prerequisites: [04 · Pod](../04-pods/overview.en.md), [05 · Deployment and ReplicaSet](../05-deployments/overview.en.md)
- Next: [08 · Storage](../08-storage/overview.en.md)
- The same idea in other tracks: [DI, configuration and logging in C#](../../../csharp/modules/13-di-config-logging/overview.en.md) — how an application reads environment variables and configuration files
