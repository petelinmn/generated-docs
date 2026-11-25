---
title: "Квиз: агрегации"
lang: ru
track: opensearch
kind: module
doc: quiz
slug: 04-aggregations
part: 1
tags: [aggregations, buckets, metrics, cardinality, composite-aggregation]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Квиз: агрегации

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, затем раскройте ответ. 🟢 [базовый](basics.ru.md) · 🟡 [продвинутый](advanced.ru.md) · 🔴 [экспертный](expert.ru.md).

## 1. 🟢 Зачем в запросе с агрегацией `size: 0`?

- A) отключить агрегации
- B) вернуть все корзины
- C) не возвращать документы
- D) обнулить счётчики

<details><summary>Ответ</summary>

**C.**
→ [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 По какому полю можно группировать `terms`?

- A) `keyword`
- B) `text` с настройками по умолчанию
- C) `_source`
- D) любому объекту

<details><summary>Ответ</summary>

**A.**
→ [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Сколько корзин вернёт `terms` без `size`?

- A) все
- B) 1
- C) 100
- D) не более 10

<details><summary>Ответ</summary>

**D.**
→ [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 `range` с границами `from: 30, to: 100`. Цена ровно 100:

- A) вызывает ошибку
- B) не входит
- C) входит
- D) входит в обе соседние

<details><summary>Ответ</summary>

**B.** `from` включается, `to` — нет.
→ [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 Опечатка в имени поля в `sum`. Результат?

- A) сумма по всем полям
- B) 0.0 без ошибки
- C) ошибка 400
- D) `null`

<details><summary>Ответ</summary>

**B.** `avg` в том же случае даёт `null`.
→ [Экспертная теория](expert.ru.md)

</details>

## 6. 🟡 Что верно о `global`?

- [ ] A) может содержать вложенные агрегации
- [ ] B) учитывает только `filter` запроса
- [ ] C) возвращает документы
- [ ] D) игнорирует запрос

<details><summary>Ответ</summary>

**A, D.**
→ [Базовая теория](basics.ru.md)

</details>

## 7. 🟡 Нужно выгрузить все сочетания город × товар. Чем?

- A) `composite`
- B) `terms` с `size: 10`
- C) `top_hits`
- D) `histogram`

<details><summary>Ответ</summary>

**A.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Что уменьшает ошибку `terms` на нескольких шардах?

- A) `size: 0`
- B) `refresh`
- C) увеличить `shard_size`
- D) увеличить число реплик

<details><summary>Ответ</summary>

**C.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🟡 Как оставить только корзины с суммой больше порога?

- A) `post_filter`
- B) `filter` в запросе
- C) `missing`
- D) `bucket_selector`

<details><summary>Ответ</summary>

**D.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 10. 🔴 Что вернула `cardinality` по умолчанию на 50 000 значений в эксперименте?

- A) ошибку
- B) приблизительное число, с отклонением больше 1%
- C) ровно 50000
- D) 3000

<details><summary>Ответ</summary>

**B.** 50741.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 11. 🔴 Месяц отсутствует в данных, `derivative` с `gap_policy` по умолчанию:

- A) ошибка
- B) месяц удаляется из ответа
- C) пропуск: значения рядом не вычисляются
- D) считается нулём

<details><summary>Ответ</summary>

**C.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 12. 🔴 Открытый вопрос: отчёт «топ-100 клиентов по выручке» по индексу из 20 шардов должен быть точным. Варианты?

<details><summary>Ответ</summary>

Маршрутизация по клиенту (все его документы в одном шарде) с учётом перекоса; обход всех клиентов через `composite` и сортировка в приложении; заранее посчитанная сводка в отдельном индексе. Увеличение `shard_size` снижает ошибку, но не гарантирует точность в общем случае.
→ [Продвинутая теория](advanced.ru.md)

</details>
