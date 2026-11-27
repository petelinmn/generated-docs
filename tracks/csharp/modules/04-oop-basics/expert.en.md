---
title: "OOP: classes and objects: expert"
lang: en
track: csharp
kind: module
doc: expert
slug: 04-oop-basics
part: 1
tags: [object-layout, virtual-dispatch, callvirt, backing-field, init-accessor, required-members, primary-constructor, access-modifiers, object-methods, interview-questions]
versions: ".NET 11, C# 15"
status: verified
updated: 2026-11-27
---

# OOP: classes and objects: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## Under the hood

### An object in memory

A class instance is an object on the managed heap ([12 · Memory and GC](../12-memory-and-gc/basics.en.md)). Before the fields comes a service part: an object header and a pointer to the type description (in CoreCLR it is called a `MethodTable`). Through that pointer the runtime learns the object's actual type and finds the implementations of virtual methods. That is why the smallest object takes 24 bytes on a 64-bit platform (verified: `new object()` allocated 24 bytes), and why `GetType()` and type casts work through this pointer.

### How the compiler calls methods

To call a method of a class instance through a reference, C# usually emits the IL instruction `callvirt`, **even if the method is not virtual**: this performs the `null` check of the receiver. When the compiler knows the receiver is not `null`, it emits `call` for a non-virtual method: `this.M()` and `new T().M()`. `base.M()` also uses `call`, and so do static methods. Verified by reading the IL: `t.NonVirtual()` and `t.Virt()` through a parameter compile to `callvirt`; `Target.St()`, `new Target().Plain()` and a call of an own method without a receiver compile to `call`.

For a virtual method, when it executes `callvirt` the runtime looks into the method table of the actual type and calls the most derived implementation. All polymorphic behavior follows from this: `override` replaces an entry in the table, while `new` does not.

### What the compiler does with `override` and `new`

In metadata the method `Dog.Speak` is marked virtual and **not** `newslot` — that is, it occupies the slot of the base method. The method `Dog.Name` with `new` is non-virtual, a separate method with the same name. Verified through reflection: `Speak: virtual=True, newslot=False`, `Name: virtual=False`. A call to `Name()` through an `Animal` variable compiles to a call of `Animal.Name`, which is why you get "Animal".

### What properties and new features hide

- An auto-property turns into **a pair of methods `get_X`/`set_X` and a private field** `<X>k__BackingField` (verified, including for a property using the `field` keyword).
- An `init` setter is an ordinary method with a special **return type modifier** `IsExternalInit`. This modifier is what forbids calling the setter outside object initialization; code that bypasses the compiler, such as reflection, gets around it (verified: through `PropertyInfo.SetValue` the value of an `init` property changed from 30 to 31).
- `required` marks the property with `RequiredMemberAttribute`, and the type's constructors with `CompilerFeatureRequiredAttribute` and `ObsoleteAttribute`: older compilers that do not know about `required` refuse to call such constructors. This was verified through reflection.
- A **primary constructor** parameter becomes the hidden field `<start>P` only if it is used in members (verified: the only members of the class are the constructor, the `Next` method and the hidden field).

## Reference

### Type members

