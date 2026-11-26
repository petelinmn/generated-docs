---
title: "Упражнения: производительность и EXPLAIN"
lang: ru
track: postgresql
kind: module
doc: exercises
slug: 09-performance-explain
part: 2
tags: [explain, query-plan, statistics, vacuum, performance]
versions: "PostgreSQL 17+"
status: verified
updated: 2026-11-26
---

# Упражнения: производительность и EXPLAIN

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Таблица — из [базовой теории](basics.ru.md), каждый ответ проверяй через `EXPLAIN (ANALYZE, BUFFERS)`.

## 1. 🟢 Сначала новые заказы

**Задача:** «десять самых старых новых заказов» читает 500 строк, чтобы вернуть 10. Сделайте, чтобы читалось около 10.

<details><summary>Подсказка</summary>

Посмотрите на `Rows Removed by Filter`. Какие строки должен содержать индекс?

</details>

<details><summary>Решение</summary>

```sql
CREATE INDEX orders_status_id ON orders (id) WHERE status = 'new';
-- or a composite index (status, id)
EXPLAIN (ANALYZE, TIMING OFF, SUMMARY OFF, COSTS OFF)
SELECT * FROM orders WHERE status = 'new' ORDER BY id LIMIT 10;
```

План становится чтением частичного индекса без фильтра.

</details>

## 2. 🟡 Быстрые далёкие страницы

**Задача:** листание по 10 строк не должно замедляться на странице 10 000.

<details><summary>Подсказка</summary>

Начинайте после последней строки, которую видел клиент, вместо отсчёта пропускаемых строк.

</details>

<details><summary>Решение</summary>

```sql
-- first page
SELECT id, amount FROM orders ORDER BY id LIMIT 10;
-- next page: the client sends the last id it saw
SELECT id, amount FROM orders WHERE id > 10 ORDER BY id LIMIT 10;
```

Перейти к произвольному номеру страницы так нельзя, зато «следующая страница» везде стоит одинаково.

</details>

## 3. 🔴 Где отстаёт autovacuum

**Задача:** вывести таблицы с наибольшим числом мёртвых строк и время их последней очистки и анализа.

<details><summary>Подсказка</summary>

Статистика по таблицам собрана в одном представлении.

</details>

<details><summary>Решение</summary>

```sql
SELECT relname, n_live_tup, n_dead_tup, last_autovacuum, last_autoanalyze
FROM pg_stat_user_tables
ORDER BY n_dead_tup DESC
LIMIT 10;
```

`n_live_tup` и `n_dead_tup` — оценки системы статистики и могут немного отставать.

</details>
