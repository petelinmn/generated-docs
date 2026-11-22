---
title: "Integration tests: expert"
lang: en
track: csharp
kind: module
doc: expert
slug: 17-testing-integration
part: 2
tags: [integration-testing, web-application-factory, test-server, host-factory, test-pyramid, flaky-tests, interview-questions]
versions: ".NET 11, C# 15; xunit.v3 4.0.3; Microsoft.AspNetCore.Mvc.Testing 11.0.0"
status: draft
updated: 2026-11-22
---

# Integration tests: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

**Prerequisites:** [basic](basics.en.md) and [advanced](advanced.en.md) theory. **Time:** ~1 h.

## How WebApplicationFactory is built

`WebApplicationFactory<TEntryPoint>` finds the application's entry point (for a `Program` with top-level statements — through the `Program` type, so it must be accessible) and **builds the host** with the same code as a normal start, but instead of Kestrel it plugs in the **`TestServer`** — an in-memory implementation of `IServer`. There are no network sockets: the `HttpClient` that `CreateClient()` returns uses a special handler that passes requests directly into the application pipeline (verified: the client address is `http://localhost/`, the server is `TestServer`).

- **Lazy creation.** The host is built not in the factory's constructor but on the first access to `CreateClient()`, `Services` or `Server`. Verified: the `ConfigureWebHost` call counter is `0` before `CreateClient()` and `1` after.
- **Environment.** `Development` by default (verified); overridden with `builder.UseEnvironment("Staging")` in `ConfigureWebHost` (verified: `env=Staging`). In `Development` the DI scope checks and detailed error pages are on — tests see the same container configuration errors a developer does.
- **Configuration order.** First the application code (`Program`) runs, then the factory's `ConfigureWebHost` and `WithWebHostBuilder`; registrations in `ConfigureTestServices` come **after** the application's registrations and override them; an ordinary `ConfigureServices` in `ConfigureWebHost` runs earlier.
- **The content root.** The factory determines the content root (by the `WebApplicationFactoryContentRootAttribute` attribute or by the project directory) — important for static files and the application's `appsettings.json`.
- **`Services`** is the application's root provider: create a scope from it (`CreateScope()`) to access the `DbContext` or services directly, as in the verified `ProviderName` test.
- **Disposal.** The factory's `Dispose`/`DisposeAsync` stops the host; resources you created yourself (the SQLite connection) are released in an overridden `Dispose(bool)`.

## What WebApplicationFactory does not check

An in-memory host does not go through the network, TLS, real Kestrel limits (body size, timeouts) or a reverse proxy. So deployment configuration and behavior under load are checked separately: with smoke tests after deployment and load tests.

## Reference

| What | API |
|---|---|
| Running the application | `WebApplicationFactory<Program>`, `CreateClient()`, `CreateDefaultClient()` |
| Host settings | `ConfigureWebHost`, `WithWebHostBuilder`, `UseEnvironment`, `ConfigureAppConfiguration` |
| Replacing services | `ConfigureTestServices` (`Microsoft.AspNetCore.TestHost`), `RemoveAll<T>()` |
| The client | `WebApplicationFactoryClientOptions` (`AllowAutoRedirect`, `BaseAddress`, `HandleCookies`) |
| Application services | `factory.Services`, `factory.Server` |
| xUnit fixtures | `IClassFixture<T>`, `ICollectionFixture<T>`, `IAsyncLifetime` |
| Databases | in-memory SQLite, EF Core InMemory, Testcontainers |

## Gotchas

