---
title: "Соединения и подзапросы: базовая теория"
lang: ru
track: postgresql
kind: module
doc: basics
slug: 04-joins-and-subqueries
part: 1
tags: [join, subquery, cte, lateral, set-operations]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-26
---

# Соединения и подзапросы: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая** · [2. Продвинутая](advanced.ru.md) · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** `SELECT` и `WHERE` · **Время:** ~2 ч

## Данные для примеров

Все результаты модуля получены на PostgreSQL 17.11 с этими данными. У заказа 13 намеренно нет клиента, а у Eve нет заказов.

```sql
CREATE TABLE customers (id int PRIMARY KEY, name text NOT NULL);
CREATE TABLE orders (id int PRIMARY KEY, customer_id int, amount numeric(10,2) NOT NULL);
INSERT INTO customers VALUES (1,'Ann'),(2,'Bob'),(3,'Eve');
INSERT INTO orders VALUES (10,1,50),(11,1,70),(12,2,20),(13,NULL,5);
```

## Типы соединений

| Соединение | Что остаётся в результате |
|---|---|
| `JOIN` (`INNER JOIN`) | только пары, удовлетворяющие условию |
| `LEFT JOIN` | пары и все левые строки без пары, дополненные `NULL` |
| `RIGHT JOIN` | то же со стороны правой таблицы |
| `FULL JOIN` | пары и строки без пары с обеих сторон |
| `CROSS JOIN` | каждая левая строка с каждой правой, без условия |

```sql
SELECT c.name, o.amount FROM customers c JOIN orders o ON o.customer_id = c.id ORDER BY o.id;
-- Ann 50.00 | Ann 70.00 | Bob 20.00

SELECT c.name, o.amount FROM customers c LEFT JOIN orders o ON o.customer_id = c.id ORDER BY c.id, o.id;
-- Ann 50.00 | Ann 70.00 | Bob 20.00 | Eve NULL
```

`FULL JOIN` показывает сразу оба вида «одиночек»:

```sql
SELECT c.name, o.id AS order_id
FROM customers c FULL JOIN orders o ON o.customer_id = c.id
ORDER BY c.id, o.id;
-- Ann 10 | Ann 11 | Bob 12 | Eve NULL | NULL 13
```

`CROSS JOIN` 3 клиентов и 4 заказов даёт 12 строк.

## ON, USING и псевдонимы таблиц

`ON` принимает любое логическое условие. `USING (col)` — сокращение для равенства одноимённых колонок, и такая колонка выводится один раз. Короткие псевдонимы (`customers c`) делают условие читаемым и обязательны, когда таблица соединяется сама с собой:

```sql
SELECT e.name, m.name AS manager
FROM employees e LEFT JOIN employees m ON m.id = e.manager_id;
```

## Есть ли связанная строка

```sql
-- customers that have orders: Ann, Bob (each once)
SELECT name FROM customers c
WHERE EXISTS (SELECT 1 FROM orders o WHERE o.customer_id = c.id);

-- customers without orders: Eve
SELECT name FROM customers c
WHERE NOT EXISTS (SELECT 1 FROM orders o WHERE o.customer_id = c.id);
```

Соединение вернуло бы Ann дважды, по разу на заказ; `EXISTS` возвращает её один раз.

## WITH: имя для шага

```sql
WITH totals AS (
    SELECT customer_id, sum(amount) AS total FROM orders GROUP BY customer_id
)
SELECT c.name, t.total
FROM customers c JOIN totals t ON t.customer_id = c.id
WHERE t.total > 30;
-- Ann 120.00
```

## Ошибки новичка

- ❌ Забыть условие соединения → ✅ писать `ON`; случайное декартово произведение перемножает размеры.
- ❌ `INNER JOIN` в отчёте «все клиенты» → ✅ `LEFT JOIN`, иначе клиенты без заказов исчезнут.
- ❌ `= NULL` для поиска строк без пары → ✅ `IS NULL` или `NOT EXISTS`.
- ❌ Полагаться на порядок строк соединения → ✅ добавить `ORDER BY`.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)

## Источники

- [PostgreSQL 17: табличные выражения и соединения](https://www.postgresql.org/docs/17/queries-table-expressions.html)
- [PostgreSQL 17: запросы WITH](https://www.postgresql.org/docs/17/queries-with.html)
- [PostgreSQL 17: выражения с подзапросами](https://www.postgresql.org/docs/17/functions-subquery.html)
