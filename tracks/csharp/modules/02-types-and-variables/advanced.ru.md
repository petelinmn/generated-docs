---
title: "Типы и переменные: продвинутая теория"
lang: ru
track: csharp
kind: module
doc: advanced
slug: 02-types-and-variables
part: 1
tags: [types, value-types, reference-types, type-conversion, strings, nullable-reference-types, boxing, floating-point]
versions: ".NET 11, C# 15"
status: verified
updated: 2026-11-20
---

# Типы и переменные: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md) · [упражнения](exercises.ru.md) · [5–6. Код и тесты](code/)

## Nullable reference types

Ссылочная переменная может содержать `null`, и обращение к её членам бросит `NullReferenceException`. С C# 8 компилятор умеет отслеживать это заранее. В проекте с `<Nullable>enable</Nullable>` (по умолчанию в новых шаблонах):

- `string` означает «никогда не null», `string?` — «может быть null»;
- обращение к членам `string?` без проверки даёт предупреждение **CS8602** («Dereference of a possibly null reference»);
- присваивание `null` в `string` — **CS8600**, неинициализированное поле `string` — **CS8618**.

Операторы для работы с null:

```csharp
// ?. yields null instead of throwing, ?? supplies a fallback value.
public static int LengthOrZero(string? text) => text?.Length ?? 0;

// ??= assigns only when the variable is null.
public static string EnsureName(string? name)
{
    name ??= "Anonymous";
    return name;
}

// Without the null check the compiler warns: CS8602 "Dereference of a possibly null reference".
public static string Shout(string? text)
{
    if (text is null)
    {
        return "";
    }
    return text.ToUpperInvariant(); // flow analysis knows that text is not null here
}
```

- `?.` — null-conditional: `text?.Length` равно `null`, если `text` — `null`.
- `??` — null-coalescing: значение справа, если слева `null`.
- `??=` — присвоить, если переменная равна `null`.
- `!` — null-forgiving: `text!.Length` говорит компилятору «я знаю, что здесь не null». Это только подавляет предупреждение, в runtime никакой проверки нет.
- `is null` / `is not null` — проверка, которую нельзя сломать перегрузкой оператора `==`.

Анализ потока (flow analysis) понимает проверки: после `if (text is null) return` компилятор считает `text` ненулевым.

## Упаковка (boxing)

Когда значимый тип приводится к `object` (или к интерфейсу), runtime создаёт в куче новый объект и **копирует** туда значение. Это boxing. Обратное действие — unboxing.

```csharp
// Two boxes of the same value are two different objects with equal contents.
public static (bool SameReference, bool EqualValues) CompareBoxes()
{
    object a = 42;
    object b = 42;
    return (ReferenceEquals(a, b), a.Equals(b));
}

// == on variables of type object compares references, not values.
public static bool EqualityOperatorOnBoxes()
{
    object a = 42;
    object b = 42;
    return a == b;
}

// Unboxing must use the exact boxed type: a boxed int cannot be unboxed as long.
public static long UnboxAsLong(object boxed) => (long)boxed;
```

- `CompareBoxes()` → `(false, true)`: две разные коробки с равным содержимым.
- `EqualityOperatorOnBoxes()` → `false`: `==` для `object` сравнивает ссылки.
- `UnboxAsLong(42)` → `InvalidCastException`: в коробке `int`, а распаковывают как `long`. Правильно: `(long)(int)boxed`.

Boxing стоит аллокации и работы GC. Он незаметно возникает при передаче значимых типов в API, которые принимают `object`. Обобщения (generics, [06 · Коллекции и обобщения](../06-collections-and-generics/overview.ru.md)) решают эту проблему. Код: [Ex05_Boxing.cs](code/Examples/Ex05_Boxing.cs). Упаковка — ещё и причина, по которой `lock` по значимому типу запрещён: см. [18 · Многопоточность](../18-concurrency/expert.ru.md).

## double и decimal

`float` и `double` — двоичные числа по стандарту IEEE 754. Большинство десятичных дробей (например, 0.1) в двоичной системе представить точно нельзя:

```csharp
// 0.1 has no exact binary representation, so the sum is slightly off.
public static bool DoubleSumIsExact() => 0.1 + 0.2 == 0.3;

public static bool DecimalSumIsExact() => 0.1m + 0.2m == 0.3m;

// "R" prints the shortest text that round-trips to the same double.
public static string DoubleSumText() => (0.1 + 0.2).ToString("R", CultureInfo.InvariantCulture);

// Compare doubles with a tolerance, never with ==.
public static bool NearlyEqual(double a, double b, double tolerance = 1e-9) => Math.Abs(a - b) <= tolerance;

// Floating-point division by zero does not throw: it yields infinity (integer division would throw).
public static double DivideByZero(double value) => value / 0.0;
```

