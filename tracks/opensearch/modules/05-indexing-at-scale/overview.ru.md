---
title: "Индексация в большом объёме"
lang: ru
track: opensearch
kind: module
doc: overview
slug: 05-indexing-at-scale
part: 2
tags: [bulk, index-template, alias, reindex, rollover]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-25
---

# Индексация в большом объёме

🌐 **Русский** · [English](overview.en.md)

## Коротко

Как загружать много данных и менять структуру индекса без простоя: `_bulk`, **шаблоны индексов**, **псевдонимы** (aliases), **reindex** и **rollover**.

## Ключевые идеи

- Пакетная запись на порядки быстрее поштучной.
- Шаблон применяется только в момент создания индекса.
- Приложение обращается к псевдониму, а не к конкретному индексу.
- `reindex` копирует документы, но не настройки и не отображение.
- Rollover переключает запись на новый индекс по условиям.

## Минимальный пример

```python
client.indices.create(index="catalog-v1", body={"aliases": {"catalog": {}}})
# later: build catalog-v2, reindex, then switch atomically
client.indices.update_aliases(body={"actions": [
    {"remove": {"index": "catalog-v1", "alias": "catalog"}},
    {"add": {"index": "catalog-v2", "alias": "catalog"}}]})
```

## Когда применять

Первичная загрузка, смена отображения, индексы по времени (логи, события).

## Уровни материала

| Уровень | Файл | Добавить |
|---|---|---|
| 1. Базовая теория | [basics.ru.md](basics.ru.md) | |
| 2. Продвинутая теория | [advanced.ru.md](advanced.ru.md) | |
| 3. Экспертная теория | [expert.ru.md](expert.ru.md) | |
| 4. Самопроверка | [quiz.ru.md](quiz.ru.md), [exercises.ru.md](exercises.ru.md) | |
| 5. Запускаемые примеры | — | `/learn opensearch 05-indexing-at-scale level 5` |
| 6. Тесты | — | `/learn opensearch 05-indexing-at-scale level 6` |

## Связанные материалы

- [Трек OpenSearch](../../overview.ru.md)
- [01 · Основы OpenSearch](../01-opensearch-basics/overview.ru.md)
- [02 · Отображения и анализ текста](../02-mappings-and-analysis/overview.ru.md)
- [Производители Kafka](../../../kafka/modules/02-producers/overview.ru.md) — тоже пакетная отправка
- [07 · Кластер и шарды](../07-cluster-and-shards/overview.ru.md)
- [08 · Жизненный цикл индексов](../08-index-lifecycle/overview.ru.md)
