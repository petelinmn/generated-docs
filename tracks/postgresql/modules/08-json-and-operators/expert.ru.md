---
title: "JSON и операторы: экспертная теория"
lang: ru
track: postgresql
kind: module
doc: expert
slug: 08-json-and-operators
part: 2
tags: [jsonb, json, jsonpath, gin, operators]
versions: "PostgreSQL 17+"
status: verified
updated: 2026-11-26
---

# JSON и операторы: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

## Под капотом

`jsonb` разбирается один раз при вводе в двоичную структуру, поэтому чтение поля не требует разбора текста, а ввод чуть медленнее, чем у `json`. Большие значения сжимаются и могут выноситься из основной строки (TOAST). При `UPDATE` неизменённые вынесенные значения обычно сохраняются как есть; изменённый документ записывается заново. Любое обновление блокирует всю строку, поэтому документация советует держать документы обозримого размера.

`jsonb_ops` индексирует ключи и значения отдельными элементами; `jsonb_path_ops` — хэши целых путей до значений, поэтому он меньше и не может ответить на вопрос «есть ли такой ключ».

## jsonpath

```sql
SELECT id FROM docs WHERE body @? '$.stock.* ? (@ > 0)';   -- 1
SELECT id FROM docs WHERE body @@ '$.price > 10';          -- 2   ("30" is a string)
SELECT jsonb_path_query_array('{"a":[1,2,3]}', '$.a[*] ? (@ >= $min)', '{"min":2}');  -- [2, 3]

SELECT jsonb_path_exists('{"a":1}', 'lax $.b');      -- f
SELECT jsonb_path_exists('{"a":1}', 'strict $.b');
-- ERROR:  JSON object does not contain key "b"
```

`$` — документ, `.key` и `[*]` спускаются внутрь, `? (...)` фильтрует, `@` — текущий элемент, `$min` — переменная, переданная из SQL.

## Функции SQL/JSON (PostgreSQL 17)

```sql
SELECT * FROM JSON_TABLE('[{"n":"a","p":1},{"n":"b"}]'::jsonb, '$[*]'
    COLUMNS (n text PATH '$.n', p int PATH '$.p'));
-- a 1 | b NULL

SELECT JSON_VALUE('{"p":"15"}'::jsonb, '$.p' RETURNING int),   -- 15
       JSON_QUERY('{"t":[1,2]}'::jsonb, '$.t'),                -- [1, 2]
       JSON_EXISTS('{"a":1}'::jsonb, '$.a');                   -- t
```

## Справочник

| Оператор | Смысл | Возвращает |
|---|---|---|
| `->`, `->>` | по ключу или индексу массива | `jsonb`, `text` |
| `#>`, `#>>` | по пути | `jsonb`, `text` |
| `@>`, `<@` | содержит, содержится | `boolean` |
| `?`, `?\|`, `?&` | ключ или строковый элемент: один, любой, все | `boolean` |
| `@?`, `@@` | jsonpath возвращает элементы, предикат jsonpath истинен | `boolean` |
| `\|\|` | поверхностная конкатенация | `jsonb` |
| `-`, `#-` | удалить ключ или элемент, удалить по пути | `jsonb` |

| Функция | Назначение |
|---|---|
| `jsonb_set`, `jsonb_set_lax`, `jsonb_insert` | изменить значение, с правилом для `NULL`, вставить |
| `jsonb_build_object`, `jsonb_build_array`, `to_jsonb` | построить |
| `jsonb_agg`, `jsonb_object_agg` | агрегировать строки |
| `jsonb_array_elements[_text]`, `jsonb_each`, `jsonb_object_keys` | развернуть |
| `jsonb_to_record`, `jsonb_to_recordset`, `jsonb_populate_record` | в типизированные строки |
| `jsonb_typeof`, `jsonb_array_length`, `jsonb_strip_nulls` | осмотреть и почистить |
| `jsonb_path_query`, `..._first`, `..._array`, `jsonb_path_exists`, `jsonb_path_match` | jsonpath |

`jsonb_set` создаёт последний ключ пути, если его нет (`create_if_missing` по умолчанию true), но все предыдущие шаги должны существовать.

## Подвохи

### Три вида «ничего»

