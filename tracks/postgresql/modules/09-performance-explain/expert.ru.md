---
title: "Производительность и EXPLAIN: экспертная теория"
lang: ru
track: postgresql
kind: module
doc: expert
slug: 09-performance-explain
part: 2
tags: [explain, query-plan, statistics, vacuum, performance]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-26
---

# Производительность и EXPLAIN: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

## Под капотом

Планировщик перебирает способы доступа и порядки соединения, оценивает стоимость каждого по статистике (`pg_statistic`, читается через `pg_stats`) и настройкам стоимости и оставляет самый дешёвый. Селективность берётся из списков частых значений, гистограмм и числа различных значений; `correlation` колонки с физическим порядком влияет на стоимость индексного чтения.

```sql
SELECT attname, n_distinct, correlation FROM pg_stats
WHERE tablename = 'orders' AND attname IN ('id', 'status', 'city');
-- city    100  0.017
-- id       -1  1
-- status    2  0.96
```

Отрицательный `n_distinct` — это доля строк: `-1` значит, что все значения уникальны.

### Подготовленные запросы и общие планы

```sql
CREATE TABLE skew (id int PRIMARY KEY, kind text);
INSERT INTO skew
SELECT i, CASE WHEN i <= 99000 THEN 'common' ELSE 'rare' || (i % 10) END
FROM generate_series(1, 100000) i;
CREATE INDEX skew_kind ON skew (kind);
ANALYZE skew;

PREPARE q(text) AS SELECT count(*) FROM skew WHERE kind = $1;
EXPLAIN (COSTS OFF) EXECUTE q('rare1');    -- Index Only Scan, Index Cond: (kind = 'rare1')
EXPLAIN (COSTS OFF) EXECUTE q('common');   -- Seq Scan, Filter: (kind = 'common')

SET plan_cache_mode = force_generic_plan;
EXPLAIN (COSTS OFF) EXECUTE q('common');   -- Index Only Scan, Index Cond: (kind = $1)
```

При `plan_cache_mode = auto` (по умолчанию) первые пять выполнений получают собственные планы под фактические значения; затем используется общий план, если его оценка не намного хуже средней собственной. На неравномерных данных общий план может плохо подходить одному из значений; для такого запроса помогает `force_custom_plan`.

### pg_stat_statements

```sql
-- server started with shared_preload_libraries = 'pg_stat_statements'
CREATE EXTENSION pg_stat_statements;
SELECT count(*) FROM orders WHERE customer_id = 1;
SELECT count(*) FROM orders WHERE customer_id = 2;

SELECT query, calls FROM pg_stat_statements
WHERE query LIKE 'SELECT count(*) FROM orders%';
-- SELECT count(*) FROM orders WHERE customer_id = $1 | 2

-- the usual question: what takes the most time overall
SELECT calls, round(total_exec_time) AS total_ms, round(mean_exec_time, 2) AS mean_ms, query
FROM pg_stat_statements ORDER BY total_exec_time DESC LIMIT 10;
```

Расширение должно быть в `shared_preload_libraries`. Запросы, различающиеся только константами, учитываются вместе.

### Настройки в плане

```sql
SET random_page_cost = 1.1;
EXPLAIN (COSTS OFF, SETTINGS) SELECT * FROM orders WHERE customer_id = 7;
-- Index Scan using orders_customer on orders
--   Index Cond: (customer_id = 7)
-- Settings: max_parallel_workers_per_gather = '0', random_page_cost = '1.1'
```

`SETTINGS` перечисляет настройки планировщика, отличные от значений по умолчанию: полезно, когда план различается между серверами.

## Справочник

