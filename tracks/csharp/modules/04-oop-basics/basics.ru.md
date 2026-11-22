---
title: "ООП: классы и объекты: базовая теория"
lang: ru
track: csharp
kind: module
doc: basics
slug: 04-oop-basics
part: 1
tags: [class, object, constructor, property, encapsulation, access-modifiers, static, inheritance, polymorphism, virtual-override]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# ООП: классы и объекты: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [02 · Типы и переменные](../02-types-and-variables/overview.ru.md), [03 · Управляющие конструкции и методы](../03-control-flow-and-methods/overview.ru.md).

## Зачем это нужно

Объектно-ориентированное программирование (ООП) группирует данные и код, который с ними работает, в один тип. **Класс** — чертёж, **объект** (экземпляр) — то, что создано по чертежу оператором `new`. Это даёт три вещи:

- **инкапсуляция**: состояние объекта скрыто, изменить его можно только через методы и свойства, которые защищают правила предметной области;
- **наследование**: новый класс берёт за основу существующий и расширяет его;
- **полиморфизм**: код работает с базовым типом, а поведение определяется фактическим типом объекта.

Класс — **ссылочный тип** (см. [02 · Типы и переменные](../02-types-and-variables/basics.ru.md)): переменная хранит ссылку на объект в управляемой куче, а освобождает объект [сборщик мусора](../12-memory-and-gc/basics.ru.md).

## Класс и объект

```csharp
public class Account
{
    private decimal _balance;               // field: the object's state, hidden from outside

    public string Owner { get; }            // property: controlled access

    public Account(string owner, decimal initial)   // constructor: runs on `new`
    {
        Owner = owner;
        _balance = initial;
    }

    public decimal Balance => _balance;     // read-only property

    public void Deposit(decimal amount)     // method: behavior
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount));
        _balance += amount;
    }
}

var account = new Account("Ann", 100m);
account.Deposit(50m);
Console.WriteLine(account.Balance);         // 150
```

Члены класса: **поля** (данные), **методы** (действия), **свойства** (доступ к данным), **конструкторы** (инициализация), **события** и **индексаторы**. Поле хранит данные, метод выполняет код, свойство выглядит как поле, но на деле является парой методов `get`/`set`.

## Модификаторы доступа

Модификатор определяет, кто видит член или тип:

| Модификатор | Виден |
|---|---|
| `public` | везде |
| `private` | только внутри своего типа (по умолчанию для членов класса) |
| `protected` | в своём типе и в наследниках |
| `internal` | в своей сборке (по умолчанию для типов верхнего уровня) |
| `protected internal` | в сборке **или** в наследниках |
| `private protected` | в наследниках **внутри своей сборки** |

Проверено через рефлексию: поле без модификатора — `private`, класс верхнего уровня без модификатора — `internal` (не `public`). Правило инкапсуляции: делай всё максимально закрытым и открывай только то, что нужно снаружи.

## Поля и свойства

Поле — переменная внутри класса. Публичные поля считаются плохой практикой: через них невозможно проверить значение. Свойство позволяет добавить логику, не меняя код вызывающей стороны:

```csharp
public class Product
{
    public string Name { get; set; } = "";          // auto-property: the compiler creates a hidden field
    public decimal Price { get; private set; }      // set only inside the class
    public string Sku { get; init; } = "";          // set only in a constructor or an object initializer
    public bool IsFree => Price == 0;               // computed property, no field
}
```

- **Автосвойство** (`{ get; set; }`): компилятор сам создаёт скрытое поле (проверено: `<Name>k__BackingField`).
- **`private set`**: менять значение можно только из класса.
- **`init`**: значение задаётся при создании (в конструкторе или инициализаторе объекта) и дальше не меняется.
- **Свойство с телом-выражением** (`=> …`): вычисляется при каждом обращении.

## Конструкторы

Конструктор — метод без возвращаемого типа с именем класса. Если конструкторов нет, компилятор создаёт пустой без параметров. Как только вы объявили свой, пустой больше не создаётся.

```csharp
public class Rectangle
{
    public double Width { get; }
    public double Height { get; }

    public Rectangle(double width, double height)
    {
        Width = width;
        Height = height;
    }

    public Rectangle(double side) : this(side, side) { }   // chaining: calls the other constructor
}
```

`: this(...)` вызывает другой конструктор того же класса, `: base(...)` — конструктор базового.

