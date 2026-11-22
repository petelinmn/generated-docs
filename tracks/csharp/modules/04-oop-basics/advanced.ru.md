---
title: "ООП: классы и объекты: продвинутая теория"
lang: ru
track: csharp
kind: module
doc: advanced
slug: 04-oop-basics
part: 1
tags: [initialization-order, virtual-override, method-hiding, abstract-class, sealed, static-constructor, extension-methods, partial-class, indexer, composition]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# ООП: классы и объекты: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Порядок инициализации объекта

При `new Derived()` код выполняется в таком порядке:

1. инициализаторы полей **наследника** (`int f = Init("derived field")`);
2. инициализаторы полей **базового** класса;
3. тело конструктора **базового** класса;
4. тело конструктора **наследника**.

Проверено запуском: журнал показал `derived field | base field | base ctor | derived ctor`. Порядок «наследник раньше базы» для инициализаторов полей — особенность C#; в Java или C++ инициализация идёт сверху вниз. Следствие: из конструктора базового класса можно вызвать виртуальный метод, и он выполнится в наследнике **до** тела его конструктора:

```csharp
class A { public A() { Hook(); } protected virtual void Hook() { } }

class C : A
{
    string name;                      // assigned in the constructor body
    public string Seen = "-";
    public C() { name = "set"; }
    protected override void Hook() { Seen = name is null ? "null" : name; }
}
// new C().Seen → "null"
```

Поле, которое наследник задаёт в **теле** конструктора, в этот момент ещё не присвоено (`null`). Если бы оно получало значение в инициализаторе (`string name = "set";`), метод увидел бы `set` — это тоже проверено. Вывод: не вызывай виртуальные методы из конструкторов.

## virtual, override и new

| Ключевое слово | Что делает |
|---|---|
| `virtual` | разрешает наследникам заменить метод |
| `override` | заменяет виртуальный метод базового класса |
| `abstract` | метод без тела, наследник обязан его реализовать |
| `sealed override` | заменяет метод и запрещает дальнейшее переопределение |
| `new` | **скрывает** член базового класса, но не заменяет его |

Разница между `override` и `new` — в том, что выбирается при вызове через переменную базового типа:

```csharp
class Animal
{
    public virtual string Speak() => "...";
    public string Name() => "Animal";
}

class Dog : Animal
{
    public override string Speak() => "Woof";
    public new string Name() => "Dog";
}

Animal a = new Dog();
Console.WriteLine(a.Speak());          // Woof   — override, decided by the actual type
Console.WriteLine(a.Name());           // Animal — new, decided by the variable type
Console.WriteLine(((Dog)a).Name());    // Dog
```

Сокрытие через `new` — почти всегда признак ошибки проектирования: одно и то же имя ведёт себя по-разному в зависимости от типа переменной. Без `new` компилятор выдаёт предупреждение CS0108.

### Перегрузка и методы наследника

Правило разрешения перегрузок неочевидно: если у наследника есть **подходящий** метод, компилятор не смотрит на методы базового класса:

```csharp
class PrinterBase { public void Print(int x) => Console.WriteLine("int in base"); }
class Printer : PrinterBase { public void Print(double x) => Console.WriteLine("double in derived"); }

new Printer().Print(5);     // double in derived
```

Хотя `Print(int)` подходит точнее, выполнился метод наследника: `5` неявно приводится к `double`, а методы, объявленные в более производном типе, имеют приоритет. Перегрузки в иерархиях лучше не смешивать. Правила перегрузки в целом — в [03 · Управляющие конструкции и методы](../03-control-flow-and-methods/advanced.ru.md).

## Абстрактные классы и запечатывание

- `abstract class` задаёт общую часть и обязательные члены. Может содержать поля, конструкторы и реализованные методы — в отличие от интерфейса (см. [05 · Интерфейсы, структуры, записи](../05-interfaces-structs-records/overview.ru.md)).
- `sealed class` нельзя наследовать. Запечатывание полезно, когда класс не рассчитан на наследование: поведение проще сопровождать.
- Класс по умолчанию не запечатан: если не хочешь, чтобы от него наследовали, пометь его `sealed`.

## Свойства: тонкости

