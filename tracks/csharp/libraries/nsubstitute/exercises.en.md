---
title: "Exercises: NSubstitute"
lang: en
track: csharp
kind: library
doc: exercises
slug: nsubstitute
part: 2
tags: [nsubstitute, mocking, callbacks, partial-substitutes]
status: draft
updated: 2026-11-22
---

# Exercises: NSubstitute

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself. The solutions were verified by running (NSubstitute 6.2.0, a console application, no test framework). There are no runnable versions in `code/` (level 5 does not exist). The types in the examples are `public` (otherwise the proxy is not created).

## 1. 🟢 Configure and verify

**Task:** substitute `ICalculator`: `Add(1, 2)` → `3`, `Add(any, 10)` → `100`. Verify that `Add(9, 9)` was not called and `Add(1, 2)` was.

**Expected result:** `3`, `100`, unconfigured `Add(7, 7)` → `0`; the checks pass.

<details><summary>Hint</summary>

`Substitute.For<ICalculator>()`, `.Returns(...)`, `Received()`, `DidNotReceive()`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 1: configure, then verify interactions.
var calc = Substitute.For<ICalculator>();
calc.Add(1, 2).Returns(3);
calc.Add(Arg.Any<int>(), 10).Returns(100);
Console.WriteLine($"{calc.Add(1, 2)} {calc.Add(5, 10)} {calc.Add(7, 7)}");   // 3 100 0
calc.Received().Add(1, 2);
calc.DidNotReceive().Add(9, 9);
```

Verified: `configured: 3 100 unconfigured=0`.

</details>

## 2. 🟡 A result from arguments and a sequence

**Task:** (a) `Add` returns the sum of its arguments; (b) a substitute returning `1, 2, 3` over four calls.

**Expected result:** `42` for `Add(20, 22)`; `1233`.

<details><summary>Hint</summary>

`Returns(c => c.ArgAt<int>(0) + c.ArgAt<int>(1))` and `Returns(1, 2, 3)`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 2: computed result and a result sequence.
echo.Add(Arg.Any<int>(), Arg.Any<int>()).Returns(c => c.ArgAt<int>(0) + c.ArgAt<int>(1));
seq.Add(0, 0).Returns(1, 2, 3);
```

Verified: `returns from args: 42`, `sequence: 1233`.

</details>

## 3. 🟡 Async and errors

**Task:** `IRepo.GetAsync(int)`: for `2` return `"two"`, for `99` — `KeyNotFoundException` on `await`; an unconfigured call — `null`.

**Expected result:** `two`; `KeyNotFoundException`; `null`.

<details><summary>Hint</summary>

`Returns("two")` for `Task<string?>`, `ThrowsAsync(new KeyNotFoundException())`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 3: Task<T> results and async failures.
repo.GetAsync(2).Returns("two");
repo.GetAsync(99).ThrowsAsync(new KeyNotFoundException());
var v = await repo.GetAsync(3);        // null
```

Verified: `async: onetwo unconfigured=` (empty — `null`), `ThrowsAsync: KeyNotFoundException`.

</details>

## 4. 🔴 A callback, an event and a partial substitute

**Task:** (a) `When…Do` to record the arguments of a `void` method; (b) raise a substitute's event; (c) `ForPartsOf<Concrete>` with a configured virtual method and a real non-virtual one.

**Expected result:** `when:a,when:b`; the handler called once; `Virtual() = 8`, `NonVirtual() = 2`; `Returns` on a non-virtual — `CouldNotSetReturnDueToNoLastCallException`.

<details><summary>Hint</summary>

`sub.When(l => l.Log(Arg.Any<string>())).Do(c => …)`, `pub.Changed += Raise.Event()`, `Substitute.ForPartsOf<Concrete>()`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 4: callbacks, events and partial substitutes.
logger.When(l => l.Log(Arg.Any<string>())).Do(c => cap.Add("when:" + c.Arg<string>()));
pub.Changed += (s, e) => got++;
pub.Changed += Raise.Event();

var part = Substitute.ForPartsOf<Concrete>();
part.Virtual().Returns(8);                 // Virtual() = 8; NonVirtual() runs the real code (2)
// Substitute.For<Concrete>().NonVirtual().Returns(5) -> CouldNotSetReturnDueToNoLastCallException
```

Verified: `captured: when:a,when:b,…`, `event raised handlers: 1`, `ForPartsOf virtual: 8 nonvirtual real: 2`, `non-virtual Returns: CouldNotSetReturnDueToNoLastCallException`.

</details>
