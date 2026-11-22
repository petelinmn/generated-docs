---
title: "Exercises: Pod"
lang: en
track: kubernetes
kind: module
doc: exercises
slug: 04-pods
part: 1
tags: [pod, restart-policy, init-container, sidecar, termination, emptydir]
status: draft
updated: 2026-11-22
---

# Exercises: Pod

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. Solutions are under spoilers. You need a kind cluster and a namespace: `kubectl create namespace learn-04`. Clean up afterwards: `kubectl delete namespace learn-04`.

## 1. 🟢 Three restart policies

**Task:** run three pods on `alpine:3.22`: one with `restartPolicy: Never` and a command that ends with code 0; the second with `Never` and code 3; the third with the default policy and code 1. After a minute explain the status, phase and restart counter of each.

**Expected result:**

```text
NAME        READY   STATUS      RESTARTS      AGE
once        0/1     Completed   0             45s
crash       0/1     Error       2 (34s ago)   45s
failnever   0/1     Error       0             45s
```

<details><summary>Hint</summary>

The phase is in `status.phase`, the termination reason in `status.containerStatuses[0].state.terminated`. The exit code is set by `sh -c "exit N"`.

</details>

<details><summary>Solution</summary>

```yaml
apiVersion: v1
kind: Pod
metadata: {name: once}
spec:
  restartPolicy: Never
  containers:
    - {name: c, image: alpine:3.22, command: ["sh", "-c", "echo done; exit 0"]}
---
apiVersion: v1
kind: Pod
metadata: {name: crash}
spec:
  containers:
    - {name: c, image: alpine:3.22, command: ["sh", "-c", "echo boom; exit 1"]}
---
apiVersion: v1
kind: Pod
metadata: {name: failnever}
spec:
  restartPolicy: Never
  containers:
    - {name: c, image: alpine:3.22, command: ["sh", "-c", "exit 3"]}
```

```text
$ kubectl -n learn-04 get pod failnever -o jsonpath='{.status.phase} reason={.status.containerStatuses[0].state.terminated.reason} exit={.status.containerStatuses[0].state.terminated.exitCode}'
Failed reason=Error exit=3
$ kubectl -n learn-04 get pod once -o jsonpath='{.status.phase} reason={.status.containerStatuses[0].state.terminated.reason}'
Succeeded reason=Completed
```

`once` finished successfully and is not restarted (`Succeeded`), `failnever` finished with an error, without a restart (`Failed`), `crash` with the `Always` policy restarts again and again, and `RESTARTS` grows.

</details>

## 2. 🟢 A shared `localhost` and a volume

**Task:** create a pod of two containers: `web` (nginx, serves `/usr/share/nginx/html`) and `writer` (alpine, writes the time into `index.html` every 2 seconds). The shared volume is an `emptyDir`. From a third container `probe` check that the page is reachable through `localhost` and that all containers have one IP.

**Expected result:** the page returns the current time, `hostname -i` is the same.

<details><summary>Hint</summary>

`emptyDir` is mounted into both containers at different paths. Run `wget -qO- http://localhost:80` through `kubectl exec -c probe`.

</details>

<details><summary>Solution</summary>

```yaml
apiVersion: v1
kind: Pod
metadata: {name: multi}
spec:
  volumes:
    - {name: shared, emptyDir: {}}
  containers:
    - name: web
      image: nginx:1.29
      volumeMounts: [{name: shared, mountPath: /usr/share/nginx/html}]
    - name: writer
      image: alpine:3.22
      command: ["sh", "-c", "while true; do date +%T > /data/index.html; sleep 2; done"]
      volumeMounts: [{name: shared, mountPath: /data}]
    - {name: probe, image: alpine:3.22, command: ["sh", "-c", "sleep 3600"]}
```

```text
$ kubectl -n learn-04 exec multi -c probe -- wget -qO- http://localhost:80
16:26:43
$ kubectl -n learn-04 exec multi -c probe -- hostname -i
10.244.0.43
$ kubectl -n learn-04 exec multi -c web -- hostname -i
10.244.0.43
```

The containers share the pod network, so `localhost` is common to them, and the shared `emptyDir` serves as the link between `writer` and `web`.

</details>

## 3. 🟡 Init containers

**Task:** create a pod with two init containers (`init-a` prints text and waits 6 seconds, `init-b` — 3 seconds) and a main container. Follow the statuses. Then create a second pod where an init container ends with an error and explain the status.

**Expected result:** `Init:0/2` → `Init:1/2` → `Running`; for the second pod `Init:Error`.

<details><summary>Hint</summary>

`kubectl get pod -w` shows the status changes. Init container logs: `kubectl logs <pod> -c <name>`.

</details>

<details><summary>Solution</summary>

