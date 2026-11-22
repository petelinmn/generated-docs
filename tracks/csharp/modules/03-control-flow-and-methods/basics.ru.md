---
title: "Управляющие конструкции и методы: базовая теория"
lang: ru
track: csharp
kind: module
doc: basics
slug: 03-control-flow-and-methods
part: 1
tags: [control-flow, switch-expression, pattern-matching, loops, methods, ref-out-in, overloading, local-functions, closures]
versions: ".NET 11, C# 15"
status: verified
updated: 2026-11-20
---

# Управляющие конструкции и методы: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md) · [упражнения](exercises.ru.md) · [5–6. Код и тесты](code/)

**Пререквизиты:** [02 · Типы и переменные](../02-types-and-variables/overview.ru.md) — значимые и ссылочные типы, копирование при передаче

## Ветвления: if и switch

```csharp
public static string Sign(int value)
{
    if (value > 0)
    {
        return "positive";
    }
    else if (value < 0)
    {
        return "negative";
    }
    else
    {
        return "zero";
    }
}

// A switch statement: every section must end with break, return, throw or goto — no fall-through.
public static string DayType(DayOfWeek day)
{
    switch (day)
    {
        case DayOfWeek.Saturday:
        case DayOfWeek.Sunday:
            return "weekend";
        default:
            return "workday";
    }
}
```

- Условие `if` должно иметь тип `bool`: `if (count)` для `int` не скомпилируется.
- В `switch` нет «проваливания» (fall-through) между секциями: каждая заканчивается `break`, `return`, `throw` или `goto`. Несколько меток подряд (`case Saturday: case Sunday:`) допустимы.

