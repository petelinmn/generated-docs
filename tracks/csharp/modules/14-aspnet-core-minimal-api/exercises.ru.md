---
title: "Упражнения: ASP.NET Core: Minimal API"
lang: ru
track: csharp
kind: module
doc: exercises
slug: 14-aspnet-core-minimal-api
part: 2
tags: [aspnet-core, minimal-api, crud, validation, endpoint-filters, middleware, exception-handler]
status: draft
updated: 2026-11-22
---

# Упражнения: ASP.NET Core: Minimal API

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Решения — под спойлерами. Код проверен запуском на Kestrel с `HttpClient` (проект на SDK `Microsoft.NET.Sdk.Web`; `UseUrls("http://127.0.0.1:0")` выбирает свободный порт, адрес — `app.Urls`). Запускаемых версий в `code/` пока нет (уровень 5 не создан).

## 1. 🟢 Приветствие и поиск

**Задача:** добавь `GET /hello/{name}` (ответ `hello <name>`) и `GET /search` с необязательным `q` и `page` (по умолчанию 1).

**Ожидаемый результат:**

```text
GET /hello/Ann                → 200 hello Ann
GET /search                   → 200 q=- page=1
GET /search?q=x&page=3        → 200 q=x page=3
GET /search?page=abc          → 400
```

<details><summary>Подсказка</summary>

Параметры обработчика: `(string name)` берётся из маршрута, `(string? q, int page = 1)` — из строки запроса.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 1: route and query parameters.
app.MapGet("/hello/{name}", (string name) => $"hello {name}");
app.MapGet("/search", (string? q, int page = 1) => $"q={q ?? "-"} page={page}");
```

Проверено: все четыре ответа совпадают с таблицей.

</details>

## 2. 🟢 CRUD для задач

**Задача:** в группе `/todos` реализуй: список, получение по `id` (`404`, если нет), создание (`201` + `Location`), удаление (`204`/`404`). Хранилище — сервис `ITodoStore` из DI.

**Ожидаемый результат:**

```text
POST /todos {"title":"write docs"} → 201 {"id":1,"title":"write docs"}
GET /todos/1  → 200;  GET /todos/99 → 404
DELETE /todos/1 → 204;  повторный DELETE → 404
```

<details><summary>Подсказка</summary>

`app.MapGroup("/todos")`; для `GET /{id}` верни `Results<Ok<Todo>, NotFound>`; хранилище регистрируй `AddSingleton<ITodoStore, TodoStore>()` до `Build()`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 2: a CRUD group backed by a DI service.
builder.Services.AddSingleton<ITodoStore, TodoStore>();

var todos = app.MapGroup("/todos");
todos.MapGet("/", (ITodoStore s) => s.All());
todos.MapGet("/{id:int}", Results<Ok<Todo>, NotFound> (int id, ITodoStore s) =>
    s.Find(id) is { } t ? TypedResults.Ok(t) : TypedResults.NotFound());
todos.MapPost("/", (CreateTodo dto, ITodoStore s) =>
{
    var t = s.Add(dto.Title);
    return TypedResults.Created($"/todos/{t.Id}", t);
});
todos.MapDelete("/{id:int}", (int id, ITodoStore s) => s.Remove(id) ? Results.NoContent() : Results.NotFound());

record Todo(int Id, string Title);
record CreateTodo(string Title);
interface ITodoStore { IReadOnlyList<Todo> All(); Todo? Find(int id); Todo Add(string title); bool Remove(int id); }
class TodoStore : ITodoStore
{
    private readonly List<Todo> _items = new(); private int _next = 1;
    public IReadOnlyList<Todo> All() => _items.ToList();
    public Todo? Find(int id) => _items.FirstOrDefault(t => t.Id == id);
    public Todo Add(string title) { var t = new Todo(_next++, title); _items.Add(t); return t; }
    public bool Remove(int id) => _items.RemoveAll(t => t.Id == id) > 0;
}
```

Проверено: коды `201`, `200`, `404`, `204`, `404` по порядку. Хранилище не потокобезопасно — для реального сервиса нужна БД ([15 · EF Core](../15-ef-core/overview.ru.md)).