**1. A shared database in `IClassFixture`.** The data is visible to all tests of the class; tests that count "everything in the table" give different results in different orders. Isolate (see the [advanced theory](advanced.en.md#data-isolation-between-tests)).

**2. The in-memory SQLite connection.** The database vanishes when the connection closes: if the context opens and closes the connection itself, each request will see an empty database. Pass an open connection (`UseSqlite(_connection)`).

**3. `Program` is not visible.** A compile error with `WebApplicationFactory<Program>`: you need `public partial class Program { }`.

**4. `ConfigureTestServices` is not found** without `using Microsoft.AspNetCore.TestHost;` (verified: `CS1061`).

**5. The EF Core InMemory provider is "too kind".** It does not check constraints, foreign keys or SQL — the test is green and production fails. For database logic take a relational provider.

**6. Background services** (`BackgroundService`) start with the host and may interfere with a test: disable them in `ConfigureTestServices`.

**7. Flaky tests.** Causes: shared data, real time, races with background processing, order. The cure is isolation and controlled dependencies, not retries and `Thread.Sleep`.

**8. Start-up time.** Every new factory is a new application start (in the verified tests about a second per start); do not create one for every small thing.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| `WebApplicationFactory` + in-memory SQLite | fast, no external dependencies | the dialect differs from the production DBMS |
| `WebApplicationFactory` + Testcontainers | a real DBMS | needs Docker, slower |
| A real server + external tests (Playwright, k6) | checks the deployed system | harder, slower, flakier |
| Unit tests only | fast | do not catch integration errors |
| Contract tests (Pact) | independent services | a separate tool |

## Interview questions

<details><summary>1. How does an integration test differ from a unit test?</summary>

A unit test checks a unit in isolation, with doubles. An integration test runs the application or part of it with real components (routes, DI, a database) and checks through the external interface. It is slower but catches errors "at the seams".

</details>

<details><summary>2. What are `WebApplicationFactory` and `TestServer`?</summary>

The factory builds the application's host for a test; `TestServer` is an in-memory server with no network. `CreateClient()` returns an `HttpClient` that sends requests straight into the pipeline.

</details>

<details><summary>3. Why is `public partial class Program` needed?</summary>

With top-level statements the `Program` class is generated as internal; to let `WebApplicationFactory<Program>` see it from another project, the type is exposed through a partial declaration.

</details>

<details><summary>4. How do you replace a service in a test?</summary>

In `ConfigureWebHost` call `builder.ConfigureTestServices(...)`: remove the registration (`RemoveAll<T>()`) and add your own. Verified: the clock and the database were replaced.

</details>

<details><summary>5. How do you isolate data between tests?</summary>

A new factory/database per test, clearing tables, unique data, transaction rollback. Verified: a factory per test via `IAsyncLifetime` — each test sees only its own data.

</details>

<details><summary>6. Which database should integration tests use?</summary>

For logic — in-memory SQLite (real SQL, fast); for fidelity — a real DBMS in a container; EF Core InMemory — only when constraints and SQL are not needed.

</details>

<details><summary>7. What is `IClassFixture` for a factory and what is dangerous about it?</summary>

One factory per class: fast, but the state (database, singletons) is shared by all tests of the class, so isolation is needed.

</details>

<details><summary>8. How do you override configuration in one test?</summary>

`factory.WithWebHostBuilder(b => b.ConfigureAppConfiguration(...))` creates a derived factory (verified: `Feature:Flag = on`).

</details>

<details><summary>9. What does `WebApplicationFactory` not check?</summary>

The network, TLS, real Kestrel and a proxy, the behavior of a deployed environment — these are checked with smoke tests after deployment.

</details>

<details><summary>10. Trick question: a test created `new MyFactory()`, but `ConfigureWebHost` was not called. Why?</summary>

The host is built lazily — on the first `CreateClient()`, `Services` or `Server` (verified: the counter `0` → `1`).

</details>

<details><summary>11. Trick question: a test with in-memory SQLite sees an empty database on every request. Why?</summary>

Each new connection to `:memory:` is a separate database. You must open one connection and pass it into `UseSqlite(connection)`, keeping it open.

</details>

<details><summary>12. Trick question: a test is green on the InMemory provider, but in production it fails on a unique index. Why?</summary>

InMemory does not check constraints and does not execute SQL; the divergence comes from a relational DBMS. Use SQLite/a container.

</details>

## Related materials

- Back: [advanced theory](advanced.en.md). Check yourself: [quiz](quiz.en.md) and [exercises](exercises.en.md).
- [16 · Unit tests](../16-testing-unit/overview.en.md) — the test pyramid.
- [14 · ASP.NET Core: Minimal API](../14-aspnet-core-minimal-api/overview.en.md) — the application under test.
- [15 · EF Core](../15-ef-core/overview.en.md) — providers and migrations.
- [Python](../../../python/overview.en.md) — testing FastAPI through `TestClient`.

## Sources

- [Integration tests in ASP.NET Core](https://learn.microsoft.com/aspnet/core/test/integration-tests)
- [Choosing an EF Core testing strategy](https://learn.microsoft.com/ef/core/testing/choosing-a-testing-strategy)
- [`TestServer`](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.testhost.testserver)