**Switch expression** (C# 8) — `switch` как выражение, которое возвращает значение. Ветки (arms) проверяются по порядку, срабатывает первая подходящая:

```csharp
// A switch expression over a tuple (C# 8+): the first matching arm wins.
public static string FizzBuzz(int n) => (n % 3, n % 5) switch
{
    (0, 0) => "FizzBuzz",
    (0, _) => "Fizz",
    (_, 0) => "Buzz",
    _ => n.ToString(CultureInfo.InvariantCulture),
};

// Relational and logical patterns (C# 9+).
public static string Classify(int celsius) => celsius switch
{
    < 0 => "freezing",
    >= 0 and < 20 => "cold",
    >= 20 and < 30 => "warm",
    _ => "hot",
};
```

`_` — шаблон «что угодно» (discard). `< 0`, `>= 0 and < 20` — реляционные и логические шаблоны (C# 9). `FizzBuzz(15)` → `FizzBuzz`, `Classify(25)` → `warm`. Сопоставление с шаблонами подробно разбирается в модуле [11 · Современный C#](../11-modern-csharp/overview.ru.md). Код: [Ex01_Branching.cs](code/Examples/Ex01_Branching.cs).

## Циклы

```csharp
public static int SumTo(int n)
{
    int sum = 0;
    for (int i = 1; i <= n; i++)
    {
        sum += i;
    }
    return sum;
}

// do-while runs the body at least once: 0 still has one digit.
public static int CountDigits(int n)
{
    n = Math.Abs(n);
    int count = 0;
    do
    {
        count++;
        n /= 10;
    }
    while (n > 0);
    return count;
}

// break leaves the loop, continue jumps to the next iteration.
public static int SumEvenUntilZero(int[] values)
{
    int sum = 0;
    foreach (int value in values)
    {
        if (value == 0)
        {
            break;
        }
        if (value % 2 != 0)
        {
            continue;
        }
        sum += value;
    }
    return sum;
}
```

| Цикл | Когда использовать |
|---|---|
| `for` | нужен индекс или известное число итераций |
| `while` | условие проверяется до тела; тело может не выполниться ни разу |
| `do … while` | тело должно выполниться хотя бы раз |
| `foreach` | обход коллекции без индекса — самый частый случай |

`SumTo(10)` → `55`, `CountDigits(0)` → `1`, `SumEvenUntilZero([2, 3, 4, 0, 100])` → `6`. Код: [Ex02_Loops.cs](code/Examples/Ex02_Loops.cs).

## Метки для break и continue (C# 15)

Обычный `break` выходит только из ближайшего цикла. Чтобы выйти сразу из двух вложенных циклов, раньше нужны были `goto` или переменная-флаг. В C# 15 `break` и `continue` могут указать метку внешнего цикла:

```csharp
// C# 15: break and continue can name the label of an enclosing loop.
// Before C# 15, leaving both loops at once needed goto or a flag variable.
public static (int Row, int Column)? FindFirst(int[][] grid, int target)
{
    (int Row, int Column)? found = null;
    outer:
    for (int row = 0; row < grid.Length; row++)
    {
        for (int column = 0; column < grid[row].Length; column++)
        {
            if (grid[row][column] == target)
            {
                found = (row, column);
                break outer;
            }
        }
    }
    return found;
}

// continue with a label skips the rest of the current row and moves to the next one.
public static int CountRowsWithoutNegatives(int[][] grid)
{
    int count = 0;
    rows:
    foreach (int[] row in grid)
    {
        foreach (int value in row)
        {
            if (value < 0)
            {
                continue rows;
            }
        }
        count++;
    }
    return count;
}
```

Для `grid = [[1, 2, 3], [4, -5, 6], [7, 8, 5]]`: `FindFirst(grid, 8)` → `(2, 1)`, `FindFirst(grid, 42)` → `null`, `CountRowsWithoutNegatives(grid)` → `2`. Метка ставится перед циклом (`outer:`), и `break outer` завершает именно этот цикл. Код: [Ex02_Loops.cs](code/Examples/Ex02_Loops.cs).

## Методы

```csharp
// An optional parameter: callers may omit it, the default value is compiled into the call site.
public static string Greet(string name, string greeting = "Hello") => $"{greeting}, {name}!";

// params: the caller passes any number of arguments, the method receives an array.
public static int Sum(params int[] values)
{
    int sum = 0;
    foreach (int value in values)
    {
        sum += value;
    }
    return sum;
}

// An expression-bodied method: a single expression instead of a block with return.
public static double CircleArea(double radius) => Math.PI * radius * radius;
```

- **Необязательные параметры** имеют значение по умолчанию: `Greet("Ann")` → `Hello, Ann!`.
- **Именованные аргументы** позволяют передавать параметры в любом порядке: `Greet(greeting: "Hi", name: "Bob")` → `Hi, Bob!`.
- **`params`** принимает любое число аргументов: `Sum()`, `Sum(1, 2, 3)`, `Sum([1, 2])`.
- **Тело-выражение** `=> …` — сокращение для метода из одного выражения.

Код: [Ex03_Methods.cs](code/Examples/Ex03_Methods.cs).

## Связанные материалы

- Пререквизиты: [02 · Типы и переменные](../02-types-and-variables/overview.ru.md)
- Раньше: [01 · Платформа .NET и dotnet CLI](../01-dotnet-platform/overview.ru.md)
- То же в других треках: [Python](../../../python/overview.ru.md) — `match`, циклы `for … in`, аргументы `*args` и `**kwargs`

## Источники

- [Операторы выбора: if, switch](https://learn.microsoft.com/dotnet/csharp/language-reference/statements/selection-statements)
- [Switch expression](https://learn.microsoft.com/dotnet/csharp/language-reference/operators/switch-expression)
- [Шаблоны (patterns)](https://learn.microsoft.com/dotnet/csharp/language-reference/operators/patterns)
- [Операторы цикла](https://learn.microsoft.com/dotnet/csharp/language-reference/statements/iteration-statements)
- [Параметры методов: ref, out, in, params](https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/method-parameters)
- [Именованные и необязательные аргументы](https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/named-and-optional-arguments)
- [Локальные функции](https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/local-functions)
- [ref-возвраты и ref-переменные](https://learn.microsoft.com/dotnet/csharp/language-reference/statements/declarations#reference-variables)
