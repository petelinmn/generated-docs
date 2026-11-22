---
title: "Quiz: Mapperly"
lang: en
track: csharp
kind: library
doc: quiz
slug: mapperly
part: 2
tags: [mapperly, object-mapping, source-generator]
status: draft
updated: 2026-11-22
---

# Quiz: Mapperly

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What is Mapperly?

- A) a reflection-based mapper that builds rules at runtime
- B) a source generator that writes the mapping code at compile time
- C) an ORM
- D) a logging library

<details><summary>Answer</summary>

**B.** The result is plain assignments without reflection.
→ [Basics](basics.en.md#what-it-is-and-why)

</details>

## 2. 🟢 What is mandatory for a mapper class?

- A) inheriting from `Mapper`
- B) the `[Mapper]` attribute and the `partial` modifier
- C) `IDisposable`
- D) static fields

<details><summary>Answer</summary>

**B.** The generator adds the implementation of `partial` methods.
→ [Basics](basics.en.md#the-first-mapper)

</details>

## 3. 🟢 How do you map `Customer.Name` to `CustomerName`?

- A) by itself
- B) `[MapProperty("Customer.Name", nameof(OrderDto.CustomerName))]`
- C) `Include`
- D) you cannot

<details><summary>Answer</summary>

**B.** Verified: `target.CustomerName = o.Customer.Name;`.
→ [Basics](basics.en.md#the-first-mapper)

</details>

## 4. 🟢 What does the warning `RMG020` mean?

- A) a syntax error
- B) a source member is not mapped to any target member
- C) an enum is not supported
- D) no `partial`

<details><summary>Answer</summary>

**B.** Verified: for `Secret` and `Created`; mark them explicitly with `MapperIgnoreSource`.
→ [Basics](basics.en.md#warnings-instead-of-surprises)

</details>

## 5. 🟡 How are enums mapped by default?

- A) by name
- B) by value (`(StatusText)s`)
- C) through strings
- D) you cannot

<details><summary>Answer</summary>

**B.** Verified from the generated code; by name — with an explicit strategy.
→ [Advanced](advanced.en.md#enums)

</details>

## 6. 🟡 What does the method `IQueryable<OrderDto> Project(IQueryable<Order> q)` give?

- A) loads everything into memory
- B) a `Select(x => new OrderDto { … })` expression translatable by the provider
- C) a copy of the collection
- D) nothing

<details><summary>Answer</summary>

**B.** Verified on the generated code.
→ [Advanced](advanced.en.md#projections-for-iqueryable)

</details>

## 7. 🟡 Select all correct statements

- [ ] A) `Update(source, target)` modifies an existing object
- [ ] B) `partial OrderDto? Map(Order? o)` returns `null` for `null`
- [ ] C) `[MapProperty]` settings should be hung on the `IQueryable` projection method
- [ ] D) the reverse direction needs a separate method

<details><summary>Answer</summary>

**A, B, D.** C is wrong: it gives `RMG065`; configure the object method.
→ [Advanced](advanced.en.md#reverse-mapping-and-null)

</details>

## 8. 🟡 Open question: how do you organize DTO mapping in a project with EF Core?

<details><summary>Key points of a good answer</summary>

- a `[Mapper] partial class` with model↔DTO methods;
- `IQueryable` projections for reading (only the needed columns);
- an explicit enum strategy, warnings as errors;
- a mapper without business logic, registered as a singleton.

</details>

## 9. 🔴 After `EmitCompilerGeneratedFiles` the project does not build (CS0757). Why?

- A) an outdated SDK
- B) the generated files lie in the project and are compiled twice
- C) enums are not supported
- D) `partial` is missing

<details><summary>Answer</summary>

**B.** Exclude the folder: `<Compile Remove="gen/**" />` (verified).
→ [Expert](expert.en.md#internals)

</details>

## 10. 🔴 Why is Mapperly compatible with Native AOT?

- A) it uses a cache
- B) it uses no reflection or IL emit at runtime — the code is generated ahead of time
- C) it works only on the server
- D) it converts types to strings

<details><summary>Answer</summary>

**B.** The resulting code is plain C#.
→ [Expert](expert.en.md#internals)

</details>
