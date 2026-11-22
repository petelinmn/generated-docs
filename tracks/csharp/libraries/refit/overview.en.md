---
title: "Refit"
lang: en
track: csharp
kind: library
doc: overview
slug: refit
part: 2
levels: [1, 2, 3, 4]
tags: [refit, http-client, rest, source-generator, json-context]
versions: ".NET 11, C# 15; Refit 17.0.0"
status: draft
updated: 2026-11-22
---

# Refit

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

Refit turns a C# interface with attributes into a typed HTTP client. The implementation is built by a source generator, in version 17 without reflection: JSON goes through a source-generated `JsonSerializerContext`. HTTP errors are an `ApiException` or an `IApiResponse<T>`; registration in DI through `Refit.HttpClientFactory` lets you hang resilience handlers on it. The behavior was verified against a local server.

## Key ideas

- Attributes `[Get]`, `[Post]`, `[Body]`, `[Header]`, `[AliasAs]`; parameters outside the path go into the query.
- Refit 17: `RestService.ForGenerated<T>` / `AddRefitGeneratedClient<T>`; `For`/`AddRefitClient` — in `Refit.Reflection`.
- You need `RefitSettings.ForJsonContext(...)`; for a camelCase API — `JsonSerializerDefaults.Web`, otherwise silent zeros.
- `ApiException` on 4xx/5xx; `IApiResponse<T>` — without exceptions.
- Resilience — on the `IHttpClientBuilder`, not in Refit.
- The interface is easy to substitute in tests.

## Minimal example

```csharp
interface IUsersApi { [Get("/users/{id}")] Task<UserDto> GetUserAsync(int id); }

var api = RestService.ForGenerated<IUsersApi>(
    new HttpClient { BaseAddress = new Uri(baseUrl) }, RefitSettings.ForJsonContext(ApiJson.Default));
```

## When to use and when not

- ✅ Many calls to a REST API with a stable contract.
- ✅ You need typing and less boilerplate.
- ✅ Compatibility with trimming/Native AOT (a generated client).
- ❌ One or two one-off requests — `HttpClient` is enough.
- ❌ You need full control over every request/stream.
- ❌ The contract is given by OpenAPI and a client is better generated from the spec.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | the interface, creating a client, the JSON context, errors |
| [2. Advanced](advanced.en.md) | `IApiResponse`, DI, headers, serialization, tests |
| [3. Expert](expert.en.md) | generator internals, Refit 17 gotchas, alternatives, 11 interview questions |
| [4. Self-check](quiz.en.md) | 10 questions and [4 exercises](exercises.en.md) with solutions |
| 5. Runnable examples | not generated yet — `/learn csharp refit level 5` |
| 6. Tests | not generated yet — `/learn csharp refit level 6` |

## Related materials

- Prerequisites: [10 · async/await](../../modules/10-async-await/overview.en.md), [05 · Interfaces, structs, records](../../modules/05-interfaces-structs-records/overview.en.md)
- Nearby: [Resilient HttpClient](../../recipes/http-client-resilience/overview.en.md), [Polly](../polly/overview.en.md), [NSubstitute](../nsubstitute/overview.en.md)
- In other tracks: [Python](../../../python/overview.en.md) — httpx
