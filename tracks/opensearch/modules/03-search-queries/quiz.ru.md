---
title: "Квиз: поисковые запросы"
lang: ru
track: opensearch
kind: module
doc: quiz
slug: 03-search-queries
part: 1
tags: [query-dsl, full-text-search, bool-query, relevance, bm25, pagination]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Квиз: поисковые запросы

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, затем раскройте ответ. 🟢 [базовый](basics.ru.md) · 🟡 [продвинутый](advanced.ru.md) · 🔴 [экспертный](expert.ru.md).

## 1. 🟢 Чем искать слово в описании товара?

- A) `ids`
- B) `exists`
- C) `match`
- D) `term`

<details><summary>Ответ</summary>

**C.** `match` анализирует запрос так же, как поле, и находит слово внутри текста. `term` ищет точный терм без анализа, `ids` — по идентификаторам, `exists` проверяет лишь наличие поля.
→ [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Где разместить фильтр по категории, не влияющий на ранжирование?

- A) `bool.filter`
- B) `bool.must`
- C) `bool.should`
- D) `highlight`

<details><summary>Ответ</summary>

**A.** Условия в `filter` отбирают документы, но не участвуют в оценке и могут кэшироваться. `must` и `should` влияют на оценку, `highlight` только подсвечивает.
→ [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 `match` по «wireless mouse» без `operator`:

- A) нужны оба слова
- B) нужна точная фраза
- C) ошибка
- D) достаточно любого из слов

<details><summary>Ответ</summary>

**D.** По умолчанию `or`.
→ [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Сколько документов вернётся по умолчанию?

- A) 1
- B) 10
- C) 100
- D) все

<details><summary>Ответ</summary>

**B.** Значение `size` по умолчанию равно 10; остальное берут большим `size` или пагинацией. Все документы сразу не возвращаются.
→ [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 `bool` с `filter` и одним `should` без `minimum_should_match`:

- A) ошибка
- B) `should` только повышает оценку
- C) `should` обязателен
- D) `should` игнорируется

<details><summary>Ответ</summary>

**B.** Когда есть `filter` или `must`, `should` не обязателен и лишь добавляет оценку совпавшим. Это не ошибка, и клауза не игнорируется.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Как показать полные счётчики категорий при выбранной категории?

- A) `filter` в `bool`
- B) `must_not`
- C) `_source`
- D) `post_filter`

<details><summary>Ответ</summary>

**D.** Применяется после агрегаций.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Что верно о `search_after`?

- [ ] A) Требует сортировки
- [ ] B) Позволяет уйти дальше 10000
- [ ] C) Позволяет сразу прыгнуть на страницу 500
- [ ] D) Вместе с PIT даёт согласованный обход

<details><summary>Ответ</summary>

**A, B, D.** `search_after` продолжает с последнего значения сортировки, поэтому сортировка обязательна и окно 10000 не мешает; PIT фиксирует снимок данных. Прыгнуть на произвольную страницу он не умеет.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Пользователь ввёл `name:(mouse` в строку поиска. Что безопаснее?

- A) `term`
- B) `script`
- C) `simple_query_string`
- D) `query_string`

<details><summary>Ответ</summary>

**C.** `query_string` вернул 400.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🔴 Какой множитель BM25 растёт, когда терм редкий?

- A) tf
- B) b
- C) k1
- D) idf

<details><summary>Ответ</summary>

**D.** idf растёт, чем в меньшем числе документов встречается терм. tf — частота внутри документа, а `k1` и `b` — настройки насыщения и нормализации длины.
→ [Экспертная теория](expert.ru.md)

</details>

## 10. 🔴 Найдёт ли `match` по полю с анализатором `english` слово `mouse` запросом «mice»?

- A) только с `highlight`
- B) нет
- C) да
- D) только с `refresh`

<details><summary>Ответ</summary>

**B.** Стеммер не знает неправильных форм; нужны синонимы.
→ [Экспертная теория](expert.ru.md)

</details>

## 11. 🔴 Почему два одинаковых документа получили разные оценки?

- A) случайность
- B) из-за `_source`
- C) они в разных шардах с разной статистикой
- D) `_id` влияет на BM25

<details><summary>Ответ</summary>

**C.** Статистика для BM25 по умолчанию считается в пределах шарда, и в разных шардах она различается. Случайности нет, `_source` и `_id` в формулу не входят.
→ [Экспертная теория](expert.ru.md)

</details>

## 12. 🔴 Открытый вопрос: спроектируйте запрос каталога — строка поиска, фильтры по категории и цене, наличие выше, счётчики категорий.

<details><summary>Ответ</summary>

`bool`: `must` — `multi_match` с бустом названия и `fuzziness`; `filter` — цена; `should` — `range stock > 0` для повышения; выбранная категория — в `post_filter`; `aggs terms` по категории; сортировка по `_score` с уникальным вторым ключом для `search_after`.
→ [Продвинутая теория](advanced.ru.md)

</details>
