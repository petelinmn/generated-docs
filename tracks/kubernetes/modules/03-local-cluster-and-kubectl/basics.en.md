---
title: "Local cluster and kubectl: basics"
lang: en
track: kubernetes
kind: module
doc: basics
slug: 03-local-cluster-and-kubectl
part: 1
tags: [kind, kubectl, kubeconfig, context, namespace, manifest, pod, port-forward]
versions: "kind v0.34 (node image kindest/node:v1.37.0), kubectl 1.36.1, Kubernetes 1.37.0"
status: draft
updated: 2026-11-22
---

# Local cluster and kubectl: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [01 · Containers and Docker](../01-containers/overview.en.md), [02 · Kubernetes architecture](../02-architecture/overview.en.md) · **Time:** ~2 h

## Why it matters

To try Kubernetes you need a cluster that you do not mind breaking. kind starts it on your machine: each node is a Docker container with a kubelet and containerd inside. You interact through `kubectl`.

## Installation

You need Docker, `kind` and `kubectl` (in the track, inside WSL2, see the [environment](../../overview.en.md#environment)).

```text
$ docker version --format 'client {{.Client.Version}} server {{.Server.Version}}'
client 29.6.1 server 29.6.1
$ kubectl version --client
Client Version: v1.36.1
$ kind version
kind v0.34.0-alpha+4530a2a5af91cd go1.27.4 windows/amd64
```

The binaries and installation methods are on the [kind](https://kind.sigs.k8s.io/docs/user/quick-start/#installation) and [kubectl](https://kubernetes.io/docs/tasks/tools/) pages. The kubectl version must differ from the cluster version by at most one minor version (see the [expert theory](expert.en.md#versions-and-compatibility)).

## Create a cluster

```text
$ kind create cluster --name learn
 ✓ Starting control-plane 🕹️
 ✓ Installing CNI 🔌
 ✓ Installing StorageClass 💾
 ✓ Waiting ≤ 3m0s for control-plane = Ready ⏳
 • Ready after 16s 💚
Set kubectl context to "kind-learn"
$ kubectl get nodes -o wide
NAME                  STATUS   ROLES           AGE   VERSION   INTERNAL-IP   OS-IMAGE                       CONTAINER-RUNTIME
learn-control-plane   Ready    control-plane   22s   v1.37.0   172.18.0.9    Debian GNU/Linux 13 (trixie)   containerd://2.3.4
```

The cluster name is `learn`, the node name is `learn-control-plane`. The node is a Docker container: `docker ps` shows `learn-control-plane`.

Listing and deleting clusters:

```bash
kind get clusters
kind delete cluster --name learn
```

## kubeconfig and contexts

`kubectl` takes the API server address and credentials from a kubeconfig file (`~/.kube/config` by default). It has three lists:

| Section | What it stores |
|---|---|
| `clusters` | the API server address and the certificate authority certificate |
| `users` | credentials (a certificate, a token) |
| `contexts` | a binding "cluster + user + default namespace" |

A context is where and as whom kubectl acts right now:

```text
$ kubectl config get-contexts
CURRENT   NAME             CLUSTER          AUTHINFO         NAMESPACE
          docker-desktop   docker-desktop   docker-desktop
*         kind-learn       kind-learn       kind-learn
$ kubectl config use-context docker-desktop
Switched to context "docker-desktop".
```

> ⚠️ `kind create cluster` switches the current context to the new cluster. Before running a dangerous command (`delete`, `apply`), check `kubectl config current-context`.

To run one command in another context without switching, use the `--context` flag: `kubectl --context kind-learn get nodes`.

## Namespace

A namespace is a "folder" for cluster objects. We create one per module:

```bash
kubectl create namespace learn-03
kubectl -n learn-03 get pods            # -n selects the namespace for one command
```

If typing `-n` every time is tedious, the default namespace can be written into the context:

```text
$ kubectl config set-context kind-learn --namespace=learn-03
Context "kind-learn" modified.
$ kubectl get pods
```

Restore the default: `kubectl config set-context kind-learn --namespace=`. Clean up: `kubectl delete namespace learn-03` — deletes everything inside.

## The first manifest

A manifest is a YAML description of an object. A minimal Pod:

```yaml
apiVersion: v1
kind: Pod
metadata:
  name: hello
  labels:
    app: hello
spec:
  containers:
    - name: app
      image: hello:multi
      imagePullPolicy: IfNotPresent
      ports:
        - containerPort: 8080
```

Required fields of any object: `apiVersion`, `kind`, `metadata` (with `name`), and for most also `spec`. The `hello:multi` image is from [module 01](../01-containers/basics.en.md) (it lies locally in Docker). The cluster does not see it until we load it:

```text
$ kubectl -n learn-03 apply -f pod.yaml
pod/hello created
$ kubectl -n learn-03 get pod hello
NAME    READY   STATUS         RESTARTS   AGE
hello   0/1     ErrImagePull   0          12s
$ kubectl -n learn-03 describe pod hello
 …
 Warning  Failed  kubelet  Failed to pull image "hello:multi": failed to resolve reference "docker.io/library/hello:multi": pull access denied, repository does not exist or may require authorization
```

The kubelet went to Docker Hub for the image, where there is none. Let us load the image onto the node and recreate the pod:

```text
$ kind load docker-image hello:multi --name learn
$ kubectl -n learn-03 delete pod hello
$ kubectl -n learn-03 apply -f pod.yaml
$ kubectl -n learn-03 wait --for=condition=Ready pod/hello --timeout=60s
pod/hello condition met
```

## Basic commands

| Command | What it does |
|---|---|
| `kubectl get <type> [name]` | a list of objects; `-o wide` — more columns |
| `kubectl describe <type> <name>` | details and events of an object |
| `kubectl logs <pod>` | logs (the container's stdout) |
| `kubectl exec <pod> -- <command>` | run a command in a container |
| `kubectl port-forward pod/<name> port:port` | forward a port to your machine |
| `kubectl apply -f file.yaml` | create or update an object from a manifest |
| `kubectl delete -f file.yaml` / `delete <type> <name>` | delete |

Examples on our pod:

```text
$ kubectl -n learn-03 logs hello
listening on :8080
$ kubectl -n learn-03 port-forward pod/hello 18090:8080 &
$ curl -s localhost:18090
Hello from hello
```

In the reply the host name is `hello`: in Kubernetes a container's hostname equals the pod name. Port 8080 inside the pod appeared on `localhost:18090` of our machine.

```text
$ kubectl -n learn-03 get pod hello -o wide
NAME    READY   STATUS    IP            NODE
hello   1/1     Running   10.244.0.18   learn-control-plane
```

The `hello:multi` image is built on `scratch` and has no `sh`, so `kubectl exec` behaves like `docker exec` ([module 01](../01-containers/expert.en.md#gotcha-a-scratch-image-has-no-shell)):

```text
$ kubectl -n learn-03 exec hello -- sh
error: Internal error occurred: … OCI runtime exec failed: exec failed: unable to start container process: exec: "sh": executable file not found in $PATH
```

And if you call `exec` right after creating the pod, while the container is not yet running, the answer differs: `error: unable to upgrade connection: container not found ("app")`. Wait for `Running`.

## Labels

Objects have labels — key-value pairs. They are used to select objects (including by Services and Deployments):

```text
$ kubectl -n learn-03 label pod hello tier=backend
$ kubectl -n learn-03 get pods -l app=hello,tier=backend --show-labels
NAME    READY   STATUS    RESTARTS   AGE   LABELS
hello   1/1     Running   0          1s    app=hello,tier=backend
$ kubectl -n learn-03 get pods -l 'tier in (frontend)'
No resources found in learn-03 namespace.
```

## Beginner mistakes

- ❌ Running a command in the wrong cluster → ✅ before dangerous commands check `kubectl config current-context`, use `--context` if needed.
- ❌ Forgetting the namespace → ✅ without `-n` the command works in `default` and "does not find" your objects.
- ❌ Expecting an image from local Docker to be visible to the cluster → ✅ `kind load docker-image` and an explicit tag.
- ❌ Editing an object and running `kubectl create` again → ✅ a repeated `create` gives `AlreadyExists`; use `apply` for repeatability.
- ❌ Leaving clusters and namespaces after experiments → ✅ `kubectl delete namespace learn-NN`, `kind delete cluster --name …`.

## Related materials

- Next: [advanced](advanced.en.md) — `apply` and `diff`, `explain`, several clusters
- Previous module: [02 · Kubernetes architecture](../02-architecture/overview.en.md)
- Containers and images: [01 · Containers and Docker](../01-containers/overview.en.md)
- Foundation: the [Linux track](../../../linux/overview.en.md)

## Sources

- [kind: Quick Start](https://kind.sigs.k8s.io/docs/user/quick-start/)
- [kind: Loading an image](https://kind.sigs.k8s.io/docs/user/quick-start/#loading-an-image-into-your-cluster)
- [Install kubectl](https://kubernetes.io/docs/tasks/tools/)
- [Organizing cluster access using kubeconfig](https://kubernetes.io/docs/concepts/configuration/organize-cluster-access-kubeconfig/)
- [kubectl: cheat sheet](https://kubernetes.io/docs/reference/kubectl/quick-reference/)
- [Labels and Selectors](https://kubernetes.io/docs/concepts/overview/working-with-objects/labels/)
- [Namespaces](https://kubernetes.io/docs/concepts/overview/working-with-objects/namespaces/)
