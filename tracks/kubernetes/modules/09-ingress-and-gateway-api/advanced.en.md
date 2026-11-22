---
title: "Ingress and Gateway API: advanced"
lang: en
track: kubernetes
kind: module
doc: advanced
slug: 09-ingress-and-gateway-api
part: 2
tags: [gateway-api, httproute, traffic-splitting, canary, redirect, urlrewrite, tls, referencegrant, ingress-annotations, status-conditions]
versions: "Kubernetes 1.37.0 (kind v0.34), Gateway API CRDs v1.4.1, Envoy Gateway v1.7.4, ingress-nginx 1.15.1 (the project is retired)"
status: draft
updated: 2026-11-22
---

# Ingress and Gateway API: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on [basics](basics.en.md): installing the controllers, `Ingress`, `Gateway` and a simple `HTTPRoute` are not repeated.

## Routes with several rules

An extended `HTTPRoute` with four rules: weights, a header, a redirect and a catch-all route:

```yaml
apiVersion: gateway.networking.k8s.io/v1
kind: HTTPRoute
metadata: {name: web-route}
spec:
  parentRefs: [{name: main-gw}]
  hostnames: ["app.example.test"]
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

### Weights: a canary rollout

`weight` in `backendRefs` splits traffic between versions. 100 requests to `/canary/`:

```text
$ kubectl -n learn-09 exec curl -- sh -c "for i in \$(seq 1 100); do curl -s -H 'Host: app.example.test' http://10.96.33.43/canary/ | cut -c1-2; done | sort | uniq -c"
     81 v1
     19 v2
```

The distribution is close to 80/20 (81 and 19). By changing the weights you gradually move traffic to the new version without redeploying: it is enough to edit the `HTTPRoute`. Doing the same without Gateway API means either changing the replica counts of two Deployments (crudely, as in module 06) or using controller annotations.

The `URLRewrite` filter with `ReplacePrefixMatch: /` removed the `/canary` prefix before sending to the backend: `/canary/` reached the application as `/`.

### Matching by header

```text
$ kubectl -n learn-09 exec curl -- curl -s -H "Host: app.example.test" -H "x-version: v1" http://10.96.33.43/
v1 from web-v1-584794c796-qcsdd
$ kubectl -n learn-09 exec curl -- curl -s -H "Host: app.example.test" http://10.96.33.43/
v2 from web-v2-577d96dff8-mk2zd
```

With the header `x-version: v1` the request went to `web-v1`, without it to `web-v2`. This is how "test" routes are made for a team without affecting the other users.

### Redirect

```text
$ kubectl -n learn-09 exec curl -- curl -s -o /dev/null -D - -H "Host: app.example.test" http://10.96.33.43/old
HTTP/1.1 301 Moved Permanently
location: http://app.example.test/new
```

`RequestRedirect` returned `301` with the new address; the request did not reach the application.

The matching conditions: path (`Exact`, `PathPrefix`, `RegularExpression`), headers, query parameters, the HTTP method. When several rules match, Gateway API has a deterministic order (an exact path, a longer prefix and so on).

> ⚠️ Verify: the exact priority order of `HTTPRoute` rules and the meaning of `RegularExpression` were not verified by running; check against the [specification](https://gateway-api.sigs.k8s.io/reference/spec/).

## A TLS listener

The Gateway terminates TLS on a listener, the certificate is taken from a Secret ([module 07](../07-config-and-secrets/advanced.en.md#secret-types)):

```yaml
listeners:
  - name: http
    protocol: HTTP
    port: 80
    allowedRoutes: {namespaces: {from: Same}}
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
$ kubectl -n learn-09 get gateway main-gw -o jsonpath='{range .status.listeners[*]}{.name} attached={.attachedRoutes} {range .conditions[*]}{.type}={.status} {end}{"\n"}{end}'
http attached=1 Programmed=True Accepted=True ResolvedRefs=True
https attached=1 Programmed=True Accepted=True ResolvedRefs=True
$ kubectl -n learn-09 exec curl -- curl -sk --resolve app.example.test:443:10.96.33.43 https://app.example.test/
v2 from web-v2-577d96dff8-kfzxp
```

Each listener has its own status and a counter of attached routes (`attachedRoutes`). Now the listeners are `Programmed=True`, although the Gateway itself still has `AddressNotAssigned` (no external address). The `Terminate` mode decrypts traffic at the Gateway; `Passthrough` passes TLS through to the backend (for `TLSRoute`).

## Permissions between namespaces

A route attaches to a Gateway only if a listener allows it. A listener with `allowedRoutes: {namespaces: {from: Same}}` accepts routes only from its own namespace. A route from another namespace is rejected:

```text
$ kubectl -n learn-09b get httproute other-route -o jsonpath='{range .status.parents[0].conditions[*]}{.type}={.status} ({.reason}){"\n"}{end}'
Accepted=False (NotAllowedByListeners)
ResolvedRefs=False (BackendNotFound)
```

`Accepted=False (NotAllowedByListeners)`. The second condition (`BackendNotFound`) arose because the Service `web-v1` is looked up in the route's namespace `learn-09b`, and it does not exist there. Foreign namespaces can be allowed in `allowedRoutes` (`from: All` or `from: Selector`). A route reference to a Service in **another** namespace requires a `ReferenceGrant` object in the target namespace.

> ⚠️ Verify: `from: All`, `from: Selector` and `ReferenceGrant` were not run in this material.

## Diagnosis by status

Gateway API puts the causes right into `status`:

| Object | Condition | Meaning |
|---|---|---|
| GatewayClass | `Accepted` | the controller accepted the class |
| Gateway | `Accepted`, `Programmed` | accepted and configured (`AddressNotAssigned` — no address) |
| Listener | `Programmed`, `ResolvedRefs` | the listener is ready, references (Secret) found |
| HTTPRoute | `Accepted` | the route is attached to its parent (`NotAllowedByListeners`) |
| HTTPRoute | `ResolvedRefs` | backends found (`BackendNotFound`) |

A route to a non-existent Service is accepted, but the reference is not resolved, and the request gets `500`:

```text
$ kubectl -n learn-09 get httproute broken -o jsonpath='{range .status.parents[0].conditions[*]}{.type}={.status} ({.reason}){"\n"}{end}'
Accepted=True (Accepted)
ResolvedRefs=False (BackendNotFound)
$ kubectl -n learn-09 exec curl -- curl -s -o /dev/null -w "http=%{http_code}\n" -H "Host: broken.example.test" http://10.96.33.43/
http=500
```

Remember: **no match — `404`, backend not found — `500`**. Ingress has no such diagnostics: you have to read the controller logs.

## Ingress annotations and typed fields

Ingress capabilities beyond "host and path" are set by controller annotations. An ingress-nginx example: a path with a capture group and a rewrite to `/api/$1`:

```yaml
metadata:
  annotations: {nginx.ingress.kubernetes.io/rewrite-target: /api/$1}
