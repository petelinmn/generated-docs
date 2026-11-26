---
title: "Distributed transactions: basics"
lang: en
track: system-design
kind: module
doc: basics
slug: 10-distributed-transactions
part: 2
tags: [system-design, distributed-transactions, two-phase-commit, saga, outbox, inbox, idempotency]
versions: ".NET 11, C# 15; Microsoft.Data.Sqlite 11.0.0"
status: draft
updated: 2026-11-26
---

# Distributed transactions: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** transactions, messaging · **Time:** ~2 h

## Why it matters

A service wrote an order into its database and has to charge money in another one. Anything can happen between these actions. The result is an order without a payment or a payment without an order.

## The dual write

"First the database, then the broker" is two independent operations. A failure between them loses the event; the reverse order publishes an event about something that did not happen.

## Two-phase commit

1. **Prepare:** the coordinator asks everyone whether they can commit.
2. **Commit or abort:** commit only on a unanimous "yes".

```csharp
bool all = true;
foreach (Participant p in participants) all &= p.Prepare();
if (crashAfterPrepare) return;
```

Having answered "yes", a participant may not decide alone. If the coordinator disappears, participants stay Prepared and keep their locks. File: [Ex01_TwoPhaseCommit.cs](code/Examples/Ex01_TwoPhaseCommit.cs). This is a model without a log or recovery.

## Saga

Instead of one big transaction, a chain of local ones. If a step fails, the completed ones are undone by compensating actions in reverse order: "release the reservation", "refund the money".

## Outbox

```csharp
using SqliteTransaction tx = _connection.BeginTransaction();
Execute("INSERT INTO orders (id) VALUES ($a)", orderId);
Execute("INSERT INTO outbox (payload) VALUES ($a)", $"OrderPlaced:{orderId}");
if (failBeforeCommit) throw new InvalidOperationException("Crash"); // dispose rolls back
tx.Commit();
```

The order and the event are committed together. A separate process (relay) reads the outbox table and publishes. File: [Ex03_Outbox.cs](code/Examples/Ex03_Outbox.cs).

## Beginner mistakes

- ❌ Calling another service inside an open database transaction → ✅ transactions are short and local.
- ❌ Treating a compensation as a rollback → ✅ it is a new action visible to everyone.
- ❌ Publishing after `Commit` without an outbox → ✅ a failure loses the event.
- ❌ Not planning for a failing compensation → ✅ retries and escalation.

## Related materials

- [Advanced theory](advanced.en.md)
- [MassTransit](../../../csharp/libraries/masstransit/overview.en.md)

## Sources

- [Saga pattern](https://learn.microsoft.com/azure/architecture/patterns/saga)
- [PostgreSQL: PREPARE TRANSACTION](https://www.postgresql.org/docs/current/sql-prepare-transaction.html)
