---
title: "Упражнения: Наблюдаемость"
lang: ru
track: csharp
kind: module
doc: exercises
slug: 22-observability
part: 3
tags: [observability, metrics, tracing, health-checks, opentelemetry, trace-context]
status: draft
updated: 2026-11-22
---

# Упражнения: Наблюдаемость

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Решения проверены запуском (проект на SDK `Microsoft.NET.Sdk.Web` с пакетами `OpenTelemetry`, `OpenTelemetry.Exporter.InMemory`, `OpenTelemetry.Extensions.Hosting`, `OpenTelemetry.Instrumentation.AspNetCore`, `OpenTelemetry.Instrumentation.Http` 1.21.0). Запускаемых версий в `code/` пока нет (уровень 5 не создан).

## 1. 🟢 Счётчик с тегами

**Задача:** создай `Meter` `Demo.Shop`, счётчик `shop.orders` и запиши два измерения: `1` с тегом `region=eu` и `2` с `region=us`. Прочитай их `MeterListener`-ом.

**Ожидаемый результат:**

```text
shop.orders=1 [region:eu] | shop.orders=2 [region:us]
```

<details><summary>Подсказка</summary>

`listener.InstrumentPublished = (inst, l) => l.EnableMeasurementEvents(inst);` и `SetMeasurementEventCallback<long>(...)`; не забудь `listener.Start()`.

</details>

<details><summary>Решение</summary>

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

Проверено: вывод совпадает.

</details>

## 2. 🟢 Liveness и readiness

**Задача:** зарегистрируй две проверки — `self` (всегда здорова, тег `live`) и `db` (зависит от флага, тег `ready`). Опубликуй `/health/live` и `/health/ready`. Покажи ответы при живой и «упавшей» БД.

**Ожидаемый результат:**

```text
БД жива:   /health/live 200 Healthy, /health/ready 200 Healthy
БД упала:  /health/live 200 Healthy, /health/ready 503 Unhealthy
```

<details><summary>Подсказка</summary>

`AddHealthChecks().AddCheck(..., tags: [...])` и `MapHealthChecks(path, new() { Predicate = r => r.Tags.Contains("live") })`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 2: liveness never depends on external systems, readiness does.
bool dbUp = true;
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("ok"), tags: ["live"])
    .AddCheck("db", () => dbUp ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy("db down"), tags: ["ready"]);

app.MapHealthChecks("/health/live", new() { Predicate = r => r.Tags.Contains("live") });
app.MapHealthChecks("/health/ready", new() { Predicate = r => r.Tags.Contains("ready") });
```

Проверено: `200`/`200` при живой БД и `200`/`503` при упавшей. Для оркестратора ([Kubernetes](../../../kubernetes/overview.ru.md)) это адреса livenessProbe и readinessProbe.

</details>

## 3. 🟡 Вложенные спаны

**Задача:** создай `ActivitySource` `Demo.Shop` и спаны: `PlaceOrder` (вид `Server`, тег `order.id`), внутри него `SaveToDb` (тег `db.system`, событие `saved`) и `Notify` со статусом `Error`. Выведи родителей и статусы.

**Ожидаемый результат:**

```text
SaveToDb parent=PlaceOrder status=Unset
Notify   parent=PlaceOrder status=Error
PlaceOrder parent=- kind=Server
```

<details><summary>Подсказка</summary>

Нужен `ActivityListener` с `ShouldListenTo` и `Sample = AllDataAndRecorded`, добавленный через `ActivitySource.AddActivityListener`; без него `StartActivity` вернёт `null`. Спаны собирай в `ActivityStopped`.

</details>

<details><summary>Решение</summary>

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

Проверено: `SaveToDb` и `Notify` — дети `PlaceOrder`, статусы `Unset` и `Error`, у `PlaceOrder` вид `Server`; спаны попадают в список в порядке завершения (дети раньше родителя).

</details>

## 4. 🟡 Трасса через HTTP

**Задача:** в приложении `/outer` должен вызывать `/inner` через `HttpClient`, а `/inner` — вернуть полученный заголовок `traceparent`. Настрой OpenTelemetry с инструментациями ASP.NET Core и `HttpClient` и собственным источником, убедись, что все спаны — в одной трассе.

**Ожидаемый результат:**

```text
inner received a traceparent header
single trace: True   (спаны: Client GET, Server GET /outer, Internal business-work, Client GET, Server GET /inner)
```

<details><summary>Подсказка</summary>

`AddOpenTelemetry().WithTracing(t => t.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().AddSource("Demo.Shop").AddInMemoryExporter(spans))`; `IHttpClientFactory` для вызова.

</details>

<details><summary>Решение</summary>

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

Проверено: `/inner` увидел `traceparent`, все пять спанов — в одной трассе.

</details>

## 5. 🔴 OpenTelemetry в памяти: спан и метрика

**Задача:** настрой провайдеры трассировки и метрик OpenTelemetry с in-memory-экспортёрами, создай спан `Checkout` с тегом `user=ann` и увеличь счётчик `shop.orders`. Убедись, что экспортёры получили данные.

**Ожидаемый результат:**

```text
spans=1 name=Checkout tag=ann
metrics содержат shop.orders
```

<details><summary>Подсказка</summary>

`Sdk.CreateTracerProviderBuilder().AddSource("Demo.Shop").AddInMemoryExporter(list).Build()` и `Sdk.CreateMeterProviderBuilder().AddMeter("Demo.Shop").AddInMemoryExporter(list).Build()`; метрики сбрасывают `ForceFlush()`.

</details>

<details><summary>Решение</summary>

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

Проверено: `spans=1 name=Checkout tag=ann`, среди метрик есть `shop.orders` (и `shop.queue.length`, если зарегистрирован gauge). В продакшене вместо in-memory подключают OTLP-экспортёр.

</details>
