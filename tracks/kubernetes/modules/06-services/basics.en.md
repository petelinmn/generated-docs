---
title: "Service and networking: basics"
lang: en
track: kubernetes
kind: module
doc: basics
slug: 06-services
part: 1
tags: [service, clusterip, dns, endpointslice, load-balancing, port-forward]
versions: "Kubernetes 1.37.0 (kind v0.34, CoreDNS, kube-proxy iptables), kubectl 1.36.1"
status: draft
updated: 2026-11-22
---

# Service and networking: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [05 · Deployment and ReplicaSet](../05-deployments/overview.en.md) · **Time:** ~2 h

All examples were run in kind (Kubernetes 1.37.0), namespace `learn-06`: `kubectl create namespace learn-06`.

## Why it matters

A pod gets an IP when created and loses it when deleted. If application A calls application B by a pod IP, everything breaks at the first rollout. A Service solves this: it has a permanent address and name, and the current pods hide behind it.

## The application for experiments

A Deployment of 3 replicas. Each pod replies with its own host name, so you can see which instance answered:

```yaml
apiVersion: apps/v1
kind: Deployment
metadata: {name: web}
spec:
  replicas: 3
  selector: {matchLabels: {app: web}}
  template:
    metadata: {labels: {app: web}}
    spec:
      containers:
        - name: nginx
          image: nginx:1.29
          ports:
            - {name: http, containerPort: 80}
          command: ["sh", "-c", "echo \"hello from $(hostname)\" > /usr/share/nginx/html/index.html; exec nginx -g 'daemon off;'"]
---
apiVersion: v1
kind: Pod
metadata: {name: client}
spec:
  containers:
    - {name: c, image: alpine:3.22, command: ["sleep", "3600"]}
```

Here the container port is named `http`: a Service can refer to the name. The `client` pod is needed to reach the Service from inside the cluster.

## The Service manifest

```yaml
apiVersion: v1
kind: Service
metadata:
  name: web
spec:
  selector:
    app: web
  ports:
    - name: http
      port: 80
      targetPort: http
```

- `selector` — the labels of pods that traffic goes to (they must match the pod template labels).
- `port` — the port of the Service itself.
- `targetPort` — the container port (a number or a port name).
- The default type is `ClusterIP`.

```text
$ kubectl -n learn-06 get svc web -o wide
NAME   TYPE        CLUSTER-IP     EXTERNAL-IP   PORT(S)   AGE   SELECTOR
web    ClusterIP   10.96.37.242   <none>        80/TCP    1s    app=web
```

