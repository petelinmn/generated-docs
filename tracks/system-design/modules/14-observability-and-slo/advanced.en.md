---
title: "Observability and SLOs: advanced"
lang: en
track: system-design
kind: module
doc: advanced
slug: 14-observability-and-slo
part: 3
tags: [system-design, observability, distributed-tracing, metrics, slo, histogram, sampling]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Observability and SLOs: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Context propagation

W3C Trace Context: `traceparent: version-trace-id-parent-id-trace-flags`, lowercase hexadecimal.

| Field | Length | Invalid |
|---|---|---|
| version | 2 | `ff` |
| trace-id | 32 | all zeros |
| parent-id | 16 | all zeros |
| trace-flags | 2 | — |

The lowest bit of the flags is sampled. It is a recommendation, not a guarantee of recording.

## Histograms

```csharp
public void Merge(Histogram other)
{
    for (int i = 0; i < _counts.Length; i++) _counts[i] += other._counts[i];
}
```

Buckets of different instances add up, percentiles do not. The price: the result is known only up to a bucket bound. File: [Ex02_Histogram.cs](code/Examples/Ex02_Histogram.cs).

## Cardinality

Every combination of tag values is a separate time series. The .NET documentation calls fewer than about 1000 combinations per instrument safe, and 10–100 times fewer for histograms. A customer identifier as a tag is a bad idea; it belongs in a span or a log.

## SLO alerts

A "budget is burning fast" alert requires the burn rate to be exceeded in both a long and a short window; the thresholds are covered in [module 01](../01-design-process/advanced.en.md).

## Best practices

- For every service: rate, errors, duration.
- Add `trace_id` to logs.
- Put bucket bounds around the SLO threshold.
- An alert should come with instructions.

## Common mistakes

- ❌ Alerts on causes (CPU) instead of symptoms → ✅ on what the user feels.
- ❌ Each service samples at random → traces with holes. ✅ A consistent decision.
- ❌ Creating a `Meter` per request → ✅ one per component.
- ❌ URLs with identifiers as a tag → ✅ the route template.

## Comparison with other tracks

- Health checks and OpenTelemetry — [C#](../../../csharp/modules/22-observability/overview.en.md).
- Collection in a cluster — [Kubernetes](../../../kubernetes/modules/16-observability/overview.en.md).

## Related materials

- [Basics](basics.en.md)
- [Expert theory](expert.en.md)

## Sources

- [W3C Trace Context](https://www.w3.org/TR/trace-context/)
- [.NET: metrics instrumentation](https://learn.microsoft.com/dotnet/core/diagnostics/metrics-instrumentation)
