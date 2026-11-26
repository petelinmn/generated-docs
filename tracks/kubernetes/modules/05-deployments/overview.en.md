---
title: "Deployment and ReplicaSet"
lang: en
track: kubernetes
kind: module
doc: overview
slug: 05-deployments
part: 1
levels: [1, 2, 3, 4]
tags: [deployment, replicaset, rolling-update, rollback, scaling, rollout, strategy, revision]
versions: "Kubernetes 1.37.0 (kind v0.34), kubectl 1.36.1"
status: draft
updated: 2026-11-22
---

# Deployment and ReplicaSet

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

A Deployment describes the desired state of an application: the image, the number of replicas, the update strategy. A ReplicaSet is the object that implements it: it makes sure exactly the required number of identical pods is running. You edit the Deployment, and the controller smoothly creates a new ReplicaSet and moves traffic to it (a rolling update), keeping history for rollback. This is the main way to run stateless applications.

## Key ideas

- **Deployment → ReplicaSet → Pod.** A Deployment manages ReplicaSets, a ReplicaSet manages pods; when the pod template changes, a new ReplicaSet is created.
- **Rolling update by default:** `maxSurge: 25%`, `maxUnavailable: 25%`; the update goes in small steps with no downtime.
- **Revision history.** Every old ReplicaSet is kept (10 revisions by default) and allows a rollback: `kubectl rollout undo`.
- **Only a change to the pod template triggers an update** (the image, variables, template labels); changing `replicas` recreates nothing.
- **A rollout can get stuck,** for example with a wrong image: the old pods keep working, and the Deployment waits `progressDeadlineSeconds` (600 s by default) and is then marked `ProgressDeadlineExceeded`.
- **`selector` is immutable** and must match the pod template labels.
- **The `Recreate` strategy** first stops all old pods, then starts new ones: downtime is unavoidable.

## Minimal example

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: web
spec:
  replicas: 3
  selector:
    matchLabels:
      app: web
  template:
    metadata:
      labels:
        app: web
    spec:
      containers:
        - name: nginx
          image: nginx:1.28
          ports:
            - containerPort: 80
```

```text
$ kubectl -n learn-05 apply -f web.yaml
$ kubectl -n learn-05 set image deploy/web nginx=nginx:1.29
$ kubectl -n learn-05 rollout status deploy/web
deployment "web" successfully rolled out
```

## When to use and when not

- ✅ Stateless applications: web servers, APIs, workers.
- ✅ You need smooth updates and a fast rollback.
- ❌ Applications with state and stable names and disks (databases): a StatefulSet fits them (module 12).
- ❌ One-off and periodic tasks: Job and CronJob (module 12).
- ❌ An agent on every node: DaemonSet (module 12).

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | the Deployment manifest, ReplicaSet and `pod-template-hash`, scaling, updating the image, `rollout status` and rollback |
| [2. Advanced](advanced.en.md) | how a rolling update goes step by step, `maxSurge` and `maxUnavailable`, `Recreate`, history and `change-cause`, a stuck rollout and `progressDeadlineSeconds`, pause/resume, `rollout restart` |
| [3. Expert](expert.en.md) | controller internals and the template hash, a reference of Deployment fields, conditions, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) with solutions |
| 5. Runnable examples | not generated yet — `/learn kubernetes 05-deployments level 5` |
| 6. Tests | not generated yet — `/learn kubernetes 05-deployments level 6` |

## Related materials

- Prerequisites: [04 · Pod](../04-pods/overview.en.md), [03 · kubectl](../03-local-cluster-and-kubectl/overview.en.md)
- Next: [06 · Service and networking](../06-services/overview.en.md)
- The reconciliation loop: [02 · Kubernetes architecture](../02-architecture/overview.en.md)
- The same idea in other tracks: [Background jobs in C#](../../../csharp/recipes/background-jobs/overview.en.md) — graceful shutdown during a rollout
- Other controllers: [12 · Other workloads](../12-workloads/overview.en.md) — StatefulSet, DaemonSet, Job, CronJob
- Templates and releases: [13 · Helm](../13-helm/overview.en.md), overlays: [14 · Kustomize](../14-kustomize/overview.en.md); autoscaling: [15 · Autoscaling](../15-autoscaling/overview.en.md)
- Logs, events and metrics: [16 · Observability](../16-observability/overview.en.md)
- Pod placement on nodes: [18 · Pod scheduling](../18-scheduling/overview.en.md)
- Troubleshooting: [21 · Troubleshooting](../21-troubleshooting/overview.en.md)
- [System design: architecture styles](../../../system-design/modules/11-architecture-styles/overview.en.md)
