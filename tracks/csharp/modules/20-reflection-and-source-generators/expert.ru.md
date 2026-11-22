---
title: "Рефлексия и генераторы кода: экспертная теория"
lang: ru
track: csharp
kind: module
doc: expert
slug: 20-reflection-and-source-generators
part: 3
tags: [reflection, metadata, trimming, native-aot, assembly-load-context, roslyn, source-generators, interview-questions]
versions: ".NET 11, C# 15; Microsoft.CodeAnalysis.CSharp 4.14.0"
status: draft
updated: 2026-11-22
---

# Рефлексия и генераторы кода: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [базовая](basics.ru.md) и [продвинутая](advanced.ru.md) теория. **Время:** ~1,5 ч.

## Метаданные и объект Type

Каждая сборка .NET содержит рядом с IL **метаданные**: таблицы типов, методов, полей, свойств, атрибутов, ссылок на другие сборки. Рефлексия читает эти таблицы и строит объекты `Type`, `MethodInfo`, `PropertyInfo`, `FieldInfo`. `Type` для одного типа в домене приложения существует в **единственном экземпляре**: `typeof(Foo) == new Foo().GetType()`, и это одна и та же ссылка (проверено: `ReferenceEquals` → `True`). У члена есть `MetadataToken` (проверено: ненулевой), у типа — `Module` и `Assembly`.

