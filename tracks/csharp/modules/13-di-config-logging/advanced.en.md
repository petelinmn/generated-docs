---
title: "DI, configuration, logging: advanced"
lang: en
track: csharp
kind: module
doc: advanced
slug: 13-di-config-logging
part: 2
tags: [dependency-injection, captive-dependency, keyed-services, options-pattern, options-validation, logger-message, background-service]
versions: ".NET 11, C# 15; Microsoft.Extensions.* 11.0.0"
status: draft
updated: 2026-11-22
---

# DI, configuration, logging: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [basics](basics.en.md). **Time:** ~1.5 h.

## Several implementations of one interface

If several implementations are registered, `GetRequiredService<T>()` returns the **last** one, and `GetServices<T>()` returns all in registration order. `TryAdd*` registers only if the service does not exist yet:

```csharp
sc.AddSingleton<IGreeter, EnGreeter>();
sc.AddSingleton<IGreeter, RuGreeter>();
Console.WriteLine(provider.GetRequiredService<IGreeter>().Hello());                   // привет (the last wins)
Console.WriteLine(string.Join(",", provider.GetServices<IGreeter>().Select(g => g.Hello())));   // hello,привет
sc.TryAddSingleton<IGreeter, EnGreeter>();                                            // does nothing: IGreeter is registered
```

(`using Microsoft.Extensions.DependencyInjection.Extensions;` for `TryAdd*`.) Verified: after `TryAdd` the collection still held 2 registrations. A constructor with an `IEnumerable<IGreeter>` parameter receives all implementations — this is how handler pipelines are built.

### Keyed services

When you need to pick an implementation by name rather than by type, register with a **key**:

```csharp
sc.AddKeyedSingleton<IGreeter, EnGreeter>("en");
sc.AddKeyedSingleton<IGreeter, RuGreeter>("ru");
var ru = provider.GetRequiredKeyedService<IGreeter>("ru");           // привет
// in a constructor: ctor([FromKeyedServices("ru")] IGreeter greeter)
```

### Generics, factories, decorators

An open generic type is registered whole: `AddSingleton(typeof(IRepo<>), typeof(Repo<>))`, and a request for `IRepo<int>` returns `Repo<int>` (verified). A **factory** is a delegate that creates a service by hand; a decorator — a wrapper over the main implementation — is built on it:

```csharp
sc.AddSingleton<OrderService>();
sc.AddSingleton<IOrderService>(sp => new LoggingOrderService(sp.GetRequiredService<OrderService>(), Audit.Log));

interface IOrderService { string Place(string item); }
class OrderService : IOrderService { public string Place(string item) => $"placed {item}"; }
class LoggingOrderService(IOrderService inner, List<string> log) : IOrderService
{
    public string Place(string item) { log.Add($"before {item}"); var r = inner.Place(item); log.Add($"after {r}"); return r; }
}
// Place("book") → "placed book"; the log holds "before book | after placed book"
```

## The captive dependency problem

If a long-lived service receives a short-lived one, the latter "gets stuck" for the lifetime of its owner: a `Singleton` that injects a `Scoped` service (for example a `DbContext`) will use **one** instance for all requests. The container can check this:

```csharp
sc.AddScoped<IScoped, Impl>(); sc.AddSingleton<Needs>();             // class Needs(IScoped s)
using var p = sc.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
// AggregateException: Some services are not able to be constructed (... Cannot consume scoped service 'IScoped' from singleton 'Needs'.)
```

`Host.CreateApplicationBuilder` enables the checks automatically only in the `Development` environment: verified — under `Production` the application was built without an error, under `Development` it threw an `AggregateException`. So the error may go unseen until production; run tests with the checks on.

The cure is to obtain a scope manually through `IServiceScopeFactory`: the singleton creates a scope for each operation.

```csharp
class Cleaner(IServiceScopeFactory factory)
{
    public string Run()
    {
        using var scope = factory.CreateScope();
        return "db#" + scope.ServiceProvider.GetRequiredService<Db>().Id;   // a fresh scoped Db per call
    }
}
// two calls → db#1 db#2
```

## Options: validation and reloading

Three ways to get settings differ in lifetime and in how they react to source changes:

| Interface | Lifetime | Sees configuration changes |
|---|---|---|
| `IOptions<T>` | Singleton | no: the value is fixed on first access |
| `IOptionsSnapshot<T>` | Scoped | yes, but at the boundary of a new scope |
| `IOptionsMonitor<T>` | Singleton | yes, immediately; has an `OnChange` event |

