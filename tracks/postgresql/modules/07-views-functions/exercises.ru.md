---
title: "Упражнения: представления и функции"
lang: ru
track: postgresql
kind: module
doc: exercises
slug: 07-views-functions
part: 2
tags: [view, materialized-view, plpgsql, stored-procedure, trigger]
versions: "PostgreSQL 17+"
status: verified
updated: 2026-11-26
---

# Упражнения: представления и функции

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Данные — из [базовой теории](basics.ru.md).

## 1. 🟢 Безопасное представление

**Задача:** представление товаров дешевле 10, через которое нельзя создать или оставить более дорогой товар, с проверкой прав вызывающего.

<details><summary>Подсказка</summary>

Две вещи: check option и параметр представления.

</details>

<details><summary>Решение</summary>

```sql
CREATE VIEW cheap_products WITH (security_invoker = true) AS
    SELECT id, name, price, active FROM products WHERE price < 10
    WITH CHECK OPTION;
```

Обновление, поднимающее цену до 10 и выше, тоже отклоняется.

</details>

## 2. 🟡 Функция, которая не молчит

**Задача:** `set_price(id, price)` должна выдавать ошибку, если товара нет.

<details><summary>Подсказка</summary>

PL/pgSQL знает, затронул ли строку последний оператор.

</details>

<details><summary>Решение</summary>

```sql
CREATE FUNCTION set_price(p_id int, p_price numeric) RETURNS void LANGUAGE plpgsql AS $$
BEGIN
    UPDATE products SET price = p_price WHERE id = p_id;
    IF NOT FOUND THEN
        RAISE EXCEPTION 'product % not found', p_id USING ERRCODE = 'P0002';
    END IF;
END $$;
```

`UPDATE` нуля строк сам по себе не ошибка.

</details>

## 3. 🔴 Защита от резкого снижения цены

**Задача:** запретить уменьшать цену больше чем вдвое за одно обновление.

<details><summary>Подсказка</summary>

Нужны и `OLD`, и `NEW`, до записи строки.

</details>

<details><summary>Решение</summary>

```sql
CREATE FUNCTION forbid_cheaper() RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
    IF NEW.price < OLD.price / 2 THEN
        RAISE EXCEPTION 'price of % cannot drop by more than half', OLD.id;
    END IF;
    RETURN NEW;
END $$;
CREATE TRIGGER products_guard BEFORE UPDATE OF price ON products
    FOR EACH ROW EXECUTE FUNCTION forbid_cheaper();
```

Ограничением `CHECK` это не выразить: оно не видит старого значения.

</details>
