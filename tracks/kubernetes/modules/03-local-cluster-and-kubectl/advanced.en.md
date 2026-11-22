---
title: "Local cluster and kubectl: advanced"
lang: en
track: kubernetes
kind: module
doc: advanced
slug: 03-local-cluster-and-kubectl
part: 1
tags: [kind, kubectl, kubeconfig, apply, diff, dry-run, explain, jsonpath, selectors, multi-node]
versions: "kind v0.34 (node image kindest/node:v1.37.0), kubectl 1.36.1, Kubernetes 1.37.0"
status: draft
updated: 2026-11-22
---

# Local cluster and kubectl: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on [basics](basics.en.md): installation, `kind create`, namespaces and the basic commands are not repeated.

## create and apply: imperative and declarative

`kubectl create` creates an object and fails if it already exists. `kubectl apply` brings the object to the description in the file and is safe to run repeatedly:

```text
$ kubectl -n learn-03 create -f pod.yaml
pod/hello created
$ kubectl -n learn-03 create -f pod.yaml
Error from server (AlreadyExists): error when creating "pod.yaml": pods "hello" already exists
$ kubectl -n learn-03 apply -f pod.yaml
Warning: resource pods/hello is missing the kubectl.kubernetes.io/last-applied-configuration annotation which is required by kubectl apply. …
pod/hello configured
$ kubectl -n learn-03 apply -f pod.yaml
pod/hello unchanged
```

Three observations:

- A repeated `create` gives `AlreadyExists`, a repeated `apply` gives `unchanged`: `apply` is idempotent.
- `apply` on an object created through `create` warns about the missing `last-applied-configuration` annotation and adds it itself. To avoid the warning, create declaratively or with `kubectl create --save-config`.
- The annotation `kubectl.kubernetes.io/last-applied-configuration` stores the last applied manifest: `apply` uses it to determine which fields you removed from the file.

