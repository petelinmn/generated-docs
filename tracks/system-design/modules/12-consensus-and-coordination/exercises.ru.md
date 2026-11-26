---
title: "Упражнения: консенсус и координация"
lang: ru
track: system-design
kind: module
doc: exercises
slug: 12-consensus-and-coordination
part: 3
tags: [system-design, consensus, raft, leader-election, distributed-lock, fencing-token, logical-clocks]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Упражнения: консенсус и координация

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Решения запускаются из [code/Solutions/](code/Solutions/), тесты к ним — в [code/Tests/](code/Tests/).

## 1. 🟢 Сколько отказов

**Задача:** по размеру кластера вернуть число переживаемых отказов.

**Ожидаемый результат:**

```text
3 -> 1; 4 -> 1; 5 -> 2
```

<details><summary>Подсказка</summary>

Целочисленное деление.

</details>

<details><summary>Решение</summary>

```csharp
public static int ToleratedFailures(int clusterSize) => (clusterSize - 1) / 2;
```

Файл: [code/Solutions/Exercises.cs](code/Solutions/Exercises.cs).

</details>

## 2. 🟡 Случайный таймаут выборов

**Задача:** вернуть длительность из интервала `[min, max]`.

**Ожидаемый результат:**

```text
(150 ms, 300 ms) -> a value in that range
```

<details><summary>Подсказка</summary>

`TimeSpan` можно умножать на `double`.

</details>

<details><summary>Решение</summary>

```csharp
public static TimeSpan ElectionTimeout(TimeSpan min, TimeSpan max, Random random) =>
    min + (max - min) * random.NextDouble();
```

`Random` передаётся ради повторяемости тестов.

</details>

## 3. 🔴 Индекс фиксации

**Задача:** найти наибольший индекс, который лидер может объявить зафиксированным.

**Ожидаемый результат:**

```text
terms 1,2,4; current 4: replicated to 3 of 5 up to index 2 -> 0; up to index 3 -> 3
```

<details><summary>Подсказка</summary>

Два условия: большинство и терм записи равен текущему.

</details>

<details><summary>Решение</summary>

```csharp
for (int n = logTerms.Count; n > committed; n--)
{
    bool majority = matchIndex.Count(m => m >= n) > matchIndex.Count / 2;
    // Entries of earlier terms are never committed by counting replicas alone.
    if (majority && logTerms[n - 1] == currentTerm) return n;
}
return committed;
```

Запись прежнего терма на большинстве ещё может быть перезаписана.

</details>
