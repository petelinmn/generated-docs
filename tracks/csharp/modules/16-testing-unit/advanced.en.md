---
title: "Unit tests: advanced"
lang: en
track: csharp
kind: module
doc: advanced
slug: 16-testing-unit
part: 2
tags: [unit-testing, xunit, fixtures, test-doubles, nsubstitute, mocking, code-coverage, traits]
versions: ".NET 11, C# 15; xunit.v3 4.0.3; NSubstitute 5.3.0; Microsoft.Testing.Extensions.CodeCoverage 18.12.0"
status: draft
updated: 2026-11-22
---

# Unit tests: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [basics](basics.en.md). **Time:** ~1.5 h.

## Data sources for Theory

Besides `[InlineData]`, data is taken from a static member — `[MemberData]`; the type-safe variant in xUnit v3 is `TheoryData<…>`:

```csharp
public static TheoryData<int, int, int> DivideCases => new()
{
    { 10, 2, 5 },
    { 7, 2, 3 },
    { -9, 3, -3 },
};

[Theory]
[MemberData(nameof(DivideCases))]
public void Divide_Theory(int a, int b, int expected) => Assert.Equal(expected, new Calculator().Divide(a, b));
```

Data can also be given by a class (`[ClassData]`) or a custom attribute. For data that xUnit cannot serialize, theory rows still run (verified: a theory with two `new object()` gave 2 tests), but in a test explorer they are indistinguishable by value.

## Fixtures: shared state

Expensive setup (a database, a file) is shared between tests through a **fixture**:

| Scope | How | When it is created |
|---|---|---|
| each test | the test class's constructor / `Dispose` | a new instance per test |
| a test class | `IClassFixture<T>` | one `T` instance per class |
| several classes | `ICollectionFixture<T>` + `[Collection("name")]` | one per collection |

```csharp
public class SharedFixture : IAsyncLifetime
{
    public int Id { get; private set; }
    public ValueTask InitializeAsync() { Id = Interlocked.Increment(ref Created); return ValueTask.CompletedTask; }
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    public static int Created;
}

public class FixtureTests(SharedFixture fx) : IClassFixture<SharedFixture>
{
    [Fact] public void One() => Assert.Equal(1, fx.Id);
    [Fact] public void Two() => Assert.Equal(1, fx.Id);   // the fixture is created once per class
}
```

Verified: both tests saw the same instance (`Id == 1`). `IAsyncLifetime` allows asynchronous setup and cleanup. A shared fixture is shared mutable state: tests must not corrupt it.

## Test doubles

A module must be tested in isolation, so external dependencies (a clock, mail, a database, the network) are replaced with **doubles**:

| Kind | Purpose |
|---|---|
| stub | returns predefined values |
| fake | a working simplified implementation (an in-memory database) |
| mock | remembers calls so the interaction can be verified |

The **NSubstitute** library creates doubles from an interface:

```csharp
public class Welcome(IClock clock, IMailer mailer)
{
    public string Run(string user)
    {
        var greeting = clock.Now.Hour < 12 ? "Good morning" : "Hello";
        mailer.Send(user, $"{greeting}, {user}");
        return greeting;
    }
}

[Fact]
public void Welcome_Morning_SendsGoodMorning()
{
    var clock = Substitute.For<IClock>();
    clock.Now.Returns(new DateTime(2026, 1, 1, 9, 0, 0));          // stub
    var mailer = Substitute.For<IMailer>();

    var result = new Welcome(clock, mailer).Run("ann");

    Assert.Equal("Good morning", result);
    mailer.Received(1).Send("ann", "Good morning, ann");           // mock check: called exactly once
    mailer.DidNotReceive().Send("bob", Arg.Any<string>());
}
```

Verified: the test passes. `Returns` sets a value, `Received(n)` checks the number of calls, `Arg.Any<T>()` is any argument. Asynchronous methods: `repo.FindAsync(1, Arg.Any<CancellationToken>()).Returns("Ann")`. A detailed look at the library is in the "NSubstitute" material (not created yet).

