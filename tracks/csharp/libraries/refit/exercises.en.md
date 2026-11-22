---
title: "Exercises: Refit"
lang: en
track: csharp
kind: library
doc: exercises
slug: refit
part: 2
tags: [refit, http-client, json-context, iapiresponse, dependency-injection]
status: draft
updated: 2026-11-22
---

# Exercises: Refit

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself. The solutions were verified by running: a local Kestrel server + Refit 17.0.0 and Refit.HttpClientFactory 17.0.0. There are no runnable versions in `code/` (level 5 does not exist). The server: `GET /users/{id}` (404 for `404`), `POST /users`, `GET /users?name&page`, `POST /form`.

## 1. 🟢 The first client

**Task:** describe `IUsersApi` with `GetUserAsync(int id)` and `CreateAsync(CreateUser)`; create the client and call the methods.

**Expected result:** `UserDto { Id = 7, Name = User7 }` and `UserDto { Id = 1, Name = Ann }`.

<details><summary>Hint</summary>

`[Get("/users/{id}")]`, `[Post("/users")]`, `[Body]`; the client — `RestService.ForGenerated<IUsersApi>(http, RefitSettings.ForJsonContext(ApiJson.Default))`, where `ApiJson` is a `JsonSerializerContext` with `JsonSerializerDefaults.Web`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 1: interface + source-generated JSON context + generated client.
interface IUsersApi
{
    [Get("/users/{id}")] Task<UserDto> GetUserAsync(int id);
    [Post("/users")] Task<UserDto> CreateAsync([Body] CreateUser user);
}

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(UserDto))]
[JsonSerializable(typeof(CreateUser))]
partial class ApiJson : JsonSerializerContext { }

var api = RestService.ForGenerated<IUsersApi>(
    new HttpClient { BaseAddress = new Uri(baseUrl) }, RefitSettings.ForJsonContext(ApiJson.Default));
```

Verified: `GET: UserDto { Id = 7, Name = User7 }`, `POST: UserDto { Id = 1, Name = Ann }`.

</details>

## 2. 🟡 Parameters and headers

**Task:** a method `SearchAsync(name, page, auth, tag)`: `name` in the query under the name `name`, `page` — in the query, `auth` and `tag` — the headers `Authorization` and `X-Tag`.

**Expected result:** the server sees `{"name":"ann","page":2,"auth":"Bearer tok","tag":"t1"}`.

<details><summary>Hint</summary>

`[AliasAs("name")]`, `[Header("Authorization")]`, `[Header("X-Tag")]`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 2: query parameters and dynamic headers.
[Get("/users")] Task<string> SearchAsync([AliasAs("name")] string name, int page,
    [Header("Authorization")] string auth, [Header("X-Tag")] string tag);
```

Verified: `query/header: {"name":"ann","page":2,"auth":"Bearer tok","tag":"t1"}`.

</details>

## 3. 🟡 Errors

**Task:** call `GetUserAsync(404)` two ways: with an exception and through `IApiResponse<UserDto>`.

**Expected result:** `ApiException` with `StatusCode = 404` and `Content = {"error":"no user"}`; `IApiResponse` — `IsSuccessStatusCode = False`, `Content = null`, `Error = ApiException`.

<details><summary>Hint</summary>

Add a method `Task<IApiResponse<UserDto>> GetUserResponseAsync(int id)` with the same `[Get]`.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 3: exceptions vs IApiResponse.
try { await api.GetUserAsync(404); }
catch (ApiException e) { Console.WriteLine($"ApiException: {(int)e.StatusCode} content={e.Content}"); }

var resp = await api.GetUserResponseAsync(404);
Console.WriteLine($"{resp.IsSuccessStatusCode} {(int)resp.StatusCode} {resp.Content is null} {resp.Error is not null}");
```

Verified: `ApiException: 404 content={"error":"no user"}`; `IApiResponse: IsSuccessStatusCode=False status=404 Content=null Error=ApiException`.

</details>

## 4. 🔴 DI and silent zeros

**Task:** (a) register the client in DI; (b) make sure that without `JsonSerializerDefaults.Web` the fields arrive as zeros on a camelCase response.

**Expected result:** the client from DI returned `UserDto { Id = 9, Name = User9 }`; a context without `Web` — `UserDto { Id = 0, Name =  }`.

<details><summary>Hint</summary>

`services.AddRefitGeneratedClient<IUsersApi>(settings).ConfigureHttpClient(...)`; remove the `[JsonSourceGenerationOptions(...)]` attribute and compare.

</details>

<details><summary>Solution</summary>

```csharp
// Exercise 4: DI registration; case-sensitive default context returns zeros silently.
services.AddRefitGeneratedClient<IUsersApi>(RefitSettings.ForJsonContext(ApiJson.Default))
        .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseUrl));
var user = await sp.GetRequiredService<IUsersApi>().GetUserAsync(9);
```

Verified: with `Web` — `DI client: UserDto { Id = 9, Name = User9 }`; without — `UserDto { Id = 0, Name =  }` with no exception.

</details>
