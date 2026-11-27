---
title: "OOP: classes and objects: basics"
lang: en
track: csharp
kind: module
doc: basics
slug: 04-oop-basics
part: 1
tags: [class, object, constructor, property, encapsulation, access-modifiers, static, inheritance, polymorphism, virtual-override]
versions: ".NET 11, C# 15"
status: verified
updated: 2026-11-27
---

# OOP: classes and objects: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [02 · Types and variables](../02-types-and-variables/overview.en.md), [03 · Control flow and methods](../03-control-flow-and-methods/overview.en.md).

## Why it matters

Object-oriented programming (OOP) groups data and the code that works with it into one type. A **class** is a blueprint; an **object** (instance) is what the `new` operator creates from it. This gives three things:

- **encapsulation**: an object's state is hidden and can be changed only through methods and properties that protect the domain rules;
- **inheritance**: a new class builds on an existing one and extends it;
- **polymorphism**: code works with a base type, while the behavior is determined by the object's actual type.

A class is a **reference type** (see [02 · Types and variables](../02-types-and-variables/basics.en.md)): a variable holds a reference to an object on the managed heap, and the [garbage collector](../12-memory-and-gc/basics.en.md) frees the object.

## A class and an object

```csharp
var account = new Account("Ann", 100m);
account.Deposit(50m);
Console.WriteLine(account.Balance);         // 150

public class Account
{
    private decimal _balance;               // field: the object's state, hidden from outside

    public string Owner { get; }            // property: controlled access

    public Account(string owner, decimal initial)   // constructor: runs on `new`
    {
        Owner = owner;
        _balance = initial;
    }

    public decimal Balance => _balance;     // read-only property

    public void Deposit(decimal amount)     // method: behavior
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount));
        _balance += amount;
    }
}
```

Class members: **fields** (data), **methods** (actions), **properties** (access to data), **constructors** (initialization), **events** and **indexers**. A field stores data, a method runs code, and a property looks like a field but is really a pair of `get`/`set` methods.

## Access modifiers

A modifier determines who sees a member or a type:

| Modifier | Visible |
|---|---|
| `public` | everywhere |
| `private` | only inside its own type (the default for class members) |
| `protected` | in its own type and in derived classes |
| `internal` | in its own assembly (the default for top-level types) |
| `protected internal` | in the assembly **or** in derived classes |
| `private protected` | in derived classes **within its own assembly** |

Verified through reflection: a field without a modifier is `private`, and a top-level class without a modifier is `internal` (not `public`). The encapsulation rule: keep everything as closed as possible and expose only what is needed from outside.

## Fields and properties

A field is a variable inside a class. Public fields are considered bad practice: there is no way to check the value through them. A property lets you add logic without changing the calling code:

```csharp
public class Product
{
    public string Name { get; set; } = "";          // auto-property: the compiler creates a hidden field
    public decimal Price { get; private set; }      // set only inside the class
    public string Sku { get; init; } = "";          // set only in a constructor or an object initializer
    public bool IsFree => Price == 0;               // computed property, no field
}
```

- **An auto-property** (`{ get; set; }`): the compiler creates a hidden field itself (verified: `<Name>k__BackingField`).
- **`private set`**: the value can be changed only from the class.
- **`init`**: the value is set at creation (in a constructor or an object initializer) and does not change afterwards.
- **An expression-bodied property** (`=> …`): computed on every access.

## Constructors

A constructor is a method with no return type and the name of the class. If there are no constructors, the compiler creates an empty parameterless one. As soon as you declare your own, the empty one is no longer created.

```csharp
public class Rectangle
{
    public double Width { get; }
    public double Height { get; }

    public Rectangle(double width, double height)
    {
        Width = width;
        Height = height;
    }

    public Rectangle(double side) : this(side, side) { }   // chaining: calls the other constructor
}
```

`: this(...)` calls another constructor of the same class, `: base(...)` calls a constructor of the base class.

