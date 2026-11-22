---
title: "Ingress and Gateway API: basics"
lang: en
track: kubernetes
kind: module
doc: basics
slug: 09-ingress-and-gateway-api
part: 2
tags: [ingress, ingressclass, ingress-controller, gateway-api, gateway, httproute, tls, ingress-nginx, envoy-gateway]
versions: "Kubernetes 1.37.0 (kind v0.34), Gateway API CRDs v1.4.1, Envoy Gateway v1.7.4, ingress-nginx 1.15.1 (the project is retired)"
status: draft
updated: 2026-11-22
---

# Ingress and Gateway API: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [06 · Service and networking](../06-services/overview.en.md) · **Time:** ~3 h

All examples were run in kind (Kubernetes 1.37.0), namespace `learn-09`. The controllers are installed into a disposable cluster; their installation is described in the sections below.

## Why it matters

To expose an application outside through a [Service](../06-services/advanced.en.md#service-types) you need a `NodePort` or `LoadBalancer`. For a dozen applications this is ten balancers and ten addresses, and a Service cannot route by domain and path. An entry point (ingress) solves this: one address and one TLS termination point, while the rules describe which request goes to which Service.

## The controller: nothing works without it

Ingress and Gateway objects are only a description. It is executed by a **controller**: a component that watches the objects and configures the proxy (nginx, Envoy and others) through which traffic flows. A cluster may have several controllers; the needed one is chosen by the class field (`ingressClassName`, `gatewayClassName`). In kind there is no controller by default.

> ⚠️ **The status of ingress-nginx.** The Kubernetes community's ingress-nginx project has been retired: after March 2026 there are no releases, fixes or security updates ([the announcement](https://kubernetes.io/blog/2025/11/11/ingress-nginx-retirement/)). Existing deployments keep working but get no patches. For new projects choose Gateway API or another maintained controller. Here ingress-nginx is used only as an illustrative example of an Ingress object in a disposable cluster.

## Ingress

### Installing a controller (for experiments)

```bash
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/main/deploy/static/provider/kind/deploy.yaml
kubectl -n ingress-nginx rollout status deploy/ingress-nginx-controller
```

```text
$ kubectl get ingressclass
NAME    CONTROLLER             PARAMETERS   AGE
nginx   k8s.io/ingress-nginx   <none>       49s
```

An `IngressClass` links a class to a controller. The manifest used controller version 1.15.1.

### The application and a rule

Two versions of a simple web server from [module 06](../06-services/basics.en.md#the-application-for-experiments) (`web-v1` and `web-v2`, each with its own Service) and this Ingress:

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata: {name: web-ing}
spec:
  ingressClassName: nginx
  rules:
    - host: ing.example.test
      http:
        paths:
          - {path: /api, pathType: Prefix, backend: {service: {name: web-v1, port: {number: 80}}}}
          - {path: /, pathType: Prefix, backend: {service: {name: web-v2, port: {number: 80}}}}
```

- `ingressClassName` — which controller serves the object.
- `rules[].host` — the domain (the request's `Host` field).
- `paths[]` — the path, the match type (`pathType`) and a Service with a port.

```text
$ kubectl -n learn-09 get ingress web-ing
NAME      CLASS   HOSTS              ADDRESS   PORTS   AGE
web-ing   nginx   ing.example.test             80      15s
$ kubectl -n learn-09 describe ingress web-ing
Rules:
  Host              Path  Backends
  ----              ----  --------
  ing.example.test
                    /api   web-v1:80 (10.244.0.27:80,10.244.0.28:80)
                    /      web-v2:80 (10.244.0.29:80,10.244.0.30:80)
```

The `describe` output shows the pod addresses behind each Service (endpoints, [module 06](../06-services/basics.en.md#endpoints-which-pods-are-behind-a-service)).

### The request

A disposable kind has no external IP, so we call the controller's Service from inside the cluster and set the `Host` header by hand:

```text
$ kubectl -n learn-09 exec curl -- sh -c "curl -s -H 'Host: ing.example.test' http://10.96.78.112/; curl -s -H 'Host: ing.example.test' http://10.96.78.112/api/; curl -s -o /dev/null -w 'wronghost=%{http_code}\n' -H 'Host: x.test' http://10.96.78.112/"
v2 from web-v2-577d96dff8-mk2zd
v1 api from web-v1-584794c796-qxvt5
wronghost=404
```

The path `/` went to `web-v2`, `/api/` to `web-v1`, a request with a foreign `Host` got `404`: there is no rule. The domain is determined by `Host`, so a DNS name must point to the entry address (in experiments it is supplied by a header).

### pathType is required

```text
$ kubectl -n learn-09 apply -f nopt.yaml
The Ingress "nopt" is invalid: spec.rules[0].http.paths[0].pathType: Required value: pathType must be specified
```

The types: `Exact` (an exact match), `Prefix` (by prefix), `ImplementationSpecific` (up to the controller).

### TLS

The certificate is stored in a Secret of type `kubernetes.io/tls` ([module 07](../07-config-and-secrets/advanced.en.md#secret-types)):

```yaml
spec:
  ingressClassName: nginx
  tls: [{hosts: [app.example.test], secretName: app-tls}]
  rules: [{host: app.example.test, http: {paths: [{path: /, pathType: Prefix, backend: {service: {name: web-v2, port: {number: 80}}}}]}}]
```

```text
$ kubectl -n learn-09 exec curl -- curl -sk --resolve app.example.test:443:10.96.78.112 https://app.example.test/
v2 from web-v2-577d96dff8-kfzxp
$ kubectl -n learn-09 exec curl -- curl -sk -o /dev/null -w "http->%{http_code} redirect=%{redirect_url}\n" -H 'Host: app.example.test' http://10.96.78.112/
http->308 redirect=https://app.example.test/
```

HTTPS works (`-k` because the certificate is self-signed), and an HTTP request is redirected to HTTPS by the ingress-nginx controller by default with code `308`. This is the behavior of a specific controller.

## Gateway API

Gateway API is the next generation of the entry API. The key objects:

| Object | Who creates it | What it describes |
|---|---|---|
| `GatewayClass` | the controller vendor | which controller implements the Gateway |
| `Gateway` | the cluster administrator | an entry point: ports, protocols, TLS, who may attach |
| `HTTPRoute` | the application developer | HTTP routing rules to Services |

### Installing a controller (Envoy Gateway)

```bash
kubectl apply --server-side -f https://github.com/envoyproxy/gateway/releases/download/v1.7.4/install.yaml
kubectl -n envoy-gateway-system rollout status deploy/envoy-gateway
```

The manifest installs both the Gateway API CRDs (the set version in it is `v1.4.1`) and the controller:

```text
$ kubectl get crd | grep gateway.networking.k8s.io
backendtlspolicies, gatewayclasses, gateways, grpcroutes, httproutes, referencegrants, tcproutes, tlsroutes, udproutes
```

### GatewayClass and Gateway

```yaml
apiVersion: gateway.networking.k8s.io/v1
kind: GatewayClass
metadata: {name: eg}
spec:
  controllerName: gateway.envoyproxy.io/gatewayclass-controller
---
apiVersion: gateway.networking.k8s.io/v1
kind: Gateway
metadata: {name: main-gw}
spec:
  gatewayClassName: eg
  listeners:
    - name: http
      protocol: HTTP
      port: 80
      allowedRoutes: {namespaces: {from: Same}}
```

```text
$ kubectl get gatewayclass
NAME   CONTROLLER                                      ACCEPTED   AGE
eg     gateway.envoyproxy.io/gatewayclass-controller   True       20s
$ kubectl -n learn-09 get gateway
NAME      CLASS   ADDRESS   PROGRAMMED   AGE
main-gw   eg                False        47s
$ kubectl -n learn-09 get gateway main-gw -o jsonpath='{range .status.conditions[*]}{.type}={.status} ({.reason}){"\n"}{end}'
Accepted=True (Accepted)
Programmed=False (AddressNotAssigned)
```

The `Gateway` is accepted (`Accepted`) but `Programmed=False` with the reason `AddressNotAssigned`: the controller created a `LoadBalancer` Service for it, and kind has no cloud to issue an address. The proxy itself works, and it can be reached through the ClusterIP of that Service ([module 06](../06-services/advanced.en.md#loadbalancer)). A listener in `listeners[]` is a port and protocol; `allowedRoutes` says from which namespaces routes are allowed (`Same` — from the same one).

### HTTPRoute

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

- `parentRefs` — which `Gateway` the route is attached to.
- `hostnames` — domains (the `Host` field).
- `rules[].matches` — conditions (path, headers), `backendRefs` — where to send.

```text
$ kubectl -n learn-09 get httproute web-route -o jsonpath='{range .status.parents[0].conditions[*]}{.type}={.status} ({.reason}){"\n"}{end}'
Accepted=True (Accepted)
ResolvedRefs=True (ResolvedRefs)
```

For each `Gateway` the controller creates its own Deployment and Service with a proxy: `envoy-learn-09-main-gw-2dc8f82a` (a `LoadBalancer` Service, `EXTERNAL-IP <pending>`). Requests to its ClusterIP:

```text
$ kubectl -n learn-09 exec client -- wget -qO- --header "Host: app.example.test" http://10.96.33.43/
v2 from web-v2-577d96dff8-mk2zd
$ kubectl -n learn-09 exec client -- wget -qO- --header "Host: app.example.test" http://10.96.33.43/api/
v1 api from web-v1-584794c796-qxvt5
$ kubectl -n learn-09 exec client -- wget -S -qO- --header "Host: other.test" http://10.96.33.43/
  HTTP/1.1 404 Not Found
```

`/` → `web-v2`, `/api/` → `web-v1`, a foreign `Host` → `404`.

## Beginner mistakes

- ❌ Creating an Ingress or Gateway and expecting traffic without a controller → ✅ install a controller and check the `IngressClass`/`GatewayClass`.
- ❌ Forgetting `pathType` in an Ingress → ✅ it is required (`Required value: pathType must be specified`).
- ❌ Testing an entry point without the `Host` header → ✅ rules are bound to a domain: `curl -H 'Host: …'` or `--resolve`.
- ❌ Considering `Programmed=False` on a Gateway in kind a breakage → ✅ there is no cloud balancer; use the ClusterIP or forward a port.
- ❌ Building a new system on ingress-nginx → ✅ the project is not maintained; choose Gateway API.

## Related materials

- Next: [advanced](advanced.en.md) — weights, redirects, rewrite, a TLS listener, statuses
- Previous modules: [06 · Service and networking](../06-services/overview.en.md), [07 · ConfigMap and Secret](../07-config-and-secrets/overview.en.md)
- Foundation: the [Linux track](../../../linux/overview.en.md)

## Sources

- [Ingress](https://kubernetes.io/docs/concepts/services-networking/ingress/)
- [Ingress Controllers](https://kubernetes.io/docs/concepts/services-networking/ingress-controllers/)
- [Gateway API](https://kubernetes.io/docs/concepts/services-networking/gateway/)
- [Gateway API: project documentation](https://gateway-api.sigs.k8s.io/)
- [Ingress NGINX Retirement: What You Need to Know](https://kubernetes.io/blog/2025/11/11/ingress-nginx-retirement/)
- [Envoy Gateway: Quickstart](https://gateway.envoyproxy.io/docs/tasks/quickstart/)
