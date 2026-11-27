---
title: "Distributed rate limiter"
lang: en
track: system-design
kind: recipe
doc: overview
slug: distributed-rate-limiter
part: 2
levels: [1, 2, 3, 4, 5, 6]
tags: [rate-limiting, redis, token-bucket, lua, distributed-systems]
versions: ".NET 11, C# 15; StackExchange.Redis 3.4.35; Redis 8"
status: draft
updated: 2026-11-27
---

# Distributed rate limiter

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

When a service runs as several instances, a limit kept in each of them multiplies: three instances allow three times as much. A distributed rate limiter keeps the state in a shared store. This recipe builds one with a token bucket in Redis and decides what happens when Redis is unavailable.

## Key ideas

- A token bucket has a burst size and a long-term rate.
- Read, refill and write must be one atomic step; a Lua script does that.
- Use the store's clock, not the instances' clocks.
- Refuse with 429 and `Retry-After`.
- Decide in advance: fail open, fail closed or fall back to a local limit.
- Taking tokens in batches saves round trips and costs accuracy.

## Minimal example

```csharp
public static (Decision Decision, Bucket Next) Take(Bucket? current, Limit limit, DateTimeOffset now, int cost)
{
    // A negative cost would add tokens.
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(cost);
    var bucket = current ?? new Bucket(limit.Capacity, now);
    // Time that goes backwards must not remove or add tokens.
    double elapsed = Math.Max(0, (now - bucket.UpdatedAt).TotalSeconds);
    double tokens = Math.Min(limit.Capacity, bucket.Tokens + elapsed * limit.RefillPerSecond);

    bool allowed = tokens >= cost;
    if (allowed) tokens -= cost;

    TimeSpan retryAfter = allowed
        ? TimeSpan.Zero
        : TimeSpan.FromSeconds((cost - tokens) / limit.RefillPerSecond);
    return (new Decision(allowed, (int)tokens, retryAfter), new Bucket(tokens, now));
}
```

## When to use and when not

- ✅ A limit that must hold for the whole service: quotas, protection of a shared dependency, paid plans.
- ❌ Protecting one process from overload: a local limiter is simpler and faster.
- ❌ Exact billing: use a durable ledger.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | the algorithm and a limiter in one process |
| [2. Advanced](advanced.en.md) | Redis, middleware, behaviour on failure |
| [3. Expert](expert.en.md) | batching, hot keys, cluster, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) |
| [5. Runnable examples](code/Examples/Ex02_Redis.cs) | the `code/` folder |
| [6. Tests](code/Tests/ExamplesTests.cs) | `dotnet test` in `code/` |

## Related materials

- [09 · Reliability patterns](../../modules/09-reliability-patterns/overview.en.md), [02 · Scalability](../../modules/02-scalability-and-load-balancing/overview.en.md)
- In one process: [rate limiting in ASP.NET Core](../../../csharp/recipes/rate-limiting/overview.en.md)
- Used by: [URL shortener](../url-shortener/overview.en.md)
