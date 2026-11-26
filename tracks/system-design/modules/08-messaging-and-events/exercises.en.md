---
title: "Exercises: messaging and events"
lang: en
track: system-design
kind: module
doc: exercises
slug: 08-messaging-and-events
part: 2
tags: [system-design, message-broker, events, delivery-semantics, idempotency, backpressure, poison-message]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Exercises: messaging and events

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. The solutions run from [code/Solutions/](code/Solutions/), their tests are in [code/Tests/](code/Tests/).

## 1. 🟢 Partition by key

**Task:** one key always lands in one partition, in every process.

**Expected result:**

```text
the same key -> the same number from 0 to partitions - 1
```

<details><summary>Hint</summary>

`string.GetHashCode` is not suitable.

</details>

<details><summary>Solution</summary>

```csharp
uint hash = BinaryPrimitives.ReadUInt32BigEndian(SHA256.HashData(Encoding.UTF8.GetBytes(key)));
return (int)(hash % (uint)partitions);
```

Changing the partition count changes the placement. File: [code/Solutions/Exercises.cs](code/Solutions/Exercises.cs).

</details>

## 2. 🟡 Windowed deduplication

**Task:** remember only the last `window` identifiers.

**Expected result:**

```text
window 2: a -> new, a -> duplicate, b, c, a -> new again
```

<details><summary>Hint</summary>

A set for lookup and a queue for order.

</details>

<details><summary>Solution</summary>

```csharp
public bool IsNew(string id)
{
    if (!_seen.Add(id)) return false;

    _order.Enqueue(id);
    if (_order.Count > window) _seen.Remove(_order.Dequeue());
    return true;
}
```

Memory is bounded, but a late repeat gets through: the window is chosen from the possible delivery period. Not thread-safe.

</details>

## 3. 🔴 Restoring order

**Task:** release items strictly by number, holding back those that arrived early.

**Expected result:**

```text
add 2 -> nothing; add 1 -> a, b; add 1 -> nothing; add 3 -> c
```

<details><summary>Hint</summary>

Keep the next expected number.

</details>

<details><summary>Solution</summary>

```csharp
if (sequence >= _next) _pending[sequence] = item; // older ones are duplicates

while (_pending.Remove(_next, out T? value))
{
    ready.Add(value);
    _next++;
}
```

A lost number stops delivery forever: a buffer limit and a timeout are needed.

</details>