A rule: **test behavior, not implementation**. If you check every call of internal dependencies, the test breaks on any refactoring; check the result and meaningful side effects (a mail was sent, a record was stored).

### Controlling nondeterminism

Time, randomness, the file system and the network are introduced through interfaces (`IClock`, `TimeProvider`, `IFileSystem`), like `IClock` above: the test is then stable and reproducible. .NET has the built-in `TimeProvider` and `FakeTimeProvider` in the package `Microsoft.Extensions.TimeProvider.Testing`.

## Cancellation and xUnit v3

In v3 the current test gets a cancellation token through `TestContext.Current.CancellationToken`: pass it into asynchronous calls so the test is interrupted when the run is stopped.

```csharp
Assert.Equal("Ann", await new Lookup(repo).NameAsync(1, TestContext.Current.CancellationToken));
```

## Filtering, traits, skipping

Tests are given labels: `[Trait("Category", "Slow")]`. Running selected tests in the Microsoft.Testing.Platform:

```text
dotnet test -- --filter-trait "Category=Slow"      # by trait
dotnet test -- --filter-method "*Add_Theory*"      # by method name
dotnet test -- --filter-class "M16.CalculatorTests"
```

Verified: by trait 1 test ran, by method `*Add_Theory*` — 3 (the number of `InlineData` rows). The exit code on failure is 2.

## Code coverage

The `Microsoft.Testing.Extensions.CodeCoverage` extension collects coverage right in `dotnet test`:

```text
dotnet add package Microsoft.Testing.Extensions.CodeCoverage
dotnet test -- --coverage --coverage-output-format cobertura
```

Verified: a `*.cobertura.xml` file is created in `TestResults/`. Coverage counts only code that lies in **other** assemblies than the tests: when the code under test was in the test project itself, the report was empty (`<packages />`) — put the code under test in a separate project. A high coverage percentage does not guarantee test quality: it shows which lines were executed, not which were checked.

## Parallelism

xUnit runs test **collections** in parallel; by default each test class is its own collection. Verified: three classes with simultaneously starting tests ran in parallel (a maximum of 3 at once). Tests inside a class run sequentially. Classes that use a shared resource are merged into one collection (`[Collection("db")]`) — it runs sequentially.

## Best practices

| Rule | Why |
|---|---|
| Fast, independent, repeatable tests | they can be run constantly |
| One scenario — one test, data through `Theory` | it is clear what broke |
| The name `Method_Scenario_Expectation` | the report reads like a specification |
| Doubles only at boundaries (time, network, database) | less brittleness |
| Do not test private methods directly | check public behavior |
| Test boundary values and errors | that is where bugs are most common |
| Do not check implementation (`Received`) without need | the test survives refactoring |

## Related materials

- Back: [basics](basics.en.md). Next: [expert theory](expert.en.md) — xUnit v3 internals, gotchas.
- [17 · Testing: integration tests](../17-testing-integration/overview.en.md) — real databases and HTTP.
- [13 · DI, configuration, logging](../13-di-config-logging/overview.en.md) — dependencies through interfaces.
- [10 · Async/await](../10-async-await/overview.en.md) — `CancellationToken` in tests.
- [09 · Delegates, events, lambdas](../09-delegates-events-lambdas/overview.en.md) — lambdas in `Assert.Throws`.

## Sources

- [xUnit shared context and fixtures](https://xunit.net/docs/shared-context)
- [Theory and data](https://xunit.net/docs/getting-started/v3/getting-started)
- [NSubstitute](https://nsubstitute.github.io/help/getting-started/)
- [Code coverage in Microsoft.Testing.Platform](https://learn.microsoft.com/dotnet/core/testing/microsoft-testing-platform-extensions-code-coverage)
- [`TimeProvider`](https://learn.microsoft.com/dotnet/api/system.timeprovider)
