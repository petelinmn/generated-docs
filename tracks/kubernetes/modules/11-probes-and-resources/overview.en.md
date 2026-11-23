---
title: "Probes and resources"
lang: en
track: kubernetes
kind: module
doc: overview
slug: 11-probes-and-resources
part: 2
levels: [1, 2, 3, 4]
tags: [probes, liveness-probe, readiness-probe, startup-probe, resources, requests, limits, qos, cgroups, cpu-throttling, oomkilled]
versions: "Kubernetes 1.37.0 (kind v0.34, cgroup v2), kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Probes and resources

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

Kubernetes does not know whether the application inside a container is healthy or how many resources it needs until you tell it. **Probes** are checks that the kubelet runs itself: is the application alive (liveness), is it ready to accept traffic (readiness), has startup finished (startup). **Requests and limits** are a resource claim and a ceiling: by the claim the scheduler picks a node, and the ceiling is enforced on that node by the Linux kernel (cgroups).

## Key ideas

- **Three probes — three different reactions.** A liveness failure — the kubelet restarts the container. A readiness failure — the pod is removed from the Service's endpoints but not restarted. Startup protects a slow start: until it passes, the others do not run.
- **Without readiness a container is "ready" right after it starts,** even if the application does not yet listen on the port.
- **Probe mechanisms:** `httpGet`, `tcpSocket`, `exec`, `grpc`. Defaults: `timeoutSeconds: 1`, `periodSeconds: 10`, `failureThreshold: 3`, `successThreshold: 1`.
- **Requests affect scheduling,** limits affect execution. The scheduler sums requests, not actual consumption.
- **CPU is compressible, memory is not.** Exceeding a CPU limit slows the container down (throttling), exceeding a memory limit kills it (`OOMKilled`).
- **The QoS class** (`Guaranteed`, `Burstable`, `BestEffort`) is computed from requests and limits and determines who is evicted first.
- **A limit below the request is an error:** the API does not accept such a pod.

## Minimal example

```yaml
apiVersion: v1
kind: Pod
metadata: {name: web}
spec:
  containers:
    - name: c
      image: nginx:1.29
      readinessProbe:
        httpGet: {path: /, port: 80}
      livenessProbe:
        httpGet: {path: /, port: 80}
        periodSeconds: 10
      resources:
        requests: {cpu: 100m, memory: 64Mi}
        limits: {memory: 128Mi}
```

```text
$ kubectl -n learn-11 get pod web
NAME   READY   STATUS    RESTARTS   AGE
web    1/1     Running   0          20s
```

## When to use and when not

- ✅ Readiness — for any application behind a Service: until it is ready, no traffic flows.
- ✅ Liveness — for applications that can hang but not crash (a deadlock).
- ✅ Startup — for slow-starting applications instead of a long `initialDelaySeconds`.
- ✅ Requests — always; memory limits — almost always.
- ❌ A liveness probe that depends on external services (a DB, a queue): a dependency failure would restart all pods at once.
- ❌ CPU limits "just in case" for latency-sensitive services: throttling gives latency spikes even on an idle node.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | liveness, readiness, mechanisms and parameters, requests and limits, QoS classes, `Insufficient cpu` |
| [2. Advanced](advanced.en.md) | the startup probe and a slow start, how to design probes, cgroups from the inside, CPU throttling, OOM, node capacity |
| [3. Expert](expert.en.md) | how the kubelet handles probes, QoS and `oom_score_adj`, a reference of fields, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) with solutions |
| 5. Runnable examples | not generated yet — `/learn kubernetes 11-probes-and-resources level 5` |
| 6. Tests | not generated yet — `/learn kubernetes 11-probes-and-resources level 6` |

## Related materials

- Prerequisites: [04 · Pod](../04-pods/overview.en.md), [06 · Service and networking](../06-services/overview.en.md) (endpoints and readiness)
- Next: [12 · Other workloads](../12-workloads/overview.en.md)
- Quotas and defaults: [10 · Namespaces and RBAC](../10-namespaces-and-rbac/overview.en.md)
- Rollouts and readiness: [05 · Deployment and ReplicaSet](../05-deployments/overview.en.md)
- The same idea in other tracks: [Linux](../../../linux/overview.en.md) — the cgroups limits are built from
- Autoscaling by requests: [15 · Autoscaling](../15-autoscaling/overview.en.md)
- Logs, events and metrics: [16 · Observability](../16-observability/overview.en.md)
- Pod placement on nodes: [18 · Pod scheduling](../18-scheduling/overview.en.md)