| Параметр `EXPLAIN` | Что добавляет |
|---|---|
| `ANALYZE` | выполнение, фактические строки, итерации, время |
| `BUFFERS` | страницы: найдены, прочитаны, изменены, записаны |
| `TIMING` | время по узлам (с `ANALYZE`) |
| `SUMMARY` | время планирования и выполнения |
| `VERBOSE` | выходные колонки, имена со схемой |
| `SETTINGS` | изменённые настройки планировщика |
| `WAL` | записи и байты WAL (с `ANALYZE`) |
| `SERIALIZE` | стоимость преобразования результата (с `ANALYZE`) |
| `MEMORY` | память планировщика |
| `GENERIC_PLAN` | план с параметрами `$n`, без `ANALYZE` |
| `FORMAT TEXT \| JSON \| XML \| YAML` | формат вывода |

| Строка плана | Смысл |
|---|---|
| `Rows Removed by Filter` | прочитано и выброшено |
| `Heap Fetches` | обращения index-only scan к таблице |
| `Sort Method: external merge  Disk:` | сортировка ушла на диск |
| `Batches: N` (> 1) | хэш разбит из-за памяти |
| `Heap Blocks: lossy` | битовая карта не поместилась в `work_mem` |
| `Workers Launched` | сколько параллельных процессов реально запущено |

| Настройка | По умолчанию | Влияние |
|---|---|---|
| `work_mem` | 4MB | память на сортировку или хэш |
| `hash_mem_multiplier` | 2.0 | дополнительная память для хэшей |
| `shared_buffers` | 128MB (обычно) | кэш страниц PostgreSQL |
| `effective_cache_size` | 4GB | предполагаемый общий кэш, только для оценок |
| `random_page_cost` / `seq_page_cost` | 4.0 / 1.0 | относительная стоимость чтения страниц |
| `default_statistics_target` | 100 | детальность статистики |
| `plan_cache_mode` | `auto` | собственные или общие планы |
| `autovacuum_vacuum_threshold` / `_scale_factor` | 50 / 0.2 | когда очищать |
| `autovacuum_analyze_threshold` / `_scale_factor` | 50 / 0.1 | когда анализировать |
| `track_io_timing` | off | время ввода-вывода в планах и статистике |

На SSD `random_page_cost` часто снижают к 1.1; проверяйте эффект на настоящих запросах.

| Представление для диагностики | Что показывает |
|---|---|
| `pg_stat_statements` | итоги по нормализованным запросам |
| `pg_stat_user_tables` | последовательные и индексные чтения, мёртвые строки, последние (auto)vacuum и analyze |
| `pg_stat_user_indexes` | использование индексов |
| `pg_stat_activity` | что выполняется сейчас и чего ждёт |
| `pg_stats` | статистика колонок |

## Подвохи

### Редкий фильтр с LIMIT

```sql
EXPLAIN (ANALYZE, TIMING OFF, SUMMARY OFF, COSTS OFF)
SELECT * FROM orders WHERE status = 'new' ORDER BY id LIMIT 10;
-- Limit (actual rows=10 loops=1)
--   ->  Index Scan using orders_pkey on orders (actual rows=10 loops=1)
--         Filter: (status = 'new'::text)
--         Rows Removed by Filter: 490
```

Здесь подходят 2% строк, поэтому ради 10 прочитано 500. Если совпадения редки или все в конце индекса, тот же план прочитает почти всю таблицу. Помогает индекс по отфильтрованным строкам.

### Приведение типа колонки отключает индекс

```sql
CREATE TABLE codes (code text PRIMARY KEY);
INSERT INTO codes SELECT i::text FROM generate_series(1, 100000) i;
ANALYZE codes;

EXPLAIN (COSTS OFF) SELECT * FROM codes WHERE code = '42';      -- Index Only Scan using codes_pkey
EXPLAIN (COSTS OFF) SELECT * FROM codes WHERE code::int = 42;   -- Seq Scan, Filter: ((code)::integer = 42)
```

Сравнивайте со значением типа колонки, а не колонку, приведённую к типу параметра. Тот же эффект дают несовпадающие типы от драйвера приложения.

### Hash join, разбитый на части

`Batches: 128` в примере про память означает, что хэш-таблица записывалась на диск 128 частями.

## Компромиссы и альтернативы

