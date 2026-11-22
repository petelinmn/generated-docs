---
title: "Span and Memory: advanced"
lang: en
track: csharp
kind: module
doc: advanced
slug: 12a-span-and-memory
part: 2
tags: [span, memory, array-pool, collections-marshal, memory-marshal, pipelines, search-values, utf8]
versions: ".NET 11, C# 15; System.IO.Pipelines 11.0.0"
status: draft
updated: 2026-11-22
---

# Span and Memory: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [basics](basics.en.md). **Time:** ~1.5 h.

## What to choose: Span, Memory or an array

| You need | Type |
|---|---|
| a synchronous method reads or writes a region | `Span<T>` / `ReadOnlySpan<T>` |
| the region is stored in a field, passed to `async`, put in a collection | `Memory<T>` / `ReadOnlyMemory<T>` |
| a ready slice of an array for an old API | `ArraySegment<T>` (it has fewer methods than `Memory<T>`) |
| a sequence of several blocks | `ReadOnlySequence<T>` |
| external memory with its own owner | `IMemoryOwner<T>` (`MemoryPool<T>`) |

From `Memory<T>` you get a `Span<T>` through `.Span`; from an array — through `AsMemory()`/`AsSpan()`. A `Span<T>` cannot be turned back into a `Memory<T>`: a `Span` has no information about the memory owner.

## More methods and techniques

### Splitting and strings

`Split` on a `ReadOnlySpan<char>` (.NET 9+) returns ranges rather than strings, so there are no allocations; `EnumerateLines()` walks lines taking `\n` and `\r\n` into account:

```csharp
ReadOnlySpan<char> csv = "a,b,,c";
foreach (var r in csv.Split(',')) Console.Write($"[{csv[r]}]");   // [a][b][][c]

foreach (var line in "one\ntwo\r\n\nthree".AsSpan().EnumerateLines())
    Console.Write($"[{line}]");                                    // [one][two][][three]
```

Formatting and parsing into a ready buffer: `TryFormat` and `TryParse` accept `Span<char>`/`ReadOnlySpan<char>`:

```csharp
Span<char> chars = stackalloc char[16];
42.TryFormat(chars, out int written);
Console.WriteLine(chars[..written].ToString());               // 42
Console.WriteLine(int.TryParse("123".AsSpan(), out var v));   // True
```

The literal `"…"u8` creates a `ReadOnlySpan<byte>` with UTF-8 bytes without allocating memory (the data lives in the assembly): `"héllo"u8.Length` is 6, because "é" takes two bytes (verified).

### Searching for a set of characters

`SearchValues<T>` prepares a set of values for fast vectorized search; create it once and keep it in a `static readonly` field:

```csharp
var vowels = SearchValues.Create("aeiou");
Console.WriteLine("hello world".AsSpan().IndexOfAny(vowels));   // 1
Console.WriteLine("hello".AsSpan().Count('l'));                 // 2
```

### out parameters and ref struct enumerators

A method can return slices through `out ReadOnlySpan<char>` — no allocations, and the slice stays valid as long as the source string is alive:

```csharp
static bool TryParseKv(ReadOnlySpan<char> s, out ReadOnlySpan<char> key, out ReadOnlySpan<char> value)
{
    int i = s.IndexOf('=');
    if (i < 0) { key = value = default; return false; }
    key = s[..i].Trim(); value = s[(i + 1)..].Trim();
    return true;
}
Console.WriteLine(TryParseKv("key = value", out var k, out var v2) + $" {k}|{v2}");   // True key|value
```

You can write your own enumerator as a `ref struct` — see [exercise 5](exercises.en.md).

## ArrayPool: reusing arrays

For temporary large buffers (beyond the limits of `stackalloc`), take the array from a pool instead of `new`: the GC load drops because the array is used many times:

```csharp
var pool = ArrayPool<byte>.Shared;
byte[] rented = pool.Rent(100);          // length >= 100, content is NOT cleared
try
{
    Span<byte> work = rented.AsSpan(0, 100);   // work with exactly what you asked for
    work.Fill(1);
}
finally
{
    pool.Return(rented);                 // return it; do not use the array afterwards
}
```

Verified: `Rent(100)` returned an array of length ≥ 100. Rules: return it in `finally`; do not use the array after `Return`; do not rely on it being zeroed (with `Return(array, clearArray: true)` the pool clears it itself if it held secrets); do not return the same array twice.

## Bridges to other types

### `List<T>` and `CollectionsMarshal`

`CollectionsMarshal.AsSpan(list)` hands out a `Span<T>` over the list's internal array — without copying, but with a risk: if the list reallocates the array (`Add` beyond capacity), the old `Span` points to an abandoned array:

```csharp
var list = new List<int>(3) { 1, 2, 3 };
var ls = CollectionsMarshal.AsSpan(list);
list.Add(4);                      // capacity exceeded: the list allocates a new array
ls[1] = 200;                      // writes to the abandoned old array
Console.WriteLine($"list[1] = {list[1]}, span[1] = {ls[1]}");   // list[1] = 2, span[1] = 200
```

