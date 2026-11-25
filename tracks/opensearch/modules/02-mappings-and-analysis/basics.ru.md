---
title: "Отображения и анализ: базовая теория"
lang: ru
track: opensearch
kind: module
doc: basics
slug: 02-mappings-and-analysis
part: 1
tags: [mapping, analyzer, inverted-index, text-keyword, nested, dynamic-mapping]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Отображения и анализ: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Обратный индекс

Вместо «документ → слова» хранится «терм → список документов». Поэтому поиск по слову быстрый, а то, **какие именно термы** получились из строки, решает, найдётся ли она.

## Анализаторы

```python
def analyze(text, analyzer, index=None):
    r = client.indices.analyze(index=index, body={"text": text, "analyzer": analyzer})
    return [t["token"] for t in r["tokens"]]

text = "The QUICK brown-foxes, 2 jumped! user@mail.com"
```

| Анализатор | Результат |
|---|---|
| `standard` | `the, quick, brown, foxes, 2, jumped, user, mail.com` |
| `simple` | `the, quick, brown, foxes, jumped, user, mail, com` |
| `whitespace` | `The, QUICK, brown-foxes,, 2, jumped!, user@mail.com` |
| `stop` | `quick, brown, foxes, jumped, user, mail, com` |
| `keyword` | вся строка одним термом |
| `pattern` | `the, quick, brown, foxes, 2, jumped, user, mail, com` |
| `english` | `quick, brown, fox, 2, jump, user, mail.com` |

Для русского есть анализатор `russian`: «Быстрые лисы прыгали через ленивых собак» → `быстр, лис, прыга, ленив, собак` (стоп-слово «через» отброшено, окончания отсечены). `standard` даёт только нижний регистр: `быстрые, лисы, и, ёжик`.

## Динамическое отображение

```python
client.index(index="d", id="1", params={"refresh": "true"}, body={
    "title": "Hello World", "n": 5, "f": 1.5, "b": True, "when": "2026-01-15T10:00:00Z",
    "day": "2026-01-15", "numstr": "42", "obj": {"a": 1}, "list": [1, 2], "nul": None})
print(client.indices.get_mapping(index="d")["d"]["mappings"]["properties"])
```

| Значение | Тип |
|---|---|
| `"Hello World"`, `"42"` | `text` с подполем `keyword` (`ignore_above: 256`) |
| `5`, `[1, 2]` | `long` |
| `1.5` | `float` |
| `true` | `boolean` |
| `"2026-01-15T10:00:00Z"`, `"2026-01-15"` | `date` |
| `{"a": 1}` | объект с полем `a` |
| `null` | поле не создано |

## `text` и `keyword`

```python
def ids(query, index="d"):
    return [h["_id"] for h in client.search(index=index, body={"query": query})["hits"]["hits"]]

ids({"term": {"title": "Hello World"}})          # []      the indexed terms are "hello", "world"
ids({"term": {"title": "hello"}})                # ['1']
ids({"term": {"title.keyword": "Hello World"}})  # ['1']
ids({"match": {"title": "WORLD"}})               # ['1']   match analyzes the query
```

| | `text` | `keyword` |
|---|---|---|
| Анализируется | да | нет |
| Полнотекстовый поиск | да | нет |
| Точное совпадение, фильтры | неудобно | да |
| Сортировка и агрегации | по умолчанию нет | да |

Сортировка по `text` даёт ошибку: `Text fields are not optimised for operations that require per-document field data ...`. Используйте `title.keyword`.

## Явное отображение

```python
client.indices.create(index="goods", body={"mappings": {"properties": {
    "name": {"type": "text", "analyzer": "english", "fields": {"raw": {"type": "keyword"}}},
    "category": {"type": "keyword"},
    "price": {"type": "double"},
    "created": {"type": "date"}}}})
```

## Что можно изменить

Добавить новое поле можно. Изменить тип существующего — нельзя: `mapper [n] cannot be changed from type [long] to [keyword]`. Нужен новый индекс и перенос данных (reindex).

## Частые ошибки новичков

- Полагаться на динамическое отображение: числа в кавычках станут текстом.
- `term` по полю `text`.
- Хранить идентификаторы как `text`.
- Не проверять анализатор через `_analyze`.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [01 · Основы](../01-opensearch-basics/basics.ru.md) · [03 · Запросы](../03-search-queries/basics.ru.md)

## Источники

- [Mappings and field types](https://opensearch.org/docs/latest/field-types/)
- [Text analysis](https://opensearch.org/docs/latest/analyzers/)
- [Analyze API](https://opensearch.org/docs/latest/api-reference/analyze-apis/)
