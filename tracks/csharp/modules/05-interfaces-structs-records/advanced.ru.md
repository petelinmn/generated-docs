---
title: "Интерфейсы, структуры, записи: продвинутая теория"
lang: ru
track: csharp
kind: module
doc: advanced
slug: 05-interfaces-structs-records
part: 1
tags: [explicit-interface-implementation, default-interface-methods, static-abstract-members, iequatable, gethashcode, mutable-struct, defensive-copy, boxing, record-inheritance, readonly-struct]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Интерфейсы, структуры, записи: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Реализация по умолчанию и явная реализация

**Реализация по умолчанию** (C# 8) позволяет добавить в интерфейс метод с телом, не ломая существующие классы:

```csharp
interface IShape
{
    string Name { get; }
    string Describe() => $"shape {Name}";   // default implementation
}

class Sq : IShape { public string Name => "sq"; }

IShape s = new Sq();
Console.WriteLine(s.Describe());             // shape sq
// new Sq().Describe();  // error CS1061: Sq has no member Describe
```

Метод по умолчанию виден **только через переменную интерфейсного типа**: у класса `Sq` такого члена нет (проверено: `typeof(Sq).GetMethod("Describe")` — `null`, а вызов `new Sq().Describe()` не компилируется). Реализации по умолчанию нужны, чтобы развивать опубликованные интерфейсы; для обычного повторного использования кода лучше абстрактный класс или метод расширения.

**Явная реализация** привязывает член к конкретному интерфейсу. Она нужна, когда два интерфейса требуют метод с одинаковой сигнатурой, но разным смыслом, либо когда член не должен быть виден у самого класса:

```csharp
interface IA { string Run(); }
interface IB { string Run(); }

class Both : IA, IB
{
    string IA.Run() => "A";
    string IB.Run() => "B";
    public string Run() => "class";
}

var both = new Both();
Console.WriteLine($"{((IA)both).Run()} {((IB)both).Run()} {both.Run()}");   // A B class
```

Явно реализованный член не имеет модификатора доступа и вызывается только через приведение к интерфейсу.

## Статические члены интерфейсов

С C# 11 интерфейс может требовать **статические абстрактные члены**: операторы и фабричные методы. Они позволяют писать обобщённый код над числами и другими типами с общим набором операций (generic math):

```csharp
interface IAdd<T> where T : IAdd<T>
{
    static abstract T Add(T a, T b);
}

record struct MyNum(int V) : IAdd<MyNum>
{
    public static MyNum Add(MyNum a, MyNum b) => new(a.V + b.V);
}

static T Sum<T>(T a, T b) where T : IAdd<T> => T.Add(a, b);
// Sum(new MyNum(2), new MyNum(3)).V → 5
```

Вызов `T.Add` разрешается через параметр типа, а не через экземпляр. Встроенные числовые интерфейсы (`INumber<T>`, `IAdditionOperators<…>`) устроены так же; подробнее об ограничениях параметров типа — в [06 · Коллекции и обобщения](../06-collections-and-generics/overview.ru.md).

## Равенство и GetHashCode

Чтобы объекты класса сравнивались по значению, нужно согласованно реализовать три вещи:

```csharp
class Eq : IEquatable<Eq>
{
    public int V;
    public Eq(int v) { V = v; }

    public bool Equals(Eq? other) => other is not null && other.V == V;     // typed, no boxing
    public override bool Equals(object? obj) => Equals(obj as Eq);          // for non-generic code
    public override int GetHashCode() => V;                                 // consistent with Equals
}
```

Контракт:

- равные объекты **обязаны** иметь равные хеш-коды (обратное неверно: коллизии допустимы);
- хеш-код не должен меняться, пока объект лежит в коллекции на основе хеша;
- `Equals` рефлексивен, симметричен и транзитивен, а `x.Equals(null)` — `false`.

`IEquatable<T>.Equals` типизирован: коллекции `Dictionary` и `HashSet` вызывают именно его (проверено: после `ContainsKey` и `List.Contains` счётчик `IEquatable` — 2, а `object.Equals` — 0), поэтому для структур он избавляет от упаковки.

### Подвох: изменяемый ключ

```csharp
var k = new Key(1);
var set = new HashSet<Key> { k };
k.Id = 2;                       // the hash code changed while the key is inside
Console.WriteLine(set.Contains(k));   // False
Console.WriteLine(set.Count);         // 1 — the element is still there, but unreachable
```

Элемент лежит в корзине для хеша `1`, а ищется в корзине для хеша `2`: он потерян, но не удалён. Ключи коллекций должны быть неизменяемыми — поэтому для них хороши записи.

### Подвох: Equals без GetHashCode

Если переопределить только `Equals`, объекты окажутся равными, но с разными хеш-кодами, и `HashSet` их не найдёт: проверка на тестовом типе показала `Contains(new NoHash(1))` → `False` при равных значениях. Компилятор предупреждает об этом (CS0659).

## Изменяемые структуры

Изменяемые структуры — источник трудных ошибок, потому что присваивание копирует значение.

```csharp
var list = new List<Pt> { new(1, 2) };
// list[0].X = 5;           // error CS1612: the indexer returns a copy
var copy = list[0];
copy.X = 5;                 // changes the copy only: list[0].X is still 1
list[0] = copy;             // the right way: replace the element

var arr = new[] { new Pt(1, 2) };
arr[0].X = 7;               // works: an array element is a variable, not a copy
```

Индексатор списка возвращает копию, поэтому изменение на месте запрещено (CS1612), а для массива — разрешено.

Вторая ловушка — **защитные копии** (defensive copies). Если вызвать изменяющий метод структуры через поле `readonly`, компилятор сделает копию, и изменение теряется:

```csharp
struct Inner { public int N; public void Inc() => N++; }
class Holder { public readonly Inner Inner = new(); }

var h = new Holder();
h.Inner.Inc(); h.Inner.Inc();
Console.WriteLine(h.Inner.N);       // 0 — each call worked on a copy
```

Ещё одна — **упаковка через интерфейс**. Приведение структуры к интерфейсу создаёт упакованную копию, а вызовы через интерфейс изменяют её:

```csharp
interface IInc { void Inc(); }
struct Counter : IInc { public int N; public void Inc() => N++; }

IInc inc = new Counter();   // boxed once
inc.Inc(); inc.Inc();
Console.WriteLine(((Counter)inc).N);    // 2 — the box was changed
```

Результат правилен только пока вы работаете с одним и тем же упакованным объектом. Решение для всех трёх ловушек: делайте структуры **неизменяемыми** — `readonly struct` или `readonly record struct`:

```csharp
public readonly struct Money(decimal amount, string currency)
{
    public decimal Amount { get; } = amount;
    public string Currency { get; } = currency;
    public Money Add(decimal x) => new(Amount + x, Currency);   // returns a new value
}
```

Компилятор не позволит изменить поле в `readonly struct`, а защитные копии для неё не нужны (метаданные помечены `IsReadOnlyAttribute` — проверено).

## Размер и раскладка структур

Размер структуры зависит от выравнивания полей:

| Структура | Поля | Размер |
|---|---|---|
| `Pt` | `int`, `int` | 8 |
| `Mixed` | `byte`, `int`, `byte` | 12 |
| `Packed` | `int`, `byte`, `byte` | 8 |
| `Empty` | нет полей | 1 |

Измерено через `Unsafe.SizeOf<T>()`. Порядок полей меняет размер: структура `Mixed` с «дырами» на 4 байта больше `Packed` с теми же полями. Пустая структура занимает 1 байт. Большие структуры дорого копируются при передаче и возврате: параметры `in` и возврат `ref readonly` разобраны в [03 · Управляющие конструкции и методы](../03-control-flow-and-methods/expert.ru.md).

## Записи глубже

### Копия `with` поверхностна

```csharp
record Team(string Name, List<string> Members);

var t1 = new Team("A", new List<string> { "x" });
var t3 = t1 with { Name = "B" };
t3.Members.Add("y");
Console.WriteLine(string.Join(",", t1.Members));    // x,y — the same list
```

`with` копирует значения свойств, но вложенный список остаётся общим: запись «неизменяема» только на один уровень.

### Равенство записей с коллекциями

Две записи `Team` с равными по содержимому, но разными списками не равны (`t1 == t2` → `False`): равенство сравнивает свойства их собственным равенством, а `List<T>` сравнивается по ссылке. Чтобы получить равенство по значению, храните неизменяемые коллекции с подходящим сравнением или переопределите `Equals`.

### Наследование и равенство

```csharp
record Person(string Name, int Age);
record Student(string Name, int Age, string School) : Person(Name, Age);

Console.WriteLine(new Student("Ann", 30, "MIT") == new Person("Ann", 30));   // False
Console.WriteLine(new Person("Ann", 30).Equals(new Student("Ann", 30, "MIT"))); // False
```

Записи разных типов не равны, даже если общие свойства совпадают: равенство учитывает **точный тип** (`EqualityContract`). Результат симметричен — это важное отличие от ручной реализации равенства в иерархиях классов.

### Изменяемость

- Позиционные свойства `record class` и `readonly record struct` — `init`; `a.Name = "b"` не компилируется (CS8852).
- У `record struct` (без `readonly`) позиционные свойства — **изменяемые**: у сеттера нет модификатора `init` (проверено).
- Свойства, объявленные вручную, могут быть любыми: запись не обязана быть неизменяемой.

## Лучшие практики

- Объявляй интерфейс рядом с кодом, который его использует, и держи его маленьким (один смысл).
- Не добавляй в интерфейс члены «на будущее»: каждый новый член обязаны реализовать все классы.
- Структуры — `readonly`, маленькие; сомневаешься — бери класс или `record`.
- Для DTO, сообщений и ключей словарей — `record` или `readonly record struct`.
- Переопределяешь `Equals` — реализуй `IEquatable<T>` и `GetHashCode`.
- Не используй изменяемые объекты как ключи коллекций.

## Частые ошибки

- ❌ Изменяемая структура в `List<T>` → ✅ `readonly struct` или класс.
- ❌ Структура реализует интерфейс и передаётся как интерфейс в «горячем» коде → ✅ обобщённый метод с ограничением `where T : IFoo` не упаковывает.
- ❌ Равенство в иерархии классов через `GetType() != other.GetType()` вручную → ✅ запись делает это сама.
- ❌ `Equals` без `GetHashCode` → ✅ всегда вместе.
- ❌ Опираться на `with` как на глубокое копирование → ✅ копировать вложенные объекты вручную.

## Сравнение с другими треками

| | C# | Python |
|---|---|---|
| Контракт | `interface` | `typing.Protocol` / `abc.ABC` |
| Данные с равенством | `record` | `@dataclass` |
| Неизменяемые данные | `readonly record struct` | `@dataclass(frozen=True)`, `NamedTuple` |
| Значимый тип | `struct` | нет (все объекты — ссылки) |
| Копия с изменением | `with` | `dataclasses.replace` |

## Связанные материалы

- Раньше: [базовая теория](basics.ru.md)
- Дальше: [экспертная теория](expert.ru.md) — что генерирует компилятор для записей, структуры в памяти, справочник, вопросы с собеседований
- `ref`, `in` и защитные копии: [03 · Управляющие конструкции и методы](../03-control-flow-and-methods/expert.ru.md)
- Упаковка и значимые типы: [02 · Типы и переменные](../02-types-and-variables/advanced.ru.md)
- Коллекции и обобщения: [06 · Коллекции и обобщения](../06-collections-and-generics/overview.ru.md)
- То же в других треках: [Python](../../../python/overview.ru.md)

## Источники

- [Явная реализация интерфейса](https://learn.microsoft.com/dotnet/csharp/programming-guide/interfaces/explicit-interface-implementation)
- [Реализации интерфейса по умолчанию](https://learn.microsoft.com/dotnet/csharp/advanced-topics/interface-implementation/default-interface-methods-versions)
- [Статические абстрактные члены](https://learn.microsoft.com/dotnet/csharp/whats-new/tutorials/static-virtual-interface-members)
- [Как определить равенство значений](https://learn.microsoft.com/dotnet/csharp/programming-guide/statements-expressions-operators/how-to-define-value-equality-for-a-type)
- [Структуры только для чтения](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/struct#readonly-struct)
- [Записи](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/record)
