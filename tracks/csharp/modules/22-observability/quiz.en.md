---
title: "Quiz: Observability"
lang: en
track: csharp
kind: module
doc: quiz
slug: 22-observability
part: 3
tags: [observability, metrics, tracing, opentelemetry, health-checks]
status: draft
updated: 2026-11-22
---

# Quiz: Observability

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 Which signal best answers "how many requests per second and what is the p95 latency"?

- A) logs
- B) metrics
- C) traces
- D) memory dumps

<details><summary>Answer</summary>

**B.** Metrics are numeric series over time (counters, histograms). Logs answer "what exactly happened", traces — "where one request's time went".
→ [Basics](basics.en.md#why-it-matters)

</details>

## 2. 🟢 What does an `Activity` represent in .NET?

- A) a metric
- B) a trace span — an operation with a start, an end, tags and a parent
- C) a log entry
- D) a health check

<details><summary>Answer</summary>

**B.** An `ActivitySource` creates an `Activity`; nesting is built through `Activity.Current` (verified: `SaveToDb` and `Notify` are children of `PlaceOrder`).
→ [Basics](basics.en.md#tracing)

</details>

## 3. 🟢 What does `source.StartActivity("x")` return if nobody is subscribed to the source?

- A) an empty activity
- B) `null`
- C) an exception
- D) an activity with a zero `TraceId`

<details><summary>Answer</summary>

**B.** That is why you write `activity?.SetTag(...)` (verified).
→ [Basics](basics.en.md#tracing)

</details>

## 4. 🟢 What do `/health/ready` and `/health/live` return if the database "went down" (the check returned `Unhealthy`) and the liveness check has no dependencies?

- A) `200` and `200`
- B) `503` and `200`
- C) `503` and `503`
- D) `200` and `503`

<details><summary>Answer</summary>

**B.** Readiness depends on the database and reports "not ready"; liveness checks only the process (verified). This way the orchestrator does not restart the application because of someone else's failure.
→ [Basics](basics.en.md#health-checks)

</details>

## 5. 🟡 How do spans of different services combine into one trace?

- A) by matching names
- B) through the W3C `traceparent` header (`00-<trace-id>-<span-id>-<flags>`), which client instrumentation passes and server instrumentation reads
- C) by IP address
- D) by time

<details><summary>Answer</summary>

**B.** Verified: the `/inner` handler received a `traceparent`, and in the exporter five spans ended up in one trace.
→ [Advanced theory](advanced.en.md#context-propagation-between-services)

</details>

## 6. 🟡 What does `activity.Tags` hold after `SetTag("order.id", 42); SetTag("name", "ann")`?

- A) both tags
- B) only `name=ann`; the number is in `TagObjects`
- C) nothing
- D) only `order.id=42`

<details><summary>Answer</summary>

**B.** `Tags` holds only string tags, while `TagObjects` holds all (verified: `order.id=42:Int32`).
→ [Advanced theory](advanced.en.md#span-tags-events-and-statuses)

</details>

## 7. 🟡 Select all correct statements about metrics

- [ ] A) a `Histogram` suits response time and percentiles
- [ ] B) a `user_id` tag with millions of values is a good idea
- [ ] C) an `ObservableGauge` is read on the collector's request
- [ ] D) a `Counter` must grow monotonically

<details><summary>Answer</summary>

**A, C, D.** B is wrong: high tag cardinality produces millions of series. Verified: the histogram recorded `12.5`, the gauge returned `3`.
→ [Basics](basics.en.md#metrics)

</details>

## 8. 🟡 What does `ActivitySamplingResult.AllData` mean compared with `AllDataAndRecorded`?

- A) the same thing
- B) `AllData` collects data (`IsAllDataRequested = True`), `AllDataAndRecorded` also marks the activity as recorded (`Recorded = True`)
- C) `AllData` disables the trace
- D) `AllDataAndRecorded` disables tags

<details><summary>Answer</summary>

**B.** Verified: `None` → `null`; `PropagationData` → created without collecting data; `AllData` → `Recorded = False`; `AllDataAndRecorded` → `Recorded = True`.
→ [Advanced theory](advanced.en.md#sampling)

</details>

## 9. 🟡 Open question: how do you link a log, a metric and a trace of one problem?

<details><summary>Key points of a good answer</summary>

- logs and traces share a `TraceId` (`Activity.Current`); the logger adds it automatically;
- metrics show "what and how much" (a rise in errors/latency), and through tags and exemplars you move to a trace;
- the trace shows which span has the error or lost time, and a log with the same `TraceId` gives the details;
- consistent service and resource names (`service.name`), OpenTelemetry conventions.

</details>

## 10. 🔴 Why is sampling made consistent across services?

- A) for beauty
- B) otherwise traces come out torn: the parent's decision is passed by flags in `traceparent`, and child services must honor it (`ParentBased`)
- C) to save disk
- D) because W3C requires it

<details><summary>Answer</summary>

**B.** If one service recorded a span and the next dropped it, the storage will hold traces with holes. Head sampling is inherited from the parent; tail sampling is done by the collector.
→ [Expert theory](expert.en.md#sampling)

</details>

## 11. 🔴 A health check returned `Degraded`, yet the load balancer keeps sending traffic. Why and how do you fix it?

- A) a load balancer bug
- B) by default `Degraded` answers HTTP 200; for 503 you need to configure `ResultStatusCodes`
- C) `Degraded` does not exist
- D) you must restart the application

<details><summary>Answer</summary>

**B.** Verified: `Degraded -> 200 Degraded`. The result status codes are set in `HealthCheckOptions.ResultStatusCodes`.
→ [Expert theory](expert.en.md#gotchas)

</details>
