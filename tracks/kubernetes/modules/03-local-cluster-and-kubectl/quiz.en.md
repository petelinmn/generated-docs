---
title: "Quiz: local cluster and kubectl"
lang: en
track: kubernetes
kind: module
doc: quiz
slug: 03-local-cluster-and-kubectl
part: 1
tags: [kind, kubectl, kubeconfig, context, apply, diff]
status: draft
updated: 2026-11-22
---

# Quiz: local cluster and kubectl

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What is a node in a kind cluster?

- A) A VirtualBox virtual machine
- B) A Docker container with a kubelet and containerd inside
- C) A cloud server
- D) A host process without isolation

<details><summary>Answer</summary>

**B.** kind runs each node as a Docker container; `docker ps` shows, for example, `learn-control-plane`. Neither a VM nor a cloud is needed for this.
→ [Basics](basics.en.md#create-a-cluster)

</details>

## 2. 🟢 What does a kubeconfig context store?

- A) Only the API server address
- B) A binding "cluster + user + default namespace"
- C) A list of pods
- D) Container images

<details><summary>Answer</summary>

**B.** The address and CA certificate are in `clusters`, credentials in `users`, and the context links them and sets the default namespace. kubeconfig contains neither a pod list nor images.
→ [Basics](basics.en.md#kubeconfig-and-contexts)

</details>

## 3. 🟢 What happens on `kubectl get pods` if you forgot `-n learn-03`?

- A) The command shows pods of all namespaces
- B) The command runs in the context's namespace (`default` by default) and "does not find" your pods
- C) An error: namespace not specified
- D) The command creates the namespace

<details><summary>Answer</summary>

**B.** Without `-n` the namespace from the context is used; if it is not set, `default`. For all at once you need the `-A` flag. The default namespace can be written into the context: `kubectl config set-context kind-learn --namespace=learn-03`.
→ [Basics](basics.en.md#namespace)

</details>

## 4. 🟢 A pod refers to the image `hello:multi`, which exists only in local Docker. What status will you see?

- A) `Running`
- B) `ErrImagePull`
- C) `Completed`
- D) `Pending` with no events

<details><summary>Answer</summary>

**B.** The kubelet of the kind node goes to Docker Hub for the image and gets `pull access denied, repository does not exist`. The image must be loaded with `kind load docker-image hello:multi --name learn`.
→ [Basics](basics.en.md#the-first-manifest)

</details>

## 5. 🟡 What do the two commands in a row print?

```bash
kubectl -n learn-03 create -f pod.yaml
kubectl -n learn-03 create -f pod.yaml
```

- A) `pod/hello created` and `pod/hello unchanged`
- B) `pod/hello created` and `Error from server (AlreadyExists): … pods "hello" already exists`
- C) Two lines `pod/hello created`
- D) Two errors

<details><summary>Answer</summary>

**B.** `create` is not idempotent. Option A describes not `create` but `apply` (the second run of `apply` gives `unchanged`).
→ [Advanced](advanced.en.md#create-and-apply-imperative-and-declarative)

</details>

## 6. 🟡 What does `kubectl create deployment demo --image=nginx:1.29 --dry-run=client -o yaml` do?

- A) Creates a Deployment
- B) Prints a manifest, without contacting the cluster and creating nothing
- C) Validates the Deployment on the server and saves it
- D) Deletes the Deployment `demo`

<details><summary>Answer</summary>

**B.** It is a way to get a manifest draft. The `--dry-run=server` mode differs in that the API server validates the request, but the object is not saved either.
→ [Advanced](advanced.en.md#diff-and-dry-run)

</details>

## 7. 🟡 Select all correct statements

- [ ] A) `kubectl apply` run again with the same file reports `unchanged`
- [ ] B) `kind create cluster` does not change kubectl's current context
- [ ] C) The selector `-l 'tier in (frontend)'` selects objects with the label `tier=frontend`
- [ ] D) An image built in local Docker is automatically available to all kind nodes

<details><summary>Answer</summary>

**A, C.** B is wrong: kind prints `Set kubectl context to "kind-learn"` and switches the context. D is wrong: the image must be loaded with `kind load docker-image`.
→ [Advanced](advanced.en.md)

</details>

## 8. 🟡 How do you get a list of contexts from two kubeconfig files on Windows?

- A) `KUBECONFIG="a.yaml:b.yaml"`
- B) `KUBECONFIG="a.yaml;b.yaml"`
- C) Specify both files separated by a comma
- D) It is impossible

<details><summary>Answer</summary>

**B.** On Windows the separator is a semicolon, on Linux and macOS a colon. The form with a colon on Windows gives `error loading config file … The filename, directory name, or volume label syntax is incorrect`.
→ [Advanced](advanced.en.md#several-clusters-and-kubeconfig)

</details>

## 9. 🔴 Find the flaw

> "I created a multi-node cluster with `kind create cluster --wait 120s` and immediately started a 3-replica Deployment on it. `--wait` guarantees the cluster is ready."

What is wrong in the reasoning and what should have been done?

<details><summary>Answer</summary>

`--wait` waits for the control plane to be ready, not the worker nodes: in the experiment right after creation the workers were `NotReady` (`Ready` after ~15 seconds). Before starting a workload you must wait for the nodes: `kubectl wait --for=condition=Ready node --all --timeout=120s`.
→ [Expert](expert.en.md#gotcha-worker-nodes-are-notready-right-after-kind-create)

</details>

## 10. 🔴 Open question

Explain how `kubectl apply -f`, `kubectl diff -f` and `kubectl apply -f --dry-run=server` differ and how you use them in a safe change process.

<details><summary>Key points of a good answer</summary>

- `apply` changes the cluster; it is idempotent and stores the last applied manifest in the `last-applied-configuration` annotation.
- `diff` shows the difference between the file and the cluster state without changing it; for immutable fields it additionally prints a validation error (in the experiment — `The Pod "hello" is invalid: spec: Forbidden: pod updates may not change fields other than …`).
- `--dry-run=server` sends the request to the API server, which validates it (validation, admission) but does not store it.
- The process: the manifest in Git → `diff` and `dry-run=server` in CI or locally → `apply` → verification (`rollout status`, `get events`).
- Before running — a `kubectl config current-context` check.

</details>

→ [Advanced](advanced.en.md#diff-and-dry-run)
