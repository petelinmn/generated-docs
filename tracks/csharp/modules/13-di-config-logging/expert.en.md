---
title: "DI, configuration, logging: expert"
lang: en
track: csharp
kind: module
doc: expert
slug: 13-di-config-logging
part: 2
tags: [dependency-injection, service-provider, disposal, constructor-selection, options-factory, configuration-providers, logging-pipeline, interview-questions]
versions: ".NET 11, C# 15; Microsoft.Extensions.* 11.0.0"
status: draft
updated: 2026-11-22
---

# DI, configuration, logging: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

**Prerequisites:** [basic](basics.en.md) and [advanced](advanced.en.md) theory. **Time:** ~1.5 h.

## How the container is built

A `ServiceCollection` is a list of descriptors (`ServiceDescriptor`): the service type, the way of creation (an implementation type, a ready instance, a factory), the lifetime and an optional key. `BuildServiceProvider()` creates the **root provider**; `CreateScope()` creates a child scope that keeps its own `Scoped` instances. Singletons live in the root.

- **Constructor selection.** The container picks the constructor with the largest number of parameters, all of which can be resolved. Verified: for a type with three constructors `()`, `(X)`, `(X, Y)` the `(X, Y)` one was chosen. If two constructors fit and neither "covers" the other — `InvalidOperationException: Unable to activate type 'Ambig'. The following constructors are ambiguous`.
- **Cycles** are detected during resolution: `A` depends on `B`, `B` on `A` → `InvalidOperationException: A circular dependency was detected for the service of type 'C1'` (verified).
- **The root provider and Scoped.** Without `ValidateScopes`, requesting a `Scoped` service from the root **works** and returns one instance for the whole lifetime of the provider (verified: `ReferenceEquals` → `True`), i.e. it behaves like a singleton. With `ValidateScopes = true` — `InvalidOperationException: Cannot resolve scoped service 'S1' from root provider.`
- **Availability check.** `IServiceProviderIsService.IsService(type)` answers whether a type is resolvable without creating an instance (verified: `True` for a registered one, `False` for an unregistered one).
- **Implementation.** On the first access to a type the provider builds and caches a resolution "plan" (based on compiled expressions or `DynamicMethod`), so the first call is more expensive than later ones. Modes without dynamic code generation (Native AOT) use a different path.

### Disposal

The container owns everything it created and calls `Dispose` on `IDisposable`/`IAsyncDisposable` services when the scope or provider is disposed, **in the reverse order of creation** (verified: `B1, A1` for `B1` that depends on `A1`). Services you passed as ready instances (`AddSingleton(instance)`) the container does **not** dispose.

The gotcha with `Transient`: the container keeps references to created disposable `Transient` instances until the scope is disposed. Verified: three requests from the root provider — three `D` created, none disposed (`disposed=0`) until `provider.Dispose()`, after which `disposed=3`. In a long-lived root provider this is a leak: request such services inside a short scope or do not register disposable types as `Transient`.

## Options internals

`IOptions<T>` does not read configuration directly: an `OptionsFactory<T>` creates the object, applying **configurators** in order: `IConfigureOptions<T>` (including `Configure`/`Bind`), then `IPostConfigureOptions<T>`, then the `IValidateOptions<T>` checks. The result is cached in an `IOptionsMonitorCache<T>`. Options have **names** (`IOptionsSnapshot<T>.Get("name")`, `Configure<T>("name", …)`) — for several sets of one type. `IOptionsMonitor` subscribes to configuration change tokens: on a change the cache is reset and `OnChange` is called. That is why in the verified example the monitor showed the new value at once, `IOptionsSnapshot` — in a new scope, and `IOptions` — the old one.

`ValidateOnStart` registers the check as part of the host start: the error ends up in `Hosting failed to start` with an `OptionsValidationException` and a list of problems (verified: `Host` is required, `Port` is out of range — 2 problems).

## Configuration internals

