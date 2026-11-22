---
title: "NSubstitute: expert"
lang: en
track: csharp
kind: library
doc: expert
slug: nsubstitute
part: 2
tags: [nsubstitute, castle-dynamicproxy, test-doubles, mocking-pitfalls, interview-questions]
versions: ".NET 11, C# 15; NSubstitute 6.2.0"
status: draft
updated: 2026-11-22
---

# NSubstitute: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

**Prerequisites:** [basics](basics.en.md) and [advanced](advanced.en.md). **Time:** ~30 min.

## Internals

`Substitute.For<T>()` creates a dynamic proxy based on **Castle DynamicProxy**: at runtime a class inheriting `T` (or implementing the interface) is generated, and every member call is intercepted. The intercepted call goes to a router (`CallRouter`) which: records the call in the history; looks for a matching configured specification (`Returns`, `Throws`, `When…Do`) and picks the result; for an unconfigured member returns an "auto value" — a default value, a recursive substitute for interfaces, a completed task for `Task`.

`.Returns(x)` is an extension method over the **result** of a call, so it does not know which call it configures: the library takes the substitute's "last call" in the current thread (`ThreadLocalContext`). Hence the limitations verified by running: configuring a non-virtual member gives `CouldNotSetReturnDueToNoLastCallException` ("Could not find a call to return from"), and a call on **another** substitute inside `Returns(...)` can disturb the "last call".

> ⚠️ Verify: the details of internal types (`CallRouter`, `ThreadLocalContext`) come from the exception text and the NSubstitute documentation; the sources were not studied.

## Edges and gotchas

**1. Types must be accessible to the proxy.** Verified: an `ICalculator` without `public` → `ArgumentException: Can not create proxy for type ICalculator because it is not accessible. Make it public, or internal and mark your assembly with [assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]`.

**2. Non-virtual members are not intercepted.** Verified: `CouldNotSetReturnDueToNoLastCallException`; with `ForPartsOf` a non-virtual member runs for real (`2`).

**3. Configuration inside `Returns(...)`.** Do not call other substitutes in the argument of `Returns` — compute the value beforehand (the library itself warns about this in the exception text).

**4. The call counter.** Verified: `Received(3)` matched three real calls; the call used to configure `Returns` is not counted. But the calls inside the checks themselves (`Received().Add(...)`) are not calls of the real logic.

**5. `Received` without argument matchers.** `calc.Received().Add(1, 2)` checks *specific* arguments; for "any" use `Arg.Any<T>()`.

**6. Sequences.** Verified: `Returns(1, 2, 3)` — after three values the last repeats (`1, 2, 3, 3`).

**7. Auto values and `null`.** Verified: an unconfigured `Task<string?>` method returned `null` inside a completed task; for non-nullable reference types be careful with `null`.

**8. Parallel tests.** Substitutes are thread-safe but the "last call" is per thread; do not configure a substitute from several threads at once.

**9. Over-specified tests.** Substituting every call and verifying the order (`Received.InOrder`) couples the test to the implementation — fragility.

> ⚠️ Verify: `Received.InOrder` was not run in this material.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| NSubstitute | readable syntax, no lambdas for setup | a runtime proxy, no substituting `static`/`sealed`/non-virtual |
| Moq | mature, many examples | a different (lambda) syntax, a dependency history |
| FakeItEasy | a similar fluent syntax | a smaller community |
| Hand-written stubs (fake classes) | full control, no magic | more code |
| Real dependencies (in-memory, [Testcontainers](../testcontainers/overview.en.md)) | test real integration | slower, more complex |

## Interview questions

<details><summary>1. What is a substitute (substitute/mock/stub/fake)?</summary>

A test double of a dependency: a stub returns given values, a mock also verifies calls, a fake is a simplified working implementation.

</details>

<details><summary>2. How does NSubstitute create substitutes?</summary>

Through Castle DynamicProxy: at runtime a subclass/interface implementation that intercepts calls is generated.

</details>

<details><summary>3. Why can't you configure a non-virtual method of a class?</summary>

The proxy intercepts only `virtual`/`abstract` members; otherwise the real code runs and `Returns` finds no "last call" (verified: `CouldNotSetReturnDueToNoLastCallException`).

</details>

<details><summary>4. How does `Substitute.For<T>()` differ from `ForPartsOf<T>()`?</summary>

`For` substitutes all members (defaults for the unconfigured); `ForPartsOf` runs the real code for unconfigured virtual members (verified: the configured `8`, the non-virtual real `2`).

</details>

<details><summary>5. How do you verify that a method was called exactly N times?</summary>

`sub.Received(N).Method(args)` (verified: `Received(3)`); `DidNotReceive()` — never.

</details>

<details><summary>6. What does an unconfigured method return?</summary>

A default value: `0`, `null`, for interfaces — a recursive substitute, for `Task` — a completed task (verified).

</details>

<details><summary>7. How do you set different results for successive calls?</summary>

`Returns(1, 2, 3)`: the last value repeats (verified `1, 2, 3, 3`).

</details>

<details><summary>8. How do you perform an action when a `void` method is called?</summary>

`sub.When(x => x.Method(...)).Do(callInfo => …)` (verified).

</details>

<details><summary>9. Trick question: `Substitute.For<IMyInternalInterface>()` fails with `ArgumentException`. Why?</summary>

The proxy cannot see an `internal` type; add `[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]` or make the type public (verified).

</details>

<details><summary>10. Trick question: `Received(2)` fails although the test "makes two calls". What could you have missed?</summary>

Extra calls with the same arguments (for example printing/logging in the test itself — verified: three calls instead of two); the call used to configure `Returns` is not counted.

</details>

<details><summary>11. When are substitutes harmful?</summary>

When a test checks the implementation (order, number of calls) instead of the result, or when interaction with a DB/external service is being tested — you need integration tests.

</details>

## Related materials

- Back: [advanced](advanced.en.md). Check yourself: [quiz](quiz.en.md) and [exercises](exercises.en.md).
- [16 · Unit testing](../../modules/16-testing-unit/overview.en.md)
- [17 · Integration testing](../../modules/17-testing-integration/overview.en.md)
- [20 · Reflection and source generators](../../modules/20-reflection-and-source-generators/overview.en.md) — dynamic proxies.

## Sources

- [NSubstitute: documentation](https://nsubstitute.github.io/)
- [Castle DynamicProxy](https://www.castleproject.org/projects/dynamicproxy/)
- [NSubstitute: Creating a substitute](https://nsubstitute.github.io/help/creating-a-substitute/)