Verified on a JSON file with `reloadOnChange: true`. Before the change everything is `one`. After editing the file: in the **same** scope `IOptionsSnapshot` stayed `one`; in a new scope it is `two`; `IOptionsMonitor.CurrentValue` is `two` and `OnChange` fired; `IOptions` stayed `one`. For singleton services that need fresh settings, take `IOptionsMonitor`.

### Validating settings

Configuration errors are better detected at startup than at first use:

```csharp
class SmtpOptions
{
    [Required] public string Host { get; set; } = "";
    [Range(1, 65535)] public int Port { get; set; } = 25;
}

builder.Services.AddOptions<SmtpOptions>()
    .Bind(builder.Configuration.GetSection("Smtp"))
    .ValidateDataAnnotations()      // attributes; needs the Microsoft.Extensions.Options.DataAnnotations package
    .ValidateOnStart();             // check when the host starts, not on first use
```

Verified: with correct settings `Host = mail.local`; with `Smtp:Port = 0` and a missing `Host` the host start fails with an `OptionsValidationException` (2 problems). Without `ValidateOnStart` the check would run only on the first access to `.Value`. For complex rules use `.Validate(o => …, "message")` or `IValidateOptions<T>`.

## Configuration: binding and sources

`GetSection("Mail").Get<MailOptions>()` creates an object and fills its properties, including lists (`Mail:Tags:0`, `Mail:Tags:1` → `List<string>`: verified, `a|b`). `GetValue<int>("Mail:Port")` converts a value to a type. Case does not matter for key names. The default order of sources in `Host.CreateApplicationBuilder`: the files `appsettings.json` and `appsettings.{Environment}.json`, in `Development` the secret manager, then environment variables and the command line; **the last one wins**. Verified on layers: an environment variable with a prefix overrode the in-memory value (`fromenv`), and the argument `--Mail:Host=cli` did too.

The environment is taken from `DOTNET_ENVIRONMENT` (or `ASPNETCORE_ENVIRONMENT` for web applications); the default is `Production` (verified: `env=Production`).

## Logging in more depth

### Scopes and filters

`BeginScope` adds context to all messages inside a block — handy for a request identifier:

```csharp
using (log.BeginScope("order-7"))
    log.LogWarning("slow {Ms}ms", 1500);          // Warning: slow 1500ms [scope order-7]
```

The minimum level is set globally (`SetMinimumLevel`) or per category in configuration (`Logging:LogLevel:Microsoft.EntityFrameworkCore: Warning`). Before expensive message preparation, check `log.IsEnabled(LogLevel.Debug)`.

### LoggerMessage: fast messages

The `[LoggerMessage]` attribute generates a method that does not box arguments and does not parse the template on every call:

```csharp
partial class Worker(ILogger<Worker> logger)
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Worker started with {Interval} ms")]
    static partial void Started(ILogger l, int interval);
}
```

Verified: code with the attribute compiles and works; the generator is part of the .NET SDK.

## Background services and stopping

`BackgroundService.ExecuteAsync(CancellationToken)` runs as long as the host works; on `StopAsync` the token is canceled, and the service must finish (verified: `start, stopping, stopped`). Pass this token into long waits, as in [10 · Async/await](../10-async-await/overview.en.md). The time for stopping is limited (`HostOptions.ShutdownTimeout`).

## Best practices

| Rule | Why |
|---|---|
| Depend on an interface, inject through the constructor | testability, replaceable implementation |
| A singleton does not hold a Scoped service | otherwise request data "gets stuck" |
| Enable `ValidateScopes` and `ValidateOnBuild` in tests | errors are found before production |
| `ValidateOnStart` for settings | a failure at startup instead of the first request |
| Message templates, not interpolation | structure and performance |
| Do not call `GetService` from business code | a Service Locator hides dependencies |

## Related materials

- Back: [basics](basics.en.md). Next: [expert theory](expert.en.md) — container internals, gotchas.
- [10 · Async/await](../10-async-await/overview.en.md) — cancellation and background tasks.
- [15 · EF Core](../15-ef-core/overview.en.md) — `DbContext` as a Scoped service.
- [14 · ASP.NET Core: Minimal API](../14-aspnet-core-minimal-api/overview.en.md) — a scope = a request.
- [12 · Memory and GC](../12-memory-and-gc/overview.en.md) — the container disposing services.

## Sources

- [Service lifetimes](https://learn.microsoft.com/dotnet/core/extensions/dependency-injection#service-lifetimes)
- [Keyed services](https://learn.microsoft.com/dotnet/core/extensions/dependency-injection#keyed-services)
- [Options validation](https://learn.microsoft.com/dotnet/core/extensions/options#options-validation)
- [Compile-time logging source generation](https://learn.microsoft.com/dotnet/core/extensions/logger-message-generator)
- [Worker services](https://learn.microsoft.com/dotnet/core/extensions/workers)
