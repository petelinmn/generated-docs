---
title: "Производительность и EXPLAIN: базовая теория"
lang: ru
track: postgresql
kind: module
doc: basics
slug: 09-performance-explain
part: 2
tags: [explain, query-plan, statistics, vacuum, performance]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-26
---

# Производительность и EXPLAIN: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [индексы](../05-indexes/overview.ru.md) · **Время:** ~2 ч

## Данные для примеров

Все результаты получены на PostgreSQL 17.11. Планы и время зависят от данных и оборудования.

```sql
CREATE TABLE orders (
    id int PRIMARY KEY, customer_id int NOT NULL, status text NOT NULL,
    city text NOT NULL, country text NOT NULL,
    amount numeric(10,2) NOT NULL, created_at timestamptz NOT NULL
);
INSERT INTO orders
SELECT i, i % 5000,
       CASE WHEN i % 50 = 0 THEN 'new' ELSE 'done' END,
       'city' || (i % 100), 'country' || (i % 100 / 10),
       (i % 1000) / 10.0,
       timestamptz '2026-01-01 00:00+00' + i * interval '30 seconds'
FROM generate_series(1, 200000) i;
CREATE INDEX orders_customer ON orders (customer_id);
VACUUM ANALYZE orders;
SET max_parallel_workers_per_gather = 0;   -- simpler plans for the examples
```

## Как читать EXPLAIN

```sql
EXPLAIN SELECT * FROM orders WHERE customer_id = 42;
```

```text
Bitmap Heap Scan on orders  (cost=4.60..147.55 rows=40 width=41)
  Recheck Cond: (customer_id = 42)
  ->  Bitmap Index Scan on orders_customer  (cost=0.00..4.59 rows=40 width=0)
        Index Cond: (customer_id = 42)
```

| Поле | Смысл |
|---|---|
| `cost=4.60..147.55` | стоимость запуска (до первой строки) и полная стоимость |
| `rows=40` | оценка числа строк, которые вернёт узел |
| `width=41` | оценка среднего размера строки в байтах |

Стоимость измеряется в условных единицах: по соглашению последовательное чтение одной страницы стоит 1 (`seq_page_cost`). Она полезна только для сравнения планов одного запроса. План — это дерево: самые вложенные узлы выполняются первыми и передают строки родителям.

## EXPLAIN ANALYZE

```sql
EXPLAIN (ANALYZE, BUFFERS, TIMING OFF) SELECT * FROM orders WHERE customer_id = 42;
```

```text
Bitmap Heap Scan on orders  (cost=4.60..147.55 rows=40 width=41) (actual rows=40 loops=1)
  Recheck Cond: (customer_id = 42)
  Heap Blocks: exact=40
  Buffers: shared hit=40 read=2
  ->  Bitmap Index Scan on orders_customer  (cost=0.00..4.59 rows=40 width=0) (actual rows=40 loops=1)
        Index Cond: (customer_id = 42)
        Buffers: shared read=2
Planning Time: 0.036 ms
Execution Time: 0.095 ms
```

`actual rows` — сколько строк узел вернул на самом деле. Здесь оценка и реальность совпали: 40 и 40. `TIMING OFF` скрывает время по узлам: его измерение само чего-то стоит и меняется от запуска к запуску.

`BUFFERS` показывает страницы по 8 КБ: `shared hit` взяты из кэша PostgreSQL, `read` — из операционной системы или с диска. В PostgreSQL 17 `BUFFERS` нужно запрашивать явно; PostgreSQL 18 добавляет его в `EXPLAIN ANALYZE` автоматически.

## ANALYZE действительно выполняет оператор

```sql
BEGIN;
EXPLAIN (ANALYZE, COSTS OFF) DELETE FROM orders WHERE id = 1;   -- the row is really deleted
ROLLBACK;                                                       -- and comes back
```

## На что смотреть в первую очередь

1. Узел с наибольшей долей времени или буферов.
2. Оценку `rows` против фактических строк.
3. `Rows Removed by Filter`: строки, прочитанные лишь затем, чтобы их выбросить.
4. Сортировки и хэши, ушедшие на диск.

## Ошибки новичка

- ❌ Смотреть только на итоговую стоимость → ✅ сравнивать оценки и фактические строки по узлам.
- ❌ `EXPLAIN ANALYZE` для `UPDATE` на рабочей базе → ✅ внутри `BEGIN ... ROLLBACK` или на копии.
- ❌ Измерять один раз, на холодную → ✅ несколько запусков; знать, был ли кэш прогрет.
- ❌ Смотреть план на десяти строках → ✅ реалистичные объёмы данных.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)

## Источники

- [PostgreSQL 17: использование EXPLAIN](https://www.postgresql.org/docs/17/using-explain.html)
- [PostgreSQL 17: EXPLAIN](https://www.postgresql.org/docs/17/sql-explain.html)
- [PostgreSQL 17: статистика планировщика](https://www.postgresql.org/docs/17/planner-stats.html)
