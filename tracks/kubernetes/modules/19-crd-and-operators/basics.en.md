---
title: "CRDs and operators: basics"
lang: en
track: kubernetes
kind: module
doc: basics
slug: 19-crd-and-operators
part: 3
tags: [crd, custom-resource, openapi-schema, validation, defaults, status-subresource, kubectl-explain]
versions: "Kubernetes 1.37.0 (kind v0.34), kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# CRDs and operators: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [02 · Architecture](../02-architecture/overview.en.md), [05 · Deployment](../05-deployments/overview.en.md) · **Time:** ~2 h

All examples were run in kind (Kubernetes 1.37.0), namespace `learn-19`. The domain object is a `Widget`: a "widget" that in the [advanced theory](advanced.en.md#a-toy-operator) turns into a Deployment.

## Why it matters

Built-in types (Pod, Service, Deployment) are not enough when you need to speak the language of your task: "certificate", "database", "application in Git". A CRD adds such a type to the cluster API, and the familiar `kubectl get/apply/delete`, RBAC, `describe` and `--watch` work with it.

## Create a CRD

```yaml
apiVersion: apiextensions.k8s.io/v1
kind: CustomResourceDefinition
metadata: {name: widgets.example.com}      # <plural>.<group>
spec:
  group: example.com
  scope: Namespaced
  names: {kind: Widget, listKind: WidgetList, plural: widgets, singular: widget, shortNames: [wg]}
  versions:
  - name: v1
    served: true
    storage: true
    subresources: {status: {}}
    additionalPrinterColumns:
    - {name: Replicas, type: integer, jsonPath: .spec.replicas}
    - {name: Phase, type: string, jsonPath: .status.phase}
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
              tier: {type: string, enum: [free, pro]}
          status:
            type: object
            properties:
              phase: {type: string}
```

- The CRD name must be `<plural>.<group>`.
- `scope`: `Namespaced` or `Cluster`.
- `versions[]`: each version has its own `served` (whether it is served by the API) and `storage` (the version stored in etcd; exactly one).
- `additionalPrinterColumns` sets the `kubectl get` columns.

```text
$ kubectl get crd widgets.example.com
NAME                  CREATED AT
widgets.example.com   2026-11-23T14:00:10Z
$ kubectl api-resources --api-group=example.com
NAME      SHORTNAMES   APIVERSION       NAMESPACED   KIND
widgets   wg           example.com/v1   true         Widget
```

## Create an object

```yaml
apiVersion: example.com/v1
kind: Widget
metadata: {name: w1}
spec: {image: nginx:1.29}
```

```text
$ kubectl -n learn-19 apply -f w1.yaml
widget.example.com/w1 created
$ kubectl -n learn-19 get wg
NAME   REPLICAS   PHASE
w1     1          
$ kubectl -n learn-19 get widget w1 -o yaml     # fragment
spec:
  image: nginx:1.29
  replicas: 1
```

The `Replicas` column is taken from `.spec.replicas`, and the value 1 itself was filled in by the API server from the schema's `default`: it was not in the manifest. The `Phase` column is empty — nobody has written a status yet.

## Schema validation

The API server rejects objects that do not fit the schema. All examples are real responses:

```text
spec: {image: nginx, replicas: 9}
  spec.replicas: Invalid value: 9: spec.replicas in body should be less than or equal to 5

spec: {replicas: 2}
  spec.image: Required value

spec: {image: nginx, tier: gold}
  spec.tier: Unsupported value: "gold": supported values: "free", "pro"

spec: {image: nginx, replicas: "two"}
  spec.replicas: Invalid value: "string": spec.replicas in body must be of type integer: "string"
```

The schema rules: type (`type`), requiredness (`required`), range (`minimum`/`maximum`), enumeration (`enum`), default (`default`). More flexible checks are written in CEL, see the [advanced theory](advanced.en.md#cel-rules).

## `kubectl explain`: documentation from the schema

The CRD schema also drives hints:

```text
$ kubectl -n learn-19 explain widget.spec
GROUP:      example.com
KIND:       Widget
VERSION:    v1

FIELD: spec <Object>

FIELDS:
  image	<string> -required-
  replicas	<integer>
  …
```

If you add a `description` to the schema, explanations appear here too (in the experiment there were none: `<no description>`).

## The status subresource

The CRD has `subresources: {status: {}}` enabled. Now `status` is special: it is changed not by ordinary changes to the resource but through `/status`. More in the [advanced theory](advanced.en.md#the-status-subresource).

## Beginner mistakes

- ❌ A CRD name not of the form `<plural>.<group>` → ✅ per the documentation it must match `names.plural` and `group` (not verified by running).
- ❌ Expecting a CRD object to "do something by itself" → ✅ a controller is needed.
- ❌ Forgetting `required` and defaults in the schema → ✅ the schema is the contract.
- ❌ Putting a field in the manifest that is not in the schema → ✅ see pruning.
- ❌ Changing the schema of `v1` incompatibly → ✅ add a new version (see the [expert theory](expert.en.md)).

## Related materials

- Next: [advanced theory](advanced.en.md) — CEL, pruning, a toy operator, cert-manager
- Controllers and the API server: [02 · Architecture](../02-architecture/overview.en.md)
- Namespace and RBAC: [10 · Namespaces and RBAC](../10-namespaces-and-rbac/overview.en.md)

## Sources

- [Custom Resources](https://kubernetes.io/docs/concepts/extend-kubernetes/api-extension/custom-resources/)
- [Extend the Kubernetes API with CustomResourceDefinitions](https://kubernetes.io/docs/tasks/extend-kubernetes/custom-resources/custom-resource-definitions/)
- [Operator pattern](https://kubernetes.io/docs/concepts/extend-kubernetes/operator/)
