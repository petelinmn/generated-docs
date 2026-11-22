---
title: "Exercises: Observability"
lang: en
track: csharp
kind: module
doc: exercises
slug: 22-observability
part: 3
tags: [observability, metrics, tracing, health-checks, opentelemetry, trace-context]
status: draft
updated: 2026-11-22
---

# Exercises: Observability

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. The solutions were verified by running (a project on the `Microsoft.NET.Sdk.Web` SDK with the packages `OpenTelemetry`, `OpenTelemetry.Exporter.InMemory`, `OpenTelemetry.Extensions.Hosting`, `OpenTelemetry.Instrumentation.AspNetCore`, `OpenTelemetry.Instrumentation.Http` 1.21.0). There are no runnable versions in `code/` yet (level 5 does not exist).

## 1. 🟢 A counter with tags

**Task:** create a `Meter` `Demo.Shop`, a counter `shop.orders` and record two measurements: `1` with the tag `region=eu` and `2` with `region=us`. Read them with a `MeterListener`.

**Expected result:**

```text
shop.orders=1 [region:eu] | shop.orders=2 [region:us]
```

<details><summary>Hint</summary>

`listener.InstrumentPublished = (inst, l) => l.EnableMeasurementEvents(inst);` and `SetMeasurementEventCallback<long>(...)`; do not forget `listener.Start()`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 1: record tagged counter measurements and read them with a MeterListener.
var meter = new Meter("Demo.Shop", "1.0.0");
var orders = meter.CreateCounter<long>("shop.orders", unit: "{order}", description: "Placed orders");
var recorded = new List<string>();
using var listener = new MeterListener();
listener.InstrumentPublished = (inst, l) => { if (inst.Meter.Name == "Demo.Shop") l.EnableMeasurementEvents(inst); };
listener.SetMeasurementEventCallback<long>((inst, v, tags, _) =>
    recorded.Add($"{inst.Name}={v} [{string.Join(",", tags.ToArray().Select(t => $"{t.Key}:{t.Value}"))}]"));
listener.Start();

orders.Add(1, new KeyValuePair<string, object?>("region", "eu"));
orders.Add(2, new KeyValuePair<string, object?>("region", "us"));
Console.WriteLine(string.Join(" | ", recorded));
```

Verified: the output matches.

</details>

## 2. 🟢 Liveness and readiness

**Task:** register two checks — `self` (always healthy, tag `live`) and `db` (depends on a flag, tag `ready`). Publish `/health/live` and `/health/ready`. Show the responses with a live and a "down" database.

**Expected result:**

```text
database up:    /health/live 200 Healthy, /health/ready 200 Healthy
database down:  /health/live 200 Healthy, /health/ready 503 Unhealthy
```

<details><summary>Hint</summary>

`AddHealthChecks().AddCheck(..., tags: [...])` and `MapHealthChecks(path, new() { Predicate = r => r.Tags.Contains("live") })`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 2: liveness never depends on external systems, readiness does.
bool dbUp = true;
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("ok"), tags: ["live"])
    .AddCheck("db", () => dbUp ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy("db down"), tags: ["ready"]);

app.MapHealthChecks("/health/live", new() { Predicate = r => r.Tags.Contains("live") });
app.MapHealthChecks("/health/ready", new() { Predicate = r => r.Tags.Contains("ready") });
```

Verified: `200`/`200` with the database up and `200`/`503` with it down. For an orchestrator ([Kubernetes](../../../kubernetes/overview.en.md)) these are the addresses of the livenessProbe and readinessProbe.

</details>

## 3. 🟡 Nested spans

**Task:** create an `ActivitySource` `Demo.Shop` and the spans: `PlaceOrder` (kind `Server`, tag `order.id`), inside it `SaveToDb` (tag `db.system`, event `saved`) and `Notify` with the status `Error`. Print the parents and statuses.

**Expected result:**

```text
SaveToDb parent=PlaceOrder status=Unset
Notify   parent=PlaceOrder status=Error
PlaceOrder parent=- kind=Server
```

<details><summary>Hint</summary>

