---
title: "Квиз: согласованность и CAP"
lang: ru
track: system-design
kind: module
doc: quiz
slug: 07-consistency-and-cap
part: 2
tags: [system-design, consistency, cap-theorem, isolation-levels, mvcc, write-skew, crdt]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Квиз: согласованность и CAP

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает уровень: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Когда приходится выбирать между C и A по CAP?

- A) Всегда
- B) Во время сетевого разделения
- C) При высокой нагрузке
- D) При обновлении схемы

<details><summary>Ответ</summary>

**B.** → [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Что такое dirty read?

- A) Чтение незафиксированных данных
- B) Чтение с реплики
- C) Повторное чтение
- D) Чтение без индекса

<details><summary>Ответ</summary>

**A.** → [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Что обещает eventual consistency?

- A) Свежие данные через секунду
- B) Схождение копий, если записи прекратятся
- C) Строгий порядок
- D) Отсутствие конфликтов

<details><summary>Ответ</summary>

**B.** Срока нет. → [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Узел в меньшинстве в режиме CP

- A) принимает записи
- B) отказывает
- C) удаляет данные
- D) становится лидером

<details><summary>Ответ</summary>

**B.** → [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 Уровень по умолчанию в PostgreSQL?

- A) Read uncommitted
- B) Read committed
- C) Repeatable read
- D) Serializable

<details><summary>Ответ</summary>

**B.** → [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Какие аномалии предотвращает Serializable?

- [ ] A) Dirty read
- [ ] B) Phantom read
- [ ] C) Write skew
- [ ] D) Отказ сети

<details><summary>Ответ</summary>

**A, B, C.** → [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Что выведет код?

```csharp
var session = new Session();
Console.WriteLine(session.Read(new Replica(2, "new")));
Console.WriteLine(session.Read(new Replica(1, "old")) is null);
```

<details><summary>Ответ</summary>

```text
new
True
```

→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Что означает E в PACELC?

- A) Ошибка
- B) «Иначе»: когда разделения нет
- C) Eventual
- D) Эластичность

<details><summary>Ответ</summary>

**B.** → [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🔴 Что получится в `d["x"]`?

```csharp
var d = new ConcurrentDictionary<string, int> { ["x"] = 0 };
// two threads: read d["x"], wait for each other, write value + 1
```

<details><summary>Ответ</summary>

```text
1
```

Lost update: пара операций не атомарна. → [Экспертная теория](expert.ru.md)

</details>

## 10. 🔴 Открытый вопрос

Как гарантировать, что последний билет не продадут дважды?

<details><summary>Ключевые пункты ответа</summary>

- Атомарное обновление с условием или ограничение базы.
- Или serializable с повторами.
- Не полагаться на реплики для решения.
- Идемпотентность запроса клиента.

</details>
