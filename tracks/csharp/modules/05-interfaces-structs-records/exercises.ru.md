---
title: "Упражнения: Интерфейсы, структуры, записи"
lang: ru
track: csharp
kind: module
doc: exercises
slug: 05-interfaces-structs-records
part: 1
tags: [interface, record, readonly-struct, equality, static-abstract, explicit-implementation]
status: draft
updated: 2026-11-22
---

# Упражнения: Интерфейсы, структуры, записи

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Решения — под спойлерами. Код проверен запуском; запускаемых версий в `code/` пока нет (уровень 5 не создан).

## 1. 🟢 Скидки через интерфейс

**Задача:** объяви интерфейс `IDiscount` с методом `decimal Apply(decimal price)` и две реализации: `PercentDiscount(decimal percent)` и `FixedDiscount(decimal amount)`. Напиши `ApplyAll(decimal price, IEnumerable<IDiscount> discounts)`, который применяет скидки по очереди; цена не опускается ниже нуля.

**Ожидаемый результат:**

```text
ApplyAll(200, [10%, -5]) → 175
ApplyAll(3, [10%, -5])   → 0
```

<details><summary>Подсказка</summary>

Метод работает с интерфейсом и не знает конкретных типов. Итог ограничь через `Math.Max(price, 0)`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 1: depend on an interface, not on concrete types.
interface IDiscount { decimal Apply(decimal price); }
class PercentDiscount(decimal percent) : IDiscount { public decimal Apply(decimal price) => price * (100 - percent) / 100; }
class FixedDiscount(decimal amount) : IDiscount { public decimal Apply(decimal price) => price - amount; }

static decimal ApplyAll(decimal price, IEnumerable<IDiscount> discounts)
{
    foreach (var d in discounts) price = d.Apply(price);
    return Math.Max(price, 0);
}
```

Проверено: `175` и `0`. Новую скидку можно добавить, не меняя `ApplyAll`.

</details>

## 2. 🟢 Запись-точка

**Задача:** объяви запись `Point(int X, int Y)`. Создай две равные точки, третью — копию первой с `X = 10` через `with`, и деконструируй её в `x`, `y`. Выведи результаты сравнения `==`, `ReferenceEquals` и строку третьей точки.

**Ожидаемый результат:**

```text
True False Point { X = 10, Y = 2 } 10,2
```

<details><summary>Подсказка</summary>

`var p3 = p1 with { X = 10 };` и `var (x, y) = p3;`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 2: positional record — value equality, with-expression, deconstruction.
record Point(int X, int Y);

var p1 = new Point(1, 2); var p2 = new Point(1, 2);
var p3 = p1 with { X = 10 };
var (x, y) = p3;
Console.WriteLine($"{p1 == p2} {ReferenceEquals(p1, p2)} {p3} {x},{y}");
```

Проверено: вывод совпадает. Это два разных объекта с равным значением.

</details>

## 3. 🟡 Неизменяемый вектор

**Задача:** объяви `readonly record struct Vec(int X, int Y)` с оператором `+`. Положи вектор в `List<Vec>` и «измени» X первого элемента: прямое `list[0].X = 5` не компилируется — сделай это через `with`.

**Ожидаемый результат:**

```text
list[0] после замены → Vec { X = 5, Y = 2 }
(1,2) + (3,4)        → Vec { X = 4, Y = 6 }
```

<details><summary>Подсказка</summary>

Элемент списка — копия. Запиши в ячейку новое значение: `list[0] = list[0] with { X = 5 };`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 3: an immutable struct changed by replacing the value.
readonly record struct Vec(int X, int Y)
{
    public static Vec operator +(Vec a, Vec b) => new(a.X + b.X, a.Y + b.Y);
}

var list = new List<Vec> { new(1, 2) };
list[0] = list[0] with { X = 5 };
Console.WriteLine(list[0]);                      // Vec { X = 5, Y = 2 }
Console.WriteLine(new Vec(1, 2) + new Vec(3, 4)); // Vec { X = 4, Y = 6 }
```

Проверено: вывод совпадает. Неизменяемые структуры исключают целый класс ошибок с копиями.

</details>

## 4. 🟡 Потерянный ключ

**Задача:** класс `BadKey` с изменяемым полем `Id` и `Equals`/`GetHashCode` по `Id` кладут в `HashSet`, затем меняют `Id`. Покажи, что `Contains` возвращает `False`, и замени ключ на запись `GoodKey(int Id)` так, чтобы поиск по равному значению находил элемент.

**Ожидаемый результат:**

```text
BadKey: Count = 1, Contains = False
GoodKey: Contains(new GoodKey(1)) = True
```

<details><summary>Подсказка</summary>

Хеш-код вычисляется при добавлении. Ключ должен быть неизменяемым — запись даёт и неизменяемость, и равенство по значению.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 4: mutable keys break hash-based collections; immutable records do not.
class BadKey
{
    public int Id;
    public override bool Equals(object? o) => o is BadKey b && b.Id == Id;
    public override int GetHashCode() => Id;
}
record GoodKey(int Id);

var set = new HashSet<BadKey>();
var k = new BadKey { Id = 1 };
set.Add(k);
k.Id = 2;
Console.WriteLine($"{set.Count} {set.Contains(k)}");   // 1 False

var set2 = new HashSet<GoodKey> { new GoodKey(1) };
Console.WriteLine(set2.Contains(new GoodKey(1)));      // True
```

Проверено: `1 False` и `True`.

</details>

## 5. 🔴 Статические члены интерфейса и явная реализация

**Задача:** (а) объяви `interface IAddable<T> where T : IAddable<T>` со `static abstract T Zero` и `static abstract operator +`; реализуй для `readonly record struct Meters(double Value)` и напиши обобщённый `Total<T>(IEnumerable<T>)`. (б) Объяви `IReader` с методом по умолчанию `Describe()` и `IWriter` со свойством `Name`; класс `File2` реализует оба, а `Name` для `IWriter` — явно.

**Ожидаемый результат:**

```text
Total([1.5 m, 2 m]).Value → 3.5
((IReader)new File2()).Describe() → I am reader
((IWriter)new File2()).Name       → writer
```

<details><summary>Подсказка</summary>

Внутри `Total` стартуй с `T.Zero` и складывай через `+`. Явная реализация: `string IWriter.Name => "writer";` — видна только через интерфейс.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 5: static abstract interface members and explicit implementation.
interface IAddable<T> where T : IAddable<T>
{
    static abstract T Zero { get; }
    static abstract T operator +(T a, T b);
}

readonly record struct Meters(double Value) : IAddable<Meters>
{
    public static Meters Zero => new(0);
    public static Meters operator +(Meters a, Meters b) => new(a.Value + b.Value);
}

static T Total<T>(IEnumerable<T> items) where T : IAddable<T>
{
    var sum = T.Zero;
    foreach (var i in items) sum = sum + i;
    return sum;
}

interface IReader { string Name => "reader"; string Describe() => $"I am {Name}"; }
interface IWriter { string Name { get; } }
class File2 : IReader, IWriter { string IWriter.Name => "writer"; }

Console.WriteLine(Total([new Meters(1.5), new Meters(2)]).Value);   // 3.5
IReader r = new File2(); Console.WriteLine(r.Describe());           // I am reader
Console.WriteLine(((IWriter)new File2()).Name);                     // writer
```

Проверено: вывод совпадает (число зависит от культуры вывода: `3.5` или `3,5`). Одно и то же имя `Name` у двух интерфейсов разведено явной реализацией.

</details>
