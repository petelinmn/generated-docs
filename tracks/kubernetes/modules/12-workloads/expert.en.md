---
title: "Other workloads: expert theory"
lang: en
track: kubernetes
kind: module
doc: expert
slug: 12-workloads
part: 2
tags: [statefulset, daemonset, job, cronjob, partition, podmanagementpolicy, pvc-retention, backofflimit, concurrency-policy, indexed-job]
versions: "Kubernetes 1.37.0 (kind v0.34), kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Other workloads: expert theory

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on [basic](basics.en.md) and [advanced](advanced.en.md) theory. All values below were obtained on Kubernetes 1.37.0 (`kubectl explain`, experiments in kind).

## Field reference

The list of `spec` fields was obtained with `kubectl explain <kind>.spec`.

### StatefulSet

| Field | Meaning | Default (observed) |
|---|---|---|
| `replicas` | number of pods | 1 |
| `selector` | pod selector (required, matches the template labels) | — |
| `serviceName` | name of the headless Service for DNS | not required (a set without it was created) |
| `template` | pod template | — |
| `volumeClaimTemplates` | PVC templates, one PVC per replica | empty |
| `podManagementPolicy` | `OrderedReady` or `Parallel` | `OrderedReady` |
| `updateStrategy` | `RollingUpdate` (with `partition`, `maxUnavailable`) or `OnDelete` | `RollingUpdate`, `partition: 0`, `maxUnavailable: 1` |
| `persistentVolumeClaimRetentionPolicy` | `whenDeleted`, `whenScaled`: `Retain`/`Delete` | both `Retain` |
| `minReadySeconds` | how long a pod must be ready to count as available | 0 |
| `revisionHistoryLimit` | how many revisions to keep | — |
| `ordinals` | starting number of replicas (`ordinals.start`) | — |

### DaemonSet

| Field | Meaning | Default |
|---|---|---|
| `selector`, `template` | as in Deployment | — |
| `updateStrategy` | `RollingUpdate` (`maxUnavailable`, `maxSurge`) or `OnDelete` | `{"rollingUpdate":{"maxSurge":0,"maxUnavailable":1},"type":"RollingUpdate"}` |
| `minReadySeconds` | readiness pause | 0 |
| `revisionHistoryLimit` | number of revisions | — |

A DaemonSet has no `replicas` field.

### Job

| Field | Meaning |
|---|---|
| `completions` | how many successful pods are needed |
| `parallelism` | how many pods at a time |
| `completionMode` | `NonIndexed` or `Indexed` (index in `JOB_COMPLETION_INDEX`) |
| `backoffLimit` | number of retries before failure |
| `backoffLimitPerIndex`, `maxFailedIndexes` | retries per index (for `Indexed`) |
| `activeDeadlineSeconds` | time limit of the whole Job |
| `ttlSecondsAfterFinished` | automatic deletion after completion |
| `suspend` | pause |
| `podFailurePolicy` | rules for reacting to pod failure reasons |
| `podReplacementPolicy` | when to create a replacement (for example, after the pod fully terminates) |
| `successPolicy` | success condition without waiting for all indexes |
| `selector`, `manualSelector` | manual selector (usually not needed) |
| `managedBy` | external controller that drives the Job |
| `scheduling` | changing scheduling parameters while the Job is suspended |
| `template` | pod template (`restartPolicy` only `Never` or `OnFailure`) |

### CronJob

| Field | Meaning | Default (observed) |
|---|---|---|
| `schedule` | 5-field cron | required |
| `jobTemplate` | Job template | required |
| `concurrencyPolicy` | `Allow`/`Forbid`/`Replace` | `Allow` |
| `suspend` | pause | `false` |
| `startingDeadlineSeconds` | how late a delayed run may still be | not set |
| `timeZone` | schedule time zone | not set |
| `successfulJobsHistoryLimit`, `failedJobsHistoryLimit` | how many finished Jobs to keep | — |

> ⚠️ Verify: the numeric defaults of `revisionHistoryLimit`, `successfulJobsHistoryLimit`, `failedJobsHistoryLimit`, `minReadySeconds` for StatefulSet/DaemonSet/CronJob were not captured here (in the CronJob experiment the history limit was set explicitly: 2).

## Internals

- **Controllers.** Each object is handled by its own controller in kube-controller-manager: it watches the object and the pods via watch ([module 02](../02-architecture/advanced.en.md)) and brings the actual state to the desired one. The names and identity of StatefulSet pods are derived from the number: the pod `web-1` is "slot number 1", not a specific instance. That is why after deletion a new pod with the same name appears, with a new UID and a new IP (experiment: `10.244.0.31` → `10.244.0.35`).
- **A PVC is tied to the name.** The controller looks for the PVC by the template `<template name>-<set name>-<number>` (`data-web-1`). If it exists, the new pod uses it — that is how the data is preserved.
- **DaemonSet and the scheduler.** A DaemonSet pod is created bound to a specific node; to land on a node with a taint, the template needs a toleration (experiment: without it 2 pods of 3 nodes, with it 3).
- **A Job tracks conditions.** On failure a Job sets `FailureTarget`, then `Failed` with a reason (`BackoffLimitExceeded`); success is the `Complete` condition. The status keeps `succeeded`, `failed`, `active`, and for `Indexed` — `completedIndexes`.
- **A CronJob creates Jobs.** A Job name is composed of the CronJob name and the launch time in minutes since the epoch (`tick-29923833`); the owner is the CronJob (`ownerReferences`), so deleting the CronJob deletes its Jobs and pods. `lastScheduleTime` is the time of the last run in UTC.

