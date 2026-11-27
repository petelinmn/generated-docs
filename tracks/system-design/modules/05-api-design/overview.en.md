---
title: "API design"
lang: en
track: system-design
kind: module
doc: overview
slug: 05-api-design
part: 1
levels: [1, 2, 3, 4, 5, 6]
tags: [system-design, api-design, rest, grpc, idempotency, optimistic-concurrency, keyset-pagination, problem-details]
versions: ".NET 11, C# 15"
status: verified
updated: 2026-11-26
---

# API design

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

An API is a contract that is hard to change once people use it. A good contract survives repeated requests, concurrent changes, large lists and new versions.

## Key ideas

- The style (REST, gRPC) is chosen by the consumers.
- Networks lose responses, so clients retry.
- An idempotency key makes retrying a POST safe.
- `ETag` and `If-Match` protect against lost updates.
- Cursor pagination is stable under inserts, offset is not.
- Errors should be machine-readable.
- Compatible changes add, they do not rename.

## Minimal example

```csharp
if (string.IsNullOrEmpty(key))
    return Results.Problem("Idempotency-Key header is required.", statusCode: 400);
```

## When to use and when not

- ✅ Any interface between parts released independently.
- ✅ Operations on money, orders, messages.
- ❌ Calls inside one process: a C# interface is enough.
- ❌ A long operation in a synchronous response: return `202` and a status.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | styles, resources, methods and codes, idempotency |
| [2. Advanced](advanced.en.md) | idempotency store, ETag, pagination, versioning |
| [3. Expert](expert.en.md) | reference, serialization gotchas, questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) |
| [5. Runnable examples](code/Examples/) | idempotency, pagination, a Minimal API |
| [6. Tests](code/Tests/) | `dotnet test` inside `code/` |

## Related materials

- Prerequisites: [design process](../01-design-process/overview.en.md), [ASP.NET Core Minimal API](../../../csharp/modules/14-aspnet-core-minimal-api/overview.en.md)
- Next: [replication and partitioning](../06-replication-and-partitioning/overview.en.md)
- In .NET: [pagination and filtering](../../../csharp/recipes/pagination-filtering/overview.en.md), [gRPC service](../../../csharp/recipes/grpc-service/overview.en.md)
- [Messaging and events](../08-messaging-and-events/overview.en.md)
- [Reliability patterns](../09-reliability-patterns/overview.en.md)
- [Security architecture](../15-security-architecture/overview.en.md)
- [Client forms in React](../../../react/modules/05-forms/overview.en.md)
