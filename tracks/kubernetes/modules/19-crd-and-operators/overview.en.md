---
title: "CRDs and operators"
lang: en
track: kubernetes
kind: module
doc: overview
slug: 19-crd-and-operators
part: 3
levels: [1, 2, 3, 4]
tags: [crd, custom-resource, operator, controller, reconcile, cel, status-subresource, owner-reference, cert-manager]
versions: "Kubernetes 1.37.0 (kind v0.34), kubectl 1.36.1, cert-manager v1.21.3, Helm 4.3"
status: draft
updated: 2026-11-23
---

# CRDs and operators

🌐 [Русский](overview.ru.md) · **English**

## In short

Kubernetes can be extended with your own object types. A **CustomResourceDefinition (CRD)** describes a new type (`Widget`, `Certificate`, `Application`): a name, a field schema and validation. After the CRD is created, the API server stores such objects itself and serves them through `kubectl` like built-in ones. An object does nothing by itself — it is only data. To make it mean something you need a **controller** that compares the desired state (the `spec` field) with the real one and brings the world in line. The pair "CRD + controller" is called an **operator**.

## Key ideas

- **A CRD is a schema, not code.** It sets the group, names, versions, an OpenAPI v3 schema, defaults and validation.
- **The API server validates objects against the schema:** types, `required`, `enum`, `minimum`/`maximum`, CEL rules (`x-kubernetes-validations`).
- **Unknown fields are not stored:** `kubectl` rejects them under strict validation, and without it they are silently dropped (pruning).
- **`status` is a separate subresource:** an ordinary `patch` of the resource does not change it; the controller writes status through `/status`.
- **A controller works in a reconcile loop:** "read desired → bring actual in line → write status", idempotently.
- **`ownerReferences` link objects:** when the owner is deleted, the garbage collector deletes dependents.
- **A controller needs RBAC** for its own objects, their `status` and everything it manages.
- **Ready-made operators are installed as applications** (a Helm chart, a manifest): cert-manager, Argo CD and others.

## Minimal example

```yaml
apiVersion: apiextensions.k8s.io/v1
kind: CustomResourceDefinition
metadata: {name: widgets.example.com}
spec:
  group: example.com
  scope: Namespaced
  names: {kind: Widget, plural: widgets, singular: widget, shortNames: [wg]}
  versions:
  - name: v1
    served: true
    storage: true
    schema:
      openAPIV3Schema:
        type: object
        properties:
          spec:
            type: object
            required: [image]
            properties:
              image: {type: string}
              replicas: {type: integer, minimum: 1, maximum: 5, default: 1}
```

```text
$ kubectl -n learn-19 apply -f w1.yaml     # spec: {image: nginx:1.29}
widget.example.com/w1 created
$ kubectl -n learn-19 get widget w1 -o jsonpath='{.spec}'
{"image":"nginx:1.29","replicas":1}
```

The `replicas` field was not set and got the default value from the schema.

## When to use and when not

| Task | Approach |
|---|---|
| Describe your own domain object declaratively | CRD + controller |
| Automate operating a complex application (a database, certificates) | an operator |
| Store application settings | ConfigMap ([module 07](../07-config-and-secrets/overview.en.md)) |
| One-off automation | a Job or a script |
| Validate and forbid objects | ValidatingAdmissionPolicy ([module 17](../17-security/advanced.en.md#validatingadmissionpolicy-your-own-rules)) |

- ❌ Do not create a CRD "just in case": it is a new part of the API that you will have to version.
- ❌ Do not write an operator where a Helm chart and a Deployment are enough.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | a CRD and an object, schema, defaults, validation, `kubectl explain`, the status subresource |
| [2. Advanced theory](advanced.en.md) | CEL rules, pruning, status, ownerReferences, a toy operator with RBAC, cert-manager |
| [3. Expert theory](expert.en.md) | CRD field reference, controller internals, versions, pitfalls, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn kubernetes 19-crd-and-operators level 5` |
| 6. Tests | not created yet — `/learn kubernetes 19-crd-and-operators level 6` |

## Related materials

- Prerequisites: [02 · Architecture](../02-architecture/overview.en.md) (controllers and the API server), [05 · Deployment](../05-deployments/overview.en.md), [10 · Namespaces and RBAC](../10-namespaces-and-rbac/overview.en.md)
- Installing operators: [13 · Helm](../13-helm/overview.en.md)
- Object validation: [17 · Security](../17-security/overview.en.md)
- Next: [20 · GitOps](../20-gitops/overview.en.md) (Argo CD is itself an operator with its own CRDs)
- In other tracks: [C# background jobs](../../../csharp/recipes/background-jobs/overview.en.md) — the "watch and reconcile" loop in an ordinary application
