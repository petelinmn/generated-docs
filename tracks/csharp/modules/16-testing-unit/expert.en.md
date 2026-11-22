---
title: "Unit tests: expert"
lang: en
track: csharp
kind: module
doc: expert
slug: 16-testing-unit
part: 2
tags: [unit-testing, xunit-v3, microsoft-testing-platform, test-parallelism, assertions, test-design, interview-questions]
versions: ".NET 11, C# 15; xunit.v3 4.0.3"
status: draft
updated: 2026-11-22
---

# Unit tests: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

**Prerequisites:** [basic](basics.en.md) and [advanced](advanced.en.md) theory. **Time:** ~1 h.

## How it is built

**xUnit v3** — test projects are now standalone executables (`OutputType=Exe`) with a built-in host that implements the **Microsoft.Testing.Platform** (MTP): test discovery, execution, filters, extensions (coverage, reports) and exit codes. `dotnet test` runs this executable and parses its output; `global.json` needs the `"runner": "Microsoft.Testing.Platform"` mode — without it v3 does not run on the .NET 10+ SDK. The exit code when tests fail is 2 (verified), success is 0.

**The test lifecycle.** For each test xUnit: creates an instance of the test class (constructor → `InitializeAsync` if needed) → calls the method → `DisposeAsync`/`Dispose`. Class and collection fixtures are created before the first test and disposed after the last one. Theory parameters are separate test cases with their own instances.

**Parallelism.** The unit of parallelism is the **test collection**; by default it is a class. Classes run in parallel, tests inside a class run sequentially. Verified: three classes with simultaneously starting tests reached 3 concurrent executions. Consequence: **static state is shared across all parallel tests**.

## Assertions and their semantics

- **Exact type match.** `Assert.Throws<Exception>(...)` **fails** for an `InvalidOperationException` ("Exception type was not an exact match"); `Assert.ThrowsAny<Exception>(...)` passes (verified).
- **Collection order.** `Assert.Equal(new[]{1,2}, new[]{2,1})` fails ("Collections differ"); an order-insensitive comparison is `Assert.Equivalent` (verified: passes).
- **Asynchronous code.** `Assert.Throws<T>(() => throw …)` with a lambda the compiler binds to `Func<Task>` is a compile error `CS0619`: "You must call Assert.ThrowsAsync<T> (and await the result) when testing async code". For asynchronous code — `ThrowsAsync` with `await`; for synchronous — an `Action` lambda or a method group (verified).
- **Conditional skip at run time.** `Assert.Skip("reason")` ends the test as skipped (verified: `skipped … not on this OS`); `[Fact(Skip = …)]` is unconditional.
- **Comparing floating-point numbers.** Only with precision (`precision`) or a tolerance.
- **Messages.** For strings the position of the first difference is given, for collections both lists (verified).

## Reference

| What | xUnit v3 API |
|---|---|
| A test | `[Fact]`, `[Fact(Skip = "…")]`, `Assert.Skip(…)` |
| Parameterization | `[Theory]`, `[InlineData]`, `[MemberData]`, `[ClassData]`, `TheoryData<…>` |
| Shared state | `IClassFixture<T>`, `ICollectionFixture<T>`, `[Collection]`, `IAsyncLifetime` |
| Labels and filters | `[Trait]`, `--filter-trait`, `--filter-method`, `--filter-class`, `--filter-namespace` |
| Test context | `TestContext.Current` (`CancellationToken`, output, status) |
| Assertions | `Equal`, `Equivalent`, `Throws`/`ThrowsAny`/`…Async`, `Collection`, `All`, `Single`, `Contains`, `Raises` |
| Parallelism | `[assembly: CollectionBehavior(...)]`, `DisableParallelization` |
| Coverage | `Microsoft.Testing.Extensions.CodeCoverage` (`--coverage`) |

## Gotchas

**1. Static state and parallelism.** Two tests that increment a shared static counter are each correct alone, but together one fails (verified: `Actual: 2`). Do not use statics in tests; if unavoidable — a collection without parallelism.

**2. An exception without `ThrowsAny`.** `Throws<Exception>` does not catch derived exceptions.

**3. `async void` tests** are not awaited — the test "passes" before it completes. Only `async Task`.

**4. Order dependence.** Tests in a class have no guaranteed order, so they cannot depend on each other.

**5. A test that cannot fail.** Without an `Assert`, or with a check that is true by construction, a test is useless. Mutation testing (Stryker.NET) helps.

**6. Coverage ≠ quality.** 100% of lines can be covered by tests without checks.

