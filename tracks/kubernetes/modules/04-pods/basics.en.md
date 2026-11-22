---
title: "Pod: basics"
lang: en
track: kubernetes
kind: module
doc: basics
slug: 04-pods
part: 1
tags: [pod, container, lifecycle, restart-policy, crashloopbackoff, logs, events]
versions: "Kubernetes 1.37.0 (kind v0.34), kubectl 1.36.1"
status: draft
updated: 2026-11-22
---

# Pod: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [03 · Local cluster and kubectl](../03-local-cluster-and-kubectl/overview.en.md) · **Time:** ~2 h

All examples were run in a kind cluster (Kubernetes 1.37.0), namespace `learn-04`: `kubectl create namespace learn-04`.

## Why it matters

Kubernetes does not run containers "bare": it wraps them in a Pod. A pod is the unit of scheduling, networking and lifecycle. If you understand how a pod goes from `Pending` to `Running`, what `Completed` and `CrashLoopBackOff` mean and how to read events, you can sort out most problems in a cluster.

## The pod manifest

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

- `metadata.name` is the pod name in the namespace, `labels` are labels for selection.
- `spec.containers` is the list of containers; each requires `name` and `image`.
- `ports` is an informational field: it documents the port and is needed for named ports, but neither opens nor closes access.

```text
$ kubectl -n learn-04 apply -f web.yaml
pod/web created
```

## Phases and statuses

`status.phase` has five values:

| Phase | Meaning |
|---|---|
| `Pending` | the pod is accepted but the containers have not started yet (waiting for a node, pulling an image) |
| `Running` | the pod is assigned to a node and at least one container is running or restarting |
| `Succeeded` | all containers finished successfully and will not be restarted |
| `Failed` | all containers finished, at least one with an error |
| `Unknown` | the state could not be determined (usually a lost connection to the node) |

The `STATUS` column in `kubectl get pods` is more detailed. While the pod starts you see `ContainerCreating`:

```text
$ kubectl -n learn-04 get pod web
NAME   READY   STATUS              RESTARTS   AGE
web    0/1     ContainerCreating   0          2s
```

After the start the pod state is described by **conditions**:

```text
$ kubectl -n learn-04 get pod web -o jsonpath='{range .status.conditions[*]}{.type}={.status}{"\n"}{end}'
PodReadyToStartContainers=True
Initialized=True
Ready=True
ContainersReady=True
PodScheduled=True
```

`PodScheduled` — a node is chosen, `Initialized` — init containers are done, `ContainersReady` and `Ready` — the containers are ready to accept traffic. A summary of our pod:

```text
$ kubectl -n learn-04 get pod web -o jsonpath='phase={.status.phase} qos={.status.qosClass} ip={.status.podIP} restartPolicy={.spec.restartPolicy} dnsPolicy={.spec.dnsPolicy} grace={.spec.terminationGracePeriodSeconds}{"\n"}'
phase=Running qos=BestEffort ip=10.244.0.5 restartPolicy=Always dnsPolicy=ClusterFirst grace=30
```

