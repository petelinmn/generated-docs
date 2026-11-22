---
title: "Quiz: ASP.NET Core: Minimal API"
lang: en
track: csharp
kind: module
doc: quiz
slug: 14-aspnet-core-minimal-api
part: 2
tags: [aspnet-core, minimal-api, routing, middleware, typed-results, problem-details]
status: draft
updated: 2026-11-22
---

# Quiz: ASP.NET Core: Minimal API

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What does `GET /items/abc` return for the route `app.MapGet("/items/{id:int}", …)`?

- A) `400`
- B) `404`
- C) `500`
- D) `200` with `id = 0`

<details><summary>Answer</summary>

**B.** The `:int` constraint rejects `abc` and the route is simply not found (verified). `400` comes when the route is found but a query parameter value cannot be converted to the type.
→ [Basics](basics.en.md#routing)

</details>

## 2. 🟢 Where does the `page` parameter come from?

```csharp
app.MapGet("/search", (string? q, int page = 1) => $"q={q ?? "-"} page={page}");
```

- A) the request body
- B) the query string: `/search?q=x&page=3`
- C) a header
- D) DI

<details><summary>Answer</summary>

**B.** A simple type that is not in the route template is taken from the query string; `/search?q=x&page=3` → `q=x page=3`, without parameters — `q=- page=1` (verified).
→ [Basics](basics.en.md#parameter-binding)

</details>

## 3. 🟢 What happens with `GET /search?page=abc`?

- A) `page = 0`
- B) `400`
- C) `404`
- D) `500`

<details><summary>Answer</summary>

**B.** The value cannot be converted to `int` → `400 Bad Request` (verified).
→ [Basics](basics.en.md#parameter-binding)

</details>

## 4. 🟢 What does `TypedResults.Created("/todos/1", todo)` do?

- A) a `200` response with a body
- B) a `201` response with a `Location` header and a body
- C) a `302` redirect
- D) a `204` response

<details><summary>Answer</summary>

**B.** `Created` is `201` with the new resource's address in `Location` and a body (verified: `201 {"id":1,"title":"write docs"}`).
→ [Basics](basics.en.md#the-request-body-and-json)

</details>

## 5. 🟡 Why is `Results<Ok<Todo>, NotFound>` needed instead of `IResult`?

- A) without it the code does not compile
- B) the compiler checks that only the listed variants are returned, and OpenAPI gets exact codes and schemas
- C) it speeds up serialization
- D) it enables caching

<details><summary>Answer</summary>

**B.** The type union describes the response contract: in the verified OpenAPI document the route has `200` with the `Todo` schema and `404`.
→ [Advanced theory](advanced.en.md#typedresults-and-describing-responses)

</details>

## 6. 🟡 What does `POST /todos` with the body `{"title":""}` return if `AddValidation()` and `AddProblemDetails()` are enabled and `CreateTodo` is marked `[Required, MinLength(1)]`?

- A) `201`
- B) `400` as `application/problem+json` describing the `Title` field error
- C) `500`
- D) `422`

<details><summary>Answer</summary>

**B.** Verified: `400`, body `{"title":"One or more validation errors occurred.","status":400,"errors":{"Title":["The Title field is required."]},…}`. Attribute validation in Minimal API — since .NET 10.
→ [Advanced theory](advanced.en.md#validation-and-problemdetails)

</details>

## 7. 🟡 What happens to an exception inside a handler with `UseExceptionHandler()` and `AddProblemDetails()`?

- A) the process terminates
- B) the client gets `500` in ProblemDetails format without the exception text
- C) the client gets the stack trace
- D) the request hangs

<details><summary>Answer</summary>

**B.** Verified: `500`, the body does not contain "secret details", only a generic title and a `traceId`. The details are in the log.
→ [Advanced theory](advanced.en.md#error-handling)

</details>

## 8. 🟡 Select all correct statements about endpoint filters

- [ ] A) a filter sees the handler's typed arguments
- [ ] B) a filter can return its own result instead of calling the handler
- [ ] C) a filter runs for requests that did not find a route
- [ ] D) a filter can be added to a route group

<details><summary>Answer</summary>

**A, B, D.** C is wrong: a filter fires only after an endpoint is selected; `404`/`405` need middleware. Verified: `?v=bad` → `400 "filtered out"` without calling the handler.
→ [Advanced theory](advanced.en.md#endpoint-filters)

</details>

## 9. 🟡 Open question: how do you organize Minimal API code when there are many endpoints?

<details><summary>Key points of a good answer</summary>

- `MapGroup("/todos")` groups with a shared prefix, tags, filters and policies;
- extension methods `MapTodoEndpoints(this IEndpointRouteBuilder)` in separate files, called from `Program.cs`;
- logic in services registered in DI, handlers stay thin;
- one error format (ProblemDetails) and validation at the boundary;
- DTOs instead of returning database entities.

</details>

## 10. 🔴 Why does a `POST` with `Content-Type: text/plain` to an endpoint that accepts JSON return `415` and not `400`?

- A) `415` and `400` are the same
- B) the endpoint accepts only `application/json`: the content type is unsupported, while `400` is for invalid data
- C) a server error
- D) only in Development

<details><summary>Answer</summary>

**B.** `415 Unsupported Media Type` is a `Content-Type` mismatch; `400` is broken JSON or a missing body (verified: `text/plain` → `415`, `{oops` → `400`). With `MapFallback` a request with an unsuitable type goes to the fallback handler.
→ [Expert theory](expert.en.md#binding-rules-and-response-codes)

</details>

## 11. 🔴 What happens if you declare `app.MapGet("/a", …)` twice?

- A) an error at `Build()`
- B) an error at startup
- C) the application starts, and a request to `/a` returns `500`
- D) the last handler wins

<details><summary>Answer</summary>

**C.** The ambiguous match is detected when matching a request, not at startup (verified). That is why duplicate routes are caught by integration-level tests ([17](../17-testing-integration/overview.en.md)).
→ [Expert theory](expert.en.md#gotchas)

</details>
