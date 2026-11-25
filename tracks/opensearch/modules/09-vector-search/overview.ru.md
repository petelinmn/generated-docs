---
title: "Векторный и гибридный поиск"
lang: ru
track: opensearch
kind: module
doc: overview
slug: 09-vector-search
part: 2
tags: [vector-search, knn, embeddings, hybrid-search, hnsw]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-25
---

# Векторный и гибридный поиск

🌐 **Русский** · [English](overview.en.md)

## Коротко

Поиск по смыслу на эмбеддингах: поле `knn_vector`, запрос `knn`, фильтры и **гибридный поиск**, объединяющий слова и векторы.

## Ключевые идеи

- Размерность и модель фиксируют индекс.
- HNSW быстрый, но приближённый; точный поиск — `script_score`.
- Фильтр внутри `knn` и фильтр снаружи — разные вещи.
- Оценки BM25 и векторов нельзя складывать напрямую: нужна нормализация или RRF.
- Память под графы считают заранее.

## Минимальный пример

```python
client.search(index="items", body={"query": {"knn": {"v": {
    "vector": [0.85, 0.15, 0.0], "k": 3, "filter": {"term": {"category": "peripherals"}}}}}})
```

## Когда применять

Поиск по смыслу, похожие объекты, RAG. Если пользователи ищут артикулы и точные названия, начните с полнотекстового.

## Уровни материала

| Уровень | Файл | Добавить |
|---|---|---|
| 1. Базовая теория | [basics.ru.md](basics.ru.md) | |
| 2. Продвинутая теория | [advanced.ru.md](advanced.ru.md) | |
| 3. Экспертная теория | [expert.ru.md](expert.ru.md) | |
| 4. Самопроверка | [quiz.ru.md](quiz.ru.md), [exercises.ru.md](exercises.ru.md) | |
| 5. Запускаемые примеры | — | `/learn opensearch 09-vector-search level 5` |
| 6. Тесты | — | `/learn opensearch 09-vector-search level 6` |

## Связанные материалы

- [Трек OpenSearch](../../overview.ru.md)
- [03 · Поисковые запросы](../03-search-queries/overview.ru.md)
- [06 · Релевантность](../06-relevance-tuning/overview.ru.md)
