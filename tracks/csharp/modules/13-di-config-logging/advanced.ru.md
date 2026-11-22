---
title: "DI, конфигурация, логирование: продвинутая теория"
lang: ru
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

# DI, конфигурация, логирование: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [базовая теория](basics.ru.md). **Время:** ~1,5 ч.

## Несколько реализаций одного интерфейса

Если зарегистрировано несколько реализаций, `GetRequiredService<T>()` отдаёт **последнюю**, а `GetServices<T>()` — все по порядку регистрации. `TryAdd*` регистрирует, только если сервиса ещё нет:

```csharp
sc.AddSingleton<IGreeter, EnGreeter>();
sc.AddSingleton<IGreeter, RuGreeter>();
Console.WriteLine(provider.GetRequiredService<IGreeter>().Hello());                   // привет (the last wins)
Console.WriteLine(string.Join(",", provider.GetServices<IGreeter>().Select(g => g.Hello())));   // hello,привет
sc.TryAddSingleton<IGreeter, EnGreeter>();                                            // does nothing: IGreeter is registered
```

(`using Microsoft.Extensions.DependencyInjection.Extensions;` для `TryAdd*`.) Проверено: после `TryAdd` в коллекции осталось 2 регистрации. Конструктор с параметром `IEnumerable<IGreeter>` получит все реализации — так делают конвейеры обработчиков.

### Ключевые сервисы (keyed)

Когда нужно выбрать реализацию по имени, а не по типу, регистрируют с **ключом**:

```csharp
sc.AddKeyedSingleton<IGreeter, EnGreeter>("en");
sc.AddKeyedSingleton<IGreeter, RuGreeter>("ru");
var ru = provider.GetRequiredKeyedService<IGreeter>("ru");           // привет
// in a constructor: ctor([FromKeyedServices("ru")] IGreeter greeter)
```

### Обобщения, фабрики, декораторы

Открытый обобщённый тип регистрируют целиком: `AddSingleton(typeof(IRepo<>), typeof(Repo<>))`, и запрос `IRepo<int>` вернёт `Repo<int>` (проверено). **Фабрика** — делегат, создающий сервис вручную; на ней строится декоратор — обёртка над основной реализацией:

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

## Проблема «захваченной зависимости»

Если долгоживущий сервис получает короткоживущий, тот «застревает» на всё время жизни владельца: `Singleton`, внедряющий `Scoped` (например, `DbContext`), будет использовать **один** экземпляр для всех запросов. Контейнер умеет проверять это:

```csharp
sc.AddScoped<IScoped, Impl>(); sc.AddSingleton<Needs>();             // class Needs(IScoped s)
using var p = sc.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
// AggregateException: Some services are not able to be constructed (... Cannot consume scoped service 'IScoped' from singleton 'Needs'.)
```

`Host.CreateApplicationBuilder` включает проверки автоматически только в окружении `Development`: проверено — при `Production` приложение собралось без ошибки, при `Development` бросило `AggregateException`. Поэтому ошибку можно не увидеть до продакшена; запускайте тесты с проверками включёнными.

Лекарство — получать область вручную через `IServiceScopeFactory`: singleton создаёт область на каждую операцию.

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

## Options: проверка и перезагрузка

Три способа получить настройки, различаются временем жизни и реакцией на изменения источника:

| Интерфейс | Время жизни | Видит изменения конфигурации |
|---|---|---|
| `IOptions<T>` | Singleton | нет: значение фиксируется при первом обращении |
| `IOptionsSnapshot<T>` | Scoped | да, но на границе новой области |
| `IOptionsMonitor<T>` | Singleton | да, немедленно; есть событие `OnChange` |

Проверено на JSON-файле с `reloadOnChange: true`. До изменения везде `one`. После правки файла: в **той же** области `IOptionsSnapshot` остался `one`; в новой области он `two`; `IOptionsMonitor.CurrentValue` — `two` и сработал `OnChange`; `IOptions` остался `one`. Для сервисов-одиночек, которым нужны свежие настройки, берут `IOptionsMonitor`.

### Проверка настроек

Ошибки в конфигурации лучше обнаруживать при запуске, а не при первом использовании:

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

Проверено: при корректных настройках `Host = mail.local`; при `Smtp:Port = 0` и отсутствующем `Host` запуск хоста падает с `OptionsValidationException` (2 проблемы). Без `ValidateOnStart` проверка выполнилась бы только при первом обращении к `.Value`. Для сложных правил — `.Validate(o => …, "message")` или `IValidateOptions<T>`.

