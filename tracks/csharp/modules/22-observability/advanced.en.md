---
title: "Observability: advanced"
lang: en
track: csharp
kind: module
doc: advanced
slug: 22-observability
part: 3
tags: [opentelemetry, distributed-tracing, trace-context, instrumentation, exporters, health-checks, correlation]
versions: ".NET 11, C# 15; OpenTelemetry 1.21.0"
status: draft
updated: 2026-11-22
---

# Observability: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [basics](basics.en.md). **Time:** ~1.5 h.

## OpenTelemetry in an application

**OpenTelemetry** (OTel) is a vendor-neutral standard: APIs, an SDK and the OTLP protocol for logs, metrics and traces. In .NET the instruments themselves (`ActivitySource`, `Meter`, `ILogger`) are part of the platform; the OTel SDK **subscribes** to them and sends data to exporters. Packages: `OpenTelemetry.Extensions.Hosting`, instrumentations (`OpenTelemetry.Instrumentation.AspNetCore`, `…Http`) and exporters (OTLP, console, in-memory for tests).

```csharp
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("shop-api"))                      // who emits the data
    .WithTracing(t => t.AddAspNetCoreInstrumentation()                      // incoming HTTP requests
                       .AddHttpClientInstrumentation()                      // outgoing HTTP requests
                       .AddSource("Demo.Shop")                              // your own spans
                       .AddInMemoryExporter(spans))                         // replace with OTLP in production
    .WithMetrics(m => m.AddAspNetCoreInstrumentation().AddMeter("Demo.Shop"));
```

The resource (`service.name` and other attributes) says **who** emits the signals. To send to a collector and to backends (Jaeger, Tempo, Prometheus and so on) you add the OTLP exporter (`.AddOtlpExporter()` or `UseOtlpExporter()`); the address is usually set with the environment variable `OTEL_EXPORTER_OTLP_ENDPOINT`.

> ⚠️ Verify: sending over OTLP to a real collector was not run in this material (there is no collector); only in-memory exporters and local listeners were verified. The scheme and environment variable names follow the OpenTelemetry documentation; check them before use.

Verified with in-memory exporters: after `using (source.StartActivity("Checkout")) …` the span list holds one span `Checkout` with the tag `user=ann`; the metrics `shop.orders` and `shop.queue.length` were exported after `ForceFlush`.

## Context propagation between services

For spans of different services to combine into one trace, identifiers are passed in the **`traceparent`** header of the **W3C Trace Context** standard: `00-<trace-id>-<span-id>-<flags>` (verified: `00-cd3fac0abe95f2592309ede0555db9ec-20a2ab3b656840de-02`; `Activity.IdFormat` is `W3C`). The `HttpClient` instrumentation adds the header to an outgoing request, the ASP.NET Core instrumentation reads it on entry and creates a child span.

Verified on an application where `/outer` calls `/inner` through `HttpClient`: the `/inner` handler received a `traceparent` header, and the exporter collected five spans **in one trace** (`single trace: True`):

```text
Client   GET                parent=-    source=System.Net.Http     (the test client's request)
Server   GET /outer         parent=yes  source=Microsoft.AspNetCore
Internal business-work      parent=yes  source=Demo.Shop           (our own span)
Client   GET                parent=yes  source=System.Net.Http     (the call to /inner)
Server   GET /inner         parent=yes  source=Microsoft.AspNetCore
```

