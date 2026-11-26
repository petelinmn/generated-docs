---
title: "Соединения и подзапросы: экспертная теория"
lang: ru
track: postgresql
kind: module
doc: expert
slug: 04-joins-and-subqueries
part: 1
tags: [join, subquery, cte, lateral, set-operations]
versions: "PostgreSQL 17+"
status: verified
updated: 2026-11-26
---

# Соединения и подзапросы: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

## Под капотом

SQL говорит, какие строки нужны; алгоритм и порядок соединения выбирает планировщик. В PostgreSQL три стратегии соединения.

| Алгоритм | Как работает | Когда подходит | Условие |
|---|---|---|---|
| Nested loop | для каждой внешней строки ищет пары во внутренней стороне | внешняя сторона мала, у внутренней есть индекс | любое |
| Hash join | строит хэш-таблицу по одной стороне и проверяет по ней другую | большие неотсортированные входы | только равенство |
| Merge join | идёт по двум отсортированным входам одновременно | входы уже отсортированы по ключу или велики | операторы, пригодные для слияния (вида равенства) |

`EXISTS` и `NOT EXISTS` обычно превращаются в semi-join и anti-join, которые останавливаются на первой паре. `NOT IN` не может стать anti-join из-за своих правил для `NULL`, и планы различаются:

```sql
EXPLAIN (COSTS OFF)
SELECT * FROM customers c WHERE NOT EXISTS (SELECT 1 FROM orders o WHERE o.customer_id = c.id);
```

```text
Hash Right Anti Join
  Hash Cond: (o.customer_id = c.id)
  ->  Seq Scan on orders o
  ->  Hash
        ->  Seq Scan on customers c
```

```sql
EXPLAIN (COSTS OFF)
SELECT * FROM customers c WHERE id NOT IN (SELECT customer_id FROM orders);
```

```text
Seq Scan on customers c
  Filter: (NOT (ANY (id = (hashed SubPlan 1).col1)))
  SubPlan 1
    ->  Seq Scan on orders
```

Конкретный план зависит от статистики и размеров: читайте его через `EXPLAIN`, а не угадывайте.

### Порядок соединения

Число возможных порядков растёт очень быстро с числом таблиц. `from_collapse_limit` и `join_collapse_limit` (оба по умолчанию 8) ограничивают, сколько элементов планировщик переставляет вместе; начиная с `geqo_threshold` элементов (по умолчанию 12) вместо полного перебора работает генетический оптимизатор. При `join_collapse_limit = 1` явные `JOIN` выполняются в написанном порядке.

### CTE: встраивается или материализуется

Нерекурсивный запрос `WITH` без побочных эффектов, на который ссылаются один раз, по умолчанию встраивается в родительский запрос, и условия могут проталкиваться внутрь. Запрос, на который ссылаются несколько раз, вычисляется отдельно. `AS MATERIALIZED` заставляет вычислять отдельно, `AS NOT MATERIALIZED` — встраивать.

### Изменяющий WITH

Внутри `WITH` допустимы `INSERT`, `UPDATE`, `DELETE` и `MERGE`. Они выполняются ровно один раз и до конца, даже если внешний запрос не читает их результат. Все части оператора видят один снимок, поэтому изменения друг друга видны только через `RETURNING`:

```sql
WITH gone AS (DELETE FROM orders WHERE customer_id IS NULL RETURNING *)
SELECT (SELECT count(*) FROM gone)   AS deleted,
       (SELECT count(*) FROM orders) AS visible_in_same_statement;
-- 1 | 4   (the next statement already sees 3 rows)
```

## Справочник

| Конструкция | Смысл |
|---|---|
| `a JOIN b ON cond` | пары, удовлетворяющие `cond` |
| `a LEFT/RIGHT/FULL [OUTER] JOIN b ON cond` | сохраняет строки без пары названной стороны |
| `a CROSS JOIN b` | декартово произведение, то же что `a, b` |
| `USING (c1, c2)` | равенство одноимённых колонок, каждая выводится один раз |
| `NATURAL JOIN` | `USING` по всем одноимённым колонкам; если их нет, ведёт себя как `CROSS JOIN` |
| `LATERAL` | элемент может ссылаться на предыдущие элементы `FROM` |

| Выражение с подзапросом | Результат | На пустом наборе |
|---|---|---|
| `EXISTS (q)` | true, если `q` вернул строку | false |
| `x IN (q)` / `x = ANY (q)` | true, если равно какому-то значению | false |
| `x NOT IN (q)` / `x <> ALL (q)` | true, если отличается от каждого значения | true |
| `x op ANY (q)` (`SOME`) | true, если верно хотя бы для одного | false |
| `x op ALL (q)` | true, если верно для всех | true |
| `(row) = (q)` | сравнение строки с запросом из одной строки | NULL |
| скалярный `(q)` | единственное значение; больше одной строки — ошибка | NULL |

| Возможность CTE | Синтаксис |
|---|---|
| Рекурсия | `WITH RECURSIVE t AS (base UNION [ALL] step)` |
| Управление планировщиком | `AS [NOT] MATERIALIZED (...)` |
| Порядок обхода | `SEARCH {DEPTH \| BREADTH} FIRST BY col SET ordercol` |
| Защита от циклов | `CYCLE col SET flag USING path` |

`UNION` (без `ALL`) в рекурсивном запросе отбрасывает уже виденные строки, что тоже останавливает некоторые циклы.

## Подвохи

### Скалярный подзапрос с несколькими строками падает только при выполнении

