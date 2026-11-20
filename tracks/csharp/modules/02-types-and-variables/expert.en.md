---
title: "Types and variables: expert"
lang: en
track: csharp
kind: module
doc: expert
slug: 02-types-and-variables
part: 1
tags: [types, value-types, reference-types, type-conversion, strings, nullable-reference-types, boxing, floating-point]
versions: ".NET 11, C# 15"
status: verified
updated: 2026-11-20
---

# Types and variables: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md) · [exercises](exercises.en.md) · [5–6. Code and tests](code/)

## Where values live

The common simplification "value types live on the stack, reference types live on the heap" is inaccurate. A value lives **where its variable is declared**:

- a local `int` variable — on the stack or in a CPU register;
- an `int` field of a class — on the heap, inside the object of that class;
- the elements of an `int[]` — on the heap, inside the array;
- a local variable captured by a lambda is moved by the compiler into a field of a hidden class, i.e. onto the heap.

The correct model of the difference: a value type is **copied by value**, a reference type is **shared by reference**. The storage location is an implementation detail that the JIT may optimize.

## Struct size and alignment

The CPU prefers to read data at addresses that are multiples of its size. So the runtime aligns fields and inserts empty bytes (padding) between them:

```csharp
// The int must start at an offset divisible by 4, so 3 bytes of padding follow the byte.
[StructLayout(LayoutKind.Sequential)]
public struct Padded
{
    public byte A;
    public int B;
}

// Pack = 1 removes the padding (useful for binary formats, but unaligned access can be slower).
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Packed
{
    public byte A;
    public int B;
}
```

`Unsafe.SizeOf<Padded>()` = **8**, `Unsafe.SizeOf<Packed>()` = **5**. A variable of a reference type takes the size of a pointer: 8 bytes in a 64-bit process. Field order affects the size — that is [exercise 4](exercises.en.md). Code: [Ex08_StructLayout.cs](code/Examples/Ex08_StructLayout.cs).

## String interning

String literals are stored in the **intern pool**: identical literals in a program are the same object.

```csharp
// "hel" + "lo" is a compile-time constant, so both variables point to the same interned literal.
public static bool LiteralsShareInstance()
{
    string a = "hello";
    string b = "hel" + "lo";
    return ReferenceEquals(a, b);
}

// Concatenation at run time creates a new string object.
public static bool RuntimeConcatSharesInstance(string prefix)
{
    string a = "hello";
    string b = prefix + "lo";
    return ReferenceEquals(a, b);
}

// string.Intern returns the instance from the intern pool.
public static bool InternedConcatSharesInstance(string prefix)
{
    string a = "hello";
    string b = string.Intern(prefix + "lo");
    return ReferenceEquals(a, b);
}
```

`LiteralsShareInstance()` → `true` (the compiler folded the constants), `RuntimeConcatSharesInstance("hel")` → `false`, `InternedConcatSharesInstance("hel")` → `true`. Takeaways: always compare strings with `==` or `string.Equals` (by contents), not with `ReferenceEquals`. `string.Intern` is rarely used: the memory of interned strings is not likely to be released until the runtime terminates. Code: [Ex09_Interning.cs](code/Examples/Ex09_Interning.cs).

## `Nullable<T>` and nullable annotations from the inside

`int?` and `string?` look alike but work in completely different ways:

- `int?` is a real separate type, `Nullable<int>`: a struct with `HasValue` and `Value`.
- `string?` is the same `string` with an **annotation** for the compiler. There are no run-time checks.

```csharp
// Boxing an int? boxes the underlying int, or produces a null reference when there is no value.
public static Type? BoxedTypeOf(int? value)
{
    object? boxed = value;
    return boxed?.GetType();
}

public static int TakesNullable(string? text) => text?.Length ?? 0;

public static int TakesNonNullable(string text) => text.Length;

// string? and string are the same runtime type; the annotation is only metadata for the compiler.
public static Type ParameterType(string methodName) => FirstParameter(methodName).ParameterType;

// The annotation can still be read through reflection.
public static NullabilityState ParameterNullability(string methodName) =>
    new NullabilityInfoContext().Create(FirstParameter(methodName)).ReadState;
```

