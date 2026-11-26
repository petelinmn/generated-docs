---
title: "Квиз: транзакции"
lang: ru
track: postgresql
kind: module
doc: quiz
slug: 06-transactions
part: 2
tags: [transaction, isolation-level, mvcc, locks, deadlock]
versions: "PostgreSQL 17+"
status: verified
updated: 2026-11-26
---

# Квиз: транзакции

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает уровень: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Какое свойство означает «всё или ничего»?

- A) Атомарность
- B) Согласованность
- C) Изоляция
- D) Долговечность

<details><summary>Ответ</summary>

**A.**

→ [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Оператор внутри `BEGIN` завершился ошибкой. Что можно сделать дальше?

- A) Продолжать другими операторами
- B) Только откатиться, полностью или к точке сохранения
- C) Зафиксировать успешную часть через `COMMIT`
- D) Ничего, соединение закрыто

<details><summary>Ответ</summary>

**B.** `COMMIT` прерванной транзакции откатывает её.

→ [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Уровень изоляции PostgreSQL по умолчанию

- A) Read Uncommitted
- B) Read Committed
- C) Repeatable Read
- D) Serializable

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 На Read Committed могут ли два одинаковых `SELECT` в одной транзакции дать разное?

- A) Нет, никогда
- B) Да, если между ними другая транзакция зафиксировала изменения
- C) Только из-за незафиксированных данных
- D) Только после `SAVEPOINT`

<details><summary>Ответ</summary>

**B.** Каждый оператор берёт новый снимок.

→ [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 Остаток 100. Две сессии прочитали его и записали `100 - 10` и `100 - 20`. Итог?

- A) 70
- B) 80 или 90, смотря кто пишет последним
- C) 100
- D) Ошибка

<details><summary>Ответ</summary>

**B.** Потерянное обновление; в порядке из модуля получилось 80.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Какой будет остаток?

```sql
-- balance = 100; session A and then session B run, A commits after B started waiting
UPDATE accounts SET balance = balance - 10 WHERE id = 1;   -- A, inside a transaction
UPDATE accounts SET balance = balance - 20 WHERE id = 1;   -- B
```

<details><summary>Ответ</summary>

```text
70
```

B ждёт A и применяет своё изменение к новой версии строки.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Выбери все верные утверждения о `FOR UPDATE`

- [ ] A) Обычный `SELECT` строки по-прежнему работает
- [ ] B) `NOWAIT` выдаёт ошибку вместо ожидания
- [ ] C) `SKIP LOCKED` ждёт заблокированные строки
- [ ] D) Другой `UPDATE` этой строки ждёт

<details><summary>Ответ</summary>

**A, B, D.** `SKIP LOCKED` их пропускает.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Два дежурных врача снимают себя, проверив, что дежурят двое. Какой уровень не даст остаться без врачей?

- A) Read Committed
- B) Repeatable Read
- C) Serializable
- D) Любой

<details><summary>Ответ</summary>

**C.** Это write skew: пишутся разные строки, поэтому Repeatable Read это допускает.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🟡 Что делать после `ERROR: deadlock detected`?

- A) Повторить последний оператор
- B) Откатиться и повторить всю транзакцию
- C) Перезапустить сервер
- D) Игнорировать

<details><summary>Ответ</summary>

**B.** Транзакция прервана.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 10. 🔴 Что выведет код для новой строки?

```sql
CREATE TABLE t (id int GENERATED ALWAYS AS IDENTITY, v int);
BEGIN; INSERT INTO t(v) VALUES (1); ROLLBACK;
INSERT INTO t(v) VALUES (2) RETURNING id;
```

<details><summary>Ответ</summary>

```text
2
```

Значения последовательности при откате не возвращаются.

→ [Экспертная теория](expert.ru.md)

</details>

## 11. 🔴 Какая блокировка останавливает обычный `SELECT`?

- A) `FOR UPDATE`
- B) `ROW EXCLUSIVE`
- C) `ACCESS EXCLUSIVE`
- D) `SHARE`

<details><summary>Ответ</summary>

**C.** Только `ACCESS EXCLUSIVE`.

→ [Экспертная теория](expert.ru.md)

</details>

## 12. 🔴 Открытый вопрос: как не продать последний товар дважды?

<details><summary>Ключевые пункты ответа</summary>

- Условное атомарное обновление `... SET stock = stock - 1 WHERE id = ? AND stock > 0` и проверка числа изменённых строк.
- Ограничение `CHECK (stock >= 0)` как страховка.
- `FOR UPDATE` или Serializable, когда решение требует нескольких строк; повторы.

→ [Экспертная теория](expert.ru.md)

</details>