Default values: `restartPolicy: Always`, `dnsPolicy: ClusterFirst`, `terminationGracePeriodSeconds: 30`. The QoS class is `BestEffort` because the pod has no requests and limits ([module 11](../../overview.en.md#part-2-advanced-topics)).

## Events and describe

The pod's life history is shown by `kubectl describe pod`:

```text
$ kubectl -n learn-04 describe pod web
…
Events:
  Type    Reason     Age   From               Message
  ----    ------     ----  ----               -------
  Normal  Scheduled  19s   default-scheduler  Successfully assigned learn-04/web to learn-control-plane
  Normal  Pulling    19s   kubelet            spec.containers{nginx}: Pulling image "nginx:1.29"
  Normal  Pulled     0s    kubelet            spec.containers{nginx}: Successfully pulled image "nginx:1.29" in 18.092s (18.092s including waiting). Image size: 62964090 bytes.
  Normal  Created    0s    kubelet            spec.containers{nginx}: Container created
  Normal  Started    0s    kubelet            spec.containers{nginx}: Container started
```

This is the same chain as in [module 02](../02-architecture/basics.en.md#what-happens-when-a-deployment-is-created): the scheduler assigned a node, the kubelet pulled the image and started the container.

## Logs and commands inside a pod

```bash
kubectl -n learn-04 logs web          # the container's stdout and stderr
kubectl -n learn-04 logs web -f       # follow the log
kubectl -n learn-04 exec web -- nginx -v
kubectl -n learn-04 exec -it web -- sh
```

If there are several containers, the `-c <name>` flag is needed (more in [advanced](advanced.en.md#several-containers-in-a-pod)).

## restartPolicy: what happens when a container ends

Three pods with different policies and commands (image `alpine:3.22`):

```yaml
apiVersion: v1
kind: Pod
metadata: {name: once}
spec:
  restartPolicy: Never
  containers:
    - name: c
      image: alpine:3.22
      command: ["sh", "-c", "echo done; exit 0"]
---
apiVersion: v1
kind: Pod
metadata: {name: crash}
spec:
  containers:
    - name: c
      image: alpine:3.22
      command: ["sh", "-c", "echo boom; exit 1"]
---
apiVersion: v1
kind: Pod
metadata: {name: failnever}
spec:
  restartPolicy: Never
  containers:
    - name: c
      image: alpine:3.22
      command: ["sh", "-c", "exit 3"]
```

After 45 seconds:

```text
$ kubectl -n learn-04 get pods once crash failnever
NAME        READY   STATUS      RESTARTS      AGE
once        0/1     Completed   0             45s
crash       0/1     Error       2 (34s ago)   45s
failnever   0/1     Error       0             45s
$ kubectl -n learn-04 get pod failnever -o jsonpath='{.status.phase} reason={.status.containerStatuses[0].state.terminated.reason} exit={.status.containerStatuses[0].state.terminated.exitCode}{"\n"}'
Failed reason=Error exit=3
$ kubectl -n learn-04 get pod once -o jsonpath='{.status.phase} reason={.status.containerStatuses[0].state.terminated.reason}{"\n"}'
Succeeded reason=Completed
```

| Policy | Behavior |
|---|---|
| `Always` (default) | always restart the container, even after a successful finish |
| `OnFailure` | restart only on a non-zero exit code |
| `Never` | do not restart |

`Completed` and `Error` are not phases but container termination reasons in the `STATUS` column: the phase of `once` is `Succeeded`, of `failnever` — `Failed`. The `crash` pod with the `Always` policy restarts constantly: the `RESTARTS` counter grows and the delay between attempts increases.

## CrashLoopBackOff

When a container crashes again and again, the kubelet increases the pause between restarts (exponentially, with a ceiling). During this time the pod status alternates between `Error` and `CrashLoopBackOff`. Watching the `crash` pod every 10 seconds:

```text
Error             4 (60s ago)
Error             4 (70s ago)
…
CrashLoopBackOff  4 (64s ago)
CrashLoopBackOff  4 (74s ago)
CrashLoopBackOff  4 (84s ago)
Error             5 (94s ago)
```

`CrashLoopBackOff` is not a cause but a waiting state: the container has already crashed and the kubelet waits before the next attempt (the events show `Back-off restarting failed container`). Look for the cause in the logs and the exit code:

```text
$ kubectl -n learn-04 logs crash
boom
$ kubectl -n learn-04 get pod crash -o jsonpath='restarts={.status.containerStatuses[0].restartCount} last={.status.containerStatuses[0].lastState.terminated.exitCode}{"\n"}'
restarts=2 last=1
```

The log of the previous run is shown by `kubectl logs crash --previous`. In our experiment this command printed `unable to retrieve container logs for containerd://…`: by the time of the request the container had already been removed. If so, look at the current log while the container runs, or at `describe`.

## Delete a pod

```bash
kubectl -n learn-04 delete pod web
kubectl -n learn-04 delete -f once.yaml
kubectl delete namespace learn-04     # remove everything
```

Stopping takes up to `terminationGracePeriodSeconds` (details in [advanced](advanced.en.md#pod-termination)).

## Beginner mistakes

- ❌ Thinking `Running` means "the application works" → ✅ `Running` means the container is started; readiness for traffic is shown by `Ready` and the readiness probe ([module 11](../../overview.en.md#part-2-advanced-topics)).
- ❌ Treating `CrashLoopBackOff` with a restart → ✅ look at `logs` and the exit code: the cause is in the application or its configuration.
- ❌ Running a "one-off" task with the `Always` policy → ✅ for one-off tasks `restartPolicy: Never` or `OnFailure` (better, a Job, module 12).
- ❌ Creating a "bare" pod for a service → ✅ use a Deployment: the controller will recreate the pod after a failure.
- ❌ Expecting `ports` to open a port → ✅ `containerPort` is only documentation; access is defined by a Service.

## Related materials

- Next: [advanced](advanced.en.md) — several containers, init, termination
- Next module: [05 · Deployment and ReplicaSet](../05-deployments/overview.en.md)
- Previous modules: [03 · kubectl](../03-local-cluster-and-kubectl/overview.en.md), [02 · Architecture](../02-architecture/overview.en.md)
- Images and `PID 1`: [01 · Containers and Docker](../01-containers/overview.en.md)

## Sources

- [Pods](https://kubernetes.io/docs/concepts/workloads/pods/)
- [Pod Lifecycle](https://kubernetes.io/docs/concepts/workloads/pods/pod-lifecycle/)
- [Container restart policy](https://kubernetes.io/docs/concepts/workloads/pods/pod-lifecycle/#restart-policy)
- [Debug Running Pods](https://kubernetes.io/docs/tasks/debug/debug-application/debug-running-pod/)
- [Configure Quality of Service for Pods](https://kubernetes.io/docs/tasks/configure-pod-container/quality-service-pod/)
