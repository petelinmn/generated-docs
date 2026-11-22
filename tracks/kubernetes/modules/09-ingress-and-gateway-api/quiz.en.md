---
title: "Quiz: Ingress and Gateway API"
lang: en
track: kubernetes
kind: module
doc: quiz
slug: 09-ingress-and-gateway-api
part: 2
tags: [ingress, gateway-api, httproute, ingress-controller, traffic-splitting]
status: draft
updated: 2026-11-22
---

# Quiz: Ingress and Gateway API

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 You created an Ingress but traffic does not flow. What do you check first?

- A) The image size
- B) That an entry controller runs in the cluster and `ingressClassName` matches
- C) The number of Deployment replicas
- D) The kubectl version

<details><summary>Answer</summary>

**B.** An Ingress is only a description; it is executed by the controller of its class (`kubectl get ingressclass`). Then check the rule for your `Host` and path and the Service endpoints.
→ [Basics](basics.en.md#the-controller-nothing-works-without-it)

</details>

## 2. 🟢 What does a request return with a `Host` that has no rule?

- A) `200` with the first backend
- B) `404`
- C) `500`
- D) `503`

<details><summary>Answer</summary>

**B.** In the experiment both Ingress and Gateway returned `404` for `Host: x.test` and `Host: other.test`.
→ [Basics](basics.en.md#the-request)

</details>

## 3. 🟢 What must be set in every Ingress path?

- A) `weight`
- B) `pathType`
- C) `hostname`
- D) `namespace`

<details><summary>Answer</summary>

**B.** Without it: `pathType must be specified`. The values: `Exact`, `Prefix`, `ImplementationSpecific`.
→ [Basics](basics.en.md#pathtype-is-required)

</details>

## 4. 🟢 Who creates which Gateway API objects?

- A) All objects — the application developer
- B) `GatewayClass` — the controller vendor, `Gateway` — the administrator, `HTTPRoute` — the application developer
- C) Only the administrator creates `HTTPRoute`
- D) Gateway API does not divide roles

<details><summary>Answer</summary>

**B.** The division of roles is the key difference from Ingress.
→ [Basics](basics.en.md#gateway-api)

</details>

## 5. 🟡 How do you send 20% of traffic to a new version with Gateway API?

- A) With a `canary` annotation on an Ingress
- B) With two `backendRefs` with weights in an `HTTPRoute` (for example, 80 and 20)
- C) By changing the number of replicas
- D) It is impossible

<details><summary>Answer</summary>

**B.** In the experiment 100 requests gave 81 and 19. Weights are a standard part of `HTTPRoute`.
→ [Advanced](advanced.en.md#weights-a-canary-rollout)

</details>

## 6. 🟡 An `HTTPRoute` refers to a non-existent Service. What happens?

- A) The route is rejected with `Accepted=False`
- B) The route is accepted, `ResolvedRefs=False (BackendNotFound)`, requests get `500`
- C) Requests get `404`
- D) The Gateway stops working

<details><summary>Answer</summary>

**B.** Distinguish `404` (no rule) from `500` (backend not found).
→ [Advanced](advanced.en.md#diagnosis-by-status)

</details>

## 7. 🟡 Select all correct statements

- [ ] A) A listener with `allowedRoutes: {namespaces: {from: Same}}` accepts routes only from its own namespace
- [ ] B) The annotation `nginx.ingress.kubernetes.io/rewrite-target` works in any controller
- [ ] C) `URLRewrite` is a standard Gateway API filter
- [ ] D) In kind `Programmed=False (AddressNotAssigned)` on a Gateway means the proxy does not work

<details><summary>Answer</summary>

**A, C.** B is wrong: annotations are controller-specific. D is wrong: the proxy works, there is just no external address; access is by ClusterIP, the listeners are `Programmed=True`.
→ [Advanced](advanced.en.md)

</details>

## 8. 🟡 A route from another namespace did not attach. Which condition will you see?

- A) `Accepted=True`
- B) `Accepted=False (NotAllowedByListeners)`
- C) `Programmed=False`
- D) `Ready=False`

<details><summary>Answer</summary>

**B.** The listener does not allow the foreign namespace. It can be allowed in `allowedRoutes` (`All` or `Selector`).
→ [Advanced](advanced.en.md#permissions-between-namespaces)

</details>

## 9. 🔴 Find the flaw

> "We are building a new platform on ingress-nginx with annotations: the project is mature, and security updates will come."

<details><summary>Answer</summary>

The Kubernetes community's ingress-nginx project is retired: after March 2026 there are no releases, fixes or security updates. A new platform should use Gateway API or another maintained controller, and one controller's annotations tie the configuration to it. Existing deployments work but get no patches.
→ [Basics](basics.en.md#the-controller-nothing-works-without-it)

</details>

## 10. 🔴 Open question

Explain how a request gets from a user to a pod with Gateway API, which components take part and how this differs from routing through a Service.

<details><summary>Key points of a good answer</summary>

- The controller watches `Gateway`, `HTTPRoute` and Service/EndpointSlice and builds the proxy configuration (control plane).
- The proxy (Envoy in the experiment; its own Deployment and Service per `Gateway`) accepts traffic (data plane) and sends it to pod addresses.
- Backend selection is per request (L7: host, path, headers, weights), not per connection as with kube-proxy.
- A Service is needed as the source of the endpoint list and as a ClusterIP inside the cluster; the external entry is the proxy's own Service (`LoadBalancer`).
- The status (`Accepted`, `Programmed`, `ResolvedRefs`) shows whether each step is accepted and allowed.

</details>

→ [Expert](expert.en.md#control-plane-and-data-plane)
