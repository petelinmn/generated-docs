---
title: "Architecture styles: basics"
lang: en
track: system-design
kind: module
doc: basics
slug: 11-architecture-styles
part: 2
tags: [system-design, architecture, modular-monolith, microservices, fitness-function, adr]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Architecture styles: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** experience building a web application · **Time:** ~2 h

## Why it matters

Badly drawn boundaries are expensive to change. The style determines what each next change will cost and what can fail independently.

## Styles

| Style | Essence |
|---|---|
| N-tier | layers, each calling only the ones below |
| Web-Queue-Worker | a web front end, a queue and a background worker |
| Microservices | small autonomous services with their own data |
| Event-driven | producers and consumers through a broker |
| Modular monolith | one process, strict modules |
| Serverless | functions managed by the platform |

The Azure Architecture Center also describes big data and big compute separately.

## Monolith and microservices

| | Monolith | Microservices |
|---|---|---|
| Deployment | one | per service |
| Calls | in memory | over the network |
| Transactions | local | saga |
| Operations | simpler | needs automation and observability |
| Team independence | low | high |

## A modular monolith

Modules talk through contracts: interfaces and events, not each other's internal classes.

```csharp
public int Place(decimal total)
{
    int id = ++_lastId;
    bus.Publish(new OrderPlaced(id, total));
    return id;
}
```

`OrderService` knows nothing about billing; `InvoiceService` subscribes to the event. File: [Ex01_ModularMonolith.cs](code/Examples/Ex01_ModularMonolith.cs). The bus in the example is synchronous: a handler failure fails the publisher.

## Beginner mistakes

- ❌ A distributed monolith: services release only together → ✅ independent contracts and data.
- ❌ One database for all services → ✅ each has its own data.
- ❌ Modules that know each other's internals → ✅ contracts.
- ❌ Choosing a style by fashion → ✅ by requirements.

## Related materials

- [Advanced theory](advanced.en.md)
- [Application architecture in C#](../../../csharp/modules/21-architecture/overview.en.md)

## Sources

- [Azure: architecture styles](https://learn.microsoft.com/azure/architecture/guide/architecture-styles/)
