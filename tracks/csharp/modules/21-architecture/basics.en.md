---
title: "Application architecture: basics"
lang: en
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

# Application architecture: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [05 · Interfaces, structs, records](../05-interfaces-structs-records/overview.en.md), [13 · DI, configuration, logging](../13-di-config-logging/overview.en.md), [14 · ASP.NET Core: Minimal API](../14-aspnet-core-minimal-api/overview.en.md). **Time:** ~1 h.

## Why it matters

An application's architecture is a way of dividing code into parts and agreeing on who depends on whom. The goal is to make **changes cheap**: switch the database, add a delivery channel, test business logic without a network or a database. Bad architecture shows up as "I changed one place — three broke", tests that need the whole system, and business rules scattered across controllers and SQL. Architecture is not an end in itself: a small project needs a simple structure, and complex schemes pay off only on large, long-lived systems.

## Layers

The classic division into layers:

| Layer | Responsibility | Example |
|---|---|---|
| Presentation | accept a request, return a response | Minimal API, console, UI |
| Application | use cases: "place an order" | command and query handlers |
| Domain | business rules and state | `Order`, `Money`, events |
| Infrastructure | database, mail, files, network | a repository, sending emails |

The main rule is the **dependency rule**: dependencies point inward, toward the domain. The domain knows nothing about the database or the web. The application knows the domain but not a concrete database. Infrastructure and presentation depend on the inner layers, not the other way round. For the inversion to work, the application declares **interfaces** (`IOrderRepository`) and infrastructure implements them ([13 · DI, configuration, logging](../13-di-config-logging/overview.en.md)).

In .NET layers are usually separate projects (assemblies): `Domain`, `Application`, `Infrastructure`, `App`. The references between projects are the physical embodiment of the rule: `Domain` references nobody, `Application` — `Domain`, `Infrastructure` — `Application`. Verified on the teaching solution: the `Domain` assembly references nothing but the platform; `Application` — only `Domain`; `Infrastructure` — `Application` and `Domain`.

## The domain: a rich model

Business rules must live in domain objects, not in services or controllers, otherwise an object turns into a "bag of data" (an anemic model). An entity protects its **invariants** — conditions that must always hold:

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

The state is closed (`private set`, `IReadOnlyList`), and changes go only through methods that check the rules. Verified by tests: an empty order cannot be placed, lines cannot be added after placing, a line's currency must match the order's currency; placing the amounts `12.50 × 2 + 1.20 × 3` gives `28.60`.

A **value object** is a type without identity, equal by value and immutable: money, an address, a date range. In C# it is a `record` or `readonly record struct` ([05 · Interfaces, structs, records](../05-interfaces-structs-records/overview.en.md)):

```csharp
public readonly record struct Money(decimal Amount, string Currency)
{
    public static Money Zero(string currency) => new(0m, currency);
    public static Money operator +(Money a, Money b) =>
        a.Currency == b.Currency ? new(a.Amount + b.Amount, a.Currency) : throw new InvalidOperationException("Currency mismatch.");
    public static Money operator *(Money m, int q) => new(m.Amount * q, m.Currency);
}
```

`new Money(5, "EUR") == new Money(5, "EUR")` is `True` (verified): the value is compared, not the reference. An **entity** is a type with identity (`OrderId`): two orders with identical contents are different orders.

## The application and the composition root

The application layer holds the use cases: "place an order" creates an `Order`, fills it, saves it through a repository interface and publishes events. It does not know where data is stored. The concrete classes are tied together by the **composition root** — the single place (usually `Program.cs`) where dependencies are registered:

```csharp
var services = new ServiceCollection().AddApplication().AddInfrastructure();
using var provider = services.BuildServiceProvider();
var mediator = provider.GetRequiredService<Mediator>();

var id = await mediator.SendAsync(new PlaceOrderCommand("EUR", [("book", 2, 12.50m), ("pen", 3, 1.20m)]));
var summary = await mediator.SendAsync(new GetOrderSummaryQuery(id));
Console.WriteLine($"{summary!.Lines} lines, total {summary.Total.ToString(System.Globalization.CultureInfo.InvariantCulture)} {summary.Currency}");
// 2 lines, total 28.60 EUR
```

Verified by running: the output is `2 lines, total 28.60 EUR`, the `OrderPlaced` event was published, and an order without lines was rejected with the message `Order is empty.`. The detailed layer code is in the [advanced theory](advanced.en.md).

## Beginner mistakes

- ❌ Business rules in controllers and SQL → ✅ in domain objects.
- ❌ The domain references EF Core or web types → ✅ the domain depends only on the platform.
- ❌ Public `set` on every property → ✅ closed state and methods with checks.
- ❌ Layers "for tidiness" in a three-screen project → ✅ complexity in proportion to the task.
- ❌ `new` of infrastructure classes inside the application → ✅ interfaces and dependency injection.
- ❌ Microservices right away → ✅ a well-separated monolith first.

## Related materials

- Next: [advanced theory](advanced.en.md) — Clean/Hexagonal, CQRS, slices, DDD, architecture tests.
- [13 · DI, configuration, logging](../13-di-config-logging/overview.en.md) — the composition root.
- [05 · Interfaces, structs, records](../05-interfaces-structs-records/overview.en.md) — records and value objects.
- [14 · ASP.NET Core: Minimal API](../14-aspnet-core-minimal-api/overview.en.md) — the presentation layer.
- [Python](../../../python/overview.en.md) — layered architecture and DDD in Python.

## Sources

- [Common architectural principles for .NET](https://learn.microsoft.com/dotnet/architecture/modern-web-apps-azure/architectural-principles)
- [Common web application architectures](https://learn.microsoft.com/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures)
- [A DDD-oriented microservice](https://learn.microsoft.com/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/)