spec:
  ingressClassName: nginx
  rules:
    - host: rw.test
      http:
        paths:
          - {path: /shop/(.*), pathType: ImplementationSpecific, backend: {service: {name: web-v1, port: {number: 80}}}}
```

```text
$ kubectl -n learn-09 exec curl -- curl -s -H 'Host: rw.test' http://10.96.78.112/shop/
v1 api from web-v1-584794c796-qcsdd
```

The request `/shop/` went to the backend's `/api/`. This annotation and the regular expression work only in ingress-nginx; another controller will ignore them. In Gateway API the same is set by the standard `URLRewrite`, the redirect by `RequestRedirect`, the weights by `weight`.

**An Ingress without `ingressClassName`.** An object without a class found a handler: the controller in kind is started with the flag `--watch-ingress-without-class=true`, so `noclass.test` answered `200`. Handling of objects without a class depends on the controller settings: do not rely on it, always set `ingressClassName`.

## Migrating from Ingress to Gateway API

1. Install the Gateway API CRDs and a controller.
2. Create a `GatewayClass` and `Gateway` with the same ports and certificates.
3. Move the rules: each "host + path" pair becomes an `HTTPRoute`; annotations are replaced by filters (`URLRewrite`, `RequestRedirect`, weights).
4. Check both entry points in parallel (different addresses), then switch DNS.

An `ingress2gateway` utility exists for automatic conversion.

> ⚠️ Verify: the `ingress2gateway` utility and carrying over ingress-nginx annotations were not run in this material.

## Best practices

- Build new entry points on Gateway API; keep Ingress for existing systems.
- Separate roles: the `Gateway` and certificates belong to the administrator, `HTTPRoute` to application teams.
- Restrict `allowedRoutes`: do not let routes in from any namespace.
- Check the `status` of objects (`Accepted`, `ResolvedRefs`) after every change.
- Automate TLS certificates (cert-manager, a [track tool](../../overview.en.md#tools)); do not keep keys in Git.
- For canary use `weight`, not a change in the number of replicas.

## Common mistakes

- ❌ Using one controller's annotations without knowing another will ignore them → ✅ typed Gateway API fields.
- ❌ Confusing `404` and `500` when diagnosing → ✅ 404 — no rule, 500 — backend not found (look at `ResolvedRefs`).
- ❌ Creating a route in another namespace and waiting for attachment → ✅ the listener's `allowedRoutes`.
- ❌ Referring in a route to a Service from another namespace without a `ReferenceGrant` → ✅ add the permission.
- ❌ Expecting an external IP on a Gateway in kind → ✅ use the ClusterIP or a port forward.

## Comparison with other tracks

- [Linux](../../../linux/overview.en.md): an entry controller is a reverse proxy (nginx, Envoy) with a configuration that the controller updates.
- [C#](../../../csharp/modules/14-aspnet-core-minimal-api/overview.en.md): such a proxy stands in front of an ASP.NET Core application and passes headers on; the application must trust `X-Forwarded-*` only from its own proxy.

## Related materials

- Previous level: [basics](basics.en.md)
- Next: [expert](expert.en.md)
- Previous modules: [06 · Service and networking](../06-services/overview.en.md), [05 · Deployment and ReplicaSet](../05-deployments/overview.en.md)

## Sources

- [Gateway API: HTTPRoute](https://gateway-api.sigs.k8s.io/api-types/httproute/)
- [Gateway API: Gateway](https://gateway-api.sigs.k8s.io/api-types/gateway/)
- [Gateway API: ReferenceGrant](https://gateway-api.sigs.k8s.io/api-types/referencegrant/)
- [Gateway API: specification](https://gateway-api.sigs.k8s.io/reference/spec/)
- [Ingress: TLS and pathType](https://kubernetes.io/docs/concepts/services-networking/ingress/)
- [Migrating from Ingress](https://gateway-api.sigs.k8s.io/guides/migrating-from-ingress/)