**7. Over-mocking.** A test that checks every internal call breaks on any refactoring.

**8. The external environment.** Clocks, randomness, the file system, environment variables, culture: a number's `ToString()` depends on the current culture — a test comparing strings fails on another locale.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| xUnit | the community standard, isolation by default, extensible | an instance per test is unfamiliar after NUnit |
| NUnit / MSTest | familiar `[SetUp]`, rich attributes | shared state by default |
| Hand-written doubles (fakes) | simple, transparent | a lot of code |
| NSubstitute / Moq | quickly create doubles | a risk of over-checking calls |
| Property-based (FsCheck) | finds unexpected inputs | needs a property mindset |
| Mutation testing (Stryker.NET) | checks the quality of tests | slow |

## Interview questions

<details><summary>1. How does a unit test differ from an integration test?</summary>

A unit test checks a unit of code in isolation (dependencies replaced with doubles), is fast and deterministic. An integration test checks the interaction of real components (a database, HTTP) and is slower ([17](../17-testing-integration/overview.en.md)).

</details>

<details><summary>2. What is Arrange-Act-Assert?</summary>

A test structure: preparing data and objects, calling the code under test, checking the result. It makes a test readable and highlights what exactly is checked.

</details>

<details><summary>3. How does `[Fact]` differ from `[Theory]`?</summary>

`Fact` is a test without parameters; `Theory` is parameterized and runs for each data row (`InlineData`, `MemberData`, `ClassData`); each row is a separate test case.

</details>

<details><summary>4. How does xUnit isolate tests?</summary>

It creates a new instance of the test class for each test (verified: two tests, each incrementing a field, see `1`). Shared state exists only explicitly, through fixtures.

</details>

<details><summary>5. What is a fixture and what kinds are there?</summary>

A shared object for several tests. `IClassFixture<T>` — one per class; `ICollectionFixture<T>` — per collection; per test — the constructor/`Dispose`. Asynchronous setup — `IAsyncLifetime`.

</details>

<details><summary>6. What are a stub, a fake and a mock?</summary>

A stub returns given values; a fake is a working simplified implementation; a mock remembers calls and lets you verify the interaction.

</details>

<details><summary>7. How do you test code that depends on time?</summary>

Introduce time through an interface (`IClock`, `TimeProvider`) and substitute a fixed value; do not use `DateTime.Now` directly.

</details>

<details><summary>8. How does xUnit parallelize tests?</summary>

Classes (test collections) run in parallel, tests inside a class sequentially. Shared static state is dangerous; resources are merged into one collection.

</details>

<details><summary>9. Why might `Assert.Throws<Exception>` not work?</summary>

It requires the exact type; for derived exceptions you need `ThrowsAny`. Verified: for `InvalidOperationException` with `Throws<Exception>` — "not an exact match".

</details>

<details><summary>10. What is code coverage and can you trust it?</summary>

The share of executed lines/branches. It shows what is untested, but not the quality of the checks: you can execute code and verify nothing.

</details>

<details><summary>11. Trick question: two correct tests fail together. Why?</summary>

Probably shared static state: tests of different classes (or the order within a class) interfere. Verified: two tests with a shared static counter — one fails.

</details>

<details><summary>12. Trick question: an `async void` test shows "success" although there is an exception inside. Why?</summary>

An `async void` cannot be awaited — the exception is not delivered to the runner, and the test ends earlier. You need `async Task`.

</details>

<details><summary>13. Trick question: coverage is 0% although the tests passed. Why?</summary>

Coverage is counted for assemblies other than the test one. If the code under test lies in the test project itself, the report is empty (verified: `<packages />`).

</details>

## Related materials

- Back: [advanced theory](advanced.en.md). Check yourself: [quiz](quiz.en.md) and [exercises](exercises.en.md).
- [17 · Testing: integration tests](../17-testing-integration/overview.en.md) — the next level of the pyramid.
- [10 · Async/await](../10-async-await/overview.en.md) — asynchronous tests and cancellation.
- [13 · DI, configuration, logging](../13-di-config-logging/overview.en.md) — injecting doubles.
- [Python](../../../python/overview.en.md) — pytest.

## Sources

- [xUnit.net: shared context](https://xunit.net/docs/shared-context)
- [xUnit.net: running tests in parallel](https://xunit.net/docs/running-tests-in-parallel)
- [Microsoft.Testing.Platform: exit codes](https://aka.ms/testingplatform/exitcodes)
- [Stryker.NET](https://stryker-mutator.io/docs/stryker-net/introduction/)
