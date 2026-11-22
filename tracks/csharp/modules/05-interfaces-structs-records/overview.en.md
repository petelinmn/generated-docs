---
title: "Interfaces, structs, records"
lang: en
track: csharp
kind: module
doc: overview
slug: 05-interfaces-structs-records
part: 1
levels: [1, 2, 3]
tags: [interface, abstract-class, struct, record, value-semantics, value-equality, with-expression, deconstruction, equals]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Interfaces, structs, records

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

Besides classes, C# has three tools for describing types. An interface defines a contract without implementation; a struct is a value type that is copied on assignment; a record (`record`) is a type for data with automatic value equality, `ToString`, `with` and deconstruction. The choice between them determines how objects are stored, copied and compared.

## Key ideas

- **An interface is a contract:** a class implements any number of interfaces; since C# 8 there are default implementations, since C# 11 `static abstract` members.
- **Explicit implementation** resolves name conflicts: `IA.Run` and `IB.Run` in one class.
- **A struct is a value type:** assignment copies; no inheritance; mutable structs cause hidden copies, so structs are made `readonly`.
- **A record is data with value equality:** `==`, `Equals`, `GetHashCode`, `ToString`, `with`, deconstruction; `with` copies shallowly.
- **Equality:** a class is compared by reference by default, a struct and a record by value; `Equals` and `GetHashCode` are overridden together.
- **Collection keys are immutable:** changing the hash "loses" an element in a `HashSet`.
- **Boxing:** a struct through an interface parameter allocates 24 bytes; through a generic method with a constraint it does not.

## Minimal example

```csharp
public record Person(string Name, int Age);

var a = new Person("Ann", 30);
var b = new Person("Ann", 30);

Console.WriteLine(a == b);                // True — equality by value
Console.WriteLine(a with { Age = 31 });   // Person { Name = Ann, Age = 31 }
```

## When to use and when not

- ✅ An interface — a contract, swappable implementations, substitution in tests.
- ✅ `record` — DTOs, messages, dictionary keys, domain values.
- ✅ `readonly record struct` — small immutable values.
- ❌ `record` for entities with identity and mutable state — value equality hurts them.
- ❌ Mutable or large structs.
- ❌ An interface "just in case" with a single implementation.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | interfaces, structs, records; reference vs value equality; what to choose |
| [2. Advanced](advanced.en.md) | default implementation, explicit implementation, `static abstract`, `IEquatable<T>` and `GetHashCode`, mutable structs and defensive copies, records in hierarchies |
| [3. Expert](expert.en.md) | what the compiler generates for records, structs and boxing, reference, gotchas, 18 interview questions |
| 4. Self-check | not generated yet — `/learn csharp 05-interfaces-structs-records level 4` |
| 5. Runnable examples | not generated yet — `/learn csharp 05-interfaces-structs-records level 5` |
| 6. Tests | not generated yet — `/learn csharp 05-interfaces-structs-records level 6` |

## Related materials

- Prerequisites: [04 · OOP: classes and objects](../04-oop-basics/overview.en.md), [02 · Types and variables](../02-types-and-variables/overview.en.md)
- Next: [06 · Collections and generics](../06-collections-and-generics/overview.en.md)
- Boxing and allocations: [12 · Memory and GC](../12-memory-and-gc/overview.en.md)
- Patterns, primary constructors, `required`/`init`: [11 · Modern C#](../11-modern-csharp/overview.en.md)
- The same in other tracks: [Python](../../../python/overview.en.md) — `Protocol`, `dataclass`
