---
title: "Квиз: основы PostgreSQL"
lang: ru
track: postgresql
kind: module
doc: quiz
slug: 01-postgresql-basics
part: 1
tags: [postgresql, database, schema, table, psql]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-25
---

# Квиз: основы PostgreSQL

🌐 **Русский** · [English](quiz.en.md)

## 1. 🟢 Что выполняет серверная роль PostgreSQL?

- A) Хранит данные и выполняет SQL
- B) Редактирует HTML
- C) Только запускает Docker
- D) Заменяет SQL-клиент

<details><summary>Ответ</summary>

**A.** Сервер хранит данные, планирует и выполняет запросы.

</details>

## 2. 🟢 Чем схема отличается от базы?

- A) Схема находится внутри базы
- B) Это одно и то же
- C) База находится внутри схемы
- D) Схема заменяет таблицу

<details><summary>Ответ</summary>

**A.** База содержит схемы, а схема является namespace внутри базы.

</details>

## 3. 🟢 Какой тип задаёт уникальное идентификаторное поле с автоинкрементом?

- A) `GENERATED ALWAYS AS IDENTITY`
- B) `SERIALIZE`
- C) `AUTO TABLE`
- D) `NULL`

<details><summary>Ответ</summary>

**A.** Identity автоматически генерирует значения для столбца.

</details>

## 4. 🟡 Что показывает `SELECT current_database();`?

- A) Версию Docker
- B) Имя текущей базы
- C) Все базы в кластере
- D) Имя пользователя ОС

<details><summary>Ответ</summary>

**B.** Текущая база доступна через системную функцию.

</details>

## 5. 🔴 Найдите проблему

```sql
SELECT * FROM accounts;
SET search_path TO reporting;
```

<details><summary>Ответ</summary>

Запрос выполняется до установки нового `search_path`; он может разрешиться к другому объекту. Укажите схемы явно или установите search_path до запросов.

</details>
