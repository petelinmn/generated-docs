---
title: "Observability and SLOs"
lang: en
track: system-design
kind: module
doc: overview
slug: 14-observability-and-slo
part: 3
levels: [1, 2, 3, 4, 5, 6]
tags: [system-design, observability, distributed-tracing, metrics, slo, histogram, sampling]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Observability and SLOs

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

Observability lets you understand from outside signals what happens inside. The main signals are metrics, traces and logs; SLOs say which deviations matter.

## Key ideas

- Metrics show that something is wrong; traces show where.
- Tags with many values are expensive.
- Trace context travels between services in the `traceparent` header.
- Percentiles cannot be averaged, but histogram buckets can be added.
- Alerts are built on SLOs and burn rate.
- Sampling must be consistent across a whole trace.
- .NET already has the tools in `System.Diagnostics`.

## Minimal example

```csharp
using Activity? activity = Source.StartActivity("PlaceOrder");
_orders.Add(1, outcome);
```

## When to use and when not

- ✅ Any service in production.
- ✅ A distributed request that cannot be followed otherwise.
- ❌ A user identifier as a metric tag.
- ❌ An alert on everything measurable.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | signals, metric instruments, spans |
| [2. Advanced](advanced.en.md) | context, histograms, cardinality, alerts |
| [3. Expert](expert.en.md) | reference, gotchas, questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) |
| [5. Runnable examples](code/Examples/) | `Meter`, `ActivitySource`, a histogram |
| [6. Tests](code/Tests/) | `dotnet test` inside `code/` |

## Related materials

- Prerequisites: [design process and SLOs](../01-design-process/overview.en.md), [reliability patterns](../09-reliability-patterns/overview.en.md)
- In other tracks: [observability in .NET](../../../csharp/modules/22-observability/overview.en.md), [observability in Kubernetes](../../../kubernetes/modules/16-observability/overview.en.md), [Serilog](../../../csharp/libraries/serilog/overview.en.md)
