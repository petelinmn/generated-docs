---
title: "Testcontainers: basics"
lang: en
track: csharp
kind: library
doc: basics
slug: testcontainers
part: 2
tags: [testcontainers, docker, integration-testing, postgresql, containers]
versions: ".NET 11, C# 15; Testcontainers 4.17.0, Testcontainers.PostgreSql 4.17.0, Testcontainers.Redis 4.17.0"
status: draft
updated: 2026-11-22
---

# Testcontainers: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [17 · Integration testing](../../modules/17-testing-integration/overview.en.md), familiarity with Docker. **Time:** ~30 min.

> ⚠️ **Verification status.** In the environment where this material was prepared **Docker is unavailable**, so not a single container was started. Verified: the packages install, code with the typed builders compiles, and without Docker a call to `Build()` throws an exception (see below). Everything describing work *with a running* container (connection strings, ports, readiness waiting, cleanup) comes from the documentation and is marked unverified. The material stays in the `draft` status.

## What it is and why

Testcontainers starts **real** dependencies (databases, Redis, brokers) in throw-away Docker containers straight from a test. Integration tests run against a real PostgreSQL rather than a substitute or a SQLite "stand-in", so they catch migration, SQL dialect and transaction errors. After the test the container is removed.

```bash
dotnet add package Testcontainers.PostgreSql   # typed module for PostgreSQL
dotnet add package Testcontainers              # generic API
```

## The PostgreSQL module

```csharp
var pg = new PostgreSqlBuilder("postgres:16-alpine")
    .WithDatabase("app").WithUsername("u").WithPassword("p")
    .Build();

await pg.StartAsync();
var cs = pg.GetConnectionString();     // host, mapped port, credentials
// ... run the code under test with cs ...
await pg.DisposeAsync();               // stops and removes the container
```

Verified by compiling: `PostgreSqlBuilder` takes the image in its constructor (in version 4.17), `WithDatabase/WithUsername/WithPassword` are available, the result is a `PostgreSqlContainer`. Likewise `RedisBuilder("redis:7-alpine")`.

> ⚠️ Verify: `StartAsync`, `GetConnectionString()`, `DisposeAsync` and the real behavior of a running container were not run (no Docker).

## A generic container and waiting for readiness

```csharp
var generic = new ContainerBuilder("nginx:alpine")
    .WithPortBinding(8080, true)          // assign a random host port
    .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(80)))
    .Build();
```

Verified by compiling: `ContainerBuilder`, `WithPortBinding`, `WithWaitStrategy(Wait.ForUnixContainer()…)` exist in 4.17. The wait strategy determines when `StartAsync` considers the container ready (the port is open, HTTP responds, a line in the log).

> ⚠️ Verify: the actual waiting and random host ports.

## What happens without Docker

Verified by running: already the call to `Build()` (before `StartAsync`) throws `DotNet.Testcontainers.Builders.DockerUnavailableException` with the text "Docker is either not running or misconfigured…" and the details "Failed to connect to Docker endpoint at 'npipe://./pipe/docker_engine'". A test without Docker fails at once — so integration tests in CI must be marked/skipped when Docker is absent ([advanced](advanced.en.md#skipping-tests-without-docker)).

## Beginner mistakes

- ❌ Running tests on a machine without Docker and expecting green → ✅ a conditional skip or CI with Docker.
- ❌ Hard-coding the DB port (5432) → ✅ take the connection string from the container (the host port is random).
- ❌ Starting a container per test → ✅ one per class/test set ([advanced](advanced.en.md#container-lifetime)).
- ❌ Using the `latest` tag → ✅ pin the image version.
- ❌ Not disposing the container → ✅ `DisposeAsync` (or `IAsyncLifetime`).

## Related materials

- Next: [advanced](advanced.en.md) — lifetime, fixtures, networks, skipping without Docker.
- [17 · Integration testing](../../modules/17-testing-integration/overview.en.md) — `WebApplicationFactory` and dependency substitution.
- [15 · EF Core](../../modules/15-ef-core/overview.en.md) — migrations and a real DB.
- [Dapper](../dapper/overview.en.md) — SQL against a real DB.
- [Kubernetes](../../../kubernetes/overview.en.md) — containers in production.

## Sources

- [Testcontainers for .NET](https://dotnet.testcontainers.org/)
- [Testcontainers for .NET: Quickstart](https://dotnet.testcontainers.org/#getting-started)
- [Testcontainers for .NET: Modules (PostgreSQL)](https://dotnet.testcontainers.org/modules/postgres/)
