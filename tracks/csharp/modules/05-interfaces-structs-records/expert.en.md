---
title: "Interfaces, structs, records: expert"
lang: en
track: csharp
kind: module
doc: expert
slug: 05-interfaces-structs-records
part: 1
tags: [record-internals, equality-contract, struct-layout, boxing, constrained-call, default-interface-methods, ref-struct, equals-gethashcode, interview-questions]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Interfaces, structs, records: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## Under the hood

### What the compiler generates for a record

For `record Person(string Name, int Age)` the compiler adds to the class (verified through reflection):

| Member | Purpose |
|---|---|
| `Name`, `Age` | `init` properties (get/set methods with the `IsExternalInit` modifier) |
| a constructor with parameters | positional |
| a protected copy constructor | for `with` |
| `<Clone>$()` | a method with a name that is "impossible" in C#: creates a copy, after which the `with` initializer is applied |
| `Equals(Person)` and `Equals(object)` | value equality; the record implements `IEquatable<Person>` |
| `GetHashCode()` | combines the hashes of the properties |
| `==` and `!=` | operators based on `Equals` |
| `ToString()` and `PrintMembers` | `Person { Name = Ann, Age = 30 }` |
| `Deconstruct(out string, out int)` | for positional deconstruction |
| `EqualityContract` | a protected virtual property of type `Type` |

`EqualityContract` returns the exact type of the record (`Person` for `Person`). The `Equals` method first compares the `EqualityContract` of both objects, which is why a record and its derived record are not equal even if the shared properties match, and the result is symmetric. A `record struct` instead inherits from `System.ValueType` and implements `IEquatable<T>`: there is no contract type, since structs cannot be inherited.

A positional record parameter creates a property; if you declare a property with the same name by hand, the compiler uses yours.

### Structs: equality and hash

A struct inherits from `System.ValueType`, and the default implementations of `Equals` and `GetHashCode` come from it (verified: `Pt.Equals` is declared in `System.ValueType`). For two structs with identical fields `Equals` returned `True` and the hashes matched. The `ValueType` implementation is universal, meaning it is not tailored to a specific type, so for structs that end up in dictionaries and sets **implement `IEquatable<T>`** yourself or use a `record struct`, which gets it automatically.

### Boxing and constrained calls

Calling an interface method through a variable of the interface type forces the struct to be boxed. Verified by measuring allocations: passing a struct to an `IInc` parameter costs **24 bytes** per call (the same as boxing an `int`), while passing it to a generic method with the constraint `where T : IInc` costs **0 bytes**. For a constrained type parameter the JIT generates separate code for each value type and calls the method directly, without boxing. That is why in hot code value types are passed through generic methods with constraints rather than interface parameters.

### Default interface implementations

A method with a body in an interface is not a class member: the class gets it only as a "virtual" member of the interface. Reflection shows this: `Sq` has no `Describe` method (verified). If a class implements the method itself, that is used instead of the default implementation; if two interfaces supply identical default implementations, the class must implement the method itself (the diamond problem is resolved explicitly).

### Layout and size

Structs have sequential layout by default (`LayoutKind.Sequential`: verified `IsLayoutSequential == True`), fields go in declaration order with alignment; for classes the runtime chooses the layout (`IsAutoLayout == True`). That is why a struct of `byte, int, byte` takes 12 bytes and `int, byte, byte` takes 8 (verified). An empty struct is 1 byte.

## Reference

### Interface members

| Member | Since | Notes |
|---|---|---|
| method, property, indexer, event | C# 1 | implemented by the class; `public` by default |
| default implementation | C# 8 | visible only through the interface; `private`, `protected`, `static` members are allowed in an interface |
| static members | C# 8 | including `static` fields and methods |
| `static abstract` / `static virtual` | C# 11 | operators, factories; generic math |
| constraints `where T : IFoo` | C# 2 | a call without boxing for structs |
| covariance and contravariance | C# 4 | `out T` / `in T`; see [06 · Collections and generics](../06-collections-and-generics/overview.en.md) |

