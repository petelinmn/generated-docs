---
title: "Modern C#: basics"
lang: en
track: csharp
kind: module
doc: basics
slug: 11-modern-csharp
part: 2
tags: [pattern-matching, switch-expression, primary-constructor, collection-expressions, required-members, init-only, raw-string-literals]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Modern C#: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [03 · Control flow and methods](../03-control-flow-and-methods/overview.en.md), [04 · OOP: classes and objects](../04-oop-basics/overview.en.md), [05 · Interfaces, structs, records](../05-interfaces-structs-records/overview.en.md). **Time:** ~1 h.

## Why it matters

C# evolves every year, and the new features remove boilerplate: type and value checks, constructors, collection creation. Code written in the C# 7 style still works, but in modern projects and documentation you will meet the new constructs. This page collects the ones you need from day one.

## Pattern matching

A pattern checks the shape of a value and, on a match, extracts data from it. The `is` operator accepts patterns:

```csharp
object o = 42;
if (o is int n && n > 0) Console.WriteLine($"positive int {n}");   // type pattern + variable
if (o is not string) Console.WriteLine("not a string");            // negation
string? s = null;
if (s is null) Console.WriteLine("null");                          // constant pattern
```

The main kinds of patterns:

| Pattern | Example | Checks |
|---|---|---|
| type | `o is int n` | the type, and declares a variable at once |
| constant | `x is 0`, `s is null` | equality to a constant |
| relational | `x is > 10`, `x is >= 0 and < 10` | a comparison |
| logical | `and`, `or`, `not` | combinations of patterns |
| property | `p is { X: 0, Y: 0 }` | property values |
| positional | `p is (0, 0)` | deconstruction (for `record`s and tuples) |
| `var`, `_` | `p is (var x, _)` | capture a value / ignore it |

## The switch expression

A `switch` expression returns a value instead of executing statements. Arms are tried top to bottom, and the first matching one wins:

```csharp
static string Classify(int n) => n switch
{
    < 0 => "negative",
    0 => "zero",
    >= 1 and < 10 => "digit",
    _ => "big",         // discard: matches everything else
};

Console.WriteLine(Classify(-5));   // negative
Console.WriteLine(Classify(7));    // digit
```

To switch on several values at once, use a tuple:

```csharp
static string Fizz(int n) => (n % 3, n % 5) switch
{
    (0, 0) => "FizzBuzz",
    (0, _) => "Fizz",
    (_, 0) => "Buzz",
    _ => n.ToString(),
};
Console.WriteLine(string.Join(" ", Enumerable.Range(1, 15).Select(Fizz)));
// 1 2 Fizz 4 Buzz Fizz 7 8 Fizz Buzz 11 Fizz 13 14 FizzBuzz
```

If no arm matches, a `SwitchExpressionException` is thrown (the compiler warns with `CS8509` if the arms do not cover all values). Verified: `x switch { 0 => 0, 2 => 1 }` throws `SwitchExpressionException` for `5`.

## Primary constructors

The parameters of a class or struct can be declared right in the type declaration (C# 12). They are available throughout the type body:

```csharp
class Account(string owner, decimal balance)
{
    public string Owner { get; } = owner;                       // copy the parameter into a property
    public void Deposit(decimal amount) => balance += amount;   // the parameter lives on as a hidden field
    public override string ToString() => $"{Owner}: {balance}";
}

var acc = new Account("Ann", 100);
acc.Deposit(50);
Console.WriteLine(acc);   // Ann: 150
```

A primary-constructor parameter is **neither a field nor a property**: if a method uses it, the compiler creates a hidden field. For a `record`, the parameters become properties immediately ([05 · Interfaces, structs, records](../05-interfaces-structs-records/overview.en.md)).

## Collection expressions

Square brackets create a collection of the needed type; `..` (a "spread") inserts the elements of another collection (C# 12):

```csharp
int[] a = [1, 2];
List<int> l = [..a, 3];                    // 1, 2, 3
ImmutableArray<int> im = [4, 5];           // using System.Collections.Immutable;
Span<int> sp = [6, 7];
int[] empty = [];
```

The type of the created collection is determined by the context (what the expression is assigned to). An empty `[]` for an array is `Array.Empty<int>()` (verified: the references are equal).

## required and init

An `init` accessor allows assigning a property only while the object is being created; `required` forces it to be set:

```csharp
var p = new Person { Name = "Bob", Age = 3 };
Console.WriteLine($"{p.Name} {p.Age}");   // Bob 3

class Person
{
    public required string Name { get; init; }   // must be set in the initializer
    public int Age { get; init; }
}
```

Verified: `p.Name = "x"` after creation is error `CS8852`, and `new Person()` without `Name` is error `CS9035`. This gives immutable objects without bulky constructors.

## Raw strings and lambda parameters

A raw string literal in `"""` needs no escaping of quotes and backslashes; `$$"""` interpolates with `{{ }}`, so single `{` are ordinary characters:

```csharp
var name = "Bob";
var json = $$"""
    {"name": "{{name}}", "ok": true}
    """;
Console.WriteLine(json);   // {"name": "Bob", "ok": true}
```

Lambda parameters can have default values (C# 12):

```csharp
var greet = (string s = "dflt") => s;
Console.WriteLine(greet() + " " + greet("x"));   // dflt x
```

## Beginner mistakes

- ❌ A chain `if (o is int) { var n = (int)o; … }` → ✅ `if (o is int n)`.
- ❌ A `_ =>` arm at the start of a `switch` → ✅ at the end: arms are checked in order, and unreachable arms are compile error `CS8510` (verified).
- ❌ Expecting a primary-constructor parameter to be a property → ✅ copy it into a property explicitly.
- ❌ `new List<int>() { 1, 2 }` everywhere → ✅ `List<int> l = [1, 2];`.
- ❌ Assigning an `init` property after creation → ✅ set it in the initializer, or use `with` for records.

## Related materials

- Next: [advanced theory](advanced.en.md) — list patterns, `field`, extension members.
- [05 · Interfaces, structs, records](../05-interfaces-structs-records/overview.en.md) — records and deconstruction.
- [06 · Collections and generics](../06-collections-and-generics/overview.en.md) — which types `[...]` creates.
- [03 · Control flow and methods](../03-control-flow-and-methods/overview.en.md) — the classic `switch`.
- [Python](../../../python/overview.en.md) — `match`/`case`: structural pattern matching.

## Sources

- [Patterns](https://learn.microsoft.com/dotnet/csharp/language-reference/operators/patterns)
- [The switch expression](https://learn.microsoft.com/dotnet/csharp/language-reference/operators/switch-expression)
- [Primary constructors](https://learn.microsoft.com/dotnet/csharp/whats-new/tutorials/primary-constructors)
- [Collection expressions](https://learn.microsoft.com/dotnet/csharp/language-reference/operators/collection-expressions)
- [The `required` member](https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/required)
