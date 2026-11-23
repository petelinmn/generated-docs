---
title: "CRDs and operators: expert"
lang: en
track: kubernetes
kind: module
doc: expert
slug: 19-crd-and-operators
part: 3
tags: [crd, operator, controller, reconcile, informer, finalizer, owner-reference, cel, conversion-webhook, controller-runtime]
versions: "Kubernetes 1.37.0 (kind v0.34), kubectl 1.36.1, cert-manager v1.21.3"
status: draft
updated: 2026-11-23
---

# CRDs and operators: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on [basic](basics.en.md) and [advanced](advanced.en.md) theory. What was confirmed by an experiment in kind (Kubernetes 1.37.0) is marked "in the experiment".

## Reference

### CustomResourceDefinition fields

| Field | Meaning |
|---|---|
| `metadata.name` | `<plural>.<group>` (in the experiment `widgets.example.com`) |
| `spec.group` | the API group |
| `spec.scope` | `Namespaced` or `Cluster` |
| `spec.names` | `kind`, `listKind`, `plural`, `singular`, `shortNames`, `categories` |
| `spec.versions[]` | `name`, `served`, `storage`, `schema.openAPIV3Schema`, `subresources`, `additionalPrinterColumns`, `deprecated` |
| `subresources.status` | a separate `/status` (in the experiment) |
| `subresources.scale` | integration with `kubectl scale` and HPA |
| `spec.conversion` | the strategy for converting between versions: `None` or `Webhook` |
| `spec.preserveUnknownFields` | a legacy mode without pruning |

> ⚠️ Verify: `categories`, `subresources.scale`, `deprecated`, `spec.conversion` (including `Webhook`), `preserveUnknownFields`, `selectableFields` are taken from the documentation and were not run. The experiment covered `group`, `scope`, `names` (`shortNames`), one version, `status`, `additionalPrinterColumns`, the schema, `default`, `x-kubernetes-validations`.

### Schema keywords (verified in the experiment)

| Keyword | Effect |
|---|---|
| `type` | the type (`string`, `integer`, `object`): the error `must be of type integer` |
| `required` | required fields: `Required value` |
| `enum` | allowed values: `Unsupported value … supported values` |
| `minimum`, `maximum` | bounds: `should be less than or equal to 5` |
| `default` | the default value is filled in by the server |
| `x-kubernetes-validations` | CEL rules with `rule` and `message` |

> ⚠️ Verify: `pattern`, `format`, `x-kubernetes-preserve-unknown-fields`, `x-kubernetes-list-type`, `nullable`, `oneOf`/`anyOf` are per the documentation and were not checked by running.

### Commands

| Command | Purpose |
|---|---|
| `kubectl get crd` | the list of CRDs |
| `kubectl api-resources --api-group=<group>` | the types of a group, short names |
| `kubectl explain <kind>.spec` | documentation from the schema |
| `kubectl patch <obj> --subresource=status` | write `status` |
| `kubectl apply --validate=false` | turn off the client's strict validation (fields are still dropped) |

## Internals

