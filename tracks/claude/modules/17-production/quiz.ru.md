---
title: "Развёртывание и production: квиз"
lang: ru
track: claude
kind: module
doc: quiz
slug: 17-production
part: 3
tags: [claude, deployment, production, retries, bedrock, vertex-ai, foundry]
versions: "Claude 5.5 family; anthropic 2.14.0; Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# Развёртывание и production: квиз

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, затем раскройте ответ. 🟢 [базовый](basics.ru.md) · 🟡 [продвинутый](advanced.ru.md) · 🔴 [экспертный](expert.ru.md).

## 1. 🟢 Как называется модель Opus 5.5 в Claude in Amazon Bedrock?

- A) `bedrock/claude-opus-5-5`
- B) `claude-opus-5-5@aws`
- C) `anthropic.claude-opus-5-5`
- D) `claude-opus-5-5`

<details><summary>Ответ</summary>

**C.** В Bedrock ID имеет префикс поставщика. Без префикса модель называют Claude API и Claude Platform on AWS; остальные два формата в модуле не используются.
→ [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Сколько попыток делает SDK по умолчанию для ответа 529?

- A) три
- B) одну
- C) две
- D) пока не получится

<details><summary>Ответ</summary>

**A.** `max_retries=2` — это первый запрос и два повтора. Бесконечных повторов нет.
→ [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Что передают в `model` в Microsoft Foundry?

- A) URL ресурса
- B) ID подписки Azure
- C) регион
- D) имя deployment

<details><summary>Ответ</summary>

**D.** Запрос адресуется вашему deployment; по умолчанию его имя совпадает с ID модели. Ресурс задаётся в клиенте, не в `model`.
→ [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Чем управляет AWS, а не Anthropic?

- A) Foundry
- B) Claude in Amazon Bedrock
- C) Claude Platform on AWS
- D) Claude API

<details><summary>Ответ</summary>

**B.** Claude Platform on AWS оплачивается через AWS, но управляется Anthropic — это частая путаница.
→ [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 Перегрузка случилась после начала стрима. Что сделает SDK?

- A) бросит `APITimeoutError`
- B) бросит `APIStatusError` без повтора
- C) повторит и склеит текст
- D) вернёт неполное сообщение без ошибки

<details><summary>Ответ</summary>

**B.** Статус 200 уже получен, поэтому обычные повторы не работают. Склеивания нет, и это не таймаут.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Какие функции отсутствуют в Bedrock и Vertex AI?

- [ ] A) Files API
- [ ] B) tool use
- [ ] C) prompt caching
- [ ] D) Message Batches

<details><summary>Ответ</summary>

**A, D.** Batch и Files там не поддерживаются, а tool use и кэширование — базовые функции на всех платформах.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Что верно о 429 из-за лимита трат уровня?

- [ ] A) повтор не поможет
- [ ] B) SDK всё равно повторяет
- [ ] C) это ошибка 402
- [ ] D) пройдёт после `retry-after`

<details><summary>Ответ</summary>

**A, B.** Доступ вернётся в новом месяце или после повышения лимита. Заголовка `retry-after` нет, но SDK считает любой 429 повторяемым.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Наценка regional endpoint в Bedrock по сравнению с global?

- A) 50%
- B) 100%
- C) 10%
- D) 0%

<details><summary>Ответ</summary>

**C.** Regional дороже на 10%; global без наценки. 50% — это скидка Batch API.
→ [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🔴 Что вернёт `hasattr(client.messages, "batches")` у `AnthropicVertex`?

- A) `False`
- B) `None`
- C) исключение
- D) `True`

<details><summary>Ответ</summary>

**D.** Атрибут есть, хотя платформа batch не поддерживает: проверять возможности по SDK нельзя.
→ [Экспертная теория](expert.ru.md)

</details>

## 10. 🔴 Где у Vertex AI указана модель в HTTP-запросе?

- A) в query-параметре
- B) в пути URL
- C) в теле, поле `model`
- D) в заголовке `x-model`

<details><summary>Ответ</summary>

**B.** Путь заканчивается на `MODEL:rawPredict`, а поля `model` в теле нет.
→ [Экспертная теория](expert.ru.md)

</details>

## 11. 🔴 Что нужно сделать, чтобы у команды на разных платформах не расходились модели Claude Code?

- A) задать `CLOUD_ML_REGION`
- B) отключить prompt cache
- C) зафиксировать `ANTHROPIC_DEFAULT_*_MODEL`
- D) ничего: по умолчанию они одинаковы

<details><summary>Ответ</summary>

**C.** В проверке без настроек Foundry запросил Sonnet 5.5, а mantle и Platform on AWS — Opus 5.5.
→ [Экспертная теория](expert.ru.md)

</details>

## 12. 🔴 Открытый вопрос: сервис в ЕС на Claude должен переживать сбой одной платформы. Что спроектировать?

<details><summary>Ответ</summary>

Два маршрута, оба допустимые по требованиям к данным; eval на каждом; перебор только по сетевым ошибкам, 429 и 5xx; таймаут короче SLA; ограничение повторов на всех уровнях; обработка обрыва стрима; отдельный путь для отказов; алерты и метрики по маршрутам; учёт холодного кэша и квот запасного.
→ [Продвинутая теория](advanced.ru.md)

</details>
