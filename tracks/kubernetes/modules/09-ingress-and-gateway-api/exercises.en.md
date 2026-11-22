---
title: "Exercises: Ingress and Gateway API"
lang: en
track: kubernetes
kind: module
doc: exercises
slug: 09-ingress-and-gateway-api
part: 2
tags: [ingress, gateway-api, httproute, tls, traffic-splitting, ingress-controller]
status: draft
updated: 2026-11-22
---

# Exercises: Ingress and Gateway API

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. Solutions are under spoilers. You need a disposable kind cluster and the namespace `learn-09`. The application (`web-v1`, `web-v2`, their Services and the pods `client`, `curl`) is from [basics](basics.en.md#ingress). The controller installation is described there too. Clean up afterwards: `kubectl delete namespace learn-09 learn-09b` and delete the cluster.

## 1. 🟢 The first Ingress

**Task:** install the ingress-nginx controller (a disposable cluster) and create an Ingress for the domain `ing.example.test`: path `/api` → `web-v1`, `/` → `web-v2`. Check three requests: `/`, `/api/` and a foreign `Host`.

**Expected result:** replies from `web-v2`, `web-v1` and `404`.

<details><summary>Hint</summary>

Call the controller's Service from inside the cluster (`kubectl -n ingress-nginx get svc`) and set the `Host` header.

</details>

<details><summary>Solution</summary>

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

```text
v2 from web-v2-577d96dff8-mk2zd
v1 api from web-v1-584794c796-qxvt5
wronghost=404
```

The domain is determined by the `Host` header: a request with another domain finds no rules.

</details>

## 2. 🟢 Ingress: errors and TLS

**Task:** (a) create an Ingress without `pathType` and explain the error. (b) Create a TLS secret (`openssl`, `kubectl create secret tls app-tls`) and an Ingress with `tls` for `app.example.test`; check HTTPS and what an HTTP request does.

**Expected result:** (a) `pathType must be specified`; (b) HTTPS answers, HTTP gets `308` to HTTPS.

<details><summary>Hint</summary>

A self-signed certificate: `openssl req -x509 -nodes -newkey rsa:2048 -keyout tls.key -out tls.crt -subj "/CN=app.example.test" -addext "subjectAltName=DNS:app.example.test"`. Check HTTPS with `curl -k --resolve`.

</details>

<details><summary>Solution</summary>

```text
The Ingress "nopt" is invalid: spec.rules[0].http.paths[0].pathType: Required value: pathType must be specified
```

```yaml
spec:
  ingressClassName: nginx
  tls: [{hosts: [app.example.test], secretName: app-tls}]
  rules: [{host: app.example.test, http: {paths: [{path: /, pathType: Prefix, backend: {service: {name: web-v2, port: {number: 80}}}}]}}]
```

```text
$ curl -sk --resolve app.example.test:443:<controller IP> https://app.example.test/
v2 from web-v2-577d96dff8-kfzxp
$ curl -sk -o /dev/null -w "http->%{http_code} redirect=%{redirect_url}\n" -H 'Host: app.example.test' http://<IP>/
http->308 redirect=https://app.example.test/
```

`pathType` is required; the ingress-nginx controller redirects HTTP to HTTPS when `tls` is present (a controller behavior).

</details>

## 3. 🟡 Gateway and HTTPRoute

**Task:** install Envoy Gateway v1.7.4, create a `GatewayClass`, a `Gateway` on port 80 and an `HTTPRoute` for `app.example.test` (`/api` → `web-v1`, `/` → `web-v2`). Show the statuses of the `Gateway` and `HTTPRoute` and explain `Programmed=False`.

**Expected result:** `Accepted=True`, `Programmed=False (AddressNotAssigned)`; the route `Accepted=True`, `ResolvedRefs=True`; requests to the proxy's ClusterIP are answered.

<details><summary>Hint</summary>

`kubectl apply --server-side -f https://github.com/envoyproxy/gateway/releases/download/v1.7.4/install.yaml`. Look for the proxy Service in `envoy-gateway-system` by the label `gateway.envoyproxy.io/owning-gateway-name=main-gw`.

</details>

<details><summary>Solution</summary>

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
Accepted=True (Accepted)
Programmed=False (AddressNotAssigned)
```

`Programmed=False`: kind has no cloud to issue an address to a `LoadBalancer` Service; the proxy works, and requests with `Host: app.example.test` to the ClusterIP of the Service `envoy-learn-09-main-gw-…` are answered by `web-v2` and `web-v1` (for `/api/`).

</details>

## 4. 🟡 Weights, a header, a redirect

**Task:** extend the `HTTPRoute`: `/canary` splits traffic 80/20 between `web-v1` and `web-v2` and removes the `/canary` prefix; a request with the header `x-version: v1` goes to `web-v1`; `/old` redirects to `/new` with code 301. Check each behavior.

**Expected result:** about 80 and 20 out of 100 requests; a `web-v1` reply by the header; `301` with `location`.

<details><summary>Hint</summary>

Weights — `weight` in `backendRefs`; rewrite — `URLRewrite` with `ReplacePrefixMatch`; redirect — `RequestRedirect`.

</details>

<details><summary>Solution</summary>

```yaml
rules:
  - matches: [{path: {type: PathPrefix, value: /canary}}]
    filters:
      - type: URLRewrite
        urlRewrite: {path: {type: ReplacePrefixMatch, replacePrefixMatch: /}}
    backendRefs:
      - {name: web-v1, port: 80, weight: 80}
      - {name: web-v2, port: 80, weight: 20}
  - matches: [{headers: [{name: x-version, value: v1}]}]
    backendRefs: [{name: web-v1, port: 80}]
  - matches: [{path: {type: Exact, value: /old}}]
    filters:
      - type: RequestRedirect
        requestRedirect: {path: {type: ReplaceFullPath, replaceFullPath: /new}, statusCode: 301}
  - matches: [{path: {type: PathPrefix, value: /}}]
    backendRefs: [{name: web-v2, port: 80}]
```

```text
     81 v1
     19 v2
v1 from web-v1-584794c796-qcsdd
HTTP/1.1 301 Moved Permanently
location: http://app.example.test/new
```

The weight distribution is random but on average matches 80/20; the header and the redirect work with standard fields, without annotations.

</details>

## 5. 🔴 TLS, permissions and errors by status

**Task:** (a) add an HTTPS listener with a certificate from a Secret and check HTTPS. (b) Create an `HTTPRoute` in another namespace with `parentRefs` to the Gateway and explain the status. (c) Create a route to a non-existent Service and show what the client gets.

**Expected result:** (a) both listeners `Programmed=True`; (b) `Accepted=False (NotAllowedByListeners)`; (c) `ResolvedRefs=False (BackendNotFound)` and `http=500`.

<details><summary>Hint</summary>

TLS: `tls: {mode: Terminate, certificateRefs: [{kind: Secret, name: app-tls}]}`. For (b) set `namespace` in `parentRefs`. Statuses are in `status.parents[0].conditions`.

</details>

<details><summary>Solution</summary>

```yaml
listeners:
  - name: https
    protocol: HTTPS
    port: 443
    hostname: app.example.test
    tls:
      mode: Terminate
      certificateRefs: [{kind: Secret, name: app-tls}]
    allowedRoutes: {namespaces: {from: Same}}
```

```text
http attached=1 Programmed=True Accepted=True ResolvedRefs=True
https attached=1 Programmed=True Accepted=True ResolvedRefs=True
Accepted=False (NotAllowedByListeners)
ResolvedRefs=False (BackendNotFound)          # for the route to a non-existent Service
http=500
```

The listener allows only its own namespace, so a foreign route is not attached. A route to a non-existent Service is accepted, but the reference is not resolved, and the client gets `500`.

</details>