- `BoxedTypeOf(5)` → `System.Int32`, not `Nullable<int>`: boxing a `Nullable<T>` boxes the inner value. `BoxedTypeOf(null)` → `null`: an "empty" `int?` becomes a null reference.
- `ParameterType` returns `System.String` for both methods.
- `ParameterNullability` returns `Nullable` and `NotNull`: the annotation is stored in metadata as attributes and is read by the compiler and by libraries (e.g. ASP.NET Core during validation).

Consequence: NRT protects only at compile time. A library's public API still has to check its arguments (`ArgumentNullException.ThrowIfNull`), because the calling code may have been built without nullable analysis or may pass `null!`. Code: [Ex10_NullableInternals.cs](code/Examples/Ex10_NullableInternals.cs).

## Reference

### All numeric types

| Type | Size | Range or precision | When to use |
|---|---|---|---|
| `sbyte`, `byte` | 1 byte | −128…127, 0…255 | bytes, binary formats |
| `short`, `ushort` | 2 bytes | ±32 thousand, 0…65,535 | compact storage |
| `int`, `uint` | 4 bytes | ±2.1 billion, 0…4.29 billion | the default for integers |
| `long`, `ulong` | 8 bytes | ±9.2·10¹⁸ | time in ticks, large counters |
| `Int128`, `UInt128` | 16 bytes | `Int128.MaxValue` = 170141183460469231731687303715884105727 | very large integers without `BigInteger` |
| `nint`, `nuint` | 4 or 8 bytes | the size of a pointer: 8 bytes in a 64-bit process | interop, low-level code |
| `BigInteger` | variable | unlimited: `BigInteger.Pow(2, 100)` = 1267650600228229401496703205376 | cryptography, exact arithmetic |
| `Half` | 2 bytes | maximum 65504 | ML, graphics, storage |
| `float` | 4 bytes | ~6–9 significant digits | graphics, large arrays |
| `double` | 8 bytes | ~15–17 significant digits | computations by default |
| `decimal` | 16 bytes | 28–29 significant digits, maximum 79228162514264337593543950335 | money |

### Literals

| Notation | Type and value |
|---|---|
| `42`, `42u`, `42L`, `42UL` | `int`, `uint`, `long`, `ulong` |
| `0x1F`, `0b1010` | hexadecimal and binary literals: `0b1010 + 0x1F` = 41 |
| `1_000_000` | digit separator, does not affect the value |
| `1.5`, `1.5f`, `1.5m`, `1e3` | `double`, `float`, `decimal`, `double` (1000) |
| `'A'`, `'A'`, `'\n'` | `char` |
| `"…"`, `@"…"`, `"""…"""` | regular, verbatim and raw string |
| `$"…{x}…"`, `$$"""…{{x}}…"""` | interpolation; in a raw string with `$$` a single `{` stays text and `{{x}}` is a placeholder |
| `"abc"u8` | a UTF-8 literal of type `ReadOnlySpan<byte>`: `"abc"u8.Length` = 3, `"привет"u8.Length` = 12 |

An example of a raw string with `$$`: `$$"""{"value": {{value}}}"""` with `value = 42` gives `{"value": 42}`.

### Implicit numeric conversions

| From | Implicitly to |
|---|---|
| `sbyte` | `short`, `int`, `long`, `float`, `double`, `decimal`, `nint` |
| `byte` | `short`, `ushort`, `int`, `uint`, `long`, `ulong`, `float`, `double`, `decimal`, `nint`, `nuint` |
| `short` | `int`, `long`, `float`, `double`, `decimal`, `nint` |
| `ushort` | `int`, `uint`, `long`, `ulong`, `float`, `double`, `decimal`, `nint`, `nuint` |
| `char` | `ushort`, `int`, `uint`, `long`, `ulong`, `float`, `double`, `decimal` |
| `int` | `long`, `float`, `double`, `decimal`, `nint` |
| `uint` | `long`, `ulong`, `float`, `double`, `decimal`, `nuint` |
| `long` | `float`, `double`, `decimal` |
| `ulong` | `float`, `double`, `decimal` |
| `float` | `double` |

