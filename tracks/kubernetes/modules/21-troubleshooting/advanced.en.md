---
title: "Troubleshooting: advanced"
lang: en
track: kubernetes
kind: module
doc: advanced
slug: 21-troubleshooting
part: 3
tags: [troubleshooting, endpoints, dns, kubectl-debug, ephemeral-containers, finalizer, resourcequota, notready, readiness]
versions: "Kubernetes 1.37.0 (kind v0.34, 3 nodes), kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Troubleshooting: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on [basic theory](basics.en.md). Experiments were run in kind on three nodes, namespace `learn-21`.

## A Service without endpoints

If a Service does not respond, first look at its endpoints: these are the addresses of the pods the Service really serves ([module 06](../06-services/overview.en.md)). Three Services for one Deployment `web` (nginx, 2 replicas):

```yaml
apiVersion: v1
kind: Service
metadata: {name: web}                # the right selector
spec: {selector: {app: web}, ports: [{port: 80}]}
---
metadata: {name: web-typo}           # a typo in the selector
spec: {selector: {app: wbe}, ports: [{port: 80}]}
---
metadata: {name: web-port}           # a wrong targetPort
spec: {selector: {app: web}, ports: [{port: 80, targetPort: 8080}]}
```

```text
$ kubectl -n learn-21 get endpoints web web-typo web-port
NAME       ENDPOINTS                         AGE
web        10.244.1.6:80,10.244.2.6:80       26s
web-typo   <none>                            26s
web-port   10.244.1.6:8080,10.244.2.6:8080   26s
$ for s in web web-typo web-port; do kubectl -n learn-21 exec client -- wget -q -T 3 -O /dev/null http://$s; done
web: OK
web-typo: wget: can't connect to remote host (10.96.75.197): Connection refused
web-port: wget: can't connect to remote host (10.96.20.92): Connection refused
```

- **A typo in the selector:** `ENDPOINTS <none>`, the connection is refused.
- **A wrong `targetPort`:** there are endpoints, but with port `8080` that nobody listens on: the same `Connection refused`. Compare `targetPort` with the container port.
- The output of `kubectl get endpoints` carries a warning that `Endpoints` has been deprecated since 1.33 in favor of `EndpointSlice` (`kubectl get endpointslices`).

### Unready pods do not get into endpoints

If pods have a readiness probe and it fails, they are `0/1` and are not in the endpoints ([module 11](../11-probes-and-resources/basics.en.md)). Experiment: a Deployment `web` with a working probe `GET /` had the path changed to `/ready` (nginx has no such path):

```text
$ kubectl -n learn-21 get pods -l app=web
web-559df6b76c-hjf7d   1/1   Running   0   39s     ← old pods
web-559df6b76c-jbcqh   1/1   Running   0   38s
web-58dd899764-fkq7r   0/1   Running   0   20s     ← the new pod, the probe fails
$ kubectl -n learn-21 get deploy web
NAME   READY   UP-TO-DATE   AVAILABLE
web    2/2     1            2
$ kubectl -n learn-21 rollout status deploy/web --timeout=10s
Waiting for deployment "web" rollout to finish: 1 out of 2 new replicas have been updated...
error: timed out waiting for the condition
$ kubectl -n learn-21 get events --field-selector reason=Unhealthy
Readiness probe failed: HTTP probe failed with statuscode: 404
```

The rolling update does not continue until the new pod becomes ready, while the old pods keep working and serving traffic (the endpoints stayed the same). The cause is in the `Unhealthy` event: code `404`. To let the rollout finish, return a working probe path (with the working probe `/` the previous rollout completed successfully: `successfully rolled out`).

## DNS

Applications find services by name ([module 06](../06-services/overview.en.md)). A check from a pod:

```text
$ kubectl -n learn-21 exec client -- nslookup web.learn-21.svc.cluster.local
Server:		10.96.0.10
Address:	10.96.0.10:53

Name:	web.learn-21.svc.cluster.local
Address: 10.96.175.169

$ kubectl -n learn-21 exec client -- nslookup nosuch.learn-21.svc.cluster.local
** server can't find nosuch.learn-21.svc.cluster.local: NXDOMAIN

$ kubectl -n learn-21 exec client -- cat /etc/resolv.conf
search learn-21.svc.cluster.local svc.cluster.local cluster.local
nameserver 10.96.0.10
options ndots:5
```

