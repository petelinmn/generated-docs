---
title: "Quiz: Reflection and source generators"
lang: en
track: csharp
kind: module
doc: quiz
slug: 20-reflection-and-source-generators
part: 3
tags: [reflection, attributes, expression-trees, source-generators, trimming]
status: draft
updated: 2026-11-22
---

# Quiz: Reflection and source generators

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 Which code gets the names and types of the public properties of the class `Person`?

- A) `typeof(Person).GetProperties()`
- B) `Person.Properties`
- C) `nameof(Person)`
- D) `Activator.CreateInstance(typeof(Person))`

<details><summary>Answer</summary>

**A.** `GetProperties()` returns `PropertyInfo` with `Name` and `PropertyType`. `nameof` gives only the type name, `Activator` creates an object.
→ [Basics](basics.en.md#type-information)

</details>

## 2. 🟢 What does the caller get if a method called through `MethodInfo.Invoke` threw `InvalidOperationException`?

- A) `InvalidOperationException` directly
- B) `TargetInvocationException`, with the original error in `InnerException`
- C) `null`
- D) nothing: the exception is swallowed

<details><summary>Answer</summary>

**B.** Reflection wraps the exception of the called method (verified: `TargetInvocationException -> inner InvalidOperationException`).
→ [Basics](basics.en.md#creating-objects-and-calling-members)

</details>

## 3. 🟢 What does the attribute `[Label("The name")]` do by itself?

- A) sets a caption in the UI
- B) nothing: it is metadata that other code (reflection or a generator) must read
- C) renames the property
- D) validates the value

<details><summary>Answer</summary>

**B.** An attribute is data. Code like `p.GetCustomAttribute<LabelAttribute>()` reads it.
→ [Basics](basics.en.md#attributes)

</details>

## 4. 🟢 How does `Expression<Func<int,int>> e = x => x * 2;` differ from `Func<int,int> f = x => x * 2;`?

- A) it does not
- B) `e` is a data structure (a tree) that can be examined and compiled; `f` is a ready delegate
- C) `e` is faster
- D) `e` cannot be called at all

<details><summary>Answer</summary>

**B.** A tree has `Body.NodeType`, can be parsed and has `Compile()` (verified: `x => ((x * 2) + 1)`, `Add`, the result `11`).
→ [Basics](basics.en.md#expression-trees)

</details>

## 5. 🟡 What do these operations allocate per call?

```csharp
_ = person.Name;                    // direct
_ = nameProp.GetValue(person);      // PropertyInfo
_ = greet.Invoke(person, ["x"]);    // MethodInfo
_ = getter(person);                 // a cached delegate
```

- A) 0, 0, 72, 0 bytes
- B) 0, 24, 0, 0 bytes
- C) 72 bytes for all
- D) 0 for all

<details><summary>Answer</summary>

**A.** Verified: `Invoke` allocated 72 bytes (an argument array and boxing), while `GetValue` for a string and the delegate — 0. Hot calls are moved to cached delegates.
→ [Advanced theory](advanced.en.md#the-cost-of-reflection-and-speeding-it-up)

</details>

## 6. 🟡 Why does the `Mapper` example have a nested static class `Cache<TFrom, TTo>`?

- A) to hide the code
- B) each pair of types has its own static field: the delegate is built once and reused
- C) to bypass AOT limitations
- D) for thread safety

<details><summary>Answer</summary>

**B.** A static field of a generic type is created separately for each closed pair. The compiled copy gave `0 B` versus `64 B` for the reflective one (verified). In Native AOT `Compile()` is unavailable.
→ [Advanced theory](advanced.en.md#compiling-expressions-for-fast-access)

</details>

## 7. 🟡 Select all correct statements about a source generator

- [ ] A) it works at compile time and adds new files
- [ ] B) it can change an existing method in your class
- [ ] C) the target class must be `partial` for it to be extended
- [ ] D) it is built for `netstandard2.0` and attached as an analyzer

<details><summary>Answer</summary>

**A, C, D.** B is wrong: a generator only **adds** files and does not change existing code; that is why `partial` is needed. Verified: the `[AutoToString]` generator created `ToString` for `Person` and `Point`.
→ [Advanced theory](advanced.en.md#source-generators)

</details>

## 8. 🟡 Why is the string `" {{ "` and not `" { "` in the generator?

- A) a typo
- B) in the generated interpolated string single `{` is an error: curly braces must be doubled
- C) it is faster
- D) to hide the output

<details><summary>Answer</summary>

**B.** The generated code is `$"Person {{ Name = {Name}, … }}"`. With single braces the compiler gave errors `CS1073`/`CS1525` in the generated file (verified).
→ [Advanced theory](advanced.en.md#source-generators)

</details>

## 9. 🟡 Open question: when is a source generator better than reflection?

<details><summary>Key points of a good answer</summary>

- when the set of types is known at compile time and you need serialization, logging, regular expressions, mappers, registration;
- no reflection cost at run time, the code is checked by the compiler and readable in the generated files;
- mandatory for Native AOT and trimming (reflection and `Expression.Compile` are limited there);
- when you need plugins or types unknown in advance (loading assemblies), reflection remains.

</details>

## 10. 🔴 What do `typeof(int?)` and `boxed.GetType()` return for `int? n = 5; object boxed = n;`?

- A) both `Nullable<int>`
- B) `Nullable<int>` and `Int32`
- C) both `Int32`
- D) `Int32` and `Nullable<int>`

<details><summary>Answer</summary>

**B.** When a `Nullable<int>` with a value is boxed, the value itself is boxed, so `GetType()` gives `Int32` (verified). For the original type use `Nullable.GetUnderlyingType`.
→ [Expert theory](expert.en.md#metadata-and-the-type-object)

</details>

## 11. 🔴 An application works in Debug, but after `PublishTrimmed` it crashes: the type found by string does not exist. Why and how do you fix it?

- A) a Windows bug
- B) trimming removed code visible only by string names; you need annotations (`DynamicallyAccessedMembers`) or a generator instead of reflection
- C) `PublishTrimmed` disables the JIT
- D) you need to reboot

<details><summary>Answer</summary>

**B.** Trimming analyzes static calls; it does not see access by string. Mark code with annotations (`IL2026`/`IL3050` warn about it) or move to generators (`JsonSerializerContext`, `[GeneratedRegex]`).
→ [Expert theory](expert.en.md#trimming-and-native-aot)

</details>
