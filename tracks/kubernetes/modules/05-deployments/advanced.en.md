---
title: "Deployment and ReplicaSet: advanced"
lang: en
track: kubernetes
kind: module
doc: advanced
slug: 05-deployments
part: 1
tags: [deployment, rolling-update, maxsurge, maxunavailable, recreate, rollback, revision, progress-deadline, pause, rollout-restart]
versions: "Kubernetes 1.37.0 (kind v0.34), kubectl 1.36.1"
status: draft
updated: 2026-11-22
---

# Deployment and ReplicaSet: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on [basics](basics.en.md): the manifest, scaling and the simple update are not repeated.

## Rolling update step by step

What exactly happens when a Deployment of 3 replicas gets a new image (defaults: `maxSurge: 25%`, `maxUnavailable: 25%`)? Watching the ReplicaSets during the update (`kubectl get rs -w`); `web-6d8567df8` is the old one (nginx:1.28), `web-c5748c689` the new one (nginx:1.29). The fields: desired, current, ready.

```text
web-c5748c689 desired=1 current=0 ready=0     # new RS: 1 extra pod (surge) is allowed
web-c5748c689 desired=1 current=1 ready=0
web-c5748c689 desired=1 current=1 ready=1     # the new pod is ready
web-6d8567df8 desired=2 current=3 ready=3     # now one old pod can be removed
web-c5748c689 desired=2 current=1 ready=1
web-6d8567df8 desired=2 current=2 ready=2
web-c5748c689 desired=2 current=2 ready=1
web-c5748c689 desired=2 current=2 ready=2
web-6d8567df8 desired=1 current=2 ready=2
web-c5748c689 desired=3 current=2 ready=2
web-6d8567df8 desired=1 current=1 ready=1
web-c5748c689 desired=3 current=3 ready=3
web-6d8567df8 desired=0 current=1 ready=1
web-6d8567df8 desired=0 current=0 ready=0
```

(rows with changes were picked, repeats removed.) The steps:

1. The new ReplicaSet gets 1 replica: `maxSurge` allows temporarily having 1 pod more than desired.
2. Until the new pod becomes `Ready`, the old ones are not touched, because `maxUnavailable` is rounded down to 0.
3. When the new pod is ready, the old ReplicaSet shrinks by 1 and the new one grows by 1.
4. The steps repeat until the new ReplicaSet reaches 3 and the old one 0.

**Rounding:** 25% of 3 is 0.75. `maxSurge` rounds up (to 1), `maxUnavailable` rounds down (to 0). So with 3 replicas the update goes strictly one pod at a time, and the number of available pods never drops below 3.

The values can be set as a number or a percentage:

```yaml
spec:
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxSurge: 0
      maxUnavailable: 1
```

| Setting | Effect |
|---|---|
| `maxSurge: 1`, `maxUnavailable: 0` | no loss of capacity, a spare node is needed for the extra pod |
| `maxSurge: 0`, `maxUnavailable: 1` | no extra resources needed, but one pod fewer during the update |
| `maxSurge: 100%` | bring up the whole new set at once (blue-green in speed) |

The two parameters cannot both be zero at once: the update would not be able to start.

> ⚠️ Verify: the ban on both `maxSurge` and `maxUnavailable` being zero is given from documentation and was not reproduced.

## The Recreate strategy

`Recreate` stops all old pods and then creates new ones:

```text
$ kubectl -n learn-05 patch deploy web -p '{"spec":{"strategy":{"type":"Recreate","rollingUpdate":null}}}'
$ kubectl -n learn-05 get pods -w   # during set image (an observation summary, not verbatim output)
Running (3 old) → Terminating (3) → Pending → ContainerCreating → Running (3 new)
```

Old and new pods do not run at the same time: there is downtime between them. `rollingUpdate: null` must be passed, otherwise the API rejects conflicting settings. Use `Recreate` when two versions cannot work together (for example, an incompatible DB schema) and downtime is acceptable.

## History of revisions and rollback

Each ReplicaSet corresponds to a revision. The history:

