---
title: "Упражнения: Testcontainers"
lang: ru
track: csharp
kind: library
doc: exercises
slug: testcontainers
part: 2
tags: [testcontainers, docker, integration-testing, xunit-fixture]
status: draft
updated: 2026-11-22
---

# Упражнения: Testcontainers

🌐 **Русский** · [English](exercises.en.md)

> ⚠️ **Требуется Docker.** Решения ниже, кроме пунктов «проверено», в среде подготовки **не запускались** (Docker недоступен); их нужно выполнить на машине с Docker. Запускаемых версий в `code/` нет (уровня 5 нет). Пакеты: `Testcontainers.PostgreSql` 4.17.0, `Testcontainers.Redis` 4.17.0, `Npgsql` 11.0.0.

## 1. 🟢 Контейнер PostgreSQL

**Задача:** создайте контейнер PostgreSQL 16 с базой `app`, пользователем `u`, паролем `p`, запустите, выполните `SELECT 1` и освободите.

**Ожидаемый результат:** запрос возвращает `1`; контейнер удалён после теста.

<details><summary>Подсказка</summary>

`new PostgreSqlBuilder("postgres:16-alpine")…Build()`, `StartAsync()`, `GetConnectionString()`, `NpgsqlConnection`, `DisposeAsync()`.

</details>

<details><summary>Решение</summary>

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

Проверено только то, что этот код **компилируется** (Testcontainers 4.17.0, Npgsql 11.0.0); выполнение требует Docker.

</details>

## 2. 🟡 Поведение без Docker

**Задача:** на машине без Docker вызовите `Build()` для контейнера. Какое исключение получите? Как написать проверку «Docker доступен»?

**Ожидаемый результат:** `DockerUnavailableException`; проверка ловит его и возвращает `false`.

<details><summary>Подсказка</summary>

Исключение возникает уже на `Build()`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 2: detect a missing Docker engine.
static bool DockerAvailable()
{
    try { _ = new ContainerBuilder("hello-world").Build(); return true; }
    catch (DockerUnavailableException) { return false; }
}
```

Проверено: на этой машине (без Docker) `Build()` бросил `DockerUnavailableException` («Docker is either not running or misconfigured… Failed to connect to Docker endpoint at 'npipe://./pipe/docker_engine'»).

</details>

## 3. 🟡 Фикстура

**Задача:** оформите контейнер PostgreSQL как фикстуру xUnit, общую для класса тестов.

**Ожидаемый результат:** контейнер поднимается один раз на класс; тесты получают `ConnectionString`.

<details><summary>Подсказка</summary>

Класс с `IAsyncLifetime` (в xUnit v3 методы возвращают `ValueTask`), в тестовом классе `IClassFixture<PostgresFixture>`.

</details>

<details><summary>Решение</summary>

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

> ⚠️ Проверить: сигнатуры `IAsyncLifetime` в вашей версии xUnit; фикстура целиком не запускалась.

</details>

## 4. 🔴 Общий контейнер с ожиданием

**Задача:** настройте универсальный контейнер `nginx:alpine` со случайным портом хоста и ожиданием успешного HTTP-ответа на порту 80.

**Ожидаемый результат:** после `StartAsync` контейнер отвечает на HTTP.

<details><summary>Подсказка</summary>

`ContainerBuilder`, `WithPortBinding(8080, true)`, `WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(80)))`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 4: a generic container with a wait strategy.
var generic = new ContainerBuilder("nginx:alpine")
    .WithPortBinding(8080, true)
    .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(80)))
    .Build();
```

Проверено только то, что код компилируется; запуск и реальное ожидание требуют Docker.

</details>
