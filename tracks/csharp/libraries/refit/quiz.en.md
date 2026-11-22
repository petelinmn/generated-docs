---
title: "Quiz: Refit"
lang: en
track: csharp
kind: library
doc: quiz
slug: refit
part: 2
tags: [refit, http-client, rest, json-context]
status: draft
updated: 2026-11-22
---

# Quiz: Refit

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What does Refit do?

- A) starts an HTTP server
- B) turns an interface with attributes into a typed HTTP client
- C) caches responses
- D) generates API documentation

<details><summary>Answer</summary>

**B.** The implementation is built by a source generator.
→ [Basics](basics.en.md#what-it-is-and-why)

</details>

## 2. 🟢 Where does the parameter `int page`, absent from the template `[Get("/users")]`, go?

- A) into the body
- B) into the query string (`?page=2`)
- C) into a header
- D) an error

<details><summary>Answer</summary>

**B.** Verified: `name=ann&page=2`.
→ [Basics](basics.en.md#the-client-interface)

</details>

## 3. 🟢 What does the call throw when the server answers `404`?

- A) `null`
- B) `ApiException` with `StatusCode = 404`
- C) `HttpRequestException` without details
- D) nothing

<details><summary>Answer</summary>

**B.** Verified: `404` and `Content = {"error":"no user"}`.
→ [Basics](basics.en.md#http-errors)

</details>

## 4. 🟢 What does a Refit 17 client need for JSON?

- A) nothing
- B) `RefitSettings.ForJsonContext(...)` with a source-generated `JsonSerializerContext`
- C) Newtonsoft.Json
- D) a `[Json]` attribute

<details><summary>Answer</summary>

**B.** Without a context — `NotSupportedException` inside `ApiException` (verified).
→ [Basics](basics.en.md#creating-a-client-and-the-json-context)

</details>

## 5. 🟡 How do you get a response to `404` without an exception?

- A) always `try/catch`
- B) return `Task<IApiResponse<T>>`
- C) `Task<T?>`
- D) you cannot

<details><summary>Answer</summary>

**B.** Verified: `IsSuccessStatusCode = False`, `Content = null`, `Error = ApiException`.
→ [Advanced](advanced.en.md#a-response-without-exceptions)

</details>

## 6. 🟡 How do you register a client in DI in Refit 17?

- A) `AddRefitClient<T>()` from the main package
- B) `AddRefitGeneratedClient<T>(settings)` (Refit.HttpClientFactory)
- C) `services.AddSingleton<T>()`
- D) `new T()`

<details><summary>Answer</summary>

**B.** Verified; `AddRefitClient` moved to `Refit.Reflection`.
→ [Advanced](advanced.en.md#dependency-injection)

</details>

## 7. 🟡 Select all correct statements

- [ ] A) `[Header("Authorization")]` on a parameter passes a header
- [ ] B) Refit performs retries itself
- [ ] C) `AliasAs` changes the parameter's name in the query
- [ ] D) a form is sent via `[Body(BodySerializationMethod.UrlEncoded)]`

<details><summary>Answer</summary>

**A, C, D.** B is wrong: retries are the job of an `IHttpClientFactory` handler.
→ [Advanced](advanced.en.md#headers-and-authorization)

</details>

## 8. 🟡 Open question: how do you organize an external API client in a service?

<details><summary>Key points of a good answer</summary>

- a Refit interface + a JSON context with `Web` settings;
- registration through `AddRefitGeneratedClient` and `IHttpClientBuilder`;
- a resilience handler (timeouts, retries), a token through a `DelegatingHandler`;
- `IApiResponse<T>` where 4xx is normal; mock the interface in tests.

</details>

## 9. 🔴 `UserDto` fields arrived as zeros without an error. The cause?

- A) the server returned an empty response
- B) the context is case-sensitive and the server returns camelCase
- C) no `[Body]`
- D) no `await`

<details><summary>Answer</summary>

**B.** Verified; `JsonSerializerDefaults.Web` solves it.
→ [Expert](expert.en.md#edges-and-gotchas)

</details>

## 10. 🔴 An example with `RestService.For<IApi>()` does not compile in Refit 17. Why?

- A) the method is removed forever
- B) the reflection path moved to `Refit.Reflection`; the main one is `ForGenerated`
- C) .NET Framework is needed
- D) a typo

<details><summary>Answer</summary>

**B.** Verified: CS0117 in a project with `Refit` alone.
→ [Expert](expert.en.md#internals)

</details>
