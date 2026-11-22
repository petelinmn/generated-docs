---
title: "Упражнения: Современный C#"
lang: ru
track: csharp
kind: module
doc: exercises
slug: 11-modern-csharp
part: 2
tags: [pattern-matching, switch-expression, collection-expressions, field-keyword, list-patterns, extension-members, union-types]
status: draft
updated: 2026-11-22
---

# Упражнения: Современный C#

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Решения — под спойлерами. Код проверен запуском; запускаемых версий в `code/` пока нет (уровень 5 не создан).

## 1. 🟢 Оценка по баллам

**Задача:** напиши `static string Grade(int score)` на `switch`-выражении: меньше 0 или больше 100 — `"invalid"`, от 90 — `"A"`, от 75 — `"B"`, от 50 — `"C"`, иначе `"F"`.

**Ожидаемый результат:**

```text
-1 → invalid, 101 → invalid, 95 → A, 80 → B, 60 → C, 10 → F
```

<details><summary>Подсказка</summary>

Образец `< 0 or > 100` ставь первым. Затем идут реляционные образцы по убыванию границ: `>= 90`, `>= 75`, `>= 50`, и `_`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 1: relational patterns in a switch expression.
static string Grade(int score) => score switch
{
    < 0 or > 100 => "invalid",
    >= 90 => "A",
    >= 75 => "B",
    >= 50 => "C",
    _ => "F",
};
```

Проверено: результат `invalid invalid A B C F`. Порядок ветвей важен: границы идут по убыванию, чтобы `95` не попало в более слабую ветвь.

</details>

## 2. 🟢 Слияние коллекций

**Задача:** напиши `static int[] Merge(int[] a, List<int> b)`: возвращает элементы `a`, затем число `0` как разделитель, затем элементы `b`. Используй выражение коллекции.

**Ожидаемый результат:**

```text
Merge([1, 2], [3, 4]) → [1, 2, 0, 3, 4]
Merge([], [])         → [0]
```

<details><summary>Подсказка</summary>

Spread `..` вставляет элементы другой коллекции: `[..a, 0, ..b]`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 2: collection expression with spreads.
static int[] Merge(int[] a, List<int> b) => [..a, 0, ..b];
```

Проверено: для пустых входов длина результата `1`. Целевой тип `int[]` определяет, что создаётся массив.

</details>

## 3. 🟡 Температура с проверкой

**Задача:** напиши класс `Temperature` со свойством `double Celsius`, которое не даёт установить значение ниже абсолютного нуля (−273,15): слишком малое значение заменяется на −273,15. Не объявляй отдельное поле.

**Ожидаемый результат:**

```text
new Temperature { Celsius = -500 }.Celsius == -273.15  → True
Celsius = 25                                            → 25
```

<details><summary>Подсказка</summary>

Ключевое слово `field` (C# 14) обращается к автоматическому полю: `get; set => field = ...`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 3: validation in a property setter via the field keyword.
class Temperature
{
    public double Celsius { get; set => field = Math.Max(value, -273.15); }
}
```

Проверено: `-500` превращается в `-273.15`, `25` сохраняется. Геттер остаётся автоматическим.

</details>

## 4. 🟡 Разбор команды

**Задача:** напиши `static string Command(string[] args)` на списочных образцах: пустой массив — `"no args"`; `["help"]` — `"help"`; `["add", x]` — `"add x"`; `["add", x, ...остальные]` — `"add x and N more"`; любая команда, у которой последний аргумент `--force`, — `"<команда> (forced)"`; всё прочее — `"unknown"`.

**Ожидаемый результат:**

```text
[]                       → no args
["help"]                 → help
["add", "x"]             → add x
["add", "x", "y", "z"]   → add x and 2 more
["rm", "a", "--force"]   → rm (forced)
["zzz"]                  → unknown
```

<details><summary>Подсказка</summary>

Более частные ветви — выше общих: `["add", var item]` — до `["add", var first, .. var rest]`. Для последнего элемента: `[var cmd, .., "--force"]`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 4: list patterns for command-line parsing.
static string Command(string[] args) => args switch
{
    [] => "no args",
    ["help"] => "help",
    ["add", var item] => $"add {item}",
    ["add", var first, .. var rest] => $"add {first} and {rest.Length} more",
    [var cmd, .., "--force"] => $"{cmd} (forced)",
    _ => "unknown",
};
```

Проверено на шести входах из таблицы. `["add", "x"]` не попадает в четвёртую ветвь, потому что третья стоит выше.

</details>

## 5. 🔴 Расширения через extension members

**Задача:** напиши блоки `extension` для: (а) свойства `bool IsPalindrome` у `string` — игнорирует регистр и всё, кроме букв и цифр; (б) **статического** члена `string.Repeat(string value, int count)`; (в) свойства `double Median` у `IEnumerable<int>` (для пустой последовательности — `InvalidOperationException`).

**Ожидаемый результат:**

```text
"A man, a plan, a canal: Panama".IsPalindrome  → True
"hello".IsPalindrome                           → False
string.Repeat("ab", 3)                         → ababab
new[] { 3, 1, 2 }.Median                       → 2
```

<details><summary>Подсказка</summary>

`static class StringExt { extension(string s) { public bool IsPalindrome => …; public static string Repeat(string value, int count) => …; } extension(IEnumerable<int> src) { … } }`. Для медианы отсортируй элементы (`Order()`).

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 5: extension properties and a static extension member (C# 14).
static class StringExt
{
    extension(string s)
    {
        public bool IsPalindrome
        {
            get
            {
                var letters = s.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray();
                return letters.SequenceEqual(letters.Reverse());
            }
        }
        public static string Repeat(string value, int count) => string.Concat(Enumerable.Repeat(value, count));
    }

    extension(IEnumerable<int> src)
    {
        public double Median
        {
            get
            {
                var sorted = src.Order().ToArray();
                if (sorted.Length == 0) throw new InvalidOperationException("Empty sequence.");
                int mid = sorted.Length / 2;
                return sorted.Length % 2 == 1 ? sorted[mid] : (sorted[mid - 1] + sorted[mid]) / 2.0;
            }
        }
    }
}
```

Проверено: результаты совпадают с таблицей. Статический член `Repeat` вызывается как `string.Repeat(...)`, хотя тип `string` не изменялся.

</details>

## 6. 🔴 Результат как union

**Задача:** объяви `record Ok<T>(T Value)`, `record Error(string Message)` и обобщённый union `Result<T>` из этих двух вариантов (C# 15). Напиши `static string Show(Result<int> r)` на `switch` без ветви `_`: для `Ok` — `"ok <значение>"`, для `Error` — `"error <сообщение>"`.

**Ожидаемый результат:**

```text
new Ok<int>(5)    → ok 5
new Error("bad")  → error bad
```

<details><summary>Подсказка</summary>

`public union Result<T>(Ok<T>, Error);`. Значение варианта неявно приводится к union, а `switch` по позиционным образцам `Ok<int>(var v)` и `Error(var m)` исчерпывающий.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 6: a generic union (C# 15) and an exhaustive switch.
public record Ok<T>(T Value);
public record Error(string Message);
public union Result<T>(Ok<T>, Error);

static string Show(Result<int> r) => r switch
{
    Ok<int>(var v) => $"ok {v}",
    Error(var m) => $"error {m}",
};
```

Проверено на SDK 11.0.100: для обоих значений выводится `ok 5` и `error bad` без предупреждений. Если убрать одну ветвь, компилятор выдаст `CS8509`.

</details>
