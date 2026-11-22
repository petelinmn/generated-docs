---
title: "DI, configuration, logging: basics"
lang: en
track: csharp
kind: module
doc: basics
slug: 13-di-config-logging
part: 2
tags: [dependency-injection, service-lifetime, configuration, options-pattern, logging, generic-host]
versions: ".NET 11, C# 15; Microsoft.Extensions.* 11.0.0"
status: draft
updated: 2026-11-22
---

# DI, configuration, logging: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [04 · OOP: classes and objects](../04-oop-basics/overview.en.md), [05 · Interfaces, structs, records](../05-interfaces-structs-records/overview.en.md). **Time:** ~1 h.

## Why it matters

A real application consists of many classes that depend on each other: an order service uses a repository, the repository uses a database connection, and all of them need settings and a log. If each class creates its own dependencies with `new`, the code is hard to test and change. Three platform mechanisms solve this:

- **dependency injection** (DI) — dependencies are not created inside a class but passed to it from outside; the container creates them;
- **configuration** — settings come from files, environment variables, the command line;
- **logging** — a single `ILogger` interface with pluggable destinations.

All of this is assembled by the **Generic Host** — the same skeleton underlies [ASP.NET Core](../14-aspnet-core-minimal-api/overview.en.md) and background services. The packages are `Microsoft.Extensions.*`.

## Dependency injection

A class declares its dependencies as constructor parameters and depends on an **interface**, not a concrete type:

```csharp
interface IClock { DateTime Now { get; } }
class FakeClock(DateTime now) : IClock { public DateTime Now { get; } = now; }

class GreetingService(IClock clock)      // the dependency comes from outside
{
    public string Greet(string n) => (clock.Now.Hour < 12 ? "Good morning, " : "Hello, ") + n;
}

var services = new ServiceCollection();
services.AddSingleton<IClock>(new FakeClock(new DateTime(2026, 1, 1, 9, 0, 0)));
services.AddTransient<GreetingService>();             // the container builds it and passes IClock itself
using var provider = services.BuildServiceProvider();
Console.WriteLine(provider.GetRequiredService<GreetingService>().Greet("Ann"));   // Good morning, Ann
```

(`using Microsoft.Extensions.DependencyInjection;`, package `Microsoft.Extensions.DependencyInjection`.) Two phases: first **registration** in a `ServiceCollection` ("when `IClock` is requested, hand out `FakeClock`"), then `BuildServiceProvider()` and **resolving** services. The benefit: in a test you substitute `FakeClock` for `SystemClock` without changing `GreetingService`.

If a service is not registered, `GetRequiredService` throws `InvalidOperationException` ("No service for type 'IGreeter' has been registered.") while `GetService` returns `null` (verified).

## Service lifetimes

At registration you choose how long a created instance lives:

| Lifetime | Instance |
|---|---|
| `Singleton` | one for the whole application |
| `Scoped` | one per **scope**; in a web application a scope is one HTTP request |
| `Transient` | a new one on every request for the service |

Verified with two scopes and two requests per service in each:

```csharp
// two scopes, two resolutions of each service per scope
// singleton=1 scoped=2 transient=4   (instances created)
```

A `Singleton` is shared across scopes too, a `Scoped` instance is the same inside a scope and different between scopes, a `Transient` is new every time. A scope is created with `provider.CreateScope()`; services that implement `IDisposable` are disposed when the scope ends (verified: `Disp disposed` is printed on leaving `using`).

A rule for choosing: a stateless service — `Singleton`; request state or a `DbContext` ([15 · EF Core](../15-ef-core/overview.en.md)) — `Scoped`; a light mutable one — `Transient`.

## Configuration

Configuration is a set of key–value pairs with a hierarchy through a colon (`Mail:Host`). It is assembled from **providers**; when keys coincide, the provider added **later** wins:

```csharp
var cfg = new ConfigurationBuilder()
    .AddInMemoryCollection(new Dictionary<string, string?> { ["Mail:Host"] = "base", ["Mail:Port"] = "25" })
    .AddInMemoryCollection(new Dictionary<string, string?> { ["Mail:Host"] = "override" })
    .Build();
Console.WriteLine($"{cfg["Mail:Host"]} {cfg.GetValue<int>("Mail:Port")}");   // override 25
```

In a typical application the sources are: `appsettings.json`, `appsettings.{Environment}.json`, environment variables, command-line arguments — each next one overrides the previous. For environment variables nesting is written with `__` (`APP__Mail__Host`), and the command line as `--Mail:Host=cli`:

