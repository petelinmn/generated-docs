---
title: "Integration tests: basics"
lang: en
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

# Integration tests: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [16 · Unit tests](../16-testing-unit/overview.en.md), [14 · ASP.NET Core: Minimal API](../14-aspnet-core-minimal-api/overview.en.md). **Time:** ~1 h.

## Why it matters

Unit tests check individual classes and do not catch errors "at the seams": a wrong route, a forgotten DI registration, an incorrect database mapping, the JSON format. An **integration test** runs the application as a whole (or a substantial part of it) and checks it through its external interface — for a web API that is real HTTP requests. Such tests are slower than unit tests, but they check what a client will actually receive.

## WebApplicationFactory

The `Microsoft.AspNetCore.Mvc.Testing` package contains `WebApplicationFactory<TEntryPoint>`: it **runs your application inside the test** on an in-memory server (`TestServer`) — with no network and no occupied ports — and hands out an `HttpClient` to talk to it. Verified: `CreateClient()` gives a client with the address `http://localhost/`, the server is `TestServer`, the environment is `Development`.

For an application with top-level statements the `Program` class must be exposed to tests by adding this to the API project:

```csharp
public partial class Program { }
```

(Without it `Program` is internal and `WebApplicationFactory<Program>` cannot see it.) The test project is an ordinary xUnit v3 project ([16 · Unit tests](../16-testing-unit/overview.en.md)) with a reference to the API project and the package `Microsoft.AspNetCore.Mvc.Testing`.

## The first test

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

The factory is passed through `IClassFixture`: the server is started **once per class** (a fixture — [16 · Unit tests](../16-testing-unit/advanced.en.md#fixtures-shared-state)), and clients are cheap to create. Such a test is called a **smoke test**: the application starts and answers.

## Checking the response

Through `HttpClient` you check the code, headers and body; JSON is read with the `System.Net.Http.Json` extensions:

```csharp
var created = await client.PostAsJsonAsync("/todos", new NewTodo("write docs"), ct);
Assert.Equal(HttpStatusCode.Created, created.StatusCode);

var todo = await created.Content.ReadFromJsonAsync<Todo>(ct);
Assert.Equal("write docs", todo!.Title);
Assert.Equal($"/todos/{todo.Id}", created.Headers.Location!.OriginalString);

var fetched = await client.GetFromJsonAsync<Todo>($"/todos/{todo.Id}", ct);   // a round trip through the real pipeline
```

Verified on a task API: `POST /todos` → `201` and `Location`, `GET` returns what was created. Error scenarios matter too: a `POST` with an empty title — `400` with `application/problem+json`, `GET /todos/999` — `404` (verified).

## Replacing dependencies

Tests must not go to the production database or depend on the clock. The factory is subclassed and services are replaced in `ConfigureWebHost`:

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

Verified: the test saw the fixed time (`CreatedAt == Now`), and `db.Database.ProviderName` contains `Sqlite`. `ConfigureTestServices` runs **after** the application's registrations, so the replacement wins. `ConfigureTestServices` needs the directive `using Microsoft.AspNetCore.TestHost;` — without it the extension method is not found (verified).

## Beginner mistakes

- ❌ `Program` not exposed to tests → ✅ `public partial class Program { }`.
- ❌ Tests go to the production database → ✅ a separate database (in-memory SQLite, a container) and replaced services.
- ❌ A real clock, randomness, external APIs → ✅ replace through `ConfigureTestServices`.
- ❌ Tests depend on each other through a shared database → ✅ isolation ([advanced theory](advanced.en.md#data-isolation-between-tests)).
- ❌ Checking every small thing with integration tests → ✅ there are few of them: the main scenarios; details by unit tests.
- ❌ Forgetting to close the in-memory SQLite connection → ✅ close it when the factory is disposed.

## Related materials

- Next: [advanced theory](advanced.en.md) — data isolation, configuration, real databases in containers.
- [16 · Unit tests](../16-testing-unit/overview.en.md) — xUnit, fixtures, doubles.
- [14 · ASP.NET Core: Minimal API](../14-aspnet-core-minimal-api/overview.en.md) — the application under test.
- [15 · EF Core](../15-ef-core/overview.en.md) — `DbContext`, providers.
- [13 · DI, configuration, logging](../13-di-config-logging/overview.en.md) — replacing services.

## Sources

- [Integration tests in ASP.NET Core](https://learn.microsoft.com/aspnet/core/test/integration-tests)
- [`WebApplicationFactory<TEntryPoint>`](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.mvc.testing.webapplicationfactory-1)
- [Testing in EF Core](https://learn.microsoft.com/ef/core/testing/)
