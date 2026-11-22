---
title: "Span и Memory: продвинутая теория"
lang: ru
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

# Span и Memory: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [базовая теория](basics.ru.md). **Время:** ~1,5 ч.

## Что выбрать: Span, Memory или массив

| Нужно | Тип |
|---|---|
| синхронный метод читает или пишет участок | `Span<T>` / `ReadOnlySpan<T>` |
| участок хранится в поле, передаётся в `async`, кладётся в коллекцию | `Memory<T>` / `ReadOnlyMemory<T>` |
| готовый кусок массива для старого API | `ArraySegment<T>` (у него меньше методов, чем у `Memory<T>`) |
| последовательность из нескольких блоков | `ReadOnlySequence<T>` |
| внешняя память с собственным владельцем | `IMemoryOwner<T>` (`MemoryPool<T>`) |

Из `Memory<T>` получают `Span<T>` через `.Span`; из массива — через `AsMemory()`/`AsSpan()`. Из `Span<T>` обратно в `Memory<T>` превратить нельзя: у `Span` нет информации о владельце памяти.

## Ещё методы и приёмы

### Разбиение и строки

`Split` на `ReadOnlySpan<char>` (.NET 9+) возвращает диапазоны, а не строки, поэтому выделений нет; `EnumerateLines()` обходит строки с учётом `\n` и `\r\n`:

```csharp
ReadOnlySpan<char> csv = "a,b,,c";
foreach (var r in csv.Split(',')) Console.Write($"[{csv[r]}]");   // [a][b][][c]

foreach (var line in "one\ntwo\r\n\nthree".AsSpan().EnumerateLines())
    Console.Write($"[{line}]");                                    // [one][two][][three]
```

Форматирование и разбор в готовый буфер: `TryFormat` и `TryParse` принимают `Span<char>`/`ReadOnlySpan<char>`:

```csharp
Span<char> chars = stackalloc char[16];
42.TryFormat(chars, out int written);
Console.WriteLine(chars[..written].ToString());               // 42
Console.WriteLine(int.TryParse("123".AsSpan(), out var v));   // True
```

Литерал `"…"u8` создаёт `ReadOnlySpan<byte>` с байтами UTF-8 без выделения памяти (данные лежат в сборке): `"héllo"u8.Length` — 6, так как «é» занимает два байта (проверено).

### Поиск набора символов

`SearchValues<T>` подготавливает набор значений для быстрого векторизованного поиска; создавайте его один раз и храните в `static readonly`-поле:

```csharp
var vowels = SearchValues.Create("aeiou");
Console.WriteLine("hello world".AsSpan().IndexOfAny(vowels));   // 1
Console.WriteLine("hello".AsSpan().Count('l'));                 // 2
```

### Параметры out и ref struct-перечислители

Метод может возвращать срезы через `out ReadOnlySpan<char>` — выделений нет, а срез остаётся действительным, пока жива исходная строка:

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

Свой перечислитель можно написать как `ref struct` — см. [упражнение 5](exercises.ru.md).

## ArrayPool: переиспользование массивов

Для временных больших буферов (больше пределов `stackalloc`) массив берут из пула вместо `new`: нагрузка на GC падает, потому что массив используется многократно:

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

Проверено: `Rent(100)` вернул массив длиной ≥ 100. Правила: возвращать в `finally`; не использовать массив после `Return`; не полагаться на то, что он нулевой (при `Return(array, clearArray: true)` пул очищает его сам, если в нём были секреты); не возвращать один массив дважды.

## Мосты к другим типам

### `List<T>` и `CollectionsMarshal`

`CollectionsMarshal.AsSpan(list)` отдаёт `Span<T>` на внутренний массив списка — без копирования, но с риском: если список перевыделит массив (`Add` сверх ёмкости), старый `Span` указывает на брошенный массив:

```csharp
var list = new List<int>(3) { 1, 2, 3 };
var ls = CollectionsMarshal.AsSpan(list);
list.Add(4);                      // capacity exceeded: the list allocates a new array
ls[1] = 200;                      // writes to the abandoned old array
Console.WriteLine($"list[1] = {list[1]}, span[1] = {ls[1]}");   // list[1] = 2, span[1] = 200
```

Не изменяйте список, пока живёт такой `Span`.

### Приведение типов: MemoryMarshal

`MemoryMarshal.Cast<TFrom, TTo>` переинтерпретирует байты как другой тип без копирования:

```csharp
Span<byte> bytes = [1, 0, 0, 0, 2, 0, 0, 0];
Span<int> ints = MemoryMarshal.Cast<byte, int>(bytes);
Console.WriteLine($"{ints.Length}: {ints[0]},{ints[1]}");   // 2: 1,2
```

Результат зависит от порядка байтов платформы (little-endian на x86/x64/ARM), поэтому для сетевых форматов используйте `BinaryPrimitives`.

## System.IO.Pipelines

`Pipe` — буфер между производителем и потребителем, который сам управляет пулом памяти и умеет передавать данные, разорванные на блоки. Пакет `System.IO.Pipelines` входит в ASP.NET Core (Kestrel построен на нём); в консольном проекте добавьте его через `dotnet add package System.IO.Pipelines`. Производитель пишет в `PipeWriter`, потребитель читает `ReadOnlySequence<byte>` из `PipeReader`:

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

(`using System.Buffers; using System.IO.Pipelines; using System.Text;`.) Ключевое — `AdvanceTo(consumed, examined)`: первая позиция — что потреблено и может быть освобождено, вторая — что просмотрено; непотреблённая часть остаётся в буфере до прихода следующих данных, поэтому строки, разорванные между блоками («hel» + «lo»), склеиваются без ручной буферизации.

## Лучшие практики

| Правило | Почему |
|---|---|
| Принимайте `ReadOnlySpan<T>`, если не изменяете данные | вызывающий передаст строку, массив или стек |
| Ограничивайте размер `stackalloc` | иначе `StackOverflow` убьёт процесс |
| `Span` — внутри синхронных методов, `Memory` — на границах `async` | ограничения `ref struct` |
| Не кэшируйте `Span` от `List<T>` | перевыделение массива |
| Арендованный массив возвращайте в `finally` | иначе пул теряет буферы |
| Сначала измерьте (BenchmarkDotNet, [19 · Производительность](../19-performance/overview.ru.md)) | выигрыш есть не везде |

## Связанные материалы

- Назад: [базовая теория](basics.ru.md). Дальше: [экспертная теория](expert.ru.md) — устройство, `ref struct`, `Unsafe`.
- [10 · Async/await](../10-async-await/overview.ru.md) — `Memory<T>` в асинхронных методах, `IAsyncEnumerable`.
- [12 · Память и GC](../12-memory-and-gc/overview.ru.md) — LOH, пулы, давление на GC.
- [06 · Коллекции и обобщения](../06-collections-and-generics/overview.ru.md) — `List<T>` и его внутренний массив.
- [18 · Многопоточность](../18-concurrency/overview.ru.md) — `Channel<T>` как другой способ передавать данные между потоками.

## Источники

- [`ArrayPool<T>`](https://learn.microsoft.com/dotnet/api/system.buffers.arraypool-1)
- [`CollectionsMarshal.AsSpan`](https://learn.microsoft.com/dotnet/api/system.runtime.interopservices.collectionsmarshal.asspan)
- [`System.IO.Pipelines`](https://learn.microsoft.com/dotnet/standard/io/pipelines)
- [`SearchValues`](https://learn.microsoft.com/dotnet/api/system.buffers.searchvalues)
- [`MemoryMarshal.Cast`](https://learn.microsoft.com/dotnet/api/system.runtime.interopservices.memorymarshal.cast)
