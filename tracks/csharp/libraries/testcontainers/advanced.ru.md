---
title: "Testcontainers: продвинутый уровень"
lang: ru
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

# Testcontainers: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [основы](basics.ru.md). **Время:** ~45 мин.

> ⚠️ **Статус проверки.** Docker в среде подготовки недоступен: все примеры ниже, кроме явно отмеченных «проверено», **не запускались**; их нужно проверить на машине с Docker. Материал остаётся `draft`.

## Время жизни контейнера

Запуск контейнера занимает секунды. Поднимать его на каждый тест слишком дорого — используйте один контейнер на класс или набор тестов (фикстуру) и чистите данные между тестами.

```csharp
public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _pg = new PostgreSqlBuilder("postgres:16-alpine").Build();
    public string ConnectionString => _pg.GetConnectionString();

    public async ValueTask InitializeAsync() => await _pg.StartAsync();
    public async ValueTask DisposeAsync() => await _pg.DisposeAsync();
}
```

> ⚠️ Проверить: интерфейс `IAsyncLifetime` в xUnit v3 использует `ValueTask` (в xUnit v2 — `Task`); сверьтесь с версией вашего xUnit и документацией по `ICollectionFixture`/`IClassFixture` ([16 · Юнит-тестирование](../../modules/16-testing-unit/overview.ru.md)). Код фикстуры целиком не запускался.

Проверено компиляцией (4.17.0): тип `PostgreSqlContainer`, билдер с образом в конструкторе, `Build()`.

## Пропуск тестов без Docker

Проверено: без Docker `Build()` бросает `DockerUnavailableException`. Интеграционные тесты не должны ломать локальный прогон без Docker:

```csharp
static bool DockerAvailable()
{
    try { _ = new ContainerBuilder("hello-world").Build(); return true; }
    catch (DockerUnavailableException) { return false; }
}
```

Результат используйте для условного пропуска (в xUnit v3 — динамический пропуск через `Assert.Skip`/атрибут в зависимости от версии). По правилам этого репозитория интеграционные тесты помечаются явно и пропускаются, если среда недоступна.

> ⚠️ Проверить: сам способ пропуска в вашей версии xUnit (`Assert.Skip`, `SkippableFact`) — не запускался.

## Подмена БД в `WebApplicationFactory`

Типичный сценарий: приложение ASP.NET Core тестируется целиком, а строка подключения подменяется на контейнерную:

```csharp
builder.ConfigureServices(s => { /* replace DbContext options with fixture.ConnectionString */ });
```

См. [17 · Интеграционное тестирование](../../modules/17-testing-integration/overview.ru.md) про `WebApplicationFactory`; миграции применяются к пустой БД контейнера в начале набора тестов ([15 · EF Core](../../modules/15-ef-core/overview.ru.md)).

> ⚠️ Проверить: связку `WebApplicationFactory` + контейнер на практике не запускали.

## Несколько контейнеров и сети

Для связки сервисов (приложение + БД + брокер) контейнеры объединяют в одну сеть (`NetworkBuilder`) и обращаются друг к другу по сетевому алиасу.

> ⚠️ Проверить: `NetworkBuilder`, `WithNetwork`, `WithNetworkAliases` — названия методов сверяйте с документацией, они не запускались.

## Очистка данных между тестами

- Транзакция с откатом на тест (быстро, но не для кода, который сам управляет транзакциями).
- Очистка таблиц (`TRUNCATE`) или библиотека вроде Respawn.
- Новая база/схема на тест из шаблона (изоляция, дороже).

> ⚠️ Проверить: Respawn и шаблонные базы в этом материале не запускали.

## Ресурсы и образы

- Фиксируйте версии образов (`postgres:16-alpine`, не `latest`).
- В CI заранее загружайте образы (`docker pull`) или используйте кэш — первая загрузка занимает минуты.
- Контейнер-«сторож» очищает оставшиеся контейнеры при аварийном завершении тестов (в Testcontainers это отдельный служебный контейнер).

> ⚠️ Проверить: поведение служебного контейнера очистки в этом материале не проверялось.

## Best practices

| Правило | Почему |
|---|---|
| Один контейнер на набор тестов | скорость |
| Строка подключения из контейнера | порт случайный |
| Фиксированная версия образа | воспроизводимость |
| Пропуск при отсутствии Docker | локальный прогон не падает |
| Очистка данных между тестами | изоляция |
| Не логировать пароли контейнера | безопасность (одноразовые, но привычка важна) |

## Связанные материалы

- Назад: [основы](basics.ru.md). Дальше: [эксперт](expert.ru.md).
- [17 · Интеграционное тестирование](../../modules/17-testing-integration/overview.ru.md)
- [16 · Юнит-тестирование](../../modules/16-testing-unit/overview.ru.md) — фикстуры xUnit.
- [Linux](../../../linux/overview.ru.md) — контейнеры и контрольные группы.

## Источники

- [Testcontainers for .NET: xUnit.net](https://dotnet.testcontainers.org/test_frameworks/xunit_net/)
- [Testcontainers for .NET: Networks](https://dotnet.testcontainers.org/api/networks/)
- [Respawn](https://github.com/jbogard/Respawn)
