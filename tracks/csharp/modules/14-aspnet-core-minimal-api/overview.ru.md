---
title: "ASP.NET Core: Minimal API"
lang: ru
track: csharp
kind: module
doc: overview
slug: 14-aspnet-core-minimal-api
part: 2
levels: [1, 2, 3, 4]
tags: [aspnet-core, minimal-api, routing, middleware, typed-results, problem-details, openapi]
versions: ".NET 11, C# 15; ASP.NET Core 11.0; Microsoft.AspNetCore.OpenApi 11.0.0"
status: draft
updated: 2026-11-22
---

# ASP.NET Core: Minimal API

🌐 **Русский** · [English](overview.en.md)

## Коротко

Minimal API — способ писать HTTP-сервисы на ASP.NET Core, объявляя конечные точки лямбдами в `Program.cs`: `app.MapGet("/hello/{name}", (string name) => …)`. Фреймворк сам привязывает параметры из маршрута, строки запроса, тела JSON и DI, а возвращаемое значение превращает в ответ. Под капотом тот же Kestrel, конвейер middleware и маршрутизация, что и у контроллеров, но с минимумом церемоний и поддержкой Native AOT.

## Ключевые идеи

- **Конечная точка** — `MapGet/Post/Put/Delete(шаблон, делегат)`; группы `MapGroup` собирают общие префиксы и фильтры.
- **Привязка параметров:** маршрут → строка запроса → тело JSON → сервисы DI; ошибки привязки — `400`, неподходящий `Content-Type` — `415`.
- **Результаты:** `TypedResults` и `Results<…>` дают точные типы ответов и корректный OpenAPI.
- **Middleware** выполняются в порядке регистрации; обработчик ошибок ставят первым.
- **Фильтры конечных точек** видят типизированные аргументы выбранной точки и могут заменить результат.
- **Ошибки:** `UseExceptionHandler` + ProblemDetails (RFC 9457); валидация DataAnnotations — `AddValidation()` (с .NET 10).
- **OpenAPI:** `AddOpenApi()` и `MapOpenApi()` публикуют описание API (`/openapi/v1.json`).

## Минимальный пример

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/hello/{name}", (string name) => $"hello {name}");
app.MapGet("/search", (string? q, int page = 1) => $"q={q ?? "-"} page={page}");

app.Run();
```

## Когда применять, а когда нет

- ✅ Небольшие и средние HTTP API, микросервисы, backend-for-frontend.
- ✅ Сервисы, которые собираются в Native AOT или должны быстро стартовать.
- ✅ Сочетание с группами, фильтрами и методами-расширениями для структуры.
- ❌ Огромный проект без собственных соглашений: код разрастается в `Program.cs`.
- ❌ Нужны богатые конвенции MVC (модели представлений, Razor Views) — используйте MVC/Razor Pages.
- ❌ Возврат сущностей БД напрямую вместо DTO.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | первое приложение, маршрутизация и ограничения, привязка параметров, JSON, результаты, группы, конвейер middleware |
| [2. Продвинутая теория](advanced.ru.md) | `TypedResults` и `Results<…>`, `[AsParameters]`, фильтры, валидация и ProblemDetails, `IExceptionHandler`, OpenAPI, структура приложения |
| [3. Экспертная теория](expert.ru.md) | хост и конвейер, генерация `RequestDelegate`, правила привязки и коды ответов, порядок middleware, AOT, подвохи, 13 вопросов с собеседований |
| [4. Самопроверка](quiz.ru.md) | 11 вопросов и [5 упражнений](exercises.ru.md) с решениями |
| 5. Запускаемые примеры | ещё не создан — `/learn csharp 14-aspnet-core-minimal-api level 5` |
| 6. Тесты | ещё не создан — `/learn csharp 14-aspnet-core-minimal-api level 6` |

## Связанные материалы

- Пререквизиты: [13 · DI, конфигурация, логирование](../13-di-config-logging/overview.ru.md), [10 · Async/await](../10-async-await/overview.ru.md)
- Дальше: [15 · EF Core](../15-ef-core/overview.ru.md), [17 · Тестирование: интеграционные тесты](../17-testing-integration/overview.ru.md)
- Тестирование: [16 · Unit-тесты](../16-testing-unit/overview.ru.md)
- Native AOT: [19 · Производительность](../19-performance/overview.ru.md)
- Генераторы для AOT: [20 · Рефлексия и генераторы кода](../20-reflection-and-source-generators/overview.ru.md)
- Структура приложения: [21 · Архитектура приложений](../21-architecture/overview.ru.md)
- Health checks и трассировка: [22 · Наблюдаемость](../22-observability/overview.ru.md)
- То же в других треках: [Python](../../../python/overview.ru.md) — FastAPI
- Рецепты и библиотеки: [FluentValidation](../../libraries/fluentvalidation/overview.ru.md), [Кэширование](../../recipes/caching/overview.ru.md), [Загрузка файлов](../../recipes/file-upload/overview.ru.md), [gRPC-сервис](../../recipes/grpc-service/overview.ru.md), [JWT-аутентификация](../../recipes/jwt-auth/overview.ru.md), [Пагинация и фильтрация](../../recipes/pagination-filtering/overview.ru.md), [Rate limiting](../../recipes/rate-limiting/overview.ru.md), [Real-time через SignalR](../../recipes/signalr-realtime/overview.ru.md)