Your own spans become children of the current one through `Activity.Current`, which is stored in an `AsyncLocal` and therefore flows through `await` ([10 · Async/await](../10-async-await/expert.en.md#execution-context-executioncontext-and-asynclocalt)). For message queues the context is passed in message attributes by hand (or through the broker client's instrumentation).

## Span tags, events and statuses

```csharp
a.SetTag("order.id", 42); a.SetTag("name", "ann");
a.Tags          // name=ann              — only string values
a.TagObjects    // order.id=42 (Int32), name=ann (String) — all values
a.GetTagItem("order.id")   // 42
```

Verified: the `Tags` property holds **only string** tags, while the number `42` is visible in `TagObjects`/`GetTagItem`. So when debugging, look at `TagObjects`. Span events (`AddEvent`) are timestamps inside a span; the status is `Unset`/`Ok`/`Error` (`SetStatus(ActivityStatusCode.Error, "…")`). Name tags by OpenTelemetry **semantic conventions** (`db.system`, `http.request.method`, `url.path`) so that backends understand the data.

## Sampling

Storing all traces is expensive. A **sampler** decides whether to record a span: `AlwaysOn`, `AlwaysOff`, by ratio (`TraceIdRatioBased`), parent-based (`ParentBased` — follow the parent's decision, which matters for whole traces). In an `ActivityListener` this is `Sample`; verified: `None` — `StartActivity` returns `null`; `PropagationData` — an activity is created only to pass context (`IsAllDataRequested = False`); `AllData` — data is collected (`IsAllDataRequested = True`, `Recorded = False`); `AllDataAndRecorded` — it is also marked as recorded (`Recorded = True`). Without a listener `StartActivity` also returns `null`. Metrics are not sampled — they are aggregated.

## Metrics: practices

- Units in the name and the `unit` field (`ms`, `s`, `By`, `{order}`); names in the style `domain.object.measurement`.
- Tag **cardinality**: `region` (3 values) is fine, `user_id` (millions) is not.
- For latencies use histograms (`Histogram`), from which p50/p95/p99 percentiles are built; the average hides the tails.
- Platform metrics (`Microsoft.AspNetCore.*`, `System.Runtime`) are collected by instrumentations with no code.
- An `ObservableGauge` calls your delegate on collection, so the delegate is fast and safe.

## Logs, traces and correlation

Log entries are tied to a trace by the `TraceId` and `SpanId` fields — OpenTelemetry/console logging providers add them automatically from `Activity.Current`. Verified: inside `StartActivity`, `Activity.Current` equals the created activity and it has a `TraceId`. This way one `TraceId` finds both the trace and all its logs, and the error. Exceptions are recorded both in the log and in the span (`SetStatus(Error)`).

## Health checks

| Check | Purpose |
|---|---|
| liveness (`/health/live`) | the process is alive; no external dependencies; on failure — a restart |
| readiness (`/health/ready`) | the application is ready to accept traffic; checks the database, cache, queue; on failure — remove from balancing |
| startup | the application has finished a slow start |

Tags and a `Predicate` in `MapHealthChecks` separate the checks by address (verified in the example above). The unhealthy status is `Unhealthy` (HTTP 503) or `Degraded` (HTTP 200 by default). Do not make checks heavy: the orchestrator calls them every few seconds.

## Best practices

| Rule | Why |
|---|---|
| Common identifiers (`TraceId`) in logs, metrics, traces | end-to-end search |
| OpenTelemetry naming conventions | compatibility with backends |
| Limit tag cardinality | otherwise metrics are expensive and slow |
| Sample traces, not metrics | storage cost |
| Do not log secrets and personal data | security |
| Alert on symptoms (errors, latency), not causes | less noise |
| SLOs and an error budget | observability is tied to a goal |

## Related materials

- Back: [basics](basics.en.md). Next: [expert theory](expert.en.md) — internals and gotchas.
- [10 · Async/await](../10-async-await/overview.en.md) — `AsyncLocal` and the flow of context.
- [14 · ASP.NET Core: Minimal API](../14-aspnet-core-minimal-api/overview.en.md) — instrumentation and health checks.
- [21 · Application architecture](../21-architecture/overview.en.md) — end-to-end tracing between services.
- [Kubernetes](../../../kubernetes/overview.en.md) — probes and cluster metrics.

## Sources

- [OpenTelemetry .NET: getting started](https://opentelemetry.io/docs/languages/dotnet/getting-started/)
- [W3C Trace Context](https://www.w3.org/TR/trace-context/)
- [OpenTelemetry semantic conventions](https://opentelemetry.io/docs/specs/semconv/)
- [Distributed tracing in .NET](https://learn.microsoft.com/dotnet/core/diagnostics/distributed-tracing-concepts)
- [ASP.NET Core metrics](https://learn.microsoft.com/aspnet/core/log-mon/metrics/metrics)
