---
title: "Современный C#: базовая теория"
lang: ru
track: csharp
kind: module
doc: basics
slug: 11-modern-csharp
part: 2
tags: [pattern-matching, switch-expression, primary-constructor, collection-expressions, required-members, init-only, raw-string-literals]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Современный C#: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [03 · Управляющие конструкции и методы](../03-control-flow-and-methods/overview.ru.md), [04 · ООП: классы и объекты](../04-oop-basics/overview.ru.md), [05 · Интерфейсы, структуры, записи](../05-interfaces-structs-records/overview.ru.md). **Время:** ~1 ч.

## Зачем это нужно

C# развивается ежегодно, и новые возможности убирают шаблонный код: проверки типов и значений, конструкторы, создание коллекций. Код, написанный в стиле C# 7, всё ещё работает, но в современных проектах и документации вы будете встречать именно новые конструкции. Здесь собраны те, что нужны с первого дня.

## Сопоставление с образцом

Образец (pattern) проверяет форму значения и, при совпадении, достаёт из него данные. Оператор `is` принимает образцы:

```csharp
object o = 42;
if (o is int n && n > 0) Console.WriteLine($"positive int {n}");   // type pattern + variable
if (o is not string) Console.WriteLine("not a string");            // negation
string? s = null;
if (s is null) Console.WriteLine("null");                          // constant pattern
```

Основные виды образцов:

| Образец | Пример | Проверяет |
|---|---|---|
| тип | `o is int n` | тип и сразу объявляет переменную |
| константа | `x is 0`, `s is null` | равенство константе |
| реляционный | `x is > 10`, `x is >= 0 and < 10` | сравнение |
| логический | `and`, `or`, `not` | комбинации образцов |
| свойств | `p is { X: 0, Y: 0 }` | значения свойств |
| позиционный | `p is (0, 0)` | деконструкцию (для `record` и кортежей) |
| `var`, `_` | `p is (var x, _)` | захватить значение / игнорировать |

## Выражение switch

`switch`-выражение возвращает значение, а не выполняет операторы. Ветви перебираются сверху вниз, побеждает первая подошедшая:

```csharp
static string Classify(int n) => n switch
{
    < 0 => "negative",
    0 => "zero",
    >= 1 and < 10 => "digit",
    _ => "big",         // discard: matches everything else
};

Console.WriteLine(Classify(-5));   // negative
Console.WriteLine(Classify(7));    // digit
```

Для нескольких значений сразу используют кортеж:

```csharp
static string Fizz(int n) => (n % 3, n % 5) switch
{
    (0, 0) => "FizzBuzz",
    (0, _) => "Fizz",
    (_, 0) => "Buzz",
    _ => n.ToString(),
};
Console.WriteLine(string.Join(" ", Enumerable.Range(1, 15).Select(Fizz)));
// 1 2 Fizz 4 Buzz Fizz 7 8 Fizz Buzz 11 Fizz 13 14 FizzBuzz
```

Если ни одна ветвь не подошла, бросается `SwitchExpressionException` (компилятор предупреждает `CS8509`, если ветви не покрывают все значения). Проверено: `x switch { 0 => 0, 2 => 1 }` для `5` бросает `SwitchExpressionException`.

## Первичные конструкторы

Параметры класса или структуры можно указать прямо в объявлении типа (C# 12). Они доступны во всём теле типа:

```csharp
class Account(string owner, decimal balance)
{
    public string Owner { get; } = owner;                       // copy the parameter into a property
    public void Deposit(decimal amount) => balance += amount;   // the parameter lives on as a hidden field
    public override string ToString() => $"{Owner}: {balance}";
}

var acc = new Account("Ann", 100);
acc.Deposit(50);
Console.WriteLine(acc);   // Ann: 150
```

Параметр первичного конструктора — **не поле и не свойство**: если его использует метод, компилятор создаёт скрытое поле. Для `record` параметры сразу становятся свойствами ([05 · Интерфейсы, структуры, записи](../05-interfaces-structs-records/overview.ru.md)).

## Выражения коллекций

Квадратные скобки создают коллекцию нужного типа; `..` («spread») вставляет элементы другой коллекции (C# 12):

```csharp
int[] a = [1, 2];
List<int> l = [..a, 3];                    // 1, 2, 3
ImmutableArray<int> im = [4, 5];           // using System.Collections.Immutable;
Span<int> sp = [6, 7];
int[] empty = [];
```

Тип создаваемой коллекции определяется контекстом (тем, чему присваивается выражение). Пустая `[]` для массива — это `Array.Empty<int>()` (проверено: ссылки равны).

## required и init

`init`-аксессор разрешает присваивать свойство только при создании объекта; `required` заставляет его задать:

```csharp
var p = new Person { Name = "Bob", Age = 3 };
Console.WriteLine($"{p.Name} {p.Age}");   // Bob 3

class Person
{
    public required string Name { get; init; }   // must be set in the initializer
    public int Age { get; init; }
}
```

Проверено: `p.Name = "x"` после создания — ошибка `CS8852`, а `new Person()` без `Name` — ошибка `CS9035`. Это даёт неизменяемые объекты без громоздких конструкторов.

## Сырые строки и параметры лямбд

Сырой строковый литерал в `"""` не требует экранирования кавычек и обратных слэшей; `$$"""` делает интерполяцию через `{{ }}`, поэтому одиночные `{` — обычные символы:

```csharp
var name = "Bob";
var json = $$"""
    {"name": "{{name}}", "ok": true}
    """;
Console.WriteLine(json);   // {"name": "Bob", "ok": true}
```

Параметры лямбд могут иметь значения по умолчанию (C# 12):

```csharp
var greet = (string s = "dflt") => s;
Console.WriteLine(greet() + " " + greet("x"));   // dflt x
```

## Ошибки новичка

- ❌ Цепочка `if (o is int) { var n = (int)o; … }` → ✅ `if (o is int n)`.
- ❌ Ветвь `_ =>` в начале `switch` → ✅ в конце: ветви проверяются по порядку, недостижимые ветви — ошибка компиляции `CS8510` (проверено).
- ❌ Ожидание, что параметр первичного конструктора — свойство → ✅ явно скопируйте его в свойство.
- ❌ `new List<int>() { 1, 2 }` везде → ✅ `List<int> l = [1, 2];`.
- ❌ Присваивание `init`-свойства после создания → ✅ задавайте в инициализаторе или используйте `with` для записей.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — списочные образцы, `field`, extension members.
- [05 · Интерфейсы, структуры, записи](../05-interfaces-structs-records/overview.ru.md) — записи и деконструкция.
- [06 · Коллекции и обобщения](../06-collections-and-generics/overview.ru.md) — какие типы создаёт `[...]`.
- [03 · Управляющие конструкции и методы](../03-control-flow-and-methods/overview.ru.md) — классический `switch`.
- [Python](../../../python/overview.ru.md) — `match`/`case`: структурное сопоставление.

## Источники

- [Образцы (patterns)](https://learn.microsoft.com/dotnet/csharp/language-reference/operators/patterns)
- [Выражение switch](https://learn.microsoft.com/dotnet/csharp/language-reference/operators/switch-expression)
- [Первичные конструкторы](https://learn.microsoft.com/dotnet/csharp/whats-new/tutorials/primary-constructors)
- [Выражения коллекций](https://learn.microsoft.com/dotnet/csharp/language-reference/operators/collection-expressions)
- [Член `required`](https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/required)
