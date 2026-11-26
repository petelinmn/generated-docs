---
title: "Квиз: индексы"
lang: ru
track: postgresql
kind: module
doc: quiz
slug: 05-indexes
part: 2
tags: [index, b-tree, gin, brin, query-plan]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-26
---

# Квиз: индексы

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает уровень: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Какой тип индекса создаёт `CREATE INDEX` по умолчанию?

- A) Hash
- B) B-tree
- C) GIN
- D) BRIN

<details><summary>Ответ</summary>

**B.** Остальные требуют `USING`.

→ [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Какое ограничение само НЕ создаёт индекс?

- A) `PRIMARY KEY`
- B) `UNIQUE`
- C) `FOREIGN KEY`
- D) Все создают

<details><summary>Ответ</summary>

**C.** Ссылающиеся колонки не получают индекс автоматически.

→ [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Какой запрос индекс `(user_id, created_at)` обслужит лучше всего?

- A) `WHERE created_at = ?`
- B) `WHERE user_id = ? ORDER BY created_at DESC`
- C) `WHERE status = ?`
- D) `ORDER BY email`

<details><summary>Ответ</summary>

**B.** Равенство по первой колонке и порядок по второй. A может лишь обойти индекс целиком.

→ [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 У 99% строк `status = 'done'`. Что вероятно сделает планировщик?

- A) Index Scan
- B) Index Only Scan
- C) Seq Scan
- D) Откажет в запросе

<details><summary>Ответ</summary>

**C.** Один раз прочитать таблицу дешевле, чем достать почти каждую строку через индекс.

→ [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 Поможет ли этот индекс запросу `WHERE lower(email) = ?`

```sql
CREATE INDEX ON events (email);
```

<details><summary>Ответ</summary>

Нет. В нём хранится `email`, а не `lower(email)`; нужен индекс по выражению.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Выбери все верные утверждения о частичных индексах

- [ ] A) Содержит только строки, подходящие под его `WHERE`
- [ ] B) Обслуживает любой запрос по индексируемой колонке
- [ ] C) Обычно меньше полного индекса
- [ ] D) Из условия запроса должно следовать условие индекса

<details><summary>Ответ</summary>

**A, C, D.** Запрос по другим строкам использовать его не может.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 После массовых обновлений выросли `Heap Fetches`. Что поможет?

- A) `VACUUM`
- B) Только `ANALYZE`
- C) Убрать колонки `INCLUDE`
- D) Hash-индекс

<details><summary>Ответ</summary>

**A.** `VACUUM` отмечает страницы полностью видимыми в карте видимости; `ANALYZE` лишь собирает статистику.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Какое условие обслуживает GIN по умолчанию на `payload`?

- A) `payload ->> 'kind' = 'k7'`
- B) `payload @> '{"kind":"k7"}'`
- C) `length(payload::text) > 10`
- D) `ORDER BY payload`

<details><summary>Ответ</summary>

**B.** Вхождение — оператор GIN; форме с `->>` нужен индекс по выражению.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🟡 Найди проблему в миграции

```sql
BEGIN;
CREATE INDEX CONCURRENTLY ix ON events (user_id);
COMMIT;
```

<details><summary>Ответ</summary>

`CREATE INDEX CONCURRENTLY` нельзя выполнять внутри блока транзакции.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 10. 🔴 Какой тип индекса может быть `UNIQUE`?

- A) B-tree
- B) Hash
- C) GIN
- D) BRIN

<details><summary>Ответ</summary>

**A.** Уникальность поддерживает только B-tree.

→ [Экспертная теория](expert.ru.md)

</details>

## 11. 🔴 Какие обновления могут быть HOT?

- A) Любые
- B) Не меняющие индексируемых колонок (кроме суммирующих индексов), когда на странице есть место
- C) Только в таблицах без индексов
- D) Только после `VACUUM FULL`

<details><summary>Ответ</summary>

**B.** Нужны оба условия.

→ [Экспертная теория](expert.ru.md)

</details>

## 12. 🔴 Открытый вопрос: запрос стал медленным, хотя «индекс используется». Как разбираться?

<details><summary>Ключевые пункты ответа</summary>

- `EXPLAIN (ANALYZE, BUFFERS)`: фактические строки, буферы, что попало в `Index Cond`, а что в `Filter`.
- Не обходится ли весь индекс? Есть ли условие на первую колонку?
- `Heap Fetches` и `VACUUM`, раздувание, устаревшая статистика.
- Сравнить с индексом лучшей формы на копии данных.

→ [Экспертная теория](expert.ru.md)

</details>
