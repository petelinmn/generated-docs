---
title: "Pod: expert"
lang: en
track: kubernetes
kind: module
doc: expert
slug: 04-pods
part: 1
tags: [pod, pause-container, sandbox, namespaces, qos, oomkilled, exit-code, termination-message, activedeadlineseconds]
versions: "Kubernetes 1.37.0 (kind v0.34, containerd 2.3.4), kubectl 1.36.1"
status: draft
updated: 2026-11-22
---

# Pod: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## Under the hood

### The sandbox and the pause container

A pod is not a Linux entity but a runtime convention. For each pod the runtime (containerd) creates a **sandbox** — a set of shared namespaces — and keeps it alive with a service container `pause`. The pod's containers join the namespaces of this sandbox. On a kind node (the `crictl` command inside the node):

```text
$ docker exec learn-control-plane crictl pods --name multi
POD ID          CREATED                  STATE   NAME    NAMESPACE   ATTEMPT   RUNTIME
d156afd06f10c   Less than a second ago   Ready   multi   learn-04    0         (default)
$ docker exec learn-control-plane sh -c 'ps -eo pid,comm,args | grep pause | head -3'
  363 pause   /pause
  422 pause   /pause
  436 pause   /pause
```

The `/pause` process exists once per pod. It does nothing but own the network space and the IP address. Therefore:

- restarting a container does not change the pod's IP: the namespace belongs to the sandbox, not the container;
- all containers have one IP and a shared `localhost` (verified: `hostname -i` in two containers of the `multi` pod gave `10.244.0.43`);
- if you enable `shareProcessNamespace: true`, `pause` becomes PID 1 of the shared process space (`PID 1 /pause`) and also reaps zombies.

The containers' file systems are separate (from each one's image); shared data goes through volumes.

### Container states

Each container has one of three `state`s:

| State | Fields |
|---|---|
| `waiting` | `reason` (`ContainerCreating`, `ImagePullBackOff`, `CrashLoopBackOff`, …) |
| `running` | `startedAt` |
| `terminated` | `exitCode`, `reason` (`Completed`, `Error`, `OOMKilled`), `message`, `startedAt`, `finishedAt` |

```text
$ kubectl -n learn-04 get pod web -o jsonpath='{.status.containerStatuses[0].state}'
{"running":{"startedAt":"2026-11-22T16:24:47Z"}}
```

`lastState` keeps the previous termination, which is where you take the exit code of a crashing container.

### The QoS class

`status.qosClass` depends on requests and limits (it is used when evicting pods under resource pressure):

| Class | Condition | Observation |
|---|---|---|
| `Guaranteed` | all containers have requests = limits for CPU and memory | — |
| `Burstable` | something is set but not as for `Guaranteed` | a pod with a single `limits.memory: 20Mi` got `qos=Burstable` |
| `BestEffort` | nothing is set | the `web` pod without resources has `qos=BestEffort` |

More in module 11 (Probes and resources).

## Reference

### Pod `spec` fields

The list was obtained from `kubectl explain pod.spec` (kubectl 1.36.1, cluster 1.37.0). The fields worth attention:

| Field | Meaning |
|---|---|
| `containers`, `initContainers`, `ephemeralContainers` | main, init and debugging containers |
| `restartPolicy` | `Always` (default), `OnFailure`, `Never` |
| `terminationGracePeriodSeconds` | time to stop (30 s by default) |
| `activeDeadlineSeconds` | the maximum active time of a pod; after it `Failed` with the reason `DeadlineExceeded` |
| `volumes` | pod volumes |
| `nodeName`, `nodeSelector`, `affinity`, `tolerations`, `topologySpreadConstraints`, `schedulerName`, `priorityClassName`, `preemptionPolicy` | scheduling (module 18) |
| `serviceAccountName`, `automountServiceAccountToken` | the pod's account |
| `securityContext`, `hostUsers` | pod-level security |
| `hostNetwork`, `hostPID`, `hostIPC`, `shareProcessNamespace` | sharing the host's or containers' namespaces |
| `dnsPolicy`, `dnsConfig`, `hostAliases`, `hostname`, `subdomain`, `setHostnameAsFQDN`, `enableServiceLinks` | DNS and names |
| `imagePullSecrets` | secrets for private registries |
| `readinessGates` | extra readiness conditions |
| `runtimeClassName`, `overhead`, `resources` | the runtime and pod resources |
| `os` | the pod's operating system |

### Container fields

`name`, `image`, `imagePullPolicy`, `command` (replaces `ENTRYPOINT`), `args` (replaces `CMD`), `workingDir`, `env`, `envFrom`, `ports`, `resources`, `volumeMounts`, `volumeDevices`, `livenessProbe`, `readinessProbe`, `startupProbe`, `lifecycle` (`postStart`, `preStop`), `securityContext`, `stdin`, `stdinOnce`, `tty`, `terminationMessagePath`, `terminationMessagePolicy`, `restartPolicy` (the restart policy of an individual container; on an init container the value `Always` makes it a native sidecar), `resizePolicy`, `restartPolicyRules`.

