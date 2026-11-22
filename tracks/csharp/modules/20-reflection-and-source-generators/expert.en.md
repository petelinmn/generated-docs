---
title: "Reflection and source generators: expert"
lang: en
track: csharp
kind: module
doc: expert
slug: 20-reflection-and-source-generators
part: 3
tags: [reflection, metadata, trimming, native-aot, assembly-load-context, roslyn, source-generators, interview-questions]
versions: ".NET 11, C# 15; Microsoft.CodeAnalysis.CSharp 4.14.0"
status: draft
updated: 2026-11-22
---

# Reflection and source generators: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

**Prerequisites:** [basic](basics.en.md) and [advanced](advanced.en.md) theory. **Time:** ~1.5 h.

## Metadata and the Type object

Every .NET assembly contains **metadata** next to the IL: tables of types, methods, fields, properties, attributes, references to other assemblies. Reflection reads these tables and builds the objects `Type`, `MethodInfo`, `PropertyInfo`, `FieldInfo`. A `Type` for one type exists in the application domain as a **single instance**: `typeof(Foo) == new Foo().GetType()`, and it is the same reference (verified: `ReferenceEquals` → `True`). A member has a `MetadataToken` (verified: non-zero), a type has a `Module` and an `Assembly`.

- **Lookup by name.** Names are case-sensitive: `GetProperty("name")` for the property `Name` returned `null`; with `BindingFlags.IgnoreCase` it found it (verified).
- **Default flags.** `GetProperties()` without flags returns public instance members; private ones need `BindingFlags.NonPublic` (verified: 1 public, 2 with the private one). Static members are read with `GetValue(null)` (verified: `3`).
- **Overloads.** `GetMethod("Over")` with two overloads throws `AmbiguousMatchException`; you must specify the parameter types (verified).
- **Creation.** `Activator.CreateInstance(typeof(NoDefault))` without a suitable constructor — `MissingMethodException` (verified).
- **Type names.** `Type.GetType` takes a name; core library types are found without naming the assembly (`System.Collections.Generic.List`1` → `List`1`: verified), while types of other assemblies must be given with the assembly name (`"Foo, m20"`).
- **Nullable.** Boxing an `int?` with a value gives an object of type `Int32` — `boxed.GetType().Name` is `Int32`, while `typeof(int?)` is `Nullable`1` (verified); `Nullable.GetUnderlyingType` helps determine the original type.

## How a reflection call executes

`MethodInfo.Invoke` takes an `object[]` array, boxes value-type arguments, checks they match and calls the method; the result is boxed back. An exception inside the method is wrapped in a `TargetInvocationException` (verified). In modern versions the runtime uses generated IL for frequent calls instead of a slow universal branch, but the cost stays higher than a direct call: `72 B` allocations per `Invoke` versus `0` for a delegate (verified in the [advanced theory](advanced.en.md#the-cost-of-reflection-and-speeding-it-up)).

## Expression trees

`Expression.Lambda(...).Compile()` generates IL (a dynamic method) at run time and returns a delegate; `Compile(preferInterpretation: true)` creates an interpreted variant with no code generation (verified: both returned `7`) — a fallback for environments without dynamic compilation. A tree is immutable; typical nodes: `Constant`, `Parameter`, `Property`, `Call`, `Add`, `Assign`, `Block`, `Lambda`. `IQueryable<T>` keeps a tree that a provider translates into another language (SQL).

## Trimming and Native AOT

Trimming removes code unreachable by call analysis. Reflection **breaks** the analysis: an application may reach by string a type that trimming cannot see. So annotations are used:

| Annotation | Meaning |
|---|---|
| `[DynamicallyAccessedMembers(...)]` | a parameter or field whose members will be reached by reflection — keep them |
| `[RequiresUnreferencedCode]` | the method is unsafe under trimming (warning `IL2026` to the caller) |
| `[RequiresDynamicCode]` | the method needs code generation (warning `IL3050`) |
| `[UnconditionalSuppressMessage]` | suppress a warning, taking responsibility |

The solution for AOT is **source generators**: they replace reflection with code visible to the compiler (`JsonSerializerContext`, `[GeneratedRegex]`, `[LoggerMessage]`). Dynamically created code (`Expression.Compile`, `Reflection.Emit`) is unavailable in Native AOT: `RuntimeFeature.IsDynamicCodeSupported == false` ([19 · Performance](../19-performance/expert.en.md#native-aot)).

## Loading assemblies

`Assembly.Load`, `Assembly.LoadFrom` and `AssemblyLoadContext` load assemblies at run time; an `AssemblyLoadContext` isolates plugins with their own dependency versions and allows unloading the context (collectible). `MetadataLoadContext` reads an assembly's metadata **without executing** its code — for analysis tools.

## How a generator works

The Roslyn compiler goes in order: it parses the sources (the syntax tree), builds the semantic model, runs the generators, **adds their files to the compilation** and emits the assembly. A generator sees syntax and symbols but cannot change existing code — only add new files; so classes must be `partial`. Requirements: the generator is deterministic, uses no disk or network, and is built for `netstandard2.0` (it is loaded into the compiler and IDE process). An incremental pipeline (`IIncrementalGenerator`) describes the transformations as a graph; the compiler remembers intermediate results and recomputes only what changed, provided the models are comparable by value (`record`, tuples, `ImmutableArray`) — that is why `ISymbol`/`SyntaxNode` are not kept in models.

Differences from related mechanisms: an **analyzer** (`DiagnosticAnalyzer`) reports diagnostics and does not create code; a **code fix** offers an edit; a generator adds code; **interceptors** replace calls at compile time — a new feature whose status must be checked against the documentation of the current C# version.

## Reference

| What | API |
|---|---|
| A type | `typeof`, `GetType()`, `Type.GetType`, `Assembly.GetTypes()` |
| Members | `GetProperties/GetMethods/GetFields/GetConstructors`, `BindingFlags` |
| Creating and calling | `Activator.CreateInstance`, `ConstructorInfo.Invoke`, `MethodInfo.Invoke`, `PropertyInfo.GetValue/SetValue` |
| Generics | `MakeGenericType`, `GetGenericArguments`, `IsGenericTypeDefinition` |
| Attributes | `GetCustomAttribute<T>`, `AttributeUsage` |
| Fast access | `Delegate.CreateDelegate`, `Expression…Compile()` |
| Loading | `Assembly.Load*`, `AssemblyLoadContext`, `MetadataLoadContext` |
| AOT annotations | `DynamicallyAccessedMembers`, `RequiresUnreferencedCode`, `RequiresDynamicCode` |
| Generators | `IIncrementalGenerator`, `ForAttributeWithMetadataName`, `RegisterSourceOutput` |

## Gotchas

**1. Name case.** `GetProperty("name")` returned `null` (verified).

**2. Ambiguous overloads.** `AmbiguousMatchException` (verified).

**3. `TargetInvocationException`.** The real error is in `InnerException` (verified).

**4. Private members without `NonPublic`.** The method returns `null`/nothing.

**5. Reflection and trimming/AOT.** A dynamically found type may be removed: annotate or move to a generator.

**6. Boxing.** `GetValue`/`Invoke` box value types and arguments (verified: 72 B per `Invoke`).

**7. Strings instead of `nameof`.** Renaming silently breaks reflective code.

**8. Generators: unescaped `{`/`}` in a generated interpolated string** give a compile error (`CS1073`) in the generated file (verified).

**9. A generator without a `partial` class.** A closed class cannot be extended.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| Reflection | flexible, simple | slow, not for AOT, no compiler checks |
| Cached delegates / `Expression.Compile` | fast after warm-up | harder, code generation (not AOT) |
| Source generator | zero run-time cost, AOT | harder to write and debug, `netstandard2.0` |
| Hand-written code | simple, transparent | a lot of repetition |
| Interfaces and polymorphism | type-safe | do not replace metadata-based discovery |

## Interview questions

<details><summary>1. What is reflection and why is it needed?</summary>

Reading type metadata and accessing members by name at run time: serializers, ORMs, DI, plugins, test frameworks. The price is speed and the loss of compiler checks.

</details>

<details><summary>2. How do you get a `Type` object?</summary>

`typeof(T)`, `obj.GetType()`, `Type.GetType(string)`, `assembly.GetTypes()`. For one type in an application the `Type` is the same one (verified: `ReferenceEquals`).

</details>

<details><summary>3. What is `BindingFlags`?</summary>

An enumeration that says which members to look for: `Public`, `NonPublic`, `Instance`, `Static`, `DeclaredOnly`, `IgnoreCase`. By default — public instance and static members.

</details>

<details><summary>4. How do you speed up reflection calls?</summary>

Cache `MethodInfo`/`PropertyInfo`, create typed delegates (`Delegate.CreateDelegate`) or compile expressions; for AOT — a generator.

</details>

<details><summary>5. What is an expression tree?</summary>

Data that describes a lambda (`Expression<Func<…>>`): it can be analyzed (a property name, SQL) and compiled into a delegate.

</details>

<details><summary>6. How does an attribute differ from an ordinary class?</summary>

Metadata attached to a code element; it does nothing itself, and is read by reflection or the compiler (a generator).

</details>

<details><summary>7. What is a source generator?</summary>

A compiler component that during the build analyzes code and adds source files. It works at compile time, so it has no run-time cost and is AOT-compatible.

</details>

<details><summary>8. Why is a generator built for `netstandard2.0`?</summary>

It is loaded into the compiler and IDE process, which run on different runtimes, so the lowest common denominator is taken.

</details>

<details><summary>9. What does an "incremental" generator mean?</summary>

A pipeline of transformations with caching: the compiler repeats only the stages whose input changed; models must be comparable by value, without symbols and syntax nodes.

</details>

<details><summary>10. Why must a generated class be `partial`?</summary>

A generator cannot change existing code, it only adds files; `partial` lets it extend the same type.

</details>

<details><summary>11. Trick question: `typeof(int?)` and `boxed.GetType()` for `int? n = 5`. Are they equal?</summary>

No: `typeof(int?)` is `Nullable<int>`, while `GetType()` on the boxed value gives `Int32`: when a `Nullable<T>` with a value is boxed, the value itself is boxed (verified).

</details>

<details><summary>12. Trick question: why did `GetProperty("name")` return `null` although the property `Name` exists?</summary>

The lookup is case-sensitive; you need `BindingFlags.IgnoreCase` or the exact name (better `nameof(Person.Name)`).

</details>

<details><summary>13. Trick question: the application works in Debug but crashes on reflection after publishing with trimming. Why?</summary>

Trimming removed code visible only by string names. You need annotations (`DynamicallyAccessedMembers`, `[DynamicDependency]`) or replacing reflection with a generator.

</details>

## Related materials

- Back: [advanced theory](advanced.en.md). Check yourself: [quiz](quiz.en.md) and [exercises](exercises.en.md).
- [19 · Performance](../19-performance/overview.en.md) — Native AOT and the cost of dynamic code.
- [13 · DI, configuration, logging](../13-di-config-logging/overview.en.md) — DI and `[LoggerMessage]`.
- [07 · LINQ](../07-linq/overview.en.md) — `IQueryable<T>` and expression trees.
- [11 · Modern C#](../11-modern-csharp/overview.en.md) — `partial` types.
- [Python](../../../python/overview.en.md) — dynamic introspection by default.

## Sources

- [Metadata and reflection](https://learn.microsoft.com/dotnet/fundamentals/reflection/overview)
- [Prepare libraries for trimming](https://learn.microsoft.com/dotnet/core/deploying/trimming/prepare-libraries-for-trimming)
- [`AssemblyLoadContext`](https://learn.microsoft.com/dotnet/core/dependency-loading/understanding-assemblyloadcontext)
- [Incremental generators cookbook](https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.cookbook.md)
