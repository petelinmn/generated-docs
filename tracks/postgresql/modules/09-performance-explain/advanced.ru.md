---
title: "Производительность и EXPLAIN: продвинутая теория"
lang: ru
track: postgresql
kind: module
doc: advanced
slug: 09-performance-explain
part: 2
tags: [explain, query-plan, statistics, vacuum, performance]
versions: "PostgreSQL 17+"
status: verified
updated: 2026-11-26
---

# Производительность и EXPLAIN: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Оценки берутся из статистики

```sql
CREATE TABLE fresh (id int, v int);
ALTER TABLE fresh SET (autovacuum_enabled = false);
INSERT INTO fresh SELECT i, i % 10 FROM generate_series(1, 100000) i;

EXPLAIN (ANALYZE, TIMING OFF, SUMMARY OFF) SELECT * FROM fresh WHERE v = 3;
-- Seq Scan on fresh  (cost=0.00..1694.48 rows=501 width=8) (actual rows=10000 loops=1)

ANALYZE fresh;
-- the same EXPLAIN:  (cost=0.00..1693.00 rows=10087 width=8) (actual rows=10000 loops=1)
```

До `ANALYZE` планировщик предположил 501 строку вместо 10 000. Autovacuum тоже выполняет `ANALYZE`, но после массовой загрузки лучше запустить его самому. Статистика собирается по выборке; `default_statistics_target` (100) задаёт её размер и длину списков частых значений и может быть повышен для колонки через `ALTER TABLE ... ALTER COLUMN ... SET STATISTICS`.

## Связанные колонки

```sql
-- every city belongs to exactly one country
EXPLAIN (ANALYZE, TIMING OFF, SUMMARY OFF)
SELECT * FROM orders WHERE city = 'city42' AND country = 'country4';
-- (cost=... rows=201 ...) (actual rows=2000 loops=1)   -- 207 in another run

CREATE STATISTICS orders_city_country (dependencies) ON city, country FROM orders;
ANALYZE orders;
-- the same EXPLAIN:  rows=2067 ... (actual rows=2000 loops=1)
```

Планировщик считает условия независимыми и перемножает их селективности, отсюда около 200 вместо 2000. `ANALYZE` читает случайную выборку, поэтому точная оценка меняется от запуска к запуску (здесь 201 и 207 в двух прогонах). Расширенная статистика сообщает ему о зависимости. Её виды — `dependencies`, `ndistinct` и `mcv`.

## Память: сортировки и хэши

```sql
SET work_mem = '64kB';
EXPLAIN (ANALYZE, TIMING OFF, SUMMARY OFF) SELECT * FROM orders ORDER BY amount;
-- Sort Method: external merge  Disk: 11408kB

SET work_mem = '64MB';
-- the same EXPLAIN:  Sort Method: quicksort  Memory: 18629kB
RESET work_mem;
```

```sql
SET enable_mergejoin = off;   -- only to show a hash join on this data
SET work_mem = '64kB';
EXPLAIN (ANALYZE, TIMING OFF, SUMMARY OFF, COSTS OFF)
SELECT count(*) FROM orders o JOIN orders p ON p.id = o.customer_id;
--   ->  Hash (actual rows=200000 loops=1)
--         Buckets: 4096  Batches: 128  Memory Usage: 89kB

SET work_mem = '64MB';
--         Buckets: 262144  Batches: 1  Memory Usage: 9080kB
```

`work_mem` (по умолчанию 4 МБ) — это предел **на одну операцию**: в одном запросе может быть несколько сортировок и хэшей, а сессий много одновременно. Хэш-операции могут использовать `work_mem × hash_mem_multiplier` (по умолчанию 2.0). Повышайте его для сессии или роли, которая строит отчёты, а не вслепую для всего сервера.

## Итерации

```sql
EXPLAIN (ANALYZE, TIMING OFF, SUMMARY OFF, COSTS OFF)
SELECT c.id, (SELECT count(*) FROM orders o WHERE o.customer_id = c.id)
FROM generate_series(1, 5) c(id);
```

