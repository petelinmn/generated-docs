---
title: "Квиз: Span и Memory"
lang: ru
track: csharp
kind: module
doc: quiz
slug: 12a-span-and-memory
part: 2
tags: [span, memory, stackalloc, ref-struct, array-pool, pipelines]
status: draft
updated: 2026-11-22
---

# Квиз: Span и Memory

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает, из какого уровня вопрос: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что такое `Span<T>`?

- A) копия участка массива
- B) представление на участок памяти без копирования: ссылка и длина
- C) коллекция с автоматическим ростом
- D) потокобезопасный буфер

<details><summary>Ответ</summary>

**B.** `Span<T>` не владеет памятью и не копирует её: он указывает на часть массива, строки или стека. A описывает `ToArray()`/`Substring`. C — `List<T>`. D неверно: потокобезопасность не гарантируется.
→ [Базовая теория](basics.ru.md#spant-и-readonlyspant)

</details>

## 2. 🟢 Что выведет код?

```csharp
int[] data = [1, 2, 3, 4, 5, 6];
Span<int> mid = data.AsSpan().Slice(1, 3);
mid[0] = 99;
Console.WriteLine(string.Join(",", data));
```

<details><summary>Ответ</summary>

```text
1,99,3,4,5,6
```

`Slice` создаёт новое представление на тот же массив, поэтому запись в `mid[0]` меняет `data[1]`.
→ [Базовая теория](basics.ru.md#spant-и-readonlyspant)

</details>

## 3. 🟢 Что делает вызов `big.AsSpan(100, 5000)` по сравнению с `big.Substring(100, 5000)`?

- A) обе операции выделяют 10 000 байт
- B) `AsSpan` не выделяет память в куче, `Substring` создаёт новую строку
- C) `AsSpan` создаёт копию, `Substring` — нет
- D) разницы нет

<details><summary>Ответ</summary>

**B.** Проверено через `GC.GetAllocatedBytesForCurrentThread`: `Substring` выделил ≥ 10 000 байт (5000 символов по 2 байта), `AsSpan` — 0 байт.
→ [Базовая теория](basics.ru.md#разбор-строки-без-копирования)

</details>

## 4. 🟢 Выберите все верные утверждения

- [ ] A) `Span<T>` можно хранить в поле обычного класса
- [ ] B) `Memory<T>` можно передавать в `async`-метод
- [ ] C) `stackalloc` выделяет память в куче
- [ ] D) `TryCopyTo` возвращает `false`, если приёмник слишком мал

<details><summary>Ответ</summary>

**B, D.** A неверно: `ref struct` в поле класса — ошибка `CS8345`. B верно — для этого `Memory<T>` и существует. C неверно: `stackalloc` берёт память со стека, поэтому она освобождается при выходе из метода. D верно: `CopyTo` бросил бы `ArgumentException`, `TryCopyTo` возвращает `false` (проверено).
→ [Базовая теория](basics.ru.md#полезные-методы)

</details>

## 5. 🟢 Почему этот код не компилируется?

```csharp
static async Task Run()
{
    Span<int> s = new int[2];
    await Task.Delay(1);
    Console.WriteLine(s.Length);
}
```

- A) `Span<T>` нельзя создавать из массива
- B) `Span<T>` нельзя использовать после `await` — ошибка `CS4007`
- C) `Task.Delay` не принимает значения
- D) нужен `ConfigureAwait(false)`

<details><summary>Ответ</summary>

**B.** Локальные переменные, пережившие `await`, живут в поле конечного автомата в куче; `ref struct` туда попасть не может. Решение — `Memory<int>` либо вынести работу со `Span` в синхронный метод.
→ [Базовая теория](basics.ru.md#ограничения-span)

</details>

## 6. 🟡 Что выведет код?

```csharp
var list = new List<int>(3) { 1, 2, 3 };
var ls = CollectionsMarshal.AsSpan(list);
list.Add(4);
ls[1] = 200;
Console.WriteLine(list[1]);
```

- A) `200`
- B) `2`
- C) `4`
- D) исключение

<details><summary>Ответ</summary>

**B.** Ёмкость списка 3; `Add` четвёртого элемента перевыделяет внутренний массив, а `ls` остаётся указателем на старый. Запись `200` попала в брошенный массив (проверено: `list[1] = 2, span[1] = 200`). Не изменяйте список, пока жив такой `Span`.
→ [Продвинутая теория](advanced.ru.md#listt-и-collectionsmarshal)

</details>

## 7. 🟡 Что верно для `ArrayPool<byte>.Shared.Rent(100)`?

- A) массив имеет ровно 100 элементов
- B) массив имеет не меньше 100 элементов и может содержать старые данные
- C) массив всегда нулевой
- D) после `Return` массив можно продолжать использовать

