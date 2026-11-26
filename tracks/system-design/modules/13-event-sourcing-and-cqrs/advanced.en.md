---
title: "Event sourcing and CQRS: advanced"
lang: en
track: system-design
kind: module
doc: advanced
slug: 13-event-sourcing-and-cqrs
part: 3
tags: [system-design, event-sourcing, cqrs, events, projection, snapshot, upcasting]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Event sourcing and CQRS: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Projections

```csharp
if (stored.Position <= Checkpoint) return; // redelivery
```

A projection reads events in order and remembers its position. Duplicates are skipped, and a deleted model can be rebuilt. The position and the model are updated in one transaction. File: [Ex02_Projection.cs](code/Examples/Ex02_Projection.cs).

## Snapshots

A snapshot is the state saved at some version: you load it and apply only later events. It is an optimization; the stream remains the source of truth, and snapshots can be regenerated.

## Event versions

| Technique | Idea |
|---|---|
| Tolerant reading | ignore extras, provide defaults |
| Version in the envelope or type name | tell shapes apart |
| Upcasting | convert on read without changing what is stored |
| Store migration | a last resort |

```csharp
// Each step converts one version to the next, so steps can be chained.
if (version == 1) payload["currency"] = "USD"; // v1 was recorded when only USD existed
```

File: [Ex03_Upcasting.cs](code/Examples/Ex03_Upcasting.cs).

## Personal data

An immutable log sits badly with data deletion. Options: keep personal data separately and refer to it by identifier, or encrypt it with a per-subject key and delete the key (crypto-shredding).

## Best practices

- Design events as part of a contract.
- Make consumers idempotent: delivery may repeat.
- Measure projection lag.
- Apply the pattern to part of the system, not all of it.

## Common mistakes

- ❌ Side effects while rebuilding a projection (emails again) → ✅ separate projections from reactions.
- ❌ Publishing internal events as they are → ✅ separate integration events.
- ❌ One huge stream → ✅ a stream per aggregate.
- ❌ A cross-stream invariant "in a projection" → ✅ revisit aggregate boundaries.

## Comparison with other tracks

- Keeping and rereading a log — [Kafka](../../../kafka/modules/04-topics-and-retention/overview.en.md).
- CQRS and vertical slices — [C#](../../../csharp/modules/21-architecture/overview.en.md).

## Related materials

- [Basics](basics.en.md)
- [Expert theory](expert.en.md)

## Sources

- [Event Sourcing pattern](https://learn.microsoft.com/azure/architecture/patterns/event-sourcing)