The list of fields was also obtained from `kubectl explain pod.spec.containers`.

> ⚠️ Verify: the purpose of the `restartPolicyRules` and `resizePolicy` fields, and whether `restartPolicy` is allowed on ordinary (non-init) containers in version 1.37, were not verified by running; check against `kubectl explain pod.spec.containers.restartPolicyRules`.

### Exit codes

If a container is killed by a signal, the exit code is 128 + the signal number. In the experiment a container that exceeded its memory limit (`limits.memory: 20Mi`, the command `tail /dev/zero`) ended like this:

```text
$ kubectl -n learn-04 get pod oom -o jsonpath='phase={.status.phase} reason={.status.containerStatuses[0].state.terminated.reason} exit={.status.containerStatuses[0].state.terminated.exitCode}'
phase=Failed reason=OOMKilled exit=137
```

137 = 128 + 9 (`SIGKILL`); the reason is `OOMKilled`. Code 143 = 128 + 15 corresponds to `SIGTERM`.

### The termination message

A container can write a reason to the file `/dev/termination-log` (the path is set by `terminationMessagePath`), and it appears in the status:

```text
$ kubectl -n learn-04 get pod msg -o jsonpath='{.status.containerStatuses[0].state.terminated.message}'
reason-for-exit
```

## Gotchas

### Gotcha: Running does not mean it works

