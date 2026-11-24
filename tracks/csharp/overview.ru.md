---
title: "C# / .NET"
lang: ru
track: csharp
kind: track
doc: overview
slug: csharp
tags: [csharp, dotnet]
versions: ".NET 11"
default_level: 6
status: draft
updated: 2026-11-20
---

# C# / .NET

🌐 **Русский** · [English](overview.en.md)

## Коротко

C# — статически типизированный объектно-ориентированный язык общего назначения от Microsoft. Код компилируется в промежуточный язык (Intermediate Language, IL) и выполняется средой .NET, которая берёт на себя JIT-компиляцию, сборку мусора и предоставляет общую библиотеку классов. .NET — кроссплатформенная платформа с открытым исходным кодом, работает на Windows, Linux и macOS. На C# пишут веб-API и бэкенд (ASP.NET Core), десктопные и консольные приложения, облачные сервисы и игры (Unity).

## Окружение

| Что | Инструмент |
|---|---|
| SDK | .NET 11 SDK или новее — `dotnet --version`; модули нацелены на `net11.0` (C# 15) |
| Редактор | VS Code + C# Dev Kit, Rider или Visual Studio |
| Код модуля | `code/` — решение `.slnx`: `Examples/` (библиотека классов), `Solutions/`, `Tests/` (xUnit v3) и `global.json`, который включает в `dotnet test` режим Microsoft.Testing.Platform |
| Запустить все тесты модуля | `dotnet test` в папке `code/` |
| Запустить один пример | `dotnet test --filter "FullyQualifiedName~<Имя>"` |
| Быстрый эксперимент | `dotnet run scratch.cs` — file-based app без проекта |

## Дорожная карта

Прогресс изучения — в [PROGRESS.md](../../PROGRESS.md). Уровень детализации модулей по умолчанию — **6** (все уровни), см. [уровни детализации](../../README.ru.md#уровни-детализации).

### Часть 1. Основы

- [01 · Платформа .NET и dotnet CLI](modules/01-dotnet-platform/overview.ru.md) — SDK и runtime, CLR, JIT и AOT, csproj/slnx, file-based apps, NuGet
- [02 · Типы и переменные](modules/02-types-and-variables/overview.ru.md) — value и reference types, приведение, строки, nullable reference types
- [03 · Управляющие конструкции и методы](modules/03-control-flow-and-methods/overview.ru.md) — if/switch, циклы, параметры ref/out/in, перегрузка, локальные функции
- [04 · ООП: классы и объекты](modules/04-oop-basics/overview.ru.md) — инкапсуляция, свойства, конструкторы, static, наследование, полиморфизм
- [05 · Интерфейсы, структуры, записи](modules/05-interfaces-structs-records/overview.ru.md) — interface и abstract class, struct, record, равенство объектов
- [06 · Коллекции и обобщения](modules/06-collections-and-generics/overview.ru.md) — `List<T>`, `Dictionary<TKey,TValue>`, `HashSet<T>`, generics, ограничения, вариантность
- [07 · LINQ](modules/07-linq/overview.ru.md) — синтаксис запросов и методов, отложенное выполнение, `IEnumerable<T>` и `IQueryable<T>`
- [08 · Исключения](modules/08-exceptions/overview.ru.md) — try/catch/finally, иерархия, свои исключения, фильтры `when`, best practices
- [09 · Делегаты, события, лямбды](modules/09-delegates-events-lambdas/overview.ru.md) — `Func`/`Action`, замыкания, события

### Часть 2. Продвинутые темы

- [10 · Async/await](modules/10-async-await/overview.ru.md) — `Task`, `ValueTask`, `CancellationToken`, deadlock-и, частые ошибки
- [11 · Современный C#](modules/11-modern-csharp/overview.ru.md) — pattern matching, switch expressions, primary constructors, collection expressions, required/init, extension members
- [12 · Память и GC](modules/12-memory-and-gc/overview.ru.md) — стек и куча, поколения GC, `IDisposable` и `using`, финализаторы и `SafeHandle`, утечки памяти, режимы и настройки GC
- [12a · Span и Memory](modules/12a-span-and-memory/overview.ru.md) — `Span<T>`, `Memory<T>`, `stackalloc`, `ref struct`, `System.IO.Pipelines`
- [13 · DI, конфигурация, логирование](modules/13-di-config-logging/overview.ru.md) — Generic Host, DI-контейнер и lifetimes, Options pattern, `ILogger`
- [14 · ASP.NET Core: Minimal API](modules/14-aspnet-core-minimal-api/overview.ru.md) — маршрутизация, middleware, валидация, OpenAPI, обработка ошибок
- [15 · EF Core](modules/15-ef-core/overview.ru.md) — DbContext, LINQ → SQL, change tracking, проблема N+1, конкурентность и транзакции, наследование, миграции
- [16 · Тестирование: unit-тесты](modules/16-testing-unit/overview.ru.md) — xUnit, AAA, параметризованные тесты, моки, покрытие
- [17 · Тестирование: интеграционные тесты](modules/17-testing-integration/overview.ru.md) — `WebApplicationFactory`, Testcontainers, тесты API и БД

### Часть 3. Темы для опытных

- [18 · Многопоточность](modules/18-concurrency/overview.ru.md) — потоки и ThreadPool, `lock` и `Interlocked`, concurrent-коллекции, Channels, `Parallel`
- [19 · Производительность](modules/19-performance/overview.ru.md) — BenchmarkDotNet, аллокации, `ArrayPool<T>`, профилирование, Native AOT
- [20 · Рефлексия и генераторы кода](modules/20-reflection-and-source-generators/overview.ru.md) — Reflection, атрибуты, Expression trees, Roslyn source generators
- [21 · Архитектура приложений](modules/21-architecture/overview.ru.md) — слои, Clean Architecture, vertical slices, CQRS, основы DDD
- [22 · Наблюдаемость](modules/22-observability/overview.ru.md) — структурные логи, метрики, трассировка, OpenTelemetry, health checks

## Библиотеки

- [Dapper](libraries/dapper/overview.ru.md) — микро-ORM: SQL вручную, маппинг автоматически
- [Serilog](libraries/serilog/overview.ru.md) — структурное логирование и sinks
- [Polly](libraries/polly/overview.ru.md) — устойчивость: retry, circuit breaker, timeout
- [FluentValidation](libraries/fluentvalidation/overview.ru.md) — валидация моделей
- [Refit](libraries/refit/overview.ru.md) — типизированные HTTP-клиенты из интерфейсов
- [Mapperly](libraries/mapperly/overview.ru.md) — маппинг объектов через source generator
- [NSubstitute](libraries/nsubstitute/overview.ru.md) — моки и стабы в тестах
- [Testcontainers](libraries/testcontainers/overview.ru.md) — настоящие БД и сервисы в Docker для тестов
- [BenchmarkDotNet](libraries/benchmarkdotnet/overview.ru.md) — микробенчмарки
- [MassTransit](libraries/masstransit/overview.ru.md) — обмен сообщениями: шина, потребители, повторы, саги, RabbitMQ
- [Spectre.Console](libraries/spectre-console/overview.ru.md) — консольные приложения с таблицами, прогрессом, промптами

## Рецепты

- [JWT-аутентификация](recipes/jwt-auth/overview.ru.md) — выдача и проверка токенов в ASP.NET Core
- [Фоновые задачи](recipes/background-jobs/overview.ru.md) — `BackgroundService`, очереди, graceful shutdown
- [Устойчивый HttpClient](recipes/http-client-resilience/overview.ru.md) — `IHttpClientFactory`, ретраи, таймауты
- [Кэширование](recipes/caching/overview.ru.md) — `IMemoryCache`, `HybridCache`, Redis
- [Rate limiting](recipes/rate-limiting/overview.ru.md) — ограничение частоты запросов к API
- [Загрузка файлов](recipes/file-upload/overview.ru.md) — upload/download, стриминг больших файлов
- [Пагинация и фильтрация](recipes/pagination-filtering/overview.ru.md) — offset- и keyset-пагинация в API
- [Real-time через SignalR](recipes/signalr-realtime/overview.ru.md) — уведомления в браузер
- [gRPC-сервис](recipes/grpc-service/overview.ru.md) — сервис и клиент на gRPC

## Проекты

- [TODO API](projects/todo-api/overview.ru.md) — REST API: Minimal API, EF Core, тесты, Docker
- [CLI-утилита](projects/cli-tool/overview.ru.md) — консольное приложение с командами и конфигом
- [Worker-сервис](projects/worker-service/overview.ru.md) — фоновая обработка очереди с ретраями и метриками

## Связанные треки

- [Python](../python/overview.ru.md) — второй язык: удобно сравнивать подходы (async, типизация, тестирование)
- [Kubernetes](../kubernetes/overview.ru.md) — деплой C#-приложений
- [JavaScript](../javascript/overview.ru.md) — `async`/`await` пришёл в JavaScript из C#: сравните реализации
- [TypeScript](../typescript/overview.ru.md) — структурная типизация против номинальной
- [LLM](../llm/overview.ru.md) — LLM-приложения на .NET через Microsoft.Extensions.AI

## Источники

- [Документация C#](https://learn.microsoft.com/dotnet/csharp/)
- [Документация .NET](https://learn.microsoft.com/dotnet/)
- [ASP.NET Core](https://learn.microsoft.com/aspnet/core/)
