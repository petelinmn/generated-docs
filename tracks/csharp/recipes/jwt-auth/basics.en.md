---
title: "JWT authentication: the minimal solution"
lang: en
track: csharp
kind: recipe
doc: basics
slug: jwt-auth
part: 2
tags: [jwt, authentication, authorization, bearer-token, aspnet-core]
versions: ".NET 11, C# 15; Microsoft.AspNetCore.Authentication.JwtBearer 11.0.0"
status: draft
updated: 2026-11-22
---

# JWT authentication: the minimal solution

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Minimal** · [2. Production](advanced.en.md) · [3. Scale and edges](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.en.md), [13 · DI, configuration, logging](../../modules/13-di-config-logging/overview.en.md). **Time:** ~45 min.

## The task

An API must know who is calling it and protect some endpoints. The solution: the client logs in with a login and password, gets a **JWT** (JSON Web Token) and sends it in the header `Authorization: Bearer <token>`. The server checks the signature and expiry without touching a database.

## What a JWT is

A token of three parts separated by dots: `header.payload.signature` (each Base64Url). The payload is a set of **claims**: `sub` (who), `iss` (who issued it), `aud` (for whom), `exp` (when it expires), roles and so on. Verified: the token has 3 parts, and the payload looks like
`{"aud":"shop-api","iss":"https://shop.local","exp":…,"sub":"ann","…/role":"user","…/name":"ann"}`. The signature guarantees that the payload was not changed; there is **no encryption** — anyone can read the payload, so it must hold no secrets.

## The minimal solution

The `Microsoft.AspNetCore.Authentication.JwtBearer` package. Validation setup and token issuing:

```csharp
const string Issuer = "https://shop.local", Audience = "shop-api";
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("super-secret-key-for-tests-only-32+chars!!"));   // from configuration in real life

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = Issuer, ValidAudience = Audience, IssuerSigningKey = key,
        ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.FromSeconds(5),
    };
});
builder.Services.AddAuthorizationBuilder().AddPolicy("AdminOnly", p => p.RequireRole("admin"));

app.UseAuthentication();
app.UseAuthorization();

string Issue(string user, string[] roles, TimeSpan lifetime)
{
    var now = DateTime.UtcNow;
    var descriptor = new SecurityTokenDescriptor
    {
        Issuer = Issuer, Audience = Audience,
        Subject = new ClaimsIdentity(roles.Select(r => new Claim(ClaimTypes.Role, r))
            .Append(new Claim(JwtRegisteredClaimNames.Sub, user)).Append(new Claim(ClaimTypes.Name, user))),
        NotBefore = now, IssuedAt = now, Expires = now.Add(lifetime),
        SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
    };
    return new JsonWebTokenHandler().CreateToken(descriptor);
}

app.MapPost("/login", (LoginDto d) => d is { User: "ann", Password: "pw" }
    ? Results.Ok(new { access_token = Issue("ann", ["user"], TimeSpan.FromMinutes(15)) })
    : Results.Unauthorized());
app.MapGet("/public", () => "anyone");
app.MapGet("/me", (ClaimsPrincipal u) => $"{u.Identity?.Name}").RequireAuthorization();
app.MapGet("/admin", () => "secret").RequireAuthorization("AdminOnly");
```

Order matters: `UseAuthentication` before `UseAuthorization`. `RequireAuthorization()` protects an endpoint; `RequireAuthorization("AdminOnly")` — by a role policy. (`using Microsoft.IdentityModel.JsonWebTokens; using Microsoft.IdentityModel.Tokens;`.)

## How it works: response codes

Verified on a running application:

| Request | Response |
|---|---|
| `POST /login` with correct data | `200`, `access_token` (3 parts) |
| `POST /login` with a wrong password | `401` |
| `GET /public` without a token | `200` |
| `GET /me` without a token | `401`, header `WWW-Authenticate: Bearer` |
| `GET /me` with a token | `200`, the user name |
| `GET /admin` with the role `user` | `403` |
| `GET /admin` with the role `admin` | `200` |

Tell them apart: **401** — "not authenticated" (no or a bad token), **403** — "authenticated but not allowed".

## Beginner mistakes

- ❌ The signing key right in the code → ✅ in configuration or a secret store.
- ❌ A password or secrets in the token payload → ✅ only identifiers and roles (anyone reads the payload).
- ❌ `UseAuthorization` without `UseAuthentication` → ✅ authentication first.
- ❌ Expiry or signature validation turned off "for convenience" → ✅ all checks on.
- ❌ A token without a lifetime → ✅ a short lifetime and renewal.
- ❌ Confusing 401 and 403 → ✅ 401 — who are you?, 403 — you may not.

## Related materials

- Next: [the production version](advanced.en.md) — keys, failure diagnostics, refresh tokens, policies.
- [14 · ASP.NET Core: Minimal API](../../modules/14-aspnet-core-minimal-api/overview.en.md) — the pipeline and `RequireAuthorization`.
- [13 · DI, configuration, logging](../../modules/13-di-config-logging/overview.en.md) — secrets and Options.
- [Python](../../../python/overview.en.md) — JWT in FastAPI.

## Sources

- [JWT Bearer authentication in ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/authentication/)
- [Policy-based authorization](https://learn.microsoft.com/aspnet/core/security/authorization/policies)
- [RFC 7519: JSON Web Token](https://www.rfc-editor.org/rfc/rfc7519)
