---
title: "Unit-тесты: базовая теория"
lang: ru
track: csharp
kind: module
doc: basics
slug: 16-testing-unit
part: 2
tags: [unit-testing, xunit, arrange-act-assert, theory, assertions, test-isolation]
versions: ".NET 11, C# 15; xunit.v3 4.0.3"
status: draft
updated: 2026-11-22
---

# Unit-тесты: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [04 · ООП: классы и объекты](../04-oop-basics/overview.ru.md), [08 · Исключения](../08-exceptions/overview.ru.md), [10 · Async/await](../10-async-await/overview.ru.md). **Время:** ~1 ч.

## Зачем это нужно

Unit-тест — небольшая программа, которая вызывает кусочек кода (метод, класс) и проверяет результат. Набор таких тестов запускается одной командой и сообщает, не сломалась ли логика после изменений. Хорошие тесты позволяют менять код без страха, служат живой документацией и ловят ошибки раньше, чем пользователи. Самый распространённый фреймворк в .NET — **xUnit** (в этом курсе — версии 3, пакет `xunit.v3`).

## Проект с тестами

Тестовый проект в xUnit v3 — это **исполняемый** проект (`OutputType=Exe`), который запускается через платформу Microsoft.Testing.Platform. В `global.json` включают этот запуск, чтобы `dotnet test` работал:

```json
{ "test": { "runner": "Microsoft.Testing.Platform" } }
```

```xml
<PropertyGroup>
  <TargetFramework>net11.0</TargetFramework>
  <OutputType>Exe</OutputType>
</PropertyGroup>
<ItemGroup>
  <PackageReference Include="xunit.v3" Version="4.0.3" />
</ItemGroup>
<ItemGroup><Using Include="Xunit" /></ItemGroup>
```

Запуск — `dotnet test` в папке проекта или решения. Результат (проверено): `итог: 21, успешно: 20, пропущено: 1`.

## Первый тест

Тест — метод с атрибутом `[Fact]`. Структура **Arrange–Act–Assert** (подготовка, действие, проверка) делает тест читаемым:

```csharp
public class Calculator
{
    public int Add(int a, int b) => a + b;
}

public class CalculatorTests
{
    [Fact]
    public void Add_TwoNumbers_ReturnsSum()
    {
        // Arrange
        var calc = new Calculator();
        // Act
        var result = calc.Add(2, 3);
        // Assert
        Assert.Equal(5, result);
    }
}
```

Имя теста описывает что проверяется: удобная схема `Метод_Сценарий_Ожидание` (`Add_TwoNumbers_ReturnsSum`). Один тест — одна идея: если он падает, сразу ясно, что сломалось.

## Проверки (Assert)

Класс `Assert` сравнивает фактический результат с ожидаемым; первым аргументом идёт **ожидаемое**:

| Проверка | Смысл |
|---|---|
| `Assert.Equal(expected, actual)` | равенство |
| `Assert.True(cond)` / `Assert.False(cond)` | условие |
| `Assert.Null(x)` / `Assert.NotNull(x)` | `null` |
| `Assert.Throws<T>(() => …)` | код бросает исключение точно типа `T` |
| `Assert.Contains(item, collection)` | элемент есть в коллекции |
| `Assert.Single(c)` / `Assert.Empty(c)` | один элемент / пусто |
| `Assert.All(c, x => …)` | проверка для каждого элемента |
| `Assert.Equal(0.3, 0.1 + 0.2, precision: 10)` | числа с плавающей точкой — с точностью |

Сообщение при падении говорит, что ожидалось и что получено (проверено):

```text
Assert.Equal() Failure: Values differ
Expected: 4
Actual:   5
```

Для строк xUnit показывает позицию первого различия. Проверка исключения возвращает его, и можно посмотреть сообщение:

```csharp
[Fact]
public void Divide_ByZero_Throws()
{
    var ex = Assert.Throws<DivideByZeroException>(() => new Calculator().Divide(1, 0));
    Assert.Equal("b is zero", ex.Message);
}
```

