---
title: "Testcontainers: advanced"
lang: en
track: csharp
kind: library
doc: advanced
slug: testcontainers
part: 2
tags: [testcontainers, xunit-fixture, iasynclifetime, webapplicationfactory, networks]
versions: ".NET 11, C# 15; Testcontainers 4.17.0"
status: draft
updated: 2026-11-22
---

# Testcontainers: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [basics](basics.en.md). **Time:** ~45 min.

> ⚠️ **Verification status.** Docker is unavailable in the preparation environment: all examples below, except those explicitly marked "verified", were **not run**; they must be checked on a machine with Docker. The material stays `draft`.

## Container lifetime

Starting a container takes seconds. Starting one per test is too expensive — use one container per class or test set (a fixture) and clean data between tests.

```csharp
public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _pg = new PostgreSqlBuilder("postgres:16-alpine").Build();
    public string ConnectionString => _pg.GetConnectionString();

    public async ValueTask InitializeAsync() => await _pg.StartAsync();
    public async ValueTask DisposeAsync() => await _pg.DisposeAsync();
}
```

> ⚠️ Verify: `IAsyncLifetime` in xUnit v3 uses `ValueTask` (in xUnit v2 — `Task`); check against your xUnit version and the documentation on `ICollectionFixture`/`IClassFixture` ([16 · Unit testing](../../modules/16-testing-unit/overview.en.md)). The fixture as a whole was not run.

Verified by compiling (4.17.0): the `PostgreSqlContainer` type, the builder with the image in its constructor, `Build()`.

## Skipping tests without Docker

Verified: without Docker `Build()` throws `DockerUnavailableException`. Integration tests must not break a local run without Docker:

```csharp
static bool DockerAvailable()
{
    try { _ = new ContainerBuilder("hello-world").Build(); return true; }
    catch (DockerUnavailableException) { return false; }
}
```

Use the result for a conditional skip (in xUnit v3 — a dynamic skip through `Assert.Skip`/an attribute depending on the version). Under this repository's rules integration tests are marked explicitly and skipped if the environment is unavailable. Verified by running on this machine: `DockerAvailable: False`.

> ⚠️ Verify: the skipping mechanism itself in your xUnit version (`Assert.Skip`, `SkippableFact`) — not run.

## Substituting the DB in `WebApplicationFactory`

A typical scenario: an ASP.NET Core application is tested as a whole, and the connection string is replaced with the container's:

```csharp
builder.ConfigureServices(s => { /* replace DbContext options with fixture.ConnectionString */ });
```

See [17 · Integration testing](../../modules/17-testing-integration/overview.en.md) on `WebApplicationFactory`; migrations are applied to the container's empty DB at the start of the test set ([15 · EF Core](../../modules/15-ef-core/overview.en.md)).

> ⚠️ Verify: the `WebApplicationFactory` + container combination was not run in practice.

## Several containers and networks

To connect services (an application + a DB + a broker) containers are put into one network (`NetworkBuilder`) and address each other by network alias.

> ⚠️ Verify: `NetworkBuilder`, `WithNetwork`, `WithNetworkAliases` — check the method names against the documentation, they were not run.

## Cleaning data between tests

- A transaction rolled back per test (fast, but not for code that manages transactions itself).
- Cleaning tables (`TRUNCATE`) or a library such as Respawn.
- A new database/schema per test from a template (isolation, more expensive).

> ⚠️ Verify: Respawn and template databases were not run in this material.

## Resources and images

- Pin image versions (`postgres:16-alpine`, not `latest`).
- In CI pre-pull images (`docker pull`) or use a cache — the first download takes minutes.
- A "watchdog" container cleans up leftover containers when the test run terminates abnormally (in Testcontainers it is a separate service container).

> ⚠️ Verify: the behavior of the cleanup service container was not verified in this material.

## Best practices

| Rule | Why |
|---|---|
| One container per test set | speed |
| The connection string from the container | the port is random |
| A pinned image version | reproducibility |
| Skip when Docker is absent | a local run does not fail |
| Clean data between tests | isolation |
| Do not log container passwords | security (they are throw-away, but the habit matters) |

## Related materials

- Back: [basics](basics.en.md). Next: [expert](expert.en.md).
- [17 · Integration testing](../../modules/17-testing-integration/overview.en.md)
- [16 · Unit testing](../../modules/16-testing-unit/overview.en.md) — xUnit fixtures.
- [Linux](../../../linux/overview.en.md) — containers and control groups.

## Sources

- [Testcontainers for .NET: xUnit.net](https://dotnet.testcontainers.org/test_frameworks/xunit_net/)
- [Testcontainers for .NET: Networks](https://dotnet.testcontainers.org/api/networks/)
- [Respawn](https://github.com/jbogard/Respawn)
