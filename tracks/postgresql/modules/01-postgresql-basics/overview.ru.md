---
title: "Основы PostgreSQL"
lang: ru
track: postgresql
kind: module
doc: overview
slug: 01-postgresql-basics
part: 1
levels: [1, 2, 3, 4]
tags: [postgresql, architecture, client-server, database, schema, table, psql]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-25
---

# Основы PostgreSQL

🌐 **Русский** · [English](overview.en.md)

## Коротко

PostgreSQL работает по модели клиент/сервер: программы отправляют SQL-серверу, а сервер планирует запросы и читает или изменяет данные. Каждый серверный кластер содержит базы данных, а таблицы и объекты группируются схемами внутри базы.

## Ключевые идеи

- **Кластер PostgreSQL** — набор баз данных, обслуживаемый одним экземпляром сервера.
- **База данных изолирует содержимое**, а схема организует объекты внутри базы.
- **`psql`** отправляет SQL в интерактивном или пакетном режиме.
- **Таблица имеет строки и именованные столбцы с типами.**
- **SQL разделяют на DDL, DML и DQL.**
- **Системная база `postgres` и база приложения создаются отдельно.**

## Минимальный пример

```sql
CREATE TABLE users (
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name text NOT NULL
);

INSERT INTO users (name) VALUES ('Ada');
TABLE users;
```

```text
 id | name
----+------
  1 | Ada
```

## Когда применять

- ✅ Используйте базы для сильной изоляции приложений или tenants.
- ✅ Используйте схемы для группировки объектов и управления поиском.
- ✅ Используйте `psql` для интерактивной диагностики и скриптов.
- ❌ Не используйте одну таблицу для неструктурированных сущностей без причины.
- ❌ Не запускайте штатной порт PostgreSQL наружу с доверенным паролем.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | контейнер, клиент/сервер, базы, схемы, таблицы, psql |
| [2. Продвинутая теория](advanced.ru.md) | идентификаторы, search_path, DDL-транзакции, метаданные |
| [3. Экспертная теория](expert.ru.md) | процессы сервера, каталог, модель подключения, подвохи |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | ещё не создан — `/learn postgresql 01-postgresql-basics level 5` |
| 6. Тесты | ещё не создан — `/learn postgresql 01-postgresql-basics level 6` |

## Связанные материалы

- Пререквизиты: нет.
- Дальше: [02 · SQL-запросы](../02-sql-queries/overview.ru.md).
- Связанный трек: [Python](../../../python/overview.ru.md).

## Источники

- [PostgreSQL: What is PostgreSQL](https://www.postgresql.org/docs/current/intro-whatis.html)
- [PostgreSQL tutorial: SQL](https://www.postgresql.org/docs/current/tutorial-sql.html)
- [PostgreSQL: Table basics](https://www.postgresql.org/docs/current/ddl-basics.html)
