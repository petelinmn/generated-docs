---
title: "Кластер и шарды"
lang: ru
track: opensearch
kind: module
doc: overview
slug: 07-cluster-and-shards
part: 2
tags: [cluster, shard, replica, shard-allocation, high-availability]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-25
---

# Кластер и шарды

🌐 **Русский** · [English](overview.en.md)

## Коротко

Как OpenSearch раскладывает индекс по узлам, переживает отказы и сколько шардов заводить.

## Ключевые идеи

- Число первичных шардов задаётся при создании, реплик — меняется когда угодно.
- Копии одного шарда не лежат на одном узле.
- Для изменения состояния нужно большинство управляющих узлов-кандидатов.
- Почему шард не назначен, отвечает `_cluster/allocation/explain`.
- Реплики — не резервная копия.

## Минимальный пример

```python
client.cat.shards(h="index,shard,prirep,state,node", v=True)
client.cluster.allocation_explain(body={"index": "orders", "shard": 0, "primary": False})
```

## Когда применять

Когда кластер больше одного узла или встал вопрос «сколько шардов». Для локальной разработки хватит одного узла без реплик.

## Уровни материала

| Уровень | Файл | Добавить |
|---|---|---|
| 1. Базовая теория | [basics.ru.md](basics.ru.md) | |
| 2. Продвинутая теория | [advanced.ru.md](advanced.ru.md) | |
| 3. Экспертная теория | [expert.ru.md](expert.ru.md) | |
| 4. Самопроверка | [quiz.ru.md](quiz.ru.md), [exercises.ru.md](exercises.ru.md) | |
| 5. Запускаемые примеры | — | `/learn opensearch 07-cluster-and-shards level 5` |
| 6. Тесты | — | `/learn opensearch 07-cluster-and-shards level 6` |

## Связанные материалы

- [Трек OpenSearch](../../overview.ru.md)
- [05 · Индексация в большом объёме](../05-indexing-at-scale/overview.ru.md)
- [Репликация и доступность в Kafka](../../../kafka/modules/07-replication-and-availability/overview.ru.md) — аналог
- [Кластеры RabbitMQ](../../../rabbitmq/modules/08-clustering/overview.ru.md) — большинство и кворум
- [08 · Жизненный цикл индексов](../08-index-lifecycle/overview.ru.md)
