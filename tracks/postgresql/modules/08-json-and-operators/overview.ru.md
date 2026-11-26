---
title: "JSON и операторы"
lang: ru
track: postgresql
kind: module
doc: overview
slug: 08-json-and-operators
part: 2
levels: [1, 2, 3, 4]
tags: [jsonb, json, jsonpath, gin, operators]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-26
---

# JSON и операторы

🌐 **Русский** · [English](overview.en.md)

## Коротко

PostgreSQL умеет хранить JSON-документы в колонке и искать внутри них. `jsonb` — двоичная форма, которая нужна почти всегда: её можно индексировать и эффективно просматривать. Она подходит для атрибутов, набор которых различается от строки к строке. Колонки она не заменяет: типы, ограничения, внешние ключи и статистика гораздо лучше работают на обычных колонках.

## Ключевые идеи

- `json` хранит точный текст, `jsonb` — разобранную структуру.
- `->` возвращает `jsonb`, `->>` возвращает `text`; числа сравнивают после приведения типа.
- `@>` спрашивает «содержит ли документ это», `?` — «есть ли такой ключ».
- JSON `null`, отсутствующий ключ и SQL `NULL` — три разные вещи.
- Обновление части документа записывает новую версию строки.
- GIN обслуживает вхождение и проверки ключей; индекс по выражению — одно извлечённое значение.
- jsonpath выражает условия внутри вложенных структур.

## Минимальный пример

```sql
CREATE TABLE docs (id int PRIMARY KEY, body jsonb NOT NULL);
INSERT INTO docs VALUES
 (1, '{"name":"Pen","price":2,"tags":["office","cheap"],"stock":{"msk":5,"spb":0}}'),
 (2, '{"name":"Book","price":15,"tags":["read"],"stock":{"msk":0}}'),
 (3, '{"name":"Lamp","price":"30","tags":[],"discount":null}');
```

```sql
SELECT body -> 'name'          AS j,       -- "Pen"    (jsonb)
       body ->> 'name'         AS t,       -- Pen      (text)
       body #>> '{tags,0}'     AS first,   -- office
       body -> 'tags' -> -1    AS last,    -- "cheap"
       body['stock']['msk']    AS msk,     -- 5
       body -> 'nope'          AS missing  -- NULL
FROM docs WHERE id = 1;
```

```sql
SELECT id FROM docs WHERE body @> '{"tags":["cheap"]}';   -- 1
SELECT id FROM docs WHERE body ? 'discount';              -- 3
SELECT id FROM docs WHERE body -> 'tags' ? 'read';        -- 2
```

## Когда применять, а когда нет

- ✅ Необязательные или пользовательские атрибуты, сырые данные внешних систем.
- ✅ Вложенная структура, которую читают и пишут целиком.
- ❌ Значения, по которым постоянно фильтруют, соединяют или ограничивают: сделайте их колонками.
- ❌ Большой документ, в котором часто меняется один счётчик.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | `json` и `jsonb`, операторы доступа и вхождения |
| [2. Продвинутая теория](advanced.ru.md) | типы, изменение, построение и разворачивание документов, индексы, ограничения |
| [3. Экспертная теория](expert.ru.md) | jsonpath, функции SQL/JSON, справочник, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | ещё не создан — `/learn postgresql 08-json-and-operators level 5` |
| 6. Тесты | ещё не создан — `/learn postgresql 08-json-and-operators level 6` |

## Связанные материалы

- Пререквизиты: [04 · Соединения и подзапросы](../04-joins-and-subqueries/overview.ru.md), [05 · Индексы](../05-indexes/overview.ru.md)
- Дальше: следующий модуль по [дорожной карте PostgreSQL](../../overview.ru.md)
- Документы или отношения: [хранение данных](../../../system-design/modules/04-data-storage/overview.ru.md)
- Документы с поиском: [OpenSearch](../../../opensearch/modules/01-opensearch-basics/overview.ru.md)
