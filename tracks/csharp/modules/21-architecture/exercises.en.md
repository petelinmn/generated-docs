---
title: "Exercises: Application architecture"
lang: en
track: csharp
kind: module
doc: exercises
slug: 21-architecture
part: 3
tags: [architecture, value-object, aggregate, ports-and-adapters, architecture-tests, cqrs]
status: draft
updated: 2026-11-22
---

# Exercises: Application architecture

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. The exercises build on the teaching solution from the [basics](basics.en.md) and [advanced](advanced.en.md) theory: projects `Domain`, `Application`, `Infrastructure`, `App`, `Tests` (xUnit v3). All solutions were verified by running them: 12 tests passed. There are no runnable versions in `code/` yet (level 5 does not exist).

## 1. 🟢 The Money value object

**Task:** write `readonly record struct Money(decimal Amount, string Currency)` with addition (the same currency, otherwise `InvalidOperationException`), multiplication by an `int` and `Zero(currency)`. Check value equality.

**Expected result:**

```text
new Money(5, "EUR") == new Money(5, "EUR") → True
Money(1, "EUR") + Money(1, "USD")          → InvalidOperationException
```

<details><summary>Hint</summary>

A `record struct` gives value equality automatically. Overload `operator +` and `operator *`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 1: a value object with behavior and an invariant.
public readonly record struct Money(decimal Amount, string Currency)
{
    public static Money Zero(string currency) => new(0m, currency);
    public static Money operator +(Money a, Money b) =>
        a.Currency == b.Currency ? new(a.Amount + b.Amount, a.Currency) : throw new InvalidOperationException("Currency mismatch.");
    public static Money operator *(Money m, int q) => new(m.Amount * q, m.Currency);
}

[Fact] public void Money_Equality_IsByValue() => Assert.Equal(new Money(5, "EUR"), new Money(5, "EUR"));
```

Verified: the test passes.

</details>

## 2. 🟢 Aggregate invariants

**Task:** for the `Order` aggregate write tests: placing two lines gives the total `28.60` and exactly one `OrderPlaced` in `Events`; an empty order cannot be placed; lines cannot be added after placing; a line's currency must match the order's currency.

**Expected result:** 4 tests pass.

<details><summary>Hint</summary>

`Assert.Throws<InvalidOperationException>(...)`, `Assert.IsType<OrderPlaced>(Assert.Single(o.Events))`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 2: the aggregate protects its own invariants.
public class OrderTests
{
    [Fact]
    public void Place_WithLines_RaisesEventAndComputesTotal()
    {
        var o = new Order("EUR");
        o.AddLine("book", 2, new Money(12.50m, "EUR"));
        o.AddLine("pen", 3, new Money(1.20m, "EUR"));
        o.Place();
        Assert.Equal(28.60m, o.Total.Amount);
        Assert.IsType<OrderPlaced>(Assert.Single(o.Events));
    }

    [Fact] public void Place_Empty_Throws() => Assert.Throws<InvalidOperationException>(() => new Order("EUR").Place());

    [Fact]
    public void AddLine_AfterPlace_Throws()
    {
        var o = new Order("EUR"); o.AddLine("a", 1, new Money(1, "EUR")); o.Place();
        Assert.Throws<InvalidOperationException>(() => o.AddLine("b", 1, new Money(1, "EUR")));
    }

    [Fact] public void AddLine_WrongCurrency_Throws() =>
        Assert.Throws<InvalidOperationException>(() => new Order("EUR").AddLine("a", 1, new Money(1, "USD")));
}
```

Verified: all four tests passed.

</details>

## 3. 🟡 A port and an adapter: testing a handler without a database

**Task:** implement fake `FakeRepo : IOrderRepository` and `FakePublisher : IEventPublisher` and check that `PlaceOrderHandler` saves the order and publishes `OrderPlaced` with the total `10`.

**Expected result:** `repo.Added` holds one order with the returned `id`; `pub.Events` holds one `OrderPlaced` event with `Total.Amount == 10`.

<details><summary>Hint</summary>

Create `new PlaceOrderHandler(repo, pub)` and call `HandleAsync(new PlaceOrderCommand("EUR", [("a", 2, 5m)]), ct)`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 3: the handler depends on ports, so it is testable with fakes.
public class FakeRepo : IOrderRepository
{
    public List<Order> Added = new();
    public Task AddAsync(Order o, CancellationToken ct) { Added.Add(o); return Task.CompletedTask; }
    public Task<Order?> FindAsync(OrderId id, CancellationToken ct) => Task.FromResult(Added.FirstOrDefault(o => o.Id == id));
}
public class FakePublisher : IEventPublisher
{
    public List<DomainEvent> Events = new();
    public Task PublishAsync(DomainEvent e, CancellationToken ct) { Events.Add(e); return Task.CompletedTask; }
}

