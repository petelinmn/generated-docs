---
title: "Exercises: event sourcing and CQRS"
lang: en
track: system-design
kind: module
doc: exercises
slug: 13-event-sourcing-and-cqrs
part: 3
tags: [system-design, event-sourcing, cqrs, events, projection, snapshot, upcasting]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Exercises: event sourcing and CQRS

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. The solutions run from [code/Solutions/](code/Solutions/), their tests are in [code/Tests/](code/Tests/).

## 1. 🟢 A past state

**Task:** return the balance after the first `version` events.

**Expected result:**

```text
+100, -30, +5 at version 2 -> 70
```

<details><summary>Hint</summary>

`Take`.

</details>

<details><summary>Solution</summary>

```csharp
public static decimal BalanceAt(IReadOnlyList<AccountEvent> events, int version) =>
    Account.Replay(events.Take(version)).Balance;
```

File: [code/Solutions/Exercises.cs](code/Solutions/Exercises.cs).

</details>

## 2. 🟡 Loading from a snapshot

**Task:** start from a snapshot and apply only later events.

**Expected result:**

```text
Load(snapshot at 2) equals a full replay
```

<details><summary>Hint</summary>

The snapshot version says how many to skip.

</details>

<details><summary>Solution</summary>

```csharp
public static Account Load(Account? snapshot, IReadOnlyList<AccountEvent> events) =>
    Account.Replay(events.Skip(snapshot?.Version ?? 0), snapshot);
```

A real store reads only the tail instead of everything followed by `Skip`.

</details>

## 3. 🔴 Retry after a conflict

**Task:** withdraw money, rereading the stream and deciding again on a conflict.

**Expected result:**

```text
balance 100, concurrent -50, withdraw 60 -> false, balance stays 50
```

<details><summary>Hint</summary>

A refusal by a business rule is not retried.

</details>

<details><summary>Solution</summary>

```csharp
Account account = Account.Replay(store.Read(stream));
if (amount > account.Balance) return false; // a business refusal is not retried

AccountEvent decision = account.Withdraw(amount);
beforeAppend?.Invoke(); // lets a test slip in a concurrent write
if (store.TryAppend(stream, account.Version, decision)) return true;
```

This is the body of the attempt loop. The old decision must not be appended without rechecking.

</details>
