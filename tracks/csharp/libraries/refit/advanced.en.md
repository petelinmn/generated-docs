---
title: "Refit: advanced"
lang: en
track: csharp
kind: library
doc: advanced
slug: refit
part: 2
tags: [refit, iapiresponse, dependency-injection, httpclientfactory, resilience]
versions: ".NET 11, C# 15; Refit 17.0.0, Refit.HttpClientFactory 17.0.0"
status: draft
updated: 2026-11-22
---

# Refit: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [basics](basics.en.md). **Time:** ~45 min.

## A response without exceptions

The return type `Task<IApiResponse<T>>` gives the status, headers and content without throwing on 4xx/5xx:

```csharp
[Get("/users/{id}")] Task<IApiResponse<UserDto>> GetUserResponseAsync(int id);

var resp = await api.GetUserResponseAsync(404);
// IsSuccessStatusCode=False, StatusCode=404, Content=null, Error=ApiException
var ok = await api.GetUserResponseAsync(5);       // ok.Content = UserDto { Id = 5, Name = User5 }
```

Verified: on `404` — `IsSuccessStatusCode = False`, `StatusCode = 404`, `Content = null`, `Error` is an `ApiException`; on success `Content` is filled. This approach suits cases where 404 is an ordinary situation, not an exception.

## Dependency injection

```csharp
services.AddRefitGeneratedClient<IUsersApi>(RefitSettings.ForJsonContext(ApiJson.Default))
        .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseUrl));

var api = sp.GetRequiredService<IUsersApi>();
```

Verified: the client from DI works (`UserDto { Id = 9, Name = User9 }`). `AddRefitGeneratedClient` returns an `IHttpClientBuilder` — everything from the `IHttpClientFactory` world hangs off it: `ConfigureHttpClient`, `AddHttpMessageHandler`, `AddStandardResilienceHandler()` ([HttpClient](../../recipes/http-client-resilience/overview.en.md)).

> In versions before 17 `AddRefitClient<T>()` did the same; in 17 it lives in the `Refit.Reflection` package.

> ⚠️ Verify: the chain `AddRefitGeneratedClient<T>(...).AddStandardResilienceHandler()` and a `DelegatingHandler` for a token were not run in this material.

## Headers and authorization

- Static: `[Headers("User-Agent: demo")]` on an interface/method.
- Dynamic: `[Header("Authorization")] string auth` (verified: `Bearer tok` reached the server).
- A token "per request": a `DelegatingHandler` in `AddHttpMessageHandler` or Refit's authorization provider.

## Forms, the query string, the body

| What | How |
|---|---|
| A JSON body | `[Body] CreateUser user` (verified) |
| A form | `[Body(BodySerializationMethod.UrlEncoded)] Dictionary<string,string>` (verified: `1|2`) |
| Query parameters | all parameters not in the path template (verified: `name=ann&page=2`) |
| Other names | `[AliasAs("name")]` |

For file upload (multipart), streams and collections in a query see the Refit documentation.

> ⚠️ Verify: multipart upload, collections in the query string and streamed responses were not run in this material.

## Serialization

In Refit 17 the main path is `System.Text.Json` with a source-generated context: fewer allocations, compatibility with trimming/Native AOT. Settings are passed to `RefitSettings.ForJsonContext(...)`. For Newtonsoft.Json and XML there are the packages `Refit.Newtonsoft.Json`, `Refit.Xml`.

> ⚠️ Verify: the Newtonsoft/XML serializers and behavior under Native AOT were not run in this material.

## Testing

A client is an interface, so unit tests substitute it ([NSubstitute](../nsubstitute/overview.en.md)). To test the request construction itself there is the `Refit.Testing` package (replacing responses and verifying calls).

> ⚠️ Verify: `Refit.Testing` was not run in this material.

## Best practices

| Rule | Why |
|---|---|
| A JSON context with `JsonSerializerDefaults.Web` for a camelCase API | otherwise silent zeros |
| `IApiResponse<T>` where 4xx is normal | no exceptions in the control flow |
| A client through DI and `IHttpClientFactory` | connection pools, DNS |
| Resilience through a handler, not in the interface | one place for the policy |
| One interface — one external service | clear boundaries |
| Mock the interface in tests | fast tests |

## Related materials

- Back: [basics](basics.en.md). Next: [expert](expert.en.md).
- [Resilient HttpClient](../../recipes/http-client-resilience/advanced.en.md) — `AddStandardResilienceHandler`.
- [NSubstitute](../nsubstitute/overview.en.md) — substituting the client in tests.
- [13 · DI, configuration, logging](../../modules/13-di-config-logging/overview.en.md)

## Sources

- [Refit: Dependency injection](https://www.reactiveui.net/documentation/refit/clients/dependency-injection/)
- [Refit: Responses](https://www.reactiveui.net/documentation/refit/results/responses/)
- [Refit: Errors](https://www.reactiveui.net/documentation/refit/results/errors/)