```csharp
Environment.SetEnvironmentVariable("APP__Mail__Host", "fromenv");
var c2 = new ConfigurationBuilder().AddEnvironmentVariables("APP__").Build();
Console.WriteLine(c2["Mail:Host"]);                                                  // fromenv
Console.WriteLine(new ConfigurationBuilder().AddCommandLine(["--Mail:Host=cli"]).Build()["Mail:Host"]);   // cli
```

A missing key gives `null`, not an exception. Secrets (passwords, keys) are not stored in `appsettings.json` — only in environment variables or the secret manager.

## Binding to types: the Options pattern

Instead of reading keys by strings, bind the settings to a class and receive it through DI:

```csharp
class MailOptions
{
    public string Host { get; set; } = "";
    public int Port { get; set; } = 25;
}

builder.Services.Configure<MailOptions>(builder.Configuration.GetSection("Mail"));

class Mailer(IOptions<MailOptions> options)
{
    public string Target => $"{options.Value.Host}:{options.Value.Port}";
}
```

`IOptions<T>.Value` is the ready settings object. More about validation and reloading in the [advanced theory](advanced.en.md#options-validation-and-reloading).

## Logging

`ILogger<T>` is injected into a class through the constructor; the category is taken from `T`. Each message has a **level**: `Trace`, `Debug`, `Information`, `Warning`, `Error`, `Critical`. A message is written as a **template** with named placeholders — the values are substituted separately and stored as structured data:

```csharp
class OrderService(ILogger<OrderService> log)
{
    public void Place(int userId, int count)
    {
        log.LogInformation("User {UserId} bought {Count} items", userId, count);
    }
}
```

The result with a destination that prints text: `Information: User 42 bought 3 items` (verified with a custom `ILoggerProvider`). Messages below the minimum level are dropped: at `Information` a `LogDebug` call writes nothing (verified: `IsEnabled(Debug)` is `False`). The exception is passed as the first argument: `log.LogError(ex, "failed for {Id}", 1)`.

Do not build the string by hand (`$"User {id}"`): the structure is lost and the text is formatted needlessly.

## The Generic Host

`Host.CreateApplicationBuilder()` assembles everything: configuration, DI and logging:

```csharp
var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton<IClock, SystemClock>();      // register services
builder.Services.AddHostedService<Worker>();               // a background service
using var host = builder.Build();
await host.RunAsync();                                     // runs until Ctrl+C / SIGTERM
```

(`using Microsoft.Extensions.Hosting;`, package `Microsoft.Extensions.Hosting`.) `builder.Configuration`, `builder.Services` and `builder.Logging` are the three configurable parts. A background service is a `BackgroundService` subclass with an `ExecuteAsync` method; when the host stops it is given a canceled token (verified: the events `start, stopping, stopped`).

## Beginner mistakes

- ❌ `new` for dependencies inside a class → ✅ dependencies through the constructor.
- ❌ Registering everything as `Singleton` → ✅ choose the lifetime deliberately: request state — `Scoped`.
- ❌ Reading settings by string all over the code → ✅ bind to a class and inject `IOptions<T>`.
- ❌ Secrets in `appsettings.json` in the repository → ✅ environment variables or the secret manager.
- ❌ `log.LogInformation($"User {id}")` → ✅ the template `"User {UserId}"` with an argument.
- ❌ Service Locator: calling `GetService` from business code → ✅ receive dependencies through the constructor.

## Related materials

- Next: [advanced theory](advanced.en.md) — scope validation, Options, keyed services, `LoggerMessage`.
- [14 · ASP.NET Core: Minimal API](../14-aspnet-core-minimal-api/overview.en.md) — the same host in a web application.
- [05 · Interfaces, structs, records](../05-interfaces-structs-records/overview.en.md) — interfaces as dependency contracts.
- [12 · Memory and GC](../12-memory-and-gc/overview.en.md) — `IDisposable` and disposing services.
- [Python](../../../python/overview.en.md) — dependency injection (FastAPI `Depends`), `logging`.

## Sources

- [Dependency injection in .NET](https://learn.microsoft.com/dotnet/core/extensions/dependency-injection)
- [Configuration in .NET](https://learn.microsoft.com/dotnet/core/extensions/configuration)
- [The Options pattern](https://learn.microsoft.com/dotnet/core/extensions/options)
- [Logging in .NET](https://learn.microsoft.com/dotnet/core/extensions/logging)
- [The Generic Host](https://learn.microsoft.com/dotnet/core/extensions/generic-host)
