---
title: "Квиз: соединения и подзапросы"
lang: ru
track: postgresql
kind: module
doc: quiz
slug: 04-joins-and-subqueries
part: 1
tags: [join, subquery, cte, lateral, set-operations]
versions: "PostgreSQL 17+"
status: verified
updated: 2026-11-26
---

# Квиз: соединения и подзапросы

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает уровень: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что остаётся в результате `LEFT JOIN`?

- A) Только совпавшие пары
- B) Пары и все левые строки без пары
- C) Все строки обеих таблиц
- D) Только левые строки без пары

<details><summary>Ответ</summary>

**B.** A — внутреннее соединение, C — полное, D — anti-join.

→ [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 `CROSS JOIN` из 3 и 4 строк даёт

- A) 4
- B) 7
- C) 12
- D) 3

<details><summary>Ответ</summary>

**C.** Каждая строка сочетается с каждой.

→ [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Какой запрос выводит каждого клиента с заказами ровно один раз?

- A) Внутреннее соединение с `orders`
- B) `WHERE EXISTS (...)`
- C) `CROSS JOIN orders`
- D) `FULL JOIN orders`

<details><summary>Ответ</summary>

**B.** Соединение повторяет клиента на каждый заказ; C и D тоже дают лишние строки.

→ [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Зачем в соединении таблицы с самой собой псевдонимы?

- A) Ради скорости
- B) Чтобы различить две роли таблицы
- C) Это включает индекс
- D) Это убирает дубликаты

<details><summary>Ответ</summary>

**B.** Без псевдонимов ссылки на колонки были бы неоднозначны.

→ [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 Сколько строк вернёт запрос на данных модуля?

```sql
SELECT c.name FROM customers c
LEFT JOIN orders o ON o.customer_id = c.id AND o.amount > 60;
```

<details><summary>Ответ</summary>

```text
3
```

Остаются все три клиента; `ON` лишь ограничивает, какие заказы считаются парой.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Что вернёт запрос на данных модуля?

```sql
SELECT count(*) FROM customers
WHERE id NOT IN (SELECT customer_id FROM orders);
```

<details><summary>Ответ</summary>

```text
0
```

Заказ 13 даёт `NULL`, поэтому условие никогда не истинно.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Выбери все верные утверждения

- [ ] A) `UNION` убирает дубликаты
- [ ] B) `UNION ALL` сохраняет дубликаты
- [ ] C) `EXCEPT` возвращает строки, имеющиеся в обоих запросах
- [ ] D) `INTERSECT` возвращает строки, имеющиеся в обоих запросах

<details><summary>Ответ</summary>

**A, B, D.** `EXCEPT` возвращает строки первого запроса, которых нет во втором.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Что разрешает `LATERAL`?

- A) Параллельное выполнение запроса
- B) Подзапросу в `FROM` ссылаться на предыдущие элементы `FROM`
- C) Соединение без условия
- D) Пропуск значений `NULL`

<details><summary>Ответ</summary>

**B.** Параллелизм — отдельная возможность планировщика.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🟡 Найди ошибку: «клиенты и число их заказов»

```sql
SELECT c.name, count(*) FROM customers c
LEFT JOIN orders o ON o.customer_id = c.id
GROUP BY c.id, c.name;
```

<details><summary>Ответ</summary>

`count(*)` считает строку из `NULL`, и Eve получает 1 вместо 0. Нужно `count(o.id)`.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 10. 🔴 Какой алгоритм соединения требует условия равенства?

- A) Nested loop
- B) Hash join
- C) Все
- D) Никакой

<details><summary>Ответ</summary>

**B.** По хэш-таблице можно искать только по равенству; nested loop работает с любым условием.

→ [Экспертная теория](expert.ru.md)

</details>

## 11. 🔴 Что выведет запрос?

```sql
SELECT 5 > ALL (SELECT amount FROM orders WHERE false);
```

<details><summary>Ответ</summary>

```text
t
```

`ALL` над пустым набором истинно.

→ [Экспертная теория](expert.ru.md)

</details>

## 12. 🔴 Открытый вопрос: отчёт в одном запросе суммирует платежи и отгрузки клиента, и итоги завышены. Объясните и исправьте.

<details><summary>Ключевые пункты ответа</summary>

- Две связи «один ко многим» размножают строки.
- Агрегировать каждую в своём подзапросе или CTE, затем соединять по одной строке на клиента.
- `DISTINCT` внутри `sum` не исправление: одинаковые суммы потеряются.

→ [Экспертная теория](expert.ru.md)

</details>
