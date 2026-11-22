---
title: "Refit: expert"
lang: en
track: csharp
kind: library
doc: expert
slug: refit
part: 2
tags: [refit, source-generator, native-aot, json-serialization, interview-questions]
versions: ".NET 11, C# 15; Refit 17.0.0"
status: draft
updated: 2026-11-22
---

# Refit: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

**Prerequisites:** [basics](basics.en.md) and [advanced](advanced.en.md). **Time:** ~30 min.

## Internals

In Refit 17 the interface implementation is **generated at compile time** (a Roslyn source generator, needing Roslyn 4.8+ and `PackageReference`): for each method the generator creates code that builds an `HttpRequestMessage` and sends it through a shared executor (`RequestExecutionHelpers`). Factories of generated clients are registered in `RestService`; `RestService.ForGenerated<T>(HttpClient, RefitSettings)` finds the factory and creates the client without reflection. Thanks to this the client is compatible with trimming and Native AOT, and interface errors (a wrong path, duplicate attributes) are caught by analyzers at build time.

The old path (`RestService.For<T>` and building requests through reflection) moved to the `Refit.Reflection` package — for those who need compatibility with the old behavior.

> ⚠️ Verify: the details of the generator, analyzers and the `Refit.Reflection` package are taken from the package README and were not reproduced by running (except that `RestService.For` is absent in `Refit` alone — verified CS0117).

## Edges and gotchas

**1. JSON without a context does not work.** Verified: `NotSupportedException` ("no type metadata") inside `ApiException`.

**2. The default case.** Verified: a context without `Web` settings returned zeros on a camelCase response — without an error.

**3. Exceptions on 4xx/5xx.** Verified: `ApiException` with `StatusCode` and `Content`; `IApiResponse<T>` is the alternative without exceptions.

**4. Parameters not from the path go into the query.** Verified: `name=ann&page=2`; for POST do not confuse them with the body — the body is set by `[Body]`.

**5. Version 17 breaks old instructions.** `RestService.For<T>` and `AddRefitClient<T>` from old articles are not available in `Refit` alone (CS0117/CS1061, verified) — use `ForGenerated`/`AddRefitGeneratedClient` or add `Refit.Reflection`.

**5a. Timeouts and retried requests.** Refit itself does not retry: for retries use an `IHttpClientFactory` handler ([Polly](../polly/overview.en.md)); request bodies must be replayable.

**6. One client — one base address.** `BaseAddress` is set on the `HttpClient`; for several environments — several registrations.

**7. Interfaces and generic methods.** Restrictions on generics and complex signatures are diagnosed by the analyzer — read the compiler messages.

**8. Tokens in headers.** A static `[Header]` ends up in logs with careless request logging — mask it.

**9. Testability.** An interface is easy to substitute, but that does not test request construction — you need integration tests with a local server (as in the examples) or `Refit.Testing`.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| Refit | less boilerplate, a typed contract | a dependency, its own rules (the JSON context) |
| `HttpClient` + `System.Net.Http.Json` | no dependencies | paths, parameters, errors by hand |
| A hand-written typed client | full control | a lot of code per method |
| A client generated from OpenAPI (NSwag, Kiota) | the contract comes from a spec | generation, big models |
| gRPC | a strict contract, streams | a different protocol ([gRPC](../../recipes/grpc-service/overview.en.md)) |

## Interview questions

<details><summary>1. What is Refit?</summary>

A library that turns an interface with attributes into a typed HTTP client; the implementation is built by a source generator.

</details>

<details><summary>2. How does Refit send parameters that are not in the path?</summary>

In the query string (verified: `name=ann&page=2`); the body — only a parameter with `[Body]`.

</details>

<details><summary>3. How do you handle a 404 without an exception?</summary>

Return `Task<IApiResponse<T>>` and look at `IsSuccessStatusCode`/`StatusCode`/`Error` (verified).

</details>

<details><summary>4. What does Refit throw on 4xx/5xx by default?</summary>

`ApiException` with `StatusCode` and `Content` (verified).

</details>

<details><summary>5. How do you register a client in DI?</summary>

`AddRefitGeneratedClient<T>(settings)` (Refit.HttpClientFactory) and configure the `IHttpClientBuilder` (verified); in old versions — `AddRefitClient<T>`.

</details>

<details><summary>6. Why does Refit 17 need a `JsonSerializerContext`?</summary>

The client uses no reflection: serialization goes through source-generated metadata; without it — `NotSupportedException` (verified). This gives AOT compatibility.

</details>

<details><summary>7. Why did the fields deserialize as zeros although there is no error?</summary>

The default context is case-sensitive while the server returns camelCase; you need `JsonSerializerDefaults.Web` (verified).

</details>

<details><summary>8. How do you add authorization?</summary>

`[Header("Authorization")]` on a parameter (verified), or a `DelegatingHandler`/authorization provider.

</details>

<details><summary>9. Trick question: an example from an article with `RestService.For<IApi>()` does not compile in Refit 17. Why?</summary>

The reflection path moved to `Refit.Reflection`; the main package has `ForGenerated` (verified CS0117).

</details>

<details><summary>10. Trick question: you added a Refit client but there are no retries. Where would they come from?</summary>

Refit does not do them; add a resilience handler on the `IHttpClientBuilder` ([Resilient HttpClient](../../recipes/http-client-resilience/overview.en.md)).

</details>

<details><summary>11. How do you test code that depends on a Refit client?</summary>

Substitute the interface (NSubstitute) for unit tests; test request construction in integration tests or with `Refit.Testing`.

</details>

## Related materials

- Back: [advanced](advanced.en.md). Check yourself: [quiz](quiz.en.md) and [exercises](exercises.en.md).
- [20 · Reflection and source generators](../../modules/20-reflection-and-source-generators/overview.en.md)
- [Resilient HttpClient](../../recipes/http-client-resilience/overview.en.md)
- [gRPC service](../../recipes/grpc-service/overview.en.md) — an alternative for service-to-service exchange.

## Sources

- [Refit README](https://github.com/reactiveui/refit)
- [Refit: Client creation](https://www.reactiveui.net/documentation/refit/clients/creation/)
- [Refit: AOT](https://www.reactiveui.net/documentation/refit/aot/)
