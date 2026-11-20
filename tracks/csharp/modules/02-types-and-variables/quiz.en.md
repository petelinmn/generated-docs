---
title: "Quiz: Types and variables"
lang: en
track: csharp
kind: module
doc: quiz
slug: 02-types-and-variables
part: 1
tags: [types, value-types, reference-types, nullable-reference-types, boxing, floating-point]
status: verified
updated: 2026-11-20
---

# Quiz: Types and variables

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 Which of these types are value types?

- [ ] A) `int`
- [ ] B) `string`
- [ ] C) `DateTime` (a struct)
- [ ] D) `int[]`

<details><summary>Answer</summary>

**A, C.** Numeric types and structs are value types. `string` and arrays (even an array of `int`) are reference types.
→ [Basics](basics.en.md)

</details>

## 2. 🟢 What does the method return?

```csharp
var original = new PointStruct { X = 1 };
var copy = original;
copy.X = 99;
return (original.X, copy.X);
```

`PointStruct` is a struct.

- A) `(1, 99)`
- B) `(99, 99)`
- C) `(1, 1)`
- D) A compile error

<details><summary>Answer</summary>

**A.** Assigning a struct copies the whole value, so `copy` is an independent copy. If `PointStruct` were a class, the result would be `(99, 99)`.
→ [Basics](basics.en.md)

</details>

## 3. 🟢 What is `(int)2.9`?

- A) `3`
- B) `2`
- C) `2.9`
- D) A compile error: `Convert.ToInt32` is required

<details><summary>Answer</summary>

**B.** An explicit `double` → `int` cast drops the fractional part (toward zero): `(int)-2.9` → `-2`. `Math.Round` is what rounds.
→ [Basics](basics.en.md)

</details>

## 4. 🟢 What happens on `int.MaxValue + 1` (values come from variables, default project settings)?

- A) `OverflowException`
- B) The result becomes a `long`
- C) The result wraps around to `int.MinValue`
- D) The result is `int.MaxValue`

<details><summary>Answer</summary>

**C.** By default integer arithmetic is unchecked, and an overflow silently wraps around. An exception is thrown only by `checked(...)` or the `CheckForOverflowUnderflow` project setting.
→ [Basics](basics.en.md)

</details>

## 5. 🟡 What do `0.1 + 0.2 == 0.3` for `double` and `0.1m + 0.2m == 0.3m` for `decimal` return?

- A) `true` and `true`
- B) `false` and `true`
- C) `false` and `false`
- D) `true` and `false`

<details><summary>Answer</summary>

**B.** `double` is binary, and 0.1 cannot be represented exactly in it: the sum is `0.30000000000000004`. `decimal` stores decimal digits, so the sum is exact.
→ [Advanced](advanced.en.md)

</details>

## 6. 🟡 What does the method return?

```csharp
object a = 42;
object b = 42;
return (ReferenceEquals(a, b), a.Equals(b), a == b);
```

- A) `(true, true, true)`
- B) `(false, true, true)`
- C) `(false, true, false)`
- D) `(false, false, false)`

<details><summary>Answer</summary>

**C.** Each assignment of an `int` to an `object` creates a new box (boxing). The references differ, so `ReferenceEquals` and `==` (which compares references for `object`) return `false`. `Equals` compares values and returns `true`.
→ [Advanced](advanced.en.md)

</details>

## 7. 🟡 Find the bug

```csharp
object boxed = 42;
long value = (long)boxed;
```

<details><summary>Answer</summary>

The box holds an `int`, but it is unboxed as `long`. Unboxing requires the exact type, so the code throws `InvalidCastException`. The fix: `long value = (long)(int)boxed;` — unbox as `int` first, then widen to `long`.
→ [Advanced](advanced.en.md)

</details>

## 8. 🟡 Select all correct statements about nullable reference types

- [ ] A) `string?` and `string` are different types at run time
- [ ] B) Accessing members of a `string?` without a check produces warning CS8602
- [ ] C) The `!` operator checks the value for null at run time
- [ ] D) After `if (text is null) return;` the compiler treats `text` as non-null

<details><summary>Answer</summary>

**B, D.** `string?` is an annotation for the compiler; at run time there is a single type (A is wrong). `!` only suppresses the warning; there is no check (C is wrong). Flow analysis understands null checks (D is right).
→ [Advanced](advanced.en.md)

</details>

## 9. 🟡 Why can `"i".ToUpper()` return something other than `"I"`?

- A) `ToUpper` works only with ASCII
- B) Without arguments it uses the current culture, and in the Turkish culture the uppercase `i` is `İ`
- C) Strings are immutable, so `ToUpper` returns the original string
- D) It is a bug in .NET

<details><summary>Answer</summary>

**B.** `ToUpper()` without arguments depends on the OS culture. For identifiers and comparisons use `ToUpperInvariant()` or `StringComparison.OrdinalIgnoreCase`.
→ [Advanced](advanced.en.md)

</details>

## 10. 🔴 What does the method return?

```csharp
string a = "hello";
string b = "hel" + "lo";
string c = prefix + "lo";   // prefix == "hel", known only at run time
return (ReferenceEquals(a, b), ReferenceEquals(a, c), a == c);
```

- A) `(true, true, true)`
- B) `(false, false, true)`
- C) `(true, false, true)`
- D) `(true, false, false)`

<details><summary>Answer</summary>

**C.** `"hel" + "lo"` is a constant: the compiler folds it into the literal `"hello"`, and literals are interned. Concatenation at run time creates a new object. `==` on `string` compares contents.
→ [Expert](expert.en.md)

</details>

## 11. 🔴 Which type does `((object)(int?)5).GetType()` return?

- A) `System.Nullable<System.Int32>`
- B) `System.Int32`
- C) `System.Object`
- D) It throws `NullReferenceException`

<details><summary>Answer</summary>

**B.** When boxing a `Nullable<T>`, the runtime boxes the inner value, and an "empty" `int?` becomes a null reference. A boxed `Nullable<int>` never exists.
→ [Expert](expert.en.md)

</details>

## 12. 🔴 Open question

A colleague says: "value types are always stored on the stack, that's why they are faster". What is wrong with this statement?

<details><summary>Key points of a good answer</summary>

- A value is stored where its variable is declared: a struct field inside a class is on the heap, array elements are on the heap, a local captured by a lambda is in a field of a hidden class on the heap.
- The essence of the difference is semantics: copying by value versus sharing by reference.
- "Faster" is not always true: a large struct is expensive to copy, and boxing creates allocations.
- The storage location is an implementation detail that the JIT may optimize.

</details>