| Вызов | Результат |
|---|---|
| `DoubleSumIsExact()` | `false` |
| `DecimalSumIsExact()` | `true` |
| `DoubleSumText()` | `0.30000000000000004` |
| `DivideByZero(1.0)` | `∞` (`double.PositiveInfinity`) |

Правило: деньги и всё, что должно сходиться «до копейки», хранят в `decimal`. Физику, графику и статистику — в `double`: он быстрее и имеет огромный диапазон. Целочисленное деление на ноль бросает `DivideByZeroException`, а деление `double` даёт бесконечность или `NaN`. Код: [Ex06_FloatingPoint.cs](code/Examples/Ex06_FloatingPoint.cs).

## Культура: регистр, сравнение, формат чисел

Многие строковые операции по умолчанию используют **текущую культуру** операционной системы. Код, который работает у вас, может сломаться у пользователя из другой страны:

```csharp
// In Turkish, the uppercase of "i" is "İ" (U+0130), not "I".
public static string ToUpperTurkish(string text) => text.ToUpper(CultureInfo.GetCultureInfo("tr-TR"));

public static string ToUpperInvariant(string text) => text.ToUpperInvariant();

// Ordinal comparison ignores the culture: the right choice for keys, identifiers, file names.
public static bool EqualsIgnoreCase(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

public static string FormatInGerman(double value) => value.ToString("N2", CultureInfo.GetCultureInfo("de-DE"));

public static string FormatInvariant(double value) => value.ToString("N2", CultureInfo.InvariantCulture);
```

- `ToUpperTurkish("i")` → `İ`, а `ToUpperInvariant("i")` → `I`.
- `FormatInGerman(1234.5)` → `1.234,50`, а `FormatInvariant(1234.5)` → `1,234.50`.

Правило: для текста, который видит человек, — культура пользователя. Для идентификаторов, ключей, файлов, протоколов и логов — `CultureInfo.InvariantCulture` и `StringComparison.Ordinal`/`OrdinalIgnoreCase`. Код: [Ex07_CultureAware.cs](code/Examples/Ex07_CultureAware.cs).

Если в проекте включён `InvariantGlobalization`, культуры недоступны вовсе — см. [подвох в модуле 01](../01-dotnet-platform/expert.ru.md#invariantglobalization-ломает-культуры).

## Частые ошибки

- ❌ Хранить деньги в `double` → ✅ `decimal`.
- ❌ Сравнивать `double` через `==` → ✅ сравнение с допуском (`NearlyEqual`).
- ❌ Ожидать, что `(int)2.9` даст `3` → ✅ `(int)` отбрасывает дробь; для округления — `Math.Round`, и помнить про банковское округление.
- ❌ `int.Parse(userInput)` → ✅ `int.TryParse` с явной культурой.
- ❌ Конкатенация строк в цикле → ✅ `StringBuilder` или `string.Join`.
- ❌ `ToUpper()` и `ToLower()` без культуры для сравнения → ✅ `string.Equals(a, b, StringComparison.OrdinalIgnoreCase)`.
- ❌ Глушить предупреждения nullable оператором `!` → ✅ проверка `is null` или осмысленное значение по умолчанию.
- ❌ Изменять копию структуры и удивляться, что оригинал не изменился → ✅ помнить семантику копирования; изменяемые структуры — источник ошибок.

## Связанные материалы

- Пререквизиты: [01 · Платформа .NET и dotnet CLI](../01-dotnet-platform/overview.ru.md) — проект, `<Nullable>enable</Nullable>`, сборка
- Дальше: [03 · Управляющие конструкции и методы](../03-control-flow-and-methods/overview.ru.md) — `ref`, `out`, `in` и передача значимых типов в методы
- Сколько стоит упаковка для GC: [12 · Память и GC](../12-memory-and-gc/advanced.ru.md#измерение-выделений-памяти)
- `decimal` в базе данных через EF Core (в SQLite хранится как `TEXT`): [15 · EF Core](../15-ef-core/expert.ru.md#особенности-sqlite)
- То же в других треках: [Python](../../../python/overview.ru.md) — динамическая типизация и аннотации типов вместо статических типов

## Источники

- [Встроенные типы C#](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/built-in-types)
- [Значимые типы](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/value-types)
- [Ссылочные типы](https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/reference-types)
- [Nullable reference types](https://learn.microsoft.com/dotnet/csharp/nullable-references)
- [Boxing и unboxing](https://learn.microsoft.com/dotnet/csharp/programming-guide/types/boxing-and-unboxing)
- [Числа с плавающей точкой](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/floating-point-numeric-types)
- [Сравнение строк](https://learn.microsoft.com/dotnet/standard/base-types/best-practices-strings)
- [Raw string literals](https://learn.microsoft.com/dotnet/csharp/language-reference/tokens/raw-string)
- [`string.Intern`](https://learn.microsoft.com/dotnet/api/system.string.intern)
