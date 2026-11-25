---
title: "Агрегации"
lang: ru
track: opensearch
kind: module
doc: overview
slug: 04-aggregations
part: 1
tags: [aggregations, buckets, metrics, cardinality, composite-aggregation]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-25
---

# Агрегации

🌐 **Русский** · [English](overview.en.md)

## Коротко

**Агрегации** считают сводные данные по найденным документам: метрики (суммы, средние), **корзины** (группы) и вычисления поверх корзин. Выполняются вместе с запросом и учитывают его.

## Ключевые идеи

- Метрика возвращает число, корзинная агрегация — группы, внутри групп можно считать вложенные агрегации.
- Группировать можно по `keyword`, числам и датам, но не по `text`.
- `terms` по нескольким шардам приблизителен; ошибку показывает `doc_count_error_upper_bound`.
- `cardinality` тоже приблизительна.
- Для обхода всех групп есть `composite`.

## Минимальный пример

```python
r = client.search(index="sales", body={"size": 0, "aggs": {
    "by_city": {"terms": {"field": "city"}, "aggs": {"avg_price": {"avg": {"field": "price"}}}}}})
print(r["aggregations"]["by_city"]["buckets"])
```

## Когда применять

Фасеты каталога, дашборды, статистика по логам. Для точных финансовых отчётов учитывайте приближённость `terms` и `cardinality`.

## Уровни материала

| Уровень | Файл | Добавить |
|---|---|---|
| 1. Базовая теория | [basics.ru.md](basics.ru.md) | |
| 2. Продвинутая теория | [advanced.ru.md](advanced.ru.md) | |
| 3. Экспертная теория | [expert.ru.md](expert.ru.md) | |
| 4. Самопроверка | [quiz.ru.md](quiz.ru.md), [exercises.ru.md](exercises.ru.md) | |
| 5. Запускаемые примеры | — | `/learn opensearch 04-aggregations level 5` |
| 6. Тесты | — | `/learn opensearch 04-aggregations level 6` |

## Связанные материалы

- [Трек OpenSearch](../../overview.ru.md)
- [03 · Поисковые запросы](../03-search-queries/overview.ru.md)
- [02 · Отображения и анализ текста](../02-mappings-and-analysis/overview.ru.md)
- [06 · Релевантность](../06-relevance-tuning/overview.ru.md)
