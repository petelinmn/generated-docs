---
title: "Interfaces, structs, records: basics"
lang: en
track: csharp
kind: module
doc: basics
slug: 05-interfaces-structs-records
part: 1
tags: [interface, abstract-class, struct, record, value-semantics, value-equality, with-expression, deconstruction, equals]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Interfaces, structs, records: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [04 · OOP: classes and objects](../04-oop-basics/overview.en.md), [02 · Types and variables](../02-types-and-variables/overview.en.md).

## Why it matters

A class is not the only way to describe a type. Three other tools solve specific problems:

- an **interface** describes a **contract** — what a type can do, without saying how; one class can implement many interfaces;
- a **struct** (`struct`) is a **value type**: data is copied on assignment, which suits small values (a point, a color, money);
- a **record** (`record`) is a type for **data**: the compiler creates value equality, `ToString()`, `with` copying and deconstruction.

## Interfaces

An interface is a set of members without implementation. A class that implements it must provide all the members:

```csharp
public interface IShape
{
    string Name { get; }
    double Area();
}

public class Circle(double radius) : IShape
{
    public string Name => "circle";
    public double Area() => Math.PI * radius * radius;
}
```

Code that needs the contract works with the interface and knows nothing about a concrete class:

```csharp
static double TotalArea(IEnumerable<IShape> shapes) => shapes.Sum(s => s.Area());
```

- By convention interface names start with `I`.
- A class implements **several interfaces** (`class Foo : IA, IB`), but inherits from only one class.
- A variable of an interface type can hold any object that implements the interface; check with `is`, `as` (`ia is IB`).
- Since C# 8 an interface can contain a **default implementation** — more in [advanced](advanced.en.md#default-implementation-and-explicit-implementation).

### Interface or abstract class

| | Interface | Abstract class |
|---|---|---|
| State (fields) | no | yes |
| Constructors | no | yes |
| How many can be implemented or inherited | any number | one |
| What for | a contract, a capability ("can do") | a common base, "is a" |

The rule: start with an interface; you need an abstract class when you want to hand ready-made state and implementation to derived classes.

## Structs

A struct is a value type. The variable holds **the data itself**, not a reference, so assignment copies the value:

```csharp
public struct Pt
{
    public int X;
    public int Y;
    public Pt(int x, int y) { X = x; Y = y; }
}

var p1 = new Pt(1, 2);
var p2 = p1;            // a copy
p2.X = 99;
Console.WriteLine($"{p1.X} {p2.X}");   // 1 99
```

A struct is not created as a "separate object" on the heap: a local variable lives on the stack, and a struct field lives inside the owning object (more in [02 · Types and variables](../02-types-and-variables/expert.en.md)). This takes load off the garbage collector, but copying large structs is expensive.

Properties of structs:

- they cannot inherit from another struct or a class (they can implement interfaces);
- a struct always has a default value — all fields zero: `default(Pt)` is `(0, 0)`;
- since C# 10 you can declare a parameterless constructor and field initializers: `struct Wd { public int V = 7; public Wd() { } }` — then `new Wd().V` is `7`, while `default(Wd).V` is `0`;
- `==` for structs is **not defined** automatically: `a == b` does not compile (error CS0019), but `a.Equals(b)` compares fields.

A **`readonly struct`** guarantees the struct is immutable: all fields are read-only. This protects against hidden copies (see [advanced](advanced.en.md#mutable-structs)).

### When to use a struct

- ✅ a small (roughly up to 16 bytes), immutable value that logically is "one number": coordinates, a color, a date;
- ✅ a type created in large quantities;
- ❌ large objects, objects with identity, types that need inheritance;
- ❌ mutable structs — a source of bugs.

## Records

A record (`record`) is a type that describes data, and the compiler generates the typical code for it:

```csharp
public record Person(string Name, int Age);

var a = new Person("Ann", 30);
var b = new Person("Ann", 30);

Console.WriteLine(a == b);              // True — equality by value
Console.WriteLine(ReferenceEquals(a, b)); // False — different objects
Console.WriteLine(a);                   // Person { Name = Ann, Age = 30 }
```

What a record gives you:

- **value equality**: `==` and `Equals` compare all properties, and `GetHashCode` is consistent with them;
- **`ToString()`** in the form `Person { Name = Ann, Age = 30 }`;
- **immutability** by default: the properties of a positional record are `init`;
- the **`with` expression** — a copy with a change:

```csharp
var older = a with { Age = 31 };        // a is unchanged
Console.WriteLine(older);               // Person { Name = Ann, Age = 31 }
```

- **deconstruction**: `var (name, age) = a;`.

A `record` is a class by default (`record class`), that is, a reference type. For a value type there is `record struct`, and `readonly record struct` is the immutable variant:

```csharp
public readonly record struct Point(int X, int Y);
```

A record can also be declared without positional parameters, with ordinary properties: `public record Order { public required int Id { get; init; } }`.

## Equality: references and values

Three ways to compare objects:

| Way | What it does by default |
|---|---|
| `ReferenceEquals(a, b)` | compares references (always `false` for value types) |
| `a.Equals(b)` | class — references; struct — fields; record — property values |
| `a == b` | class — references (unless overloaded); struct — not defined; record — values; `string` — by content |

That is why for ordinary classes two objects with the same data are not equal, while for records they are. If you need value equality for a class, override `Equals` and `GetHashCode` (see [advanced](advanced.en.md#equality-and-gethashcode)).

## What to choose

| Need | Choice |
|---|---|
| A contract, swappable implementations, test doubles | interface |
| An object with identity and mutable state | `class` |
| Data compared by value that does not change | `record` |
| A small value without identity | `struct` / `readonly record struct` |

## Beginner mistakes

- ❌ Using a "data container" class and being surprised that `==` gives `false` → ✅ `record`.
- ❌ Making a struct large or mutable → ✅ a small immutable one, or an ordinary class.
- ❌ Expecting `with` to copy nested objects → ✅ the copy is shallow.
- ❌ Creating an interface "just in case" with a single implementation → ✅ an interface is needed when there is a second implementation or substitution in tests.
- ❌ Trying to change an element of `List<struct>` in place (`list[0].X = 5`) → ✅ error CS1612; replace the whole element.

## Related materials

- Next: [advanced](advanced.en.md) — explicit implementation, static interface members, `IEquatable<T>`, mutable structs, records in hierarchies
- Classes, inheritance, polymorphism: [04 · OOP: classes and objects](../04-oop-basics/basics.en.md)
- Value and reference types, boxing: [02 · Types and variables](../02-types-and-variables/basics.en.md)
- Collections where `Equals` and `GetHashCode` matter: [06 · Collections and generics](../06-collections-and-generics/overview.en.md)
- The same in other tracks: [Python](../../../python/overview.en.md) — `typing.Protocol`, `dataclass`, `NamedTuple`

## Sources

- [Interfaces](https://learn.microsoft.com/dotnet/csharp/fundamentals/types/interfaces)
- [Structure types](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/struct)
- [Records](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/record)
- [Choosing between class and struct](https://learn.microsoft.com/dotnet/standard/design-guidelines/choosing-between-class-and-struct)
- [Equality operators](https://learn.microsoft.com/dotnet/csharp/language-reference/operators/equality-operators)
