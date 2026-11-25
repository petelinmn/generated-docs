---
title: "Exercises: data modeling and DDL"
lang: en
track: postgresql
kind: module
doc: exercises
slug: 03-data-modeling-ddl
part: 1
tags: [ddl, foreign-key, constraints]
status: draft
updated: 2026-11-25
---

# Exercises: data modeling and DDL

🌐 [Русский](exercises.ru.md) · **English**

## 1. 🟢 Orders table

Create `customers` and `orders` tables with primary keys and a required foreign key.

<details><summary>Solution</summary>

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

Create a join table for products and orders with a composite primary key.

<details><summary>Solution</summary>

```sql
CREATE TABLE order_items (
    order_id bigint NOT NULL REFERENCES orders(id),
    product_id bigint NOT NULL REFERENCES products(id),
    quantity integer NOT NULL CHECK (quantity > 0),
    PRIMARY KEY (order_id, product_id)
);
```

</details>
