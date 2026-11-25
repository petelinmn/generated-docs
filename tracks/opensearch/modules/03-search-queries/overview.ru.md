---
title: "Поисковые запросы"
lang: ru
track: opensearch
kind: module
doc: overview
slug: 03-search-queries
part: 1
tags: [query-dsl, full-text-search, bool-query, relevance, bm25, pagination]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-25
---

# Поисковые запросы

🌐 **Русский** · [English](overview.en.md)

## Коротко

**Query DSL** — JSON-язык запросов OpenSearch. Запросы делятся на **полнотекстовые** (анализируют строку) и **уровня термов** (ищут значение как есть). `bool` объединяет их. В контексте запроса вычисляется оценка релевантности `_score`, в контексте фильтра — нет.

## Ключевые идеи

- `match` для текста, `term` для `keyword`, чисел и дат.
- То, что не должно влиять на ранжирование, кладите в `filter`.
- `should` без `must` и `filter` требует хотя бы одного совпадения; рядом с `filter` — не требует.
- Глубокую постраничность делают через `search_after`, а не `from`.

## Минимальный пример

```python
client.search(index="shop", body={"query": {"bool": {
    "must": [{"match": {"description": "keyboard"}}],
    "filter": [{"term": {"category": "electronics"}}, {"range": {"stock": {"gt": 0}}}]}}})
```

## Уровни материала

| Уровень | Файл | Добавить |
|---|---|---|
| 1. Базовая теория | [basics.ru.md](basics.ru.md) | |
| 2. Продвинутая теория | [advanced.ru.md](advanced.ru.md) | |
| 3. Экспертная теория | [expert.ru.md](expert.ru.md) | |
| 4. Самопроверка | [quiz.ru.md](quiz.ru.md), [exercises.ru.md](exercises.ru.md) | |
| 5. Запускаемые примеры | — | `/learn opensearch 03-search-queries level 5` |
| 6. Тесты | — | `/learn opensearch 03-search-queries level 6` |

## Связанные материалы

- [Трек OpenSearch](../../overview.ru.md)
- [02 · Отображения и анализ текста](../02-mappings-and-analysis/overview.ru.md)
- [01 · Основы OpenSearch](../01-opensearch-basics/overview.ru.md)
- [04 · Агрегации](../04-aggregations/overview.ru.md)
- [06 · Релевантность](../06-relevance-tuning/overview.ru.md)
