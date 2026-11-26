---
title: "Квиз: распределённые транзакции"
lang: ru
track: system-design
kind: module
doc: quiz
slug: 10-distributed-transactions
part: 2
tags: [system-design, distributed-transactions, two-phase-commit, saga, outbox, inbox, idempotency]
versions: ".NET 11, C# 15; Microsoft.Data.Sqlite 11.0.0"
status: draft
updated: 2026-11-26
---

# Квиз: распределённые транзакции

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает уровень: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Когда 2PC фиксирует транзакцию?

- A) Если большинство «за»
- B) Если все «за»
- C) Если координатор «за»
- D) Всегда

<details><summary>Ответ</summary>

**B.** → [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Что делает saga при неудаче шага?

- A) Общий rollback базы
- B) Компенсирует выполненные шаги
- C) Удаляет сервисы
- D) Ничего

<details><summary>Ответ</summary>

**B.** → [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Что сохраняется вместе с заказом в outbox?

- A) Событие для публикации
- B) Копия базы
- C) Ответ брокера
- D) Лог ошибок

<details><summary>Ответ</summary>

**A.** → [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Чем опасно «записать в базу, затем отправить»?

- A) Медленно
- B) Сбой между шагами теряет событие
- C) Нельзя индексировать
- D) Нарушает порядок колонок

<details><summary>Ответ</summary>

**B.** → [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 В каком порядке выполняются компенсации?

- A) В прямом
- B) В обратном
- C) Случайно
- D) Параллельно всегда

<details><summary>Ответ</summary>

**B.** → [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Выбери контрмеры против аномалий саги

- [ ] A) Семантическая блокировка
- [ ] B) Коммутативные обновления
- [ ] C) Повторное чтение перед записью
- [ ] D) Отключение компенсаций

<details><summary>Ответ</summary>

**A, B, C.** → [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Что выведет код?

```csharp
Participant[] all = [new(), new()];
TwoPhaseCommit.Run(all, crashAfterPrepare: true);
Console.WriteLine(string.Join(" ", all.Select(p => p.State)));
```

<details><summary>Ответ</summary>

```text
Prepared Prepared
```

Участники в неопределённости. → [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Relay упал после публикации, до отметки. Что будет?

- A) Потеря
- B) Повторная публикация
- C) Откат заказа
- D) Ничего

<details><summary>Ответ</summary>

**B.** → [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🔴 Компенсация второго шага бросила исключение. Выполнится ли компенсация первого в примере?

- A) Да
- B) Нет
- C) Дважды
- D) Позже автоматически

<details><summary>Ответ</summary>

**B.** Исключение выходит из цикла отката. → [Экспертная теория](expert.ru.md)

</details>

## 10. 🔴 Открытый вопрос

Спроектируйте оформление заказа: резерв товара, оплата, доставка.

<details><summary>Ключевые пункты ответа</summary>

- Шаги, компенсации, pivot.
- Идемпотентность и outbox.
- Статус заказа как семантическая блокировка.
- Таймауты и ручной разбор.

</details>
