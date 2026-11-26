---
title: "Производительность и EXPLAIN"
lang: ru
track: postgresql
kind: module
doc: overview
slug: 09-performance-explain
part: 2
levels: [1, 2, 3, 4]
tags: [explain, query-plan, statistics, vacuum, performance]
versions: "PostgreSQL 17+"
status: verified
updated: 2026-11-26
---

# Производительность и EXPLAIN

🌐 **Русский** · [English](overview.en.md)

## Коротко

Планировщик выбирает план по оценке стоимости, а оценки берутся из статистики таблиц. Большинство медленных запросов объясняется одной из немногих причин: план читает гораздо больше строк, чем возвращает, оценки неверны, операция не помещается в память или таблица раздута мёртвыми строками. `EXPLAIN` показывает план, `EXPLAIN ANALYZE` выполняет его и показывает, что произошло на самом деле, а `pg_stat_statements` подсказывает, какие запросы заслуживают внимания.

## Ключевые идеи

- Сначала найдите дорогие запросы по суммарному времени, а не по ощущению.
- Сравнивайте оценку `rows` с фактическими строками: большой разрыв означает плохую статистику.
- `EXPLAIN ANALYZE` по-настоящему выполняет оператор.
- Фактические строки и время указаны на одну итерацию: умножайте на `loops`.
- Сортировка или хэш, не поместившиеся в `work_mem`, уходят на диск.
- `ANALYZE` обновляет статистику, `VACUUM` делает место мёртвых строк пригодным для повторного использования.
- Настройку, влияющую на планы, меняют по данным, а не по совету.

## Минимальный пример

```sql
CREATE TABLE orders (
    id int PRIMARY KEY, customer_id int NOT NULL, status text NOT NULL,
    city text NOT NULL, country text NOT NULL,
    amount numeric(10,2) NOT NULL, created_at timestamptz NOT NULL
);
INSERT INTO orders
SELECT i, i % 5000,
       CASE WHEN i % 50 = 0 THEN 'new' ELSE 'done' END,
       'city' || (i % 100), 'country' || (i % 100 / 10),
       (i % 1000) / 10.0,
       timestamptz '2026-01-01 00:00+00' + i * interval '30 seconds'
FROM generate_series(1, 200000) i;
CREATE INDEX orders_customer ON orders (customer_id);
VACUUM ANALYZE orders;
SET max_parallel_workers_per_gather = 0;   -- simpler plans for the examples
```

```sql
EXPLAIN (ANALYZE, BUFFERS, TIMING OFF) SELECT * FROM orders WHERE customer_id = 42;
```

```text
Bitmap Heap Scan on orders  (cost=4.60..147.55 rows=40 width=41) (actual rows=40 loops=1)
  Recheck Cond: (customer_id = 42)
  Heap Blocks: exact=40
  Buffers: shared hit=40 read=2
  ->  Bitmap Index Scan on orders_customer  (cost=0.00..4.59 rows=40 width=0) (actual rows=40 loops=1)
        Index Cond: (customer_id = 42)
        Buffers: shared read=2
Planning Time: 0.036 ms
Execution Time: 0.095 ms
```

## Когда применять, а когда нет

- ✅ Запрос медленный или его время выросло.
- ✅ До и после добавления индекса или переписывания запроса.
- ❌ Настраивать сервер, не зная дорогих запросов.
- ❌ Судить о плане на базе из нескольких строк.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | `EXPLAIN`, стоимость, `EXPLAIN ANALYZE`, буферы |
| [2. Продвинутая теория](advanced.ru.md) | оценки и статистика, память, итерации, `OFFSET`, `VACUUM` и раздувание |
| [3. Экспертная теория](expert.ru.md) | общие планы, `pg_stat_statements`, справочник, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | ещё не создан — `/learn postgresql 09-performance-explain level 5` |
| 6. Тесты | ещё не создан — `/learn postgresql 09-performance-explain level 6` |

## Связанные материалы

- Пререквизиты: [05 · Индексы](../05-indexes/overview.ru.md), [06 · Транзакции](../06-transactions/overview.ru.md)
- Дальше: следующий модуль по [дорожной карте PostgreSQL](../../overview.ru.md)
- Измерение всей системы: [наблюдаемость и SLO](../../../system-design/modules/14-observability-and-slo/overview.ru.md)
- Из .NET: [EF Core](../../../csharp/modules/15-ef-core/overview.ru.md)
- [Планы запросов и движки хранения: хранение данных](../../../system-design/modules/04-data-storage/overview.ru.md)
