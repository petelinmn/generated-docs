---
title: "Exercises: other workloads"
lang: en
track: kubernetes
kind: module
doc: exercises
slug: 12-workloads
part: 2
tags: [statefulset, daemonset, job, cronjob, workload]
status: draft
updated: 2026-11-23
---

# Exercises: other workloads

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself; hints and solutions are under spoilers. You need a kind cluster and a namespace: `kubectl create namespace learn-12`. Clean up afterwards: `kubectl delete namespace learn-12`. Exercise 3 needs a multi-node cluster.

## 1. 🟢 StatefulSet and a stable name

**Task:** create a headless Service `web` and a StatefulSet `web` of 3 replicas with `volumeClaimTemplates` of 100Mi. Write a file into the volume of `web-1`, delete the pod and make sure the file remained while the IP changed.

**Expected result:** three pods `web-0..2`, three PVCs `data-web-0..2`, the file in place after recreation.

<details><summary>Hint</summary>

`kubectl exec web-1 -- sh -c 'echo custom-data > /usr/share/nginx/html/custom.txt'`, then `kubectl delete pod web-1 --now`, then `kubectl exec web-1 -- cat …`.

</details>

<details><summary>Solution</summary>

The manifest is from [basic theory](basics.en.md#statefulset).

```text
$ kubectl -n learn-12 exec web-1 -- cat /usr/share/nginx/html/custom.txt
custom-data
ip before=10.244.0.31 after=10.244.0.35  uid changed=yes
```

The name and PVC are constant, the IP and UID are new.

</details>

## 2. 🟢 A Job with indexes

**Task:** create an Indexed Job with `completions: 6`, `parallelism: 2`, each pod prints `index=$JOB_COMPLETION_INDEX`. Show `completedIndexes`.

**Expected result:** `succeeded=6 completedIndexes=0-5`, no more than two pods run at once.

<details><summary>Hint</summary>

`completionMode: Indexed`, `restartPolicy: Never`, the command `echo index=$JOB_COMPLETION_INDEX`.

</details>

<details><summary>Solution</summary>

```yaml
apiVersion: batch/v1
kind: Job
metadata: {name: parallel}
spec:
  completions: 6
  parallelism: 2
  completionMode: Indexed
  template:
    spec:
      restartPolicy: Never
      containers: [{name: c, image: alpine:3.22, command: ["sh","-c","echo index=$JOB_COMPLETION_INDEX"]}]
```

```text
$ kubectl -n learn-12 get job parallel -o jsonpath='succeeded={.status.succeeded} completedIndexes={.status.completedIndexes}'
succeeded=6 completedIndexes=0-5
```

</details>

## 3. 🟡 DaemonSet, taint and nodeSelector

**Task:** on a 3-node cluster (1 control plane + 2 workers) run a DaemonSet. How many pods? Add a toleration and compare. Then restrict the set of nodes with the label `disk=ssd`.

**Expected result:** 2 pods without a toleration, 3 with a toleration, 1 with a `nodeSelector` on one labeled node.

<details><summary>Hint</summary>

Toleration: `{key: node-role.kubernetes.io/control-plane, operator: Exists, effect: NoSchedule}`. Label: `kubectl label node <node> disk=ssd`. Cluster: `kind create cluster --config` with one `control-plane` and two `worker` ([module 03](../03-local-cluster-and-kubectl/advanced.en.md#multi-node-kind)).

</details>

<details><summary>Solution</summary>

```text
agent   DESIRED 2   (no toleration)
agent2  DESIRED 3   (with toleration, a pod on the control-plane too)
$ kubectl label node learn-multi-worker disk=ssd
$ kubectl -n learn-12 patch ds agent -p '{"spec":{"template":{"spec":{"nodeSelector":{"disk":"ssd"}}}}}'
desired=1 current=1 ready=1
```

</details>

## 4. 🟡 PVC policy

**Task:** for the StatefulSet from exercise 1 enable `whenScaled: Delete` and `whenDeleted: Delete`. Scale down to 2 replicas, then delete the set. What about the PVCs?

**Expected result:** after the scale `data-web-2` is gone; after deleting the set there are no PVCs.

<details><summary>Hint</summary>

`kubectl patch sts web -p '{"spec":{"persistentVolumeClaimRetentionPolicy":{"whenDeleted":"Delete","whenScaled":"Delete"}}}'`.

</details>

<details><summary>Solution</summary>

```text
$ kubectl -n learn-12 scale sts/web --replicas=2
$ kubectl -n learn-12 get pvc --no-headers | awk '{print $1}'
data-web-0 data-web-1
$ kubectl -n learn-12 delete sts web
$ kubectl -n learn-12 get pvc
No resources found in learn-12 namespace.
```

</details>

## 5. 🔴 Job and CronJob retries without overlap

**Task:** (a) create a Job with the command `exit 1` and `backoffLimit: 2`; find the conditions and the number of pods. (b) Create an every-minute CronJob with `concurrencyPolicy: Forbid` and the task `sleep 100`, and find the reason for the skips in the events. (c) Try the schedule `every minute`.

**Expected result:** (a) 3 `Error` pods, conditions `FailureTarget`/`Failed` with the reason `BackoffLimitExceeded`; (b) the event `JobAlreadyActive`; (c) the error `expected exactly 5 fields, found 2`.

<details><summary>Hint</summary>

(a) `kubectl get job failing -o jsonpath='{range .status.conditions[*]}{.type}={.status} ({.reason}){"\n"}{end}'`. (b) `kubectl describe cronjob`, the Events section.

</details>

<details><summary>Solution</summary>

```text
failing-gt5sd 0/1 Error
failing-qwf2r 0/1 Error
failing-rtzsv 0/1 Error
FailureTarget=True (BackoffLimitExceeded)
Failed=True (BackoffLimitExceeded)
JobAlreadyActive   Not starting job because prior execution is running and concurrency policy is Forbid
The CronJob "badsched" is invalid: spec.schedule: Invalid value: "every minute": expected exactly 5 fields, found 2: [every minute]
```

Three pods — the first attempt and two retries; `Forbid` skips a run while the previous one is going.

</details>
