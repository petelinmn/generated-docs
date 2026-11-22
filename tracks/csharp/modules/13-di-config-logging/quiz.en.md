---
title: "Quiz: DI, configuration, logging"
lang: en
track: csharp
kind: module
doc: quiz
slug: 13-di-config-logging
part: 2
tags: [dependency-injection, service-lifetime, options-pattern, configuration, logging]
status: draft
updated: 2026-11-22
---

# Quiz: DI, configuration, logging

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 How many instances will be created?

```csharp
sc.AddSingleton<SingletonSvc>(); sc.AddScoped<ScopedSvc>(); sc.AddTransient<TransientSvc>();
// two scopes; in each scope every service is requested twice
```

- A) singleton=1, scoped=1, transient=1
- B) singleton=1, scoped=2, transient=4
- C) singleton=2, scoped=2, transient=2
- D) singleton=1, scoped=4, transient=4

<details><summary>Answer</summary>

**B.** A Singleton — one per application; Scoped — one per scope (two scopes → two); Transient — a new one for every request (2 × 2 = 4). Verified by running.
→ [Basics](basics.en.md#service-lifetimes)

</details>

## 2. 🟢 What does `GetRequiredService<IGreeter>()` throw if the service is not registered, and what does `GetService<IGreeter>()` return?

- A) `NullReferenceException` / `null`
- B) `InvalidOperationException` / `null`
- C) `KeyNotFoundException` / an exception
- D) `null` / `null`

<details><summary>Answer</summary>

**B.** `GetRequiredService` throws `InvalidOperationException` ("No service for type … has been registered"), `GetService` returns `null` (verified).
→ [Basics](basics.en.md#dependency-injection)

</details>

## 3. 🟢 Which configuration source wins if the same key is set in `appsettings.json`, an environment variable and the command line?

- A) `appsettings.json`
- B) the environment variable
- C) the command line
- D) the first one added

<details><summary>Answer</summary>

**C.** The provider added **later** wins; in a typical host the order is files, environment variables, the command line (verified on layers: the environment overrode memory, and `--Mail:Host=cli` did too).
→ [Basics](basics.en.md#configuration)

</details>

## 4. 🟢 Why should you not write `log.LogInformation($"User {id} logged in")`?

- A) it does not compile
- B) the message structure is lost and the text is always formatted, even when the level is off
- C) it does not work with the `Information` level
- D) exceptions are not logged

<details><summary>Answer</summary>

**B.** The template `"User {UserId} logged in"` with an argument keeps a named property for search and aggregation, and formatting happens only when the level is enabled.
→ [Basics](basics.en.md#logging)

</details>

## 5. 🟡 What happens when the container is built?

```csharp
sc.AddScoped<IScoped, Impl>(); sc.AddSingleton<Needs>();   // class Needs(IScoped s)
using var p = sc.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
```

- A) all is fine
- B) an exception: a scoped service cannot be consumed in a singleton
- C) `Needs` becomes scoped automatically
- D) a compiler warning

<details><summary>Answer</summary>

**B.** The check finds the "captive dependency": "Cannot consume scoped service 'IScoped' from singleton 'Needs'" (verified). Without these options in `Production` there will be no error at build time.
→ [Advanced theory](advanced.en.md#the-captive-dependency-problem)

</details>

## 6. 🟡 What does the code return?

```csharp
sc.AddSingleton<IGreeter, EnGreeter>();
sc.AddSingleton<IGreeter, RuGreeter>();
Console.WriteLine(provider.GetRequiredService<IGreeter>().Hello());
Console.WriteLine(provider.GetServices<IGreeter>().Count());
```

<details><summary>Answer</summary>

```text
привет
2
```

`GetRequiredService` returns the last registration (`RuGreeter`), `GetServices` returns all (verified).
→ [Advanced theory](advanced.en.md#several-implementations-of-one-interface)

</details>

## 7. 🟡 Which interface should a singleton service use if it needs fresh settings after a file changes?

- A) `IOptions<T>`
- B) `IOptionsSnapshot<T>`
- C) `IOptionsMonitor<T>`
- D) any

<details><summary>Answer</summary>

**C.** `IOptionsMonitor<T>` is a singleton with immediate update and an `OnChange` event. `IOptions<T>` is not updated, and `IOptionsSnapshot<T>` is scoped and updates only in a new scope (verified: in the same scope the value stayed old).
→ [Advanced theory](advanced.en.md#options-validation-and-reloading)

</details>

## 8. 🟡 Select all correct statements about `ValidateOnStart`

- [ ] A) it validates settings when the host starts
- [ ] B) on an error the host does not start (`OptionsValidationException`)
- [ ] C) it replaces the `[Required]`/`[Range]` attributes
- [ ] D) `ValidateDataAnnotations` needs the separate package `Microsoft.Extensions.Options.DataAnnotations`

<details><summary>Answer</summary>

**A, B, D.** C is wrong: `ValidateOnStart` only selects the **moment** of the check, while the rules themselves are set by attributes or `Validate(...)`. Verified: with `Port = 0` and no `Host` the start fails with 2 problems.
→ [Advanced theory](advanced.en.md#validating-settings)

</details>

## 9. 🟡 Open question: a singleton service needs a `DbContext` (scoped). How do you do it correctly?

<details><summary>Key points of a good answer</summary>

- it cannot be injected directly — a "captive dependency": one `DbContext` for all requests, and it is not thread-safe;
- inject `IServiceScopeFactory`, create a scope for each operation and get the `DbContext` from it; dispose the scope with `using`;
- or make the service itself Scoped if it does not have to be a singleton;
- enable `ValidateScopes`/`ValidateOnBuild` so the error is found early ([15 · EF Core](../15-ef-core/overview.en.md)).

</details>

## 10. 🔴 What will happen?

```csharp
var sc = new ServiceCollection(); sc.AddTransient<D>();   // D : IDisposable
var p = sc.BuildServiceProvider();
for (int i = 0; i < 3; i++) p.GetRequiredService<D>();
Console.WriteLine($"created={D.Created} disposed={D.Disposed}");
```

- A) `created=3 disposed=3`
- B) `created=3 disposed=0`
- C) `created=1 disposed=0`
- D) `created=0 disposed=0`

<details><summary>Answer</summary>

**B.** The container keeps created disposable Transient instances until the provider or scope is disposed (after `p.Dispose()` it became `disposed=3`). In a long-lived root provider this is a leak: request such services in a short scope.
→ [Expert theory](expert.en.md#disposal)

</details>

## 11. 🔴 Which constructor will the container choose?

```csharp
class Many { public Many() {} public Many(X x) {} public Many(X x, Y y) {} }   // X and Y are registered
```

- A) the parameterless one
- B) `Many(X x)`
- C) `Many(X x, Y y)`
- D) it will throw an exception

<details><summary>Answer</summary>

**C.** The constructor with the most resolvable parameters is chosen (verified: `X,Y`). Ambiguity arises when two constructors fit and neither includes the other.
→ [Expert theory](expert.en.md#how-the-container-is-built)

</details>
