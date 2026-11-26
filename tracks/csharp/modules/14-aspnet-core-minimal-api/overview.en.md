---
title: "ASP.NET Core: Minimal API"
lang: en
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

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

Minimal API is a way to write HTTP services on ASP.NET Core by declaring endpoints with lambdas in `Program.cs`: `app.MapGet("/hello/{name}", (string name) => …)`. The framework binds parameters from the route, query string, JSON body and DI by itself and turns the return value into a response. Under the hood is the same Kestrel, middleware pipeline and routing as with controllers, but with a minimum of ceremony and Native AOT support.

## Key ideas

- **An endpoint** is `MapGet/Post/Put/Delete(template, delegate)`; `MapGroup` groups gather shared prefixes and filters.
- **Parameter binding:** route → query string → JSON body → DI services; binding errors are `400`, an unsuitable `Content-Type` is `415`.
- **Results:** `TypedResults` and `Results<…>` give exact response types and correct OpenAPI.
- **Middleware** runs in registration order; the error handler goes first.
- **Endpoint filters** see the typed arguments of the selected endpoint and can replace the result.
- **Errors:** `UseExceptionHandler` + ProblemDetails (RFC 9457); DataAnnotations validation — `AddValidation()` (since .NET 10).
- **OpenAPI:** `AddOpenApi()` and `MapOpenApi()` publish the API description (`/openapi/v1.json`).

## Minimal example

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/hello/{name}", (string name) => $"hello {name}");
app.MapGet("/search", (string? q, int page = 1) => $"q={q ?? "-"} page={page}");

app.Run();
```

## When to use and when not

- ✅ Small and medium HTTP APIs, microservices, backend-for-frontend.
- ✅ Services built with Native AOT or that must start quickly.
- ✅ Combined with groups, filters and extension methods for structure.
- ❌ A huge project without its own conventions: code sprawls in `Program.cs`.
- ❌ You need rich MVC conventions (view models, Razor Views) — use MVC/Razor Pages.
- ❌ Returning database entities directly instead of DTOs.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | the first application, routing and constraints, parameter binding, JSON, results, groups, the middleware pipeline |
| [2. Advanced](advanced.en.md) | `TypedResults` and `Results<…>`, `[AsParameters]`, filters, validation and ProblemDetails, `IExceptionHandler`, OpenAPI, application structure |
| [3. Expert](expert.en.md) | the host and pipeline, `RequestDelegate` generation, binding rules and response codes, middleware order, AOT, gotchas, 13 interview questions |
| [4. Self-check](quiz.en.md) | 11 questions and [5 exercises](exercises.en.md) with solutions |
| 5. Runnable examples | not generated yet — `/learn csharp 14-aspnet-core-minimal-api level 5` |
| 6. Tests | not generated yet — `/learn csharp 14-aspnet-core-minimal-api level 6` |

## Related materials

- Prerequisites: [13 · DI, configuration, logging](../13-di-config-logging/overview.en.md), [10 · Async/await](../10-async-await/overview.en.md)
- Next: [15 · EF Core](../15-ef-core/overview.en.md), [17 · Testing: integration tests](../17-testing-integration/overview.en.md)
- Testing: [16 · Unit tests](../16-testing-unit/overview.en.md)
- Native AOT: [19 · Performance](../19-performance/overview.en.md)
- Generators for AOT: [20 · Reflection and source generators](../20-reflection-and-source-generators/overview.en.md)
- Application structure: [21 · Application architecture](../21-architecture/overview.en.md)
- Health checks and tracing: [22 · Observability](../22-observability/overview.en.md)
- The same idea in other tracks: [Python](../../../python/overview.en.md) — FastAPI
- Recipes and libraries: [FluentValidation](../../libraries/fluentvalidation/overview.en.md), [Caching](../../recipes/caching/overview.en.md), [File upload](../../recipes/file-upload/overview.en.md), [gRPC service](../../recipes/grpc-service/overview.en.md), [JWT authentication](../../recipes/jwt-auth/overview.en.md), [Pagination and filtering](../../recipes/pagination-filtering/overview.en.md), [Rate limiting](../../recipes/rate-limiting/overview.en.md), [Real-time with SignalR](../../recipes/signalr-realtime/overview.en.md)
- [System design: API design](../../../system-design/modules/05-api-design/overview.en.md)
