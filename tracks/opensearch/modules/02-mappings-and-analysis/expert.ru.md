---
title: "Отображения и анализ: экспертная теория"
lang: ru
track: opensearch
kind: module
doc: expert
slug: 02-mappings-and-analysis
part: 1
tags: [mapping, analyzer, inverted-index, text-keyword, nested, dynamic-mapping]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Отображения и анализ: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

## Структуры внутри Lucene

| Структура | Для чего | Отключается |
|---|---|---|
| Обратный индекс | поиск по термам | `index: false` |
| Doc values (по столбцам) | сортировка, агрегации, скрипты | `doc_values: false` |
| `_source` | выдача, reindex, `update` | `_source.enabled: false` |
| Нормы | учёт длины поля в оценке | `norms: false` |

У `text` нет doc values, поэтому сортировка по нему требует `fielddata=true` — загрузки в память кучи, чего обычно избегают. Без `_source` перестают работать `update` и `reindex` из этого индекса.

## Типы полей

Список по документации (в этом модуле проверены `text`, `keyword`, `long`, `float`, `double`, `boolean`, `date`, `object`, `nested`):

| Группа | Типы |
|---|---|
| Строки | `text`, `keyword`, `match_only_text`, `wildcard` |
| Числа | `long`, `integer`, `short`, `byte`, `double`, `float`, `half_float`, `scaled_float`, `unsigned_long` |
| Прочие простые | `boolean`, `date`, `date_nanos`, `ip`, `binary` |
| Диапазоны | `integer_range`, `long_range`, `float_range`, `double_range`, `date_range`, `ip_range` |
| Структуры | `object`, `nested`, `flat_object`, `join` |
| Геоданные | `geo_point`, `geo_shape` |
| Специальные | `completion`, `search_as_you_type`, `token_count`, `percolator`, `rank_feature`, `alias`, `knn_vector` |

## Параметры отображения

| Параметр | Смысл |
|---|---|
| `analyzer`, `search_analyzer` | анализ при записи и поиске |
| `normalizer` | обработка `keyword` |
| `fields` | несколько способов индексации одного значения |
| `copy_to` | копировать значение в общее поле |
| `ignore_above` | не индексировать слишком длинный `keyword` |
| `ignore_malformed` | пропустить некорректное значение вместо отказа |
| `null_value` | чем индексировать явный `null` |
| `format` | форматы даты |
| `coerce` | приведение типов |

## Части анализатора

| Часть | Количество | Примеры |
|---|---|---|
| фильтры символов | 0 и более | `html_strip`, `mapping`, `pattern_replace` |
| токенизатор | ровно 1 | `standard`, `whitespace`, `keyword`, `pattern`, `ngram`, `edge_ngram`, `path_hierarchy` |
| фильтры токенов | 0 и более | `lowercase`, `asciifolding`, `stop`, `stemmer`, `synonym`, `synonym_graph`, `ngram`, `shingle` |

## Подвохи

1. Значение длиннее `ignore_above` не индексируется в `keyword`: документ с `title` из 300 символов не нашёлся по `term` в `title.keyword`, хотя сохранён.
2. Тип поля определяет **первый** документ: `"42"` сделает поле текстом навсегда.
3. `float` 7.9 в поле `long` не отклоняется.
4. `standard` не разбивает `mail.com`, а `simple` разбивает.
5. Изменение анализатора не переиндексирует старые документы.
6. Предел `index.mapping.total_fields.limit` по умолчанию 1000.

## Вопросы с собеседований

1. Из чего состоит анализатор?
<details><summary>Ответ</summary>

Фильтры символов, один токенизатор, фильтры токенов — в этом порядке.
</details>

2. Почему `term` не находит «Hello World» в `text`?
<details><summary>Ответ</summary>

Запрос не анализируется, а в индексе термы `hello` и `world`.
</details>

3. Как искать и по словам, и точно?
<details><summary>Ответ</summary>

Multi-fields: `text` с подполем `keyword`.
</details>

4. Как изменить тип поля?
<details><summary>Ответ</summary>

Новый индекс с нужным отображением, reindex, переключение псевдонима.
</details>

5. `object` против `nested`?
<details><summary>Ответ</summary>

`object` разворачивается в плоские поля и теряет связь внутри элемента; `nested` хранит элементы скрытыми документами.
</details>

6. Что такое doc values?
<details><summary>Ответ</summary>

Колоночное представление на диске для сортировки и агрегаций.
</details>

7. Зачем `search_analyzer`?
<details><summary>Ответ</summary>

Чтобы запрос анализировался иначе, чем документ, например при автодополнении на `edge_ngram`.
</details>

8. Что такое mapping explosion?
<details><summary>Ответ</summary>

Неограниченный рост числа полей, раздувающий состояние кластера; помогают `dynamic: strict`, лимит полей, `flat_object`.
</details>

9. Что делает `dynamic: false`?
<details><summary>Ответ</summary>

Новые поля остаются в `_source`, но не индексируются.
</details>

10. Чем опасно отключение `_source`?
<details><summary>Ответ</summary>

Перестают работать `update` и reindex, нечего вернуть в результатах.
</details>

11. Как проверить, что делает анализатор?
<details><summary>Ответ</summary>

`_analyze`.
</details>

12. Чем `normalizer` отличается от анализатора?
<details><summary>Ответ</summary>

Он для `keyword`: без токенизации, значение остаётся одним термом.
</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md) · [Квиз](quiz.ru.md)
- [03 · Поисковые запросы](../03-search-queries/expert.ru.md)

## Источники

- [Supported field types](https://opensearch.org/docs/latest/field-types/supported-field-types/index/)
- [Mapping parameters](https://opensearch.org/docs/latest/field-types/mapping-parameters/index/)
- [Token filters](https://opensearch.org/docs/latest/analyzers/token-filters/index/)
