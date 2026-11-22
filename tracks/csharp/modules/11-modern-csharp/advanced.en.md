---
title: "Modern C#: advanced"
lang: en
track: csharp
kind: module
doc: advanced
slug: 11-modern-csharp
part: 2
tags: [pattern-matching, list-patterns, extension-members, field-keyword, collection-expressions, primary-constructor, required-members, null-conditional-assignment]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Modern C#: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [basics](basics.en.md). **Time:** ~1.5 h.

## Patterns in more detail

### Nested properties and guards

A property pattern can reach nested properties through a dot (C# 10):

```csharp
var o = new Order(new Customer(new Address("Berlin")), 120m);
Console.WriteLine(o is { Customer.Address.City: "Berlin", Total: > 100 });   // True

record Customer(Address Address);
record Address(string City);
record Order(Customer Customer, decimal Total);
```

If a `null` is met along the chain, the pattern simply does not match — no exception. The pattern `{ }` means "not `null`", so `x is { } v` is a compact check with capture: `o.Customer is { } c ? c.Address.City : "none"`. An extra condition on a `switch` arm is given with `when`:

```csharp
static string Classify(object o) => o switch
{
    null => "null",
    int n when n < 0 => "negative int",
    int and >= 0 and < 10 => "digit",
    int => "big int",
    string { Length: 0 } => "empty string",
    string s => $"string {s}",
    Point { X: 0, Y: 0 } => "origin",
    Point(var x, var y) when x == y => "diagonal",
    Point { X: > 0 } => "right half",
    _ => "other",
};

record Point(int X, int Y);
```

Verified on `null`, `-5`, `7`, `100`, `""`, `"hi"`, `Point(0,0)`, `Point(3,3)`, `Point(2,1)`: the answers are `null`, `negative int`, `digit`, `big int`, `empty string`, `string hi`, `origin`, `diagonal`, `right half`. The order of arms matters: put the more specific pattern above the general one. An arm that can never fire (everything is already covered above) is compile error `CS8510` (verified).

### List patterns

A list pattern (C# 11) matches arrays, `List<T>` and anything with `Length`/`Count` and an indexer. The slice `..` matches any number of elements and can be captured:

```csharp
int[] arr = [1, 2, 3, 4, 5];
Console.WriteLine(arr is [1, _, 3, ..]);           // True
Console.WriteLine(arr is [.., 4, 5]);              // True
if (arr is [var first, .. var mid, var last])
    Console.WriteLine($"{first} {string.Join(",", mid)} {last}");   // 1 2,3,4 5
Console.WriteLine(arr is [_, _]);                  // False: exactly two elements
```

A practical example is parsing command-line arguments: `["add", var item]` — exactly one argument, `["add", var first, .. var rest]` — more. Patterns work with strings too: `"abc" is ['a', .. var tail]`.

## Primary constructors: subtleties

A primary-constructor parameter used in a method is "captured" into a hidden field. If you both copy it into a property **and** use it in a method, the value is stored twice, and the compiler warns `CS9124`:

```csharp
class Svc(string name)
{
    public string Name { get; } = name;   // copy
    public string Show() => name;         // capture: warning CS9124 (stored twice)
}
```

Rules: use a parameter either only for initialization or only as captured state. Parameters cannot be changed from outside the type and do not appear in the API. For `struct` and `class`, additional constructors must call the primary one through `: this(...)`.

## Collection expressions: types and spread

| Target type | What is created |
|---|---|
| `T[]`, `List<T>`, `HashSet<T>` | an array / list / set |
| `Span<T>`, `ReadOnlySpan<T>` | usually on the stack or as assembly data |
| `ImmutableArray<T>` and others with `[CollectionBuilder]` | via a factory method |
| `IEnumerable<T>`, `IReadOnlyList<T>` | an immutable hidden collection |
| `IList<T>`, `ICollection<T>` | `List<T>` |

Verified: `IEnumerable<int> en = [..a1, ..l1]` creates a hidden type `<>z__ReadOnlyArray`, so the result cannot be cast to `List<T>`. Each `..source` expression is evaluated **once**: two spreads over a generator mean two calls (verified: `calls=2`).

A custom type supports `[...]` if it implements `IEnumerable` and is marked with `CollectionBuilder`:

```csharp
[CollectionBuilder(typeof(MyListBuilder), nameof(MyListBuilder.Create))]
class MyList<T> : IEnumerable<T> { /* ... */ }

static class MyListBuilder
{
    public static MyList<T> Create<T>(ReadOnlySpan<T> items) { /* copy items */ }
}

MyList<int> ml = [1, 2, 3];   // calls MyListBuilder.Create
```

### Collection expression arguments (C# 15)

A first element `with(...)` passes arguments to the collection's constructor or factory — for example, a list's capacity:

```csharp
List<int> xs = [with(capacity: 10), 1, 2, 3];
Console.WriteLine($"{xs.Count} cap={xs.Capacity}");   // 3 cap=10
```

## Properties: `field`, `required`, `init`

The `field` keyword (C# 14) refers to the property's automatic backing field so you can add logic without declaring your own field:

```csharp
class Temperature
{
    public double Celsius { get; set => field = Math.Max(value, -273.15); }   // validate in the setter
}

var t = new Temperature { Celsius = -500 };
Console.WriteLine(t.Celsius == -273.15);   // True
```

If your type already has a member named `field`, write `@field` or `this.field`. A constructor with the `[SetsRequiredMembers]` attribute promises that it sets all `required` members itself:

```csharp
class Req
{
    public required string Name { get; init; }

    [SetsRequiredMembers]   // using System.Diagnostics.CodeAnalysis;
    public Req(string n) { Name = n; }
}
```

## Null-conditional assignment

C# 14 allows `?.` on the left of an assignment: the right-hand side is evaluated only if the object is not `null`:

```csharp
Holder? h = null;
h?.Value = 5;                     // nothing happens, 5 is not even evaluated
h = new Holder();
h?.Value = 7;
Console.WriteLine(h.Value);       // 7

class Holder { public int Value { get; set; } }
```

## Extension members

The `extension` block (C# 14) lets you add to types not only methods but also **properties** and **static** members:

```csharp
static class Ext
{
    extension(string s)
    {
        public IEnumerable<string> Words => s.Split(',');                         // instance property
        public static bool IsNullOrEmpty2(string? v) => string.IsNullOrEmpty(v);  // "static" member of string
    }

    extension<T>(IEnumerable<T> src)
    {
        public bool IsEmpty => !src.Any();
    }
}

Console.WriteLine("a,b,c".Words.Count());        // 3
Console.WriteLine(string.IsNullOrEmpty2(""));    // True
Console.WriteLine(new[] { 1, 2, 3 }.IsEmpty);    // False
```

Old extension methods with `this` keep working. Extensions have no access to a type's private members and do not change it: they are syntactic sugar over static methods.

## Other features

| Feature | Since | Example |
|---|---|---|
| Aliases for any types | C# 12 | `using Coord = (double Lat, double Lon);` |
| `params` for collections and `Span` | C# 13 | `static int Sum(params ReadOnlySpan<int> xs)` |
| The `System.Threading.Lock` type | C# 13 / .NET 9 | `lock (new Lock()) { … }` ([18 · Concurrency](../18-concurrency/overview.en.md)) |
| Inline arrays | C# 12 | `[InlineArray(4)] struct Buf4 { int _e0; }` |
| `nameof` for unbound generics | C# 14 | `nameof(List<>)` → `List` |
| Labels on `break`/`continue` | C# 15 | `continue outer;` |

Verified by running: a tuple alias, `params ReadOnlySpan<int>` (`Sum(1, 2, 3)` = 6), `nameof(List<>)` = `List`, an inline array of four `int`s takes 16 bytes.

Labels let you leave nested loops without flags:

```csharp
outer:
for (int i = 0; i < 3; i++)
    for (int j = 0; j < 3; j++)
    {
        if (j == 2) continue outer;   // jump to the next i
        if (i == 2) break outer;      // leave both loops
        Console.WriteLine($"{i},{j}");
    }
// 0,0  0,1  1,0  1,1
```

## Best practices

- Use a `switch` expression when you need to return a value; a regular `switch` when you need side effects.
- Put `_` last and do not overuse it: without it the compiler helps find uncovered cases (`CS8509`).
- `required` + `init` instead of a constructor with ten parameters for DTOs and configurations.
- For `[..]` with a known size prefer a concrete target type (`int[]`, `List<T>`) over `IEnumerable<T>`.
- Do not put heavy logic or side effects in a `field` property.

## Related materials

- Back: [basics](basics.en.md). Next: [expert theory](expert.en.md) — union types, internals, gotchas.
- [06 · Collections and generics](../06-collections-and-generics/overview.en.md) — collection types.
- [05 · Interfaces, structs, records](../05-interfaces-structs-records/overview.en.md) — positional records, `with`.
- [07 · LINQ](../07-linq/overview.en.md) — extension methods and their `extension` alternative.
- [12a · Span and Memory](../12a-span-and-memory/overview.en.md) — `Span<T>`, `params ReadOnlySpan<T>`.

## Sources

- [What's new in C# 14](https://learn.microsoft.com/dotnet/csharp/whats-new/csharp-14)
- [What's new in C# 15](https://learn.microsoft.com/dotnet/csharp/whats-new/csharp-15)
- [List patterns](https://learn.microsoft.com/dotnet/csharp/language-reference/operators/patterns#list-patterns)
- [Extension members](https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/extension-methods)
