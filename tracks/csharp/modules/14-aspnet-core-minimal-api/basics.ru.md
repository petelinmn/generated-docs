---
title: "ASP.NET Core: Minimal API: базовая теория"
lang: ru
track: csharp
kind: module
doc: basics
slug: 14-aspnet-core-minimal-api
part: 2
tags: [aspnet-core, minimal-api, routing, middleware, model-binding, http-results]
versions: ".NET 11, C# 15; ASP.NET Core 11.0"
status: draft
updated: 2026-11-22
---

# ASP.NET Core: Minimal API: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [13 · DI, конфигурация, логирование](../13-di-config-logging/overview.ru.md), [09 · Делегаты, события, лямбды](../09-delegates-events-lambdas/overview.ru.md), [10 · Async/await](../10-async-await/overview.ru.md). **Время:** ~1 ч.

## Зачем это нужно

ASP.NET Core — веб-фреймворк .NET: принимает HTTP-запросы и возвращает ответы. **Minimal API** — способ описывать конечные точки (endpoints) прямо в `Program.cs` лямбдами, без контроллеров и атрибутов. Он строится на [универсальном хосте](../13-di-config-logging/overview.ru.md): DI, конфигурация и логирование — те же. Для небольших сервисов и API это самый короткий путь; для больших есть группы маршрутов, фильтры и разбиение по файлам.

Проект создаётся шаблоном `dotnet new web` (SDK `Microsoft.NET.Sdk.Web`); запуск — `dotnet run`.

## Первое приложение

```csharp
var builder = WebApplication.CreateBuilder(args);   // host: DI, configuration, logging
var app = builder.Build();

app.MapGet("/", () => "hello");                     // GET /  → text
app.MapGet("/hello/{name}", (string name) => $"hello {name}");   // route parameter

app.Run();                                          // starts Kestrel and blocks
```

`MapGet`, `MapPost`, `MapPut`, `MapDelete` связывают HTTP-метод и путь с обработчиком — **делегатом** (лямбда или метод). Возвращённое значение превращается в ответ: строка — `text/plain`, объект — JSON. Проверено на запущенном приложении: `GET /` → `200 text/plain hello`, `GET /hello/Ann` → `hello Ann`, `GET /json` с `new { Name = "Ann", Age = 30 }` → `application/json {"name":"Ann","age":30}` (имена свойств в camelCase). Неподдерживаемый метод — `405`: `PUT /` дал `405`.

## Маршрутизация

Параметры маршрута пишут в фигурных скобках; ограничения — после двоеточия:

```csharp
app.MapGet("/items/{id:int}", (int id) => $"item {id}");
```

Проверено: `/items/5` → `item 5`, а `/items/abc` → `404` — ограничение `:int` не пропускает значение, маршрут не находится. Другие ограничения: `guid`, `bool`, `min(1)`, `alpha`, `regex(...)`.

## Привязка параметров

Параметры обработчика заполняются автоматически из запроса, источник определяется по имени и типу:

| Источник | Когда |
|---|---|
| маршрут | имя параметра совпадает с `{name}` в пути |
| строка запроса | простой тип, которого нет в маршруте (`/search?q=x&page=3`) |
| тело (JSON) | сложный тип в `POST`/`PUT` |
| сервисы DI | зарегистрированный сервис (например, `ITodoStore`) |
| особые типы | `HttpContext`, `CancellationToken`, `ClaimsPrincipal` |

```csharp
app.MapGet("/search", (string? q, int page = 1) => $"q={q ?? "-"} page={page}");
```

Проверено: `/search` → `q=- page=1`; `/search?q=x&page=3` → `q=x page=3`; `/search?page=abc` → `400` (значение не приводится к `int`). Обязательный параметр без значения даёт `400`; необязательный делают `string?` или задают значение по умолчанию.

## Тело запроса и JSON

Сложный тип из тела читается из JSON автоматически. Для данных удобны записи (`record`):

