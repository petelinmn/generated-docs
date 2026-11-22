---
title: "Exercises: ConfigMap and Secret"
lang: en
track: kubernetes
kind: module
doc: exercises
slug: 07-config-and-secrets
part: 1
tags: [configmap, secret, volume, subpath, immutable, rollout]
status: draft
updated: 2026-11-22
---

# Exercises: ConfigMap and Secret

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. Solutions are under spoilers. You need a kind cluster and a namespace: `kubectl create namespace learn-07`. Clean up afterwards: `kubectl delete namespace learn-07`.

## 1. 🟢 Create and attach

**Task:** create a ConfigMap `app-config` (`MODE=prod`, `TIMEOUT=30`) and a Secret `db` (`username=admin`, `password=S3cr3t!`). Run a pod on `alpine:3.22` that receives `MODE` as `APP_MODE` and the password as `DB_PASSWORD`. Check the variables.

**Expected result:**

```text
APP_MODE=prod
DB_PASSWORD=S3cr3t!
```

<details><summary>Hint</summary>

`env[].valueFrom.configMapKeyRef` and `secretKeyRef`. Check: `kubectl exec app -- env`.

</details>

<details><summary>Solution</summary>

```bash
kubectl -n learn-07 create configmap app-config --from-literal=MODE=prod --from-literal=TIMEOUT=30
kubectl -n learn-07 create secret generic db --from-literal=username=admin --from-literal=password='S3cr3t!'
```

```yaml
apiVersion: v1
kind: Pod
metadata: {name: app}
spec:
  containers:
    - name: c
      image: alpine:3.22
      command: ["sh", "-c", "sleep 3600"]
      env:
        - name: APP_MODE
          valueFrom: {configMapKeyRef: {name: app-config, key: MODE}}
        - name: DB_PASSWORD
          valueFrom: {secretKeyRef: {name: db, key: password}}
```

```text
$ kubectl -n learn-07 exec app -- env | grep -E "^(APP_MODE|DB_PASSWORD)="
APP_MODE=prod
DB_PASSWORD=S3cr3t!
```

</details>

## 2. 🟢 base64 and describe

**Task:** print the Secret `db` as YAML, decode the password and show what `kubectl describe secret` shows. Explain why base64 is not protection.

**Expected result:** `UzNjcjN0IQ==` in the YAML, `S3cr3t!` after decoding, only `7 bytes` in `describe`.

<details><summary>Hint</summary>

`kubectl get secret db -o jsonpath='{.data.password}' | base64 -d`.

</details>

<details><summary>Solution</summary>

```text
$ kubectl -n learn-07 get secret db -o yaml
data:
  password: UzNjcjN0IQ==
  username: YWRtaW4=
$ kubectl -n learn-07 get secret db -o jsonpath='{.data.password}' | base64 -d
S3cr3t!
$ kubectl -n learn-07 describe secret db
password:  7 bytes
username:  5 bytes
```

base64 is a reversible encoding without a key: anyone who reads the object decodes it. `describe` hides values only in its own output.

</details>

## 3. 🟡 Files, permissions and updates

**Task:** attach the ConfigMap `app-config` as a volume at `/etc/cfg`, and the Secret `db` at `/etc/sec` with only the key `password` at the path `nested/pw.txt` with permissions `0400`. Then change `MODE` in the ConfigMap, measure how many seconds it takes for the file `/etc/cfg/MODE` to change, and whether the variable `APP_MODE` changes.

**Expected result:** `-r--------` on `pw.txt`; the file updated in about a minute, the variable did not.

<details><summary>Hint</summary>

`secret.items` and `defaultMode: 0400`. For the measurement — a loop with `sleep 5` and `kubectl exec … cat /etc/cfg/MODE`.

</details>

<details><summary>Solution</summary>

```yaml
volumes:
  - name: cfg
    configMap: {name: app-config}
  - name: sec
    secret:
      secretName: db
      defaultMode: 0400
      items:
        - {key: password, path: nested/pw.txt}
```

```text
$ kubectl -n learn-07 exec modes -- sh -c 'ls -lL /etc/sec/nested; ls /etc/sec'
-r--------    1 root     root             7 Nov 22 18:26 pw.txt
nested
$ kubectl -n learn-07 patch cm app-config -p '{"data":{"MODE":"dev"}}'
volume updated to 'dev' after ~57s
env still: prod
```

The kubelet updates volume files periodically, variables are fixed at container start.

</details>

## 4. 🟡 Missing objects

**Task:** create three pods: one referring to a non-existent ConfigMap, one to a non-existent key of an existing ConfigMap and one with `optional: true`. Explain the statuses and messages, then create the missing ConfigMap and follow the first pod.

**Expected result:** `CreateContainerConfigError`, `CreateContainerConfigError`, `Running`; after the object is created the first pod becomes `Running`.

<details><summary>Hint</summary>

Look for the cause in `kubectl describe pod`. `optional: true` is set in `configMapKeyRef`.

</details>

<details><summary>Solution</summary>

```text
$ kubectl -n learn-07 get pods missing badkey optional
NAME       READY   STATUS                       RESTARTS   AGE
missing    0/1     CreateContainerConfigError   0          16s
badkey     0/1     CreateContainerConfigError   0          15s
optional   1/1     Running                      0          15s
$ kubectl -n learn-07 describe pod missing
  Warning  Failed  kubelet  Error: configmap "nope" not found
$ kubectl -n learn-07 describe pod badkey
  Warning  Failed  kubelet  Error: couldn't find key nokey in ConfigMap learn-07/app-config
$ kubectl -n learn-07 create configmap nope --from-literal=k=v
missing pod: Running after ~20s
```

Without `optional` a missing object or key blocks startup; the pod starts by itself when the object appears.

</details>

## 5. 🔴 Updating a Deployment when a ConfigMap changes

**Task:** run a Deployment `web` that gets variables from a ConfigMap through `envFrom`. Change the ConfigMap and show that no new ReplicaSet appeared. Then make the Deployment pick up the value and explain two more ways.

**Expected result:** before `rollout restart` the number of ReplicaSets is the same and the pod has the old value; after it — the new one.

<details><summary>Hint</summary>

`kubectl set env deploy/web --from=configmap/app-config`, then `kubectl rollout restart deploy/web`.

</details>

<details><summary>Solution</summary>

```bash
kubectl -n learn-07 create deployment web --image=alpine:3.22 -- sleep 3600
kubectl -n learn-07 set env deploy/web --from=configmap/app-config
kubectl -n learn-07 patch cm app-config -p '{"data":{"MODE":"changed"}}'
kubectl -n learn-07 get rs --no-headers | wc -l
kubectl -n learn-07 exec deploy/web -- sh -c 'echo MODE=$MODE'
kubectl -n learn-07 rollout restart deploy/web
kubectl -n learn-07 rollout status deploy/web
```

```text
RS count before=2 after=2
MODE=dev
```

After `rollout restart` and `rollout status` a repeated `exec` check shows `MODE=changed`, and there are 3 ReplicaSets.

The value stayed old (`dev`) because the pod template did not change. `rollout restart` adds an annotation to the template and starts a rolling update. Two more ways: a hash of the ConfigMap content in a template annotation (like `checksum/config` in Helm) and a new ConfigMap name on every change with the reference in the Deployment edited.

</details>
