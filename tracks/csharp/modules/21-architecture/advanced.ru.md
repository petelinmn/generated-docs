---
title: "Архитектура приложений: продвинутая теория"
lang: ru
track: csharp
kind: module
doc: advanced
slug: 21-architecture
part: 3
tags: [clean-architecture, hexagonal-architecture, cqrs, mediator, vertical-slices, ddd, architecture-tests, domain-events]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Архитектура приложений: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [базовая теория](basics.ru.md). **Время:** ~1,5 ч.

## Clean Architecture и Hexagonal

Несколько названий описывают одну идею: **домен в центре, зависимости направлены внутрь**.

| Название | Суть |
|---|---|
| Layered (слоистая) | сверху вниз: представление → бизнес-логика → данные; часто бизнес-слой зависит от слоя данных |
| Hexagonal (порты и адаптеры) | приложение определяет **порты** (интерфейсы), внешний мир подключается **адаптерами** |
| Clean / Onion | концентрические слои: сущности → сценарии → адаптеры → фреймворки; зависимости только внутрь |

В слоистой архитектуре «сверху вниз» домен часто зависит от доступа к данным (EF Core). В Clean/Hexagonal зависимость **инвертирована**: слой приложения владеет интерфейсом `IOrderRepository` (порт), а инфраструктура реализует его (адаптер); так домен и сценарии тестируются без БД:

```csharp
// Application: the port
public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken ct);
    Task<Order?> FindAsync(OrderId id, CancellationToken ct);
}

// Infrastructure: the adapter
public sealed class InMemoryOrderRepository : IOrderRepository
{
    private readonly Dictionary<OrderId, Order> _store = new();
    public Task AddAsync(Order order, CancellationToken ct) { _store[order.Id] = order; return Task.CompletedTask; }
    public Task<Order?> FindAsync(OrderId id, CancellationToken ct) => Task.FromResult(_store.GetValueOrDefault(id));
}
```

Проверено: обработчик команды с поддельным репозиторием (`FakeRepo`) и публикатором (`FakePublisher`) тестируется без БД — заказ сохранён, опубликовано событие `OrderPlaced` с суммой `10`.

## CQRS: команды и запросы

**CQRS** (Command Query Responsibility Segregation) разделяет операции на **команды** (меняют состояние, возвращают минимум) и **запросы** (возвращают данные, ничего не меняют). Обычно для каждой операции свой запрос-объект и обработчик, а диспетчер (mediator) находит обработчик:

```csharp
public interface IRequest<TResponse> { }
public interface IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    Task<TResponse> HandleAsync(TRequest request, CancellationToken ct);
}

public sealed class Mediator(IServiceProvider sp)
{
    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken ct = default)
    {
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
        dynamic handler = sp.GetRequiredService(handlerType);
        return handler.HandleAsync((dynamic)request, ct);
    }
}

// Command: changes state
public record PlaceOrderCommand(string Currency, IReadOnlyList<(string Sku, int Qty, decimal Price)> Lines) : IRequest<OrderId>;
public sealed class PlaceOrderHandler(IOrderRepository repo, IEventPublisher events) : IRequestHandler<PlaceOrderCommand, OrderId>
{
    public async Task<OrderId> HandleAsync(PlaceOrderCommand cmd, CancellationToken ct)
    {
        var order = new Order(cmd.Currency);
        foreach (var l in cmd.Lines) order.AddLine(l.Sku, l.Qty, new Money(l.Price, cmd.Currency));
        order.Place();
        await repo.AddAsync(order, ct);
        foreach (var e in order.Events) await events.PublishAsync(e, ct);
        return order.Id;
    }
}

// Query: returns data and changes nothing
public record GetOrderSummaryQuery(OrderId Id) : IRequest<OrderSummary?>;
public record OrderSummary(Guid Id, int Lines, decimal Total, string Currency);
public sealed class GetOrderSummaryHandler(IOrderRepository repo) : IRequestHandler<GetOrderSummaryQuery, OrderSummary?>
{
    public async Task<OrderSummary?> HandleAsync(GetOrderSummaryQuery q, CancellationToken ct) =>
        await repo.FindAsync(q.Id, ct) is { } o ? new OrderSummary(o.Id.Value, o.Lines.Count, o.Total.Amount, o.Currency) : null;
}
```

Обработчики регистрируют в DI (`AddTransient<IRequestHandler<PlaceOrderCommand, OrderId>, PlaceOrderHandler>()`); диспетчер выбирает нужный через `MakeGenericType` (приём из [20 · Рефлексия и генераторы кода](../20-reflection-and-source-generators/overview.ru.md)). Проверено: команда вернула `OrderId`, запрос — `OrderSummary` с `Lines = 1`, а запрос несуществующего заказа — `null`. В реальных проектах такой диспетчер берут из библиотек (MediatR и аналоги), добавляя конвейер поведений (валидация, логирование, транзакции).

Зачем разделять: запросы можно оптимизировать независимо (чтение через проекции или отдельную БД), команды — защищать инвариантами домена. Цена — больше типов; в простом CRUD это избыточно.