Implicit does not mean lossless: `int` → `float` and `long` → `double` may round (see the gotchas). Conversions between `decimal` and `float`/`double` are explicit only.

### Cast, Convert and Parse

| Call | Result |
|---|---|
| `(int)3.5` | `3` — the fractional part is dropped |
| `Convert.ToInt32(3.5)` / `Convert.ToInt32(2.5)` | `4` / `2` — banker's rounding |
| `Convert.ToInt32((string?)null)` | `0` |
| `int.Parse(null)` | `ArgumentNullException` |
| `int.Parse("٣")` (an Arabic-Indic digit) | `FormatException` |
| `int.TryParse(…, out var n)` | `false` instead of an exception |

### const, readonly, static readonly

| | `const` | `readonly` | `static readonly` |
|---|---|---|---|
| When the value is known | at compile time | in the constructor | at type initialization |
| Types | numbers, `bool`, `char`, `string`, `enum`, `null` | any | any |
| Where it is stored | substituted into the **calling** assembly's code | an object field | a static field |
| Changing the value in a library | requires recompiling consumers | safe | safe |

Substituting a `const` at the use site is the same trap as with optional parameter values (see [03 · Control flow and methods](../03-control-flow-and-methods/expert.en.md)).

### var, default, dynamic

- `var` requires an initializer to infer the type from: `var x = null;` is error CS0815. `var` cannot be used for fields.
- `default` is the type's default value: `0`, `false`, `null`. `default(int?)` is `null`.
- `dynamic` postpones type checking until run time. `dynamic d = 1; d.Foo();` compiles but throws `RuntimeBinderException`; `d + "x"` gives `"1x"`. `dynamic` is for COM, dynamic languages and untyped data, not a replacement for `var`.

### Tuples

`ValueTuple` is a mutable struct with named fields: `(int x, int y) = (3, 4);` deconstructs a tuple into variables (`x + y` = 7). Tuples are compared element by element with `==`: `(1, "a") == (1, "a")` is `true`. Element names exist only for the compiler; at run time they are `Item1`, `Item2`.

### Enums

- The default underlying type is `int`. `(DayOfWeek)42` is a legal value; it prints as `42`, and `Enum.IsDefined((DayOfWeek)42)` returns `false`.
- `[Flags]` is a set of bit flags: values 1, 2, 4…; `(Perm.Read | Perm.Write).ToString()` → `Read, Write`, `HasFlag(Perm.Write)` → `true`.

### Strings and Unicode

- `string` stores UTF-16 code units. Characters outside the basic plane take two units: `"😀".Length` = 2, while `"😀".EnumerateRunes().Count()` = 1.
- One letter can be encoded in different ways: `"é"` (U+00E9) and `"é"` (e + a combining accent). Ordinal `==` gives `false`, culture-aware comparison (`string.Compare(…, CultureInfo.InvariantCulture, CompareOptions.None)`) reports equality, and after `Normalize()` the strings match.

### Nullable analysis attributes

| Attribute | Meaning |
|---|---|
| `[NotNullWhen(true)]` | when the method returns `true`, the argument is not `null` (like `string.IsNullOrEmpty` with `false`) |
| `[MaybeNullWhen(false)]` | when the method returns `false`, the out value may be `null` (`TryGetValue`) |
| `[NotNull]` | after the call the argument is definitely not `null` (`ArgumentNullException.ThrowIfNull`) |
| `[MaybeNull]`, `[AllowNull]`, `[DisallowNull]` | refinements for generic types and properties |
| `[MemberNotNull(nameof(field))]` | the method guarantees that the field is initialized |
| `[NotNullIfNotNull("arg")]` | the result is not `null` if the argument is not `null` |

