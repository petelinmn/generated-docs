---
title: "Упражнения: Управляющие конструкции и методы"
lang: ru
track: csharp
kind: module
doc: exercises
slug: 03-control-flow-and-methods
part: 1
tags: [switch-expression, ref-out-in, loops, local-functions]
status: verified
updated: 2026-11-20
---

# Упражнения: Управляющие конструкции и методы

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Решения — под спойлерами, запускаемые версии — в [code/Solutions/Solutions.cs](code/Solutions/Solutions.cs), тесты к ним — в [code/Tests/SolutionsTests.cs](code/Tests/SolutionsTests.cs).

## 1. 🟢 Оценка по баллам

**Задача:** напиши `Grades.Grade(int score)`, который возвращает оценку: 90–100 → `A`, 80–89 → `B`, 70–79 → `C`, 60–69 → `D`, 0–59 → `F`. Для баллов вне 0–100 метод бросает `ArgumentOutOfRangeException`. Используй switch expression.

**Ожидаемый результат:**

```text
Grade(90)  → "A"
Grade(59)  → "F"
Grade(101) → ArgumentOutOfRangeException
```

<details><summary>Подсказка</summary>

Первой веткой поставь проверку диапазона: `< 0 or > 100 => throw …`. Дальше ветки идут от большего к меньшему, и каждая следующая может полагаться на то, что предыдущие не сработали.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 1: a switch expression with relational patterns and a guard for invalid input.
public static class Grades
{
    public static string Grade(int score) => score switch
    {
        < 0 or > 100 => throw new ArgumentOutOfRangeException(nameof(score), score, "Score must be 0..100"),
        >= 90 => "A",
        >= 80 => "B",
        >= 70 => "C",
        >= 60 => "D",
        _ => "F",
    };
}
```

Класс `Grades` в [Solutions.cs](code/Solutions/Solutions.cs). `throw` можно использовать как выражение прямо в ветке switch.

</details>

## 2. 🟡 Разбор координат

**Задача:** напиши `Coordinates.TryParse(string? input, out int x, out int y)` для строк вида `"3,-4"`. Метод возвращает `true` и заполняет `x` и `y`, если в строке ровно два целых числа через запятую, иначе — `false`.

**Ожидаемый результат:**

```text
TryParse("3,-4", out x, out y) → true, x = 3, y = -4
TryParse("3,4,5", …)           → false
TryParse(null, …)              → false
```

<details><summary>Подсказка</summary>

`out`-параметры должны быть присвоены на любом пути выхода — присвой им `0` в начале. `int.TryParse` с `out x` присваивает значение напрямую.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 2: the Try-pattern with out parameters.
public static class Coordinates
{
    public static bool TryParse(string? input, out int x, out int y)
    {
        x = 0;
        y = 0;
        if (input is null)
        {
            return false;
        }
        string[] parts = input.Split(',');
        return parts.Length == 2
            && int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out x)
            && int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out y);
    }
}
```

Класс `Coordinates` в [Solutions.cs](code/Solutions/Solutions.cs). Цепочка `&&` останавливается на первом `false`, поэтому второй `TryParse` не выполняется, если первый не удался.

</details>

## 3. 🟡 Подсчёт простых чисел

**Задача:** напиши `Primes.CountUpTo(int limit)`, который считает простые числа от 2 до `limit` включительно. Проверку «простое ли число» оформи как `static` локальную функцию.

**Ожидаемый результат:**

```text
CountUpTo(10)  → 4    (2, 3, 5, 7)
CountUpTo(100) → 25
CountUpTo(1)   → 0
```

<details><summary>Подсказка</summary>

Делители достаточно проверять до квадратного корня: условие цикла `divisor * divisor <= n`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 3: loops plus a static local function.
public static class Primes
{
    public static int CountUpTo(int limit)
    {
        int count = 0;
        for (int n = 2; n <= limit; n++)
        {
            if (IsPrime(n))
            {
                count++;
            }
        }
        return count;

        static bool IsPrime(int n)
        {
            for (int divisor = 2; divisor * divisor <= n; divisor++)
            {
                if (n % divisor == 0)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
```

Класс `Primes` в [Solutions.cs](code/Solutions/Solutions.cs).

</details>

## 4. 🔴 Обратный отсчёт без IEnumerable

**Задача:** создай структуру `Countdown`, которую можно обойти в `foreach`. `new Countdown(3)` даёт `3, 2, 1`, `new Countdown(0)` — ничего. Не реализуй `IEnumerable` и не создавай объектов в куче.

**Ожидаемый результат:**

```text
foreach (int n in new Countdown(3)) → 3, 2, 1
```

<details><summary>Подсказка</summary>

Посмотри на [Ex10_DuckTypedForeach.cs](code/Examples/Ex10_DuckTypedForeach.cs): нужен метод `GetEnumerator()`, возвращающий структуру с `MoveNext()` и `Current`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 4: a foreach-able type without IEnumerable and without heap allocations.
public readonly struct Countdown
{
    private readonly int _from;

    public Countdown(int from) => _from = from;

    public Enumerator GetEnumerator() => new(_from);

    public struct Enumerator
    {
        private int _current;

        public Enumerator(int from) => _current = from + 1;

        public readonly int Current => _current;

        public bool MoveNext() => --_current > 0;
    }
}
```

Структура `Countdown` в [Solutions.cs](code/Solutions/Solutions.cs). Перечислитель начинает с `from + 1`, а `MoveNext` сначала уменьшает значение, потом проверяет его.

</details>
