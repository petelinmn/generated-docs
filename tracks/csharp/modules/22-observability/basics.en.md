---
title: "Observability: basics"
lang: en
track: csharp
kind: module
doc: basics
slug: 22-observability
part: 3
tags: [observability, logging, metrics, tracing, opentelemetry, health-checks]
versions: ".NET 11, C# 15; OpenTelemetry 1.21.0"
status: draft
updated: 2026-11-22
---

# Observability: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [13 · DI, configuration, logging](../13-di-config-logging/overview.en.md), [14 · ASP.NET Core: Minimal API](../14-aspnet-core-minimal-api/overview.en.md). **Time:** ~1 h.

## Why it matters

An application in production cannot simply be "run in a debugger". When something goes wrong you need to understand **what is happening inside** from the signals the application emits itself. The ability to answer questions about a system's behavior from its observable outputs is called **observability**. It rests on three kinds of signals:

| Signal | Answers the question | Example |
|---|---|---|
| **Logs** | what exactly happened | "order 42 was not placed: out of stock" |
| **Metrics** | how many and how often, what is the trend | orders per minute, the 95th percentile of response time |
| **Traces** | where one request's time went across services | request → API → database → external service |

They are complemented by **health checks** — a simple "alive/ready" answer for an orchestrator. In .NET the de facto standard is **OpenTelemetry**: one set of APIs and a protocol that different monitoring systems work with.

## Logs

Structured logs through `ILogger` ([13 · DI, configuration, logging](../13-di-config-logging/overview.en.md)): a template with named placeholders, levels, scopes. For observability it matters that an entry has **identifiers for searching** (an order, a user) and the **trace context** — then log entries can be tied to a specific request. Verified: inside an activity (`Activity`) the current `Activity.Current` is available, and the `TraceId` is 32 characters:

```csharp
using (var act = source.StartActivity("Work"))
{
    log.LogInformation("inside activity");
    // Activity.Current == act; act.TraceId is 32 hex characters
}
```

## Metrics

A metric is a numeric series over time. In .NET they are created through `System.Diagnostics.Metrics`:

```csharp
var meter = new Meter("Demo.Shop", "1.0.0");
var orders = meter.CreateCounter<long>("shop.orders", unit: "{order}", description: "Placed orders");
var latency = meter.CreateHistogram<double>("shop.order.duration", unit: "ms");
meter.CreateObservableGauge("shop.queue.length", () => queue);

orders.Add(1, new KeyValuePair<string, object?>("region", "eu"));
latency.Record(12.5);
```

Instrument kinds:

| Instrument | Meaning | Example |
|---|---|---|
| `Counter<T>` | a monotonically growing event counter | the number of orders |
| `UpDownCounter<T>` | a value that rises and falls | active connections |
| `Histogram<T>` | a distribution of values | processing time |
| `ObservableGauge<T>` | the current value, read on demand | queue length |

A measurement is accompanied by **tags** (`region=eu`) — a metric is sliced by them; tags with a large number of values (a user id) are avoided, otherwise the number of series explodes. Measurements are collected by a **listener** (`MeterListener` or an OpenTelemetry exporter). Verified through `MeterListener`: received `shop.orders=1 [region:eu]`, `shop.orders=2 [region:us]`, a histogram `12,5` (in the current culture) and `shop.queue.length=3`.

## Tracing

A trace describes the path of one request through the system. The unit is a **span**: an operation with a start and an end, a name, tags and a parent. In .NET a span is an `Activity`, and the source is an `ActivitySource`:

```csharp
var source = new ActivitySource("Demo.Shop");

using (var parent = source.StartActivity("PlaceOrder", ActivityKind.Server))
{
    parent?.SetTag("order.id", 42);
    using (var child = source.StartActivity("SaveToDb"))
    {
        child?.SetTag("db.system", "sqlite");
        child?.AddEvent(new ActivityEvent("saved"));
    }
    using (var failing = source.StartActivity("Notify"))
    {
        failing?.SetStatus(ActivityStatusCode.Error, "smtp down");
    }
}
```

Verified: `SaveToDb` and `Notify` have the parent `PlaceOrder`, all spans are in one trace, `Notify` has the status `Error`, and `PlaceOrder` has the kind `Server`. If nobody is subscribed to the source, `StartActivity` returns `null` — hence `?.` everywhere (verified). Nesting is built automatically through `Activity.Current`. A trace is a tree of spans with a shared `TraceId` (32 hex characters); each span has its own `SpanId` (16).

## Health checks

An orchestrator (Kubernetes, a load balancer) asks the application whether it is alive and ready to accept traffic:

```csharp
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("ok"), tags: ["live"])
    .AddCheck("db", () => dbUp ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy("db down"), tags: ["ready"]);

app.MapHealthChecks("/health/live", new() { Predicate = r => r.Tags.Contains("live") });
app.MapHealthChecks("/health/ready", new() { Predicate = r => r.Tags.Contains("ready") });
```

Verified: while the database is "alive", all three addresses answer `200 Healthy`; when `dbUp = false`, `/health/ready` and `/health` give `503 Unhealthy` while `/health/live` stays `200`. **Liveness** ("the process is alive") must not depend on external systems, otherwise the orchestrator will restart the application because of someone else's failure; **readiness** ("ready to accept requests") checks dependencies.

## Beginner mistakes

- ❌ Only logs "just in case" → ✅ three signals: logs, metrics, traces.
- ❌ Logs without identifiers and trace context → ✅ structured fields and the `TraceId`.
- ❌ Metrics with huge-cardinality tags (a user id) → ✅ a bounded set of values.
- ❌ A liveness check that goes to the database → ✅ liveness is simple, readiness has dependencies.
- ❌ Not using `?.` on the result of `StartActivity` → ✅ it can be `null`.
- ❌ No alerts at all → ✅ metrics and logs exist so that you are notified before users complain.

## Related materials

- Next: [advanced theory](advanced.en.md) — OpenTelemetry, context propagation, correlation.
- [13 · DI, configuration, logging](../13-di-config-logging/overview.en.md) — `ILogger`.
- [14 · ASP.NET Core: Minimal API](../14-aspnet-core-minimal-api/overview.en.md) — health check endpoints.
- [19 · Performance](../19-performance/overview.en.md) — metrics and profiling.
- [Kubernetes](../../../kubernetes/overview.en.md) — liveness/readiness probes.

## Sources

- [Observability in .NET](https://learn.microsoft.com/dotnet/core/diagnostics/observability-with-otel)
- [Metrics: `System.Diagnostics.Metrics`](https://learn.microsoft.com/dotnet/core/diagnostics/metrics)
- [Tracing: `ActivitySource` and `Activity`](https://learn.microsoft.com/dotnet/core/diagnostics/distributed-tracing)
- [Health checks in ASP.NET Core](https://learn.microsoft.com/aspnet/core/host-and-deploy/health-checks)
- [OpenTelemetry](https://opentelemetry.io/docs/languages/dotnet/)