| Средство | Плюс | Минус |
|---|---|---|
| Индекс | большое ускорение запроса | медленнее запись, место |
| Переписать запрос | без новых объектов | нужно понимать план |
| `ANALYZE`, расширенная статистика | лучше оценки везде | ещё один объект в сопровождении |
| Больше `work_mem` | без ухода на диск | риск нехватки памяти |
| Материализованное представление или сводная таблица | быстрые отчёты | устаревание |
| Секционирование | отсечение и дешёвое удаление старых данных | сложнее схема |
| Оборудование | немного помогает всему | плохой план не исправит |

## Вопросы с собеседований

**1. Что означают два числа в `cost=`?**

<details><summary>Ответ</summary>

Стоимость до первой строки и полная стоимость узла, в единицах планировщика.

</details>

**2. Чем `EXPLAIN` отличается от `EXPLAIN ANALYZE`?**

<details><summary>Ответ</summary>

Первый только планирует; второй выполняет и добавляет фактические строки, итерации и время.

</details>

**3. Оценка 200 строк, фактически 2 000 000. Что делать?**

<details><summary>Ответ</summary>

Выполнить `ANALYZE`; проверить связанные колонки (расширенная статистика), размер статистики, функции над колонками.

</details>

**4. Что означает `Sort Method: external merge`?**

<details><summary>Ответ</summary>

Сортировка не поместилась в `work_mem` и использовала временные файлы.

</details>

**5. `work_mem` выделяется на запрос?**

<details><summary>Ответ</summary>

Нет, на операцию сортировки или хэширования; запрос может использовать его несколько раз.

</details>

**6. Как найти запросы, которые стоит оптимизировать?**

<details><summary>Ответ</summary>

`pg_stat_statements`, упорядоченный по суммарному времени выполнения.

</details>

**7. `VACUUM` против `VACUUM FULL`?**

<details><summary>Ответ</summary>

`VACUUM` освобождает место для повторного использования, не закрывая таблицу; `VACUUM FULL` переписывает её под эксклюзивной блокировкой и возвращает место системе.

</details>

**8. Когда autovacuum начинает обработку таблицы?**

<details><summary>Ответ</summary>

Когда мёртвых строк больше порога + коэффициент × строк, по умолчанию 50 + 0,2 × строк.

</details>

**9. Что такое общий (generic) план?**

<details><summary>Ответ</summary>

План подготовленного запроса без учёта значений параметров. После пяти собственных планов сервер может перейти на него, если в среднем он не дороже.

</details>

**10. Подвох: `actual rows=10 loops=1000`. Сколько строк?**

<details><summary>Ответ</summary>

10 000: значения указаны на одну итерацию.

</details>

**11. Подвох: `ORDER BY id LIMIT 10` с фильтром медленный, хотя есть индекс по `id`. Почему?**

<details><summary>Ответ</summary>

Индекс идёт по порядку `id` и выбрасывает строки, пока не найдётся десять подходящих; при редких совпадениях читает очень много.

</details>

**12. Что изменилось в `EXPLAIN` в PostgreSQL 18?**

<details><summary>Ответ</summary>

`EXPLAIN ANALYZE` автоматически включает `BUFFERS`.

</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [Квиз](quiz.ru.md) и [упражнения](exercises.ru.md)

## Источники

- [PostgreSQL 17: использование EXPLAIN](https://www.postgresql.org/docs/17/using-explain.html)
- [PostgreSQL 17: EXPLAIN](https://www.postgresql.org/docs/17/sql-explain.html)
- [PostgreSQL 17: статистика планировщика](https://www.postgresql.org/docs/17/planner-stats.html)
- [PostgreSQL 17: настройки планирования](https://www.postgresql.org/docs/17/runtime-config-query.html)
- [PostgreSQL 17: PREPARE](https://www.postgresql.org/docs/17/sql-prepare.html)
- [PostgreSQL 17: pg_stat_statements](https://www.postgresql.org/docs/17/pgstatstatements.html)
- [PostgreSQL 18: список изменений](https://www.postgresql.org/docs/18/release-18.html)
