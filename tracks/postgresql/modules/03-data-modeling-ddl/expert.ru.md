---
title: "Проектирование схемы и DDL: экспертная теория"
lang: ru
track: postgresql
kind: module
doc: expert
slug: 03-data-modeling-ddl
part: 1
tags: [foreign-key, constraints, dependencies, catalog, migrations]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-25
---

# Проектирование схемы и DDL: экспертная теория

🌐 **Русский** · [English](expert.en.md)

## Справочник связей

| Связь | Реализация |
|---|---|
| Один к одному | UNIQUE FK на дочерней таблице |
| Один ко многим | FK на дочерний столбец |
| Многие ко многим | join-таблица с composite primary key |
| Наследование сущностей | отдельные таблицы или разделение через subtype |

## Ограничения

PostgreSQL хранит ограничения в каталоге и проверяет их при DML. Добавление NOT VALID-ограничения имеет иные последствия проверки существующих строк и требует последующей VALIDATE.

## Подвохи

- Composite FK должен ссылаться на UNIQUE или PRIMARY KEY соответствующего набора столбцов.
- `numeric(p,s)` ограничивает точность и масштаб для финансовых данных.
- `ON DELETE CASCADE` может удалить больше данных, чем ожидается.
- Имена enum нельзя легко переупорядочивать или удалять.
- Эксклюзивные ограничения требуют `EXCLUDE` и индексирующего оператора.

## Вопросы с собеседований

<details><summary>1. Когда нужен composite key?</summary>

Когда бизнес-сущность однозначно идентифицируется комбинацией столбцов.

</details>

<details><summary>2. Чем UNIQUE отличается от PRIMARY KEY?</summary>

UNIQUE запрещает дубликаты, но столбец может допускать NULL в обычной логике. PRIMARY KEY одновременно уникален и NOT NULL.

</details>

<details><summary>3. Что делает NOT VALID?</summary>

Оно ограничивает новые вставки или обновления без немедленной проверки всех существующих строк.

</details>

<details><summary>4. Зачем нормализация?</summary>

Она устраняет повтор фактов и предотвращает несогласованные записи.

</details>

<details><summary>5. Что опасно в CASCADE?</summary>

Удаление родителя может распространиться на много дочерних строк без отдельного SQL.

</details>

## Источники

- [PostgreSQL constraints](https://www.postgresql.org/docs/current/ddl-constraints.html)
- [PostgreSQL table constraints](https://www.postgresql.org/docs/current/ddl-constraints.html)
