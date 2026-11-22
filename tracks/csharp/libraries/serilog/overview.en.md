---
title: "Serilog"
lang: en
track: csharp
kind: library
doc: overview
slug: serilog
part: 2
levels: [1, 2, 3, 4]
tags: [serilog, logging, structured-logging, sinks, enrichers]
versions: ".NET 11, C# 15; Serilog 4.4.0"
status: draft
updated: 2026-11-22
---

# Serilog

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

Serilog is a structured logging library. A message is a template with named properties; the values are kept as data and are searchable. Output is configured with sinks (console, file, JSON, network), context is added by enrichers and `LogContext`, and noisy sources are muted with `MinimumLevel.Override`. Through `Serilog.Extensions.Logging` the library accepts messages from the standard `ILogger`.

## Key ideas

- A template with parameters instead of interpolation (verified: interpolation loses properties).
- `{@obj}` — destructuring, `{$obj}` — a string.
- Levels `Verbose…Fatal`, `MinimumLevel` and `Override` by source prefix.
- `Enrich.WithProperty`, `Enrich.FromLogContext` + `LogContext.PushProperty`.
- Sinks: console, a rolling file, JSON (`CompactJsonFormatter`), custom ones (`ILogEventSink`).
- `AddSerilog` — a bridge from `ILogger`.
- `Log.CloseAndFlush()` on exit.

## Minimal example

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
Log.Information("User {User} has {Count} items", "ann", 3);
Log.CloseAndFlush();
```

## When to use and when not

- ✅ Applications whose logs are analyzed in Seq/Elastic/Loki.
- ✅ You need rich sinks, enrichment, destructuring.
- ✅ You want one log format across several services.
- ❌ The built-in `ILogger` with a console is enough.
- ❌ Hot paths without a level guard/`LoggerMessage`.
- ❌ Logging secrets and personal data.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | configuration, templates, `@`/`$`, levels, a file |
| [2. Advanced](advanced.en.md) | enrichment, `LogContext`, `Override`, JSON, the `ILogger` bridge |
| [3. Expert](expert.en.md) | internals, a custom sink, gotchas, 11 interview questions |
| [4. Self-check](quiz.en.md) | 10 questions and [4 exercises](exercises.en.md) with solutions |
| 5. Runnable examples | not generated yet — `/learn csharp serilog level 5` |
| 6. Tests | not generated yet — `/learn csharp serilog level 6` |

## Related materials

- Prerequisites: [13 · DI, configuration, logging](../../modules/13-di-config-logging/overview.en.md)
- Nearby: [22 · Observability](../../modules/22-observability/overview.en.md), [Background jobs](../../recipes/background-jobs/overview.en.md)
- In other tracks: [Python](../../../python/overview.en.md) — `logging`, structlog
- Recipes and libraries: [Spectre.Console](../spectre-console/overview.en.md)
