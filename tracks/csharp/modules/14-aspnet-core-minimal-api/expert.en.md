---
title: "ASP.NET Core: Minimal API: expert"
lang: en
track: csharp
kind: module
doc: expert
slug: 14-aspnet-core-minimal-api
part: 2
tags: [aspnet-core, minimal-api, kestrel, endpoint-routing, request-delegate, native-aot, content-negotiation, interview-questions]
versions: ".NET 11, C# 15; ASP.NET Core 11.0"
status: draft
updated: 2026-11-22
---

# ASP.NET Core: Minimal API: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

**Prerequisites:** [basic](basics.en.md) and [advanced](advanced.en.md) theory. **Time:** ~1.5 h.

## How it is built

`WebApplication` combines three roles: the **host** (the [generic host](../13-di-config-logging/overview.en.md)), the pipeline builder (`IApplicationBuilder`) and the route builder (`IEndpointRouteBuilder`). The server is **Kestrel**: it accepts connections, parses HTTP and for each request calls the pipeline — a chain of `RequestDelegate` delegates (`HttpContext → Task`). Each `app.Use(...)` wraps the next delegate; so registration order is the order of execution "in", and the return goes in reverse order.

**Endpoint routing.** `MapGet` and the like register an `Endpoint` — a pair "route template + `RequestDelegate` + metadata". Templates are compiled into an automaton for matching; metadata (method constraints, `Consumes`, authorization, name) takes part in the selection. For Minimal API the routing middleware is added implicitly, and execution of the selected endpoint is built into the end of the pipeline.

**From a lambda to a `RequestDelegate`.** Your handler has an arbitrary signature, so when an endpoint is built, the `RequestDelegateFactory` analyzes the parameters and **generates** a `RequestDelegate`: binding code (route, query, body, services), a call to your delegate and writing of the result. Generation happens once at endpoint startup (on expression trees); in Native AOT a compile-time source generator (the Request Delegate Generator) works instead, to avoid dynamic code.

### Binding rules and response codes

The parameter source is determined by the order of checks: an attribute (`[FromRoute]`, `[FromQuery]`, `[FromBody]`, `[FromServices]`, `[FromHeader]`) → special types (`HttpContext`, `CancellationToken` — this is `RequestAborted`) → a name in the route template → a registered service → a simple type from the query string → a complex type from the body. Verified:

| Request | Response |
|---|---|
| `POST /todos` with `Content-Type: text/plain` | `415 Unsupported Media Type` |
| `POST /todos` with broken JSON | `400` |
| `POST /todos` without a body | `400` |
| `GET /search?page=abc` (an `int` parameter) | `400` |
| `GET /items/abc` (the `:int` constraint) | `404` |
| `PUT /` with only `MapGet("/")` | `405` |
| `HEAD` for `MapGet` | `405` (in the verified version) |
| a path in another case (`/WHO` instead of `/who`) | found: path matching is case-insensitive |
| two identical `MapGet("/a")` | the application starts, but a request to `/a` gives `500` (an ambiguous match) |

With `MapFallback`, unmatched requests (including `text/plain` instead of JSON and a non-matching method) go to the fallback handler — verified: instead of `415` the `fallback` response arrived.

**JSON.** `System.Text.Json` is used with web defaults: camelCase names, case-insensitive parsing. Anonymous types, records and ordinary classes are serialized without attributes. Native AOT needs a `JsonSerializerContext` (a generator), otherwise reflection-based serialization is unavailable.

## The pipeline and ordering

The recommended order of the built-in middleware: exception handling → HTTPS redirection/HSTS → static files → routing → CORS → authentication → authorization → your middleware → endpoints. Rules: the error handler first (it must wrap everything), authentication before authorization, CORS before authorization. Verified: with `UseExceptionHandler()` first and a custom correlation middleware, the `X-Request-Id` header is present in the response.

Endpoint filters and middleware differ in level: middleware sees the `HttpContext` of every request, while a filter sees the typed **arguments** of an already selected endpoint and the result; a filter can replace the result and works on a group/endpoint.

## Reference

| What | API |
|---|---|
| Methods | `MapGet/Post/Put/Delete/Patch`, `MapMethods`, `Map` |
| Groups | `MapGroup`, `WithTags`, `RequireAuthorization`, `AddEndpointFilter` |
| Results | `Results.*`, `TypedResults.*`, `Results<T1, T2, …>` |
| Binding | `[FromRoute/Query/Header/Body/Services/Form]`, `[AsParameters]` |
| Metadata | `WithName`, `WithSummary`, `Produces<T>`, `Accepts<T>`, `WithOpenApi` |
| Errors | `UseExceptionHandler`, `IExceptionHandler`, `AddProblemDetails`, `UseStatusCodePages` |
| Validation | `AddValidation` (.NET 10+), DataAnnotations, filters |
| Documentation | `AddOpenApi`, `MapOpenApi` |
| AOT | `CreateSlimBuilder`, `CreateEmptyBuilder`, `JsonSerializerContext` |

## Gotchas

**1. Middleware order.** `UseExceptionHandler` after your own middleware will not catch its exceptions; `UseAuthorization` without `UseAuthentication` has no user.

**2. Two identical routes are not an error at startup.** The application starts, and the request fails with `500` (verified).

**3. `HEAD` does not work on `MapGet` automatically** (verified: `405`); it needs `MapMethods` or a hand-written endpoint.

