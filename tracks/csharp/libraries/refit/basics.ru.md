---
title: "Refit: основы"
lang: ru
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

# Refit: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [10 · async/await](../../modules/10-async-await/overview.ru.md), [05 · Интерфейсы, структуры, records](../../modules/05-interfaces-structs-records/overview.ru.md). **Время:** ~30 мин.

## Что это и зачем

Refit превращает интерфейс C# в типизированный HTTP-клиент: атрибуты описывают метод, URL, заголовки и тело, а реализацию на этапе компиляции строит **source generator**. Вместо ручного `HttpClient` + `GetFromJsonAsync` + сборки строки запроса вы вызываете обычные методы интерфейса.

```bash
dotnet add package Refit
dotnet add package Refit.HttpClientFactory   # DI and IHttpClientFactory
```

> **Версия 17.** В Refit 17 клиент по умолчанию **не использует рефлексию**: он строится генератором и работает с `System.Text.Json` через source-generated контекст (`JsonSerializerContext`). Рефлексивные `RestService.For<T>` и `AddRefitClient<T>` вынесены в необязательный пакет `Refit.Reflection`. В старых версиях (до 17) основным путём был `RestService.For<T>`. Проверено на 17.0.0: в проекте с одним `Refit` метода `RestService.For` нет (CS0117).

## Интерфейс клиента

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

| Атрибут | Эффект |
|---|---|
| `[Get]`, `[Post]`, `[Put]`, `[Delete]` | HTTP-метод и путь; `{id}` подставляется из параметра |
| `[Body]` | тело запроса (JSON по умолчанию) |
| параметры, не вошедшие в путь | идут в строку запроса (`?name=ann&page=2`) |
| `[AliasAs("name")]` | другое имя в запросе |
| `[Header("…")]` | значение заголовка из параметра |
| `[Body(BodySerializationMethod.UrlEncoded)]` | форма `application/x-www-form-urlencoded` |

## Создание клиента и JSON-контекст

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

Проверено на локальном Kestrel-сервере: `GET /users/7` → `UserDto { Id = 7, Name = User7 }`; `POST` с телом вернул `UserDto { Id = 1, Name = Ann }`; запрос с параметрами дошёл как `{"name":"ann","page":2,"auth":"Bearer tok","tag":"t1"}`; форма — `"1|2"`.

**Два подвоха, обнаруженных при проверке:**

1. **Без контекста — исключение.** Вызов без `RefitSettings.ForJsonContext(...)` дал `ApiException` с внутренним `NotSupportedException`: «These JSON serializer options have no type metadata. Supply a source-generated JsonSerializerContext …».
2. **Регистр имён свойств.** ASP.NET Core отдаёт JSON в camelCase (`"id"`, `"name"`). Контекст с настройками по умолчанию регистрозависим и **молча** вернул `UserDto { Id = 0, Name = }` — без ошибки. Лечится `JsonSerializerDefaults.Web` (как выше) или `PropertyNameCaseInsensitive`.

## Ошибки HTTP

```csharp
try { await api.GetUserAsync(404); }
catch (ApiException e) { Console.WriteLine($"{(int)e.StatusCode} {e.Content}"); }
```

Проверено: ответ `404` → `ApiException`, `StatusCode = 404`, `Content = {"error":"no user"}`; ответ `500` → `ApiException 500`. Чтобы не ловить исключения, возвращайте `Task<IApiResponse<T>>` ([продвинутый](advanced.ru.md#ответ-без-исключений)).

## Частые ошибки новичка

- ❌ Забыть JSON-контекст в Refit 17 → ✅ `RefitSettings.ForJsonContext(...)` ([см. выше](#создание-клиента-и-json-контекст)).
- ❌ Регистрозависимый контекст при camelCase-сервере → ✅ `JsonSerializerDefaults.Web`.
- ❌ Создавать `HttpClient` на каждый вызов вручную → ✅ `AddRefitGeneratedClient` + `IHttpClientFactory`.
- ❌ Ожидать, что 4xx/5xx вернут значение → ✅ они бросают `ApiException` (или используйте `IApiResponse<T>`).
- ❌ Путать параметр пути и запроса → ✅ то, чего нет в шаблоне пути, уходит в запрос.

## Связанные материалы

- Дальше: [продвинутый](advanced.ru.md) — DI, `IApiResponse`, устойчивость.
- [Устойчивый HttpClient](../../recipes/http-client-resilience/overview.ru.md) — таймауты и ретраи.
- [20 · Reflection и source generators](../../modules/20-reflection-and-source-generators/overview.ru.md) — генераторы и JSON-контексты.
- [Polly](../polly/overview.ru.md) — политики устойчивости.
- [Python](../../../python/overview.ru.md) — httpx.

## Источники

- [Refit на GitHub](https://github.com/reactiveui/refit)
- [Refit: документация](https://reactiveui.net/documentation/refit/)
- [Refit: JSON](https://www.reactiveui.net/documentation/refit/serialization/json/)