Do not modify the list while such a `Span` is alive.

### Casting: MemoryMarshal

`MemoryMarshal.Cast<TFrom, TTo>` reinterprets bytes as another type without copying:

```csharp
Span<byte> bytes = [1, 0, 0, 0, 2, 0, 0, 0];
Span<int> ints = MemoryMarshal.Cast<byte, int>(bytes);
Console.WriteLine($"{ints.Length}: {ints[0]},{ints[1]}");   // 2: 1,2
```

The result depends on the platform's byte order (little-endian on x86/x64/ARM), so for network formats use `BinaryPrimitives`.

## System.IO.Pipelines

`Pipe` is a buffer between a producer and a consumer that manages the memory pool itself and can pass data split across blocks. The `System.IO.Pipelines` package is part of ASP.NET Core (Kestrel is built on it); in a console project add it with `dotnet add package System.IO.Pipelines`. The producer writes to a `PipeWriter`, the consumer reads a `ReadOnlySequence<byte>` from a `PipeReader`:

```csharp
var pipe = new Pipe();

async Task Produce(PipeWriter writer)
{
    foreach (var chunk in new[] { "hel", "lo\nwor", "ld\nlast" })    // lines split across chunks
    {
        var bytes = Encoding.UTF8.GetBytes(chunk);
        bytes.CopyTo(writer.GetSpan(bytes.Length));                   // write straight into the pipe buffer
        writer.Advance(bytes.Length);
        await writer.FlushAsync();
    }
    await writer.CompleteAsync();
}

async Task Consume(PipeReader reader)
{
    while (true)
    {
        ReadResult result = await reader.ReadAsync();
        ReadOnlySequence<byte> buffer = result.Buffer;
        SequencePosition? nl;
        while ((nl = buffer.PositionOf((byte)'\n')) is { } pos)
        {
            Console.WriteLine($"line: {Encoding.UTF8.GetString(buffer.Slice(0, pos))}");
            buffer = buffer.Slice(buffer.GetPosition(1, pos));         // skip the line and '\n'
        }
        reader.AdvanceTo(buffer.Start, buffer.End);                    // consumed up to Start, examined everything
        if (result.IsCompleted)
        {
            if (!buffer.IsEmpty) Console.WriteLine($"tail: {Encoding.UTF8.GetString(buffer)}");
            break;
        }
    }
    await reader.CompleteAsync();
}

await Task.WhenAll(Produce(pipe.Writer), Consume(pipe.Reader));
```

```text
line: hello
line: world
tail: last
```

(`using System.Buffers; using System.IO.Pipelines; using System.Text;`.) The key is `AdvanceTo(consumed, examined)`: the first position is what was consumed and can be released, the second is what was examined; the unconsumed part stays in the buffer until more data arrives, so lines split between blocks ("hel" + "lo") are joined without manual buffering.

## Best practices

| Rule | Why |
|---|---|
| Accept `ReadOnlySpan<T>` if you do not modify data | the caller can pass a string, an array or stack memory |
| Bound the size of `stackalloc` | otherwise a stack overflow kills the process |
| `Span` inside synchronous methods, `Memory` at `async` boundaries | `ref struct` restrictions |
| Do not cache a `Span` from a `List<T>` | the array may be reallocated |
| Return a rented array in `finally` | otherwise the pool loses buffers |
| Measure first (BenchmarkDotNet, module "19 · Performance" is not created yet) | the gain is not everywhere |

## Related materials

- Back: [basics](basics.en.md). Next: [expert theory](expert.en.md) — internals, `ref struct`, `Unsafe`.
- [10 · Async/await](../10-async-await/overview.en.md) — `Memory<T>` in asynchronous methods, `IAsyncEnumerable`.
- [12 · Memory and GC](../12-memory-and-gc/overview.en.md) — LOH, pools, GC pressure.
- [06 · Collections and generics](../06-collections-and-generics/overview.en.md) — `List<T>` and its internal array.
- [18 · Concurrency](../18-concurrency/overview.en.md) — `Channel<T>` as another way to pass data between threads.

## Sources

- [`ArrayPool<T>`](https://learn.microsoft.com/dotnet/api/system.buffers.arraypool-1)
- [`CollectionsMarshal.AsSpan`](https://learn.microsoft.com/dotnet/api/system.runtime.interopservices.collectionsmarshal.asspan)
- [`System.IO.Pipelines`](https://learn.microsoft.com/dotnet/standard/io/pipelines)
- [`SearchValues`](https://learn.microsoft.com/dotnet/api/system.buffers.searchvalues)
- [`MemoryMarshal.Cast`](https://learn.microsoft.com/dotnet/api/system.runtime.interopservices.memorymarshal.cast)
