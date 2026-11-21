---
title: "OOP: classes and objects: advanced"
lang: en
track: csharp
kind: module
doc: advanced
slug: 04-oop-basics
part: 1
tags: [initialization-order, virtual-override, method-hiding, abstract-class, sealed, static-constructor, extension-methods, partial-class, indexer, composition]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# OOP: classes and objects: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md)

## Object initialization order

On `new Derived()` the code runs in this order:

1. field initializers of the **derived** class (`int f = Init("derived field")`);
2. field initializers of the **base** class;
3. the body of the **base** class constructor;
4. the body of the **derived** class constructor.

Verified by running: the log showed `derived field | base field | base ctor | derived ctor`. "Derived before base" for field initializers is a C# peculiarity; in Java or C++ initialization goes top-down. A consequence: a base constructor can call a virtual method, and it runs in the derived class **before** the body of its constructor:

```csharp
class A { public A() { Hook(); } protected virtual void Hook() { } }

class C : A
{
    string name;                      // assigned in the constructor body
    public string Seen = "-";
    public C() { name = "set"; }
    protected override void Hook() { Seen = name is null ? "null" : name; }
}
// new C().Seen → "null"
```

A field that the derived class sets in the constructor **body** is still unassigned (`null`) at that point. Had it received its value in an initializer (`string name = "set";`), the method would have seen `set` — this was verified too. The takeaway: do not call virtual methods from constructors.

## virtual, override and new

| Keyword | What it does |
|---|---|
| `virtual` | lets derived classes replace the method |
| `override` | replaces a virtual method of the base class |
| `abstract` | a method without a body, derived classes must implement it |
| `sealed override` | replaces the method and forbids further overriding |
| `new` | **hides** a base class member but does not replace it |

The difference between `override` and `new` is what gets chosen when calling through a variable of the base type:

```csharp
class Animal
{
    public virtual string Speak() => "...";
    public string Name() => "Animal";
}

class Dog : Animal
{
    public override string Speak() => "Woof";
    public new string Name() => "Dog";
}

Animal a = new Dog();
Console.WriteLine(a.Speak());          // Woof   — override, decided by the actual type
Console.WriteLine(a.Name());           // Animal — new, decided by the variable type
Console.WriteLine(((Dog)a).Name());    // Dog
```

Hiding with `new` is almost always a design smell: the same name behaves differently depending on the variable's type. Without `new` the compiler issues warning CS0108.

### Overloading and derived-class methods

The overload resolution rule is not obvious: if the derived class has an **applicable** method, the compiler does not look at the base class methods:

```csharp
class PrinterBase { public void Print(int x) => Console.WriteLine("int in base"); }
class Printer : PrinterBase { public void Print(double x) => Console.WriteLine("double in derived"); }

new Printer().Print(5);     // double in derived
```

Although `Print(int)` fits better, the derived class's method ran: `5` converts implicitly to `double`, and methods declared in a more derived type take priority. It is better not to mix overloads across a hierarchy. Overload rules in general are in [03 · Control flow and methods](../03-control-flow-and-methods/advanced.en.md).

## Abstract classes and sealing

- `abstract class` provides the common part and mandatory members. It may contain fields, constructors and implemented methods — unlike an interface (see [05 · Interfaces, structs, records](../05-interfaces-structs-records/overview.en.md)).
- A `sealed class` cannot be inherited. Sealing is useful when a class is not designed for inheritance: its behavior is easier to maintain.
- A class is not sealed by default: if you do not want it inherited, mark it `sealed`.

## Properties: details

