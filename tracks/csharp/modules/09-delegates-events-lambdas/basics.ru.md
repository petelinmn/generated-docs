---
title: "Делегаты, события, лямбды: базовая теория"
lang: ru
track: csharp
kind: module
doc: basics
slug: 09-delegates-events-lambdas
part: 1
tags: [delegates, lambdas, events, func-action, multicast, closures, event-handler, callbacks]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Делегаты, события, лямбды: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md)

**Пререквизиты:** [03 · Управляющие конструкции и методы](../03-control-flow-and-methods/overview.ru.md), [04 · ООП: классы и объекты](../04-oop-basics/overview.ru.md).

## Зачем это нужно

Иногда методу нужно передать **действие**, а не данные: «отсортируй по этому правилу», «вызови это, когда загрузка завершится», «примени к каждому элементу». Для этого нужны ссылки на методы. В C# их задают три связанные возможности:

- **делегат** (delegate) — тип, описывающий сигнатуру метода; переменная такого типа хранит ссылку на метод;
- **лямбда-выражение** — короткая запись анонимного метода, которую присваивают делегату;
- **событие** (event) — защищённый способ уведомлять подписчиков: объект «публикует» событие, остальные на него «подписываются».

Они лежат в основе [LINQ](../07-linq/overview.ru.md), обработчиков в UI и веб-фреймворках, обратных вызовов (callback) и асинхронного кода.

## Делегаты

Делегат объявляется как метод с ключевым словом `delegate`:

```csharp
delegate int Calc(int a, int b);

static int Multiply(int a, int b) => a * b;

Calc mul = Multiply;                    // a reference to the method
Calc add = (a, b) => a + b;             // a lambda
Console.WriteLine(add(2, 3));           // 5
Console.WriteLine(mul(2, 3));           // 6
Console.WriteLine(add.Invoke(1, 1));    // 2 — the same as add(1, 1)
```

Делегат — **ссылочный тип**, значение которого — метод (а для метода экземпляра ещё и объект, у которого он вызывается). Присвоить можно только метод с совместимой сигнатурой (те же параметры и возвращаемый тип). Проверено: у делегата `mul` свойство `Method.Name` — `Multiply`, а `Target` — `null`, потому что метод статический.

### Готовые типы: Func, Action, Predicate

Собственные делегаты нужны редко: в библиотеке уже есть обобщённые.

| Тип | Сигнатура | Пример |
|---|---|---|
| `Action`, `Action<T1,…>` | возвращает `void` | `Action<string> print = s => Console.WriteLine(s);` |
| `Func<…, TResult>` | возвращает значение; **последний** параметр типа — результат | `Func<int, int> sq = x => x * x;` |
| `Predicate<T>` | возвращает `bool` | `Predicate<int> pos = x => x > 0;` |
| `Comparison<T>` | сравнение двух значений | `Comparison<string> cmp = (a, b) => a.Length - b.Length;` |
| `EventHandler`, `EventHandler<TEventArgs>` | стандартные обработчики событий | см. ниже |

## Лямбда-выражения

Лямбда — анонимная функция: `параметры => тело`. Тело бывает выражением или блоком:

```csharp
Func<int, int> sq = x => x * x;                       // expression body
Func<int, int, int> max = (a, b) => a > b ? a : b;    // several parameters
Func<int, string> describe = n =>                     // statement body
{
    if (n < 0) return "negative";
    return n == 0 ? "zero" : "positive";
};
Action hello = () => Console.WriteLine("hi");         // no parameters
```

Типы параметров обычно выводятся из делегата. С C# 10 у лямбды есть «естественный» тип, поэтому можно писать `var`:

```csharp
var natural = (int x) => x + 1;     // Func<int, int>
```

Проверено: тип `natural` — `Func<int,int>`. С C# 12 допустимы параметры по умолчанию: `(int x, int y = 10) => x + y` — вызов `withDefault(1)` дал `11`.

## Делегаты как параметры

Главное применение — передать поведение методу:

```csharp
static List<int> Filter(List<int> source, Predicate<int> keep)
{
    var result = new List<int>();
    foreach (var x in source)
        if (keep(x)) result.Add(x);
    return result;
}

var evens = Filter([1, 2, 3, 4], x => x % 2 == 0);    // 2, 4
```

Так устроены `List<T>.Sort(Comparison<T>)`, `RemoveAll(Predicate<T>)` и все операторы [LINQ](../07-linq/basics.ru.md) (`Where(x => …)`, `Select(x => …)`).

## Многоадресные делегаты

Делегат может хранить **несколько** методов. Операторы `+=` и `-=` добавляют и убирают их; при вызове они выполняются по порядку:

