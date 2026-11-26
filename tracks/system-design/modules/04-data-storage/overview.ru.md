---
title: "Хранение данных"
lang: ru
track: system-design
kind: module
doc: overview
slug: 04-data-storage
part: 1
levels: [1, 2, 3, 4, 5, 6]
tags: [system-design, data-modeling, database-index, b-tree, lsm-tree, query-plan, nosql]
versions: ".NET 11, C# 15; Microsoft.Data.Sqlite 11.0.0"
status: verified
updated: 2026-11-26
---

# Хранение данных

🌐 **Русский** · [English](overview.en.md)

## Коротко

Хранилище выбирают по тому, как к данным обращаются: какие запросы, сколько записей и чтений, какая согласованность нужна. Этот модуль объясняет, что происходит внутри (индексы, B-tree, LSM) и почему от этого зависит выбор.

## Ключевые идеи

- Сначала выписывают паттерны доступа, потом выбирают хранилище.
- Индекс ускоряет чтение и замедляет запись.
- Порядок колонок в составном индексе решает, какие запросы он обслужит.
- B-tree обновляет страницы на месте, LSM дописывает и потом объединяет.
- NoSQL — это несколько разных моделей, а не одна.
- Денормализация ускоряет чтение ценой согласования копий.
- План запроса показывает правду, догадки — нет.

## Минимальный пример

```sql
CREATE INDEX ix_chat ON messages (chat_id, sent_at);
SELECT body FROM messages WHERE chat_id = 1 ORDER BY sent_at DESC LIMIT 20;
```

## Когда применять, а когда нет

- ✅ Проектирование новой схемы или выбор базы.
- ✅ Запрос стал медленным с ростом данных.
- ❌ Выбор «NoSQL, потому что масштабируется» без паттернов доступа.
- ❌ Индекс на каждую колонку «на всякий случай».

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | модели хранилищ, паттерны доступа, индексы |
| [2. Продвинутая теория](advanced.ru.md) | составные и покрывающие индексы, B-tree и LSM, денормализация |
| [3. Экспертная теория](expert.ru.md) | усиления, справочник, подвохи планов, вопросы |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| [5. Запускаемые примеры](code/Examples/) | игрушечный LSM, вторичный индекс, планы SQLite |
| [6. Тесты](code/Tests/) | `dotnet test` в папке `code/` |

## Связанные материалы

- Пререквизиты: [процесс проектирования](../01-design-process/overview.ru.md)
- Дальше: [репликация и партиционирование](../06-replication-and-partitioning/overview.ru.md), [проектирование API](../05-api-design/overview.ru.md)
- В других треках: [моделирование данных в PostgreSQL](../../../postgresql/modules/03-data-modeling-ddl/overview.ru.md), [EF Core](../../../csharp/modules/15-ef-core/overview.ru.md), [OpenSearch](../../../opensearch/modules/01-opensearch-basics/overview.ru.md)
- [Соединения и подзапросы в PostgreSQL](../../../postgresql/modules/04-joins-and-subqueries/overview.ru.md)
