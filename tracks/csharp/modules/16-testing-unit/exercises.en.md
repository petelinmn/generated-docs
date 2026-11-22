---
title: "Exercises: Unit tests"
lang: en
track: csharp
kind: module
doc: exercises
slug: 16-testing-unit
part: 2
tags: [unit-testing, xunit, theory, assertions, nsubstitute, async-tests]
status: draft
updated: 2026-11-22
---

# Exercises: Unit tests

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. In these exercises the code under test is given, and **you write the tests**. All solutions were verified by running them: a project with `xunit.v3` 4.0.3 and `NSubstitute` 5.3.0, `dotnet test` — 40 tests, 39 passed, 1 skipped (a demonstration one). There are no runnable versions in `code/` yet (level 5 does not exist).

## 1. 🟢 A leap year

**Task:** given `DateRules.IsLeapYear(int y)` — `(y % 4 == 0 && y % 100 != 0) || y % 400 == 0`. Write a theory with four cases.

**Expected result:**

```text
2024 → true, 1900 → false, 2000 → true, 2023 → false  (4 test cases)
```

<details><summary>Hint</summary>

You need all branches of the rule: divisible by 4, the exception for hundreds, and the exception to the exception for 400.

</details>

<details><summary>Solution</summary>

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

Verified: 4 tests passed.

</details>

## 2. 🟢 An account and exceptions

**Task:** given an `Account` with `Withdraw(decimal)`: the amount must be positive (`ArgumentOutOfRangeException` with `ParamName = "amount"`), and you cannot withdraw more than the balance (`InvalidOperationException("Insufficient funds.")`). Write tests: a successful withdrawal, exceeding the balance (the balance does not change), non-positive amounts.

**Expected result:** 4 tests (1 + 1 + 2 theory cases) pass.

<details><summary>Hint</summary>

`Assert.Throws<T>(...)` returns the exception — check its message; for `ParamName` there is the overload `Assert.Throws<ArgumentOutOfRangeException>("amount", …)`.

</details>

<details><summary>Solution</summary>

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

Verified: all tests passed. The error test also checks that the state was not corrupted.

</details>

## 3. 🟡 FizzBuzz with TheoryData

**Task:** for `Fizz.Of(int)` write a theory whose data lives in a static `TheoryData<int, string>`: `1→"1"`, `3→"Fizz"`, `5→"Buzz"`, `15→"FizzBuzz"`, `14→"14"`.

**Expected result:** 5 test cases pass.

<details><summary>Hint</summary>

`public static TheoryData<int, string> Cases => new() { { 1, "1" }, … };` and `[Theory, MemberData(nameof(Cases))]`.

</details>

<details><summary>Solution</summary>

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

Verified: 5 test cases passed.

</details>

## 4. 🟡 A notification mock

**Task:** `OrderService(INotifier)`: `Place(total)` returns `false` for `total <= 0`; for `total > 100` it calls `Notify($"Big order: {total}")`. Write NSubstitute tests: a big order notifies once; a small one does not notify; an invalid one returns `false`.

**Expected result:** 3 tests pass.

<details><summary>Hint</summary>

`Substitute.For<INotifier>()`, `notifier.Received(1).Notify("Big order: 150")`, `notifier.DidNotReceive().Notify(Arg.Any<string>())`.

</details>

<details><summary>Solution</summary>

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

Verified: 3 tests passed. Here the call check is justified: the notification is the requirement itself.

</details>

## 5. 🔴 An asynchronous stub and a cancellation token

**Task:** `Lookup(IRepo).NameAsync(id, ct)` returns the name from `repo.FindAsync(id, ct)` or `"unknown"`. Write three asynchronous tests: found, not found, the token is passed to the repository unchanged. Use `TestContext.Current.CancellationToken`.

**Expected result:** 3 tests pass.

<details><summary>Hint</summary>

`repo.FindAsync(1, Arg.Any<CancellationToken>()).Returns("Ann")`; for `null` — `.Returns((string?)null)`; the token check — `await repo.Received(1).FindAsync(3, cts.Token)`.

</details>

<details><summary>Solution</summary>

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

Verified: 3 tests passed. The third test protects against the "forgot to pass the token" bug.

</details>