```sql
SELECT (SELECT amount FROM orders WHERE id = 999);          -- NULL
SELECT (SELECT amount FROM orders WHERE customer_id = 1);
-- ERROR:  more than one row returned by a subquery used as an expression
```

Запрос работает, пока у каждого клиента один заказ, и ломается, когда появляется второй.

### ALL над пустым набором истинно

```sql
SELECT 5 > ALL (SELECT amount FROM orders WHERE false),   -- t
       5 > ANY (SELECT amount FROM orders WHERE false);   -- f
```

«Цена выше, чем у всех конкурентов» истинно, когда конкурентов нет.

### NULL никогда не соединяется с NULL

```sql
SELECT count(*) FROM (VALUES (NULL::int)) a(x) JOIN (VALUES (NULL::int)) b(x) USING (x);             -- 0
SELECT count(*) FROM (VALUES (NULL::int)) a(x) JOIN (VALUES (NULL::int)) b(x)
    ON a.x IS NOT DISTINCT FROM b.x;                                                                 -- 1
```

`IS NOT DISTINCT FROM` считает два `NULL` равными, но это не обычный оператор равенства для алгоритмов соединения, поэтому на больших таблицах проверьте план.

### NATURAL JOIN выбирает колонки по имени

```sql
-- both tables have a column "id", so NATURAL compares customers.id with orders.id
SELECT count(*) FROM customers NATURAL JOIN orders;   -- 0
```

Добавленная позже в обе таблицы колонка вроде `created_at` изменит результат старого запроса без всякой ошибки.

## Компромиссы и альтернативы

| Выбор | Плюс | Минус |
|---|---|---|
| Соединение | колонки обеих таблиц, свобода планировщика | дубликаты при связи «один ко многим» |
| `EXISTS` | нет дубликатов, останавливается на первой паре | нет колонок внутренней таблицы |
| `LATERAL` | первые N в группе, переиспользование выражения | обычно выполняется построчно; нужен индекс |
| Оконная функция | ранги внутри групп за один проход | читает все строки групп |
| `AS MATERIALIZED` | дорогой шаг выполняется один раз | условия не проталкиваются внутрь |
| Денормализация | нет соединения при чтении | копии нужно согласовывать |

## Вопросы с собеседований

**1. Чем `INNER JOIN` отличается от `LEFT JOIN`?**

<details><summary>Ответ</summary>

`LEFT JOIN` сохраняет и левые строки без пары, с `NULL` в правых колонках.

</details>

**2. Куда писать фильтр по правой таблице `LEFT JOIN`?**

<details><summary>Ответ</summary>

В `ON`, чтобы ограничить пары; в `WHERE` — только если строки без пары нужно отбросить.

</details>

**3. Почему `NOT IN` может не вернуть ничего?**

<details><summary>Ответ</summary>

`NULL` в подзапросе делает сравнение неизвестным для любого несовпавшего значения.

</details>

**4. Соединение или `EXISTS`?**

<details><summary>Ответ</summary>

`EXISTS`, когда важно только наличие: без дубликатов и когда колонки внутренней стороны не нужны.

</details>

**5. Назовите алгоритмы соединения.**

<details><summary>Ответ</summary>

Nested loop, hash join, merge join. Hash join требует условия равенства.

</details>

**6. CTE всегда является барьером оптимизации?**

<details><summary>Ответ</summary>

Нет. Нерекурсивный CTE без побочных эффектов с одной ссылкой по умолчанию встраивается; `MATERIALIZED` возвращает отдельное вычисление.

</details>

**7. Как взять три последних заказа каждого клиента?**

<details><summary>Ответ</summary>

`LEFT JOIN LATERAL (... ORDER BY ... LIMIT 3) ON true` или `row_number()` по разделам с фильтром по нему.

</details>

**8. Как завершается рекурсивный CTE?**

<details><summary>Ответ</summary>

Когда рекурсивная часть не возвращает новых строк. Для циклов нужны `CYCLE`, проверка пути или дедупликация через `UNION`.

</details>

**9. `UNION` против `UNION ALL`?**

<details><summary>Ответ</summary>

`UNION` убирает дубликаты, что стоит дополнительной работы.

</details>

**10. Подвох: `LEFT JOIN` вернул меньше строк, чем ожидалось. Как это возможно?**

<details><summary>Ответ</summary>

Условие на правую таблицу в `WHERE` убрало строки, дополненные `NULL`.

</details>

**11. Подвох: после добавления второго соединения `sum` удвоилась. Почему?**

<details><summary>Ответ</summary>

Две связи «один ко многим» умножают друг друга. Агрегируйте каждую до соединения.

</details>

**12. Может ли `DELETE` в `WITH` не выполниться, если его результат не используется?**

<details><summary>Ответ</summary>

Нет. Изменяющие операторы в `WITH` выполняются ровно один раз и до конца.

</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [Квиз](quiz.ru.md) и [упражнения](exercises.ru.md)

## Источники

- [PostgreSQL 17: табличные выражения и соединения](https://www.postgresql.org/docs/17/queries-table-expressions.html)
- [PostgreSQL 17: запросы WITH](https://www.postgresql.org/docs/17/queries-with.html)
- [PostgreSQL 17: выражения с подзапросами](https://www.postgresql.org/docs/17/functions-subquery.html)
- [PostgreSQL 17: планировщик и стратегии соединения](https://www.postgresql.org/docs/17/planner-optimizer.html)
- [PostgreSQL 17: настройки планирования запросов](https://www.postgresql.org/docs/17/runtime-config-query.html)
