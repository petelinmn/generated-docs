---
title: "Control flow and methods: basics"
lang: en
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

# Control flow and methods: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md) · [exercises](exercises.en.md) · [5–6. Code and tests](code/)

**Prerequisites:** [02 · Types and variables](../02-types-and-variables/overview.en.md) — value and reference types, copying when passing arguments

## Branching: if and switch

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

- An `if` condition must be of type `bool`: `if (count)` for an `int` does not compile.
- A `switch` has no fall-through between sections: each one ends with `break`, `return`, `throw` or `goto`. Several labels in a row (`case Saturday: case Sunday:`) are allowed.

A **switch expression** (C# 8) is a `switch` used as an expression that returns a value. The arms are checked in order, and the first matching one wins:

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

`_` is the "anything" pattern (discard). `< 0` and `>= 0 and < 20` are relational and logical patterns (C# 9). `FizzBuzz(15)` → `FizzBuzz`, `Classify(25)` → `warm`. Pattern matching is covered in detail in module 11. Code: [Ex01_Branching.cs](code/Examples/Ex01_Branching.cs).

## Loops

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

| Loop | When to use |
|---|---|
| `for` | an index or a known number of iterations is needed |
| `while` | the condition is checked before the body; the body may never run |
| `do … while` | the body must run at least once |
| `foreach` | iterating a collection without an index — the most common case |

`SumTo(10)` → `55`, `CountDigits(0)` → `1`, `SumEvenUntilZero([2, 3, 4, 0, 100])` → `6`. Code: [Ex02_Loops.cs](code/Examples/Ex02_Loops.cs).

## Labels for break and continue (C# 15)

A plain `break` leaves only the nearest loop. Leaving two nested loops at once used to require `goto` or a flag variable. In C# 15, `break` and `continue` can name the label of an enclosing loop:

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

For `grid = [[1, 2, 3], [4, -5, 6], [7, 8, 5]]`: `FindFirst(grid, 8)` → `(2, 1)`, `FindFirst(grid, 42)` → `null`, `CountRowsWithoutNegatives(grid)` → `2`. The label goes before the loop (`outer:`), and `break outer` ends exactly that loop. Code: [Ex02_Loops.cs](code/Examples/Ex02_Loops.cs).

## Methods

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

- **Optional parameters** have a default value: `Greet("Ann")` → `Hello, Ann!`.
- **Named arguments** let you pass parameters in any order: `Greet(greeting: "Hi", name: "Bob")` → `Hi, Bob!`.
- **`params`** accepts any number of arguments: `Sum()`, `Sum(1, 2, 3)`, `Sum([1, 2])`.
- **An expression body** `=> …` is shorthand for a method made of a single expression.

Code: [Ex03_Methods.cs](code/Examples/Ex03_Methods.cs).

## Related materials

- Prerequisites: [02 · Types and variables](../02-types-and-variables/overview.en.md)
- Earlier: [01 · The .NET platform and dotnet CLI](../01-dotnet-platform/overview.en.md)
- The same idea in other tracks: [Python](../../../python/overview.en.md) — `match`, `for … in` loops, `*args` and `**kwargs` arguments

## Sources

- [Selection statements: if, switch](https://learn.microsoft.com/dotnet/csharp/language-reference/statements/selection-statements)
- [Switch expression](https://learn.microsoft.com/dotnet/csharp/language-reference/operators/switch-expression)
- [Patterns](https://learn.microsoft.com/dotnet/csharp/language-reference/operators/patterns)
- [Iteration statements](https://learn.microsoft.com/dotnet/csharp/language-reference/statements/iteration-statements)
- [Method parameters: ref, out, in, params](https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/method-parameters)
- [Named and optional arguments](https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/named-and-optional-arguments)
- [Local functions](https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/local-functions)
- [Ref returns and ref variables](https://learn.microsoft.com/dotnet/csharp/language-reference/statements/declarations#reference-variables)
