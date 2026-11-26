---
title: "Репликация и партиционирование: продвинутая теория"
lang: ru
track: system-design
kind: module
doc: advanced
slug: 06-replication-and-partitioning
part: 2
tags: [system-design, replication, partitioning, shard, quorum, replication-lag, version-vector]
versions: ".NET 11, C# 15"
status: verified
updated: 2026-11-26
---

# Репликация и партиционирование: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Синхронная и асинхронная репликация

Синхронная: лидер подтверждает запись после реплики — потерь меньше, задержка выше, и недоступная реплика может остановить запись. Асинхронная: быстро, но последние записи могут пропасть при отказе лидера. В PostgreSQL это настраивает `synchronous_commit` (по умолчанию `on`) вместе с `synchronous_standby_names`.

## Read-your-writes

Клиент запоминает позицию своей записи в журнале; реплика годится, только если уже применила её.

```csharp
public static string? ReadYourWrites(Leader leader, Follower follower, string key, long lastWritePosition) =>
    follower.AppliedPosition >= lastWritePosition ? follower.Read(key) : leader.Read(key);
```

## Кворумы

Из N копий запись подтверждают W, чтение опрашивает R. При `W + R > N` множества пересекаются, и среди ответов есть самая новая подтверждённая версия.

```csharp
public (long Version, string? Value) Read(params int[] asked) =>
    asked.Select(i => _replicas[i]).MaxBy(r => r.Version);
```

| N | W | R | Переживает отказов для обеих операций |
|---|---|---|---|
| 3 | 2 | 2 | 1 |
| 5 | 3 | 3 | 2 |
| 3 | 3 | 1 | 0 |

Файл: [Ex02_Quorum.cs](code/Examples/Ex02_Quorum.cs).

## Горячие ключи

Один ключ получает непропорционально много запросов. Соль разбивает его на несколько, а читатель опрашивает все.

```csharp
public static string Salted(string key, long sequence, int buckets) => $"{key}#{sequence % buckets}";
```

## Ребалансировка

`hash % N` при изменении N переносит много данных: на 100 000 ключах переход с 4 на 8 партиций переместил 50,2%. Применяют [consistent hashing](../02-scalability-and-load-balancing/advanced.ru.md) или заранее создают много логических партиций и перемещают их целиком.

## Лучшие практики

- Выбирайте ключ так, чтобы частые запросы попадали в одну партицию.
- Следите за лагом и перекосом партиций.
- Репетируйте failover.
- Помните об ограничениях: в PostgreSQL уникальный ключ секционированной таблицы должен включать все колонки ключа секционирования.

## Частые ошибки

- ❌ Транзакции и join между шардами на горячем пути → ✅ ключ, объединяющий связанные данные.
- ❌ Автоматический failover без защиты от двух лидеров → ✅ fencing.
- ❌ Считать `W + R > N` полной гарантией согласованности → ✅ см. экспертный уровень.
- ❌ Менять ключ шардирования «потом» → ✅ это миграция всех данных.

## Сравнение с другими треками

- ISR и `min.insync.replicas` — [Kafka](../../../kafka/modules/07-replication-and-availability/overview.ru.md).
- Кворумные очереди — [RabbitMQ](../../../rabbitmq/modules/08-clustering/overview.ru.md).
- Первичные и реплика-шарды — [OpenSearch](../../../opensearch/modules/07-cluster-and-shards/overview.ru.md).

## Связанные материалы

- [Базовая теория](basics.ru.md)
- [Экспертная теория](expert.ru.md)

## Источники

- [PostgreSQL: настройки WAL](https://www.postgresql.org/docs/current/runtime-config-wal.html)
- [PostgreSQL: партиционирование таблиц](https://www.postgresql.org/docs/current/ddl-partitioning.html)
