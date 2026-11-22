---
title: "Quiz: Interfaces, structs, records"
lang: en
track: csharp
kind: module
doc: quiz
slug: 05-interfaces-structs-records
part: 1
tags: [interface, struct, record, equality, gethashcode, with-expression]
status: draft
updated: 2026-11-22
---

# Quiz: Interfaces, structs, records

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 Which of these is possible in C#?

- A) a class inherits from two classes
- B) a class implements several interfaces
- C) an interface is created with `new`
- D) a struct inherits from a class

<details><summary>Answer</summary>

**B.** There is one base class, but you can implement any number of interfaces. An interface cannot be created — only an object that implements it. Structs do not inherit classes (apart from the implicit `ValueType`).
→ [Basics](basics.en.md#interfaces)

</details>

## 2. 🟢 What does the code print?

```csharp
record Point(int X, int Y);

var p1 = new Point(1, 2); var p2 = new Point(1, 2);
var p3 = p1 with { X = 10 };
Console.WriteLine($"{p1 == p2} {ReferenceEquals(p1, p2)} {p3}");
```

<details><summary>Answer</summary>

```text
True False Point { X = 10, Y = 2 }
```

A record has value equality (`==` compares properties), but these are two different objects in memory. `with` creates a copy with a changed property; the original `p1` does not change.
→ [Basics](basics.en.md#records)

</details>

## 3. 🟢 Select all correct statements

- [ ] A) a `record` is a reference type by default
- [ ] B) a `record struct` is a value type
- [ ] C) properties of a positional record can be assigned after creation
- [ ] D) a struct is copied on assignment

<details><summary>Answer</summary>

**A, B, D.** C is wrong: positional record properties are `init`, and assignment after creation is error `CS8852` (verified). Use `with` to change.
→ [Basics](basics.en.md#structs)

</details>

## 4. 🟢 How does an interface differ from an abstract class?

- A) an interface describes a contract without state (fields), and a class can implement several interfaces
- B) an interface can have instance fields
- C) an abstract class can be implemented by several classes at once
- D) there is no difference

<details><summary>Answer</summary>

**A.** An interface is a contract without state (since C# 8 it allows default methods). An abstract class can hold fields and shared logic, but you can inherit only one.
→ [Basics](basics.en.md#interface-or-abstract-class)

</details>

## 5. 🟡 What happens?

```csharp
struct Vec { public int X; }
var list = new List<Vec> { new() { X = 1 } };
list[0].X = 5;
```

- A) `list[0].X` becomes 5
- B) compile error CS1612
- C) a run-time exception
- D) nothing, the value stays 1

<details><summary>Answer</summary>

**B.** A list indexer returns a **copy** of the struct, so changing the copy is pointless, and the compiler refuses (`CS1612`). The fix: write the whole new value — `list[0] = list[0] with { X = 5 }` (for a `record struct`) or `var v = list[0]; v.X = 5; list[0] = v;`. For an array `arr[0].X = 7` works: an array element is accessible by reference.
→ [Advanced theory](advanced.en.md#mutable-structs)

</details>

## 6. 🟡 What does the code print?

```csharp
class BadKey { public int Id; public override bool Equals(object? o) => o is BadKey b && b.Id == Id; public override int GetHashCode() => Id; }

var set = new HashSet<BadKey>();
var k = new BadKey { Id = 1 };
set.Add(k);
k.Id = 2;
Console.WriteLine($"{set.Count} {set.Contains(k)}");
```

- A) `1 True`
- B) `1 False`
- C) `0 False`
- D) `2 True`

<details><summary>Answer</summary>

**B.** The element stayed in the set (`Count == 1`) but sits in the bucket for the old hash code, so a lookup with the new hash does not find it. Collection keys must be immutable — for example a record `GoodKey(int Id)`.
→ [Advanced theory](advanced.en.md#gotcha-a-mutable-key)

</details>

## 7. 🟡 Select all correct statements

- [ ] A) `Equals` without `GetHashCode` gives compiler warning `CS0659`
- [ ] B) two records with equal-by-content `List<int>` are equal
- [ ] C) `with` copies a `List<T>` shallowly
- [ ] D) an explicit interface implementation is visible only through a variable of the interface type

<details><summary>Answer</summary>

**A, C, D.** B is wrong: `List<T>` is compared by reference, so such records are not equal, and `with` does not clone the list — a change to the copy is visible in the original. A: without `GetHashCode` such types are not found in `HashSet`/`Dictionary` (verified).
→ [Advanced theory](advanced.en.md#records-in-depth)

</details>

## 8. 🟡 A default interface method is not visible through the class. Why does this code not compile?

```csharp
interface IShape { string Describe() => "shape"; }
class Sq : IShape { }

new Sq().Describe();
```

<details><summary>Answer</summary>

Error `CS1061`: default methods are accessible only through a variable of the interface type — `IShape s = new Sq(); s.Describe();`. The class "inherits" the contract but not the members themselves. If you need the call through the class, implement the method in the class.
→ [Advanced theory](advanced.en.md#default-implementation-and-explicit-implementation)

</details>

## 9. 🟡 Open question: when do you choose `struct`, `class` and `record`?

<details><summary>Key points of a good answer</summary>

- a struct — small immutable values (around 16 bytes), without frequent boxing, when copy semantics are needed;
- a class — when there is identity, mutable state, a large size or inheritance;
- a `record` (class) — data with value equality, `with`, deconstruction: DTOs, events, domain values;
- a `readonly record struct` — small values with value equality;
- structs boxed into `object`/an interface lose the advantage — they are copied to the heap.

</details>

## 10. 🔴 What does the code print?

```csharp
Console.WriteLine(double.NaN == double.NaN);
Console.WriteLine(double.NaN.Equals(double.NaN));
```

- A) `True` / `True`
- B) `False` / `True`
- C) `False` / `False`
- D) `True` / `False`

<details><summary>Answer</summary>

**B.** The `==` operator follows IEEE 754 (`NaN` is not equal to anything, even itself), while `Equals` must be reflexive so that `NaN` can be found in a dictionary. The same divergence arises for records and structs with a `double` field (verified).
→ [Expert theory](expert.en.md#nan)

</details>

## 11. 🔴 Find the bug

```csharp
struct Inner { public int N; public void Inc() => N++; }
class Holder { public readonly Inner I = new(); }

var h = new Holder();
h.I.Inc(); h.I.Inc();
Console.WriteLine(h.I.N);
```

<details><summary>Answer</summary>

It prints `0`: for a `readonly` field holding a struct the compiler calls the method on a **defensive copy**, so the changes are lost (verified: `N == 0`). The fix: make the struct a `readonly struct` with immutable state (and the method returns a new value), or remove `readonly` from the field.
→ [Expert theory](expert.en.md#calling-a-struct-method-through-a-readonly-field)

</details>