```text
$ kubectl -n learn-05 rollout history deploy/web
deployment.apps/web
REVISION  CHANGE-CAUSE
2         to 1.29 (recreate)
3         <none>
```

Revision details:

```text
$ kubectl -n learn-05 rollout history deploy/web --revision=2
deployment.apps/web with revision #2
Pod Template:
  Labels:	app=web
	pod-template-hash=c5748c689
  Annotations:	kubernetes.io/change-cause: to 1.29 (recreate)
  Containers:
   nginx:
    Image:	nginx:1.29
```

Rollback:

```bash
kubectl -n learn-05 rollout undo deploy/web                    # to the previous revision
kubectl -n learn-05 rollout undo deploy/web --to-revision=1    # to a specific one
```

After a rollback the numbers change: rolling back to an old ReplicaSet makes it the newest revision. In the experiment revision 1 (nginx:1.28) got number 3 after the rollback, and revision 1 vanished from the list. The number of stored revisions is limited by `revisionHistoryLimit` (10 by default); with 0 a rollback is impossible.

### The change reason (change-cause)

The `CHANGE-CAUSE` column comes from the annotation `kubernetes.io/change-cause`. Set it **after** the template change: an annotation placed on the Deployment is copied to the current ReplicaSet. In the experiment the annotation `upgrade nginx to 1.29` was set before `set image`, and both revisions got this text:

```text
REVISION  CHANGE-CAUSE
1         upgrade nginx to 1.29
2         upgrade nginx to 1.29
```

The old revision was labeled wrongly. The right order: first change the template, then `kubectl annotate deploy web kubernetes.io/change-cause="…" --overwrite`. The `--record` flag is deprecated.

> ⚠️ Verify: the status of the `--record` flag (deprecated or removed) in kubectl 1.36 was not verified by running.

## A stuck rollout

What happens if you specify a non-existent image?

```text
$ kubectl -n learn-05 set image deploy/web nginx=nginx:does-not-exist
$ kubectl -n learn-05 rollout status deploy/web --timeout=10s
Waiting for deployment "web" rollout to finish: 1 out of 3 new replicas have been updated...
error: timed out waiting for the condition
$ kubectl -n learn-05 get pods
web-7b466b6cf6-n9xnt   0/1   ErrImagePull
web-c5748c689-4r6cf    1/1   Running
web-c5748c689-6t6k2    1/1   Running
web-c5748c689-h4lz6    1/1   Running
$ kubectl -n learn-05 get deploy web
NAME   READY   UP-TO-DATE   AVAILABLE
web    3/3     1            3
```

The rolling update protected the application: the new pod did not become ready, so no old pod was removed (`READY 3/3`, `AVAILABLE 3`). The Deployment state:

```text
Available=True (MinimumReplicasAvailable)
Progressing=True (ReplicaSetUpdated)
```

With `progressDeadlineSeconds: 600` (the default) the Deployment is considered "in progress" for 10 minutes. To see the failure quickly, shorten the deadline:

```text
$ kubectl -n learn-05 patch deploy web -p '{"spec":{"progressDeadlineSeconds":20}}'
$ kubectl -n learn-05 rollout status deploy/web --timeout=5s
error: deployment "web" exceeded its progress deadline
Available=True (MinimumReplicasAvailable)
Progressing=False (ProgressDeadlineExceeded)
```

The `ProgressDeadlineExceeded` state is only a signal: Kubernetes rolls nothing back by itself. A rollback is done with `kubectl rollout undo deploy/web`; in the experiment it returned `nginx:1.29`.

## Pause and resume

If you need to make several changes in one update, the rollout can be paused:

```text
$ kubectl -n learn-05 rollout pause deploy/web
$ kubectl -n learn-05 set image deploy/web nginx=nginx:1.29
$ kubectl -n learn-05 set resources deploy/web --requests=cpu=10m
$ kubectl -n learn-05 get rs --no-headers | awk '{print $1,$2,$3,$4}'
web-6d8567df8 0 0 0
web-79f5c5889 3 3 3          # no new ReplicaSet, the previous one works
web-7b466b6cf6 0 0 0
web-c5748c689 0 0 0
$ kubectl -n learn-05 rollout resume deploy/web
$ kubectl -n learn-05 get rs --no-headers | awk '{print $1,$2,$3,$4}'
web-754dbc8465 3 3 3         # after resume, one new ReplicaSet
web-79f5c5889 0 0 0
…
```

