---
title: "Рефлексия и генераторы кода"
lang: ru
track: csharp
kind: module
doc: overview
slug: 20-reflection-and-source-generators
part: 3
levels: [1, 2, 3, 4]
tags: [reflection, attributes, expression-trees, source-generators, trimming, native-aot]
versions: ".NET 11, C# 15; Microsoft.CodeAnalysis.CSharp 4.14.0"
status: draft
updated: 2026-11-22
---

# Рефлексия и генераторы кода

🌐 **Русский** · [English](overview.en.md)

## Коротко

Рефлексия читает метаданные типов во время выполнения: найти члены, создать объект, вызвать метод по имени, прочитать атрибуты. Деревья выражений представляют лямбды как данные, которые можно изучить и скомпилировать. Source generators (генераторы исходного кода) делают то же заранее — на этапе компиляции добавляют файлы исходников, поэтому не требуют рефлексии при выполнении и совместимы с Native AOT. Рефлексия гибка, но медленнее и несовместима с обрезкой без аннотаций; генераторы быстры, но сложнее в разработке.

## Ключевые идеи

- **`Type` и `BindingFlags`:** `GetProperties`, `GetMethod`, `Activator.CreateInstance`; имена регистрозависимы, закрытые члены — только с `NonPublic`.
- **Атрибуты** — метаданные; читаются `GetCustomAttribute<T>`; сами ничего не делают.
- **Стоимость:** `MethodInfo.Invoke` выделяет память (72 Б в проверенном замере), кэшированный делегат — 0; кэшируйте `MethodInfo` и делегаты.
- **Деревья выражений:** `Expression<Func<…>>` — данные; `Compile()` даёт делегат (недоступно в Native AOT).
- **Source generators:** `IIncrementalGenerator` в проекте `netstandard2.0`, подключается как анализатор; только добавляет файлы; класс должен быть `partial`.
- **Готовые генераторы:** `[LoggerMessage]`, `[GeneratedRegex]`, `JsonSerializerContext`.
- **Trimming и AOT:** рефлексия по строкам ломает обрезку — аннотации (`DynamicallyAccessedMembers`) или генераторы.

## Минимальный пример

```csharp
[AttributeUsage(AttributeTargets.Property)]
public class LabelAttribute(string text) : Attribute { public string Text { get; } = text; }

public class Person(string name, int age)
{
    [Label("Full name")] public string Name { get; set; } = name;
    public int Age { get; set; } = age;
}

foreach (var p in typeof(Person).GetProperties())
    Console.WriteLine(p.GetCustomAttribute<LabelAttribute>()?.Text ?? p.Name);
// Full name
// Age
```

## Когда применять, а когда нет

- ✅ Плагины, сериализаторы, ORM, DI, тестовые фреймворки — рефлексия по метаданным.
- ✅ Генератор: когда набор типов известен при компиляции, а нужны скорость и AOT.
- ✅ Деревья выражений: анализ лямбд (имя свойства, SQL) и быстрые динамические делегаты.
- ❌ Рефлексия в горячем пути без кэша.
- ❌ Обращение к закрытым членам в обычном коде.
- ❌ Рефлексия по строковым именам в приложении с обрезкой/AOT без аннотаций.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | `Type`, создание и вызов, закрытые члены, атрибуты, обобщения, поиск типов, деревья выражений, идея генераторов |
| [2. Продвинутая теория](advanced.ru.md) | стоимость и кэширование, компиляция выражений, плагины, `IIncrementalGenerator` (рабочий пример), готовые генераторы, отладка |
| [3. Экспертная теория](expert.ru.md) | метаданные, правила поиска, исполнение `Invoke`, обрезка и AOT-аннотации, загрузка сборок, устройство генератора, 13 вопросов с собеседований |
| [4. Самопроверка](quiz.ru.md) | 11 вопросов и [5 упражнений](exercises.ru.md) с решениями |
| 5. Запускаемые примеры | ещё не создан — `/learn csharp 20-reflection-and-source-generators level 5` |
| 6. Тесты | ещё не создан — `/learn csharp 20-reflection-and-source-generators level 6` |

## Связанные материалы

- Пререквизиты: [09 · Делегаты, события, лямбды](../09-delegates-events-lambdas/overview.ru.md), [06 · Коллекции и обобщения](../06-collections-and-generics/overview.ru.md)
- Рядом: [19 · Производительность](../19-performance/overview.ru.md), [07 · LINQ](../07-linq/overview.ru.md)
- Дальше: [21 · Архитектура приложений](../21-architecture/overview.ru.md)
- `[LoggerMessage]` и DI: [13 · DI, конфигурация, логирование](../13-di-config-logging/overview.ru.md)
- То же в других треках: [Python](../../../python/overview.ru.md) — интроспекция
- Рецепты и библиотеки: [Dapper](../../libraries/dapper/overview.ru.md), [FluentValidation](../../libraries/fluentvalidation/overview.ru.md), [Mapperly](../../libraries/mapperly/overview.ru.md), [NSubstitute](../../libraries/nsubstitute/overview.ru.md), [Refit](../../libraries/refit/overview.ru.md)
