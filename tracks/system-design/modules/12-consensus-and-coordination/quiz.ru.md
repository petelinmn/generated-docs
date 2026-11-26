---
title: "Квиз: консенсус и координация"
lang: ru
track: system-design
kind: module
doc: quiz
slug: 12-consensus-and-coordination
part: 3
tags: [system-design, consensus, raft, leader-election, distributed-lock, fencing-token, logical-clocks]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Квиз: консенсус и координация

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает уровень: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Большинство в кластере из 5?

- A) 2
- B) 3
- C) 4
- D) 5

<details><summary>Ответ</summary>

**B.** → [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Состояния узла Raft?

- A) master, slave
- B) follower, candidate, leader
- C) open, closed
- D) reader, writer

<details><summary>Ответ</summary>

**B.** → [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Зачем срок у lease?

- A) Чтобы упавший владелец не держал ресурс вечно
- B) Для шифрования
- C) Для скорости
- D) Для порядка

<details><summary>Ответ</summary>

**A.** → [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Хранилище видело токен 34. Придёт 33. Что будет?

- A) Принято
- B) Отклонено
- C) Токен обновится на 33
- D) Блокировка продлится

<details><summary>Ответ</summary>

**B.** → [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 Какой журнал актуальнее?

- A) Тот, где больше терм последней записи
- B) Всегда более длинный
- C) Того, кто раньше стартовал
- D) Случайный

<details><summary>Ответ</summary>

**A.** Длина решает только при равных термах. → [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Сколько голосов за терм может отдать узел?

- A) Один
- B) Два
- C) Сколько угодно
- D) Ни одного

<details><summary>Ответ</summary>

**A.** → [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Что выведет код?

```csharp
var voter = new Node(1);
Console.WriteLine(voter.RequestVote(1, 2, 0, 0));
Console.WriteLine(voter.RequestVote(1, 3, 0, 0));
```

<details><summary>Ответ</summary>

```text
True
False
```

→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Что делает лидер, увидев больший терм?

- A) Игнорирует
- B) Становится follower
- C) Увеличивает свой на два
- D) Выключается

<details><summary>Ответ</summary>

**B.** → [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🔴 Метка Лэмпорта A = 1, B = 5. Что известно?

- A) A точно раньше B
- B) B точно не причина A
- C) События одновременны
- D) Ничего нельзя сказать вообще

<details><summary>Ответ</summary>

**B.** Если бы B было причиной A, метка B была бы меньше. → [Экспертная теория](expert.ru.md)

</details>

## 10. 🔴 Открытый вопрос

Как гарантировать, что отчёт формирует только один экземпляр сервиса?

<details><summary>Ключевые пункты ответа</summary>

- Lease с продлением.
- Fencing или уникальный ключ результата.
- Идемпотентность задачи.
- Поведение при паузе и потере связи.

</details>
