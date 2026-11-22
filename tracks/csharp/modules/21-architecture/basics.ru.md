---
title: "Архитектура приложений: базовая теория"
lang: ru
track: csharp
kind: module
doc: basics
slug: 21-architecture
part: 3
tags: [architecture, layered-architecture, dependency-rule, domain-model, composition-root, separation-of-concerns]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Архитектура приложений: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [05 · Интерфейсы, структуры, записи](../05-interfaces-structs-records/overview.ru.md), [13 · DI, конфигурация, логирование](../13-di-config-logging/overview.ru.md), [14 · ASP.NET Core: Minimal API](../14-aspnet-core-minimal-api/overview.ru.md). **Время:** ~1 ч.

## Зачем это нужно

Архитектура приложения — это способ разделить код на части и договориться, кто от кого зависит. Цель — сделать **изменения дешёвыми**: сменить базу данных, добавить способ доставки, протестировать бизнес-логику без сети и БД. Плохая архитектура проявляется как «изменил в одном месте — сломалось в трёх», тесты, которым нужна вся система, и бизнес-правила, разбросанные по контроллерам и SQL. Архитектура — не самоцель: в маленьком проекте достаточно простой структуры, а сложные схемы окупаются только на больших и долгоживущих системах.

## Слои

Классическое разделение на слои:

| Слой | Ответственность | Пример |
|---|---|---|
| Представление (Presentation) | принять запрос, вернуть ответ | Minimal API, консоль, UI |
| Приложение (Application) | сценарии использования: «оформить заказ» | обработчики команд и запросов |
| Домен (Domain) | бизнес-правила и состояние | `Order`, `Money`, события |
| Инфраструктура (Infrastructure) | БД, почта, файлы, сеть | репозиторий, отправка писем |

Главное правило — **правило зависимостей**: зависимости направлены внутрь, к домену. Домен не знает ни про БД, ни про веб. Приложение знает про домен, но не про конкретную БД. Инфраструктура и представление зависят от внутренних слоёв, а не наоборот. Чтобы инверсия работала, приложение объявляет **интерфейсы** (`IOrderRepository`), а инфраструктура их реализует ([13 · DI, конфигурация, логирование](../13-di-config-logging/overview.ru.md)).

В .NET слои обычно — отдельные проекты (сборки): `Domain`, `Application`, `Infrastructure`, `App`. Ссылки между проектами и есть физическое воплощение правила: `Domain` не ссылается ни на кого, `Application` — на `Domain`, `Infrastructure` — на `Application`. Проверено на учебном решении: сборка `Domain` не ссылается ни на что, кроме платформы; `Application` — только на `Domain`; `Infrastructure` — на `Application` и `Domain`.

## Домен: богатая модель

Бизнес-правила должны жить в доменных объектах, а не в сервисах или контроллерах, иначе объект превращается в «мешок данных» (анемичная модель). Сущность защищает свои **инварианты** — условия, которые должны быть истинны всегда:

```csharp
public sealed class Order
{
    private readonly List<OrderLine> _lines = new();
    private readonly List<DomainEvent> _events = new();

    public OrderId Id { get; } = OrderId.New();
    public string Currency { get; }
    public bool IsPlaced { get; private set; }
    public IReadOnlyList<OrderLine> Lines => _lines;
    public IReadOnlyList<DomainEvent> Events => _events;

    public Order(string currency) => Currency = currency;

    public Money Total => _lines.Aggregate(Money.Zero(Currency), (sum, l) => sum + l.Total);

    public void AddLine(string sku, int quantity, Money unitPrice)
    {
        if (IsPlaced) throw new InvalidOperationException("Order is already placed.");
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        if (unitPrice.Currency != Currency) throw new InvalidOperationException("Currency mismatch.");
        _lines.Add(new OrderLine(sku, quantity, unitPrice));
    }

    public void Place()
    {
        if (IsPlaced) throw new InvalidOperationException("Order is already placed.");
        if (_lines.Count == 0) throw new InvalidOperationException("Order is empty.");
        IsPlaced = true;
        _events.Add(new OrderPlaced(Id, Total));
    }
}
```

