---
title: "Quiz: other workloads"
lang: en
track: kubernetes
kind: module
doc: quiz
slug: 12-workloads
part: 2
tags: [statefulset, daemonset, job, cronjob, workload]
status: draft
updated: 2026-11-23
---

# Quiz: other workloads

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 Which controller suits a database with its own disk per replica?

- A) Deployment
- B) DaemonSet
- C) StatefulSet
- D) CronJob

<details><summary>Answer</summary>

**C.** StatefulSet gives stable names and a PVC per replica (`data-web-0`, `data-web-1`, …). A Deployment makes pods interchangeable, a DaemonSet — one pod per node.
→ [Basics](basics.en.md#statefulset)

</details>

## 2. 🟢 What are the pods of a three-replica StatefulSet `web` called?

- A) `web-<random suffix>`
- B) `web-0`, `web-1`, `web-2`
- C) `web-a`, `web-b`, `web-c`
- D) `web-1`, `web-2`, `web-3`

<details><summary>Answer</summary>

**B.** Numbers start at zero and are stable. A random suffix is what Deployment pods have.
→ [Basics](basics.en.md#what-statefulset-gives-you)

</details>

## 3. 🟢 A DaemonSet on a 3-node cluster (1 control plane with a taint + 2 workers), no toleration. How many pods?

- A) 3
- B) 2
- C) 1
- D) As many as `replicas`

<details><summary>Answer</summary>

**B.** The control plane has a taint, and without a toleration a pod does not land there. A DaemonSet has no `replicas` field.
→ [Basics](basics.en.md#daemonset)

</details>

## 4. 🟢 Which `restartPolicy` values are allowed in a Job template?

- A) `Always` and `Never`
- B) `OnFailure` and `Never`
- C) Only `Always`
- D) Any

<details><summary>Answer</summary>

**B.** For `Always` the API answers `Required value: valid values: "OnFailure", "Never"`.
→ [Basics](basics.en.md#job)

</details>

## 5. 🟡 You deleted the pod `web-1` of a StatefulSet. What happens?

- A) The pod does not come back
- B) `web-1` appears with the same PVC but a new IP and UID
- C) `web-3` appears with a new PVC
- D) The data in the volume is lost

<details><summary>Answer</summary>

**B.** In the experiment the file on the volume survived, the IP changed (`10.244.0.31` → `10.244.0.35`).
→ [Advanced](advanced.en.md#statefulset-identity-survives-recreation)

</details>

## 6. 🟡 `kubectl scale sts/web --replicas=1` with three replicas. What about the PVCs?

- A) `data-web-1` and `data-web-2` are deleted
- B) All three PVCs remain
- C) All three are deleted
- D) Only `data-web-0` remains

<details><summary>Answer</summary>

**B.** By default `whenScaled: Retain`. With `Delete` the PVCs of the removed replicas would go.
→ [Advanced](advanced.en.md#pvc-policy)

</details>

## 7. 🟡 Select all correct statements about `partition`

- [ ] A) With `partition: 2` the pod `web-2` is updated (in a set of three)
- [ ] B) With `partition: 2` `web-0` and `web-1` are updated
- [ ] C) Lowering `partition` to 0 updates the remaining pods
- [ ] D) `rollout status` with `partition > 0` honestly reports that one pod was updated

<details><summary>Answer</summary>

**A, C.** Pods with a number not less than `partition` are updated. D is wrong: it prints `partitioned roll out complete: 3 new pods have been updated`.
→ [Advanced](advanced.en.md#update-with-partition)

</details>

## 8. 🟡 A Job with `backoffLimit: 2` and the command `exit 1`. How many pods are created and what is the outcome?

- A) 1 pod, the Job is `Failed`
- B) 3 pods, the Job is `Failed` with the reason `BackoffLimitExceeded`
- C) 2 pods, the Job is `Complete`
- D) Infinitely many

<details><summary>Answer</summary>

**B.** The first attempt and two retries: in the experiment three `Error` pods.
→ [Advanced](advanced.en.md#backofflimit)

</details>

## 9. 🟡 A CronJob with `concurrencyPolicy: Forbid`, an every-minute schedule, the task lasts 100 seconds. What is in the events?

- A) A new Job is created every minute
- B) `JobAlreadyActive … concurrency policy is Forbid`: the run is skipped
- C) The old Job is killed
- D) The CronJob is deleted

<details><summary>Answer</summary>

**B.** The old run is killed by the `Replace` policy, not by `Forbid`.
→ [Advanced](advanced.en.md#concurrencypolicy)

</details>

## 10. 🔴 Find the flaw

> "We scaled the StatefulSet from 3 to 1, the disk was freed, and it is visible in the cluster cost immediately."

<details><summary>Answer</summary>

By default PVCs remain (`whenScaled: Retain`): the disks `data-web-1` and `data-web-2` keep existing and costing money. You can free them by deleting the PVCs or with `persistentVolumeClaimRetentionPolicy.whenScaled: Delete` — but then the data is gone.
→ [Pitfall 2](expert.en.md#2-scale-down-does-not-free-pvcs)

</details>

## 11. 🔴 What will the code print: what does `rollout status` show?

```text
$ kubectl -n learn-12 patch sts web -p '{"spec":{"updateStrategy":{"type":"RollingUpdate","rollingUpdate":{"partition":2}}}}'
$ kubectl -n learn-12 set image sts/web nginx=nginx:1.28
$ kubectl -n learn-12 rollout status sts/web
```

- A) Waits until all three pods are updated
- B) Reports `partitioned roll out complete` although one pod was updated
- C) An error: `partition` is not supported
- D) Updates `web-0` first

<details><summary>Answer</summary>

**B.** Only `web-2` was updated; to see the true state compare `currentRevision` and `updateRevision`.
→ [Pitfall 1](expert.en.md#1-rollout-status-with-partition-says-complete)

</details>

## 12. 🔴 Open question

Explain what guarantees a StatefulSet gives compared with a Deployment and what they cost.

<details><summary>Key points of a good answer</summary>

- A stable name (`web-N`), DNS via a headless Service, its own PVC per replica that survives pod recreation.
- Ordering: creation by increasing number, deletion by decreasing, update from the highest; `Parallel` turns the waiting off.
- Cost: `OrderedReady` is blocked by the first unready replica, updates are slower, PVCs are not freed by themselves (`Retain`), and Kubernetes does not do the application's replication and backups.
- Alternatives: an operator for a specific database, a managed database outside the cluster.

</details>

→ [Expert](expert.en.md#trade-offs-and-alternatives)
