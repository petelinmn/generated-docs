---
title: "Транзакции: экспертная теория"
lang: ru
track: postgresql
kind: module
doc: expert
slug: 06-transactions
part: 2
tags: [transaction, isolation-level, mvcc, locks, deadlock]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-26
---

# Транзакции: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

## Под капотом

### Версии строк

В каждой версии строки записаны `xmin` — создавшая её транзакция, и `xmax`, отмечающий удаление или блокировку. `ctid` — физическое положение. `UPDATE` записывает новую версию в другое место:

```sql
SELECT xmin, xmax, ctid, balance FROM accounts WHERE id = 1;   -- 790 | 0 | (0,1) | 100
UPDATE accounts SET balance = 101 WHERE id = 1;
SELECT xmin, xmax, ctid, balance FROM accounts WHERE id = 1;   -- 791 | 0 | (0,3) | 101
```

Снимок решает, какие версии видны. Старые версии позже убирает `VACUUM`. Ненулевой `xmax` не обязательно означает, что строка удалена: удаляющая транзакция может быть не завершена или отменена. `ctid` меняется при обновлениях и при `VACUUM FULL`, поэтому идентификатором он не является.

### Переполнение идентификаторов транзакций

Идентификаторы транзакций 32-битные и идут по кругу. `VACUUM` замораживает старые строки, чтобы они оставались «в прошлом». Если заморозка сильно отстаёт, PostgreSQL выдаёт предупреждения, а когда остаётся меньше трёх миллионов идентификаторов, перестаёт выдавать новые.

### Serializable

Serializable работает на снимках, как Repeatable Read, и дополнительно отслеживает зависимости чтения и записи. Когда возникает опасная картина, одна транзакция отменяется с кодом `40001`. Он не блокирует так, как строгие блокировки, но требует повторов. Документация советует: объявлять `READ ONLY`, где можно, ограничивать число активных соединений пулом, не допускать «idle in transaction» и убирать ставшие ненужными явные блокировки.

## Справочник

| Блокировка строки | Кто берёт | Конфликтует с |
|---|---|---|
| `FOR UPDATE` | `DELETE`, `UPDATE` ключевых колонок, явно | всеми четырьмя режимами |
| `FOR NO KEY UPDATE` | прочие `UPDATE`, явно | `FOR SHARE`, `FOR NO KEY UPDATE`, `FOR UPDATE` |
| `FOR SHARE` | явно | `FOR NO KEY UPDATE`, `FOR UPDATE` |
| `FOR KEY SHARE` | проверки внешних ключей, явно | `FOR UPDATE` |

Режимов табличных блокировок восемь: от `ACCESS SHARE` (берёт `SELECT`) до `ACCESS EXCLUSIVE` (например `DROP TABLE`, `TRUNCATE`, `VACUUM FULL`). Обычный `SELECT` блокирует только `ACCESS EXCLUSIVE`.

| Оператор | Действие |
|---|---|
| `BEGIN ISOLATION LEVEL ...` | уровень этой транзакции |
| `SET TRANSACTION ...` | то же, до первого запроса |
| `BEGIN READ ONLY` | запрет записи |
| `SAVEPOINT`, `ROLLBACK TO`, `RELEASE` | частичный откат |
| `LOCK TABLE ... IN ... MODE` | явная табличная блокировка |
| `pg_advisory_lock`, `pg_advisory_xact_lock` | прикладные блокировки на сессию или транзакцию |

| Настройка | По умолчанию | Что ограничивает |
|---|---|---|
| `default_transaction_isolation` | `read committed` | уровень новых транзакций |
| `deadlock_timeout` | `1s` | ожидание перед проверкой на deadlock |
| `lock_timeout` | `0` (выключено) | ожидание блокировки |
| `statement_timeout` | `0` (выключено) | длительность оператора |
| `idle_in_transaction_session_timeout` | `0` (выключено) | простой внутри транзакции |
| `transaction_timeout` | `0` (выключено) | длительность транзакции |

Куда смотреть, когда что-то ждёт: `pg_stat_activity` (`state`, `wait_event_type`, `xact_start`) и `pg_locks`; `pg_blocking_pids(pid)` возвращает, кто блокирует процесс.

## Подвохи

### Снимок Repeatable Read начинается с первого запроса

```sql
-- A                                          -- B
BEGIN ISOLATION LEVEL REPEATABLE READ;
                                              UPDATE accounts SET balance = 150 WHERE id = 1;
SELECT balance FROM accounts WHERE id = 1;    -- 150
COMMIT;
```

### Последовательности не замечают отката

```sql
CREATE TABLE t (id int GENERATED ALWAYS AS IDENTITY, v int);
BEGIN;
INSERT INTO t(v) VALUES (1);
ROLLBACK;
INSERT INTO t(v) VALUES (2) RETURNING id;   -- 2, not 1
```

