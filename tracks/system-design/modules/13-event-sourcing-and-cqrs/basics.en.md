---
title: "Event sourcing and CQRS: basics"
lang: en
track: system-design
kind: module
doc: basics
slug: 13-event-sourcing-and-cqrs
part: 3
tags: [system-design, event-sourcing, cqrs, events, projection, snapshot, upcasting]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Event sourcing and CQRS: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** messaging, records in C# · **Time:** ~2 h

## Why it matters

An ordinary table stores a balance of 70 but does not say how it came about. An event stream stores "deposited 100", "withdrew 30", and any past state can be derived from it.

## The aggregate

```csharp
// Apply states facts: it never rejects an event that already happened.
public Account Apply(AccountEvent e) => e switch
{
    Deposited d => new(Balance + d.Amount, Version + 1),
    Withdrawn w => new(Balance - w.Amount, Version + 1),
    _ => throw new InvalidOperationException($"Unknown event {e.GetType().Name}"),
};
```

Rules are checked when a decision is made (a command), not when an event is applied. Events are named in the past tense.

## Event store and version

```csharp
public bool TryAppend(string stream, int expectedVersion, params AccountEvent[] events)
{
    if (Read(stream).Count != expectedVersion) return false; // someone else wrote first
```

This is optimistic concurrency: two read version 1, the second is refused and must reread. File: [Ex01_EventStore.cs](code/Examples/Ex01_EventStore.cs). The example keeps everything in memory and is not thread-safe.

## CQRS

Commands change state through the aggregate, queries read a separate model shaped for the screen. CQRS is possible without event sourcing, even in one database.

## Beginner mistakes

- ❌ An `AccountUpdated` event with the whole state → ✅ an event that carries intent.
- ❌ Rule checks in `Apply` → ✅ when deciding.
- ❌ Editing or deleting old events → ✅ a compensating event.
- ❌ Expecting the read model to update instantly → ✅ account for the delay.

## Related materials

- [Advanced theory](advanced.en.md)
- [CQRS inside an application](../../../csharp/modules/21-architecture/overview.en.md)

## Sources

- [Event Sourcing pattern](https://learn.microsoft.com/azure/architecture/patterns/event-sourcing)
- [CQRS pattern](https://learn.microsoft.com/azure/architecture/patterns/cqrs)