| Member | What it is | Notes |
|---|---|---|
| Field | a variable in a type | `readonly`, `const`, `static`, `volatile` |
| Method | a named block of code | `virtual`, `abstract`, `override`, `sealed`, `static`, `async`, `extern`, `partial` |
| Property | a pair of accessors | `get`, `set`, `init`; `required`; `field` |
| Indexer | a property with parameters | named `this[...]` |
| Event | a subscription to notifications | see [09 · Delegates, events, lambdas](../09-delegates-events-lambdas/overview.en.md) |
| Constructor | instance initialization | primary, chaining `this(...)`/`base(...)` |
| Static constructor | type initialization | once, no parameters or access modifiers |
| Finalizer | `~T()` | see [12 · Memory and GC](../12-memory-and-gc/advanced.en.md#finalizers) |
| Operator | overloading `+`, `==`, … | `public static` |
| Nested type | a type inside a type | `private` by default |

### Access modifiers

| | Same type | Derived, same assembly | Derived, other assembly | Assembly | Other assembly |
|---|---|---|---|---|---|
| `private` | ✅ | ❌ | ❌ | ❌ | ❌ |
| `private protected` | ✅ | ✅ | ❌ | ❌ | ❌ |
| `protected` | ✅ | ✅ | ✅ | ❌ | ❌ |
| `internal` | ✅ | ✅ | ❌ | ✅ | ❌ |
| `protected internal` | ✅ | ✅ | ✅ | ✅ | ❌ |
| `public` | ✅ | ✅ | ✅ | ✅ | ✅ |

There is also `file` — a type visible only in one source file (for generated code). Defaults: type members are `private`, top-level types are `internal`, interface members are `public`. Reflection verified that `protected internal` means "family **or** assembly" and `private protected` means "family **and** assembly".

### Class and member modifiers

| Modifier | Applies to | Meaning |
|---|---|---|
| `abstract` | class, member | cannot be instantiated / no implementation |
| `sealed` | class, override | forbids inheritance / overriding |
| `static` | class, member | belongs to the type; a static class is `abstract sealed` in metadata |
| `partial` | type, method, property and indexer (C# 13), constructor and event (C# 14) | the definition is in several parts |
| `virtual` / `override` / `new` | member | replaceable / replacing / hiding |
| `readonly` | field | changed only in the constructor |
| `required` | property, field | a value is mandatory on initialization |
| `unsafe`, `extern`, `volatile` | rare | unsafe code, external implementation, memory barriers |

### System.Object methods

| Method | Default | When to override |
|---|---|---|
| `ToString()` | the type name (`Plain`) | for readable output and debugging |
| `Equals(object)` | reference comparison | for value equality |
| `GetHashCode()` | based on object identity (different for two distinct objects in the check) | always together with `Equals` |
| `GetType()` | the exact runtime type | not overridable |
| `MemberwiseClone()` | a shallow copy (protected) | for cloning |
| `Finalize()` | empty | through the `~T()` syntax |
| `Equals(a, b)`, `ReferenceEquals(a, b)` (static) | the first handles `null` and calls `a.Equals(b)`; the second compares references | not overridable |

## Other class features

```csharp
Animal animal = new Cat();
Console.WriteLine(animal.Create().GetType().Name);   // Cat
Console.WriteLine(new Cat().Create().Name);          // cat — the static type is already Cat

var (x, y) = new Point(1, 2);                        // calls Deconstruct
double degrees = new Celsius(100);                   // implicit conversion
var back = (Celsius)36.6;                            // explicit conversion

var tag = new Tag("a");                              // no initializer needed
var id = Id.Create(5);                               // new Id(5) does not compile
Console.WriteLine(new Loud().Say());                 // HI!

class Animal { public virtual Animal Create() => new Animal(); }
class Cat : Animal
{
    public string Name => "cat";
    public override Cat Create() => new Cat();       // covariant return (C# 9)
}

class Point(int x, int y)
{
    public void Deconstruct(out int px, out int py) { px = x; py = y; }
}

class Celsius(double degrees)
{
    public double Degrees => degrees;
    public static implicit operator double(Celsius c) => c.Degrees;
    public static explicit operator Celsius(double d) => new(d);
}

class Tag
{
    public required string Name { get; init; }
    public Tag() { }
    [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
    public Tag(string name) => Name = name;
}

class Id
{
    public int Value { get; }
    private Id(int value) => Value = value;          // only the class itself can create
    public static Id Create(int value) => new(value);
}

class Quiet { public virtual string Say() => "hi"; }
class Loud : Quiet { public override string Say() => base.Say().ToUpper() + "!"; }
```

| Feature | Rule |
|---|---|
| Covariant return | an `override` method or read-only property may return a more derived type |
| `base.M()` | calls the base implementation, bypassing the override |
| `Deconstruct` | `out` parameters enable `var (x, y) = obj` |
| `implicit` / `explicit operator` | user-defined conversions; make it `explicit` if information can be lost |
| `SetsRequiredMembers` | the constructor promises to set all `required` members; the compiler does not check that promise |
| Private constructor | creation only through factory methods |
| Primary constructor and other constructors | every other explicit constructor must chain with `this(...)` |

## Gotchas

Every gotcha was verified by running it.

### A virtual method in a constructor

The result is subtler than it looks: the derived class's field initializers have **already** run by the time of the base constructor, while assignments in the derived constructor body have not.

```csharp
class A { public A() { Hook(); } protected virtual void Hook() { } }
class C : A
{
    string name;
    public string Seen = "-";
    public C() { name = "set"; }
    protected override void Hook() { Seen = name is null ? "null" : name; }
}
// new C().Seen → "null"
```

If you replace `string name;` with `string name = "set";`, the result becomes `set`. The derived method runs on a half-built object.

### new hides, it does not replace

`Animal a = new Dog();` — `a.Name()` returns `Animal`, `((Dog)a).Name()` returns `Dog`. One object, two answers, depending on the variable's type.

### A derived method shadows base overloads

`new Printer().Print(5)` called `Print(double)` from the derived class, although the base class has `Print(int)`. An applicable derived method excludes base methods from consideration.

### An instance method beats an extension method

`new Greeter().Hello()` returned `instance`, although `Hello` is also defined as an extension. An extension is called only when there is no applicable instance method.

### Accessing protected through a base reference

In a class `D : B`, the code `other.v`, where `other` has type `B`, does not compile: error CS1540. Through a reference of **its own** type (`D other`) it compiles. A protected member is accessible to a derived class only through instances of the derived class.

### A cast to the wrong type

`(int)(object)"text"` throws `InvalidCastException`, while `o as int?` for a string returns `null` without an exception.

### Static initializer order

In a type with a static constructor, static field initializers run **before** the static constructor body (log: `field init, static ctor`), even if the constructor is written earlier in the text. Repeated accesses to the type do not run them again.

### const and references between assemblies

The value of a `const` is substituted into the calling assembly's code at compile time, so after a constant in a library changes, consumers keep the old value until they are rebuilt. In metadata a constant is `static` and `literal` (verified).

### required does not protect against reflection

`required` is checked by the compiler. When an object is created with `Activator.CreateInstance`, the requirement is not enforced (verified: `Name` stayed `null`), and an `init` property can be changed by reflection (verified: 30 → 31).

## Trade-offs and alternatives

| Task | Option | Price |
|---|---|---|
| Code reuse | inheritance | tight coupling to the base, the fragile base class problem |
| | composition | slightly more delegation code |
| A shared contract | interface | no state and no constructors |
| | abstract class | one base class, but shared state |
| Immutable data | `init` and `readonly` | copying on change; for values there is `record` ([05](../05-interfaces-structs-records/overview.en.md)) |
| Extending someone else's type | an extension method | not virtual, no access to private members |
| Forbidding inheritance | `sealed` | derived code will have to use composition |

## Interview questions

**1. How does a class differ from an object?**

<details><summary>Answer</summary>

A class is a type description: fields, methods, properties. An object (instance) is a concrete area of heap memory created from that description with `new`. A class can have many objects, each with its own state; static members belong to the class and are shared by all objects.

</details>

**2. Name the three principles of OOP and give an example of each in C#.**

<details><summary>Answer</summary>

Encapsulation — a private field `_balance` and a `Deposit` method that validates its argument. Inheritance — `Dog : Animal`. Polymorphism — a variable of type `Animal` calls `Speak()`, and the `Dog` implementation runs. Sometimes a fourth one is added — abstraction (the abstract class `Shape`, which defines a contract while hiding details).

</details>

**3. How do `override` and `new` differ?**

<details><summary>Answer</summary>

`override` replaces a virtual method: a call through a base variable runs the derived implementation. `new` hides a non-virtual (or virtual) member: a call through a variable of the base type runs the base implementation. Verified: `a.Speak()` → `Woof`, `a.Name()` → `Animal`.

</details>

**4. In what order do initializers and constructors run on `new Derived()`?**

<details><summary>Answer</summary>

Derived field initializers → base field initializers → base constructor → derived constructor. This differs from Java and C++, where the derived initializers run after the base constructor.

</details>

**5. What happens if you call a virtual method from a base class constructor?**

<details><summary>Answer</summary>

The overridden version in the derived class runs, although the derived constructor body has not run yet: fields assigned in the body will be `null` or `0` (verified: `Seen == "null"`). Field initializers have already run. That is why virtual calls in constructors are a source of hard-to-find bugs.

</details>

**6. Which access modifiers does C# have? How does `protected internal` differ from `private protected`?**

<details><summary>Answer</summary>

`public`, `private`, `protected`, `internal`, `protected internal`, `private protected` (and `file`). `protected internal` is "in the assembly **or** in derived classes", `private protected` is "in derived classes **within its own assembly**" (the intersection of the conditions). By default members are `private` and top-level types are `internal`.

</details>

**7. What is an auto-property and what does it turn into?**

<details><summary>Answer</summary>

A property without accessor bodies: `public int X { get; set; }`. The compiler creates a private field `<X>k__BackingField` and methods `get_X`, `set_X`. The advantage over a public field is that logic can be added later without changing the binary contract or the calling code; properties also work with data binding and interfaces.

</details>

**8. How does `init` differ from `private set` and from a `readonly` field?**

<details><summary>Answer</summary>

`init` lets the value be set at creation (in a constructor or an object initializer), including from outside, but it is immutable afterwards. `private set` can be changed from any method inside the class, but not from outside. A `readonly` field can be changed only in a constructor or initializer. Technically `init` is guarded by the `IsExternalInit` modifier, which the compiler checks: reflection bypasses it (verified).

</details>

**9. What does `required` do?**

<details><summary>Answer</summary>

It forces the caller to set the property or field at creation through an object initializer or a constructor with the `SetsRequiredMembers` attribute; otherwise it is a compile error. It is a compiler check: the metadata gets `RequiredMemberAttribute`, and the constructors get guard attributes for older compilers.

</details>

**10. What is a primary constructor of a class? Does it create properties?**

<details><summary>Answer</summary>

Parameters in the class header (`class Counter(int start)`), available throughout the class. Unlike a `record`, for an ordinary class **no properties are created**: the parameter is just a captured variable; if it is used in members, the compiler creates a hidden field. Explicit properties must be declared yourself.

</details>

**11. How does an abstract class differ from an interface?**

<details><summary>Answer</summary>

An abstract class can contain fields, constructors and implemented members with any access, but a class can inherit from only one. An interface defines a contract (since C# 8 also default implementations), and a class implements any number of interfaces. More in [05 · Interfaces, structs, records](../05-interfaces-structs-records/overview.en.md).

</details>

**12. What is `sealed` for?**

<details><summary>Answer</summary>

It forbids inheriting from a class or overriding a method further. It is used when a class is not designed for inheritance: its behavior is easier to guarantee. By default C# classes are **not** sealed.

</details>

**13. Trick question: an instance method and an extension method with the same signature. Which is called?**

<details><summary>Answer</summary>

The instance method. An extension is called only if there is no applicable instance method (verified: `instance`). So an extension cannot "replace" someone else's method, and adding a method to a class can silently change what third-party code calls.

</details>

**14. Trick question: the base has `Print(int)`, the derived class has `Print(double)`. Which does `derived.Print(5)` call?**

<details><summary>Answer</summary>

`Print(double)` from the derived class. The overload rule: if the derived class has an applicable method, base class methods are not considered (verified).

</details>

**15. When does a static constructor run?**

<details><summary>Answer</summary>

Once per type, before the first instance is created or the first static member is accessed. The order: static field initializers first, then the constructor body (verified). No parameters, no access modifier.

</details>

**16. How does `const` differ from `static readonly`?**

<details><summary>Answer</summary>

`const` is a compile-time constant, its value is substituted into code (in metadata the field is `static literal`); it is possible only for numbers, `bool`, `char`, enums, strings, and `null` for reference types. `static readonly` is computed at run time, can be of any type, and its value is not "baked into" other assemblies. For values that may change, use `static readonly`.

</details>

**17. Why does the compiler use `callvirt` for non-virtual methods?**

<details><summary>Answer</summary>

To get a `null` check: calling a method on a `null` reference must throw `NullReferenceException`. It is not used for every call: when the receiver cannot be `null` (`this.M()`, `new T().M()`) a non-virtual method is called with `call`, as are `base.M()` and static methods (verified by reading the IL).

</details>

**18. What does `partial` do and why?**

<details><summary>Answer</summary>

It lets you describe a class (or a method) in several parts and files; the compiler merges them. It is needed for generated code: a tool generates one part, a developer writes the other; changes to the second are not overwritten when the first is regenerated.

</details>

## Cheat sheet

| Need | Solution |
|---|---|
| Hide state | a `private` field + a property |
| Immutable after creation | `{ get; init; }`, `readonly` |
| A mandatory value | `required` |
| Replace behavior | `virtual` + `override` |
| Forbid inheritance | `sealed` |
| A common template | `abstract class` |
| Constructor chaining | `: this(...)`, `: base(...)` |
| Logic in an auto-property | `field` (C# 14) |
| Add a method to someone else's type | an extension method (an instance method wins) |
| A constant that may change | `static readonly` |

## Related materials

- Before: [basics](basics.en.md), [advanced](advanced.en.md)
- Memory allocation for objects, `Dispose`: [12 · Memory and GC](../12-memory-and-gc/overview.en.md)
- Reflection and attributes: [20 · Reflection and source generators](../20-reflection-and-source-generators/overview.en.md)
- Interfaces, structs, records: [05 · Interfaces, structs, records](../05-interfaces-structs-records/overview.en.md)

## Sources

- [Classes and objects](https://learn.microsoft.com/dotnet/csharp/fundamentals/types/classes)
- [Constructors](https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/constructors)
- [Access modifiers](https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/access-modifiers)
- [init](https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/init)
- [required](https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/required)
- [Primary constructors](https://learn.microsoft.com/dotnet/csharp/whats-new/tutorials/primary-constructors)
- [The Object class](https://learn.microsoft.com/dotnet/api/system.object)
- [Order of initialization](https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/using-constructors)
