---
title: "Квиз: репликация и партиционирование"
lang: ru
track: system-design
kind: module
doc: quiz
slug: 06-replication-and-partitioning
part: 2
tags: [system-design, replication, partitioning, shard, quorum, replication-lag, version-vector]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Квиз: репликация и партиционирование

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает уровень: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Для чего в первую очередь нужно шардирование?

- A) Копии на случай отказа
- B) Разделить объём и запись
- C) Резервное копирование
- D) Шифрование

<details><summary>Ответ</summary>

**B.** A — репликация. → [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Что бывает при асинхронной репликации?

- A) Реплика может отставать
- B) Запись невозможна
- C) Реплики всегда впереди
- D) Конфликты неизбежны

<details><summary>Ответ</summary>

**A.** → [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Ключ растёт со временем, партиции по диапазонам. Куда идут новые записи?

- A) Равномерно
- B) В последнюю партицию
- C) В первую
- D) Во все

<details><summary>Ответ</summary>

**B.** → [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Какой ключ хуже всего?

- A) Идентификатор пользователя
- B) Идентификатор заказа
- C) Статус заказа из трёх значений
- D) Идентификатор устройства

<details><summary>Ответ</summary>

**C.** Три значения — три группы, нагрузку не разделить. → [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 N = 5. Какие настройки дают пересечение?

- [ ] A) W=3, R=3
- [ ] B) W=2, R=2
- [ ] C) W=5, R=1
- [ ] D) W=1, R=5

<details><summary>Ответ</summary>

**A, C, D.** У B сумма 4 не больше 5. → [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 N=3, W=2, R=2. Сколько недоступных реплик переживут обе операции?

- A) 0
- B) 1
- C) 2
- D) 3

<details><summary>Ответ</summary>

**B.** → [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Как разбить горячий ключ на запись?

- A) Добавить соль и читать все корзины
- B) Увеличить TTL
- C) Включить синхронную репликацию
- D) Переименовать ключ

<details><summary>Ответ</summary>

**A.** D оставит один ключ на одном узле. → [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Что выведет код?

```csharp
var leader = new Leader();
var follower = new Follower(leader);
leader.Write("name", "old");
follower.Replicate();
leader.Write("name", "new");
Console.WriteLine(follower.Read("name"));
```

<details><summary>Ответ</summary>

```text
old
```

Вторая запись ещё не применена репликой. → [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🔴 Что выведет код?

```csharp
var r = new QuorumRegister(3);
r.Write(1, "v1", 0, 1, 2);
r.Write(2, "v2", 0);
Console.WriteLine($"{r.Read(0, 1).Value} {r.Read(1, 2).Value}");
```

<details><summary>Ответ</summary>

```text
v2 v1
```

Запись не достигла W=2 и не откатилась. → [Экспертная теория](expert.ru.md)

</details>

## 10. 🔴 Открытый вопрос

Как выбрать ключ шардирования для мессенджера?

<details><summary>Ключевые пункты ответа</summary>

- Частые запросы: история чата, список чатов пользователя.
- Ключ по чату держит историю вместе; большой чат может стать горячим.
- Для списка чатов нужна вторая денормализованная структура.
- План ребалансировки.

</details>
