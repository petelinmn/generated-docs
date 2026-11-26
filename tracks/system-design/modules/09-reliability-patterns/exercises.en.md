---
title: "Exercises: reliability patterns"
lang: en
track: system-design
kind: module
doc: exercises
slug: 09-reliability-patterns
part: 2
tags: [system-design, resilience, retry, circuit-breaker, bulkhead, rate-limiting, load-shedding]
versions: ".NET 11, C# 15; Polly.Core 8.9.1; System.Threading.RateLimiting 11.0.0"
status: draft
updated: 2026-11-26
---

# Exercises: reliability patterns

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. The solutions run from [code/Solutions/](code/Solutions/), their tests are in [code/Tests/](code/Tests/).

## 1. 🟢 Backoff with full jitter

**Task:** a random delay from zero to an exponential ceiling with an upper bound.

**Expected result:**

```text
base 100 ms, max 1 s, retry 3 -> between 0 and 800 ms
```

<details><summary>Hint</summary>

The ceiling first, then `random.NextDouble()`.

</details>

<details><summary>Solution</summary>

```csharp
double cap = Math.Min(maxDelay.TotalMilliseconds, baseDelay.TotalMilliseconds * Math.Pow(2, retry));
return TimeSpan.FromMilliseconds(random.NextDouble() * cap);
```

`Random` is a parameter so that tests are repeatable. File: [code/Solutions/Exercises.cs](code/Solutions/Exercises.cs).

</details>

## 2. 🟡 Remaining deadline

**Task:** return the time left, or `null` if starting the call is pointless.

**Expected result:**

```text
2 s left, minimum 1 s -> 2 s; 0.5 s left -> null
```

<details><summary>Hint</summary>

Subtracting `DateTimeOffset` values gives a `TimeSpan`.

</details>

<details><summary>Solution</summary>

```csharp
TimeSpan left = deadline - now;
return left >= minimumUseful ? left : null;
```

Between machines pass the remaining duration, not an absolute time: clocks drift.

</details>

## 3. 🔴 Retry budget

**Task:** allow retries only as a share of ordinary requests.

**Expected result:**

```text
ratio 0.1: after 10 requests one retry is allowed, the second is not
```

<details><summary>Hint</summary>

Accumulating fractional "tokens" and `double` rounding error.

</details>

<details><summary>Solution</summary>

```csharp
public bool TryRetry()
{
    lock (_gate)
    {
        if (_tokens < 1 - 1e-9) return false; // tolerance: 0.1 added ten times is below 1
        _tokens -= 1;
        return true;
    }
}
```

In a mass failure the budget runs out quickly and the load barely grows. With integers (for example in tenths) there would be no rounding error.

</details>
