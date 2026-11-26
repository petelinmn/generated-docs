---
title: "JWT authentication"
lang: en
track: csharp
kind: recipe
doc: overview
slug: jwt-auth
part: 2
levels: [1, 2, 3, 4]
tags: [jwt, authentication, authorization, bearer-token, refresh-token, aspnet-core]
versions: ".NET 11, C# 15; Microsoft.AspNetCore.Authentication.JwtBearer 11.0.0"
status: draft
updated: 2026-11-22
---

# JWT authentication

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

A recipe: issue and validate a JWT in ASP.NET Core. The client logs in with a login and password, receives a signed token and sends it in `Authorization: Bearer …`; the server checks the signature, expiry, issuer and audience without touching a database. Rights are expressed with roles and policies. The production version adds short access tokens, refresh tokens, key rotation and failure diagnostics; at scale an external provider (OpenID Connect) takes over issuing.

## Key ideas

- A JWT = header.payload.signature; the payload is **not encrypted**.
- `AddJwtBearer` + `TokenValidationParameters`: check the signature, expiry, `iss`, `aud`.
- `UseAuthentication` before `UseAuthorization`; `RequireAuthorization("Policy")`.
- 401 — not authenticated, 403 — no rights; the cause is in `WWW-Authenticate`.
- A JWT cannot be revoked before `exp`: a short lifetime, refresh tokens, a `jti` list.
- Keys come from secrets; support rotation (`IssuerSigningKeys`).
- An external IdP (OIDC) is better than hand-written issuing.

## Minimal example

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = "https://shop.local", ValidAudience = "shop-api", IssuerSigningKey = key,
        ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true,
    });
builder.Services.AddAuthorization();

app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/me", (ClaimsPrincipal u) => u.Identity?.Name).RequireAuthorization();
```

## When to use and when not

- ✅ No state between API instances, mobile and SPA clients.
- ✅ Service-to-service with a shared authentication center.
- ❌ You need instant revocation and a simple server session — cookie sessions.
- ❌ Secrets or personal data in the payload.
- ❌ Hand-written issuing and password storage instead of Identity/an IdP.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Minimal](basics.en.md) | what a JWT is, validation setup, issuing, roles and a policy, response codes |
| [2. Production](advanced.en.md) | failure diagnostics, keys and rotation, refresh tokens, policies, security |
| [3. Scale and edges](expert.en.md) | IdP and JWKS, revocation, gotchas, alternatives, 10 interview questions |
| [4. Self-check](quiz.en.md) | 10 questions and [4 exercises](exercises.en.md) with solutions |
| 5. Runnable examples | not generated yet — `/learn csharp jwt-auth level 5` |
| 6. Tests | not generated yet — `/learn csharp jwt-auth level 6` |

## Related materials

- Prerequisites: [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.en.md), [13 · DI, configuration, logging](../../modules/13-di-config-logging/overview.en.md)
- Nearby: [Rate limiting](../rate-limiting/overview.en.md), [17 · Integration tests](../../modules/17-testing-integration/overview.en.md)
- The same idea in other tracks: [Python](../../../python/overview.en.md) — JWT and OAuth2 in FastAPI
- Recipes and libraries: [File upload](../file-upload/overview.en.md), [gRPC service](../grpc-service/overview.en.md), [Real-time with SignalR](../signalr-realtime/overview.en.md)
- [System design: security architecture](../../../system-design/modules/15-security-architecture/overview.en.md)
