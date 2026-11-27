---
title: "Distributed rate limiter: basics"
lang: en
track: system-design
kind: recipe
doc: basics
slug: distributed-rate-limiter
part: 2
tags: [rate-limiting, redis, token-bucket, lua, distributed-systems]
versions: ".NET 11, C# 15; StackExchange.Redis 3.4.35; Redis 8"
status: draft
updated: 2026-11-27
---

# Distributed rate limiter: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [09 · Reliability patterns](../../modules/09-reliability-patterns/basics.en.md) · **Time:** ~1 h

## The token bucket

A bucket holds up to `Capacity` tokens and refills at a constant rate. A request takes tokens; with too few it is refused.

```csharp
// Capacity bounds a burst; the refill rate bounds the long-term average.
public sealed record Limit(int Capacity, double RefillPerSecond);

public sealed record Decision(bool Allowed, int Remaining, TimeSpan RetryAfter);
```

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

The function takes the time as a parameter, so tests need no waiting. With capacity 5 and 1 token per second, five requests pass at once, the sixth is refused with `RetryAfter` of one second.

## In one process

```csharp
lock (_gate) // read and write must be one step
{
    Bucket? current = _buckets.TryGetValue(key, out var found) ? found : null;
    var (decision, next) = TokenBucket.Take(current, limit, clock.GetUtcNow(), cost);
    _buckets[key] = next;
    return ValueTask.FromResult(decision);
}
```

The lock matters: without it two requests could see the same last token. File: [Ex01_TokenBucket.cs](code/Examples/Ex01_TokenBucket.cs).

## Why this is not enough

Three instances with this limiter and a load balancer in front let through 15 requests instead of 5 (a test shows it). The state has to be shared: see [advanced](advanced.en.md).

## Algorithms

| Algorithm | Burst | Memory per key | Note |
|---|---|---|---|
| Fixed window | up to twice the limit around a boundary | one counter | simplest |
| Sliding window log | exact | a timestamp per request | expensive |
| Sliding window counter | approximate | two counters | see the exercises |
| Token bucket | up to capacity | two numbers | used here |

## Running

```bash
cd code
dotnet test --filter "FullyQualifiedName~TokenBucket"
```

## Beginner mistakes

- ❌ Reading and writing without synchronization → ✅ one atomic step.
- ❌ Using the real clock inside the algorithm → ✅ pass the time in.

## Related materials

- [Advanced](advanced.en.md)

## Sources

- [Redis: scripting with Lua](https://redis.io/docs/latest/develop/programmability/eval-intro/)
- [Redis: Lua API](https://redis.io/docs/latest/develop/programmability/lua-api/)
- [Redis: TIME](https://redis.io/docs/latest/commands/time/)
- [RFC 6585, status 429](https://www.rfc-editor.org/rfc/rfc6585#section-4)
- [ASP.NET Core rate limiting](https://learn.microsoft.com/aspnet/core/performance/rate-limit)
- [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis)
