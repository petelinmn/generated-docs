---
title: "Modern C#: expert"
lang: en
track: csharp
kind: module
doc: expert
slug: 11-modern-csharp
part: 2
tags: [pattern-matching, union-types, switch-expression, collection-expressions, primary-constructor, extension-members, required-members, interview-questions]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Modern C#: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

**Prerequisites:** [basic](basics.en.md) and [advanced](advanced.en.md) theory. **Time:** ~1.5 h.

## Union types (C# 15)

A union declares a type whose value is exactly one of the listed **case types** and lets the compiler check `switch` exhaustiveness:

```csharp
public union Pet(Cat, Dog);
public record Cat(string Name);
public record Dog(string Name);

Pet p = new Cat("Tom");                       // implicit conversion from a case type
string d = p switch { Cat c => $"cat {c.Name}", Dog g => $"dog {g.Name}" };   // exhaustive: no discard needed
Console.WriteLine(d);                         // cat Tom
Console.WriteLine(p.Value?.GetType().Name);   // Cat
```

Verified on SDK 11.0.100 (no extra flags). What running established:

- a union is a **struct** (`IsValueType = True`) marked with `UnionAttribute` and implementing `IUnion`;
- the contents are accessed through the `Value` property of type `object?`; for `default(Pet)` it is `null` — an "empty" union is possible, as with any struct;
- a `switch` without a `_` arm over both cases gives no warning; if one case is removed, the compiler reports `CS8509` and names the uncovered case (`Dog`);
- `p is Dog` works as an ordinary type check.

Unions can be generic: `public union Result<T>(Ok<T>, Error);` — verified in [exercise 6](exercises.en.md). This is the most significant extension of the type system in years: before it, variants were modeled with `abstract record` hierarchies or libraries such as OneOf. The feature is new, so check the "What's new in C# 15" page before using it in public APIs.

## How patterns are compiled

- A `switch` expression turns into a sequence of tests or, for dense integer constants, a jump table; the compiler builds a decision tree and **does not repeat** identical tests between arms.
- An unreachable arm is error `CS8510`; incomplete coverage is warning `CS8509`; at run time it becomes a `SwitchExpressionException`:

```csharp
static int Half(int x) => x switch { 0 => 0, 2 => 1 };
try { Console.WriteLine(Half(5)); }
catch (Exception e) { Console.WriteLine(e.GetType().Name + ": " + e.Message); }
```

```text
SwitchExpressionException: Non-exhaustive switch expression failed to match its input.
Unmatched value was 5.
```

- For an `enum`, values outside the enumeration (`(Color)42`) are also an "uncovered" case, so even when all constants are listed, a `switch` without `_` gives warning `CS8524` (the value can be any `int`; verified).
- The patterns `is null` and `is not null` **do not call** an overloaded `==`. Verified on a type whose `==` always returns `true`: `w == null` is `True`, while `w is null` is `False`.

## How primary constructors and collections are compiled

- A primary-constructor parameter used outside initializers becomes a **hidden field** with an unpronounceable name; use it as state deliberately (warning `CS9124` about double storage).
- `[a, ..b, c]` for an array computes the length and copies elements directly; for `List<T>` and `Span<T>` it allocates the needed size and fills it without intermediate collections. For `IEnumerable<T>`/`IReadOnlyList<T>` a hidden type is created (in the verified run — `<>z__ReadOnlyArray`).
- An empty `[]` for an array is `Array.Empty<T>()` (verified: `ReferenceEquals` gives `True`); so reference checks against an "empty" array are unreliable.
- A type without a public parameterless constructor, or with required members, does not satisfy the `new()` constraint: error `CS9040` (verified on a type with `required`).

## Reference: what each release brought

