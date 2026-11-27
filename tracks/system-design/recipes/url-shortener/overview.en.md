---
title: "URL shortener"
lang: en
track: system-design
kind: recipe
doc: overview
slug: url-shortener
part: 2
levels: [1, 2, 3, 4, 5, 6]
tags: [url-shortener, caching, redirect, rate-limiting, minimal-api]
versions: ".NET 11, C# 15; Microsoft.Extensions.Caching.Hybrid 11.0.1; Microsoft.Data.Sqlite 11.0.0"
status: draft
updated: 2026-11-27
---

# URL shortener

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

A URL shortener stores a pair "short code → long address" and redirects. The recipe builds one on ASP.NET Core in steps: a version in memory, then validation, a database, a cache, click counting and a limit on creation. The design reasoning and capacity estimates are in [17 · Design case studies](../../modules/17-design-case-studies/overview.en.md); this recipe is about the implementation.

## Key ideas

- Random codes, with the database primary key resolving collisions.
- Accept only `http` and `https`, without credentials and not pointing back.
- Reads dominate, so cache them, including "not found".
- Check expiry on every request, even for a cached link.
- Count clicks off the request path.
- With several instances, local caches and rate limits are per instance.

## Minimal example

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

## When to use and when not

- ✅ You need your own domain, rules, analytics or private links.
- ❌ A few links for a presentation: an existing service is enough.
- ❌ Without moderation for the public: shorteners attract phishing.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | the minimal working service |
| [2. Advanced](advanced.en.md) | production-ready: validation, storage, cache, clicks, limits |
| [3. Expert](expert.en.md) | several instances, scale, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) |
| [5. Runnable examples](code/Examples/Ex06_Service.cs) | the `code/` folder |
| [6. Tests](code/Tests/ExamplesTests.cs) | `dotnet test` in `code/` |

## Related materials

- Design: [17 · Design case studies](../../modules/17-design-case-studies/overview.en.md)
- [03 · Caching](../../modules/03-caching/overview.en.md), [09 · Reliability patterns](../../modules/09-reliability-patterns/overview.en.md)
- In C#: [caching](../../../csharp/recipes/caching/overview.en.md), [rate limiting](../../../csharp/recipes/rate-limiting/overview.en.md), [background jobs](../../../csharp/recipes/background-jobs/overview.en.md)