</details>

## 3. 🟡 Валидация с ProblemDetails

**Задача:** запрещай пустой `Title` у `CreateTodo`: ответ `400` в формате `application/problem+json` с ошибкой по полю `Title`.

**Ожидаемый результат:**

```text
POST /todos {"title":""} → 400 application/problem+json, errors.Title = ["The Title field is required."]
```

<details><summary>Подсказка</summary>

`builder.Services.AddValidation(); builder.Services.AddProblemDetails();` и атрибуты `[Required, MinLength(1)]` на параметре записи (`[property: …]`).

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 3: built-in validation of DataAnnotations (.NET 10+) with ProblemDetails.
builder.Services.AddValidation();
builder.Services.AddProblemDetails();

record CreateTodo([property: Required, MinLength(1)] string Title);
```

Проверено: `400 application/problem+json`, `"errors":{"Title":["The Title field is required."]}`.

</details>

## 4. 🟡 Фильтр конечной точки

**Задача:** добавь к `POST /filtered?v=...` фильтр, который отвечает `400 "filtered out"`, если `v == "bad"`, и пропускает остальные значения.

**Ожидаемый результат:**

```text
POST /filtered?v=ok  → 200 ok
POST /filtered?v=bad → 400 "filtered out"
```

<details><summary>Подсказка</summary>

`AddEndpointFilter(async (ctx, next) => …)`; значение аргумента — `ctx.GetArgument<string>(0)`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 4: an endpoint filter sees typed arguments and can short-circuit.
app.MapPost("/filtered", (string v) => v).AddEndpointFilter(async (ctx, next) =>
{
    if (ctx.GetArgument<string>(0) == "bad") return Results.BadRequest("filtered out");
    return await next(ctx);
});
```

Проверено: `200 ok` и `400 "filtered out"`.

</details>

## 5. 🔴 Единая обработка ошибок и корреляция

**Задача:** (а) middleware берёт `X-Request-Id` запроса или создаёт новый и возвращает его в ответе; (б) исключение `NotFoundException` превращается в `404 application/problem+json`, любое другое — в `500` без раскрытия текста; (в) `/health` отвечает `ok`.

**Ожидаемый результат:**

```text
GET /orders/42 → 200, заголовок X-Request-Id есть; с входным X-Request-Id: abc123 вернётся abc123
GET /orders/7  → 404 problem+json, detail = "Order 7 not found"
GET /crash     → 500, текст «secret details» в ответе отсутствует
GET /health    → 200
```

<details><summary>Подсказка</summary>

`UseExceptionHandler()` первым; `AddExceptionHandler<NotFoundHandler>()` с `IExceptionHandler`, который возвращает `false` для чужих исключений; запись ответа — через `IProblemDetailsService`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 5: correlation id middleware and a centralized exception mapping.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<NotFoundHandler>();
var app = builder.Build();

app.UseExceptionHandler();
app.Use(async (ctx, next) =>
{
    var id = ctx.Request.Headers["X-Request-Id"].FirstOrDefault() ?? Guid.NewGuid().ToString("N")[..8];
    ctx.Response.Headers["X-Request-Id"] = id;
    await next();
});
app.Map("/health", () => Results.Text("ok"));
app.MapGet("/orders/{id:int}", (int id) => id == 42 ? Results.Ok(new { id }) : throw new NotFoundException($"Order {id} not found"));

class NotFoundException(string m) : Exception(m);
class NotFoundHandler(IProblemDetailsService pds) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext ctx, Exception ex, CancellationToken ct)
    {
        if (ex is not NotFoundException) return false;
        ctx.Response.StatusCode = 404;
        return await pds.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = ctx,
            ProblemDetails = new ProblemDetails { Status = 404, Title = "Not found", Detail = ex.Message }
        });
    }
}
```

Проверено: все пункты таблицы (`using Microsoft.AspNetCore.Diagnostics;` и `Microsoft.AspNetCore.Mvc;` для `ProblemDetails`).

</details>
