---
title: "Упражнения: Refit"
lang: ru
track: csharp
kind: library
doc: exercises
slug: refit
part: 2
tags: [refit, http-client, json-context, iapiresponse, dependency-injection]
status: draft
updated: 2026-11-22
---

# Упражнения: Refit

🌐 **Русский** · [English](exercises.en.md)

> Решайте сами. Решения проверены запуском: локальный Kestrel-сервер + Refit 17.0.0 и Refit.HttpClientFactory 17.0.0. Запускаемых версий в `code/` нет (уровня 5 нет). Сервер: `GET /users/{id}` (404 для `404`), `POST /users`, `GET /users?name&page`, `POST /form`.

## 1. 🟢 Первый клиент

**Задача:** опишите `IUsersApi` с `GetUserAsync(int id)` и `CreateAsync(CreateUser)`; создайте клиента и вызовите методы.

**Ожидаемый результат:** `UserDto { Id = 7, Name = User7 }` и `UserDto { Id = 1, Name = Ann }`.

<details><summary>Подсказка</summary>

`[Get("/users/{id}")]`, `[Post("/users")]`, `[Body]`; клиент — `RestService.ForGenerated<IUsersApi>(http, RefitSettings.ForJsonContext(ApiJson.Default))`, где `ApiJson` — `JsonSerializerContext` с `JsonSerializerDefaults.Web`.

</details>

<details><summary>Решение</summary>

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

Проверено: `GET: UserDto { Id = 7, Name = User7 }`, `POST: UserDto { Id = 1, Name = Ann }`.

</details>

## 2. 🟡 Параметры и заголовки

**Задача:** метод `SearchAsync(name, page, auth, tag)`: `name` в запросе под именем `name`, `page` — в запросе, `auth` и `tag` — заголовки `Authorization` и `X-Tag`.

**Ожидаемый результат:** сервер видит `{"name":"ann","page":2,"auth":"Bearer tok","tag":"t1"}`.

<details><summary>Подсказка</summary>

`[AliasAs("name")]`, `[Header("Authorization")]`, `[Header("X-Tag")]`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 2: query parameters and dynamic headers.
[Get("/users")] Task<string> SearchAsync([AliasAs("name")] string name, int page,
    [Header("Authorization")] string auth, [Header("X-Tag")] string tag);
```

Проверено: `query/header: {"name":"ann","page":2,"auth":"Bearer tok","tag":"t1"}`.

</details>

## 3. 🟡 Ошибки

**Задача:** вызовите `GetUserAsync(404)` двумя способами: с исключением и через `IApiResponse<UserDto>`.

**Ожидаемый результат:** `ApiException` со `StatusCode = 404` и `Content = {"error":"no user"}`; `IApiResponse` — `IsSuccessStatusCode = False`, `Content = null`, `Error = ApiException`.

<details><summary>Подсказка</summary>

Добавьте метод `Task<IApiResponse<UserDto>> GetUserResponseAsync(int id)` с тем же `[Get]`.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 3: exceptions vs IApiResponse.
try { await api.GetUserAsync(404); }
catch (ApiException e) { Console.WriteLine($"ApiException: {(int)e.StatusCode} content={e.Content}"); }

var resp = await api.GetUserResponseAsync(404);
Console.WriteLine($"{resp.IsSuccessStatusCode} {(int)resp.StatusCode} {resp.Content is null} {resp.Error is not null}");
```

Проверено: `ApiException: 404 content={"error":"no user"}`; `IApiResponse: IsSuccessStatusCode=False status=404 Content=null Error=ApiException`.

</details>

## 4. 🔴 DI и молчаливые нули

**Задача:** (а) зарегистрируйте клиент в DI; (б) убедитесь, что без `JsonSerializerDefaults.Web` поля приходят нулями при camelCase-ответе.

**Ожидаемый результат:** клиент из DI вернул `UserDto { Id = 9, Name = User9 }`; контекст без `Web` — `UserDto { Id = 0, Name =  }`.

<details><summary>Подсказка</summary>

`services.AddRefitGeneratedClient<IUsersApi>(settings).ConfigureHttpClient(...)`; уберите атрибут `[JsonSourceGenerationOptions(...)]` и сравните.

</details>

<details><summary>Решение</summary>

```csharp
// Exercise 4: DI registration; case-sensitive default context returns zeros silently.
services.AddRefitGeneratedClient<IUsersApi>(RefitSettings.ForJsonContext(ApiJson.Default))
        .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseUrl));
var user = await sp.GetRequiredService<IUsersApi>().GetUserAsync(9);
```

Проверено: с `Web` — `DI client: UserDto { Id = 9, Name = User9 }`; без — `UserDto { Id = 0, Name =  }` без исключения.

</details>