You need an `ActivityListener` with `ShouldListenTo` and `Sample = AllDataAndRecorded`, added through `ActivitySource.AddActivityListener`; without it `StartActivity` returns `null`. Collect the spans in `ActivityStopped`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 3: parent/child spans, tags, events and an error status.
var source = new ActivitySource("Demo.Shop");
var spans = new List<Activity>();
using var al = new ActivityListener
{
    ShouldListenTo = s => s.Name == "Demo.Shop",
    Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
    ActivityStopped = a => spans.Add(a)
};
ActivitySource.AddActivityListener(al);

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
foreach (var s in spans) Console.WriteLine($"{s.OperationName} parent={(s.Parent?.OperationName ?? "-")} status={s.Status} kind={s.Kind}");
```

Verified: `SaveToDb` and `Notify` are children of `PlaceOrder`, the statuses are `Unset` and `Error`, `PlaceOrder` has the kind `Server`; spans land in the list in completion order (children before the parent).

</details>

## 4. 🟡 A trace over HTTP

**Task:** in an application `/outer` must call `/inner` through `HttpClient`, and `/inner` must return the received `traceparent` header. Configure OpenTelemetry with the ASP.NET Core and `HttpClient` instrumentations and your own source, and make sure all spans are in one trace.

**Expected result:**

```text
inner received a traceparent header
single trace: True   (spans: Client GET, Server GET /outer, Internal business-work, Client GET, Server GET /inner)
```

<details><summary>Hint</summary>

`AddOpenTelemetry().WithTracing(t => t.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().AddSource("Demo.Shop").AddInMemoryExporter(spans))`; `IHttpClientFactory` for the call.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 4: context propagation through HttpClient — one trace across both requests.
var spans = new List<Activity>();
builder.Services.AddHttpClient();
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("shop-api"))
    .WithTracing(t => t.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().AddSource("Demo.Shop").AddInMemoryExporter(spans));

var src = new ActivitySource("Demo.Shop");
app.MapGet("/inner", (HttpContext c) => $"traceparent={c.Request.Headers["traceparent"]}");
app.MapGet("/outer", async (IHttpClientFactory f, HttpContext c) =>
{
    using var work = src.StartActivity("business-work");
    return await f.CreateClient().GetStringAsync($"{c.Request.Scheme}://{c.Request.Host}/inner");
});

// after calling /outer:
Console.WriteLine(spans.Select(s => s.TraceId).Distinct().Count() == 1);   // True
```

Verified: `/inner` saw the `traceparent`, all five spans are in one trace.

</details>

## 5. 🔴 In-memory OpenTelemetry: a span and a metric

**Task:** set up OpenTelemetry tracing and metrics providers with in-memory exporters, create a span `Checkout` with the tag `user=ann` and increment the counter `shop.orders`. Make sure the exporters received the data.

**Expected result:**

```text
spans=1 name=Checkout tag=ann
metrics contain shop.orders
```

<details><summary>Hint</summary>

`Sdk.CreateTracerProviderBuilder().AddSource("Demo.Shop").AddInMemoryExporter(list).Build()` and `Sdk.CreateMeterProviderBuilder().AddMeter("Demo.Shop").AddInMemoryExporter(list).Build()`; metrics are flushed with `ForceFlush()`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 5: wire the OpenTelemetry SDK to in-memory exporters (handy in tests).
var exportedSpans = new List<Activity>();
var exportedMetrics = new List<Metric>();
using (var tracer = Sdk.CreateTracerProviderBuilder().AddSource("Demo.Shop").AddInMemoryExporter(exportedSpans).Build())
using (var meters = Sdk.CreateMeterProviderBuilder().AddMeter("Demo.Shop").AddInMemoryExporter(exportedMetrics).Build())
{
    using (var a = source.StartActivity("Checkout")) { a?.SetTag("user", "ann"); }
    orders.Add(5, new KeyValuePair<string, object?>("region", "eu"));
    meters!.ForceFlush();
}
Console.WriteLine($"spans={exportedSpans.Count} name={exportedSpans.First().OperationName} tag={exportedSpans.First().GetTagItem("user")}");
Console.WriteLine(string.Join(",", exportedMetrics.Select(m => m.Name)));   // contains shop.orders
```

Verified: `spans=1 name=Checkout tag=ann`, the metrics include `shop.orders` (and `shop.queue.length` if a gauge is registered). In production an OTLP exporter is attached instead of the in-memory one.

</details>