Состояние закрыто (`private set`, `IReadOnlyList`), изменения идут только через методы, которые проверяют правила. Проверено тестами: пустой заказ оформить нельзя, после оформления строки добавить нельзя, валюта строки должна совпадать с валютой заказа; оформление сумм `12.50 × 2 + 1.20 × 3` даёт `28.60`.

**Объект-значение (value object)** — тип без идентичности, равный по значению, неизменяемый: деньги, адрес, диапазон дат. В C# это `record` или `readonly record struct` ([05 · Интерфейсы, структуры, записи](../05-interfaces-structs-records/overview.ru.md)):

```csharp
public readonly record struct Money(decimal Amount, string Currency)
{
    public static Money Zero(string currency) => new(0m, currency);
    public static Money operator +(Money a, Money b) =>
        a.Currency == b.Currency ? new(a.Amount + b.Amount, a.Currency) : throw new InvalidOperationException("Currency mismatch.");
    public static Money operator *(Money m, int q) => new(m.Amount * q, m.Currency);
}
```

`new Money(5, "EUR") == new Money(5, "EUR")` — `True` (проверено): сравнивается значение, а не ссылка. **Сущность (entity)** — тип с идентичностью (`OrderId`): два заказа с одинаковым содержимым — разные заказы.

## Приложение и точка сборки

Слой приложения содержит сценарии: «разместить заказ» создаёт `Order`, наполняет его, сохраняет через интерфейс репозитория и публикует события. Он не знает, где хранятся данные. Конкретные классы связывает **корень композиции** (composition root) — единственное место (обычно `Program.cs`), где регистрируются зависимости:

```csharp
var services = new ServiceCollection().AddApplication().AddInfrastructure();
using var provider = services.BuildServiceProvider();
var mediator = provider.GetRequiredService<Mediator>();

var id = await mediator.SendAsync(new PlaceOrderCommand("EUR", [("book", 2, 12.50m), ("pen", 3, 1.20m)]));
var summary = await mediator.SendAsync(new GetOrderSummaryQuery(id));
Console.WriteLine($"{summary!.Lines} lines, total {summary.Total.ToString(System.Globalization.CultureInfo.InvariantCulture)} {summary.Currency}");
// 2 lines, total 28.60 EUR
```

Проверено запуском: вывод `2 lines, total 28.60 EUR`, событие `OrderPlaced` опубликовано, а заказ без строк отклонён сообщением `Order is empty.`. Подробный код слоёв — в [продвинутой теории](advanced.ru.md).

## Ошибки новичка

- ❌ Бизнес-правила в контроллерах и SQL → ✅ в доменных объектах.
- ❌ Домен ссылается на EF Core или веб-типы → ✅ домен зависит только от платформы.
- ❌ Публичные `set` у всех свойств → ✅ закрытое состояние и методы с проверками.
- ❌ Слои «для порядка» в проекте из трёх экранов → ✅ сложность по размеру задачи.
- ❌ `new` инфраструктурных классов внутри приложения → ✅ интерфейсы и внедрение зависимостей.
- ❌ Сразу микросервисы → ✅ сначала хорошо разделённый монолит.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — Clean/Hexagonal, CQRS, срезы, DDD, архитектурные тесты.
- [13 · DI, конфигурация, логирование](../13-di-config-logging/overview.ru.md) — корень композиции.
- [05 · Интерфейсы, структуры, записи](../05-interfaces-structs-records/overview.ru.md) — записи и value objects.
- [14 · ASP.NET Core: Minimal API](../14-aspnet-core-minimal-api/overview.ru.md) — слой представления.
- [Python](../../../python/overview.ru.md) — слоистая архитектура и DDD на Python.

## Источники

- [Общие архитектурные принципы для .NET](https://learn.microsoft.com/dotnet/architecture/modern-web-apps-azure/architectural-principles)
- [Типичные архитектуры веб-приложений](https://learn.microsoft.com/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures)
- [DDD-ориентированный микросервис](https://learn.microsoft.com/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/)
