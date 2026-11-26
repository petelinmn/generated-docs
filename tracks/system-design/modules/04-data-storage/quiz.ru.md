---
title: "Квиз: хранение данных"
lang: ru
track: system-design
kind: module
doc: quiz
slug: 04-data-storage
part: 1
tags: [system-design, data-modeling, database-index, b-tree, lsm-tree, query-plan, nosql]
versions: ".NET 11, C# 15; Microsoft.Data.Sqlite 11.0.0"
status: verified
updated: 2026-11-26
---

# Квиз: хранение данных

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает уровень: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 С чего начинать выбор хранилища?

- A) С популярности
- B) С паттернов доступа
- C) С языка приложения
- D) С цены диска

<details><summary>Ответ</summary>

**B.** Модель и индексы следуют из запросов. → [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Что делает индекс с записью?

- A) Ускоряет
- B) Замедляет
- C) Не влияет
- D) Запрещает дубликаты всегда

<details><summary>Ответ</summary>

**B.** Его тоже надо обновить. D верно только для уникального индекса. → [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Что означает `SCAN` в плане SQLite?

- A) Просмотр всей таблицы или всего индекса
- B) Поиск по ключу
- C) Покрывающий индекс
- D) Ошибку

<details><summary>Ответ</summary>

**A.** Поиск части строк обозначается `SEARCH`. → [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Где хранить видеофайлы?

- A) В колонке основной таблицы
- B) В объектном хранилище, в базе — ссылка
- C) В кэше
- D) В поисковом индексе

<details><summary>Ответ</summary>

**B.** → [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 Какой индекс лучше для `WHERE chat_id = ? ORDER BY sent_at DESC`?

- A) `(sent_at)`
- B) `(sent_at, chat_id)`
- C) `(chat_id, sent_at)`
- D) `(body)`

<details><summary>Ответ</summary>

**C.** Фильтр и порядок обслуживаются одним проходом. → [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Выбери все верные утверждения об LSM

- [ ] A) Сегменты на диске неизменяемы
- [ ] B) Удаление немедленно стирает все старые копии
- [ ] C) Чтение может проверить несколько структур
- [ ] D) Compaction убирает старые версии

<details><summary>Ответ</summary>

**A, C, D.** Удаление лишь пишет tombstone. → [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Что выведет код?

```csharp
var store = new LsmStore(memtableLimit: 2);
store.Put("a", "1"); store.Put("b", "2");
store.Put("a", "10"); store.Delete("b");
Console.WriteLine(store.StoredEntries);
store.Compact();
Console.WriteLine(store.StoredEntries);
```

<details><summary>Ответ</summary>

```text
4
1
```

До compaction хранятся две версии `a`, значение `b` и его tombstone. → [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Какая цена денормализации?

- A) Больше join
- B) Копии нужно согласовывать
- C) Нельзя индексировать
- D) Нет транзакций

<details><summary>Ответ</summary>

**B.** → [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🔴 Найди проблему

```sql
CREATE INDEX ix_email ON users (email);
SELECT name FROM users WHERE lower(email) = 'a@b.c';
```

<details><summary>Ответ</summary>

Индекс не используется (`SCAN users`). Нужен индекс по `lower(email)` или хранение email в нормализованном виде. → [Экспертная теория](expert.ru.md)

</details>

## 10. 🔴 Открытый вопрос

Спроектируйте хранение сообщений чата с поиском по тексту.

<details><summary>Ключевые пункты ответа</summary>

- Паттерны доступа и объёмы.
- Ключ `(chat_id, sent_at)`, возможно партиционирование по чату.
- Поиск — отдельный индекс, обновляемый асинхронно.
- Назвать задержку индексации и источник истины.

</details>