| Version | Features |
|---|---|
| C# 8 | `switch` expressions, property/tuple/positional patterns, `using` declarations, indices and ranges, `IAsyncEnumerable<T>`, nullable reference types |
| C# 9 | `record`, `init`, top-level statements, `and`/`or`/`not` patterns, relational patterns, target-typed `new()` |
| C# 10 | `record struct`, file-scoped namespaces, global usings, nested property patterns, constant interpolated strings |
| C# 11 | `required`, list patterns, raw strings, UTF-8 literals `"…"u8`, generic math, `file` |
| C# 12 | primary constructors for classes and structs, collection expressions, aliases for any types, default lambda parameters, `[InlineArray]` |
| C# 13 | `params` for collections and `Span`, the `Lock` type, the `\e` escape, `partial` properties, `allows ref struct` |
| C# 14 | extension members, the `field` keyword, `?.=`, `nameof(List<>)`, `partial` constructors, implicit `Span` conversions |
| C# 15 | union types, collection expression arguments `with(...)`, labels for `break`/`continue`, extension indexers, the first preview of the `unsafe` model |

The list is based on the "What's new in C#" pages; check individual items against them. Verified by running: switch expressions, list patterns, primary constructors, collection expressions, `required`/`init`, `field`, `?.=`, extension members, `Lock`, inline arrays, aliases, `params ReadOnlySpan<T>`, `nameof` without arguments, `with(...)`, labels and union types.

## Gotchas

**1. `is null` versus `== null`.** Patterns bypass an overloaded `==` (see above): for types with custom equality the results diverge.

**2. Arm order.** A general pattern above a specific one makes the specific one unreachable — `CS8510`:

```csharp
string s = n switch { int => "int", 1 => "one" };   // error CS8510: pattern already handled
```

**3. A primary-constructor parameter is not `readonly`.** It can be assigned inside methods (`balance += amount` in the [basics](basics.en.md)), and that changes the object's state. For immutability copy it into a `readonly` field or a `{ get; }` property.

**4. `required` breaks `new()`.** Generic code with `where T : new()` does not accept types with required members (`CS9040`).

**5. `init` does not protect against reflection or assignment inside the constructor.** It is a compile-time restriction, not a run-time one.

**6. One spread — one pass.** `[..Gen(), ..Gen()]` calls the generator twice; an `IEnumerable<T>` result cannot be cast to `List<T>`.

**7. The assignment `h?.Value = 5` does not evaluate the right-hand side when `h` is `null`.** Side effects on the right will not happen.

**8. `default(Pet)`.** A union struct has `Value == null` by default: a `switch` over the cases only throws `SwitchExpressionException`, while an added arm `null => "empty"` fires (verified).

## Trade-offs and alternatives

