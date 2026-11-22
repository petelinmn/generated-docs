---
title: "NSubstitute: basics"
lang: en
track: csharp
kind: library
doc: basics
slug: nsubstitute
part: 2
tags: [nsubstitute, mocking, unit-testing, test-doubles]
versions: ".NET 11, C# 15; NSubstitute 6.2.0"
status: draft
updated: 2026-11-22
---

# NSubstitute: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [16 · Unit testing](../../modules/16-testing-unit/overview.en.md), [05 · Interfaces, structs, records](../../modules/05-interfaces-structs-records/overview.en.md). **Time:** ~30 min.

## What it is and why

NSubstitute is a library for creating **test doubles** (substitutes/mocks): fake implementations of interfaces and classes whose behavior you set in the test. It lets you test code in isolation from the DB, the network and other dependencies. The syntax needs no lambda expressions for setup: you just call a method of the substitute and then `.Returns(...)`.

```bash
dotnet add package NSubstitute
```

## Creating and configuring

```csharp
public interface ICalculator { int Add(int a, int b); string Mode { get; set; } }

var calc = Substitute.For<ICalculator>();
calc.Add(1, 2).Returns(3);
calc.Add(Arg.Any<int>(), 10).Returns(100);
calc.Mode.Returns("sci");
```

Verified by running:

| Call | Result |
|---|---|
| `calc.Add(1, 2)` before configuring | `0` (the default value for `int`) |
| `calc.Add(1, 2)` after `.Returns(3)` | `3` |
| `calc.Add(5, 10)` by the pattern `Arg.Any<int>(), 10` | `100` |
| `calc.Add(7, 7)` — not configured | `0` |
| `calc.Mode` | `"sci"` |

Unconfigured members return **default values** (0, `null`, …) rather than throwing.

## Verifying calls

```csharp
calc.Received().Add(1, 2);          // at least one call
calc.Received(3).Add(1, 2);         // exactly three calls
calc.DidNotReceive().Add(9, 9);
```

Verified: `Received(3)` passed with three real calls of `Add(1, 2)` — the call inside `.Returns(...)` during setup is **not counted**. A mismatch (`Received(5)`) and a violated `DidNotReceive` give a `ReceivedCallsException` listing the actual calls.

## Argument matching

| Notation | Meaning |
|---|---|
| a value (`1`) | equality |
| `Arg.Any<T>()` | any value of the type |
| `Arg.Is<T>(x => …)` | a condition |
| `Arg.Do<T>(x => …)` | perform an action with the argument |

Verified: `m.Received().Log(Arg.Is<string>(s => s.StartsWith("he")))` passed for the call `Log("hello")`, and `DidNotReceive` with the condition `StartsWith("x")` passed too.

## Sequences, exceptions, async

```csharp
seq.Add(0, 0).Returns(1, 2, 3);                     // 1, 2, 3, 3, ...
bad.Add(Arg.Any<int>(), Arg.Any<int>()).Throws(new InvalidOperationException("boom"));
repo.GetAsync(2).Returns("two");                     // wraps into Task<string?>
repo.GetAsync(99).ThrowsAsync(new KeyNotFoundException());
```

Verified: the sequence — four calls gave `1, 2, 3, 3` (the last value repeats); `Throws` threw `InvalidOperationException`; for `Task<T>` returning the value is enough (`two`), an unconfigured `GetAsync(3)` returned `null`; `ThrowsAsync` — the exception on `await`.

## Beginner mistakes

- ❌ Mocking a concrete class with non-virtual members → ✅ interfaces or `virtual` members ([advanced](advanced.en.md#classes-and-virtual-members)).
- ❌ Verifying the implementation rather than the result → ✅ verify behavior; `Received` only for important side effects.
- ❌ Configuring another substitute inside `.Returns(...)` → ✅ create the value beforehand.
- ❌ An `internal` interface without `InternalsVisibleTo` → ✅ [a gotcha](expert.en.md#edges-and-gotchas).
- ❌ Expecting an exception from an unconfigured member → ✅ a default value is returned.

## Related materials

- Next: [advanced](advanced.en.md) — callbacks, events, recursive substitutes, `ForPartsOf`.
- [16 · Unit testing](../../modules/16-testing-unit/overview.en.md) — xUnit and test structure.
- [Refit](../refit/overview.en.md) — substituting an HTTP client in tests.
- [Testcontainers](../testcontainers/overview.en.md) — when you need a real DB, not a substitute.
- [Python](../../../python/overview.en.md) — `unittest.mock`.

## Sources

- [NSubstitute: Getting started](https://nsubstitute.github.io/help/getting-started/)
- [NSubstitute: Return for specific args](https://nsubstitute.github.io/help/return-for-args/)
- [NSubstitute: Checking received calls](https://nsubstitute.github.io/help/received-calls/)
