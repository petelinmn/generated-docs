---
title: "PostgreSQL"
lang: ru
track: postgresql
kind: track
doc: overview
slug: postgresql
tags: [postgresql, sql, relational-database, transactions, indexes, ddl]
versions: "PostgreSQL 17+"
default_level: 4
status: draft
updated: 2026-11-25
---

# PostgreSQL

🌐 **Русский** · [English](overview.en.md)

## Коротко

PostgreSQL — объектно-реляционная СУБД с открытым исходным кодом, поддержкой SQL, ACID-транзакций и расширенной системой типов. Сервер хранит данные в таблицах, выполняет запросы через планировщик, проверяет целостность через ограничения и обеспечивает несколько уровней изоляции транзакций.

PostgreSQL подходит для OLTP-приложений, поиска внутри реляционных данных, JSON-документов, геоданных и аналитики. Трек начинается с установки, SQL и проектирования схемы, затем проходит запросы, индексы, транзакции, производительность и инфраструктуру.

**Пререквизиты:** базовые SQL-понятия необязательны, достаточно умеющих читать текстовый ввод и выхода командной строки.

## Окружение

| Что | Инструмент |
|---|---|
| Сервер | PostgreSQL 17 в Docker: образ `postgres:17-alpine` |
| Клиент | `psql`, выполняется внутри контейнера или как локальная утилита |
| Язык примеров | SQL и Python 3.12+ для запросов через `psycopg` |
| Код модулей | на уровнях 5–6: `code/` с SQL-примерами, решениями и pytest |
| Запустить SQL в одноразовом сервере | `docker exec -i learn-postgres psql -U postgres -d learn < file.sql` |
| Запустить модульные тесты | `uv run pytest` внутри `code/` |

```bash
docker run -d --name learn-postgres \
  -e POSTGRES_PASSWORD=learn -e POSTGRES_DB=learn \
  -p 5432:5432 postgres:17-alpine
docker exec -it learn-postgres psql -U postgres -d learn
```

> Пароль `learn` используется только в одноразовом локальном контейнере. Не публикуйте порт и не используйте эти настройки в production.

## Дорожная карта

Прогресс изучения ведётся в [PROGRESS.md](../../PROGRESS.md). Уровень детализации модулей по умолчанию: **4** (теория и самопроверка).

### Часть 1. Основы

- [01 · Основы PostgreSQL](modules/01-postgresql-basics/overview.ru.md) — архитектура, клиент/сервер, базы, схемы, таблицы, `psql`
- [02 · SQL-запросы](modules/02-sql-queries/overview.ru.md) — `SELECT`, фильтрация, сортировка, объединения строк, агрегаты
- [03 · Проектирование схемы](modules/03-data-modeling-ddl/overview.ru.md) — типы, первичные и внешние ключи, ограничения, DDL
- [04 · Соединения и подзапросы](modules/04-joins-and-subqueries/overview.ru.md) — INNER, OUTER, CROSS, CTE, EXISTS

### Часть 2. Продвинутые темы

- [05 · Индексы](modules/05-indexes/overview.ru.md) — B-tree, Hash, GIN, GiST, BRIN, частичные индексы
- [06 · Транзакции](modules/06-transactions/overview.ru.md) — ACID, уровни изоляции, блокировки и MVCC
- [07 · Представления и функции](modules/07-views-functions/overview.ru.md) — представления, функции, триггеры, процедурный язык
- [08 · Функции и JSON](modules/08-json-and-operators/overview.ru.md) — `jsonb`, индексы по JSON, операторные классы
- [09 · Производительность и EXPLAIN](modules/09-performance-explain/overview.ru.md) — план запросов, статистика, VACUUM и ANALYZE

### Часть 3. Темы для опытных

- [10 · Разделение и секционирование](modules/10-partitioning/overview.ru.md) — декларативное секционирование и inheritance
- [11 · Репликация](modules/11-replication/overview.ru.md) — streaming replication, WAL, логическая репликация
- [12 · Резервное копирование](modules/12-backups-and-recovery/overview.ru.md) — pg_dump, pg_basebackup, PITR
- [13 · Безопасность](modules/13-security/overview.ru.md) — роли, привилегии, RLS, SSL
- [14 · Эксплуатация и наблюдаемость](modules/14-operations/overview.ru.md) — конфигурация, журналирование, метрики, расширения

## Библиотеки и инструменты

- [psycopg](libraries/psycopg/overview.ru.md) — Python-драйвер PostgreSQL
- [SQLAlchemy](libraries/sqlalchemy/overview.ru.md) — ORM и SQL toolkit
- [psql](tools/psql/overview.ru.md) — интерактивный и пакетный SQL-клиент
- [pg_dump](tools/pg-dump/overview.ru.md) — логическое резервное копирование
- [pgAdmin](tools/pgadmin/overview.ru.md) — графическая администрация

## Рецепты

- [Пагинация](recipes/pagination/overview.ru.md) — keyset и offset пагинация
- [Миграции схемы](recipes/schema-migrations/overview.ru.md) — безопасные изменения и откаты
- [Transactional outbox](recipes/outbox/overview.ru.md) — надёжная публикация событий
- [Безопасная конфигурация](recipes/configuration/overview.ru.md) — роли и строки подключения

## Проекты

- [Инвентаризационный сервис](projects/inventory-service/overview.ru.md) — схема, запросы, транзакции и резервное копирование

## Связанные треки

- [Python](../python/overview.ru.md) — примеры и клиенты
- [C# / .NET](../csharp/overview.ru.md) — EF Core и базы данных
- [Kubernetes](../kubernetes/overview.ru.md) — запуск StatefulSet и операторов
- [Linux](../linux/overview.ru.md) — процессы, сеть и диагностика

## Источники

- [PostgreSQL 19 documentation](https://www.postgresql.org/docs/19/index.html)
- [PostgreSQL tutorial](https://www.postgresql.org/docs/current/tutorial-sql.html)
- [PostgreSQL SQL commands](https://www.postgresql.org/docs/current/sql.html)
- [PostgreSQL EXPLAIN](https://www.postgresql.org/docs/current/using-explain.html)
