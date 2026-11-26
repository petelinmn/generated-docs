---
title: "API design: advanced"
lang: en
track: system-design
kind: module
doc: advanced
slug: 05-api-design
part: 1
tags: [system-design, api-design, rest, grpc, idempotency, optimistic-concurrency, keyset-pagination, problem-details]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# API design: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## An idempotency key store

Claiming a key must be atomic: "check, then write" lets two concurrent requests through.

```csharp
// TryAdd is the atomic claim: only one caller gets to run the action.
while (!_entries.TryAdd(key, pending))
{
    if (!_entries.TryGetValue(key, out Entry? existing)) continue;
    if (existing.Fingerprint != fingerprint) return (IdempotencyOutcome.KeyReused, default);
    return existing.Done
        ? (IdempotencyOutcome.Replayed, existing.Result)
        : (IdempotencyOutcome.InProgress, default);
}
```

File: [Ex01_Idempotency.cs](code/Examples/Ex01_Idempotency.cs). This is a teaching version. In production the record lives in the database with a unique constraint, is written in one transaction with the effect, has a bounded lifetime and the key is scoped to a client.

## Optimistic concurrency

Two users read order version 1 and both change it. Without protection the second update silently overwrites the first. The server returns an `ETag`, the client sends it back in `If-Match`.

```csharp
// TryUpdate makes check-and-write one atomic step.
if (ifMatch != ETag(current) || !orders.TryUpdate(id, updated, current))
    return Results.Problem("The order has changed.", statusCode: 412);
```

Without `If-Match` the example answers 428 (RFC 6585).

## Pagination

Offset skips N rows. If a record is inserted between pages, rows shift and repeat: the first page is `4 3`, after an insert the second is `3 2`. A cursor (keyset) remembers the last row and takes the ones after it.

```csharp
// One extra row tells whether there is a next page without a COUNT.
List<Post> page = [.. posts.Where(p => p.Id < before).OrderByDescending(p => p.Id).Take(size + 1)];
```

The ordering must be unambiguous: for equal values add the key. Make the cursor opaque and validate it on input. File: [Ex02_Pagination.cs](code/Examples/Ex02_Pagination.cs).

## Versioning and compatibility

| Protobuf change | Compatible? |
|---|---|
| a new field, method or service | yes |
| a new enum value | yes, if old clients cope with it |
| removing a field | breaks client code when the contract is updated; reserve the number and name |
| changing a field number, or a type to an incompatible one | no |
| renaming a method, service or package | no |
| renaming a field | not breaking for protobuf, breaking for JSON |

The same rule applies to JSON: adding something optional is fine, renaming and changing meaning are not. Introduce a version (`/v2`, a header) when a compatible change is impossible.

## Best practices

- Set a maximum page size and request body size.
- Long operations: `202` and a status resource.
- Publish the contract and check compatibility in CI.
- Document which errors may be retried.

## Common mistakes

- ❌ A new idempotency key on every retry → ✅ one key per intent.
- ❌ A new required request field → ✅ an optional one with a default.
- ❌ The client parses the cursor → ✅ the cursor is opaque.
- ❌ Retries without a pause → ✅ exponential backoff and `Retry-After`.

## Comparison with other tracks

- Client-side retries — [resilient HttpClient](../../../csharp/recipes/http-client-resilience/overview.en.md).
- Keyset in EF Core — [pagination and filtering](../../../csharp/recipes/pagination-filtering/overview.en.md).
- `.proto` contracts — [gRPC service](../../../csharp/recipes/grpc-service/overview.en.md).

## Related materials

- [Basics](basics.en.md)
- [Expert theory](expert.en.md)

## Sources

- [ASP.NET Core: gRPC versioning](https://learn.microsoft.com/aspnet/core/grpc/versioning)
- [RFC 6585: additional status codes](https://www.rfc-editor.org/rfc/rfc6585)
