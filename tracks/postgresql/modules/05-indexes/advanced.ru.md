---
title: "Индексы: продвинутая теория"
lang: ru
track: postgresql
kind: module
doc: advanced
slug: 05-indexes
part: 2
tags: [index, b-tree, gin, brin, query-plan]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-26
---

# Индексы: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Частичные индексы

```sql
CREATE INDEX events_failed ON events (created_at) WHERE status = 'failed';

EXPLAIN (COSTS OFF) SELECT id FROM events WHERE status = 'failed' ORDER BY created_at LIMIT 5;
-- Limit
--   ->  Index Scan using events_failed on events

-- the same query for status = 'done' cannot use it: Sort over Seq Scan
```

Индекс содержит только строки, удовлетворяющие его `WHERE`, поэтому он мал и дёшев в сопровождении. Планировщик использует его, только когда из условия запроса следует условие индекса.

## Индексы по выражению

```sql
CREATE INDEX events_email ON events (email);
EXPLAIN (COSTS OFF) SELECT * FROM events WHERE lower(email) = 'user7@example.com';
-- Seq Scan on events

CREATE INDEX events_lower_email ON events (lower(email));
ANALYZE events;
EXPLAIN (COSTS OFF) SELECT * FROM events WHERE lower(email) = 'user7@example.com';
-- Index Scan using events_lower_email on events
```

Индекс хранит значения, а не результаты функций над ними. `ANALYZE` после создания собирает статистику по выражению.

## LIKE и классы операторов

```sql
-- database collation is en_US.utf8
EXPLAIN (COSTS OFF) SELECT * FROM events WHERE email LIKE 'User777%';        -- Seq Scan
CREATE INDEX events_email_pattern ON events (email text_pattern_ops);
EXPLAIN (COSTS OFF) SELECT * FROM events WHERE email LIKE 'User777%';
-- Index Scan using events_email_pattern on events
--   Index Cond: ((email ~>=~ 'User777') AND (email ~<~ 'User778'))
EXPLAIN (COSTS OFF) SELECT * FROM events WHERE email LIKE '%777@example.com'; -- Seq Scan
```

При сортировке (collation), отличной от `C`, обычный B-tree не обслуживает `LIKE 'abc%'`; класс операторов `text_pattern_ops` сравнивает посимвольно и обслуживает. С ведущим `%` не помогает ни тот, ни другой; для поиска подстроки есть расширение `pg_trgm`.

## Покрывающие индексы и index-only scan

```sql
CREATE INDEX events_user_inc ON events (user_id) INCLUDE (status);

EXPLAIN (ANALYZE, COSTS OFF, TIMING OFF, SUMMARY OFF) SELECT user_id, status FROM events WHERE user_id = 42;
-- Index Only Scan using events_user_inc ... (actual rows=100)
--   Heap Fetches: 0

UPDATE events SET status = status WHERE user_id = 42;
-- the same EXPLAIN: Heap Fetches: 200

VACUUM events;
-- the same EXPLAIN: Heap Fetches: 0
```

Колонки `INCLUDE` — это нагрузка: они хранятся в индексе, но не упорядочивают его. В записях индекса нет сведений о видимости, поэтому PostgreSQL смотрит в карту видимости; для недавно изменённых страниц приходится идти в таблицу (`Heap Fetches`), пока `VACUUM` не отметит их полностью видимыми.

## GIN для jsonb

```sql
CREATE INDEX events_payload ON events USING gin (payload);
ANALYZE events;

EXPLAIN (COSTS OFF) SELECT * FROM events WHERE payload @> '{"kind":"k7"}';
-- Bitmap Heap Scan on events
--   ->  Bitmap Index Scan on events_payload

EXPLAIN (COSTS OFF) SELECT * FROM events WHERE payload ->> 'kind' = 'k7';
-- Seq Scan on events
```

GIN с классом по умолчанию `jsonb_ops` поддерживает вхождение `@>` и операторы существования ключа, но не произвольное выражение вроде равенства по `->>`. Для него создайте B-tree по выражению `(payload ->> 'kind')`.

## BRIN

```sql
CREATE INDEX events_brin ON events USING brin (created_at);
CREATE INDEX events_btree_time ON events (created_at);
```

| Object | Size |
|---|---|
| table `events` | 9888 kB |
| BRIN on `created_at` | 24 kB |
| B-tree on `created_at` | 2208 kB |
| GIN on `payload` | 536 kB |

BRIN хранит сводку (минимум и максимум) на диапазон страниц таблицы. Он работает, когда колонка коррелирует с физическим порядком, например время вставки в таблице, куда только дописывают. Он лишь сужает набор страниц, поэтому строки перепроверяются.

## Построение без блокировки записи

Обычный `CREATE INDEX` блокирует запись (но не чтение) до конца построения. `CONCURRENTLY` разрешает запись, работает дольше и не может выполняться внутри транзакции:

```sql
BEGIN;
CREATE INDEX CONCURRENTLY x1 ON events (id);
-- ERROR:  CREATE INDEX CONCURRENTLY cannot run inside a transaction block
ROLLBACK;

CREATE TABLE dup (a int);
INSERT INTO dup VALUES (1), (1);
CREATE UNIQUE INDEX CONCURRENTLY dup_a ON dup (a);
-- ERROR:  could not create unique index "dup_a"

SELECT c.relname, i.indisvalid FROM pg_index i JOIN pg_class c ON c.oid = i.indexrelid
WHERE i.indrelid = 'dup'::regclass;
-- dup_a | f
```

Недействительный индекс запросы не используют, но на запись он тратит время. Удалите его и повторите или используйте `REINDEX INDEX CONCURRENTLY`.

## Лучшие практики

- Начинайте с медленного запроса и проектируйте индекс под него.
- Сначала колонки с равенством, за ними колонка диапазона или сортировки.
- Неиспользуемые индексы ищите через `pg_stat_user_indexes` за представительный период.
- На рабочих таблицах создавайте индексы с `CONCURRENTLY` и затем проверяйте действительность.

## Частые ошибки

- ❌ Функция над колонкой при индексе по самой колонке → ✅ индекс по выражению.
- ❌ Дублирующие `(a)` и `(a, b)` без причины → ✅ обычно хватает составного.
- ❌ BRIN на значениях в случайном порядке → ✅ тогда каждый диапазон покрывает всё.

## Связанные материалы

- [Базовая теория](basics.ru.md)
- [Экспертная теория](expert.ru.md)

## Источники

- [PostgreSQL 17: типы индексов](https://www.postgresql.org/docs/17/indexes-types.html)
- [PostgreSQL 17: составные индексы](https://www.postgresql.org/docs/17/indexes-multicolumn.html)
- [PostgreSQL 17: index-only scans](https://www.postgresql.org/docs/17/indexes-index-only-scans.html)
- [PostgreSQL 17: CREATE INDEX](https://www.postgresql.org/docs/17/sql-createindex.html)
