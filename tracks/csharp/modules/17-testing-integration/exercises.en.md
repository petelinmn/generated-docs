---
title: "Exercises: Integration tests"
lang: en
track: csharp
kind: module
doc: exercises
slug: 17-testing-integration
part: 2
tags: [integration-testing, web-application-factory, test-isolation, configuration, problem-details]
status: draft
updated: 2026-11-22
---

# Exercises: Integration tests

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. The API under test (a project on `Microsoft.NET.Sdk.Web` with EF Core SQLite, the endpoints `GET /todos`, `GET /todos/{id}`, `POST /todos`, an `IClock` service) and the `ApiFactory` factory with in-memory SQLite and a fixed time are given in the [basics](basics.en.md#replacing-dependencies). All solutions were verified by running them: 9 tests, all passed. There are no runnable versions in `code/` yet (level 5 does not exist).

## 1. 🟢 A smoke test

**Task:** check that `GET /todos` returns `200` and `Content-Type: application/json`.

**Expected result:** the test passes.

<details><summary>Hint</summary>

`IClassFixture<ApiFactory>`, `factory.CreateClient()`, `r.Content.Headers.ContentType!.MediaType`.

</details>

<details><summary>Solution</summary>

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

Verified: the test passed.

</details>

## 2. 🟢 Create and get

**Task:** send `POST /todos` with the title `write docs`, check `201`, the `Location` header and the body; then get the task with `GET /todos/{id}` and compare the title.

**Expected result:** `201`, `Location = /todos/<id>`, `GET` returns `write docs`.

<details><summary>Hint</summary>

`PostAsJsonAsync`, `ReadFromJsonAsync<Todo>`, `GetFromJsonAsync<Todo>`.

</details>

<details><summary>Solution</summary>

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

Verified: the test passed (as part of the `TodoApiTests` class).

</details>

## 3. 🟡 A fixed time

**Task:** prove that the replaced clock reached the application: the `CreatedAt` of the created task equals `ApiFactory.Now`.

**Expected result:** `todo.CreatedAt == ApiFactory.Now`.

<details><summary>Hint</summary>

In the factory's `ConfigureTestServices`: `RemoveAll<IClock>()` and `AddSingleton<IClock>(new FixedClock(Now))`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 3: the replaced clock reaches the application.
var created = await _http.PostAsJsonAsync("/todos", new NewTodo("with fixed time"), ct);
var todo = await created.Content.ReadFromJsonAsync<Todo>(ct);
Assert.Equal(ApiFactory.Now, todo!.CreatedAt);
```

Verified: the value equals the replaced one. Without the replacement the time would depend on the moment of the run.

</details>

## 4. 🟡 A validation error

**Task:** check that `POST /todos` with an empty title answers `400` of type `application/problem+json` and the body contains `Title is required.`; and an unknown `id` gives `404`.

**Expected result:** both tests pass.

<details><summary>Hint</summary>

`await r.Content.ReadAsStringAsync(ct)` and `Assert.Contains`.

</details>

<details><summary>Solution</summary>

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

Verified: both tests passed.

</details>

## 5. 🔴 Isolation: a clean database per test

**Task:** make each test see only its own data: two tests create one task each with different titles and check that the list holds exactly one — its own.

**Expected result:** both tests pass regardless of order.

<details><summary>Hint</summary>

The test class implements `IAsyncLifetime`: `InitializeAsync` creates `new ApiFactory()`, `DisposeAsync` releases it. xUnit creates a class instance per test, so the factory and the database are per test.

</details>

<details><summary>Solution</summary>

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

Verified: both tests passed. The price is an application start per test; with many tests you use a shared factory and data clearing.

</details>
