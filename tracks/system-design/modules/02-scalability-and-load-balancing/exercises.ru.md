---
title: "Упражнения: масштабирование и балансировка"
lang: ru
track: system-design
kind: module
doc: exercises
slug: 02-scalability-and-load-balancing
part: 1
tags: [system-design, scaling, scale-out, load-balancing, consistent-hashing, yarp]
versions: ".NET 11, C# 15; Yarp.ReverseProxy 2.3.0"
status: draft
updated: 2026-11-26
---

# Упражнения: масштабирование и балансировка

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Решения запускаются из [code/Solutions/](code/Solutions/), тесты к ним — в [code/Tests/](code/Tests/).

## 1. 🟢 Round robin с учётом здоровья

**Задача:** выбирать по кругу только здоровые узлы; если таких нет, вернуть `null`.

**Ожидаемый результат:**

```text
nodes a b c, b is down -> a c a c
```

<details><summary>Подсказка</summary>

Что произойдёт с соседом мёртвого узла, если просто пропускать его по кругу?

</details>

<details><summary>Решение</summary>

```csharp
public string? Pick()
{
    // Filter first, then rotate: otherwise the neighbour of a dead node gets double load.
    string[] healthy = [.. nodes.Where(isHealthy)];
    if (healthy.Length == 0) return null;
    uint i = unchecked((uint)Interlocked.Increment(ref _next));
    return healthy[(int)(i % (uint)healthy.Length)];
}
```

Это учебный вариант: массив создаётся на каждый выбор. В production список здоровых обновляют при смене состояния. Файл: [code/Solutions/Exercises.cs](code/Solutions/Exercises.cs).

</details>

## 2. 🟡 Доля переехавших ключей

**Задача:** по списку ключей и двум функциям размещения («до» и «после») вернуть долю ключей, сменивших узел.

**Ожидаемый результат:**

```text
keys a b; before: x x; after: x y -> 0.5
```

<details><summary>Подсказка</summary>

`Count` с предикатом и деление в `double`.

</details>

<details><summary>Решение</summary>

```csharp
public static double MovedFraction(IReadOnlyList<string> keys, Func<string, string> before, Func<string, string> after) =>
    (double)keys.Count(k => before(k) != after(k)) / keys.Count;
```

Подставьте `HashRing.NodeFor` и `ModuloSharding.ShardFor`, чтобы сравнить подходы.

</details>

## 3. 🔴 Оптимальное число узлов

**Задача:** найти `n` от 1 до `max` с наибольшей пропускной способностью по USL.

**Ожидаемый результат:**

```text
(contention 0.05, coherency 0.001, max 200) -> 31
```

<details><summary>Подсказка</summary>

`Enumerable.Range` и `MaxBy`.

</details>

<details><summary>Решение</summary>

```csharp
public static int BestNodeCount(double contention, double coherency, int max) =>
    Enumerable.Range(1, max).MaxBy(n => ScalingLaws.Universal(contention, coherency, n));
```

После пика каждый новый узел уменьшает общую производительность.

</details>