<details><summary>Ответ</summary>

**B.** Пул возвращает массив подходящего размера (≥ запрошенного), содержимое не очищается. Работайте через `AsSpan(0, 100)`. После `Return` массив принадлежит пулу — использование ведёт к повреждению чужих данных.
→ [Продвинутая теория](advanced.ru.md#arraypool-переиспользование-массивов)

</details>

## 8. 🟡 Выберите все верные утверждения

- [ ] A) `"héllo"u8` даёт `ReadOnlySpan<byte>` из 6 байт
- [ ] B) `SearchValues<char>` создают один раз и переиспользуют
- [ ] C) `MemoryMarshal.Cast<byte, int>` копирует данные
- [ ] D) `Split` у `ReadOnlySpan<char>` (.NET 9+) возвращает диапазоны, а не строки

<details><summary>Ответ</summary>

**A, B, D.** A: «é» — 2 байта в UTF-8, итого 6 (проверено). B: подготовка набора дорогая, поиск быстрый. C неверно: приведение переинтерпретирует ту же память. D: `csv[range]` даёт срез.
→ [Продвинутая теория](advanced.ru.md#разбиение-и-строки)

</details>

## 9. 🟡 Что делает `reader.AdvanceTo(buffer.Start, buffer.End)` в потребителе `Pipelines`, если в буфере осталась неполная строка?

- A) освобождает весь буфер, неполная строка теряется
- B) освобождает уже обработанные строки (всё до `buffer.Start`), а неполный остаток оставляет в буфере и отмечает просмотренным — следующий `ReadAsync` вернёт его вместе с новыми данными
- C) блокирует производителя навсегда
- D) вызывает исключение

<details><summary>Ответ</summary>

**B.** К моменту вызова `buffer` уже срезан после обработанных строк, поэтому `consumed = buffer.Start` освобождает их. `examined = buffer.End` говорит, что остаток просмотрен, и `ReadAsync` не вернётся, пока не появятся новые данные. Так строки, разорванные между блоками, склеиваются (в проверенном примере «hel» + «lo» → `hello`).
→ [Продвинутая теория](advanced.ru.md#systemiopipelines)

</details>

## 10. 🟡 Открытый вопрос: как разобрать строку `"42;17;8"` и сложить числа без выделений в куче? Что здесь можно, а что нельзя хранить между итерациями?

<details><summary>Ключевые пункты хорошего ответа</summary>

- `ReadOnlySpan<char> rest = line;` и цикл: `IndexOf(';')`, срез до разделителя, `int.Parse(span)`;
- затем `rest = rest[(i + 1)..]` — переход к остатку без копирования;
- альтернативы: `MemoryExtensions.Split` (диапазоны) или `EnumerateLines()` для строк;
- хранить срезы между итерациями цикла можно (локальные переменные), но не в полях класса, не через `await` и не в лямбдах;
- результат (число) — значение, поэтому выделений нет.

</details>

## 11. 🔴 Что произойдёт?

```csharp
object[] objs = new string[1];
Span<object> s = objs;
```

- A) компилируется и работает
- B) `ArrayTypeMismatchException` при присваивании
- C) ошибка компиляции
- D) `InvalidCastException`

<details><summary>Ответ</summary>

**B.** Массивы ковариантны: `string[]` можно присвоить `object[]`, но `Span<object>` над таким массивом позволил бы записать в строковый массив не-строку. Поэтому конструктор проверяет точный тип массива и бросает `ArrayTypeMismatchException` (проверено). `ReadOnlySpan<object>` создаётся без ошибки, так как записи невозможны.
→ [Экспертная теория](expert.ru.md#подвохи)

</details>

## 12. 🔴 Найдите ошибку

```csharp
static Span<int> Make()
{
    Span<int> s = stackalloc int[4];
    return s;
}
```

<details><summary>Ответ</summary>

Ошибка `CS8352`: возвращаемый `Span` указывает на стековую память метода, которая будет освобождена при выходе. Решения: вернуть массив (`new int[4]`) или принять буфер параметром (`static void Fill(Span<int> buffer)`), чтобы вызывающий владел памятью.
→ [Экспертная теория](expert.ru.md#почему-span-обязан-быть-ref-struct)

</details>
