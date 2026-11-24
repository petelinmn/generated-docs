---
title: "Testcontainers"
lang: en
track: csharp
kind: library
doc: overview
slug: testcontainers
part: 2
levels: [1, 2, 3, 4]
tags: [testcontainers, docker, integration-testing, postgresql]
versions: ".NET 11, C# 15; Testcontainers 4.17.0"
status: draft
updated: 2026-11-22
---

# Testcontainers

🌐 [Русский](overview.ru.md) · **English**

> ⚠️ **Verification status.** Docker is unavailable in the preparation environment, no containers were started. Verified: installing the packages, compiling the builders and `DockerUnavailableException` on `Build()` without Docker. The rest is per the documentation; the material stays `draft` until it is checked on a machine with Docker.

## In a nutshell

Testcontainers starts real dependencies (databases, Redis, brokers) in throw-away Docker containers from test code: integration tests run against a real PostgreSQL rather than a substitute. There are typed modules (`Testcontainers.PostgreSql`, `.Redis`…) and a generic `ContainerBuilder` with wait strategies. One container per test set, the connection string from the container, a skip without Docker.

## Key ideas

- `new PostgreSqlBuilder("postgres:16-alpine")…Build()`, `StartAsync`, `GetConnectionString()`, `DisposeAsync`.
- Host ports are random — do not hard-code.
- A wait strategy: "started" ≠ "ready".
- A fixture per class/set + cleaning data between tests.
- Without Docker `Build()` throws `DockerUnavailableException` (verified).
- Pinned image versions, image caching in CI.

## Minimal example

```csharp
var pg = new PostgreSqlBuilder("postgres:16-alpine").Build();
await pg.StartAsync();
var cs = pg.GetConnectionString();
await pg.DisposeAsync();
```

## When to use and when not

- ✅ Integration tests with a real DB/Redis/broker.
- ✅ Verifying migrations and the SQL dialect.
- ✅ A reproducible environment "from code".
- ❌ Fast unit tests (substitutes are enough).
- ❌ Environments without Docker.
- ❌ Production-scale load tests.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | the PostgreSQL module, a generic container, behavior without Docker |
| [2. Advanced](advanced.en.md) | fixtures, skipping without Docker, `WebApplicationFactory`, networks, data cleanup |
| [3. Expert](expert.en.md) | internals, gotchas, alternatives, 11 interview questions |
| [4. Self-check](quiz.en.md) | 10 questions and [4 exercises](exercises.en.md) with solutions |
| 5. Runnable examples | not generated yet — `/learn csharp testcontainers level 5` |
| 6. Tests | not generated yet — `/learn csharp testcontainers level 6` |

## Related materials

- Prerequisites: [17 · Integration testing](../../modules/17-testing-integration/overview.en.md)
- Nearby: [15 · EF Core](../../modules/15-ef-core/overview.en.md), [Dapper](../dapper/overview.en.md), [NSubstitute](../nsubstitute/overview.en.md), [MassTransit](../masstransit/overview.en.md) — a real RabbitMQ in tests
- In other tracks: [Kubernetes](../../../kubernetes/overview.en.md) — containers and kind; [Containers and Docker](../../../kubernetes/modules/01-containers/overview.en.md) — the images and containers that Testcontainers starts
