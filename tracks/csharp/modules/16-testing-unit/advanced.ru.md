---
title: "Unit-тесты: продвинутая теория"
lang: ru
track: csharp
kind: module
doc: advanced
slug: 16-testing-unit
part: 2
tags: [unit-testing, xunit, fixtures, test-doubles, nsubstitute, mocking, code-coverage, traits]
versions: ".NET 11, C# 15; xunit.v3 4.0.3; NSubstitute 5.3.0; Microsoft.Testing.Extensions.CodeCoverage 18.12.0"
status: draft
updated: 2026-11-22
---

# Unit-тесты: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [базовая теория](basics.ru.md). **Время:** ~1,5 ч.

## Источники данных для Theory

Кроме `[InlineData]`, данные берут из статического члена — `[MemberData]`; типобезопасный вариант в xUnit v3 — `TheoryData<…>`:

```csharp
public static TheoryData<int, int, int> DivideCases => new()
{
    { 10, 2, 5 },
    { 7, 2, 3 },
    { -9, 3, -3 },
};

[Theory]
[MemberData(nameof(DivideCases))]
public void Divide_Theory(int a, int b, int expected) => Assert.Equal(expected, new Calculator().Divide(a, b));
```

Данные можно также задать классом (`[ClassData]`) или собственным атрибутом. Для данных, которые xUnit не умеет сериализовать, строки теории всё равно выполняются (проверено: теория с двумя `new object()` дала 2 теста), но в обозревателе тестов они не различимы по значению.

## Фикстуры: общее состояние

Дорогую подготовку (БД, файл) разделяют между тестами через **фикстуру**:

| Область | Как | Когда создаётся |
|---|---|---|
| каждый тест | конструктор / `Dispose` тестового класса | новый экземпляр на тест |
| тестовый класс | `IClassFixture<T>` | один экземпляр `T` на класс |
| несколько классов | `ICollectionFixture<T>` + `[Collection("name")]` | один на коллекцию |

```csharp
public class SharedFixture : IAsyncLifetime
{
    public int Id { get; private set; }
    public ValueTask InitializeAsync() { Id = Interlocked.Increment(ref Created); return ValueTask.CompletedTask; }
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    public static int Created;
}

public class FixtureTests(SharedFixture fx) : IClassFixture<SharedFixture>
{
    [Fact] public void One() => Assert.Equal(1, fx.Id);
    [Fact] public void Two() => Assert.Equal(1, fx.Id);   // the fixture is created once per class
}
```

Проверено: оба теста увидели один и тот же экземпляр (`Id == 1`). `IAsyncLifetime` позволяет делать асинхронную подготовку и очистку. Общая фикстура — общее изменяемое состояние: тесты должны её не портить.

## Тестовые дублёры

Модуль должен проверяться изолированно, поэтому внешние зависимости (часы, почта, БД, сеть) заменяют **дублёрами**:

| Вид | Назначение |
|---|---|
| заглушка (stub) | возвращает заранее заданные значения |
| имитация (fake) | рабочая упрощённая реализация (БД в памяти) |
| мок (mock) | запоминает вызовы, чтобы проверить взаимодействие |

Библиотека **NSubstitute** создаёт дублёры по интерфейсу:

```csharp
public class Welcome(IClock clock, IMailer mailer)
{
    public string Run(string user)
    {
        var greeting = clock.Now.Hour < 12 ? "Good morning" : "Hello";
        mailer.Send(user, $"{greeting}, {user}");
        return greeting;
    }
}

[Fact]
public void Welcome_Morning_SendsGoodMorning()
{
    var clock = Substitute.For<IClock>();
    clock.Now.Returns(new DateTime(2026, 1, 1, 9, 0, 0));          // stub
    var mailer = Substitute.For<IMailer>();

    var result = new Welcome(clock, mailer).Run("ann");

    Assert.Equal("Good morning", result);
    mailer.Received(1).Send("ann", "Good morning, ann");           // mock check: called exactly once
    mailer.DidNotReceive().Send("bob", Arg.Any<string>());
}
```

Проверено: тест проходит. `Returns` задаёт значение, `Received(n)` проверяет число вызовов, `Arg.Any<T>()` — любой аргумент. Асинхронные методы: `repo.FindAsync(1, Arg.Any<CancellationToken>()).Returns("Ann")`. Подробный разбор библиотеки — в материале «NSubstitute» (ещё не создан).