The **`field` keyword** (C# 14) lets you add logic to an auto-property without declaring your own field:

```csharp
public class Temp
{
    public double Celsius
    {
        get => field;
        set => field = Math.Max(value, -273.15);   // clamp to absolute zero
    }
}
// new Temp { Celsius = -300 }.Celsius → -273.15
```

The compiler still creates the hidden field `<Celsius>k__BackingField` (verified). If your class already has a member named `field`, refer to it as `@field` or `this.field`.

More forms:

| Form | When |
|---|---|
| `{ get; }` | the value is set only in a constructor or an initializer |
| `{ get; init; }` | the value is set when the object is created |
| `{ get; private set; }` | changed only from inside |
| `required` | the caller must set the value |
| `=> expr` | computed, no field |

An **indexer** lets you access an object like an array:

```csharp
public class Grid
{
    private readonly int[,] _cells = new int[3, 3];
    public int this[int row, int col]
    {
        get => _cells[row, col];
        set => _cells[row, col] = value;
    }
}
// var g = new Grid(); g[1, 2] = 7; g[1, 2] → 7
```

## static, const and readonly

| | When it is set | Can it change |
|---|---|---|
| `const` | at compile time (the value is substituted into code) | no; only simple types and strings |
| `readonly` | in an initializer or constructor | no, after the constructor |
| `static readonly` | in a static initializer or static constructor | no |

A `const` is implicitly static: in metadata the constant is marked `static` and `literal` (verified). Because a `const` value is substituted into code, an assembly that uses a constant from another assembly will not see a new value until it is recompiled. For values that may change, use `static readonly`.

A **static constructor** runs once — before the first access to the type. For a type with a static constructor the order is: static field initializers first, then the static constructor body (verified: `field init, static ctor`), and two accesses to the type did not run them again.

## Extension methods and partial

An **extension method** adds a method to an existing type without inheritance:

```csharp
public class Greeter { public string Hello() => "instance"; }

public static class GreeterExt
{
    public static string Hello(this Greeter g) => "extension";
}
// new Greeter().Hello() → "instance"
```

The result shows the rule: an **instance method always wins** over an extension method with the same signature. An extension cannot be used to "override" an existing method. The new extension blocks (`extension`) are covered in 11 · Modern C#.

A **`partial class`** lets you split a class definition across several files, and the compiler merges them. It is used for generated code (forms and code generators, for example): the generated part in one file, yours in another.

## Nested types

A type declared inside another type is nested. It is `private` by default and sees the private members of the outer type. It suits implementation details. A nested derived class in the check showed one more subtlety: derived code can access a `protected` member through a reference of **its own** type (`other.v` for `other` of type `D`), but not through a reference of the base type.

## Composition instead of inheritance

Inheritance tightly couples a derived class to the base class implementation: a change to the base class can break every derived class. Composition — storing an object of another type in a field and delegating calls to it — gives the same reuse without the tight coupling:

```csharp
public class Logger { public void Log(string m) => Console.WriteLine(m); }

public class OrderService(Logger logger)       // has a Logger, is not a Logger
{
    public void Place() => logger.Log("order placed");
}
```

A rule of thumb: inheritance is for "is a" and for cases where polymorphism is needed; otherwise composition and interfaces.

## Best practices

- Make classes `sealed` if inheritance is not planned.
- Do not call virtual methods from constructors.
- Make fields `private`, expose properties; for immutable data use `init` and `readonly`.
- Validate constructor arguments: an object must be valid right after `new`.
- When overriding `Equals`, override `GetHashCode` too (more in [05 · Interfaces, structs, records](../05-interfaces-structs-records/overview.en.md)).
- Avoid `new` hiding.

## Common mistakes

- ❌ A virtual call in a constructor → ✅ move the logic into an `Init` method called after creation.
- ❌ `new` instead of `override` by mistake → ✅ do not ignore warning CS0108.
- ❌ Mixing overloads in the base and derived class → ✅ rename the methods.
- ❌ `public const` for values that may change → ✅ `static readonly`.
- ❌ Deep inheritance hierarchies → ✅ composition and interfaces.
- ❌ Static mutable fields as "global variables" → ✅ pass dependencies through the constructor.

## Comparison with other tracks

| | C# | Python |
|---|---|---|
| Constructor | `public C()` / primary | `__init__` |
| Properties | `{ get; set; }` | `@property` |
| Access modifiers | `private`, `protected`, … (checked by the compiler) | convention: `_name`, `__name` |
| Overriding | explicit `virtual`/`override` | any method can be overridden |
| Multiple inheritance | no (yes for interfaces) | yes |
| Initialization order | derived fields → base → constructor | `__init__` calls `super().__init__()` manually |

## Related materials

- Before: [basics](basics.en.md)
- Next: [expert](expert.en.md) — how virtual calls work, property metadata, reference, interview questions
- Overloading and parameters: [03 · Control flow and methods](../03-control-flow-and-methods/advanced.en.md)
- Interfaces, structs, records: [05 · Interfaces, structs, records](../05-interfaces-structs-records/overview.en.md)
- The same in other tracks: [Python](../../../python/overview.en.md)

## Sources

- [Static constructors](https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/static-constructors)
- [Extension methods](https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/extension-methods)
- [Indexers](https://learn.microsoft.com/dotnet/csharp/programming-guide/indexers/)
- [Knowing when to use override and new keywords](https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/knowing-when-to-use-override-and-new-keywords)
- [Overload resolution](https://learn.microsoft.com/dotnet/csharp/language-reference/language-specification/expressions#12122-overload-resolution)
- [The field keyword](https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/field)
- [Partial classes](https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/partial-classes-and-methods)
