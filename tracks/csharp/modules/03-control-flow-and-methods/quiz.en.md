---
title: "Quiz: Control flow and methods"
lang: en
track: csharp
kind: module
doc: quiz
slug: 03-control-flow-and-methods
part: 1
tags: [control-flow, switch-expression, loops, ref-out-in, overloading, closures]
status: verified
updated: 2026-11-20
---

# Quiz: Control flow and methods

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What happens if a `case 1:` section of a `switch` statement contains code and does not end with `break`?

- A) Execution "falls through" into the next section, as in C
- B) A compile error
- C) A compiler warning; the code runs
- D) A run-time exception

<details><summary>Answer</summary>

**B.** C# has no implicit fall-through: a section with code must end with a statement after which execution cannot reach its end: `break`, `return`, `throw`, `goto case`/`goto default`, `continue` inside a loop. Empty labels in a row (`case 1: case 2:`) are allowed.
→ [Basics](basics.en.md)

</details>

## 2. 🟢 What does `Classify(20)` return?

```csharp
public static string Classify(int celsius) => celsius switch
{
    < 0 => "freezing",
    >= 0 and < 20 => "cold",
    >= 20 and < 30 => "warm",
    _ => "hot",
};
```

- A) `cold`
- B) `warm`
- C) `hot`
- D) `SwitchExpressionException`

<details><summary>Answer</summary>

**B.** `20` does not match `< 20`, but it matches `>= 20 and < 30`. Arms are checked in order, and the first matching one wins.
→ [Basics](basics.en.md)

</details>

## 3. 🟢 Which loop is guaranteed to run its body at least once?

- A) `for`
- B) `while`
- C) `do … while`
- D) `foreach`

<details><summary>Answer</summary>

**C.** `do … while` checks the condition after the body. That is why `CountDigits(0)` in the examples returns `1`.
→ [Basics](basics.en.md)

</details>

## 4. 🟢 What does `Greet(greeting: "Hi", name: "Bob")` return for `Greet(string name, string greeting = "Hello")`?

- A) `Hello, Bob!`
- B) `Hi, Bob!`
- C) `Bob, Hi!`
- D) A compile error: the argument order does not match

<details><summary>Answer</summary>

**B.** Named arguments can be passed in any order; they are matched by parameter name.
→ [Basics](basics.en.md)

</details>

## 5. 🟢 What does `FindFirst(grid, 8)` return (C# 15)?

```csharp
int[][] grid = [[1, 2, 3], [4, -5, 6], [7, 8, 5]];
```

`FindFirst` walks rows and columns with two nested loops. On a match it runs `found = (row, column); break outer;`, where `outer:` is the label of the outer loop.

- A) `(2, 1)`
- B) `(2, 2)`
- C) `null`
- D) A compile error: `break` does not take a label

<details><summary>Answer</summary>

**A.** `8` is in row 2, column 1. `break outer` (C# 15) ends the outer loop at once, so the search stops at the first match. Before C# 15, option D would have been correct: `break` with a label appeared only in C# 15.
→ [Basics](basics.en.md)

</details>

## 6. 🟡 What do the methods return?

```csharp
var getters = new List<Func<int>>();
for (int i = 1; i <= 3; i++) getters.Add(() => i);
// A: getters.Select(g => g())

var getters2 = new List<Func<int>>();
foreach (int i in new[] { 1, 2, 3 }) getters2.Add(() => i);
// B: getters2.Select(g => g())
```

- A) A = `[1, 2, 3]`, B = `[1, 2, 3]`
- B) A = `[4, 4, 4]`, B = `[1, 2, 3]`
- C) A = `[3, 3, 3]`, B = `[3, 3, 3]`
- D) A = `[4, 4, 4]`, B = `[4, 4, 4]`

<details><summary>Answer</summary>

**B.** A lambda captures the variable. In `for` there is one `i` for the whole loop, and after the loop it equals 4. In `foreach`, since C# 5, the variable is new on every iteration.
→ [Advanced](advanced.en.md)

</details>

## 7. 🟡 Which overload is called for `Describe('a')` if there are `Describe(int)`, `Describe(long)`, `Describe(double)` and `Describe(object)`?

