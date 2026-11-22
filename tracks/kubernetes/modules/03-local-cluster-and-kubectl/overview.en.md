---
title: "Local cluster and kubectl"
lang: en
track: kubernetes
kind: module
doc: overview
slug: 03-local-cluster-and-kubectl
part: 1
levels: [1, 2, 3, 4]
tags: [kind, kubectl, kubeconfig, context, namespace, manifest, declarative]
versions: "kind v0.34 (node image kindest/node:v1.37.0), kubectl 1.36.1, Kubernetes 1.37.0"
status: draft
updated: 2026-11-22
---

# Local cluster and kubectl

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

To learn Kubernetes you do not need to rent a cloud: kind (Kubernetes in Docker) brings up a real cluster of Docker containers in seconds. You talk to the cluster through `kubectl`, a REST API client: it reads the address and credentials from a kubeconfig file, sends requests and shows the responses nicely. The module teaches creating a cluster, switching between clusters and namespaces, and running the basic commands.

## Key ideas

- **kind runs nodes as Docker containers.** The command `kind create cluster --name learn` creates a single-node cluster; a configuration file lets you define several nodes.
- **kubeconfig stores clusters, users and contexts.** A context is a triple "cluster + user + default namespace"; `kubectl config use-context` switches it.
- **Always know which cluster a command goes to.** `kind create cluster` itself switches the current context to the new cluster.
- **Two styles of work:** imperative (`kubectl create`, `run`) and declarative (`kubectl apply -f manifest.yaml`). The declarative one is repeatable: `apply` can be run many times.
- **Output is controlled by flags:** `-o wide`, `-o yaml`, `-o jsonpath=…`, `-o custom-columns=…`; the description of any field is `kubectl explain`.
- **Images from local Docker are not visible to the cluster.** They are loaded with `kind load docker-image` and used with an explicit tag, not `latest`.
- **Isolate experiments with namespaces** (`learn-NN`) and delete them whole: `kubectl delete namespace learn-03`.

## Minimal example

```bash
kind create cluster --name learn
kubectl get nodes
kubectl apply -f pod.yaml
kubectl get pods
kubectl logs hello
kubectl delete -f pod.yaml
```

```text
NAME                  STATUS   ROLES           AGE   VERSION
learn-control-plane   Ready    control-plane   22s   v1.37.0
```

The `pod.yaml` manifest and the output of the other commands are in [basics](basics.en.md).

## When to use and when not

- ✅ kind — for learning, checking manifests and CI tests: the cluster is disposable.
- ✅ `kubectl apply -f` — for everything that must be reproducible and stored in Git.
- ✅ `kubectl create`, `run`, `--dry-run=client -o yaml` — to quickly generate a manifest draft.
- ❌ kind is not suitable for production or load testing: there is no real HA, the nodes are containers on one machine.
- ❌ Do not edit a production cluster with imperative commands without recording in Git: the changes cannot be reproduced.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | installing kind and kubectl, creating a cluster, kubeconfig and contexts, namespaces, a pod manifest, get/describe/logs/port-forward/delete |
| [2. Advanced](advanced.en.md) | `create` and `apply`, `diff`, `--dry-run`, `explain`, output formats, selectors, several clusters and `KUBECONFIG`, multi-node kind, loading images |
| [3. Expert](expert.en.md) | kubeconfig structure, how kubectl talks to the API, plugins, a reference of commands and output formats, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) with solutions |
| 5. Runnable examples | not generated yet — `/learn kubernetes 03-local-cluster-and-kubectl level 5` |
| 6. Tests | not generated yet — `/learn kubernetes 03-local-cluster-and-kubectl level 6` |

## Related materials

- Prerequisites: [01 · Containers and Docker](../01-containers/overview.en.md), [02 · Kubernetes architecture](../02-architecture/overview.en.md)
- Next: [04 · Pod](../04-pods/overview.en.md)
- Foundation: the [Linux track](../../../linux/overview.en.md) — the shell and working with WSL2
