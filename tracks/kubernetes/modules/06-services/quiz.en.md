---
title: "Quiz: Service and networking"
lang: en
track: kubernetes
kind: module
doc: quiz
slug: 06-services
part: 1
tags: [service, clusterip, dns, nodeport, headless-service, endpointslice]
status: draft
updated: 2026-11-22
---

# Quiz: Service and networking

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 Why is a Service needed if every pod has its own IP?

- A) A pod IP cannot be used inside the cluster
- B) A pod IP changes on every replacement, while a Service has a stable address and name
- C) A Service speeds up the network
- D) A Service encrypts traffic

<details><summary>Answer</summary>

**B.** In the experiment after deleting the pods their IPs changed (`.33–.35` → `.39–.41`), while the Service ClusterIP stayed `10.96.37.242`.
→ [Basics](basics.en.md#stable-address)

</details>

## 2. 🟢 Which pods receive a Service's traffic?

- A) All pods of the namespace
- B) Pods whose labels match `selector` and which are ready (`Ready`)
- C) Only pods on the same node
- D) Pods whose name matches the Service name

<details><summary>Answer</summary>

**B.** The addresses of ready pods with matching labels get into the EndpointSlice.
→ [Basics](basics.en.md#endpoints-which-pods-are-behind-a-service)

</details>

## 3. 🟢 What is the DNS name of the Service `web` in the namespace `learn-06`?

- A) `web.learn-06.svc.cluster.local`
- B) `learn-06.web.cluster`
- C) `web.default.svc.cluster.local`
- D) `service.web.local`

<details><summary>Answer</summary>

**A.** The format is `<service>.<namespace>.svc.cluster.local`; inside the namespace `web` also works.
→ [Basics](basics.en.md#dns-finding-a-service-by-name)

</details>

## 4. 🟢 How do `port` and `targetPort` differ?

- A) They do not
- B) `port` is the Service port, `targetPort` is the container port
- C) `port` is the node port, `targetPort` is the Service port
- D) `targetPort` is needed only for NodePort

<details><summary>Answer</summary>

**B.** A client connects to `port`, and traffic reaches the pod's `targetPort` (a number or a port name). The node port is `nodePort`.
→ [Basics](basics.en.md#the-service-manifest)

</details>

## 5. 🟡 What does `kubectl get svc` show for a `LoadBalancer` in kind without extra tools?

- A) An external IP after a minute
- B) `EXTERNAL-IP <pending>` and an allocated NodePort
- C) An error on creation
- D) `EXTERNAL-IP localhost`

<details><summary>Answer</summary>

**B.** The external address is issued by a cloud controller, and kind has none; in the experiment `80:31708/TCP` and `<pending>`.
→ [Advanced](advanced.en.md#loadbalancer)

</details>

## 6. 🟡 What does DNS return for a headless Service with three pods?

- A) One ClusterIP
- B) Three A records with the pod IPs
- C) A CNAME
- D) Nothing

<details><summary>Answer</summary>

**B.** Without `clusterIP` DNS returns the addresses of ready pods; kube-proxy does not balance.
→ [Advanced](advanced.en.md#headless-service)

</details>

## 7. 🟡 Select all correct statements

- [ ] A) A Service with a typo in `selector` gives empty endpoints and `Connection refused`
- [ ] B) A pod with a failing readiness probe gets into the endpoints
- [ ] C) ExternalName returns a CNAME to an external name
- [ ] D) The default NodePort range is 30000–32767

<details><summary>Answer</summary>

**A, C, D.** B is wrong: only ready pods get into the endpoints (in the experiment the new `0/1` pod was absent from the list).
→ [Advanced](advanced.en.md)

</details>

## 8. 🟡 What does `kubectl exec other -- wget -qO- http://web` print from a pod in the namespace `learn-06b` if the Service `web` is in `learn-06`?

- A) The application's reply
- B) `wget: bad address 'web'`
- C) `Connection refused`
- D) A timeout

<details><summary>Answer</summary>

**B.** `search` gets the pod's namespace (`learn-06b`). You need `web.learn-06`.
→ [Expert](expert.en.md#gotcha-the-short-name-works-only-in-its-own-namespace)

</details>

## 9. 🔴 Find the flaw

> "`ping 10.96.37.242` gets no reply, so the Service does not work and the pods are unreachable."

<details><summary>Answer</summary>

A ClusterIP is virtual: kube-proxy rules (DNAT) work for TCP/UDP ports and ICMP is not handled, so ping always gives 100% loss (verified). Health is checked by calling a port: `wget http://web`, `nc`. `kubectl get endpoints web` is also useful.
→ [Expert](expert.en.md#gotcha-ping-to-a-clusterip-does-not-work)

</details>

## 10. 🔴 Open question

An application gets `Connection refused` from a Service. Describe the order of diagnosis and two possible causes that give the same symptom.

<details><summary>Key points of a good answer</summary>

- First `kubectl get endpointslices -l kubernetes.io/service-name=<svc>`: empty or not.
- Empty → a typo in `selector` or pods not ready (compare `kubectl get pods --show-labels`, the `READY` status, the readiness probe).
- Addresses exist but the port is foreign → a wrong `targetPort`: compare with the port the container listens on.
- Both cases give `Connection refused` (in the experiment `wget: can't connect to remote host … Connection refused`).
- Additionally: check DNS (`nslookup`), the namespace in the name, NetworkPolicy.

</details>

→ [Advanced](advanced.en.md#typical-failures)
