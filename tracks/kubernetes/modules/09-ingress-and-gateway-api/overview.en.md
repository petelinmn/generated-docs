---
title: "Ingress and Gateway API"
lang: en
track: kubernetes
kind: module
doc: overview
slug: 09-ingress-and-gateway-api
part: 2
levels: [1, 2, 3, 4]
tags: [ingress, gateway-api, httproute, gateway, ingress-controller, envoy-gateway, ingress-nginx, tls, routing]
versions: "Kubernetes 1.37.0 (kind v0.34), Gateway API CRDs v1.4.1, Envoy Gateway v1.7.4, ingress-nginx 1.15.1 (the project is retired)"
status: draft
updated: 2026-11-22
---

# Ingress and Gateway API

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

A [Service](../06-services/overview.en.md) gives a stable address inside the cluster, but it cannot route HTTP by domains and paths or terminate TLS. For incoming HTTP(S) traffic there are two APIs. **Ingress** is the old and simple one: rules "host and path → Service". **Gateway API** is its successor: roles are separated (`GatewayClass`, `Gateway`, `HTTPRoute`), and capabilities such as weights, redirects and path rewriting are described by typed fields rather than annotations. The objects themselves do nothing: a **controller** is needed, which turns them into a proxy configuration.

## Key ideas

- **One entry point instead of many `LoadBalancer`s.** One balancer and one IP serve many applications, routing by `Host` and path.
- **An object without a controller is useless.** An Ingress or Gateway is created and accepted by the API, but traffic flows only if a controller (ingress-nginx, Envoy Gateway, Traefik and others) runs in the cluster.
- **Ingress is frozen, Gateway API evolves.** The ingress-nginx project has not received updates since March 2026; the community recommends Gateway API or another controller.
- **Gateway API divides roles:** a cluster administrator creates the `GatewayClass` and `Gateway`, an application developer creates the `HTTPRoute` in their namespace.
- **The status of objects tells what is wrong:** conditions `Accepted`, `Programmed`, `ResolvedRefs` with reasons (`NotAllowedByListeners`, `BackendNotFound`).
- **No match — 404, no backend — 500.**
- **Ingress annotations depend on the controller;** in Gateway API the same things (redirect, rewrite, weights) are part of the standard.

## Minimal example

```yaml
apiVersion: gateway.networking.k8s.io/v1
kind: HTTPRoute
metadata: {name: web-route}
spec:
  parentRefs: [{name: main-gw}]
  hostnames: ["app.example.test"]
  rules:
    - matches: [{path: {type: PathPrefix, value: /api}}]
      backendRefs: [{name: web-v1, port: 80}]
    - matches: [{path: {type: PathPrefix, value: /}}]
      backendRefs: [{name: web-v2, port: 80}]
```

```text
$ kubectl -n learn-09 exec client -- wget -qO- --header "Host: app.example.test" http://10.96.33.43/api/
v1 api from web-v1-584794c796-qxvt5
```

## When to use and when not

- ✅ Several HTTP applications behind one address: routing by domain and path, TLS at the entry.
- ✅ Canary rollouts by weights and by headers (Gateway API).
- ❌ Non-HTTP protocols (plain TCP, databases): use a `Service` of type `LoadBalancer` or Gateway API's `TCPRoute`/`UDPRoute`.
- ❌ For new projects do not build the architecture on ingress-nginx annotations: the project is not maintained, choose Gateway API.
- ❌ Internal traffic between pods does not need to go through an Ingress: a Service handles that.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | why an entry point is needed, the controller and IngressClass, an Ingress with hosts and paths, TLS, the first Gateway and HTTPRoute |
| [2. Advanced](advanced.en.md) | matching by path and headers, weights, redirect, rewrite, a TLS listener, permissions between namespaces, diagnosis by status, migrating from Ingress |
| [3. Expert](expert.en.md) | controller and data plane internals, Gateway API resources, a reference of fields and filters, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) with solutions |
| 5. Runnable examples | not generated yet — `/learn kubernetes 09-ingress-and-gateway-api level 5` |
| 6. Tests | not generated yet — `/learn kubernetes 09-ingress-and-gateway-api level 6` |

## Related materials

- Prerequisites: [06 · Service and networking](../06-services/overview.en.md), [07 · ConfigMap and Secret](../07-config-and-secrets/overview.en.md) (TLS secrets)
- Next: [10 · Namespaces and RBAC](../10-namespaces-and-rbac/overview.en.md)
- Rollouts and canary: [05 · Deployment and ReplicaSet](../05-deployments/overview.en.md)
- Foundation: the [Linux track](../../../linux/overview.en.md) — networking, TLS, proxies
