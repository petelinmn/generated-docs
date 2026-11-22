---
title: "Quiz: Deployment and ReplicaSet"
lang: en
track: kubernetes
kind: module
doc: quiz
slug: 05-deployments
part: 1
tags: [deployment, replicaset, rolling-update, rollback, progress-deadline]
status: draft
updated: 2026-11-22
---

# Quiz: Deployment and ReplicaSet

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What does a Deployment create when started with 3 replicas?

- A) Three pods directly
- B) A ReplicaSet that creates three pods
- C) Three ReplicaSets with one pod each
- D) A StatefulSet

<details><summary>Answer</summary>

**B.** The chain is Deployment → ReplicaSet → Pod; in the experiment there was one ReplicaSet `web-6d8567df8` and three pods with the same prefix.
→ [Basics](basics.en.md#deployment-replicaset-and-pods)

</details>

## 2. 🟢 What triggers a rolling update?

- A) Any change to the Deployment
- B) A change to the pod template (`spec.template`)
- C) A change to `replicas`
- D) Deleting a pod

<details><summary>Answer</summary>

**B.** A template change gives a new hash and a new ReplicaSet. Changing `replicas` merely scales the current ReplicaSet, deleting a pod recreates it.
→ [Basics](basics.en.md#updating-the-image)

</details>

## 3. 🟢 How do you roll a Deployment back to the previous version?

- A) `kubectl rollout undo deploy/web`
- B) Delete all pods
- C) `kubectl scale --replicas=0`
- D) It cannot be rolled back

<details><summary>Answer</summary>

**A.** The command returns the template of the previous revision; old ReplicaSets are kept for this (`revisionHistoryLimit`, 10 by default).
→ [Basics](basics.en.md#rollback)

</details>

## 4. 🟢 What must match in a Deployment?

- A) The Deployment name and the image name
- B) `selector.matchLabels` and the labels `template.metadata.labels`
- C) `replicas` and the number of nodes
- D) The namespace and the pod name

<details><summary>Answer</summary>

**B.** Otherwise the API rejects the object: ``selector` does not match template `labels` ``.
→ [Basics](basics.en.md#the-deployment-manifest)

</details>

## 5. 🟡 How many pods does a rolling update replace at a time with 3 replicas and the default values?

- A) All three at once
- B) One at a time: `maxSurge` rounds up to 1, `maxUnavailable` down to 0
- C) Two at a time
- D) None until the old ones are removed by hand

<details><summary>Answer</summary>

**B.** 25% of 3 = 0.75: `maxSurge` → 1, `maxUnavailable` → 0. In the ReplicaSet trace you can see the new RS first gets 1 replica, and the old one shrinks only after the new pod is ready.
→ [Advanced](advanced.en.md#rolling-update-step-by-step)

</details>

## 6. 🟡 The image in a Deployment is specified with a typo. What happens?

- A) All pods crash
- B) The new pod is in `ErrImagePull`, the old pods keep working, the Deployment shows `READY 3/3`
- C) Kubernetes rolls back automatically
- D) The Deployment is deleted

<details><summary>Answer</summary>

**B.** A rolling update does not delete old pods until the new ones are ready. The rollback is not automatic: after `progressDeadlineSeconds` `ProgressDeadlineExceeded` appears, but the pods stay. The rollback is `kubectl rollout undo`.
→ [Advanced](advanced.en.md#a-stuck-rollout)

</details>

## 7. 🟡 Select all correct statements

- [ ] A) `Recreate` stops all old pods before starting new ones
- [ ] B) `kubectl rollout restart` kills all pods at once
- [ ] C) After `rollout pause` template changes do not start a rollout until `resume`
- [ ] D) The default value of `progressDeadlineSeconds` is 600

<details><summary>Answer</summary>

**A, C, D.** B is wrong: `restart` adds an annotation to the template, and pods are replaced by the update strategy.
→ [Advanced](advanced.en.md)

</details>

## 8. 🟡 Why did `rollout history` show the same `CHANGE-CAUSE` on revisions 1 and 2?

- A) It is a kubectl bug
- B) The annotation `kubernetes.io/change-cause` was set before the image change and was copied to the old ReplicaSet
- C) Both revisions are identical
- D) History is not stored

<details><summary>Answer</summary>

**B.** The label must be set after changing the template.
→ [Advanced](advanced.en.md#the-change-reason-change-cause)

</details>

## 9. 🔴 Find the flaw

> "In CI after `kubectl set image` I check `kubectl get deploy web` and consider the rollout successful by `READY 3/3`."

<details><summary>Answer</summary>

With a wrong image `READY 3/3` and `Available=True`, because the old pods keep working; the new pod in `ErrImagePull` is visible only as `UP-TO-DATE 1`. You must check `kubectl rollout status deploy/web --timeout=…`: its return code reflects the update result.
→ [Expert](expert.en.md#gotcha-a-stuck-rollout-looks-healthy)

</details>

## 10. 🔴 Open question

Explain how Kubernetes tracks Deployment revisions and what happens to the numbers on a rollback. How does this affect keeping a release history?

<details><summary>Key points of a good answer</summary>

- Each template version is a separate ReplicaSet with the annotation `deployment.kubernetes.io/revision`; the template hash is in the name and the `pod-template-hash` label.
- A rollback reuses the old ReplicaSet and gives it a new largest number; the former number disappears (in the experiment revision 1 became 3, `revision-history: "1"`).
- The number of stored revisions is `revisionHistoryLimit` (10 by default).
- A revision number is not a permanent release identifier: keep history in Git and labels; set `change-cause` after changing the template.

</details>

→ [Expert](expert.en.md#gotcha-revision-numbers-change-after-a-rollback)
