---
title: "Quiz: JWT authentication"
lang: en
track: csharp
kind: recipe
doc: quiz
slug: jwt-auth
part: 2
tags: [jwt, authentication, authorization, bearer-token]
status: draft
updated: 2026-11-22
---

# Quiz: JWT authentication

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [minimal](basics.en.md) · 🟡 [production](advanced.en.md) · 🔴 [scale and edges](expert.en.md).

## 1. 🟢 How many parts does a JWT have and what is in them?

- A) one: encrypted data
- B) three: a header, a payload (claims), a signature
- C) two: login and password
- D) four: including the key

<details><summary>Answer</summary>

**B.** Verified: `token.Split('.').Length == 3`. Anyone reads the payload (Base64Url), the signature protects against modification.
→ [Minimal](basics.en.md#what-a-jwt-is)

</details>

## 2. 🟢 What does a protected endpoint return without a token, and with the role `user` on an `AdminOnly` endpoint?

- A) 403 and 401
- B) 401 and 403
- C) 401 and 401
- D) 200 and 403

<details><summary>Answer</summary>

**B.** No token — not authenticated (401, `WWW-Authenticate: Bearer`); an unsuitable role — 403 (verified).
→ [Minimal](basics.en.md#how-it-works-response-codes)

</details>

## 3. 🟢 What should be in a token's payload?

- A) the user's password
- B) the user identifier, roles, expiry, issuer and audience
- C) a card number
- D) the private key

<details><summary>Answer</summary>

**B.** The payload is not encrypted — there must be no secrets in it.
→ [Minimal](basics.en.md#what-a-jwt-is)

</details>

## 4. 🟢 In what order is the middleware added?

- A) `UseAuthorization`, then `UseAuthentication`
- B) `UseAuthentication`, then `UseAuthorization`
- C) order does not matter
- D) only `UseAuthorization`

<details><summary>Answer</summary>

**B.** First the user must be established, then rights are checked.
→ [Minimal](basics.en.md#the-minimal-solution)

</details>

## 5. 🟡 What does the server return for a token expired a minute ago with `ClockSkew = 5 s`?

- A) 200
- B) 401 with `error="invalid_token"` and a message about an invalid lifetime
- C) 403
- D) 500

<details><summary>Answer</summary>

**B.** Verified: `The token lifetime is invalid; NotBefore: …, Expires: …`.
→ [Production](advanced.en.md#failure-diagnostics)

</details>

## 6. 🟡 Select all correct statements

- [ ] A) the access token is made short-lived
- [ ] B) the refresh token is stored on the server and rotated
- [ ] C) a JWT can be revoked instantly with no stored state
- [ ] D) the signing key is kept in configuration or secrets, not in code

<details><summary>Answer</summary>

**A, B, D.** C is wrong: a valid token is accepted until `exp`; revocation needs a short lifetime, revoking the refresh token or a `jti` list.
→ [Production](advanced.en.md#lifetime-refresh-tokens-and-revocation)

</details>

## 7. 🟡 A token was issued for the audience `other`. What does an API with `ValidAudience = "shop-api"` return?

- A) 200
- B) 401: the audience is invalid
- C) 403
- D) 404

<details><summary>Answer</summary>

**B.** Verified: `error="invalid_token"`, 401. The `aud` check prevents using a token issued for another service.
→ [Production](advanced.en.md#failure-diagnostics)

</details>

## 8. 🟡 Open question: how do you arrange sign-in so that stealing an access token does minimal damage?

<details><summary>Key points of a good answer</summary>

- a short access token lifetime (minutes);
- a refresh token on the server, hashed, with rotation and revocation;
- HTTPS only; storage in an `HttpOnly` cookie or in the application's memory;
- a minimal set of claims, checking `iss`/`aud`/the signature;
- limiting login attempts and monitoring anomalies.

</details>

## 9. 🔴 A user was deleted but their token is still accepted. Why?

- A) an ASP.NET Core bug
- B) the token is checked locally by signature and expiry; the server does not know of the deletion until `exp`
- C) a browser cache
- D) a JWT cannot be deleted from the database

<details><summary>Answer</summary>

**B.** This is the price of stateless checking. Solutions: a short lifetime, a list of revoked `jti`, reference tokens with introspection.
→ [Scale and edges](expert.en.md#edges-and-gotchas)

</details>

## 10. 🔴 Why is an asymmetric signature needed in a system of many services?

- A) tokens become shorter
- B) one center issues tokens with a private key, and services verify with a public key (JWKS), unable to forge a token
- C) it is faster than HMAC
- D) HTTPS requires it

<details><summary>Answer</summary>

**B.** With HMAC each verifier knows the secret and could issue a token; with RSA/ECDSA the verifiers need only the public key.
→ [Scale and edges](expert.en.md#scale-several-services)

</details>
