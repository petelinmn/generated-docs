---
title: "Batch, evals и надёжность: экспертный уровень"
lang: ru
track: claude
kind: module
doc: expert
slug: 14-evals
part: 3
tags: [claude, evals, batch-api, retries, reliability]
versions: "Claude 5.5 family; anthropic 2.14.0; Message Batches API"
status: draft
updated: 2026-11-25
---

# Batch, evals и надёжность: экспертный уровень

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

## Коды ошибок

| Код | `type` |
|---|---|
| 400 | `invalid_request_error` (также свой лимит трат) |
| 401 | `authentication_error` |
| 402 | `billing_error` |
| 403 | `permission_error` |
| 404 | `not_found_error` |
| 409 | `conflict_error` |
| 413 | `request_too_large` |
| 429 | `rate_limit_error` |
| 500 | `api_error` |
| 504 | `timeout_error` |
| 529 | `overloaded_error` |

## Лимиты

RPM, ITPM и OTPM на класс моделей, алгоритм token bucket: ёмкость пополняется непрерывно, а короткий всплеск всё равно может превысить лимит. В ITPM входят `input_tokens` и `cache_creation_input_tokens`, но не `cache_read_input_tokens`. `max_tokens` не влияет на OTPM.

## Ещё о batch

Отмена переводит в `canceling`. Бета `output-300k-2026-03-24` поднимает предел вывода в batch до 300k для ряда моделей. Скидка batch и кэширование промпта сочетаются по мере попаданий в кэш.

## Подвохи

1. Отказ не вызывает исключения, а `content` пуст: `response.content[0]` упадёт.
2. С настройками по умолчанию один вызов к перегруженному API — это три запроса.
3. Свои повторы поверх SDK перемножаются с его повторами.
4. Порядок результатов batch не совпадает с порядком запросов.
5. SDK не повторяет ошибку, пришедшую внутри потока (по документации).

> ⚠️ Проверить: пределы, цены, тексты ошибок сервиса, `fallbacks`, отмена batch и расширенный вывод не проверялись вживую.

## Вопросы с собеседований

1. Какие ошибки повторять?
<details><summary>Ответ</summary>

Сетевые, 408, 409, 429, 5xx с учётом `x-should-retry`; 400, 401, 403, 404 — нет.
</details>

2. Почему 429 не всегда стоит повторять?
<details><summary>Ответ</summary>

429 при достижении лимита трат по тарифу не имеет `retry-after` и не пройдёт, пока доступ не вернётся.
</details>

3. Как увеличить пропускную способность по входным токенам?
<details><summary>Ответ</summary>

Кэшированные чтения не входят в ITPM, поэтому кэширование помогает.
</details>

4. Что передать поддержке?
<details><summary>Ответ</summary>

`request-id` из заголовка или `request_id`.
</details>

5. Когда batch?
<details><summary>Ответ</summary>

Нет ожидающего пользователя, большой объём, допустима задержка до суток.
</details>

6. Как сопоставить результаты batch?
<details><summary>Ответ</summary>

Только по `custom_id`.
</details>

7. Как узнать об отказе?
<details><summary>Ответ</summary>

По `stop_reason`, не по коду HTTP.
</details>

8. Как не переобучиться на eval?
<details><summary>Ответ</summary>

Разделить на настроечный и отложенный наборы.
</details>

9. Что мерить кроме точности?
<details><summary>Ответ</summary>

Задержку, стоимость, долю отказов и обрезок, разброс между запусками, провалы по категориям.
</details>

10. Как поймать регрессию при смене модели?
<details><summary>Ответ</summary>

Прогнать тот же набор на обеих и сравнить по случаям, а не только среднее.
</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md) · [Квиз](quiz.ru.md)
- [15 · Observability](../15-observability/basics.ru.md)

## Источники

- [Rate limits](https://platform.claude.com/docs/en/api/rate-limits)
- [Errors](https://platform.claude.com/docs/en/api/errors)