- `Server: 10.96.0.10` is the CoreDNS Service (`kube-dns`).
- The name of a nonexistent Service gives `NXDOMAIN`.
- `search` and `ndots:5` let you write the short name `web` inside a namespace: the resolver appends the suffixes in turn. That is why a query for an external name first goes through several failed lookups.

If DNS does not work at all: check the CoreDNS pods in `kube-system`, `resolv.conf` in the pod and [NetworkPolicy](../17-security/advanced.en.md#egress-and-dns) (egress without DNS).

## `kubectl debug`: looking inside

An image often has no shell (`distroless`), and `exec` does not help. `kubectl debug` adds a **temporary container** (ephemeral) with the tools you need to a running pod:

```text
$ kubectl -n learn-21 debug pod/web-… --image=busybox:1.37 --target=nginx -c dbg -- sleep 300
$ kubectl -n learn-21 exec web-… -c dbg -- sh -c 'ps | head -5; ls /proc/1/root/etc/nginx | head -3; wget -qO- -T2 localhost | head -2'
PID   USER     TIME  COMMAND
    1 root      0:00 nginx: master process nginx -g daemon off;
   33 101       0:00 nginx: worker process
conf.d  fastcgi_params  mime.types
<!DOCTYPE html>
<html>
$ kubectl -n learn-21 get pod web-… -o jsonpath='{.spec.ephemeralContainers[0].name}'
dbg
```

`--target=nginx` shares the process namespace with the target container: its processes are visible (`nginx: master process`), and its filesystem is reachable through `/proc/1/root`. The containers of a pod share the network, so `wget localhost` reaches nginx itself.

The same trick works for a node: a pod is created with the node's filesystem mounted at `/host`:

```text
$ kubectl -n learn-21 debug node/learn-worker --image=busybox:1.37 -- sh -c 'ls /host/etc/kubernetes; cat /host/etc/hostname'
Creating debugging pod node-debugger-learn-worker-rvvxh with container debugger on node learn-worker.
kubelet.conf
manifests
pki
learn-worker
```

The debugging pod stays in the namespace after it finishes: delete it (`kubectl delete pod node-debugger-…`). An ephemeral container cannot be removed from a pod, it disappears with the pod.

> ⚠️ Verify: interactive work (`-it`), `--copy-to`, `--share-processes` and profiles (`--profile`) were not run in the experiments.

## An object stuck in Terminating

If an object has a finalizer, deletion only sets `deletionTimestamp`, and the object stays until the finalizer is removed ([module 19](../19-crd-and-operators/expert.en.md#internals)):

```text
$ kubectl -n learn-21 create cm fin
$ kubectl -n learn-21 patch cm fin -p '{"metadata":{"finalizers":["example.com/block"]}}'
$ kubectl -n learn-21 delete cm fin --wait=false
configmap "fin" deleted from learn-21 namespace
$ kubectl -n learn-21 get cm fin -o jsonpath='{.metadata.deletionTimestamp}{" "}{.metadata.finalizers}'
2026-11-23T13:57:20Z ["example.com/block"]
$ kubectl -n learn-21 patch cm fin --type=merge -p '{"metadata":{"finalizers":null}}'
$ kubectl -n learn-21 get cm fin
Error from server (NotFound): configmaps "fin" not found
```

`kubectl delete` reported "deleted", yet the object stayed with a `deletionTimestamp`. First find out **which controller** sets the finalizer and why it does not remove it; removing it by hand (`finalizers: null`) is justified only when the controller is gone or cleanup is not needed.

## Quota: ReplicaFailure

A `ResourceQuota` limits the number of objects in a namespace. When the limit is used up, a Deployment cannot create pods:

```text
$ kubectl -n learn-21 create deploy qd --image=nginx:1.29 --replicas=5
$ kubectl -n learn-21 get deploy qd
NAME   READY   UP-TO-DATE   AVAILABLE
qd     0/5     0            0
$ kubectl -n learn-21 get events --field-selector reason=FailedCreate
Error creating: pods "qd-5c6ccb5cd9-6stpj" is forbidden: exceeded quota: q, requested: pods=1, used: pods=4, limited: pods=4
$ kubectl -n learn-21 get deploy qd -o jsonpath='{.status.conditions[?(@.type=="ReplicaFailure")]}'
{"message":"pods \"qd-…\" is forbidden: exceeded quota: q, requested: pods=1, used: pods=4, limited: pods=4","reason":"FailedCreate","status":"True","type":"ReplicaFailure"}
```

There are no pods, and in the Deployment itself the cause is visible only in the `ReplicaFailure` condition (`reason: FailedCreate`) and in the ReplicaSet events. A rejection by Pod Security Admission gives the same condition ([module 17](../17-security/advanced.en.md#pod-security-admission-modes-in-practice)). Quotas are in [module 10](../10-namespaces-and-rbac/overview.en.md).

## A `NotReady` node

Experiment: the container of the node `learn-worker2` was stopped (`docker stop`), and two Deployment pods ran on it.

```text
t=48  learn-worker2=Ready
t=60  learn-worker2=NotReady        ← about 50 seconds after the stop
$ kubectl get node learn-worker2 -o jsonpath='{.spec.taints}'
[{"effect":"NoSchedule","key":"node.kubernetes.io/unreachable","timeAdded":"…"},
 {"effect":"NoExecute","key":"node.kubernetes.io/unreachable","timeAdded":"…"}]
$ kubectl describe node learn-worker2 | grep -A5 ^Conditions
  Ready  Unknown  NodeStatusUnknown  Kubelet stopped posting node status.
  (MemoryPressure, DiskPressure, PIDPressure — also Unknown)
$ kubectl -n learn-21 get pods -o wide        # ~84 s after the stop
spread-…  1/1  Running  learn-worker2
```

The node controller, receiving no messages from the kubelet, marked the node `NotReady` (the conditions are `Unknown`) and set the `unreachable` taints. The pods on the node are **not evicted yet**: by default they have a 300-second toleration ([module 18](../18-scheduling/advanced.en.md#noexecute-and-tolerationseconds)), and `kubectl` keeps showing them as `Running`. After `docker start` the node returned to `Ready` and the taints disappeared.

> ⚠️ Verify: the eviction of pods from an unreachable node after 300 seconds was not observed in this experiment, since it was stopped earlier.

## Best practices

- Facts first: `get`, `describe`, events, logs — and only then changes.
- Check related objects: Service → endpoints → pods → nodes; pod → PVC → StorageClass.
- Temporary containers instead of extending the image with tools.
- Do not remove finalizers blindly.
- Keep an image with tools (`busybox`, `nicolaka/netshoot`) at hand — not run here, but common.

## Common mistakes

- ❌ A typo in a Service selector → ✅ `get endpoints` (empty).
- ❌ A `targetPort` that does not match the container port → ✅ compare.
- ❌ Expecting a rolling update to finish with a failing readiness probe → ✅ `Unhealthy` events.
- ❌ Looking for the cause in the Deployment when there are no pods → ✅ `ReplicaFailure`, ReplicaSet events.
- ❌ Forgetting to delete the node debugging pod → ✅ `kubectl delete pod node-debugger-…`.

## Related materials

- Next: [expert theory](expert.en.md)
- Service and DNS: [06 · Service and networking](../06-services/overview.en.md)
- Probes: [11 · Probes and resources](../11-probes-and-resources/overview.en.md)
- Taints and nodes: [18 · Pod scheduling](../18-scheduling/overview.en.md)
- Finalizers: [19 · CRDs and operators](../19-crd-and-operators/overview.en.md)

## Sources

- [Debug Services](https://kubernetes.io/docs/tasks/debug/debug-application/debug-service/)
- [Debugging DNS Resolution](https://kubernetes.io/docs/tasks/administer-cluster/dns-debugging-resolution/)
- [Debug Running Pods (ephemeral containers)](https://kubernetes.io/docs/tasks/debug/debug-application/debug-running-pod/)
- [Debugging a Kubernetes node with kubectl debug](https://kubernetes.io/docs/tasks/debug/debug-cluster/kubectl-node-debug/)
- [Finalizers](https://kubernetes.io/docs/concepts/overview/working-with-objects/finalizers/)
- [Resource Quotas](https://kubernetes.io/docs/concepts/policy/resource-quotas/)
