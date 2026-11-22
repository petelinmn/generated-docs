---
title: "DI, конфигурация, логирование: базовая теория"
lang: ru
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

# DI, конфигурация, логирование: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [04 · ООП: классы и объекты](../04-oop-basics/overview.ru.md), [05 · Интерфейсы, структуры, записи](../05-interfaces-structs-records/overview.ru.md). **Время:** ~1 ч.

## Зачем это нужно

Реальное приложение состоит из множества классов, которые зависят друг от друга: сервис заказов использует репозиторий, репозиторий — подключение к БД, всем нужны настройки и журнал. Если каждый класс сам создаёт свои зависимости через `new`, код трудно тестировать и менять. Три механизма платформы решают эту задачу:

- **внедрение зависимостей** (Dependency Injection, DI) — зависимости не создаются внутри класса, а передаются ему снаружи; создаёт их контейнер;
- **конфигурация** — настройки приходят из файлов, переменных окружения, командной строки;
- **логирование** — единый интерфейс `ILogger` с подключаемыми получателями.

Всё это собирает **универсальный хост** (Generic Host) — тот же каркас лежит в основе [ASP.NET Core](../14-aspnet-core-minimal-api/overview.ru.md) и фоновых сервисов. Пакеты — `Microsoft.Extensions.*`.

## Внедрение зависимостей

Класс объявляет зависимости параметрами конструктора и зависит от **интерфейса**, а не от конкретного типа:

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

(`using Microsoft.Extensions.DependencyInjection;`, пакет `Microsoft.Extensions.DependencyInjection`.) Две фазы: сначала **регистрация** в `ServiceCollection` («когда просят `IClock` — отдай `FakeClock`»), затем `BuildServiceProvider()` и **получение** сервисов. Выигрыш: в тесте вместо `SystemClock` подставляют `FakeClock`, не меняя `GreetingService`.

Если сервис не зарегистрирован, `GetRequiredService` бросает `InvalidOperationException` («No service for type 'IGreeter' has been registered.»), а `GetService` возвращает `null` (проверено).

## Время жизни сервисов

При регистрации выбирают, как долго живёт созданный экземпляр:

| Время жизни | Экземпляр |
|---|---|
| `Singleton` | один на всё приложение |
| `Scoped` | один на **область** (scope); в веб-приложении область — один HTTP-запрос |
| `Transient` | новый при каждом запросе сервиса |

Проверено на двух областях с двумя запросами к каждому сервису:

```csharp
// two scopes, two resolutions of each service per scope
// singleton=1 scoped=2 transient=4   (instances created)
```

`Singleton` общий и между областями, `Scoped` одинаков внутри области и разный между областями, `Transient` каждый раз новый. Область создаёт `provider.CreateScope()`; сервисы, реализующие `IDisposable`, освобождаются при завершении области (проверено: `Disp disposed` печатается при выходе из `using`).

Правило выбора: сервис без состояния — `Singleton`; состояние запроса или `DbContext` ([15 · EF Core](../15-ef-core/overview.ru.md)) — `Scoped`; лёгкий и изменяемый — `Transient`.

## Конфигурация

Конфигурация — набор пар «ключ — значение» с иерархией через двоеточие (`Mail:Host`). Её собирают из **провайдеров**; при совпадении ключей побеждает провайдер, добавленный **позже**:

```csharp
var cfg = new ConfigurationBuilder()
    .AddInMemoryCollection(new Dictionary<string, string?> { ["Mail:Host"] = "base", ["Mail:Port"] = "25" })
    .AddInMemoryCollection(new Dictionary<string, string?> { ["Mail:Host"] = "override" })
    .Build();
Console.WriteLine($"{cfg["Mail:Host"]} {cfg.GetValue<int>("Mail:Port")}");   // override 25
```

В типичном приложении источники такие: `appsettings.json`, `appsettings.{Environment}.json`, переменные окружения, аргументы командной строки — каждый следующий перекрывает предыдущие. Для переменных окружения вложенность записывают через `__` (`APP__Mail__Host`), а командную строку — `--Mail:Host=cli`:

```csharp
Environment.SetEnvironmentVariable("APP__Mail__Host", "fromenv");
var c2 = new ConfigurationBuilder().AddEnvironmentVariables("APP__").Build();
Console.WriteLine(c2["Mail:Host"]);                                                  // fromenv
Console.WriteLine(new ConfigurationBuilder().AddCommandLine(["--Mail:Host=cli"]).Build()["Mail:Host"]);   // cli
```

