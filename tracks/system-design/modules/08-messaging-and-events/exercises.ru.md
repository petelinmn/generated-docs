---
title: "Упражнения: сообщения и события"
lang: ru
track: system-design
kind: module
doc: exercises
slug: 08-messaging-and-events
part: 2
tags: [system-design, message-broker, events, delivery-semantics, idempotency, backpressure, poison-message]
versions: ".NET 11, C# 15"
status: verified
updated: 2026-11-27
---

# Упражнения: сообщения и события

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Решения запускаются из [code/Solutions/](code/Solutions/), тесты к ним — в [code/Tests/](code/Tests/).

## 1. 🟢 Партиция по ключу

**Задача:** один ключ всегда попадает в одну партицию, во всех процессах.

**Ожидаемый результат:**

```text
the same key -> the same number from 0 to partitions - 1
```

<details><summary>Подсказка</summary>

`string.GetHashCode` не подходит.

</details>

<details><summary>Решение</summary>

```csharp
uint hash = BinaryPrimitives.ReadUInt32BigEndian(SHA256.HashData(Encoding.UTF8.GetBytes(key)));
return (int)(hash % (uint)partitions);
```

Изменение числа партиций меняет размещение. Файл: [code/Solutions/Exercises.cs](code/Solutions/Exercises.cs).

</details>

## 2. 🟡 Дедупликация с окном

**Задача:** помнить только последние `window` идентификаторов.

**Ожидаемый результат:**

```text
window 2: a -> new, a -> duplicate, b, c, a -> new again
```

<details><summary>Подсказка</summary>

Множество для поиска и очередь для порядка.

</details>

<details><summary>Решение</summary>

```csharp
public bool IsNew(string id)
{
    if (!_seen.Add(id)) return false;

    _order.Enqueue(id);
    if (_order.Count > window) _seen.Remove(_order.Dequeue());
    return true;
}
```

Память ограничена, но поздний повтор пройдёт: окно выбирают по сроку возможной доставки. Не потокобезопасно.

</details>

## 3. 🔴 Восстановление порядка

**Задача:** выдавать элементы строго по номерам, придерживая те, что пришли раньше.

**Ожидаемый результат:**

```text
add 2 -> nothing; add 1 -> a, b; add 1 -> nothing; add 3 -> c
```

<details><summary>Подсказка</summary>

Храните следующий ожидаемый номер.

</details>

<details><summary>Решение</summary>

```csharp
if (sequence >= _next) _pending[sequence] = item; // older ones are duplicates

while (_pending.Remove(_next, out T? value))
{
    ready.Add(value);
    _next++;
}
```

Потерянный номер остановит выдачу навсегда: нужны лимит буфера и таймаут.

</details>