### now() замирает внутри транзакции

```sql
BEGIN;
SELECT now() AS t1 \gset
SELECT pg_sleep(0.2);
SELECT now() = :'t1'::timestamptz            AS now_same,      -- t
       clock_timestamp() > :'t1'::timestamptz AS clock_moved;  -- t
COMMIT;
```

`now()` возвращает время начала транзакции; для измерения прошедшего времени используйте `clock_timestamp()`.

### DDL транзакционен

```sql
BEGIN;
CREATE TABLE temp_x (a int);
ROLLBACK;
SELECT to_regclass('temp_x') IS NULL AS gone;   -- t
```

В отличие от некоторых других баз, миграцию можно откатить целиком. Команды вроде `CREATE INDEX CONCURRENTLY` внутри транзакции выполнять нельзя.

## Компромиссы и альтернативы

| Подход | Плюс | Минус |
|---|---|---|
| Атомарный `UPDATE ... SET x = x - 1` | просто, без повторов | только для логики, выразимой в SQL |
| `SELECT FOR UPDATE` | предсказуемо | ожидание, deadlock |
| Колонка версии (оптимистично) | нет блокировок между чтением и записью | повторы, код в приложении |
| Repeatable Read | устойчивые отчёты | ошибки сериализации, write skew остаётся |
| Serializable | все правила соблюдаются, если каждая транзакция корректна сама по себе | повторы, накладные расходы на отслеживание |
| Ограничения (`UNIQUE`, `CHECK`, `EXCLUDE`) | действуют на любом уровне | только то, что ими можно выразить |

## Вопросы с собеседований

**1. Какой уровень изоляции по умолчанию?**

<details><summary>Ответ</summary>

Read Committed.

</details>

**2. Бывают ли в PostgreSQL грязные чтения?**

<details><summary>Ответ</summary>

Нет. Read Uncommitted ведёт себя как Read Committed.

</details>

**3. Чем Repeatable Read отличается от минимума стандарта SQL?**

<details><summary>Ответ</summary>

В PostgreSQL он не допускает и фантомы, но аномалии сериализации остаются возможными.

</details>

**4. Что происходит с `UPDATE` на Read Committed, когда строку меняют?**

<details><summary>Ответ</summary>

Он ждёт, затем заново проверяет условие `WHERE` на новой версии и применяет изменение к ней.

</details>

**5. Что такое write skew?**

<details><summary>Ответ</summary>

Транзакции читают пересекающиеся данные и пишут разные строки, нарушая правило, которое ни одна по отдельности не нарушает. Помогают Serializable или явные блокировки.

</details>

**6. Что делать при SQLSTATE 40001?**

<details><summary>Ответ</summary>

Повторить всю транзакцию, с ограничением числа попыток и паузой.

</details>

**7. Блокирует ли `SELECT ... FOR UPDATE` читателей?**

<details><summary>Ответ</summary>

Обычный `SELECT` не блокируется. Блокируются другие писатели и блокирующие чтения той же строки.

</details>

**8. Как избежать deadlock?**

<details><summary>Ответ</summary>

Блокировать объекты в согласованном порядке, держать транзакции короткими и всё равно быть готовым повторить.

</details>

**9. Чем вредна долгая транзакция?**

<details><summary>Ответ</summary>

Она не даёт `VACUUM` убрать старые версии строк и держит свои блокировки.

</details>

**10. Подвох: транзакцию откатили. Будет ли значение identity использовано повторно?**

<details><summary>Ответ</summary>

Нет. Значения последовательности не возвращаются, пропуски нормальны.

</details>

**11. Подвох: когда берётся снимок транзакции Repeatable Read?**

<details><summary>Ответ</summary>

При первом запросе, а не при `BEGIN`.

</details>

**12. Что такое advisory lock?**

<details><summary>Ответ</summary>

Блокировка по числу, выбранному приложением; база не придаёт ему смысла. Держится до конца сессии или транзакции.

</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [Квиз](quiz.ru.md) и [упражнения](exercises.ru.md)

## Источники

- [PostgreSQL 17: изоляция транзакций](https://www.postgresql.org/docs/17/transaction-iso.html)
- [PostgreSQL 17: явные блокировки](https://www.postgresql.org/docs/17/explicit-locking.html)
- [PostgreSQL 17: системные колонки](https://www.postgresql.org/docs/17/ddl-system-columns.html)
- [PostgreSQL 17: регулярная очистка](https://www.postgresql.org/docs/17/routine-vacuuming.html)
- [PostgreSQL 17: настройки клиентских соединений](https://www.postgresql.org/docs/17/runtime-config-client.html)
- [PostgreSQL 17: настройки блокировок](https://www.postgresql.org/docs/17/runtime-config-locks.html)
- [PostgreSQL 17: функции последовательностей](https://www.postgresql.org/docs/17/functions-sequence.html)
