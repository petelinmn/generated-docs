---
title: "DI, конфигурация, логирование"
lang: ru
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

# DI, конфигурация, логирование

🌐 **Русский** · [English](overview.en.md)

## Коротко

Универсальный хост (Generic Host) объединяет три механизма, на которых стоят современные приложения .NET: внедрение зависимостей (DI), конфигурацию и логирование. Классы получают зависимости через конструктор, настройки приходят из слоёв источников (файлы, окружение, командная строка) и привязываются к типам, а `ILogger` пишет структурные сообщения в подключаемые получатели. Те же механизмы используют ASP.NET Core, фоновые сервисы и консольные приложения.

## Ключевые идеи

- **DI:** класс зависит от интерфейса, контейнер создаёт и подаёт зависимости; регистрация отделена от получения.
- **Время жизни:** `Singleton` — один, `Scoped` — один на область (запрос), `Transient` — каждый раз новый.
- **«Захваченная зависимость»:** singleton с scoped-зависимостью — частая ошибка; ловится `ValidateScopes`/`ValidateOnBuild` (по умолчанию включены только в `Development`).
- **Конфигурация:** слои источников, побеждает последний; вложенность через `:` (в окружении — `__`); секреты не в репозитории.
- **Options:** `IOptions<T>` (без перезагрузки), `IOptionsSnapshot<T>` (на область), `IOptionsMonitor<T>` (немедленно); `ValidateOnStart` ловит ошибки при запуске.
- **Логирование:** шаблоны с именованными местами, уровни, области, `[LoggerMessage]`; интерполяция строк — антипаттерн.
- **Хост:** `Host.CreateApplicationBuilder`, `BackgroundService` с аккуратной остановкой по токену.

## Минимальный пример

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

## Когда применять, а когда нет

- ✅ Любое приложение с несколькими взаимодействующими классами и тестами.
- ✅ Настройки, зависящие от окружения (dev/prod), — слои конфигурации и Options.
- ✅ Структурное логирование и диагностика.
- ❌ `Service Locator` — вызов `GetService` из бизнес-кода.
- ❌ Singleton, держащий scoped-сервис (`DbContext`).
- ❌ Секреты в `appsettings.json` в репозитории.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | DI и время жизни, конфигурация и слои, привязка и `IOptions`, `ILogger` и шаблоны, универсальный хост |
| [2. Продвинутая теория](advanced.ru.md) | несколько реализаций, ключевые сервисы, декораторы, захваченная зависимость и `IServiceScopeFactory`, Options и проверка, области и `LoggerMessage`, `BackgroundService` |
| [3. Экспертная теория](expert.ru.md) | устройство контейнера, выбор конструктора, освобождение и утечки Transient, Options и конфигурация изнутри, конвейер логирования, 14 вопросов с собеседований |
| [4. Самопроверка](quiz.ru.md) | 11 вопросов и [5 упражнений](exercises.ru.md) с решениями |
| 5. Запускаемые примеры | ещё не создан — `/learn csharp 13-di-config-logging level 5` |
| 6. Тесты | ещё не создан — `/learn csharp 13-di-config-logging level 6` |

## Связанные материалы

- Пререквизиты: [05 · Интерфейсы, структуры, записи](../05-interfaces-structs-records/overview.ru.md), [12 · Память и GC](../12-memory-and-gc/overview.ru.md)
- Дальше: [14 · ASP.NET Core: Minimal API](../14-aspnet-core-minimal-api/overview.ru.md), [15 · EF Core](../15-ef-core/overview.ru.md)
- Фоновые задачи: [10 · Async/await](../10-async-await/overview.ru.md)
- Корень композиции и слои: [21 · Архитектура приложений](../21-architecture/overview.ru.md)
- Логи, метрики, трассы: [22 · Наблюдаемость](../22-observability/overview.ru.md)
- Подмена зависимостей в тестах: [16 · Unit-тесты](../16-testing-unit/overview.ru.md)
- То же в других треках: [Python](../../../python/overview.ru.md) — зависимости FastAPI, `logging`