`Running` is the pod phase: the container is started. The application may not accept requests yet or may be broken; readiness is shown by the `Ready` condition and the readiness probe. The `crash` pod from the [basics](basics.en.md#crashloopbackoff) is also in the `Running` phase between restarts.

### Gotcha: Completed and Error are not phases

```text
$ kubectl -n learn-04 get pod failnever -o jsonpath='{.status.phase}'
Failed
```

The `STATUS` column shows `Error` but the phase is `Failed`; for `once` `STATUS=Completed`, the phase is `Succeeded`. In scripts use `status.phase`.

### Gotcha: activeDeadlineSeconds kills a pod with a green status

```text
$ kubectl -n learn-04 get pod deadline -o jsonpath='phase={.status.phase} reason={.status.reason} msg={.status.message}'
phase=Failed reason=DeadlineExceeded msg=Pod was active on the node longer than the specified deadline
```

A pod with `activeDeadlineSeconds: 8` and `sleep 3600` went to `Failed` after 8 seconds: a perfectly healthy `sleep` had not finished. 26 seconds after creation it still showed `Running`: the kubelet applies the deadline not instantly (on a repeated request the pod was already `Failed`). For tasks with a time limit a Job is better.

### Gotcha: the previous container's logs can vanish

`kubectl logs crash --previous` in our experiment returned `unable to retrieve container logs for containerd://…`: the runtime had already removed the previous container. For diagnosing failures set up centralized log collection (module 16).

### Gotcha: no `sh` — no `exec`

An image without a shell (`scratch`) does not let you run `kubectl exec … sh` ([module 03](../03-local-cluster-and-kubectl/basics.en.md#basic-commands)). Debugging such pods is through ephemeral containers (module 21).

### Gotcha: deleting a pod takes 30 seconds

If the application does not handle `SIGTERM`, every deletion and every rollout waits for `terminationGracePeriodSeconds`: in the experiment `term-ignore deleted in 32s` against `1s` for a signal handler ([advanced](advanced.en.md#pod-termination)).

## Trade-offs and alternatives

| What you need | Choice |
|---|---|
| A long-lived service with replicas | Deployment ([module 05](../05-deployments/overview.en.md)) |
| A one-off or periodic task | Job and CronJob (module 12) |
| An application with state and stable names | StatefulSet (module 12) |
| An agent on every node | DaemonSet (module 12) |
| Two tightly coupled processes | one pod, two containers or a native sidecar |
| Independent scaling | separate pods |

## Interview questions

**1. What is a Pod and why not just a container?**

<details><summary>Answer</summary>

A Pod is the unit of scheduling and networking: one or more containers with a shared IP, `localhost` and volumes that always live on one node. It lets you describe tightly coupled processes (an application and a sidecar) as a whole.

</details>

**2. What do containers of one pod share and what is separate?**

<details><summary>Answer</summary>

Shared: the network space (one IP, `localhost`), the host name, volumes mounted into both containers and (by a flag) the process space. Separate: the file system from the image, processes (without `shareProcessNamespace`), the containers' resources and `securityContext`.

</details>

**3. What is the `pause` container for?**

<details><summary>Answer</summary>

It holds the pod's sandbox: the network space and the IP. Thanks to it the IP does not change when ordinary containers restart. With `shareProcessNamespace: true` it becomes PID 1 and reaps finished processes.

</details>

**4. (Trick) How does the `Failed` phase differ from the `CrashLoopBackOff` status?**

<details><summary>Answer</summary>

`Failed` is a pod phase: all containers have finished, one with an error, and no restarts will happen (for example, `restartPolicy: Never`). `CrashLoopBackOff` is a container waiting state under a policy that restarts: the pod phase is `Running` then.

</details>

**5. What values does `restartPolicy` have and what is the default?**

<details><summary>Answer</summary>

`Always` (default), `OnFailure`, `Never`. With `Always` a container restarts even after a successful finish; in the experiment a pod with `exit 0` and `Never` stayed `Completed`, while a pod with `Always` and `exit 1` kept restarting.

</details>

**6. How do init containers work?**

<details><summary>Answer</summary>

They run one after another before the main containers, each must finish successfully. While they run the status is `Init:N/M`; on failure — `Init:Error` with restarts per the policy. They are for preparation: waiting for a dependency, migrations, copying files into a shared volume.

</details>

**7. How is a native sidecar built?**

<details><summary>Answer</summary>

It is an init container with `restartPolicy: Always`: it starts before the main containers, must not finish and works for the pod's whole life, and stops after the main ones. The pod becomes `Running` and `READY` includes the sidecar (`2/2`).

</details>

**8. (Trick) What happens on `kubectl delete pod` if the application ignores `SIGTERM`?**

<details><summary>Answer</summary>

`preStop` runs, then `SIGTERM`, waiting up to `terminationGracePeriodSeconds` (30 s) and `SIGKILL`. In the experiment `sleep` as PID 1 led to deletion in 32 s, while a `trap` handler took 1 s.

</details>

**9. When does `preStop` run and does its time count toward the grace period?**

<details><summary>Answer</summary>

Before `SIGTERM` is sent; its time counts toward the total `terminationGracePeriodSeconds`. In the experiment a 3-second hook plus signal handling gave a deletion in 4 seconds.

</details>

**10. What does exit code 137 mean?**

<details><summary>Answer</summary>

128 + 9: the process was killed by `SIGKILL`. Typical causes: `OOMKilled` (the memory limit was exceeded; in the experiment `reason=OOMKilled exit=137`) or `terminationGracePeriodSeconds` expired.

</details>

**11. How does `Succeeded` differ from `Completed`?**

<details><summary>Answer</summary>

`Succeeded` is the pod phase (`status.phase`); `Completed` is the container termination reason in `STATUS` of `kubectl get`. Likewise `Failed` is a phase, `Error` is a reason.

</details>

**12. (Trick) A pod with `activeDeadlineSeconds: 8` runs `sleep 3600`. What happens?**

<details><summary>Answer</summary>

After ~8 seconds the pod goes to `Failed` with the reason `DeadlineExceeded` (`Pod was active on the node longer than the specified deadline`), although the process did not crash. Applying the deadline is not instant.

</details>

**13. How does a pod learn its own name and IP?**

<details><summary>Answer</summary>

Through the Downward API: environment variables with `valueFrom.fieldRef` (`metadata.name`, `status.podIP`) or volumes. In the experiment `POD_NAME=multi`, `POD_IP=10.244.0.11`.

</details>

## Cheat sheet

```bash
kubectl run web --image=nginx:1.29 --restart=Never          # a quick pod
kubectl get pod web -o jsonpath='{.status.phase}'
kubectl get pod web -o jsonpath='{.status.containerStatuses[0].state}'
kubectl describe pod web                                    # events
kubectl logs web [-c name] [-f] [--previous] [--all-containers]
kubectl exec -it web -c name -- sh
kubectl delete pod web --now                                # without waiting for the grace period
kubectl get pod web -o jsonpath='{.status.qosClass}'
```

## Related materials

- Previous levels: [basics](basics.en.md), [advanced](advanced.en.md)
- Next module: [05 · Deployment and ReplicaSet](../05-deployments/overview.en.md)
- Previous modules: [03 · kubectl](../03-local-cluster-and-kubectl/overview.en.md), [02 · Architecture](../02-architecture/overview.en.md), [01 · Containers](../01-containers/overview.en.md)
- Foundation: the [Linux track](../../../linux/overview.en.md) — namespaces and cgroups

## Sources

- [Pods](https://kubernetes.io/docs/concepts/workloads/pods/)
- [Pod Lifecycle](https://kubernetes.io/docs/concepts/workloads/pods/pod-lifecycle/)
- [Pod Quality of Service Classes](https://kubernetes.io/docs/concepts/workloads/pods/pod-qos/)
- [Pod API reference](https://kubernetes.io/docs/reference/kubernetes-api/workload-resources/pod-v1/)
- [Determine the Reason for Pod Failure (termination message)](https://kubernetes.io/docs/tasks/debug/debug-application/determine-reason-pod-failure/)
- [Share Process Namespace between Containers in a Pod](https://kubernetes.io/docs/tasks/configure-pod-container/share-process-namespace/)
- [Container Runtime Interface (CRI)](https://kubernetes.io/docs/concepts/architecture/cri/)
