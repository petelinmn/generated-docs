---
title: "Quiz: Helm"
lang: en
track: kubernetes
kind: module
doc: quiz
slug: 13-helm
part: 2
tags: [helm, chart, values, release]
status: draft
updated: 2026-11-23
---

# Quiz: Helm

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What is a Helm release?

- A) An application version in Git
- B) An installed instance of a chart in the cluster with a revision history
- C) A `.tgz` archive
- D) A `values.yaml` file

<details><summary>Answer</summary>

**B.** The archive is a packaged chart, `values.yaml` holds default values.
→ [Basics](basics.en.md#release-lifecycle)

</details>

## 2. 🟢 How do you view the final manifests without contacting the cluster?

- A) `helm get manifest`
- B) `helm template`
- C) `helm status`
- D) `helm test`

<details><summary>Answer</summary>

**B.** `get manifest` shows what is already installed, `status` — the release state, `test` runs test hooks.
→ [Basics](basics.en.md#viewing-the-result-without-installing)

</details>

## 3. 🟢 A rollback `helm rollback web 1` with two revisions. What is in the history?

- A) Revision 2 is deleted
- B) Revision 3 "Rollback to 1" appears
- C) Revision 1 becomes current with no change to the history
- D) A rollback is impossible

<details><summary>Answer</summary>

**B.** In the experiment: `1 superseded`, `2 superseded`, `3 deployed (Rollback to 1)`.
→ [Basics](basics.en.md#release-lifecycle)

</details>

## 4. 🟢 Where does the default image tag come from if `image.tag` is empty?

- A) From `appVersion` in `Chart.yaml` (if the template is written so)
- B) Always `latest`
- C) From `Release.Name`
- D) Helm chooses it

<details><summary>Answer</summary>

**A.** The template uses `.Values.image.tag | default .Chart.AppVersion`; in the experiment the result was `nginx:1.29`.
→ [Basics](basics.en.md#templates)

</details>

## 5. 🟡 Order: `-f v.yaml` sets `replicaCount: 5`, and `--set replicaCount=7`. What results?

- A) 5
- B) 7
- C) 12
- D) An error

<details><summary>Answer</summary>

**B.** `--set` has the highest priority.
→ [Advanced](advanced.en.md#values-precedence)

</details>

## 6. 🟡 You ran `helm upgrade web mini --set replicaCount=4`, then `helm upgrade web mini --set image.tag=1.28`. How many replicas?

- A) 4
- B) The default from `values.yaml`
- C) 1
- D) An error

<details><summary>Answer</summary>

**B.** Without `--reuse-values` earlier `--set` values are not inherited (`replicaCount=4` disappeared from `helm get values`).
→ [Advanced](advanced.en.md#--reuse-values-and-the-reset-on-upgrade)

</details>

## 7. 🟡 An upgrade with `--wait` and a non-existent image ended with a timeout. What is in the history?

- A) A `deployed` revision
- B) A `failed` revision, no automatic rollback
- C) A rollback to the previous revision
- D) The history did not change

<details><summary>Answer</summary>

**B.** The rollback is provided by `--rollback-on-failure` (`--atomic`); then a "Rollback to N" revision appears.
→ [Advanced](advanced.en.md#failed-update---wait-and---rollback-on-failure)

</details>

## 8. 🟡 Select all correct statements about hooks

- [ ] A) A `pre-install` Job runs before the Deployment is created
- [ ] B) With `hook-succeeded` the Job is deleted after success
- [ ] C) A hook resource is part of the ordinary release manifest
- [ ] D) A test hook is started by `helm test`

<details><summary>Answer</summary>

**A, B, D.** C is wrong: hooks are managed separately and visible in `helm get hooks`.
→ [Advanced](advanced.en.md#hooks)

</details>

## 9. 🟡 How do you disable the subchart `mini` through values?

- A) Delete `charts/mini`
- B) `--set mini.enabled=false` with `condition: mini.enabled`
- C) You cannot
- D) `helm uninstall mini`

<details><summary>Answer</summary>

**B.** In the experiment with `mini.enabled=false` there were no manifests.
→ [Advanced](advanced.en.md#dependencies-subcharts)

</details>

## 10. 🔴 Find the flaw

> "Secrets in `values.yaml` are safe because Helm stores a release in a closed format."

<details><summary>Answer</summary>

A release is stored in a Secret as gzip and two layers of base64 — that is encoding, not encryption: anyone who can read the Secret gets the values. Besides, `values.yaml` in Git is visible to everyone.
→ [Internals](expert.en.md#internals)

</details>

## 11. 🔴 `helm upgrade` fails: `conflict with "kubectl.exe" with subresource "scale"`. Cause and fix?

- A) A network error
- B) The field `.spec.replicas` was taken by `kubectl scale`; apply with `--force-conflicts` or change `replicaCount`
- C) No RBAC permissions
- D) An outdated chart

<details><summary>Answer</summary>

**B.** Helm 4 uses server-side apply; the conflict is with a field manager.
→ [Advanced](advanced.en.md#server-side-apply-in-helm-4)

</details>

## 12. 🔴 Open question

When would you choose Helm and when Kustomize?

<details><summary>Key points of a good answer</summary>

- Helm: ready-made third-party charts, parameterization, release history and rollbacks, hooks and tests; the cost is Go templates and a Secret per release.
- Kustomize: plain YAML and environment overlays, built into kubectl; no history or packages.
- They are often combined: Helm for others' components, Kustomize or GitOps for your own.

</details>

→ [Expert](expert.en.md#trade-offs-and-alternatives)