## Параметризованные тесты: Theory

Если один и тот же тест нужно прогнать на разных данных, используют `[Theory]` с источником данных — `[InlineData]`:

```csharp
[Theory]
[InlineData(1, 2, 3)]
[InlineData(-1, 1, 0)]
[InlineData(0, 0, 0)]
public void Add_Theory(int a, int b, int expected) =>
    Assert.Equal(expected, new Calculator().Add(a, b));
```

Каждая строка данных — отдельный тест в отчёте. Пограничные значения (ноль, отрицательные, границы) — первые кандидаты на строки данных.

## Изоляция тестов

xUnit создаёт **новый экземпляр** тестового класса для каждого теста, поэтому поля класса не «текут» между тестами:

```csharp
public class LifecycleTests
{
    private readonly Counter _counter = new();
    [Fact] public void A() { _counter.Inc(); Assert.Equal(1, _counter.Value); }
    [Fact] public void B() { _counter.Inc(); Assert.Equal(1, _counter.Value); }   // a new instance: still 1
}
```

Проверено: оба теста прошли. Подготовку делают в конструкторе, очистку — в `Dispose` (если класс реализует `IDisposable`). Тесты не должны зависеть друг от друга и от порядка запуска.

## Асинхронные тесты

Метод теста может быть `async Task`; ожидания исключений — `Assert.ThrowsAsync`:

```csharp
[Fact]
public async Task Async_Test_Awaits() => Assert.Equal(42, await Task.Run(() => 42));

[Fact]
public async Task Async_Throws() =>
    await Assert.ThrowsAsync<InvalidOperationException>(async () => { await Task.Yield(); throw new InvalidOperationException(); });
```

Не используйте `async void` в тестах и не блокируйтесь на `.Result` ([10 · Async/await](../10-async-await/overview.ru.md)).

## Пропуск тестов

`[Fact(Skip = "причина")]` пропускает тест и показывает причину в отчёте (проверено: `пропущено M16.CalculatorTests.Skipped`). Пропуск — временная мера: не оставляйте пропущенные тесты надолго.

## Ошибки новичка

- ❌ Тест без проверки (`Assert`) → ✅ каждый тест что-то утверждает.
- ❌ Несколько несвязанных проверок в одном тесте → ✅ один сценарий — один тест, данные — через `Theory`.
- ❌ Тесты зависят друг от друга или от порядка → ✅ каждый создаёт своё состояние.
- ❌ Тест использует текущее время, случайность, сеть → ✅ подставляйте управляемые зависимости ([продвинутая теория](advanced.ru.md#тестовые-дублёры)).
- ❌ Перепутанные аргументы `Assert.Equal(actual, expected)` → ✅ сначала ожидаемое.
- ❌ `Assert.True(a == b)` вместо `Assert.Equal(a, b)` → ✅ `Equal` показывает значения при падении.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — фикстуры, данные, тестовые дублёры, покрытие.
- [08 · Исключения](../08-exceptions/overview.ru.md) — что проверять в негативных сценариях.
- [10 · Async/await](../10-async-await/overview.ru.md) — асинхронные тесты.
- [13 · DI, конфигурация, логирование](../13-di-config-logging/overview.ru.md) — зависимости через интерфейсы делают код тестируемым.
- [Python](../../../python/overview.ru.md) — pytest: параметризация и фикстуры.

## Источники

- [Тестирование в .NET](https://learn.microsoft.com/dotnet/core/testing/)
- [Unit-тестирование на C#](https://learn.microsoft.com/dotnet/core/testing/unit-testing-csharp-with-xunit)
- [Документация xUnit.net v3](https://xunit.net/docs/getting-started/v3/getting-started)
- [Microsoft.Testing.Platform](https://learn.microsoft.com/dotnet/core/testing/microsoft-testing-platform-intro)
