---
title: "Quiz: Modern C#"
lang: en
track: csharp
kind: module
doc: quiz
slug: 11-modern-csharp
part: 2
tags: [pattern-matching, switch-expression, primary-constructor, collection-expressions, required-members, union-types]
status: draft
updated: 2026-11-22
---

# Quiz: Modern C#

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What does `Classify(7)` return?

```csharp
static string Classify(int n) => n switch
{
    < 0 => "negative",
    0 => "zero",
    >= 1 and < 10 => "digit",
    _ => "big",
};
```

- A) `"negative"`
- B) `"digit"`
- C) `"big"`
- D) a `SwitchExpressionException`

<details><summary>Answer</summary>

**B.** Arms are checked top to bottom; `7` is not below zero, not equal to zero and matches `>= 1 and < 10`. The `_` arm is a fallback that fires only if nothing above matched, so C is wrong. D would happen without `_` for a value that matched no arm.
→ [Basics](basics.en.md#the-switch-expression)

</details>

## 2. 🟢 Which expression correctly checks "`o` is a positive integer" and yields the variable `n`?

- A) `if (o is int n && n > 0)`
- B) `if (o is int n || n > 0)`
- C) `if (o as int n)`
- D) `if (o == int n)`

<details><summary>Answer</summary>

**A.** The type pattern `is int n` declares a variable that is definitely assigned only in the branch where the match happened, so the condition `n > 0` is appended with `&&`. In B the variable `n` is unassigned if the first part is false — a compile error. C and D are not valid syntax.
→ [Basics](basics.en.md#pattern-matching)

</details>

## 3. 🟢 How does a primary-constructor parameter differ from a property?

- A) it does not: the compiler always makes it a public property
- B) it is a parameter available in the type body; if a method uses it, the compiler creates a hidden field, but there is no public member (for a `record` there is)
- C) it is a compile-time constant
- D) it is available only in field and property initializers

<details><summary>Answer</summary>

**B.** Only a `record` creates a public property from a parameter. In ordinary `class`/`struct` types the parameter can be used in methods — then it becomes a hidden field (in the verified example `Deposit` changes `balance`) — or explicitly copied into a property. D is wrong: it is available in methods too.
→ [Basics](basics.en.md#primary-constructors)

</details>

## 4. 🟢 Select all correct statements

- [ ] A) `List<int> l = [1, 2, 3];` is valid code
- [ ] B) `int[] a = [..b, 0];` inserts the elements of collection `b` into the array
- [ ] C) a `required` property may be left unset if it has `init`
- [ ] D) an `init` property can be changed at any time after the object is created

<details><summary>Answer</summary>

**A, B.** A is a collection expression with the target type `List<int>`. B is a spread. C is wrong: `required` obliges you to set the value (otherwise `CS9035`). D is wrong: `init` allows assignment only during creation (otherwise `CS8852`; verified).
→ [Basics](basics.en.md#required-and-init)

</details>

## 5. 🟢 What does `$$"""…"""` do?

- A) creates a string where `{{ }}` is interpolation and single `{` are ordinary characters
- B) creates a string in which `$` must be escaped
- C) creates two copies of the string
- D) throws an error: such a literal does not exist

<details><summary>Answer</summary>

**A.** The number of `$` signs sets how many braces open an interpolation: with `$$` — two. So in a JSON template the single `{`/`}` do not interfere.
→ [Basics](basics.en.md#raw-strings-and-lambda-parameters)

</details>

## 6. 🟡 What does the code print?

```csharp
int[] arr = [1, 2, 3, 4, 5];
Console.WriteLine(arr is [1, _, 3, ..]);
Console.WriteLine(arr is [_, _]);
if (arr is [var first, .. var mid, var last])
    Console.WriteLine($"{first} {string.Join(",", mid)} {last}");
```

<details><summary>Answer</summary>

```text
True
False
1 2,3,4 5
```

`[1, _, 3, ..]` — the first three elements matched, the rest are arbitrary. `[_, _]` requires exactly two elements. In the third pattern `first` and `last` are the outer elements, and `mid` is a slice of the remaining three.
→ [Advanced theory](advanced.en.md#list-patterns)

</details>

## 7. 🟡 In what order must the arms be placed for the code to compile?

```csharp
string s = n switch
{
    int => "int",
    1 => "one",
};
```

- A) the code is correct
- B) the specific arm `1` must be placed above `int`
- C) `1` must be replaced with `== 1`
- D) `int` must be replaced with `_`

<details><summary>Answer</summary>

**B.** A general pattern above a specific one makes the specific one unreachable — error `CS8510` (verified). Put `1 => "one"` before `int => "int"`. D does not help: `_` swallows everything too.
→ [Advanced theory](advanced.en.md#nested-properties-and-guards)

</details>

## 8. 🟡 Select all correct statements about collection expressions

- [ ] A) `IEnumerable<int> e = [1, 2];` creates a hidden type that cannot be cast to `List<int>`
- [ ] B) `[..a, ..b]` evaluates each operand once
- [ ] C) an empty `[]` for an array creates a new array every time
- [ ] D) a custom type can support `[…]` through `[CollectionBuilder]`

<details><summary>Answer</summary>

**A, B, D.** A: in the verified run `<>z__ReadOnlyArray` was created. B: each spread expression is evaluated once. C is wrong: an empty array is the cached `Array.Empty<T>()` (`ReferenceEquals` returned `True`). D is right.
→ [Advanced theory](advanced.en.md#collection-expressions-types-and-spread)

</details>

## 9. 🟡 What happens with `h?.Value = 5` if `h` is `null`?

- A) `NullReferenceException`
- B) nothing: the assignment and the evaluation of the right-hand side are skipped
- C) a new object `h` is created
- D) a compile error

<details><summary>Answer</summary>

**B.** Null-conditional assignment (C# 14) evaluates the right-hand side only if `h` is not `null`. In the verified example nothing happened for `null`, and after the object was created `Value` became `7`.
→ [Advanced theory](advanced.en.md#null-conditional-assignment)

</details>

## 10. 🟡 Open question: how do you add value validation to a property with `field`, and why is it better than a separate field?

<details><summary>Key points of a good answer</summary>

- `field` refers to the property's automatic backing field: `public double Celsius { get; set => field = Math.Max(value, -273.15); }`;
- there is no separate private field that could be changed by accident, bypassing the check;
- works with `get`, `set` and `init`; you can leave one of the accessors automatic;
- if the type has its own member named `field`, write `@field` or `this.field`;
- the feature appeared in C# 14.

</details>

## 11. 🔴 What does the code print?

```csharp
var w = new Weird();   // operator == always returns true
Console.WriteLine($"{w == null}; {w is null}");
```

- A) `True; True`
- B) `True; False`
- C) `False; False`
- D) `False; True`

<details><summary>Answer</summary>

**B.** `w == null` calls the overloaded operator, while `w is null` is a pattern that checks the reference against `null` directly and ignores the overload. Verified by running.
→ [Expert theory](expert.en.md#how-patterns-are-compiled)

</details>

## 12. 🔴 Find the bug

```csharp
static T Create<T>() where T : new() => new T();

class Req { public required string Name { get; init; } }

var r = Create<Req>();
```

<details><summary>Answer</summary>

Compile error `CS9040`: a type with `required` members does not satisfy the `new()` constraint, because generic code cannot set the required properties. Solutions: remove `required` from the type, or give up `new()` and pass a `Func<T>` factory.
→ [Expert theory](expert.en.md#how-primary-constructors-and-collections-are-compiled)

</details>