Отсутствующий ключ даёт `null`, а не исключение. Секреты (пароли, ключи) в `appsettings.json` не хранят — только в переменных окружения или менеджере секретов.

## Привязка к типам: шаблон Options

Вместо чтения ключей по строкам настройки привязывают к классу и получают его через DI:

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

`IOptions<T>.Value` — готовый объект настроек. Подробнее о проверке и перезагрузке — в [продвинутой теории](advanced.ru.md#options-проверка-и-перезагрузка).

## Логирование

`ILogger<T>` внедряется в класс через конструктор; категория берётся из `T`. У каждого сообщения есть **уровень**: `Trace`, `Debug`, `Information`, `Warning`, `Error`, `Critical`. Сообщение пишут **шаблоном** с именованными местами — значения подставляются отдельно и сохраняются как структурные данные:

```csharp
class OrderService(ILogger<OrderService> log)
{
    public void Place(int userId, int count)
    {
        log.LogInformation("User {UserId} bought {Count} items", userId, count);
    }
}
```

Результат с получателем, печатающим текст: `Information: User 42 bought 3 items` (проверено на собственном `ILoggerProvider`). Сообщения ниже минимального уровня отбрасываются: при `Information` вызов `LogDebug` ничего не пишет (проверено: `IsEnabled(Debug)` — `False`). Исключение передают первым аргументом: `log.LogError(ex, "failed for {Id}", 1)`.

Не склеивайте строку вручную (`$"User {id}"`): теряется структура и лишний раз форматируется текст.

## Универсальный хост

`Host.CreateApplicationBuilder()` собирает всё вместе: конфигурацию, DI и логирование:

```csharp
var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton<IClock, SystemClock>();      // register services
builder.Services.AddHostedService<Worker>();               // a background service
using var host = builder.Build();
await host.RunAsync();                                     // runs until Ctrl+C / SIGTERM
```

(`using Microsoft.Extensions.Hosting;`, пакет `Microsoft.Extensions.Hosting`.) `builder.Configuration`, `builder.Services` и `builder.Logging` — три настраиваемые части. Фоновый сервис — наследник `BackgroundService` с методом `ExecuteAsync`; при остановке хоста ему передаётся отменённый токен (проверено: события `start, stopping, stopped`).

## Ошибки новичка

- ❌ `new` зависимостей внутри класса → ✅ зависимости через конструктор.
- ❌ Регистрировать всё как `Singleton` → ✅ выбирать время жизни осознанно: состояние запроса — `Scoped`.
- ❌ Читать настройки строками по всему коду → ✅ привязать к классу и внедрять `IOptions<T>`.
- ❌ Секреты в `appsettings.json` в репозитории → ✅ переменные окружения или менеджер секретов.
- ❌ `log.LogInformation($"User {id}")` → ✅ шаблон `"User {UserId}"` с аргументом.
- ❌ `Service Locator`: вызывать `GetService` из бизнес-кода → ✅ получать зависимости конструктором.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — проверка областей, Options, ключевые сервисы, `LoggerMessage`.
- [14 · ASP.NET Core: Minimal API](../14-aspnet-core-minimal-api/overview.ru.md) — тот же хост в веб-приложении.
- [05 · Интерфейсы, структуры, записи](../05-interfaces-structs-records/overview.ru.md) — интерфейсы как контракты зависимостей.
- [12 · Память и GC](../12-memory-and-gc/overview.ru.md) — `IDisposable` и освобождение сервисов.
- [Python](../../../python/overview.ru.md) — внедрение зависимостей (FastAPI `Depends`), `logging`.

## Источники

- [Внедрение зависимостей в .NET](https://learn.microsoft.com/dotnet/core/extensions/dependency-injection)
- [Конфигурация в .NET](https://learn.microsoft.com/dotnet/core/extensions/configuration)
- [Шаблон Options](https://learn.microsoft.com/dotnet/core/extensions/options)
- [Логирование в .NET](https://learn.microsoft.com/dotnet/core/extensions/logging)
- [Универсальный хост](https://learn.microsoft.com/dotnet/core/extensions/generic-host)
