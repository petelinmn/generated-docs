---
title: "Индексы: экспертная теория"
lang: ru
track: postgresql
kind: module
doc: expert
slug: 05-indexes
part: 2
tags: [index, b-tree, gin, brin, query-plan]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-26
---

# Индексы: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

## Под капотом

Таблица (heap) — это набор страниц по 8 КБ с версиями строк без определённого порядка. Запись индекса сопоставляет ключу физическое положение версии строки. MVCC хранит старые версии, а записи индекса не знают, видна ли вам версия.

- **Карта видимости (visibility map).** Один бит на страницу таблицы говорит, что все её строки видны всем; на него опираются index-only scan, устанавливает его `VACUUM`.
- **HOT updates.** Если `UPDATE` не меняет ни одной колонки, на которую ссылается какой-либо индекс (суммирующие индексы, в ядре это только BRIN, не в счёт), и новая версия помещается на той же странице, новые записи в индексах не нужны. Пониженный `fillfactor` таблицы оставляет для этого место.
- **Сопровождение B-tree.** `fillfactor` индекса по умолчанию 90. Дедупликация (`deduplicate_items`, по умолчанию включена) компактно хранит повторяющиеся ключи; для индексов с колонками `INCLUDE` она не применяется.
- **О версиях.** В PostgreSQL 18 появился skip scan для B-tree, смягчающий правило первой колонки. Утверждения об этом правиле здесь проверены на 17.

## Справочник

| Тип | Типичные операторы и применение | Несколько колонок | Уникальность | `INCLUDE` |
|---|---|---|---|---|
| B-tree | `< <= = >= >`, `BETWEEN`, `IN`, `IS NULL`, порядок | да | да | да |
| Hash | `=` | нет | нет | нет |
| GiST | геометрия, диапазоны, ближайшие соседи; зависит от класса операторов | да | нет | да |
| SP-GiST | разбиваемые структуры: точки, префиксы | нет | нет | да |
| GIN | много значений в строке: `jsonb`, массивы, полнотекстовый поиск | да | нет | нет |
| BRIN | минимум и максимум на диапазон блоков | да | нет | нет |

В индексе может быть до 32 колонок, включая колонки `INCLUDE`.

| Параметр `CREATE INDEX` | Смысл |
|---|---|
| `UNIQUE` | запрет дубликатов (B-tree) |
| `NULLS NOT DISTINCT` | считать `NULL` равными при проверке уникальности |
| `CONCURRENTLY` | построение без блокировки записи |
| `USING method` | метод доступа, по умолчанию `btree` |
| `(expression)` | индекс по выражению |
| `opclass` | например `text_pattern_ops`, `jsonb_path_ops` |
| `ASC` / `DESC`, `NULLS FIRST` / `LAST` | порядок внутри B-tree |
| `INCLUDE (cols)` | колонки-нагрузка |
| `WHERE predicate` | частичный индекс |
| `WITH (fillfactor = n)` | свободное место в страницах индекса |

| Сопровождение | Команда или представление |
|---|---|
| Статистика использования | `pg_stat_user_indexes` |
| Размер | `pg_relation_size('index_name')` |
| Перестроение | `REINDEX INDEX [CONCURRENTLY]` |
| Удаление без блокировки | `DROP INDEX CONCURRENTLY` |
| Действительность | `pg_index.indisvalid` |

## Подвохи

### «Index Scan» может означать чтение всего индекса

```sql
-- only the second column of (user_id, created_at)
EXPLAIN (ANALYZE, BUFFERS, COSTS OFF, TIMING OFF, SUMMARY OFF)
SELECT * FROM events WHERE created_at = timestamptz '2026-01-02 00:00+00';
-- Index Scan using events_user_created ...   Buffers: shared hit=6 read=382

CREATE INDEX events_time ON events (created_at);
-- Index Scan using events_time ...           Buffers: shared hit=1 read=2
```

Оба плана называются `Index Scan`; один прочитал 388 буферов, другой 3.

### Unique не значит «один NULL»

```sql
CREATE TABLE u (a int UNIQUE);
INSERT INTO u VALUES (NULL), (NULL);          -- both accepted

CREATE TABLE u2 (a int UNIQUE NULLS NOT DISTINCT);
INSERT INTO u2 VALUES (NULL);
INSERT INTO u2 VALUES (NULL);
-- ERROR:  duplicate key value violates unique constraint "u2_a_key"
```

### Внешний ключ не даёт индекса

```sql
CREATE TABLE parent (id int PRIMARY KEY);
CREATE TABLE child (id int PRIMARY KEY, parent_id int REFERENCES parent(id));
SELECT indexname FROM pg_indexes WHERE tablename = 'child';
-- child_pkey   (nothing on parent_id)
```

