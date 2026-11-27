---
title: "URL shortener: basics"
lang: en
track: system-design
kind: recipe
doc: basics
slug: url-shortener
part: 2
tags: [url-shortener, caching, redirect, rate-limiting, minimal-api]
versions: ".NET 11, C# 15; Microsoft.Extensions.Caching.Hybrid 11.0.1; Microsoft.Data.Sqlite 11.0.0"
status: draft
updated: 2026-11-27
---

# URL shortener: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** minimal APIs, [17 · Design case studies](../../modules/17-design-case-studies/basics.en.md) · **Time:** ~1 h

## What we build

| Request | Answer |
|---|---|
| `POST /links` with `{ "url": "…" }` | `201 Created` and the code |
| `GET /{code}` | `302 Found` with `Location`, or `404` |

## The code

```csharp
public static string Random(int length = 7) => RandomNumberGenerator.GetString(Alphabet, length);
```

Seven base62 characters give about 3.5 trillion variants. Random codes do not reveal how many links exist and cannot be walked in order. A secret link still needs access control: a short code is not a password.

## The service

```csharp
app.MapPost("/links", (CreateLink request) =>
{
    string code = ShortCode.Random();
    while (!links.TryAdd(code, request.Url))
        code = ShortCode.Random();
    return Results.Created($"/{code}", new { code });
});

app.MapGet("/{code}", (string code) =>
    links.TryGetValue(code, out string? url) ? Results.Redirect(url) : Results.NotFound());
```

`TryAdd` stores the pair only if the code is free; otherwise a new code is taken. `Results.Redirect` answers 302 by default.

File: [Ex01_Minimal.cs](code/Examples/Ex01_Minimal.cs).

## What is missing

Links disappear on restart, any string is accepted, and nothing limits creation. The [advanced level](advanced.en.md) fixes this.

## Running

```bash
cd code
dotnet test --filter "FullyQualifiedName~Minimal"
```

## Beginner mistakes

- ❌ Sequential codes for private links → ✅ random codes and access checks.
- ❌ "Check, then add" → ✅ one atomic operation.
- ❌ 301 everywhere → ✅ 302 while clicks matter and targets may change.

## Related materials

- [Advanced](advanced.en.md)

## Sources

- [HybridCache](https://learn.microsoft.com/aspnet/core/performance/caching/hybrid)
- [Rate limiting middleware](https://learn.microsoft.com/aspnet/core/performance/rate-limit)
- [RFC 9110, redirection](https://www.rfc-editor.org/rfc/rfc9110#name-redirection-3xx)
- [Minimal APIs](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis)
- [Channels](https://learn.microsoft.com/dotnet/core/extensions/channels)
- [Microsoft.Data.Sqlite errors](https://learn.microsoft.com/dotnet/standard/data/sqlite/database-errors)
