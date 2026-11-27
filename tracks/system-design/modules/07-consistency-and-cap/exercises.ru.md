---
title: "Упражнения: согласованность и CAP"
lang: ru
track: system-design
kind: module
doc: exercises
slug: 07-consistency-and-cap
part: 2
tags: [system-design, consistency, cap-theorem, isolation-levels, mvcc, write-skew, crdt]
versions: ".NET 11, C# 15"
status: verified
updated: 2026-11-27
---

# Упражнения: согласованность и CAP

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Решения запускаются из [code/Solutions/](code/Solutions/), тесты к ним — в [code/Tests/](code/Tests/).

## 1. 🟢 Есть ли большинство

**Задача:** может ли группа из `reachable` узлов продолжать работу в кластере из `n`.

**Ожидаемый результат:**

```text
(2, 3) -> true; (2, 4) -> false; (3, 5) -> true
```

<details><summary>Подсказка</summary>

Половины недостаточно.

</details>

<details><summary>Решение</summary>

```csharp
public static bool HasQuorum(int reachable, int n) => reachable > n / 2;
```

Поэтому чётное число узлов не добавляет устойчивости. Файл: [code/Solutions/Exercises.cs](code/Solutions/Exercises.cs).

</details>

## 2. 🟡 Повтор транзакции

**Задача:** повторять попытку до успеха и вернуть номер удачной; после лимита бросить исключение.

**Ожидаемый результат:**

```text
succeeds on the third call -> 3; never succeeds -> InvalidOperationException
```

<details><summary>Подсказка</summary>

Лимит обязателен.

</details>

<details><summary>Решение</summary>

```csharp
for (int i = 1; i <= maxAttempts; i++)
    if (attempt()) return i;
throw new InvalidOperationException($"No success after {maxAttempts} attempts.");
```

В реальном коде добавьте задержку со случайной составляющей.

</details>

## 3. 🔴 G-Counter

**Задача:** счётчик, который растёт на разных узлах и сходится при любом порядке слияний.

**Ожидаемый результат:**

```text
a: +1, b: +2; merge twice in any order -> 3 on both
```

<details><summary>Подсказка</summary>

Храните вклад каждого узла отдельно.

</details>

<details><summary>Решение</summary>

```csharp
public void Merge(GCounter other)
{
    // Maximum, not sum: merging the same state twice must change nothing.
    foreach (var (key, count) in other._counts)
        _counts[key] = Math.Max(_counts.GetValueOrDefault(key), count);
}
```

Уменьшать такой счётчик нельзя; для этого нужен второй.

</details>
