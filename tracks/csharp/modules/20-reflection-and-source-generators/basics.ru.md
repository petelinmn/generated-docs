---
title: "Рефлексия и генераторы кода: базовая теория"
lang: ru
track: csharp
kind: module
doc: basics
slug: 20-reflection-and-source-generators
part: 3
tags: [reflection, attributes, expression-trees, source-generators, metadata, activator]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Рефлексия и генераторы кода: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [04 · ООП: классы и объекты](../04-oop-basics/overview.ru.md), [06 · Коллекции и обобщения](../06-collections-and-generics/overview.ru.md), [09 · Делегаты, события, лямбды](../09-delegates-events-lambdas/overview.ru.md). **Время:** ~1 ч.

## Зачем это нужно

Обычно типы и члены известны на этапе компиляции. Но иногда нужно работать с ними **динамически**: найти все классы-плагины в сборке, сопоставить свойства двух объектов, прочитать атрибуты для валидации или сериализации. Для этого есть **рефлексия** (reflection) — чтение метаданных типов во время выполнения и обращение к членам по имени. Платой за гибкость становится скорость и потеря проверок компилятора; поэтому там, где можно, часть работы переносят на этап компиляции — **генераторы исходного кода** (source generators).

## Информация о типе

Метаданные получают через объект `Type`: `typeof(Person)`, `obj.GetType()` или `Type.GetType("имя")`. Через него доступны свойства, методы, конструкторы, поля.

```csharp
public class Person(string name, int age)
{
    public string Name { get; set; } = name;
    public int Age { get; } = age;
    public string Greet(string greeting) => $"{greeting}, {Name}";
}

var t = typeof(Person);
foreach (var p in t.GetProperties())
    Console.WriteLine($"prop {p.Name}: {p.PropertyType.Name} canWrite={p.CanWrite}");
// prop Name: String canWrite=True
// prop Age: Int32 canWrite=False
```

Проверено: `GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)` вернул `get_Name`, `set_Name`, `get_Age`, `Greet` — свойство компилируется в пару методов. Параметры конструктора — `name`, `age` (`GetConstructors()[0].GetParameters()`).

## Создание объектов и вызов членов

```csharp
var obj = Activator.CreateInstance(t, "Ann", 30)!;                  // new Person("Ann", 30)
t.GetProperty("Name")!.SetValue(obj, "Bob");
Console.WriteLine(t.GetProperty("Name")!.GetValue(obj));            // Bob
Console.WriteLine(t.GetMethod("Greet")!.Invoke(obj, ["Hi"]));       // Hi, Bob
```

Если вызванный метод бросает исключение, рефлексия оборачивает его в `TargetInvocationException`; настоящее исключение — в `InnerException` (проверено: `TargetInvocationException -> inner InvalidOperationException`).

## Закрытые члены

Флаги `BindingFlags` задают, какие члены искать. `NonPublic` открывает закрытые поля и методы — это обходит инкапсуляцию, поэтому годится для тестов и инфраструктуры, но не для обычного кода:

```csharp
var secret = t.GetField("_secret", BindingFlags.NonPublic | BindingFlags.Instance)!;
Console.WriteLine(secret.GetValue(obj));    // 7
secret.SetValue(obj, 99);                    // changes private state
```

Проверено: закрытое поле прочитано (`7`), изменено (`99`), закрытый метод вызван (`hidden`).

## Атрибуты

Атрибут — метаданные, прикреплённые к типу или члену. Свой атрибут — класс-наследник `Attribute`:

```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
public class LabelAttribute(string text) : Attribute { public string Text { get; } = text; }

[Label("A person")]
public class Person(string name, int age)
{
    [Label("The name")] public string Name { get; set; } = name;
    public int Age { get; } = age;
}

foreach (var p in typeof(Person).GetProperties())
    Console.WriteLine($"{p.Name}: {p.GetCustomAttribute<LabelAttribute>()?.Text ?? "(no label)"}");
// Name: The name
// Age: (no label)
```

