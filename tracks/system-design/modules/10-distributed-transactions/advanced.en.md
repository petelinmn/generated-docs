---
title: "Distributed transactions: advanced"
lang: en
track: system-design
kind: module
doc: advanced
slug: 10-distributed-transactions
part: 2
tags: [system-design, distributed-transactions, two-phase-commit, saga, outbox, inbox, idempotency]
versions: ".NET 11, C# 15; Microsoft.Data.Sqlite 11.0.0"
status: draft
updated: 2026-11-26
---

# Distributed transactions: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Orchestration and choreography

| | Choreography | Orchestration |
|---|---|---|
| Who decides | each reacts to events | a central coordinator |
| Fits | simple processes with few services | complex processes |
| Pro | no extra component | the process is visible in one place |
| Con | hard to follow, cycles possible | the coordinator has to be built and protected |

## Kinds of steps

- **Compensable** — can be undone.
- **Pivot** — the point of no return.
- **Retryable** — after the pivot, idempotent and driven to completion.

```csharp
// After the pivot there is no way back: someone has to finish forward.
if (pastPivot) return SagaOutcome.NeedsAttention;

while (done.TryPop(out SagaStep? previous))
{
    await previous.Compensate!(); // reverse order
    journal.Add($"undo {previous.Name}");
}
```

File: [Ex02_Saga.cs](code/Examples/Ex02_Saga.cs). The example keeps state in memory and does not retry steps: a real orchestrator saves progress after every step.

## Anomalies

A saga has no isolation: lost updates, dirty and nonrepeatable reads are possible. Countermeasures: a semantic lock (an "in progress" status), commutative updates, reordering steps (risky ones after the pivot), rereading before writing, versions.

## Relay and inbox

```csharp
publish(id, payload);
if (crashBeforeMark) return;
Execute("UPDATE outbox SET sent = 1 WHERE id = $a", id);
```

A failure between publishing and marking gives a duplicate, not a loss. The receiver remembers identifiers (an inbox).

## Best practices

- Make every step and compensation idempotent.
- Store the saga state and show it to an operator.
- Set step timeouts.
- Clean up processed outbox and inbox records.

## Common mistakes

- ❌ Marking as sent before publishing → ✅ after.
- ❌ Several relays without row locking → ✅ claim a batch, otherwise duplicates and broken order.
- ❌ A compensation that depends on vanished data → ✅ keep what it needs in the saga state.
- ❌ An irreversible action (an email) at the start → ✅ closer to the end.

## Comparison with other tracks

- [MassTransit sagas](../../../csharp/libraries/masstransit/overview.en.md).
- [Kafka delivery semantics](../../../kafka/modules/05-delivery-semantics/overview.en.md).

## Related materials

- [Basics](basics.en.md)
- [Expert theory](expert.en.md)

## Sources

- [Saga pattern](https://learn.microsoft.com/azure/architecture/patterns/saga)
- [Transactional outbox](https://learn.microsoft.com/azure/architecture/databases/guide/transactional-outbox-cosmos)
