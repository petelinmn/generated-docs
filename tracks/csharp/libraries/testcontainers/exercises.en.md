---
title: "Exercises: Testcontainers"
lang: en
track: csharp
kind: library
doc: exercises
slug: testcontainers
part: 2
tags: [testcontainers, docker, integration-testing, xunit-fixture]
status: draft
updated: 2026-11-22
---

# Exercises: Testcontainers

🌐 [Русский](exercises.ru.md) · **English**

> ⚠️ **Docker is required.** The solutions below, except the "verified" points, were **not run** in the preparation environment (Docker is unavailable); they must be run on a machine with Docker. There are no runnable versions in `code/` (level 5 does not exist). Packages: `Testcontainers.PostgreSql` 4.17.0, `Testcontainers.Redis` 4.17.0, `Npgsql` 11.0.0.

## 1. 🟢 A PostgreSQL container

**Task:** create a PostgreSQL 16 container with the database `app`, user `u`, password `p`, start it, run `SELECT 1` and dispose of it.

**Expected result:** the query returns `1`; the container is removed after the test.

<details><summary>Hint</summary>

`new PostgreSqlBuilder("postgres:16-alpine")…Build()`, `StartAsync()`, `GetConnectionString()`, `NpgsqlConnection`, `DisposeAsync()`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 1: a throw-away PostgreSQL container.
var pg = new PostgreSqlBuilder("postgres:16-alpine")
    .WithDatabase("app").WithUsername("u").WithPassword("p")
    .Build();
await pg.StartAsync();
await using var conn = new NpgsqlConnection(pg.GetConnectionString());
await conn.OpenAsync();
await using var cmd = new NpgsqlCommand("SELECT 1", conn);
var one = (int)(await cmd.ExecuteScalarAsync())!;
await pg.DisposeAsync();
```

Verified only that this code **compiles** (Testcontainers 4.17.0, Npgsql 11.0.0); running it requires Docker.

</details>

## 2. 🟡 Behavior without Docker

**Task:** on a machine without Docker call `Build()` for a container. Which exception do you get? How do you write a "Docker is available" check?

**Expected result:** `DockerUnavailableException`; the check catches it and returns `false`.

<details><summary>Hint</summary>

The exception occurs already on `Build()`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 2: detect a missing Docker engine.
static bool DockerAvailable()
{
    try { _ = new ContainerBuilder("hello-world").Build(); return true; }
    catch (DockerUnavailableException) { return false; }
}
```

Verified: on this machine (without Docker) `Build()` threw `DockerUnavailableException` ("Docker is either not running or misconfigured… Failed to connect to Docker endpoint at 'npipe://./pipe/docker_engine'"), and `DockerAvailable()` returned `False`.

</details>

## 3. 🟡 A fixture

**Task:** package the PostgreSQL container as an xUnit fixture shared by a test class.

**Expected result:** the container starts once per class; tests get `ConnectionString`.

<details><summary>Hint</summary>

A class with `IAsyncLifetime` (in xUnit v3 the methods return `ValueTask`), in the test class `IClassFixture<PostgresFixture>`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 3: one container per test class.
public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _pg = new PostgreSqlBuilder("postgres:16-alpine").Build();
    public string ConnectionString => _pg.GetConnectionString();
    public async ValueTask InitializeAsync() => await _pg.StartAsync();
    public async ValueTask DisposeAsync() => await _pg.DisposeAsync();
}

public class RepoTests(PostgresFixture fx) : IClassFixture<PostgresFixture> { /* tests use fx.ConnectionString */ }
```

> ⚠️ Verify: the `IAsyncLifetime` signatures in your xUnit version; the fixture as a whole was not run. Note: the field initializer calls `Build()`, which without Docker throws on construction of the fixture.

</details>

## 4. 🔴 A generic container with waiting

**Task:** configure a generic `nginx:alpine` container with a random host port and waiting for a successful HTTP response on port 80.

**Expected result:** after `StartAsync` the container answers HTTP.

<details><summary>Hint</summary>

`ContainerBuilder`, `WithPortBinding(8080, true)`, `WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(80)))`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 4: a generic container with a wait strategy.
var generic = new ContainerBuilder("nginx:alpine")
    .WithPortBinding(8080, true)
    .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(80)))
    .Build();
```

Verified only that the code compiles; running and the real waiting require Docker.

</details>
