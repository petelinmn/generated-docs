---
title: "JWT authentication: scale and edges"
lang: en
track: csharp
kind: recipe
doc: expert
slug: jwt-auth
part: 2
tags: [jwt, openid-connect, jwks, token-revocation, security, interview-questions]
versions: ".NET 11, C# 15; Microsoft.AspNetCore.Authentication.JwtBearer 11.0.0"
status: draft
updated: 2026-11-22
---

# JWT authentication: scale and edges

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Minimal](basics.en.md) · [2. Production](advanced.en.md) · **3. Scale and edges** · [4. Quiz](quiz.en.md)

**Prerequisites:** [minimal](basics.en.md) and [production](advanced.en.md). **Time:** ~45 min.

## Scale: several services

- **One issuer, many verifiers.** An authentication center (Identity Provider) issues tokens with an asymmetric signature; consumer services get the public keys from JWKS and verify the token locally — without calling the center on every request. This is what makes JWT scalable.
- **OpenID Connect and OAuth 2.0.** User sign-in is through OpenID Connect (an id token), access to an API through OAuth 2.0 (an access token); writing your own issuing is rare. In ASP.NET Core: `AddJwtBearer(o => o.Authority = "https://idp…")` — the keys and issuer are fetched from metadata.
- **Between services** the original token is passed on or exchanged for a token with the needed audience (token exchange); service calls without a user use client credentials.
- **Key caching:** the library caches JWKS and refreshes it when a key changes; if the center is unavailable, already obtained keys keep working.

## Edges and gotchas

**1. You cannot "log out" by token.** Until `exp` a valid token is accepted. For immediate revocation: a short lifetime, a list of revoked `jti`/a session version in the check, or opaque (reference) tokens with introspection.

**2. Size.** Every request carries the token; extra claims bloat headers (proxy limits ~8 KB).

**3. Clock drift.** A `ClockSkew` that is too large — an expired token is accepted; too small — a valid one is rejected on "lagging" servers (verified: a token expired by a minute was rejected with `ClockSkew = 5 s`).

**4. The `none` algorithm and algorithm substitution.** The library checks the algorithm against the settings: do not accept an arbitrary `alg` from the header.

**5. HS256 and RS256 confusion.** If the verifier accepts "any" algorithm, a token can be signed with the public key used as an HMAC secret; fix the expected algorithms.

**6. Mixing "our" and a foreign `iss`.** Always check the issuer and audience (verified: a foreign audience → 401).

**7. Client-side storage.** `localStorage` is vulnerable to XSS; an `HttpOnly`+`Secure`+`SameSite` cookie is vulnerable to CSRF — anti-forgery measures are needed. There is no "safe everywhere" option.

**8. Claims from the payload are not truth forever.** Roles in a token may be stale by the time of use; for sensitive operations check against the source.

## Alternatives

| Approach | Pros | Cons |
|---|---|---|
| JWT access token | stateless, scalable | revocation is hard, size |
| Server sessions + cookie | immediate revocation, simple | a shared store is needed with several nodes |
| Reference tokens + introspection | immediate revocation | a call to the center for each check |
| API keys | simple for services | no user, manual rotation |
| mTLS | strong mutual service authentication | a complex certificate infrastructure |

## Interview questions

<details><summary>1. What does a JWT consist of and is it protected by encryption?</summary>

A header, a payload and a signature (Base64Url). The signature protects against modification but does not encrypt: anyone can read the payload (verified: the payload is readable by Base64Url decoding).

</details>

<details><summary>2. How does 401 differ from 403?</summary>

401 — not authenticated (no or a bad token), 403 — authenticated but without rights. Verified: no token → 401, the role `user` on `/admin` → 403.

</details>

<details><summary>3. What does the server check when it receives a token?</summary>

The signature, expiry (`exp`, `nbf`), issuer (`iss`) and audience (`aud`) — per the `TokenValidationParameters` settings.

</details>

<details><summary>4. How do you revoke a JWT?</summary>

Not directly: a short access token + revoking the refresh token, a list of revoked `jti`, or reference tokens.

</details>

<details><summary>5. Why is a refresh token needed?</summary>

It lets you keep the access token short without making the user log in again; it is stored on the server, rotated and revoked.

</details>

<details><summary>6. Symmetric or asymmetric signature?</summary>

Symmetric is simpler when one service issues and verifies; asymmetric — when there are many verifiers: they hold only the public key (JWKS).

</details>

<details><summary>7. What is `ClockSkew`?</summary>

The tolerance for clock differences when checking the `exp`/`nbf` times.

</details>

<details><summary>8. Where do you store a token in a browser?</summary>

A compromise: an `HttpOnly` cookie protects from XSS but needs CSRF protection; `localStorage` is convenient but the token is accessible to scripts.

</details>

<details><summary>9. Trick question: a user was deleted, yet their token still works. Why?</summary>

The token is checked locally by signature and expiry and knows nothing about the deletion; it is valid until `exp`. You need short tokens or a revocation check.

</details>

<details><summary>10. Trick question: a token from another of your services is accepted. How do you prevent it?</summary>

Check `aud` and `iss` (`ValidateAudience`, `ValidateIssuer`): a foreign audience gives 401 (verified).

</details>

## Related materials

- Back: [production](advanced.en.md). Check yourself: [quiz](quiz.en.md) and [exercises](exercises.en.md).
- [Rate limiting](../rate-limiting/overview.en.md) — protecting sign-in.
- [21 · Application architecture](../../modules/21-architecture/overview.en.md) — service boundaries.
- [22 · Observability](../../modules/22-observability/overview.en.md) — logging authentication failures.

## Sources

- [OAuth 2.0 (RFC 6749)](https://www.rfc-editor.org/rfc/rfc6749)
- [OpenID Connect Core](https://openid.net/specs/openid-connect-core-1_0.html)
- [JWT Best Current Practices (RFC 8725)](https://www.rfc-editor.org/rfc/rfc8725)
