---
title: "Pod and network security"
lang: en
track: kubernetes
kind: module
doc: overview
slug: 17-security
part: 3
levels: [1, 2, 3, 4]
tags: [networkpolicy, pod-security-admission, pod-security-standards, securitycontext, capabilities, serviceaccount, validatingadmissionpolicy, seccomp]
versions: "Kubernetes 1.37.0 (kind v0.34, kindnet), kubectl 1.36.1"
status: draft
updated: 2026-11-23
---

# Pod and network security

🌐 [Русский](overview.ru.md) · **English**

## In short

By default in Kubernetes any pod can reach any other, a container runs as root with a set of privileges, and a ServiceAccount token is mounted into the pod. Protection is built from several independent layers: **NetworkPolicy** limits network connections, **securityContext** sets the rights the process runs with, **Pod Security Admission (PSA)** keeps unsafe pods out of a namespace, **ValidatingAdmissionPolicy** adds your own rules in CEL. Access rights to the API are a separate topic: [RBAC](../10-namespaces-and-rbac/overview.en.md).

## Key ideas

- **The network is open by default.** While no NetworkPolicy selects a pod, it accepts and sends everything (in the experiment all clients got a response).
- **The first policy closes the rest.** Once a pod is selected by a policy of the `Ingress` direction, only what the policies list is allowed; `default-deny` is a policy with an empty rule.
- **A NetworkPolicy is enforced by the network plugin (CNI),** not by the API server. In kind (kindnet) policies work.
- **`securityContext` narrows the process rights:** an unprivileged user, no extra capabilities, a read-only root filesystem.
- **PSA checks pods at creation** by namespace labels: the levels `privileged`, `baseline`, `restricted` and the modes `enforce`, `warn`, `audit`.
- **PSA does not touch running pods** and blocks not the Deployment but the pods it creates.
- **The ServiceAccount token is mounted by default;** it can be disabled (`automountServiceAccountToken: false`).
- **Your own rules — ValidatingAdmissionPolicy:** a CEL expression evaluated by the API server itself.

## Minimal example

```yaml
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata: {name: default-deny-ingress, namespace: learn-17}
spec:
  podSelector: {}
  policyTypes: [Ingress]
```

```text
$ kubectl -n learn-17 exec client-ok -- wget -q -T 3 -O /dev/null http://server.learn-17.svc
wget: download timed out
```

Before the policy was applied, the same request returned a response.

## When to use and when not

| Task | Tool |
|---|---|
| Limit who talks to whom | NetworkPolicy |
| Forbid root and privileged containers | securityContext + PSA |
| Keep unsafe pods out of a namespace | PSA (`enforce`) |
| A custom rule (for example, forbid the `latest` tag) | ValidatingAdmissionPolicy |
| Limit actions in the API | RBAC ([module 10](../10-namespaces-and-rbac/overview.en.md)) |

- ❌ Do not treat a NetworkPolicy as an application-level firewall: it works at the level of IPs, ports and labels.
- ❌ Do not enable `enforce: restricted` in a namespace with running applications without checking first: new pods without the required fields will not be created.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | NetworkPolicy: default deny and allowing by labels; securityContext; PSA labels |
| [2. Advanced theory](advanced.en.md) | namespaceSelector (AND and OR), egress and DNS, PSA modes, capabilities, ServiceAccount token, ValidatingAdmissionPolicy |
| [3. Expert theory](expert.en.md) | field reference, PSS levels, internals, pitfalls, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn kubernetes 17-security level 5` |
| 6. Tests | not created yet — `/learn kubernetes 17-security level 6` |

## Related materials

- Prerequisites: [04 · Pod](../04-pods/overview.en.md), [06 · Service and networking](../06-services/overview.en.md), [10 · Namespaces and RBAC](../10-namespaces-and-rbac/overview.en.md)
- Secrets: [07 · ConfigMap and Secret](../07-config-and-secrets/overview.en.md)
- Previous module: [16 · Observability](../16-observability/overview.en.md)
- Next: [18 · Pod scheduling](../18-scheduling/overview.en.md)
- In other tracks: [Linux](../../../linux/overview.en.md) — permissions, users, capabilities at the OS level
- CRDs and operators: [19 · CRDs and operators](../19-crd-and-operators/overview.en.md)
- GitOps and Argo CD: [20 · GitOps](../20-gitops/overview.en.md)
- Troubleshooting: [21 · Troubleshooting](../21-troubleshooting/overview.en.md)
