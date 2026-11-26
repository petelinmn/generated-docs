---
title: "Упражнения: jSON и операторы"
lang: ru
track: postgresql
kind: module
doc: exercises
slug: 08-json-and-operators
part: 2
tags: [jsonb, json, jsonpath, gin, operators]
versions: "PostgreSQL 17+"
status: verified
updated: 2026-11-26
---

# Упражнения: jSON и операторы

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Данные — из [базовой теории](basics.ru.md).

## 1. 🟢 Дешёвые товары

**Задача:** названия и цены товаров дешевле 20 по возрастанию цены. В документе 3 цена записана строкой.

<details><summary>Подсказка</summary>

Извлеките текст, затем приведите тип.

</details>

<details><summary>Решение</summary>

```sql
SELECT body ->> 'name' AS name, (body ->> 'price')::numeric AS price
FROM docs
WHERE (body ->> 'price')::numeric < 20
ORDER BY price;
-- Pen 2 | Book 15
```

Приведение работает и для строки `"30"`; значение вроде `"n/a"` привело бы к ошибке запроса.

</details>

## 2. 🟡 Статистика тегов

**Задача:** посчитать, в скольких документах встречается каждый тег.

<details><summary>Подсказка</summary>

Превратите массив в строки.

</details>

<details><summary>Решение</summary>

```sql
SELECT t.tag, count(*)
FROM docs d CROSS JOIN LATERAL jsonb_array_elements_text(d.body -> 'tags') AS t(tag)
GROUP BY t.tag ORDER BY t.tag;
-- cheap 1 | office 1 | read 1
```

</details>

## 3. 🔴 Добавить остаток там, где его нет

**Задача:** задать остаток в `kzn` равным 3 для документа 3, где объекта `stock` нет вовсе.

<details><summary>Подсказка</summary>

`jsonb_set` не создаёт промежуточные объекты.

</details>

<details><summary>Решение</summary>

```sql
UPDATE docs
SET body = jsonb_set(body, '{stock}',
                     coalesce(body -> 'stock', '{}') || '{"kzn": 3}')
WHERE id = 3;

SELECT body -> 'stock' FROM docs WHERE id = 3;   -- {"kzn": 3}
```

`coalesce` подставляет пустой объект, `||` сохраняет другие города, если они есть.

</details>