```csharp
record Todo(int Id, string Title);
record CreateTodo(string Title);

app.MapPost("/todos", (CreateTodo dto, ITodoStore store) =>
{
    var t = store.Add(dto.Title);
    return TypedResults.Created($"/todos/{t.Id}", t);     // 201 + Location header
});
```

Проверено: `POST /todos` с `{"title":"write docs"}` → `201 {"id":1,"title":"write docs"}`; `POST` без тела → `400`. Сервис `ITodoStore` берётся из контейнера — его регистрируют до `Build()`: `builder.Services.AddSingleton<ITodoStore, TodoStore>();`.

## Результаты ответа

Обработчик может вернуть готовый результат с нужным кодом:

| Что | Код |
|---|---|
| `Results.Ok(obj)` / просто объект | 200 |
| `Results.Created(uri, obj)` | 201 |
| `Results.NoContent()` | 204 |
| `Results.BadRequest(...)` | 400 |
| `Results.NotFound()` | 404 |

```csharp
app.MapGet("/todos/{id:int}", (int id, ITodoStore s) =>
    s.Find(id) is { } t ? Results.Ok(t) : Results.NotFound());
app.MapDelete("/todos/{id:int}", (int id, ITodoStore s) => s.Remove(id) ? Results.NoContent() : Results.NotFound());
```

Проверено: `GET /todos/99` → `404`, `DELETE /todos/1` → `204`, повторный `DELETE` → `404`.

## Группы маршрутов

Общий префикс и настройки удобно собирать в группу:

```csharp
var todos = app.MapGroup("/todos");
todos.MapGet("/", (ITodoStore s) => s.All());
todos.MapPost("/", (CreateTodo dto, ITodoStore s) => TypedResults.Created("/todos/1", s.Add(dto.Title)));
```

## Конвейер middleware

Запрос проходит цепочку **middleware**; каждое звено может действовать до и после следующего. `app.Use` добавляет своё:

```csharp
app.Use(async (ctx, next) =>
{
    Console.WriteLine($"before {ctx.Request.Path}");
    await next();                                        // the rest of the pipeline and the endpoint
    Console.WriteLine($"after {ctx.Response.StatusCode}");
});
```

Проверено: журнал `mw1 before / | mw1 after 200 | mw1 before /hello/Ann | mw1 after 200`. **Порядок регистрации важен**: middleware выполняются в том порядке, в каком добавлены. Если не вызвать `next()`, запрос дальше не пойдёт (короткое замыкание).

## Ошибки новичка

- ❌ Всё в `Program.cs` на тысячи строк → ✅ группы маршрутов и методы-расширения (`MapTodoEndpoints`).
- ❌ Возвращать `Results.Ok(...)` везде, не думая о типах → ✅ `TypedResults` для точных типов (см. продвинутую теорию).
- ❌ Не проверять входные данные → ✅ валидация и ответы `400` ([продвинутая теория](advanced.ru.md#валидация-и-problemdetails)).
- ❌ Регистрировать сервис после `Build()` → ✅ сервисы только до `Build()`.
- ❌ Ставить middleware в произвольном порядке → ✅ обработка ошибок первой.
- ❌ Блокировать поток (`.Result`) в обработчике → ✅ `async`/`await` ([10 · Async/await](../10-async-await/overview.ru.md)).

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — `TypedResults`, фильтры, валидация, ошибки, OpenAPI.
- [13 · DI, конфигурация, логирование](../13-di-config-logging/overview.ru.md) — хост и сервисы.
- [10 · Async/await](../10-async-await/overview.ru.md) — асинхронные обработчики и `CancellationToken`.
- [15 · EF Core](../15-ef-core/overview.ru.md) — данные для API.
- [Python](../../../python/overview.ru.md) — FastAPI: похожие конечные точки на декораторах.

## Источники

- [Minimal API: обзор](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/overview)
- [Маршрутизация](https://learn.microsoft.com/aspnet/core/fundamentals/routing)
- [Привязка параметров](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/parameter-binding)
- [Middleware](https://learn.microsoft.com/aspnet/core/fundamentals/middleware/)
- [Результаты ответа](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/responses)
