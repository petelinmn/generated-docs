---
title: "Design process and estimation: basics"
lang: en
track: system-design
kind: module
doc: basics
slug: 01-design-process
part: 1
tags: [system-design, capacity-estimation, slo, availability, latency, percentiles]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Design process and estimation: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** basic C# · **Time:** ~2 h

## Why it matters

The phrase "build a chat" does not define a system. A chat for 100 people is one process and one database. A chat for 10 million is queues, sharding and several data centers. Numbers make the difference, so design starts with them.

## The design order

1. **Requirements.** Functional: send a message, read the history. Non-functional: latency, availability, data volume, security, budget. State explicitly what is *out* of scope.
2. **Estimates.** Requests per second, storage volume, traffic.
3. **API and data model.** Which operations and which entities.
4. **High-level design.** Client, load balancer, services, stores, queues.
5. **Bottlenecks.** What breaks first and what to do about it.
6. **Trade-offs.** What you chose, what you gave up and why.

## Back-of-the-envelope estimates

A few numbers are enough: a day has 86,400 seconds (≈10⁵), 1 million requests a day is about 12 per second. Peak load is the average multiplied by a peak factor; that is an assumption and you have to name it.

```csharp
public static Capacity Estimate(Workload w)
{
    double writesPerDay = w.DailyActiveUsers * w.WritesPerUserPerDay;
    double averageWriteQps = writesPerDay / SecondsPerDay;
    double peakWriteQps = averageWriteQps * w.PeakFactor;
    double peakReadQps = peakWriteQps * w.ReadsPerWrite;

    // Replicas multiply disk usage, not the incoming traffic.
    long storage = (long)(writesPerDay * w.BytesPerWrite * w.RetentionDays * w.ReplicationFactor);

    return new Capacity(averageWriteQps, peakWriteQps, peakReadQps, storage, peakReadQps * w.BytesPerWrite);
}
```

Full file: [Ex01_CapacityEstimate.cs](code/Examples/Ex01_CapacityEstimate.cs). For a chat with 10M DAU, 20 messages per user, 500 bytes per message, a ×3 peak, a 5:1 read-to-write ratio, 365 days of retention and three replicas you get:

| Quantity | Value |
|---|---|
| Average writes | ≈2,315 per second |
| Peak writes | ≈6,944 per second |
| Peak reads | ≈34,722 per second |
| Storage per year with replicas | 109.5 TB |
| Peak egress | ≈17.4 MB/s |

The conclusions matter more than the digits: 100 TB will not fit on one ordinary database server, so you will need sharding; reads outnumber writes 5 to 1, so a cache and read replicas make sense.

## Little's law

The number of requests inside a system equals the arrival rate multiplied by the time spent processing. At 2000 requests per second and 200 ms latency, 400 requests are in progress at once. This is how you size a connection pool and the number of instances.

```csharp
public static double InFlight(double requestsPerSecond, TimeSpan latency) =>
    requestsPerSecond * latency.TotalSeconds;
```

## SLI, SLO and SLA

| Term | What it is | Example |
|---|---|---|
| SLI (service level indicator) | a measured quantity | share of successful requests |
| SLO (service level objective) | the target value of an SLI | 99.9% over 30 days |
| SLA (service level agreement) | a contract with consequences for a breach | a refund if availability is below 99.5% |

The SLO is usually stricter than the SLA, so that you notice a problem before it becomes a breach of contract.

## Availability

| Target | Allowed downtime per 30 days | Per 365 days |
|---|---|---|
| 99% | 432 min | 5,256 min |
| 99.9% | 43.2 min | 525.6 min |
| 99.99% | 4.32 min | 52.56 min |
| 99.999% | 0.43 min | 5.26 min |

If a request needs all components, their availabilities multiply. If any one replica is enough, the failure probabilities multiply.

```csharp
// All components are required: probabilities multiply.
public static double Serial(params double[] parts) => parts.Aggregate(1.0, (acc, a) => acc * a);

// Any one replica is enough. Valid only when failures are independent.
public static double Parallel(params double[] replicas) =>
    1.0 - replicas.Aggregate(1.0, (acc, a) => acc * (1.0 - a));
```

Three 99.9% dependencies in a row give 99.70%. Two independent 99% replicas give 99.99%. File: [Ex02_Availability.cs](code/Examples/Ex02_Availability.cs).

## Beginner mistakes

- ❌ Drawing the diagram before clarifying requirements → ✅ questions and assumptions first.
- ❌ Counting only the average load → ✅ design for the peak.
- ❌ "We need 100% availability" → ✅ pick a target and calculate its price.
- ❌ Adding components "for the future" → ✅ every component must solve a named problem.

## Related materials

- [Advanced theory](advanced.en.md)
- [Scaling and load balancing](../02-scalability-and-load-balancing/overview.en.md)

## Sources

- [Google SRE Book: Service Level Objectives](https://sre.google/sre-book/service-level-objectives/)