Тогда удаление родительской строки вынуждено сканировать дочернюю таблицу.

### NULL сортируется как наибольшее значение

```sql
CREATE TABLE s (x int);
INSERT INTO s VALUES (2), (NULL), (1);

SELECT x FROM s ORDER BY x;        -- 1, 2, NULL
SELECT x FROM s ORDER BY x DESC;   -- NULL, 2, 1
```

`ORDER BY created_at DESC LIMIT 10` вернёт строки с `NULL` первыми, если не указать `NULLS LAST`, и индекс стоит объявить так же.

## Компромиссы и альтернативы

| Выбор | Плюс | Минус |
|---|---|---|
| Больше индексов | быстрее чтение | медленнее запись, место, меньше HOT updates |
| Составной вместо нескольких | обслуживает фильтр и порядок вместе | бесполезен без первой колонки (в 17) |
| Частичный | маленький, дешёвый | обслуживает только подходящие запросы |
| Покрывающий | без обращения к таблице | индекс больше, зависит от `VACUUM` |
| BRIN вместо B-tree | крошечный | только для физически упорядоченных данных, менее точен |
| Секционирование | дешёвое удаление старых данных, индексы меньше в каждой части | сложнее схема |

## Вопросы с собеседований

**1. Какие типы индексов встроены?**

<details><summary>Ответ</summary>

B-tree (по умолчанию), Hash, GiST, SP-GiST, GIN и BRIN. `bloom` — расширение.

</details>

**2. Почему индекс не используется?**

<details><summary>Ответ</summary>

Запрос возвращает слишком большую часть таблицы, условие не соответствует индексу (функция, другой класс операторов, нет первой колонки), таблица мала или устарела статистика.

</details>

**3. Какой индекс может обеспечивать уникальность?**

<details><summary>Ответ</summary>

Только B-tree.

</details>

**4. Создаёт ли внешний ключ индекс?**

<details><summary>Ответ</summary>

По ссылающимся колонкам — нет. На ссылаемой стороне уже есть уникальный индекс.

</details>

**5. Что означает `Heap Fetches`?**

<details><summary>Ответ</summary>

Сколько раз index-only scan пришлось идти в таблицу, чтобы проверить видимость.

</details>

**6. Что такое HOT update?**

<details><summary>Ответ</summary>

Обновление без новых записей в индексах: не изменилась ни одна индексируемая колонка (суммирующие индексы вроде BRIN не в счёт) и на странице есть место.

</details>

**7. Когда BRIN хорош?**

<details><summary>Ответ</summary>

Очень большая таблица, значения колонки которой следуют физическому порядку.

</details>

**8. Что может пойти не так с `CREATE INDEX CONCURRENTLY`?**

<details><summary>Ответ</summary>

Он может завершиться ошибкой и оставить недействительный индекс, который надо удалить или перестроить.

</details>

**9. Чем `INCLUDE` отличается от добавления ключевой колонки?**

<details><summary>Ответ</summary>

Включённые колонки не входят в ключ поиска и в уникальность; они нужны только для index-only scan.

</details>

**10. Подвох: `EXPLAIN` показывает `Index Scan`. Запрос эффективен?**

<details><summary>Ответ</summary>

Не обязательно: может обходиться весь индекс. Смотрите `Index Cond`, буферы и фактические строки.

</details>

**11. Подвох: два `NULL` в уникальной колонке — ошибка?**

<details><summary>Ответ</summary>

По умолчанию нет. С `NULLS NOT DISTINCT` — да.

</details>

**12. Изменилось ли что-то в правиле первой колонки в PostgreSQL 18?**

<details><summary>Ответ</summary>

В PostgreSQL 18 появился skip scan для B-tree, и составной индекс помогает в большем числе случаев без условия на первую колонку. Этот модуль проверен на 17.

</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [Квиз](quiz.ru.md) и [упражнения](exercises.ru.md)

## Источники

- [PostgreSQL 17: типы индексов](https://www.postgresql.org/docs/17/indexes-types.html)
- [PostgreSQL 17: составные индексы](https://www.postgresql.org/docs/17/indexes-multicolumn.html)
- [PostgreSQL 17: index-only scans](https://www.postgresql.org/docs/17/indexes-index-only-scans.html)
- [PostgreSQL 17: CREATE INDEX](https://www.postgresql.org/docs/17/sql-createindex.html)
- [PostgreSQL 17: heap-only tuples](https://www.postgresql.org/docs/17/storage-hot.html)
- [PostgreSQL 18: release notes](https://www.postgresql.org/docs/18/release-18.html)
