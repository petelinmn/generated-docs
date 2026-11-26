---
title: "Observability and SLOs: basics"
lang: en
track: system-design
kind: module
doc: basics
slug: 14-observability-and-slo
part: 3
tags: [system-design, observability, distributed-tracing, metrics, slo, histogram, sampling]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Observability and SLOs: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** web applications in .NET · **Time:** ~2 h

## Why it matters

"Everything is slow" is not a diagnosis. You need to know what share of requests suffers, since when and at which stage.

## Three signals

| Signal | Answers | Cost grows with |
|---|---|---|
| Metrics | how much and how often | the number of tag combinations |
| Traces | where time went | the number of stored spans |
| Logs | what exactly happened | the volume of events |

## Metrics in .NET

| Instrument | What for |
|---|---|
| `Counter` | only grows: requests, errors |
| `UpDownCounter` | grows and shrinks: items in a queue |
| `Gauge`, `ObservableGauge` | a current value |
| `Histogram` | a distribution: durations |

```csharp
var outcome = new KeyValuePair<string, object?>("outcome", succeed ? "ok" : "error");
_orders.Add(1, outcome);
_duration.Record(seconds, outcome);
```

Names are written in lowercase with dots, units are given separately.

## Spans

In .NET a span is an `Activity`.

```csharp
// Null when nobody listens, hence the "?." below.
using Activity? activity = Source.StartActivity("PlaceOrder");
```

Without a listener `StartActivity` returns `null`: that saves resources and has to be accounted for. File: [Ex01_Instrumentation.cs](code/Examples/Ex01_Instrumentation.cs). The tests listen to metrics and spans through `MeterListener` and `ActivityListener`; in an application an exporter such as OpenTelemetry does that.

## Beginner mistakes

- ❌ Writing everything to logs and treating them as metrics → ✅ separate metrics.
- ❌ `activity.SetTag` without a `null` check → ✅ `activity?.`.
- ❌ Watching only CPU → ✅ errors and latency from the user's point of view.
- ❌ Secrets in tags and logs → ✅ hide them.

## Related materials

- [Advanced theory](advanced.en.md)
- [Observability in .NET](../../../csharp/modules/22-observability/overview.en.md)

## Sources

- [.NET: metrics instrumentation](https://learn.microsoft.com/dotnet/core/diagnostics/metrics-instrumentation)
- [.NET: tracing instrumentation](https://learn.microsoft.com/dotnet/core/diagnostics/distributed-tracing-instrumentation-walkthroughs)
