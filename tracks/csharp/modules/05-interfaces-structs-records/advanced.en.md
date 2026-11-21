---
title: "Interfaces, structs, records: advanced"
lang: en
track: csharp
kind: module
doc: advanced
slug: 05-interfaces-structs-records
part: 1
tags: [explicit-interface-implementation, default-interface-methods, static-abstract-members, iequatable, gethashcode, mutable-struct, defensive-copy, boxing, record-inheritance, readonly-struct]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Interfaces, structs, records: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md)

## Default implementation and explicit implementation

A **default implementation** (C# 8) lets you add a method with a body to an interface without breaking existing classes:

```csharp
interface IShape
{
    string Name { get; }
    string Describe() => $"shape {Name}";   // default implementation
}

class Sq : IShape { public string Name => "sq"; }

IShape s = new Sq();
Console.WriteLine(s.Describe());             // shape sq
// new Sq().Describe();  // error CS1061: Sq has no member Describe
```

A default method is visible **only through a variable of the interface type**: the `Sq` class has no such member (verified: `typeof(Sq).GetMethod("Describe")` is `null`, and the call `new Sq().Describe()` does not compile). Default implementations exist to evolve published interfaces; for ordinary code reuse an abstract class or an extension method is better.

An **explicit implementation** binds a member to a specific interface. It is needed when two interfaces require a method with the same signature but different meanings, or when a member should not be visible on the class itself:

```csharp
interface IA { string Run(); }
interface IB { string Run(); }

class Both : IA, IB
{
    string IA.Run() => "A";
    string IB.Run() => "B";
    public string Run() => "class";
}

var both = new Both();
Console.WriteLine($"{((IA)both).Run()} {((IB)both).Run()} {both.Run()}");   // A B class
```

An explicitly implemented member has no access modifier and is called only through a cast to the interface.

## Static interface members

Since C# 11 an interface can require **static abstract members**: operators and factory methods. They let you write generic code over numbers and other types with a common set of operations (generic math):

```csharp
interface IAdd<T> where T : IAdd<T>
{
    static abstract T Add(T a, T b);
}

record struct MyNum(int V) : IAdd<MyNum>
{
    public static MyNum Add(MyNum a, MyNum b) => new(a.V + b.V);
}

static T Sum<T>(T a, T b) where T : IAdd<T> => T.Add(a, b);
// Sum(new MyNum(2), new MyNum(3)).V → 5
```

The call `T.Add` is resolved through the type parameter, not an instance. The built-in numeric interfaces (`INumber<T>`, `IAdditionOperators<…>`) work the same way; more on type parameter constraints in [06 · Collections and generics](../06-collections-and-generics/overview.en.md).

## Equality and GetHashCode

For class objects to compare by value, you must implement three things consistently:

```csharp
class Eq : IEquatable<Eq>
{
    public int V;
    public Eq(int v) { V = v; }

    public bool Equals(Eq? other) => other is not null && other.V == V;     // typed, no boxing
    public override bool Equals(object? obj) => Equals(obj as Eq);          // for non-generic code
    public override int GetHashCode() => V;                                 // consistent with Equals
}
```

The contract:

- equal objects **must** have equal hash codes (the reverse is not true: collisions are allowed);
- the hash code must not change while the object is in a hash-based collection;
- `Equals` is reflexive, symmetric and transitive, and `x.Equals(null)` is `false`.

`IEquatable<T>.Equals` is typed: the `Dictionary` and `HashSet` collections call exactly it (verified: after `ContainsKey` and `List.Contains` the `IEquatable` counter is 2 and `object.Equals` is 0), so for structs it avoids boxing.

### Gotcha: a mutable key

```csharp
var k = new Key(1);
var set = new HashSet<Key> { k };
k.Id = 2;                       // the hash code changed while the key is inside
Console.WriteLine(set.Contains(k));   // False
Console.WriteLine(set.Count);         // 1 — the element is still there, but unreachable
```

The element sits in the bucket for hash `1` but is looked up in the bucket for hash `2`: it is lost but not removed. Collection keys must be immutable — which is why records are good for them.

### Gotcha: Equals without GetHashCode

If you override only `Equals`, objects are equal but have different hash codes, and a `HashSet` will not find them: the check on a test type showed `Contains(new NoHash(1))` → `False` with equal values. The compiler warns about it (CS0659).

## Mutable structs

Mutable structs are a source of hard-to-find bugs, because assignment copies the value.

```csharp
var list = new List<Pt> { new(1, 2) };
// list[0].X = 5;           // error CS1612: the indexer returns a copy
var copy = list[0];
copy.X = 5;                 // changes the copy only: list[0].X is still 1
list[0] = copy;             // the right way: replace the element

var arr = new[] { new Pt(1, 2) };
arr[0].X = 7;               // works: an array element is a variable, not a copy
```

The list indexer returns a copy, so in-place modification is forbidden (CS1612), while for an array it is allowed.

The second trap is **defensive copies**. If you call a mutating struct method through a `readonly` field, the compiler makes a copy, and the change is lost:

```csharp
struct Inner { public int N; public void Inc() => N++; }
class Holder { public readonly Inner Inner = new(); }

var h = new Holder();
h.Inner.Inc(); h.Inner.Inc();
Console.WriteLine(h.Inner.N);       // 0 — each call worked on a copy
```

One more — **boxing through an interface**. Casting a struct to an interface creates a boxed copy, and calls through the interface change it:

```csharp
interface IInc { void Inc(); }
struct Counter : IInc { public int N; public void Inc() => N++; }

IInc inc = new Counter();   // boxed once
inc.Inc(); inc.Inc();
Console.WriteLine(((Counter)inc).N);    // 2 — the box was changed
```

The result is correct only as long as you work with the same boxed object. The solution to all three traps: make structs **immutable** — `readonly struct` or `readonly record struct`:

```csharp
public readonly struct Money(decimal amount, string currency)
{
    public decimal Amount { get; } = amount;
    public string Currency { get; } = currency;
    public Money Add(decimal x) => new(Amount + x, Currency);   // returns a new value
}
```

The compiler does not allow changing a field in a `readonly struct`, and defensive copies are not needed for it (the metadata is marked `IsReadOnlyAttribute` — verified).

## Struct size and layout

A struct's size depends on field alignment:

| Struct | Fields | Size |
|---|---|---|
| `Pt` | `int`, `int` | 8 |
| `Mixed` | `byte`, `int`, `byte` | 12 |
| `Packed` | `int`, `byte`, `byte` | 8 |
| `Empty` | no fields | 1 |

Measured with `Unsafe.SizeOf<T>()`. Field order changes the size: the `Mixed` struct with "holes" is 4 bytes larger than `Packed` with the same fields. An empty struct takes 1 byte. Large structs are expensive to copy on passing and returning: `in` parameters and `ref readonly` returns are covered in [03 · Control flow and methods](../03-control-flow-and-methods/expert.en.md).

## Records in depth

### The `with` copy is shallow

```csharp
record Team(string Name, List<string> Members);

var t1 = new Team("A", new List<string> { "x" });
var t3 = t1 with { Name = "B" };
t3.Members.Add("y");
Console.WriteLine(string.Join(",", t1.Members));    // x,y — the same list
```

`with` copies property values, but the nested list stays shared: a record is "immutable" only one level deep.

### Equality of records with collections

Two `Team` records with lists equal in content but different list objects are not equal (`t1 == t2` → `False`): equality compares properties using their own equality, and `List<T>` is compared by reference. To get value equality, keep immutable collections with a suitable comparison or override `Equals`.

### Inheritance and equality

```csharp
record Person(string Name, int Age);
record Student(string Name, int Age, string School) : Person(Name, Age);

Console.WriteLine(new Student("Ann", 30, "MIT") == new Person("Ann", 30));   // False
Console.WriteLine(new Person("Ann", 30).Equals(new Student("Ann", 30, "MIT"))); // False
```

Records of different types are not equal even if the shared properties match: equality takes the **exact type** into account (`EqualityContract`). The result is symmetric — an important difference from hand-written equality in class hierarchies.

### Mutability

- The positional properties of `record class` and `readonly record struct` are `init`; `a.Name = "b"` does not compile (CS8852).
- For a `record struct` (without `readonly`) the positional properties are **mutable**: the setter has no `init` modifier (verified).
- Hand-declared properties can be anything: a record need not be immutable.

## Best practices

- Declare an interface next to the code that uses it, and keep it small (one meaning).
- Do not add members to an interface "for the future": every new member must be implemented by all classes.
- Structs should be `readonly` and small; when in doubt, take a class or a `record`.
- For DTOs, messages and dictionary keys — `record` or `readonly record struct`.
- If you override `Equals`, implement `IEquatable<T>` and `GetHashCode`.
- Do not use mutable objects as collection keys.

## Common mistakes

- ❌ A mutable struct in `List<T>` → ✅ `readonly struct` or a class.
- ❌ A struct implements an interface and is passed as the interface in hot code → ✅ a generic method with the constraint `where T : IFoo` does not box.
- ❌ Equality in a class hierarchy through a manual `GetType() != other.GetType()` → ✅ a record does this itself.
- ❌ `Equals` without `GetHashCode` → ✅ always together.
- ❌ Relying on `with` as a deep copy → ✅ copy nested objects manually.

## Comparison with other tracks

| | C# | Python |
|---|---|---|
| Contract | `interface` | `typing.Protocol` / `abc.ABC` |
| Data with equality | `record` | `@dataclass` |
| Immutable data | `readonly record struct` | `@dataclass(frozen=True)`, `NamedTuple` |
| Value type | `struct` | none (all objects are references) |
| Copy with a change | `with` | `dataclasses.replace` |

## Related materials

- Before: [basics](basics.en.md)
- Next: [expert](expert.en.md) — what the compiler generates for records, structs in memory, reference, interview questions
- `ref`, `in` and defensive copies: [03 · Control flow and methods](../03-control-flow-and-methods/expert.en.md)
- Boxing and value types: [02 · Types and variables](../02-types-and-variables/advanced.en.md)
- Collections and generics: [06 · Collections and generics](../06-collections-and-generics/overview.en.md)
- The same in other tracks: [Python](../../../python/overview.en.md)

## Sources

- [Explicit interface implementation](https://learn.microsoft.com/dotnet/csharp/programming-guide/interfaces/explicit-interface-implementation)
- [Default interface implementations](https://learn.microsoft.com/dotnet/csharp/advanced-topics/interface-implementation/default-interface-methods-versions)
- [Static abstract members](https://learn.microsoft.com/dotnet/csharp/whats-new/tutorials/static-virtual-interface-members)
- [How to define value equality for a type](https://learn.microsoft.com/dotnet/csharp/programming-guide/statements-expressions-operators/how-to-define-value-equality-for-a-type)
- [Readonly structs](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/struct#readonly-struct)
- [Records](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/record)
