---
title: "Kubernetes architecture"
lang: en
track: kubernetes
kind: module
doc: overview
slug: 02-architecture
part: 1
levels: [1, 2, 3, 4]
tags: [kubernetes, architecture, control-plane, api-server, etcd, scheduler, kubelet, controller, reconciliation]
versions: "Kubernetes 1.37.0 (kind v0.34, containerd 2.3.4), kubectl 1.36.1"
status: draft
updated: 2026-11-22
---

# Kubernetes architecture

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

A Kubernetes cluster consists of a **control plane** (the managing part that makes decisions) and **nodes** (worker machines on which containers run). You describe the desired state of objects (Deployment, Service and others), the API server writes it to etcd, and controllers and the kubelet continuously bring the actual state to the desired one. Understanding this scheme explains why a deleted pod appears again, where events come from, and what breaks when a given component goes down.

## Key ideas

- **The API server (`kube-apiserver`) is the only entry point.** kubectl, controllers, the scheduler and kubelets all talk through it; components do not talk to each other directly.
- **etcd is the source of truth.** It is a distributed key-value store holding the whole cluster state; only the API server talks to it.
- **Controllers work in a reconciliation loop:** they compare the desired state (`spec`) with the actual one (`status`) and fix the difference. That is why a pod deleted from a Deployment is created again.
- **The scheduler only picks a node** for a new pod and writes the choice into the object; it does not start the pod.
- **The kubelet on a node** watches the pods assigned to its node and starts containers through a runtime (containerd) over the CRI interface.
- **Objects are linked by owners:** Deployment → ReplicaSet → Pod (the `ownerReferences` field).
- **Conflicts are resolved by optimistic locking:** a stale version of an object is rejected with a `Conflict` error.

## Minimal example

We create a Deployment with 2 replicas and look at the events: you can see who did what.

```text
$ kubectl -n learn-02 create deployment web --image=nginx:1.29 --replicas=2
$ kubectl -n learn-02 get events -o custom-columns=REASON:.reason,SOURCE:.source.component,MSG:.message
REASON              SOURCE                  MSG
Scheduled           default-scheduler       Successfully assigned learn-02/web-689b9bdbdd-24sgv to learn-control-plane
Pulling             kubelet                 Pulling image "nginx:1.29"
ScalingReplicaSet   deployment-controller   Scaled up replica set web-689b9bdbdd from 0 to 2
SuccessfulCreate    replicaset-controller   Created pod: web-689b9bdbdd-24sgv
```

Here `deployment-controller` created a ReplicaSet, `replicaset-controller` created the pods, `default-scheduler` chose a node, and the `kubelet` pulled the image and started the container. The events are not listed in order; details are in the [expert theory](expert.en.md#gotcha-event-order-is-imprecise).

## When to use and when not

- ✅ Knowing the architecture is always useful: it helps diagnose why a pod did not start ("Pending" is the scheduler, "ImagePull" is the kubelet).
- ✅ It helps set the right expectations: changes are applied asynchronously, not instantly.
- ❌ You do not need to start control plane components by hand: in kind and managed cloud clusters they are already deployed.
- ❌ Do not work with etcd directly, bypassing the API: the storage layout is not a public contract.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | control plane and node components, ports, what is visible in kind, the event chain when a Deployment is created |
| [2. Advanced](advanced.en.md) | the reconciliation loop, watch, object owners, optimistic locking, the request path through the API server, API groups, leader election |
| [3. Expert](expert.en.md) | storage in etcd, a reference of endpoints and metadata fields, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) with solutions |
| 5. Runnable examples | not generated yet — `/learn kubernetes 02-architecture level 5` |
| 6. Tests | not generated yet — `/learn kubernetes 02-architecture level 6` |

## Related materials

- Prerequisites: [01 · Containers and Docker](../01-containers/overview.en.md) — the containerd runtime and images
- Next: [03 · Local cluster and kubectl](../03-local-cluster-and-kubectl/overview.en.md)
- Foundation: the [Linux track](../../../linux/overview.en.md) — processes, network, cgroups
