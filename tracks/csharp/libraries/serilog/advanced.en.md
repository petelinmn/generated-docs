---
title: "Serilog: advanced"
lang: en
track: csharp
kind: library
doc: advanced
slug: serilog
part: 2
tags: [serilog, enrichers, log-context, minimum-level-override, compact-json, ilogger]
versions: ".NET 11, C# 15; Serilog 4.4.0, Serilog.Extensions.Logging 11.0.0, Serilog.Formatting.Compact 3.0.0"
status: draft
updated: 2026-11-22
---

# Serilog: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [basics](basics.en.md). **Time:** ~45 min.

## Enrichment and context

Enrichers add properties to all events. `LogContext` — properties "for the duration of a scope":

```csharp
Log.Logger = new LoggerConfiguration()
    .Enrich.WithProperty("App", "demo")
    .Enrich.FromLogContext()          // required for LogContext properties
    .WriteTo.Console(outputTemplate: "[{Level:u3}] {Message:lj} {Properties:j}{NewLine}")
    .CreateLogger();

using (LogContext.PushProperty("RequestId", "r-1"))
    Log.Warning("inside context");
Log.Warning("outside context");
```

Verified: inside the scope `{"App": "demo", "RequestId": "r-1"}`, outside — only `{"App": "demo"}`. Without `.Enrich.FromLogContext()` a property from `LogContext` does not appear.

`Log.ForContext("SourceContext", "Noisy.Thing")` or `Log.ForContext<T>()` creates a logger with a "source".

## Overriding levels per source

```csharp
.MinimumLevel.Debug()
.MinimumLevel.Override("Noisy", LogEventLevel.Warning)
```

Verified on four messages: from `Noisy.Thing` only `Warning` passed (info dropped), from `Noisy` — `Warning`, from `Other` and `Foo` — `Information` (passed because the minimum is `Debug`). The override works by the `SourceContext` prefix. This is how noisy namespaces are muted (in ASP.NET Core — `Microsoft.AspNetCore`).

## Structured JSON

```csharp
.WriteTo.File(new CompactJsonFormatter(), "log.json")      // Serilog.Formatting.Compact
```

Verified: one line per event, `{"@t":"…","@mt":"User {User} has {Count} items","User":"ann","Count":3,"App":"demo"}` — `@t` time, `@mt` template, `@l` level (omitted for `Information`). Numbers are kept as numbers (`"Count":3`), so numeric queries work on them.

## Integration with `Microsoft.Extensions.Logging`

```csharp
using var lf = LoggerFactory.Create(b => b.AddSerilog(Log.Logger, dispose: false));   // Serilog.Extensions.Logging
var logger = lf.CreateLogger("MyApp.Service");
using (logger.BeginScope("scope-{Id}", 42))
    logger.LogInformation("Order {OrderId} placed by {User}", 7, "ann");
```

Verified: the message reached Serilog with the properties `OrderId=7`, `User="ann"`, `SourceContext="MyApp.Service"` and the scope's properties (`Id=42`, `Scope=["scope-42"]`). `ILogger<T>` in application code stays standard, and Serilog provides the output. In ASP.NET Core the `Serilog.AspNetCore` package is usually used (`builder.Host.UseSerilog(...)`).

> ⚠️ Verify: `Serilog.AspNetCore` (`UseSerilog`, `UseSerilogRequestLogging`) was not run in this material.

## Configuration from a file

`Serilog.Settings.Configuration` reads settings (`MinimumLevel`, `WriteTo`) from `appsettings.json`: levels can be changed without a rebuild.

> ⚠️ Verify: the format of the `Serilog` section in `appsettings.json` and configuration reloading were not run in this material.

## Best practices

| Rule | Why |
|---|---|
| Templates with parameters, not interpolation | structure |
| `Override` for noisy sources | less junk |
| Consistent property names (`UserId`, `OrderId`) | search by field |
| Common properties through enrichers/`LogContext` | do not repeat in every message |
| JSON for machine processing | queries in log systems |
| `CloseAndFlush` on exit | do not lose the tail |
| Do not log secrets | security |

## Related materials

- Back: [basics](basics.en.md). Next: [expert](expert.en.md).
- [13 · DI, configuration, logging](../../modules/13-di-config-logging/overview.en.md) — `ILogger`.
- [22 · Observability](../../modules/22-observability/overview.en.md) — correlating logs and traces.
- [Background jobs](../../recipes/background-jobs/overview.en.md) — logging in workers.

## Sources

- [Serilog: Enrichment](https://github.com/serilog/serilog/wiki/Enrichment)
- [Serilog: Configuration basics (MinimumLevel.Override)](https://github.com/serilog/serilog/wiki/Configuration-Basics)
- [serilog-extensions-logging](https://github.com/serilog/serilog-extensions-logging)
- [serilog-formatting-compact](https://github.com/serilog/serilog-formatting-compact)
