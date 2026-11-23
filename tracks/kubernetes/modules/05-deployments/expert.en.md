---
title: "Deployment and ReplicaSet: expert"
lang: en
track: kubernetes
kind: module
doc: expert
slug: 05-deployments
part: 1
tags: [deployment, replicaset, pod-template-hash, revision, conditions, rollout, collisioncount, observedgeneration]
versions: "Kubernetes 1.37.0 (kind v0.34), kubectl 1.36.1"
status: draft
updated: 2026-11-22
---

# Deployment and ReplicaSet: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## Under the hood

### How the controller keeps revisions

A Deployment does not create pods itself: it manages ReplicaSets. On every change of the pod template the controller computes the template hash and looks for a ReplicaSet with that hash. If it finds one, it scales it up (this is how a rollback works); if not, it creates a new one. The hash goes into the ReplicaSet name and into the `pod-template-hash` label, which both the ReplicaSet and its pods receive: this way two sets of pods of one Deployment do not overlap by selector.

The links between objects are recorded in metadata. The ReplicaSet of the active revision (`web-6d8567df8`):

```text
deployment.kubernetes.io/desired-replicas: "3"
deployment.kubernetes.io/max-replicas: "3"
deployment.kubernetes.io/revision: "3"
deployment.kubernetes.io/revision-history: "1"
ownerReferences: Deployment (controller: true)
```

- `revision` — the revision number; the Deployment has the same annotation with the number of the current one;
- `revision-history` — a list of former numbers this ReplicaSet has already carried: in the experiment the ReplicaSet from revision 1 got number 3 after the rollback and remembered `1`;
- `desired-replicas` and `max-replicas` — the controller's service values from which it computes the update steps (`max-replicas` = desired + `maxSurge`);
- `ownerReferences` — the Deployment as the owner with `controller: true`.

### The update steps

