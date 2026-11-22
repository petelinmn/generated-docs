---
title: "Deployment and ReplicaSet: basics"
lang: en
track: kubernetes
kind: module
doc: basics
slug: 05-deployments
part: 1
tags: [deployment, replicaset, scaling, rollout, rollback, pod-template-hash]
versions: "Kubernetes 1.37.0 (kind v0.34), kubectl 1.36.1"
status: draft
updated: 2026-11-22
---

# Deployment and ReplicaSet: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [04 · Pod](../04-pods/overview.en.md) · **Time:** ~2 h

All examples were run in kind (Kubernetes 1.37.0), namespace `learn-05`: `kubectl create namespace learn-05`.

## Why it matters

A single pod does not come back if it is deleted or its node fails. An application must run in several replicas and be updated without downtime. A Deployment solves both tasks: you describe which image and how many copies are needed, and Kubernetes maintains that state and can change it gradually.

## The Deployment manifest

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: web
spec:
  replicas: 3
  selector:
    matchLabels:
      app: web
  template:
    metadata:
      labels:
        app: web
    spec:
      containers:
        - name: nginx
          image: nginx:1.28
          ports:
            - containerPort: 80
```

- `replicas` — how many pods are needed.
- `selector.matchLabels` — by which labels the Deployment finds "its" pods.
- `template` — the pod template: everything that is in the `spec` of an ordinary pod (the image, ports, variables).
- The labels `template.metadata.labels` must match `selector`.

```text
$ kubectl -n learn-05 apply -f web.yaml
deployment.apps/web created
$ kubectl -n learn-05 rollout status deploy/web
deployment "web" successfully rolled out
```

## Deployment, ReplicaSet and pods

```text
$ kubectl -n learn-05 get deploy,rs,pods -L pod-template-hash
NAME                  READY  UP-TO-DATE  AVAILABLE  AGE
deployment.apps/web   3/3    3           3          88s

NAME                           DESIRED  CURRENT  READY  AGE
replicaset.apps/web-6d8567df8  3        3        3      88s

NAME                     READY  STATUS   RESTARTS  AGE
pod/web-6d8567df8-bqbpd  1/1    Running  0         88s
pod/web-6d8567df8-fjvhm  1/1    Running  0         88s
pod/web-6d8567df8-n9kgg  1/1    Running  0         88s
```

The Deployment created the ReplicaSet `web-6d8567df8`, and it created three pods. A ReplicaSet name consists of the Deployment name and the pod template hash (`6d8567df8`); a pod name consists of the ReplicaSet name and a random suffix. The hash is also recorded in the label `pod-template-hash`.

The Deployment columns:

| Column | Meaning |
|---|---|
| `READY` | ready pods / desired |
| `UP-TO-DATE` | pods with the current template |
| `AVAILABLE` | pods available to users |

The default values that Kubernetes filled in itself:

```text
$ kubectl -n learn-05 get deploy web -o jsonpath='strategy={.spec.strategy}{"\n"}revHist={.spec.revisionHistoryLimit} progressDeadline={.spec.progressDeadlineSeconds}{"\n"}'
strategy={"rollingUpdate":{"maxSurge":"25%","maxUnavailable":"25%"},"type":"RollingUpdate"}
revHist=10 progressDeadline=600
```

## Scaling

```text
$ kubectl -n learn-05 scale deploy/web --replicas=5
deployment.apps/web scaled
$ kubectl -n learn-05 get deploy web --no-headers | awk '{print $2}'
3/5
```

Right after the command `READY 3/5`: the missing pods are still starting. Editing `replicas` in the manifest and running `apply` gives the same result. Scaling neither recreates the existing pods nor creates a new ReplicaSet.

## Updating the image

The `kubectl set image` command changes the pod template:

```text
$ kubectl -n learn-05 set image deploy/web nginx=nginx:1.29
$ kubectl -n learn-05 rollout status deploy/web
Waiting for deployment "web" rollout to finish: 1 out of 3 new replicas have been updated...
Waiting for deployment "web" rollout to finish: 2 out of 3 new replicas have been updated...
Waiting for deployment "web" rollout to finish: 1 old replicas are pending termination...
deployment "web" successfully rolled out
```

The format `nginx=nginx:1.29` is `container-name=image`. Changing the template gave a new hash and a new ReplicaSet; the old ReplicaSet shrank to zero replicas but stayed for rollback:

```text
$ kubectl -n learn-05 get rs
NAME            DESIRED   CURRENT   READY
web-6d8567df8   0         0         0
web-c5748c689   3         3         3
```

(the AGE column is omitted). In the declarative style you edit the image in YAML and run `apply`: the result is the same.

## Rollback

```text
$ kubectl -n learn-05 rollout undo deploy/web
deployment.apps/web rolled back
$ kubectl -n learn-05 rollout status deploy/web
deployment "web" successfully rolled out
$ kubectl -n learn-05 get deploy web -o jsonpath='{.spec.template.spec.containers[0].image}{"\n"}'
nginx:1.28
```

`rollout undo` returns the template of the previous revision. History and rollback to a specific revision are in [advanced](advanced.en.md#history-of-revisions-and-rollback).

> ⚠️ After a rollback `kubectl` may warn: `resource deployments/web was previously managed with 'kubectl apply'. Rolling back will not update the kubectl.kubernetes.io/last-applied-configuration annotation`. If you keep manifests in Git, after a rollback bring the file in line with the cluster, otherwise the next `apply` will bring back the old version.

## Self-healing

The ReplicaSet watches the number of pods: a deleted pod appears again ([module 02](../02-architecture/basics.en.md#self-healing)). So to stop an application, reduce `replicas` (`--replicas=0`) or delete the Deployment.

```bash
kubectl -n learn-05 delete deploy web       # deletes both the ReplicaSet and the pods
kubectl delete namespace learn-05           # remove everything
```

## Beginner mistakes

- ❌ Editing pods by hand or deleting them to "update" → ✅ change the Deployment template: the controller will replace pods itself.
- ❌ `selector` and the template labels do not match → ✅ they must match, otherwise `apply` is rejected.
- ❌ Using the `latest` tag and expecting an update → ✅ without a template change there is no update; set unique image tags.
- ❌ Scaling and waiting for a new ReplicaSet → ✅ scaling does not change the template and the ReplicaSet is the same.
- ❌ Considering `READY 3/5` right after `scale` an error → ✅ the pods are still starting; watch `rollout status`.

## Related materials

- Next: [advanced](advanced.en.md) — a rolling update step by step, rollback, a stuck rollout
- Next module: [06 · Service and networking](../06-services/overview.en.md)
- Previous module: [04 · Pod](../04-pods/overview.en.md)
- The reconciliation loop: [02 · Kubernetes architecture](../02-architecture/overview.en.md)

## Sources

- [Deployments](https://kubernetes.io/docs/concepts/workloads/controllers/deployment/)
- [ReplicaSet](https://kubernetes.io/docs/concepts/workloads/controllers/replicaset/)
- [Performing a Rolling Update](https://kubernetes.io/docs/tutorials/kubernetes-basics/update/update-intro/)
- [kubectl rollout](https://kubernetes.io/docs/reference/kubectl/generated/kubectl_rollout/)
- [kubectl scale](https://kubernetes.io/docs/reference/kubectl/generated/kubectl_scale/)
