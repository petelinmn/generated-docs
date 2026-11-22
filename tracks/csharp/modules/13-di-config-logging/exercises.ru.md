---
title: "Упражнения: DI, конфигурация, логирование"
lang: ru
track: csharp
kind: module
doc: exercises
slug: 13-di-config-logging
part: 2
tags: [dependency-injection, service-lifetime, options-validation, decorator, service-scope-factory]
status: draft
updated: 2026-11-22
---

# Упражнения: DI, конфигурация, логирование

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Решения — под спойлерами. Код проверен запуском (пакеты `Microsoft.Extensions.Hosting` и `Microsoft.Extensions.Options.DataAnnotations` 11.0.0); запускаемых версий в `code/` пока нет (уровень 5 не создан).

## 1. 🟢 Подмена зависимости

**Задача:** `GreetingService` зависит от `IClock` и возвращает «Good morning, имя» до полудня, иначе «Hello, имя». Зарегистрируй `FakeClock` с фиксированным временем 09:00 и получи сервис из контейнера.

**Ожидаемый результат:**

```text
Good morning, Ann
```

<details><summary>Подсказка</summary>

`AddSingleton<IClock>(new FakeClock(...))`, `AddTransient<GreetingService>()`, `GetRequiredService<GreetingService>()`.

</details>

<details><summary>Решение</summary>

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

Проверено: `Good morning, Ann`. В тесте на «вечер» достаточно другого `FakeClock`.

</details>

## 2. 🟢 Считаем экземпляры

**Задача:** зарегистрируй `SingletonSvc`, `ScopedSvc`, `TransientSvc`; каждый конструктор записывает своё имя в общий список. Создай две области и в каждой запроси каждый сервис дважды. Сгруппируй записи.

**Ожидаемый результат:**

```text
singleton=1 scoped=2 transient=4
```

<details><summary>Подсказка</summary>

`provider.CreateScope()` в цикле `for (i < 2)`, внутри — ещё цикл на два запроса.

</details>

<details><summary>Решение</summary>

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

Проверено: `singleton=1 scoped=2 transient=4`.

</details>

## 3. 🟡 Проверка настроек при старте

**Задача:** класс `SmtpOptions` с обязательным `Host` и `Port` от 1 до 65535. Привяжи его к секции `Smtp`, включи проверку атрибутов и проверку при старте. Покажи успешный запуск и падение при `Port = 0` без `Host`.

**Ожидаемый результат:**

```text
Host = mail.local, Port = 587 → mail.local
Port = 0, без Host            → startup failed: 2 problems
```

<details><summary>Подсказка</summary>

`AddOptions<SmtpOptions>().Bind(section).ValidateDataAnnotations().ValidateOnStart()`; ошибка проявится при `await host.StartAsync()` как `OptionsValidationException`. Нужны `using Microsoft.Extensions.Logging;` (для `ClearProviders`) и пакет `Microsoft.Extensions.Options.DataAnnotations`.

</details>

<details><summary>Решение</summary>

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

Проверено: `mail.local` и `startup failed: 2 problems`.

</details>

## 4. 🟡 Singleton и Scoped

**Задача:** singleton `Cleaner` должен при каждом вызове `Run()` работать со **свежим** scoped-сервисом `Db` (его `Id` — порядковый номер экземпляра). Реализуй без «захваченной зависимости» и включи проверки областей, чтобы это подтвердить.

**Ожидаемый результат:**

```text
Run() Run() → db#1 db#2
```

<details><summary>Подсказка</summary>

Внедряй `IServiceScopeFactory`, в `Run()` создавай область и получай `Db` из неё.

</details>

<details><summary>Решение</summary>

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

Проверено: `db#1 db#2`, проверки областей не возражают.

</details>

## 5. 🔴 Декоратор через фабрику

**Задача:** есть `IOrderService` и реализация `OrderService`. Оберни её в `LoggingOrderService`, записывающий в журнал `before …` и `after …`, так, чтобы потребители запрашивали `IOrderService`, а получали декоратор.

**Ожидаемый результат:**

```text
Place("book") → "placed book"
журнал: before book | after placed book
```

<details><summary>Подсказка</summary>

Зарегистрируй `OrderService` как конкретный тип, а `IOrderService` — фабрикой, которая создаёт `LoggingOrderService` поверх `GetRequiredService<OrderService>()`.

</details>

<details><summary>Решение</summary>

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

Проверено: вывод совпадает. Библиотека Scrutor (внешняя) даёт для этого `Decorate<>()`.

</details>
