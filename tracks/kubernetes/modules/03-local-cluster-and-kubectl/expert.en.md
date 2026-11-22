---
title: "Local cluster and kubectl: expert"
lang: en
track: kubernetes
kind: module
doc: expert
slug: 03-local-cluster-and-kubectl
part: 1
tags: [kind, kubectl, kubeconfig, context, plugins, jsonpath, discovery, apply, diff]
versions: "kind v0.34 (node image kindest/node:v1.37.0), kubectl 1.36.1, Kubernetes 1.37.0"
status: draft
updated: 2026-11-22
---

# Local cluster and kubectl: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## Under the hood

### The kubeconfig structure

A kubeconfig file is YAML with `kind: Config`. The `kind-learn` context without secrets (`kubectl config view --minify`):

```text
apiVersion: v1
clusters:
- cluster:
    certificate-authority-data: DATA+OMITTED
    server: https://127.0.0.1:PORT
  name: kind-learn
contexts:
- context:
    cluster: kind-learn
    user: kind-learn
  name: kind-learn
current-context: kind-learn
kind: Config
users:
- name: kind-learn
  user:
    client-certificate-data: DATA+OMITTED
    client-key-data: DATA+OMITTED
```

kind authenticates you with a client certificate (the file holds the certificate and key). The address `127.0.0.1:PORT` is the API server port of the node that Docker forwards to your machine; the port is random. Cloud clusters usually use tokens or exec plugins that obtain a token by running a command.

The file lookup order: the `--kubeconfig` flag, then the `KUBECONFIG` variable (a list of files merged by kubectl), then `~/.kube/config`.

### What kubectl does for one command

1. Finds the kubeconfig and takes the current context (or `--context`).
2. Determines which resource a name in the command matches (`pods`, `po`, `deployments`). For this it asks the server for the list of resources (discovery) and caches it in `~/.kube/cache/discovery`, separately for each server. On our setup the cache has folders named `127.0.0.1_<port>` for different kind clusters.
3. Sends an HTTP request to the API server.
4. Prints the result. For `kubectl get` the table is built by the server: kubectl asks for it with the header `Accept: application/json;as=Table;v=v1;g=meta.k8s.io,…`, so the columns are the same for any client and any resource, including CRDs.

You can see the requests with the `-v` flag: `-v=6` prints the method, URL and status, `-v=8` also the headers.

### Versions and compatibility

kubectl may differ from the API server by one minor version in either direction. In our setup kubectl `1.36.1` works with the server `1.37.0`. For kind the Kubernetes version is determined by the node image `kindest/node:vX.Y.Z` (ours is `v1.37.0`); another image is set with the `--image` flag or the `image` field in the config.

> ⚠️ Verify: the availability of `kindest/node` images for specific versions and the compatibility of the kind version with them should be checked against kind releases; this material verified only the pair kind v0.34 and `kindest/node:v1.37.0`.

## Reference

### kubectl commands

The list was obtained from `kubectl --help` on kubectl 1.36.1.

| Group | Commands |
|---|---|
| Creating | `create`, `apply`, `replace`, `run`, `expose`, `kustomize` |
| Viewing | `get`, `describe`, `explain`, `events`, `logs`, `top`, `api-resources`, `api-versions`, `cluster-info`, `version` |
| Changing | `edit`, `patch`, `label`, `annotate`, `set`, `scale`, `autoscale`, `rollout`, `delete`, `taint` |
| Access to containers | `exec`, `attach`, `port-forward`, `cp`, `proxy`, `debug` |
| Node maintenance | `cordon`, `drain`, `taint` |
| Checks | `diff`, `wait`, `auth` (`auth can-i`), `certificate` |
| Client setup | `config`, `plugin`, `completion` |

### Global and frequent flags

