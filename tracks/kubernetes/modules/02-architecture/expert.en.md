---
title: "Kubernetes architecture: expert"
lang: en
track: kubernetes
kind: module
doc: expert
slug: 02-architecture
part: 1
tags: [kubernetes, architecture, etcd, api-server, resourceversion, finalizers, generation, kubelet, scheduler, controller]
versions: "Kubernetes 1.37.0 (kind v0.34, containerd 2.3.4), kubectl 1.36.1"
status: draft
updated: 2026-11-22
---

# Kubernetes architecture: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## Under the hood

### How the API server stores data

The API server is the only client of etcd. Each object lies under the key `/registry/<resource>/<namespace>/<name>`. Let us look at the keys of the Deployment and pods in the `learn-02` namespace (etcd is reachable through `etcdctl` inside the `etcd-…` pod, client certificates are required):

```text
$ kubectl -n kube-system exec etcd-learn-control-plane -- etcdctl \
    --cacert=/etc/kubernetes/pki/etcd/ca.crt \
    --cert=/etc/kubernetes/pki/etcd/server.crt \
    --key=/etc/kubernetes/pki/etcd/server.key \
    get /registry/deployments/learn-02 --prefix --keys-only
/registry/deployments/learn-02/web

$ … get /registry/pods/learn-02 --prefix --keys-only
/registry/pods/learn-02/web-689b9bdbdd-7hpbd
/registry/pods/learn-02/web-689b9bdbdd-r4tkv
```

The value is neither YAML nor JSON. The first bytes of the Deployment value:

```text
0000000   k   8   s  \0  \n 025  \n  \a   a   p   p   s   /   v   1 022
0000020  \n   D   e   p   l   o   y   m   e   n   t 022 …
```

The prefix `k8s\0`, then binary protobuf with `apps/v1` and `Deployment`. So you cannot "look into etcd with your eyes": a decoder is needed, and in normal operation you should not touch the storage layout — it is an internal detail of the API server.

### The request pipeline

The API server has one and the same pipeline for writes and reads: authentication → authorization → admission → validation → storage. For reads the admission and validation stages are skipped. The authorization mode in our cluster is `--authorization-mode=Node,RBAC`; the key flags from the `kube-apiserver` manifest:

| Flag | Value in kind | Meaning |
|---|---|---|
| `--secure-port` | `6443` | the HTTPS API port |
| `--etcd-servers` | `https://127.0.0.1:2379` | where etcd is |
| `--service-cluster-ip-range` | `10.96.0.0/16` | the Service address range |
| `--authorization-mode` | `Node,RBAC` | authorization modes |
| `--enable-admission-plugins` | `NodeRestriction` | additionally enabled admission plugins |

For the controller-manager in kind: `--cluster-cidr=10.244.0.0/16` (the pod address range), `--allocate-node-cidrs=true`, `--leader-elect=true`, `--controllers=*,bootstrapsigner,tokencleaner` (`*` means all default controllers).

### Versions and compatibility

A Kubernetes minor version is released several times a year. The compatibility policy allows kubectl to differ from the API server by one minor version in either direction. In our setup kubectl `v1.36.1` works with the server `v1.37.0`. The control plane components and the kubelet also have permitted skews (the kubelet cannot be newer than the API server).

> ⚠️ Verify: the exact permitted skews for the kubelet, scheduler and controller-manager in version 1.37 were not checked against the version skew policy.

## Reference

### API server endpoints

| Path | What it returns | Verified |
|---|---|---|
| `/livez`, `/readyz`, `/healthz` | the server state (`ok`); `/readyz?verbose` lists the checks, including `etcd` | yes, `ok` |
| `/version` | version and platform | yes |
| `/api`, `/apis` | the list of core group versions and the list of API groups | yes |
| `/api/v1/…`, `/apis/<group>/<version>/…` | resources | yes |
| `/openapi/v3` | schema description (OpenAPI) | yes, returns JSON |
| `/metrics` | metrics in Prometheus format | yes, returns text |

### Object metadata