- **Поиск по имени.** Имена регистрозависимы: `GetProperty("name")` для свойства `Name` вернул `null`; с `BindingFlags.IgnoreCase` — нашёл (проверено).
- **Флаги по умолчанию.** `GetProperties()` без флагов возвращает публичные члены экземпляра; закрытые требуют `BindingFlags.NonPublic` (проверено: 1 публичное, 2 вместе с закрытым). Статические члены читают `GetValue(null)` (проверено: `3`).
- **Перегрузки.** `GetMethod("Over")` при двух перегрузках бросает `AmbiguousMatchException`; нужно указать типы параметров (проверено).
- **Создание.** `Activator.CreateInstance(typeof(NoDefault))` без подходящего конструктора — `MissingMethodException` (проверено).
- **Имена типов.** `Type.GetType` принимает имя; типы корневой библиотеки находятся без указания сборки (`System.Collections.Generic.List`1` → `List`1`: проверено), а типы других сборок нужно указывать с именем сборки (`"Foo, m20"`).
- **Nullable.** Упаковка `int?` со значением даёт объект типа `Int32` — `boxed.GetType().Name` равно `Int32`, а `typeof(int?)` — `Nullable`1` (проверено); определить исходный тип поможет `Nullable.GetUnderlyingType`.

## Как выполняется вызов через рефлексию

`MethodInfo.Invoke` принимает массив `object[]`, упаковывает аргументы значимых типов, проверяет их соответствие и вызывает метод; результат упаковывается обратно. Исключение внутри метода оборачивается в `TargetInvocationException` (проверено). В современных версиях runtime для частых вызовов использует сгенерированный IL-код вместо медленной универсальной ветки, но стоимость остаётся выше прямого вызова: выделения `72 Б` на `Invoke` против `0` у делегата (проверено в [продвинутой теории](advanced.ru.md#стоимость-рефлексии-и-ускорение)).

## Деревья выражений

`Expression.Lambda(...).Compile()` генерирует IL (динамический метод) во время выполнения и возвращает делегат; `Compile(preferInterpretation: true)` создаёт интерпретируемый вариант без генерации кода (проверено: оба вернули `7`) — запасной путь для сред без динамической компиляции. Дерево неизменяемо; типовые узлы: `Constant`, `Parameter`, `Property`, `Call`, `Add`, `Assign`, `Block`, `Lambda`. `IQueryable<T>` хранит дерево, которое провайдер переводит в другой язык (SQL).

## Trimming и Native AOT

Обрезка (trimming) удаляет код, до которого не дотягивается анализ вызовов. Рефлексия **ломает** анализ: приложение может обратиться по строке к типу, которого обрезка не видит. Поэтому используются аннотации:

| Аннотация | Смысл |
|---|---|
| `[DynamicallyAccessedMembers(...)]` | параметр или поле, к членам которого обратятся рефлексией — сохранить их |
| `[RequiresUnreferencedCode]` | метод небезопасен при обрезке (предупреждение `IL2026` вызывающему) |
| `[RequiresDynamicCode]` | метод требует генерации кода (предупреждение `IL3050`) |
| `[UnconditionalSuppressMessage]` | подавить предупреждение, взяв ответственность |

Решение для AOT — **генераторы исходного кода**: они заменяют рефлексию кодом, видимым компилятору (`JsonSerializerContext`, `[GeneratedRegex]`, `[LoggerMessage]`). Динамически создаваемый код (`Expression.Compile`, `Reflection.Emit`) в Native AOT недоступен: `RuntimeFeature.IsDynamicCodeSupported == false` ([19 · Производительность](../19-performance/expert.ru.md#native-aot)).

## Загрузка сборок

`Assembly.Load`, `Assembly.LoadFrom` и `AssemblyLoadContext` загружают сборки во время выполнения; `AssemblyLoadContext` изолирует плагины со своими версиями зависимостей и позволяет выгружать контекст (collectible). `MetadataLoadContext` читает метаданные сборки **без выполнения** её кода — для инструментов анализа.

## Как работает генератор

Компилятор Roslyn последовательно: разбирает исходники (дерево синтаксиса), строит семантическую модель, запускает генераторы, **добавляет их файлы в компиляцию** и выпускает сборку. Генератор видит синтаксис и символы, но не может изменить существующий код — только добавить новые файлы; поэтому классы должны быть `partial`. Требования: генератор детерминирован, не использует диск и сеть, собирается под `netstandard2.0` (загружается в процесс компилятора и IDE). Инкрементальный конвейер (`IIncrementalGenerator`) описывает преобразования в виде графа; компилятор запоминает промежуточные результаты и пересчитывает только изменившееся, если модели сравнимы по значению (`record`, кортежи, `ImmutableArray`) — поэтому в моделях не держат `ISymbol`/`SyntaxNode`.

Отличия от смежных механизмов: **анализатор** (`DiagnosticAnalyzer`) сообщает диагностики и не создаёт код; **code fix** предлагает правку; генератор добавляет код; **перехватчики** (interceptors) подменяют вызовы на этапе компиляции — новая возможность, статус которой нужно сверять с документацией текущей версии C#.

## Справочник

| Что | API |
|---|---|
| Тип | `typeof`, `GetType()`, `Type.GetType`, `Assembly.GetTypes()` |
| Члены | `GetProperties/GetMethods/GetFields/GetConstructors`, `BindingFlags` |
| Создание и вызов | `Activator.CreateInstance`, `ConstructorInfo.Invoke`, `MethodInfo.Invoke`, `PropertyInfo.GetValue/SetValue` |
| Обобщения | `MakeGenericType`, `GetGenericArguments`, `IsGenericTypeDefinition` |
| Атрибуты | `GetCustomAttribute<T>`, `AttributeUsage` |
| Быстрый доступ | `Delegate.CreateDelegate`, `Expression…Compile()` |
| Загрузка | `Assembly.Load*`, `AssemblyLoadContext`, `MetadataLoadContext` |
| AOT-аннотации | `DynamicallyAccessedMembers`, `RequiresUnreferencedCode`, `RequiresDynamicCode` |
| Генераторы | `IIncrementalGenerator`, `ForAttributeWithMetadataName`, `RegisterSourceOutput` |

## Подвохи

**1. Регистр имён.** `GetProperty("name")` вернул `null` (проверено).

**2. Неоднозначные перегрузки.** `AmbiguousMatchException` (проверено).

**3. `TargetInvocationException`.** Настоящая ошибка — в `InnerException` (проверено).

**4. Закрытые члены без `NonPublic`.** Метод вернёт `null`/пусто.

**5. Рефлексия и обрезка/AOT.** Динамически найденный тип может быть удалён: аннотируйте или переходите на генератор.

**6. Упаковка.** `GetValue`/`Invoke` боксят значимые типы и аргументы (проверено: 72 Б на `Invoke`).

**7. Строки вместо `nameof`.** Переименование молча ломает рефлексивный код.

**8. Генераторы: неэкранированные `{`/`}` в генерируемой интерполированной строке** дают ошибку компиляции (`CS1073`) в сгенерированном файле (проверено).

**9. Генератор без `partial`-класса.** Расширить закрытый класс нельзя.

## Компромиссы и альтернативы

| Подход | Плюсы | Минусы |
|---|---|---|
| Рефлексия | гибко, просто | медленно, не для AOT, нет проверок компилятора |
| Кэшированные делегаты / `Expression.Compile` | быстро после прогрева | сложнее, генерация кода (не AOT) |
| Source generator | нулевая стоимость во время выполнения, AOT | сложнее писать и отлаживать, `netstandard2.0` |
| Ручной код | просто, прозрачно | много повторов |
| Интерфейсы и полиморфизм | типобезопасно | не заменяют обнаружение по метаданным |

## Вопросы с собеседований

<details><summary>1. Что такое рефлексия и зачем она нужна?</summary>

Чтение метаданных типов и обращение к членам по имени во время выполнения: сериализаторы, ORM, DI, плагины, тестовые фреймворки. Цена — скорость и потеря проверок компилятора.

</details>

<details><summary>2. Как получить объект `Type`?</summary>

`typeof(T)`, `obj.GetType()`, `Type.GetType(string)`, `assembly.GetTypes()`. Для одного типа в приложении `Type` один и тот же (проверено: `ReferenceEquals`).

</details>

<details><summary>3. Что такое `BindingFlags`?</summary>

Перечисление, задающее, какие члены искать: `Public`, `NonPublic`, `Instance`, `Static`, `DeclaredOnly`, `IgnoreCase`. По умолчанию — публичные члены экземпляра и статические.

</details>

<details><summary>4. Как ускорить вызовы через рефлексию?</summary>

Кэшировать `MethodInfo`/`PropertyInfo`, создавать типизированные делегаты (`Delegate.CreateDelegate`) или компилировать выражения; для AOT — генератор.

</details>

<details><summary>5. Что такое дерево выражений?</summary>

Данные, описывающие лямбду (`Expression<Func<…>>`): их можно анализировать (имя свойства, SQL) и компилировать в делегат.

</details>

<details><summary>6. Чем атрибут отличается от обычного класса?</summary>

Метаданные, привязанные к элементу кода; не выполняют ничего сами, читаются рефлексией или компилятором (генератором).

</details>

<details><summary>7. Что такое source generator?</summary>

Компонент компилятора, который при сборке анализирует код и добавляет файлы исходников. Работает на этапе компиляции, поэтому без затрат при выполнении и совместим с AOT.

</details>

<details><summary>8. Почему генератор собирают под `netstandard2.0`?</summary>

Он загружается в процесс компилятора и IDE, которые работают на разных средах выполнения, поэтому берут наименьший общий знаменатель.

</details>

<details><summary>9. Что значит «инкрементальный» генератор?</summary>

Конвейер из преобразований с кэшированием: компилятор повторяет только этапы, у которых изменился вход; модели должны быть сравнимы по значению, без символов и узлов синтаксиса.

</details>

<details><summary>10. Почему генерируемый класс должен быть `partial`?</summary>

Генератор не может изменить существующий код, он только добавляет файлы; `partial` позволяет дополнить тот же тип.

</details>

<details><summary>11. Вопрос с подвохом: `typeof(int?)` и `boxed.GetType()` для `int? n = 5`. Равны ли они?</summary>

Нет: `typeof(int?)` — `Nullable<int>`, а `GetType()` у упакованного значения даёт `Int32`: при упаковке `Nullable<T>` со значением упаковывается само значение (проверено).

</details>

<details><summary>12. Вопрос с подвохом: почему `GetProperty("name")` вернул `null`, хотя свойство `Name` есть?</summary>

Поиск регистрозависим; нужен `BindingFlags.IgnoreCase` или точное имя (лучше `nameof(Person.Name)`).

</details>

<details><summary>13. Вопрос с подвохом: приложение работает в Debug, но после публикации с обрезкой падает на рефлексии. Почему?</summary>

Обрезка удалила код, который виден только по строковым именам. Нужны аннотации (`DynamicallyAccessedMembers`, `[DynamicDependency]`) или замена рефлексии генератором.

</details>

## Связанные материалы

- Назад: [продвинутая теория](advanced.ru.md). Проверка: [квиз](quiz.ru.md) и [упражнения](exercises.ru.md).
- [19 · Производительность](../19-performance/overview.ru.md) — Native AOT и стоимость динамического кода.
- [13 · DI, конфигурация, логирование](../13-di-config-logging/overview.ru.md) — DI и `[LoggerMessage]`.
- [07 · LINQ](../07-linq/overview.ru.md) — `IQueryable<T>` и деревья выражений.
- [11 · Современный C#](../11-modern-csharp/overview.ru.md) — `partial`-типы.
- [Python](../../../python/overview.ru.md) — динамическая интроспекция по умолчанию.

## Источники

- [Метаданные и рефлексия](https://learn.microsoft.com/dotnet/fundamentals/reflection/overview)
- [Подготовка библиотек к обрезке](https://learn.microsoft.com/dotnet/core/deploying/trimming/prepare-libraries-for-trimming)
- [`AssemblyLoadContext`](https://learn.microsoft.com/dotnet/core/dependency-loading/understanding-assemblyloadcontext)
- [Cookbook инкрементальных генераторов](https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.cookbook.md)
