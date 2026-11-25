---
title: "SQL-запросы: экспертная теория"
lang: ru
track: postgresql
kind: module
doc: expert
slug: 02-sql-queries
part: 1
tags: [query-lifecycle, planner, logical-order, null, window-functions]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-25
---

# SQL-запросы: экспертная теория

🌐 **Русский** · [English](expert.en.md)

## Логический порядок

Практически запрос читают в порядке `FROM`, `WHERE`, `GROUP BY`, `HAVING`, `SELECT`, `DISTINCT`, `ORDER BY`, `LIMIT`. Это не гарантирует физический порядок выполнения.

## Планировщик

PostgreSQL анализирует SQL, выбирает стратегию и строит план. Физическое выполнение может отличаться от текста из-за индексов, статистики и JOIN-алгоритмов.

## Подвохи

- `COUNT(*)` считает строки, `COUNT(column)` игнорирует NULL.
- `NULL` не участвует в обычных предикатах истинно.
- `DISTINCT` включает NULL как одно значение для набора.
- `LIMIT` без сортировки не определяет стабильную страницу.
- Window-функция выполняется после WHERE и GROUP BY в логической модели.

## Вопросы с собеседований

<details><summary>1. Чем WHERE отличается от HAVING?</summary>

WHERE фильтрует строки до группировки, HAVING фильтрует агрегированные группы.

</details>

<details><summary>2. Зачем нужен DISTINCT ON?</summary>

PostgreSQL удаляет все строки кроме первой по набору ключей и с учетом порядка. Без deterministic ORDER BY результат не определён.

</details>

<details><summary>3. Почему COUNT(column) не считает NULL?</summary>

Функция count игнорирует NULL-значения входного выражения.

</details>

<details><summary>4. Когда OFFSET плохой выбор?</summary>

При глубоких страницах и частых вставках, когда сканирование пропущенных строк растет и ключи смещаются.

</details>

<details><summary>5. Чем UNION ALL отличается от UNION?</summary>

UNION удаляет дубликаты, UNION ALL сохраняет все строки.

</details>

## Источники

- [PostgreSQL SELECT](https://www.postgresql.org/docs/current/sql-select.html)
- [PostgreSQL EXPLAIN](https://www.postgresql.org/docs/current/using-explain.html)
