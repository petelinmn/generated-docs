---
title: "Batch, evals и надёжность: квиз"
lang: ru
track: claude
kind: module
doc: quiz
slug: 14-evals
part: 3
tags: [claude, evals, batch-api, retries, reliability]
versions: "Claude 5.5 family; anthropic 2.14.0; Message Batches API"
status: draft
updated: 2026-11-25
---

# Batch, evals и надёжность: квиз

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, затем раскройте ответ. 🟢 [базовый](basics.ru.md) · 🟡 [продвинутый](advanced.ru.md) · 🔴 [экспертный](expert.ru.md).

## 1. 🟢 Что такое eval?

- A) функция Python `eval`
- B) тариф
- C) примеры, способ оценки и порог
- D) лог запросов

<details><summary>Ответ</summary>

**C.**
→ [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Самый дешёвый способ проверить точную метку?

- A) кодом
- B) моделью
- C) человеком
- D) никак

<details><summary>Ответ</summary>

**A.**
→ [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Хороший критерий успеха:

- A) «отвечает хорошо»
- B) «быстро»
- C) мнение автора промпта
- D) измеримый порог на отложенном наборе

<details><summary>Ответ</summary>

**D.**
→ [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Почему проваленные случаи важнее среднего?

- A) так дешевле
- B) показывают, что чинить
- C) среднее нельзя вычислить
- D) их меньше

<details><summary>Ответ</summary>

**B.**
→ [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 Что SDK повторит по умолчанию?

- [ ] A) 401
- [ ] B) 429
- [ ] C) 529
- [ ] D) 400

<details><summary>Ответ</summary>

**B, C.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Сколько попыток при `max_retries=2`?

- A) 2
- B) 1
- C) без ограничения
- D) до 3

<details><summary>Ответ</summary>

**D.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Как выглядит отказ модели?

- A) 200 и `stop_reason: "refusal"`
- B) 403
- C) исключение `RefusalError`
- D) 429

<details><summary>Ответ</summary>

**A.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Что верно о Message Batches?

- [ ] A) поддерживают `stream`
- [ ] B) ответ за секунды гарантирован
- [ ] C) половина цены
- [ ] D) результаты в любом порядке

<details><summary>Ответ</summary>

**C, D.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🔴 Что НЕ входит в ITPM?

- A) `input_tokens`
- B) `cache_creation_input_tokens`
- C) всё входит
- D) `cache_read_input_tokens`

<details><summary>Ответ</summary>

**D.**
→ [Экспертная теория](expert.ru.md)

</details>

## 10. 🔴 Код `overloaded_error`:

- A) 502
- B) 529
- C) 503
- D) 429

<details><summary>Ответ</summary>

**B.**
→ [Экспертная теория](expert.ru.md)

</details>

## 11. 🔴 429 без `retry-after` из-за лимита трат тарифа:

- A) это 400
- B) SDK исправит сам
- C) повторы не помогут
- D) пройдёт через секунду

<details><summary>Ответ</summary>

**C.**
→ [Экспертная теория](expert.ru.md)

</details>

## 12. 🔴 Открытый вопрос: нужно переехать на новую модель. Как доказать, что не стало хуже?

<details><summary>Ответ</summary>

Зафиксированный набор с критериями; прогон обеих моделей (можно batch); сравнение по случаям и категориям; несколько запусков; задержка, стоимость, отказы, обрезки; разбор расхождений; постепенное включение с мониторингом и откатом.
→ [Продвинутая теория](advanced.ru.md)

</details>
