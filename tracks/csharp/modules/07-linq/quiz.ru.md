---
title: "Квиз: LINQ"
lang: ru
track: csharp
kind: module
doc: quiz
slug: 07-linq
part: 1
tags: [linq, deferred-execution, closures, iterator, iqueryable, yield]
status: draft
updated: 2026-11-22
---

# Квиз: LINQ

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает, из какого уровня вопрос: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что выведет код?

```csharp
var nums = new List<int> { 1, 2, 3 };
var q = nums.Where(x => x > 1);
nums.Add(4);
Console.WriteLine(string.Join(",", q));
```

- A) `2,3`
- B) `2,3,4`
- C) `1,2,3,4`
- D) ошибка выполнения

<details><summary>Ответ</summary>

**B.** `Where` не выполняется при создании запроса — он выполняется при переборе (отложенное выполнение), поэтому запрос видит добавленный элемент (проверено). `ToList()` до добавления дал бы снимок `2,3`.
→ [Базовая теория](basics.ru.md#отложенное-выполнение)

</details>

## 2. 🟢 Какие вызовы выполняют запрос немедленно?

- [ ] A) `Where`
- [ ] B) `ToList`
- [ ] C) `Count`
- [ ] D) `Select`

<details><summary>Ответ</summary>

**B, C.** `ToList`, `ToArray`, `Count`, `Sum`, `First` и подобные запускают перебор сразу. `Where` и `Select` лишь описывают запрос и выполняются позже.
→ [Базовая теория](basics.ru.md#материализация-в-коллекции)

</details>

## 3. 🟢 Что бросит `Single()` на последовательности из двух элементов?

- A) вернёт первый элемент
- B) `InvalidOperationException` («Sequence contains more than one element»)
- C) `null`
- D) `ArgumentException`

<details><summary>Ответ</summary>

**B.** `Single` требует ровно один элемент: на двух — «more than one element», на пустой — «no elements» (проверено). `First` возьмёт первый, `FirstOrDefault` вернёт значение по умолчанию на пустой.
→ [Базовая теория](basics.ru.md#выбор-одного-элемента)

</details>

## 4. 🟢 Что делает `GroupBy(w => w[0])` над списком слов?

- A) сортирует слова по первой букве
- B) возвращает последовательность групп: ключ — первая буква, элементы — слова на неё
- C) удаляет дубликаты
- D) объединяет слова в одну строку

<details><summary>Ответ</summary>

**B.** Каждая группа — `IGrouping<char, string>` с `Key`. Для сортировки есть `OrderBy`, для дубликатов — `Distinct`.
→ [Базовая теория](basics.ru.md#группировка)

</details>

## 5. 🟡 Сколько раз выполнится источник?

```csharp
int evaluations = 0;
IEnumerable<int> Source() { foreach (var i in new[] { 1, 2, 3 }) { evaluations++; yield return i; } }

var lazy = Source();
var c = lazy.Count();
var s = lazy.Sum();
Console.WriteLine($"{c} {s} evaluations={evaluations}");
```

- A) `evaluations=3`
- B) `evaluations=6`
- C) `evaluations=1`
- D) `evaluations=0`

<details><summary>Ответ</summary>

**B.** Каждый терминальный оператор перебирает отложенный запрос заново: `Count()` и `Sum()` дали 3 + 3 (проверено). Если результат нужен несколько раз — материализуйте один раз: `var snap = Source().ToList();` (тогда `evaluations=3`).
→ [Продвинутая теория](advanced.ru.md#многократное-перечисление)

</details>

## 6. 🟡 Что выведет код?

```csharp
int limit = 3;
var q = Enumerable.Range(1, 10).Where(x => x <= limit);
limit = 5;
Console.WriteLine(q.Count());
```

- A) `3`
- B) `5`
- C) `10`
- D) `0`

<details><summary>Ответ</summary>