### Structs

| Feature | Status |
|---|---|
| inheritance from a class/struct | no; interfaces can be implemented |
| a parameterless constructor | C# 10+ |
| field initializers | C# 10+ (with an explicit constructor) |
| `readonly struct` | all fields read-only, no defensive copies |
| a `readonly` method in a struct | the method does not change state |
| `ref struct` | lives only on the stack: not a class field, cannot be boxed, not allowed in an `async` method or lambda (see the "Span and Memory" module) |
| `record struct`, `readonly record struct` | C# 10 |
| `default` | all fields zero, no constructor is called |
| `with` | works for structs and records |

### Records

| Form | Kind | Properties |
|---|---|---|
| `record P(int X)` / `record class` | reference | `init` |
| `record struct P(int X)` | value | **mutable** `get; set` |
| `readonly record struct P(int X)` | value | `init`, immutable |
| `record P { … }` | reference | hand-declared |
| `record B : A` | record inheritance | equality takes the exact type into account |

### Equality

| | `class` | `struct` | `record` | `record struct` | `string` |
|---|---|---|---|---|---|
| `==` by default | references | does not compile (CS0019) | values | values | content |
| `Equals(object)` | references | fields (`ValueType`) | values | values | content |
| `ReferenceEquals` | references | always `false` | references | always `false` | references |

## Gotchas

Every gotcha was verified by running it.

### A struct in a list cannot be changed in place

`list[0].X = 5;` is error CS1612. You can change a copy, but the element stays the same: `var copy = list[0]; copy.X = 5;` → `list[0].X == 1`. For an array `arr[0].X = 7` works.

### Calling a struct method through a readonly field

`h.Inner.Inc(); h.Inner.Inc();` for a `readonly Inner` field left `N == 0`: each call ran on a defensive copy.

### Equality of records with collections

Two records with lists that are equal in content are **not equal**: `List<T>` is compared by reference. `with` does not copy the list — a change to the copy is visible in the original (`x,y`).

### A mutable key in a HashSet

After `k.Id = 2` the element stayed in the set (`Count == 1`), but `Contains(k)` returned `False`.

### Equals without GetHashCode

`Contains(new NoHash(1))` → `False` for a type that overrides only `Equals`. The compiler warns: CS0659.

### A record and its derived record are not equal

`new Student("Ann", 30, "MIT") == new Person("Ann", 30)` → `False`, `person.Equals(student)` → `False`. Equality requires the same `EqualityContract`.

### NaN

`double.NaN == double.NaN` → `False`, while `double.NaN.Equals(double.NaN)` → `True`: the operator follows IEEE 754, and `Equals` must be reflexive (so that `NaN` can be found in a dictionary). For records and structs with a `double` field this gives the same discrepancy.

### A default method is not visible on the class

`new Sq().Describe()` is error CS1061: the method is available only through `IShape`.

### Assigning to a record and inheriting from a sealed class

`p.Name = "b"` for a positional record is error CS8852 (`init` only); inheriting from a `sealed` class is CS0509.

### Strings and references

`"a" + "b" == string.Concat("a", "b")` → `True` (compared by content), while `ReferenceEquals(new string('a', 2), new string('a', 2))` → `False`: two different strings in memory.

## Trade-offs and alternatives

| Task | Option | Price |
|---|---|---|
| Data with equality | `record` | not suitable for objects with identity (database entities) |
| | a class + manual `Equals` | a lot of boilerplate, easy to get wrong |
| A small value | `readonly record struct` | copied whole; a large size hurts |
| | a class | a heap allocation, load on the GC |
| A contract | interface | an implementation for each class |
| | abstract class | one base class |
| A shared implementation in a contract | a default method | visible only through the interface |
| Copying with a change | `with` | shallow |

Records suit messages, DTOs, keys and domain values. Entities with identity (an order with an `Id` that changes over time) are better modeled as classes: value equality hurts them.

