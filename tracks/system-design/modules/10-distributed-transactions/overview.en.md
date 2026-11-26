---
title: "Distributed transactions"
lang: en
track: system-design
kind: module
doc: overview
slug: 10-distributed-transactions
part: 2
levels: [1, 2, 3, 4, 5, 6]
tags: [system-design, distributed-transactions, two-phase-commit, saga, outbox, inbox, idempotency]
versions: ".NET 11, C# 15; Microsoft.Data.Sqlite 11.0.0"
status: draft
updated: 2026-11-26
---

# Distributed transactions

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

One operation touches several services and databases, and they usually share no transaction. These are the ways not to leave the system half done: two-phase commit, saga, outbox and inbox.

## Key ideas

- A local transaction is atomic, a chain of calls is not.
- 2PC gives atomicity at the price of blocking when the coordinator fails.
- A saga is a sequence of local transactions with compensations.
- A compensation undoes the meaning of an action, it does not "rewind time".
- After the point of no return (pivot) you can only go forward.
- An outbox saves the event together with the data.
- An inbox makes redelivery harmless.

## Minimal example

```csharp
Execute("INSERT INTO orders (id) VALUES ($a)", orderId);
Execute("INSERT INTO outbox (payload) VALUES ($a)", $"OrderPlaced:{orderId}");
```

## When to use and when not

- ✅ A business process passes through several data owners.
- ✅ Events have to be published reliably.
- ❌ Everything fits into one local transaction: do that.
- ❌ A saga where an intermediate state must never be seen.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | the dual write problem, 2PC, saga, outbox |
| [2. Advanced](advanced.en.md) | orchestration and choreography, pivot, anomalies, inbox |
| [3. Expert](expert.en.md) | reference, gotchas, questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) |
| [5. Runnable examples](code/Examples/) | 2PC, saga, an outbox on SQLite |
| [6. Tests](code/Tests/) | `dotnet test` inside `code/` |

## Related materials

- Prerequisites: [consistency and CAP](../07-consistency-and-cap/overview.en.md), [messaging and events](../08-messaging-and-events/overview.en.md)
- Next: [architecture styles](../11-architecture-styles/overview.en.md)
- In other tracks: [sagas in MassTransit](../../../csharp/libraries/masstransit/overview.en.md), [transactions in EF Core](../../../csharp/modules/15-ef-core/overview.en.md)
