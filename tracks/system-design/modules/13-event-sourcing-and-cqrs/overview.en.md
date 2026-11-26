---
title: "Event sourcing and CQRS"
lang: en
track: system-design
kind: module
doc: overview
slug: 13-event-sourcing-and-cqrs
part: 3
levels: [1, 2, 3, 4, 5, 6]
tags: [system-design, event-sourcing, cqrs, events, projection, snapshot, upcasting]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Event sourcing and CQRS

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

Event sourcing stores not the current state but all changes as events. CQRS separates the write and read models. They are often used together, but they are independent decisions, and both add complexity.

## Key ideas

- State is a fold of the event stream.
- Events are immutable; a correction is a new event.
- Concurrent writes are rejected by checking the stream version.
- Projections build read models and lag behind writes.
- A snapshot speeds up loading but does not replace events.
- Old events are read through upcasting.
- The pattern is expensive to adopt and expensive to undo.

## Minimal example

```csharp
Account account = Account.Replay([new Deposited(100), new Withdrawn(30)]); // balance 70
```

## When to use and when not

- ✅ History and the reason for the current state are needed.
- ✅ Read models differ greatly from the write model.
- ❌ Plain CRUD.
- ❌ The team is not ready for eventual consistency and event versioning.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | events, aggregate, event store, CQRS |
| [2. Advanced](advanced.en.md) | projections, snapshots, event versions, personal data |
| [3. Expert](expert.en.md) | reference, gotchas, questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) |
| [5. Runnable examples](code/Examples/) | event store, projection, upcasting |
| [6. Tests](code/Tests/) | `dotnet test` inside `code/` |

## Related materials

- Prerequisites: [consistency](../07-consistency-and-cap/overview.en.md), [messaging and events](../08-messaging-and-events/overview.en.md), [distributed transactions](../10-distributed-transactions/overview.en.md)
- In other tracks: [C#: architecture and CQRS](../../../csharp/modules/21-architecture/overview.en.md), [Kafka basics](../../../kafka/modules/01-kafka-basics/overview.en.md)
