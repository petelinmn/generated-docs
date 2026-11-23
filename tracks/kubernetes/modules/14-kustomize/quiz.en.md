---
title: "Quiz: Kustomize"
lang: en
track: kubernetes
kind: module
doc: quiz
slug: 14-kustomize
part: 2
tags: [kustomize, overlay, patch, configmap-generator]
status: draft
updated: 2026-11-23
---

# Quiz: Kustomize

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 Which command shows the build result without applying it?

- A) `kubectl apply -k <dir>`
- B) `kubectl kustomize <dir>`
- C) `kubectl get -k <dir>`
- D) `kubectl delete -k <dir>`

<details><summary>Answer</summary>

**B.** `apply -k` applies, `get -k` and `delete -k` work with the objects from the result.
→ [Basics](basics.en.md#applying)

</details>

## 2. 🟢 What does `namePrefix: prod-` do in an overlay?

- A) Adds a prefix to object names and updates references to them
- B) Adds a prefix only to the Deployment
- C) Changes the namespace
- D) Creates a new namespace `prod-`

<details><summary>Answer</summary>

**A.** In the experiment `web` became `prod-web` in all objects.
→ [Basics](basics.en.md#overlay)

</details>

## 3. 🟢 `kubectl apply -k` into a namespace that does not exist. What happens?

- A) The namespace is created
- B) The error `namespaces "…" not found`
- C) Objects go to `default`
- D) Only the Service is applied

<details><summary>Answer</summary>

**B.** Kustomize only fills the namespace into the objects.
→ [Basics](basics.en.md#applying)

</details>

## 4. 🟢 How do you change the `nginx` image tag to `1.29` in an overlay?

- A) `images: [{name: nginx, newTag: "1.29"}]`
- B) Copy the Deployment and edit by hand
- C) `replicas: [{name: nginx, count: 1}]`
- D) You cannot

<details><summary>Answer</summary>

**A.** In the experiment the result was `image: nginx:1.29`.
→ [Basics](basics.en.md#main-transformers)

</details>

## 5. 🟡 A patch is written for `Deployment/ghost`, which does not exist. What happens?

- A) The patch is silently skipped
- B) The error `no resource matches strategic merge patch`
- C) The object `ghost` is created
- D) It is applied to the first Deployment

<details><summary>Answer</summary>

**B.** A patch for a non-existent object is not ignored.
→ [Advanced](advanced.en.md#patch-errors)

</details>

## 6. 🟡 You changed `LOG_LEVEL` in `configMapGenerator` and ran `apply -k`. What happens to the pods?

- A) Nothing, the ConfigMap is updated in place
- B) A new ConfigMap with a new hash is created, the Deployment gets the new name and restarts pods
- C) The apply fails
- D) The old ConfigMap is deleted

<details><summary>Answer</summary>

**B.** In the experiment the name changed from `…dfk4bdbtkk` to `…47668c6k28` and a rolling update ran. The old ConfigMap stayed.
→ [Advanced](advanced.en.md#configmap-and-secret-generators)

</details>

## 7. 🟡 Select all correct statements

- [ ] A) A JSON 6902 patch is a list of `op/path/value` operations
- [ ] B) A strategic merge patch finds a container by its `name` field
- [ ] C) `labels` with `includeSelectors: true` adds the label to selectors as well
- [ ] D) `commonLabels` is the recommended way to add labels

<details><summary>Answer</summary>

**A, B, C.** D is wrong: `commonLabels` is deprecated (`Please use 'labels' instead`) and changes selectors.
→ [Advanced](advanced.en.md#patches)

</details>

## 8. 🟡 What happens with `disableNameSuffixHash: true` if you change the ConfigMap data?

- A) Pods restart
- B) The name does not change, pods do not restart
- C) An error
- D) The ConfigMap is deleted

<details><summary>Answer</summary>

**B.** The pod template did not change; a manual restart is needed.
→ [Advanced](advanced.en.md#configmap-and-secret-generators)

</details>

## 9. 🔴 Find the flaw

> "We added `commonLabels: {team: a}` to a running Deployment and everything applied without problems."

<details><summary>Answer</summary>

`commonLabels` changes the `selector` too; a Deployment's selector is immutable, so for an already existing object the apply will be rejected (in the experiment the build put the label in five places, including selectors). Use `labels` without `includeSelectors`.
→ [Pitfall 1](expert.en.md#1-commonlabels-changes-selectors)

</details>

## 10. 🔴 What will the build print?

```yaml
resources: [deployment.yaml, service.yaml, deployment.yaml]
```

- A) Two Deployments
- B) The error `may not add resource with an already registered id`
- C) One Deployment, the duplicate dropped
- D) A warning and a successful build

<details><summary>Answer</summary>

**B.** A resource identifier is unique within a build.
→ [Internals](expert.en.md#internals)

</details>

## 11. 🔴 Open question

Compare Kustomize and Helm and say how you would choose between them.

<details><summary>Key points of a good answer</summary>

- Kustomize: plain YAML, base/overlay and patches, built into kubectl; no releases, history or packages.
- Helm: templates and values, releases with history and rollbacks, hooks, ready charts; the cost is template complexity.
- Own services per environment — Kustomize; third-party components and rollbacks — Helm; they can be combined (`--enable-helm`, not tested).

</details>

→ [Expert](expert.en.md#trade-offs-and-alternatives)
