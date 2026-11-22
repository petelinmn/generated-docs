---
title: "Exercises: JWT authentication"
lang: en
track: csharp
kind: recipe
doc: exercises
slug: jwt-auth
part: 2
tags: [jwt, authentication, authorization-policies, testing]
status: draft
updated: 2026-11-22
---

# Exercises: JWT authentication

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. The solutions build on the application from the [minimal solution](basics.en.md) and were verified by running (Kestrel + `HttpClient`, the package `Microsoft.AspNetCore.Authentication.JwtBearer` 11.0.0). There are no runnable versions in `code/` (level 5 does not exist).

## 1. 🟢 Sign-in and a protected endpoint

**Task:** implement `POST /login` (`ann`/`pw` → a token, otherwise 401) and `GET /me` with `RequireAuthorization()`. Check the codes.

**Expected result:**

```text
correct login → 200, a token of 3 parts;  wrong → 401
/me without a token → 401;  /me with a token → 200 (the name ann)
```

<details><summary>Hint</summary>

`Results.Ok(new { access_token = ... })`, `Results.Unauthorized()`; on the client — `AuthenticationHeaderValue("Bearer", token)`.

</details>

<details><summary>Solution</summary>

See the code of `Issue`, `/login`, `/me` in the [minimal solution](basics.en.md#the-minimal-solution).

```csharp
// Exercise 1: sending the token from the client.
var req = new HttpRequestMessage(HttpMethod.Get, "/me");
req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
var r = await http.SendAsync(req);   // 200 "ann"; without the header — 401
```

Verified: `bad login: 401`, `/me` without a token `401`, with a token `200`.

</details>

## 2. 🟡 Roles and a policy

**Task:** add a policy `AdminOnly` and an endpoint `/admin`. Show that `user` gets 403 and `admin` gets 200.

**Expected result:** `403` for the role `user`, `200 secret` for `admin`.

<details><summary>Hint</summary>

`AddAuthorizationBuilder().AddPolicy("AdminOnly", p => p.RequireRole("admin"))`; the role is put in the `ClaimTypes.Role` claim.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 2: a role-based policy.
builder.Services.AddAuthorizationBuilder().AddPolicy("AdminOnly", p => p.RequireRole("admin"));
app.MapGet("/admin", () => "secret").RequireAuthorization("AdminOnly");
```

Verified: `admin, user role: 403`, `admin, admin role: 200 secret`.

</details>

## 3. 🟡 A set of negative checks

**Task:** write checks where the token is rejected: expired, signed with another key, issued for another audience, garbage. For each make sure of 401 and the content of `WWW-Authenticate`.

**Expected result:** in all four cases `401` and `error="invalid_token"`.

<details><summary>Hint</summary>

Extend `Issue` with the parameters `signWith`, `audience`; the lifetime is a negative `TimeSpan`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 3: negative cases — each must be rejected with 401.
await Show("expired",        "/me", Issue("ann", ["user"], TimeSpan.FromSeconds(-60)));
await Show("wrong key",      "/me", Issue("ann", ["user"], TimeSpan.FromMinutes(5), new SymmetricSecurityKey(Encoding.UTF8.GetBytes("another-key-another-key-another-key-123456"))));
await Show("wrong audience", "/me", Issue("ann", ["user"], TimeSpan.FromMinutes(5), audience: "other"));
await Show("garbage",        "/me", "not.a.jwt");
```

Verified: all four — `401`; the messages: the lifetime is invalid, the signature key was not found, the audience is invalid, `invalid_token`.

</details>

## 4. 🔴 Key rotation

**Task:** configure validation so that tokens signed with the old **and** the new key (`IssuerSigningKeys`) are accepted, while issuing uses the new one. Check that a token of the old key passes and a foreign one does not.

**Expected result:** old key → 200, new → 200, foreign → 401.

<details><summary>Hint</summary>

`TokenValidationParameters.IssuerSigningKeys = [newKey, oldKey]` instead of `IssuerSigningKey`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 4: accept several signing keys during rotation.
o.TokenValidationParameters = new TokenValidationParameters
{
    ValidIssuer = Issuer, ValidAudience = Audience,
    IssuerSigningKeys = [newKey, oldKey],      // issue with newKey; oldKey is dropped after old tokens expire
    ValidateIssuerSigningKey = true,
};
```

Verified: a token of the old key — `200`, of the new — `200`, of a foreign one — `401`. After all tokens of the old key expire, it is removed from the list.

</details>