**B.** Лямбда захватывает **переменную** `limit`, а не её значение, и читает её при выполнении запроса, когда `limit` уже `5` (проверено).
→ [Продвинутая теория](advanced.ru.md#замыкания-переменная-читается-при-выполнении)

</details>

## 7. 🟡 Что вернёт пустая последовательность?

- [ ] A) `new int[0].Max()` — исключение
- [ ] B) `new int?[0].Max()` — `null`
- [ ] C) `new int[0].Sum()` — `0`
- [ ] D) `new int[0].Average()` — `0`

<details><summary>Ответ</summary>

**A, B, C.** `Max` и `Average` для `int` на пустой последовательности бросают `InvalidOperationException`, для `int?` (и ссылочных типов) `Max` возвращает `null`, `Sum` — `0` (проверено). D неверно: `Average` бросает исключение.
→ [Продвинутая теория](advanced.ru.md#пустые-последовательности-и-граничные-случаи)

</details>

## 8. 🟡 Что выведут два цикла?

```csharp
var a = new List<Func<int>>();
for (int i = 0; i < 3; i++) a.Add(() => i);

var b = new List<Func<int>>();
foreach (var j in Enumerable.Range(0, 3)) b.Add(() => j);

Console.WriteLine(string.Join(",", a.Select(f => f())));
Console.WriteLine(string.Join(",", b.Select(f => f())));
```

<details><summary>Ответ</summary>

```text
3,3,3
0,1,2
```

В `for` одна переменная `i` на весь цикл — все замыкания видят конечное значение `3`. В `foreach` переменная создаётся заново на каждой итерации (проверено).
→ [Экспертная теория](expert.ru.md#for-и-foreach-захватывают-по-разному)

</details>

## 9. 🟡 Открытый вопрос: чем `IEnumerable<T>` отличается от `IQueryable<T>` и чем это опасно?

<details><summary>Ключевые пункты хорошего ответа</summary>

- `IEnumerable<T>` выполняет лямбды в памяти как делегаты; `IQueryable<T>` хранит дерево выражения (`Expression`), которое провайдер (например, EF Core) переводит в SQL;
- в `IQueryable` в `Where` можно использовать только то, что провайдер умеет перевести; обычные методы C# внутри не работают;
- вызов `AsEnumerable()`/`ToList()` слишком рано переносит фильтрацию в память и загружает лишние данные;
- сочетание: фильтры и проекции — на стороне БД, финальную обработку — в памяти ([15 · EF Core](../15-ef-core/overview.ru.md)).

</details>

## 10. 🔴 Сколько элементов прочитает источник до первого результата?

```csharp
Src("a", 3).OrderBy(x => -x).Take(1);   // Src yields 3 elements
```

- A) 1
- B) 3
- C) 2
- D) 0

<details><summary>Ответ</summary>

**B.** `OrderBy` буферизует всю последовательность перед выдачей первого элемента — `Take(1)` не спасает (проверено). Аналогично `Join` читает всю вторую последовательность (журнал `o1 i1 i2 i3 got1`). Ленивые «потоковые» операторы — `Where`, `Select`, `Take`.
→ [Экспертная теория](expert.ru.md#orderby-буферизует-take-не-спасает)

</details>

## 11. 🔴 Найдите ошибку

```csharp
static IEnumerable<int> Read(IEnumerable<int> src)
{
    ArgumentNullException.ThrowIfNull(src);
    try { foreach (var x in src) yield return x; }
    catch (Exception) { yield break; }
}
```

<details><summary>Ответ</summary>

Две проблемы. Во-первых, `yield return` внутри `try` с `catch` — ошибка компиляции `CS1626`. Во-вторых, даже без неё проверка аргумента в итераторе откладывается до первого `MoveNext()`, а не выполняется при вызове (в отличие от LINQ-операторов, которые проверяют сразу). Решение: разделить метод на обёртку, проверяющую аргументы и возвращающую локальный итератор.
→ [Экспертная теория](expert.ru.md#yield-и-trycatch)

</details>