```sql
SELECT body -> 'discount' IS NULL,        -- f: there is a JSON null
       body ->> 'discount',               -- NULL
       jsonb_typeof(body -> 'discount'),  -- null
       body -> 'nope' IS NULL             -- t: the key is missing
FROM docs WHERE id = 3;
```

### SQL NULL уничтожает документ

```sql
SELECT jsonb_set('{"a":1}'::jsonb, '{a}', NULL);       -- NULL: the whole document is gone
SELECT jsonb_set_lax('{"a":1}'::jsonb, '{a}', NULL);   -- {"a": null}
```

### CHECK проходит, когда ключа нет

```sql
CREATE TABLE c (body jsonb CHECK (jsonb_typeof(body -> 'name') = 'string'));
INSERT INTO c VALUES ('{}');   -- accepted: the check evaluates to NULL, not false
```

Ограничение отклоняет строку только при `false`. Добавьте `body ? 'name'` или используйте `coalesce`.

### jsonb не хранит нулевой символ

`'{"a":"\u0000"}'::jsonb` завершается ошибкой `unsupported Unicode escape sequence`. Внешние данные могут его содержать.

## Компромиссы и альтернативы

| Выбор | Плюс | Минус |
|---|---|---|
| Колонки | типы, ограничения, статистика, небольшие обновления | изменение схемы на каждый атрибут |
| `jsonb` | гибкие атрибуты | слабее проверки, перезапись документа целиком |
| Колонки + `jsonb` для остального | обычный компромисс | нужно решать, где живёт поле |
| Таблица «сущность–атрибут–значение» | строки и индексы на атрибут | много соединений, неудобные запросы |
| Документная база | модель и инструменты вокруг документов | ещё одна система в эксплуатации |

## Вопросы с собеседований

**1. `json` или `jsonb`?**

<details><summary>Ответ</summary>

`jsonb` для запросов и индексов; `json` только ради точного входного текста.

</details>

**2. `->` против `->>`?**

<details><summary>Ответ</summary>

Первый возвращает `jsonb`, второй — `text`.

</details>

**3. Какие операторы поддерживает GIN по умолчанию на `jsonb`?**

<details><summary>Ответ</summary>

`?`, `?|`, `?&`, `@>`, `@?`, `@@`.

</details>

**4. Когда лучше `jsonb_path_ops`?**

<details><summary>Ответ</summary>

Нужны только вхождение и jsonpath: индекс обычно намного меньше. Операторы существования ключа теряются.

</details>

**5. Чего стоит обновление строки с большим документом?**

<details><summary>Ответ</summary>

Новая версия строки и блокировка всей строки; изменённое большое значение записывается заново.

</details>

**6. Как проиндексировать одно поле документа?**

<details><summary>Ответ</summary>

Индекс по выражению `((body ->> 'field'))` или вычисляемая колонка с индексом.

</details>

**7. Чем отличаются lax и strict в jsonpath?**

<details><summary>Ответ</summary>

Lax (по умолчанию) подстраивается под структуру и подавляет структурные ошибки; strict их выдаёт.

</details>

**8. Что нового для JSON появилось в PostgreSQL 17?**

<details><summary>Ответ</summary>

`JSON_TABLE`, `JSON_VALUE`, `JSON_QUERY`, `JSON_EXISTS`.

</details>

**9. Подвох: `body -> 'k' IS NULL` ложно, а `body ->> 'k' IS NULL` истинно. Почему?**

<details><summary>Ответ</summary>

В ключе лежит JSON `null`.

</details>

**10. Подвох: `UPDATE` с `jsonb_set` стёр документ. Как?**

<details><summary>Ответ</summary>

Новое значение было SQL `NULL`, и результат стал `NULL`.

</details>

**11. Когда поле должно стать колонкой?**

<details><summary>Ответ</summary>

Когда по нему регулярно фильтруют, соединяют, сортируют или накладывают ограничения.

</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [Квиз](quiz.ru.md) и [упражнения](exercises.ru.md)

## Источники

- [PostgreSQL 17: типы JSON](https://www.postgresql.org/docs/17/datatype-json.html)
- [PostgreSQL 17: функции и операторы JSON](https://www.postgresql.org/docs/17/functions-json.html)
- [PostgreSQL 17: TOAST](https://www.postgresql.org/docs/17/storage-toast.html)
- [PostgreSQL 17: список изменений](https://www.postgresql.org/docs/17/release-17.html)
