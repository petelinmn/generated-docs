---
title: "Релевантность"
lang: ru
track: opensearch
kind: module
doc: overview
slug: 06-relevance-tuning
part: 2
tags: [relevance, bm25, boosting, function-score, synonyms]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-25
---

# Релевантность

🌐 **Русский** · [English](overview.en.md)

## Коротко

Как управлять порядком результатов: оценка **BM25**, веса полей, сочетание текстовой оценки с популярностью и свежестью, **синонимы** и измерение качества.

## Ключевые идеи

- Сначала поймите оценку через `explain`, потом меняйте.
- Бустинг поля и тип `multi_match` решают большинство задач.
- Сырое числовое поле в множителе легко заглушает текст.
- Синонимы удобнее применять при поиске.
- Изменения проверяют на наборе оценённых запросов.

## Минимальный пример

```python
client.search(index="art", body={"query": {"function_score": {
    "query": {"multi_match": {"query": "search", "fields": ["title^3", "body"]}},
    "field_value_factor": {"field": "views", "modifier": "log1p"},
    "boost_mode": "sum"}}})
```

## Когда применять

Когда выдача «технически верная, но неудобная». Не стоит крутить веса без измеримой цели.

## Уровни материала

| Уровень | Файл | Добавить |
|---|---|---|
| 1. Базовая теория | [basics.ru.md](basics.ru.md) | |
| 2. Продвинутая теория | [advanced.ru.md](advanced.ru.md) | |
| 3. Экспертная теория | [expert.ru.md](expert.ru.md) | |
| 4. Самопроверка | [quiz.ru.md](quiz.ru.md), [exercises.ru.md](exercises.ru.md) | |
| 5. Запускаемые примеры | — | `/learn opensearch 06-relevance-tuning level 5` |
| 6. Тесты | — | `/learn opensearch 06-relevance-tuning level 6` |

## Связанные материалы

- [Трек OpenSearch](../../overview.ru.md)
- [03 · Поисковые запросы](../03-search-queries/overview.ru.md)
- [02 · Отображения и анализ текста](../02-mappings-and-analysis/overview.ru.md)
- [09 · Векторный и гибридный поиск](../09-vector-search/overview.ru.md)