| Field | Meaning |
|---|---|
| `name`, `namespace` | the identifier within a type and namespace |
| `uid` | a unique identifier of the instance (new when an object with the same name is recreated) |
| `resourceVersion` | the record version for optimistic locking and watch; a client treats it as an opaque string |
| `generation` | grows when `spec` changes; in the experiment after `kubectl set image` it became `gen=5` and `status.observedGeneration` caught up |
| `creationTimestamp`, `deletionTimestamp` | creation time; time deletion began (if set, the object is `Terminating`) |
| `labels`, `annotations` | labels for selecting objects; arbitrary notes |
| `ownerReferences` | the object's owners (garbage collection) |
| `finalizers` | deletion blockers: the object disappears when the list is empty |
| `managedFields` | who changed which fields (server-side apply); kubectl does not show it by default, the `--show-managed-fields` flag is needed |

### API verbs

`get`, `list`, `watch`, `create`, `update`, `patch`, `delete`, `deletecollection`. These are the ones used in RBAC rules (module 10).

### What runs on a node

| Component | Task |
|---|---|
| `kubelet` | registers the node, starts pods, reports status and runs probes |
| container runtime (`containerd`) | starts containers through CRI |
| `kube-proxy` | rules for Service (in kind, `iptables` mode) |
| network plugin (in kind, `kindnet`) | pod networking |

The `kube-proxy` mode was taken from its ConfigMap: `mode: iptables`.

## Gotchas

### Gotcha: event order is imprecise

```text
$ kubectl -n learn-02 get events --sort-by=.metadata.creationTimestamp \
    -o custom-columns=REASON:.reason,SOURCE:.source.component
REASON              SOURCE
Scheduled           default-scheduler
Pulling             kubelet
ScalingReplicaSet   deployment-controller
SuccessfulCreate    replicaset-controller
SuccessfulCreate    replicaset-controller
Scheduled           default-scheduler
```

The `Scheduled` event ended up above `ScalingReplicaSet`, although logically the scheduler could act only after the ReplicaSet and pods were created. The `creationTimestamp` label stores seconds, so it does not define the order of events of the same second. To reconstruct the sequence, follow the cause-and-effect chain (who owns whom), not the sorting.

### Gotcha: etcd values are not YAML

See above: the value starts with `k8s\0` and contains protobuf. `etcdctl get` without `--keys-only` prints unreadable bytes.

### Gotcha: an object is stuck in Terminating

```text
$ kubectl -n learn-02 patch configmap fin -p '{"metadata":{"finalizers":["example.com/block"]}}'
$ kubectl -n learn-02 delete configmap fin --wait=false
configmap "fin" deleted from learn-02 namespace
$ kubectl -n learn-02 get configmap fin -o jsonpath='{.metadata.deletionTimestamp} {.metadata.finalizers}'
2026-11-22T16:02:41Z ["example.com/block"]
```

The `delete` command only set `deletionTimestamp`: while there is an entry in `finalizers` the object is not removed. Usually a controller removes the finalizer after cleaning up external resources. If there is no controller, remove the entry by hand:

```text
$ kubectl -n learn-02 patch configmap fin -p '{"metadata":{"finalizers":null}}'
$ kubectl -n learn-02 get configmap fin
Error from server (NotFound): configmaps "fin" not found
```

Make sure beforehand that the external resources really are not needed.

### Gotcha: the status is stale when a controller is unavailable

