---
title: "Quiz: Pod"
lang: en
track: kubernetes
kind: module
doc: quiz
slug: 04-pods
part: 1
tags: [pod, restart-policy, init-container, sidecar, termination, qos]
status: draft
updated: 2026-11-22
---

# Quiz: Pod

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What does the policy `restartPolicy: Always` do to a container that exited with code 0?

- A) Nothing, the pod becomes `Succeeded`
- B) Restarts it
- C) Deletes the pod
- D) Marks the pod `Failed`

<details><summary>Answer</summary>

**B.** `Always` restarts a container on any exit, even a successful one. One-off tasks need `Never` or `OnFailure`: the `once` pod with `Never` became `Completed` (phase `Succeeded`).
→ [Basics](basics.en.md#restartpolicy-what-happens-when-a-container-ends)

</details>

## 2. 🟢 Which of these is a pod phase (`status.phase`)?

- A) `CrashLoopBackOff`
- B) `Completed`
- C) `Running`
- D) `ContainerCreating`

<details><summary>Answer</summary>

**C.** The phases are `Pending`, `Running`, `Succeeded`, `Failed`, `Unknown`. `CrashLoopBackOff`, `Completed` and `ContainerCreating` are values of the `STATUS` column.
→ [Basics](basics.en.md#phases-and-statuses)

</details>

## 3. 🟢 Two programs must run in a pod and talk through `localhost`. What is true?

- A) They must be run in two different pods
- B) They can run as two containers of one pod: they share an IP and `localhost`
- C) It is impossible, `localhost` differs for each container
- D) A Service is needed

<details><summary>Answer</summary>

**B.** Containers of a pod share the network space: in the experiment `hostname -i` in two containers gave the same `10.244.0.43`, and `wget http://localhost:80` from the `probe` container reached nginx.
→ [Advanced](advanced.en.md#several-containers-in-a-pod)

</details>

## 4. 🟢 What does `Init:1/2` mean?

- A) One of two main containers is running
- B) One of two init containers is done
- C) One init container failed
- D) The pod is being deleted

<details><summary>Answer</summary>

**B.** Init containers run one after another; `Init:N/M` means N of M have finished. After the last one the pod becomes `Running`.
→ [Advanced](advanced.en.md#init-containers)

</details>

## 5. 🟡 How long does `kubectl delete pod` take for a pod whose PID 1 is `sleep 3600` without a signal handler?

- A) About a second
- B) About 30 seconds (the default `terminationGracePeriodSeconds`)
- C) About 5 minutes
- D) The deletion hangs forever

<details><summary>Answer</summary>

**B.** `SIGTERM` is ignored, and after 30 seconds `SIGKILL` arrives (32 s in the experiment). A pod with a `trap` handler was deleted in 1 s, and with `terminationGracePeriodSeconds: 5` in 6 s.
→ [Advanced](advanced.en.md#pod-termination)

</details>

## 6. 🟡 What does `kubectl logs multi` print for a pod with containers `web`, `writer`, `probe`?

- A) The logs of all three containers
- B) An error: a container must be specified
- C) The `web` logs and the line `Defaulted container "web" out of: web, writer, probe`
- D) Empty output

<details><summary>Answer</summary>

**C.** Without `-c` kubectl picks the default container (the first one, or the one set by an annotation) and says so. For all at once use `--all-containers`.
→ [Advanced](advanced.en.md#logs-and-choosing-a-container)

</details>

## 7. 🟡 Select all correct statements

- [ ] A) A native sidecar is an init container with `restartPolicy: Always`
- [ ] B) An `emptyDir` volume survives pod recreation
- [ ] C) The `preStop` hook runs before `SIGTERM`
- [ ] D) A pod's containers see each other's processes by default

<details><summary>Answer</summary>

**A, C.** B is wrong: `emptyDir` lives with the pod. D is wrong: a shared process list is enabled by `shareProcessNamespace: true`; by default processes are separate (in the experiment `ps` in `probe` showed only its own).
→ [Advanced](advanced.en.md)

</details>

## 8. 🟡 A pod is in `CrashLoopBackOff`. What to do first?

- A) Delete and recreate the pod
- B) Look at `kubectl logs` and the exit code (`lastState.terminated.exitCode`), then `describe`
- C) Increase `terminationGracePeriodSeconds`
- D) Add `restartPolicy: Never`

<details><summary>Answer</summary>

**B.** `CrashLoopBackOff` is only a wait between restarts; the cause is in the application or its configuration. Recreating does not help (A), `Never` only hides the restarts (D). The log of the previous run (`--previous`) may already be unavailable, as in our experiment.
→ [Basics](basics.en.md#crashloopbackoff)

</details>

## 9. 🔴 Find the flaw

> "The pod shows `Running` and `1/1`, so the application works and accepts requests."

<details><summary>Answer</summary>

`Running` is only the pod phase: the container is started. Readiness for traffic is determined by the `Ready` condition, which without a readiness probe means "the container is started", not "the application is healthy". The application may be broken or not warmed up. Readiness and liveness probes are needed (module 11).
→ [Expert](expert.en.md#gotcha-running-does-not-mean-it-works)

</details>

## 10. 🔴 Open question

A container exited with code 137 and the reason `OOMKilled`. Explain what it means, how to reproduce it and what to change.

<details><summary>Key points of a good answer</summary>

- 137 = 128 + 9: the process was killed by `SIGKILL`; the reason `OOMKilled` — the container exceeded its memory limit (`limits.memory`) and the kernel (the OOM killer) killed it.
- Reproduction: a pod with `limits.memory: 20Mi` and the command `tail /dev/zero`; in the experiment `reason=OOMKilled exit=137`, QoS `Burstable`.
- What to change: raise the limit or reduce the application's memory use; look at real consumption (`kubectl top`, metrics, modules 11 and 16).
- Do not confuse it with a timeout stop: there it is also `SIGKILL`, but the reason differs (`terminationGracePeriodSeconds` expired).

</details>

→ [Expert](expert.en.md#exit-codes)
