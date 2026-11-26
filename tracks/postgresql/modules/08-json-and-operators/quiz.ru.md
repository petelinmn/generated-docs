---
title: "Квиз: jSON и операторы"
lang: ru
track: postgresql
kind: module
doc: quiz
slug: 08-json-and-operators
part: 2
tags: [jsonb, json, jsonpath, gin, operators]
versions: "PostgreSQL 17+"
status: verified
updated: 2026-11-26
---

# Квиз: jSON и операторы

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает уровень: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что возвращает `->>`?

- A) `jsonb`
- B) `text`
- C) `json`
- D) Тип значения

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Какой тип сохраняет дубли ключей из ввода?

- A) `jsonb`
- B) `json`
- C) Оба
- D) Никакой

<details><summary>Ответ</summary>

**B.** `json` хранит текст как есть.

→ [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Что проверяет `@>`?

- A) Больше чем
- B) Что левый документ содержит правый
- C) Что ключ существует
- D) Схожесть текста

<details><summary>Ответ</summary>

**B.** Существование ключа — это `?`.

→ [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Какие документы модуля подходят под `body ? 'discount'`?

- A) Никакие, значение null
- B) Документ 3
- C) Все
- D) Документы 1 и 2

<details><summary>Ответ</summary>

**B.** Ключ существует, хотя его значение — JSON `null`.

→ [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 Документы модуля упорядочены по `body ->> 'price'`. Какой id первый?

- A) 1
- B) 2
- C) 3
- D) Порядок не определён

<details><summary>Ответ</summary>

**B.** Текстовый порядок: `'15'` раньше `'2'`.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Какой результат?

```sql
SELECT '{"a":{"x":1}}'::jsonb || '{"a":{"y":2}}';
```

<details><summary>Ответ</summary>

```text
{"a": {"y": 2}}
```

Конкатенация поверхностная: вложенный объект заменяется.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Выбери все верные утверждения об индексах

- [ ] A) `jsonb_ops` поддерживает `?`
- [ ] B) `jsonb_path_ops` поддерживает `?`
- [ ] C) `jsonb_path_ops` обычно меньше
- [ ] D) GIN по `body` обслуживает `body ->> 'name' = 'x'`

<details><summary>Ответ</summary>

**A, C.** Равенству извлечённого значения нужен индекс по выражению.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Какой индекс обслужит `body ->> 'name' = 'Pen'`?

- A) GIN по `body`
- B) B-tree по `((body ->> 'name'))`
- C) BRIN по `body`
- D) Никакой

<details><summary>Ответ</summary>

**B.**

→ [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🟡 Найди проблему: «документы дешевле 20»

```sql
SELECT id FROM docs WHERE body ->> 'price' < '20';
```

<details><summary>Ответ</summary>

Сравниваются строки, а не числа: `'100' < '20'` истинно, а `'3' < '20'` ложно. Приведите к `numeric`.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 10. 🔴 Что выведет запрос?

```sql
SELECT jsonb_set('{"a":1}'::jsonb, '{a}', NULL);
```

<details><summary>Ответ</summary>

```text
NULL
```

SQL `NULL` в новом значении делает результат `NULL`; это обходит `jsonb_set_lax`.

→ [Экспертная теория](expert.ru.md)

</details>

## 11. 🔴 Отклонит ли `CHECK (jsonb_typeof(body -> 'name') = 'string')` документ `{}`?

- A) Да
- B) Нет
- C) Только с `NOT NULL`
- D) Будет ошибка типа

<details><summary>Ответ</summary>

**B.** Условие равно `NULL`, а проверка отклоняет только `false`. `NOT NULL` на колонке не помогает.

→ [Экспертная теория](expert.ru.md)

</details>

## 12. 🔴 Открытый вопрос: у товаров разных категорий разные атрибуты. Как хранить?

<details><summary>Ключевые пункты ответа</summary>

- Общие поля — типизированными колонками, остальное — в `jsonb`.
- Ограничения на обязательные ключи и типы.
- GIN или индексы по выражениям по настоящим запросам.
- Переносить ключ в колонку, когда он становится важным.

→ [Экспертная теория](expert.ru.md)

</details>
