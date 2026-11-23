---
title: "Probes and resources: advanced"
lang: en
track: kubernetes
kind: module
doc: advanced
slug: 11-probes-and-resources
part: 2
tags: [startup-probe, probes, cgroups, cpu-throttling, oomkilled, cpu-limits, node-allocatable, overcommit]
versions: "Kubernetes 1.37.0 (kind v0.34, cgroup v2), kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Probes and resources: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on [basics](basics.en.md): the three kinds of probes, their defaults, requests, limits and QoS classes are not repeated.

## A slow start and the startup probe

An application that starts for 25 seconds (simulated: `sleep 25` before launching nginx) and a liveness probe with `periodSeconds: 3`, `failureThreshold: 2`. Two pods: without a startup probe and with it.

```yaml
# with a startup probe
startupProbe:
  httpGet: {path: /, port: 80}
  periodSeconds: 3
  failureThreshold: 15
livenessProbe:
  httpGet: {path: /, port: 80}
  periodSeconds: 3
  failureThreshold: 2
readinessProbe:
  httpGet: {path: /, port: 80}
  periodSeconds: 3
```

The state every 12 seconds:

```text
t~12s: slow-nostartup[1/1 Running r=0]  slow-startup[0/1 Running r=0]
t~24s: slow-nostartup[1/1 Running r=0]  slow-startup[0/1 Running r=0]
t~36s: slow-nostartup[1/1 Running r=0]  slow-startup[1/1 Running r=0]
t~48s: slow-nostartup[1/1 Running r=1]  slow-startup[1/1 Running r=0]
```

- **Without startup:** liveness fails with `connection refused` (the application does not listen yet), after two failures the kubelet restarts the container (`r=1`), and the cycle repeats: the application never manages to start. A pod without a readiness probe has `READY 1/1` from the first second although the port is closed.
- **With startup:** until it passes, liveness and readiness do not run; the reserve is 15 × 3 s = 45 s. Two failed attempts, then success, `started=true ready=true restarts=0`. The events: `Startup probe failed: … connection refused` (2 times).

```text
Warning  Unhealthy  6s (x4 over 45s)  kubelet  Liveness probe failed: Get "http://10.244.0.15:80/": dial tcp 10.244.0.15:80: connect: connection refused
```

Previously a large `initialDelaySeconds` was set on liveness for this, but it delays the check even for fast starts. The startup probe waits exactly as long as needed and protects against restarts.

## How to design probes

**Liveness** answers the question "is the application hopelessly hung?". It must be simple and independent of external systems: if liveness checks a database, then when the database fails Kubernetes restarts all pods at once, and they all become unavailable.

**Readiness** answers the question "can I serve a request right now?". It may take into account dependencies, cache warm-up, the request queue: a failure removes traffic but does not kill.

**Startup** is for a long start.

Practice:

- separate addresses for probes (`/livez` and `/readyz`), lightweight and without side effects;
- `timeoutSeconds` and `failureThreshold` with reserve: a brief load spike must not lead to a restart;
- an `exec` probe launches a process in the container on every check, which is more expensive than `httpGet` and `tcpSocket`;
- probes are run by the kubelet from the node: a probe to the pod address goes over the pod network (that is why a `tcpSocket` on a closed port gives `connection refused`).

Other mechanisms on closed ports:

```text
$ kubectl -n learn-11 get pod tcp-bad        # readinessProbe tcpSocket on port 9999
0/1 Running
  Warning  Unhealthy  kubelet  Readiness probe failed: dial tcp 10.244.0.14:9999: connect: connection refused
$ kubectl -n learn-11 describe pod grpc-probe  # livenessProbe grpc on port 9000 in a pod without gRPC
    Liveness:       grpc <pod>:9000  delay=0s timeout=1s period=3s #success=1 #failure=3
  Warning  Unhealthy  Liveness probe failed: timeout: failed to connect service "10.244.0.27:9000" within 1s: context deadline exceeded
```

A pod with a failed readiness stays `Running` indefinitely but gets no traffic. A gRPC probe expects the service `grpc.health.v1.Health`.

## Resources through cgroups

Requests and limits are turned into the container's cgroup v2 settings. The values from inside the pods (QoS `Guaranteed` — 100m/64Mi; `Burstable` — requests 50m/32Mi and a memory limit of 128Mi; `BestEffort` — no resources):

```text
-- guaranteed:  cpu.max=10000 100000   memory.max=67108864
-- burstable:   cpu.max=max 100000     memory.max=134217728
-- besteffort:  cpu.max=max 100000     memory.max=max
```

- `cpu.max` — the quota: `10000 100000` means "10,000 µs of CPU per period of 100,000 µs" = 10% of one core = the limit `100m`. Without a CPU limit — `max`.
- `memory.max` — the memory limit in bytes: 64 MiB = 67,108,864; without a limit — `max`.

The CPU claim (`requests.cpu`) turns into a **weight** `cpu.weight` for distribution under contention:

```text
guaranteed cpu.weight=17     # requests 100m
burstable  cpu.weight=11     # requests 50m
besteffort cpu.weight=1      # no requests
onlyreq    cpu.weight=17     # requests 100m
```

A claim does not reserve CPU: while the node has free cores, a container without a limit uses them as much as it wants; the weight works when cores are short. Memory, when its limit is exceeded, kills the container.

## CPU throttling

CPU is a "compressible" resource: when the limit is exceeded the container is not killed but slowed down. The experiment: a loop burning CPU for 20 seconds in two pods, one with a limit of `100m`, the other without a limit. The cgroup statistics after the loop:

```text
=== burn-limited (limit 100m)
usage_usec 2023496        # 2.0 s of CPU used in 20 s
nr_periods 202
nr_throttled 202          # throttled in every period
throttled_usec 8562744    # 8.6 s of waiting
=== burn-free (no limit)
usage_usec 20017330       # 20 s of CPU in 20 s — a whole core
nr_periods 0
nr_throttled 0
throttled_usec 0
```

The pod with a limit got 10% of a core (2.0 of 20 seconds) and was throttled in each of 202 periods although the node is almost idle: **a limit works independently of node load**. Hence the practice: latency-sensitive services get no CPU limit (only requests are left), or a limit with reserve and a throttling metric to watch.

> ⚠️ Verify: the recommendation "do not set a CPU limit" depends on cluster policy; in this material only the throttling effect was verified.

## Memory and OOMKilled

Memory is an "incompressible" resource: when the limit is exceeded the kernel kills the process (the OOM killer). A container with `limits.memory: 20Mi` and a greedy command ended with `reason=OOMKilled exit=137` ([module 04](../04-pods/expert.en.md#exit-codes)). Practice: the memory limit equals the claim or is close to it; watch real consumption and reserve.

## Node capacity

The scheduler compares the total requests of pods with the node's **allocatable**:

```text
$ kubectl get node learn-control-plane -o jsonpath='cpu={.status.allocatable.cpu} mem={.status.allocatable.memory} pods={.status.allocatable.pods}'
cpu=12 mem=32736456Ki pods=110
```

The node has 12 cores, about 31 GiB of memory and a limit of 110 pods. While the sum of claims does not exceed allocatable, new pods are scheduled; `describe node` shows the total requests and limits. The sums of limits may exceed capacity (`Total limits may be over 100 percent, i.e., overcommitted`): ceilings do not reserve resources, while claims do (for scheduling).

## Best practices

- Always set `requests`; for memory set `limits` equal or close to `requests`.
- For applications behind a Service a readiness probe is mandatory; liveness — only if the application can really hang.
- A slow start — a startup probe, not a big `initialDelaySeconds`.
- Design liveness without external dependencies, and keep probes lightweight.
- Apply LimitRange and ResourceQuota per namespace ([module 10](../10-namespaces-and-rbac/advanced.en.md#limitrange-defaults-and-bounds)).
- Measure actual consumption (metrics-server, Prometheus) and tune requests by the data.

## Common mistakes

- ❌ Liveness checks a database → ✅ external dependencies go in readiness.
- ❌ A big `initialDelaySeconds` on liveness instead of startup → ✅ a startup probe.
- ❌ Expecting `requests.cpu` to limit consumption → ✅ only `limits` limits; a claim defines the weight and scheduling.
- ❌ Setting a CPU limit below the real load → ✅ throttling and latency spikes even on an idle node.
- ❌ Considering `READY 1/1` a sign of a working application without a readiness probe → ✅ it only means "the container is started".

## Comparison with other tracks

- [Linux](../../../linux/overview.en.md): `cpu.max`, `cpu.weight` and `memory.max` are cgroup v2 files; Kubernetes only writes values into them.
- [C#](../../../csharp/modules/22-observability/overview.en.md): the `/livez` and `/readyz` health endpoints of an ASP.NET Core application correspond to liveness and readiness.
- [Docker](../01-containers/expert.en.md#reference): the `docker run --memory` and `--cpus` flags are the same cgroups.

## Related materials

- Previous level: [basics](basics.en.md)
- Next: [expert](expert.en.md)
- Rollouts and readiness: [05 · Deployment and ReplicaSet](../05-deployments/advanced.en.md#a-stuck-rollout), [06 · Service and networking](../06-services/advanced.en.md#pod-readiness-and-endpoints)

## Sources

- [Liveness, Readiness, and Startup Probes: startup probe](https://kubernetes.io/docs/concepts/configuration/liveness-readiness-startup-probes/#startup-probe)
- [Resource Management: how Pods with resource limits are run](https://kubernetes.io/docs/concepts/configuration/manage-resources-containers/#how-pods-with-resource-limits-are-run)
- [Node Allocatable](https://kubernetes.io/docs/tasks/administer-cluster/reserve-compute-resources/)
- [Linux kernel: cgroup v2 CPU controller](https://docs.kernel.org/admin-guide/cgroup-v2.html#cpu)
- [Configure Quality of Service for Pods](https://kubernetes.io/docs/tasks/configure-pod-container/quality-service-pod/)
