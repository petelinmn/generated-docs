---
title: "Интеграционные тесты"
lang: ru
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

# Интеграционные тесты

🌐 **Русский** · [English](overview.en.md)

## Коротко

Интеграционный тест запускает приложение целиком (или значительную его часть) и проверяет его через внешний интерфейс. Для ASP.NET Core это `WebApplicationFactory<Program>`: она строит хост в памяти на `TestServer` (без сети) и выдаёт `HttpClient`. Внешние зависимости (время, почта, база) подменяют в `ConfigureTestServices`; базу берут SQLite in-memory или настоящую СУБД в контейнере. Такие тесты медленнее unit-тестов, но ловят ошибки «на стыках»: маршруты, DI, SQL, формат ответов.

## Ключевые идеи

- **`WebApplicationFactory<Program>`** строит хост лениво (при первом `CreateClient()`); окружение по умолчанию — `Development`.
- **`public partial class Program { }`** открывает `Program` тестам.
- **`ConfigureTestServices`** подменяет сервисы после регистраций приложения (`using Microsoft.AspNetCore.TestHost;`).
- **Изоляция данных:** фабрика в `IClassFixture` — одна на класс, база общая; для независимых тестов — фабрика на тест, очистка или уникальные данные.
- **База:** InMemory (не реляционная), SQLite in-memory (держите соединение открытым), Testcontainers (настоящая СУБД, нужен Docker).
- **Что проверять:** основные сценарии, коды ошибок и их формат, регистрацию DI, конфигурацию; мелочи — unit-тестами.
- **Не путать:** хост в памяти не проверяет сеть, TLS и поведение развёрнутой среды.

## Минимальный пример

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

## Когда применять, а когда нет

- ✅ Основные сценарии API «от запроса до базы».
- ✅ Проверка регистрации зависимостей, конфигурации и форматов ошибок.
- ✅ Работа с БД: ограничения, миграции, запросы.
- ❌ Каждая мелкая вариация логики — unit-тесты быстрее и точнее.
- ❌ Общая боевая база как тестовая.
- ❌ Тесты, зависящие от порядка и реального времени.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | зачем интеграционные тесты, `WebApplicationFactory`, первый тест, проверка ответа, подмена зависимостей |
| [2. Продвинутая теория](advanced.ru.md) | изоляция данных, конфигурация теста, выбор базы (InMemory, SQLite, Testcontainers), что проверять, практики |
| [3. Экспертная теория](expert.ru.md) | устройство фабрики и `TestServer`, ленивый хост, окружение, порядок настройки, подвохи, 12 вопросов с собеседований |
| [4. Самопроверка](quiz.ru.md) | 11 вопросов и [5 упражнений](exercises.ru.md) с решениями |
| 5. Запускаемые примеры | ещё не создан — `/learn csharp 17-testing-integration level 5` |
| 6. Тесты | ещё не создан — `/learn csharp 17-testing-integration level 6` |

## Связанные материалы

- Пререквизиты: [16 · Unit-тесты](../16-testing-unit/overview.ru.md), [14 · ASP.NET Core: Minimal API](../14-aspnet-core-minimal-api/overview.ru.md)
- Данные: [15 · EF Core](../15-ef-core/overview.ru.md)
- Подмена сервисов: [13 · DI, конфигурация, логирование](../13-di-config-logging/overview.ru.md)
- Smoke-тесты и health checks: [22 · Наблюдаемость](../22-observability/overview.ru.md)
- То же в других треках: [Python](../../../python/overview.ru.md) — `TestClient` FastAPI
