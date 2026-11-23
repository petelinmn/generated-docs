---
title: "Exercises: CRDs and operators"
lang: en
track: kubernetes
kind: module
doc: exercises
slug: 19-crd-and-operators
part: 3
tags: [crd, operator, cel, cert-manager]
status: draft
updated: 2026-11-23
---

# Exercises: CRDs and operators

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself; hints and solutions are under spoilers. You need a kind cluster and a namespace: `kubectl create namespace learn-19`. Exercise 5 needs Helm and internet access. Afterwards: `kubectl delete crd widgets.example.com`, `kubectl delete namespace learn-19`.

## 1. 🟢 The first CRD

**Task:** create the CRD `widgets.example.com` (`spec.image` required, `spec.replicas` 1–5 with default 1) and an object `w1` without `replicas`. Show that the value was filled in.

**Expected result:** `{"image":"nginx:1.29","replicas":1}`.

<details><summary>Hint</summary>

The manifest is in the [basic theory](basics.en.md#create-a-crd); `kubectl get widget w1 -o jsonpath='{.spec}'`.

</details>

<details><summary>Solution</summary>

```text
$ kubectl -n learn-19 get widget w1 -o jsonpath='{.spec}'
{"image":"nginx:1.29","replicas":1}
```

</details>

## 2. 🟢 Schema rejections

**Task:** try to create objects: without `image`; with `replicas: 9`; with `tier: gold` (if the schema has `enum: [free, pro]`); with `replicas: "two"`. Write down the messages.

**Expected result:** `Required value`, `should be less than or equal to 5`, `Unsupported value: "gold"`, `must be of type integer`.

<details><summary>Hint</summary>

Each message contains the field path (`spec.replicas`).

</details>

<details><summary>Solution</summary>

```text
spec.image: Required value
spec.replicas: Invalid value: 9: spec.replicas in body should be less than or equal to 5
spec.tier: Unsupported value: "gold": supported values: "free", "pro"
spec.replicas: Invalid value: "string": spec.replicas in body must be of type integer: "string"
```

</details>

## 3. 🟡 CEL and `has()`

**Task:** add the rule `self.tier != 'free' || self.replicas <= 2` and test it on an object without `tier`. Fix the rule and make sure `tier: free, replicas: 4` is rejected while an object without `tier` works.

**Expected result:** first `no such key: tier`, then `free tier allows at most 2 replicas` only with `tier: free`.

<details><summary>Hint</summary>

`!has(self.tier) || …`

</details>

<details><summary>Solution</summary>

```yaml
x-kubernetes-validations:
- rule: "!has(self.tier) || self.tier != 'free' || self.replicas <= 2"
  message: "free tier allows at most 2 replicas"
```

</details>

## 4. 🟡 Status and pruning

**Task:** (a) write `status.phase` with an ordinary `patch` and with `--subresource=status`. (b) Apply an object with an extra field `color` strictly and with `--validate=false`; see what was saved.

**Expected result:** (a) `(no change)` and then `Ready`; (b) `strict decoding error: unknown field "spec.color"`, then `{"image":"nginx","replicas":1}`.

<details><summary>Hint</summary>

`kubectl get wg` shows the `Phase` column.

</details>

<details><summary>Solution</summary>

The commands and the result are in the [advanced theory](advanced.en.md#the-status-subresource) and [here](advanced.en.md#pruning-unknown-fields).

</details>

## 5. 🔴 An operator: your own and a ready-made one

**Task:** (a) run the toy operator from the [advanced theory](advanced.en.md#a-toy-operator) (a ConfigMap with the script, a ServiceAccount, a ClusterRole, a Deployment). Create a `Widget` with `replicas: 2`, then change it to 3, then reduce the Deployment by hand, then delete the Widget. (b) Install cert-manager (`crds.enabled=true`), create an `Issuer` (selfSigned) and a `Certificate`, find the Secret and delete it.

**Expected result:** (a) the Deployment follows `spec`, the manual `scale` is undone, and deleting the Widget removes the Deployment; (b) a Secret `kubernetes.io/tls`, created again after deletion.

<details><summary>Hint</summary>

Do not forget `get` for `widgets/status` in the ClusterRole.

</details>

<details><summary>Solution</summary>

```text
$ kubectl -n learn-19 get wg
NAME   REPLICAS   PHASE
shop   3          Ready
$ kubectl -n learn-19 get deploy widget-shop
widget-shop   3/3   3   3
$ kubectl -n learn-19 get certificate,issuer
certificate.cert-manager.io/demo    True   demo-tls
issuer.cert-manager.io/selfsigned   True
```

</details>