Правило: **проверяйте поведение, а не реализацию**. Если проверять каждый вызов внутренних зависимостей, тест ломается при любом рефакторинге; проверяйте результат и значимые побочные эффекты (отправлено письмо, записано в хранилище).

### Управление недетерминизмом

Время, случайность, файловую систему и сеть вводят через интерфейсы (`IClock`, `TimeProvider`, `IFileSystem`), как `IClock` выше: тест тогда стабилен и воспроизводим. В .NET есть встроенный `TimeProvider` и `FakeTimeProvider` в пакете `Microsoft.Extensions.TimeProvider.Testing`.

## Отмена и xUnit v3

В v3 текущий тест получает токен отмены через `TestContext.Current.CancellationToken`: его передают в асинхронные вызовы, чтобы тест прерывался при остановке запуска.

```csharp
Assert.Equal("Ann", await new Lookup(repo).NameAsync(1, TestContext.Current.CancellationToken));
```

## Фильтрация, трейты, пропуск

Тестам задают метки: `[Trait("Category", "Slow")]`. Запуск выбранных тестов в Microsoft.Testing.Platform:

```text
dotnet test -- --filter-trait "Category=Slow"      # by trait
dotnet test -- --filter-method "*Add_Theory*"      # by method name
dotnet test -- --filter-class "M16.CalculatorTests"
```

Проверено: по трейту выполнился 1 тест, по методу `*Add_Theory*` — 3 (по числу строк `InlineData`). Код возврата при сбое — 2.

## Покрытие кода

Расширение `Microsoft.Testing.Extensions.CodeCoverage` собирает покрытие прямо в `dotnet test`:

```text
dotnet add package Microsoft.Testing.Extensions.CodeCoverage
dotnet test -- --coverage --coverage-output-format cobertura
```

Проверено: файл `*.cobertura.xml` создаётся в `TestResults/`. Покрытие считает только код, лежащий в **других** сборках, чем тесты: когда проверяемый код был в самом тестовом проекте, отчёт оказался пустым (`<packages />`) — размещайте тестируемый код в отдельном проекте. Высокий процент покрытия не гарантирует качество тестов: он показывает, какие строки выполнялись, а не проверялись.

## Параллельность

xUnit запускает тестовые **коллекции** параллельно; по умолчанию каждый тестовый класс — своя коллекция. Проверено: три класса с одновременно стартующими тестами работали параллельно (максимум 3 одновременно). Тесты внутри класса идут последовательно. Классы, которые используют общий ресурс, объединяют в одну коллекцию (`[Collection("db")]`) — она выполняется последовательно.

## Лучшие практики

| Правило | Почему |
|---|---|
| Быстрые, независимые, повторяемые тесты | можно запускать постоянно |
| Один сценарий — один тест, данные через `Theory` | понятно, что сломалось |
| Имя `Метод_Сценарий_Ожидание` | отчёт читается как спецификация |
| Дублёры только на границах (время, сеть, БД) | меньше хрупкости |
| Не тестируйте закрытые методы напрямую | проверяйте публичное поведение |
| Тестируйте граничные значения и ошибки | там чаще всего баги |
| Не проверяйте реализацию (`Received`) без нужды | тест переживёт рефакторинг |

## Связанные материалы

- Назад: [базовая теория](basics.ru.md). Дальше: [экспертная теория](expert.ru.md) — устройство xUnit v3, подвохи.
- [17 · Тестирование: интеграционные тесты](../17-testing-integration/overview.ru.md) — настоящие БД и HTTP.
- [13 · DI, конфигурация, логирование](../13-di-config-logging/overview.ru.md) — зависимости через интерфейсы.
- [10 · Async/await](../10-async-await/overview.ru.md) — `CancellationToken` в тестах.
- [09 · Делегаты, события, лямбды](../09-delegates-events-lambdas/overview.ru.md) — лямбды в `Assert.Throws`.

## Источники

- [Общий контекст и фикстуры xUnit](https://xunit.net/docs/shared-context)
- [Theory и данные](https://xunit.net/docs/getting-started/v3/getting-started)
- [NSubstitute](https://nsubstitute.github.io/help/getting-started/)
- [Покрытие кода в Microsoft.Testing.Platform](https://learn.microsoft.com/dotnet/core/testing/microsoft-testing-platform-extensions-code-coverage)
- [`TimeProvider`](https://learn.microsoft.com/dotnet/api/system.timeprovider)
