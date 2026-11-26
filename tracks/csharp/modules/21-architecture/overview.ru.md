---
title: "Архитектура приложений"
lang: ru
track: csharp
kind: module
doc: overview
slug: 21-architecture
part: 3
levels: [1, 2, 3, 4]
tags: [architecture, clean-architecture, ddd, cqrs, vertical-slices, hexagonal-architecture, architecture-tests]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Архитектура приложений

🌐 **Русский** · [English](overview.en.md)

## Коротко

Архитектура приложения — способ разделить код на части и договориться, кто от кого зависит, чтобы изменения были дешёвыми, а бизнес-логика проверялась без БД и сети. Основа — правило зависимостей: домен в центре и ничего не знает о внешнем мире, интерфейсы (порты) принадлежат ядру, а базы, веб и почта подключаются адаптерами. Поверх этого — богатая доменная модель, CQRS, вертикальные срезы и тактический DDD; на больших системах добавляются ограниченные контексты и надёжные события.

## Ключевые идеи

- **Слои:** представление → приложение → домен ← инфраструктура; зависимости направлены к домену.
- **Богатая модель:** агрегат защищает инварианты; объекты-значения — `record`/`readonly record struct`.
- **Порты и адаптеры:** интерфейс — у потребителя, реализация — во внешнем слое; корень композиции связывает.
- **CQRS:** команды меняют состояние, запросы читают; диспетчер и обработчики.
- **Вертикальные срезы:** код по фичам, а не по техническим ролям.
- **DDD:** единый язык, сущность, значение, агрегат, репозиторий, доменные события, bounded context.
- **Защита границ:** архитектурные тесты на ссылки между сборками.
- **Сложность по задаче:** CQRS/DDD/микросервисы — не для каждого CRUD.

## Минимальный пример

```csharp
var services = new ServiceCollection().AddApplication().AddInfrastructure();
using var provider = services.BuildServiceProvider();
var mediator = provider.GetRequiredService<Mediator>();

var id = await mediator.SendAsync(new PlaceOrderCommand("EUR", [("book", 2, 12.50m), ("pen", 3, 1.20m)]));
var summary = await mediator.SendAsync(new GetOrderSummaryQuery(id));
Console.WriteLine($"{summary!.Lines} lines, total {summary.Total.ToString(System.Globalization.CultureInfo.InvariantCulture)} {summary.Currency}");
// 2 lines, total 28.60 EUR
```

## Когда применять, а когда нет

- ✅ Долгоживущие системы со сложными бизнес-правилами и тестами.
- ✅ Команды, которым нужны чёткие границы и независимые изменения.
- ✅ Архитектурные тесты и корень композиции даже в небольших проектах.
- ❌ CQRS/DDD/слои для CRUD-формы — стоимость без выгоды.
- ❌ Универсальный `IRepository<T>` с `IQueryable` поверх EF Core.
- ❌ Микросервисы до появления реальной потребности.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | зачем архитектура, слои и правило зависимостей, богатая модель, value objects, корень композиции |
| [2. Продвинутая теория](advanced.ru.md) | Clean/Hexagonal, порты и адаптеры, CQRS и медиатор, срезы, основы DDD, доменные события, архитектурные тесты |
| [3. Экспертная теория](expert.ru.md) | bounded context и ACL, Outbox и согласованность, монолит против микросервисов, репозиторий и EF Core, подвохи, 12 вопросов с собеседований |
| [4. Самопроверка](quiz.ru.md) | 11 вопросов и [5 упражнений](exercises.ru.md) с решениями |
| 5. Запускаемые примеры | ещё не создан — `/learn csharp 21-architecture level 5` |
| 6. Тесты | ещё не создан — `/learn csharp 21-architecture level 6` |

## Связанные материалы

- Пререквизиты: [13 · DI, конфигурация, логирование](../13-di-config-logging/overview.ru.md), [14 · ASP.NET Core: Minimal API](../14-aspnet-core-minimal-api/overview.ru.md), [16 · Unit-тесты](../16-testing-unit/overview.ru.md)
- Данные: [15 · EF Core](../15-ef-core/overview.ru.md)
- Дальше: [22 · Наблюдаемость](../22-observability/overview.ru.md)
- То же в других треках: [Python](../../../python/overview.ru.md) — архитектура приложений
- Рецепты и библиотеки: [FluentValidation](../../libraries/fluentvalidation/overview.ru.md), [Polly](../../libraries/polly/overview.ru.md), [Testcontainers](../../libraries/testcontainers/overview.ru.md), [Фоновые задачи](../../recipes/background-jobs/overview.ru.md), [Кэширование](../../recipes/caching/overview.ru.md), [Загрузка файлов](../../recipes/file-upload/overview.ru.md), [gRPC-сервис](../../recipes/grpc-service/overview.ru.md), [Устойчивый HttpClient](../../recipes/http-client-resilience/overview.ru.md), [JWT-аутентификация](../../recipes/jwt-auth/overview.ru.md), [Пагинация и фильтрация](../../recipes/pagination-filtering/overview.ru.md), [Rate limiting](../../recipes/rate-limiting/overview.ru.md), [Real-time через SignalR](../../recipes/signalr-realtime/overview.ru.md)
- [System design: процесс проектирования](../../../system-design/modules/01-design-process/overview.ru.md)
- [System design: архитектурные стили](../../../system-design/modules/11-architecture-styles/overview.ru.md)
