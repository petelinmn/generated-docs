---
title: "Exercises: distributed rate limiter"
lang: en
track: system-design
kind: recipe
doc: exercises
slug: distributed-rate-limiter
part: 2
tags: [rate-limiting, redis, token-bucket, lua, distributed-systems]
versions: ".NET 11, C# 15; StackExchange.Redis 3.4.35; Redis 8"
status: draft
updated: 2026-11-27
---

# Exercises: distributed rate limiter

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. Solutions are in `code/Solutions/` and covered by tests.

## 1. 🟡 Sliding window counter

**Task:** limit requests per minute without the double burst at a window boundary.

<details><summary>Hint</summary>

Weigh the previous window by the part of it still inside the sliding window.

</details>

<details><summary>Solution</summary>

```csharp
public bool TryAcquire()
{
    long ticks = clock.GetUtcNow().UtcTicks;
    long index = ticks / window.Ticks;
    if (index != _index)
    {
        _previous = index == _index + 1 ? _current : 0;
        _current = 0;
        _index = index;
    }

    double passed = (double)(ticks % window.Ticks) / window.Ticks;
    if (_previous * (1 - passed) + _current + 1 > limit) return false;
    _current++;
    return true;
}
```

File: [Exercises.cs](code/Solutions/Exercises.cs).

</details>

## 2. 🟡 Several limits at once

**Task:** a refusal by any limit must consume from none.

<details><summary>Hint</summary>

Check all, then subtract, under one lock.

</details>

<details><summary>Solution</summary>

```csharp
public bool TryAcquire(int cost)
{
    lock (_gate)
    {
        if (_left.Any(left => left < cost)) return false;
        for (int i = 0; i < _left.Length; i++) _left[i] -= cost;
        return true;
    }
}
```

In Redis this is one script with keys sharing a hash tag.

</details>

## 3. 🟢 Split a hot limit

**Task:** split a limit of 100 into 8 shards without losing a request.

<details><summary>Hint</summary>

Distribute the remainder.

</details>

<details><summary>Solution</summary>

```csharp
public static int[] Split(int limit, int shards) =>
    [.. Enumerable.Range(0, shards).Select(i => limit / shards + (i < limit % shards ? 1 : 0))];
```

</details>
