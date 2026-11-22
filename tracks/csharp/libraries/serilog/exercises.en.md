---
title: "Exercises: Serilog"
lang: en
track: csharp
kind: library
doc: exercises
slug: serilog
part: 2
tags: [serilog, enrichers, log-context, sinks, minimum-level-override]
status: draft
updated: 2026-11-22
---

# Exercises: Serilog

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself. The solutions were verified by running (Serilog 4.4.0, Serilog.Sinks.Console 6.1.1, Serilog.Sinks.File 7.0.0, Serilog.Formatting.Compact 3.0.0, Serilog.Extensions.Logging 11.0.0). There are no runnable versions in `code/` (level 5 does not exist).

## 1. 🟢 A structured message

**Task:** print `User ann has 3 items` to the console with an added property `App = demo`.

**Expected result:** `[INF] User ann has 3 items {"App": "demo"}`.

<details><summary>Hint</summary>

`Enrich.WithProperty`, `WriteTo.Console(outputTemplate: "[{Level:u3}] {Message:lj} {Properties:j}{NewLine}")`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 1: structured message with an enricher property.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.WithProperty("App", "demo")
    .WriteTo.Console(outputTemplate: "[{Level:u3}] {Message:lj} {Properties:j}{NewLine}")
    .CreateLogger();
Log.Information("User {User} has {Count} items", "ann", 3);
Log.CloseAndFlush();
```

Verified: `[INF] User ann has 3 items {"App": "demo"}`.

</details>

## 2. 🟡 Destructuring

**Task:** log `record Order(int Id, string Customer, decimal Total)` three ways — `{Order}`, `{@Order}`, `{$Order}`. How do they differ?

**Expected result:** the first and third — the string `Order { … }`, the second — a JSON structure.

<details><summary>Hint</summary>

Compare the output and the value type in `LogEvent.Properties`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 2: scalar vs destructured vs stringified.
var order = new Order(7, "Ann", 9.5m);
Log.Information("Plain: {Order}", order);
Log.Information("Destructured: {@Order}", order);
Log.Information("Stringified: {$Order}", order);
```

Verified: `Plain: Order { Id = 7, Customer = Ann, Total = 9,5 }`, `Destructured: {"Id": 7, "Customer": "Ann", "Total": 9.5, "$type": "Order"}`, `Stringified: Order { … }`.

</details>

## 3. 🟡 Context and a level override

**Task:** (a) add `RequestId` only to messages inside a scope; (b) mute the source `Noisy` to `Warning`.

**Expected result:** inside the scope `RequestId` is present, outside it is not; from `Noisy.Thing` only `Warning` passes.

<details><summary>Hint</summary>

`Enrich.FromLogContext()` + `LogContext.PushProperty`; `MinimumLevel.Override("Noisy", LogEventLevel.Warning)` and `Log.ForContext("SourceContext", "Noisy.Thing")`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 3: scoped property and per-source level override.
.MinimumLevel.Override("Noisy", LogEventLevel.Warning)
.Enrich.FromLogContext()

using (LogContext.PushProperty("RequestId", "r-1")) Log.Warning("inside context");
Log.Warning("outside context");

var noisy = Log.ForContext("SourceContext", "Noisy.Thing");
noisy.Information("dropped");
noisy.Warning("kept");
```

Verified: `inside context {"App": "demo", "RequestId": "r-1"}`, `outside context {"App": "demo"}`; on four test messages from `Noisy`, `Noisy.Thing`, `Other`, `Foo` only the info from `Noisy.Thing` was dropped.

</details>

## 4. 🔴 A JSON file and `ILogger` integration

**Task:** write events to `log.json` through `CompactJsonFormatter` and route `ILogger` messages with a scope `scope-{Id}` into Serilog. Which properties does the event get?

**Expected result:** `OrderId=7`, `User="ann"`, `SourceContext="MyApp.Service"`, `Id=42`, `Scope=["scope-42"]`.

<details><summary>Hint</summary>

`.WriteTo.File(new CompactJsonFormatter(), "log.json")`; `LoggerFactory.Create(b => b.AddSerilog(Log.Logger, dispose: false))`; `logger.BeginScope("scope-{Id}", 42)`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 4: JSON output and ILogger bridge.
Log.Logger = new LoggerConfiguration().MinimumLevel.Information()
    .WriteTo.File(new CompactJsonFormatter(), "log.json")
    .CreateLogger();
using var lf = LoggerFactory.Create(b => b.AddSerilog(Log.Logger, dispose: false));
var logger = lf.CreateLogger("MyApp.Service");
using (logger.BeginScope("scope-{Id}", 42))
    logger.LogInformation("Order {OrderId} placed by {User}", 7, "ann");
Log.CloseAndFlush();
```

Verified: the event's properties `OrderId=7,User="ann",SourceContext="MyApp.Service",Id=42,Scope=["scope-42"]`; a JSON line: `{"@t":"…","@mt":"User {User} has {Count} items","User":"ann","Count":3,"App":"demo"}`.

</details>
