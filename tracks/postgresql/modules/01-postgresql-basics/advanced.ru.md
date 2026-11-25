---
title: "Основы PostgreSQL: продвинутая теория"
lang: ru
track: postgresql
kind: module
doc: advanced
slug: 01-postgresql-basics
part: 1
tags: [search-path, identifiers, ddl-transactions, information-schema, pg-catalog]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-25
---

# Основы PostgreSQL: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md).

## Идентификаторы и схемы

Необъятые идентификаторы отображаются в lowercase. Кавычки сохраняют регистр, но усложняют обращения:

```sql
CREATE TABLE app.accounts (
    account_id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    display_name text
);
SHOW search_path;
SET search_path TO app, public;
SELECT * FROM accounts;
```

Для приложений лучше задавать явную схему и одну стратегию именования.

## Транзакции DDL

Большинство DDL-команд в PostgreSQL может выполняться в транзакции:

```sql
BEGIN;
CREATE TABLE app.audit_log (id bigint GENERATED ALWAYS AS IDENTITY, event text);
ALTER TABLE app.audit_log ADD CONSTRAINT audit_event_required CHECK (event IS NOT NULL);
ROLLBACK;
```

Таблица и ограничение после отката не создаются.

## Метаданные

Системный каталог `pg_catalog` содержит описания объектов. Стандартное представление `information_schema` проще для переносимых запросов.

```sql
SELECT table_schema, table_name
FROM information_schema.tables
WHERE table_type = 'BASE TABLE'
ORDER BY table_schema, table_name;
```

## Лучшие практики

- Явно указывайте схему в SQL-запросах и миграциях.
- Создавайте приложение в отдельной базе или схеме по необходимости.
- Используйте транзакции для связанных DDL-изменений.
- Не создавайте произвольные quoted identifiers без причины.
- Проверяйте метаданные до удаления объектов.

## Частые ошибки

- ❌ Полагаться на порядок поиска схем → ✅ установить и задокументировать `search_path`.
- ❌ Создать объект в public для тестового приложения → ✅ выделить отдельную схему или роль.
- ❌ Считать транзакцию rollback для network cutoff → ✅ проверять состояние сервера и журналы.

## Связанные материалы

- Следующий модуль: [02 · SQL-запросы](../02-sql-queries/overview.ru.md).

## Источники

- [PostgreSQL schemas](https://www.postgresql.org/docs/current/ddl-schemas.html)
- [PostgreSQL identifiers](https://www.postgresql.org/docs/current/sql-syntax-lexical.html)
- [PostgreSQL system catalogs](https://www.postgresql.org/docs/current/catalogs.html)
