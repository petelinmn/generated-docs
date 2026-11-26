---
title: "Упражнения: репликация и партиционирование"
lang: ru
track: system-design
kind: module
doc: exercises
slug: 06-replication-and-partitioning
part: 2
tags: [system-design, replication, partitioning, shard, quorum, replication-lag, version-vector]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Упражнения: репликация и партиционирование

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Решения запускаются из [code/Solutions/](code/Solutions/), тесты к ним — в [code/Tests/](code/Tests/).

## 1. 🟢 Минимальный кворум чтения

**Задача:** по N и W вычислить наименьшее R с пересечением; недопустимое W отклонить.

**Ожидаемый результат:**

```text
(3, 2) -> 2; (5, 1) -> 5; (5, 5) -> 1
```

<details><summary>Подсказка</summary>

Условие `W + R > N` для целых чисел.

</details>

<details><summary>Решение</summary>

```csharp
public static int MinReadQuorum(int n, int w)
{
    if (w < 1 || w > n) throw new ArgumentOutOfRangeException(nameof(w));
    return n - w + 1;
}
```

Чем меньше W, тем дороже и менее доступно чтение. Файл: [code/Solutions/Exercises.cs](code/Solutions/Exercises.cs).

</details>

## 2. 🟡 Слияние version vectors

**Задача:** построить вектор, видевший всё, что видели оба.

**Ожидаемый результат:**

```text
{a:2, b:1} + {b:3, c:1} -> {a:2, b:3, c:1}
```

<details><summary>Подсказка</summary>

Максимум по каждому узлу.

</details>

<details><summary>Решение</summary>

```csharp
a.Keys.Union(b.Keys).ToDictionary(
    node => node,
    node => Math.Max(a.GetValueOrDefault(node), b.GetValueOrDefault(node)));
```

Слияние векторов не сливает значения: что делать с двумя версиями данных, решает приложение.

</details>

## 3. 🔴 Перекос партиций

**Задача:** отношение нагрузки самой занятой партиции к средней.

**Ожидаемый результат:**

```text
[0, 0, 0, 1000] -> 4
```

<details><summary>Подсказка</summary>

Что вернуть, если нагрузки нет?

</details>

<details><summary>Решение</summary>

```csharp
double total = load.Sum();
return total == 0 ? 1 : load.Max() / (total / load.Count);
```

Значение 1 означает идеальную равномерность, число партиций — что всё идёт в одну.

</details>