`ConfigurationBuilder` puts providers into a list; `Build()` creates the root configuration, which on reading a key walks the providers **from the end** until it finds a value (so the last one wins). Keys are case-insensitive, the separator is `:`; arrays use indices (`Tags:0`). Providers that can track changes (a file with `reloadOnChange`) publish an `IChangeToken`; on reload the value is read anew. Environment variables turn `__` into `:`; the prefix is stripped (`APP__Mail__Host` → `Mail:Host`: verified).

## The logging pipeline

`ILogger<T>` is a thin wrapper: the category logger holds a set of loggers of all **providers** and walks them on a call; each decides through `IsEnabled` whether to write. Filters by category and level are applied earlier. The template is parsed once and cached; values are passed separately from the text, so a structured-logging provider (Serilog, OpenTelemetry) receives named properties. `[LoggerMessage]` removes parsing and argument boxing on every call. Verified with a custom `ILoggerProvider`: the level, the formatted message, the scope and the exception type were received; a `Debug` message at `Information` was dropped.

## Reference

| API | Purpose |
|---|---|
| `AddSingleton/Scoped/Transient<TService, TImpl>()` | registration by type |
| `AddSingleton<T>(instance)` | a ready instance, the container does not dispose it |
| `AddSingleton<T>(sp => …)` | a factory |
| `AddKeyed*` / `GetRequiredKeyedService` / `[FromKeyedServices]` | keyed services (.NET 8) |
| `TryAdd*`, `Replace`, `RemoveAll` | managing registrations |
| `ServiceProviderOptions.ValidateScopes/ValidateOnBuild` | checks at build time |
| `IServiceScopeFactory` | create a scope from a singleton |
| `IOptions<T>` / `IOptionsSnapshot<T>` / `IOptionsMonitor<T>` | settings |
| `ValidateDataAnnotations` / `Validate` / `ValidateOnStart` | settings validation |
| `BackgroundService`, `IHostedService`, `IHostApplicationLifetime` | the host lifecycle |
| `ILogger<T>`, `ILoggerFactory`, `ILoggerProvider`, `[LoggerMessage]` | logging |

## Gotchas

**1. A singleton with a `Scoped` dependency without the checks on.** In `Production` the checks are off by default: the application was built (verified), and the error shows up as one request's data being "foreign" in another.

**2. A disposable `Transient` in the root.** It is kept until the provider is disposed (see above).

**3. `AddSingleton(instance)` and `Dispose`.** The container does not dispose instances you passed — you are the owner.

**4. Registration order.** For one interface the last one wins; `TryAdd` does not replace an existing one (verified: 2 registrations).

**5. `ValidateDataAnnotations` is a separate package.** Without `Microsoft.Extensions.Options.DataAnnotations` the method is unavailable (a compile error — verified).

**6. `IOptions<T>` is not updated.** For hot reload you need `IOptionsMonitor`/`IOptionsSnapshot`.

**7. `ClearProviders` requires `using Microsoft.Extensions.Logging;`.** Without the directive the extension method is not visible (verified).

**8. Service Locator.** Calling `provider.GetService<T>()` from business code hides dependencies: the class cannot be understood from its constructor and is hard to test.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| The built-in container | standard, fast, enough for most | no assembly auto-registration, decorators, property injection |
| Autofac, Lamar | modules, decorators, property injection | an extra dependency and complexity |
| Scrutor | assembly scanning and decorators on top of the built-in one | an external library |
| Manual composition (Pure DI) | everything is visible in one place | a lot of code at the application root |
| Configuration through `IConfiguration` directly | simple | no typing and validation |

## Interview questions

<details><summary>1. What is dependency injection and why is it needed?</summary>

A class receives its dependencies from outside (through the constructor) rather than creating them. This reduces coupling, allows substituting implementations (tests) and centralizes object creation in the container.

</details>

<details><summary>2. How do Singleton, Scoped and Transient differ?</summary>