**4. Filters do not see requests that did not reach an endpoint** (`404`, `405`, `415`) — those need middleware or `UseStatusCodePages`.

**5. A singleton and a scope.** A service parameter in a handler is taken from the request scope: `Scoped` services work correctly ([13 · DI](../13-di-config-logging/overview.en.md)); you must not inject a scoped service into singleton middleware.

**6. The default binding is not `[FromBody]` for simple types.** According to the documentation a simple type such as `string title` is looked up in the query string, not in the body; for the body you need a record type or `[FromBody]`.

**7. Returning an EF Core entity.** Navigation property cycles break serialization, and extra fields leak to the client — return DTOs.

**8. Blocking code in a handler** occupies a pool thread ([10 · Async/await](../10-async-await/overview.en.md)).

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| Minimal API | minimal code, quick start, AOT | you need your own structure in large projects |
| MVC controllers (`[ApiController]`) | conventions, filters, a familiar structure | more ceremony |
| gRPC | a binary contract, streaming | not for browsers directly |
| FastEndpoints, Carter (external) | a request-response structure on top of Minimal API | an external dependency |

## Interview questions

<details><summary>1. What is Minimal API and how does it differ from controllers?</summary>

A way to declare endpoints with lambdas (`MapGet` and so on) without controller classes and route attributes. It is the same pipeline and routing, but with less ceremony and a quick start; you build the structure (groups, files) yourself.

</details>

<details><summary>2. What is middleware and why does order matter?</summary>

A `RequestDelegate` pipeline link that can act before and after the next one. They run in registration order; for example the error handler must be first so that it catches the exceptions of the rest.

</details>

<details><summary>3. How does Minimal API decide where to take a parameter from?</summary>

An attribute → special types → a name from the route template → a DI service → a simple type from the query string → a complex type from the JSON body.

</details>

<details><summary>4. When does 415 come, and when 400?</summary>

`415` — a `Content-Type` mismatch with what the endpoint accepts (for example `text/plain` instead of JSON); `400` — invalid JSON, a missing body or a value that cannot be converted to the type (verified).

</details>

<details><summary>5. How do `Results` and `TypedResults` differ?</summary>

`Results.*` return `IResult`; `TypedResults.*` — concrete types (`Ok<T>`, `NotFound`) and together with `Results<…>` give the compiler and OpenAPI an exact list of responses.

</details>

<details><summary>6. How does an endpoint filter differ from middleware?</summary>

Middleware handles every request, seeing the `HttpContext`; a filter works only for a selected endpoint/group, sees typed arguments and can replace the result.

</details>

<details><summary>7. How do you handle errors uniformly?</summary>

`UseExceptionHandler` + `AddProblemDetails` for `500`, `IExceptionHandler` for domain exceptions (`404`, `409`), validation returns `400` in the ProblemDetails format.

</details>

<details><summary>8. What is ProblemDetails?</summary>

The standard error body format of HTTP APIs (RFC 9457): `type`, `title`, `status`, `detail`, extra fields (`errors`, `traceId`).

</details>

<details><summary>9. How does body binding work and what is the `CancellationToken` in a handler?</summary>

A complex type is deserialized from JSON by `System.Text.Json`; a `CancellationToken` is `HttpContext.RequestAborted`: it is canceled when the client drops the connection.

</details>

<details><summary>10. How do you prepare Minimal API for Native AOT?</summary>

`CreateSlimBuilder`, source generators (the Request Delegate Generator and a `JsonSerializerContext`), giving up reflection and dynamically created code.

</details>

<details><summary>11. Trick question: why did a `POST` with `text/plain` return `404` rather than `415`?</summary>

The verified application had a `MapFallback`: the endpoint did not match by `Content-Type`, and the request went to the fallback handler. Without the fallback, `415` would have come.

</details>

<details><summary>12. Trick question: why do two identical `MapGet("/a")` not fail at startup?</summary>

The ambiguity is detected only when matching a request: the application starts, and a request to `/a` gives `500` (verified).

</details>

<details><summary>13. Trick question: will the exception text reach the client?</summary>

No, with `UseExceptionHandler` + ProblemDetails the body contains only a generic title and a `traceId` (verified: "secret details" is absent); the details are in the log.

</details>

## Related materials

- Back: [advanced theory](advanced.en.md). Check yourself: [quiz](quiz.en.md) and [exercises](exercises.en.md).
- [13 · DI, configuration, logging](../13-di-config-logging/overview.en.md) — the host and the container.
- [10 · Async/await](../10-async-await/overview.en.md) — `CancellationToken`, blocking.
- [11 · Modern C#](../11-modern-csharp/overview.en.md) — records for DTOs, `required`.
- [Python](../../../python/overview.en.md) — FastAPI/ASGI: an analog of the pipeline and binding.

## Sources

- [The request pipeline](https://learn.microsoft.com/aspnet/core/fundamentals/middleware/)
- [Endpoint routing](https://learn.microsoft.com/aspnet/core/fundamentals/routing)
- [Minimal APIs and Native AOT](https://learn.microsoft.com/aspnet/core/fundamentals/native-aot)
- [Kestrel](https://learn.microsoft.com/aspnet/core/fundamentals/servers/kestrel)
- [ProblemDetails (RFC 9457)](https://www.rfc-editor.org/rfc/rfc9457)