## Вертикальные срезы

В слоях код группируют **по технической роли** (все контроллеры в одной папке, все репозитории — в другой). Во **vertical slices** код группируют **по возможности**: папка `PlaceOrder/` содержит запрос, обработчик, валидацию и конечную точку; слои внутри среза выбираются по сложности.

| Подход | Плюсы | Минусы |
|---|---|---|
| Слои | привычно, единые правила | изменение одной фичи затрагивает много папок |
| Вертикальные срезы | фича целиком в одном месте, срезы независимы | возможное дублирование, нужна дисциплина общего кода |

Подходы сочетаются: домен общий и богатый, а сценарии организованы срезами.

## Основы DDD

**Domain-Driven Design** — подход, где модель строится вокруг предметной области вместе с экспертами. Основные блоки:

| Блок | Смысл | Пример |
|---|---|---|
| Единый язык (ubiquitous language) | одни термины в разговорах и в коде | «оформить заказ» = `Order.Place()` |
| Сущность | идентичность, жизненный цикл | `Order` (`OrderId`) |
| Объект-значение | равенство по значению, неизменяемость | `Money` |
| Агрегат и корень агрегата | группа объектов с общими инвариантами; изменения — через корень | `Order` с `OrderLine` |
| Репозиторий | доступ к агрегатам (интерфейс в домене/приложении) | `IOrderRepository` |
| Доменное событие | факт, который произошёл | `OrderPlaced` |
| Доменная служба | логика, не принадлежащая сущности | расчёт скидки по нескольким агрегатам |

Агрегат — **граница согласованности**: правила внутри него проверяются сразу; между агрегатами — согласованность в конечном счёте (через события). Ссылаются на другой агрегат по идентификатору, а не по объекту.

## Доменные события

Агрегат накапливает события (`Events`), а слой приложения публикует их после сохранения; подписчики реагируют (письмо клиенту, обновление витрины). Так домен остаётся независимым от способов уведомления. Проверено: после `Place()` в `Events` ровно одно `OrderPlaced`; обработчик отмены публикует `OrderCancelled`, а отмена непринятого заказа — исключение «Only a placed order can be cancelled.».

## Архитектурные тесты

Правила зависимостей проверяют автоматически, чтобы архитектура не разрушалась незаметно:

```csharp
private static IEnumerable<string> RefsOf(Assembly a) => a.GetReferencedAssemblies().Select(r => r.Name!)
    .Where(n => !n.StartsWith("System") && n != "netstandard" && n != "mscorlib" && !n.StartsWith("Microsoft."));

[Fact] public void Domain_DependsOnNothingElse() => Assert.Empty(RefsOf(typeof(Order).Assembly));
[Fact] public void Application_DependsOnlyOnDomain() => Assert.Equal(["Domain"], RefsOf(typeof(Mediator).Assembly).Order());
[Fact] public void Infrastructure_DependsOnApplicationAndDomain() => Assert.Equal(["Application", "Domain"], RefsOf(typeof(InMemoryOrderRepository).Assembly).Order());
```

Проверено: все три теста проходят. Первый запуск упал неожиданным образом: использование `dynamic` в `Mediator` добавило в `Application` ссылку на `Microsoft.CSharp` — тест её нашёл и потому в фильтре исключают всю платформу `Microsoft.*`. Для более богатых правил существуют библиотеки (NetArchTest, ArchUnitNET) — проверка пространств имён, наследования, зависимостей между типами.

## Лучшие практики

| Правило | Почему |
|---|---|
| Домен без ссылок на фреймворки | тесты без инфраструктуры |
| Интерфейсы (порты) — у потребителя | инверсия зависимостей |
| Короткие обработчики, логика в домене | не получить анемичную модель |
| Агрегаты небольшие | меньше блокировок и конфликтов |
| Один агрегат — одна транзакция | граница согласованности |
| Архитектурные тесты в CI | правила не нарушаются тихо |
| Сложность по необходимости | CQRS/DDD для CRUD не нужны |

## Связанные материалы

- Назад: [базовая теория](basics.ru.md). Дальше: [экспертная теория](expert.ru.md) — bounded context, outbox, монолит и микросервисы.
- [13 · DI, конфигурация, логирование](../13-di-config-logging/overview.ru.md) — регистрация обработчиков.
- [16 · Unit-тесты](../16-testing-unit/overview.ru.md) — тесты с поддельным репозиторием.
- [15 · EF Core](../15-ef-core/overview.ru.md) — реализация репозитория и единицы работы.
- [20 · Рефлексия и генераторы кода](../20-reflection-and-source-generators/overview.ru.md) — генераторы вместо динамического диспетчера.

## Источники

- [Microsoft: микросервис в стиле DDD](https://learn.microsoft.com/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/ddd-oriented-microservice)
- [Паттерн CQRS](https://learn.microsoft.com/azure/architecture/patterns/cqrs)
- [Clean Architecture с ASP.NET Core](https://learn.microsoft.com/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures#clean-architecture)
- [NetArchTest](https://github.com/BenMorris/NetArchTest)