[Fact]
public async Task PlaceOrder_StoresOrderAndPublishesEvent()
{
    var repo = new FakeRepo(); var pub = new FakePublisher();
    var id = await new PlaceOrderHandler(repo, pub).HandleAsync(new PlaceOrderCommand("EUR", [("a", 2, 5m)]), TestContext.Current.CancellationToken);
    Assert.Single(repo.Added);
    Assert.Equal(id, repo.Added[0].Id);
    var placed = Assert.IsType<OrderPlaced>(Assert.Single(pub.Events));
    Assert.Equal(10m, placed.Total.Amount);
}
```

Verified: the test passes, no database needed.

</details>

## 4. 🟡 Architecture tests

**Task:** write three tests: `Domain` references nothing but the platform; `Application` — only `Domain`; `Infrastructure` — `Application` and `Domain`.

**Expected result:** 3 tests pass; if a `Domain → Infrastructure` reference were added, the first test would fail.

<details><summary>Hint</summary>

`assembly.GetReferencedAssemblies()`; filter out the names `System*`, `Microsoft.*`, `netstandard`, `mscorlib`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 4: dependency rules as tests.
public class ArchitectureTests
{
    private static IEnumerable<string> RefsOf(Assembly a) => a.GetReferencedAssemblies().Select(r => r.Name!)
        .Where(n => !n.StartsWith("System") && n != "netstandard" && n != "mscorlib" && !n.StartsWith("Microsoft."));

    [Fact] public void Domain_DependsOnNothingElse() => Assert.Empty(RefsOf(typeof(Order).Assembly));
    [Fact] public void Application_DependsOnlyOnDomain() => Assert.Equal(["Domain"], RefsOf(typeof(Mediator).Assembly).Order());
    [Fact] public void Infrastructure_DependsOnApplicationAndDomain() => Assert.Equal(["Application", "Domain"], RefsOf(typeof(InMemoryOrderRepository).Assembly).Order());
}
```

Verified: the three tests pass. The first version of the filter excluded only `Microsoft.Extensions*`, and the test `Application_DependsOnlyOnDomain` failed because of a reference to `Microsoft.CSharp` (it appeared because of `dynamic` in `Mediator`) — which is the benefit of architecture tests.

</details>

## 5. 🔴 A new command: cancelling an order

**Task:** add cancellation: in the `Order` aggregate — `Cancel(reason)` (only a placed and not yet cancelled order; the event `OrderCancelled`), a command `CancelOrderCommand(OrderId, Reason)` and a handler that returns `true` if the order is found and `false` if not. Register the handler in DI and check through the dispatcher.

**Expected result:**

```text
cancelling a placed order   → true, OrderCancelled published
cancelling a nonexistent one → false
Cancel on an unplaced one   → InvalidOperationException
```

<details><summary>Hint</summary>

The rules live in the domain (`Cancel` throws exceptions); the handler just finds the aggregate, calls the method and publishes `order.Events[^1]`. Registration: `AddTransient<IRequestHandler<CancelOrderCommand, bool>, CancelOrderHandler>()`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 5: a new use case — the rules stay in the domain, the handler orchestrates.
// Domain
public record OrderCancelled(OrderId OrderId, string Reason) : DomainEvent;
public bool IsCancelled { get; private set; }
public void Cancel(string reason)
{
    if (!IsPlaced) throw new InvalidOperationException("Only a placed order can be cancelled.");
    if (IsCancelled) throw new InvalidOperationException("Order is already cancelled.");
    IsCancelled = true;
    _events.Add(new OrderCancelled(Id, reason));
}

// Application
public record CancelOrderCommand(OrderId Id, string Reason) : IRequest<bool>;
public sealed class CancelOrderHandler(IOrderRepository repo, IEventPublisher events) : IRequestHandler<CancelOrderCommand, bool>
{
    public async Task<bool> HandleAsync(CancelOrderCommand cmd, CancellationToken ct)
    {
        var order = await repo.FindAsync(cmd.Id, ct);
        if (order is null) return false;
        order.Cancel(cmd.Reason);
        await events.PublishAsync(order.Events[^1], ct);
        return true;
    }
}
```

Verified: the tests for cancelling a placed order (`true`, the `OrderCancelled` event), a nonexistent one (`false`), an unplaced one (`InvalidOperationException`) and routing through `Mediator` pass.

</details>
