---
title: "Хранение данных: продвинутая теория"
lang: ru
track: system-design
kind: module
doc: advanced
slug: 04-data-storage
part: 1
tags: [system-design, data-modeling, database-index, b-tree, lsm-tree, query-plan, nosql]
versions: ".NET 11, C# 15; Microsoft.Data.Sqlite 11.0.0"
status: verified
updated: 2026-11-26
---

# Хранение данных: продвинутая теория

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · **2. Продвинутая** · [3. Экспертная](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Составные и покрывающие индексы

Индекс `(chat_id, sent_at)` упорядочен сначала по чату, внутри чата по времени. Поэтому он обслуживает фильтр по чату вместе с сортировкой по времени. Индекс `(sent_at, chat_id)` для того же запроса не даёт `SEARCH`.

Покрывающий индекс содержит все колонки запроса, и таблица не читается вовсе:

```sql
CREATE INDEX ix_cover ON messages (chat_id, sent_at, body);
```

План содержит `COVERING INDEX ix_cover`. Цена — копия `body` в индексе.

## B-tree и LSM

| | B-tree | LSM-дерево |
|---|---|---|
| Запись | изменение страницы на месте | в память, затем неизменяемые файлы |
| Чтение по ключу | один путь по дереву | возможно несколько структур |
| Фоновая работа | обслуживание страниц | compaction |
| Хорошо подходит | смешанная нагрузка, диапазоны | интенсивная запись |

В LSM удаление — это тоже запись: надгробие (tombstone), скрывающее старые значения.

```csharp
// A delete is a write of a tombstone: older segments are immutable.
public void Delete(string key) => Write(key, null);
```

Чтение идёт от новых структур к старым, и отсутствующий ключ приходится искать везде:

```csharp
for (int i = _segments.Count - 1; i >= 0; i--) // newest wins
{
    structuresChecked++;
    if (_segments[i].TryGetValue(key, out value)) return value;
}
```

Compaction объединяет сегменты и выбрасывает старые версии. Файл: [Ex01_LsmStore.cs](code/Examples/Ex01_LsmStore.cs). Настоящие реализации добавляют журнал предзаписи и фильтры Блума.

## Нормализация и денормализация

Нормализованная схема хранит каждый факт один раз. Денормализованная копирует имя автора в сообщение, чтобы не делать join. Копию придётся обновлять; решите, кто это делает и насколько она может отставать.

## Лучшие практики

- Выписывайте запросы и их частоту до схемы.
- Проверяйте планы на объёме, близком к настоящему.
- Удаляйте неиспользуемые индексы.
- Предусматривайте миграции: схема будет меняться.

## Частые ошибки

- ❌ Три отдельных индекса вместо одного составного → ✅ индекс под запрос.
- ❌ Функция над колонкой в `WHERE` → ✅ индекс по выражению или преобразованные данные.
- ❌ Случайный первичный ключ там, где база упорядочивает строки по нему → ✅ упорядоченные идентификаторы (например, UUID v7).
- ❌ Писать в две базы из кода по очереди → ✅ один источник истины и распространение изменений.

## Сравнение с другими треками

- Типы и ограничения — [DDL в PostgreSQL](../../../postgresql/modules/03-data-modeling-ddl/overview.ru.md).
- Доступ из .NET — [EF Core](../../../csharp/modules/15-ef-core/overview.ru.md), [Dapper](../../../csharp/libraries/dapper/overview.ru.md).
- Другая структура индекса, инвертированная, — [OpenSearch: анализ текста](../../../opensearch/modules/02-mappings-and-analysis/overview.ru.md).

## Связанные материалы

- [Базовая теория](basics.ru.md)
- [Экспертная теория](expert.ru.md)

## Источники

- [SQLite: Query Planning](https://www.sqlite.org/queryplanner.html)
- [Azure: партиционирование данных](https://learn.microsoft.com/azure/architecture/best-practices/data-partitioning)
