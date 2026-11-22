---
title: "Типы и переменные: экспертная теория"
lang: ru
track: csharp
kind: module
doc: expert
slug: 02-types-and-variables
part: 1
tags: [types, value-types, reference-types, type-conversion, strings, nullable-reference-types, boxing, floating-point]
versions: ".NET 11, C# 15"
status: verified
updated: 2026-11-20
---

# Типы и переменные: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md) · [упражнения](exercises.ru.md) · [5–6. Код и тесты](code/)

## Где живут значения

Распространённое упрощение «значимые типы живут в стеке, ссылочные — в куче» неточно. Значение живёт **там, где объявлена переменная**:

- локальная переменная типа `int` — в стеке или в регистре процессора;
- поле `int` внутри класса — в куче, внутри объекта этого класса;
- элементы массива `int[]` — в куче, внутри массива;
- локальная переменная, захваченная лямбдой, переносится компилятором в поле скрытого класса, то есть в кучу.

Правильная модель различия: значимый тип **копируется по значению**, ссылочный **разделяется по ссылке**. Место хранения — деталь реализации, которую JIT может оптимизировать.

## Размер и выравнивание структур

Процессору удобнее читать данные по адресам, кратным их размеру. Поэтому runtime выравнивает поля и добавляет между ними пустые байты (padding):

```csharp
// The int must start at an offset divisible by 4, so 3 bytes of padding follow the byte.
[StructLayout(LayoutKind.Sequential)]
public struct Padded
{
    public byte A;
    public int B;
}

// Pack = 1 removes the padding (useful for binary formats, but unaligned access can be slower).
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Packed
{
    public byte A;
    public int B;
}
```

`Unsafe.SizeOf<Padded>()` = **8**, `Unsafe.SizeOf<Packed>()` = **5**. Переменная ссылочного типа занимает размер указателя: 8 байт в 64-битном процессе. Порядок полей влияет на размер — это [упражнение 4](exercises.ru.md). Код: [Ex08_StructLayout.cs](code/Examples/Ex08_StructLayout.cs).

## Интернирование строк

Строковые литералы хранятся в **пуле интернирования** (intern pool): одинаковые литералы в программе — один и тот же объект.

```csharp
// "hel" + "lo" is a compile-time constant, so both variables point to the same interned literal.
public static bool LiteralsShareInstance()
{
    string a = "hello";
    string b = "hel" + "lo";
    return ReferenceEquals(a, b);
}

// Concatenation at run time creates a new string object.
public static bool RuntimeConcatSharesInstance(string prefix)
{
    string a = "hello";
    string b = prefix + "lo";
    return ReferenceEquals(a, b);
}

// string.Intern returns the instance from the intern pool.
public static bool InternedConcatSharesInstance(string prefix)
{
    string a = "hello";
    string b = string.Intern(prefix + "lo");
    return ReferenceEquals(a, b);
}
```

`LiteralsShareInstance()` → `true` (компилятор сложил константы), `RuntimeConcatSharesInstance("hel")` → `false`, `InternedConcatSharesInstance("hel")` → `true`. Выводы: строки всегда сравнивают через `==` или `string.Equals` (по содержимому), а не через `ReferenceEquals`. `string.Intern` применяют редко: память интернированных строк, скорее всего, не освободится до завершения runtime. Код: [Ex09_Interning.cs](code/Examples/Ex09_Interning.cs). По той же причине строку не используют как объект для `lock`: см. [18 · Многопоточность](../18-concurrency/basics.ru.md).

## `Nullable<T>` и nullable-аннотации изнутри

`int?` и `string?` выглядят похоже, но устроены совершенно по-разному:

- `int?` — настоящий отдельный тип `Nullable<int>`: структура с полями `HasValue` и `Value`.
- `string?` — тот же `string` с **аннотацией** для компилятора. В runtime проверок нет.

```csharp
// Boxing an int? boxes the underlying int, or produces a null reference when there is no value.
public static Type? BoxedTypeOf(int? value)
{
    object? boxed = value;
    return boxed?.GetType();
}

public static int TakesNullable(string? text) => text?.Length ?? 0;

public static int TakesNonNullable(string text) => text.Length;

// string? and string are the same runtime type; the annotation is only metadata for the compiler.
public static Type ParameterType(string methodName) => FirstParameter(methodName).ParameterType;

// The annotation can still be read through reflection.
public static NullabilityState ParameterNullability(string methodName) =>
    new NullabilityInfoContext().Create(FirstParameter(methodName)).ReadState;
```