**Ключевое слово `field`** (C# 14) позволяет добавить логику в автосвойство без объявления собственного поля:

```csharp
public class Temp
{
    public double Celsius
    {
        get => field;
        set => field = Math.Max(value, -273.15);   // clamp to absolute zero
    }
}
// new Temp { Celsius = -300 }.Celsius → -273.15
```

Компилятор по-прежнему создаёт скрытое поле `<Celsius>k__BackingField` (проверено). Если в вашем классе уже есть член с именем `field`, обращайтесь к нему как `@field` или `this.field`.

Ещё варианты:

| Форма | Когда |
|---|---|
| `{ get; }` | значение задаётся только в конструкторе или инициализаторе |
| `{ get; init; }` | значение задаётся при создании объекта |
| `{ get; private set; }` | меняется только изнутри |
| `required` | вызывающая сторона обязана задать значение |
| `=> expr` | вычисляемое, без поля |

**Индексатор** позволяет обращаться к объекту как к массиву:

```csharp
public class Grid
{
    private readonly int[,] _cells = new int[3, 3];
    public int this[int row, int col]
    {
        get => _cells[row, col];
        set => _cells[row, col] = value;
    }
}
// var g = new Grid(); g[1, 2] = 7; g[1, 2] → 7
```

## static, const и readonly

| | Когда задаётся | Можно ли менять |
|---|---|---|
| `const` | при компиляции (значение подставляется в код) | нет; только простые типы и строки |
| `readonly` | в инициализаторе или конструкторе | нет, после конструктора |
| `static readonly` | в статическом инициализаторе или статическом конструкторе | нет |

`const` неявно статическая: в метаданных константа помечена как `static` и `literal` (проверено). Поскольку значение `const` подставляется в код, сборка, которая использует константу из другой сборки, не увидит нового значения, пока её не перекомпилируют. Для значений, которые могут измениться, берут `static readonly`.

**Статический конструктор** выполняется один раз — перед первым обращением к типу. Порядок для типа со статическим конструктором: сначала инициализаторы статических полей, затем тело статического конструктора (проверено: `field init, static ctor`), и два обращения к типу не запустили их повторно.

## Расширяющие методы и partial

**Метод расширения** добавляет метод к существующему типу без наследования:

```csharp
public class Greeter { public string Hello() => "instance"; }

public static class GreeterExt
{
    public static string Hello(this Greeter g) => "extension";
}
// new Greeter().Hello() → "instance"
```

Результат показывает правило: **метод экземпляра всегда побеждает** метод расширения с той же сигнатурой. Расширение нельзя использовать, чтобы «переопределить» существующий метод. Новые блоки расширений (`extension`) разобраны в 11 · Современный C#.

**`partial class`** разрешает разбить определение класса на несколько файлов, а компилятор их объединит. Применяется для сгенерированного кода (например, форм и генераторов кода): сгенерированная часть в одном файле, ваша — в другом.

## Вложенные типы

Тип, объявленный внутри другого типа, — вложенный. Он по умолчанию `private` и видит закрытые члены внешнего типа. Подходит для деталей реализации. Вложенный класс-наследник в примере проверки показал и ещё одну тонкость: код наследника может обратиться к `protected`-члену через ссылку **своего** типа (`other.v` для `other` типа `D`), но не через ссылку базового типа.

## Композиция вместо наследования

Наследование жёстко связывает наследника с реализацией базового класса: изменение базового класса может сломать всех наследников. Композиция — хранить объект другого типа в поле и делегировать ему вызовы — даёт ту же возможность повторного использования без жёсткой связи:

```csharp
public class Logger { public void Log(string m) => Console.WriteLine(m); }

public class OrderService(Logger logger)       // has a Logger, is not a Logger
{
    public void Place() => logger.Log("order placed");
}
```

Эмпирическое правило: наследование — для отношения «является» и для случаев, когда нужен полиморфизм; в остальных случаях — композиция и интерфейсы.

## Лучшие практики

- Делай классы `sealed`, если наследование не планируется.
- Не вызывай виртуальные методы из конструкторов.
- Делай поля `private`, наружу — свойства; для неизменяемых данных — `init` и `readonly`.
- Проверяй аргументы конструктора: объект должен быть корректным сразу после `new`.
- Переопределяя `Equals`, переопределяй и `GetHashCode` (подробнее — в [05 · Интерфейсы, структуры, записи](../05-interfaces-structs-records/overview.ru.md)).
- Избегай `new`-сокрытия.

## Частые ошибки

- ❌ Виртуальный вызов в конструкторе → ✅ вынести логику в метод `Init`, который вызывают после создания.
- ❌ `new` вместо `override` по ошибке → ✅ предупреждение CS0108 не игнорировать.
- ❌ Смешивать перегрузки в базе и наследнике → ✅ переименовать методы.
- ❌ `public const` для значений, которые могут измениться → ✅ `static readonly`.
- ❌ Глубокие иерархии наследования → ✅ композиция и интерфейсы.
- ❌ Статические изменяемые поля как «глобальные переменные» → ✅ передавать зависимости через конструктор.

## Сравнение с другими треками

| | C# | Python |
|---|---|---|
| Конструктор | `public C()` / первичный | `__init__` |
| Свойства | `{ get; set; }` | `@property` |
| Модификаторы доступа | `private`, `protected`, … (проверяет компилятор) | соглашение: `_name`, `__name` |
| Переопределение | явное `virtual`/`override` | любой метод переопределяется |
| Множественное наследование | нет (для интерфейсов — да) | да |
| Порядок инициализации | поля наследника → база → конструктор | `__init__` вызывает `super().__init__()` вручную |

## Связанные материалы

- Раньше: [базовая теория](basics.ru.md)
- Дальше: [экспертная теория](expert.ru.md) — как устроен вызов виртуальных методов, метаданные свойств, справочник, вопросы с собеседований
- Перегрузка и параметры: [03 · Управляющие конструкции и методы](../03-control-flow-and-methods/advanced.ru.md)
- Интерфейсы, структуры, записи: [05 · Интерфейсы, структуры, записи](../05-interfaces-structs-records/overview.ru.md)
- То же в других треках: [Python](../../../python/overview.ru.md)

## Источники

- [Статические конструкторы](https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/static-constructors)
- [Методы расширения](https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/extension-methods)
- [Индексаторы](https://learn.microsoft.com/dotnet/csharp/programming-guide/indexers/)
- [Ключевые слова virtual, override и new](https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/knowing-when-to-use-override-and-new-keywords)
- [Разрешение перегрузки](https://learn.microsoft.com/dotnet/csharp/language-reference/language-specification/expressions#12122-overload-resolution)
- [Ключевое слово field](https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/field)
- [Partial-классы](https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/partial-classes-and-methods)
