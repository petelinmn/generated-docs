---
title: "Exercises: DI, configuration, logging"
lang: en
track: csharp
kind: module
doc: exercises
slug: 13-di-config-logging
part: 2
tags: [dependency-injection, service-lifetime, options-validation, decorator, service-scope-factory]
status: draft
updated: 2026-11-22
---

# Exercises: DI, configuration, logging

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. Solutions are under spoilers. The code was verified by running it (packages `Microsoft.Extensions.Hosting` and `Microsoft.Extensions.Options.DataAnnotations` 11.0.0); there are no runnable versions in `code/` yet (level 5 does not exist).

## 1. 🟢 Substituting a dependency

**Task:** `GreetingService` depends on `IClock` and returns "Good morning, name" before noon, otherwise "Hello, name". Register a `FakeClock` with a fixed time of 09:00 and get the service from the container.

**Expected result:**

```text
Good morning, Ann
```

<details><summary>Hint</summary>

`AddSingleton<IClock>(new FakeClock(...))`, `AddTransient<GreetingService>()`, `GetRequiredService<GreetingService>()`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 1: depend on an interface; the container injects a fake clock.
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

Verified: `Good morning, Ann`. A test for "evening" needs just a different `FakeClock`.

</details>

## 2. 🟢 Counting instances

**Task:** register `SingletonSvc`, `ScopedSvc`, `TransientSvc`; each constructor writes its name into a shared list. Create two scopes and in each request every service twice. Group the records.

**Expected result:**

```text
singleton=1 scoped=2 transient=4
```

<details><summary>Hint</summary>

`provider.CreateScope()` in a `for (i < 2)` loop, with another loop of two requests inside.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 2: instance counts reveal the lifetimes.
static class Counted { public static List<string> Log = new(); }
class SingletonSvc { public SingletonSvc() => Counted.Log.Add("singleton"); }
class ScopedSvc    { public ScopedSvc()    => Counted.Log.Add("scoped"); }
class TransientSvc { public TransientSvc() => Counted.Log.Add("transient"); }

var sc = new ServiceCollection();
sc.AddSingleton<SingletonSvc>(); sc.AddScoped<ScopedSvc>(); sc.AddTransient<TransientSvc>();
using var p = sc.BuildServiceProvider();
for (int i = 0; i < 2; i++)
{
    using var scope = p.CreateScope();
    for (int j = 0; j < 2; j++)
    {
        scope.ServiceProvider.GetRequiredService<SingletonSvc>();
        scope.ServiceProvider.GetRequiredService<ScopedSvc>();
        scope.ServiceProvider.GetRequiredService<TransientSvc>();
    }
}
Console.WriteLine(string.Join(" ", Counted.Log.GroupBy(x => x).Select(g => $"{g.Key}={g.Count()}")));
```

Verified: `singleton=1 scoped=2 transient=4`.

</details>

## 3. 🟡 Validating settings at startup

**Task:** a class `SmtpOptions` with a required `Host` and a `Port` from 1 to 65535. Bind it to the `Smtp` section, enable attribute validation and validation at start. Show a successful start and a failure with `Port = 0` and no `Host`.

**Expected result:**

```text
Host = mail.local, Port = 587 → mail.local
Port = 0, no Host             → startup failed: 2 problems
```

<details><summary>Hint</summary>

`AddOptions<SmtpOptions>().Bind(section).ValidateDataAnnotations().ValidateOnStart()`; the error shows up on `await host.StartAsync()` as an `OptionsValidationException`. You need `using Microsoft.Extensions.Logging;` (for `ClearProviders`) and the package `Microsoft.Extensions.Options.DataAnnotations`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 3: fail fast on invalid configuration.
class SmtpOptions
{
    [Required] public string Host { get; set; } = "";
    [Range(1, 65535)] public int Port { get; set; } = 25;
}

var b = Host.CreateApplicationBuilder();
b.Configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["Smtp:Host"] = "mail.local", ["Smtp:Port"] = "587" });
b.Services.AddOptions<SmtpOptions>().Bind(b.Configuration.GetSection("Smtp")).ValidateDataAnnotations().ValidateOnStart();
b.Logging.ClearProviders();
using var h = b.Build();
Console.WriteLine(h.Services.GetRequiredService<IOptions<SmtpOptions>>().Value.Host);   // mail.local

var b2 = Host.CreateApplicationBuilder(); b2.Logging.ClearProviders();
b2.Configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["Smtp:Port"] = "0" });
b2.Services.AddOptions<SmtpOptions>().Bind(b2.Configuration.GetSection("Smtp")).ValidateDataAnnotations().ValidateOnStart();
using var h2 = b2.Build();
try { await h2.StartAsync(); }
catch (OptionsValidationException e) { Console.WriteLine($"startup failed: {e.Failures.Count()} problems"); }
```

Verified: `mail.local` and `startup failed: 2 problems`.

</details>

## 4. 🟡 Singleton and Scoped

**Task:** the singleton `Cleaner` must on each `Run()` call work with a **fresh** scoped service `Db` (its `Id` is the instance's sequence number). Implement it without a "captive dependency" and enable scope validation to confirm it.

**Expected result:**

```text
Run() Run() → db#1 db#2
```

<details><summary>Hint</summary>

Inject `IServiceScopeFactory`, create a scope in `Run()` and get `Db` from it.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 4: a singleton creates a scope per operation.
class Db { public static int Next; public int Id = Interlocked.Increment(ref Next); }
class Cleaner(IServiceScopeFactory factory)
{
    public string Run()
    {
        using var scope = factory.CreateScope();
        return "db#" + scope.ServiceProvider.GetRequiredService<Db>().Id;
    }
}

var sc = new ServiceCollection();
sc.AddScoped<Db>(); sc.AddSingleton<Cleaner>();
using var p = sc.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
var cleaner = p.GetRequiredService<Cleaner>();
Console.WriteLine(cleaner.Run() + " " + cleaner.Run());   // db#1 db#2
```

Verified: `db#1 db#2`, the scope checks raise no objection.

</details>

## 5. 🔴 A decorator via a factory

**Task:** there is `IOrderService` and an implementation `OrderService`. Wrap it in a `LoggingOrderService` that writes `before …` and `after …` to a log so that consumers request `IOrderService` but receive the decorator.

**Expected result:**

```text
Place("book") → "placed book"
log: before book | after placed book
```

<details><summary>Hint</summary>

Register `OrderService` as a concrete type and `IOrderService` with a factory that creates `LoggingOrderService` on top of `GetRequiredService<OrderService>()`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 5: a decorator built with a factory registration.
interface IOrderService { string Place(string item); }
class OrderService : IOrderService { public string Place(string item) => $"placed {item}"; }
class LoggingOrderService(IOrderService inner, List<string> log) : IOrderService
{
    public string Place(string item) { log.Add($"before {item}"); var r = inner.Place(item); log.Add($"after {r}"); return r; }
}
static class Audit { public static readonly List<string> Log = new(); }

var sc = new ServiceCollection();
sc.AddSingleton<OrderService>();
sc.AddSingleton<IOrderService>(sp => new LoggingOrderService(sp.GetRequiredService<OrderService>(), Audit.Log));
using var p = sc.BuildServiceProvider();
Console.WriteLine(p.GetRequiredService<IOrderService>().Place("book"));   // placed book
Console.WriteLine(string.Join(" | ", Audit.Log));                         // before book | after placed book
```

Verified: the output matches. The external library Scrutor provides `Decorate<>()` for this.

</details>