A **primary constructor** (C# 12) puts the parameters right in the class header:

```csharp
public class Counter(int start)
{
    public int Next() => start++;      // the parameter is visible in the whole class
}
```

A primary constructor parameter is **neither a field nor a property**: the compiler creates a hidden field only if the parameter is used in members (verified: the `Counter` class has a private field `<start>P`).

An **object initializer** sets properties right after creation, and the `required` modifier forces the caller to set them:

```csharp
var ann = new Person { Name = "Ann", Age = 30 };   // without Name — a compile error

public class Person
{
    public required string Name { get; init; }
    public int Age { get; init; }
}
```

## Static members and this

A member with `static` belongs to the type, not an instance: it is shared by all objects and is called through the type name (`Math.Max`). A **static class** (`static class`) cannot be instantiated or inherited — it holds only static members; in metadata it is both `abstract` and `sealed` (verified).

`this` is a reference to the current object. It is needed when a parameter name matches a member name, when passing the object to another method, and in constructor chaining.

## Inheritance

A class inherits from one base class (C# has no multiple inheritance of classes). The derived class gets all its members except constructors:

```csharp
public class Animal
{
    public string Name { get; set; } = "";
    public virtual string Speak() => "...";
}

public class Dog : Animal
{
    public override string Speak() => "Woof";
}
```

All classes implicitly inherit from `System.Object`. That provides `ToString()`, `Equals()`, `GetHashCode()` and `GetType()`. The default `ToString()` returns the type name (`Plain`), and the default `Equals` compares references: two different objects with the same content are not equal (verified).

## Polymorphism

A method declared `virtual` can be replaced by a derived class with `override`. Which version runs is decided by the **actual type** of the object, not the variable's type:

```csharp
Animal animal = new Dog();
Console.WriteLine(animal.Speak());     // Woof — the actual type is Dog
```

An abstract class (`abstract`) cannot be instantiated; it provides a template, and its `abstract` members must be implemented by derived classes:

```csharp
public abstract class Shape
{
    public abstract string Name { get; }
    public string Describe() => $"I am {Name}";     // shared behavior
}

public class Circle : Shape
{
    public override string Name => "circle";
}
```

The result of `new Circle().Describe()` is `I am circle`. The `sealed` modifier forbids further inheritance from a class or overriding of a method.

## Type checks and casts

```csharp
object o = "text";
bool a = o is string s && s.Length == 4;    // true; s is available in the true branch
string? b = o as string;                    // null if the type does not match
int c = (int)o;                             // InvalidCastException
```

`is` checks the type, `as` returns `null` instead of throwing, and a direct cast `(T)x` throws `InvalidCastException`. More in [02 · Types and variables](../02-types-and-variables/basics.en.md).

## Beginner mistakes

- ❌ Public fields instead of properties → ✅ properties: you can add validation without breaking calling code.
- ❌ Inheriting to reuse code → ✅ inherit when "is a" holds (a dog is an animal); otherwise use composition: a field of another type.
- ❌ Forgetting `override` and getting a hiding warning → ✅ more in [advanced](advanced.en.md#virtual-override-and-new).
- ❌ Expecting `==` or `Equals` to compare the content of class objects → ✅ references are compared by default.
- ❌ Making everything `public` → ✅ the minimum necessary access.
- ❌ A constructor without argument checks → ✅ an object must be valid from the very start.

## Related materials

- Next: [advanced](advanced.en.md) — initialization order, `new` hiding, the `field` keyword, extensions, `partial`
- Value and reference types: [02 · Types and variables](../02-types-and-variables/basics.en.md)
- Method parameters and overloading: [03 · Control flow and methods](../03-control-flow-and-methods/basics.en.md)
- Interfaces, structs, records: [05 · Interfaces, structs, records](../05-interfaces-structs-records/overview.en.md)
- Releasing objects: [12 · Memory and GC](../12-memory-and-gc/basics.en.md)
- The same in other tracks: [Python](../../../python/overview.en.md) — classes, `__init__`, inheritance, `@property`

## Sources

- [Classes](https://learn.microsoft.com/dotnet/csharp/fundamentals/types/classes)
- [Access modifiers](https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/access-modifiers)
- [Properties](https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/properties)
- [Instance constructors](https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/instance-constructors)
- [Inheritance](https://learn.microsoft.com/dotnet/csharp/fundamentals/object-oriented/inheritance)
- [Polymorphism](https://learn.microsoft.com/dotnet/csharp/fundamentals/object-oriented/polymorphism)
