---
title: "Quiz: ConfigMap and Secret"
lang: en
track: kubernetes
kind: module
doc: quiz
slug: 07-config-and-secrets
part: 1
tags: [configmap, secret, environment-variables, volume, base64, immutable]
status: draft
updated: 2026-11-22
---

# Quiz: ConfigMap and Secret

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What does the `data` field of a Secret contain?

- A) Encrypted values
- B) Values in base64
- C) Values as plain text
- D) Hashes of the values

<details><summary>Answer</summary>

**B.** base64 is an encoding, the value is decoded with `base64 -d` (in the experiment `UzNjcjN0IQ==` → `S3cr3t!`). There is no encryption. Plain text goes into `stringData`, and the API stores it in `data` in base64.
→ [Basics](basics.en.md#secret)

</details>

## 2. 🟢 How does `envFrom` differ from `env` + `valueFrom`?

- A) It does not
- B) `envFrom` attaches all keys of the object as variables, `valueFrom` one value under a chosen name
- C) `envFrom` works only for a Secret
- D) `valueFrom` updates the value on the fly

<details><summary>Answer</summary>

**B.** `envFrom` takes all keys (optionally with a `prefix`), `valueFrom` a specific key. Both fix the values at container start.
→ [Basics](basics.en.md#how-to-attach-to-a-pod)

</details>

## 3. 🟢 Which flag cannot be mixed with `--from-file` and `--from-literal` in `kubectl create configmap`?

- A) `--dry-run`
- B) `--from-env-file`
- C) `--namespace`
- D) `--output`

<details><summary>Answer</summary>

**B.** The error: `from-env-file cannot be combined with from-file or from-literal`.
→ [Basics](basics.en.md#configmap)

</details>

## 4. 🟢 Which value of the key `TIMEOUT` in a ConfigMap is valid in YAML?

- A) `TIMEOUT: 30`
- B) `TIMEOUT: "30"`
- C) Any number
- D) Only base64 values

<details><summary>Answer</summary>

**B.** `data` values are strings; a number must be quoted. Binary data goes into `binaryData`.
→ [Basics](basics.en.md#creation)

</details>

## 5. 🟡 You changed a ConfigMap. What is updated in a running pod?

- A) Everything at once
- B) The volume files in about a minute; environment variables and a file through `subPath` are not
- C) Only environment variables
- D) Nothing, not even volume files

<details><summary>Answer</summary>

**B.** In the experiment the volume updated after ~57 s, the variable stayed `prod`, the file through `subPath` was `prod` too.
→ [Advanced](advanced.en.md#updating-values)

</details>

## 6. 🟡 What is the status of a pod that refers to a non-existent ConfigMap key?

- A) `Pending`
- B) `ImagePullBackOff`
- C) `CreateContainerConfigError`
- D) `CrashLoopBackOff`

<details><summary>Answer</summary>

**C.** The event: `couldn't find key nokey in ConfigMap learn-07/app-config`. With `optional: true` the pod starts without the variable.
→ [Advanced](advanced.en.md#missing-objects-and-keys)

</details>

## 7. 🟡 Select all correct statements

- [ ] A) The `data` of an `immutable: true` object cannot be changed
- [ ] B) A Secret volume is mounted as `tmpfs`
- [ ] C) `kubectl describe secret` shows values in plain form
- [ ] D) Changing a ConfigMap automatically restarts the Deployment

<details><summary>Answer</summary>

**A, B.** C is wrong: `describe` shows only sizes (`password: 7 bytes`). D is wrong: the number of ReplicaSets did not change, the pods kept the old values.
→ [Advanced](advanced.en.md)

</details>

## 8. 🟡 How do you make a Deployment pick up a new ConfigMap value?

- A) Wait a minute
- B) Change the pod template: `rollout restart`, a ConfigMap hash in an annotation or a new ConfigMap name
- C) Delete the ConfigMap
- D) Increase `replicas`

<details><summary>Answer</summary>

**B.** A rollout is triggered only by a change to the pod template. New pods from increasing `replicas` also get the new value, but already running ones keep the old one.
→ [Advanced](advanced.en.md#a-configmap-change-does-not-restart-pods)

</details>

## 9. 🔴 Find the flaw

> "We encrypt secrets: in the manifest they are base64, and even `kubectl describe` does not show them. So an attacker with access to an etcd backup will read nothing."

<details><summary>Answer</summary>

Wrong: base64 is not encryption, and in etcd a Secret lies as plain text by default (in the experiment `password..S3cr3t!` in the key value, and the flag `--encryption-provider-config` is absent). `describe` hides values only in kubectl's output. You need etcd encryption (`EncryptionConfiguration`/KMS), RBAC and protected backups.
→ [Expert](expert.en.md#gotcha-a-secret-lies-in-etcd-as-plain-text)

</details>

## 10. 🔴 Open question

How would you organize storing secrets for an application in a GitOps process (manifests in Git)? Name at least two approaches and their trade-offs.

<details><summary>Key points of a good answer</summary>

- A `Secret` must not be kept in Git as plain text (base64 and `stringData` are no protection).
- Sealed Secrets: encryption with the cluster's public key, Git holds a `SealedSecret`; a plus is simplicity, a minus is that the key is bound to the cluster.
- SOPS: encrypting values right in the manifest; needs a key (KMS, age) and GitOps integration.
- External Secrets Operator or Secrets Store CSI Driver: Git holds only a reference to a secret in an external store (Vault, a cloud secret manager); a minus is dependence on an external system.
- In any case: RBAC on `secrets`, etcd encryption, mounting as files with `0400` permissions.

</details>

→ [Expert](expert.en.md#external-secret-stores-and-alternatives)
