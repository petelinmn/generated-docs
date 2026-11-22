---
title: "Exercises: ASP.NET Core: Minimal API"
lang: en
track: csharp
kind: module
doc: exercises
slug: 14-aspnet-core-minimal-api
part: 2
tags: [aspnet-core, minimal-api, crud, validation, endpoint-filters, middleware, exception-handler]
status: draft
updated: 2026-11-22
---

# Exercises: ASP.NET Core: Minimal API

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. Solutions are under spoilers. The code was verified by running it on Kestrel with an `HttpClient` (a project on the SDK `Microsoft.NET.Sdk.Web`; `UseUrls("http://127.0.0.1:0")` picks a free port, the address is `app.Urls`). There are no runnable versions in `code/` yet (level 5 does not exist).

## 1. 🟢 Greeting and search

**Task:** add `GET /hello/{name}` (the response `hello <name>`) and `GET /search` with an optional `q` and `page` (default 1).

**Expected result:**

```text
GET /hello/Ann                → 200 hello Ann
GET /search                   → 200 q=- page=1
GET /search?q=x&page=3        → 200 q=x page=3
GET /search?page=abc          → 400
```

<details><summary>Hint</summary>

Handler parameters: `(string name)` is taken from the route, `(string? q, int page = 1)` from the query string.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 1: route and query parameters.
app.MapGet("/hello/{name}", (string name) => $"hello {name}");
app.MapGet("/search", (string? q, int page = 1) => $"q={q ?? "-"} page={page}");
```

Verified: all four responses match the table.

</details>

## 2. 🟢 CRUD for tasks

**Task:** in a `/todos` group implement: a list, get by `id` (`404` if absent), create (`201` + `Location`), delete (`204`/`404`). The storage is an `ITodoStore` service from DI.

**Expected result:**

```text
POST /todos {"title":"write docs"} → 201 {"id":1,"title":"write docs"}
GET /todos/1  → 200;  GET /todos/99 → 404
DELETE /todos/1 → 204;  a repeated DELETE → 404
```

<details><summary>Hint</summary>

`app.MapGroup("/todos")`; for `GET /{id}` return `Results<Ok<Todo>, NotFound>`; register the storage with `AddSingleton<ITodoStore, TodoStore>()` before `Build()`.

</details>

<details><summary>Solution</summary>

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

Verified: the codes `201`, `200`, `404`, `204`, `404` in order. The storage is not thread-safe — a real service needs a database ([15 · EF Core](../15-ef-core/overview.en.md)).

</details>

## 3. 🟡 Validation with ProblemDetails

**Task:** forbid an empty `Title` in `CreateTodo`: the response is `400` as `application/problem+json` with an error on the `Title` field.

**Expected result:**

```text
POST /todos {"title":""} → 400 application/problem+json, errors.Title = ["The Title field is required."]
```

<details><summary>Hint</summary>

`builder.Services.AddValidation(); builder.Services.AddProblemDetails();` and the attributes `[Required, MinLength(1)]` on the record parameter (`[property: …]`).

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 3: built-in validation of DataAnnotations (.NET 10+) with ProblemDetails.
builder.Services.AddValidation();
builder.Services.AddProblemDetails();

record CreateTodo([property: Required, MinLength(1)] string Title);
```

Verified: `400 application/problem+json`, `"errors":{"Title":["The Title field is required."]}`.

</details>

## 4. 🟡 An endpoint filter

**Task:** add to `POST /filtered?v=...` a filter that responds `400 "filtered out"` if `v == "bad"` and lets other values through.

**Expected result:**

```text
POST /filtered?v=ok  → 200 ok
POST /filtered?v=bad → 400 "filtered out"
```

<details><summary>Hint</summary>

`AddEndpointFilter(async (ctx, next) => …)`; the argument value is `ctx.GetArgument<string>(0)`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 4: an endpoint filter sees typed arguments and can short-circuit.
app.MapPost("/filtered", (string v) => v).AddEndpointFilter(async (ctx, next) =>
{
    if (ctx.GetArgument<string>(0) == "bad") return Results.BadRequest("filtered out");
    return await next(ctx);
});
```

Verified: `200 ok` and `400 "filtered out"`.

</details>

## 5. 🔴 Uniform error handling and correlation

**Task:** (a) middleware takes the request's `X-Request-Id` or creates a new one and returns it in the response; (b) a `NotFoundException` turns into `404 application/problem+json`, any other into `500` without revealing the text; (c) `/health` answers `ok`.

**Expected result:**

```text
GET /orders/42 → 200, the X-Request-Id header is present; with an incoming X-Request-Id: abc123, abc123 comes back
GET /orders/7  → 404 problem+json, detail = "Order 7 not found"
GET /crash     → 500, the text "secret details" is absent from the response
GET /health    → 200
```

<details><summary>Hint</summary>

`UseExceptionHandler()` first; `AddExceptionHandler<NotFoundHandler>()` with an `IExceptionHandler` that returns `false` for foreign exceptions; write the response through `IProblemDetailsService`.

</details>

<details><summary>Solution</summary>

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

Verified: all rows of the table (`using Microsoft.AspNetCore.Diagnostics;` and `Microsoft.AspNetCore.Mvc;` for `ProblemDetails`).

</details>
