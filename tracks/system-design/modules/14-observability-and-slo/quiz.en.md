---
title: "Quiz: observability and SLOs"
lang: en
track: system-design
kind: module
doc: quiz
slug: 14-observability-and-slo
part: 3
tags: [system-design, observability, distributed-tracing, metrics, slo, histogram, sampling]
versions: ".NET 11, C# 15; Microsoft.Extensions.Telemetry 11.0.1"
status: draft
updated: 2026-11-26
---

# Quiz: observability and SLOs

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows the level: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 Which signal shows a request's path across services?

- A) Metrics
- B) Tracing
- C) Configuration
- D) A memory dump

<details><summary>Answer</summary>

**B.** → [Basics](basics.en.md)

</details>

## 2. 🟢 The instrument for the number of processed requests?

- A) `Counter`
- B) `Histogram`
- C) `Gauge`
- D) `ActivitySource`

<details><summary>Answer</summary>

**A.** → [Basics](basics.en.md)

</details>

## 3. 🟢 What represents a span in .NET?

- A) `Task`
- B) `Activity`
- C) `Meter`
- D) `ILogger`

<details><summary>Answer</summary>

**B.** → [Basics](basics.en.md)

</details>

## 4. 🟢 What does `StartActivity` return without a listener?

- A) An empty object
- B) `null`
- C) An exception
- D) A background span

<details><summary>Answer</summary>

**B.** → [Basics](basics.en.md)

</details>

## 5. 🟡 How many characters are in a trace-id?

- A) 16
- B) 32
- C) 36
- D) 64

<details><summary>Answer</summary>

**B.** → [Advanced](advanced.en.md)

</details>

## 6. 🟡 What can be added across instances?

- [ ] A) Counter values
- [ ] B) Histogram buckets with identical bounds
- [ ] C) p99
- [ ] D) Error counts

<details><summary>Answer</summary>

**A, B, D.** → [Advanced](advanced.en.md)

</details>

## 7. 🟡 Which tag is most dangerous for a metric?

- A) `outcome`
- B) `region`
- C) `order_id`
- D) `method`

<details><summary>Answer</summary>

**C.** An unbounded number of values. → [Advanced](advanced.en.md)

</details>

## 8. 🟡 What does this code print?

```csharp
var h = new Histogram(0.1, 0.5, 2);
h.Record(1.5);
Console.WriteLine(h.QuantileUpperBound(0.5));
```

<details><summary>Answer</summary>

```text
2
```

Only the bucket bound is known. → [Advanced](advanced.en.md)

</details>

## 9. 🔴 What happens without a listener?

```csharp
var a = source.StartActivity("op");
a!.SetTag("k", 1);
```

<details><summary>Answer</summary>

`NullReferenceException`. → [Expert](expert.en.md)

</details>

## 10. 🔴 Open question

Which signals and alerts would you set up for a payment service?

<details><summary>Key points of a good answer</summary>

- An SLI from the user's point of view and an SLO.
- Rate, errors, duration, dependencies.
- Burn rate in two windows and a runbook.
- No personal data in telemetry.

</details>
