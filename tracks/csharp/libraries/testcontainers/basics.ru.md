---
title: "Testcontainers: основы"
lang: ru
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

# Testcontainers: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [17 · Интеграционное тестирование](../../modules/17-testing-integration/overview.ru.md), знакомство с Docker. **Время:** ~30 мин.

> ⚠️ **Статус проверки.** В среде, где готовился этот материал, **Docker недоступен**, поэтому ни один контейнер не запускался. Проверено: пакеты ставятся, код с типизированными билдерами компилируется, а без Docker вызов `Build()` бросает исключение (см. ниже). Всё, что описывает работу *с запущенным* контейнером (строки подключения, порты, ожидание готовности, очистка), взято из документации и помечено как непроверенное. Материал остаётся в статусе `draft`.

## Что это и зачем

Testcontainers поднимает **настоящие** зависимости (БД, Redis, брокеры) в одноразовых Docker-контейнерах прямо из теста. Интеграционные тесты идут против реального PostgreSQL, а не против подмены или SQLite-«заглушки», поэтому ловят ошибки миграций, SQL-диалектов и транзакций. После теста контейнер удаляется.

```bash
dotnet add package Testcontainers.PostgreSql   # typed module for PostgreSQL
dotnet add package Testcontainers              # generic API
```

## Модуль PostgreSQL

```csharp
var pg = new PostgreSqlBuilder("postgres:16-alpine")
    .WithDatabase("app").WithUsername("u").WithPassword("p")
    .Build();

await pg.StartAsync();
var cs = pg.GetConnectionString();     // host, mapped port, credentials
// ... run the code under test with cs ...
await pg.DisposeAsync();               // stops and removes the container
```

Проверено компиляцией: `PostgreSqlBuilder` принимает образ в конструкторе (в версии 4.17), доступны `WithDatabase/WithUsername/WithPassword`, результат — `PostgreSqlContainer`. Аналогично `RedisBuilder("redis:7-alpine")`.

> ⚠️ Проверить: `StartAsync`, `GetConnectionString()`, `DisposeAsync` и реальное поведение запущенного контейнера не запускались (нет Docker).

## Общий контейнер и ожидание готовности

```csharp
var generic = new ContainerBuilder("nginx:alpine")
    .WithPortBinding(8080, true)          // assign a random host port
    .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(80)))
    .Build();
```

Проверено компиляцией: `ContainerBuilder`, `WithPortBinding`, `WithWaitStrategy(Wait.ForUnixContainer()…)` существуют в 4.17. Стратегия ожидания определяет, когда `StartAsync` считает контейнер готовым (порт открыт, HTTP отвечает, строка в логе).

> ⚠️ Проверить: фактическое ожидание и случайные порты хоста.

## Что будет без Docker

Проверено запуском: уже вызов `Build()` (до `StartAsync`) бросает `DotNet.Testcontainers.Builders.DockerUnavailableException` с текстом «Docker is either not running or misconfigured…» и деталями «Failed to connect to Docker endpoint at 'npipe://./pipe/docker_engine'». Тест без Docker падает сразу — поэтому интеграционные тесты в CI нужно помечать/пропускать, если Docker нет ([продвинутый](advanced.ru.md#пропуск-тестов-без-docker)).

## Частые ошибки новичка

- ❌ Запускать тесты на машине без Docker и ждать зелёных → ✅ условный пропуск или CI с Docker.
- ❌ Зашивать порт БД (5432) → ✅ брать строку подключения из контейнера (порт хоста случайный).
- ❌ Поднимать контейнер на каждый тест → ✅ один на класс/набор тестов ([продвинутый](advanced.ru.md#время-жизни-контейнера)).
- ❌ Использовать тег `latest` → ✅ фиксировать версию образа.
- ❌ Не освобождать контейнер → ✅ `DisposeAsync` (или `IAsyncLifetime`).

## Связанные материалы

- Дальше: [продвинутый](advanced.ru.md) — время жизни, фикстуры, сети, пропуск без Docker.
- [17 · Интеграционное тестирование](../../modules/17-testing-integration/overview.ru.md) — `WebApplicationFactory` и подмена зависимостей.
- [15 · EF Core](../../modules/15-ef-core/overview.ru.md) — миграции и реальная БД.
- [Dapper](../dapper/overview.ru.md) — SQL против настоящей БД.
- [Kubernetes](../../../kubernetes/overview.ru.md) — контейнеры в продакшене.

## Источники

- [Testcontainers for .NET](https://dotnet.testcontainers.org/)
- [Testcontainers for .NET: Quickstart](https://dotnet.testcontainers.org/#getting-started)
- [Testcontainers for .NET: Modules (PostgreSQL)](https://dotnet.testcontainers.org/modules/postgres/)