After stopping the controller-manager and deleting a pod, `kubectl get deploy web` showed `READY 3/3` with two pods (the experiment is in [advanced](advanced.en.md#what-happens-when-a-component-is-unavailable)). A status is a controller's output, not an observation "in real time".

### Gotcha: nodeName bypasses the scheduler

```text
$ kubectl -n learn-02 apply -f - <<'EOF'
apiVersion: v1
kind: Pod
metadata: {name: manual}
spec:
  nodeName: learn-control-plane
  containers: [{name: c, image: "nginx:1.29"}]
EOF
$ kubectl -n learn-02 get events --field-selector involvedObject.name=manual \
    -o custom-columns=REASON:.reason,SOURCE:.source.component
REASON    SOURCE
Pulled    kubelet
Created   kubelet
Started   kubelet
```

There is no `Scheduled` event: the pod bypassed the scheduler and went straight to the node. Resource, taint and affinity checks are not performed.

### Gotcha: in a single-node kind applications run on the control plane

```text
$ kubectl get node learn-control-plane -o jsonpath='{.spec.taints}'
                                  # empty: no taints
```

In our single-node kind the control plane node has no taints, and application pods run on it (in the `get pods -o wide` output it is `learn-control-plane` everywhere). In a multi-node kind (1 control plane + 2 workers, [module 03](../03-local-cluster-and-kubectl/advanced.en.md#multi-node-kind)) the picture is different: the control plane is isolated with a taint, and the three application pods went to the workers:

```text
$ kubectl get nodes -o jsonpath='{range .items[*]}{.metadata.name}{" taints="}{.spec.taints}{"\n"}{end}'
learn-multi-control-plane taints=[{"effect":"NoSchedule","key":"node-role.kubernetes.io/control-plane"}]
learn-multi-worker taints=
learn-multi-worker2 taints=
$ kubectl -n learn-03 get pods -o custom-columns=NAME:.metadata.name,NODE:.spec.nodeName
hello-5cf47849b9-2g9qq   learn-multi-worker2
hello-5cf47849b9-smm56   learn-multi-worker
hello-5cf47849b9-zgn5l   learn-multi-worker2
```

A `NoSchedule` taint keeps off the node pods that lack a matching toleration (more in module 18).

## Trade-offs and alternatives

| Choice | Pros | Cons |
|---|---|---|
| Managed Kubernetes (cloud) | the provider operates the control plane and etcd | cloud dependence, less control over the control plane |
| Self-managed cluster (kubeadm and similar) | full control | you are responsible for etcd, upgrades, certificates |
| kind, minikube, Docker Desktop | a fast local cluster for learning | not for production, no real HA |
| Docker Swarm, Nomad | easier to learn | a smaller ecosystem, a different API model |
| One server with Docker Compose | minimum complexity | no self-healing across machines and no cluster-level rolling updates |

## Interview questions

**1. Name the control plane components and what each is responsible for.**

<details><summary>Answer</summary>

`kube-apiserver` — the REST API and the only entry point; `etcd` — the state storage; `kube-scheduler` — choosing a node for a pod; `kube-controller-manager` — controllers that bring the actual state to the desired one. On the nodes run the `kubelet`, a container runtime and `kube-proxy`.

</details>

**2. Who starts a container: the scheduler or the kubelet?**

<details><summary>Answer</summary>

The kubelet. The scheduler only writes the chosen node into the pod (`spec.nodeName`), and the kubelet of that node sees the assigned pod and starts the containers through the runtime.

</details>

**3. What is the reconciliation loop and why is it better than a "command"?**

<details><summary>Answer</summary>

A controller compares the desired state (`spec`) with the actual one and fixes the difference. It reacts to a level (a discrepancy), not to an event, so a lost event or a component restart does not lose a change: the next loop fixes everything.

</details>

**4. (Trick) You deleted a pod owned by a Deployment. What happens, and what if the controller-manager is stopped?**

<details><summary>Answer</summary>

Normally the ReplicaSet creates a new pod. If the controller-manager is stopped there is no replacement, and the Deployment's `READY` stays stale (in the experiment `3/3` with two pods). After the controller-manager returns, the missing pod is created.

</details>

**5. What happens to a new pod if the scheduler is unavailable?**

<details><summary>Answer</summary>

The pod will be created by controllers but stay `Pending` without a node. There will be no `FailedScheduling` events, because the scheduler itself emits them. After the scheduler returns, the pod gets a node.

</details>

**6. Why is `resourceVersion` needed and what is a `Conflict` error?**

<details><summary>Answer</summary>

It is the record version for optimistic locking. On update the client sends the version it read; if the object has changed, the server answers `Conflict`, and the client must re-read the object and repeat. This prevents lost updates. A client must treat the value as an opaque string.

</details>

**7. What stages does a request pass in the API server?**

<details><summary>Answer</summary>

Authentication, authorization (in our setup `Node,RBAC`), admission controllers (mutating and validating), schema validation, and writing to etcd.

</details>

**8. Who may access etcd?**

<details><summary>Answer</summary>

In the standard scheme only the API server. The other components work through the API. Direct access to etcd means access to the whole state, including Secrets, and bypasses authorization and validation.

</details>

**9. (Trick) What does `kubectl delete` do if an object has a finalizer?**

<details><summary>Answer</summary>

It only sets `deletionTimestamp`. The object stays `Terminating` while there are entries in the `finalizers` list; controllers remove them after cleanup. In the experiment the object vanished right after a `patch` with `finalizers: null`.

</details>

**10. What is `generation` and how does it differ from `resourceVersion`?**

<details><summary>Answer</summary>

`generation` grows when `spec` changes (4 → 5 after `set image` in the experiment), while `status.observedGeneration` shows which version of the spec the controller has processed. `resourceVersion` changes on any write of the object, including `status`, and serves for locking and watch.

</details>

**11. (Trick) You set `spec.nodeName` by hand. What happens?**

<details><summary>Answer</summary>

The pod skips the scheduler: the `Scheduled` event is absent, and the kubelet of the chosen node starts the containers at once. Scheduling rules (taints, affinity, resource checks) are not applied.

</details>

**12. Why is leader election needed and where is it visible?**

<details><summary>Answer</summary>

So that several copies of the controller-manager or scheduler do not work simultaneously: one is active, the others wait. The leader is recorded in a `Lease` object in `kube-system` (`kube-controller-manager`, `kube-scheduler`), and the manifest enables the `--leader-elect=true` flag.

</details>

**13. Why are kind nodes Docker containers rather than virtual machines, and what does it mean for kubectl?**

<details><summary>Answer</summary>

kind runs each node as a container with systemd, a kubelet and containerd inside; pods run inside these containers. For kubectl it is an ordinary cluster: it talks to the API server (the port is forwarded to `127.0.0.1`).

</details>

## Cheat sheet

```bash
kubectl get pods -n kube-system -o wide         # control plane components in kind
kubectl get --raw '/readyz?verbose'             # API server checks
kubectl get --raw /version                      # server version
kubectl api-resources                           # resource types
kubectl get events --sort-by=.metadata.creationTimestamp
kubectl get <type> <name> -o jsonpath='{.metadata.ownerReferences[0].kind}'
kubectl -n kube-system get lease                # leader election
kubectl auth can-i <verb> <resource> [--as=…]   # permission check
kubectl get pods -w                             # watch
```

## Related materials

- Previous levels: [basics](basics.en.md), [advanced](advanced.en.md)
- Next module: [03 · Local cluster and kubectl](../03-local-cluster-and-kubectl/overview.en.md)
- Previous module: [01 · Containers and Docker](../01-containers/overview.en.md) — the runtime and CRI
- Foundation: the [Linux track](../../../linux/overview.en.md)

## Sources

- [Kubernetes Components](https://kubernetes.io/docs/concepts/overview/components/)
- [Kubernetes API concepts: resourceVersion](https://kubernetes.io/docs/reference/using-api/api-concepts/#resource-versions)
- [Finalizers](https://kubernetes.io/docs/concepts/overview/working-with-objects/finalizers/)
- [kube-apiserver: options](https://kubernetes.io/docs/reference/command-line-tools-reference/kube-apiserver/)
- [Version Skew Policy](https://kubernetes.io/releases/version-skew-policy/)
- [Kubernetes API health endpoints](https://kubernetes.io/docs/reference/using-api/health-checks/)
- [Operating etcd clusters](https://kubernetes.io/docs/tasks/administer-cluster/configure-upgrade-etcd/)
- [Scheduler: assigning a node (nodeName)](https://kubernetes.io/docs/concepts/scheduling-eviction/assign-pod-node/#nodename)
