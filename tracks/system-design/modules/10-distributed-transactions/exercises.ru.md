---
title: "Упражнения: распределённые транзакции"
lang: ru
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

# Упражнения: распределённые транзакции

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Решения запускаются из [code/Solutions/](code/Solutions/), тесты к ним — в [code/Tests/](code/Tests/).

## 1. 🟢 Решение координатора

**Задача:** по голосам участников вернуть `Commit` или `Abort`.

**Ожидаемый результат:**

```text
Yes Yes -> Commit; Yes Unknown -> Abort; no votes -> Abort
```

<details><summary>Подсказка</summary>

`All` для пустой коллекции возвращает `true`.

</details>

<details><summary>Решение</summary>

```csharp
public static Decision Decide(IReadOnlyCollection<Vote> votes) =>
    votes.Count > 0 && votes.All(v => v == Vote.Yes) ? Decision.Commit : Decision.Abort;
```

Файл: [code/Solutions/Exercises.cs](code/Solutions/Exercises.cs).

</details>

## 2. 🟡 Повтор компенсации

**Задача:** повторять компенсацию до успеха, не более заданного числа раз; вернуть номер попытки.

**Ожидаемый результат:**

```text
fails twice, then works -> 3; limit 2 -> the exception propagates
```

<details><summary>Подсказка</summary>

Фильтр исключения `when`.

</details>

<details><summary>Решение</summary>

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

Между попытками в production нужна задержка.

</details>

## 3. 🔴 Семантическая блокировка

**Задача:** разрешить переходы только из `Pending`.

**Ожидаемый результат:**

```text
Pending -> Approved: true; Approved -> Rejected: false
```

<details><summary>Подсказка</summary>

Шаблон по кортежу.

</details>

<details><summary>Решение</summary>

```csharp
public static bool CanTransition(OrderStatus from, OrderStatus to) =>
    (from, to) is (OrderStatus.Pending, OrderStatus.Approved) or (OrderStatus.Pending, OrderStatus.Rejected);
```

Проверку и смену статуса в базе делают одним условным обновлением.

</details>
