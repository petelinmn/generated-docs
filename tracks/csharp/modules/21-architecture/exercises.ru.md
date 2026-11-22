---
title: "Упражнения: Архитектура приложений"
lang: ru
track: csharp
kind: module
doc: exercises
slug: 21-architecture
part: 3
tags: [architecture, value-object, aggregate, ports-and-adapters, architecture-tests, cqrs]
status: draft
updated: 2026-11-22
---

# Упражнения: Архитектура приложений

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Упражнения опираются на учебное решение из [базовой](basics.ru.md) и [продвинутой](advanced.ru.md) теории: проекты `Domain`, `Application`, `Infrastructure`, `App`, `Tests` (xUnit v3). Все решения проверены запуском: 12 тестов пройдено. Запускаемых версий в `code/` пока нет (уровень 5 не создан).

## 1. 🟢 Объект-значение Money

**Задача:** напиши `readonly record struct Money(decimal Amount, string Currency)` со сложением (одинаковая валюта, иначе `InvalidOperationException`), умножением на `int` и `Zero(currency)`. Проверь равенство по значению.

**Ожидаемый результат:**

```text
new Money(5, "EUR") == new Money(5, "EUR") → True
Money(1, "EUR") + Money(1, "USD")          → InvalidOperationException
```

<details><summary>Подсказка</summary>

`record struct` даёт равенство по значению автоматически. Перегрузи `operator +` и `operator *`.

</details>

<details><summary>Решение</summary>

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

Проверено: тест проходит.

</details>

## 2. 🟢 Инварианты агрегата

**Задача:** для агрегата `Order` напиши тесты: оформление двух строк даёт сумму `28.60`, в `Events` ровно одно `OrderPlaced`; пустой заказ оформить нельзя; после оформления строки добавлять нельзя; валюта строки должна совпадать с валютой заказа.

**Ожидаемый результат:** 4 теста проходят.

<details><summary>Подсказка</summary>

`Assert.Throws<InvalidOperationException>(...)`, `Assert.IsType<OrderPlaced>(Assert.Single(o.Events))`.

</details>

<details><summary>Решение</summary>

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

Проверено: все четыре теста пройдены.

</details>

## 3. 🟡 Порт и адаптер: тест обработчика без БД

**Задача:** реализуй поддельные `FakeRepo : IOrderRepository` и `FakePublisher : IEventPublisher` и проверь, что `PlaceOrderHandler` сохраняет заказ и публикует `OrderPlaced` с суммой `10`.

**Ожидаемый результат:** в `repo.Added` один заказ с возвращённым `id`; в `pub.Events` одно событие `OrderPlaced` с `Total.Amount == 10`.

<details><summary>Подсказка</summary>

Создай `new PlaceOrderHandler(repo, pub)` и вызови `HandleAsync(new PlaceOrderCommand("EUR", [("a", 2, 5m)]), ct)`.

</details>

<details><summary>Решение</summary>

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

Проверено: тест проходит, БД не нужна.

</details>

## 4. 🟡 Архитектурные тесты

**Задача:** напиши три теста: `Domain` не ссылается ни на что, кроме платформы; `Application` — только на `Domain`; `Infrastructure` — на `Application` и `Domain`.

**Ожидаемый результат:** 3 теста проходят; при добавлении ссылки `Domain → Infrastructure` первый тест упал бы.

<details><summary>Подсказка</summary>

`assembly.GetReferencedAssemblies()`; отфильтруй имена `System*`, `Microsoft.*`, `netstandard`, `mscorlib`.

</details>

<details><summary>Решение</summary>

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

Проверено: три теста проходят. Первая версия фильтра отсекала только `Microsoft.Extensions*`, и тест `Application_DependsOnlyOnDomain` упал из-за ссылки на `Microsoft.CSharp` (она появилась из-за `dynamic` в `Mediator`) — это и есть польза архитектурных тестов.

</details>

## 5. 🔴 Новая команда: отмена заказа

**Задача:** добавь отмену: в агрегат `Order` — `Cancel(reason)` (только оформленный и ещё не отменённый заказ; событие `OrderCancelled`), команду `CancelOrderCommand(OrderId, Reason)` и обработчик, возвращающий `true`, если заказ найден, и `false`, если нет. Зарегистрируй обработчик в DI и проверь через диспетчер.

**Ожидаемый результат:**

```text
отмена оформленного заказа  → true, опубликовано OrderCancelled
отмена несуществующего      → false
Cancel у неоформленного     → InvalidOperationException
```

<details><summary>Подсказка</summary>

Правила — в домене (`Cancel` бросает исключения); обработчик только находит агрегат, вызывает метод и публикует `order.Events[^1]`. Регистрация: `AddTransient<IRequestHandler<CancelOrderCommand, bool>, CancelOrderHandler>()`.

</details>

<details><summary>Решение</summary>

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

Проверено: тесты на отмену оформленного заказа (`true`, событие `OrderCancelled`), несуществующего (`false`), неоформленного (`InvalidOperationException`) и маршрутизацию через `Mediator` проходят.

</details>
