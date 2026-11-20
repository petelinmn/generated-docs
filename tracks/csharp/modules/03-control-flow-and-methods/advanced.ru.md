---
title: "Управляющие конструкции и методы: продвинутая теория"
lang: ru
track: csharp
kind: module
doc: advanced
slug: 03-control-flow-and-methods
part: 1
tags: [control-flow, switch-expression, pattern-matching, loops, methods, ref-out-in, overloading, local-functions, closures]
versions: ".NET 11, C# 15"
status: verified
updated: 2026-11-20
---

# Управляющие конструкции и методы: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md) · [упражнения](exercises.ru.md) · [5–6. Код и тесты](code/)

## Модификаторы параметров: ref, out, in, params

По умолчанию аргумент передаётся **по значению**: метод получает копию значения (для ссылочных типов — копию ссылки, см. [02 · Типы и переменные](../02-types-and-variables/basics.ru.md)). Модификаторы меняют это:

```csharp
// ref: the method works with the caller's variable itself, not with a copy.
public static void Swap(ref int a, ref int b) => (a, b) = (b, a);

// out: the method must assign the value before returning. This is the Try-pattern.
public static bool TryDivide(int dividend, int divisor, out int quotient)
{
    if (divisor == 0)
    {
        quotient = 0;
        return false;
    }
    quotient = dividend / divisor;
    return true;
}

// in: passed by reference but read-only, so a large struct is not copied.
public static double Length(in Vector3D v) => Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);

// params collections (C# 13): params over a span, no array allocation at the call site.
public static int SumSpan(params ReadOnlySpan<int> values)
{
    int sum = 0;
    foreach (int value in values)
    {
        sum += value;
    }
    return sum;
}
```

| Модификатор | Передача | Метод может менять | Вызывающий пишет |
|---|---|---|---|
| нет | копия значения | только копию | `M(x)` |
| `ref` | ссылка на переменную | да | `M(ref x)`, переменная инициализирована |
| `out` | ссылка на переменную | обязан присвоить | `M(out var x)` |
| `in` | ссылка, только чтение | нет | `M(in x)` или `M(x)` |
| `params` | набор аргументов | — | `M(1, 2, 3)` |

Паттерн **Try** (`bool TryXxx(…, out T result)`) — стандартный способ сообщить о неудаче без исключения: `int.TryParse`, `Dictionary.TryGetValue`. С C# 13 `params` работает не только с массивом, но и с коллекциями, включая `ReadOnlySpan<T>`, что убирает аллокацию массива. Код: [Ex04_ParameterModifiers.cs](code/Examples/Ex04_ParameterModifiers.cs).

Для маленьких типов (`int`, `double`, небольших структур) `ref` и `in` не дают выигрыша в скорости: копирование нескольких байт не дороже передачи ссылки, а доступ через ссылку добавляет косвенность. `in` имеет смысл для больших структур, причём `readonly struct` — почему, разобрано в [экспертной теории](expert.ru.md).

## Перегрузка методов

Методы с одним именем, но разными параметрами — перегрузки (overloads). Компилятор выбирает перегрузку **во время компиляции** по типам аргументов и предпочитает «лучшее» преобразование:

```csharp
public static class Overloading
{
    // The parameter is unused on purpose: only the chosen overload matters.
    public static string Describe(int value) => "int";

    public static string Describe(long value) => "long";

    public static string Describe(double value) => "double";

    public static string Describe(string? value) => "string";

    public static string Describe(object? value) => "object";
}
```

| Вызов | Выбрано | Почему |
|---|---|---|
| `Describe(5)` | `int` | точное совпадение |
| `Describe(5L)` | `long` | точное совпадение |
| `Describe(5f)` | `double` | `float` → `double` лучше, чем упаковка в `object` |
| `Describe('a')` | `int` | `char` неявно преобразуется в `int`, `long`, `double`; `int` — самый «узкий» |
| `Describe((short)5)` | `int` | то же правило |
| `Describe(5m)` | `object` | неявного `decimal` → `double` нет, остаётся только упаковка |
| `Describe(null)` | `string` | `string` конкретнее, чем `object` |