Both changes (the image and the resources) went into one new ReplicaSet `web-754dbc8465`, instead of two consecutive updates.

## Restarting without changing the image

```text
$ kubectl -n learn-05 rollout restart deploy/web
deployment.apps/web restarted
$ kubectl -n learn-05 get deploy web -o jsonpath='{.spec.template.metadata.annotations}'
{"kubectl.kubernetes.io/restartedAt":"2026-11-22T20:32:57+04:00"}
```

The command changes the template: it adds the annotation `restartedAt`. This is a new template hash, a new ReplicaSet and a smooth rolling update, not an instantaneous restart. The time is written in the client's time zone.

## The immutable selector

You cannot change `spec.selector` of an existing Deployment; an attempt gets a validation error:

```text
$ kubectl -n learn-05 patch deploy web --type=merge -p '{"spec":{"selector":{"matchLabels":{"app":"other"}}}}'
The Deployment "web" is invalid:
* spec.template.metadata.labels: Invalid value: {"app":"web"}: `selector` does not match template `labels`
```

The error shows the rule: the template labels must match the selector. To change the selector, create a new Deployment.

## Best practices

- Image tags are concrete versions (`nginx:1.29`), not `latest`.
- Set a `readinessProbe` (module 11): without it a rolling update considers a pod ready as soon as it is started.
- Tune `maxSurge` and `maxUnavailable` to the available resources and the acceptable downtime.
- Keep manifests in Git and apply with `apply`; after `rollout undo` bring the file in line.
- Set `kubernetes.io/change-cause` after changing the template.
- Do not leave `progressDeadlineSeconds` at the default in CI: shorten it so that a failed rollout is detected quickly.

## Common mistakes

- ❌ Thinking `ProgressDeadlineExceeded` will roll the release back → ✅ it only marks the failure; you must roll back yourself (or with a rollout tool).
- ❌ Expecting an "instant" update → ✅ it goes in steps; watch `rollout status`.
- ❌ Labeling a revision with `change-cause` before changing the template → ✅ the label lands on the previous revision.
- ❌ Setting `maxSurge: 0` and `maxUnavailable: 0` → ✅ the update cannot make a step.
- ❌ Using `Recreate` for a service that needs continuity → ✅ `RollingUpdate` with a readiness probe.

## Comparison with other tracks

- [C#](../../../csharp/recipes/background-jobs/overview.en.md): a rolling update sends `SIGTERM` to old pods; a worker must finish its work correctly.
- [Linux](../../../linux/overview.en.md): `rollout restart` does not send a signal to processes directly but replaces pods one by one.

## Related materials

- Previous level: [basics](basics.en.md)
- Next: [expert](expert.en.md)
- Next module: [06 · Service and networking](../06-services/overview.en.md)
- Stopping pods during a rollout: [04 · Pod](../04-pods/advanced.en.md#pod-termination)

## Sources

- [Deployments: strategy](https://kubernetes.io/docs/concepts/workloads/controllers/deployment/#strategy)
- [Deployments: updating](https://kubernetes.io/docs/concepts/workloads/controllers/deployment/#updating-a-deployment)
- [Deployments: rolling back](https://kubernetes.io/docs/concepts/workloads/controllers/deployment/#rolling-back-a-deployment)
- [Deployments: progress deadline](https://kubernetes.io/docs/concepts/workloads/controllers/deployment/#progress-deadline-seconds)
- [Deployments: pausing and resuming](https://kubernetes.io/docs/concepts/workloads/controllers/deployment/#pausing-and-resuming-a-deployment)
- [kubectl rollout restart](https://kubernetes.io/docs/reference/kubectl/generated/kubectl_rollout/kubectl_rollout_restart/)
