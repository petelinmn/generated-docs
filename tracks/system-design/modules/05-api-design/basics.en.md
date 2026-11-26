---
title: "API design: basics"
lang: en
track: system-design
kind: module
doc: basics
slug: 05-api-design
part: 1
tags: [system-design, api-design, rest, grpc, idempotency, optimistic-concurrency, keyset-pagination, problem-details]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# API design: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** HTTP, Minimal API · **Time:** ~2 h

## Why it matters

Clients cannot be updated at the same moment as the server. Everything you published has to be supported, and everything you did not think through (retries, large lists) surfaces under load.

## Styles

| | REST over HTTP | gRPC |
|---|---|---|
| Contract | OpenAPI (optional) | `.proto` (required) |
| Format | usually JSON | Protocol Buffers |
| Streaming | limited | client, server, bidirectional |
| From a browser | directly | needs gRPC-Web or a gateway |
| Usually | public APIs | service-to-service calls |

## Resources, methods and codes

Resources are named with nouns (`/orders/1`), the method gives the action. According to RFC 9110, GET, HEAD, OPTIONS and TRACE are safe; they are idempotent, and so are PUT and DELETE. POST and PATCH are not declared as such.

| Code | When |
|---|---|
| 201 | a resource was created, `Location` points to it |
| 202 | accepted but not yet done |
| 400 | a malformed request |
| 404 | no such resource |
| 409 | a conflict with the current state |
| 412 / 428 | a precondition failed / a precondition is required |
| 429 | too many requests |

## Idempotency

A client sent a POST, the response got lost. Retrying risks a second order. The solution: the client sends a unique key in `Idempotency-Key`, the server remembers the result and returns it on a repeat.

```csharp
IdempotencyOutcome.KeyReused => Results.Problem("The key was used with another payload.", statusCode: 422),
IdempotencyOutcome.InProgress => Results.Problem("The original request is still running.", statusCode: 409),
_ => Results.Created($"/orders/{order!.Id}", order),
```

File: [Ex03_OrdersApi.cs](code/Examples/Ex03_OrdersApi.cs). The codes follow the IETF draft standard for this header; as of November 2026 it is a draft, not an RFC.

## Errors

`Results.Problem` returns `application/problem+json` per RFC 9457: the fields `type`, `title`, `status`, `detail`, `instance`. The client parses errors uniformly instead of guessing from text.

## Beginner mistakes

- ❌ `200` with `{"error": ...}` → ✅ a proper code and problem details.
- ❌ A GET that changes data → ✅ GET only reads.
- ❌ An unbounded list → ✅ pagination with a maximum size.
- ❌ Returning database entities as they are → ✅ separate DTOs.

## Related materials

- [Advanced theory](advanced.en.md)
- [ASP.NET Core Minimal API](../../../csharp/modules/14-aspnet-core-minimal-api/overview.en.md)

## Sources

- [RFC 9110: HTTP Semantics](https://www.rfc-editor.org/rfc/rfc9110)
- [RFC 9457: Problem Details](https://www.rfc-editor.org/rfc/rfc9457)
- [IETF draft: Idempotency-Key](https://datatracker.ietf.org/doc/draft-ietf-httpapi-idempotency-key-header/)