At each step the controller computes how many pods can be added in the new ReplicaSet (within `replicas + maxSurge`) and how many removed from the old ones (so that at least `replicas − maxUnavailable` stay available). The algorithm works by level, not by event, so an interrupted update continues after the controller-manager restarts ([the reconciliation loop](../02-architecture/advanced.en.md#the-reconciliation-loop)).

### The Deployment status

The `status` fields obtained from `kubectl explain deployment.status`:

| Field | Meaning |
|---|---|
| `replicas` | how many pods are created (of all revisions) |
| `updatedReplicas` | pods with the current template (`UP-TO-DATE`) |
| `readyReplicas` | ready ones |
| `availableReplicas` | available ones (ready for at least `minReadySeconds`) |
| `unavailableReplicas` | unavailable ones |
| `terminatingReplicas` | terminating ones |
| `observedGeneration` | the last `generation` the controller processed |
| `collisionCount` | how many times the hash had to be changed because of a collision |
| `conditions` | conditions (see below) |

The conditions of a healthy Deployment after a successful update:

```text
Available=True    (MinimumReplicasAvailable)  Deployment has minimum availability.
Progressing=True  (NewReplicaSetAvailable)    ReplicaSet "web-6d8567df8" has successfully progressed.
```

| Condition and reason | When |
|---|---|
| `Progressing=True`, `ReplicaSetUpdated` | an update is in progress |
| `Progressing=True`, `NewReplicaSetAvailable` | the update is finished |
| `Progressing=False`, `ProgressDeadlineExceeded` | `progressDeadlineSeconds` exceeded |
| `Available=True`, `MinimumReplicasAvailable` | at least `replicas − maxUnavailable` pods available |
| `ReplicaFailure` | the ReplicaSet cannot create pods (for example, a quota is exceeded) |

The values `ReplicaSetUpdated`, `NewReplicaSetAvailable`, `MinimumReplicasAvailable`, `ProgressDeadlineExceeded` were obtained by experiment in this material. `ReplicaFailure=True` with the reason `FailedCreate` was reproduced in other modules: when a quota is exceeded ([module 10](../10-namespaces-and-rbac/overview.en.md)) and when Pod Security Admission rejects pods ([module 17](../17-security/advanced.en.md#pod-security-admission-modes-in-practice)).

`kubectl rollout status` ends with code 0 on success and non-zero on an error or timeout (`error: deployment "web" exceeded its progress deadline`), which is handy in CI.

## Reference

### Deployment `spec` fields

The list from `kubectl explain deployment.spec`:

| Field | Meaning | Default |
|---|---|---|
| `replicas` | the number of pods | 1 |
| `selector` | the pod selector, immutable | required |
| `template` | the pod template | required |
| `strategy` | `RollingUpdate` (`maxSurge`, `maxUnavailable`) or `Recreate` | `RollingUpdate`, 25% / 25% (verified) |
| `minReadySeconds` | how long a pod must be ready to count as available | not set (0) |
| `revisionHistoryLimit` | how many old ReplicaSets to keep | 10 (verified) |
| `progressDeadlineSeconds` | the progress wait time | 600 (verified) |
| `paused` | pause the updates | `false` |

The default of `replicas` is 1 by documentation, not by experiment.

The ReplicaSet `spec` fields are `replicas`, `selector`, `template`, `minReadySeconds`.

### `kubectl rollout` commands

| Command | What it does |
|---|---|
| `rollout status deploy/X [--timeout=…]` | waits for completion, the return code reflects the result |
| `rollout history deploy/X [--revision=N]` | revisions and a revision's template |
| `rollout undo deploy/X [--to-revision=N]` | rollback |
| `rollout pause` / `resume` | pause / continue |
| `rollout restart` | restart through the `restartedAt` annotation |

The commands `scale`, `set image`, `set resources`, `set env` change a Deployment imperatively; `autoscale` creates an HPA (module 15).

## Gotchas

### Gotcha: change-cause labels the wrong revision

An annotation set before the template change is copied to the current ReplicaSet, and the history shows one text on two revisions (the experiment is in [advanced](advanced.en.md#the-change-reason-change-cause)). Label after the change.

### Gotcha: revision numbers change after a rollback

```text
REVISION  CHANGE-CAUSE
2         to 1.29 (recreate)
3         <none>
```

Revision 1 (nginx:1.28) vanished: the rollback reused its ReplicaSet and gave it number 3 (`revision-history: "1"`). A revision number cannot be considered a permanent release identifier; use labels and Git for history.

### Gotcha: a stuck rollout looks healthy

```text
NAME   READY   UP-TO-DATE   AVAILABLE
web    3/3     1            3
Available=True (MinimumReplicasAvailable)
Progressing=True (ReplicaSetUpdated)
```

With a wrong image `READY 3/3` and `Available=True`: the old pods work. Only `UP-TO-DATE 1` and `ErrImagePull` on the new pod are alarming; after 600 seconds `ProgressDeadlineExceeded` appears. Monitor `rollout status` and `UP-TO-DATE`, not just `READY`.

### Gotcha: ProgressDeadlineExceeded does not roll back

The `Progressing=False` condition is only a mark; the Deployment keeps trying and the old pods stay. A rollback is done by a human or a rollout system.

### Gotcha: rollout restart is not a restart of pods

The command adds the `restartedAt` annotation to the template: the hash changes, a new ReplicaSet is created, and the pods are replaced according to the update strategy (smoothly, with `RollingUpdate`). The time is written in the client's time zone (`+04:00` in the experiment).

### Gotcha: changes are not applied during a pause

After `rollout pause` the commands `set image` and `set resources` changed the template, but the active ReplicaSet stayed the same until `resume`; after it one new ReplicaSet with both changes appeared.

### Gotcha: the selector and the template labels

An attempt to change the `selector` was rejected: ``selector` does not match template `labels` ``. The selector is immutable after creation.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| `RollingUpdate` | no downtime, rollback by revisions | two versions run at the same time |
| `Recreate` | the two versions do not overlap | unavoidable downtime |
| Blue-green | instant switching, an easy rollback | twice the resources, a model outside Deployment (the canary and blue-green recipe) |
| Canary | testing on a small share of traffic | needs routing support (Ingress, a service mesh) |
| Argo Rollouts, Flagger | advanced strategies, metrics | a separate component (module 20) |
| StatefulSet | stable names and disks | different update behavior (module 12) |

## Interview questions

**1. How are Deployment, ReplicaSet and Pod related?**

<details><summary>Answer</summary>

A Deployment manages ReplicaSets (one per template revision), a ReplicaSet manages pods. The link is set by `ownerReferences`, and pods are matched by the selector and the `pod-template-hash` label.

</details>

**2. What triggers a rolling update?**

<details><summary>Answer</summary>

Only a change to the pod template (`spec.template`): the image, variables, template labels, template annotations. Changing `replicas` recreates nothing.

</details>

**3. How do `maxSurge` and `maxUnavailable` work and how are they rounded?**

<details><summary>Answer</summary>

`maxSurge` is by how many pods above `replicas` you may temporarily exceed; `maxUnavailable` is how many pods may be unavailable. Percentages are computed from `replicas`: `maxSurge` rounds up, `maxUnavailable` down. With 3 replicas and 25% this is 1 and 0, so the update goes one pod at a time.

</details>

**4. (Trick) The image in a Deployment is wrong. Will the application go down?**

<details><summary>Answer</summary>

No, while the old pods work: a rolling update does not remove them until the new ones are ready. The rollout gets stuck (`ErrImagePull`, `UP-TO-DATE 1`), and after `progressDeadlineSeconds` `ProgressDeadlineExceeded` appears. A rollback is not performed automatically.

</details>

**5. How do you roll back a release and what happens to revision numbers?**

<details><summary>Answer</summary>

`kubectl rollout undo deploy/X [--to-revision=N]`. The controller reuses the old ReplicaSet and gives it a new, the largest revision number; the former number disappears from the list (in the experiment revision 1 became 3).

</details>

**6. (Trick) What does `kubectl rollout restart` do?**

<details><summary>Answer</summary>

It adds the annotation `kubectl.kubernetes.io/restartedAt` to the template. This changes the hash, a new ReplicaSet is created and pods are replaced by the update strategy, not all at once.

</details>

**7. How does `Recreate` differ from `RollingUpdate`?**

<details><summary>Answer</summary>

`Recreate` stops all old pods and then starts new ones: there is downtime, but the two versions do not run together. `RollingUpdate` replaces pods gradually with no downtime.

</details>

**8. What is `revisionHistoryLimit` for?**

<details><summary>Answer</summary>

How many old ReplicaSets to keep for rollback (10 by default). With 0 a rollback is impossible; a larger value takes more objects in etcd.

</details>

**9. What does `Progressing=False (ProgressDeadlineExceeded)` show?**

<details><summary>Answer</summary>

The update has not progressed for longer than `progressDeadlineSeconds` (600 s by default). It is only a signal: Kubernetes rolls nothing back and keeps trying. In the experiment with a 20 s deadline `rollout status` returned `error: deployment "web" exceeded its progress deadline`.

</details>

**10. (Trick) Why does `kubectl rollout history` show the same CHANGE-CAUSE on two revisions?**

<details><summary>Answer</summary>

The annotation `kubernetes.io/change-cause` was set on the Deployment before the template change and copied to the current (still old) ReplicaSet. You must label after changing the template.

</details>

**11. What are `pause`/`resume` for?**

<details><summary>Answer</summary>

To make several changes in one update. While paused, template changes do not start a rollout; after `resume` one ReplicaSet with all the changes is created.

</details>

**12. Can you change the `selector` of an existing Deployment?**

<details><summary>Answer</summary>

No, the selector is immutable; the template labels must match it (the validation error ``selector` does not match template `labels` ``). For a new selector you create a new Deployment.

</details>

## Cheat sheet

```bash
kubectl apply -f web.yaml
kubectl set image deploy/web nginx=nginx:1.29
kubectl annotate deploy/web kubernetes.io/change-cause="to 1.29" --overwrite   # after set image
kubectl rollout status deploy/web --timeout=120s        # a return code for CI
kubectl rollout history deploy/web [--revision=N]
kubectl rollout undo deploy/web [--to-revision=N]
kubectl rollout pause|resume|restart deploy/web
kubectl scale deploy/web --replicas=5
kubectl get deploy,rs,pods -L pod-template-hash
kubectl get deploy web -o jsonpath='{.status.conditions[*].reason}'
```

## Related materials

- Previous levels: [basics](basics.en.md), [advanced](advanced.en.md)
- Next module: [06 · Service and networking](../06-services/overview.en.md)
- Previous modules: [04 · Pod](../04-pods/overview.en.md), [02 · Architecture](../02-architecture/overview.en.md)

## Sources

- [Deployments](https://kubernetes.io/docs/concepts/workloads/controllers/deployment/)
- [Deployment: status and conditions](https://kubernetes.io/docs/concepts/workloads/controllers/deployment/#deployment-status)
- [Deployment API reference](https://kubernetes.io/docs/reference/kubernetes-api/workload-resources/deployment-v1/)
- [ReplicaSet](https://kubernetes.io/docs/concepts/workloads/controllers/replicaset/)
- [Well-Known Labels, Annotations and Taints](https://kubernetes.io/docs/reference/labels-annotations-taints/)
- [kubectl rollout](https://kubernetes.io/docs/reference/kubectl/generated/kubectl_rollout/)
