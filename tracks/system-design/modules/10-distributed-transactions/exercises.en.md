---
title: "Exercises: distributed transactions"
lang: en
track: system-design
kind: module
doc: exercises
slug: 10-distributed-transactions
part: 2
tags: [system-design, distributed-transactions, two-phase-commit, saga, outbox, inbox, idempotency]
versions: ".NET 11, C# 15; Microsoft.Data.Sqlite 11.0.0"
status: draft
updated: 2026-11-26
---

# Exercises: distributed transactions

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. The solutions run from [code/Solutions/](code/Solutions/), their tests are in [code/Tests/](code/Tests/).

## 1. 🟢 The coordinator's decision

**Task:** from the participants' votes return `Commit` or `Abort`.

**Expected result:**

```text
Yes Yes -> Commit; Yes Unknown -> Abort; no votes -> Abort
```

<details><summary>Hint</summary>

`All` returns `true` for an empty collection.

</details>

<details><summary>Solution</summary>

```csharp
public static Decision Decide(IReadOnlyCollection<Vote> votes) =>
    votes.Count > 0 && votes.All(v => v == Vote.Yes) ? Decision.Commit : Decision.Abort;
```

File: [code/Solutions/Exercises.cs](code/Solutions/Exercises.cs).

</details>

## 2. 🟡 Retrying a compensation

**Task:** repeat a compensation until success, at most a given number of times; return the attempt number.

**Expected result:**

```text
fails twice, then works -> 3; limit 2 -> the exception propagates
```

<details><summary>Hint</summary>

A `when` exception filter.

</details>

<details><summary>Solution</summary>

```csharp
for (int attempt = 1; ; attempt++)
{
    try
    {
        await compensate();
        return attempt;
    }
    catch (Exception) when (attempt < maxAttempts)
    {
        // try again; the compensation has to be idempotent
    }
}
```

In production a delay is needed between attempts.

</details>

## 3. 🔴 A semantic lock

**Task:** allow transitions only out of `Pending`.

**Expected result:**

```text
Pending -> Approved: true; Approved -> Rejected: false
```

<details><summary>Hint</summary>

A tuple pattern.

</details>

<details><summary>Solution</summary>

```csharp
public static bool CanTransition(OrderStatus from, OrderStatus to) =>
    (from, to) is (OrderStatus.Pending, OrderStatus.Approved) or (OrderStatus.Pending, OrderStatus.Rejected);
```

In the database the status check and change are done by one conditional update.

</details>
