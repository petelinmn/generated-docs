---
title: "Соединения и подзапросы: продвинутая теория"
lang: ru
track: postgresql
kind: module
doc: advanced
slug: 04-joins-and-subqueries
part: 1
tags: [join, subquery, cte, lateral, set-operations]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-26
---

# Соединения и подзапросы: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

## ON против WHERE во внешних соединениях

```sql
-- condition in ON: filters which orders match, every customer stays
SELECT c.name, o.amount FROM customers c
LEFT JOIN orders o ON o.customer_id = c.id AND o.amount > 60;
-- Ann 70.00 | Bob NULL | Eve NULL

-- condition in WHERE: filters the joined result, NULL rows disappear
SELECT c.name, o.amount FROM customers c
LEFT JOIN orders o ON o.customer_id = c.id
WHERE o.amount > 60;
-- Ann 70.00
```

`ON` решает, что считать парой; `WHERE` выполняется после соединения. Условие на правую таблицу в `WHERE` молча превращает `LEFT JOIN` во внутреннее соединение.

## NOT IN и NULL

```sql
SELECT name FROM customers WHERE id NOT IN (SELECT customer_id FROM orders);
-- (0 rows): order 13 has customer_id NULL
```

`x NOT IN (1, 2, NULL)` не может быть истинным: отличается ли `x` от неизвестного значения — неизвестно. Используйте `NOT EXISTS` или отфильтруйте `NULL` в подзапросе.

## Размножение строк

```sql
-- join order rows with two tags of Ann: every order is counted twice
SELECT sum(o.amount), count(*)
FROM customers c
JOIN orders o ON o.customer_id = c.id
JOIN (VALUES (1,'a'),(1,'b')) t(customer_id, tag) ON t.customer_id = c.id;
-- 240.00 | 4   (Ann's real total is 120.00)

-- aggregate each side first, then join
SELECT c.name, o.total, t.tags
FROM customers c
JOIN (SELECT customer_id, sum(amount) AS total FROM orders GROUP BY customer_id) o ON o.customer_id = c.id
JOIN (SELECT customer_id, count(*) AS tags
      FROM (VALUES (1,'a'),(1,'b')) v(customer_id, tag) GROUP BY customer_id) t ON t.customer_id = c.id;
-- Ann | 120.00 | 2
```

Каждая независимая сторона «многие» умножает остальные. Агрегируйте до соединения или берите отдельные подзапросы.

## LATERAL

```sql
-- the largest order of every customer
SELECT c.name, top.id, top.amount
FROM customers c
LEFT JOIN LATERAL (
    SELECT id, amount FROM orders o
    WHERE o.customer_id = c.id
    ORDER BY amount DESC LIMIT 1
) AS top ON true
ORDER BY c.id;
-- Ann 11 70.00 | Bob 12 20.00 | Eve NULL NULL
```

Без `LATERAL` подзапрос в `FROM` не видит `c.id`. `LEFT JOIN LATERAL ... ON true` сохраняет клиентов, для которых подзапрос ничего не вернул. Это обычный способ получить «первые N в каждой группе».

## Рекурсивный CTE

```sql
CREATE TABLE employees (id int PRIMARY KEY, name text, manager_id int);
INSERT INTO employees VALUES (1,'Root',NULL),(2,'Mid',1),(3,'Leaf',2);

WITH RECURSIVE chain AS (
    SELECT id, name, 1 AS depth FROM employees WHERE manager_id IS NULL
    UNION ALL
    SELECT e.id, e.name, c.depth + 1
    FROM employees e JOIN chain c ON e.manager_id = c.id
)
SELECT name, depth FROM chain ORDER BY depth;
-- Root 1 | Mid 2 | Leaf 3
```

Первая часть выполняется один раз; вторая — снова и снова над строками предыдущего шага, пока не вернёт пустой результат. На данных с циклом она бы не остановилась, поэтому в PostgreSQL есть предложение `CYCLE`:

```sql
WITH RECURSIVE g(id, next) AS (VALUES (1,2),(2,1)),
walk AS (
    SELECT id, next FROM g WHERE id = 1
    UNION ALL
    SELECT g.id, g.next FROM g JOIN walk w ON g.id = w.next
) CYCLE id SET is_cycle USING path
SELECT id, is_cycle, path FROM walk;
-- 1 f {(1)} | 2 f {(1),(2)} | 1 t {(1),(2),(1)}
```

`SEARCH DEPTH FIRST BY` и `SEARCH BREADTH FIRST BY` добавляют колонку для упорядочивания обхода.

## Операции над множествами

| Операция | Результат | Дубликаты |
|---|---|---|
| `UNION` | строки любого из запросов | убираются |
| `UNION ALL` | строки обоих запросов | остаются, и нет затрат на их удаление |
| `INTERSECT` | строки, которые есть в обоих | убираются |
| `EXCEPT` | строки первого, которых нет во втором | убираются |

`INTERSECT ALL` и `EXCEPT ALL` сохраняют дубликаты по количеству. Запросы должны возвращать одинаковое число колонок совместимых типов.

## Лучшие практики

- Указывайте псевдоним таблицы у каждой колонки.
- Для подзапросов предпочитайте `NOT EXISTS`, а не `NOT IN`.
- Берите `UNION ALL`, если дубликаты не обязательно убирать.
- Проверяйте число строк после каждого добавленного соединения.

## Частые ошибки

- ❌ `SELECT DISTINCT`, чтобы спрятать дубликаты от соединения → ✅ найти, какое соединение размножает строки.
- ❌ `count(*)` после `LEFT JOIN` → ✅ `count(o.id)`: `count(*)` считает и строку из `NULL`.
- ❌ Коррелированный подзапрос на каждую колонку одной таблицы → ✅ одно соединение или один `LATERAL`.

## Связанные материалы

- [Базовая теория](basics.ru.md)
- [Экспертная теория](expert.ru.md)

## Источники

- [PostgreSQL 17: табличные выражения и соединения](https://www.postgresql.org/docs/17/queries-table-expressions.html)
- [PostgreSQL 17: запросы WITH](https://www.postgresql.org/docs/17/queries-with.html)
- [PostgreSQL 17: выражения с подзапросами](https://www.postgresql.org/docs/17/functions-subquery.html)
