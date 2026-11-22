---
title: "DI, configuration, logging"
lang: en
track: csharp
kind: module
doc: overview
slug: 13-di-config-logging
part: 2
levels: [1, 2, 3, 4]
tags: [dependency-injection, service-lifetime, configuration, options-pattern, logging, generic-host]
versions: ".NET 11, C# 15; Microsoft.Extensions.* 11.0.0"
status: draft
updated: 2026-11-22
---

# DI, configuration, logging

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

The Generic Host brings together three mechanisms on which modern .NET applications stand: dependency injection (DI), configuration and logging. Classes receive dependencies through the constructor, settings come from layers of sources (files, environment, the command line) and are bound to types, and `ILogger` writes structured messages to pluggable destinations. The same mechanisms are used by ASP.NET Core, background services and console applications.

## Key ideas

- **DI:** a class depends on an interface, the container creates and supplies the dependencies; registration is separate from resolving.
- **Lifetimes:** `Singleton` — one, `Scoped` — one per scope (request), `Transient` — new every time.
- **The "captive dependency":** a singleton with a scoped dependency is a common bug; caught by `ValidateScopes`/`ValidateOnBuild` (on by default only in `Development`).
- **Configuration:** layers of sources, the last wins; nesting with `:` (in the environment — `__`); no secrets in the repository.
- **Options:** `IOptions<T>` (no reload), `IOptionsSnapshot<T>` (per scope), `IOptionsMonitor<T>` (immediate); `ValidateOnStart` catches errors at startup.
- **Logging:** templates with named placeholders, levels, scopes, `[LoggerMessage]`; string interpolation is an anti-pattern.
- **The host:** `Host.CreateApplicationBuilder`, `BackgroundService` with a clean stop through a token.

## Minimal example

```csharp
interface IClock { DateTime Now { get; } }
class FakeClock(DateTime now) : IClock { public DateTime Now { get; } = now; }
class GreetingService(IClock clock)
{
    public string Greet(string n) => (clock.Now.Hour < 12 ? "Good morning, " : "Hello, ") + n;
}

var sc = new ServiceCollection();
sc.AddSingleton<IClock>(new FakeClock(new DateTime(2026, 1, 1, 9, 0, 0)));
sc.AddTransient<GreetingService>();
using var p = sc.BuildServiceProvider();
Console.WriteLine(p.GetRequiredService<GreetingService>().Greet("Ann"));   // Good morning, Ann
```

## When to use and when not

- ✅ Any application with several interacting classes and tests.
- ✅ Settings that depend on the environment (dev/prod) — configuration layers and Options.
- ✅ Structured logging and diagnostics.
- ❌ A Service Locator — calling `GetService` from business code.
- ❌ A singleton holding a scoped service (`DbContext`).
- ❌ Secrets in `appsettings.json` in the repository.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | DI and lifetimes, configuration and layers, binding and `IOptions`, `ILogger` and templates, the generic host |
| [2. Advanced](advanced.en.md) | several implementations, keyed services, decorators, the captive dependency and `IServiceScopeFactory`, Options and validation, scopes and `LoggerMessage`, `BackgroundService` |
| [3. Expert](expert.en.md) | container internals, constructor selection, disposal and Transient leaks, Options and configuration internals, the logging pipeline, 14 interview questions |
| [4. Self-check](quiz.en.md) | 11 questions and [5 exercises](exercises.en.md) with solutions |
| 5. Runnable examples | not generated yet — `/learn csharp 13-di-config-logging level 5` |
| 6. Tests | not generated yet — `/learn csharp 13-di-config-logging level 6` |

## Related materials

- Prerequisites: [05 · Interfaces, structs, records](../05-interfaces-structs-records/overview.en.md), [12 · Memory and GC](../12-memory-and-gc/overview.en.md)
- Next: [14 · ASP.NET Core: Minimal API](../14-aspnet-core-minimal-api/overview.en.md), [15 · EF Core](../15-ef-core/overview.en.md)
- Background tasks: [10 · Async/await](../10-async-await/overview.en.md)
- The composition root and layers: [21 · Application architecture](../21-architecture/overview.en.md)
- Logs, metrics, traces: [22 · Observability](../22-observability/overview.en.md)
- Replacing dependencies in tests: [16 · Unit tests](../16-testing-unit/overview.en.md)
- The same idea in other tracks: [Python](../../../python/overview.en.md) — FastAPI dependencies, `logging`
- Recipes and libraries: [FluentValidation](../../libraries/fluentvalidation/overview.en.md), [Polly](../../libraries/polly/overview.en.md), [Refit](../../libraries/refit/overview.en.md), [Serilog](../../libraries/serilog/overview.en.md), [Spectre.Console](../../libraries/spectre-console/overview.en.md), [Background jobs](../../recipes/background-jobs/overview.en.md), [Resilient HttpClient](../../recipes/http-client-resilience/overview.en.md), [JWT authentication](../../recipes/jwt-auth/overview.en.md)
