---
title: "Design process and estimation"
lang: en
track: system-design
kind: module
doc: overview
slug: 01-design-process
part: 1
levels: [1, 2, 3, 4, 5, 6]
tags: [system-design, capacity-estimation, slo, availability, latency, percentiles]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Design process and estimation

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

System design starts not with a box diagram but with questions and numbers: what the system does, what load it gets, how fast and available it has to be. This module gives the order of steps and the arithmetic that the other decisions rest on.

## Key ideas

- Requirements first: functional (what it does) and non-functional (how well it does it).
- Back-of-the-envelope estimates give the order of magnitude, not precision.
- An SLI measures, an SLO sets the target, an SLA is a contract with consequences.
- Required dependencies lower availability, independent replicas raise it.
- Latency is described with percentiles: the average hides the tail.
- The wider the fan-out, the more often a user sees the tail.
- Every decision is a trade-off worth writing down.

## Minimal example

```csharp
double writesPerDay = 10_000_000 * 20;          // DAU * messages per user
double averageQps = writesPerDay / 86_400;      // about 2315
double peakQps = averageQps * 3;                // about 6944
```

## When to use and when not

- ✅ A new system or a large feature where an architecture mistake is expensive.
- ✅ A discussion about goals: "it must be fast" becomes a number.
- ✅ A system design interview.
- ❌ A one-week prototype: a list of assumptions is enough.
- ❌ An exact capacity forecast: that needs load tests and measurements.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | the design order, requirements, estimates, availability |
| [2. Advanced](advanced.en.md) | percentiles, error budget, burn rate, ADR, common mistakes |
| [3. Expert](expert.en.md) | reference, calculation gotchas, interview questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) |
| [5. Runnable examples](code/Examples/) | load calculator, availability, percentiles |
| [6. Tests](code/Tests/) | `dotnet test` inside `code/` |

## Related materials

- Prerequisites: [C# basics](../../../csharp/modules/02-types-and-variables/overview.en.md)
- Next: [scaling and load balancing](../02-scalability-and-load-balancing/overview.en.md), [caching](../03-caching/overview.en.md)
- The same idea in other tracks: [observability in .NET](../../../csharp/modules/22-observability/overview.en.md), [application architecture](../../../csharp/modules/21-architecture/overview.en.md)
- [Data storage](../04-data-storage/overview.en.md)
- [API design](../05-api-design/overview.en.md)
- [Reliability patterns](../09-reliability-patterns/overview.en.md)
- [Observability and SLOs](../14-observability-and-slo/overview.en.md)
- [Multi-region and recovery](../16-multi-region-and-dr/overview.en.md)
- [Case studies](../17-design-case-studies/overview.en.md)
