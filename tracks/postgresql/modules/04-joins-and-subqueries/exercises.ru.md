---
title: "Упражнения: соединения и подзапросы"
lang: ru
track: postgresql
kind: module
doc: exercises
slug: 04-joins-and-subqueries
part: 1
tags: [join, subquery, cte, lateral, set-operations]
versions: "PostgreSQL 17+"
status: verified
updated: 2026-11-26
---

# Упражнения: соединения и подзапросы

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Данные — из [базовой теории](basics.ru.md).

## 1. 🟢 Все клиенты с итогами

**Задача:** вывести каждого клиента с числом заказов и общей суммой; у клиентов без заказов должны быть нули.

<details><summary>Подсказка</summary>

Какое соединение сохранит Eve? Что `count` и `sum` вернут для строки из `NULL`?

</details>

<details><summary>Решение</summary>

```sql
SELECT c.name, count(o.id) AS orders, coalesce(sum(o.amount), 0) AS total
FROM customers c LEFT JOIN orders o ON o.customer_id = c.id
GROUP BY c.id, c.name
ORDER BY c.id;
-- Ann 2 120.00 | Bob 1 20.00 | Eve 0 0
```

`count(o.id)` пропускает строку из `NULL`, `coalesce` заменяет отсутствующую сумму.

</details>

## 2. 🟡 Самый крупный заказ каждого клиента

**Задача:** для каждого клиента вернуть id и сумму самого крупного заказа, сохранив клиентов без заказов.

<details><summary>Подсказка</summary>

Подзапрос в `FROM`, который видит текущего клиента.

</details>

<details><summary>Решение</summary>

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

Индекс по `(customer_id, amount DESC)` позволил бы каждому поиску останавливаться на первой строке.

</details>

## 3. 🔴 Выше среднего заказа

**Задача:** найти клиентов, у которых общая сумма больше средней суммы одного заказа.

<details><summary>Подсказка</summary>

Два скалярных подзапроса в `WHERE`. Что будет с клиентом без заказов?

</details>

<details><summary>Решение</summary>

```sql
SELECT name FROM customers c
WHERE (SELECT sum(amount) FROM orders o WHERE o.customer_id = c.id)
    > (SELECT avg(amount) FROM orders);
-- Ann
```

Для Eve сумма равна `NULL`, сравнение неизвестно, и строка отфильтровывается.

</details>
