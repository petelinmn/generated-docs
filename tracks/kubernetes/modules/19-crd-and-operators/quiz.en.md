---
title: "Quiz: CRDs and operators"
lang: en
track: kubernetes
kind: module
doc: quiz
slug: 19-crd-and-operators
part: 3
tags: [crd, operator, cel, status-subresource]
status: draft
updated: 2026-11-23
---

# Quiz: CRDs and operators

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What does a CRD do?

- A) Runs a controller for the new type
- B) Adds a new object type with a schema to the cluster API
- C) Creates a Deployment
- D) Installs a Helm chart

<details><summary>Answer</summary>

**B.** A CRD by itself only stores and validates objects; a controller has to act (together they form an operator).
→ [Overview](overview.en.md#in-short)

</details>

## 2. 🟢 What is the CRD name for `kind: Widget`, `plural: widgets`, `group: example.com`?

- A) `widget.example.com`
- B) `widgets.example.com`
- C) `example.com/widgets`
- D) `widgets`

<details><summary>Answer</summary>

**B.** The format is `<plural>.<group>`.
→ [Basics](basics.en.md#create-a-crd)

</details>

## 3. 🟢 The object manifest has no `replicas` field, and the schema has `default: 1`. What is in `get -o yaml`?

- A) No field
- B) `replicas: 1`
- C) An error
- D) `replicas: null`

<details><summary>Answer</summary>

**B.** The API server fills the value in.
→ [Basics](basics.en.md#create-an-object)

</details>

## 4. 🟢 What does an attempt to create an object with `replicas: 9` return when `maximum: 5`?

- A) The object is created, the value is cut to 5
- B) A validation error: the value must be at most 5
- C) A warning and creation
- D) The object is created with 1

<details><summary>Answer</summary>

**B.** `spec.replicas in body should be less than or equal to 5`.
→ [Basics](basics.en.md#schema-validation)

</details>

## 5. 🟡 The CEL rule `self.tier != 'free' || self.replicas <= 2` and an object without `tier`. What happens?

- A) The rule evaluates to true
- B) The error `no such key: tier`, the change is rejected
- C) The rule is skipped
- D) `tier` gets the value `free`

<details><summary>Answer</summary>

**B.** You need `!has(self.tier) || …`.
→ [Advanced](advanced.en.md#cel-rules)

</details>

## 6. 🟡 You applied an object with `color: red`, which is not in the schema, with `--validate=false`. What results?

- A) The field was saved
- B) The server silently dropped the field (pruning)
- C) A server error
- D) The object was not created

<details><summary>Answer</summary>

**B.** Without `--validate=false` `kubectl` would refuse with a `strict decoding error`.
→ [Advanced](advanced.en.md#pruning-unknown-fields)

</details>

## 7. 🟡 `kubectl patch widget w1 -p '{"status":{"phase":"Ready"}}'` with the `status` subresource enabled. The result?

- A) The status is updated
- B) `patched (no change)`
- C) A permissions error
- D) The object is deleted

<details><summary>Answer</summary>

**B.** You need `--subresource=status`.
→ [Advanced](advanced.en.md#the-status-subresource)

</details>

## 8. 🟡 What happens to a ConfigMap whose `ownerReferences` point to a Widget after the Widget is deleted?

- A) It stays
- B) The garbage collector deletes it
- C) It becomes `Terminating` forever
- D) It moves to another owner

<details><summary>Answer</summary>

**B.** In the experiment `configmaps "owned" not found`.
→ [Advanced](advanced.en.md#ownerreferences-and-garbage-collection)

</details>

## 9. 🟡 Select all correct statements

- [ ] A) An operator needs RBAC for its own resources and their `status`
- [ ] B) The reconcile loop must be idempotent
- [ ] C) An operator runs inside the API server
- [ ] D) cert-manager uses the CRDs `Certificate` and `Issuer`

<details><summary>Answer</summary>

**A, B, D.** C is wrong: an operator is a separate application (a pod) that talks to the API.
→ [Advanced](advanced.en.md#a-toy-operator)

</details>

## 10. 🔴 Find the flaw

> "An operator reacts to events: if it misses an event about a change, the state stays wrong forever."

<details><summary>Answer</summary>

Controllers work "by level": at each reconcile they compare the actual and desired state, so the next reconcile makes up for a missed event. In the experiment the operator undid a manual `scale` without an event.
→ [Internals](expert.en.md#internals)

</details>

## 11. 🔴 `kubectl patch --subresource=status` from an operator gives `Forbidden: cannot get resource "widgets/status"`. What do you fix?

- A) Add the `get` verb on `widgets/status` to the role
- B) Remove `status` from the CRD
- C) Grant cluster-admin
- D) Restart the API server

<details><summary>Answer</summary>

**A.** `kubectl patch` reads the object before writing; targeted rights are better than cluster-admin.
→ [Gotcha 3](expert.en.md#3-rbac-on-status-and-get)

</details>

## 12. 🔴 Open question

When should you write an operator and when is a Helm chart enough?

<details><summary>Key points of a good answer</summary>

- An operator — when you need continuous reconcile and operational logic (backups, upgrades, recovery), and the state is described by your own API.
- Helm — for one-off installation and parameterization without "day 2" logic.
- First look for a ready-made operator; write your own only for your domain.
- The price of an operator: code, CRD versions, RBAC, operations.

</details>

→ [Expert](expert.en.md#trade-offs-and-alternatives)
