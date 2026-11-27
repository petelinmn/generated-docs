---
title: "Упражнения: распределённый rate limiter"
lang: ru
track: system-design
kind: recipe
doc: exercises
slug: distributed-rate-limiter
part: 2
tags: [rate-limiting, redis, token-bucket, lua, distributed-systems]
versions: ".NET 11, C# 15; StackExchange.Redis 3.4.35; Redis 8"
status: draft
updated: 2026-11-27
---

# Упражнения: распределённый rate limiter

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Решения лежат в `code/Solutions/` и покрыты тестами.

## 1. 🟡 Счётчик скользящего окна

**Задача:** ограничить запросы в минуту без двойного всплеска на границе окна.

<details><summary>Подсказка</summary>

Учтите предыдущее окно с весом той части, что ещё попадает в скользящее.

</details>

<details><summary>Решение</summary>

```csharp
public bool TryAcquire()
{
    long ticks = clock.GetUtcNow().UtcTicks;
    long index = ticks / window.Ticks;
    if (index != _index)
    {
        _previous = index == _index + 1 ? _current : 0;
        _current = 0;
        _index = index;
    }

    double passed = (double)(ticks % window.Ticks) / window.Ticks;
    if (_previous * (1 - passed) + _current + 1 > limit) return false;
    _current++;
    return true;
}
```

Файл: [Exercises.cs](code/Solutions/Exercises.cs).

</details>

## 2. 🟡 Несколько лимитов сразу

**Задача:** отказ по любому лимиту не должен списывать ни из одного.

<details><summary>Подсказка</summary>

Сначала проверить все, затем вычитать, под одной блокировкой.

</details>

<details><summary>Решение</summary>

```csharp
public bool TryAcquire(int cost)
{
    lock (_gate)
    {
        if (_left.Any(left => left < cost)) return false;
        for (int i = 0; i < _left.Length; i++) _left[i] -= cost;
        return true;
    }
}
```

В Redis это один скрипт с ключами с общим хеш-тегом.

</details>

## 3. 🟢 Разделить горячий лимит

**Задача:** разделить лимит 100 на 8 шардов, не потеряв ни одного запроса.

<details><summary>Подсказка</summary>

Раздайте остаток.

</details>

<details><summary>Решение</summary>

```csharp
public static int[] Split(int limit, int shards) =>
    [.. Enumerable.Range(0, shards).Select(i => limit / shards + (i < limit % shards ? 1 : 0))];
```

</details>
