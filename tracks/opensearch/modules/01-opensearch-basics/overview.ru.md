---
title: "Основы OpenSearch"
lang: ru
track: opensearch
kind: module
doc: overview
slug: 01-opensearch-basics
part: 1
tags: [opensearch, index, document, shard, replica, refresh, bulk, optimistic-concurrency]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-25
---

# Основы OpenSearch

🌐 **Русский** · [English](overview.en.md)

## Коротко

OpenSearch хранит JSON-**документы** в **индексах** и ищет по ним. Индекс разбит на **шарды**, шарды распределены по **узлам** **кластера**. Все операции — HTTP-запросы к REST API.

Главное, что отличает его от базы данных: запись по `_id` видна чтению по `_id` сразу, а **поиску** — только после обновления (refresh), по умолчанию раз в секунду.

## Ключевые идеи

- Документ имеет `_id`, `_source` (исходный JSON) и версию.
- `index` перезаписывает документ целиком, `create` отказывает, если `_id` занят, `update` меняет часть полей.
- Конкурентные изменения защищаются парой `if_seq_no` и `if_primary_term`.
- Число основных шардов задаётся при создании индекса; число реплик можно менять.
- Много документов записывают через `_bulk`, и ошибки проверяют по каждой операции.

## Минимальный пример

```python
from opensearchpy import OpenSearch

client = OpenSearch(hosts=[{"host": "localhost", "port": 9200}])
client.index(index="products", id="1", body={"name": "Blue mug", "price": 12.5}, params={"refresh": "true"})
print(client.search(index="products", body={"query": {"match": {"name": "mug"}}})["hits"]["total"])
```

## Когда использовать

- Полнотекстовый поиск, фильтры и фасеты по каталогу, поиск по логам, аналитика по большим объёмам.

**Не использовать** как единственное хранилище с транзакциями и связями между сущностями.

## Уровни материала

| Уровень | Файл | Добавить |
|---|---|---|
| 1. Базовая теория | [basics.ru.md](basics.ru.md) | |
| 2. Продвинутая теория | [advanced.ru.md](advanced.ru.md) | |
| 3. Экспертная теория | [expert.ru.md](expert.ru.md) | |
| 4. Самопроверка | [quiz.ru.md](quiz.ru.md), [exercises.ru.md](exercises.ru.md) | |
| 5. Запускаемые примеры | — | `/learn opensearch 01-opensearch-basics level 5` |
| 6. Тесты | — | `/learn opensearch 01-opensearch-basics level 6` |

## Связанные материалы

- [Трек OpenSearch](../../overview.ru.md)
- [02 · Отображения и анализ текста](../02-mappings-and-analysis/overview.ru.md)
- [Основы Kafka](../../../kafka/modules/01-kafka-basics/overview.ru.md) — другой распределённый сервис с партициями и репликами
- [05 · Индексация в большом объёме](../05-indexing-at-scale/overview.ru.md)
