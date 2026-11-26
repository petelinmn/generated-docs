---
title: "Квиз: event sourcing и CQRS"
lang: ru
track: system-design
kind: module
doc: quiz
slug: 13-event-sourcing-and-cqrs
part: 3
tags: [system-design, event-sourcing, cqrs, events, projection, snapshot, upcasting]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Квиз: event sourcing и CQRS

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает уровень: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что является источником истины?

- A) Модель чтения
- B) Поток событий
- C) Снимок
- D) Кэш

<details><summary>Ответ</summary>

**B.** → [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Как назвать событие?

- A) `Withdraw`
- B) `Withdrawn`
- C) `DoWithdraw`
- D) `UpdateRow`

<details><summary>Ответ</summary>

**B.** Прошедшее время. → [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Что делает CQRS?

- A) Шифрует события
- B) Разделяет модели записи и чтения
- C) Объединяет базы
- D) Удаляет историю

<details><summary>Ответ</summary>

**B.** → [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Что выведет код?

```csharp
Console.WriteLine(Account.Replay([new Deposited(100), new Withdrawn(30)]).Balance);
```

<details><summary>Ответ</summary>

```text
70
```

→ [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 Зачем checkpoint?

- A) Чтобы продолжить с места и пропустить повторы
- B) Для шифрования
- C) Для снимка агрегата
- D) Для удаления событий

<details><summary>Ответ</summary>

**A.** → [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Что такое upcasting?

- A) Переписывание хранилища
- B) Преобразование старой формы при чтении
- C) Приведение типов в C#
- D) Увеличение версии потока

<details><summary>Ответ</summary>

**B.** → [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Выбери верное о снимках

- [ ] A) Их можно пересоздать из событий
- [ ] B) После снимка старые события обязательно удаляют
- [ ] C) Они ускоряют загрузку
- [ ] D) Они заменяют поток как источник истины

<details><summary>Ответ</summary>

**A, C.** → [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Что такое crypto-shredding?

- A) Удаление ключа, без которого данные не прочитать
- B) Сжатие журнала
- C) Хэширование событий
- D) Разбиение потока

<details><summary>Ответ</summary>

**A.** → [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🔴 Что выведет код?

```csharp
Base e = new Dep(5);
Console.WriteLine(JsonSerializer.Serialize(e));
```

<details><summary>Ответ</summary>

```text
{}
```

Сериализуется объявленный тип `Base`. → [Экспертная теория](expert.ru.md)

</details>

## 10. 🔴 Открытый вопрос

Когда вы откажетесь от event sourcing?

<details><summary>Ключевые пункты ответа</summary>

- Простой CRUD без ценности истории.
- Частые инварианты между агрегатами.
- Нет готовности к версиям и eventual consistency.
- Достаточно аудита.

</details>
