---
title: "Observability: basics"
lang: en
track: kubernetes
kind: module
doc: basics
slug: 16-observability
part: 3
tags: [logs, kubectl-logs, events, previous-logs, kubelet, observability]
versions: "Kubernetes 1.37.0 (kind v0.34), kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Observability: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [04 · Pod](../04-pods/overview.en.md) · **Time:** ~1.5 h

All examples were run in kind (Kubernetes 1.37.0, three nodes), namespace `learn-16`. Two pods for the experiments:

- `multi` — two containers: `app` writes `app line N` to stdout once a second, `side` writes `side-log` to stderr once every two seconds;
- `crash` — the container prints `boot-<time>` and exits with code 1.

## Why it matters

You cannot "open and look at" an application in a cluster. Three questions come up all the time: what it writes, what is happening to it (restarts, image pull errors) and how it feels over time. `kubectl` is enough for the first two; the third needs metrics (see [advanced theory](advanced.en.md)).

## Logs: `kubectl logs`

A container writes to stdout and stderr, and the kubelet stores this output. If a pod has several containers, `kubectl` picks the first and says so:

```text
$ kubectl -n learn-16 logs multi
Defaulted container "app" out of: app, side
app line 1
app line 2
…
```

The main flags:

```text
$ kubectl -n learn-16 logs multi -c app --tail=2
app line 19
app line 20

$ kubectl -n learn-16 logs multi --all-containers --prefix --tail=2
[pod/multi/app] app line 19
[pod/multi/app] app line 20
[pod/multi/side] side-log
[pod/multi/side] side-log

$ kubectl -n learn-16 logs -l app=logdemo --tail=1 --prefix
[pod/crash/c] boot-1795439802
[pod/multi/app] app line 20

$ kubectl -n learn-16 logs multi -c app --since=5s --timestamps
2026-11-23T13:16:47.040690397Z app line 16
2026-11-23T13:16:48.042243973Z app line 17
```

| Flag | What it does |
|---|---|
| `-c <container>` | choose a container |
| `--all-containers` | output of all containers of the pod |
| `--prefix` | add `[pod/container]` before a line |
| `-l <selector>` | logs of all pods by labels |
| `--tail=N` | the last N lines |
| `--since=5s` | only the last 5 seconds |
| `--timestamps` | the time of every line |
| `-f` | follow the output |
| `-p` (`--previous`) | the previous instance of the container |

The flags `-f` and `--limit-bytes` were not run in the experiments.

### Logs of a crashed container: `--previous`

If a container restarted, `kubectl logs` shows the current instance and `--previous` shows the previous one. Experiment with the pod `crash2` (runs for 8 seconds, prints `start-<time>` and `dying`, exits with code 1):

```text
$ kubectl -n learn-16 get pod crash2 --no-headers
crash2   1/1   Running   1 (0s ago)   9s
$ kubectl -n learn-16 logs crash2
start-1795439848
$ kubectl -n learn-16 logs crash2 --previous
start-1795439840
dying
```

While the container is running again, `--previous` shows the last lines of the previous run. But with the pod `crash`, which crashes instantly and waits in `CrashLoopBackOff`, `--previous` did not work after several restarts:

```text
$ kubectl -n learn-16 get pod crash --no-headers
crash   0/1   Error   2 (36s ago)   37s
$ kubectl -n learn-16 logs crash --previous
unable to retrieve container logs for containerd://46ae9ad0…
```

Only the file of the last run (`2.log`) was left on the node; the kubelet had already removed the earlier instances. So for crashed pods also look at the ordinary `kubectl logs` (in the experiment it returned `boot-1795439802`) and at events.

## Events

Events are records of what happened to an object. They are shown by `kubectl describe` (the `Events` section) and by separate commands:

```text
$ kubectl -n learn-16 events --for pod/crash --types=Warning
LAST SEEN           TYPE      REASON    OBJECT      MESSAGE
48s (x3 over 86s)   Warning   BackOff   Pod/crash   Back-off restarting failed container c in pod crash_learn-16(…)

$ kubectl -n learn-16 get events --field-selector involvedObject.name=crash2,type=Normal \
    --sort-by=.lastTimestamp -o custom-columns=REASON:.reason,COUNT:.count
REASON      COUNT
Scheduled   1
Pulled      3
Created     3
Started     3
```

The usual order of pod events: `Scheduled` → `Pulled` → `Created` → `Started`; with problems — `BackOff`, `Failed`, `FailedScheduling` (see [module 18](../18-scheduling/basics.en.md#how-the-scheduler-chooses-a-node)).

Events are kept in the API for a limited time. In the experiment the API server has no `--event-ttl` flag; according to the documentation the default lifetime of an event is one hour.

> ⚠️ Verify: the default event retention (1 hour) is taken from the documentation; the experiment only confirmed that the kube-apiserver manifest has no `--event-ttl` flag.

## Where logs are on a node

The kubelet writes container output to files on the node. In kind you can look at them through `docker exec`:

```text
$ docker exec learn-worker ls -l /var/log/pods/learn-16_multi_<uid>/app/
-rw-r----- 1 root root 1445 Nov 23 13:16 0.log
$ docker exec learn-worker tail -2 /var/log/pods/learn-16_multi_<uid>/app/0.log
2026-11-23T13:16:58.055797529Z stdout F app line 27
2026-11-23T13:16:59.056976392Z stdout F app line 28
$ docker exec learn-worker ls -l /var/log/containers | grep multi
multi_learn-16_app-a8dc….log -> /var/log/pods/learn-16_multi_<uid>/app/0.log
```

- The directory `/var/log/pods/<namespace>_<pod>_<uid>/<container>/` holds files by restart number (`0.log`, `1.log`, …).
- The directory `/var/log/containers/` holds symbolic links to them, convenient for log collection agents.
- The line format is CRI: time, stream (`stdout` or `stderr`), a tag (`F` — a full line, `P` — a partial one), text.

The file size and the number of files are limited by kubelet settings:

```text
$ kubectl get --raw /api/v1/nodes/learn-worker/proxy/configz   # kubeletconfig fields
{'containerLogMaxSize': '10Mi', 'containerLogMaxFiles': 5, 'containerLogMaxWorkers': 1, 'containerLogMonitorInterval': '10s'}
```

After a pod is deleted, its files are removed from the node: log history needs external storage (see [advanced theory](advanced.en.md#log-collection-with-a-node-daemon)).

## Beginner mistakes

- ❌ Forgetting `-c` in a multi-container pod and not noticing `Defaulted container` → ✅ name the container or use `--all-containers`.
- ❌ Expecting `--previous` to always work → ✅ complement it with events and ordinary logs; keep important logs in external storage.
- ❌ Looking for the cause of a failure only in logs → ✅ start with `kubectl describe pod` and events.
- ❌ Relying on events as an archive → ✅ they are short-lived.
- ❌ Writing logs to files inside the container → ✅ to stdout/stderr.

## Related materials

- Next: [advanced theory](advanced.en.md) — log collection, Prometheus, Grafana
- Pod and lifecycle: [04 · Pod](../04-pods/overview.en.md)
- Probes and restarts: [11 · Probes and resources](../11-probes-and-resources/overview.en.md)
- `kubectl top`: [11 · Probes and resources](../11-probes-and-resources/basics.en.md#actual-consumption-kubectl-top)

## Sources

- [Logging Architecture](https://kubernetes.io/docs/concepts/cluster-administration/logging/)
- [kubectl logs](https://kubernetes.io/docs/reference/kubectl/generated/kubectl_logs/)
- [kubectl events](https://kubernetes.io/docs/reference/kubectl/generated/kubectl_events/)
- [Debug Running Pods](https://kubernetes.io/docs/tasks/debug/debug-application/debug-running-pod/)