The `#nullable enable` / `disable` / `restore` directives turn analysis on for part of a file. `<WarningsAsErrors>nullable</WarningsAsErrors>` turns all nullable warnings into errors.

### Type testing and casting

| Expression | Result for `object o = "text"` |
|---|---|
| `o is string` | `True` |
| `o is string s` | `True`, and `s` already has type `string`: `s.Length` = 4 |
| `o as string` | `"text"`; for an unsuitable type (`o as Uri`) — `null` |
| `(Uri)o` | `InvalidCastException` |
| `o is null` / `o is not null` | a null check that an `==` overload cannot break |
| `typeof(string) == o.GetType()` | `True`: `typeof` is the type known at compile time, `GetType()` is the actual type of the object |

For a boxed value the check is exact: `boxed is int` is `True`, `boxed is long` is `False`. `null is object` is `False`. `sizeof(int)` = 4, `sizeof(char)` = 2. `nameof(localVar)` returns the name `"localVar"` as a string and is checked by the compiler.

### Types covered in other modules

| Kind of types | Where |
|---|---|
| classes, properties, inheritance | module 04 |
| structs, records, interfaces | module 05 |
| arrays, collections, generics | module 06 |
| delegates and events | module 09 |
| union types, closed hierarchies (C# 15) | module 11 |
| `Span<T>`, `ref struct`, pointers | module 12 |

## Gotchas

### Is float "more precise" than double?

```csharp
0.1f + 0.2f == 0.3f   // True
0.1 + 0.2 == 0.3      // False
```

`float` is not more precise: its sum just rounds to the same `float` as `0.3f`. Both types have representation errors, and neither should be compared with `==`.

### NaN and negative zero

```csharp
double.NaN == double.NaN        // False
double.NaN.Equals(double.NaN)   // True
-0.0 == 0.0                     // True
1 / -0.0                        // -Infinity
```

`==` follows IEEE 754 (NaN is equal to nothing), while `Equals` treats NaN as equal to itself, otherwise NaN could not be found in a dictionary. The check is `double.IsNaN(x)`.

### Convert rounds, a cast truncates

`(int)3.5` = 3, `Convert.ToInt32(3.5)` = 4, `Convert.ToInt32(2.5)` = 2: `Convert` uses banker's rounding.

### Convert.ToInt32(null) = 0

`Convert.ToInt32((string?)null)` returns `0`, while `int.Parse(null)` throws `ArgumentNullException`. Empty input silently becomes zero.

### Unicode digits

`char.IsDigit('٣')` (the Arabic-Indic three) is `true`, but `int.Parse("٣")` throws `FormatException`. To check for ASCII digits use `char.IsAsciiDigit`.

### Comparisons with null in Nullable

```csharp
int? n = null;
n + 1     // null
n > 0     // False
n <= 0    // False
```

Arithmetic with `null` gives `null`, and any `<`, `>`, `<=`, `>=` comparison with `null` gives `false`. So `!(n > 0)` does not mean `n <= 0`.

### Unsigned subtraction and byte overflow

`5u - 6u` = 4294967295, and `byte b = 255; b++;` gives `0`. Without `checked`, overflow wraps around silently for unsigned types too.

### Dividing negative numbers

`-7 / 2` = `-3`, `-7 % 2` = `-1`: integer division rounds toward zero, and the sign of the remainder matches the dividend. To check for odd numbers use `n % 2 != 0`, not `n % 2 == 1`.

### decimal is not infinitely precise either

`1m / 3m * 3m` = `0.9999999999999999999999999999`. `decimal` is exact for decimal fractions like 0.1, but not for 1/3.

### Math.Round(2.675, 2) = 2.67

As a `double`, 2.675 is stored slightly below the real 2.675, so it rounds down. Round money in `decimal`: `Math.Round(2.675m, 2)`.

### Implicit conversions lose precision

`(int)(float)16777217` = `16777216`, and `(double)9007199254740993L == 9007199254740992.0` is `true`. `float` has a 24-bit mantissa, `double` has 53 bits, and large integers do not fit.

### null in concatenation

```csharp
string? s = null;
s += "a";   // "a"
```

Concatenation treats `null` as an empty string and does not throw.

## Trade-offs: struct or class

| | struct | class |
|---|---|---|
| Assignment | copies the value | copies the reference |
| Heap allocation | no (unless boxed or a field of a class) | always |
| Can be `null` | only as `Nullable<T>` | yes |
| Inheritance | no | yes |
| When to choose | small immutable values: coordinates, money, identifiers | entities with identity, large objects, hierarchies |

More on structs and records in module 05, on memory and GC in module 12. C# 15 adds one more kind of type — union types: a value of such a type is one of several case types. They are covered in module 11.

## Interview questions

**1. How do value types differ from reference types? Is it true that value types always live on the stack?**

<details><summary>Answer</summary>

A value type is copied by value, a reference type is shared by reference. No: a value lives where its variable is declared — a struct field inside a class is on the heap, a local captured by a lambda is in a field of a hidden class.

</details>

**2. What is boxing, and why is it bad?**

<details><summary>Answer</summary>

Converting a value type to `object` or an interface copies the value into a new heap object. That is an allocation and GC pressure. Two boxes of the same value are different objects, so `==` on them gives `false`. Generics avoid boxing.

</details>

**3. Why is `0.1 + 0.2 != 0.3` for `double`? When should you use `decimal`?**

<details><summary>Answer</summary>

0.1 cannot be represented exactly in binary. `decimal` stores decimal digits and is exact for money and decimal fractions. `double` is faster and has a huge range; it suits measurements and computations.

</details>

**4. What do `checked` and `unchecked` do? What is the default behavior?**

<details><summary>Answer</summary>

By default integer overflow is not checked and wraps around silently. `checked` turns overflow into `OverflowException`. Checking can be enabled for the whole project with the `CheckForOverflowUnderflow` property.

</details>

**5. How does `int?` differ from `string?` at the runtime level?**

<details><summary>Answer</summary>

`int?` is a separate type, `Nullable<int>` (a struct with `HasValue`/`Value`). `string?` is the same `string` with an annotation for the compiler; there are no run-time checks.

</details>

**6. What does `ReferenceEquals("ab", "a" + "b")` return, and why?**

<details><summary>Answer</summary>

`True`: `"a" + "b"` is a constant expression, the compiler folds it into a literal, and literals are interned. If at least one part is known only at run time, you get a new object and `False`.

</details>

**7. Why is `ToUpper()` without arguments dangerous for comparing strings?**

<details><summary>Answer</summary>

It depends on the current culture: in Turkish, `"i".ToUpper()` is `"İ"`. Identifiers use `ToUpperInvariant()` or `string.Equals(a, b, StringComparison.OrdinalIgnoreCase)`.

</details>

**8. How does `const` differ from `static readonly`?**

<details><summary>Answer</summary>

A `const` is known at compile time and substituted into the calling assembly's code — changing the value in a library does not reach consumers without recompiling them. A `static readonly` is computed at run time, can be of any type and is read from a field.

</details>

**9. How does `dynamic` differ from `object` and `var`?**

<details><summary>Answer</summary>

`var` is a static type inferred by the compiler. `object` is a static type; calling members needs a cast. `dynamic` postpones member resolution until run time: compile errors turn into `RuntimeBinderException`.

</details>

**10. 🪤 `int? n = null;` What are `n > 0` and `n <= 0`?**

<details><summary>Answer</summary>

Both are `false`. Lifted comparison operators with `null` always give `false`, so negating one comparison is not equal to the other.

</details>

**11. 🪤 What is `"😀".Length`?**

<details><summary>Answer</summary>

2. `string` stores UTF-16 code units, and this character takes a surrogate pair. Unicode characters are counted with `EnumerateRunes()` (1 here), graphemes with `StringInfo`.

</details>

**12. 🪤 How does `Convert.ToInt32(x)` differ from `(int)x` for a `double`?**

<details><summary>Answer</summary>

A cast drops the fractional part, `Convert` rounds with banker's rounding: `(int)3.5` = 3, `Convert.ToInt32(3.5)` = 4, `Convert.ToInt32(2.5)` = 2. Also, `Convert.ToInt32((string?)null)` returns 0 instead of throwing.

</details>

**13. 🪤 Why is `double.NaN == double.NaN` `false`, while `double.NaN.Equals(double.NaN)` is `true`?**

<details><summary>Answer</summary>

`==` implements IEEE 754, where NaN is equal to nothing. `Equals` must be reflexive (`x.Equals(x)`), otherwise NaN could not be used as a dictionary key or found in a collection.

</details>

**14. What is the `[NotNullWhen(true)]` attribute for?**

<details><summary>Answer</summary>

It tells the nullable analyzer that when the method returns `true`, the argument is not `null`. Thanks to such attributes, after `if (dict.TryGetValue(key, out var v))` or `if (!string.IsNullOrEmpty(s))` the compiler produces no warnings.

</details>

## Cheat sheet

| Task | How |
|---|---|
| Check for null | `x is null`, `x is not null` |
| Default value for null | `x ?? fallback`, `x ??= fallback` |
| Safe call | `x?.Member` |
| Parse a number from a string | `int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n)` |
| Enable overflow checking | `checked(a + b)` |
| "School" rounding | `Math.Round(x, MidpointRounding.AwayFromZero)` |
| Case-insensitive string comparison | `string.Equals(a, b, StringComparison.OrdinalIgnoreCase)` |
| Money | `decimal`, literal `10.5m` |
| Compare `double` values | `Math.Abs(a - b) <= tolerance` |
| Multi-line text without escaping | `"""…"""` |

## Related materials

- Prerequisites: [01 · The .NET platform and dotnet CLI](../01-dotnet-platform/overview.en.md) — the project, `<Nullable>enable</Nullable>`, building
- Next: [03 · Control flow and methods](../03-control-flow-and-methods/overview.en.md) — `ref`, `out`, `in` and passing value types to methods
- The same idea in other tracks: [Python](../../../python/overview.en.md) — dynamic typing and type annotations instead of static types

## Sources

- [C# built-in types](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/built-in-types)
- [Value types](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/value-types)
- [Reference types](https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/reference-types)
- [Nullable reference types](https://learn.microsoft.com/dotnet/csharp/nullable-references)
- [Boxing and unboxing](https://learn.microsoft.com/dotnet/csharp/programming-guide/types/boxing-and-unboxing)
- [Floating-point numeric types](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/floating-point-numeric-types)
- [Best practices for comparing strings](https://learn.microsoft.com/dotnet/standard/base-types/best-practices-strings)
- [Raw string literals](https://learn.microsoft.com/dotnet/csharp/language-reference/tokens/raw-string)
- [`string.Intern`](https://learn.microsoft.com/dotnet/api/system.string.intern)
- [Integral numeric types](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/integral-numeric-types)
- [Built-in numeric conversions](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/numeric-conversions)
- [`Convert.ToInt32`](https://learn.microsoft.com/dotnet/api/system.convert.toint32)
- [The dynamic type](https://learn.microsoft.com/dotnet/csharp/advanced-topics/interop/using-type-dynamic)
- [Tuples](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/value-tuples)
- [Enums](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/enum)
- [Character encoding in .NET](https://learn.microsoft.com/dotnet/standard/base-types/character-encoding-introduction)
- [Nullable analysis attributes](https://learn.microsoft.com/dotnet/csharp/language-reference/attributes/nullable-analysis)
