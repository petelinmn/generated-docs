---
title: "Observability"
lang: en
track: csharp
kind: module
doc: overview
slug: 22-observability
part: 3
levels: [1, 2, 3, 4]
tags: [observability, logging, metrics, tracing, opentelemetry, health-checks, sampling]
versions: ".NET 11, C# 15; OpenTelemetry 1.21.0"
status: draft
updated: 2026-11-22
---

# Observability

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

Observability is the ability to understand what is happening inside a running system from the signals it emits itself: logs (what happened), metrics (how many and how often) and traces (where a request's time went across services). In .NET the signal sources are built into the platform (`ILogger`, `Meter`, `ActivitySource`), while OpenTelemetry is the standard that subscribes to them and sends data to any backend. Health checks give an orchestrator a simple "alive" and "ready" answer.

## Key ideas

- **Three signals:** logs, metrics (`Counter`, `Histogram`, `ObservableGauge`), traces (`Activity` = a span).
- **Source — listener:** without a listener `StartActivity` returns `null`, so instrumentation is added freely (`?.`).
- **Trace context:** `traceparent` (W3C) is passed through `HttpClient`/ASP.NET Core; in the verified example five spans ended up in one trace.
- **Tag cardinality:** `region` is fine, `user_id` is not; details belong in traces and logs.
- **OpenTelemetry:** `AddSource`/`AddMeter`, instrumentations, exporters (OTLP, in-memory for tests).
- **Health checks:** liveness without dependencies, readiness with them; `Degraded` is HTTP 200 by default.
- **Sampling and SLOs:** we do not keep everything; alert on symptoms, set goals through SLI/SLO.

## Minimal example

```csharp
var source = new ActivitySource("Demo.Shop");
var meter = new Meter("Demo.Shop");
var orders = meter.CreateCounter<long>("shop.orders");

using (var span = source.StartActivity("PlaceOrder"))     // null when nobody listens
{
    span?.SetTag("order.id", 42);
    orders.Add(1, new KeyValuePair<string, object?>("region", "eu"));
}
```

## When to use and when not

- ✅ Any service in production: logs with a `TraceId`, basic metrics, health checks.
- ✅ Several services: distributed tracing and consistent sampling.
- ✅ SLOs and alerts on user-visible symptoms.
- ❌ Metric tags with user identifiers.
- ❌ Secrets and personal data in logs, tags, baggage.
- ❌ A liveness check that depends on a database or external services.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | the three signals, structured logs, `Meter` and instruments, `ActivitySource`, health checks |
| [2. Advanced](advanced.en.md) | the OpenTelemetry SDK, W3C context propagation, tags and statuses, sampling, metric practices, correlation, probes |
| [3. Expert](expert.en.md) | the source—listener model, `Tags` and `TagObjects`, cardinality, head/tail sampling, SLI/SLO, diagnostics, gotchas, 13 interview questions |
| [4. Self-check](quiz.en.md) | 11 questions and [5 exercises](exercises.en.md) with solutions |
| 5. Runnable examples | not generated yet — `/learn csharp 22-observability level 5` |
| 6. Tests | not generated yet — `/learn csharp 22-observability level 6` |

## Related materials

- Prerequisites: [13 · DI, configuration, logging](../13-di-config-logging/overview.en.md), [14 · ASP.NET Core: Minimal API](../14-aspnet-core-minimal-api/overview.en.md)
- Nearby: [19 · Performance](../19-performance/overview.en.md), [21 · Application architecture](../21-architecture/overview.en.md)
- Deployment: [Kubernetes](../../../kubernetes/overview.en.md) — probes and metrics
- The same idea in other tracks: [Python](../../../python/overview.en.md) — logging and monitoring
