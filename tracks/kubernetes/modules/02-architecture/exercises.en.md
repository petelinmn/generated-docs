---
title: "Exercises: Kubernetes architecture"
lang: en
track: kubernetes
kind: module
doc: exercises
slug: 02-architecture
part: 1
tags: [kubernetes, architecture, controller, scheduler, etcd, finalizers]
status: draft
updated: 2026-11-22
---

# Exercises: Kubernetes architecture

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. Solutions are under spoilers. You need a kind cluster (`kind create cluster --name learn`) and a namespace for experiments: `kubectl create namespace learn-02`. Clean up afterwards: `kubectl delete namespace learn-02`.

## 1. 🟢 Read the event chain

**Task:** create a Deployment `web` with the image `nginx:1.29` and 2 replicas and, from the events, name the component that performed each step: creating the ReplicaSet, creating the pods, choosing a node, starting the container.

**Expected result:** the output has the sources `deployment-controller`, `replicaset-controller`, `default-scheduler`, `kubelet`.

<details><summary>Hint</summary>

`kubectl get events` can print selected fields through `-o custom-columns`. The source field is `.source.component`.

</details>

<details><summary>Solution</summary>

```bash
kubectl create namespace learn-02
kubectl -n learn-02 create deployment web --image=nginx:1.29 --replicas=2
kubectl -n learn-02 rollout status deploy/web
kubectl -n learn-02 get events -o custom-columns=REASON:.reason,SOURCE:.source.component,MSG:.message
```

```text
REASON              SOURCE                  MSG
Scheduled           default-scheduler       Successfully assigned learn-02/web-… to learn-control-plane
Pulling             kubelet                 Pulling image "nginx:1.29"
ScalingReplicaSet   deployment-controller   Scaled up replica set web-689b9bdbdd from 0 to 2
SuccessfulCreate    replicaset-controller   Created pod: web-689b9bdbdd-24sgv
```

The `deployment-controller` created the ReplicaSet, the `replicaset-controller` the pods, the `default-scheduler` chose a node, the `kubelet` pulled the image. The line order in the output may differ: the timestamp stores only seconds.

</details>

## 2. 🟢 Find the owners

**Task:** for each pod and for the ReplicaSet print the owner kind. Then delete the Deployment and check that the ReplicaSet and pods disappear too.

**Expected result:**

```text
NAME                   OWNER
web-689b9bdbdd-24sgv   ReplicaSet
web-689b9bdbdd-7hpbd   ReplicaSet
NAME             OWNER
web-689b9bdbdd   Deployment
```

<details><summary>Hint</summary>

Owners are recorded in `metadata.ownerReferences`. Use `-o custom-columns` with the path `.metadata.ownerReferences[0].kind`.

</details>

<details><summary>Solution</summary>

```bash
kubectl -n learn-02 get pods -o custom-columns=NAME:.metadata.name,OWNER:.metadata.ownerReferences[0].kind
kubectl -n learn-02 get rs   -o custom-columns=NAME:.metadata.name,OWNER:.metadata.ownerReferences[0].kind
kubectl -n learn-02 delete deployment web
kubectl -n learn-02 get rs,pods
```

After the Deployment is deleted the garbage collector deletes the ReplicaSet and pods (the last command prints `No resources found`; finishing may take a few seconds).

</details>

## 3. 🟡 Stop the scheduler

**Task:** in kind temporarily stop the scheduler, raise `replicas` and make sure the new pod stays `Pending` with no `FailedScheduling` event. Bring the scheduler back and check that the pod started.

**Expected result:** one pod `Pending`, `Running` after the return.

<details><summary>Hint</summary>

The scheduler in kind is a static pod: it is enough to move its manifest out of `/etc/kubernetes/manifests` inside the node. Open the node through `docker exec learn-control-plane`. **Only in a disposable kind cluster.**

</details>

<details><summary>Solution</summary>

```bash
docker exec learn-control-plane mv /etc/kubernetes/manifests/kube-scheduler.yaml /tmp/
sleep 20
kubectl -n learn-02 scale deploy/web --replicas=3
kubectl -n learn-02 get pods
kubectl -n learn-02 get events --field-selector reason=FailedScheduling
docker exec learn-control-plane mv /tmp/kube-scheduler.yaml /etc/kubernetes/manifests/
sleep 25
kubectl -n learn-02 get pods
```

```text
web-689b9bdbdd-7hpbd 1/1 Running
web-689b9bdbdd-jcc7k 0/1 Pending
web-689b9bdbdd-r4tkv 1/1 Running
No resources found in learn-02 namespace.
```

Without a scheduler the pod gets no node. There are no `FailedScheduling` events: the scheduler itself emits them. After the manifest was returned the pod became `Running`.

</details>

## 4. 🟡 Version conflict

**Task:** reproduce the `Conflict` error: read the Deployment as JSON, replace `resourceVersion` with `"1"` and run `kubectl replace`. Explain why the server rejected the request and what the client should do.

**Expected result:** `Error from server (Conflict): … the object has been modified; please apply your changes to the latest version and try again`.

<details><summary>Hint</summary>

The record version is `metadata.resourceVersion`. Replace it with `sed` before `kubectl replace -f -`.

</details>

<details><summary>Solution</summary>

```bash
kubectl -n learn-02 get deploy web -o jsonpath='{.metadata.resourceVersion}{"\n"}'
kubectl -n learn-02 get deploy web -o json \
  | sed 's/"resourceVersion": "[0-9]*"/"resourceVersion": "1"/' \
  | kubectl replace -f -
```

```text
Error from server (Conflict): error when replacing "STDIN": Operation cannot be fulfilled on deployments.apps "web": the object has been modified; please apply your changes to the latest version and try again
```

The client sent a stale version, and the server protected against a lost update. The correct behavior: re-read the object, apply the change to the fresh version and repeat the request.

</details>

## 5. 🔴 Finalizer and Terminating

**Task:** create a ConfigMap `fin`, add the finalizer `example.com/block` to it, delete it and show that the object stayed with a `deletionTimestamp`. Then remove the finalizer so the object disappears.

**Expected result:**

```text
2026-11-22T16:02:41Z ["example.com/block"]
Error from server (NotFound): configmaps "fin" not found
```

(your time will differ)

<details><summary>Hint</summary>

`kubectl patch -p` can change `metadata.finalizers`. To delete without waiting: `--wait=false`; to remove the finalizer, set it to `null`.

</details>

<details><summary>Solution</summary>

```bash
kubectl -n learn-02 create configmap fin --from-literal=a=b
kubectl -n learn-02 patch configmap fin -p '{"metadata":{"finalizers":["example.com/block"]}}'
kubectl -n learn-02 delete configmap fin --wait=false
kubectl -n learn-02 get configmap fin -o jsonpath='{.metadata.deletionTimestamp} {.metadata.finalizers}{"\n"}'
kubectl -n learn-02 patch configmap fin -p '{"metadata":{"finalizers":null}}'
kubectl -n learn-02 get configmap fin
```

`delete` only sets `deletionTimestamp`; while the finalizer is in place the object stays. After the finalizer was removed the server deleted the object at once. In real life a controller removes the finalizer after cleaning up external resources; removing it by hand is acceptable only when no cleanup is needed.

</details>