- `BoxedTypeOf(5)` → `System.Int32`, а не `Nullable<int>`: упаковка `Nullable<T>` упаковывает внутреннее значение. `BoxedTypeOf(null)` → `null`: «пустой» `int?` превращается в null-ссылку.
- `ParameterType` для обоих методов → `System.String`.
- `ParameterNullability` → `Nullable` и `NotNull`: аннотация хранится в метаданных атрибутами, её читают компилятор и библиотеки (например, ASP.NET Core при валидации).

Следствие: NRT защищает только на этапе компиляции. Публичный API библиотеки всё равно должен проверять аргументы (`ArgumentNullException.ThrowIfNull`), потому что вызывающий код мог быть собран без nullable-анализа или передать `null!`. Код: [Ex10_NullableInternals.cs](code/Examples/Ex10_NullableInternals.cs).

## Справочник

### Все числовые типы

| Тип | Размер | Диапазон или точность | Когда нужен |
|---|---|---|---|
| `sbyte`, `byte` | 1 байт | −128…127, 0…255 | байты, бинарные форматы |
| `short`, `ushort` | 2 байта | ±32 тыс., 0…65 535 | компактное хранение |
| `int`, `uint` | 4 байта | ±2,1 млрд, 0…4,29 млрд | по умолчанию для целых |
| `long`, `ulong` | 8 байт | ±9,2·10¹⁸ | время в тиках, большие счётчики |
| `Int128`, `UInt128` | 16 байт | `Int128.MaxValue` = 170141183460469231731687303715884105727 | очень большие целые без `BigInteger` |
| `nint`, `nuint` | 4 или 8 байт | размер указателя: 8 байт в 64-битном процессе | interop, низкоуровневый код |
| `BigInteger` | переменный | без ограничений: `BigInteger.Pow(2, 100)` = 1267650600228229401496703205376 | криптография, точная арифметика |
| `Half` | 2 байта | максимум 65504 | ML, графика, хранение |
| `float` | 4 байта | ~6–9 значащих цифр | графика, большие массивы |
| `double` | 8 байт | ~15–17 значащих цифр | вычисления по умолчанию |
| `decimal` | 16 байт | 28–29 значащих цифр, максимум 79228162514264337593543950335 | деньги |

### Литералы

| Запись | Тип и значение |
|---|---|
| `42`, `42u`, `42L`, `42UL` | `int`, `uint`, `long`, `ulong` |
| `0x1F`, `0b1010` | шестнадцатеричный и двоичный литералы: `0b1010 + 0x1F` = 41 |
| `1_000_000` | разделитель разрядов, на значение не влияет |
| `1.5`, `1.5f`, `1.5m`, `1e3` | `double`, `float`, `decimal`, `double` (1000) |
| `'A'`, `'A'`, `'\n'` | `char` |
| `"…"`, `@"…"`, `"""…"""` | обычная, verbatim и raw-строка |
| `$"…{x}…"`, `$$"""…{{x}}…"""` | интерполяция; в raw-строке с `$$` фигурная скобка `{` остаётся текстом, а `{{x}}` — подстановка |
| `"abc"u8` | UTF-8 литерал, тип `ReadOnlySpan<byte>`: `"abc"u8.Length` = 3, `"привет"u8.Length` = 12 |

Пример raw-строки с `$$`: `$$"""{"value": {{value}}}"""` при `value = 42` даёт `{"value": 42}`.

### Неявные числовые преобразования

| Из | Неявно в |
|---|---|
| `sbyte` | `short`, `int`, `long`, `float`, `double`, `decimal`, `nint` |
| `byte` | `short`, `ushort`, `int`, `uint`, `long`, `ulong`, `float`, `double`, `decimal`, `nint`, `nuint` |
| `short` | `int`, `long`, `float`, `double`, `decimal`, `nint` |
| `ushort` | `int`, `uint`, `long`, `ulong`, `float`, `double`, `decimal`, `nint`, `nuint` |
| `char` | `ushort`, `int`, `uint`, `long`, `ulong`, `float`, `double`, `decimal` |
| `int` | `long`, `float`, `double`, `decimal`, `nint` |
| `uint` | `long`, `ulong`, `float`, `double`, `decimal`, `nuint` |
| `long` | `float`, `double`, `decimal` |
| `ulong` | `float`, `double`, `decimal` |
| `float` | `double` |

