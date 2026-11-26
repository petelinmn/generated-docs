---
title: "Упражнения: архитектурные стили"
lang: ru
track: system-design
kind: module
doc: exercises
slug: 11-architecture-styles
part: 2
tags: [system-design, architecture, modular-monolith, microservices, fitness-function, adr]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Упражнения: архитектурные стили

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Решения запускаются из [code/Solutions/](code/Solutions/), тесты к ним — в [code/Tests/](code/Tests/).

## 1. 🟢 Неустойчивость модуля

**Задача:** вычислить `Ce / (Ca + Ce)`.

**Ожидаемый результат:**

```text
(4, 1) -> 0.2; (0, 3) -> 1; (0, 0) -> 0
```

<details><summary>Подсказка</summary>

Деление на ноль и целочисленное деление.

</details>

<details><summary>Решение</summary>

```csharp
public static double Instability(int incoming, int outgoing) =>
    incoming + outgoing == 0 ? 0 : (double)outgoing / (incoming + outgoing);
```

Файл: [code/Solutions/Exercises.cs](code/Solutions/Exercises.cs).

</details>

## 2. 🟡 Поиск циклов

**Задача:** определить, есть ли цикл в зависимостях модулей.

**Ожидаемый результат:**

```text
a->b,c; b->c -> false; a->b->c->a -> true
```

<details><summary>Подсказка</summary>

Обход в глубину с двумя множествами.

</details>

<details><summary>Решение</summary>

```csharp
bool Visit(string module)
{
    if (done.Contains(module)) return false;
    if (!visiting.Add(module)) return true; // back edge

    foreach (string next in dependencies.GetValueOrDefault(module, []))
        if (Visit(next)) return true;

    visiting.Remove(module);
    done.Add(module);
    return false;
}
```

Рекурсия подходит для графа модулей; для огромных графов нужен явный стек.

</details>

## 3. 🔴 Журнал решений

**Задача:** новая запись может отменить старую; отменить дважды нельзя, текст старых не меняется.

**Ожидаемый результат:**

```text
add 1; add 2 supersedes 1 -> active: 2; superseding 1 again -> error
```

<details><summary>Подсказка</summary>

`record` и выражение `with`.

</details>

<details><summary>Решение</summary>

```csharp
var record = new Adr(_records.Count + 1, title);
if (supersedes is int old)
{
    if (_records[old - 1].SupersededBy is not null)
        throw new InvalidOperationException($"ADR {old} is already superseded.");
    _records[old - 1] = _records[old - 1] with { SupersededBy = record.Number };
}
_records.Add(record);
```

История остаётся читаемой: видно, что и чем заменено.

</details>