**Первичный конструктор** (primary constructor, C# 12) записывает параметры прямо в заголовке класса:

```csharp
public class Counter(int start)
{
    public int Next() => start++;      // the parameter is visible in the whole class
}
```

Параметр первичного конструктора — **не поле и не свойство**: компилятор создаёт скрытое поле, только если параметр используется в методах (проверено: в классе `Counter` есть закрытое поле `<start>P`).

**Инициализатор объекта** задаёт свойства сразу после создания, а модификатор `required` заставляет вызывающую сторону их задать:

```csharp
public class Person
{
    public required string Name { get; init; }
    public int Age { get; init; }
}

var ann = new Person { Name = "Ann", Age = 30 };   // without Name — a compile error
```

## Статические члены и this

Член с `static` принадлежит типу, а не экземпляру: он общий для всех объектов и вызывается через имя типа (`Math.Max`). **Статический класс** (`static class`) нельзя создать и наследовать — в нём только статические члены; в метаданных он `abstract` и `sealed` одновременно (проверено).

`this` — ссылка на текущий объект. Нужна, когда имя параметра совпадает с именем члена, при передаче объекта в другой метод и в цепочке конструкторов.

## Наследование

Класс наследует от одного базового класса (множественного наследования классов в C# нет). Наследник получает все его члены, кроме закрытых конструкторов:

```csharp
public class Animal
{
    public string Name { get; set; } = "";
    public virtual string Speak() => "...";
}

public class Dog : Animal
{
    public override string Speak() => "Woof";
}
```

Все классы неявно наследуют от `System.Object`. Это даёт `ToString()`, `Equals()`, `GetHashCode()` и `GetType()`. Реализация `ToString()` по умолчанию возвращает имя типа (`Plain`), а `Equals` по умолчанию сравнивает ссылки: два разных объекта с одинаковым содержимым не равны (проверено).

## Полиморфизм

Метод, объявленный `virtual`, наследник может заменить через `override`. Какой вариант выполнится — решает **фактический тип** объекта, а не тип переменной:

```csharp
Animal animal = new Dog();
Console.WriteLine(animal.Speak());     // Woof — the actual type is Dog
```

Абстрактный класс (`abstract`) нельзя создать, он задаёт заготовку, а его `abstract`-члены наследники обязаны реализовать:

```csharp
public abstract class Shape
{
    public abstract string Name { get; }
    public string Describe() => $"I am {Name}";     // shared behavior
}

public class Circle : Shape
{
    public override string Name => "circle";
}
```

Результат `new Circle().Describe()` — `I am circle`. Модификатор `sealed` запрещает дальнейшее наследование от класса или переопределение метода.

## Проверка и приведение типов

```csharp
object o = "text";
bool a = o is string s && s.Length == 4;    // true; s is available in the true branch
string? b = o as string;                    // null if the type does not match
int c = (int)o;                             // InvalidCastException
```

`is` проверяет тип, `as` возвращает `null` вместо исключения, прямое приведение `(T)x` бросает `InvalidCastException`. Подробнее — в [02 · Типы и переменные](../02-types-and-variables/basics.ru.md).

## Ошибки новичка

- ❌ Публичные поля вместо свойств → ✅ свойства: можно добавить проверку, не ломая вызывающий код.
- ❌ Наследование ради повторного использования кода → ✅ наследуй, когда верно «является» (собака — это животное); иначе используй композицию: поле другого типа.
- ❌ Забыть `override` и получить предупреждение о сокрытии → ✅ подробнее в [продвинутой теории](advanced.ru.md#virtual-override-и-new).
- ❌ Ожидать, что `==` или `Equals` сравнивает содержимое объектов класса → ✅ по умолчанию сравниваются ссылки.
- ❌ Делать всё `public` → ✅ минимально необходимый доступ.
- ❌ Конструктор без проверки аргументов → ✅ объект с самого начала должен быть корректным.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — порядок инициализации, сокрытие `new`, ключевое слово `field`, расширения, `partial`
- Значимые и ссылочные типы: [02 · Типы и переменные](../02-types-and-variables/basics.ru.md)
- Параметры методов и перегрузка: [03 · Управляющие конструкции и методы](../03-control-flow-and-methods/basics.ru.md)
- Интерфейсы, структуры, записи: [05 · Интерфейсы, структуры, записи](../05-interfaces-structs-records/overview.ru.md)
- Освобождение объектов: [12 · Память и GC](../12-memory-and-gc/basics.ru.md)
- То же в других треках: [Python](../../../python/overview.ru.md) — классы, `__init__`, наследование, `@property`

## Источники

- [Классы](https://learn.microsoft.com/dotnet/csharp/fundamentals/types/classes)
- [Модификаторы доступа](https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/access-modifiers)
- [Свойства](https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/properties)
- [Конструкторы экземпляров](https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/instance-constructors)
- [Наследование](https://learn.microsoft.com/dotnet/csharp/fundamentals/object-oriented/inheritance)
- [Полиморфизм](https://learn.microsoft.com/dotnet/csharp/fundamentals/object-oriented/polymorphism)
