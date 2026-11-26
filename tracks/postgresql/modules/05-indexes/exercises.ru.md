---
title: "Упражнения: индексы"
lang: ru
track: postgresql
kind: module
doc: exercises
slug: 05-indexes
part: 2
tags: [index, b-tree, gin, brin, query-plan]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-26
---

# Упражнения: индексы

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Таблица — из [базовой теории](basics.ru.md), каждый ответ проверяй через `EXPLAIN`.

## 1. 🟢 Последние события пользователя

**Задача:** сделать так, чтобы «десять последних событий одного пользователя» выполнялись без шага сортировки.

<details><summary>Подсказка</summary>

Какая колонка сравнивается на равенство, а какая упорядочивает результат?

</details>

<details><summary>Решение</summary>

```sql
CREATE INDEX events_user_created ON events (user_id, created_at DESC);
SELECT * FROM events WHERE user_id = 42 ORDER BY created_at DESC LIMIT 10;
```

Подойдёт и индекс по возрастанию: он читается в обратную сторону.

</details>

## 2. 🟡 Очередь неудач

**Задача:** неудачные события составляют 1% таблицы и обрабатываются начиная со старых. Сделайте индекс как можно меньше.

<details><summary>Подсказка</summary>

Индекс не обязан содержать все строки.

</details>

<details><summary>Решение</summary>

```sql
CREATE INDEX events_failed ON events (created_at) WHERE status = 'failed';
```

Запрос должен повторять `status = 'failed'`.

</details>

## 3. 🔴 Вход без учёта регистра

**Задача:** найти событие по email независимо от регистра букв, используя индекс.

<details><summary>Подсказка</summary>

Что именно сравнивается в `WHERE`?

</details>

<details><summary>Решение</summary>

```sql
CREATE INDEX events_lower_email ON events (lower(email));
ANALYZE events;
EXPLAIN (COSTS OFF) SELECT id FROM events WHERE lower(email) = 'user7@example.com';
```

Выражение в запросе должно совпадать с выражением в индексе. `UNIQUE`-индекс по тому же выражению запретил бы и адреса, различающиеся только регистром.

</details>
