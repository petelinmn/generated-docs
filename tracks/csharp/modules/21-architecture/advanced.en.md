---
title: "Application architecture: advanced"
lang: en
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

# Application architecture: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [basics](basics.en.md). **Time:** ~1.5 h.

## Clean Architecture and Hexagonal

Several names describe one idea: **the domain in the center, dependencies pointing inward**.

| Name | The essence |
|---|---|
| Layered | top to bottom: presentation → business logic → data; the business layer often depends on the data layer |
| Hexagonal (ports and adapters) | the application defines **ports** (interfaces), the outside world plugs in through **adapters** |
| Clean / Onion | concentric layers: entities → use cases → adapters → frameworks; dependencies only inward |

In "top-down" layered architecture the domain often depends on data access (EF Core). In Clean/Hexagonal the dependency is **inverted**: the application layer owns the interface `IOrderRepository` (a port), and infrastructure implements it (an adapter); this way the domain and use cases are tested without a database:

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

Verified: a command handler with a fake repository (`FakeRepo`) and publisher (`FakePublisher`) is tested without a database — the order was saved and the `OrderPlaced` event with the amount `10` was published.

## CQRS: commands and queries

**CQRS** (Command Query Responsibility Segregation) divides operations into **commands** (change state, return little) and **queries** (return data, change nothing). Usually each operation has its own request object and handler, and a dispatcher (mediator) finds the handler:

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

Handlers are registered in DI (`AddTransient<IRequestHandler<PlaceOrderCommand, OrderId>, PlaceOrderHandler>()`); the dispatcher picks the right one through `MakeGenericType` (a technique from [20 · Reflection and source generators](../20-reflection-and-source-generators/overview.en.md)). Verified: the command returned an `OrderId`, the query an `OrderSummary` with `Lines = 1`, and a query for a nonexistent order returned `null`. In real projects such a dispatcher is taken from libraries (MediatR and analogs), adding a pipeline of behaviors (validation, logging, transactions).

Why separate: queries can be optimized independently (reading through projections or a separate database), commands protected by domain invariants. The price is more types; in a simple CRUD this is excessive.

## Vertical slices

In layers, code is grouped **by technical role** (all controllers in one folder, all repositories in another). In **vertical slices** code is grouped **by capability**: a `PlaceOrder/` folder holds the request, handler, validation and endpoint; the layers inside a slice are chosen by complexity.

| Approach | Pros | Cons |
|---|---|---|
| Layers | familiar, uniform rules | changing one feature touches many folders |
| Vertical slices | a feature entirely in one place, slices are independent | possible duplication, discipline for shared code is needed |

The approaches combine: the domain is shared and rich, while use cases are organized as slices.

## DDD basics

**Domain-Driven Design** is an approach where the model is built around the problem domain together with experts. The main blocks:

| Block | Meaning | Example |
|---|---|---|
| Ubiquitous language | the same terms in conversations and in code | "place an order" = `Order.Place()` |
| Entity | identity, lifecycle | `Order` (`OrderId`) |
| Value object | equality by value, immutability | `Money` |
| Aggregate and aggregate root | a group of objects with shared invariants; changes go through the root | `Order` with `OrderLine` |
| Repository | access to aggregates (the interface lives in the domain/application) | `IOrderRepository` |
| Domain event | a fact that happened | `OrderPlaced` |
| Domain service | logic that does not belong to an entity | discount calculation across several aggregates |

An aggregate is a **consistency boundary**: rules inside it are checked immediately; between aggregates — eventual consistency (through events). Another aggregate is referenced by identifier, not by object.

## Domain events

An aggregate accumulates events (`Events`), and the application layer publishes them after saving; subscribers react (an email to the customer, updating a view). This keeps the domain independent of notification mechanisms. Verified: after `Place()` there is exactly one `OrderPlaced` in `Events`; the cancel handler publishes `OrderCancelled`, and cancelling an unplaced order throws "Only a placed order can be cancelled.".

## Architecture tests

Dependency rules are checked automatically so the architecture does not erode unnoticed:

```csharp
private static IEnumerable<string> RefsOf(Assembly a) => a.GetReferencedAssemblies().Select(r => r.Name!)
    .Where(n => !n.StartsWith("System") && n != "netstandard" && n != "mscorlib" && !n.StartsWith("Microsoft."));

[Fact] public void Domain_DependsOnNothingElse() => Assert.Empty(RefsOf(typeof(Order).Assembly));
[Fact] public void Application_DependsOnlyOnDomain() => Assert.Equal(["Domain"], RefsOf(typeof(Mediator).Assembly).Order());
[Fact] public void Infrastructure_DependsOnApplicationAndDomain() => Assert.Equal(["Application", "Domain"], RefsOf(typeof(InMemoryOrderRepository).Assembly).Order());
```

Verified: all three tests pass. The first run failed unexpectedly: using `dynamic` in `Mediator` added a reference to `Microsoft.CSharp` to `Application` — the test found it, which is why the filter excludes the whole `Microsoft.*` platform. For richer rules there are libraries (NetArchTest, ArchUnitNET) — checks on namespaces, inheritance, dependencies between types.

## Best practices

| Rule | Why |
|---|---|
| A domain without framework references | tests without infrastructure |
| Interfaces (ports) belong to the consumer | dependency inversion |
| Short handlers, logic in the domain | do not end up with an anemic model |
| Small aggregates | fewer locks and conflicts |
| One aggregate — one transaction | the consistency boundary |
| Architecture tests in CI | rules are not broken silently |
| Complexity as needed | CQRS/DDD are not needed for CRUD |

## Related materials

- Back: [basics](basics.en.md). Next: [expert theory](expert.en.md) — bounded context, outbox, monolith and microservices.
- [13 · DI, configuration, logging](../13-di-config-logging/overview.en.md) — registering handlers.
- [16 · Unit tests](../16-testing-unit/overview.en.md) — tests with a fake repository.
- [15 · EF Core](../15-ef-core/overview.en.md) — implementing a repository and unit of work.
- [20 · Reflection and source generators](../20-reflection-and-source-generators/overview.en.md) — generators instead of a dynamic dispatcher.

## Sources

- [Microsoft: a DDD-oriented microservice](https://learn.microsoft.com/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/ddd-oriented-microservice)
- [The CQRS pattern](https://learn.microsoft.com/azure/architecture/patterns/cqrs)
- [Clean Architecture with ASP.NET Core](https://learn.microsoft.com/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures#clean-architecture)
- [NetArchTest](https://github.com/BenMorris/NetArchTest)
