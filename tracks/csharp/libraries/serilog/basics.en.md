---
title: "Serilog: basics"
lang: en
track: csharp
kind: library
doc: basics
slug: serilog
part: 2
tags: [serilog, logging, structured-logging, sinks, message-templates]
versions: ".NET 11, C# 15; Serilog 4.4.0, Serilog.Sinks.Console 6.1.1, Serilog.Sinks.File 7.0.0"
status: draft
updated: 2026-11-22
---

# Serilog: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [13 · DI, configuration, logging](../../modules/13-di-config-logging/overview.en.md). **Time:** ~30 min.

## What it is and why

Serilog is a **structured** logging library. A message is a template with named properties (`"User {User} has {Count} items"`), not just a string: values are kept as data and can be searched and filtered in log systems (Seq, Elastic, Loki). Output is configured through **sinks** (console, file, network).

```bash
dotnet add package Serilog
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File
```

## The first configuration

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.WithProperty("App", "demo")
    .WriteTo.Console(outputTemplate: "[{Level:u3}] {Message:lj} {Properties:j}{NewLine}")
    .CreateLogger();

var user = "ann"; var count = 3;
Log.Information("User {User} has {Count} items", user, count);
Log.CloseAndFlush();   // flush buffered sinks on exit
```

Output (verified by running):

```text
[INF] User ann has 3 items {"App": "demo"}
```

`{Properties:j}` prints the extra properties (here the `App` added by the enricher) as JSON. The properties `User` and `Count` already entered the message text, so they are not in `{Properties}`.

## Levels

`Verbose < Debug < Information < Warning < Error < Fatal`. `MinimumLevel.Debug()` lets through everything from `Debug` up; the message `Log.Debug("debug visible")` in the example passed, while with `MinimumLevel.Information()` it would be filtered out.

## Message templates and properties

| Notation | What it does |
|---|---|
| `{User}` | the value as a scalar (a quoted string in JSON) |
| `{@Order}` | **destructuring**: the object becomes a structure with properties |
| `{$Order}` | force a string (`ToString()`) |

Verified on `record Order(int Id, string Customer, decimal Total)`: `{Order}` and `{$Order}` gave `Order { Id = 7, Customer = Ann, Total = 9,5 }` (a string), while `{@Order}` gave the structure `{"Id": 7, "Customer": "Ann", "Total": 9.5, "$type": "Order"}`. Note that in the string form the decimal is formatted by the current culture (`9,5`), while in the structure it is the number `9.5`.

Pass an exception as the first argument: `Log.Error(e, "failed for {User}", user)` — the text and the stack are kept separately from the template.

## A file

```csharp
.WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 3)
```

Verified: the file name gets the date (`app-20261122.log`), the line — `2026-11-22 17:19:18.016 +04:00 [INF] Order 7 placed by ann`.

## Beginner mistakes

- ❌ String interpolation: `Log.Information($"user {user}")` → ✅ a template with parameters. Verified: with interpolation the template is the ready string `interpolated ann` and there are no properties; you cannot search by `User`.
- ❌ Not calling `Log.CloseAndFlush()` → ✅ the last messages may not be written.
- ❌ Logging whole objects without `@` → ✅ you get `ToString()`, not a structure.
- ❌ Logging secrets and personal data → ✅ exclude/mask them.
- ❌ `Debug` in production "just in case" → ✅ the default level is `Information`.

## Related materials

- Next: [advanced](advanced.en.md) — enrichment, context, overrides, JSON, `ILogger` integration.
- [13 · DI, configuration, logging](../../modules/13-di-config-logging/overview.en.md) — `ILogger<T>`.
- [22 · Observability](../../modules/22-observability/overview.en.md) — logs, metrics, tracing.
- [Python](../../../python/overview.en.md) — `logging` and structlog.

## Sources

- [Serilog: Getting started](https://github.com/serilog/serilog/wiki/Getting-Started)
- [Serilog: Writing log events](https://github.com/serilog/serilog/wiki/Writing-Log-Events)
- [Serilog: Structured data](https://github.com/serilog/serilog/wiki/Structured-Data)