| Flag | Meaning |
|---|---|
| `-n`, `--namespace` / `-A` | a namespace / all namespaces |
| `--context`, `--kubeconfig` | a context / a kubeconfig file for one command |
| `-o` | the output format (see below) |
| `-l`, `--field-selector` | selection by labels / by fields (`status.phase=Running`) |
| `-w` | watch for changes |
| `--show-labels`, `--sort-by` | show labels / sorting (for example, `.metadata.creationTimestamp`) |
| `--dry-run=none\|client\|server` | do not execute (a client or server check) |
| `--as` | act as a user or ServiceAccount |
| `-v` | log verbosity (6 — requests, 8 — also headers) |

### Output formats

| Format | Used for |
|---|---|
| (empty) | a table formatted by the server |
| `wide` | a table with extra columns |
| `yaml`, `json` | the full object |
| `name` | `type/name`, handy in scripts |
| `jsonpath`, `jsonpath-file` | selecting fields by an expression |
| `custom-columns`, `custom-columns-file` | your own table |
| `go-template`, `go-template-file` | Go templates |

### kind commands

The list is from `kind --help`.

| Command | What it does |
|---|---|
| `kind create cluster [--name N] [--config file] [--image image] [--wait 120s]` | create a cluster |
| `kind delete cluster --name N` | delete a cluster (and its context) |
| `kind get clusters\|nodes\|kubeconfig` | a list of clusters / nodes / the kubeconfig |
| `kind export kubeconfig\|logs` | write the kubeconfig / collect logs |
| `kind load docker-image\|image-archive` | load an image from the host into the nodes |
| `kind build node-image` | build a node image |

The configuration file (`kind.x-k8s.io/v1alpha4`) was verified in this material only for `nodes` with the `control-plane` and `worker` roles. The other fields (`extraPortMappings`, `networking`, `containerdConfigPatches`, `kubeadmConfigPatches` and others) are given from the kind documentation and were not run.

