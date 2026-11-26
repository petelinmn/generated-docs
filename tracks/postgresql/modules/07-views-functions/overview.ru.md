---
title: "Представления и функции"
lang: ru
track: postgresql
kind: module
doc: overview
slug: 07-views-functions
part: 2
levels: [1, 2, 3, 4]
tags: [view, materialized-view, plpgsql, stored-procedure, trigger]
versions: "PostgreSQL 17+"
status: verified
updated: 2026-11-26
---

# Представления и функции

🌐 **Русский** · [English](overview.en.md)

## Коротко

Представление (view) — это сохранённый запрос, который ведёт себя как таблица. Функция — код, хранимый в базе и возвращающий значение или набор строк, процедура — код, который вызывают через `CALL`, а триггер — функция, которую база сама запускает при изменении данных. Встроенный процедурный язык PostgreSQL — PL/pgSQL. Эти инструменты переносят логику к данным: это полезно для общих правил и опасно, когда она становится невидимой.

## Ключевые идеи

- Обычное представление хранит запрос, а не данные; материализованное хранит результат.
- Простые представления можно изменять; `WITH CHECK OPTION` не даёт строкам «уйти» из представления.
- `SELECT *` в представлении раскрывается один раз, при создании.
- Изменчивость (`VOLATILE`, `STABLE`, `IMMUTABLE`) — это обещание, на которое полагается планировщик.
- Функция не может фиксировать транзакцию; процедура, вызванная через `CALL`, и блок `DO` — могут.
- Триггеры `BEFORE` могут изменить или пропустить строку, `AFTER` реагируют на неё.
- `SECURITY DEFINER` выполняется с правами владельца и требует фиксированного `search_path`.

## Минимальный пример

```sql
CREATE TABLE products (
    id int PRIMARY KEY, name text NOT NULL,
    price numeric(10,2) NOT NULL, active boolean NOT NULL DEFAULT true
);
INSERT INTO products VALUES (1,'Pen',2,true), (2,'Book',15,true), (3,'Lamp',30,false);
```

```sql
CREATE VIEW active_products AS
    SELECT id, name, price, active FROM products WHERE active;

SELECT id, name FROM active_products ORDER BY id;   -- 1 Pen | 2 Book
```

```sql
CREATE FUNCTION price_with_tax(price numeric, rate numeric DEFAULT 0.2) RETURNS numeric
LANGUAGE sql IMMUTABLE
RETURN round(price * (1 + rate), 2);

SELECT price_with_tax(10), price_with_tax(10, rate => 0.1);   -- 12.00 | 11.00
```

## Когда применять, а когда нет

- ✅ Устойчивый интерфейс над меняющейся схемой; ограничение того, что видит роль.
- ✅ Инварианты и аудит, обязательные для любого клиента базы.
- ❌ Бизнес-процессы с внешними вызовами внутри триггеров.
- ❌ Слои представлений над представлениями, план которых никто не может объяснить.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | представления, SQL-функции, параметры, табличные функции |
| [2. Продвинутая теория](advanced.ru.md) | изменяемые и материализованные представления, PL/pgSQL, процедуры, триггеры |
| [3. Экспертная теория](expert.ru.md) | изменчивость, безопасность, справочник, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | ещё не создан — `/learn postgresql 07-views-functions level 5` |
| 6. Тесты | ещё не создан — `/learn postgresql 07-views-functions level 6` |

## Связанные материалы

- Пререквизиты: [04 · Соединения и подзапросы](../04-joins-and-subqueries/overview.ru.md), [06 · Транзакции](../06-transactions/overview.ru.md)
- Дальше: следующий модуль по [дорожной карте PostgreSQL](../../overview.ru.md)
- Из .NET: [EF Core](../../../csharp/modules/15-ef-core/overview.ru.md)
- Место хранимой логики в проекте: [архитектурные стили](../../../system-design/modules/11-architecture-styles/overview.ru.md)
