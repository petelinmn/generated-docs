---
title: "Span и Memory"
lang: ru
track: csharp
kind: module
doc: overview
slug: 12a-span-and-memory
part: 2
levels: [1, 2, 3, 4]
tags: [span, memory, stackalloc, ref-struct, array-pool, pipelines, zero-allocation]
versions: ".NET 11, C# 15; System.IO.Pipelines 11.0.0"
status: draft
updated: 2026-11-22
---

# Span и Memory

🌐 **Русский** · [English](overview.en.md)

## Коротко

`Span<T>` и `ReadOnlySpan<T>` — представления на непрерывный участок памяти (массив, строку, стек, нативную память): ссылка плюс длина, без копирования. Они позволяют разбирать строки и обрабатывать буферы без выделений в куче и нагрузки на сборщик мусора. Ограничение — `ref struct`: `Span` живёт только на стеке; для полей и `async` служит `Memory<T>`. Поверх этих типов построены `ArrayPool<T>`, `System.IO.Pipelines` и быстрый разбор в самой библиотеке .NET.

## Ключевые идеи

- **Span не копирует:** `Slice` и диапазоны создают новое представление на ту же память; запись видна в исходном массиве.
- **`ReadOnlySpan<char>`** заменяет `Substring` и `Split` при разборе: `IndexOf`, срезы, `int.Parse(span)`, `Split` по диапазонам.
- **`stackalloc`** даёт буфер на стеке без GC; размер нужно ограничивать.
- **`Span<T>` — `ref struct`:** нельзя в поле класса, в лямбде, через `await`, как аргумент обобщения (если нет `allows ref struct`).
- **`Memory<T>`** хранится в полях и ходит через `async`; `.Span` даёт быстрый доступ.
- **`ArrayPool<T>`** переиспользует большие массивы: `Rent` возвращает массив ≥ запрошенного, `Return` — в `finally`.
- **`System.IO.Pipelines`** склеивает данные, разорванные между блоками, и сам управляет буферами.
- **Осторожно:** `Span` над `List<T>` устаревает после перевыделения; `==` сравнивает ссылку и длину.

## Минимальный пример

```csharp
string line = "42;17;8";
ReadOnlySpan<char> rest = line;
int sum = 0;
while (true)
{
    int i = rest.IndexOf(';');
    var part = i < 0 ? rest : rest[..i];   // a slice, no allocation
    sum += int.Parse(part);
    if (i < 0) break;
    rest = rest[(i + 1)..];
}
Console.WriteLine(sum);   // 67
```

## Когда применять, а когда нет

- ✅ Горячие пути разбора и форматирования, сетевые протоколы, буферы.
- ✅ Методы, принимающие «часть массива или строки»: `ReadOnlySpan<T>` вместо копий.
- ✅ Малые временные буферы — `stackalloc`; большие — `ArrayPool<T>`.
- ❌ Преждевременная оптимизация: сначала замер, потом `Span`.
- ❌ Хранение `Span<T>` между вызовами или через `await` — используйте `Memory<T>`.
- ❌ `stackalloc` по размеру из внешнего ввода.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | зачем Span, `Span<T>`/`ReadOnlySpan<T>`, срезы, разбор строк, `stackalloc`, ограничения `ref struct`, `Memory<T>` |
| [2. Продвинутая теория](advanced.ru.md) | выбор типа, `Split`/`EnumerateLines`/`SearchValues`/UTF-8, `ArrayPool`, `CollectionsMarshal`, `MemoryMarshal`, `System.IO.Pipelines` |
| [3. Экспертная теория](expert.ru.md) | устройство и внутренние указатели, правила `ref struct`, `scoped` и `allows ref struct`, равенство, подвохи, 14 вопросов с собеседований |
| [4. Самопроверка](quiz.ru.md) | 12 вопросов и [5 упражнений](exercises.ru.md) с решениями |
| 5. Запускаемые примеры | ещё не создан — `/learn csharp 12a-span-and-memory level 5` |
| 6. Тесты | ещё не создан — `/learn csharp 12a-span-and-memory level 6` |

## Связанные материалы

- Пререквизиты: [12 · Память и GC](../12-memory-and-gc/overview.ru.md), [06 · Коллекции и обобщения](../06-collections-and-generics/overview.ru.md)
- Рядом: [10 · Async/await](../10-async-await/overview.ru.md) (`Memory<T>` и `await`), [11 · Современный C#](../11-modern-csharp/overview.ru.md) (выражения коллекций)
- Дальше: [18 · Многопоточность](../18-concurrency/overview.ru.md)
- Измерение выделений: [19 · Производительность](../19-performance/overview.ru.md)
- То же в других треках: [Python](../../../python/overview.ru.md) — `memoryview` и `bytes`/`bytearray`
- Рецепты и библиотеки: [BenchmarkDotNet](../../libraries/benchmarkdotnet/overview.ru.md)