Неявно — не значит без потерь точности: `int` → `float` и `long` → `double` могут округлять (см. подвохи). В `decimal` из `float`/`double` и обратно — только явно.

### Приведение, Convert и Parse

| Вызов | Результат |
|---|---|
| `(int)3.5` | `3` — отбрасывание дробной части |
| `Convert.ToInt32(3.5)` / `Convert.ToInt32(2.5)` | `4` / `2` — банковское округление |
| `Convert.ToInt32((string?)null)` | `0` |
| `int.Parse(null)` | `ArgumentNullException` |
| `int.Parse("٣")` (арабско-индийская цифра) | `FormatException` |
| `int.TryParse(…, out var n)` | `false` вместо исключения |

### const, readonly, static readonly

| | `const` | `readonly` | `static readonly` |
|---|---|---|---|
| Когда известно значение | при компиляции | в конструкторе | при инициализации типа |
| Типы | числа, `bool`, `char`, `string`, `enum`, `null` | любые | любые |
| Где хранится | подставляется в код **вызывающей** сборки | поле объекта | статическое поле |
| Смена значения в библиотеке | требует перекомпиляции потребителей | безопасна | безопасна |

Подстановка `const` в место использования — та же ловушка, что и со значениями необязательных параметров (см. [03 · Управляющие конструкции и методы](../03-control-flow-and-methods/expert.ru.md)).

### var, default, dynamic

- `var` требует инициализатора, из которого выводится тип: `var x = null;` — ошибка CS0815. `var` нельзя использовать для полей.
- `default` — значение по умолчанию типа: `0`, `false`, `null`. `default(int?)` — `null`.
- `dynamic` откладывает проверку типов до выполнения. `dynamic d = 1; d.Foo();` компилируется, но бросает `RuntimeBinderException`; `d + "x"` даёт `"1x"`. `dynamic` нужен для COM, динамических языков и разбора нетипизированных данных, а не вместо `var`.

### Кортежи

`ValueTuple` — изменяемая структура с именованными полями: `(int x, int y) = (3, 4);` разбирает кортеж на переменные (`x + y` = 7). Кортежи сравниваются поэлементно оператором `==`: `(1, "a") == (1, "a")` — `true`. Имена элементов существуют только для компилятора, во время выполнения это `Item1`, `Item2`.

### Перечисления

- Базовый тип по умолчанию — `int`. `(DayOfWeek)42` — законное значение, оно печатается как `42`, а `Enum.IsDefined((DayOfWeek)42)` возвращает `false`.
- `[Flags]` — набор битовых флагов: значения 1, 2, 4…; `(Perm.Read | Perm.Write).ToString()` → `Read, Write`, `HasFlag(Perm.Write)` → `true`.

### Строки и Unicode

- `string` хранит кодовые единицы UTF-16. Символы вне базовой плоскости занимают две единицы: `"😀".Length` = 2, а `"😀".EnumerateRunes().Count()` = 1.
- Одна буква может кодироваться по-разному: `"é"` (U+00E9) и `"é"` (e + комбинирующий акцент). Ординальное сравнение `==` даёт `false`, сравнение с культурой (`string.Compare(…, CultureInfo.InvariantCulture, CompareOptions.None)`) — равенство, а после `Normalize()` строки совпадают.

### Атрибуты nullable-анализа

| Атрибут | Смысл |
|---|---|
| `[NotNullWhen(true)]` | при возврате `true` аргумент не `null` (как у `string.IsNullOrEmpty` с `false`) |
| `[MaybeNullWhen(false)]` | при возврате `false` out-значение может быть `null` (`TryGetValue`) |
| `[NotNull]` | после вызова аргумент точно не `null` (`ArgumentNullException.ThrowIfNull`) |
| `[MaybeNull]`, `[AllowNull]`, `[DisallowNull]` | уточнения для обобщённых типов и свойств |
| `[MemberNotNull(nameof(field))]` | метод гарантирует, что поле инициализировано |
| `[NotNullIfNotNull("arg")]` | результат не `null`, если аргумент не `null` |