Правило «лучшего целевого типа»: из двух подходящих перегрузок выигрывает та, чей тип параметра неявно преобразуется в тип другой, но не наоборот (`int` → `long` есть, `long` → `int` нет). Код: [Ex05_Overloading.cs](code/Examples/Ex05_Overloading.cs).

## Локальные функции

```csharp
// The local function Fib is visible only inside Fibonacci and can use its locals (memo).
public static long Fibonacci(int n)
{
    ArgumentOutOfRangeException.ThrowIfNegative(n);
    var memo = new Dictionary<int, long>();
    return Fib(n);

    long Fib(int k)
    {
        if (k < 2)
        {
            return k;
        }
        if (memo.TryGetValue(k, out long cached))
        {
            return cached;
        }
        long result = Fib(k - 1) + Fib(k - 2);
        memo[k] = result;
        return result;
    }
}

// A static local function cannot capture locals: an accidental capture becomes a compile error.
public static int SumOfSquares(int[] values)
{
    int total = 0;
    foreach (int value in values)
    {
        total += Square(value);
    }
    return total;

    static int Square(int x) => x * x;
}
```

Локальная функция объявлена внутри метода и видна только в нём. В отличие от лямбды, она может быть рекурсивной без трюков, может быть `static` (тогда захват переменных — ошибка компиляции) и не требует делегата. `Fibonacci(50)` → `12586269025`. Код: [Ex06_LocalFunctions.cs](code/Examples/Ex06_LocalFunctions.cs).

## Ловушки циклов

```csharp
// Wrong: changing a List<T> invalidates its enumerator, the next MoveNext throws.
public static void RemoveNegativesWrong(List<int> values)
{
    foreach (int value in values)
    {
        if (value < 0)
        {
            values.Remove(value);
        }
    }
}

// foreach creates a fresh variable for every iteration (since C# 5), so each closure sees its own value.
public static int[] CaptureInForeach()
{
    var getters = new List<Func<int>>();
    foreach (int i in new[] { 1, 2, 3 })
    {
        getters.Add(() => i);
    }
    return getters.Select(get => get()).ToArray();
}

// for has a single variable for the whole loop: every closure sees its final value.
public static int[] CaptureInFor()
{
    var getters = new List<Func<int>>();
    for (int i = 1; i <= 3; i++)
    {
        getters.Add(() => i);
    }
    return getters.Select(get => get()).ToArray();
}
```

- `RemoveNegativesWrong([1, -2, 3])` бросает `InvalidOperationException`: коллекцию нельзя менять, пока её обходит `foreach`. Правильно — идти по индексу с конца или вызвать `RemoveAll`.
- `CaptureInForeach()` → `[1, 2, 3]`, а `CaptureInFor()` → `[4, 4, 4]`. Лямбда захватывает **переменную**, а не её значение. В `foreach` переменная новая на каждой итерации, в `for` — одна на весь цикл, и после выхода она равна `4`.

Код: [Ex07_LoopPitfalls.cs](code/Examples/Ex07_LoopPitfalls.cs).

## Частые ошибки

- ❌ Изменять `List<T>` внутри `foreach` по нему → ✅ цикл `for` с конца или `RemoveAll`.
- ❌ Захватывать переменную `for` в лямбду и ждать текущего значения → ✅ скопировать в локальную переменную внутри тела или использовать `foreach`.
- ❌ `Math.Abs(int.MinValue)` → ✅ помнить, что у `int.MinValue` нет положительной пары: метод бросает `OverflowException` (проверено тестом для `DigitSum(int.MinValue)`).
- ❌ Возвращать «магические» значения (`-1`, `null`) при ошибке разбора → ✅ паттерн Try с `out`.
- ❌ Перегрузки, которые отличаются только числовыми типами → ✅ помнить правила выбора; иногда понятнее разные имена методов.
- ❌ Длинные цепочки `if/else if` по одному значению → ✅ switch expression.

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
