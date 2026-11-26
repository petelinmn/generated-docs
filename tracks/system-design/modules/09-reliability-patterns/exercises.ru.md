---
title: "Упражнения: паттерны надёжности"
lang: ru
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

# Упражнения: паттерны надёжности

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Решения запускаются из [code/Solutions/](code/Solutions/), тесты к ним — в [code/Tests/](code/Tests/).

## 1. 🟢 Backoff с full jitter

**Задача:** случайная задержка от нуля до экспоненциального потолка с ограничением сверху.

**Ожидаемый результат:**

```text
base 100 ms, max 1 s, retry 3 -> between 0 and 800 ms
```

<details><summary>Подсказка</summary>

Сначала потолок, затем `random.NextDouble()`.

</details>

<details><summary>Решение</summary>

```csharp
double cap = Math.Min(maxDelay.TotalMilliseconds, baseDelay.TotalMilliseconds * Math.Pow(2, retry));
return TimeSpan.FromMilliseconds(random.NextDouble() * cap);
```

`Random` передаётся параметром ради повторяемых тестов. Файл: [code/Solutions/Exercises.cs](code/Solutions/Exercises.cs).

</details>

## 2. 🟡 Остаток дедлайна

**Задача:** вернуть оставшееся время или `null`, если начинать вызов бессмысленно.

**Ожидаемый результат:**

```text
2 s left, minimum 1 s -> 2 s; 0.5 s left -> null
```

<details><summary>Подсказка</summary>

`DateTimeOffset` вычитаются в `TimeSpan`.

</details>

<details><summary>Решение</summary>

```csharp
TimeSpan left = deadline - now;
return left >= minimumUseful ? left : null;
```

Между машинами передают оставшуюся длительность, а не абсолютное время: часы расходятся.

</details>

## 3. 🔴 Retry budget

**Задача:** разрешать повторы только как долю обычных запросов.

**Ожидаемый результат:**

```text
ratio 0.1: after 10 requests one retry is allowed, the second is not
```

<details><summary>Подсказка</summary>

Накопление дробных «жетонов» и погрешность `double`.

</details>

<details><summary>Решение</summary>

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

При массовом отказе бюджет быстро кончается, и нагрузка почти не растёт. В целых числах (например, в десятых долях) погрешности не было бы.

</details>
