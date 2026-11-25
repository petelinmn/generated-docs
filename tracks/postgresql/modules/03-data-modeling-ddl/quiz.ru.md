---
title: "Квиз: проектирование схемы"
lang: ru
track: postgresql
kind: module
doc: quiz
slug: 03-data-modeling-ddl
part: 1
tags: [ddl, primary-key, foreign-key, constraints]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-25
---

# Квиз: проектирование схемы

🌐 **Русский** · [English](quiz.en.md)

## 1. 🟢 Какой тип подходит для денежной суммы с фиксированной точностью?

- A) `numeric`
- B) `text`
- C) `jsonb`
- D) `bigserial`

<details><summary>Ответ</summary>

**A.** `numeric(p,s)` сохраняет точную десятичную сумму.

</details>

## 2. 🟡 Что делает `ON DELETE CASCADE`?

- A) Запрещает удаление родителя
- B) Удаляет зависимые дочерние строки
- C) Создает резервную копию
- D) Ничего

<details><summary>Ответ</summary>

**B.** Каскад распространяет удаление на связанные строки.

</details>

## 3. 🔴 Найдите проблему

```sql
CREATE TABLE orders (
    id bigint PRIMARY KEY,
    customer_id bigint REFERENCES customers(id)
);
```

<details><summary>Ответ</summary>

Если клиент обязателен, добавьте `NOT NULL` к `customer_id`. Без него заказ может существовать без клиента.

</details>
