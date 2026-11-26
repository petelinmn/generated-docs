---
title: "Репликация и партиционирование"
lang: ru
track: system-design
kind: module
doc: overview
slug: 06-replication-and-partitioning
part: 2
levels: [1, 2, 3, 4, 5, 6]
tags: [system-design, replication, partitioning, shard, quorum, replication-lag, version-vector]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Репликация и партиционирование

🌐 **Русский** · [English](overview.en.md)

## Коротко

Репликация хранит копии одних и тех же данных на нескольких узлах: ради отказоустойчивости и чтения. Партиционирование (шардирование) делит данные между узлами: ради объёма и записи. Обычно нужны оба приёма сразу.

## Ключевые идеи

- Чаще всего пишет один лидер, реплики повторяют его журнал.
- Асинхронная репликация быстрее, но реплика отстаёт и может потерять данные при переключении.
- Read-your-writes требует знать позицию своей записи.
- Кворумы `W + R > N` гарантируют пересечение, но не всё остальное.
- Ключ партиционирования определяет распределение нагрузки и стоимость запросов.
- Горячий ключ не лечится добавлением узлов.
- Одновременные записи нужно обнаруживать, а не затирать по часам.

## Минимальный пример

```csharp
public static bool Overlaps(int n, int w, int r) => w + r > n;
```

## Когда применять, а когда нет

- ✅ Репликация — почти всегда для важных данных.
- ✅ Партиционирование — когда объём или запись не помещаются на узел.
- ❌ Шардировать заранее без оценки.
- ❌ Считать реплику резервной копией: удаление реплицируется тоже.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | лидер и реплики, лаг, виды партиционирования |
| [2. Продвинутая теория](advanced.ru.md) | синхронность, read-your-writes, кворумы, горячие ключи |
| [3. Экспертная теория](expert.ru.md) | конфликты, version vectors, подвохи, вопросы |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| [5. Запускаемые примеры](code/Examples/) | лаг, кворумы, партиции, version vectors |
| [6. Тесты](code/Tests/) | `dotnet test` в папке `code/` |

## Связанные материалы

- Пререквизиты: [хранение данных](../04-data-storage/overview.ru.md), [consistent hashing](../02-scalability-and-load-balancing/overview.ru.md)
- В других треках: [репликация Kafka](../../../kafka/modules/07-replication-and-availability/overview.ru.md), [кластер RabbitMQ](../../../rabbitmq/modules/08-clustering/overview.ru.md), [шарды OpenSearch](../../../opensearch/modules/07-cluster-and-shards/overview.ru.md)
- [Согласованность и CAP](../07-consistency-and-cap/overview.ru.md)
- [Консенсус и координация](../12-consensus-and-coordination/overview.ru.md)
- [Несколько регионов и восстановление](../16-multi-region-and-dr/overview.ru.md)
- [Разбор задач](../17-design-case-studies/overview.ru.md)
