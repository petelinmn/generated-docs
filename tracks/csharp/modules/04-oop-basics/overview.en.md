---
title: "OOP: classes and objects"
lang: en
track: csharp
kind: module
doc: overview
slug: 04-oop-basics
part: 1
levels: [1, 2, 3, 4]
tags: [class, object, constructor, property, encapsulation, access-modifiers, static, inheritance, polymorphism, virtual-override]
versions: ".NET 11, C# 15"
status: verified
updated: 2026-11-27
---

# OOP: classes and objects

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

A class describes a type: data (fields, properties) and behavior (methods). An object is an instance of a class on the managed heap, created with `new`. The object-oriented approach rests on encapsulation (state hidden behind properties and methods), inheritance (a class extends another) and polymorphism (behavior is determined by the object's actual type). In C# 15 classes have primary constructors, `required`, `init` and the `field` keyword.

## Key ideas

- **A class is a reference type:** a variable holds a reference, and `==` and `Equals` compare references by default.
- **Encapsulation:** `private` fields, properties outside; six access levels, members are `private` by default and types are `internal`.
- **Properties:** auto-properties, `init`, `private set`, `required`, `field` for logic without your own field.
- **Constructors:** `this(...)`/`base(...)` chains, primary constructors, the static constructor.
- **Polymorphism:** `virtual` + `override`; `new` only hides a member — the result depends on the variable's type.
- **Initialization order:** derived fields → base fields → base constructor → derived constructor; a virtual call in a constructor sees a half-built object.
- **Composition is often better than inheritance:** inherit for "is a", otherwise keep an object in a field.

## Minimal example

```csharp
Animal animal = new Dog();
Console.WriteLine(animal.Speak());     // Woof — decided by the actual type

public class Animal
{
    public virtual string Speak() => "...";
}

public class Dog : Animal
{
    public override string Speak() => "Woof";
}
```

## When to use and when not

- ✅ Classes — for entities with identity and mutable state, for types with behavior.
- ✅ Inheritance — for a real "is a" relationship and shared implementation.
- ✅ `sealed` — by default for classes not designed for inheritance.
- ❌ Inheritance just to reuse code — use composition.
- ❌ Virtual calls in constructors.
- ❌ `new` hiding — use `override` or another name.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | a class and an object, access modifiers, fields and properties, constructors, `static`, inheritance, polymorphism, type casts |
| [2. Advanced](advanced.en.md) | initialization order, `virtual`/`override`/`new`, overloading in hierarchies, `field`, indexers, `const`/`readonly`, extension methods, `partial`, composition |
| [3. Expert](expert.en.md) | an object in memory, `callvirt`, property metadata and `init`/`required`, a reference of modifiers, gotchas, 18 interview questions |
| [4. Self-check](quiz.en.md) | 10 questions and [5 exercises](exercises.en.md) with solutions |
| 5. Runnable examples | not generated yet — `/learn csharp 04-oop-basics level 5` |
| 6. Tests | not generated yet — `/learn csharp 04-oop-basics level 6` |

## Related materials

- Prerequisites: [02 · Types and variables](../02-types-and-variables/overview.en.md), [03 · Control flow and methods](../03-control-flow-and-methods/overview.en.md)
- Next: [05 · Interfaces, structs, records](../05-interfaces-structs-records/overview.en.md)
- Releasing objects: [12 · Memory and GC](../12-memory-and-gc/overview.en.md)
- The same in other tracks: [Python](../../../python/overview.en.md) — classes, `__init__`, `@property`
- Recipes and libraries: [Mapperly](../../libraries/mapperly/overview.en.md)
