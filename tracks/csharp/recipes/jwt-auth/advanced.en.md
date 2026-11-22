---
title: "JWT authentication: production"
lang: en
track: csharp
kind: recipe
doc: advanced
slug: jwt-auth
part: 2
tags: [jwt, refresh-token, authorization-policies, key-management, token-validation, claims]
versions: ".NET 11, C# 15; Microsoft.AspNetCore.Authentication.JwtBearer 11.0.0"
status: draft
updated: 2026-11-22
---

# JWT authentication: production

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Minimal](basics.en.md) · **2. Production** · [3. Scale and edges](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [the minimal solution](basics.en.md). **Time:** ~1 h.

## Failure diagnostics

On a failure the server puts the cause in the `WWW-Authenticate` header — from it the client and the developer see what is wrong. Verified:

| Situation | Status | `WWW-Authenticate` |
|---|---|---|
| no token | 401 | `Bearer` |
| the token is expired | 401 | `Bearer error="invalid_token", error_description="The token lifetime is invalid; NotBefore: …, Expires: …"` |
| signed with another key | 401 | `error="invalid_token", error_description="The signature key was not found"` |
| wrong `aud` | 401 | `error="invalid_token", error_description="The audience … is invalid"` |
| garbage instead of a token | 401 | `error="invalid_token"` |

In production the cause is not disclosed in detail to the client; for diagnostics log the `JwtBearerEvents.OnAuthenticationFailed` events (on the server).

## Keys and secrets

- A symmetric key (HMAC) is one secret for both signing and verifying: fine when one service issues and verifies. The key length for HS256 is at least 32 bytes.
- Asymmetric keys (RSA/ECDSA): the issuer keeps the private key, verifiers get the public one (through the JWKS address `.well-known/jwks.json`). This is how external providers (Entra ID, Auth0, Keycloak) work.
- The key is taken from configuration/secrets ([13 · DI, configuration, logging](../../modules/13-di-config-logging/advanced.en.md#options-validation-and-reloading)), not from code; plan for **rotation**: accept several keys (`IssuerSigningKeys`) for the transition.

```csharp
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));    // Issuer, Audience, Key, LifetimeMinutes
builder.Services.AddOptions<JwtOptions>().ValidateDataAnnotations().ValidateOnStart();
```

## Lifetime, refresh tokens and revocation

A JWT cannot be revoked before `exp`: the server holds no state. Hence:

- the **access token** lives briefly (minutes) and is sent with every request;
- the **refresh token** lives long, is stored on the server (in a database, hashed), is exchanged for a new pair of tokens at `/refresh` and is **rotated** (the old one becomes invalid);
- on compromise the refresh token is revoked; the access token "waits out" its expiry (a short lifetime) or a list of revoked identifiers (`jti`) is used.

`ClockSkew` is the allowed clock difference between servers (5 minutes by default); the example sets 5 seconds, and a token expired a minute ago is rejected (verified).

## Policies and claims

Roles are a special case of claims. Policies express rules more precisely:

```csharp
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", p => p.RequireRole("admin"))
    .AddPolicy("CanRefund", p => p.RequireClaim("permission", "refund"))
    .AddPolicy("Adult", p => p.RequireAssertion(ctx => int.TryParse(ctx.User.FindFirstValue("age"), out var a) && a >= 18));
```

Verified: the role `user` on `/admin` gives `403`, the role `admin` — `200`. For checks over a resource ("only your own order can be edited") use resource-based authorization (`IAuthorizationService`, `AuthorizationHandler`).

## Security

| Rule | Why |
|---|---|
| HTTPS only | a token is a "door key": interception = access |
| Check `iss`, `aud`, the signature, expiry | otherwise a foreign token is accepted |
| Set the algorithm explicitly, forbid `none` | protection from algorithm substitution |
| A short access token | limits the damage of theft |
| In the browser keep it in an `HttpOnly` cookie, not `localStorage` | protection from XSS |
| Passwords — only hashes (`PasswordHasher`) | a database leak does not reveal passwords |
| Limit login attempts ([rate limiting](../rate-limiting/overview.en.md)) | protection from brute force |

## Best practices

- Token issuing is moved into a service (`ITokenService`), not kept in a handler.
- For real users use ASP.NET Core Identity or an external provider (OpenID Connect), not a hand-written password check.
- Tests: replace the key and the time ([17 · Integration tests](../../modules/17-testing-integration/overview.en.md)), check all rows of the table above.

## Related materials

- Back: [minimal](basics.en.md). Next: [scale and edges](expert.en.md).
- [Rate limiting](../rate-limiting/overview.en.md) — limiting login attempts.
- [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.en.md) — the pipeline.
- [17 · Testing: integration tests](../../modules/17-testing-integration/overview.en.md) — authorization tests.

## Sources

- [Configure JWT bearer authentication](https://learn.microsoft.com/aspnet/core/security/authentication/configure-jwt-bearer-authentication)
- [Resource-based authorization](https://learn.microsoft.com/aspnet/core/security/authorization/resourcebased)
- [OWASP: JWT cheat sheet](https://cheatsheetseries.owasp.org/cheatsheets/JSON_Web_Token_for_Java_Cheat_Sheet.html)
