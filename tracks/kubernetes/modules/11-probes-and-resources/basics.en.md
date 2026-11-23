---
title: "Probes and resources: basics"
lang: en
track: kubernetes
kind: module
doc: basics
slug: 11-probes-and-resources
part: 2
tags: [probes, liveness-probe, readiness-probe, resources, requests, limits, qos]
versions: "Kubernetes 1.37.0 (kind v0.34, cgroup v2), kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Probes and resources: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [04 · Pod](../04-pods/overview.en.md) · **Time:** ~3 h

All examples were run in kind (Kubernetes 1.37.0), namespace `learn-11`.

## Why it matters

A container can be "running" but not working: the application hung, has not warmed up yet, or lost its connection to a database. Without hints Kubernetes sees only that the process is alive. Probes let the kubelet check the application itself. The second question is resources: without a claim the scheduler does not know how much room a pod needs, and without a ceiling one pod can eat the whole node.

## Probes

### Three kinds

| Probe | Question | Reaction on failure |
|---|---|---|
| `livenessProbe` | is the application alive? | the kubelet **restarts** the container |
| `readinessProbe` | is the application ready to accept traffic? | the pod is removed from the Service endpoints, **without a restart** |
| `startupProbe` | has the application finished starting? | until it passes, liveness and readiness are not run; on failure — a restart |

### Mechanisms

| Mechanism | How it checks | Success |
|---|---|---|
| `httpGet` | an HTTP GET to the pod address | code 200–399 |
| `tcpSocket` | an attempt to open a TCP connection | the port accepts a connection |
| `exec` | a command in the container | exit code 0 |
| `grpc` | a gRPC health request | status `SERVING` |

