---
title: "Exercises: Interfaces, structs, records"
lang: en
track: csharp
kind: module
doc: exercises
slug: 05-interfaces-structs-records
part: 1
tags: [interface, record, readonly-struct, equality, static-abstract, explicit-implementation]
status: draft
updated: 2026-11-22
---

# Exercises: Interfaces, structs, records

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. Solutions are under spoilers. The code was verified by running it; there are no runnable versions in `code/` yet (level 5 does not exist).

## 1. 🟢 Discounts through an interface

**Task:** declare an interface `IDiscount` with a method `decimal Apply(decimal price)` and two implementations: `PercentDiscount(decimal percent)` and `FixedDiscount(decimal amount)`. Write `ApplyAll(decimal price, IEnumerable<IDiscount> discounts)` that applies the discounts in turn; the price does not fall below zero.

**Expected result:**

```text
ApplyAll(200, [10%, -5]) → 175
ApplyAll(3, [10%, -5])   → 0
```

<details><summary>Hint</summary>

The method works with the interface and knows no concrete types. Clamp the result with `Math.Max(price, 0)`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 1: depend on an interface, not on concrete types.
interface IDiscount { decimal Apply(decimal price); }
class PercentDiscount(decimal percent) : IDiscount { public decimal Apply(decimal price) => price * (100 - percent) / 100; }
class FixedDiscount(decimal amount) : IDiscount { public decimal Apply(decimal price) => price - amount; }

static decimal ApplyAll(decimal price, IEnumerable<IDiscount> discounts)
{
    foreach (var d in discounts) price = d.Apply(price);
    return Math.Max(price, 0);
}
```

Verified: `175` and `0`. A new discount can be added without changing `ApplyAll`.

</details>

## 2. 🟢 A point record

**Task:** declare a record `Point(int X, int Y)`. Create two equal points, a third as a copy of the first with `X = 10` via `with`, and deconstruct it into `x`, `y`. Print the results of `==`, `ReferenceEquals` and the string of the third point.

**Expected result:**

```text
True False Point { X = 10, Y = 2 } 10,2
```

<details><summary>Hint</summary>

`var p3 = p1 with { X = 10 };` and `var (x, y) = p3;`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 2: positional record — value equality, with-expression, deconstruction.
record Point(int X, int Y);

var p1 = new Point(1, 2); var p2 = new Point(1, 2);
var p3 = p1 with { X = 10 };
var (x, y) = p3;
Console.WriteLine($"{p1 == p2} {ReferenceEquals(p1, p2)} {p3} {x},{y}");
```

Verified: the output matches. These are two different objects with an equal value.

</details>

## 3. 🟡 An immutable vector

**Task:** declare `readonly record struct Vec(int X, int Y)` with a `+` operator. Put a vector into a `List<Vec>` and "change" the X of the first element: a direct `list[0].X = 5` does not compile — do it through `with`.

**Expected result:**

```text
list[0] after replacement → Vec { X = 5, Y = 2 }
(1,2) + (3,4)             → Vec { X = 4, Y = 6 }
```

<details><summary>Hint</summary>

A list element is a copy. Write a new value into the slot: `list[0] = list[0] with { X = 5 };`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 3: an immutable struct changed by replacing the value.
readonly record struct Vec(int X, int Y)
{
    public static Vec operator +(Vec a, Vec b) => new(a.X + b.X, a.Y + b.Y);
}

var list = new List<Vec> { new(1, 2) };
list[0] = list[0] with { X = 5 };
Console.WriteLine(list[0]);                      // Vec { X = 5, Y = 2 }
Console.WriteLine(new Vec(1, 2) + new Vec(3, 4)); // Vec { X = 4, Y = 6 }
```

Verified: the output matches. Immutable structs eliminate a whole class of copy-related bugs.

</details>

## 4. 🟡 A lost key

**Task:** a class `BadKey` with a mutable field `Id` and `Equals`/`GetHashCode` based on `Id` is put in a `HashSet`, then `Id` is changed. Show that `Contains` returns `False`, and replace the key with a record `GoodKey(int Id)` so that a lookup by an equal value finds the element.

**Expected result:**

```text
BadKey: Count = 1, Contains = False
GoodKey: Contains(new GoodKey(1)) = True
```

<details><summary>Hint</summary>

The hash code is computed on add. A key must be immutable — a record gives both immutability and value equality.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 4: mutable keys break hash-based collections; immutable records do not.
class BadKey
{
    public int Id;
    public override bool Equals(object? o) => o is BadKey b && b.Id == Id;
    public override int GetHashCode() => Id;
}
record GoodKey(int Id);

var set = new HashSet<BadKey>();
var k = new BadKey { Id = 1 };
set.Add(k);
k.Id = 2;
Console.WriteLine($"{set.Count} {set.Contains(k)}");   // 1 False

var set2 = new HashSet<GoodKey> { new GoodKey(1) };
Console.WriteLine(set2.Contains(new GoodKey(1)));      // True
```

Verified: `1 False` and `True`.

</details>

## 5. 🔴 Static interface members and explicit implementation

**Task:** (a) declare `interface IAddable<T> where T : IAddable<T>` with `static abstract T Zero` and `static abstract operator +`; implement it for `readonly record struct Meters(double Value)` and write a generic `Total<T>(IEnumerable<T>)`. (b) Declare `IReader` with a default method `Describe()` and `IWriter` with a property `Name`; the class `File2` implements both, and `Name` for `IWriter` — explicitly.

**Expected result:**

```text
Total([1.5 m, 2 m]).Value → 3.5
((IReader)new File2()).Describe() → I am reader
((IWriter)new File2()).Name       → writer
```

<details><summary>Hint</summary>

Inside `Total` start from `T.Zero` and add with `+`. Explicit implementation: `string IWriter.Name => "writer";` — visible only through the interface.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 5: static abstract interface members and explicit implementation.
interface IAddable<T> where T : IAddable<T>
{
    static abstract T Zero { get; }
    static abstract T operator +(T a, T b);
}

readonly record struct Meters(double Value) : IAddable<Meters>
{
    public static Meters Zero => new(0);
    public static Meters operator +(Meters a, Meters b) => new(a.Value + b.Value);
}

static T Total<T>(IEnumerable<T> items) where T : IAddable<T>
{
    var sum = T.Zero;
    foreach (var i in items) sum = sum + i;
    return sum;
}

interface IReader { string Name => "reader"; string Describe() => $"I am {Name}"; }
interface IWriter { string Name { get; } }
class File2 : IReader, IWriter { string IWriter.Name => "writer"; }

Console.WriteLine(Total([new Meters(1.5), new Meters(2)]).Value);   // 3.5
IReader r = new File2(); Console.WriteLine(r.Describe());           // I am reader
Console.WriteLine(((IWriter)new File2()).Name);                     // writer
```

Verified: the output matches (the number depends on the output culture: `3.5` or `3,5`). The same name `Name` in two interfaces is disambiguated by explicit implementation.

</details>
