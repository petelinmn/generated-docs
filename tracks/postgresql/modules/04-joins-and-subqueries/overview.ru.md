---
title: "Соединения и подзапросы"
lang: ru
track: postgresql
kind: module
doc: overview
slug: 04-joins-and-subqueries
part: 1
levels: [1, 2, 3, 4]
tags: [join, subquery, cte, lateral, set-operations]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-26
---

# Соединения и подзапросы

🌐 **Русский** · [English](overview.en.md)

## Коротко

Реляционная база хранит факты в отдельных таблицах и собирает их обратно в запросе. Соединения (joins) объединяют строки двух таблиц по условию, подзапросы используют результат одного запроса внутри другого, а `WITH` (обобщённое табличное выражение, common table expression, CTE) даёт части запроса имя. Большинство отчётных и прикладных запросов строятся из этих трёх инструментов, а большинство сюрпризов — от `NULL` и размножающихся строк.

## Ключевые идеи

- Тип соединения отвечает на один вопрос: что делать со строками без пары.
- Во внешнем соединении условие в `ON` и то же условие в `WHERE` дают разный результат.
- `EXISTS` и `NOT EXISTS` спрашивают «есть ли хотя бы одна строка» и никогда не дублируют внешнюю строку.
- `NOT IN` с `NULL` в списке не возвращает ничего.
- Соединение сразу двух связей «один ко многим» размножает строки и ломает суммы.
- `LATERAL` позволяет подзапросу в `FROM` ссылаться на текущую строку предыдущей таблицы.
- Рекурсивный CTE обходит деревья и графы.

## Минимальный пример

```sql
CREATE TABLE customers (id int PRIMARY KEY, name text NOT NULL);
CREATE TABLE orders (id int PRIMARY KEY, customer_id int, amount numeric(10,2) NOT NULL);
INSERT INTO customers VALUES (1,'Ann'),(2,'Bob'),(3,'Eve');
INSERT INTO orders VALUES (10,1,50),(11,1,70),(12,2,20),(13,NULL,5);
```

```sql
SELECT c.name, o.amount FROM customers c JOIN orders o ON o.customer_id = c.id ORDER BY o.id;
-- Ann 50.00 | Ann 70.00 | Bob 20.00

SELECT c.name, o.amount FROM customers c LEFT JOIN orders o ON o.customer_id = c.id ORDER BY c.id, o.id;
-- Ann 50.00 | Ann 70.00 | Bob 20.00 | Eve NULL
```

## Когда применять, а когда нет

- ✅ Объединение связанных таблиц, отчёты, поиск строк без пары.
- ✅ Именование шагов длинного запроса через `WITH`.
- ❌ Соединение только ради проверки существования: оно дублирует строки, нужен `EXISTS`.
- ❌ `NATURAL JOIN` в коде приложения: условие молча меняется вместе со схемой.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | типы соединений, `EXISTS`, `WITH` |
| [2. Продвинутая теория](advanced.ru.md) | `ON` и `WHERE`, ловушки `NULL`, размножение строк, `LATERAL`, рекурсия |
| [3. Экспертная теория](expert.ru.md) | алгоритмы соединения и планы, справочник, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | ещё не создан — `/learn postgresql 04-joins-and-subqueries level 5` |
| 6. Тесты | ещё не создан — `/learn postgresql 04-joins-and-subqueries level 6` |

## Связанные материалы

- Пререквизиты: [02 · SQL-запросы](../02-sql-queries/overview.ru.md), [03 · Проектирование схемы](../03-data-modeling-ddl/overview.ru.md)
- Дальше: следующий модуль по [дорожной карте PostgreSQL](../../overview.ru.md)
- То же в других треках: [LINQ в C#](../../../csharp/modules/07-linq/overview.ru.md)
- Где это важно при проектировании: [хранение данных](../../../system-design/modules/04-data-storage/overview.ru.md)
