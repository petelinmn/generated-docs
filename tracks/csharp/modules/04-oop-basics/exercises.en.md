---
title: "Exercises: OOP: classes and objects"
lang: en
track: csharp
kind: module
doc: exercises
slug: 04-oop-basics
part: 1
tags: [class, encapsulation, polymorphism, abstract-class, virtual-override, equality]
status: draft
updated: 2026-11-22
---

# Exercises: OOP: classes and objects

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. Solutions are under spoilers. The code was verified by running it; there are no runnable versions in `code/` yet (level 5 does not exist).

## 1. 🟢 A bank account

**Task:** write a class `BankAccount(string owner, decimal initial)` with the properties `Owner`, `Balance` (read-only) and the methods `Deposit`, `Withdraw`. The amount must be positive (`ArgumentOutOfRangeException`), and withdrawing more than the balance is not allowed (`InvalidOperationException`). The balance must not be changeable from outside except through the methods.

**Expected result:**

```text
Deposit(50), Withdraw(30) with initial = 100 → Balance = 120
Withdraw(1000)                               → InvalidOperationException "Insufficient funds."
Deposit(-1)                                  → ArgumentOutOfRangeException (ParamName = "amount")
```

<details><summary>Hint</summary>

The field `_balance` is `private`, the property is `Balance => _balance`. For the checks there is `ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount)`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 1: encapsulation — the balance changes only through validated methods.
public class BankAccount
{
    private decimal _balance;
    public string Owner { get; }
    public decimal Balance => _balance;

    public BankAccount(string owner, decimal initial)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(owner);
        ArgumentOutOfRangeException.ThrowIfNegative(initial);
        Owner = owner;
        _balance = initial;
    }

    public void Deposit(decimal amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
        _balance += amount;
    }

    public void Withdraw(decimal amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
        if (amount > _balance) throw new InvalidOperationException("Insufficient funds.");
        _balance -= amount;
    }
}
```

Verified: the results match the table. The constructor does not let an invalid state through: the object is correct from the very start.

</details>

## 2. 🟢 Shapes

**Task:** write an abstract class `Shape` with the method `abstract double Area()` and a `ToString` override that prints the type name and the area with two decimals (invariant culture). Derived classes: `Circle(double r)` and `Rectangle(double w, double h)`. Compute the sum of the areas of an array of shapes.

**Expected result:**

```text
Circle(1) and Rectangle(2, 3) → sum 9.14
Circle area=3.14
Rectangle area=6.00
```

<details><summary>Hint</summary>

`Shape[] shapes = [new Circle(1), new Rectangle(2, 3)];` and `shapes.Sum(s => s.Area())`. For a culture-independent format — `FormattableString.Invariant($"…")`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 2: an abstract base class and polymorphic calls.
public abstract class Shape
{
    public abstract double Area();
    public override string ToString() => FormattableString.Invariant($"{GetType().Name} area={Area():F2}");
}

public class Circle(double r) : Shape { public override double Area() => Math.PI * r * r; }
public class Rectangle(double w, double h) : Shape { public override double Area() => w * h; }
```

Verified: the sum is `9.14`, the strings are as in the table. The summing code knows nothing about the concrete types — this is polymorphism.

</details>

## 3. 🟡 Initialization order

**Task:** write classes `Base` and `Derived : Base` with field initializers and constructors that write the messages `base field`, `base ctor`, `derived field`, `derived ctor` into a shared list `Trace.Log`. Predict the order, then check.

**Expected result:**

```text
derived field | base field | base ctor | derived ctor
```

<details><summary>Hint</summary>

A field initializer: `int f = Trace.Add("base field");`, where `Add` records the string and returns `0`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 3: the order of initialization in an inheritance chain.
static class Trace
{
    public static readonly List<string> Log = new();
    public static int Add(string m) { Log.Add(m); return 0; }
}

class Base
{
    int f = Trace.Add("base field");
    public Base() { Trace.Add("base ctor"); }
}

class Derived : Base
{
    int g = Trace.Add("derived field");
    public Derived() { Trace.Add("derived ctor"); }
}

new Derived();
Console.WriteLine(string.Join(" | ", Trace.Log));
```

Verified: `derived field | base field | base ctor | derived ctor`. The derived class's field initializers run before the base's — that is why calling virtual methods from a constructor is dangerous.

</details>

## 4. 🟡 override instead of new

**Task:** given code where `Dog.Kind()` is declared with `new`, so `Animal a = new Dog(); a.Kind()` returns `"Animal"`. Fix the classes so that a call through `Animal` gives `"Dog"`, and explain the difference.

**Expected result:**

```text
Before: a.Kind() → Animal, ((Dog)a).Kind() → Dog
After:  a.Kind() → Dog
```

<details><summary>Hint</summary>

The base class needs `virtual`, the derived class needs `override` instead of `new`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 4: replace member hiding with real overriding.
public class Animal { public virtual string Kind() => "Animal"; }
public class Dog : Animal { public override string Kind() => "Dog"; }

Animal b = new Dog();
Console.WriteLine(b.Kind());   // Dog
```

Verified: `Dog`. `new` hides the member and is chosen by the variable's type, while `virtual`/`override` is chosen by the object's actual type.

</details>

## 5. 🔴 Value equality

**Task:** write a sealed class `Money` with properties `required decimal Amount` and `required string Currency` (both `init`) that implements value equality: `IEquatable<Money>`, `Equals`, `GetHashCode`, the operators `==` and `!=`, and `ToString` (`5 EUR`). Objects must work as keys of a `HashSet<Money>`.

**Expected result:**

```text
two Money { 5, EUR } → == True, Equals True, same hash True, string "5 EUR"
HashSet<Money> { first }.Contains(second) → True
```

<details><summary>Hint</summary>

`GetHashCode` must use the same fields as `Equals`: `HashCode.Combine(Amount, Currency)`. The `==` operator checks `null` first.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 5: value equality for a class.
public sealed class Money : IEquatable<Money>
{
    public required decimal Amount { get; init; }
    public required string Currency { get; init; }

    public bool Equals(Money? o) => o is not null && Amount == o.Amount && Currency == o.Currency;
    public override bool Equals(object? o) => Equals(o as Money);
    public override int GetHashCode() => HashCode.Combine(Amount, Currency);
    public static bool operator ==(Money? a, Money? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(Money? a, Money? b) => !(a == b);
    public override string ToString() => $"{Amount} {Currency}";
}
```

Verified: all checks from the table gave `True`. A `record` gives the same in less code ([05 · Interfaces, structs, records](../05-interfaces-structs-records/overview.en.md)); the exercise shows what a record does for you.

</details>
