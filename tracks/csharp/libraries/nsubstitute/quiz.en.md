---
title: "Quiz: NSubstitute"
lang: en
track: csharp
kind: library
doc: quiz
slug: nsubstitute
part: 2
tags: [nsubstitute, mocking, unit-testing]
status: draft
updated: 2026-11-22
---

# Quiz: NSubstitute

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 How do you create a substitute of an interface?

- A) `new ICalculator()`
- B) `Substitute.For<ICalculator>()`
- C) `Mock.Of<ICalculator>()`
- D) `Activator.CreateInstance`

<details><summary>Answer</summary>

**B.** Unconfigured members return default values.
→ [Basics](basics.en.md#creating-and-configuring)

</details>

## 2. 🟢 What does an unconfigured `calc.Add(7, 7)` return?

- A) an exception
- B) `0`
- C) `null`
- D) `7`

<details><summary>Answer</summary>

**B.** Verified: the default value.
→ [Basics](basics.en.md#creating-and-configuring)

</details>

## 3. 🟢 How do you set the result `3` for `Add(1, 2)`?

- A) `calc.Setup(...)`
- B) `calc.Add(1, 2).Returns(3)`
- C) `calc.Returns(3)`
- D) `when(calc.Add(1,2)).thenReturn(3)`

<details><summary>Answer</summary>

**B.** The `Returns` extension method over the call's result.
→ [Basics](basics.en.md#creating-and-configuring)

</details>

## 4. 🟢 What does `Arg.Any<int>()` do?

- A) matches any `int`
- B) only zero
- C) creates a substitute
- D) throws

<details><summary>Answer</summary>

**A.** Verified: `Add(Arg.Any<int>(), 10)` → `100` for `Add(5, 10)`.
→ [Basics](basics.en.md#argument-matching)

</details>

## 5. 🟡 How do you perform an action when a `void` method is called?

- A) `Returns`
- B) `sub.When(x => x.Log(Arg.Any<string>())).Do(c => …)`
- C) `Throws`
- D) you cannot

<details><summary>Answer</summary>

**B.** Verified: `when:a`, `when:b`.
→ [Advanced](advanced.en.md#callbacks-whendo-and-argdo)

</details>

## 6. 🟡 What happens with `cls.NonVirtual().Returns(5)` for a class substitute?

- A) 5 is returned
- B) `CouldNotSetReturnDueToNoLastCallException`
- C) silently ignored
- D) the real method is called and returns 5

<details><summary>Answer</summary>

**B.** Verified: non-virtual members are not intercepted.
→ [Advanced](advanced.en.md#classes-and-virtual-members)

</details>

## 7. 🟡 Select all correct statements

- [ ] A) `ForPartsOf<T>` runs the real code for unconfigured virtual members
- [ ] B) `Received(3)` also counts the call inside the `Returns` setup
- [ ] C) `Returns(1, 2, 3)` repeats the last value
- [ ] D) a method returning an interface gives a recursive substitute

<details><summary>Answer</summary>

**A, C, D.** B is wrong: verified — the setup call is not counted.
→ [Expert](expert.en.md#edges-and-gotchas)

</details>

## 8. 🟡 Open question: what do you substitute in a test of an order service and what not?

<details><summary>Key points of a good answer</summary>

- substitute external dependencies (a repository, a payment gateway, a clock) through interfaces;
- do not substitute the class under test and plain values/DTOs;
- verify the result and important side effects (`Received`), not the order of internal calls;
- DB integration — separate tests.

</details>

## 9. 🔴 `Substitute.For<IInternal>()` fails with `ArgumentException`. The cause?

- A) an outdated .NET
- B) the proxy cannot see an `internal` type without `InternalsVisibleTo("DynamicProxyGenAssembly2")`
- C) the type must be `sealed`
- D) a wrong package

<details><summary>Answer</summary>

**B.** Verified by the exception text.
→ [Expert](expert.en.md#edges-and-gotchas)

</details>

## 10. 🔴 What is dangerous about tests that verify `Received.InOrder` on all calls?

- A) nothing
- B) they couple the test to the implementation and break on harmless refactoring
- C) they are slow
- D) they do not compile

<details><summary>Answer</summary>

**B.** Verify behavior and significant effects.
→ [Expert](expert.en.md#edges-and-gotchas)

</details>