## Pitfalls

### 1. `rollout status` with partition says "complete"

```text
$ kubectl -n learn-12 rollout status sts/web
partitioned roll out complete: 3 new pods have been updated
```

(with `partition: 2` in fact one pod was updated). Check `status.updateRevision` against `currentRevision` and the pod images.

### 2. `scale` down does not free PVCs

After `scale --replicas=1` the PVCs `data-web-0`, `data-web-1`, `data-web-2` remained. The disk space keeps being used; clean it up manually or set `whenScaled: Delete`.

### 3. A pod with the same name is a different object

After `delete pod web-1` a pod with the same name appears, but with a new UID and a new IP (`10.244.0.31` → `10.244.0.35`). You cannot cache a replica's IP; address it by the DNS name `web-1.web`.

### 4. OrderedReady is blocked by the first replica

If `stuck-0` does not pass readiness, `stuck-1` and `stuck-2` will not appear. With `Parallel` all three are created at once. That is why a broken image in a StatefulSet looks like "one pod in `Running 0/1` and nothing else".

### 5. A Job creates pods even if they keep failing

Three `Error` pods with `backoffLimit: 2` and the condition `BackoffLimitExceeded`. With `restartPolicy: OnFailure` the restart happens inside the pod (`RESTARTS` grows) and no new pod is created.

### 6. A CronJob skips a run with `Forbid`

The event `JobAlreadyActive` is not an error but a normal skip. The history and `lastScheduleTime` do not grow for skipped runs.

### 7. Deleting a CronJob deletes the history

Jobs are owned by the CronJob; after the CronJob is deleted, the logs of finished Jobs are lost unless the pods' logs are collected into a log store.

### 8. TTL deletes logs too

With `ttlSecondsAfterFinished: 15` the Job and its pods disappeared in about a minute and a half; collect logs beforehand or ship them to external storage.

> ⚠️ Verify: how a CronJob reacts to runs missed due to downtime (`startingDeadlineSeconds`, "more than 100 missed start times") and the behavior of `podFailurePolicy`, `successPolicy`, `backoffLimitPerIndex`, `managedBy` were not run in this material.

## Trade-offs and alternatives

| Task | Option | Trade-off |
|---|---|---|
| A database in Kubernetes | StatefulSet by hand | you own replication, backups, failover |
| A database in Kubernetes | an operator (CRD) | automation, but more complex and depends on the operator |
| A database | a managed database outside the cluster | simpler operation, but cost and networking |
| An agent on a node | DaemonSet | a pod per node, resources on every node |
| A periodic task | CronJob | a cluster scheduler, not accurate to the second |
| A periodic task in the app | `BackgroundService`/timer | simpler, but tied to a living process ([recipe](../../../csharp/recipes/background-jobs/overview.en.md)) |
| A task queue | a Job per item / worker Deployment | depends on load and ordering |

## Interview questions

<details><summary>1. How does a StatefulSet differ from a Deployment?</summary>

Stable pod names (`web-0`, `web-1`), its own PVC per replica (`volumeClaimTemplates`), stable DNS via a headless Service and ordered creation, deletion and update. A Deployment creates interchangeable pods with random names.

