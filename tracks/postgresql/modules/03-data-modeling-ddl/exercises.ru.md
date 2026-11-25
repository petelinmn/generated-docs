---
title: "Упражнения: проектирование схемы"
lang: ru
track: postgresql
kind: module
doc: exercises
slug: 03-data-modeling-ddl
part: 1
tags: [ddl, foreign-key, constraints]
status: draft
updated: 2026-11-25
---

# Упражнения: проектирование схемы

🌐 **Русский** · [English](exercises.en.md)

## 1. 🟢 Таблица заказов

Создайте таблицу `customers` и `orders` с первичными ключами и обязательной внешней связью.

<details><summary>Решение</summary>

```sql
CREATE TABLE customers (
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    email text NOT NULL UNIQUE
);

CREATE TABLE orders (
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    customer_id bigint NOT NULL REFERENCES customers(id),
    amount numeric(12,2) NOT NULL CHECK (amount >= 0)
);
```

</details>

## 2. 🟡 Composite key

Создайте join-таблицу продуктов и заказов с composite primary key.

<details><summary>Решение</summary>

```sql
CREATE TABLE order_items (
    order_id bigint NOT NULL REFERENCES orders(id),
    product_id bigint NOT NULL REFERENCES products(id),
    quantity integer NOT NULL CHECK (quantity > 0),
    PRIMARY KEY (order_id, product_id)
);
```

</details>
