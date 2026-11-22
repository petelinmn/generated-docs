---
title: "Упражнения: Unit-тесты"
lang: ru
track: csharp
kind: module
doc: exercises
slug: 16-testing-unit
part: 2
tags: [unit-testing, xunit, theory, assertions, nsubstitute, async-tests]
status: draft
updated: 2026-11-22
---

# Упражнения: Unit-тесты

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. В упражнениях код под тестом дан, **тесты пишешь ты**. Все решения проверены запуском: проект с `xunit.v3` 4.0.3 и `NSubstitute` 5.3.0, `dotnet test` — 40 тестов, 39 пройдено, 1 пропущен (демонстрационный). Запускаемых версий в `code/` пока нет (уровень 5 не создан).

## 1. 🟢 Високосный год

**Задача:** дан `DateRules.IsLeapYear(int y)` — `(y % 4 == 0 && y % 100 != 0) || y % 400 == 0`. Напиши теорию с четырьмя случаями.

**Ожидаемый результат:**

```text
2024 → true, 1900 → false, 2000 → true, 2023 → false  (4 тест-кейса)
```

<details><summary>Подсказка</summary>

Нужны все ветви правила: кратность 4, исключение для сотен и исключение из исключения для 400.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 1: one theory covers every branch of the rule.
public class Ex1_LeapYear
{
    [Theory]
    [InlineData(2024, true)]
    [InlineData(1900, false)]
    [InlineData(2000, true)]
    [InlineData(2023, false)]
    public void IsLeapYear_Cases(int year, bool expected) => Assert.Equal(expected, DateRules.IsLeapYear(year));
}
```

Проверено: 4 теста пройдены.

</details>

## 2. 🟢 Счёт и исключения

**Задача:** дан `Account` с `Withdraw(decimal)`: сумма должна быть положительной (`ArgumentOutOfRangeException` с `ParamName = "amount"`), снять больше остатка нельзя (`InvalidOperationException("Insufficient funds.")`). Напиши тесты: успешное снятие, превышение остатка (баланс не меняется), неположительные суммы.

**Ожидаемый результат:** 4 теста (1 + 1 + 2 кейса теории) проходят.

<details><summary>Подсказка</summary>

`Assert.Throws<T>(...)` возвращает исключение — проверь его сообщение; для `ParamName` есть перегрузка `Assert.Throws<ArgumentOutOfRangeException>("amount", …)`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 2: happy path, an error path that leaves the state intact, and a theory for invalid input.
public class Ex2_Account
{
    [Fact]
    public void Withdraw_ReducesBalance()
    {
        var a = new Account(100m);
        a.Withdraw(30m);
        Assert.Equal(70m, a.Balance);
    }

    [Fact]
    public void Withdraw_TooMuch_ThrowsAndKeepsBalance()
    {
        var a = new Account(100m);
        var ex = Assert.Throws<InvalidOperationException>(() => a.Withdraw(500m));
        Assert.Equal("Insufficient funds.", ex.Message);
        Assert.Equal(100m, a.Balance);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Withdraw_NonPositive_Throws(int amount) =>
        Assert.Throws<ArgumentOutOfRangeException>("amount", () => new Account(100m).Withdraw(amount));
}
```

Проверено: все тесты пройдены. Тест с ошибкой проверяет и то, что состояние не испортилось.

</details>

## 3. 🟡 FizzBuzz через TheoryData

**Задача:** для `Fizz.Of(int)` напиши теорию, данные которой лежат в статическом `TheoryData<int, string>`: `1→"1"`, `3→"Fizz"`, `5→"Buzz"`, `15→"FizzBuzz"`, `14→"14"`.

**Ожидаемый результат:** 5 тест-кейсов проходят.

<details><summary>Подсказка</summary>

`public static TheoryData<int, string> Cases => new() { { 1, "1" }, … };` и `[Theory, MemberData(nameof(Cases))]`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 3: typed theory data.
public class Ex3_Fizz
{
    public static TheoryData<int, string> Cases => new()
    {
        { 1, "1" }, { 3, "Fizz" }, { 5, "Buzz" }, { 15, "FizzBuzz" }, { 14, "14" },
    };

    [Theory, MemberData(nameof(Cases))]
    public void Of_Cases(int n, string expected) => Assert.Equal(expected, Fizz.Of(n));
}
```

Проверено: 5 тест-кейсов пройдены.

</details>

## 4. 🟡 Мок уведомления

**Задача:** `OrderService(INotifier)`: `Place(total)` возвращает `false` для `total <= 0`; для `total > 100` вызывает `Notify($"Big order: {total}")`. Напиши тесты с NSubstitute: большой заказ уведомляет один раз; маленький — не уведомляет; некорректный возвращает `false`.

**Ожидаемый результат:** 3 теста проходят.

<details><summary>Подсказка</summary>

`Substitute.For<INotifier>()`, `notifier.Received(1).Notify("Big order: 150")`, `notifier.DidNotReceive().Notify(Arg.Any<string>())`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 4: verifying interaction with a mock.
public class Ex4_Mock
{
    [Fact]
    public void BigOrder_NotifiesOnce()
    {
        var notifier = Substitute.For<INotifier>();
        var ok = new OrderService(notifier).Place(150m);
        Assert.True(ok);
        notifier.Received(1).Notify("Big order: 150");
    }

    [Fact]
    public void SmallOrder_DoesNotNotify()
    {
        var notifier = Substitute.For<INotifier>();
        Assert.True(new OrderService(notifier).Place(10m));
        notifier.DidNotReceive().Notify(Arg.Any<string>());
    }

    [Fact]
    public void InvalidOrder_ReturnsFalse() => Assert.False(new OrderService(Substitute.For<INotifier>()).Place(0m));
}
```

Проверено: 3 теста пройдены. Здесь проверка вызова оправдана: уведомление — само требование.

</details>

## 5. 🔴 Асинхронная заглушка и токен отмены

**Задача:** `Lookup(IRepo).NameAsync(id, ct)` возвращает имя из `repo.FindAsync(id, ct)` или `"unknown"`. Напиши три асинхронных теста: найдено, не найдено, токен передан в репозиторий без изменений. Используй `TestContext.Current.CancellationToken`.

**Ожидаемый результат:** 3 теста проходят.

<details><summary>Подсказка</summary>

`repo.FindAsync(1, Arg.Any<CancellationToken>()).Returns("Ann")`; для `null` — `.Returns((string?)null)`; проверка токена — `await repo.Received(1).FindAsync(3, cts.Token)`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 5: async stubs and cancellation-token propagation.
public class Ex5_AsyncStub
{
    [Fact]
    public async Task Name_WhenFound_ReturnsName()
    {
        var repo = Substitute.For<IRepo>();
        repo.FindAsync(1, Arg.Any<CancellationToken>()).Returns("Ann");
        Assert.Equal("Ann", await new Lookup(repo).NameAsync(1, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Name_WhenMissing_ReturnsUnknown()
    {
        var repo = Substitute.For<IRepo>();
        repo.FindAsync(2, Arg.Any<CancellationToken>()).Returns((string?)null);
        Assert.Equal("unknown", await new Lookup(repo).NameAsync(2, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Name_PassesTheCancellationToken()
    {
        var repo = Substitute.For<IRepo>();
        using var cts = new CancellationTokenSource();
        repo.FindAsync(3, cts.Token).Returns("x");
        await new Lookup(repo).NameAsync(3, cts.Token);
        await repo.Received(1).FindAsync(3, cts.Token);
    }
}
```

Проверено: 3 теста пройдены. Третий тест защищает от ошибки «забыли передать токен».

</details>
