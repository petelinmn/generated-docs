---
title: "Troubleshooting: Pending, CrashLoopBackOff, network, nodes"
lang: en
track: kubernetes
kind: module
doc: overview
slug: 21-troubleshooting
part: 3
levels: [1, 2, 3, 4]
tags: [troubleshooting, debugging, crashloopbackoff, imagepullbackoff, oomkilled, pending, endpoints, dns, kubectl-debug, events, finalizer]
versions: "Kubernetes 1.37.0 (kind v0.34, 3 nodes), kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Troubleshooting: Pending, CrashLoopBackOff, network, nodes

🌐 [Русский](overview.ru.md) · **English**

## In short

When something does not work, one order of actions helps: **object status → events → logs → going inside**. The status (`kubectl get`) hints at the stage where a pod stopped (`Pending`, `ImagePullBackOff`, `CrashLoopBackOff`, `CreateContainerConfigError`, `OOMKilled`). Events (`kubectl describe`) name the cause. Logs show what the application did. If you need to look inside there are `kubectl exec`, the temporary container of `kubectl debug` and a debugging pod on a node. The network (a Service without endpoints, DNS) and the node state are checked separately.

## Key ideas

- **Read the status, then `describe`:** the cause is almost always in the `Events` section or in the container's `state`/`lastState`.
- **`Pending` is the scheduler or a volume, not the application:** no node fits (resources, taints, selectors) or a PVC is unbound.
- **`ImagePullBackOff` is the image, tag or registry;** `CreateContainerConfigError` — a missing ConfigMap or Secret; `RunContainerError` — the command does not start.
- **`CrashLoopBackOff` means the application crashes;** the exit code and logs say why; `137` and `OOMKilled` mean out of memory.
- **A Service without endpoints is a wrong selector, unready pods or a wrong port.**
- **`kubectl debug` adds a temporary container to a running pod** (even when the image has no shell) and opens the node's filesystem.
- **An object in `Terminating` is held by a finalizer;** a `Deployment` with `ReplicaFailure` — quota or admission.
- **A `NotReady` node gets taints,** pods on it do not leave at once but wait out `tolerationSeconds`.

## Minimal example

```text
$ kubectl -n learn-21 get pods
NAME     READY   STATUS                       RESTARTS      AGE
badimg   0/1     ImagePullBackOff             0             45s
nocfg    0/1     CreateContainerConfigError   0             45s
oom      0/1     OOMKilled                    2 (24s ago)   25s
$ kubectl -n learn-21 describe pod nocfg | tail -2
  Warning  Failed  12s (x4 over 37s)  kubelet  spec.containers{c}: Error: configmap "missing-cm" not found
```

## When to use and when not

| Symptom | Where to look |
|---|---|
| A pod does not start | `get pod`, `describe pod` (events) |
| A pod crashes | `logs`, `logs --previous`, `lastState` |
| A service is unreachable | `get endpoints`, DNS, `port-forward` |
| You need to "look inside" a container without a shell | `kubectl debug` |
| A node is not ready | `get nodes`, `describe node`, taints |
| An object will not delete | `deletionTimestamp`, `finalizers` |

- ❌ Do not restart a pod "for luck": you lose the state that the cause is searched from.
- ❌ Do not remove finalizers without understanding: external resources stay uncleaned.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | the diagnostic method, typical pod statuses and their causes |
| [2. Advanced theory](advanced.en.md) | Service and endpoints, DNS, `kubectl debug`, quota, `Terminating`, a `NotReady` node |
| [3. Expert theory](expert.en.md) | reference of statuses and commands, an algorithm, internals, pitfalls, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn kubernetes 21-troubleshooting level 5` |
| 6. Tests | not created yet — `/learn kubernetes 21-troubleshooting level 6` |

## Related materials

- Prerequisites: [04 · Pod](../04-pods/overview.en.md), [06 · Service and networking](../06-services/overview.en.md), [11 · Probes and resources](../11-probes-and-resources/overview.en.md)
- Logs, events, metrics: [16 · Observability](../16-observability/overview.en.md)
- Pod placement and `Pending`: [18 · Pod scheduling](../18-scheduling/overview.en.md)
- Storage and PVC: [08 · Storage](../08-storage/overview.en.md)
- Quotas and RBAC: [10 · Namespaces and RBAC](../10-namespaces-and-rbac/overview.en.md)
- Previous module: [20 · GitOps](../20-gitops/overview.en.md)
