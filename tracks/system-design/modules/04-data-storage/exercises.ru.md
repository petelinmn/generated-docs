---
title: "Упражнения: хранение данных"
lang: ru
track: system-design
kind: module
doc: exercises
slug: 04-data-storage
part: 1
tags: [system-design, data-modeling, database-index, b-tree, lsm-tree, query-plan, nosql]
versions: ".NET 11, C# 15; Microsoft.Data.Sqlite 11.0.0"
status: verified
updated: 2026-11-26
---

# Упражнения: хранение данных

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Решения запускаются из [code/Solutions/](code/Solutions/), тесты к ним — в [code/Tests/](code/Tests/).

## 1. 🟢 Ключ под паттерн доступа

**Задача:** построить строковый ключ, обычный порядок которого — «по чату, внутри чата от новых к старым».

**Ожидаемый результат:**

```text
chat 1 at 200, chat 1 at 100, chat 2 at 100, chat 10 at 100
```

<details><summary>Подсказка</summary>

Дополните числа нулями до фиксированной ширины. Чтобы новые шли первыми, вычтите время из максимума.

</details>

<details><summary>Решение</summary>

```csharp
public static string MessageKey(long chatId, long sentAtUnixMs) =>
    string.Create(CultureInfo.InvariantCulture, $"{chatId:D19}#{long.MaxValue - sentAtUnixMs:D19}");
```

Сравнивать нужно ординально; решение рассчитано на неотрицательные числа. Файл: [code/Solutions/Exercises.cs](code/Solutions/Exercises.cs).

</details>

## 2. 🟡 Слияние сегментов

**Задача:** объединить старый и новый сегменты; новое значение побеждает, надгробия по флагу сохраняются или выбрасываются.

**Ожидаемый результат:**

```text
older a=1 b=2, newer b=null c=3 -> keep: a b c; drop: a c
```

<details><summary>Подсказка</summary>

Когда удалять tombstone опасно?

</details>

<details><summary>Решение</summary>

```csharp
var result = new SortedDictionary<string, string?>(StringComparer.Ordinal);
foreach (var (key, value) in older) result[key] = value;
foreach (var (key, value) in newer) result[key] = value;

if (dropTombstones)
    foreach (string key in result.Where(e => e.Value is null).Select(e => e.Key).ToList())
        result.Remove(key);

return result;
```

Выбрасывать надгробие можно, только если не осталось более старых сегментов, иначе удалённое значение «воскреснет».

</details>

## 3. 🔴 Усиление записи

**Задача:** по объёму пользовательских данных и всем записям на диск вычислить усиление записи.

**Ожидаемый результат:**

```text
100 user bytes; writes 100, 100, 200 -> 4
```

<details><summary>Подсказка</summary>

Сложите все записи: журнал, сброс памяти, compaction.

</details>

<details><summary>Решение</summary>

```csharp
public static double WriteAmplification(long userBytes, params long[] diskWrites) =>
    (double)diskWrites.Sum() / userBytes;
```

Чем агрессивнее compaction, тем меньше усиление чтения и пространства, но больше записи.

</details>
