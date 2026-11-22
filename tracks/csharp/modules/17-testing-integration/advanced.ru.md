---
title: "Интеграционные тесты: продвинутая теория"
lang: ru
track: csharp
kind: module
doc: advanced
slug: 17-testing-integration
part: 2
tags: [integration-testing, web-application-factory, test-isolation, testcontainers, sqlite, test-configuration]
versions: ".NET 11, C# 15; xunit.v3 4.0.3; Microsoft.AspNetCore.Mvc.Testing 11.0.0"
status: draft
updated: 2026-11-22
---

# Интеграционные тесты: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [базовая теория](basics.ru.md). **Время:** ~1,5 ч.

## Изоляция данных между тестами

Фабрика в `IClassFixture` живёт один раз на класс, поэтому база данных — **общая** для всех тестов класса: данные одного теста видны другим, а порядок тестов не гарантирован. Способы изоляции:

| Способ | Плюсы | Минусы |
|---|---|---|
| Новая фабрика (и база) на каждый тест | полная изоляция, просто | медленно: приложение стартует заново |
| Общая фабрика + очистка таблиц перед тестом | быстро | нужно не забыть очистить; параллельные тесты мешают |
| Уникальные данные в каждом тесте (случайные имена/ключи) | параллельность | тесты не должны считать «всю таблицу» |
| Транзакция на тест с откатом | быстро | не для всех сценариев; приложение использует свои подключения |

Вариант «своя фабрика на тест» через `IAsyncLifetime` (xUnit создаёт экземпляр класса на тест):

```csharp
public class IsolationTests : IAsyncLifetime
{
    private ApiFactory _factory = null!;
    public ValueTask InitializeAsync() { _factory = new ApiFactory(); return ValueTask.CompletedTask; }
    public async ValueTask DisposeAsync() => await _factory.DisposeAsync();

    [Fact] public async Task First_SeesOnlyItsOwnData() => await Run("one");
    [Fact] public async Task Second_SeesOnlyItsOwnData() => await Run("two");

    private async Task Run(string title)
    {
        var ct = TestContext.Current.CancellationToken;
        var http = _factory.CreateClient();
        await http.PostAsJsonAsync("/todos", new NewTodo(title), ct);
        var list = await http.GetFromJsonAsync<List<Todo>>("/todos", ct);
        Assert.Single(list!);                      // never sees the other test's todo
        Assert.Equal(title, list![0].Title);
    }
}
```

Проверено: оба теста прошли — в каждом в списке ровно одна своя задача.

## Конфигурация теста

Конфигурацию переопределяют на уровне теста через `WithWebHostBuilder` — он создаёт производную фабрику со своими настройками:

```csharp
using var factory = new ApiFactory().WithWebHostBuilder(b =>
    b.ConfigureAppConfiguration((_, cfg) => cfg.AddInMemoryCollection(
        new Dictionary<string, string?> { ["Feature:Flag"] = "on" })));
var cfg = (IConfiguration)factory.Services.GetService(typeof(IConfiguration))!;
Assert.Equal("on", cfg["Feature:Flag"]);               // verified
```

Так включают флаги, меняют строки подключения, отключают фоновые сервисы. Опции клиента (`WebApplicationFactoryClientOptions`) настраивают автоматические перенаправления (`AllowAutoRedirect = false`), обработку cookie, базовый адрес.

## Какую базу брать

| Вариант | Достоверность | Скорость | Примечания |
|---|---|---|---|
| EF Core InMemory | низкая: не реляционная | высокая | не проверяет ограничения, транзакции и SQL |
| SQLite in-memory | средняя: настоящий SQL, но другой диалект | высокая | подходит для логики; различия с боевой СУБД возможны |
| Настоящая СУБД в контейнере (Testcontainers) | высокая | ниже | нужен Docker |
| Общая тестовая БД | высокая | зависит | риск конфликтов между запусками |

В проверенных тестах использована SQLite in-memory: настоящий SQL и ограничения, быстро, без внешних зависимостей. Соединение должно оставаться открытым, пока живёт база, — поэтому его держит фабрика.

> ⚠️ Проверить: **Testcontainers** запускает настоящую СУБД (например, PostgreSQL) в Docker-контейнере на время тестов; в среде, где готовился материал, Docker-демон был недоступен, поэтому этот способ не запускался. Схема: фикстура с `IAsyncLifetime` запускает контейнер (`PostgreSqlBuilder().Build()` → `StartAsync()`), передаёт строку подключения в фабрику через `ConfigureTestServices`, останавливает контейнер в `DisposeAsync`. Тесты с контейнерами помечают и пропускают, если Docker недоступен. Подробности — в материале «Testcontainers» (ещё не создан).

## Что проверять интеграционными тестами

| Что | Пример |
|---|---|
| Основные сценарии «счастливого пути» | создать → получить → удалить |
| Ошибки на границе | `400`/`404`/`409`, формат ProblemDetails |
| Регистрацию зависимостей | приложение стартует, нужные сервисы резолвятся |
| Конфигурацию и привязку | `ValidateOnStart` падает при неверных настройках |
| Работу с БД | миграции, ограничения, запросы ([15 · EF Core](../15-ef-core/overview.ru.md)) |
| Авторизацию | `401`/`403` без токена и с ролью |

Мелкие вариации и граничные значения — unit-тестами: интеграционные дороже. Пирамида: много unit-тестов, меньше интеграционных, ещё меньше сквозных.

## Лучшие практики

| Правило | Почему |
|---|---|
| Подменяйте внешние зависимости (время, почта, платежи) | детерминизм |
| Один запуск приложения на класс, а не на тест — если данные изолированы | скорость |
| Не зависьте от порядка тестов | параллельный и случайный порядок |
| Ассерты на код ответа и тело | по ним видна причина |
| Передавайте `TestContext.Current.CancellationToken` | остановка запуска прерывает запросы |
| Держите тесты быстрыми: SQLite in-memory вместо тяжёлой БД, где можно | обратная связь |

## Связанные материалы

- Назад: [базовая теория](basics.ru.md). Дальше: [экспертная теория](expert.ru.md) — устройство фабрики, подвохи.
- [16 · Unit-тесты](../16-testing-unit/overview.ru.md) — фикстуры и `IAsyncLifetime`.
- [15 · EF Core](../15-ef-core/overview.ru.md) — провайдеры и миграции.
- [13 · DI, конфигурация, логирование](../13-di-config-logging/overview.ru.md) — `ConfigureTestServices`, настройки.
- [Kubernetes](../../../kubernetes/overview.ru.md) — одноразовые окружения для тестов (kind).

## Источники

- [Интеграционные тесты: подмена сервисов](https://learn.microsoft.com/aspnet/core/test/integration-tests#inject-mock-services)
- [Тестирование приложений на EF Core](https://learn.microsoft.com/ef/core/testing/choosing-a-testing-strategy)
- [Testcontainers для .NET](https://dotnet.testcontainers.org/)
- [xUnit: `IAsyncLifetime`](https://xunit.net/docs/shared-context)
