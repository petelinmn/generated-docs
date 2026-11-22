---
title: "Mapperly: expert"
lang: en
track: csharp
kind: library
doc: expert
slug: mapperly
part: 2
tags: [mapperly, automapper, source-generator, diagnostics, interview-questions]
versions: ".NET 11, C# 15; Riok.Mapperly 4.3.1"
status: draft
updated: 2026-11-22
---

# Mapperly: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

**Prerequisites:** [basics](basics.en.md) and [advanced](advanced.en.md). **Time:** ~30 min.

## Internals

Mapperly is an incremental Roslyn source generator. At compile time it finds classes with `[Mapper]`, analyzes the signatures of `partial` methods and the source/target types through the semantic model, builds a tree of mappings (properties, nested objects, collections, enums, user methods) and emits plain C# implementation code (`OrderMapper.g.cs`). Verified: for `ToDto` direct assignments `target.Id = o.Id; target.CustomerName = o.Customer.Name; …` were produced, for a list — `Enumerable.ToList(Enumerable.Select(orders, x => ToDto(x)))`, for `IQueryable` — `Queryable.Select(q, x => new OrderDto { … })`.

Consequences:

- no reflection or IL emit at runtime → compatibility with trimming and Native AOT, speed like hand-written code;
- mapping errors are compile **errors/warnings**, not runtime exceptions;
- the generated code can be read, a breakpoint set and compared in code review.

To see the code: `<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>` and exclude the output folder from compilation (`<Compile Remove="gen/**" />`) — otherwise CS0757 (verified).

## Edges and gotchas

**1. enum by value.** Verified: `(StatusText)s`; mismatched numbers — a silent error.

**2. Configuration on a projection.** Verified: `RMG065` with `MapProperty` on an `IQueryable` method.

**3. Warnings about unmapped members.** Verified: `RMG020` for `Secret`, `Created` and the reverse direction; they can be raised to errors.

**4. `partial` is mandatory.** The class and the method must be `partial`.

**5. Duplicates when emitting generated files.** Verified: CS0757.

**6. Updating an existing object.** Verified: `Update` does not touch the target's unmapped properties (`Label` stayed `keep?`).

**7. Depth and cycles.** Circular references between objects need a separate strategy (otherwise infinite recursion at runtime).

> ⚠️ Verify: behavior with circular references and `UseReferenceHandling` was not run in this material.

**8. Projection and EF Core.** Calls to your own methods inside an expression are not translated to SQL; for `IQueryable` Mapperly generates an inline expression, but user methods with logic cannot be used in it.

**9. Versions.** Options and diagnostic names change between versions — check the documentation for your version.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| Mapperly | no reflection, AOT, visible code, build-time diagnostics | each mapping must be declared |
| AutoMapper | flexible conventions, a big community | reflection/expression trees, errors surface at runtime |
| Hand-written methods/`ToDto` | maximum control | a lot of boilerplate |
| A projection via LINQ `Select` | simple SQL | duplication across queries |
| Mapster | code generation, fast | a different API |

## Interview questions

<details><summary>1. How does Mapperly differ from AutoMapper?</summary>

Mapperly is a source generator: the mapping code is created at compile time without reflection; errors are visible at build. AutoMapper builds the mapping at runtime.

</details>

<details><summary>2. How does Mapperly map properties?</summary>

By matching names; the rest — `[MapProperty]`; the unneeded — `[MapperIgnoreTarget]`/`[MapperIgnoreSource]` (verified).

</details>

<details><summary>3. What is `RMG020`?</summary>

A warning about a source member not mapped to the target (verified); it helps not to lose fields.

</details>

<details><summary>4. How are enums mapped by default?</summary>

By value (`(StatusText)s`, verified); by name — with an explicit strategy.

</details>

<details><summary>5. How do you use Mapperly with EF Core?</summary>

A method over `IQueryable<T>` gives a `Select(x => new Dto { … })` expression that is translated to SQL.

</details>

<details><summary>6. Why is `partial` needed?</summary>

The generator adds the method implementation to another part of the class.

</details>

<details><summary>7. How do you update an existing object?</summary>

`partial void Update(Source s, Target t)`; the target's unmapped properties stay (verified).

</details>

<details><summary>8. How do you view the generated code?</summary>

`EmitCompilerGeneratedFiles` + exclude the folder from compilation (otherwise CS0757, verified), or through the IDE.

</details>

<details><summary>9. Trick question: after turning on `EmitCompilerGeneratedFiles` the project does not build with CS0757. Why?</summary>

The generated files lie inside the project and are picked up by the default compile glob — the implementation is declared twice. Exclude the folder: `<Compile Remove="gen/**" />`.

</details>

<details><summary>10. Trick question: enum values in two types are numbered differently and the mapping "works". What is wrong?</summary>

The by-value strategy: numbers are copied as is, names are not compared (the code shape `(StatusText)s` was verified). Use mapping by name.

</details>

<details><summary>11. When is hand-written mapping better than Mapperly?</summary>

When the conversion logic is non-trivial and not repeated; for simple DTO conversions the generator reduces boilerplate.

</details>

## Related materials

- Back: [advanced](advanced.en.md). Check yourself: [quiz](quiz.en.md) and [exercises](exercises.en.md).
- [20 · Reflection and source generators](../../modules/20-reflection-and-source-generators/overview.en.md)
- [19 · Performance](../../modules/19-performance/overview.en.md) — the cost of reflection.
- [BenchmarkDotNet](../benchmarkdotnet/overview.en.md) — measuring mapping.

## Sources

- [Mapperly docs](https://mapperly.riok.app/docs/intro/)
- [Mapperly: Analyzer diagnostics](https://mapperly.riok.app/docs/configuration/analyzer-diagnostics/)
- [Roslyn incremental generators](https://learn.microsoft.com/dotnet/csharp/roslyn-sdk/source-generators-overview)
