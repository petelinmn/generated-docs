---
title: "Exercises: Span and Memory"
lang: en
track: csharp
kind: module
doc: exercises
slug: 12a-span-and-memory
part: 2
tags: [span, readonly-span, stackalloc, ref-struct, enumerator, zero-allocation]
status: draft
updated: 2026-11-22
---

# Exercises: Span and Memory

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. Solutions are under spoilers. The code was verified by running it; there are no runnable versions in `code/` yet (level 5 does not exist).

## 1. 🟢 Word count

**Task:** write `static int CountWords(ReadOnlySpan<char> text)`: it counts words (runs of non-whitespace characters) without allocating memory.

**Expected result:**

```text
CountWords("  hello   big world ") → 3
CountWords("")                     → 0
CountWords("   ")                  → 0
```

<details><summary>Hint</summary>

Walk over the characters and remember whether you are inside a word. A new word starts when a non-space is met and the previous one was a space (or the start).

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 1: count words without allocating.
static int CountWords(ReadOnlySpan<char> text)
{
    int count = 0;
    bool inWord = false;
    foreach (char c in text)
    {
        bool isSpace = char.IsWhiteSpace(c);
        if (!isSpace && !inWord) count++;
        inWord = !isSpace;
    }
    return count;
}
```

Verified on the three examples. A string converts implicitly to `ReadOnlySpan<char>`, so the caller does not need to change anything.

</details>

## 2. 🟢 Sum of numbers from a CSV

**Task:** write `static int SumCsv(ReadOnlySpan<char> csv)`: numbers are separated by commas, there may be spaces around them, empty elements are skipped. Without `string.Split` and `Substring`.

**Expected result:**

```text
SumCsv("1, 2,3 ,,4") → 10
SumCsv("")           → 0
```

<details><summary>Hint</summary>

`csv.Split(',')` on a `ReadOnlySpan<char>` (.NET 9+) returns ranges: `csv[range]` is a slice. `Trim()` on a span also allocates nothing.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 2: parse a CSV line with spans only.
static int SumCsv(ReadOnlySpan<char> csv)
{
    int sum = 0;
    foreach (var range in csv.Split(','))
    {
        var part = csv[range].Trim();
        if (!part.IsEmpty) sum += int.Parse(part);
    }
    return sum;
}
```

Verified: `10` and `0`. `int.Parse` accepts a `ReadOnlySpan<char>`.

</details>

## 3. 🟡 Palindrome without copies

**Task:** write `static bool IsPalindrome(ReadOnlySpan<char> s)`: it ignores case and all characters except letters and digits. Do not create new strings or arrays.

**Expected result:**

```text
"A man, a plan, a canal: Panama" → True
"hello"                          → False
""                               → True
```

<details><summary>Hint</summary>

Two indices, `i` from the start and `j` from the end; at each step skip non-alphanumeric characters and compare `char.ToLowerInvariant`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 3: two-pointer palindrome check over a span.
static bool IsPalindrome(ReadOnlySpan<char> s)
{
    int i = 0, j = s.Length - 1;
    while (i < j)
    {
        while (i < j && !char.IsLetterOrDigit(s[i])) i++;
        while (i < j && !char.IsLetterOrDigit(s[j])) j--;
        if (char.ToLowerInvariant(s[i++]) != char.ToLowerInvariant(s[j--])) return false;
    }
    return true;
}
```

Verified on three examples. The solution is O(n) in time and O(1) in memory.

</details>

## 4. 🟡 A hexadecimal string

**Task:** write `static string ToHex(ReadOnlySpan<byte> bytes)`: a string of two lowercase hex digits per byte. For small inputs (up to 64 bytes) use `stackalloc`, otherwise an array.

**Expected result:**

```text
ToHex([0, 15, 255, 16]) → "000fff10"
```

<details><summary>Hint</summary>

`Span<char> buf = bytes.Length <= 64 ? stackalloc char[bytes.Length * 2] : new char[bytes.Length * 2];` and `bytes[i].TryFormat(buf.Slice(i * 2, 2), out _, "x2")`. In the end — `new string(buf)`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 4: hex string with a stack buffer for small inputs.
static string ToHex(ReadOnlySpan<byte> bytes)
{
    Span<char> buf = bytes.Length <= 64 ? stackalloc char[bytes.Length * 2] : new char[bytes.Length * 2];
    for (int i = 0; i < bytes.Length; i++)
        bytes[i].TryFormat(buf.Slice(i * 2, 2), out _, "x2");
    return new string(buf);
}
```

Verified: `000fff10`. Real code has the ready `Convert.ToHexStringLower` (.NET 9+); the exercise shows the "stack for small, heap for large" technique.

</details>

## 5. 🔴 Your own line enumerator

**Task:** write a `ref struct LineReader` that walks the lines of a `ReadOnlySpan<char>` through `foreach` without allocations: the separator is `\n`, a trailing `\r` is dropped. Then write `static bool TryParseKv(ReadOnlySpan<char> s, out ReadOnlySpan<char> key, out ReadOnlySpan<char> value)` for lines of the form `key = value`.

**Expected result:**

```text
foreach (var l in new LineReader("a\nbb\r\nccc")) → <a><bb><ccc>
TryParseKv("key = value", …)                      → True, key = "key", value = "value"
TryParseKv("novalue", …)                          → False
```

<details><summary>Hint</summary>

`foreach` works with any type that has `GetEnumerator()` with `MoveNext()` and `Current`. `LineReader` can be its own enumerator: `GetEnumerator()` returns a copy. The fields are `ReadOnlySpan<char>`, so the type is a `ref struct`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 5: a ref struct enumerator over lines and a span-returning parser.
ref struct LineReader
{
    private ReadOnlySpan<char> _rest;
    private ReadOnlySpan<char> _current;
    public LineReader(ReadOnlySpan<char> text) { _rest = text; _current = default; }
    public readonly ReadOnlySpan<char> Current => _current;
    public LineReader GetEnumerator() => this;
    public bool MoveNext()
    {
        if (_rest.IsEmpty) return false;
        int i = _rest.IndexOf('\n');
        ReadOnlySpan<char> line;
        if (i < 0) { line = _rest; _rest = default; }
        else { line = _rest[..i]; _rest = _rest[(i + 1)..]; }
        _current = line.TrimEnd('\r');
        return true;
    }
}

static bool TryParseKv(ReadOnlySpan<char> s, out ReadOnlySpan<char> key, out ReadOnlySpan<char> value)
{
    int i = s.IndexOf('=');
    if (i < 0) { key = value = default; return false; }
    key = s[..i].Trim(); value = s[(i + 1)..].Trim();
    return true;
}
```

Verified: `<a><bb><ccc>`, `True key|value`, `False`. There is a ready `EnumerateLines()`, but the exercise shows how an enumerator over a `ref struct` works: `Span` fields are allowed only in a `ref struct`.

</details>