Атрибуты читают валидаторы, сериализаторы, ORM, DI-фреймворки: код по метаданным решает, что делать. Сам по себе атрибут ничего не выполняет.

## Обобщённые типы и поиск типов

```csharp
var listType = typeof(List<>).MakeGenericType(typeof(int));        // List<int> built at run time
var list = (System.Collections.IList)Activator.CreateInstance(listType)!;
list.Add(5);                                                        // count = 1
```

Поиск реализаций интерфейса в сборке (плагины):

```csharp
var plugins = Assembly.GetExecutingAssembly().GetTypes()
    .Where(x => typeof(IPlugin).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);
```

Проверено: нашлись `PluginA`, `PluginB`; абстрактный `PluginBase` отфильтрован.

## Деревья выражений

Лямбда, присвоенная типу `Expression<Func<…>>`, превращается не в код, а в **структуру данных** — дерево, которое можно изучать и компилировать:

```csharp
Expression<Func<int, int>> expr = x => x * 2 + 1;
Console.WriteLine(expr);                          // x => ((x * 2) + 1)
Console.WriteLine(expr.Body.NodeType);            // Add
Console.WriteLine(expr.Compile()(5));             // 11
```

Дерево позволяет, например, достать имя свойства: `Expression<Func<Person, string>> sel = p => p.Name;` → `((MemberExpression)sel.Body).Member.Name` вернёт `Name` (проверено). Так LINQ-провайдеры (EF Core) переводят лямбды в SQL ([07 · LINQ](../07-linq/overview.ru.md)).

## Генераторы исходного кода — коротко

**Source generator** — компонент компилятора, который во время сборки анализирует код и **добавляет новые файлы исходников**. В отличие от рефлексии, всё происходит на этапе компиляции: нет затрат во время выполнения, код проверяется компилятором и подходит для Native AOT. Готовые генераторы вы уже встречали: `[LoggerMessage]`, `[GeneratedRegex]`, `JsonSerializerContext`. Подробнее — в [продвинутой теории](advanced.ru.md#генераторы-исходного-кода).

## Ошибки новичка

- ❌ Рефлексия в горячем пути ([19 · Производительность](../19-performance/overview.ru.md)) → ✅ кэшировать `MethodInfo`/делегаты или использовать генератор.
- ❌ Строковые имена членов (`"Name"`) → ✅ `nameof(...)` или дерево выражений: переименование не сломает код молча.
- ❌ Обращаться к закрытым членам в продакшене-коде → ✅ только в тестах и инфраструктуре.
- ❌ Забыть про `TargetInvocationException` → ✅ смотреть `InnerException`.
- ❌ Искать типы по всем сборкам без нужды → ✅ ограничить сборку и кэшировать результат.
- ❌ Ожидать, что атрибут сам что-то делает → ✅ атрибут — данные; логику пишет читающий его код.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — стоимость, кэширование, компиляция выражений, генераторы.
- [09 · Делегаты, события, лямбды](../09-delegates-events-lambdas/overview.ru.md) — делегаты как быстрая альтернатива `Invoke`.
- [07 · LINQ](../07-linq/overview.ru.md) — `Expression<T>` и `IQueryable<T>`.
- [13 · DI, конфигурация, логирование](../13-di-config-logging/overview.ru.md) — DI использует рефлексию для конструкторов.
- [Python](../../../python/overview.ru.md) — интроспекция и декораторы.

## Источники

- [Рефлексия в .NET](https://learn.microsoft.com/dotnet/fundamentals/reflection/overview)
- [Атрибуты](https://learn.microsoft.com/dotnet/csharp/advanced-topics/reflection-and-attributes/)
- [Деревья выражений](https://learn.microsoft.com/dotnet/csharp/advanced-topics/expression-trees/)
- [Генераторы исходного кода](https://learn.microsoft.com/dotnet/csharp/roslyn-sdk/source-generators-overview)
