---
title: "Жизненный цикл индексов"
lang: ru
track: opensearch
kind: module
doc: overview
slug: 08-index-lifecycle
part: 2
tags: [index-lifecycle, data-stream, ism, retention, rollover]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-25
---

# Жизненный цикл индексов

🌐 **Русский** · [English](overview.en.md)

## Коротко

Как хранить растущие по времени данные: **data streams** для записи и **ISM** для автоматических rollover, уплотнения и удаления.

## Ключевые идеи

- Удалить индекс дешевле, чем удалить документы.
- Data stream — одно имя, много backing-индексов.
- ISM-политика — это состояния, действия и переходы.
- ISM работает по расписанию с задержкой, а не мгновенно.
- Управляемый индекс держит ту версию политики, с которой был привязан.

## Минимальный пример

```python
api("GET", "/_data_stream/logs-app")
api("GET", "/_plugins/_ism/explain/logs-app")
```

## Когда применять

Логи, метрики, события. Не подходит для каталога товаров и других изменяемых данных.

## Уровни материала

| Уровень | Файл | Добавить |
|---|---|---|
| 1. Базовая теория | [basics.ru.md](basics.ru.md) | |
| 2. Продвинутая теория | [advanced.ru.md](advanced.ru.md) | |
| 3. Экспертная теория | [expert.ru.md](expert.ru.md) | |
| 4. Самопроверка | [quiz.ru.md](quiz.ru.md), [exercises.ru.md](exercises.ru.md) | |
| 5. Запускаемые примеры | — | `/learn opensearch 08-index-lifecycle level 5` |
| 6. Тесты | — | `/learn opensearch 08-index-lifecycle level 6` |

## Связанные материалы

- [Трек OpenSearch](../../overview.ru.md)
- [05 · Индексация в большом объёме](../05-indexing-at-scale/overview.ru.md)
- [07 · Кластер и шарды](../07-cluster-and-shards/overview.ru.md)
- [Топики и хранение в Kafka](../../../kafka/modules/04-topics-and-retention/overview.ru.md) — аналог retention
