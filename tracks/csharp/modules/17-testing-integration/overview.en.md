---
title: "Integration tests"
lang: en
track: csharp
kind: module
doc: overview
slug: 17-testing-integration
part: 2
levels: [1, 2, 3, 4]
tags: [integration-testing, web-application-factory, test-server, test-isolation, sqlite, testcontainers]
versions: ".NET 11, C# 15; xunit.v3 4.0.3; Microsoft.AspNetCore.Mvc.Testing 11.0.0"
status: draft
updated: 2026-11-22
---

# Integration tests

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

An integration test runs the application as a whole (or a significant part of it) and checks it through its external interface. For ASP.NET Core this is `WebApplicationFactory<Program>`: it builds an in-memory host on a `TestServer` (no network) and hands out an `HttpClient`. External dependencies (time, mail, the database) are replaced in `ConfigureTestServices`; for the database you take in-memory SQLite or a real DBMS in a container. Such tests are slower than unit tests, but they catch errors "at the seams": routes, DI, SQL, response formats.

## Key ideas

- **`WebApplicationFactory<Program>`** builds the host lazily (on the first `CreateClient()`); the default environment is `Development`.
- **`public partial class Program { }`** exposes `Program` to tests.
- **`ConfigureTestServices`** replaces services after the application's registrations (`using Microsoft.AspNetCore.TestHost;`).
- **Data isolation:** a factory in `IClassFixture` is one per class, the database is shared; for independent tests — a factory per test, clearing or unique data.
- **The database:** InMemory (not relational), in-memory SQLite (keep the connection open), Testcontainers (a real DBMS, needs Docker).
- **What to check:** the main scenarios, error codes and their format, DI registration, configuration; small things — with unit tests.
- **Do not confuse:** an in-memory host does not check the network, TLS or the behavior of a deployed environment.

## Minimal example

```csharp
public class SmokeTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GetTodos_Returns200()
    {
        var r = await factory.CreateClient().GetAsync("/todos", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
    }
}
```

## When to use and when not

- ✅ The main API scenarios "from request to database".
- ✅ Checking dependency registration, configuration and error formats.
- ✅ Database work: constraints, migrations, queries.
- ❌ Every small variation of logic — unit tests are faster and more precise.
- ❌ A shared production database used as the test one.
- ❌ Tests that depend on order and real time.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | why integration tests, `WebApplicationFactory`, the first test, checking the response, replacing dependencies |
| [2. Advanced](advanced.en.md) | data isolation, test configuration, choosing a database (InMemory, SQLite, Testcontainers), what to check, practices |
| [3. Expert](expert.en.md) | factory and `TestServer` internals, the lazy host, the environment, configuration order, gotchas, 12 interview questions |
| [4. Self-check](quiz.en.md) | 11 questions and [5 exercises](exercises.en.md) with solutions |
| 5. Runnable examples | not generated yet — `/learn csharp 17-testing-integration level 5` |
| 6. Tests | not generated yet — `/learn csharp 17-testing-integration level 6` |

## Related materials

- Prerequisites: [16 · Unit tests](../16-testing-unit/overview.en.md), [14 · ASP.NET Core: Minimal API](../14-aspnet-core-minimal-api/overview.en.md)
- Data: [15 · EF Core](../15-ef-core/overview.en.md)
- Replacing services: [13 · DI, configuration, logging](../13-di-config-logging/overview.en.md)
- Smoke tests and health checks: [22 · Observability](../22-observability/overview.en.md)
- The same idea in other tracks: [Python](../../../python/overview.en.md) — FastAPI's `TestClient`
- Recipes and libraries: [NSubstitute](../../libraries/nsubstitute/overview.en.md), [Testcontainers](../../libraries/testcontainers/overview.en.md), [Resilient HttpClient](../../recipes/http-client-resilience/overview.en.md), [JWT authentication](../../recipes/jwt-auth/overview.en.md)
