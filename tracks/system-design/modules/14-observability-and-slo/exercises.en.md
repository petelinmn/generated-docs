---
title: "Exercises: observability and SLOs"
lang: en
track: system-design
kind: module
doc: exercises
slug: 14-observability-and-slo
part: 3
tags: [system-design, observability, distributed-tracing, metrics, slo, histogram, sampling]
versions: ".NET 11, C# 15; Microsoft.Extensions.Telemetry 11.0.1"
status: draft
updated: 2026-11-26
---

# Exercises: observability and SLOs

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. The solutions run from [code/Solutions/](code/Solutions/), their tests are in [code/Tests/](code/Tests/).

## 1. 🟢 When to wake the on-call

**Task:** return `true` only if the burn rate is above the threshold in both windows.

**Expected result:**

```text
(20, 30) -> true; (20, 1) -> false
```

<details><summary>Hint</summary>

The default threshold is 14.4.

</details>

<details><summary>Solution</summary>

```csharp
public static bool ShouldPage(double longWindowBurn, double shortWindowBurn, double threshold = 14.4) =>
    longWindowBurn > threshold && shortWindowBurn > threshold;
```

File: [code/Solutions/Exercises.cs](code/Solutions/Exercises.cs).

</details>

## 2. 🟡 Parsing traceparent

**Task:** parse a version `00` header, rejecting invalid ones.

**Expected result:**

```text
valid header -> (trace id, parent id, sampled); zeros, upper case, "ff" -> null
```

<details><summary>Hint</summary>

Four parts, lowercase, not all zeros.

</details>

<details><summary>Solution</summary>

```csharp
string[] p = header.Split('-');
if (p.Length != 4 || p[0] != "00") return null;
if (!Valid(p[1], 32) || !Valid(p[2], 16) || p[3].Length != 2 || p[3].AsSpan().ContainsAnyExcept(Hex)) return null;
if (p[1].All(c => c == '0') || p[2].All(c => c == '0')) return null;
```

A simplification: the specification also requires parsing future versions by their first 55 characters. In an application the platform does this.

</details>

## 3. 🔴 Consistent sampling

**Task:** from a trace id and a ratio, make a decision that is the same in all services.

**Expected result:**

```text
the same id -> the same answer; about 10% of random ids at ratio 0.1
```

<details><summary>Hint</summary>

Use part of the identifier itself as a number.

</details>

<details><summary>Solution</summary>

```csharp
ulong value = ulong.Parse(traceId.AsSpan(traceId.Length - 16), NumberStyles.HexNumber);
return value / (double)ulong.MaxValue < ratio;
```

It works with uniformly random identifiers.

</details>
