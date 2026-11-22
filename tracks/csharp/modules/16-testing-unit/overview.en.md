---
title: "Unit tests"
lang: en
track: csharp
kind: module
doc: overview
slug: 16-testing-unit
part: 2
levels: [1, 2, 3, 4]
tags: [unit-testing, xunit, arrange-act-assert, theory, fixtures, mocking, code-coverage]
versions: ".NET 11, C# 15; xunit.v3 4.0.3; NSubstitute 5.3.0"
status: draft
updated: 2026-11-22
---

# Unit tests

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

A unit test is a small program that calls a unit of code and checks the result. In .NET the de facto standard is xUnit (v3): a test project is an executable on the Microsoft.Testing.Platform, and `dotnet test` runs it. Tests must be fast, independent and deterministic; to achieve this, external dependencies (time, mail, a database) are replaced with doubles (NSubstitute). Code coverage shows what is not checked, but not the quality of the checks.

## Key ideas

- **Arrange–Act–Assert** and names like `Method_Scenario_Expectation`; one scenario — one test.
- **`[Fact]` and `[Theory]`** (`InlineData`, `MemberData`/`TheoryData`): data instead of copied tests.
- **Isolation:** a new class instance per test; shared state only through fixtures (`IClassFixture`).
- **Assertions:** the expected value as the first argument; `Throws<T>` requires the exact type, `ThrowsAny` — any derived one.
- **Doubles:** stub, fake, mock; test behavior, not implementation.
- **Parallelism:** test classes run in parallel, static state in tests is dangerous.
- **Coverage:** `--coverage`, counted only for code outside the test assembly.

## Minimal example

```csharp
public class Calculator { public int Add(int a, int b) => a + b; }

public class CalculatorTests
{
    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(-1, 1, 0)]
    public void Add_Cases(int a, int b, int expected) =>
        Assert.Equal(expected, new Calculator().Add(a, b));
}
```

## When to use and when not

- ✅ Business logic, calculations, validation, parsing — fast unit tests.
- ✅ Boundary values and errors — theories and `Assert.Throws`.
- ✅ Dependencies at the boundaries — doubles.
- ❌ Tests that check implementation (every internal call): they break on refactoring.
- ❌ Unit tests for what should be checked with a real database or HTTP — integration tests ([17](../17-testing-integration/overview.en.md)).
- ❌ Static state and order dependence.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | an xUnit v3 project, `[Fact]`, AAA, `Assert`, `[Theory]`, isolation, asynchronous tests, skipping |
| [2. Advanced](advanced.en.md) | `MemberData`/`TheoryData`, fixtures, doubles and NSubstitute, `TimeProvider`, `TestContext`, filters and traits, coverage, parallelism |
| [3. Expert](expert.en.md) | xUnit v3 and MTP internals, lifecycle, assertion semantics, gotchas, reference, 13 interview questions |
| [4. Self-check](quiz.en.md) | 11 questions and [5 exercises](exercises.en.md) with solutions |
| 5. Runnable examples | not generated yet — `/learn csharp 16-testing-unit level 5` |
| 6. Tests | not generated yet — `/learn csharp 16-testing-unit level 6` |

## Related materials

- Prerequisites: [08 · Exceptions](../08-exceptions/overview.en.md), [13 · DI, configuration, logging](../13-di-config-logging/overview.en.md)
- Next: [17 · Testing: integration tests](../17-testing-integration/overview.en.md)
- Asynchronous tests: [10 · Async/await](../10-async-await/overview.en.md)
- Architecture tests: [21 · Application architecture](../21-architecture/overview.en.md)
- The same idea in other tracks: [Python](../../../python/overview.en.md) — pytest
