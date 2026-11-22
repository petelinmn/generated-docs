---
title: "Refit: basics"
lang: en
track: csharp
kind: library
doc: basics
slug: refit
part: 2
tags: [refit, http-client, rest, source-generator, json-context]
versions: ".NET 11, C# 15; Refit 17.0.0, Refit.HttpClientFactory 17.0.0"
status: draft
updated: 2026-11-22
---

# Refit: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [10 · async/await](../../modules/10-async-await/overview.en.md), [05 · Interfaces, structs, records](../../modules/05-interfaces-structs-records/overview.en.md). **Time:** ~30 min.

## What it is and why

Refit turns a C# interface into a typed HTTP client: attributes describe the method, URL, headers and body, and a **source generator** builds the implementation at compile time. Instead of a hand-written `HttpClient` + `GetFromJsonAsync` + building the query string you call ordinary interface methods.

```bash
dotnet add package Refit
dotnet add package Refit.HttpClientFactory   # DI and IHttpClientFactory
```

> **Version 17.** In Refit 17 the client **does not use reflection** by default: it is built by the generator and works with `System.Text.Json` through a source-generated context (`JsonSerializerContext`). The reflection-based `RestService.For<T>` and `AddRefitClient<T>` moved to the optional package `Refit.Reflection`. In older versions (before 17) `RestService.For<T>` was the main path. Verified on 17.0.0: in a project with `Refit` alone the method `RestService.For` does not exist (CS0117).

## The client interface

```csharp
interface IUsersApi
{
    [Get("/users/{id}")] Task<UserDto> GetUserAsync(int id);
    [Post("/users")] Task<UserDto> CreateAsync([Body] CreateUser user);
    [Get("/users")] Task<string> SearchAsync([AliasAs("name")] string name, int page,
        [Header("Authorization")] string auth, [Header("X-Tag")] string tag);
    [Post("/form")] Task<string> FormAsync([Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, string> form);
}

record UserDto(int Id, string Name);
record CreateUser(string Name);
```

| Attribute | Effect |
|---|---|
| `[Get]`, `[Post]`, `[Put]`, `[Delete]` | the HTTP method and path; `{id}` is substituted from the parameter |
| `[Body]` | the request body (JSON by default) |
| parameters not in the path | go into the query string (`?name=ann&page=2`) |
| `[AliasAs("name")]` | another name in the query |
| `[Header("…")]` | a header value from the parameter |
| `[Body(BodySerializationMethod.UrlEncoded)]` | an `application/x-www-form-urlencoded` form |

## Creating a client and the JSON context

```csharp
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(UserDto))]
[JsonSerializable(typeof(CreateUser))]
[JsonSerializable(typeof(string))]
partial class ApiJson : JsonSerializerContext { }

var http = new HttpClient { BaseAddress = new Uri(baseUrl) };
var api = RestService.ForGenerated<IUsersApi>(http, RefitSettings.ForJsonContext(ApiJson.Default));

var user = await api.GetUserAsync(7);        // UserDto { Id = 7, Name = User7 }
```

Verified against a local Kestrel server: `GET /users/7` → `UserDto { Id = 7, Name = User7 }`; a `POST` with a body returned `UserDto { Id = 1, Name = Ann }`; the request with parameters arrived as `{"name":"ann","page":2,"auth":"Bearer tok","tag":"t1"}`; the form — `"1|2"`.

**Two gotchas found while verifying:**

1. **Without a context — an exception.** A call without `RefitSettings.ForJsonContext(...)` gave an `ApiException` with an inner `NotSupportedException`: "These JSON serializer options have no type metadata. Supply a source-generated JsonSerializerContext …".
2. **Property name case.** ASP.NET Core returns JSON in camelCase (`"id"`, `"name"`). A context with default settings is case-sensitive and **silently** returned `UserDto { Id = 0, Name = }` — with no error. Fix it with `JsonSerializerDefaults.Web` (as above) or `PropertyNameCaseInsensitive`.

## HTTP errors

```csharp
try { await api.GetUserAsync(404); }
catch (ApiException e) { Console.WriteLine($"{(int)e.StatusCode} {e.Content}"); }
```

Verified: a `404` response → `ApiException`, `StatusCode = 404`, `Content = {"error":"no user"}`; a `500` response → `ApiException 500`. To avoid catching exceptions return `Task<IApiResponse<T>>` ([advanced](advanced.en.md#a-response-without-exceptions)).

## Beginner mistakes

- ❌ Forgetting the JSON context in Refit 17 → ✅ `RefitSettings.ForJsonContext(...)` ([see above](#creating-a-client-and-the-json-context)).
- ❌ A case-sensitive context with a camelCase server → ✅ `JsonSerializerDefaults.Web`.
- ❌ Creating an `HttpClient` by hand per call → ✅ `AddRefitGeneratedClient` + `IHttpClientFactory`.
- ❌ Expecting 4xx/5xx to return a value → ✅ they throw `ApiException` (or use `IApiResponse<T>`).
- ❌ Confusing path and query parameters → ✅ whatever is not in the path template goes into the query.

## Related materials

- Next: [advanced](advanced.en.md) — DI, `IApiResponse`, resilience.
- [Resilient HttpClient](../../recipes/http-client-resilience/overview.en.md) — timeouts and retries.
- [20 · Reflection and source generators](../../modules/20-reflection-and-source-generators/overview.en.md) — generators and JSON contexts.
- [Polly](../polly/overview.en.md) — resilience policies.
- [Python](../../../python/overview.en.md) — httpx.

## Sources

- [Refit on GitHub](https://github.com/reactiveui/refit)
- [Refit: documentation](https://reactiveui.net/documentation/refit/)
- [Refit: JSON](https://www.reactiveui.net/documentation/refit/serialization/json/)
