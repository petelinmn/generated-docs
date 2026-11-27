---
title: "Упражнения: ООП: классы и объекты"
lang: ru
track: csharp
kind: module
doc: exercises
slug: 04-oop-basics
part: 1
tags: [class, encapsulation, polymorphism, abstract-class, virtual-override, equality]
status: verified
updated: 2026-11-27
---

# Упражнения: ООП: классы и объекты

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Решения — под спойлерами. Код проверен запуском; запускаемых версий в `code/` пока нет (уровень 5 не создан).

## 1. 🟢 Банковский счёт

**Задача:** напиши класс `BankAccount(string owner, decimal initial)` с свойствами `Owner`, `Balance` (только чтение) и методами `Deposit`, `Withdraw`. Сумма должна быть положительной (`ArgumentOutOfRangeException`), снять больше остатка нельзя (`InvalidOperationException`). Баланс нельзя изменить снаружи иначе, чем через методы.

**Ожидаемый результат:**

```text
Deposit(50), Withdraw(30) при initial = 100 → Balance = 120
Withdraw(1000)                              → InvalidOperationException "Insufficient funds."
Deposit(-1)                                 → ArgumentOutOfRangeException (ParamName = "amount")
```

<details><summary>Подсказка</summary>

Поле `_balance` — `private`, свойство `Balance => _balance`. Для проверок есть `ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount)`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 1: encapsulation — the balance changes only through validated methods.
public class BankAccount
{
    private decimal _balance;
    public string Owner { get; }
    public decimal Balance => _balance;

    public BankAccount(string owner, decimal initial)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(owner);
        ArgumentOutOfRangeException.ThrowIfNegative(initial);
        Owner = owner;
        _balance = initial;
    }

    public void Deposit(decimal amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
        _balance += amount;
    }

    public void Withdraw(decimal amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
        if (amount > _balance) throw new InvalidOperationException("Insufficient funds.");
        _balance -= amount;
    }
}
```

Проверено: результаты совпадают с таблицей. Конструктор не пропускает некорректное состояние: объект корректен с самого начала.

</details>

## 2. 🟢 Фигуры

**Задача:** напиши абстрактный класс `Shape` с методом `abstract double Area()` и переопределением `ToString`, которое выводит имя типа и площадь с двумя знаками (инвариантная культура). Наследники: `Circle(double r)` и `Rectangle(double w, double h)`. Посчитай сумму площадей массива фигур.

**Ожидаемый результат:**

```text
Circle(1) и Rectangle(2, 3) → сумма 9.14
Circle area=3.14
Rectangle area=6.00
```

<details><summary>Подсказка</summary>

`Shape[] shapes = [new Circle(1), new Rectangle(2, 3)];` и `shapes.Sum(s => s.Area())`. Для формата без привязки к культуре — `FormattableString.Invariant($"…")`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 2: an abstract base class and polymorphic calls.
public abstract class Shape
{
    public abstract double Area();
    public override string ToString() => FormattableString.Invariant($"{GetType().Name} area={Area():F2}");
}

public class Circle(double r) : Shape { public override double Area() => Math.PI * r * r; }
public class Rectangle(double w, double h) : Shape { public override double Area() => w * h; }
```

Проверено: сумма `9.14`, строки как в таблице. Код суммирования ничего не знает о конкретных типах — это и есть полиморфизм.

</details>

## 3. 🟡 Порядок инициализации

**Задача:** напиши классы `Base` и `Derived : Base` с инициализаторами полей и конструкторами, которые записывают в общий список `Trace.Log` сообщения `base field`, `base ctor`, `derived field`, `derived ctor`. Предскажи порядок, затем проверь.

**Ожидаемый результат:**

```text
derived field | base field | base ctor | derived ctor
```

<details><summary>Подсказка</summary>

Инициализатор поля: `int f = Trace.Add("base field");`, где `Add` записывает строку и возвращает `0`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 3: the order of initialization in an inheritance chain.
new Derived();
Console.WriteLine(string.Join(" | ", Trace.Log));

static class Trace
{
    public static readonly List<string> Log = new();
    public static int Add(string m) { Log.Add(m); return 0; }
}

class Base
{
    int f = Trace.Add("base field");
    public Base() { Trace.Add("base ctor"); }
}

class Derived : Base
{
    int g = Trace.Add("derived field");
    public Derived() { Trace.Add("derived ctor"); }
}
```

Проверено: `derived field | base field | base ctor | derived ctor`. Инициализаторы полей наследника идут раньше базы — поэтому вызов виртуальных методов из конструктора опасен.

</details>

## 4. 🟡 override вместо new

**Задача:** дан код, где `Dog.Kind()` объявлен с `new`, поэтому `Animal a = new Dog(); a.Kind()` возвращает `"Animal"`. Исправь классы так, чтобы вызов через `Animal` давал `"Dog"`, и объясни разницу.

**Ожидаемый результат:**

```text
До:    a.Kind() → Animal, ((Dog)a).Kind() → Dog
После: a.Kind() → Dog
```

<details><summary>Подсказка</summary>

В базовом классе нужен `virtual`, в наследнике — `override` вместо `new`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 4: replace member hiding with real overriding.
Animal b = new Dog();
Console.WriteLine(b.Kind());   // Dog

public class Animal { public virtual string Kind() => "Animal"; }
public class Dog : Animal { public override string Kind() => "Dog"; }
```

Проверено: `Dog`. `new` скрывает член и выбирается по типу переменной, а `virtual`/`override` — по фактическому типу объекта.

</details>

## 5. 🔴 Значимое равенство

**Задача:** напиши запечатанный класс `Money` со свойствами `required decimal Amount` и `required string Currency` (оба `init`), реализующий равенство по значению: `IEquatable<Money>`, `Equals`, `GetHashCode`, операторы `==` и `!=`, `ToString` (`5 EUR`). Объекты должны работать как ключи `HashSet<Money>`.

**Ожидаемый результат:**

```text
два Money { 5, EUR } → == True, Equals True, одинаковый хеш True, строка "5 EUR"
HashSet<Money> { первый }.Contains(второй) → True
```

<details><summary>Подсказка</summary>

`GetHashCode` должен использовать те же поля, что и `Equals`: `HashCode.Combine(Amount, Currency)`. Оператор `==` сначала проверяет `null`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 5: value equality for a class.
public sealed class Money : IEquatable<Money>
{
    public required decimal Amount { get; init; }
    public required string Currency { get; init; }

    public bool Equals(Money? o) => o is not null && Amount == o.Amount && Currency == o.Currency;
    public override bool Equals(object? o) => Equals(o as Money);
    public override int GetHashCode() => HashCode.Combine(Amount, Currency);
    public static bool operator ==(Money? a, Money? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(Money? a, Money? b) => !(a == b);
    public override string ToString() => $"{Amount} {Currency}";
}
```

Проверено: все проверки из таблицы дали `True`. Короче то же даёт `record` ([05 · Интерфейсы, структуры, записи](../05-interfaces-structs-records/overview.ru.md)); упражнение показывает, что запись делает за вас.

</details>
