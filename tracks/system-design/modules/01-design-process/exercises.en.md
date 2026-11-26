---
title: "Exercises: design process and estimation"
lang: en
track: system-design
kind: module
doc: exercises
slug: 01-design-process
part: 1
tags: [system-design, capacity-estimation, slo, availability, latency, percentiles]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Exercises: design process and estimation

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. The solutions run from [code/Solutions/](code/Solutions/), their tests are in [code/Tests/](code/Tests/).

## 1. 🟢 How many servers

**Task:** from the peak QPS, the throughput of a server, the target utilization and the number of spare servers, compute the fleet size.

**Expected result:**

```text
7000 QPS, 1000 QPS/server, utilization 0.6, 2 spares -> 14
```

<details><summary>Hint</summary>

A server's useful capacity is not its whole throughput. A fraction of a server rounds up.

</details>

<details><summary>Solution</summary>

```csharp
public static int ServersNeeded(double peakQps, double qpsPerServer, double targetUtilization, int spares) =>
    (int)Math.Ceiling(peakQps / (qpsPerServer * targetUtilization)) + spares;
```

Spares are added after rounding, so that a failure or a rollout does not overload the rest. File: [code/Solutions/Exercises.cs](code/Solutions/Exercises.cs).

</details>

## 2. 🟡 How many replicas

**Task:** find the minimum number of independent replicas with availability `single` that reaches `target`. Reject unreachable values.

**Expected result:**

```text
(0.99, 0.999) -> 2
(0.9, 0.9995) -> 4
```

<details><summary>Hint</summary>

Increase `n` while `1 - (1 - single)^n` is below the target. Remember `double` rounding error.

</details>

<details><summary>Solution</summary>

```csharp
public static int ReplicasFor(double single, double target)
{
    if (single is <= 0 or >= 1) throw new ArgumentOutOfRangeException(nameof(single));
    if (target is < 0 or >= 1) throw new ArgumentOutOfRangeException(nameof(target));

    int n = 1;
    // The tolerance absorbs floating-point error when the target is hit exactly.
    while (1 - Math.Pow(1 - single, n) < target - 1e-12) n++;
    return n;
}
```

A 100% target is unreachable with a finite number of replicas, so it is an argument error.

</details>

## 3. 🔴 Burn rate

**Task:** compute the burn rate from the SLO, the total number of requests and the number of failures.

**Expected result:**

```text
(0.999, 10000, 100) -> 10
```

<details><summary>Hint</summary>

Divide the observed error ratio by the allowed one. Think about zero requests.

</details>

<details><summary>Solution</summary>

```csharp
public static double BurnRate(double slo, long total, long failed) =>
    total == 0 ? 0 : (double)failed / total / (1 - slo);
```

The cast to `double` is mandatory: integer division would give zero.

</details>
