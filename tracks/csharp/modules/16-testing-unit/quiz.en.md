---
title: "Quiz: Unit tests"
lang: en
track: csharp
kind: module
doc: quiz
slug: 16-testing-unit
part: 2
tags: [unit-testing, xunit, theory, fixtures, mocking, parallelism]
status: draft
updated: 2026-11-22
---

# Quiz: Unit tests

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What does AAA mean in unit tests?

- A) Always Assert Anything
- B) Arrange – Act – Assert: set up, act, check
- C) Async – Await – Assert
- D) Abstract – Apply – Accept

<details><summary>Answer</summary>

**B.** The test structure: prepare data, perform the action under test, check the result.
→ [Basics](basics.en.md#the-first-test)

</details>

## 2. 🟢 What is the argument order in `Assert.Equal`?

- A) `(actual, expected)`
- B) `(expected, actual)`
- C) it does not matter
- D) only one argument

<details><summary>Answer</summary>

**B.** The expected value goes first; then the message "Expected: 4, Actual: 5" does not mislead (verified).
→ [Basics](basics.en.md#assertions-assert)

</details>

## 3. 🟢 How many test cases will the report show?

```csharp
[Theory]
[InlineData(1, 2, 3)]
[InlineData(-1, 1, 0)]
[InlineData(0, 0, 0)]
public void Add_Theory(int a, int b, int expected) => Assert.Equal(expected, new Calculator().Add(a, b));
```

- A) 1
- B) 3
- C) 6
- D) 0

<details><summary>Answer</summary>

**B.** Each data row is a separate test case (verified: a run by `*Add_Theory*` gave 3 tests).
→ [Basics](basics.en.md#parameterized-tests-theory)

</details>

## 4. 🟢 What does the code show?

```csharp
public class LifecycleTests
{
    private readonly Counter _counter = new();
    [Fact] public void A() { _counter.Inc(); Assert.Equal(1, _counter.Value); }
    [Fact] public void B() { _counter.Inc(); Assert.Equal(1, _counter.Value); }
}
```

- A) `A` passes, `B` fails (the value is 2)
- B) both pass: a new class instance is created for each test
- C) both fail
- D) the order is random, the result is unstable

<details><summary>Answer</summary>

**B.** xUnit creates a new instance of the test class for each test, so fields are not shared (verified).
→ [Basics](basics.en.md#test-isolation)

</details>

## 5. 🟡 How many times is `SharedFixture` created in a class of two tests with `IClassFixture<SharedFixture>`?

- A) 2
- B) 1
- C) 0
- D) by the number of cores

<details><summary>Answer</summary>

**B.** A class fixture is created once and passed to the constructor; both tests saw `Id == 1` (verified).
→ [Advanced theory](advanced.en.md#fixtures-shared-state)

</details>

## 6. 🟡 What does `mailer.Received(1).Send("ann", "Good morning, ann")` do?

- A) calls `Send`
- B) verifies that `Send` was called exactly once with these arguments
- C) sets a return value
- D) creates a mock

<details><summary>Answer</summary>

**B.** `Received(n)` is an interaction check on an NSubstitute mock; `Returns` sets the return value of a stub; `Substitute.For<T>()` creates a double.
→ [Advanced theory](advanced.en.md#test-doubles)

</details>

## 7. 🟡 Select all correct statements

- [ ] A) `--filter-trait "Category=Slow"` runs only tests with that label
- [ ] B) coverage is also counted for code that lies in the test project itself
- [ ] C) test classes run in parallel by default
- [ ] D) `TestContext.Current.CancellationToken` lets you interrupt an asynchronous call when the run is stopped

<details><summary>Answer</summary>

**A, C, D.** B is wrong: the coverage report was empty when the code under test was in the test assembly (verified); move the code to a separate project. C: three classes reached 3 concurrent executions.
→ [Advanced theory](advanced.en.md#code-coverage)

</details>

## 8. 🟡 How do you make a test that depends on the current time deterministic?

- A) compare with `DateTime.Now` in the test
- B) introduce time through an interface (`IClock`/`TimeProvider`) and substitute a fixed value
- C) run the tests at night
- D) add `Thread.Sleep`

<details><summary>Answer</summary>

**B.** The clock dependency is replaced by a double: `clock.Now.Returns(new DateTime(2026, 1, 1, 9, 0, 0))`, and the result does not depend on the moment of the run.
→ [Advanced theory](advanced.en.md#controlling-nondeterminism)

</details>

## 9. 🟡 Open question: what is a good unit test and what should it avoid?

<details><summary>Key points of a good answer</summary>

- fast, independent, repeatable, self-checking; one scenario — one idea;
- checks **behavior** (the result, meaningful side effects), not implementation details;
- the name describes the method, the scenario and the expectation;
- avoid: order dependence, static state, real time, the network, `Thread.Sleep`, over-checking mock calls;
- boundary values and errors — in a `Theory`.

</details>

## 10. 🔴 What will happen?

```csharp
static void Boom() => throw new InvalidOperationException();
[Fact] public void T() => Assert.Throws<Exception>(Boom);
```

- A) the test passes: `InvalidOperationException` derives from `Exception`
- B) the test fails: `Throws<T>` requires an exact type match
- C) a compile error
- D) the test is skipped

<details><summary>Answer</summary>

**B.** "Exception type was not an exact match" (verified). For derived types — `Assert.ThrowsAny<Exception>`.
→ [Expert theory](expert.en.md#assertions-and-their-semantics)

</details>

## 11. 🔴 Two tests change a shared static counter and both check `Assert.Equal(1, Static.N)`. What happens when the whole suite runs?

- A) both pass
- B) one passes, the other fails: the state is shared across tests
- C) both always fail
- D) xUnit resets the static automatically

<details><summary>Answer</summary>

**B.** Static state lives for the whole process and is shared by all tests; xUnit does not reset it (verified: `Actual: 2` in one of the tests). Do not use statics in tests.
→ [Expert theory](expert.en.md#gotchas)

</details>
