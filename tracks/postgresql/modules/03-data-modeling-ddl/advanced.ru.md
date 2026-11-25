---
title: "Проектирование схемы и DDL: продвинутая теория"
lang: ru
track: postgresql
kind: module
doc: advanced
slug: 03-data-modeling-ddl
part: 1
tags: [normalization, enum, generated-column, migrations, on-delete]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-25
---

# Проектирование схемы и DDL: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

## Нормализация

Нормализованная модель избегает повторяемого факта в разных таблицах. Денормализация возможна ради чтения или производительности, но требует стратегии синхронизации.

## ON DELETE

```sql
CREATE TABLE order_items (
    order_id bigint NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    product_id bigint NOT NULL REFERENCES products(id),
    quantity integer NOT NULL CHECK (quantity > 0),
    PRIMARY KEY (order_id, product_id)
);
```

`CASCADE` удаляет зависимые строки вместе с родителем; для частых удалений это должно быть явно спроектировано.

## Сгенерированные столбцы

```sql
CREATE TABLE invoices (
    net numeric(12,2),
    tax numeric(12,2),
    total numeric(12,2) GENERATED ALWAYS AS (net + tax) STORED
);
```

## Миграции

Безопасные миграции добавляют, не удаляя старые зависимости за один запуск. Приложение должно поддерживать совместимость чтения и записи между версиями.

## Источники

- [PostgreSQL constraints](https://www.postgresql.org/docs/current/ddl-constraints.html)
- [PostgreSQL generated columns](https://www.postgresql.org/docs/current/ddl-generated-columns.html)
