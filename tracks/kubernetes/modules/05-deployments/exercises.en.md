---
title: "Exercises: Deployment and ReplicaSet"
lang: en
track: kubernetes
kind: module
doc: exercises
slug: 05-deployments
part: 1
tags: [deployment, replicaset, rolling-update, rollback, pause, progress-deadline]
status: draft
updated: 2026-11-22
---

# Exercises: Deployment and ReplicaSet

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. Solutions are under spoilers. You need a kind cluster and a namespace: `kubectl create namespace learn-05`. Clean up afterwards: `kubectl delete namespace learn-05`.

## 1. 🟢 The first Deployment

**Task:** create a Deployment `web` on `nginx:1.28` with 3 replicas, wait for readiness, print the Deployment, ReplicaSet and pods with the `pod-template-hash` label. Then raise the replicas to 5 and explain why no new ReplicaSet appeared.

**Expected result:** one ReplicaSet with the hash in its name; after `scale` the same ReplicaSet, 5 pods.

<details><summary>Hint</summary>

`kubectl get deploy,rs,pods -L pod-template-hash`. Scaling does not change the pod template.

</details>

<details><summary>Solution</summary>

```yaml
apiVersion: apps/v1
kind: Deployment
metadata: {name: web}
spec:
  replicas: 3
  selector: {matchLabels: {app: web}}
  template:
    metadata: {labels: {app: web}}
    spec:
      containers:
        - {name: nginx, image: "nginx:1.28", ports: [{containerPort: 80}]}
```

```bash
kubectl -n learn-05 apply -f web.yaml
kubectl -n learn-05 rollout status deploy/web
kubectl -n learn-05 get deploy,rs,pods -L pod-template-hash
kubectl -n learn-05 scale deploy/web --replicas=5
kubectl -n learn-05 get rs
```

The ReplicaSet name is `web-<template hash>`. `scale` changes only `replicas`, the template is the same, so the same ReplicaSet simply gets more pods.

</details>

## 2. 🟢 Update and rollback

**Task:** update the image to `nginx:1.29`, wait for completion, then roll back and check which image is in the template. How many ReplicaSets exist now and how many replicas do they have?

**Expected result:** the image `nginx:1.28` after the rollback; two ReplicaSets, one with 3 replicas, the other with 0.

<details><summary>Hint</summary>

`kubectl set image deploy/web nginx=nginx:1.29`, then `kubectl rollout undo deploy/web`.

</details>

<details><summary>Solution</summary>

```bash
kubectl -n learn-05 set image deploy/web nginx=nginx:1.29
kubectl -n learn-05 rollout status deploy/web
kubectl -n learn-05 rollout undo deploy/web
kubectl -n learn-05 rollout status deploy/web
kubectl -n learn-05 get deploy web -o jsonpath='{.spec.template.spec.containers[0].image}{"\n"}'
kubectl -n learn-05 get rs
```

```text
nginx:1.28
```

After the update there was a new ReplicaSet (1.29), the old one shrank to 0 replicas but stayed for rollback; `undo` brought back the old ReplicaSet with 3 replicas, and the new one became 0.

</details>

## 3. 🟡 The rolling update trace

**Task:** start watching ReplicaSets (`kubectl get rs -w`) and change the image in another terminal. From the output explain how `desired`, `current` and `ready` changed and why with 3 replicas the update goes one pod at a time.

**Expected result:** the new ReplicaSet first gets `desired=1`, the old one shrinks only after the new pod is ready.

<details><summary>Hint</summary>

`maxSurge: 25%` of 3 rounds up, `maxUnavailable: 25%` rounds down.

</details>

<details><summary>Solution</summary>

```bash
kubectl -n learn-05 get rs -w &
kubectl -n learn-05 set image deploy/web nginx=nginx:1.29
```