> ⚠️ Verify: the code range 200–399 for `httpGet` and the status `SERVING` for `grpc` are given from documentation; by running, the failure of `httpGet` (code 404), `tcpSocket`, `exec` and the failure of `grpc` on a closed port were verified (more in [advanced](advanced.en.md#how-to-design-probes)).

### Parameters and defaults

The values Kubernetes filled in itself (from `kubectl describe pod`):

```text
Liveness:   tcp-socket :80 delay=0s timeout=1s period=10s #success=1 #failure=3
Readiness:  http-get http://:80/ delay=0s timeout=1s period=10s #success=1 #failure=3
```

| Field | Meaning | Default |
|---|---|---|
| `initialDelaySeconds` | the delay before the first check | 0 |
| `periodSeconds` | the interval between checks | 10 |
| `timeoutSeconds` | how long to wait for a response | 1 |
| `successThreshold` | consecutive successes for "success" | 1 |
| `failureThreshold` | consecutive failures for "failure" | 3 |

## Liveness: restarting a hung one

A pod whose liveness probe goes to the non-existent path `/healthz` (nginx answers `404`):

```yaml
apiVersion: v1
kind: Pod
metadata: {name: live-http}
spec:
  containers:
    - name: c
      image: nginx:1.29
      livenessProbe:
        httpGet: {path: /healthz, port: 80}
        periodSeconds: 3
        failureThreshold: 2
```

```text
$ kubectl -n learn-11 get pod live-http
NAME        READY   STATUS    RESTARTS     AGE
live-http   1/1     Running   2 (1s ago)   25s
$ kubectl -n learn-11 describe pod live-http
    Last State:     Terminated
      Reason:       Completed
      Exit Code:    0
    Restart Count:  2
    Liveness:       http-get http://:80/healthz delay=0s timeout=1s period=3s #success=1 #failure=2
  Warning  Unhealthy  1s (x4 over 10s)  kubelet  Liveness probe failed: HTTP probe failed with statuscode: 404
  Normal   Killing    1s (x2 over 7s)   kubelet  Container c failed liveness probe, will be restarted
```

After two failures in a row (`#failure=2`) the kubelet killed the container (`Killing … will be restarted`), the `RESTARTS` counter grew. The container ended with code `0` and the reason `Completed`: nginx handled `SIGTERM` gracefully ([module 04](../04-pods/advanced.en.md#pod-termination)). This is not an application crash but a kubelet decision.

## Readiness: ready to accept traffic

An `exec` probe reads the file `/tmp/ready`; while the file exists, the pod is ready:

```yaml
spec:
  containers:
    - name: c
      image: nginx:1.29
      command: ["sh", "-c", "touch /tmp/ready; exec nginx -g 'daemon off;'"]
      readinessProbe:
        exec: {command: ["cat", "/tmp/ready"]}
        periodSeconds: 2
```

```text
$ kubectl -n learn-11 get pod ready-flip
ready-flip: 1/1
$ kubectl -n learn-11 exec ready-flip -- rm /tmp/ready
$ kubectl -n learn-11 get pod ready-flip          # after ~6 s
after rm: 0/1 Running
$ kubectl -n learn-11 get pod ready-flip -o jsonpath='{range .status.conditions[*]}{.type}={.status} {end}'
PodReadyToStartContainers=True Initialized=True Ready=False ContainersReady=False PodScheduled=True
$ kubectl -n learn-11 exec ready-flip -- touch /tmp/ready
after touch: 1/1 Running
```

The pod stays `Running` but `Ready=False`: the kubelet did not restart the container, and the Service stopped sending traffic to it (the address disappears from the EndpointSlice, [module 06](../06-services/advanced.en.md#pod-readiness-and-endpoints)). When the probe passes again, the pod returns to the Service by itself.

Readiness also works during a rollout: a [Deployment](../05-deployments/advanced.en.md#a-stuck-rollout) considers a new pod ready to replace an old one only after a successful readiness.

## Resources: requests and limits

```yaml
resources:
  requests: {cpu: 100m, memory: 64Mi}
  limits: {cpu: 200m, memory: 128Mi}
```

- **`requests`** — how many resources are guaranteed to be needed: the scheduler looks for a node with at least that free.
- **`limits`** — the ceiling: a container cannot exceed it.
- **Units:** CPU — cores (`1`) or millicores (`100m` = 0.1 core); memory — bytes with the suffixes `Mi`, `Gi` (`64Mi`).

### Scheduling goes by requests

A pod asking for more than the node has is not scheduled:

```text
$ kubectl -n learn-11 get pod huge
0/1 Pending
$ kubectl -n learn-11 describe pod huge
  Warning  FailedScheduling  default-scheduler  0/1 nodes are available: 1 Insufficient cpu. preemption: 0/1 nodes are available: 1 Preemption is not helpful for scheduling.
```

This is a pod with `requests.cpu: "100"` on a node of 12 cores. The event `Insufficient cpu` is the most frequent cause of `Pending` ([module 04](../04-pods/basics.en.md#phases-and-statuses)). How many resources are already taken by claims is visible in `kubectl describe node`:

```text
Allocated resources:
  Resource           Requests     Limits
  cpu                1200m (10%)  100m (0%)
  memory             450Mi (1%)   532Mi (1%)
```

These are the sums of the claims and ceilings of all pods on the node, not actual consumption.

### A limit cannot be below the request

```text
The Pod "badlimits" is invalid: spec.containers[0].resources.requests: Invalid value: "500m": must be less than or equal to cpu limit of 100m
```

## QoS classes

The class is computed from the set requests and limits:

| Class | Condition | Example |
|---|---|---|
| `Guaranteed` | every container has both CPU and memory with requests = limits | `requests` = `limits` = `100m`/`64Mi` |
| `Burstable` | something is set but not as for `Guaranteed` | a limit only on memory, or only requests |
| `BestEffort` | nothing is set | a pod without resources |

```text
$ for p in guaranteed burstable besteffort onlyreq; do printf "%s -> " $p; kubectl -n learn-11 get pod $p -o jsonpath='{.status.qosClass}{"\n"}'; done
guaranteed -> Guaranteed       # requests = limits for CPU and memory
burstable -> Burstable         # requests 50m/32Mi, a limit only on memory 128Mi
besteffort -> BestEffort       # without resources
onlyreq -> Burstable           # only requests
```

QoS matters when a node runs short of resources: `BestEffort` is evicted first, then `Burstable`, `Guaranteed` last (more in the [expert theory](expert.en.md#qos-and-oom_score_adj)).

## Actual consumption: kubectl top

`kubectl top` shows real consumption, but it needs metrics-server. Our kind does not have it:

```text
$ kubectl top pods -n learn-11
error: Metrics API not available
```

Installing metrics-server and monitoring are in modules 15 and 16.

## Beginner mistakes

- ❌ Not setting readiness and considering a pod ready at once → ✅ without readiness a container is `Ready` right after start, even if the application does not yet listen on the port.
- ❌ Making liveness the same as readiness with a short timeout → ✅ liveness must be softer: an extra restart is worse than an extra delay.
- ❌ Not setting requests → ✅ without a claim the scheduler does not know how much is needed, and the pod gets the `BestEffort` class.
- ❌ Confusing requests with actual consumption → ✅ the scheduler and `describe node` count claims, not load.
- ❌ Setting `limits` below `requests` → ✅ such a pod is rejected.

## Related materials

- Next: [advanced](advanced.en.md) — the startup probe, cgroups, throttling
- Next module: module 12 · Other workloads (in the roadmap of the [track](../../overview.en.md))
- Previous modules: [06 · Service and networking](../06-services/overview.en.md), [04 · Pod](../04-pods/overview.en.md)
- Quotas and LimitRange: [10 · Namespaces and RBAC](../10-namespaces-and-rbac/advanced.en.md#resourcequota-namespace-limits)
- Foundation: the [Linux track](../../../linux/overview.en.md)

## Sources

- [Configure Liveness, Readiness and Startup Probes](https://kubernetes.io/docs/tasks/configure-pod-container/configure-liveness-readiness-startup-probes/)
- [Liveness, Readiness, and Startup Probes](https://kubernetes.io/docs/concepts/configuration/liveness-readiness-startup-probes/)
- [Resource Management for Pods and Containers](https://kubernetes.io/docs/concepts/configuration/manage-resources-containers/)
- [Pod Quality of Service Classes](https://kubernetes.io/docs/concepts/workloads/pods/pod-qos/)
- [Assign CPU Resources to Containers and Pods](https://kubernetes.io/docs/tasks/configure-pod-container/assign-cpu-resource/)