## Конфигурация: привязка и источники

`GetSection("Mail").Get<MailOptions>()` создаёт объект и заполняет свойства, включая списки (`Mail:Tags:0`, `Mail:Tags:1` → `List<string>`: проверено, `a|b`). `GetValue<int>("Mail:Port")` преобразует значение к типу. Для имён ключей регистр не важен. Порядок источников по умолчанию в `Host.CreateApplicationBuilder`: файлы `appsettings.json` и `appsettings.{Environment}.json`, в `Development` — менеджер секретов, затем переменные окружения и командная строка; **побеждает последний**. Проверено на слоях: переменная окружения с префиксом перекрыла значение из памяти (`fromenv`), аргумент `--Mail:Host=cli` — тоже.

Окружение берётся из `DOTNET_ENVIRONMENT` (или `ASPNETCORE_ENVIRONMENT` для веб-приложений); по умолчанию — `Production` (проверено: `env=Production`).

## Логирование глубже

### Области (scopes) и фильтры

`BeginScope` добавляет контекст ко всем сообщениям внутри блока — удобно для идентификатора запроса:

```csharp
using (log.BeginScope("order-7"))
    log.LogWarning("slow {Ms}ms", 1500);          // Warning: slow 1500ms [scope order-7]
```

Минимальный уровень задают глобально (`SetMinimumLevel`) или по категориям в конфигурации (`Logging:LogLevel:Microsoft.EntityFrameworkCore: Warning`). Перед дорогой подготовкой сообщения проверяют `log.IsEnabled(LogLevel.Debug)`.

### LoggerMessage: быстрые сообщения

Атрибут `[LoggerMessage]` генерирует метод, который не боксит аргументы и не разбирает шаблон при каждом вызове:

```csharp
partial class Worker(ILogger<Worker> logger)
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Worker started with {Interval} ms")]
    static partial void Started(ILogger l, int interval);
}
```

Проверено: код с атрибутом компилируется и работает; источник генерации входит в SDK .NET.

## Фоновые сервисы и остановка

`BackgroundService.ExecuteAsync(CancellationToken)` выполняется, пока хост работает; при `StopAsync` токен отменяется, и сервис должен завершиться (проверено: `start, stopping, stopped`). Внутри длинных ожиданий передавайте этот токен, как в [10 · Async/await](../10-async-await/overview.ru.md). Время на остановку ограничено (`HostOptions.ShutdownTimeout`).

## Лучшие практики

| Правило | Почему |
|---|---|
| Зависимость от интерфейса, внедрение через конструктор | тестируемость, замена реализации |
| Singleton не держит Scoped | иначе «застрявшие» данные запроса |
| Включайте `ValidateScopes` и `ValidateOnBuild` в тестах | ошибки находятся до продакшена |
| `ValidateOnStart` для настроек | падение на старте вместо первого запроса |
| Шаблоны сообщений, а не интерполяция | структура и производительность |
| Не вызывайте `GetService` из бизнес-кода | Service Locator скрывает зависимости |

## Связанные материалы

- Назад: [базовая теория](basics.ru.md). Дальше: [экспертная теория](expert.ru.md) — устройство контейнера, подвохи.
- [10 · Async/await](../10-async-await/overview.ru.md) — отмена и фоновые задачи.
- [15 · EF Core](../15-ef-core/overview.ru.md) — `DbContext` как Scoped-сервис.
- [14 · ASP.NET Core: Minimal API](../14-aspnet-core-minimal-api/overview.ru.md) — область = запрос.
- [12 · Память и GC](../12-memory-and-gc/overview.ru.md) — освобождение сервисов контейнером.

## Источники

- [Время жизни сервисов](https://learn.microsoft.com/dotnet/core/extensions/dependency-injection#service-lifetimes)
- [Ключевые сервисы](https://learn.microsoft.com/dotnet/core/extensions/dependency-injection#keyed-services)
- [Проверка Options](https://learn.microsoft.com/dotnet/core/extensions/options#options-validation)
- [Генерация логирования во время компиляции](https://learn.microsoft.com/dotnet/core/extensions/logger-message-generator)
- [Рабочие сервисы](https://learn.microsoft.com/dotnet/core/extensions/workers)