```text
web-c5748c689 desired=1 current=0 ready=0
web-c5748c689 desired=1 current=1 ready=1
web-6d8567df8 desired=2 current=3 ready=3
web-c5748c689 desired=2 current=2 ready=2
web-6d8567df8 desired=1 current=2 ready=2
web-c5748c689 desired=3 current=3 ready=3
web-6d8567df8 desired=0 current=0 ready=0
```

(rows picked from the full output). `maxSurge` = ⌈0.75⌉ = 1, `maxUnavailable` = ⌊0.75⌋ = 0: first one new pod appears, and only when it is ready is one old one removed.

</details>

## 4. 🟡 A wrong image and a stuck rollout

**Task:** set the image `nginx:does-not-exist`. Check `READY`, `UP-TO-DATE` and the pod state. Then shorten `progressDeadlineSeconds` to 20, wait for the failure and print the Deployment conditions. Roll back.

**Expected result:** `READY 3/3`, `UP-TO-DATE 1`, `ErrImagePull` on the new pod; then `Progressing=False (ProgressDeadlineExceeded)`.

<details><summary>Hint</summary>

`kubectl patch deploy web -p '{"spec":{"progressDeadlineSeconds":20}}'`. Conditions: `-o jsonpath='{range .status.conditions[*]}{.type}={.status} ({.reason}){"\n"}{end}'`.

</details>

<details><summary>Solution</summary>

```bash
kubectl -n learn-05 set image deploy/web nginx=nginx:does-not-exist
kubectl -n learn-05 get deploy web
kubectl -n learn-05 patch deploy web -p '{"spec":{"progressDeadlineSeconds":20}}'
sleep 35
kubectl -n learn-05 rollout status deploy/web --timeout=5s
kubectl -n learn-05 get deploy web -o jsonpath='{range .status.conditions[*]}{.type}={.status} ({.reason}){"\n"}{end}'
kubectl -n learn-05 rollout undo deploy/web
```

```text
error: deployment "web" exceeded its progress deadline
Available=True (MinimumReplicasAvailable)
Progressing=False (ProgressDeadlineExceeded)
```

The old pods are untouched (`Available=True`), the new one does not start. The `ProgressDeadlineExceeded` condition is only a signal, and the rollback is done by a human.

</details>

## 5. 🔴 Pause, resume and the Recreate strategy

**Task:** (a) pause the Deployment, change the image and the CPU request, make sure there are no new ReplicaSets, then resume and check that one new ReplicaSet appeared. (b) Switch the strategy to `Recreate` and, by changing the image, show that pods are replaced without old and new running at once.

**Expected result:** (a) one new ReplicaSet after `resume`; (b) all old pods finish before the new ones appear.

<details><summary>Hint</summary>

`kubectl rollout pause|resume deploy/web`, `kubectl set resources deploy/web --requests=cpu=10m`. For `Recreate`: `kubectl patch deploy web -p '{"spec":{"strategy":{"type":"Recreate","rollingUpdate":null}}}'`.

</details>

<details><summary>Solution</summary>

```bash
kubectl -n learn-05 rollout pause deploy/web
kubectl -n learn-05 set image deploy/web nginx=nginx:1.29
kubectl -n learn-05 set resources deploy/web --requests=cpu=10m
kubectl -n learn-05 get rs
kubectl -n learn-05 rollout resume deploy/web
kubectl -n learn-05 rollout status deploy/web
kubectl -n learn-05 get rs

kubectl -n learn-05 patch deploy web -p '{"spec":{"strategy":{"type":"Recreate","rollingUpdate":null}}}'
kubectl -n learn-05 get pods -w &
kubectl -n learn-05 set image deploy/web nginx=nginx:1.28
```

During the pause the ReplicaSet list did not change, after `resume` one ReplicaSet with both changes appeared. With `Recreate` the watch shows all old pods `Terminating` first, then the new ones `Pending` → `ContainerCreating` → `Running`: there is downtime between the versions.

</details>
