---
title: "Span и Memory: базовая теория"
lang: ru
track: csharp
kind: module
doc: basics
slug: 12a-span-and-memory
part: 2
tags: [span, memory, readonly-span, stackalloc, ref-struct, slicing, zero-allocation]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Span и Memory: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [02 · Типы и переменные](../02-types-and-variables/overview.ru.md), [06 · Коллекции и обобщения](../06-collections-and-generics/overview.ru.md), [12 · Память и GC](../12-memory-and-gc/overview.ru.md). **Время:** ~1 ч.

## Зачем это нужно

Чтобы обработать часть массива или строки, привычный код копирует её: `Substring`, `Split`, `Array.Copy`. Каждая копия — выделение памяти в куче, а значит, работа для [сборщика мусора](../12-memory-and-gc/overview.ru.md). `Span<T>` — это **представление** (view) на непрерывный кусок памяти: оно показывает часть массива, строки или стека без копирования.

## `Span<T>` и `ReadOnlySpan<T>`

`Span<T>` хранит ссылку на начало участка и его длину. Через него можно читать и **изменять** элементы; `ReadOnlySpan<T>` — только читать (для строк `string` неявно приводится к `ReadOnlySpan<char>`).

```csharp
int[] data = [1, 2, 3, 4, 5, 6];
Span<int> all = data;                 // a view of the whole array
Span<int> mid = all.Slice(1, 3);      // elements 2, 3, 4 — no copy
mid[0] = 99;                          // writes through to the array
Console.WriteLine(string.Join(",", data));   // 1,99,3,4,5,6
Console.WriteLine(mid.Length);               // 3
```

Срез (`Slice` или диапазон `[1..4]`) создаёт новое представление на тот же участок памяти. Изменение через `mid` видно в исходном массиве `data`.

### Разбор строки без копирования

Вместо `Split` и `Substring` — срезы `ReadOnlySpan<char>`:

```csharp
string line = "42;17;8";
ReadOnlySpan<char> rest = line;
int sum = 0;
while (true)
{
    int i = rest.IndexOf(';');
    var part = i < 0 ? rest : rest[..i];   // no allocation
    sum += int.Parse(part);                // int.Parse accepts ReadOnlySpan<char>
    if (i < 0) break;
    rest = rest[(i + 1)..];
}
Console.WriteLine(sum);   // 67
```

Проверено: `Substring` на строке в 10 000 символов выделяет ≥ 10 000 байт, а `AsSpan` — 0 байт (`GC.GetAllocatedBytesForCurrentThread` до и после).

## Полезные методы

Методы работают одинаково для массивов, списков (через `CollectionsMarshal`) и стековой памяти:

```csharp
Span<int> a = [3, 1, 2];
a.Sort();                                    // 1,2,3
Console.WriteLine(a.IndexOf(2));             // 1
Console.WriteLine(a.SequenceEqual([1, 2, 3]));   // True
a.Reverse();                                 // 3,2,1
Span<int> dst = new int[5];
a.CopyTo(dst);                               // 3,2,1,0,0
Console.WriteLine(a.TryCopyTo(new int[1]));  // False: too small, no exception
```

`CopyTo` в слишком маленький приёмник бросает `ArgumentException`, `TryCopyTo` возвращает `false`. Выход за границы — `IndexOutOfRangeException` (индексатор) или `ArgumentOutOfRangeException` (`Slice`): проверено.

## stackalloc: память на стеке

`stackalloc` выделяет массив на стеке вызовов — без участия GC и очень быстро, но память живёт только до конца метода:

```csharp
Span<byte> buf = stackalloc byte[16];
buf.Fill(7);
Console.WriteLine(buf.Length + " " + buf[15]);   // 16 7
Span<int> sp = stackalloc int[] { 1, 2, 3 };
```

Размер стека ограничен (порядка мегабайта на поток, зависит от ОС и настроек), поэтому `stackalloc` подходит только для небольших буферов (сотни байт), а размер нужно ограничивать, например: `Span<char> buf = n <= 256 ? stackalloc char[n] : new char[n];`.

## Ограничения Span

`Span<T>` — `ref struct`: тип, который живёт только на стеке. Компилятор запрещает всё, что могло бы вынести его в кучу (проверено, коды ошибок):

| Нельзя | Ошибка |
|---|---|
| поле обычного класса или структуры | `CS8345` |
| захват в лямбду или локальную функцию | `CS8175` |
| приведение к `object`, упаковка | `CS0029` |
| хранение через `await` или `yield` | `CS4007` |
| аргумент обобщения (`List<Span<int>>`) | `CS9244` |
| возврат `stackalloc`-памяти из метода | `CS8352` |

Для хранения в полях и использования с `async` существует `Memory<T>`.

## `Memory<T>`: когда нужно хранить

`Memory<T>` — обычная структура: её можно хранить в поле, передавать в `async`-методы и коллекции. Чтобы работать с данными, берут `.Span`:

```csharp
static async Task<int> ReadAsync(Memory<byte> m)
{
    await Task.Delay(1);       // Memory can cross an await, Span cannot
    m.Span.Fill(1);
    return m.Length;
}

var arr = new byte[8];
Console.WriteLine(await ReadAsync(arr.AsMemory(2, 4)));   // 4
Console.WriteLine(string.Join("", arr));                  // 00111100
```

Правило: в параметрах синхронных методов — `Span<T>`/`ReadOnlySpan<T>`, в `async` и полях — `Memory<T>`/`ReadOnlyMemory<T>`.

## Ошибки новичка

- ❌ `Substring`/`Split` в горячем цикле разбора → ✅ срезы `ReadOnlySpan<char>`.
- ❌ `stackalloc` с размером из пользовательского ввода → ✅ ограничивать размер и падать на массив.
- ❌ Хранить `Span<T>` в поле → ✅ хранить `Memory<T>` и брать `.Span` по месту.
- ❌ Ждать (`await`) при живом `Span<T>` в `async`-методе → ✅ вынести работу со `Span` в синхронный метод.
- ❌ Использовать `Span` как «ускоритель» без замера → ✅ сначала проверить, что копирование — узкое место.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — `Memory<T>` и `ArrayPool`, `CollectionsMarshal`, `Pipelines`.
- [12 · Память и GC](../12-memory-and-gc/overview.ru.md) — куча, стек, выделения.
- [02 · Типы и переменные](../02-types-and-variables/overview.ru.md) — структуры и строки.
- [10 · Async/await](../10-async-await/overview.ru.md) — почему `Span` нельзя использовать через `await`.
- [11 · Современный C#](../11-modern-csharp/overview.ru.md) — выражения коллекций для `Span<T>`.

## Источники

- [`Span<T>`](https://learn.microsoft.com/dotnet/api/system.span-1)
- [Рекомендации по использованию `Memory<T>` и `Span<T>`](https://learn.microsoft.com/dotnet/standard/memory-and-spans/memory-t-usage-guidelines)
- [Memory и spans](https://learn.microsoft.com/dotnet/standard/memory-and-spans/)
- [stackalloc](https://learn.microsoft.com/dotnet/csharp/language-reference/operators/stackalloc)
