---
title: "ASP.NET Core: Minimal API: advanced"
lang: en
track: csharp
kind: module
doc: advanced
slug: 14-aspnet-core-minimal-api
part: 2
tags: [aspnet-core, minimal-api, typed-results, endpoint-filters, validation, problem-details, openapi, exception-handling]
versions: ".NET 11, C# 15; ASP.NET Core 11.0; Microsoft.AspNetCore.OpenApi 11.0.0"
status: draft
updated: 2026-11-22
---

# ASP.NET Core: Minimal API: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [basics](basics.en.md). **Time:** ~1.5 h.

## TypedResults and describing responses

`Results.Ok(...)` returns an `IResult` — the type does not say which codes are possible. `TypedResults.Ok(...)` returns a concrete type (`Ok<T>`), and the union `Results<…>` lists **all** response variants — the compiler checks that only they are returned, and OpenAPI gets exact codes:

```csharp
app.MapGet("/todos/{id:int}", Results<Ok<Todo>, NotFound> (int id, ITodoStore s) =>
    s.Find(id) is { } t ? TypedResults.Ok(t) : TypedResults.NotFound());
```

(`using Microsoft.AspNetCore.Http.HttpResults;`.) Verified: the responses `200 {"id":1,...}` and `404`; in the generated OpenAPI document the route describes `200` with the `Todo` schema and `404`. For `Results.Ok(...)` the response schema is not inferred as precisely — use `TypedResults`.

### `[AsParameters]`

A group of request parameters can be gathered into one type:

```csharp
record Paging(int Page = 1, int Size = 20);
app.MapGet("/params", ([AsParameters] Paging p) => $"{p.Page}/{p.Size}");   // /params?page=2&size=10 → 2/10
```

(Names in the request are matched case-insensitively: `page` → `Page`; verified.)

## Endpoint filters

A filter wraps the handler of a specific endpoint or group and sees its arguments: it suits validation and shared logic:

```csharp
app.MapPost("/filtered", (string v) => v).AddEndpointFilter(async (ctx, next) =>
{
    if (ctx.GetArgument<string>(0) == "bad") return Results.BadRequest("filtered out");
    return await next(ctx);
});
```

Verified: `?v=ok` → `200 ok`, `?v=bad` → `400 "filtered out"`. The difference from middleware: a filter works **after** the route is selected and knows the argument types; middleware handles all requests. A filter can also be added to a group: `group.AddEndpointFilter(...)` — it applies to all its endpoints.

## Validation and ProblemDetails

Starting with .NET 10, Minimal API can check data with `DataAnnotations` attributes automatically: you only need to enable the service.

```csharp
builder.Services.AddValidation();
builder.Services.AddProblemDetails();

record CreateTodo([property: Required, MinLength(1)] string Title);
```

Verified: `POST /todos` with `{"title":""}` → `400 application/problem+json` with the body `{"type":"…","title":"One or more validation errors occurred.","status":400,"errors":{"Title":["The Title field is required."]},"traceId":"…"}`. **ProblemDetails** is the standard error format of HTTP APIs (RFC 9457): the fields `type`, `title`, `status`, `detail`, and the `errors` extension. For complex rules — an endpoint filter or the FluentValidation library (not created yet).

## Error handling

`app.UseExceptionHandler()` intercepts an unhandled exception and returns a `500` response in the ProblemDetails format **without revealing** details:

```csharp
app.MapGet("/crash", () => { throw new InvalidOperationException("secret details"); return "x"; });
// → 500, the response body does not contain "secret details"
```

Verified: code `500`, the exception text is absent from the body (`contains secret=False`). For your own exceptions implement `IExceptionHandler`, which turns a domain error into the needed response:

```csharp
builder.Services.AddExceptionHandler<NotFoundHandler>();

class NotFoundHandler(IProblemDetailsService pds) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext ctx, Exception ex, CancellationToken ct)
    {
        if (ex is not NotFoundException) return false;        // not ours: let the next handler try
        ctx.Response.StatusCode = 404;
        return await pds.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = ctx,
            ProblemDetails = new ProblemDetails { Status = 404, Title = "Not found", Detail = ex.Message }
        });
    }
}
```

Verified: `GET /orders/7` → `404 application/problem+json` with `"detail":"Order 7 not found"`. The `UseExceptionHandler()` call goes **first** in the pipeline so that it covers everything else.

## Custom middleware

Middleware can be written as a lambda, a class or an extension method. A typical task is a correlation identifier: take it from a header or create one, and return it in the response:

```csharp
app.Use(async (ctx, next) =>
{
    var id = ctx.Request.Headers["X-Request-Id"].FirstOrDefault() ?? Guid.NewGuid().ToString("N")[..8];
    ctx.Response.Headers["X-Request-Id"] = id;
    await next();
});
app.Map("/health", () => Results.Text("ok"));   // an endpoint for liveness probes
```

Verified: the `X-Request-Id` header is in the response, and the `abc123` sent by the client came back unchanged. For liveness checks use an endpoint like `/health` (in production — `AddHealthChecks`, [22 · Observability](../22-observability/overview.en.md)).

## OpenAPI

The `Microsoft.AspNetCore.OpenApi` package builds an API description from the endpoints:

```csharp
builder.Services.AddOpenApi();
app.MapOpenApi();          // GET /openapi/v1.json
```

Verified: `GET /openapi/v1.json` → `200 application/json`; the document, version `3.2.0` (the value depends on the package version), contains all routes, path and query parameters (`id` — `integer/int32`, `q` — `string`, `page` — with a default value `1`), the `CreateTodo` body with `minLength: 1` and the `Todo` responses. A UI for browsing (Scalar, Swagger UI) is added by separate packages.

## Structuring a large application

| Technique | Why |
|---|---|
| `MapGroup("/todos")` | a shared prefix, filters and policies |
| A static class with `MapXxxEndpoints(this IEndpointRouteBuilder)` | splitting into files |
| Separate handler classes and services | testability |
| An `Endpoints/` folder + registration in `Program.cs` | readability |

## Best practices

| Rule | Why |
|---|---|
| `TypedResults` and `Results<…>` | an exact contract and OpenAPI |
| Validation at the boundary (`AddValidation`) | `400` instead of errors deeper down |
| One error format — ProblemDetails | easier for clients to parse |
| `UseExceptionHandler` first | covers the whole pipeline |
| `CancellationToken` in handlers | cancellation when the connection drops |
| Do not return database entities directly | separate DTOs ([15 · EF Core](../15-ef-core/overview.en.md)) |

## Related materials

- Back: [basics](basics.en.md). Next: [expert theory](expert.en.md) — the pipeline, binding, AOT.
- [13 · DI, configuration, logging](../13-di-config-logging/overview.en.md) — `ValidateOnStart`, services.
- [10 · Async/await](../10-async-await/overview.en.md) — asynchronous handlers.
- [08 · Exceptions](../08-exceptions/overview.en.md) — which errors to turn into `4xx`.
- [17 · Testing: integration tests](../17-testing-integration/overview.en.md) — `WebApplicationFactory`.

## Sources

- [TypedResults](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/responses#typedresults-vs-results)
- [Endpoint filters](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/min-api-filters)
- [Validation in Minimal APIs](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/parameter-binding#validation-support-in-minimal-apis)
- [Error handling](https://learn.microsoft.com/aspnet/core/fundamentals/error-handling)
- [OpenAPI in ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/openapi/overview)