The ClusterIP `10.96.37.242` is taken from the range `10.96.0.0/16` (`--service-cluster-ip-range` of the API server in kind, [module 02](../02-architecture/expert.en.md#the-request-pipeline)).

## Endpoints: which pods are behind a Service

Kubernetes keeps a list of ready pod addresses for the Service:

```text
$ kubectl -n learn-06 get endpointslices -l kubernetes.io/service-name=web
NAME        ADDRESSTYPE   PORTS   ENDPOINTS                             AGE
web-574j6   IPv4          80      10.244.0.34,10.244.0.35,10.244.0.33   1s
$ kubectl -n learn-06 get endpoints web
Warning: v1 Endpoints is deprecated in v1.33+; use discovery.k8s.io/v1 EndpointSlice
NAME   ENDPOINTS                                      AGE
web    10.244.0.33:80,10.244.0.34:80,10.244.0.35:80   1s
```

These are the IPs of three pods. The `Endpoints` object is deprecated (a warning in the output), the current object is `EndpointSlice`. If the endpoints are empty, the Service has no suitable ready pods.

## Balancing

Ten requests from the `client` pod by the Service name:

```text
$ for i in $(seq 1 10); do kubectl -n learn-06 exec client -- wget -qO- -T 3 http://web; done | sort | uniq -c
      3 hello from web-8678fd4948-5p9dp
      2 hello from web-8678fd4948-7xm64
      5 hello from web-8678fd4948-hb7n7
```

The requests spread unevenly over three pods (3, 2 and 5): the pod choice is random, not round-robin. With more requests the distribution evens out.

## DNS: finding a Service by name

CoreDNS gives every Service the name `<service>.<namespace>.svc.cluster.local`. All four forms work from a pod in the same namespace:

```text
web -> hello from web-8678fd4948-7xm64
web.learn-06 -> hello from web-8678fd4948-7xm64
web.learn-06.svc -> hello from web-8678fd4948-hb7n7
web.learn-06.svc.cluster.local -> hello from web-8678fd4948-hb7n7
```

The short name `web` works thanks to the pod's DNS settings:

```text
$ kubectl -n learn-06 exec client -- cat /etc/resolv.conf
search learn-06.svc.cluster.local svc.cluster.local cluster.local
nameserver 10.96.0.10
options ndots:5
```

`nameserver 10.96.0.10` is the ClusterIP of CoreDNS itself, `search` is the list of suffixes appended to a short name. From a **different** namespace the short name does not work: you need `web.learn-06` (details in the [expert theory](expert.en.md#gotcha-the-short-name-works-only-in-its-own-namespace)).

```text
$ kubectl -n learn-06 exec client -- nslookup web.learn-06.svc.cluster.local
Name:	web.learn-06.svc.cluster.local
Address: 10.96.37.242
```

## Stable address

Let us delete all pods of the Deployment: their IPs change, the ClusterIP does not.

```text
$ kubectl -n learn-06 get svc web -o jsonpath='{.spec.clusterIP}{"\n"}'
10.96.37.242
$ kubectl -n learn-06 delete pod -l app=web --now
$ kubectl -n learn-06 get svc web -o jsonpath='{.spec.clusterIP}{"\n"}'
10.96.37.242
$ kubectl -n learn-06 get endpoints web
NAME   ENDPOINTS                                      AGE
web    10.244.0.39:80,10.244.0.40:80,10.244.0.41:80   63s
```

The Service address stayed the same, while the endpoints were updated (they were `.33`, `.34`, `.35`, they became `.39`, `.40`, `.41`).

## Access from your machine: port-forward

A ClusterIP is visible only inside the cluster. For experiments use port forwarding to a Service:

```text
$ kubectl -n learn-06 port-forward svc/web 18080:80 &
$ curl -s localhost:18080
hello from web-8678fd4948-5tgw5
```

`port-forward` to a Service picks one of the pods and connects you to it; it is a debugging tool, not a way to publish an application.

## Beginner mistakes

- ❌ Calling pods by IP → ✅ use the Service name: a pod's IP changes.
- ❌ Pinging a ClusterIP → ✅ it is a virtual address: `ping` loses 100% of packets (check a port, for example with `wget` or `nc`).
- ❌ A typo in `selector` or a wrong `targetPort` → ✅ the connection is refused (`Connection refused`); check `get endpoints`.
- ❌ Expecting the short name to work from another namespace → ✅ use `service.namespace`.
- ❌ Exposing an application outside through a ClusterIP → ✅ you need NodePort, LoadBalancer or Ingress.

## Related materials

- Next: [advanced](advanced.en.md) — NodePort, LoadBalancer, headless, failures
- Previous module: [05 · Deployment and ReplicaSet](../05-deployments/overview.en.md)
- Networking components: [02 · Kubernetes architecture](../02-architecture/overview.en.md)
- Foundation: the [Linux track](../../../linux/overview.en.md)

## Sources

- [Service](https://kubernetes.io/docs/concepts/services-networking/service/)
- [EndpointSlices](https://kubernetes.io/docs/concepts/services-networking/endpoint-slices/)
- [DNS for Services and Pods](https://kubernetes.io/docs/concepts/services-networking/dns-pod-service/)
- [Connecting Applications with Services](https://kubernetes.io/docs/tutorials/services/connect-applications-service/)
- [Use Port Forwarding to Access Applications](https://kubernetes.io/docs/tasks/access-application-cluster/port-forward-access-application-cluster/)
