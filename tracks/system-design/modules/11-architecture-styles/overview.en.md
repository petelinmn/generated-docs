---
title: "Architecture styles"
lang: en
track: system-design
kind: module
doc: overview
slug: 11-architecture-styles
part: 2
levels: [1, 2, 3, 4, 5, 6]
tags: [system-design, architecture, modular-monolith, microservices, fitness-function, adr]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Architecture styles

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

A style defines how a system is split into parts and how the parts talk. The choice depends on the team, the load and the pace of change, not on fashion.

## Key ideas

- A monolith is one deployment unit, not necessarily a mess.
- A modular monolith keeps boundaries inside one process.
- Microservices give independence at the price of distribution.
- A network call is slower and less reliable than a method call.
- Boundaries follow business capabilities.
- Architecture rules are worth checking with tests.
- Decisions are recorded in ADRs.

## Minimal example

```csharp
bus.Subscribe<OrderPlaced>(e => Invoices[e.OrderId] = e.Total);
```

## When to use and when not

- ✅ A new product: start with a modular monolith.
- ✅ Several teams get in each other's way when releasing.
- ❌ Microservices for a team of three without a reason.
- ❌ Splitting technical layers into separate services.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | styles and their cost, a modular monolith |
| [2. Advanced](advanced.en.md) | boundaries, coupling, fitness functions, migration, ADR |
| [3. Expert](expert.en.md) | network cost, reference, gotchas, questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) |
| [5. Runnable examples](code/Examples/) | a modular monolith, boundary checks |
| [6. Tests](code/Tests/) | `dotnet test` inside `code/` |

## Related materials

- Prerequisites: [messaging and events](../08-messaging-and-events/overview.en.md), [distributed transactions](../10-distributed-transactions/overview.en.md)
- Inside an application: [C#: architecture](../../../csharp/modules/21-architecture/overview.en.md)
- Deployment: [Kubernetes: deployments](../../../kubernetes/modules/05-deployments/overview.en.md)
- [Views, functions and triggers in PostgreSQL](../../../postgresql/modules/07-views-functions/overview.en.md)
