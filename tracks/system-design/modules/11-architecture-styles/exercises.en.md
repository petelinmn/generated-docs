---
title: "Exercises: architecture styles"
lang: en
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

# Exercises: architecture styles

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. The solutions run from [code/Solutions/](code/Solutions/), their tests are in [code/Tests/](code/Tests/).

## 1. 🟢 Module instability

**Task:** compute `Ce / (Ca + Ce)`.

**Expected result:**

```text
(4, 1) -> 0.2; (0, 3) -> 1; (0, 0) -> 0
```

<details><summary>Hint</summary>

Division by zero and integer division.

</details>

<details><summary>Solution</summary>

```csharp
public static double Instability(int incoming, int outgoing) =>
    incoming + outgoing == 0 ? 0 : (double)outgoing / (incoming + outgoing);
```

File: [code/Solutions/Exercises.cs](code/Solutions/Exercises.cs).

</details>

## 2. 🟡 Finding cycles

**Task:** determine whether module dependencies contain a cycle.

**Expected result:**

```text
a->b,c; b->c -> false; a->b->c->a -> true
```

<details><summary>Hint</summary>

Depth-first search with two sets.

</details>

<details><summary>Solution</summary>

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

Recursion suits a module graph; huge graphs need an explicit stack.

</details>

## 3. 🔴 A decision log

**Task:** a new record may supersede an old one; you cannot supersede twice, and old text does not change.

**Expected result:**

```text
add 1; add 2 supersedes 1 -> active: 2; superseding 1 again -> error
```

<details><summary>Hint</summary>

A `record` and a `with` expression.

</details>

<details><summary>Solution</summary>

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

The history stays readable: you can see what was replaced by what.

</details>
