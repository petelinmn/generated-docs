---
title: "Exercises: troubleshooting"
lang: en
track: kubernetes
kind: module
doc: exercises
slug: 21-troubleshooting
part: 3
tags: [troubleshooting, events, endpoints, kubectl-debug]
status: draft
updated: 2026-11-23
---

# Exercises: troubleshooting

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself; hints and solutions are under spoilers. You need a kind cluster (preferably with two worker nodes) and a namespace: `kubectl create namespace learn-21`. Afterwards: `kubectl delete namespace learn-21` and delete the node debugging pods.

## 1. 🟢 Four broken pods

**Task:** create pods with the image `nginx:no-such-tag`, with `envFrom` for a nonexistent ConfigMap, with the command `/no/such/binary`, and with `limits.memory: 50Mi` that grows a string in a loop. For each find the status, the cause and the source of the cause.

**Expected result:** `ImagePullBackOff`, `CreateContainerConfigError`, `RunContainerError`, `OOMKilled` (code `137`).

<details><summary>Hint</summary>

`kubectl describe pod`, `kubectl get pod -o jsonpath='{.status.containerStatuses[0].state}'` and `…lastState`.

</details>

<details><summary>Solution</summary>

```text
badimg   0/1   ImagePullBackOff             nginx:no-such-tag: not found
nocfg    0/1   CreateContainerConfigError   configmap "missing-cm" not found
badcmd   0/1   RunContainerError            exec: "/no/such/binary": stat /no/such/binary: no such file or directory
oom      0/1   OOMKilled                    exitCode 137
```

</details>

## 2. 🟢 Pending because of a PVC

**Task:** create a PVC with `storageClassName: no-such-class` and a pod that mounts it. Find the cause in the pod and PVC events.

**Expected result:** `pod has unbound immediate PersistentVolumeClaims` and `storageclass … "no-such-class" not found`.

<details><summary>Hint</summary>

`kubectl describe pod` and `kubectl describe pvc`.

</details>

<details><summary>Solution</summary>

```text
Warning  FailedScheduling    default-scheduler  0/3 nodes are available: pod has unbound immediate PersistentVolumeClaims. not found
Warning  ProvisioningFailed  persistentvolume-controller  storageclass.storage.k8s.io "no-such-class" not found
```

</details>

## 3. 🟡 Three Services

**Task:** deploy a Deployment `web` (nginx, 2 replicas) and three Services: with the right selector, with a typo in the selector and with `targetPort: 8080`. Check the endpoints and reachability from a `busybox` pod.

**Expected result:** endpoints `10.244…:80`, `<none>`, `…:8080`; only the first is reachable, the others give `Connection refused`.

<details><summary>Hint</summary>

`kubectl get endpoints`, `kubectl exec client -- wget -q -T 3 -O /dev/null http://<svc>`.

</details>

<details><summary>Solution</summary>

The manifests and output are in the [advanced theory](advanced.en.md#a-service-without-endpoints).

</details>

## 4. 🟡 A probe and a temporary container

**Task:** (a) change the Deployment's readiness probe to `/ready` and find the cause of the stuck rollout. (b) Add a `busybox` container to a running pod with `kubectl debug` and `--target=nginx`, and show the nginx processes and the files of `/etc/nginx`.

**Expected result:** (a) `Readiness probe failed: HTTP probe failed with statuscode: 404`, `rollout status` ends with a timeout, the old pods are `1/1`; (b) `nginx: master process …`, `conf.d fastcgi_params mime.types`.

<details><summary>Hint</summary>

(b) `ls /proc/1/root/etc/nginx`.

</details>

<details><summary>Solution</summary>

The commands and output are in the [advanced theory](advanced.en.md#kubectl-debug-looking-inside).

</details>

## 5. 🔴 A finalizer, a quota and a node

**Task:** (a) create a ConfigMap, add the finalizer `example.com/block`, delete the ConfigMap and find the cause, then remove the finalizer. (b) Create a `ResourceQuota` with `pods: "4"` and a Deployment of 5 replicas; find the cause of `0/5`. (c) Stop the container of a worker node (`docker stop`) and follow the node status, taints and pods; then start it.

**Expected result:** (a) `deletionTimestamp` and `["example.com/block"]`, `NotFound` after removal; (b) `ReplicaFailure`, `exceeded quota`; (c) `NotReady` after about 50 seconds, `unreachable` taints, the pods still `Running`.

<details><summary>Hint</summary>

(c) `kubectl get nodes`, `kubectl get node N -o jsonpath='{.spec.taints}'`.

</details>

<details><summary>Solution</summary>

The output is in the [advanced theory](advanced.en.md#an-object-stuck-in-terminating), [here](advanced.en.md#quota-replicafailure) and [here](advanced.en.md#a-notready-node).

</details>
