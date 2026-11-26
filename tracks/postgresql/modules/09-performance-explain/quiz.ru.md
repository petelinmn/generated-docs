---
title: "Квиз: производительность и EXPLAIN"
lang: ru
track: postgresql
kind: module
doc: quiz
slug: 09-performance-explain
part: 2
tags: [explain, query-plan, statistics, vacuum, performance]
versions: "PostgreSQL 17+"
status: draft
updated: 2026-11-26
---

# Квиз: производительность и EXPLAIN

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответь сам, потом раскрывай ответ. Отметка показывает уровень: 🟢 [базовая теория](basics.ru.md) · 🟡 [продвинутая](advanced.ru.md) · 🔴 [экспертная](expert.ru.md).

## 1. 🟢 Что в `cost=4.60..147.55` означает 4.60?

- A) Миллисекунды до первой строки
- B) Оценка стоимости запуска
- C) Прочитано строк
- D) Память в КБ

<details><summary>Ответ</summary>

**B.** Стоимость — единицы планировщика, а не время.

→ [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Что делает `EXPLAIN ANALYZE DELETE ...`?

- A) Только показывает план
- B) Удаляет строки и показывает план с фактическими данными
- C) Ничего не удаляет
- D) Выдаёт ошибку

<details><summary>Ответ</summary>

**B.** Оборачивайте в `BEGIN ... ROLLBACK`.

→ [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 `Buffers: shared hit=40 read=2` означает

- A) 40 страниц из кэша PostgreSQL, 2 запрошены у ОС
- B) 40 ошибок и 2 повтора
- C) 40 строк прочитаны дважды
- D) 42 КБ

<details><summary>Ответ</summary>

**A.** Страница — 8 КБ.

→ [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Какой признак указывает на плохую статистику?

- A) Высокая стоимость
- B) Оценка строк далека от фактических
- C) `Seq Scan`
- D) Много итераций

<details><summary>Ответ</summary>

**B.**

→ [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 `actual rows=40 loops=5`. Сколько строк узел вернул всего?

- A) 40
- B) 45
- C) 200
- D) 8

<details><summary>Ответ</summary>

**C.** Значения указаны на одну итерацию.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 `Sort Method: external merge Disk: 11408kB`. Что поможет?

- A) `VACUUM FULL`
- B) Больше `work_mem` для этой сессии или индекс, дающий порядок
- C) Меньший `random_page_cost`
- D) `ANALYZE`

<details><summary>Ответ</summary>

**B.**

→ [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Два связанных условия, оценка 201, факт 2000. Что исправит оценку?

- A) `CREATE STATISTICS ... (dependencies)` и `ANALYZE`
- B) Больший `work_mem`
- C) Индекс на каждую колонку
- D) `VACUUM`

<details><summary>Ответ</summary>

**A.** Индексы не меняют предположения о независимости.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Выбери все верные утверждения о `VACUUM`

- [ ] A) Обычный `VACUUM` позволяет повторно использовать место
- [ ] B) Обычный `VACUUM` обычно возвращает место ОС
- [ ] C) `VACUUM FULL` берёт эксклюзивную блокировку
- [ ] D) Autovacuum также анализирует таблицы

<details><summary>Ответ</summary>

**A, C, D.**

→ [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🟡 Почему эта страница медленная?

```sql
SELECT * FROM orders ORDER BY id OFFSET 100000 LIMIT 10;
```

<details><summary>Ответ</summary>

`OFFSET` читает и выбрасывает 100 000 строк: в плане `actual rows=100010`. Нужна keyset-пагинация `WHERE id > last ORDER BY id LIMIT 10`.

→ [Продвинутая теория](advanced.ru.md)

</details>

## 10. 🔴 После пяти выполнений подготовленный запрос внезапно замедлился для одного значения. Вероятная причина?

- A) Общий план
- B) Autovacuum
- C) Deadlock
- D) Сброс кэша

<details><summary>Ответ</summary>

**A.** На неравномерных данных общий план плохо подходит некоторым значениям.

→ [Экспертная теория](expert.ru.md)

</details>

## 11. 🔴 Почему `WHERE code::int = 42` не использует индекс по `code text`?

- A) Индекс недействителен
- B) Индекс хранит текст, а не результат приведения
- C) Целые нельзя индексировать
- D) Статистика устарела

<details><summary>Ответ</summary>

**B.** Сравнивайте с `'42'`.

→ [Экспертная теория](expert.ru.md)

</details>

## 12. 🔴 Открытый вопрос: пользователи жалуются, что за месяц приложение стало медленнее. Ваши шаги?

<details><summary>Ключевые пункты ответа</summary>

- `pg_stat_statements`: какие запросы занимают больше всего суммарного времени и что изменилось.
- `EXPLAIN (ANALYZE, BUFFERS)` этих запросов на данных, похожих на рабочие.
- Оценки против фактических строк, уход на диск, фильтры, выбрасывающие много строк.
- Рост таблиц, мёртвые строки, работа autovacuum, долгие транзакции.
- Одно изменение за раз, с измерением.

→ [Экспертная теория](expert.ru.md)

</details>
