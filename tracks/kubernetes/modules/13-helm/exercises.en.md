---
title: "Exercises: Helm"
lang: en
track: kubernetes
kind: module
doc: exercises
slug: 13-helm
part: 2
tags: [helm, chart, values, release]
status: draft
updated: 2026-11-23
---

# Exercises: Helm

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself; hints and solutions are under spoilers. You need Helm 4 and a kind cluster; namespace: `kubectl create namespace learn-13`. Clean up afterwards: `kubectl delete namespace learn-13`. The chart `mini` is from [basic theory](basics.en.md#chart-structure).

## 1. 🟢 Your own chart and rendering

**Task:** create the chart `mini` (a Deployment and a Service from basic theory), check `helm lint` and render it with `--set env.A=1`.

**Expected result:** `0 chart(s) failed`, the manifest contains `name: A`, `value: "1"` and the image `nginx:1.29`.

<details><summary>Hint</summary>

The image: `{{ .Values.image.tag | default .Chart.AppVersion }}`; env via `range $k, $v := .Values.env` and `quote`.

</details>

<details><summary>Solution</summary>

```text
$ helm lint mini
1 chart(s) linted, 0 chart(s) failed
$ helm template rel mini --set env.A=1
          image: "nginx:1.29"
          env:
            - name: A
              value: "1"
```

</details>

## 2. 🟢 Install, upgrade, rollback

**Task:** install the release `web`, upgrade to 3 replicas and image `1.28`, roll back to revision 1. Show the history.

**Expected result:** three revisions: `superseded`, `superseded`, `deployed (Rollback to 1)`; in the cluster 2 replicas and `nginx:1.29`.

<details><summary>Hint</summary>

`helm install web mini -n learn-13 --wait`, `helm upgrade … --set replicaCount=3 --set image.tag=1.28`, `helm rollback web 1`.

</details>

<details><summary>Solution</summary>

```text
$ helm history web -n learn-13
1  superseded  Install complete
2  superseded  Upgrade complete
3  deployed    Rollback to 1
$ kubectl -n learn-13 get deploy web-mini -o jsonpath='{.spec.replicas} {.spec.template.spec.containers[0].image}'
2 nginx:1.29
```

</details>

## 3. 🟡 Losing values on upgrade

**Task:** run two `helm upgrade` in a row with different `--set` (`replicaCount=4`, then `image.tag=1.28`) and look at `helm get values`. Then run `--reuse-values --set env.X=1`.

**Expected result:** after the second call `replicaCount` is gone; after `--reuse-values` `image.tag` and `env.X` remain.

<details><summary>Hint</summary>

`helm get values web -n learn-13`.

</details>

<details><summary>Solution</summary>

```text
USER-SUPPLIED VALUES:
image:
  tag: "1.28"
# after --reuse-values --set env.X=1
USER-SUPPLIED VALUES:
env:
  X: 1
image:
  tag: "1.28"
```

</details>

## 4. 🟡 Input protection and rollback on failure

**Task:** add a `values.schema.json` with `replicaCount` as an integer of at least 1 and make sure `replicaCount=0` is rejected. Then run an upgrade with a non-existent image, first with `--wait --timeout 30s`, then with `--atomic`, and compare the history.

**Expected result:** the error `minimum: got 0, want 1`; a `failed` revision without a rollback in the first case and `Rollback to N` in the second.

<details><summary>Hint</summary>

`helm history web -n learn-13`.

</details>

<details><summary>Solution</summary>

```text
Error: INSTALLATION FAILED: values don't meet the specifications of the schema(s) …
- at '/replicaCount': minimum: got 0, want 1
7  failed    Upgrade "web" failed: resource Deployment/learn-13/web-mini not ready…
8  failed    Upgrade "web" failed…
9  deployed  Rollback to 6
```

</details>

## 5. 🔴 A migration hook and a server-side apply conflict

**Task:** (a) add a `pre-install,pre-upgrade` hook with a `sleep 3` Job and the delete policy `before-hook-creation,hook-succeeded`; during an install with `--wait` watch the Job and the Deployment. (b) Run `kubectl scale deploy web-mini --replicas=9`, then `helm upgrade --reuse-values` and after the error — with `--force-conflicts`.

**Expected result:** (a) the Job runs until the Deployment appears and then disappears; (b) `conflict with "kubectl.exe" with subresource "scale"`, then `replicas=2`.

<details><summary>Hint</summary>

(a) a loop `kubectl get jobs,deploy` once per second. (b) `--force-conflicts` on `helm upgrade`.

</details>

<details><summary>Solution</summary>

```text
t=1 jobs=hk2-mini-migrate:Running  deploy=
t=5 jobs=                          deploy=hk2-mini:0/2
t=6 jobs=                          deploy=hk2-mini:2/2
Error: UPGRADE FAILED: conflict occurred while applying object … conflict with "kubectl.exe" with subresource "scale" using apps/v1: .spec.replicas
replicas=2 manual=yes
```

</details>