Директивы `#nullable enable` / `disable` / `restore` включают анализ для части файла. `<WarningsAsErrors>nullable</WarningsAsErrors>` превращает все nullable-предупреждения в ошибки.

### Проверка и приведение типов

| Выражение | Результат для `object o = "text"` |
|---|---|
| `o is string` | `True` |
| `o is string s` | `True`, и `s` уже имеет тип `string`: `s.Length` = 4 |
| `o as string` | `"text"`; для неподходящего типа (`o as Uri`) — `null` |
| `(Uri)o` | `InvalidCastException` |
| `o is null` / `o is not null` | проверка на `null`, которую не сломает перегрузка `==` |
| `typeof(string) == o.GetType()` | `True`: `typeof` — тип, известный при компиляции, `GetType()` — фактический тип объекта |

Для упакованного значения проверка точная: `boxed is int` — `True`, `boxed is long` — `False`. `null is object` — `False`. `sizeof(int)` = 4, `sizeof(char)` = 2. `nameof(localVar)` возвращает имя `"localVar"` как строку и проверяется компилятором.

### Типы, которые разбираются в других модулях

| Вид типов | Где |
|---|---|
| классы, свойства, наследование | [04 · ООП: классы и объекты](../04-oop-basics/overview.ru.md) |
| структуры, записи, интерфейсы | [05 · Интерфейсы, структуры, записи](../05-interfaces-structs-records/overview.ru.md) |
| массивы, коллекции, обобщения | [06 · Коллекции и обобщения](../06-collections-and-generics/overview.ru.md) |
| делегаты и события | [09 · Делегаты, события, лямбды](../09-delegates-events-lambdas/overview.ru.md) |
| union types, closed-иерархии (C# 15) | [11 · Современный C#](../11-modern-csharp/overview.ru.md) |
| `Span<T>`, `ref struct`, указатели | `Span<T>` и `ref struct` — [12a · Span и Memory](../12a-span-and-memory/overview.ru.md); указатели — [12 · Память и GC](../12-memory-and-gc/expert.ru.md#закрепление-объектов) |

## Подвохи

### float «точнее» double?

```csharp
0.1f + 0.2f == 0.3f   // True
0.1 + 0.2 == 0.3      // False
```

`float` не точнее: его сумма просто округляется до того же `float`, что и `0.3f`. Ошибка представления есть в обоих типах, сравнивать их через `==` нельзя.

### NaN и отрицательный ноль

```csharp
double.NaN == double.NaN        // False
double.NaN.Equals(double.NaN)   // True
-0.0 == 0.0                     // True
1 / -0.0                        // -Infinity
```

`==` следует IEEE 754 (NaN не равен ничему), а `Equals` считает NaN равным себе, иначе NaN нельзя было бы найти в словаре. Проверка — `double.IsNaN(x)`.

### Convert округляет, приведение отбрасывает

`(int)3.5` = 3, `Convert.ToInt32(3.5)` = 4, `Convert.ToInt32(2.5)` = 2: `Convert` использует банковское округление.

### Convert.ToInt32(null) = 0

`Convert.ToInt32((string?)null)` возвращает `0`, а `int.Parse(null)` бросает `ArgumentNullException`. Пустой ввод молча превращается в ноль.

### Unicode-цифры

`char.IsDigit('٣')` (арабско-индийская тройка) — `true`, но `int.Parse("٣")` бросает `FormatException`. Для проверки ASCII-цифр — `char.IsAsciiDigit`.

### Сравнения с null в Nullable

```csharp
int? n = null;
n + 1     // null
n > 0     // False
n <= 0    // False
```

Арифметика с `null` даёт `null`, а любое сравнение `<`, `>`, `<=`, `>=` с `null` — `false`. Поэтому `!(n > 0)` не означает `n <= 0`.

### Беззнаковое вычитание и переполнение byte

`5u - 6u` = 4294967295, а `byte b = 255; b++;` даёт `0`. Без `checked` переполнение и для беззнаковых типов молча заворачивается.

### Деление отрицательных чисел

`-7 / 2` = `-3`, `-7 % 2` = `-1`: целочисленное деление округляет к нулю, а знак остатка совпадает со знаком делимого. Для проверки нечётности используют `n % 2 != 0`, а не `n % 2 == 1`.

### decimal тоже не бесконечно точен

`1m / 3m * 3m` = `0.9999999999999999999999999999`. `decimal` точен для десятичных дробей вроде 0.1, но не для 1/3.

### Math.Round(2.675, 2) = 2.67

Число 2.675 в `double` хранится чуть меньше настоящего 2.675, поэтому округляется вниз. Деньги округляют в `decimal`: `Math.Round(2.675m, 2)`.

### Неявные преобразования теряют точность

`(int)(float)16777217` = `16777216`, а `(double)9007199254740993L == 9007199254740992.0` — `true`. У `float` 24 бита мантиссы, у `double` — 53, и большие целые в них не помещаются.

### null в конкатенации

```csharp
string? s = null;
s += "a";   // "a"
```

Конкатенация считает `null` пустой строкой и не бросает исключение.

## Компромиссы: struct или class

| | struct | class |
|---|---|---|
| Присваивание | копия значения | копия ссылки |
| Аллокация в куче | нет (если не упакован и не поле класса) | всегда |
| Может быть `null` | только `Nullable<T>` | да |
| Наследование | нет | да |
| Когда выбирать | маленькие неизменяемые значения: координаты, деньги, идентификаторы | сущности с идентичностью, большие объекты, иерархии |

Подробнее о структурах и записях — в [05 · Интерфейсы, структуры, записи](../05-interfaces-structs-records/overview.ru.md), о памяти и GC — в [12 · Память и GC](../12-memory-and-gc/overview.ru.md). В C# 15 появился ещё один вид типов — union types: значение такого типа является одним из нескольких типов-вариантов (case types). Они разбираются в модуле [11 · Современный C#](../11-modern-csharp/overview.ru.md).

## Вопросы с собеседований

**1. Чем значимые типы отличаются от ссылочных? Верно ли, что значимые типы всегда в стеке?**

<details><summary>Ответ</summary>

Значимый тип копируется по значению, ссылочный разделяется по ссылке. Нет: значение живёт там, где объявлена переменная — поле структуры внутри класса лежит в куче, захваченная лямбдой локальная переменная — в поле скрытого класса.

</details>

**2. Что такое boxing и чем он плох?**

<details><summary>Ответ</summary>

Приведение значимого типа к `object` или интерфейсу копирует значение в новый объект в куче. Это аллокация и нагрузка на GC. Две упаковки одного значения — разные объекты, поэтому `==` для них даёт `false`. Обобщения позволяют избежать упаковки.

</details>

**3. Почему `0.1 + 0.2 != 0.3` для `double`? Когда использовать `decimal`?**

<details><summary>Ответ</summary>

0.1 не представимо точно в двоичной системе. `decimal` хранит десятичные цифры и точен для денег и десятичных дробей. `double` быстрее, у него огромный диапазон, он подходит для измерений и вычислений.

</details>

**4. Что делают `checked` и `unchecked`? Какое поведение по умолчанию?**

<details><summary>Ответ</summary>

По умолчанию целочисленное переполнение не проверяется и молча заворачивается. `checked` превращает переполнение в `OverflowException`. Включить проверку для всего проекта можно свойством `CheckForOverflowUnderflow`.

</details>

**5. Чем `int?` отличается от `string?` на уровне runtime?**

<details><summary>Ответ</summary>

`int?` — отдельный тип `Nullable<int>` (структура с `HasValue`/`Value`). `string?` — тот же `string` с аннотацией для компилятора; проверок во время выполнения нет.

</details>

**6. Что выведет `ReferenceEquals("ab", "a" + "b")` и почему?**

<details><summary>Ответ</summary>

`True`: `"a" + "b"` — константное выражение, компилятор складывает его в литерал, а литералы интернированы. Если хотя бы одна часть известна только во время выполнения, получится новый объект и `False`.

</details>

**7. Почему `ToUpper()` без аргументов опасен для сравнения строк?**

<details><summary>Ответ</summary>

Он зависит от текущей культуры: в турецкой `"i".ToUpper()` — `"İ"`. Для идентификаторов используют `ToUpperInvariant()` или `string.Equals(a, b, StringComparison.OrdinalIgnoreCase)`.

</details>

**8. Чем `const` отличается от `static readonly`?**

<details><summary>Ответ</summary>

`const` известна при компиляции и подставляется в код вызывающей сборки — смена значения в библиотеке не дойдёт до потребителей без перекомпиляции. `static readonly` вычисляется при выполнении, может быть любого типа и читается из поля.

</details>

**9. Чем `dynamic` отличается от `object` и `var`?**

<details><summary>Ответ</summary>

`var` — статический тип, выведенный компилятором. `object` — статический тип, для вызова членов нужен каст. `dynamic` откладывает разрешение членов до выполнения: ошибки превращаются из ошибок компиляции в `RuntimeBinderException`.

</details>

**10. 🪤 `int? n = null;` Чему равны `n > 0` и `n <= 0`?**

<details><summary>Ответ</summary>

Оба `false`. Сравнение «поднятых» (lifted) операторов с `null` всегда даёт `false`, поэтому отрицание одного сравнения не равно другому.

</details>

**11. 🪤 Чему равно `"😀".Length`?**

<details><summary>Ответ</summary>

2. `string` хранит кодовые единицы UTF-16, а этот символ занимает суррогатную пару. Количество символов Unicode считают через `EnumerateRunes()` (здесь 1), а графемы — через `StringInfo`.

</details>

**12. 🪤 Чем `Convert.ToInt32(x)` отличается от `(int)x` для `double`?**

<details><summary>Ответ</summary>

Приведение отбрасывает дробную часть, `Convert` округляет банковским способом: `(int)3.5` = 3, `Convert.ToInt32(3.5)` = 4, `Convert.ToInt32(2.5)` = 2. Кроме того, `Convert.ToInt32((string?)null)` возвращает 0, а не бросает исключение.

</details>

**13. 🪤 Почему `double.NaN == double.NaN` — `false`, а `double.NaN.Equals(double.NaN)` — `true`?**

<details><summary>Ответ</summary>

`==` реализует IEEE 754, где NaN не равен ничему. `Equals` должен быть рефлексивным (`x.Equals(x)`), иначе NaN нельзя было бы использовать как ключ словаря или найти в коллекции.

</details>

**14. Зачем атрибут `[NotNullWhen(true)]`?**

<details><summary>Ответ</summary>

Он сообщает анализатору nullable, что при возврате `true` аргумент не `null`. Благодаря таким атрибутам после `if (dict.TryGetValue(key, out var v))` или `if (!string.IsNullOrEmpty(s))` компилятор не выдаёт предупреждений.

</details>

## Шпаргалка

| Задача | Как |
|---|---|
| Проверить на null | `x is null`, `x is not null` |
| Значение по умолчанию для null | `x ?? fallback`, `x ??= fallback` |
| Безопасный вызов | `x?.Member` |
| Разобрать число из строки | `int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n)` |
| Включить проверку переполнения | `checked(a + b)` |
| Округлить «по-школьному» | `Math.Round(x, MidpointRounding.AwayFromZero)` |
| Сравнить строки без учёта регистра | `string.Equals(a, b, StringComparison.OrdinalIgnoreCase)` |
| Деньги | `decimal`, литерал `10.5m` |
| Сравнить `double` | `Math.Abs(a - b) <= tolerance` |
| Многострочный текст без экранирования | `"""…"""` |

## Связанные материалы

- Пререквизиты: [01 · Платформа .NET и dotnet CLI](../01-dotnet-platform/overview.ru.md) — проект, `<Nullable>enable</Nullable>`, сборка
- Дальше: [03 · Управляющие конструкции и методы](../03-control-flow-and-methods/overview.ru.md) — `ref`, `out`, `in` и передача значимых типов в методы
- Куча, GC и `Dispose`: [12 · Память и GC](../12-memory-and-gc/overview.ru.md)
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
- [Целочисленные типы](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/integral-numeric-types)
- [Встроенные числовые преобразования](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/numeric-conversions)
- [`Convert.ToInt32`](https://learn.microsoft.com/dotnet/api/system.convert.toint32)
- [Тип dynamic](https://learn.microsoft.com/dotnet/csharp/advanced-topics/interop/using-type-dynamic)
- [Кортежи](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/value-tuples)
- [Перечисления](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/enum)
- [Кодировка символов в .NET](https://learn.microsoft.com/dotnet/standard/base-types/character-encoding-introduction)
- [Атрибуты nullable-анализа](https://learn.microsoft.com/dotnet/csharp/language-reference/attributes/nullable-analysis)
