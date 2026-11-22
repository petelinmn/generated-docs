---
title: "Упражнения: Интеграционные тесты"
lang: ru
track: csharp
kind: module
doc: exercises
slug: 17-testing-integration
part: 2
tags: [integration-testing, web-application-factory, test-isolation, configuration, problem-details]
status: draft
updated: 2026-11-22
---

# Упражнения: Интеграционные тесты

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Тестируемый API (проект на `Microsoft.NET.Sdk.Web` с EF Core SQLite, конечные точки `GET /todos`, `GET /todos/{id}`, `POST /todos`, сервис `IClock`) и фабрика `ApiFactory` с SQLite in-memory и фиксированным временем даны в [базовой теории](basics.ru.md#подмена-зависимостей). Все решения проверены запуском: 9 тестов, все пройдены. Запускаемых версий в `code/` пока нет (уровень 5 не создан).

## 1. 🟢 Smoke-тест

**Задача:** проверь, что `GET /todos` возвращает `200` и `Content-Type: application/json`.

**Ожидаемый результат:** тест проходит.

<details><summary>Подсказка</summary>

`IClassFixture<ApiFactory>`, `factory.CreateClient()`, `r.Content.Headers.ContentType!.MediaType`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 1: a smoke test — the app starts and answers.
public class Ex1_Smoke(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task GetTodos_Returns200_Json()
    {
        var r = await factory.CreateClient().GetAsync("/todos", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.Equal("application/json", r.Content.Headers.ContentType!.MediaType);
    }
}
```

Проверено: тест пройден.

</details>

## 2. 🟢 Создать и получить

**Задача:** отправь `POST /todos` с названием `write docs`, проверь `201`, заголовок `Location` и тело; затем получи задачу по `GET /todos/{id}` и сравни название.

**Ожидаемый результат:** `201`, `Location = /todos/<id>`, `GET` возвращает `write docs`.

<details><summary>Подсказка</summary>

`PostAsJsonAsync`, `ReadFromJsonAsync<Todo>`, `GetFromJsonAsync<Todo>`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 2: a round trip through the real pipeline.
[Fact]
public async Task Post_Then_Get_RoundTrips()
{
    var ct = TestContext.Current.CancellationToken;
    var created = await _http.PostAsJsonAsync("/todos", new NewTodo("write docs"), ct);

    Assert.Equal(HttpStatusCode.Created, created.StatusCode);
    var todo = await created.Content.ReadFromJsonAsync<Todo>(ct);
    Assert.Equal("write docs", todo!.Title);
    Assert.Equal($"/todos/{todo.Id}", created.Headers.Location!.OriginalString);

    var fetched = await _http.GetFromJsonAsync<Todo>($"/todos/{todo.Id}", ct);
    Assert.Equal("write docs", fetched!.Title);
}
```

Проверено: тест пройден (в составе класса `TodoApiTests`).

</details>

## 3. 🟡 Фиксированное время

**Задача:** докажи, что подменённые часы попали в приложение: `CreatedAt` созданной задачи равен `ApiFactory.Now`.

**Ожидаемый результат:** `todo.CreatedAt == ApiFactory.Now`.

<details><summary>Подсказка</summary>

В фабрике `ConfigureTestServices`: `RemoveAll<IClock>()` и `AddSingleton<IClock>(new FixedClock(Now))`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 3: the replaced clock reaches the application.
var created = await _http.PostAsJsonAsync("/todos", new NewTodo("with fixed time"), ct);
var todo = await created.Content.ReadFromJsonAsync<Todo>(ct);
Assert.Equal(ApiFactory.Now, todo!.CreatedAt);
```

Проверено: значение равно подменённому. Без подмены время зависело бы от момента запуска.

</details>

## 4. 🟡 Ошибка валидации

**Задача:** проверь, что `POST /todos` с пустым названием отвечает `400` типа `application/problem+json`, а тело содержит `Title is required.`; а неизвестный `id` даёт `404`.

**Ожидаемый результат:** оба теста проходят.

<details><summary>Подсказка</summary>

`await r.Content.ReadAsStringAsync(ct)` и `Assert.Contains`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 4: error paths at the boundary.
[Fact]
public async Task Post_EmptyTitle_Returns400_WithProblemDetails()
{
    var ct = TestContext.Current.CancellationToken;
    var r = await _http.PostAsJsonAsync("/todos", new NewTodo(""), ct);
    Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
    Assert.Equal("application/problem+json", r.Content.Headers.ContentType!.MediaType);
    Assert.Contains("Title is required.", await r.Content.ReadAsStringAsync(ct));
}

[Fact]
public async Task Get_Unknown_Returns404() =>
    Assert.Equal(HttpStatusCode.NotFound, (await _http.GetAsync("/todos/999", TestContext.Current.CancellationToken)).StatusCode);
```

Проверено: оба теста пройдены.

</details>

## 5. 🔴 Изоляция: чистая база на каждый тест

**Задача:** сделай так, чтобы каждый тест видел только свои данные: два теста создают по задаче с разными названиями и проверяют, что в списке ровно одна — своя.

**Ожидаемый результат:** оба теста проходят независимо от порядка.

<details><summary>Подсказка</summary>

Тестовый класс реализует `IAsyncLifetime`: в `InitializeAsync` создаёт `new ApiFactory()`, в `DisposeAsync` освобождает. xUnit создаёт экземпляр класса на каждый тест, поэтому фабрика и база — на каждый тест.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 5: a fresh factory (and database) per test.
public class Ex5_Isolation : IAsyncLifetime
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
        Assert.Single(list!);
        Assert.Equal(title, list![0].Title);
    }
}
```

Проверено: оба теста прошли. Цена — запуск приложения на каждый тест; при большом числе тестов используют общую фабрику и очистку данных.

</details>
