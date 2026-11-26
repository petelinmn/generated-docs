---
title: "Security architecture: advanced"
lang: en
track: system-design
kind: module
doc: advanced
slug: 15-security-architecture
part: 3
tags: [system-design, security, threat-modeling, authorization, password-hashing, hmac, ssrf]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Security architecture: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Between services

| Method | Idea |
|---|---|
| mTLS | both sides present certificates |
| Service tokens | short-lived, with an audience and permissions |
| Request signing (HMAC) | a shared key, an integrity check |

## Signing and replay protection

```csharp
if (!CryptographicOperations.FixedTimeEquals(expected, Encoding.ASCII.GetBytes(r.Signature))) return false;

TimeSpan age = clock.GetUtcNow() - DateTimeOffset.FromUnixTimeSeconds(r.Timestamp);
if (age.Duration() > MaxAge) return false;

return _usedNonces.Add(r.Nonce); // only a valid signature may consume a nonce
```

The method, path, time, nonce and body hash are signed. A change to any part, a replay and a stale request are rejected. File: [Ex02_SignedRequests.cs](code/Examples/Ex02_SignedRequests.cs). Nonces pile up in memory here; a system needs shared expiring storage.

## Zero trust

The network grants no trust: every request is authenticated and authorized, privileges are minimal, and the intruder is assumed to be inside already.

## SSRF

If a server calls an address taken from a request, it can be pointed at the internal network. You need an allow list, redirects disabled or each one checked, and control at the network level.

## Best practices

- Mark trust boundaries on the diagram.
- Rotate keys, keep token lifetimes short.
- Limit request size and rate.
- Audit actions that matter for investigation.

## Common mistakes

- ❌ Trusting a user header from anyone → ✅ only from a verified gateway.
- ❌ One shared key for all services → ✅ each has its own.
- ❌ Returning exception details to the client → ✅ a generic answer and a log entry.
- ❌ Deserializing straight into a domain entity → ✅ a DTO with only the allowed fields.

## Comparison with other tracks

- [Kubernetes security](../../../kubernetes/modules/17-security/overview.en.md), [RBAC](../../../kubernetes/modules/10-namespaces-and-rbac/overview.en.md).

## Related materials

- [Basics](basics.en.md)
- [Expert theory](expert.en.md)

## Sources

- [OWASP API Security Top 10, 2023](https://owasp.org/API-Security/editions/2023/en/0x11-t10/)