```text
Function Scan on generate_series c (actual rows=5 loops=1)
  SubPlan 1
    ->  Aggregate (actual rows=1 loops=5)
          ->  Index Only Scan using orders_customer on orders o (actual rows=40 loops=5)
                Index Cond: (customer_id = c.id)
```

`actual rows=40 loops=5` значит 40 строк за одно выполнение, всего 200. Время в таких узлах тоже указано на одну итерацию.

## OFFSET читает то, что пропускает

```sql
EXPLAIN (ANALYZE, TIMING OFF, SUMMARY OFF, COSTS OFF)
SELECT * FROM orders ORDER BY id OFFSET 100000 LIMIT 10;
-- Limit (actual rows=10 loops=1)
--   ->  Index Scan using orders_pkey on orders (actual rows=100010 loops=1)

EXPLAIN (ANALYZE, TIMING OFF, SUMMARY OFF, COSTS OFF)
SELECT * FROM orders WHERE id > 100000 ORDER BY id LIMIT 10;
-- Limit (actual rows=10 loops=1)
--   ->  Index Scan using orders_pkey on orders (actual rows=10 loops=1)
--         Index Cond: (id > 100000)
```

Keyset-пагинация запоминает последний ключ и начинает после него.

## VACUUM и раздувание

```sql
CREATE TABLE b (id int PRIMARY KEY, v int);
ALTER TABLE b SET (autovacuum_enabled = false);
INSERT INTO b SELECT i, 0 FROM generate_series(1, 100000) i;
VACUUM ANALYZE b;                                   -- size 3544 kB

UPDATE b SET v = 1;
SELECT pg_stat_force_next_flush();
SELECT n_dead_tup FROM pg_stat_user_tables WHERE relname = 'b';   -- 100000
                                                    -- size 7080 kB
VACUUM b;
UPDATE b SET v = 2;                                 -- still 7080 kB: freed space is reused
VACUUM FULL b;                                      -- 3544 kB: the table is rewritten
```

`UPDATE` записывает новую версию строки, старая становится мёртвой. `VACUUM` помечает место свободным для новых версий, но обычно не уменьшает файл. `VACUUM FULL` переписывает таблицу и всё это время держит блокировку `ACCESS EXCLUSIVE`, поэтому запускать его походя нельзя.

Autovacuum очищает таблицу, когда мёртвых строк больше, чем `autovacuum_vacuum_threshold + autovacuum_vacuum_scale_factor × строк` (по умолчанию 50 + 0,2 × строк), и анализирует её после 50 + 0,1 × строк изменённых строк. Для таблицы в сто миллионов строк это двадцать миллионов мёртвых строк; большим часто изменяемым таблицам задают собственные, меньшие значения.

## Лучшие практики

- Меняйте одно за раз и измеряйте до и после.
- Запускайте `ANALYZE` после массовых загрузок и больших удалений.
- Настраивайте autovacuum отдельно для больших таблиц с частыми обновлениями.
- Следите за долгими транзакциями: они мешают `VACUUM` убирать старые версии.

## Частые ошибки

- ❌ Глобальный `work_mem` в гигабайты → ✅ на роль или на сессию.
- ❌ `VACUUM FULL` по расписанию → ✅ обычный `VACUUM` и поиск причины раздувания.
- ❌ Отключить autovacuum «ради скорости» → ✅ он защищает от раздувания и переполнения счётчика транзакций.

## Связанные материалы

- [Базовая теория](basics.ru.md)
- [Экспертная теория](expert.ru.md)

## Источники

- [PostgreSQL 17: использование EXPLAIN](https://www.postgresql.org/docs/17/using-explain.html)
- [PostgreSQL 17: EXPLAIN](https://www.postgresql.org/docs/17/sql-explain.html)
- [PostgreSQL 17: статистика планировщика](https://www.postgresql.org/docs/17/planner-stats.html)
- [PostgreSQL 17: потребление ресурсов](https://www.postgresql.org/docs/17/runtime-config-resource.html)
- [PostgreSQL 17: регулярная очистка](https://www.postgresql.org/docs/17/routine-vacuuming.html)
- [PostgreSQL 17: настройки autovacuum](https://www.postgresql.org/docs/17/runtime-config-autovacuum.html)
