---
title: "Квиз: релевантность"
lang: ru
track: opensearch
kind: module
doc: quiz
slug: 06-relevance-tuning
part: 2
tags: [relevance, bm25, boosting, function-score, synonyms]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Квиз: релевантность

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, затем раскройте ответ. 🟢 [базовый](basics.ru.md) · 🟡 [продвинутый](advanced.ru.md) · 🔴 [экспертный](expert.ru.md).

## 1. 🟢 Один и тот же терм по разу в коротком и длинном поле. Что обычно выше?

- A) одинаково всегда
- B) зависит от `_id`
- C) короткое поле
- D) длинное поле

<details><summary>Ответ</summary>

**C.**
→ [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Что значит `title^3`?

- A) вклад поля умножается на 3
- B) слово должно встретиться 3 раза
- C) 3 опечатки
- D) третья версия поля

<details><summary>Ответ</summary>

**A.**
→ [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Как опустить документы со словом, но не убрать их?

- A) `must_not`
- B) `post_filter`
- C) `filter`
- D) `boosting` с `negative_boost`

<details><summary>Ответ</summary>

**D.**
→ [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Тип `multi_match` по умолчанию?

- A) `phrase`
- B) `best_fields`
- C) `most_fields`
- D) `cross_fields`

<details><summary>Ответ</summary>

**B.**
→ [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 `field_value_factor` по `views` без модификатора. Что произошло?

- A) оценки нормировались к 1
- B) порядок определили просмотры
- C) ничего
- D) ошибка

<details><summary>Ответ</summary>

**B.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Что верно о синонимах в `search_analyzer`?

- [ ] A) действуют на запрос
- [ ] B) изменяют `_source`
- [ ] C) требуют нового индекса при каждом поиске
- [ ] D) не требуют переиндексации

<details><summary>Ответ</summary>

**A, D.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Чему подвергается документ вне `window_size` при `rescore`?

- A) не переоценивается
- B) удаляется из выдачи
- C) получает 0
- D) переоценивается позже

<details><summary>Ответ</summary>

**A.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Результат `script_score` отрицательный:

- A) берётся модуль
- B) оценка 0
- C) ошибка 400
- D) документ опускается в конец

<details><summary>Ответ</summary>

**C.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🔴 Что делает `b = 0` в BM25?

- A) убирает idf
- B) убирает частоту терма
- C) обнуляет оценку
- D) убирает влияние длины поля

<details><summary>Ответ</summary>

**D.**
→ [Экспертная теория](expert.ru.md)

</details>

## 10. 🔴 Одинаковые документы получили разные оценки в трёх шардах. Что их выровняло?

- A) `explain`
- B) `dfs_query_then_fetch`
- C) `refresh`
- D) реплика

<details><summary>Ответ</summary>

**B.**
→ [Экспертная теория](expert.ru.md)

</details>

## 11. 🔴 Единственный релевантный документ стоит третьим. MRR@3?

- A) 0
- B) 3
- C) 1/3
- D) 1

<details><summary>Ответ</summary>

**C.**
→ [Экспертная теория](expert.ru.md)

</details>

## 12. 🔴 Открытый вопрос: пользователи жалуются, что в выдаче сверху старые популярные товары. Как действовать?

<details><summary>Ответ</summary>

Собрать проблемные запросы и оценки; `explain` — увидеть вклад популярности; сгладить её (`log1p`, сложение, ограничение), добавить затухание по свежести с ненулевым минимумом; сравнить метрики `_rank_eval` до и после; проверить на части трафика.
→ [Продвинутая теория](advanced.ru.md)

</details>
