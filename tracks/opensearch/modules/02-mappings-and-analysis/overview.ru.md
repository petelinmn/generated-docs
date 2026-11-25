---
title: "Отображения и анализ текста"
lang: ru
track: opensearch
kind: module
doc: overview
slug: 02-mappings-and-analysis
part: 1
tags: [mapping, analyzer, inverted-index, text-keyword, nested, dynamic-mapping]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-25
---

# Отображения и анализ текста

🌐 **Русский** · [English](overview.en.md)

## Коротко

**Отображение** (mapping) — схема индекса: какие есть поля и как их индексировать. Для текста **анализатор** превращает строку в термы, по которым затем ищут. Поле `text` ищут по словам, поле `keyword` — только целиком.

## Ключевые идеи

- Анализатор = фильтры символов → токенизатор → фильтры токенов.
- Динамическое отображение угадывает типы по первому документу, и угадывает не всегда удачно.
- Тип существующего поля изменить нельзя — нужен новый индекс.
- `term` не анализирует запрос, `match` анализирует.
- Массивы объектов теряют связь полей внутри элемента, если не объявить `nested`.

## Минимальный пример

```python
client.indices.create(index="goods", body={"mappings": {"properties": {
    "name": {"type": "text", "analyzer": "english", "fields": {"raw": {"type": "keyword"}}},
    "category": {"type": "keyword"}, "price": {"type": "double"}}}})
```

## Когда это важно

- Всегда, когда индекс живёт дольше эксперимента: отображение задают явно.

## Уровни материала

| Уровень | Файл | Добавить |
|---|---|---|
| 1. Базовая теория | [basics.ru.md](basics.ru.md) | |
| 2. Продвинутая теория | [advanced.ru.md](advanced.ru.md) | |
| 3. Экспертная теория | [expert.ru.md](expert.ru.md) | |
| 4. Самопроверка | [quiz.ru.md](quiz.ru.md), [exercises.ru.md](exercises.ru.md) | |
| 5. Запускаемые примеры | — | `/learn opensearch 02-mappings-and-analysis level 5` |
| 6. Тесты | — | `/learn opensearch 02-mappings-and-analysis level 6` |

## Связанные материалы

- [Трек OpenSearch](../../overview.ru.md)
- [01 · Основы OpenSearch](../01-opensearch-basics/overview.ru.md)
- [03 · Поисковые запросы](../03-search-queries/overview.ru.md)
- [04 · Агрегации](../04-aggregations/overview.ru.md)
- [05 · Индексация в большом объёме](../05-indexing-at-scale/overview.ru.md)
- [06 · Релевантность](../06-relevance-tuning/overview.ru.md)