- A) `object` — `char` is not a number
- B) `int`
- C) `long`
- D) A compile error: ambiguous call

<details><summary>Answer</summary>

**B.** `char` converts implicitly to `int`, `long` and `double`. Of these, `int` wins: it converts implicitly to `long` and `double`, but not the other way round. A test checks this in [Ex05_Overloading](code/Examples/Ex05_Overloading.cs).
→ [Advanced](advanced.en.md)

</details>

## 8. 🟡 Select all correct statements about parameter modifiers

- [ ] A) A method with an `out` parameter must assign it before returning
- [ ] B) A `ref` argument must be initialized before the call
- [ ] C) An `in` parameter can be changed inside the method
- [ ] D) `params ReadOnlySpan<int>` (C# 13) allows calling the method as `M(1, 2, 3)`

<details><summary>Answer</summary>

**A, B, D.** `in` is a read-only reference, so the parameter cannot be changed (C is wrong). Since C# 13, `params` works with collections, including spans.
→ [Advanced](advanced.en.md)

</details>

## 9. 🟡 Find the bug

```csharp
public static void RemoveNegatives(List<int> values)
{
    foreach (int value in values)
    {
        if (value < 0)
        {
            values.Remove(value);
        }
    }
}
```

<details><summary>Answer</summary>

Changing a `List<T>` while `foreach` iterates it invalidates the enumerator: the next `MoveNext` throws `InvalidOperationException`. The fix is `values.RemoveAll(v => v < 0)` or a backwards `for` loop by index.
→ [Advanced](advanced.en.md)

</details>

## 10. 🟡 What happens when `Math.Abs(int.MinValue)` is called?

- A) It returns `int.MaxValue`
- B) It returns `int.MinValue`
- C) It throws `OverflowException`
- D) It returns `2147483648` as a `long`

<details><summary>Answer</summary>

**C.** The `int` range is −2147483648 to 2147483647: the minimum value has no positive counterpart. In this case `Math.Abs` throws `OverflowException`; the `DigitSum(int.MinValue)` test checks this.
→ [Advanced](advanced.en.md)

</details>

## 11. 🔴 What happens when `Action((TrafficLight)42)` is called?

```csharp
public enum TrafficLight { Red, Yellow, Green }

public static string Action(TrafficLight light) => light switch
{
    TrafficLight.Red => "stop",
    TrafficLight.Yellow => "wait",
    TrafficLight.Green => "go",
};
```

- A) A compile error: `42` is not a `TrafficLight` value
- B) It returns `null`
- C) It throws `SwitchExpressionException`
- D) It returns `"go"` — the last arm

<details><summary>Answer</summary>

**C.** The cast `(TrafficLight)42` is legal: an `enum` can hold any value of its underlying type. The compiler warns (CS8524) and generates an arm that throws `SwitchExpressionException`. It is better to write `_ => throw new ArgumentOutOfRangeException(…)` explicitly.
→ [Expert](expert.en.md)

</details>

## 12. 🔴 What does a type need in order to be iterated with `foreach`?

- A) Implement `IEnumerable<T>`
- B) Implement `IEnumerable` or `IEnumerable<T>`
- C) Have an accessible `GetEnumerator()` method whose result has `MoveNext()` and `Current`
- D) Be an array or a collection from `System.Collections`

<details><summary>Answer</summary>

**C.** `foreach` works by pattern; an interface is not required. This is how `List<T>.Enumerator` (a struct, no allocations) and the [Ex10_DuckTypedForeach](code/Examples/Ex10_DuckTypedForeach.cs) example work.
→ [Expert](expert.en.md)

</details>

## 13. 🔴 Open question

Library v1 contains `public static string Greet(string name, string greeting = "Hello")`. In v2 the default was changed to `"Hi"`, and the app's DLL was updated without recompiling the app. What does `Greet("Ann")` print in the app, and why?

<details><summary>Key points of a good answer</summary>

- It prints `Hello, Ann!`: the default value is substituted into the calling assembly's code when that assembly is compiled.
- The app passes `"Hello"` explicitly; the new DLL simply receives this argument.
- After the app is recompiled, it prints `Hi, Ann!`.
- In public APIs, adding an overload is safer than changing a default value.

</details>