```csharp
Action act = () => Console.Write("A");
act += () => Console.Write("B");
act += () => Console.Write("C");
act();                                  // ABC
```

Проверено: журнал показал `A B C`. Если у многоадресного делегата есть возвращаемое значение, результатом становится значение **последнего** метода (`3` для `1`, `2`, `3`). Подробности — в [продвинутой теории](advanced.ru.md#многоадресные-делегаты).

## События

**Событие** — делегат, обёрнутый так, что снаружи на него можно только подписаться (`+=`) и отписаться (`-=`), а вызвать его может **только сам класс-издатель**.

```csharp
public class Counter
{
    public event Action? Clicked;                  // the event

    public void Click() => Clicked?.Invoke();      // only the owner raises it
}

var c = new Counter();
c.Clicked += () => Console.WriteLine("clicked");   // subscribe
c.Click();                                         // clicked
```

Если подписчиков нет, `Clicked` равно `null`: поэтому вызов через `?.Invoke()` (проверено: вызов без подписчиков не бросает исключения, а прямой вызов `null`-делегата — `NullReferenceException`).

Снаружи событие нельзя вызвать или присвоить: `c.Clicked();` и `c.Clicked = null;` — ошибка компиляции CS0070. Это и отличает событие от обычного публичного поля-делегата.

### Стандартный шаблон событий

В .NET принят единый вид: обработчик получает **отправителя** и **данные события**.

```csharp
public class ValueEventArgs(int value) : EventArgs
{
    public int Value { get; } = value;
}

public class Publisher
{
    public event EventHandler<ValueEventArgs>? Changed;

    public void Set(int v) => Changed?.Invoke(this, new ValueEventArgs(v));
}

var pub = new Publisher();
pub.Changed += (sender, e) => Console.WriteLine($"changed: {e.Value}");
pub.Set(5);                                        // changed: 5
```

`EventHandler<TEventArgs>` имеет сигнатуру `void (object? sender, TEventArgs e)`. Для события без данных — `EventHandler`.

## Замыкания

Лямбда может использовать **локальные переменные** окружающего метода. Это называется замыканием (closure): лямбда запоминает не значение, а **саму переменную**:

```csharp
int counter = 0;
Action inc = () => counter++;
inc();
inc();
Console.WriteLine(counter);     // 2 — the lambda changed the outer variable
```

Поэтому лямбды, созданные в цикле `for`, видят итоговое значение переменной цикла, а запросы LINQ читают переменные в момент выполнения — подробности в [07 · LINQ](../07-linq/advanced.ru.md#замыкания-переменная-читается-при-выполнении) и в [продвинутой теории](advanced.ru.md#замыкания-и-их-стоимость).

## Ошибки новичка

- ❌ Вызывать делегат или событие без проверки на `null` → ✅ `handler?.Invoke(...)`.
- ❌ Публичное поле-делегат вместо `event` → ✅ `event`: снаружи нельзя вызвать или обнулить.
- ❌ Подписаться на событие долгоживущего объекта и не отписаться → ✅ утечка памяти ([12 · Память и GC](../12-memory-and-gc/advanced.ru.md#утечки-памяти-в-управляемом-коде)): отписывайтесь через `-=`.
- ❌ Пытаться отписаться другой копией той же лямбды → ✅ сохранить делегат в переменную.
- ❌ Ожидать, что замыкание хранит значение → ✅ оно хранит переменную.
- ❌ Писать собственный тип делегата, когда подходит `Func`/`Action` → ✅ готовые типы.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — многоадресные делегаты, замыкания и аллокации, события в деталях, потокобезопасность
- LINQ, построенный на делегатах: [07 · LINQ](../07-linq/overview.ru.md)
- Методы и параметры: [03 · Управляющие конструкции и методы](../03-control-flow-and-methods/overview.ru.md)
- Исключения в обработчиках: [08 · Исключения](../08-exceptions/overview.ru.md)
- То же в других треках: [Python](../../../python/overview.ru.md) — функции как объекты, `lambda`, замыкания

## Источники

- [Делегаты](https://learn.microsoft.com/dotnet/csharp/programming-guide/delegates/)
- [Лямбда-выражения](https://learn.microsoft.com/dotnet/csharp/language-reference/operators/lambda-expressions)
- [События](https://learn.microsoft.com/dotnet/csharp/programming-guide/events/)
- [Func](https://learn.microsoft.com/dotnet/api/system.func-2)
- [Action](https://learn.microsoft.com/dotnet/api/system.action)
- [Стандартные шаблоны событий .NET](https://learn.microsoft.com/dotnet/csharp/event-pattern)
