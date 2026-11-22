---
title: "Exercises: local cluster and kubectl"
lang: en
track: kubernetes
kind: module
doc: exercises
slug: 03-local-cluster-and-kubectl
part: 1
tags: [kind, kubectl, kubeconfig, context, namespace, apply, diff, multi-node]
status: draft
updated: 2026-11-22
---

# Exercises: local cluster and kubectl

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. Solutions are under spoilers. You need Docker, `kind` and `kubectl`; the `hello:multi` image is from [module 01](../01-containers/basics.en.md). All experiments run in a disposable cluster and namespace; clean up afterwards: `kubectl delete namespace learn-03`, `kind delete cluster --name learn`.

## 1. 🟢 The first pod

**Task:** create the cluster `learn` and the namespace `learn-03`, load the `hello:multi` image into the cluster, start the pod `hello` from a manifest and reach it through `port-forward`.

**Expected result:**

```text
pod/hello condition met
Hello from hello
```

<details><summary>Hint</summary>

An image from local Docker must be loaded with `kind load docker-image`, and the manifest should specify `imagePullPolicy: IfNotPresent`. `port-forward` needs a running pod.

</details>

<details><summary>Solution</summary>

```yaml
# pod.yaml
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

```bash
kind create cluster --name learn
kubectl create namespace learn-03
kind load docker-image hello:multi --name learn
kubectl -n learn-03 apply -f pod.yaml
kubectl -n learn-03 wait --for=condition=Ready pod/hello --timeout=60s
kubectl -n learn-03 port-forward pod/hello 18090:8080 &
curl -s localhost:18090
```

```text
pod/hello condition met
Hello from hello
```

In Kubernetes a container's hostname equals the pod name, hence the reply `Hello from hello`.

</details>

## 2. 🟢 Contexts and namespaces

**Task:** print the list of contexts, determine the current one, write `learn-03` as the default namespace for the context `kind-learn` (so you do not type `-n`), check that `kubectl get pods` sees the pod `hello`, and restore the default namespace.

**Expected result:** `kubectl get pods` without `-n` shows the pod `hello`.

<details><summary>Hint</summary>

`kubectl config set-context <context> --namespace=<ns>`. To restore the default, set an empty namespace.

</details>

<details><summary>Solution</summary>

```bash
kubectl config get-contexts
kubectl config current-context
kubectl config set-context kind-learn --namespace=learn-03
kubectl --context kind-learn get pods
kubectl config set-context kind-learn --namespace=
```

```text
Context "kind-learn" modified.
NAME    READY   STATUS    RESTARTS   AGE
hello   1/1     Running   0          1m
```

The namespace is written in the context, so `-n` is not needed. The `--context` flag lets you run a command in the desired context without switching the current one.

</details>

## 3. 🟡 create, apply and diff

**Task:** delete the pod `hello` and create it through `create` twice in a row. Then run `apply` for the same file twice. Change `containerPort` in a copy of the manifest and look at `kubectl diff`. Explain the three different results.

**Expected result:** `AlreadyExists` on the second `create`; `configured` on the first `apply` (with a warning), `unchanged` on the second; `diff` shows the port change.

<details><summary>Hint</summary>

The warning about `last-applied-configuration` appears when `apply` is applied to an object created through `create`.

</details>

<details><summary>Solution</summary>

```bash
kubectl -n learn-03 delete pod hello --now
kubectl -n learn-03 create -f pod.yaml
kubectl -n learn-03 create -f pod.yaml
kubectl -n learn-03 apply -f pod.yaml
kubectl -n learn-03 apply -f pod.yaml
sed 's/containerPort: 8080/containerPort: 8081/' pod.yaml > pod2.yaml
kubectl -n learn-03 diff -f pod2.yaml
```

```text
pod/hello created
Error from server (AlreadyExists): error when creating "pod.yaml": pods "hello" already exists
Warning: resource pods/hello is missing the kubectl.kubernetes.io/last-applied-configuration annotation which is required by kubectl apply. …
pod/hello configured
pod/hello unchanged
The Pod "hello" is invalid: spec: Forbidden: pod updates may not change fields other than …
@@ -104,7 +104,7 @@
-     "ContainerPort": 8080,
+     "ContainerPort": 8081,
```

`create` is not idempotent, `apply` is idempotent and added the annotation on the first run, while `diff` showed the difference and also warned that the port of an existing pod cannot be changed.

</details>

## 4. 🟡 A manifest draft and output

**Task:** generate the manifest of the Deployment `demo` on `nginx:1.29` without creating the object, save it to a file, apply it, and then print the pod names and IPs with one `jsonpath` command and a table with the node through `custom-columns`.

**Expected result:** lines like `demo-…<TAB>10.244.0.…` and a table `NAME  IMAGE  NODE`.

<details><summary>Hint</summary>

`--dry-run=client -o yaml` prints a manifest. For `jsonpath` use `range .items[*]`.

</details>

<details><summary>Solution</summary>

```bash
kubectl create deployment demo --image=nginx:1.29 --dry-run=client -o yaml > demo.yaml
kubectl -n learn-03 apply -f demo.yaml
kubectl -n learn-03 rollout status deploy/demo
kubectl -n learn-03 get pods -o jsonpath='{range .items[*]}{.metadata.name}{"\t"}{.status.podIP}{"\n"}{end}'
kubectl -n learn-03 get pods -o custom-columns=NAME:.metadata.name,IMAGE:.spec.containers[0].image,NODE:.spec.nodeName
```

```text
demo-645c7f9dc-kjxff	10.244.0.24
NAME                   IMAGE        NODE
demo-645c7f9dc-kjxff   nginx:1.29   learn-control-plane
```

The manifest from `--dry-run=client` contains extra empty fields (`strategy: {}`, `status: {}`): they can be removed. For scripts `jsonpath` and `custom-columns` are more reliable than parsing a text table.

</details>

## 5. 🔴 A multi-node cluster

**Task:** create the cluster `learn-multi` of 1 control plane and 2 worker nodes, wait for all nodes to be ready, load the `hello:multi` image and start a Deployment with 3 replicas. Show which nodes the pods landed on and explain why not on the control plane. Then delete the cluster.

**Expected result:** pods only on `learn-multi-worker` and `learn-multi-worker2`.

<details><summary>Hint</summary>

Node roles are set in the `kind` configuration file. `kubectl wait --for=condition=Ready node --all` waits for readiness. For the control plane check `.spec.taints`.

</details>

<details><summary>Solution</summary>

```yaml
# kind-multi.yaml
kind: Cluster
apiVersion: kind.x-k8s.io/v1alpha4
nodes:
  - role: control-plane
  - role: worker
  - role: worker
