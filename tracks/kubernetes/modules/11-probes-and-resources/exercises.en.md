---
title: "Exercises: Probes and resources"
lang: en
track: kubernetes
kind: module
doc: exercises
slug: 11-probes-and-resources
part: 2
tags: [probes, liveness-probe, readiness-probe, startup-probe, resources, qos, cpu-throttling]
status: draft
updated: 2026-11-23
---

# Exercises: Probes and resources

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. Solutions are under spoilers. You need a kind cluster and a namespace: `kubectl create namespace learn-11`. Clean up afterwards: `kubectl delete namespace learn-11`.

## 1. 🟢 Liveness restarts a container

**Task:** run nginx with an `httpGet` liveness probe on the non-existent path `/healthz` (period 3 s, two failures). Find the cause of the restarts in the events and explain the exit code.

**Expected result:** `RESTARTS` grows, the events `Unhealthy … 404` and `Killing … will be restarted`, exit code `0` (`Completed`).

<details><summary>Hint</summary>

`kubectl describe pod` shows `Last State` and the events; parameters — `periodSeconds: 3`, `failureThreshold: 2`.

</details>

<details><summary>Solution</summary>

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
live-http   1/1   Running   2 (1s ago)   25s
    Last State:     Terminated
      Reason:       Completed
      Exit Code:    0
  Warning  Unhealthy  Liveness probe failed: HTTP probe failed with statuscode: 404
  Normal   Killing    Container c failed liveness probe, will be restarted
```

nginx got `SIGTERM` and ended gracefully with code 0: this is a kubelet decision, not an application crash.

</details>

## 2. 🟢 Readiness removes traffic but does not restart

**Task:** run a pod with an `exec` readiness (`cat /tmp/ready`), remove the file and show that `READY` became `0/1` and there are no restarts. Restore the file.

**Expected result:** `0/1 Running` with no growth of `RESTARTS`; after the file is back `1/1`.

<details><summary>Hint</summary>

Create the file with the start command: `touch /tmp/ready; exec nginx -g 'daemon off;'`. Pod conditions — `-o jsonpath='{range .status.conditions[*]}{.type}={.status} {end}'`.

</details>

<details><summary>Solution</summary>

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
ready-flip: 1/1
after rm: 0/1 Running
PodReadyToStartContainers=True Initialized=True Ready=False ContainersReady=False PodScheduled=True
after touch: 1/1 Running
```

A readiness failure changes `Ready` but does not restart the container; the address disappears from the Service endpoints.

</details>

## 3. 🟡 A startup probe against a restart loop

**Task:** run two pods that simulate a 25-second start (`sleep 25; exec nginx -g 'daemon off;'`): both with liveness (period 3 s, two failures), but only the second with a startup probe (period 3 s, 15 failures). Watch them for a minute and explain the difference.

**Expected result:** the first restarts (`r=1`), the second starts without restarts.

<details><summary>Hint</summary>

Startup blocks liveness and readiness until the first success. The reserve is `failureThreshold × periodSeconds`.

</details>

<details><summary>Solution</summary>

```yaml
startupProbe:
  httpGet: {path: /, port: 80}
  periodSeconds: 3
  failureThreshold: 15
```

```text
t~36s: slow-nostartup[1/1 Running r=0]  slow-startup[1/1 Running r=0]
t~48s: slow-nostartup[1/1 Running r=1]  slow-startup[1/1 Running r=0]
```

Without startup liveness kills the application before it starts (`connection refused`). With startup liveness does not start until the application answers; a reserve of 45 seconds. Note: without readiness the first pod was `1/1` from the first second although the port was still closed.

</details>

## 4. 🟡 QoS and cgroup values

**Task:** create four pods: `Guaranteed` (requests = limits = 100m/64Mi), one with `requests 50m/32Mi` and a memory-only limit of `128Mi`, one without resources, and one with only `requests 100m/64Mi`. Print each QoS class, and from inside — `cpu.max`, `memory.max`, `cpu.weight` and `oom_score_adj`.

**Expected result:** classes `Guaranteed`, `Burstable`, `BestEffort`, `Burstable`; `cpu.max=10000 100000` and `memory.max=67108864` for the first; `oom_score_adj` `-997`, `999`, `1000`, `998`.

<details><summary>Hint</summary>

`kubectl exec <pod> -- cat /sys/fs/cgroup/cpu.max /sys/fs/cgroup/memory.max /sys/fs/cgroup/cpu.weight /proc/1/oom_score_adj`.

</details>

<details><summary>Solution</summary>

```text
guaranteed -> Guaranteed     cpu.max=10000 100000  memory.max=67108864  cpu.weight=17  oom_score_adj=-997
burstable  -> Burstable      cpu.max=max 100000    memory.max=134217728 cpu.weight=11  oom_score_adj=999
besteffort -> BestEffort     cpu.max=max 100000    memory.max=max       cpu.weight=1   oom_score_adj=1000
onlyreq    -> Burstable      cpu.weight=17                                             oom_score_adj=998
```

A limit turns into `cpu.max` and `memory.max`, a CPU claim into `cpu.weight`, and the QoS class is reflected in `oom_score_adj`: `BestEffort` is killed first.

</details>

## 5. 🔴 Throttling, scheduling and invalid resources

**Task:** (a) run two pods that burn CPU for 20 seconds: one with a `100m` limit, the other without a limit; compare `usage_usec`, `nr_throttled`. (b) Create a pod with `requests.cpu: "100"` and find the cause of `Pending`. (c) Create a pod with `requests.cpu: 500m` and `limits.cpu: 100m`.

**Expected result:** (a) about 2 s of CPU and `nr_throttled == nr_periods` for the first, 20 s for the second; (b) `Insufficient cpu`; (c) `must be less than or equal to cpu limit`.

<details><summary>Hint</summary>

(a) `timeout 20 sh -c 'while :; do :; done'`, then `cat /sys/fs/cgroup/cpu.stat`. (b) `kubectl describe pod`, the events section.

</details>

<details><summary>Solution</summary>

```text
=== burn-limited (limit 100m)
usage_usec 2023496
nr_periods 202
nr_throttled 202
throttled_usec 8562744
=== burn-free (no limit)
usage_usec 20017330
nr_throttled 0
0/1 Pending
  Warning  FailedScheduling  default-scheduler  0/1 nodes are available: 1 Insufficient cpu. preemption: 0/1 nodes are available: 1 Preemption is not helpful for scheduling.
The Pod "badlimits" is invalid: spec.containers[0].resources.requests: Invalid value: "500m": must be less than or equal to cpu limit of 100m
```

A limit throttles regardless of node load; the scheduler looks at claims; a limit cannot be below the claim.

</details>
