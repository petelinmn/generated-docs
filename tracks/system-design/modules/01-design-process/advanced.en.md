---
title: "Design process and estimation: advanced"
lang: en
track: system-design
kind: module
doc: advanced
slug: 01-design-process
part: 1
tags: [system-design, capacity-estimation, slo, availability, latency, percentiles]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Design process and estimation: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Percentiles instead of the average

p99 = 200 ms means that 99% of requests are no slower than 200 ms. The nearest-rank method takes the smallest sample that at least p% of the samples do not exceed.

```csharp
double[] sorted = [.. samples.Order()];
int rank = (int)Math.Ceiling(p * sorted.Length / 100.0);
return sorted[rank - 1];
```

For a sample of 98 requests at 10 ms and 2 requests at 2000 ms the average is 49.8 ms, p50 is 10 ms and p99 is 2000 ms. The average describes neither the typical user nor the affected one. File: [Ex03_Percentiles.cs](code/Examples/Ex03_Percentiles.cs).

## Tail amplification with fan-out

If a page calls 100 services in parallel and waits for all of them, and each call is slow with probability 1%, the page is slow with probability `1 - 0.99^100` ≈ 63%. What helps: reducing the fan-out, timeouts with a partial response, a delayed duplicate request (hedged request) for idempotent reads.

## Error budget and burn rate

The error budget is `1 - SLO`. With a 99.9% SLO and a million requests you may "spend" 1000 failures; 500 failures is half the budget. While budget remains, the team ships changes; once it is exhausted, it works on reliability.

The burn rate shows how fast the budget is consumed relative to the SLO: the error ratio divided by the allowed ratio. A burn rate of 1 spends the budget exactly over the SLO window. Recommendations from the Google SRE Workbook for a 99.9% SLO:

| Reaction | Long window | Short window | Burn rate | Budget consumed |
|---|---|---|---|---|
| page the on-call | 1 h | 5 min | 14.4 | 2% |
| page the on-call | 6 h | 30 min | 6 | 5% |
| open a ticket | 3 days | 6 h | 1 | 10% |

The alert fires when the threshold is exceeded in both windows: the long one filters out random spikes, the short one clears the alert quickly after recovery.

## Recording decisions (ADR)

An Architecture Decision Record is a short document: context, decision, alternatives considered, consequences. It lives next to the code and is not rewritten after the fact: a new decision gets a new record that supersedes the old one.

## Best practices

- Write assumptions next to the calculation: they will be checked and changed.
- Round to an order of magnitude.
- Define SLIs from the user's point of view, not from CPU usage.
- Look for single points of failure on the diagram.

## Common mistakes

- ❌ Averaging the p99 of several servers → ✅ merge the raw samples or histograms.
- ❌ Treating replicas as independent when they share a database or a region → ✅ account for common causes of failure.
- ❌ An SLO without a time window → ✅ "99.9% over 30 days".
- ❌ Confusing Mbit/s and MB/s → ✅ always write units.

## Comparison with other tracks

- How to collect these metrics in .NET — [observability](../../../csharp/modules/22-observability/overview.en.md).
- Probes and resource limits that affect availability — [Kubernetes: probes and resources](../../../kubernetes/modules/11-probes-and-resources/overview.en.md).

## Related materials

- [Basics](basics.en.md)
- [Expert theory](expert.en.md)

## Sources

- [Google SRE Book: Service Level Objectives](https://sre.google/sre-book/service-level-objectives/)
- [Google SRE Workbook: Alerting on SLOs](https://sre.google/workbook/alerting-on-slos/)
