---
title: "Other workloads: StatefulSet, DaemonSet, Job, CronJob"
lang: en
track: kubernetes
kind: module
doc: overview
slug: 12-workloads
part: 2
levels: [1, 2, 3, 4]
tags: [statefulset, daemonset, job, cronjob, workload, headless-service, volumeclaimtemplates, batch, backofflimit, concurrency-policy]
versions: "Kubernetes 1.37.0 (kind v0.34), kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Other workloads: StatefulSet, DaemonSet, Job, CronJob

🌐 [Русский](overview.ru.md) · **English**

## In short

[Deployment](../05-deployments/overview.en.md) suits stateless applications where all replicas are interchangeable. Other tasks have other controllers: **StatefulSet** — applications with state and stable names (databases), **DaemonSet** — one pod per node (agents), **Job** — a task that runs to successful completion, **CronJob** — a Job on a schedule. All of them create pods from the same template but decide differently how many pods to run, where and when.

## Key ideas

- **StatefulSet gives pods a stable identity:** names `web-0`, `web-1`, …, stable DNS names and its own PVC per replica that survives pod recreation.
- **StatefulSet pods are created in order** (`web-0`, then `web-1`) and deleted in reverse; the `Parallel` mode turns the waiting off.
- **StatefulSet PVCs are not deleted by default** on scale down and on deletion of the set; this is controlled by `persistentVolumeClaimRetentionPolicy`.
- **DaemonSet runs a pod on every suitable node,** including new ones; nodes with a taint (for example, control plane) need a toleration.
- **Job runs a task to success:** `completions`, `parallelism`, `backoffLimit`; pods are restarted only with `restartPolicy: Never` or `OnFailure`.
- **CronJob produces Jobs on a schedule (cron, 5 fields)** with a concurrency policy, pause and history.
- **Choosing a controller is choosing guarantees:** interchangeability, identity, "one per node", "once until done".

## Minimal example

```yaml
apiVersion: batch/v1
kind: Job
metadata: {name: simple}
spec:
  template:
    spec:
      restartPolicy: Never
      containers: [{name: c, image: alpine:3.22, command: ["sh","-c","echo job-done; exit 0"]}]
```

```text
$ kubectl -n learn-12 wait --for=condition=complete job/simple --timeout=90s
job.batch/simple condition met
$ kubectl -n learn-12 logs job/simple
job-done
```

## When to use and when not

| Task | Controller |
|---|---|
| Stateless service, interchangeable replicas | Deployment |
| Database, broker, cluster with replica identity | StatefulSet |
| Agent on every node (logs, metrics, network) | DaemonSet |
| One-off batch task, migration | Job |
| Periodic task (backup, report) | CronJob |

- ❌ Do not use a Deployment with one PVC for a database with replicas (module 08): you need a StatefulSet.
- ❌ Do not run "one-off" tasks as a Deployment or bare pods: they will be restarted on completion.
- ❌ CronJob does not guarantee second-level accuracy and exactly one run: tasks must be idempotent.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | StatefulSet and headless Service, DaemonSet, Job, CronJob: manifests and basic behavior |
| [2. Advanced theory](advanced.en.md) | data persistence and ordering in StatefulSet, update with `partition`, PVC policy, DaemonSet tolerations, Job retries and indexes, CronJob: concurrency, pause, history |
| [3. Expert theory](expert.en.md) | field reference of the four controllers, Job conditions, pitfalls, trade-offs, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) with solutions |
| 5. Runnable examples | not created yet — `/learn kubernetes 12-workloads level 5` |
| 6. Tests | not created yet — `/learn kubernetes 12-workloads level 6` |

## Related materials

- Prerequisites: [05 · Deployment and ReplicaSet](../05-deployments/overview.en.md), [08 · Storage](../08-storage/overview.en.md) (PVC), [06 · Service and networking](../06-services/overview.en.md) (headless Service)
- Next: [13 · Helm](../13-helm/overview.en.md)
- Stopping and restarting pods: [04 · Pod](../04-pods/overview.en.md)
- In other tracks: [background jobs in C#](../../../csharp/recipes/background-jobs/overview.en.md) — the same "periodic and one-off" outside Kubernetes
