---
title: "Unit tests: basics"
lang: en
track: csharp
kind: module
doc: basics
slug: 16-testing-unit
part: 2
tags: [unit-testing, xunit, arrange-act-assert, theory, assertions, test-isolation]
versions: ".NET 11, C# 15; xunit.v3 4.0.3"
status: draft
updated: 2026-11-22
---

# Unit tests: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [04 · OOP: classes and objects](../04-oop-basics/overview.en.md), [08 · Exceptions](../08-exceptions/overview.en.md), [10 · Async/await](../10-async-await/overview.en.md). **Time:** ~1 h.

## Why it matters

A unit test is a small program that calls a piece of code (a method, a class) and checks the result. A set of such tests runs with one command and reports whether the logic broke after changes. Good tests let you change code without fear, serve as living documentation and catch bugs before users do. The most common .NET framework is **xUnit** (in this course — version 3, the package `xunit.v3`).

## A test project

An xUnit v3 test project is an **executable** project (`OutputType=Exe`) that runs through the Microsoft.Testing.Platform. This mode is enabled in `global.json` so that `dotnet test` works:

```json
{ "test": { "runner": "Microsoft.Testing.Platform" } }
```

```xml
<PropertyGroup>
  <TargetFramework>net11.0</TargetFramework>
  <OutputType>Exe</OutputType>
</PropertyGroup>
<ItemGroup>
  <PackageReference Include="xunit.v3" Version="4.0.3" />
</ItemGroup>
<ItemGroup><Using Include="Xunit" /></ItemGroup>
```

You run it with `dotnet test` in the project or solution folder. The result (verified): `total: 21, succeeded: 20, skipped: 1`.

## The first test

A test is a method with the `[Fact]` attribute. The **Arrange–Act–Assert** structure (set up, act, check) makes a test readable:

```csharp
public class Calculator
{
    public int Add(int a, int b) => a + b;
}

public class CalculatorTests
{
    [Fact]
    public void Add_TwoNumbers_ReturnsSum()
    {
        // Arrange
        var calc = new Calculator();
        // Act
        var result = calc.Add(2, 3);
        // Assert
        Assert.Equal(5, result);
    }
}
```

The test name describes what is checked: a convenient scheme is `Method_Scenario_Expectation` (`Add_TwoNumbers_ReturnsSum`). One test — one idea: if it fails, it is clear at once what broke.

## Assertions (Assert)

The `Assert` class compares the actual result with the expected one; the **expected** value goes first:

| Assertion | Meaning |
|---|---|
| `Assert.Equal(expected, actual)` | equality |
| `Assert.True(cond)` / `Assert.False(cond)` | a condition |
| `Assert.Null(x)` / `Assert.NotNull(x)` | `null` |
| `Assert.Throws<T>(() => …)` | the code throws exactly type `T` |
| `Assert.Contains(item, collection)` | the element is in the collection |
| `Assert.Single(c)` / `Assert.Empty(c)` | one element / empty |
| `Assert.All(c, x => …)` | a check for every element |
| `Assert.Equal(0.3, 0.1 + 0.2, precision: 10)` | floating-point numbers — with precision |

On failure the message says what was expected and what was received (verified):

```text
Assert.Equal() Failure: Values differ
Expected: 4
Actual:   5
```

For strings xUnit shows the position of the first difference. An exception check returns the exception, so you can examine the message:

```csharp
[Fact]
public void Divide_ByZero_Throws()
{
    var ex = Assert.Throws<DivideByZeroException>(() => new Calculator().Divide(1, 0));
    Assert.Equal("b is zero", ex.Message);
}
```

## Parameterized tests: Theory

If the same test must run on different data, use `[Theory]` with a data source — `[InlineData]`:

```csharp
[Theory]
[InlineData(1, 2, 3)]
[InlineData(-1, 1, 0)]
[InlineData(0, 0, 0)]
public void Add_Theory(int a, int b, int expected) =>
    Assert.Equal(expected, new Calculator().Add(a, b));
```

Each data row is a separate test in the report. Boundary values (zero, negatives, limits) are the first candidates for data rows.

## Test isolation

xUnit creates a **new instance** of the test class for each test, so class fields do not "leak" between tests:

```csharp
public class LifecycleTests
{
    private readonly Counter _counter = new();
    [Fact] public void A() { _counter.Inc(); Assert.Equal(1, _counter.Value); }
    [Fact] public void B() { _counter.Inc(); Assert.Equal(1, _counter.Value); }   // a new instance: still 1
}
```

Verified: both tests passed. Preparation goes in the constructor, cleanup in `Dispose` (if the class implements `IDisposable`). Tests must not depend on each other or on run order.

## Asynchronous tests

A test method can be `async Task`; exceptions are awaited with `Assert.ThrowsAsync`:

```csharp
[Fact]
public async Task Async_Test_Awaits() => Assert.Equal(42, await Task.Run(() => 42));

[Fact]
public async Task Async_Throws() =>
    await Assert.ThrowsAsync<InvalidOperationException>(async () => { await Task.Yield(); throw new InvalidOperationException(); });
```

Do not use `async void` in tests and do not block on `.Result` ([10 · Async/await](../10-async-await/overview.en.md)).

## Skipping tests

`[Fact(Skip = "reason")]` skips a test and shows the reason in the report (verified: `skipped M16.CalculatorTests.Skipped`). Skipping is a temporary measure: do not leave skipped tests for long.

## Beginner mistakes

- ❌ A test without an assertion (`Assert`) → ✅ every test asserts something.
- ❌ Several unrelated checks in one test → ✅ one scenario — one test, data through `Theory`.
- ❌ Tests depend on each other or on order → ✅ each creates its own state.
- ❌ A test uses the current time, randomness, the network → ✅ substitute controllable dependencies ([advanced theory](advanced.en.md#test-doubles)).
- ❌ Swapped arguments `Assert.Equal(actual, expected)` → ✅ expected first.
- ❌ `Assert.True(a == b)` instead of `Assert.Equal(a, b)` → ✅ `Equal` shows the values on failure.

## Related materials

- Next: [advanced theory](advanced.en.md) — fixtures, data, test doubles, coverage.
- [08 · Exceptions](../08-exceptions/overview.en.md) — what to check in negative scenarios.
- [10 · Async/await](../10-async-await/overview.en.md) — asynchronous tests.
- [13 · DI, configuration, logging](../13-di-config-logging/overview.en.md) — dependencies through interfaces make code testable.
- [Python](../../../python/overview.en.md) — pytest: parametrization and fixtures.

## Sources

- [Testing in .NET](https://learn.microsoft.com/dotnet/core/testing/)
- [Unit testing C# with xUnit](https://learn.microsoft.com/dotnet/core/testing/unit-testing-csharp-with-xunit)
- [xUnit.net v3 documentation](https://xunit.net/docs/getting-started/v3/getting-started)
- [Microsoft.Testing.Platform](https://learn.microsoft.com/dotnet/core/testing/microsoft-testing-platform-intro)
