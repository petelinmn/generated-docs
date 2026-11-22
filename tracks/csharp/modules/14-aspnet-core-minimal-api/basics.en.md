---
title: "ASP.NET Core: Minimal API: basics"
lang: en
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

# ASP.NET Core: Minimal API: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [13 · DI, configuration, logging](../13-di-config-logging/overview.en.md), [09 · Delegates, events, lambdas](../09-delegates-events-lambdas/overview.en.md), [10 · Async/await](../10-async-await/overview.en.md). **Time:** ~1 h.

## Why it matters

ASP.NET Core is the .NET web framework: it accepts HTTP requests and returns responses. **Minimal API** is a way to describe endpoints right in `Program.cs` with lambdas, without controllers and attributes. It is built on the [generic host](../13-di-config-logging/overview.en.md): DI, configuration and logging are the same. For small services and APIs it is the shortest path; for large ones there are route groups, filters and splitting across files.

A project is created with the template `dotnet new web` (the SDK `Microsoft.NET.Sdk.Web`); you run it with `dotnet run`.

## The first application

```csharp
var builder = WebApplication.CreateBuilder(args);   // host: DI, configuration, logging
var app = builder.Build();

app.MapGet("/", () => "hello");                     // GET /  → text
app.MapGet("/hello/{name}", (string name) => $"hello {name}");   // route parameter

app.Run();                                          // starts Kestrel and blocks
```

`MapGet`, `MapPost`, `MapPut`, `MapDelete` bind an HTTP method and a path to a handler — a **delegate** (a lambda or a method). The returned value becomes the response: a string — `text/plain`, an object — JSON. Verified on a running application: `GET /` → `200 text/plain hello`, `GET /hello/Ann` → `hello Ann`, `GET /json` with `new { Name = "Ann", Age = 30 }` → `application/json {"name":"Ann","age":30}` (property names in camelCase). An unsupported method gives `405`: `PUT /` returned `405`.

## Routing

Route parameters are written in curly braces; constraints go after a colon:

```csharp
app.MapGet("/items/{id:int}", (int id) => $"item {id}");
```

Verified: `/items/5` → `item 5`, while `/items/abc` → `404` — the `:int` constraint rejects the value and the route is not found. Other constraints: `guid`, `bool`, `min(1)`, `alpha`, `regex(...)`.

## Parameter binding

The handler's parameters are filled automatically from the request; the source is determined by name and type:

| Source | When |
|---|---|
| route | the parameter name matches `{name}` in the path |
| query string | a simple type that is not in the route (`/search?q=x&page=3`) |
| body (JSON) | a complex type in `POST`/`PUT` |
| DI services | a registered service (for example `ITodoStore`) |
| special types | `HttpContext`, `CancellationToken`, `ClaimsPrincipal` |

```csharp
app.MapGet("/search", (string? q, int page = 1) => $"q={q ?? "-"} page={page}");
```

Verified: `/search` → `q=- page=1`; `/search?q=x&page=3` → `q=x page=3`; `/search?page=abc` → `400` (the value cannot be converted to `int`). A required parameter without a value gives `400`; make an optional one `string?` or give it a default value.

## The request body and JSON

A complex type from the body is read from JSON automatically. Records are convenient for data:

```csharp
record Todo(int Id, string Title);
record CreateTodo(string Title);

app.MapPost("/todos", (CreateTodo dto, ITodoStore store) =>
{
    var t = store.Add(dto.Title);
    return TypedResults.Created($"/todos/{t.Id}", t);     // 201 + Location header
});
```

Verified: `POST /todos` with `{"title":"write docs"}` → `201 {"id":1,"title":"write docs"}`; a `POST` without a body → `400`. The `ITodoStore` service is taken from the container — register it before `Build()`: `builder.Services.AddSingleton<ITodoStore, TodoStore>();`.

## Response results

A handler can return a ready result with the needed code:

| What | Code |
|---|---|
| `Results.Ok(obj)` / just an object | 200 |
| `Results.Created(uri, obj)` | 201 |
| `Results.NoContent()` | 204 |
| `Results.BadRequest(...)` | 400 |
| `Results.NotFound()` | 404 |

```csharp
app.MapGet("/todos/{id:int}", (int id, ITodoStore s) =>
    s.Find(id) is { } t ? Results.Ok(t) : Results.NotFound());
app.MapDelete("/todos/{id:int}", (int id, ITodoStore s) => s.Remove(id) ? Results.NoContent() : Results.NotFound());
```

Verified: `GET /todos/99` → `404`, `DELETE /todos/1` → `204`, a repeated `DELETE` → `404`.

## Route groups

A common prefix and settings are conveniently gathered into a group:

```csharp
var todos = app.MapGroup("/todos");
todos.MapGet("/", (ITodoStore s) => s.All());
todos.MapPost("/", (CreateTodo dto, ITodoStore s) => TypedResults.Created("/todos/1", s.Add(dto.Title)));
```

## The middleware pipeline

A request passes through a chain of **middleware**; each link can act before and after the next. `app.Use` adds your own:

```csharp
app.Use(async (ctx, next) =>
{
    Console.WriteLine($"before {ctx.Request.Path}");
    await next();                                        // the rest of the pipeline and the endpoint
    Console.WriteLine($"after {ctx.Response.StatusCode}");
});
```

Verified: the log `mw1 before / | mw1 after 200 | mw1 before /hello/Ann | mw1 after 200`. **Registration order matters**: middleware runs in the order it was added. If you do not call `next()`, the request goes no further (short-circuiting).

## Beginner mistakes

- ❌ Everything in `Program.cs`, thousands of lines → ✅ route groups and extension methods (`MapTodoEndpoints`).
- ❌ Returning `Results.Ok(...)` everywhere without thinking about types → ✅ `TypedResults` for exact types (see the advanced theory).
- ❌ Not validating input → ✅ validation and `400` responses ([advanced theory](advanced.en.md#validation-and-problemdetails)).
- ❌ Registering a service after `Build()` → ✅ services only before `Build()`.
- ❌ Putting middleware in arbitrary order → ✅ error handling first.
- ❌ Blocking a thread (`.Result`) in a handler → ✅ `async`/`await` ([10 · Async/await](../10-async-await/overview.en.md)).

## Related materials

- Next: [advanced theory](advanced.en.md) — `TypedResults`, filters, validation, errors, OpenAPI.
- [13 · DI, configuration, logging](../13-di-config-logging/overview.en.md) — the host and services.
- [10 · Async/await](../10-async-await/overview.en.md) — asynchronous handlers and `CancellationToken`.
- [15 · EF Core](../15-ef-core/overview.en.md) — data for the API.
- [Python](../../../python/overview.en.md) — FastAPI: similar endpoints on decorators.

## Sources

- [Minimal APIs overview](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/overview)
- [Routing](https://learn.microsoft.com/aspnet/core/fundamentals/routing)
- [Parameter binding](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/parameter-binding)
- [Middleware](https://learn.microsoft.com/aspnet/core/fundamentals/middleware/)
- [Responses](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/responses)