> ⚠️ Verify: the kind configuration fields `extraPortMappings`, `networking`, `containerdConfigPatches`, `kubeadmConfigPatches` were not run in this material; check them against the [kind documentation](https://kind.sigs.k8s.io/docs/user/configuration/).

### kubectl plugins

Any executable `kubectl-<name>` in `PATH` becomes the command `kubectl <name>`. The plugin receives the command-line arguments:

```text
$ kubectl hello a b
plugin hello, args: a b
```

The list of found plugins: `kubectl plugin list`. On Windows a plugin must have an extension from `PATHEXT` (ours is `kubectl-hello.cmd`): a file without an extension was listed by `kubectl plugin list` but could not be run (`error: unknown command "hello" for "kubectl"`). The plugin manager is krew.

## Gotchas

### Gotcha: kind create cluster changes the current context

```text
$ kubectl config current-context
docker-desktop            # your previous context (ours is Docker Desktop)
$ kind create cluster --name learn
Set kubectl context to "kind-learn"
$ kubectl config current-context
kind-learn
```

After creating any cluster, your commands go to it. If you worked with another cluster before, a `delete` or `apply` without a check may land in the wrong place. To go back: `kubectl config use-context <previous context>`; in our case after the experiments we ran `kubectl config use-context docker-desktop`.

### Gotcha: image not found (ErrImagePull)

A pod from a local image without `kind load` does not start: the kubelet goes to Docker Hub (`pull access denied, repository does not exist …`). Details are in [basics](basics.en.md#the-first-manifest).

### Gotcha: diff warns that apply will not pass

`kubectl diff` on a change of an immutable pod field prints both the difference and `The Pod "hello" is invalid: spec: Forbidden: pod updates may not change fields other than …`. The message means a real `apply` with such a change will also be rejected ([advanced](advanced.en.md#diff-and-dry-run)).

### Gotcha: apply on top of create prints a warning

```text
$ kubectl apply -f pod.yaml     # the object was created through create
Warning: resource pods/hello is missing the kubectl.kubernetes.io/last-applied-configuration annotation which is required by kubectl apply. …
pod/hello configured
```

The warning is harmless (the annotation will be added) but shows that the object was not managed declaratively from the start.

### Gotcha: get all does not show everything

```text
$ kubectl -n learn-03 create configmap cm1 --from-literal=a=b
$ kubectl -n learn-03 get all
NAME        READY   STATUS    RESTARTS   AGE
pod/hello   1/1     Running   0          56s
$ kubectl -n learn-03 get cm cm1 -o name
configmap/cm1
```

`all` is not "all objects" but a small set of types (pods, services, deployments and the like). The ConfigMap did not appear in `get all`. A list of all types in a namespace is available through `kubectl api-resources --verbs=list --namespaced -o name` and a loop over them.

### Gotcha: exec right after creating a pod

```text
$ kubectl -n learn-03 apply -f pod.yaml
$ kubectl -n learn-03 exec hello -- ls
error: unable to upgrade connection: container not found ("app")
```

The container is not running yet. This is not an image error: wait for `Running` (`kubectl wait --for=condition=Ready pod/hello`).

### Gotcha: worker nodes are NotReady right after kind create

```text
$ kind create cluster --name learn-multi --config kind-multi.yaml --wait 120s
$ kubectl --context kind-learn-multi get nodes
learn-multi-control-plane   Ready      control-plane   26s   v1.37.0
learn-multi-worker          NotReady   <none>          11s   v1.37.0
learn-multi-worker2         NotReady   <none>          11s   v1.37.0
```

`--wait` waits for the control plane to be ready, not the worker nodes. Wait, or use `kubectl wait --for=condition=Ready node --all`.

### Gotcha: the separator in KUBECONFIG depends on the OS

On Windows the separator is `;`: writing `kc-learn.yaml:C:\…\config` leads to `error: error loading config file … The filename, directory name, or volume label syntax is incorrect`. On Linux and macOS the separator is `:`.

### Gotcha: a lone pod does not come back

```text
$ kubectl -n learn-03 delete pod hello --now
$ kubectl -n learn-03 get pods
No resources found in learn-03 namespace.
```

A pod created directly belongs to no controller: nobody will restore it after deletion. For self-healing use a Deployment ([module 02](../02-architecture/basics.en.md#self-healing)).

## Trade-offs and alternatives

| Tool | Pros | Cons |
|---|---|---|
| kind | a cluster of containers, a fast start, several nodes, convenient for CI | needs Docker and resources; not production |
| minikube | drivers (VM, Docker), addons | heavier than kind |
| Kubernetes in Docker Desktop | built in, enabled with a checkbox | one node, the version is set by Docker Desktop |
| k3d, k3s | a lightweight distribution | differences from standard Kubernetes |
| A cloud cluster | a real environment | costs money, needs access |

kubectl can be replaced by client libraries (Go, Python, C#) and terminal UIs such as k9s: all of them use the same API.

## Interview questions

**1. What is a kubeconfig context?**

<details><summary>Answer</summary>

A binding "cluster + user + default namespace". kubectl runs commands in the current context, which is switched with `kubectl config use-context` and overridden for one command with the `--context` flag.

</details>

**2. (Trick) You ran `kind create cluster` and then `kubectl delete deployment web`. Which cluster did the command go to?**

<details><summary>Answer</summary>

To the new kind cluster: `kind create` switches the current context to it. That is why you check `kubectl config current-context` before dangerous commands.

</details>

**3. What is the difference between `kubectl create` and `kubectl apply`?**

<details><summary>Answer</summary>

`create` creates an object and gives `AlreadyExists` if it exists (imperative). `apply` brings the object to the description in the file and is idempotent: a repeated run gives `unchanged`. `apply` stores the last applied manifest in the `last-applied-configuration` annotation.

</details>

**4. How do you see what `apply` would change without applying it?**

<details><summary>Answer</summary>

`kubectl diff -f file.yaml` or `--dry-run=server`. If a field is immutable, `diff` additionally prints a validation error.

</details>

**5. How do you get a manifest draft without creating an object?**

<details><summary>Answer</summary>

`kubectl create … --dry-run=client -o yaml`, for example `kubectl create deployment demo --image=nginx:1.29 --dry-run=client -o yaml`. Extra empty fields (`strategy: {}`, `status: {}`) can be removed.

</details>

**6. How do you find a manifest field's description?**

<details><summary>Answer</summary>

`kubectl explain deployment.spec.strategy.rollingUpdate` (recursively: `--recursive`). The description comes from the API schema of your cluster.

</details>

**7. (Trick) An image is built in Docker, a pod refers to it, but the status is `ErrImagePull`. What to do?**

<details><summary>Answer</summary>

A kind cluster does not see local Docker images. Load it with `kind load docker-image name:tag --name cluster`, use an explicit tag (not `latest`, otherwise the `Always` policy goes to the registry) and `imagePullPolicy: IfNotPresent`.

</details>

**8. How do you attach several kubeconfig files?**

<details><summary>Answer</summary>

Through `KUBECONFIG` with a list of files (the separator is `:` on Linux and macOS, `;` on Windows) or one at a time through `--kubeconfig`. `kind get kubeconfig --name N` prints a cluster's kubeconfig without touching the main file.

</details>

**9. (Trick) What does `kubectl get all` show?**

<details><summary>Answer</summary>

Only a small set of types (pods, services, deployments and the like), not all objects of the namespace. In the experiment the created ConfigMap did not appear in the output.

</details>

**10. How does kubectl build the table for `get`?**

<details><summary>Answer</summary>

The server returns the table itself: kubectl requests it with the header `Accept: application/json;as=Table;v=v1;g=meta.k8s.io,…`. So the columns are the same for all clients and work for CRDs.

</details>

**11. How do you extend kubectl with your own command?**

<details><summary>Answer</summary>

Put an executable `kubectl-<name>` into `PATH`: it becomes the command `kubectl <name>`. The list is `kubectl plugin list`. On Windows an extension from `PATHEXT` is needed, otherwise the file will not run.

</details>

**12. What happens if you delete a pod created directly, without a Deployment?**

<details><summary>Answer</summary>

It disappears for good: there is no controller to recreate it. In the experiment after `delete pod hello` the output was `No resources found`.

</details>

**13. Why can worker nodes be `NotReady` right after `kind create cluster --wait`?**

<details><summary>Answer</summary>

`--wait` waits only for the control plane to be ready. Worker nodes join and initialize a bit later: in the experiment they became `Ready` in ~15 seconds.

</details>

## Cheat sheet

```bash
kind create cluster --name learn            # create (switches the context!)
kind get clusters; kind delete cluster --name learn
kind load docker-image app:1.0 --name learn
kubectl config get-contexts; kubectl config use-context NAME
kubectl config set-context --current --namespace=learn-03
kubectl -n NS apply -f f.yaml; kubectl diff -f f.yaml
kubectl get pods -o wide -l app=x --show-labels
kubectl get pods -o jsonpath='{.items[*].metadata.name}'
kubectl describe pod NAME; kubectl logs NAME; kubectl port-forward pod/NAME 8080:8080
kubectl explain pod.spec --recursive | less
kubectl create deployment d --image=nginx:1.29 --dry-run=client -o yaml
kubectl delete namespace learn-03
```

## Related materials

- Previous levels: [basics](basics.en.md), [advanced](advanced.en.md)
- Previous modules: [01 · Containers and Docker](../01-containers/overview.en.md), [02 · Kubernetes architecture](../02-architecture/overview.en.md)
- Foundation: the [Linux track](../../../linux/overview.en.md)

## Sources

- [kubectl reference](https://kubernetes.io/docs/reference/kubectl/)
- [kubectl: cheat sheet](https://kubernetes.io/docs/reference/kubectl/quick-reference/)
- [Organizing cluster access using kubeconfig](https://kubernetes.io/docs/concepts/configuration/organize-cluster-access-kubeconfig/)
- [Extend kubectl with plugins](https://kubernetes.io/docs/tasks/extend-kubectl/kubectl-plugins/)
- [Server-side printing (Table)](https://kubernetes.io/docs/reference/using-api/api-concepts/#receiving-resources-as-tables)
- [Version Skew Policy](https://kubernetes.io/releases/version-skew-policy/)
- [kind: Configuration](https://kind.sigs.k8s.io/docs/user/configuration/)
- [kind: Quick Start](https://kind.sigs.k8s.io/docs/user/quick-start/)