A Singleton — one instance per application; Scoped — one per scope (in the web — per request); Transient — a new one on every request for the service. Verified: with 2 scopes × 2 requests `singleton=1 scoped=2 transient=4` were created.

</details>

<details><summary>3. What is a "captive dependency" and how do you find it?</summary>

A long-lived service (a singleton) injects a short-lived one (scoped), and the latter "gets stuck". It is found by enabling `ValidateScopes`/`ValidateOnBuild`; by default they are on only in `Development`.

</details>

<details><summary>4. How do you get a Scoped service from a Singleton?</summary>

Inject `IServiceScopeFactory`, create a scope for each operation and resolve the service from it; dispose the scope with `using`.

</details>

<details><summary>5. What happens with several registrations of one interface?</summary>

`GetService<T>` returns the last, `GetServices<T>` — all in order. `TryAdd*` will not add if a registration already exists.

</details>

<details><summary>6. How does the container choose a constructor?</summary>

It takes the constructor with the most resolvable parameters; on ambiguity it throws an `InvalidOperationException` about ambiguous constructors.

</details>

<details><summary>7. Who disposes services and when?</summary>

The container disposes the `IDisposable`s it created when the scope or provider is disposed, in the reverse order of creation; it does not dispose passed-in ready instances.

</details>

<details><summary>8. What is the difference between IOptions, IOptionsSnapshot and IOptionsMonitor?</summary>

`IOptions` — a singleton without reload; `IOptionsSnapshot` — scoped, updated at the scope boundary; `IOptionsMonitor` — a singleton with immediate update and `OnChange`.

</details>

<details><summary>9. How do you validate settings at startup?</summary>

`AddOptions<T>().Bind(...).ValidateDataAnnotations().ValidateOnStart()` — the host will not start with invalid settings (`OptionsValidationException`).

</details>

<details><summary>10. How do configuration layers override each other?</summary>

The provider added later wins: usually `appsettings.json` → the environment-specific file → secrets (in Development) → environment variables → the command line.

</details>

<details><summary>11. Why is a message template better than interpolation in logs?</summary>

A template keeps structure (named properties) for search and aggregation, and formatting happens only if the level is enabled; interpolation always formats the string and loses the structure.

</details>

<details><summary>12. Trick question: why did the application build, yet in production one user's data is visible to another?</summary>

A singleton holds a Scoped service (for example `DbContext`), and scope checks are off by default in `Production` — there is no build error (verified), but the instance is shared by all requests.

</details>

<details><summary>13. Trick question: why does memory grow with `AddTransient<MyDisposable>()` and requests from the root provider?</summary>

The container keeps references to disposable Transient instances until the provider is disposed (verified: 3 created, 0 disposed). Request them inside a short scope.

</details>

<details><summary>14. Trick question: will the container dispose an instance passed to `AddSingleton(instance)`?</summary>

No: you remain the owner. It disposes only what it created itself.

</details>

## Related materials

- Back: [advanced theory](advanced.en.md). Check yourself: [quiz](quiz.en.md) and [exercises](exercises.en.md).
- [12 · Memory and GC](../12-memory-and-gc/overview.en.md) — `Dispose`, finalizers.
- [10 · Async/await](../10-async-await/overview.en.md) — `IAsyncDisposable`, canceling background services.
- [15 · EF Core](../15-ef-core/overview.en.md) — the `DbContext` lifetime.
- [Python](../../../python/overview.en.md) — dependencies in FastAPI, `logging`.

## Sources

- [Dependency injection guidelines](https://learn.microsoft.com/dotnet/core/extensions/dependency-injection-guidelines)
- [Configuration providers](https://learn.microsoft.com/dotnet/core/extensions/configuration-providers)
- [Logging providers](https://learn.microsoft.com/dotnet/core/extensions/logging-providers)
- [`ServiceProviderOptions`](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.serviceprovideroptions)
