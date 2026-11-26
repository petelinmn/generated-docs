---
title: "Упражнения: кэширование"
lang: ru
track: system-design
kind: module
doc: exercises
slug: 03-caching
part: 1
tags: [system-design, caching, cache-aside, cache-invalidation, cache-stampede, lru-cache, hybrid-cache]
versions: ".NET 11, C# 15; Microsoft.Extensions.Caching.Hybrid 11.0.1"
status: verified
updated: 2026-11-26
---

# Упражнения: кэширование

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Решения запускаются из [code/Solutions/](code/Solutions/), тесты к ним — в [code/Tests/](code/Tests/).

## 1. 🟢 Нагрузка на источник

**Задача:** по общему QPS и доле попаданий вычислить, сколько запросов дойдёт до источника.

**Ожидаемый результат:**

```text
(10000, 0.99) -> 100
```

<details><summary>Подсказка</summary>

До источника доходят только промахи.

</details>

<details><summary>Решение</summary>

```csharp
public static double OriginQps(double totalQps, double hitRatio) => totalQps * (1 - hitRatio);
```

Файл: [code/Solutions/Exercises.cs](code/Solutions/Exercises.cs).

</details>

## 2. 🟡 Свежесть записи

**Задача:** по возрасту, TTL и окну устаревания вернуть `Fresh`, `Stale` или `Missing`.

**Ожидаемый результат:**

```text
ttl 10 s, window 5 s: 9 -> Fresh, 10 -> Stale, 15 -> Missing
```

<details><summary>Подсказка</summary>

Границы строгие: ровно на TTL запись уже не свежая.

</details>

<details><summary>Решение</summary>

```csharp
public static Freshness Classify(TimeSpan age, TimeSpan ttl, TimeSpan staleWindow) =>
    age < ttl ? Freshness.Fresh
    : age < ttl + staleWindow ? Freshness.Stale
    : Freshness.Missing;
```

`Stale` отдают сразу и запускают фоновое обновление.

</details>

## 3. 🔴 Кэш с версиями

**Задача:** запись с версией не новее уже сохранённой должна игнорироваться, в том числе при конкурентных вызовах.

**Ожидаемый результат:**

```text
Set(1, version 2, "new"); Set(1, version 1, "old"); Get(1) -> "new"
```

<details><summary>Подсказка</summary>

`ConcurrentDictionary.AddOrUpdate` пересчитывает значение, если оно изменилось под ногами.

</details>

<details><summary>Решение</summary>

```csharp
public void Set(TKey key, long version, TValue value) =>
    _entries.AddOrUpdate(
        key,
        (version, value),
        (_, current) => version > current.Version ? (version, value) : current);
```

Делегат обновления может выполниться несколько раз, поэтому у него нет побочных эффектов. После удаления ключа версия забывается: для защиты от гонки с удалением нужна запись-надгробие с версией.

</details>
