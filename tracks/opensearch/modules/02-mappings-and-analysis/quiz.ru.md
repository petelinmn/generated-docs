---
title: "Квиз: отображения и анализ"
lang: ru
track: opensearch
kind: module
doc: quiz
slug: 02-mappings-and-analysis
part: 1
tags: [mapping, analyzer, inverted-index, text-keyword, nested, dynamic-mapping]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Квиз: отображения и анализ

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, затем раскройте ответ. 🟢 [базовый](basics.ru.md) · 🟡 [продвинутый](advanced.ru.md) · 🔴 [экспертный](expert.ru.md).

## 1. 🟢 Что получит поле, если первым пришло значение `"42"`?

- A) `integer`
- B) `date`
- C) `text` с подполем `keyword`
- D) `long`

<details><summary>Ответ</summary>

**C.**
→ [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Для какого поля подходит `keyword`?

- A) код категории
- B) описание товара
- C) текст статьи
- D) отзыв

<details><summary>Ответ</summary>

**A.** Остальное ищут по словам.
→ [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Что вернёт `english` для «foxes jumped»?

- A) `foxes, jumped`
- B) `Foxes, Jumped`
- C) одну строку
- D) `fox, jump`

<details><summary>Ответ</summary>

**D.** Стемминг.
→ [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Можно ли сменить тип существующего поля `long` на `keyword`?

- A) да, после закрытия индекса
- B) нет, нужен новый индекс
- C) да, `put_mapping`
- D) да, после refresh

<details><summary>Ответ</summary>

**B.**
→ [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 `title` = «Hello World» (`text`). Что найдёт `term: {title: "Hello World"}`?

- A) все документы
- B) ничего
- C) документ
- D) ошибку 400

<details><summary>Ответ</summary>

**B.** `term` не анализирует запрос.
→ [Базовая теория](basics.ru.md)

</details>

## 6. 🟡 Автодополнение на `edge_ngram` находит «Laptop» по запросу «lamp». Причина?

- A) нет `refresh`
- B) мал `max_gram`
- C) поле `keyword`
- D) не задан `search_analyzer`

<details><summary>Ответ</summary>

**D.** Запрос тоже режется на префиксы, и `la` совпадает.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Что верно при `dynamic: false`?

- [ ] A) новое поле хранится в `_source`
- [ ] B) по нему нельзя искать
- [ ] C) документ отклоняется
- [ ] D) поле добавляется в отображение

<details><summary>Ответ</summary>

**A, B.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Массив объектов нужно фильтровать по паре значений одного элемента. Тип?

- A) `keyword`
- B) `text`
- C) `nested`
- D) `object`

<details><summary>Ответ</summary>

**C.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🟡 В поле `long` записали `7.9`. Что произошло в эксперименте?

- A) ошибка
- B) поле стало `float`
- C) сохранено `8`
- D) принято, находится по `term: 7`

<details><summary>Ответ</summary>

**D.** Coercion; `_source` не меняется.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 10. 🔴 Какая структура нужна для сортировки?

- A) translog
- B) doc values
- C) обратный индекс
- D) нормы

<details><summary>Ответ</summary>

**B.**
→ [Экспертная теория](expert.ru.md)

</details>

## 11. 🔴 `keyword` с `ignore_above: 256`, значение из 300 символов:

- A) обрезается до 256
- B) индексируется целиком
- C) сохраняется, но не находится по этому полю
- D) документ отклоняется

<details><summary>Ответ</summary>

**C.**
→ [Экспертная теория](expert.ru.md)

</details>

## 12. 🔴 Открытый вопрос: клиенты присылают произвольные атрибуты товаров. Как не получить взрыв отображения?

<details><summary>Ответ</summary>

Не делать атрибуты именами полей: `dynamic: false` или `strict` для объекта; `flat_object`; либо массив `nested` вида `{name, value}`; лимит полей как защита, а не решение.
→ [Экспертная теория](expert.ru.md)

</details>