- **A CRD is part of the API server.** After a CRD is created the API server starts serving a new path (`/apis/example.com/v1/…`), stores objects in etcd and validates them against the schema. There is no separate process for this.
- **A controller is a separate application.** It usually runs as a pod and works through the API: it reads and watches objects and creates dependents. In the experiment that is the `operator` Deployment; cert-manager has three Deployments (the controller, cainjector, the webhook).
- **The reconcile loop.** A controller receives an object key, reads the desired (`spec`) and actual state, performs the minimal actions and writes `status`. The loop is "level-triggered": the current state matters, not the event, so a missed event is not a problem. In the experiment the operator undid a manual `scale` without any event about it.
- **Informer and queue.** Libraries (client-go, controller-runtime) keep a local cache of objects through a watch and put changed keys into a queue with retries. The toy operator in the experiment simply polls the API every 3 seconds.
- **Ownership.** `ownerReferences` provide garbage collection: when the owner is deleted, dependents are deleted (in the experiment). `blockOwnerDeletion` and foreground deletion wait for dependents.
- **Finalizers.** If an object has `metadata.finalizers`, deletion only sets `deletionTimestamp`, and the object stays until the controller performs cleanup and removes the finalizer (the mechanism is shown in [module 21](../21-troubleshooting/advanced.en.md#an-object-stuck-in-terminating)).
- **Admission for CRDs.** Objects are validated by the schema and CEL rules inside the API server; complex checks and mutations go to a webhook or a ValidatingAdmissionPolicy ([module 17](../17-security/overview.en.md)).
- **Versions.** An object is stored in etcd in one version (`storage`), the others are served through conversion: `None` just changes `apiVersion`, `Webhook` calls an external service.

> ⚠️ Verify: leader election and retries with exponential backoff in controller-runtime, `kubebuilder`, `operator-sdk` and version conversion were not run.

## Gotchas

### 1. A CEL rule without `has()`

For an optional field the rule fails with `no such key: tier` and rejects even harmless changes (in the experiment). You need `!has(self.tier) || …`.

### 2. An ordinary `patch` does not change `status`

With the `status` subresource enabled you get `patched (no change)` (in the experiment). You need `--subresource=status`.

### 3. RBAC on `status` and `get`

`kubectl patch --subresource=status` needs read rights too: the operator got `Forbidden: cannot get resource "widgets/status"` (in the experiment).

### 4. Pruning silently removes unknown fields

With `--validate=false` the field `color: red` vanished with no error (in the experiment). Data is lost until the field is described in the schema.

### 5. `default` is applied by the server

Defaults are filled in on write and on read. If you remove them from the schema, older objects may lack the fields. Rules and code must allow for a missing field.

### 6. Deleting a CRD deletes all its objects

After `kubectl delete crd` the type vanished: `the server could not find the requested resource` (in the experiment). By the documentation all objects are deleted together with the type, so a CRD must not be deleted without a backup.

### 7. Installing and updating CRDs outside a chart

Helm does not update the `crds/` directory on `helm upgrade`; cert-manager uses `crds.enabled=true` and installs the CRDs as part of the chart. This is a general question of CRD lifecycle.

### 8. Polling instead of a watch

The toy operator loads the API server every 3 seconds for all objects. Real controllers use a watch and a cache.

> ⚠️ Verify: gotchas 6 (deleting objects with the CRD) and 7 (Helm behavior with `crds/`) are from the documentation and were not reproduced.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| CRD + operator | declarative, native to Kubernetes, RBAC and `kubectl` out of the box | code, versioning, operations |
| A Helm chart | simple, no code | no continuous reconcile and no "day 2" logic |
| A script/CronJob | quick | no declarative API and status |
| An aggregated API server | full control over the API | complex; not run |
| A ready-made operator | saves work | a dependency, you need to understand the CRDs |
| Frameworks (kubebuilder, operator-sdk, Metacontroller) | a template and generation | not run |

## Interview questions

<details><summary>1. What is a CRD and how does it differ from an operator?</summary>

A CRD adds a new type to the API with storage and validation. An operator is a CRD plus a controller that does real work with these objects.

→ [Overview](overview.en.md#in-short)

</details>

<details><summary>2. How does the API server validate CRD objects?</summary>

By an OpenAPI v3 schema (`type`, `required`, `enum`, bounds), defaults are filled in, extra rules are CEL (`x-kubernetes-validations`).

→ [Basic theory](basics.en.md#schema-validation)

</details>

<details><summary>3. Trick: a CEL rule rejects an object without a field although the field is optional. Why?</summary>

Accessing a missing field is an error (`no such key`). You need `has()`.

→ [Gotcha 1](#1-a-cel-rule-without-has)

</details>

<details><summary>4. Why is status a subresource?</summary>

So that the user and the controller write to different parts of the object and permissions are granted separately (`widgets/status`).

→ [Advanced theory](advanced.en.md#the-status-subresource)

</details>

<details><summary>5. What does a "level-triggered" reconcile loop mean?</summary>

The controller compares the current and desired state each time rather than handling an event. A missed event breaks nothing, and a repeat is safe (idempotency).

→ [Internals](#internals)

</details>

<details><summary>6. Trick: a <code>kubectl patch</code> of status succeeded but nothing changed. Why?</summary>

Status is a subresource: you need `--subresource=status`.

→ [Gotcha 2](#2-an-ordinary-patch-does-not-change-status)

</details>

<details><summary>7. What do ownerReferences do?</summary>

They link a dependent object to its owner: when the owner is deleted, the garbage collector deletes the dependent.

→ [Advanced theory](advanced.en.md#ownerreferences-and-garbage-collection)

</details>

<details><summary>8. What are finalizers for?</summary>

They block the final deletion until the controller performs cleanup (external resources).

→ [Internals](#internals)

</details>

<details><summary>9. What happens when a CRD is deleted?</summary>

The type disappears from the API, and all objects of that type are deleted with it.

→ [Gotcha 6](#6-deleting-a-crd-deletes-all-its-objects)

</details>

<details><summary>10. What rights does an operator need?</summary>

On its own resources (`get/list/watch`), on `<resource>/status` (including `get` for `kubectl patch`) and on the managed objects (Deployment and so on).

→ [Advanced theory](advanced.en.md#a-toy-operator)

</details>

<details><summary>11. Where did a field that is not in the schema go?</summary>

It was dropped by pruning: without `--validate=false` `kubectl` rejects the request strictly, with it the server silently removes the field.

→ [Gotcha 4](#4-pruning-silently-removes-unknown-fields)

</details>

<details><summary>12. How does cert-manager work at the level of Kubernetes objects?</summary>

The CRDs `Issuer` and `Certificate`; the controller creates a `CertificateRequest`, gets the certificate and stores it in a Secret `kubernetes.io/tls`; a deleted Secret is created again.

→ [Advanced theory](advanced.en.md#a-real-operator-cert-manager)

</details>

## Cheat sheet

```text
kubectl get crd
kubectl api-resources --api-group=example.com
kubectl explain widget.spec
kubectl patch widget NAME --subresource=status --type=merge -p '{"status":{…}}'
```

- A CRD = a schema, an operator = a CRD + a controller.
- The schema: `required`, `enum`, bounds, `default`; CEL with `has()`.
- `status` through the subresource and RBAC `<resource>/status`.
- `ownerReferences` — garbage collection; finalizers — cleanup.
- Deleting a CRD deletes its objects.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Controller architecture: [02 · Architecture](../02-architecture/expert.en.md)
- GitOps: [20 · GitOps](../20-gitops/overview.en.md)
- Diagnosing objects in `Terminating`: [21 · Troubleshooting](../21-troubleshooting/overview.en.md)

## Sources

- [Custom Resources](https://kubernetes.io/docs/concepts/extend-kubernetes/api-extension/custom-resources/)
- [Versions in CustomResourceDefinitions](https://kubernetes.io/docs/tasks/extend-kubernetes/custom-resources/custom-resource-definition-versioning/)
- [Operator pattern](https://kubernetes.io/docs/concepts/extend-kubernetes/operator/)
- [Finalizers](https://kubernetes.io/docs/concepts/overview/working-with-objects/finalizers/)
- [Owners and Dependents](https://kubernetes.io/docs/concepts/overview/working-with-objects/owners-dependents/)
