---
title: "NSubstitute"
lang: en
track: csharp
kind: library
doc: overview
slug: nsubstitute
part: 2
levels: [1, 2, 3, 4]
tags: [nsubstitute, mocking, unit-testing, test-doubles]
versions: ".NET 11, C# 15; NSubstitute 6.2.0"
status: draft
updated: 2026-11-22
---

# NSubstitute

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

NSubstitute creates test doubles of interfaces and classes on top of Castle DynamicProxy. The syntax is natural: call a method of the substitute and add `.Returns(...)`; verify with `Received()`/`DidNotReceive()`. It supports argument matchers, sequences, async, events, `When…Do` callbacks, recursive substitutes and partial substitutes of classes. Limitations: only public (or proxy-visible) types and virtual members can be substituted.

## Key ideas

- `Substitute.For<T>()`; unconfigured → default values.
- `x.Method(args).Returns(value)`; `Arg.Any/Is/Do` for arguments.
- `Received(n)`/`DidNotReceive()` to verify calls (the setup call is not counted).
- `When…Do` for `void` methods; `Throws`/`ThrowsAsync` for errors.
- Recursive substitutes for methods returning interfaces.
- Classes: only `virtual`; `ForPartsOf` runs the real code.
- `internal` types need `InternalsVisibleTo("DynamicProxyGenAssembly2")`.

## Minimal example

```csharp
var calc = Substitute.For<ICalculator>();
calc.Add(1, 2).Returns(3);
Assert.Equal(3, calc.Add(1, 2));
calc.Received().Add(1, 2);
```

## When to use and when not

- ✅ Unit tests of code with external dependencies.
- ✅ You need to verify significant side effects.
- ✅ Interfaces of clients, repositories, clocks.
- ❌ Substituting the class under test as a whole.
- ❌ Verifying the order and number of all internal calls.
- ❌ Verifying DB/network integration — you need integration tests.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | creating, `Returns`, `Received`, arguments, sequences, async |
| [2. Advanced](advanced.en.md) | callbacks, events, recursive substitutes, classes, `ForPartsOf` |
| [3. Expert](expert.en.md) | internals (the proxy), gotchas, alternatives, 11 interview questions |
| [4. Self-check](quiz.en.md) | 10 questions and [4 exercises](exercises.en.md) with solutions |
| 5. Runnable examples | not generated yet — `/learn csharp nsubstitute level 5` |
| 6. Tests | not generated yet — `/learn csharp nsubstitute level 6` |

## Related materials

- Prerequisites: [16 · Unit testing](../../modules/16-testing-unit/overview.en.md), [05 · Interfaces, structs, records](../../modules/05-interfaces-structs-records/overview.en.md)
- Nearby: [17 · Integration testing](../../modules/17-testing-integration/overview.en.md), [Testcontainers](../testcontainers/overview.en.md), [Refit](../refit/overview.en.md)
- In other tracks: [Python](../../../python/overview.en.md) — `unittest.mock`
