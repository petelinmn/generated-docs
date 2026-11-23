---
title: "Pod"
lang: en
track: kubernetes
kind: module
doc: overview
slug: 04-pods
part: 1
levels: [1, 2, 3, 4]
tags: [pod, container, init-container, sidecar, lifecycle, restart-policy, termination, crashloopbackoff]
versions: "Kubernetes 1.37.0 (kind v0.34), kubectl 1.36.1"
status: draft
updated: 2026-11-22
---

# Pod

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

A Pod is the smallest unit that Kubernetes runs: one or more containers with a shared IP address, a shared `localhost` and shared volumes. A pod is ephemeral: it is not "repaired" but replaced. Pods are rarely created directly; usually a controller, such as a [Deployment](../05-deployments/overview.en.md), produces them. But understanding the pod lifecycle is mandatory: all diagnostic statuses such as `Pending`, `CrashLoopBackOff` and `Init:Error` depend on it.

## Key ideas

- **Containers of one pod live together:** they start on one node, share a network space (one IP, a common `localhost`) and can share volumes.
- **The pod phase** (`Pending`, `Running`, `Succeeded`, `Failed`, `Unknown`) is a coarse state; the `STATUS` column in `kubectl get` shows more detailed reasons (`ContainerCreating`, `Completed`, `Error`, `CrashLoopBackOff`).
- **`restartPolicy`** (`Always` by default, `OnFailure`, `Never`) decides whether the kubelet restarts a finished container.
- **Init containers** run one after another before the main containers; until they complete successfully the pod is not `Running`.
- **A sidecar container** works next to the main one (logging, a proxy); in modern versions it is an init container with `restartPolicy: Always`.
- **Stopping a pod:** `SIGTERM`, waiting for `terminationGracePeriodSeconds` (30 s by default), then `SIGKILL`.
- **Almost all pod fields are immutable:** the image is changed in place, everything else requires recreation.

## Minimal example

```yaml
apiVersion: v1
kind: Pod
metadata:
  name: web
  labels:
    app: web
spec:
  containers:
    - name: nginx
      image: nginx:1.29
      ports:
        - containerPort: 80
```

```text
$ kubectl -n learn-04 apply -f web.yaml
$ kubectl -n learn-04 get pod web
NAME   READY   STATUS    RESTARTS   AGE
web    1/1     Running   0          20s
```

## When to use and when not

- ✅ One-off tasks and debugging: `kubectl run`, pods with `restartPolicy: Never`.
- ✅ Several tightly coupled containers that must share network and volumes (an application + a sidecar).
- ❌ An application "just in a pod" without a controller: it will not come back after a node failure or deletion. Use a Deployment.
- ❌ Unrelated parts of an application in one pod: they cannot be scaled independently. Make separate pods.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | the pod manifest, phases and conditions, `restartPolicy`, `Completed`/`Error`/`CrashLoopBackOff`, logs, exec, events |
| [2. Advanced](advanced.en.md) | several containers, a shared `localhost` and volumes, variables from `fieldRef`, init containers, the native sidecar, pod termination and `preStop` |
| [3. Expert](expert.en.md) | pod internals (the pause container and namespaces), a reference of `spec` fields, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) with solutions |
| 5. Runnable examples | not generated yet — `/learn kubernetes 04-pods level 5` |
| 6. Tests | not generated yet — `/learn kubernetes 04-pods level 6` |

## Related materials

- Prerequisites: [03 · Local cluster and kubectl](../03-local-cluster-and-kubectl/overview.en.md), [02 · Kubernetes architecture](../02-architecture/overview.en.md)
- Next: [05 · Deployment and ReplicaSet](../05-deployments/overview.en.md)
- Foundation: the [Linux track](../../../linux/overview.en.md) — processes and signals
- Containers: [01 · Containers and Docker](../01-containers/overview.en.md) — `PID 1` and `SIGTERM`
- Pod restarts and completion in Jobs: [12 · Other workloads](../12-workloads/overview.en.md)
- Logs, events and metrics: [16 · Observability](../16-observability/overview.en.md)
- Pod and network security: [17 · Security](../17-security/overview.en.md)
- Pod placement on nodes: [18 · Pod scheduling](../18-scheduling/overview.en.md)
- Troubleshooting: [21 · Troubleshooting](../21-troubleshooting/overview.en.md)
