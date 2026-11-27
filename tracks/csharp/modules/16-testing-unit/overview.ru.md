---
title: "Unit-тесты"
lang: ru
track: csharp
kind: module
doc: overview
slug: 16-testing-unit
part: 2
levels: [1, 2, 3, 4]
tags: [unit-testing, xunit, arrange-act-assert, theory, fixtures, mocking, code-coverage]
versions: ".NET 11, C# 15; xunit.v3 4.0.3; NSubstitute 5.3.0"
status: draft
updated: 2026-11-26
---

# Unit-тесты

🌐 **Русский** · [English](overview.en.md)

## Коротко

Unit-тест — небольшая программа, которая вызывает единицу кода и проверяет результат. В .NET стандарт де-факто — xUnit (v3): тестовый проект — исполняемый файл на платформе Microsoft.Testing.Platform, `dotnet test` его запускает. Тесты должны быть быстрыми, независимыми и детерминированными; для этого внешние зависимости (время, почта, БД) заменяют дублёрами (NSubstitute). Покрытие кода показывает, что не проверяется, но не качество проверок.

## Ключевые идеи

- **Arrange–Act–Assert** и имена `Метод_Сценарий_Ожидание`; один сценарий — один тест.
- **`[Fact]` и `[Theory]`** (`InlineData`, `MemberData`/`TheoryData`): данные вместо копий тестов.
- **Изоляция:** новый экземпляр класса на каждый тест; общее состояние — только через фикстуры (`IClassFixture`).
- **Проверки:** первым аргументом — ожидаемое; `Throws<T>` требует точного типа, `ThrowsAny` — любого наследника.
- **Дублёры:** stub, fake, mock; проверяйте поведение, а не реализацию.
- **Параллельность:** тестовые классы идут параллельно, статическое состояние в тестах опасно.
- **Покрытие:** `--coverage`, считается только для кода вне тестовой сборки.

## Минимальный пример

```csharp
public class Calculator { public int Add(int a, int b) => a + b; }

public class CalculatorTests
{
    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(-1, 1, 0)]
    public void Add_Cases(int a, int b, int expected) =>
        Assert.Equal(expected, new Calculator().Add(a, b));
}
```

## Когда применять, а когда нет

- ✅ Бизнес-логика, вычисления, валидация, парсинг — быстрые unit-тесты.
- ✅ Граничные значения и ошибки — теории и `Assert.Throws`.
- ✅ Зависимости на границах — дублёры.
- ❌ Тесты, проверяющие реализацию (каждый внутренний вызов): ломаются при рефакторинге.
- ❌ Unit-тесты для того, что надо проверять с настоящей БД или HTTP — интеграционные ([17](../17-testing-integration/overview.ru.md)).
- ❌ Статическое состояние и зависимость от порядка.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | проект xUnit v3, `[Fact]`, AAA, `Assert`, `[Theory]`, изоляция, асинхронные тесты, пропуск |
| [2. Продвинутая теория](advanced.ru.md) | `MemberData`/`TheoryData`, фикстуры, дублёры и NSubstitute, `TimeProvider`, `TestContext`, фильтры и трейты, покрытие, параллельность |
| [3. Экспертная теория](expert.ru.md) | устройство xUnit v3 и MTP, жизненный цикл, семантика проверок, подвохи, справочник, 13 вопросов с собеседований |
| [4. Самопроверка](quiz.ru.md) | 11 вопросов и [5 упражнений](exercises.ru.md) с решениями |
| 5. Запускаемые примеры | ещё не создан — `/learn csharp 16-testing-unit level 5` |
| 6. Тесты | ещё не создан — `/learn csharp 16-testing-unit level 6` |

## Связанные материалы

- Пререквизиты: [08 · Исключения](../08-exceptions/overview.ru.md), [13 · DI, конфигурация, логирование](../13-di-config-logging/overview.ru.md)
- Дальше: [17 · Тестирование: интеграционные тесты](../17-testing-integration/overview.ru.md)
- Асинхронные тесты: [10 · Async/await](../10-async-await/overview.ru.md)
- Архитектурные тесты: [21 · Архитектура приложений](../21-architecture/overview.ru.md)
- То же в других треках: [Python](../../../python/overview.ru.md) — pytest
- Рецепты и библиотеки: [FluentValidation](../../libraries/fluentvalidation/overview.ru.md), [NSubstitute](../../libraries/nsubstitute/overview.ru.md), [Spectre.Console](../../libraries/spectre-console/overview.ru.md), [Testcontainers](../../libraries/testcontainers/overview.ru.md)
- [Тестирование компонентов React](../../../react/modules/10-testing/overview.ru.md)
