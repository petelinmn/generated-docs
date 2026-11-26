---
title: "Упражнения: несколько регионов и восстановление"
lang: ru
track: system-design
kind: module
doc: exercises
slug: 16-multi-region-and-dr
part: 3
tags: [system-design, disaster-recovery, multi-region, failover, rpo-rto, backups, crdt]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Упражнения: несколько регионов и восстановление

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Решения запускаются из [code/Solutions/](code/Solutions/), тесты к ним — в [code/Tests/](code/Tests/).

## 1. 🟢 Выбор региона

**Задача:** вернуть ближайший здоровый регион нужной юрисдикции.

**Ожидаемый результат:**

```text
US 10 ms, EU down 80 ms, EU 120 ms; jurisdiction EU -> the 120 ms region
```

<details><summary>Подсказка</summary>

Сначала отфильтровать, потом `MinBy`.

</details>

<details><summary>Решение</summary>

```csharp
public static Region? Route(IEnumerable<Region> regions, string jurisdiction) =>
    regions.Where(r => r.Healthy && r.Jurisdiction == jurisdiction).MinBy(r => r.LatencyMs);
```

Отказ лучше, чем нарушение требований о месте хранения. Файл: [code/Solutions/Exercises.cs](code/Solutions/Exercises.cs).

</details>

## 2. 🟡 Самая дешёвая стратегия

**Задача:** по целевому RTO и измеренным значениям выбрать самую дешёвую подходящую стратегию.

**Ожидаемый результат:**

```text
RTO 30 min with the test numbers -> PilotLight
```

<details><summary>Подсказка</summary>

Порядок значений enum задаёт цену. Что вернуть, если не подходит ни одна?

</details>

<details><summary>Решение</summary>

```csharp
Enum.GetValues<Strategy>() // declared from the cheapest to the most expensive
    .Where(s => achievable.TryGetValue(s, out TimeSpan t) && t <= rto)
    .Cast<Strategy?>()
    .FirstOrDefault();
```

`Cast<Strategy?>` нужен, чтобы «не нашлось» было `null`, а не первой стратегией.

</details>

## 3. 🔴 Хранение копий

**Задача:** оставить последние `daily` копий и самую свежую копию каждого из последних `monthly` месяцев.

**Ожидаемый результат:**

```text
Jan 30, Jan 31, Feb 27, Feb 28, Mar 1 with daily 2, monthly 3 -> Jan 31, Feb 28, Mar 1
```

<details><summary>Подсказка</summary>

После сортировки по убыванию первый элемент каждой группы — самый свежий.

</details>

<details><summary>Решение</summary>

```csharp
DateOnly[] newestFirst = [.. backups.OrderDescending()];
var keep = new HashSet<DateOnly>(newestFirst.Take(daily));
keep.UnionWith(newestFirst.GroupBy(d => (d.Year, d.Month)).Take(monthly).Select(g => g.First()));
```

Удаление копий необратимо: сначала выводят список, потом удаляют.

</details>
