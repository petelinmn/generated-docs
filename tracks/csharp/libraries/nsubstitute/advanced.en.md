---
title: "NSubstitute: advanced"
lang: en
track: csharp
kind: library
doc: advanced
slug: nsubstitute
part: 2
tags: [nsubstitute, callbacks, events, recursive-mocks, partial-substitutes]
versions: ".NET 11, C# 15; NSubstitute 6.2.0"
status: draft
updated: 2026-11-22
---

# NSubstitute: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [basics](basics.en.md). **Time:** ~45 min.

## A result depending on arguments

```csharp
echo.Add(Arg.Any<int>(), Arg.Any<int>()).Returns(c => c.ArgAt<int>(0) + c.ArgAt<int>(1));
```

Verified: `echo.Add(20, 22)` → `42`. `c.Arg<T>()` and `c.ArgAt<T>(i)` give access to the call's arguments.

## Callbacks: `When…Do` and `Arg.Do`

```csharp
logger.When(l => l.Log(Arg.Any<string>())).Do(c => cap.Add("when:" + c.Arg<string>()));
logger.Log(Arg.Do<string>(s => cap.Add("do:" + s)));
```

`When…Do` runs on **every** matching call — including for `void` methods where `Returns` is not applicable. `Arg.Do` is an action on matching a specific configured specification. Verified: after two calls `Log("a")`, `Log("b")` the result is `when:a, when:b, do:b` — `Arg.Do` was registered by the call that configured it and fired only on the subsequent `Log("b")`.

## Recursive substitutes

If a method returns an interface, NSubstitute automatically returns a substitute of that interface:

```csharp
f.Create().Name.Returns("auto");           // IFactory.Create() returns IProduct (auto-substituted)
```

Verified: `f.Create().Name` → `"auto"`. This saves creating nested substitutes by hand but "starts" call chains — do not overuse it (the Law of Demeter).

## Events

```csharp
pub.Changed += (s, e) => got++;
pub.Changed += Raise.Event();            // raises the event
```

Verified: the handler was called (`got = 1`). For events with arguments — `Raise.EventWith(...)`/`Raise.Event<EventHandler<T>>(...)`.

> ⚠️ Verify: `Raise.EventWith` and events with typed arguments were not run in this material.

## Classes and virtual members

A **class** substitute intercepts only `virtual`/`abstract` members. Verified on `class Concrete { virtual Virtual(); NonVirtual(); }`:

| Operation | Result |
|---|---|
| `Substitute.For<Concrete>()` → `v.Virtual().Returns(7)` | `7` |
| `cls.NonVirtual().Returns(5)` | `CouldNotSetReturnDueToNoLastCallException` |
| `Substitute.ForPartsOf<Concrete>()` → `part.Virtual().Returns(8)` | `Virtual() = 8`, while `NonVirtual()` runs the **real** code (`2`) |

`ForPartsOf` is a "partial substitute": the real code runs if a member is not configured. The class constructor is called.

## Asynchronous members

`Returns("two")` for `Task<string?>` wraps the value; `ThrowsAsync` sets an error on `await` ([basics](basics.en.md#sequences-exceptions-async)).

## Best practices

| Rule | Why |
|---|---|
| Substitute interfaces | cleaner and without `virtual` restrictions |
| Verify the result, not the calls | tests do not break on refactoring |
| `Received` — for essential side effects | sending an email, a write |
| Recursive substitutes — in moderation | do not hide coupling |
| One test — one behavior | clarity |
| Common setup in a factory method/constructor | less duplication |

## Related materials

- Back: [basics](basics.en.md). Next: [expert](expert.en.md).
- [16 · Unit testing](../../modules/16-testing-unit/overview.en.md)
- [17 · Integration testing](../../modules/17-testing-integration/overview.en.md) — when substitutes are not enough.
- [Refit](../refit/overview.en.md) — client interfaces as ideal candidates for substitutes.

## Sources

- [NSubstitute: Callbacks, void calls and When..Do](https://nsubstitute.github.io/help/callbacks/)
- [NSubstitute: Auto-values and recursive mocks](https://nsubstitute.github.io/help/auto-values/)
- [NSubstitute: Partial subs](https://nsubstitute.github.io/help/partial-subs/)
- [NSubstitute: Raising events](https://nsubstitute.github.io/help/raise-event/)
