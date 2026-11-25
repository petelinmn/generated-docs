---
title: "Отображения и анализ: продвинутая теория"
lang: ru
track: opensearch
kind: module
doc: advanced
slug: 02-mappings-and-analysis
part: 1
tags: [mapping, analyzer, inverted-index, text-keyword, nested, dynamic-mapping]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Отображения и анализ: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Свой анализатор

```python
client.indices.create(index="st", body={
    "settings": {"number_of_replicas": 0, "analysis": {
        "char_filter": {"amp": {"type": "mapping", "mappings": ["& => and"]}},
        "filter": {"syn": {"type": "synonym", "synonyms": ["tv, television"]},
                   "eg": {"type": "edge_ngram", "min_gram": 2, "max_gram": 10}},
        "analyzer": {
            "my": {"type": "custom", "char_filter": ["html_strip", "amp"],
                   "tokenizer": "standard", "filter": ["lowercase", "asciifolding", "syn"]},
            "autocomplete": {"type": "custom", "tokenizer": "standard", "filter": ["lowercase", "eg"]}}}},
    "mappings": {"properties": {
        "ac": {"type": "text", "analyzer": "autocomplete", "search_analyzer": "standard"},
        "ac_bad": {"type": "text", "analyzer": "autocomplete"},
        "flat": {"type": "object"}, "nest": {"type": "nested"}}}})

analyze("<b>Café</b> & TV", "my", "st")      # ['cafe', 'and', 'tv', 'television']
analyze("Laptop", "autocomplete", "st")      # ['la', 'lap', 'lapt', 'lapto', 'laptop']
```

Порядок фильтров токенов важен: синонимы после `lowercase` видят уже приведённые к нижнему регистру термы.

## Автодополнение: разные анализаторы для записи и поиска

`edge_ngram` нужен при индексации, а запрос резать на префиксы нельзя.

```python
client.index(index="st", id="1", body={"ac": "Laptop", "ac_bad": "Laptop"}, params={"refresh": "true"})
ids({"match": {"ac": "lap"}}, "st")      # ['1']
ids({"match": {"ac": "lamp"}}, "st")     # []
ids({"match": {"ac_bad": "lamp"}}, "st") # ['1']  the query was cut into "la", "lam", "lamp"
```

## Нормализатор для `keyword`

`normalizer` применяет фильтры символ-за-символом, оставляя значение одним термом.

```python
client.indices.create(index="nz", body={
    "settings": {"analysis": {"normalizer": {"low": {"type": "custom", "filter": ["lowercase"]}}}},
    "mappings": {"properties": {"k": {"type": "keyword", "normalizer": "low"}}}})
client.index(index="nz", id="1", body={"k": "ABC"}, params={"refresh": "true"})
ids({"term": {"k": "abc"}}, "nz")            # ['1']
client.get(index="nz", id="1")["_source"]    # {'k': 'ABC'}
```

## Объекты и `nested`

```python
objs = [{"n": "a", "q": 1}, {"n": "b", "q": 2}]
client.index(index="st", id="1", body={"flat": objs, "nest": objs}, params={"refresh": "true"})

def pair(field, q):
    return {"bool": {"filter": [{"term": {field + ".n.keyword": "a"}}, {"term": {field + ".q": q}}]}}

ids(pair("flat", 2), "st")                                    # ['1']  false positive
ids({"nested": {"path": "nest", "query": pair("nest", 2)}}, "st")  # []
ids({"nested": {"path": "nest", "query": pair("nest", 1)}}, "st")  # ['1']
ids({"term": {"nest.q": 1}}, "st")                            # []  needs a nested query
```

Обычный объект разворачивается в плоские массивы `flat.n` и `flat.q`, и пара значений теряется. `nested` индексирует каждый элемент отдельным скрытым документом: для одного документа с двумя элементами `_cat/indices` показал 3 документа, а `_count` — 1. Это не бесплатно.

## Управление динамикой

```python
client.indices.create(index="m", body={"mappings": {"dynamic": "strict", "properties": {"a": {"type": "keyword"}}}})
client.index(index="m", body={"a": "1", "zz": 2})     # 400 strict_dynamic_mapping_exception

client.indices.create(index="ex", body={"mappings": {"dynamic": False, "properties": {"a": {"type": "keyword"}}}})
client.index(index="ex", id="1", body={"a": "1", "zz": "hi"}, params={"refresh": "true"})
ids({"match": {"zz": "hi"}}, "ex")                    # []  not indexed
client.get(index="ex", id="1")["_source"]             # {'a': '1', 'zz': 'hi'}  still stored
```

| `dynamic` | Незнакомое поле |
|---|---|
| `true` (по умолчанию) | добавляется в отображение |
| `false` | хранится в `_source`, не индексируется |
| `strict` | документ отклоняется |

## Приведение типов

В поле `long` строка `"7"` и число `7.9` принимаются (coercion): оба находятся запросом `term: 7`, а `_source` остаётся каким прислали. Строка `"abc"` и объект в поле `text` дают `mapper_parsing_exception`. Отключается параметром `coerce: false`.

## Частые ошибки

- Один и тот же `edge_ngram` при записи и поиске.
- Динамические ключи (например, идентификаторы как имена полей) — «взрыв отображения».
- `nested` там, где не нужна связь внутри элемента.

## Связанные материалы

- [Базовая теория](basics.ru.md) · [Эксперт](expert.ru.md)
- [Схемы в Kafka](../../../kafka/modules/06-schemas-and-serialization/basics.ru.md) — тоже эволюция структуры данных

## Источники

- [Custom analyzers](https://opensearch.org/docs/latest/analyzers/custom-analyzer/)
- [Nested field type](https://opensearch.org/docs/latest/field-types/supported-field-types/nested/)
- [Dynamic parameter](https://opensearch.org/docs/latest/field-types/mapping-parameters/dynamic/)