For declarative work keep manifests in Git and apply them with `apply -f` (a file, a folder, or `-k` for Kustomize, [module 14](../../overview.en.md#part-2-advanced-topics)).

## diff and dry-run

`kubectl diff -f` compares a file with the cluster state without changing it. We edit the port in the manifest and look at the difference:

```text
$ kubectl -n learn-03 diff -f pod2.yaml
The Pod "hello" is invalid: spec: Forbidden: pod updates may not change fields other than `spec.containers[*].image`,`spec.initContainers[*].image`,`spec.activeDeadlineSeconds`,`spec.tolerations` (only additions to existing tolerations),`spec.terminationGracePeriodSeconds` (allow it to be set to 1 if it was previously negative)
@@ -104,7 +104,7 @@
     {
      "Name": "",
      "HostPort": 0,
-     "ContainerPort": 8080,
+     "ContainerPort": 8081,
```

`diff` showed the change, and the server immediately warned that a real `apply` would not accept such a field on an existing pod: most pod fields are immutable. This result is useful before applying: the error is visible while the cluster is untouched.

`--dry-run=client -o yaml` does not contact the cluster and prints the object that would be created. This is how manifest drafts are made:

```text
$ kubectl create deployment demo --image=nginx:1.29 --dry-run=client -o yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  labels:
    app: demo
  name: demo
spec:
  replicas: 1
  selector:
    matchLabels:
      app: demo
  strategy: {}
  template:
    metadata:
      labels:
        app: demo
    spec:
      containers:
      - image: nginx:1.29
        name: nginx
        resources: {}
status: {}
```

The lines `strategy: {}`, `resources: {}` and `status: {}` are empty values and can be removed. The `--dry-run=server` mode sends the request to the API server, which validates it but does not store it.

## explain: field help

`kubectl explain` shows the description of any field straight from the API schema of your cluster:

```text
$ kubectl explain deployment.spec.strategy.rollingUpdate
GROUP:      apps
KIND:       Deployment
VERSION:    v1

FIELD: rollingUpdate <RollingUpdateDeployment>

DESCRIPTION:
    Rolling update config params. Present only if DeploymentStrategyType =
    RollingUpdate.
…
FIELDS:
  maxSurge	<IntOrString>
```

This is faster than searching the documentation and always matches your cluster's version. All fields recursively: `kubectl explain pod.spec --recursive`.

## Output formats

| Flag | Result |
|---|---|
| `-o wide` | extra columns (IP, node) |
| `-o yaml`, `-o json` | the full object |
| `-o name` | only `type/name` |
| `-o jsonpath='…'` | selected fields by a JSONPath expression |
| `-o custom-columns=HEADER:path,…` | your own table |
| `-o go-template='…'` | a Go template |

```text
$ kubectl -n learn-02 get pods -o jsonpath='{range .items[*]}{.metadata.name}{"\t"}{.status.podIP}{"\n"}{end}'
web-689b9bdbdd-7hpbd	10.244.0.5
web-689b9bdbdd-r4tkv	10.244.0.7
$ kubectl -n learn-02 get pods -o custom-columns=NAME:.metadata.name,IMAGE:.spec.containers[0].image,NODE:.spec.nodeName
NAME                   IMAGE        NODE
web-689b9bdbdd-7hpbd   nginx:1.29   learn-control-plane
web-689b9bdbdd-r4tkv   nginx:1.29   learn-control-plane
$ kubectl get ns learn-03 -o go-template='{{.metadata.name}}{{"\n"}}'
learn-03
```

## Selectors

Labels are set at creation or with `label`; selection uses the `-l` flag. Supported are equality (`app=hello`), several conditions separated by commas (a logical "and") and sets (`tier in (a,b)`, `tier notin (…)`, and also plain `key` and `!key`):

```text
$ kubectl -n learn-03 get pods -l app=hello,tier=backend --show-labels
$ kubectl -n learn-03 get pods -l 'tier in (frontend)'
No resources found in learn-03 namespace.
$ kubectl -n learn-03 delete pod -l app=demo        # delete everything matching the selector
```

Selectors underlie Kubernetes relations: a Service selects pods, a Deployment selects its ReplicaSets.

## Several clusters and KUBECONFIG

By default `~/.kube/config` is read. The `KUBECONFIG` environment variable lets you attach several files; kubectl merges them. The separator is a colon on Linux and macOS and a semicolon on Windows. An experiment on Windows:

```text
$ kind get kubeconfig --name learn > kc-learn.yaml
$ KUBECONFIG="kc-learn.yaml;C:\Users\petel\.kube\config" kubectl config get-contexts -o name
docker-desktop
kind-learn
```

Writing a colon instead of the semicolon on Windows gives `The filename, directory name, or volume label syntax is incorrect`. The command `kind get kubeconfig --name <name>` prints the cluster's kubeconfig without changing your file. The `--kubeconfig` flag points to a file for one command.

Always know where you are: `kubectl config current-context` and `kubectl --context <name> …`.

## Multi-node kind

A configuration file describes the nodes:

```yaml
kind: Cluster
apiVersion: kind.x-k8s.io/v1alpha4
nodes:
  - role: control-plane
  - role: worker
  - role: worker
```

```text
$ kind create cluster --name learn-multi --config kind-multi.yaml --wait 120s
$ kubectl --context kind-learn-multi get nodes
NAME                        STATUS     ROLES           AGE   VERSION
learn-multi-control-plane   Ready      control-plane   26s   v1.37.0
learn-multi-worker          NotReady   <none>          11s   v1.37.0
learn-multi-worker2         NotReady   <none>          11s   v1.37.0
```

Right after creation the worker nodes showed `NotReady`: `--wait` waits only for the control plane. After 15 seconds both became `Ready`:

```text
$ kubectl --context kind-learn-multi get nodes
learn-multi-worker          Ready      <none>          32s   v1.37.0
learn-multi-worker2         Ready      <none>          32s   v1.37.0
```

The role label of the `worker` nodes is empty: `ROLES  <none>`. Such a cluster is needed for experiments with scheduling and node failures (module 18). It is deleted with `kind delete cluster --name learn-multi`, which also removes its context from kubeconfig.

## Images in kind

An image built in local Docker is not visible to a kind node. Loading:

```text
$ kind load docker-image hello:multi --name learn
Image: "hello:multi" with ID "sha256:abf0c4…" not yet present on node "learn-control-plane", loading...
```

If the image changed and you rebuilt it with the same tag, load it again: kind compares the ID and reloads. Use an explicit tag and `imagePullPolicy: IfNotPresent`: for the `latest` tag the default policy is `Always` ([module 01](../01-containers/expert.en.md#gotcha-latest-in-kubernetes)).

## Best practices

- One namespace per module or experiment (`learn-NN`), deleted with one command.
- Before `apply` — `kubectl diff`, before dangerous commands — a context check.
- Keep manifests in files and use imperative commands for drafts (`--dry-run=client -o yaml`).
- Do not use `latest`; set `imagePullPolicy` explicitly for local images.
- For scripts choose stable formats: `-o name`, `-o jsonpath`, rather than parsing text tables.

## Common mistakes

- ❌ `kubectl create` instead of `apply` in repeatable scenarios → ✅ `apply`: repeatable and idempotent.
- ❌ Editing an immutable field in the manifest of an existing pod → ✅ recreate the pod; almost all pod fields are immutable, and changing the image and similar fields should go through a controller (a Deployment).
- ❌ Parsing `kubectl get` output as text → ✅ `-o json`, `jsonpath`, `custom-columns`.
- ❌ Forgetting that `kind create cluster` changed the context → ✅ check `current-context`; after experiments switch back with `kubectl config use-context`.
- ❌ Treating the cluster as ready right after `kind create` → ✅ check `kubectl get nodes`: worker nodes may be `NotReady` for a few more seconds.

## Comparison with other tracks

- [Linux](../../../linux/overview.en.md): kubectl is a thin wrapper over HTTP; output tasks are conveniently solved with the same `grep`, `awk` and `jq`.
- [Python](../../../python/overview.en.md) and [C#](../../../csharp/overview.en.md): what kubectl does is also done by the Kubernetes client libraries through the same REST API.

## Related materials

- Previous level: [basics](basics.en.md)
- Next: [expert](expert.en.md)
- Previous modules: [01 · Containers and Docker](../01-containers/overview.en.md), [02 · Kubernetes architecture](../02-architecture/overview.en.md)

## Sources

- [kubectl apply](https://kubernetes.io/docs/reference/kubectl/generated/kubectl_apply/)
- [kubectl diff](https://kubernetes.io/docs/reference/kubectl/generated/kubectl_diff/)
- [Declarative management of objects using kubectl](https://kubernetes.io/docs/tasks/manage-kubernetes-objects/declarative-config/)
- [kubectl explain](https://kubernetes.io/docs/reference/kubectl/generated/kubectl_explain/)
- [JSONPath support](https://kubernetes.io/docs/reference/kubectl/jsonpath/)
- [Configure access to multiple clusters](https://kubernetes.io/docs/tasks/access-application-cluster/configure-access-multiple-clusters/)
- [kind: Configuration](https://kind.sigs.k8s.io/docs/user/configuration/)
- [kind: Working with kind (load docker-image)](https://kind.sigs.k8s.io/docs/user/quick-start/#loading-an-image-into-your-cluster)
