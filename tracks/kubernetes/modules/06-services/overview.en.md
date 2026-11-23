---
title: "Service and networking"
lang: en
track: kubernetes
kind: module
doc: overview
slug: 06-services
part: 1
levels: [1, 2, 3, 4]
tags: [service, clusterip, nodeport, loadbalancer, dns, coredns, endpointslice, kube-proxy, headless-service]
versions: "Kubernetes 1.37.0 (kind v0.34, CoreDNS, kube-proxy iptables), kubectl 1.36.1"
status: draft
updated: 2026-11-22
---

# Service and networking

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

Pods are ephemeral and their IPs change with every replacement. A Service gives a set of pods a stable address (a virtual ClusterIP and a DNS name) and distributes traffic among the ready pods. Pods are selected by labels, and Kubernetes keeps the pod addresses itself in EndpointSlice objects. It is through a Service that applications inside the cluster find each other.

## Key ideas

- **A Service selects pods by labels** (`selector`) and directs traffic only to the ready (`Ready`) ones.
- **A ClusterIP is a virtual address:** it belongs to no interface; you do not "ping" it but use it by ports. The address does not change while the Service lives.
- **DNS:** CoreDNS answers for `<service>.<namespace>.svc.cluster.local`; inside a namespace the short name `web` is enough.
- **Types:** `ClusterIP` (default, inside the cluster), `NodePort` (a port on every node, 30000–32767), `LoadBalancer` (a cloud external balancer), `ExternalName` (a CNAME to an external name), headless (`clusterIP: None`, DNS returns pod addresses).
- **Balancing is done by kube-proxy** on each node with rules (iptables): the pod choice is random.
- **Selector and port mistakes give `Connection refused`,** not a clear message: check the endpoints.
- **`port` and `targetPort`:** the Service port and the container port may differ; `targetPort` can be given as a port name.

## Minimal example

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

```text
$ kubectl -n learn-06 get svc web
NAME   TYPE        CLUSTER-IP     EXTERNAL-IP   PORT(S)   AGE
web    ClusterIP   10.96.37.242   <none>        80/TCP    1s
$ kubectl -n learn-06 exec client -- wget -qO- http://web
hello from web-8678fd4948-7xm64
```

## When to use and when not

- ✅ Any interaction between applications inside the cluster: use the Service name, not a pod IP.
- ✅ `NodePort` — for experiments and access in kind; `LoadBalancer` — for exposing outside in a cloud.
- ❌ Do not connect to pods by their IP: it changes.
- ❌ For HTTP routing by paths and domains and TLS you need an Ingress or the Gateway API (module 09), not dozens of `LoadBalancer`s.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | the Service manifest, ClusterIP and endpoints, balancing, DNS names, `port-forward` to a Service |
| [2. Advanced](advanced.en.md) | NodePort, LoadBalancer, headless, ExternalName, readiness and endpoints, typical failures, environment variables, kube-proxy rules |
| [3. Expert](expert.en.md) | EndpointSlice and DNS internals, a reference of Service fields, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) with solutions |
| 5. Runnable examples | not generated yet — `/learn kubernetes 06-services level 5` |
| 6. Tests | not generated yet — `/learn kubernetes 06-services level 6` |

## Related materials

- Prerequisites: [05 · Deployment and ReplicaSet](../05-deployments/overview.en.md), [04 · Pod](../04-pods/overview.en.md)
- Next: [07 · ConfigMap and Secret](../07-config-and-secrets/overview.en.md)
- Networking components: [02 · Kubernetes architecture](../02-architecture/overview.en.md) — kube-proxy and CoreDNS
- Foundation: the [Linux track](../../../linux/overview.en.md) — iptables and networking
- Pod and network security: [17 · Security](../17-security/overview.en.md)
- Troubleshooting: [21 · Troubleshooting](../21-troubleshooting/overview.en.md)