```yaml
apiVersion: v1
kind: Pod
metadata: {name: init-demo}
spec:
  initContainers:
    - {name: wait-a, image: alpine:3.22, command: ["sh", "-c", "echo init-a; sleep 6"]}
    - {name: wait-b, image: alpine:3.22, command: ["sh", "-c", "echo init-b; sleep 3"]}
  containers:
    - {name: app, image: alpine:3.22, command: ["sh", "-c", "echo app-started; sleep 3600"]}
```

```text
$ kubectl -n learn-04 get pod init-demo -w
0/1 Init:0/2
0/1 Init:1/2
1/1 Running
```

The second pod (`command: ["sh","-c","echo failing; exit 1"]` in the init container) stays `Init:Error`, and `RESTARTS` grows: the main container does not start until the init container finishes successfully.

</details>

## 4. 🟡 Deletion speed

**Task:** create two pods on `alpine:3.22`: one with `sleep 3600` and one with `sh -c "trap 'echo got-SIGTERM; exit 0' TERM; sleep 3600 & wait"`. Measure the `kubectl delete pod` time for each. Then cut the wait of the first to 5 seconds and measure again.

**Expected result:** about 30 s, about 1 s and about 5 s.

<details><summary>Hint</summary>

`terminationGracePeriodSeconds` is set in the pod's `spec`. Measure time with `date +%s` before and after `delete`.

</details>

<details><summary>Solution</summary>

```yaml
apiVersion: v1
kind: Pod
metadata: {name: term-ignore}
spec:
  containers:
    - {name: c, image: alpine:3.22, command: ["sleep", "3600"]}
---
apiVersion: v1
kind: Pod
metadata: {name: term-trap}
spec:
  containers:
    - {name: c, image: alpine:3.22, command: ["sh", "-c", "trap 'echo got-SIGTERM; exit 0' TERM; sleep 3600 & wait"]}
---
apiVersion: v1
kind: Pod
metadata: {name: term-short}
spec:
  terminationGracePeriodSeconds: 5
  containers:
    - {name: c, image: alpine:3.22, command: ["sleep", "3600"]}
```

```text
term-ignore deleted in 32s
term-trap deleted in 1s
term-short deleted in 6s
```

`sleep` as PID 1 ignores `SIGTERM`, and `SIGKILL` arrives after 30 seconds; a signal handler ends the process at once; a shortened `terminationGracePeriodSeconds` limits the wait.

</details>

## 5. 🔴 A native sidecar and OOMKilled

**Task:** (a) create a pod with a native sidecar (`initContainers` with `restartPolicy: Always` that prints a line every 5 seconds) and a main container, check `READY` and the sidecar logs. (b) Create a pod with `limits.memory: 20Mi` and the command `tail /dev/zero`, `restartPolicy: Never`, find the reason, the exit code and the QoS class.

**Expected result:** (a) `2/2 Running`, `sidecar-alive`; (b) `reason=OOMKilled exit=137 qos=Burstable`.

<details><summary>Hint</summary>

(a) A sidecar is an element of the `initContainers` list with the field `restartPolicy: Always`. (b) `tail /dev/zero` reads an endless stream and accumulates it in memory.

</details>

<details><summary>Solution</summary>

```yaml
apiVersion: v1
kind: Pod
metadata: {name: sidecar-demo}
spec:
  initContainers:
    - name: logger
      image: alpine:3.22
      restartPolicy: Always
      command: ["sh", "-c", "while true; do echo sidecar-alive; sleep 5; done"]
  containers:
    - {name: app, image: alpine:3.22, command: ["sh", "-c", "echo app-up; sleep 3600"]}
---
apiVersion: v1
kind: Pod
metadata: {name: oom}
spec:
  restartPolicy: Never
  containers:
    - name: c
      image: alpine:3.22
      command: ["sh", "-c", "tail /dev/zero"]
      resources:
        limits: {memory: 20Mi}
```

```text
$ kubectl -n learn-04 get pod sidecar-demo
NAME           READY   STATUS    RESTARTS   AGE
sidecar-demo   2/2     Running   0          1s
$ kubectl -n learn-04 logs sidecar-demo -c logger
sidecar-alive
$ kubectl -n learn-04 get pod oom -o jsonpath='phase={.status.phase} reason={.status.containerStatuses[0].state.terminated.reason} exit={.status.containerStatuses[0].state.terminated.exitCode} qos={.status.qosClass}'
phase=Failed reason=OOMKilled exit=137 qos=Burstable
```

The sidecar started before the main container and does not finish, so the pod is `2/2 Running`. A container that exceeded the limit is killed by `SIGKILL` (137 = 128 + 9) with the reason `OOMKilled`; the class is `Burstable` because only a memory limit is set.

</details>