## Interview questions

**1. How does an interface differ from an abstract class? What to choose?**

<details><summary>Answer</summary>

An interface is a contract without state and constructors (since C# 8 it can contain default implementations and static members); a class implements many interfaces. An abstract class is a base with fields, constructors and implemented members, but only one can be inherited. The choice: a contract and swappable implementations — an interface; shared implementation and state — an abstract class.

</details>

**2. What is explicit interface implementation and why is it needed?**

<details><summary>Answer</summary>

A member is implemented as `string IA.Run() => …` and is visible only through a cast to the interface. It is needed to resolve a conflict of identical signatures (`IA.Run` and `IB.Run` gave `A` and `B`, and the class method gave `class`) or to hide a member from the class's public API.

</details>

**3. How does a struct differ from a class?**

<details><summary>Answer</summary>

A value type: assignment copies data, there is no identity of its own and no inheritance (interfaces only), there is always a default value, and it is stored where the variable is declared (the stack, inside an object or an array). A class is a reference type: an object on the heap, references, inheritance, `null`.

</details>

**4. When is a struct better than a class?**

<details><summary>Answer</summary>

For small (by the guideline, up to ~16 bytes) immutable values that are created in large quantities and logically represent a single value. For large, mutable objects and objects with identity use a class: copying a struct is expensive, and mutable structs cause bugs with copies.

</details>

**5. Why does `list[0].X = 5` not compile for a struct?**

<details><summary>Answer</summary>

The `List<T>` indexer returns a **copy** of the struct, and changing the copy would be pointless; the compiler forbids it (CS1612). The solution: take a copy, change it, write it back (`list[0] = copy`), or make the struct immutable. For an array it works: an array element is a variable, not a copy.

</details>

**6. What is a defensive copy?**

<details><summary>Answer</summary>

A copy of a struct that the compiler creates to call its method through a `readonly` field or an `in` parameter: otherwise the method could change the "immutable" value. Because of it, a mutating method changes nothing (verified: `N == 0`) and extra copying is added. A `readonly struct` needs no defensive copies.

</details>

**7. What is a `record` and how does it differ from an ordinary class?**

<details><summary>Answer</summary>

Syntax for data types: the compiler generates value equality (`Equals`, `GetHashCode`, `==`, `!=`), `ToString`, deconstruction, a copy constructor and a method for `with`. By default a `record` is a class (a reference type), and the properties of positional parameters are `init`. For a value type there is `record struct`.

</details>

**8. What does the `with` expression do? Is it a deep copy?**

<details><summary>Answer</summary>

It creates a copy of the record (through the copy constructor / `<Clone>$`) and applies the initializer. The copy is shallow: nested reference objects are shared — adding to the copy's list changed the original's list (verified).

</details>

**9. Are two records with the same values equal? And with the same list contents?**

<details><summary>Answer</summary>

Records with equal simple values are equal (`==`, `Equals`, equal hashes). If a property is a `List<T>`, the list is compared by reference, so records with different lists, even identical in content, are not equal (verified).

</details>

**10. Are a record and an instance of its derived record with the same shared properties equal?**

<details><summary>Answer</summary>

No: `Equals` compares `EqualityContract` (the exact type), so `student == person` and `person.Equals(student)` gave `False` — the result is symmetric.

</details>

**11. What are the rules of the `Equals`/`GetHashCode` contract?**

<details><summary>Answer</summary>

Equal objects must have equal hash codes; the hash code does not change while the object is in a collection; `Equals` is reflexive, symmetric and transitive; `x.Equals(null)` is `false`. Overriding one without the other is warning CS0659, and objects get lost in `HashSet`/`Dictionary` (verified).

</details>

**12. Why implement `IEquatable<T>`?**

<details><summary>Answer</summary>

A typed `Equals(T)` needs no cast and no boxing (for structs), and it is what `Dictionary<TKey,…>`, `HashSet<T>` and `List<T>.Contains` use (verified: `IEquatable` was called 2 times, `object.Equals` 0).

</details>

**13. Trick question: what happens when you change a field that participates in `GetHashCode` for an object in a `HashSet`?**

<details><summary>Answer</summary>

The element becomes "lost": it stays in the set (`Count == 1`), but `Contains` does not find it because it looks in the bucket for the new hash. Keys must be immutable.

</details>

**14. How does a struct that implements an interface get boxed? How to avoid it?**

<details><summary>Answer</summary>

On assignment to a variable of the interface type or on passing to a parameter of the interface type a boxed copy is created (verified: 24 bytes per call). To avoid it: a generic method with the constraint `where T : IFoo` — the JIT calls the method directly for the concrete struct (0 bytes).

</details>

**15. What is a default interface implementation and what are its limits?**

<details><summary>Answer</summary>

A method with a body directly in an interface (C# 8): it lets you add members to published interfaces without breaking implementations. Limits: it is available only through an interface variable (`new Sq().Describe()` does not compile); if two implementations conflict, the class must implement the method itself; there is no state (instance fields) in an interface.

</details>

**16. What are `static abstract` interface members?**

<details><summary>Answer</summary>

Since C# 11 an interface can require a static member (an operator, a factory): `static abstract T Add(T a, T b)`. The implementing type provides it, and generic code calls it through the type parameter (`T.Add(a, b)`). Generic math (`INumber<T>`) is built on this.

</details>

**17. Trick question: how does `double.NaN == double.NaN` differ from `double.NaN.Equals(double.NaN)`?**

<details><summary>Answer</summary>

`==` follows IEEE 754 and gives `False`; `Equals` gives `True` because the contract requires reflexivity (otherwise `NaN` could not be found in a dictionary or set).

</details>

**18. What is the `==` of `string`? Why can `ReferenceEquals` of two identical strings be `false`?**

<details><summary>Answer</summary>

For `string` the `==` operator is overloaded and compares content. `ReferenceEquals` compares references: two strings created at run time (`new string('a', 2)`) are different objects (verified: `False`), although `==` returns `True`.

</details>

## Cheat sheet

| Need | Solution |
|---|---|
| A contract | `interface` |
| Evolve an interface without breaking implementations | a default implementation |
| Two interfaces with the same method name | explicit implementation |
| Generic math | `static abstract` |
| Data with equality and `with` | `record` |
| A small immutable value | `readonly record struct` |
| A struct in hot code | a generic method with `where T : IFoo`, `IEquatable<T>` |
| A dictionary key | an immutable type (`record`) |
| A class with value equality | `IEquatable<T>` + `Equals` + `GetHashCode` |
| A deep copy | manually; `with` is shallow |

## Related materials

- Before: [basics](basics.en.md), [advanced](advanced.en.md)
- Classes, inheritance, `object`: [04 · OOP: classes and objects](../04-oop-basics/expert.en.md)
- Boxing, `ref struct`: [02 · Types and variables](../02-types-and-variables/expert.en.md)
- `in`, `ref readonly`, defensive copies: [03 · Control flow and methods](../03-control-flow-and-methods/expert.en.md)
- Allocations and measurements: [12 · Memory and GC](../12-memory-and-gc/advanced.en.md#measuring-allocations)
- Generics and constraints: [06 · Collections and generics](../06-collections-and-generics/overview.en.md)

## Sources

- [Records: what the compiler generates](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/record#nondestructive-mutation)
- [Records specification](https://learn.microsoft.com/dotnet/csharp/language-reference/proposals/csharp-9.0/records)
- [Structure types](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/struct)
- [Interfaces](https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/interface)
- [Generic math](https://learn.microsoft.com/dotnet/standard/generics/math)
- [Object.Equals: remarks](https://learn.microsoft.com/dotnet/api/system.object.equals)
- [Object.GetHashCode: remarks](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)
- [Guidelines for choosing between class and struct](https://learn.microsoft.com/dotnet/standard/design-guidelines/choosing-between-class-and-struct)