→ [Basic theory](basics.en.md#statefulset)

</details>

<details><summary>2. What happens to the data if you delete a StatefulSet pod?</summary>

The new pod gets the same name and the same PVC, the data is preserved (experiment: the file `custom.txt` remained); the IP and UID change.

→ [Advanced theory](advanced.en.md#statefulset-identity-survives-recreation)

</details>

<details><summary>3. What happens to PVCs after <code>scale</code> down and after deleting a StatefulSet?</summary>

By default `Retain`: PVCs remain in both cases. With `persistentVolumeClaimRetentionPolicy` set to `Delete` they are deleted on scale down (the highest number) and on deletion of the set.

→ [Advanced theory](advanced.en.md#pvc-policy)

</details>

<details><summary>4. Why does a StatefulSet need a headless Service?</summary>

So that each pod has its own DNS record (`web-1.web.<ns>.svc`) and a client can address a specific replica rather than a random one.

→ [Basic theory](basics.en.md#what-statefulset-gives-you)

</details>

<details><summary>5. What does <code>partition</code> do and in what order does the update go?</summary>

Only pods with a number not less than `partition` are updated, from the highest to the lowest. This is a canary: first `partition: 2`, then lower it to 0.

→ [Advanced theory](advanced.en.md#update-with-partition)

</details>

<details><summary>6. Trap: <code>rollout status</code> wrote "partitioned roll out complete". Is everything updated?</summary>

No: with `partition > 0` only part of the pods is updated (in the experiment one of three). Compare `currentRevision` and `updateRevision` and the pod images.

→ [Pitfall 1](#1-rollout-status-with-partition-says-complete)

</details>

<details><summary>7. Why did a DaemonSet start 2 pods on a 3-node cluster?</summary>

The control plane node has the taint `node-role.kubernetes.io/control-plane:NoSchedule`; without a toleration a pod does not land there. With a toleration there are 3 pods.

→ [Advanced theory](advanced.en.md#daemonset-nodes-tolerations-and-selector)

</details>

<details><summary>8. Can you set <code>replicas</code> on a DaemonSet?</summary>

No: there is no such field; the number of pods is determined by the number of suitable nodes (taking `nodeSelector` and tolerations into account).

→ [Reference](#field-reference)

</details>

<details><summary>9. Which <code>restartPolicy</code> values are valid in a Job and what happens with <code>Always</code>?</summary>

`Never` and `OnFailure`. With `Always` the object is rejected: `Required value: valid values: "OnFailure", "Never"`.

→ [Basic theory](basics.en.md#job)

</details>

<details><summary>10. How does <code>OnFailure</code> differ from <code>Never</code> on failure?</summary>

With `OnFailure` the kubelet restarts the container in the same pod (`RESTARTS` grows). With `Never` the pod stays in `Error` and the Job creates a new pod until `backoffLimit` is exhausted.

→ [Pitfall 5](#5-a-job-creates-pods-even-if-they-keep-failing)

</details>

<details><summary>11. What does <code>backoffLimit: 2</code> mean, how many pods will there be on permanent failure?</summary>

Three pods: the first attempt and two retries (in the experiment three `Error` pods, the conditions `FailureTarget` and `Failed` with the reason `BackoffLimitExceeded`).

→ [Advanced theory](advanced.en.md#backofflimit)

</details>

<details><summary>12. How does an <code>Indexed</code> Job work?</summary>

Each pod gets an index in `JOB_COMPLETION_INDEX` (and in the pod name), the status keeps `completedIndexes` (`0-5`). `completions: 6` and `parallelism: 2` give six runs in pairs.

→ [Basic theory](basics.en.md#several-runs-completions-and-parallelism)

</details>

<details><summary>13. How do you prevent finished Jobs from piling up?</summary>

`ttlSecondsAfterFinished` on a Job and `successfulJobsHistoryLimit`/`failedJobsHistoryLimit` on a CronJob.

→ [Advanced theory](advanced.en.md#automatic-cleanup-ttlsecondsafterfinished)

</details>

<details><summary>14. What does <code>concurrencyPolicy: Forbid</code> do?</summary>

While the previous run is going, a new one is skipped; the event `JobAlreadyActive … concurrency policy is Forbid`.

→ [Advanced theory](advanced.en.md#concurrencypolicy)

</details>

<details><summary>15. Does a CronJob guarantee exactly one run on time?</summary>

No: runs may be skipped, delayed or duplicated, so tasks must be idempotent.

→ [Beginner mistakes](basics.en.md#beginner-mistakes)

</details>

<details><summary>16. How do you run a CronJob manually?</summary>

`kubectl create job manual-run --from=cronjob/tick`: the Job is created from the template, the owner is the CronJob.

→ [Basic theory](basics.en.md#cronjob)

</details>

<details><summary>17. Trap: the schedule <code>every minute</code>. What happens?</summary>

Creation is rejected: `expected exactly 5 fields, found 2`. You need `* * * * *`.

→ [Basic theory](basics.en.md#cronjob)

</details>

<details><summary>18. How do <code>OrderedReady</code> and <code>Parallel</code> differ in practice?</summary>

With `OrderedReady`, while `stuck-0` is not ready the other pods are not created; with `Parallel` all three are created at once. The deletion order on `scale 0` is from the highest number (`web-2`, `web-1`, `web-0`).

→ [Advanced theory](advanced.en.md#podmanagementpolicy-ordered-or-parallel)

</details>

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Self-check: [quiz](quiz.en.md), [exercises](exercises.en.md)
- Architecture and watch: [02 · Architecture](../02-architecture/advanced.en.md)
- Storage: [08 · Storage](../08-storage/expert.en.md)

## Sources

- [StatefulSet basics](https://kubernetes.io/docs/tutorials/stateful-application/basic-stateful-set/)
- [StatefulSet API reference](https://kubernetes.io/docs/reference/kubernetes-api/workload-resources/stateful-set-v1/)
- [DaemonSet API reference](https://kubernetes.io/docs/reference/kubernetes-api/workload-resources/daemon-set-v1/)
- [Job API reference](https://kubernetes.io/docs/reference/kubernetes-api/workload-resources/job-v1/)
- [CronJob API reference](https://kubernetes.io/docs/reference/kubernetes-api/workload-resources/cron-job-v1/)
- [Pod failure policy](https://kubernetes.io/docs/concepts/workloads/controllers/job/#pod-failure-policy)