| Task | Modern solution | Classic alternative |
|---|---|---|
| "Either-or" variants | union types (C# 15) | an `abstract record` hierarchy + `switch` on type |
| Parsing by data shape | patterns | `if`/`as` chains |
| An immutable model | `record` / `required` + `init` | a constructor with all parameters |
| Validation in a property | `field` | an explicit private field |
| A configurable list | `[..]` | `new List<T> { }` |
| Adding an API to someone else's type | `extension` | a static extension method |

## Interview questions

<details><summary>1. How does a `switch` expression differ from a `switch` statement?</summary>

The expression returns a value, every arm has a result, there is no `break` or fall-through, and the compiler checks exhaustiveness (warning `CS8509`). When no arm matches, a `SwitchExpressionException` is thrown at run time.

</details>

<details><summary>2. What kinds of patterns exist in C#?</summary>

Type, constant, relational, logical (`and`/`or`/`not`), property (including nested), positional (deconstruction), `var`, the discard `_`, list, and `{ }` (not null).

</details>

<details><summary>3. How does `x is null` differ from `x == null`?</summary>

`is null` is a pattern; it does not use an overloaded `==` operator; `== null` calls the overload if there is one. For ordinary types the result is the same; for types with custom equality it may differ.

</details>

<details><summary>4. What is a list pattern and what does `..` do?</summary>

Matching a sequence by length and elements: `[1, _, ..]`. The slice `..` matches any number of elements, can be captured (`.. var rest`) and is allowed once per pattern.

</details>

<details><summary>5. Is a primary-constructor parameter a property?</summary>

No. It is a parameter available throughout the type body; if a method uses it, the compiler creates a hidden field. In a `record` the parameters additionally become public properties.

</details>

<details><summary>6. What is warning CS9124?</summary>

A primary-constructor parameter is both captured into the type's state and used to initialize a property or field: the value is stored twice and can diverge.

</details>

<details><summary>7. What can collection expressions do?</summary>

They create an array, `List<T>`, `Span<T>`, `ImmutableArray<T>`, `IEnumerable<T>` and any type with `[CollectionBuilder]` according to the target type; they support the spread `..`; in C# 15 — the `with(...)` element for constructor arguments.

</details>

<details><summary>8. How does `required` differ from a constructor with parameters?</summary>

`required` forces the caller to set the property in an object initializer without needing a constructor. The downside is that it does not work with the `new()` constraint; constructors that set all required members themselves are marked `[SetsRequiredMembers]`.

</details>

<details><summary>9. What does `init` give and how does it differ from `readonly`?</summary>

An `init` property can be set only in an object initializer, a `with` expression or a constructor; after creation the object is immutable at the compiler level. `readonly` applies to fields and is set only in a constructor.

</details>

<details><summary>10. What is the `field` keyword for?</summary>

It refers to the property's automatic backing field, allowing logic in `get`/`set` without declaring your own field (C# 14).

</details>

<details><summary>11. What is new in extension members compared with extension methods?</summary>

An `extension` block can declare extension **properties**, **static** members and operators, not only methods. Regular extension methods with `this` keep working.

</details>

<details><summary>12. Explain `h?.Value = 5`.</summary>

Null-conditional assignment (C# 14): if `h` is `null`, nothing is executed and the right-hand side is not evaluated; otherwise the assignment happens as usual.

</details>

<details><summary>13. What is a C# 15 union type and how is it better than a hierarchy?</summary>

A value of one of the listed case types, which need no common base type. The compiler checks `switch` exhaustiveness; the cases can be any types (including ones from other libraries). Under the hood it is a struct with a `Value` field.

</details>

<details><summary>14. Trick question: what does `int[] e = []; Console.WriteLine(ReferenceEquals(e, Array.Empty<int>()));` print?</summary>

`True`: an empty collection expression for an array uses the cached `Array.Empty<int>()` (verified). Do not rely on the uniqueness of a reference to an empty array.

</details>

<details><summary>15. Trick question: why does `Create<Req>()` with `where T : new()` not compile if `Req` has a `required` member?</summary>

With `new()` the compiler cannot guarantee that the required members will be set: error `CS9040`. The solution is to remove `required` or the `new()` constraint and pass a `Func<T>` factory.

</details>

<details><summary>16. Trick question: is a `switch` without `_` enough for an `enum` with all values listed?</summary>

The compiler still warns (`CS8524`): an enum value can be any number of the underlying type (`(Color)42`), and at run time a `SwitchExpressionException` will occur. You need a `_` arm.

</details>

## Related materials

- Back: [advanced theory](advanced.en.md). Check yourself: [quiz](quiz.en.md) and [exercises](exercises.en.md).
- [05 · Interfaces, structs, records](../05-interfaces-structs-records/overview.en.md) — records, deconstruction, `init`.
- [08 · Exceptions](../08-exceptions/overview.en.md) — `when` filters, `SwitchExpressionException`.
- [06 · Collections and generics](../06-collections-and-generics/overview.en.md) — the `new()` constraint.
- [12a · Span and Memory](../12a-span-and-memory/overview.en.md) — `Span<T>` as a collection expression target.
- [Python](../../../python/overview.en.md) — `match`/`case` as an analog of patterns.

## Sources

- [What's new in C# 15](https://learn.microsoft.com/dotnet/csharp/whats-new/csharp-15)
- [What's new in C# 14](https://learn.microsoft.com/dotnet/csharp/whats-new/csharp-14)
- [Patterns: specification](https://learn.microsoft.com/dotnet/csharp/language-reference/operators/patterns)
- [Error CS9040](https://learn.microsoft.com/dotnet/csharp/language-reference/compiler-messages/required-member-errors)