```

```bash
kind create cluster --name learn-multi --config kind-multi.yaml --wait 120s
kubectl --context kind-learn-multi wait --for=condition=Ready node --all --timeout=120s
kind load docker-image hello:multi --name learn-multi
kubectl --context kind-learn-multi create namespace learn-03
kubectl --context kind-learn-multi -n learn-03 create deployment hello --image=hello:multi --replicas=3
kubectl --context kind-learn-multi -n learn-03 rollout status deploy/hello --timeout=90s
kubectl --context kind-learn-multi -n learn-03 get pods -o custom-columns=NAME:.metadata.name,NODE:.spec.nodeName
kubectl --context kind-learn-multi get nodes -o jsonpath='{range .items[*]}{.metadata.name}{" taints="}{.spec.taints}{"\n"}{end}'
kind delete cluster --name learn-multi
```

```text
hello-5cf47849b9-2g9qq   learn-multi-worker2
hello-5cf47849b9-smm56   learn-multi-worker
hello-5cf47849b9-zgn5l   learn-multi-worker2
learn-multi-control-plane taints=[{"effect":"NoSchedule","key":"node-role.kubernetes.io/control-plane"}]
```

The control plane node has the taint `node-role.kubernetes.io/control-plane:NoSchedule`, so the scheduler does not place ordinary pods on it. `kind load docker-image` loaded the image onto all three nodes (one line each in the output), so the pods start on any node. For the tag `multi` the default pull policy is `IfNotPresent`, and the kubelet does not go to Docker Hub; for `latest` it would be `Always` ([module 01](../01-containers/expert.en.md#gotcha-latest-in-kubernetes)).

</details>
