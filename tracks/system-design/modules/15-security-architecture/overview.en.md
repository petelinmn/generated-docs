---
title: "Security architecture"
lang: en
track: system-design
kind: module
doc: overview
slug: 15-security-architecture
part: 3
levels: [1, 2, 3, 4, 5, 6]
tags: [system-design, security, threat-modeling, authorization, password-hashing, hmac, ssrf]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Security architecture

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

Security is built in at design time: understand what you protect and from what, where the trust boundaries are and what happens if one line of defence gives way.

## Key ideas

- Threat modeling starts from a data diagram and trust boundaries.
- Authentication says "who", authorization says "may they".
- Permissions are checked on the particular object.
- Access is denied by default, privileges are minimal.
- Passwords are stored as a slow salted hash.
- Defence is built in layers.
- Secrets do not get into code or logs.

## Minimal example

```csharp
public static bool CanRead(User user, Document document) =>
    user.Tenant == document.Tenant && (user.Id == document.OwnerId || user.IsAdmin);
```

## When to use and when not

- ✅ Always, in proportion to the value of the data.
- ✅ A new external interface or a flow of personal data.
- ❌ Your own cryptographic algorithms.
- ❌ Your own sign-in system when a proven one exists.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | threats, authentication and authorization, passwords |
| [2. Advanced](advanced.en.md) | signatures, service-to-service, zero trust, SSRF, secrets |
| [3. Expert](expert.en.md) | OWASP API Top 10, gotchas, questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) |
| [5. Runnable examples](code/Examples/) | PBKDF2, HMAC signing, object authorization |
| [6. Tests](code/Tests/) | `dotnet test` inside `code/` |

## Related materials

- Prerequisites: [API design](../05-api-design/overview.en.md), [reliability patterns](../09-reliability-patterns/overview.en.md)
- In other tracks: [JWT authentication](../../../csharp/recipes/jwt-auth/overview.en.md), [Kubernetes security](../../../kubernetes/modules/17-security/overview.en.md), [RBAC](../../../kubernetes/modules/10-namespaces-and-rbac/overview.en.md)
