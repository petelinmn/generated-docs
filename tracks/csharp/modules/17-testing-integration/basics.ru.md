---
title: "Интеграционные тесты: базовая теория"
lang: ru
track: csharp
kind: module
doc: basics
slug: 17-testing-integration
part: 2
tags: [integration-testing, web-application-factory, test-server, xunit, http-client]
versions: ".NET 11, C# 15; xunit.v3 4.0.3; Microsoft.AspNetCore.Mvc.Testing 11.0.0"
status: draft
updated: 2026-11-22
---

# Интеграционные тесты: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [16 · Unit-тесты](../16-testing-unit/overview.ru.md), [14 · ASP.NET Core: Minimal API](../14-aspnet-core-minimal-api/overview.ru.md). **Время:** ~1 ч.

## Зачем это нужно

Unit-тесты проверяют отдельные классы и не ловят ошибок «на стыках»: неверный маршрут, забытая регистрация в DI, неправильное сопоставление с базой, формат JSON. **Интеграционный тест** запускает приложение целиком (или его существенную часть) и проверяет его через внешний интерфейс — для веб-API это настоящие HTTP-запросы. Такие тесты медленнее unit-тестов, но проверяют то, что реально получит клиент.

## WebApplicationFactory

Пакет `Microsoft.AspNetCore.Mvc.Testing` содержит `WebApplicationFactory<TEntryPoint>`: она **запускает ваше приложение внутри теста** на in-memory-сервере (`TestServer`) — без сети и занятых портов — и выдаёт `HttpClient` для обращений к нему. Проверено: `CreateClient()` даёт клиент с адресом `http://localhost/`, сервер — `TestServer`, окружение — `Development`.

Для приложения с top-level statements у класса `Program` нужно открыть тип тестам, добавив в проект API:

```csharp
public partial class Program { }
```

(Без этого `Program` внутренний, и `WebApplicationFactory<Program>` его не видит.) Тестовый проект — обычный проект xUnit v3 ([16 · Unit-тесты](../16-testing-unit/overview.ru.md)) с ссылкой на проект API и пакетом `Microsoft.AspNetCore.Mvc.Testing`.

## Первый тест

```csharp
public class SmokeTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GetTodos_Returns200_Json()
    {
        var client = factory.CreateClient();
        var r = await client.GetAsync("/todos", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.Equal("application/json", r.Content.Headers.ContentType!.MediaType);
    }
}
```

Фабрика передаётся через `IClassFixture`: сервер поднимается **один раз на класс** (фикстура — [16 · Unit-тесты](../16-testing-unit/advanced.ru.md#фикстуры-общее-состояние)), а клиенты создаются дёшево. Такой тест называют **smoke-тестом**: приложение стартует и отвечает.

## Проверка ответа

Через `HttpClient` проверяют код, заголовки и тело; JSON читается расширениями `System.Net.Http.Json`:

```csharp
var created = await client.PostAsJsonAsync("/todos", new NewTodo("write docs"), ct);
Assert.Equal(HttpStatusCode.Created, created.StatusCode);

var todo = await created.Content.ReadFromJsonAsync<Todo>(ct);
Assert.Equal("write docs", todo!.Title);
Assert.Equal($"/todos/{todo.Id}", created.Headers.Location!.OriginalString);

var fetched = await client.GetFromJsonAsync<Todo>($"/todos/{todo.Id}", ct);   // a round trip through the real pipeline
```

Проверено на API задач: `POST /todos` → `201` и `Location`, `GET` возвращает то, что создали. Ошибочные сценарии тоже важны: `POST` с пустым заголовком — `400` с `application/problem+json`, `GET /todos/999` — `404` (проверено).

## Подмена зависимостей

Тесты не должны ходить в боевую базу и зависеть от часов. Фабрику наследуют и в `ConfigureWebHost` подменяют сервисы:

```csharp
public class ApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    public static readonly DateTime Now = new(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();                                            // an in-memory database lives while the connection is open
        builder.ConfigureTestServices(services =>                      // using Microsoft.AspNetCore.TestHost;
        {
            services.RemoveAll<DbContextOptions<AppDb>>();
            services.AddDbContext<AppDb>(o => o.UseSqlite(_connection));
            services.RemoveAll<IClock>();
            services.AddSingleton<IClock>(new FixedClock(Now));
        });
    }

    protected override void Dispose(bool disposing) { base.Dispose(disposing); if (disposing) _connection.Dispose(); }
}
```

Проверено: тест увидел фиксированное время (`CreatedAt == Now`), а `db.Database.ProviderName` содержит `Sqlite`. `ConfigureTestServices` выполняется **после** регистраций приложения, поэтому подмена побеждает. Для `ConfigureTestServices` нужна директива `using Microsoft.AspNetCore.TestHost;` — без неё метод расширения не найден (проверено).

## Ошибки новичка

- ❌ `Program` не открыт тестам → ✅ `public partial class Program { }`.
- ❌ Тесты ходят в боевую БД → ✅ отдельная база (SQLite в памяти, контейнер) и подмена сервисов.
- ❌ Настоящие часы, случайность, внешние API → ✅ подмена через `ConfigureTestServices`.
- ❌ Тесты зависят друг от друга через общую БД → ✅ изоляция ([продвинутая теория](advanced.ru.md#изоляция-данных-между-тестами)).
- ❌ Интеграционными тестами проверяют каждую мелочь → ✅ их немного: основные сценарии; детали — unit-тестами.
- ❌ Забыть закрыть соединение SQLite in-memory → ✅ закрывать при освобождении фабрики.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — изоляция данных, конфигурация, настоящие базы в контейнерах.
- [16 · Unit-тесты](../16-testing-unit/overview.ru.md) — xUnit, фикстуры, дублёры.
- [14 · ASP.NET Core: Minimal API](../14-aspnet-core-minimal-api/overview.ru.md) — тестируемое приложение.
- [15 · EF Core](../15-ef-core/overview.ru.md) — `DbContext`, провайдеры.
- [13 · DI, конфигурация, логирование](../13-di-config-logging/overview.ru.md) — подмена сервисов.

## Источники

- [Интеграционные тесты в ASP.NET Core](https://learn.microsoft.com/aspnet/core/test/integration-tests)
- [`WebApplicationFactory<TEntryPoint>`](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.mvc.testing.webapplicationfactory-1)
- [Тестирование в EF Core](https://learn.microsoft.com/ef/core/testing/)
