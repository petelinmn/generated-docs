---
title: "Observability: квиз"
lang: ru
track: claude
kind: module
doc: quiz
slug: 15-observability
part: 3
tags: [claude, observability, opentelemetry, usage, cost]
versions: "Claude Code 2.1.332; anthropic 2.14.0; Usage and Cost Analytics API"
status: draft
updated: 2026-11-25
---

# Observability: квиз

🌐 **Русский** · [English](quiz.en.md)

> Сначала ответьте сами, затем раскройте ответ. 🟢 [базовый](basics.ru.md) · 🟡 [продвинутый](advanced.ru.md) · 🔴 [экспертный](expert.ru.md).

## 1. 🟢 Что передать поддержке о конкретном запросе?

- A) текст всех промптов
- B) IP пользователя
- C) `request-id`
- D) API-ключ

<details><summary>Ответ</summary>

**C.**
→ [Базовая теория](basics.ru.md)

</details>

## 2. 🟢 Где в ответе число токенов?

- A) `usage`
- B) `content`
- C) `stop_details`
- D) `model`

<details><summary>Ответ</summary>

**A.**
→ [Базовая теория](basics.ru.md)

</details>

## 3. 🟢 Весь вход — это:

- A) только `input_tokens`
- B) только чтение кэша
- C) `output_tokens`
- D) сумма `input_tokens`, записи и чтения кэша

<details><summary>Ответ</summary>

**D.**
→ [Базовая теория](basics.ru.md)

</details>

## 4. 🟢 Чего не писать в лог по умолчанию?

- A) имя модели
- B) тексты промптов с персональными данными
- C) `stop_reason`
- D) задержку

<details><summary>Ответ</summary>

**B.**
→ [Базовая теория](basics.ru.md)

</details>

## 5. 🟡 Что включает телеметрию Claude Code?

- A) ничего, она всегда идёт в ваш сборщик
- B) `CLAUDE_CODE_ENABLE_TELEMETRY=1`
- C) `--verbose`
- D) `/cost`

<details><summary>Ответ</summary>

**B.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 6. 🟡 Текст промпта в событии по умолчанию:

- A) полный
- B) первые 500 символов
- C) хеш
- D) `<REDACTED>`

<details><summary>Ответ</summary>

**D.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 7. 🟡 Что несёт событие `api_request`?

- [ ] A) токены и `cost_usd`
- [ ] B) `duration_ms`
- [ ] C) содержимое файлов
- [ ] D) API-ключ

<details><summary>Ответ</summary>

**A, B.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 8. 🟡 Что нужно для Analytics API?

- A) ничего
- B) OTel-сборщик
- C) ключ администратора
- D) обычный ключ

<details><summary>Ответ</summary>

**C.**
→ [Продвинутая теория](advanced.ru.md)

</details>

## 9. 🔴 `DISABLE_TELEMETRY=1` при настроенном экспорте OTel:

- A) экспорт останавливается
- B) ошибка запуска
- C) идут только трассы
- D) экспорт продолжается

<details><summary>Ответ</summary>

**D.**
→ [Экспертная теория](expert.ru.md)

</details>

## 10. 🔴 Что раскрывает `OTEL_LOG_RAW_API_BODIES`?

- A) версию программы
- B) полные тела запросов и ответов
- C) только коды ответов
- D) только метрики

<details><summary>Ответ</summary>

**B.**
→ [Экспертная теория](expert.ru.md)

</details>

## 11. 🔴 Какие спаны пришли с трассировкой?

- A) `db.query`
- B) ни одного
- C) `claude_code.llm_request` и `claude_code.tool`
- D) `http.server`

<details><summary>Ответ</summary>

**C.**
→ [Экспертная теория](expert.ru.md)

</details>

## 12. 🔴 Открытый вопрос: расходы выросли втрое за неделю. Как искать причину?

<details><summary>Ответ</summary>

Разложить по модели, ключу или workspace, функции и типу токенов; проверить попадания в кэш, длину истории, число повторов и ходов агентов, новые модели и effort; сравнить стоимость на задачу до и после релиза.
→ [Продвинутая теория](advanced.ru.md)

</details>
