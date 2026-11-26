---
title: "Упражнения: транзакции"
lang: ru
track: postgresql
kind: module
doc: exercises
slug: 06-transactions
part: 2
tags: [transaction, isolation-level, mvcc, locks, deadlock]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-26
---

# Упражнения: транзакции

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Таблица — из [базовой теории](basics.ru.md), нужны две сессии `psql`.

## 1. 🟢 Безопасный перевод

**Задача:** перевести 30 со счёта 1 на счёт 2 так, чтобы деньги не пропали и остаток не стал отрицательным.

<details><summary>Подсказка</summary>

Одна транзакция и условие в первом `UPDATE`.

</details>

<details><summary>Решение</summary>

```sql
BEGIN;
UPDATE accounts SET balance = balance - 30 WHERE id = 1 AND balance >= 30;
-- check that one row was updated, otherwise ROLLBACK
UPDATE accounts SET balance = balance + 30 WHERE id = 2;
COMMIT;
```

Ограничение `CHECK` защищает данные, даже если приложение забудет условие.

</details>

## 2. 🟡 Очередь заданий

**Задача:** несколько воркеров берут невыполненные задания; ни одно задание не должно достаться двоим, и никто не должен ждать другого.

<details><summary>Подсказка</summary>

Блокирующее предложение, которое пропускает уже занятые строки.

</details>

<details><summary>Решение</summary>

```sql
CREATE TABLE jobs (id int PRIMARY KEY, done boolean NOT NULL DEFAULT false);
INSERT INTO jobs (id) VALUES (1), (2), (3);

BEGIN;
SELECT id FROM jobs WHERE NOT done ORDER BY id LIMIT 1 FOR UPDATE SKIP LOCKED;
-- process the job, then mark it done
UPDATE jobs SET done = true WHERE id = 1;
COMMIT;
```

Если воркер умрёт, его транзакция откатится и задание снова станет доступно.

</details>

## 3. 🔴 Переводы без deadlock

**Задача:** переводы между двумя счетами идут одновременно в обе стороны. Сделайте deadlock между ними невозможным.

<details><summary>Подсказка</summary>

Кто какую строку блокирует первой?

</details>

<details><summary>Решение</summary>

```sql
BEGIN;
SELECT id FROM accounts WHERE id IN (1, 2) ORDER BY id FOR UPDATE;
-- both rows are locked in one fixed order, now move the money in either direction
COMMIT;
```

Каждая транзакция берёт блокировки в порядке `id`, поэтому цикл ожидания возникнуть не может.

</details>
