---
title: "Квиз: сообщения и события"
lang: ru
track: system-design
kind: module
doc: quiz
slug: 08-messaging-and-events
part: 2
tags: [system-design, message-broker, events, delivery-semantics, idempotency, backpressure, poison-message]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Квиз: сообщения и события

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает уровень: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что из этого — событие?

- A) `SendInvoice`
- B) `InvoiceSent`
- C) `CreateOrder`
- D) `DeleteUser`

<details><summary>Ответ</summary>

**B.** Факт в прошедшем времени. → [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Что происходит с прочитанным событием в журнале?

- A) Удаляется
- B) Остаётся до истечения хранения
- C) Переносится в dead-letter
- D) Блокируется

<details><summary>Ответ</summary>

**B.** → [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Зачем ack?

- A) Шифрование
- B) Сообщить, что сообщение обработано
- C) Увеличить приоритет
- D) Сжать данные

<details><summary>Ответ</summary>

**B.** → [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Главная выгода асинхронности?

- A) Мгновенный результат
- B) Получатель может быть временно недоступен
- C) Нет повторов
- D) Строгий порядок

<details><summary>Ответ</summary>

**B.** → [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 Ack до обработки даёт

- A) at-most-once
- B) at-least-once
- C) exactly-once
- D) упорядоченность

<details><summary>Ответ</summary>

**A.** → [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Какой `FullMode` по умолчанию?

- A) `DropOldest`
- B) `Wait`
- C) `DropWrite`
- D) `DropNewest`

<details><summary>Ответ</summary>

**B.** → [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Что выведет код?

```csharp
var (accepted, buffered) = Backpressure.Overflow(BoundedChannelFullMode.DropOldest, 2, 5);
Console.WriteLine($"{accepted}: {string.Join(" ", buffered)}");
```

<details><summary>Ответ</summary>

```text
5: 4 5
```

Все записи «успешны», остались две последние. → [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Найди ошибку

«Сохраняем заказ в базу, затем публикуем событие в брокер».

<details><summary>Ответ</summary>

Сбой между шагами потеряет событие. Нужен outbox. → [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🔴 Почему может не завершиться этот цикл?

```csharp
await foreach (var x in channel.Reader.ReadAllAsync()) Handle(x);
```

<details><summary>Ответ</summary>

Писатель не вызвал `Complete()`: читатель ждёт новых элементов. → [Экспертная теория](expert.ru.md)

</details>

## 10. 🔴 Открытый вопрос

Спроектируйте обработку платёжных сообщений без двойных списаний.

<details><summary>Ключевые пункты ответа</summary>

- Ack после обработки, идентификатор сообщения.
- Дедупликация в одной транзакции с эффектом.
- Idempotency key у внешнего провайдера.
- Dead-letter, алерты и сверка.

</details>
