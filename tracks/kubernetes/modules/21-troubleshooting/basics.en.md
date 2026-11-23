---
title: "Troubleshooting: basics"
lang: en
track: kubernetes
kind: module
doc: basics
slug: 21-troubleshooting
part: 3
tags: [troubleshooting, describe, events, imagepullbackoff, crashloopbackoff, oomkilled, pending, createcontainerconfigerror]
versions: "Kubernetes 1.37.0 (kind v0.34, 3 nodes), kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Troubleshooting: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [04 · Pod](../04-pods/overview.en.md), [16 · Observability](../16-observability/basics.en.md) · **Time:** ~2 h

All examples were run in kind (Kubernetes 1.37.0), namespace `learn-21`: for each typical failure a pod with a deliberately wrong setup was created.

## Why it matters

Most of the time with Kubernetes goes not on writing manifests but on the question "why does it not work". The answer is almost always already in the cluster: you need to know how to find and read it.

## The method: from status to cause

1. **Status:** `kubectl get pods` — the `STATUS` column names the stage.
2. **Description and events:** `kubectl describe pod <pod>` — the `Events` section at the bottom explains the cause.
3. **Logs:** `kubectl logs <pod>` (and `--previous` for a restarted container, see [module 16](../16-observability/basics.en.md#logs-of-a-crashed-container---previous)).
4. **Going inside:** `kubectl exec` or `kubectl debug` ([advanced theory](advanced.en.md#kubectl-debug-looking-inside)).
5. **Related objects:** a Service, a PVC, a node, a namespace (quota).

The container state lives in the pod itself: `status.containerStatuses[].state` (now) and `lastState` (the previous run) with the fields `reason`, `message` and `exitCode`.

## Typical failures

All six pods were created at once:

```text
$ kubectl -n learn-21 get pods
NAME     READY   STATUS                       RESTARTS      AGE
badcmd   0/1     RunContainerError            2 (20s ago)   45s
badimg   0/1     ImagePullBackOff             0             45s
nocfg    0/1     CreateContainerConfigError   0             45s
noreg    0/1     ErrImagePull                 0             45s
oom      0/1     Error                        2 (29s ago)   45s
pvcpod   0/1     Pending                      0             45s
```

### ImagePullBackOff and ErrImagePull

The image cannot be pulled: there is no such tag or the registry is unreachable. `ErrImagePull` is the error itself, `ImagePullBackOff` is the pause before the next attempt.

```text
$ kubectl -n learn-21 get pod badimg -o jsonpath='{.status.containerStatuses[0].state}'
{"waiting":{"message":"Back-off pulling image \"nginx:no-such-tag\": ErrImagePull: rpc error: code = NotFound desc = failed to pull and unpack image \"docker.io/library/nginx:no-such-tag\": … docker.io/library/nginx:no-such-tag: not found","reason":"ImagePullBackOff"}}
```

The pod `noreg` with the image `registry.invalid/app:1` got the same status because the registry is unreachable. Check the name, tag, rights (`imagePullSecrets`) and the network to the registry.

### CreateContainerConfigError

The pod refers to a ConfigMap or Secret that does not exist:

```text
$ kubectl -n learn-21 get pod nocfg -o jsonpath='{.status.containerStatuses[0].state}'
{"waiting":{"message":"configmap \"missing-cm\" not found","reason":"CreateContainerConfigError"}}
```

The image was pulled (`Successfully pulled image`), but the container could not be created. Create the missing object ([module 07](../07-config-and-secrets/overview.en.md)); the pod starts by itself on the next attempt.

### RunContainerError

The container is created but the process does not start (the command `/no/such/binary`):

```text
Error: failed to create containerd task: failed to create shim task: OCI runtime create failed: runc create failed: unable to start container process: error during container init: exec: "/no/such/binary": stat /no/such/binary: no such file or directory
```

Then the pod goes into `BackOff`. Check `command` and `args`, the path in the image and execute permissions.

### OOMKilled

The container exceeded its memory limit (`limits.memory: 50Mi`) and was killed by the kernel:

```text
$ kubectl -n learn-21 get pod oom -o jsonpath='{.status.containerStatuses[0].lastState.terminated}'
{"exitCode":137,"finishedAt":"2026-11-23T13:55:12Z","reason":"OOMKilled","startedAt":"2026-11-23T13:55:12Z"}
$ kubectl -n learn-21 get pod oom
NAME   READY   STATUS      RESTARTS      AGE
oom    0/1     OOMKilled   2 (24s ago)   25s
```

Code `137` equals 128 + 9, that is, the signal `SIGKILL`. Raise the limit or look for a leak ([module 11](../11-probes-and-resources/basics.en.md)).

In one of the experiments a pod that tried to take memory with the command `x=$(head -c 200000000 /dev/zero | tr …)` finished with code `255` and the reason `Error`, not `OOMKilled`. So do not conclude a lack of memory from the exit code alone, look at `lastState.reason`.

### CrashLoopBackOff

A container starts and exits at once with an error (`exit 1`); the kubelet restarts it with a growing pause, and the status shows `Error` between attempts (in the experiment of module 16) and `CrashLoopBackOff` during the pause (per the documentation). Look for the cause in the logs (`logs`, `logs --previous`) and the exit code (`lastState.terminated.exitCode`). An example and the subtleties of `--previous` are in [module 16](../16-observability/basics.en.md#logs-of-a-crashed-container---previous).

### Pending

The pod is accepted but not placed. The cause is in the pod's event:

```text
$ kubectl -n learn-21 describe pod pvcpod | tail -3
  Warning  FailedScheduling  46s   default-scheduler  0/3 nodes are available: pod has unbound immediate PersistentVolumeClaims. not found
$ kubectl -n learn-21 describe pvc nopv | tail -3
  Warning  ProvisioningFailed  14s (x4 over 51s)  persistentvolume-controller  storageclass.storage.k8s.io "no-such-class" not found
```

The pod waits for a PVC, and the PVC names a StorageClass that does not exist. Conclusion: look at the related object (the PVC), not only at the pod itself. Other causes of `Pending` (resources, taints, selectors) are covered in [module 18](../18-scheduling/basics.en.md#how-the-scheduler-chooses-a-node), and PVCs in [module 08](../08-storage/overview.en.md).

## The table "status → first check"

| Status | First check |
|---|---|
| `Pending` | pod events, PVC, node resources, taints |
| `ImagePullBackOff`, `ErrImagePull` | image name and tag, the registry, `imagePullSecrets` |
| `CreateContainerConfigError` | the ConfigMaps and Secrets that are referenced |
| `RunContainerError` | `command`, `args`, the path in the image |
| `CrashLoopBackOff`, `Error` | `logs`, `logs --previous`, the exit code |
| `OOMKilled` (`137`) | `limits.memory`, memory leaks |
| `0/1 Running` | the readiness probe, `describe` ([advanced theory](advanced.en.md#a-service-without-endpoints)) |

## Beginner mistakes

- ❌ Looking only at `logs` when the container has not been created yet → ✅ `describe` and events.
- ❌ Deleting and re-creating the pod without reading events → ✅ collect facts first.
- ❌ Mixing up `ErrImagePull` and `ImagePullBackOff` → ✅ the first is the error, the second is the pause between attempts.
- ❌ Taking code `137` always for OOM → ✅ check `lastState.reason`.
- ❌ Looking for the cause of `Pending` in the application itself → ✅ the scheduler and volumes.

## Related materials

- Next: [advanced theory](advanced.en.md) — Service, DNS, `kubectl debug`, quota, `Terminating`, a node
- Logs and events: [16 · Observability](../16-observability/overview.en.md)
- Resources and probes: [11 · Probes and resources](../11-probes-and-resources/overview.en.md)
- Storage: [08 · Storage](../08-storage/overview.en.md)

## Sources

- [Debug Pods](https://kubernetes.io/docs/tasks/debug/debug-application/debug-pods/)
- [Troubleshoot Applications](https://kubernetes.io/docs/tasks/debug/debug-application/)
- [Determine the Reason for Pod Failure](https://kubernetes.io/docs/tasks/debug/debug-application/determine-reason-pod-failure/)
- [Images (pull policy and ImagePullBackOff)](https://kubernetes.io/docs/concepts/containers/images/)
