---
title: "Observability: expert"
lang: en
track: csharp
kind: module
doc: expert
slug: 22-observability
part: 3
tags: [observability, activity-listener, meter-listener, sampling, cardinality, slo, tail-sampling, interview-questions]
versions: ".NET 11, C# 15; OpenTelemetry 1.21.0"
status: draft
updated: 2026-11-22
---

# Observability: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

**Prerequisites:** [basic](basics.en.md) and [advanced](advanced.en.md) theory. **Time:** ~1 h.

## The "source — listener" model

The .NET platform separates **emitting** signals from **collecting** them. Code creates an `ActivitySource` and a `Meter` and always writes to them; whether anyone needs this data is decided by **listeners**: `ActivityListener` (traces), `MeterListener` (metrics), `ILogger` providers. The OpenTelemetry SDK is just a sophisticated listener.

- **Zero cost without a listener.** While there is no listener, `StartActivity` returns `null` (verified), and `Counter.Add` is almost a no-op. That is why instrumentation is added to libraries freely.
- **Subscription by name.** A listener selects sources by name (`ShouldListenTo`, `AddSource("Demo.Shop")`, `AddMeter`). A forgotten `AddSource` is a frequent cause of "there are no spans".
- **The decision to record** is made by the listener's `Sample`. Verified: `None` → `null`; `PropagationData` → an activity is created but data is not collected (`IsAllDataRequested = False`); `AllData` → `IsAllDataRequested = True`; `AllDataAndRecorded` → also `Recorded = True` (the recorded flag, passed in `traceparent` as `01`).
- **`Activity.Current`** is stored in an `AsyncLocal`, so it flows through `await` and `Task.Run` ([10 · Async/await](../10-async-await/expert.en.md#execution-context-executioncontext-and-asynclocalt)); child spans get their parent implicitly. After an `await` on a "foreign" thread (for example through your own pool) the context can be lost.
- **`MeterListener`** receives measurements synchronously on the recording thread (verified: `shop.orders=1 [region:eu]` arrived inside `Add`); an `ObservableGauge` is polled on `RecordObservableInstruments()`. So callbacks must be fast and thread-safe.

## Data subtleties

- **`Tags` and `TagObjects`.** `SetTag("order.id", 42)` puts a value of type `Int32` into `TagObjects`, while `Tags` holds only string tags (verified). Exporters use `TagObjects`.
- **Metric cardinality.** Each unique combination of tag values is a separate time series; `user_id` produces millions of series and the backend falls over. Large identifiers go in **traces and logs**, not in metric tags.
- **Aggregation.** Counters and histograms are aggregated on the SDK side over an interval (cumulatively or as a delta); raw measurements are not stored. For histograms the bucket boundaries matter: pick them for the expected latencies.
- **Exemplars.** Exemplars link a metric point to a specific trace — the "spike on the chart → trace" transition.
- **Baggage** is key-value pairs that travel with the context (the `baggage` header); it costs traffic and can leak — do not put secrets in it.

## Sampling

| Kind | How it works | Pros | Cons |
|---|---|---|---|
| Head (at the start) | decided when the root span is created, inherited (`ParentBased`) | cheap, simple | may miss interesting errors |
| Tail (at the end) | a collector sees the whole trace and keeps failed/slow ones | keeps what matters | needs a buffer and state on the collector |
| By ratio (`TraceIdRatioBased`) | deterministically by `TraceId` | the same on all services | ignores "importance" |

The decision must be made **consistently** across services — otherwise traces come out torn: the parent's decision is passed by flags in `traceparent`.

## SLI, SLO and alerts

- An **SLI** is a measurable quality indicator (the share of successful requests, p95 latency); an **SLO** is a target for an SLI ("99.9% successful over 30 days"); the **error budget** is the allowed share of failures, used to manage the pace of change.
- Alert on **symptoms the user sees** (errors, latency, saturation), not on every internal cause: less noise. Methods: RED (rate, errors, duration — for services), USE (utilization, saturation, errors — for resources), the "four golden signals".
- An alert must be **actionable**: a person knows what to do; otherwise it is switched off.

## Process diagnostics

Besides OpenTelemetry, .NET has low-level tools: **EventPipe/EventSource** (runtime counters and events: GC, thread pool, allocations) is read through `dotnet-counters`/`dotnet-trace`; dumps (`dotnet-dump`) and profilers give an "inside the process" picture ([19 · Performance](../19-performance/overview.en.md)). OpenTelemetry metrics show the symptom at the service level, while a profiler shows the cause in the code.

> ⚠️ Verify: the `dotnet-counters`, `dotnet-trace`, `dotnet-dump` commands were not run in this material (the tools were not installed); check the documentation before use.

## Reference

| What | API / tool |
|---|---|
| Spans | `ActivitySource`, `Activity`, `ActivityKind`, `SetTag`, `AddEvent`, `SetStatus` |
| Metrics | `Meter`, `Counter<T>`, `UpDownCounter<T>`, `Histogram<T>`, `ObservableGauge<T>` |
| Listeners | `ActivityListener`, `MeterListener` |
| Context | W3C `traceparent`, `tracestate`, `baggage` |
| SDK | `OpenTelemetry.*`: `AddSource`, `AddMeter`, exporters (OTLP, console, in-memory) |
| Health | `AddHealthChecks`, `MapHealthChecks`, `HealthCheckResult` |
| Diagnostics | `dotnet-counters`, `dotnet-trace`, `dotnet-dump` |

## Gotchas

**1. `StartActivity` returns `null`.** Without a listener or with `None`: write `a?.SetTag(...)` (verified).

**2. Forgot `AddSource`/`AddMeter`.** The data is emitted but the SDK does not listen to it.

**3. `Tags` does not show non-string tags** (verified) — look at `TagObjects`.

**4. High tag cardinality** ruins the metrics backend.

**5. A liveness check with dependencies.** A database failure restarts all instances of the application; liveness must check only the process.

**6. `Degraded` is 200 by default** (verified): the orchestrator does not remove the instance from balancing; for 503 change `ResultStatusCodes`.

**7. Context loss.** Your own thread/pool without `Activity.Current` breaks the trace; instrument brokers and background tasks by hand.

**8. Secrets and PII in tags/logs** end up in external systems.

**9. Heavy `MeterListener`/`ObservableGauge` handlers** slow the application, since they run synchronously.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| OpenTelemetry + OTLP | a standard, any backend | you must set up a collector and a backend |
| A vendor SDK (Application Insights, Datadog) | rich integrations out of the box | vendor lock-in |
| Logs only | simple | no metrics and end-to-end traces |
| Prometheus + Grafana, Jaeger/Tempo, Loki | an open stack | you operate it yourself |
| An on-demand profiler | the cause in code | does not replace monitoring |

## Interview questions

<details><summary>1. What is observability and how does it differ from monitoring?</summary>

Monitoring answers known questions in advance (charts and alerts on known metrics); observability is the ability to figure out **unplanned** problems from logs, metrics and traces.

</details>

<details><summary>2. The three observability signals and when to use which?</summary>

Metrics — "something is wrong, how much" (alerts and trends), traces — "where time is lost/where the error is in a chain of services", logs — "what exactly happened" (details). They are tied together by shared identifiers.

</details>

<details><summary>3. What is OpenTelemetry?</summary>

A vendor-neutral standard of APIs, an SDK and the OTLP protocol for logs, metrics and traces. In .NET the sources are part of the platform (`ActivitySource`, `Meter`), and the SDK subscribes to them and exports.

</details>

<details><summary>4. How are `Activity` and an OpenTelemetry span related?</summary>

They are the same thing: `Activity` is the .NET implementation of a span; `ActivitySource` is the source, `ActivityKind` the kind (Server, Client, Internal...).

</details>

<details><summary>5. How does a trace pass from service to service?</summary>

Through the W3C `traceparent` header (`00-<trace-id>-<span-id>-<flags>`): client instrumentation adds it, server instrumentation reads it and creates a child span. Verified: five spans in one trace when `/outer` calls `/inner`.

</details>

<details><summary>6. What is metric cardinality and why does it matter?</summary>

The number of unique combinations of tag values; each is a separate series. `user_id` as a tag gives millions of series and breaks the backend.

</details>

<details><summary>7. How do Counter, UpDownCounter, Histogram and ObservableGauge differ?</summary>

A Counter is a monotonic counter; an UpDownCounter rises and falls; a Histogram is a distribution of values (percentiles); an ObservableGauge is the current value read on demand.

</details>

<details><summary>8. How does liveness differ from readiness?</summary>

Liveness — the process is alive (no external dependencies; on failure a restart), readiness — ready to accept traffic (checks dependencies; on failure removed from balancing). Verified: when the database fails, `/health/live` is 200 and `/health/ready` is 503.

</details>

<details><summary>9. What is trace sampling and what kinds are there?</summary>

Choosing which traces to keep: head (a decision at the start, `ParentBased`/by ratio) and tail (on the collector by outcome, keeping errors and slow ones). The decision must be consistent across services.

</details>

<details><summary>10. What are SLI, SLO and an error budget?</summary>

An SLI is a measurable indicator, an SLO is a target for it, an error budget is the allowed share of failures; the release pace is managed with it.

</details>

<details><summary>11. Trick question: `activity.Tags` is empty although you called `SetTag("order.id", 42)`. Why?</summary>

`Tags` holds only string tags; the number is in `TagObjects` (verified).

</details>

<details><summary>12. Trick question: spans exist in the code, but the exporter receives nothing. Why?</summary>

You did not subscribe to the source (`AddSource("Demo.Shop")`) or the sampler returned `None` — then `StartActivity` gives `null` (verified).

</details>

<details><summary>13. Trick question: a health check returned `Degraded`, yet the load balancer keeps sending traffic. Why?</summary>

By default `Degraded` answers HTTP 200 (verified); for 503 you need to configure `ResultStatusCodes`.

</details>

## Related materials

- Back: [advanced theory](advanced.en.md). Check yourself: [quiz](quiz.en.md) and [exercises](exercises.en.md).
- [19 · Performance](../19-performance/overview.en.md) — profiling and runtime counters.
- [21 · Application architecture](../21-architecture/overview.en.md) — tracing between services and Outbox.
- [10 · Async/await](../10-async-await/overview.en.md) — the execution context.
- [Kubernetes](../../../kubernetes/overview.en.md) — probes and cluster monitoring.

## Sources

- [`ActivityListener`](https://learn.microsoft.com/dotnet/api/system.diagnostics.activitylistener)
- [`MeterListener`](https://learn.microsoft.com/dotnet/api/system.diagnostics.metrics.meterlistener)
- [Sampling in OpenTelemetry](https://opentelemetry.io/docs/concepts/sampling/)
- [Google SRE Book: SLOs](https://sre.google/sre-book/service-level-objectives/)
