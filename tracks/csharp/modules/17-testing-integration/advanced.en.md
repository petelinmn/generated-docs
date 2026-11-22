---
title: "Integration tests: advanced"
lang: en
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

# Integration tests: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [basics](basics.en.md). **Time:** ~1.5 h.

## Data isolation between tests

A factory in `IClassFixture` lives once per class, so the database is **shared** by all tests of the class: one test's data is visible to the others, and test order is not guaranteed. Ways to isolate:

| Way | Pros | Cons |
|---|---|---|
| A new factory (and database) per test | full isolation, simple | slow: the application starts anew |
| A shared factory + clearing tables before a test | fast | you must not forget to clear; parallel tests interfere |
| Unique data in each test (random names/keys) | parallelism | tests must not count "everything in the table" |
| A transaction per test with rollback | fast | not for all scenarios; the application uses its own connections |

The "own factory per test" variant through `IAsyncLifetime` (xUnit creates a class instance per test):

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

Verified: both tests passed — in each the list holds exactly one task, its own.

## Test configuration

Configuration is overridden at the test level with `WithWebHostBuilder` — it creates a derived factory with its own settings:

```csharp
using var factory = new ApiFactory().WithWebHostBuilder(b =>
    b.ConfigureAppConfiguration((_, cfg) => cfg.AddInMemoryCollection(
        new Dictionary<string, string?> { ["Feature:Flag"] = "on" })));
var cfg = (IConfiguration)factory.Services.GetService(typeof(IConfiguration))!;
Assert.Equal("on", cfg["Feature:Flag"]);               // verified
```

This is how you turn flags on, change connection strings, disable background services. The client options (`WebApplicationFactoryClientOptions`) configure automatic redirects (`AllowAutoRedirect = false`), cookie handling, the base address.

## Which database to take

| Option | Fidelity | Speed | Notes |
|---|---|---|---|
| EF Core InMemory | low: not relational | high | does not check constraints, transactions and SQL |
| SQLite in-memory | medium: real SQL, but a different dialect | high | good for logic; differences from the production DBMS are possible |
| A real DBMS in a container (Testcontainers) | high | lower | needs Docker |
| A shared test database | high | varies | a risk of conflicts between runs |

The verified tests use in-memory SQLite: real SQL and constraints, fast, no external dependencies. The connection must stay open as long as the database lives — so the factory holds it.

> ⚠️ Verify: **Testcontainers** starts a real DBMS (for example PostgreSQL) in a Docker container for the duration of the tests; in the environment where this material was prepared the Docker daemon was unavailable, so this method was not run. The scheme: a fixture with `IAsyncLifetime` starts the container (`PostgreSqlBuilder().Build()` → `StartAsync()`), passes the connection string to the factory through `ConfigureTestServices`, and stops the container in `DisposeAsync`. Container tests are marked and skipped if Docker is unavailable. Details are in the "Testcontainers" material (not created yet).

## What to check with integration tests

| What | Example |
|---|---|
| The main "happy path" scenarios | create → get → delete |
| Errors at the boundary | `400`/`404`/`409`, the ProblemDetails format |
| Dependency registration | the application starts, the needed services resolve |
| Configuration and binding | `ValidateOnStart` fails on wrong settings |
| Database work | migrations, constraints, queries ([15 · EF Core](../15-ef-core/overview.en.md)) |
| Authorization | `401`/`403` without a token and with a role |

Small variations and boundary values — with unit tests: integration tests cost more. The pyramid: many unit tests, fewer integration tests, even fewer end-to-end ones.

## Best practices

| Rule | Why |
|---|---|
| Replace external dependencies (time, mail, payments) | determinism |
| One application start per class, not per test — if data is isolated | speed |
| Do not depend on test order | parallel and random order |
| Assert on both the response code and the body | the body shows the cause |
| Pass `TestContext.Current.CancellationToken` | stopping the run interrupts requests |
| Keep tests fast: in-memory SQLite instead of a heavy database where possible | feedback |

## Related materials

- Back: [basics](basics.en.md). Next: [expert theory](expert.en.md) — factory internals, gotchas.
- [16 · Unit tests](../16-testing-unit/overview.en.md) — fixtures and `IAsyncLifetime`.
- [15 · EF Core](../15-ef-core/overview.en.md) — providers and migrations.
- [13 · DI, configuration, logging](../13-di-config-logging/overview.en.md) — `ConfigureTestServices`, settings.
- [Kubernetes](../../../kubernetes/overview.en.md) — disposable environments for tests (kind).

## Sources

- [Integration tests: replacing services](https://learn.microsoft.com/aspnet/core/test/integration-tests#inject-mock-services)
- [Testing applications on EF Core](https://learn.microsoft.com/ef/core/testing/choosing-a-testing-strategy)
- [Testcontainers for .NET](https://dotnet.testcontainers.org/)
- [xUnit: `IAsyncLifetime`](https://xunit.net/docs/shared-context)
