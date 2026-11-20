---
title: "Упражнения: Типы и переменные"
lang: ru
track: csharp
kind: module
doc: exercises
slug: 02-types-and-variables
part: 1
tags: [type-conversion, nullable-reference-types, overflow, struct-layout]
status: verified
updated: 2026-11-20
---

# Упражнения: Типы и переменные

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Решения — под спойлерами, запускаемые версии — в [code/Solutions/Solutions.cs](code/Solutions/Solutions.cs), тесты к ним — в [code/Tests/SolutionsTests.cs](code/Tests/SolutionsTests.cs).

## 1. 🟢 Разбор температуры

**Задача:** напиши `Temperature.ParseCelsius(string? input)`, который возвращает `double?`: число, если строка — корректная десятичная запись с точкой, и `null` в остальных случаях. Добавь `Temperature.ToFahrenheit(double celsius)`.

**Ожидаемый результат:**

```text
ParseCelsius("36.6")  → 36.6
ParseCelsius("36,6")  → null
ParseCelsius(null)    → null
ToFahrenheit(100)     → 212
```

<details><summary>Подсказка</summary>

`double.TryParse` с `NumberStyles.Float` и `CultureInfo.InvariantCulture`. Формула: `F = C × 9 / 5 + 32`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 1: parse user input safely and convert units.
public static class Temperature
{
    // Invariant culture: "36.6" must parse the same way on every machine.
    public static double? ParseCelsius(string? input) =>
        double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out double value)
            ? value
            : null;

    public static double ToFahrenheit(double celsius) => celsius * 9 / 5 + 32;
}
```

Класс `Temperature` в [Solutions.cs](code/Solutions/Solutions.cs). Явная культура важна: без неё `"36.6"` не разберётся на машине с русской локалью, где разделитель — запятая.

</details>

## 2. 🟡 Сумма без тихого переполнения

**Задача:** напиши `SafeMath.CheckedSum(int[] values)`, который бросает `OverflowException` при переполнении, и `SafeMath.WideSum(int[] values)`, который возвращает `long` и не переполняется.

**Ожидаемый результат:**

```text
CheckedSum([1, 2, 3])             → 6
CheckedSum([int.MaxValue, 1])     → OverflowException
WideSum([int.MaxValue, int.MaxValue]) → 4294967294
```

<details><summary>Подсказка</summary>

`checked(sum + value)` внутри цикла. Для `WideSum` достаточно аккумулятора типа `long`: `int` неявно расширяется.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 2: detect overflow instead of returning a wrong sum.
public static class SafeMath
{
    public static int CheckedSum(int[] values)
    {
        int sum = 0;
        foreach (int value in values)
        {
            sum = checked(sum + value);
        }
        return sum;
    }

    // A wider accumulator avoids the overflow altogether.
    public static long WideSum(int[] values)
    {
        long sum = 0;
        foreach (int value in values)
        {
            sum += value;
        }
        return sum;
    }
}
```

Класс `SafeMath` в [Solutions.cs](code/Solutions/Solutions.cs).

</details>

## 3. 🟡 Нормализация имени

**Задача:** напиши `Names.Normalize(string? name)`. Для `null`, пустой строки или строки из пробелов метод возвращает `"Anonymous"`. В остальных случаях — обрезает пробелы, делает первую букву заглавной, остальные строчными (без учёта культуры).

**Ожидаемый результат:**

```text
Normalize(null)      → "Anonymous"
Normalize("  alice ") → "Alice"
Normalize("bOB")     → "Bob"
```

<details><summary>Подсказка</summary>

`string.IsNullOrWhiteSpace` — после этой проверки компилятор считает `name` ненулевым. Диапазон `trimmed[1..]` даёт хвост строки.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 3: handle nullable input explicitly.
public static class Names
{
    public static string Normalize(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "Anonymous";
        }
        string trimmed = name.Trim();
        return char.ToUpperInvariant(trimmed[0]) + trimmed[1..].ToLowerInvariant();
    }
}
```

Класс `Names` в [Solutions.cs](code/Solutions/Solutions.cs). Обрати внимание: предупреждений nullable нет, хотя оператор `!` не используется.

</details>

## 4. 🔴 Компактная структура

**Задача:** структура `Wasteful { byte A; long B; byte C; }` занимает 24 байта. Объяви структуру `Compact` с теми же тремя полями так, чтобы `Unsafe.SizeOf<Compact>()` был равен 16.

**Ожидаемый результат:**

```text
Unsafe.SizeOf<Wasteful>() → 24
Unsafe.SizeOf<Compact>()  → 16
```

<details><summary>Подсказка</summary>

Нарисуй раскладку по байтам: `long` должен начинаться со смещения, кратного 8. Где появляются пустые байты?

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 4: the same fields in a different order take less memory.
public struct Wasteful
{
    public byte A;
    public long B;
    public byte C;
}

// Largest fields first: the two bytes share one 8-byte slot after the long.
public struct Compact
{
    public long B;
    public byte A;
    public byte C;
}
```

Структуры `Wasteful` и `Compact` в [Solutions.cs](code/Solutions/Solutions.cs). В `Wasteful` после `A` идут 7 байт выравнивания, потом `B` (8), потом `C` и ещё 7 байт: всего 24. В `Compact` `long` идёт первым, два `byte` занимают смещения 8 и 9, а размер округляется до кратного 8: 16.

</details>
