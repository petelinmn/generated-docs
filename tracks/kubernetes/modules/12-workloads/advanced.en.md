---
title: "Other workloads: advanced theory"
lang: en
track: kubernetes
kind: module
doc: advanced
slug: 12-workloads
part: 2
tags: [statefulset, daemonset, job, cronjob, partition, podmanagementpolicy, pvc-retention, toleration, backofflimit, concurrency-policy, ttl]
versions: "Kubernetes 1.37.0 (kind v0.34), kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Other workloads: advanced theory

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on [basic theory](basics.en.md): the manifests of the four controllers and their basic behavior are not repeated.

## StatefulSet: identity survives recreation

Write a file into the volume of replica `web-1` and delete the pod:

```text
$ kubectl -n learn-12 exec web-1 -- sh -c 'echo custom-data > /usr/share/nginx/html/custom.txt'
$ kubectl -n learn-12 delete pod web-1 --now
ip before=10.244.0.31 after=10.244.0.35  uid changed=yes
$ kubectl -n learn-12 exec web-1 -- cat /usr/share/nginx/html/custom.txt
custom-data
$ kubectl -n learn-12 get pvc data-web-1
data-web-1 Bound
```

The pod got the same name `web-1` and the same PVC (`data-web-1`) with the data, but the IP and UID changed. So address replicas by name (`web-1.web`), not by IP ([module 06](../06-services/basics.en.md#stable-address)).

### Scaling

```text
$ kubectl -n learn-12 scale sts/web --replicas=1
$ kubectl -n learn-12 get pods,pvc --no-headers | awk '{print $1,$2}'
web-0 1/1
data-web-0 Bound
data-web-1 Bound
data-web-2 Bound
$ kubectl -n learn-12 scale sts/web --replicas=3
$ kubectl -n learn-12 exec web-1 -- cat /usr/share/nginx/html/custom.txt
custom-data
```

On scale down the pods `web-2` and `web-1` are deleted (highest number first), but their PVCs remain. On scale up `web-1` gets its PVC again, and the data is in place. On deletion in reverse order:

```text
web-2 Terminating
web-1 Terminating
web-0 Terminating
```

### PVC policy

What happens to PVCs on scale down and on deletion of the set is defined by `persistentVolumeClaimRetentionPolicy`:

```text
$ kubectl -n learn-12 get sts web -o jsonpath='{.spec.persistentVolumeClaimRetentionPolicy}'
{"whenDeleted":"Retain","whenScaled":"Retain"}
```

By default both events are `Retain`: after `delete sts web` the PVCs (`data-web-0`, `data-web-1`, `data-web-2`) remained. With `whenDeleted: Delete` and `whenScaled: Delete`:

```text
$ kubectl -n learn-12 patch sts web -p '{"spec":{"persistentVolumeClaimRetentionPolicy":{"whenDeleted":"Delete","whenScaled":"Delete"}}}'
$ kubectl -n learn-12 scale sts/web --replicas=2
$ kubectl -n learn-12 get pvc --no-headers | awk '{print $1}'
data-web-0 data-web-1
$ kubectl -n learn-12 delete sts web
$ kubectl -n learn-12 get pvc
No resources found in learn-12 namespace.
```

On scale down `data-web-2` disappeared, on deletion of the set all the others. Choose `Delete` only deliberately: the data will be gone.

### podManagementPolicy: ordered or parallel

The default is `OrderedReady`: the next pod is created only after the previous one is ready. If `web-0` does not become ready, the others are not created. Experiment: two sets of 3 replicas with a readiness probe that never passes:

```text
OrderedReady:  stuck-0 0/1 Running
Parallel:      par-0 0/1 Running   par-1 0/1 Running   par-2 0/1 Running
```

`OrderedReady` stopped at `stuck-0`, while `Parallel` (`podManagementPolicy: Parallel`) created all three at once. The parallel mode speeds up the start if the application does not care about the order.

### Update with partition

`updateStrategy.rollingUpdate.partition` says: only pods with a number **not less than** `partition` are updated. This is a canary and a staged rollout. Change the image with `partition: 2`:

```text
$ kubectl -n learn-12 patch sts web -p '{"spec":{"updateStrategy":{"type":"RollingUpdate","rollingUpdate":{"partition":2}}}}'
$ kubectl -n learn-12 set image sts/web nginx=nginx:1.28
web-0 image: nginx:1.29
web-1 image: nginx:1.29
web-2 image: nginx:1.28
updated=1 current=2 revisions: cur=web-796ff6cbd7 upd=web-84b47bb8f8
$ kubectl -n learn-12 patch sts web -p '{"spec":{"updateStrategy":{"type":"RollingUpdate","rollingUpdate":{"partition":0}}}}'
web-0: nginx:1.28
web-1: nginx:1.28
web-2: nginx:1.28
```

Only `web-2` was updated; once it is checked, `partition` is lowered to 0 and the update goes in descending order of numbers: 2, 1, 0. The status shows two revisions: `currentRevision` and `updateRevision`. The `kubectl rollout status` message with `partition > 0` is misleading: it already wrote `partitioned roll out complete: 3 new pods have been updated` although only one pod had been updated.

Defaults filled in by the API: `podManagementPolicy: OrderedReady`, `updateStrategy: {rollingUpdate: {maxUnavailable: 1, partition: 0}, type: RollingUpdate}`.

> ⚠️ Verify: `serviceName` is not required in this version: a StatefulSet without it created the pod `nosvc-0`, but without a headless Service there are no stable DNS names; the behavior of `updateStrategy.type: OnDelete` and `maxUnavailable` was not tested.

## DaemonSet: nodes, tolerations and selector

The same DaemonSet with a toleration for the control plane taint:

```yaml
spec:
  template:
    spec:
      tolerations:
        - {key: node-role.kubernetes.io/control-plane, operator: Exists, effect: NoSchedule}
```

```text
$ kubectl -n learn-12 get ds agent2
NAME     DESIRED   CURRENT   READY   UP-TO-DATE   AVAILABLE
agent2   3         3         3       3            3
agent2-4hngv Running learn-multi-worker2
agent2-q8vwh Running learn-multi-worker
agent2-qs2bd Running learn-multi-control-plane
```

With the toleration a pod appeared on the control plane node too (3 of 3).

A deleted pod comes back on the same node:

```text
$ kubectl -n learn-12 delete pod agent-29n99 --now     # was on learn-multi-worker2
agent-56hbr Running learn-multi-worker
agent-5kbq8 Running learn-multi-worker2
```

You can restrict a DaemonSet to a subset of nodes with `nodeSelector`:

```text
$ kubectl label node learn-multi-worker disk=ssd
$ kubectl -n learn-12 patch ds agent -p '{"spec":{"template":{"spec":{"nodeSelector":{"disk":"ssd"}}}}}'
agent-56hbr Running learn-multi-worker
agent-5kbq8 Terminating learn-multi-worker2
desired=1 current=1 ready=1
```

The pod stayed only on the labeled node (module 18 covers `nodeSelector`, affinity and taints). A DaemonSet's default update is `RollingUpdate` with `maxUnavailable: 1`:

```text
{"rollingUpdate":{"maxSurge":0,"maxUnavailable":1},"type":"RollingUpdate"}
```

A new node gets a pod automatically.

> ⚠️ Verify: the automatic appearance of a pod on an added node and the `OnDelete` strategy of a DaemonSet were not run.

## Job: retries, indexes, cleanup

### backoffLimit

On failures a Job creates new pods up to `backoffLimit` retries (6 by default), then is marked failed:

```text
$ kubectl -n learn-12 get job failing        # backoffLimit: 2, command exit 1
NAME      STATUS   COMPLETIONS   DURATION   AGE
failing   Failed   0/1           69s        69s
$ kubectl -n learn-12 get pods -l job-name=failing
failing-gt5sd 0/1 Error
failing-qwf2r 0/1 Error
failing-rtzsv 0/1 Error
$ kubectl -n learn-12 get job failing -o jsonpath='{range .status.conditions[*]}{.type}={.status} ({.reason}): {.message}{"\n"}{end}'
FailureTarget=True (BackoffLimitExceeded): Job has reached the specified backoff limit
Failed=True (BackoffLimitExceeded): Job has reached the specified backoff limit
```

Three pods: the first attempt and two retries (`backoffLimit: 2`), with a growing delay between attempts (all three took 69 seconds). The conditions `FailureTarget` and `Failed` with the reason `BackoffLimitExceeded`.

### Parallelism

In the Job `parallel` (`completions: 6`, `parallelism: 2`) observation every two seconds showed pairs:

```text
t=3  running=0 completed=2
t=5  running=2 completed=2
t=7  running=0 completed=4
t=9  running=2 completed=4
t=11 running=0 completed=6
$ kubectl -n learn-12 get job parallel -o jsonpath='succeeded={.status.succeeded} completedIndexes={.status.completedIndexes}'
succeeded=6 completedIndexes=0-5
```

No more than two pods ran at once; six were completed in total.

### Automatic cleanup: ttlSecondsAfterFinished

Finished Jobs and their pods stay until they are deleted. The `ttlSecondsAfterFinished` field deletes the Job automatically:

```text
$ kubectl -n learn-12 get job ttl          # ttlSecondsAfterFinished: 15, after ~90 s
Error from server (NotFound): jobs.batch "ttl" not found
```

### Other Job fields

`activeDeadlineSeconds` — the maximum time of the whole Job; `suspend` — pause; `backoffLimitPerIndex`, `podFailurePolicy`, `successPolicy` — fine-tuning of retries and success (see [expert theory](expert.en.md#field-reference)).

## CronJob: concurrency, pause, history

### concurrencyPolicy

What to do if the previous run is still going:

| Policy | Behavior |
|---|---|
| `Allow` (default) | run in parallel |
| `Forbid` | skip the new run |
| `Replace` | stop the old one and start a new one |

With `Forbid` and a 100-second task on a every-minute schedule there is one Job, and an event explains the skips:

```text
JobAlreadyActive   Not starting job because prior execution is running and concurrency policy is Forbid
```

Defaults in our cluster: `concurrencyPolicy=Allow`, `suspend=false`, `startingDeadlineSeconds` and `timeZone` not set (`<none>`).

### History and pause

`successfulJobsHistoryLimit` and `failedJobsHistoryLimit` limit the number of kept finished Jobs. With `successfulJobsHistoryLimit: 2` after several runs there were two finished and one running (`tick-…831`, `…832` — `Complete`, `…833` — `Running`). Pause:

```text
$ kubectl -n learn-12 patch cronjob tick -p '{"spec":{"suspend":true}}'
jobs before=3 after=2
```

With `suspend: true` no new Jobs are created (the number of Jobs did not grow in a minute).

> ⚠️ Verify: the exact meaning of `startingDeadlineSeconds`, the behavior of `Replace` and the work of `timeZone` were not run in this material; by default the schedule is computed in the controller's time (in the experiment `lastScheduleTime` in UTC: `2026-11-23T10:33:00Z`).

## Best practices

- For a StatefulSet always create a headless Service; address replicas by name.
- Decide the fate of PVCs in advance (`Retain` or `Delete`) and make backups.
- For a DaemonSet set tolerations for the needed taints and resources (an agent must not eat the node, [module 11](../11-probes-and-resources/overview.en.md)).
- Make Jobs and CronJobs idempotent: retries and skips are inevitable.
- Set `ttlSecondsAfterFinished`, `activeDeadlineSeconds` and CronJob history limits so objects do not pile up.
- Choose `concurrencyPolicy` for a CronJob deliberately; for long tasks use `Forbid`.

## Common mistakes

- ❌ Addressing a StatefulSet replica by IP → ✅ by name: the IP changes on recreation (in the experiment `.31` → `.35`).
- ❌ Expecting `scale` down to delete the data → ✅ PVCs remain (`Retain`).
- ❌ Judging update completion by `rollout status` with `partition > 0` → ✅ look at `updateRevision` and pod images.
- ❌ Leaving a Job without `backoffLimit` and without cleanup → ✅ limit retries and enable TTL.
- ❌ A CronJob schedule like "every minute" → ✅ exactly 5 cron fields.

## Comparison with other tracks

- [Linux](../../../linux/overview.en.md): a CronJob is cron at the cluster level (the same 5 fields), a DaemonSet is the analog of a systemd service on every host.
- [C#](../../../csharp/recipes/background-jobs/overview.en.md): a `BackgroundService` with `PeriodicTimer` inside the application and a CronJob outside solve different tasks: in the first case the process lives permanently, in the second it is started for the duration of the work.

## Related materials

- Previous level: [basic theory](basics.en.md)
- Next: [expert theory](expert.en.md)
- Storage: [08 · Storage](../08-storage/advanced.en.md#reclaim-policy-reclaimpolicy)
- Rollout and rollback: [05 · Deployment and ReplicaSet](../05-deployments/advanced.en.md#rolling-update-step-by-step)

## Sources

- [StatefulSet: partitioned rolling updates](https://kubernetes.io/docs/concepts/workloads/controllers/statefulset/#partitions)
- [StatefulSet: PersistentVolumeClaim retention](https://kubernetes.io/docs/concepts/workloads/controllers/statefulset/#persistentvolumeclaim-retention)
- [StatefulSet: pod management policies](https://kubernetes.io/docs/concepts/workloads/controllers/statefulset/#pod-management-policies)
- [DaemonSet: how daemon pods are scheduled](https://kubernetes.io/docs/concepts/workloads/controllers/daemonset/#how-daemon-pods-are-scheduled)
- [Jobs: handling pod and container failures](https://kubernetes.io/docs/concepts/workloads/controllers/job/#pod-backoff-failure-policy)
- [Automatic cleanup for finished Jobs](https://kubernetes.io/docs/concepts/workloads/controllers/job/#ttl-mechanism-for-finished-jobs)
- [CronJob: concurrency policy](https://kubernetes.io/docs/concepts/workloads/controllers/cron-jobs/#concurrency-policy)
