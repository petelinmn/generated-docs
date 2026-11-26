---
title: "Упражнения: event sourcing и CQRS"
lang: ru
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

# Упражнения: event sourcing и CQRS

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Решения запускаются из [code/Solutions/](code/Solutions/), тесты к ним — в [code/Tests/](code/Tests/).

## 1. 🟢 Состояние в прошлом

**Задача:** вернуть баланс после первых `version` событий.

**Ожидаемый результат:**

```text
+100, -30, +5 at version 2 -> 70
```

<details><summary>Подсказка</summary>

`Take`.

</details>

<details><summary>Решение</summary>

```csharp
public static decimal BalanceAt(IReadOnlyList<AccountEvent> events, int version) =>
    Account.Replay(events.Take(version)).Balance;
```

Файл: [code/Solutions/Exercises.cs](code/Solutions/Exercises.cs).

</details>

## 2. 🟡 Загрузка со снимка

**Задача:** начать со снимка и применить только более поздние события.

**Ожидаемый результат:**

```text
Load(snapshot at 2) equals a full replay
```

<details><summary>Подсказка</summary>

Версия снимка говорит, сколько пропустить.

</details>

<details><summary>Решение</summary>

```csharp
public static Account Load(Account? snapshot, IReadOnlyList<AccountEvent> events) =>
    Account.Replay(events.Skip(snapshot?.Version ?? 0), snapshot);
```

Настоящее хранилище читает только хвост, а не всё с последующим `Skip`.

</details>

## 3. 🔴 Повтор после конфликта

**Задача:** снять деньги, перечитывая поток и принимая решение заново при конфликте.

**Ожидаемый результат:**

```text
balance 100, concurrent -50, withdraw 60 -> false, balance stays 50
```

<details><summary>Подсказка</summary>

Отказ по бизнес-правилу не повторяют.

</details>

<details><summary>Решение</summary>

```csharp
Account account = Account.Replay(store.Read(stream));
if (amount > account.Balance) return false; // a business refusal is not retried

AccountEvent decision = account.Withdraw(amount);
beforeAppend?.Invoke(); // lets a test slip in a concurrent write
if (store.TryAppend(stream, account.Version, decision)) return true;
```

Это тело цикла по попыткам. Добавить старое решение без перепроверки нельзя.

</details>
