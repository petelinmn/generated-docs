---
title: "Квиз: разбор задач"
lang: ru
track: system-design
kind: module
doc: quiz
slug: 17-design-case-studies
part: 3
tags: [system-design, case-studies, url-shortener, news-feed, notifications, id-generation]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Квиз: разбор задач

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает уровень: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Первый шаг разбора?

- A) Выбрать базу
- B) Уточнить требования
- C) Нарисовать кластер
- D) Выбрать язык

<details><summary>Ответ</summary>

**B.** → [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Что выведет код?

```csharp
Console.WriteLine(Base62.Encode(62));
```

<details><summary>Ответ</summary>

```text
10
```

→ [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Нужна статистика кликов и смена адреса. Какой код?

- A) 301
- B) 302
- C) 200
- D) 404

<details><summary>Ответ</summary>

**B.** 301 эвристически кэшируется. → [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Где считать клики?

- A) Синхронно в базе ссылок
- B) Асинхронно через событие
- C) В браузере без отправки
- D) Нигде

<details><summary>Ответ</summary>

**B.** → [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 Когда fan-out при записи дорог?

- A) Мало подписчиков
- B) Миллионы подписчиков
- C) Нет постов
- D) Редкие чтения у одного автора

<details><summary>Ответ</summary>

**B.** → [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Что нужно генератору «время + узел + номер»?

- [ ] A) Уникальный номер узла
- [ ] B) Защита от скачка часов назад
- [ ] C) Центральный счётчик на каждый id
- [ ] D) Ограничение номера в пределах миллисекунды

<details><summary>Ответ</summary>

**A, B, D.** → [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Уведомление с тем же id пришло второй раз. Что сделать?

- A) Отправить снова
- B) Считать дубликатом
- C) Положить в dead-letter
- D) Удалить пользователя

<details><summary>Ответ</summary>

**B.** → [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Куда уходит уведомление после исчерпания попыток?

- A) Теряется
- B) В dead-letter
- C) Отправляется бесконечно
- D) В кэш

<details><summary>Ответ</summary>

**B.** → [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🔴 Что выведет код?

```csharp
long id = (1L << 53) + 1;
Console.WriteLine((long)(double)id);
```

<details><summary>Ответ</summary>

```text
9007199254740992
```

→ [Экспертная теория](expert.ru.md)

</details>

## 10. 🔴 Открытый вопрос

Спроектируйте чат с историей и офлайн-доставкой.

<details><summary>Ключевые пункты ответа</summary>

- Требования и оценки.
- Партиция по диалогу, номера сообщений.
- Постоянные соединения и очередь для офлайн.
- Дубли, порядок, отказы.

</details>
