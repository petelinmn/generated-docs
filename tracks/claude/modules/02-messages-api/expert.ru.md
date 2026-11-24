---
title: "Messages API: экспертный уровень"
lang: ru
track: claude
kind: module
doc: expert
slug: 02-messages-api
part: 1
tags: [claude, messages-api, sse, stop-reason, api-versioning, rate-limits, request-limits, api-errors]
versions: "Claude 5.5 family; Messages API, anthropic-version 2026-10-21; Python SDK anthropic 2.13.0"
status: draft
updated: 2026-11-24
---

# Messages API: экспертный уровень

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

Опирается на [базовую](basics.ru.md) и [продвинутую](advanced.ru.md) теорию. Справочные данные взяты со страниц документации Claude Platform (Messages API, Errors, Streaming, Versioning) на дату подготовки; то, что запускалось, помечено «в опыте» (локальный мок-сервер, SDK `anthropic` 2.13.0).

## Справочник

### Параметры запроса `POST /v1/messages`

| Параметр | Тип и значения | Замечания |
|---|---|---|
| `model` | строка | идентификатор модели |
| `max_tokens` | число ≥ 0 | потолок вывода; `0` прогревает кэш промпта без генерации |
| `messages` | массив `MessageParam` | `role`: `user` или `assistant`; до 100 000 сообщений |
| `system` | строка или массив текстовых блоков | инструкции верхнего уровня |
| `stop_sequences` | массив строк | пользовательские остановки |
| `stream` | boolean | потоковая выдача (SSE) |
| `tools`, `tool_choice` | массив / объект | `auto`, `any`, `tool`, `none` + `disable_parallel_tool_use`; `any`/`tool` не поддерживаются на ряде моделей |
| `thinking` | `adaptive`, `enabled` (+`budget_tokens` ≥ 1024), `disabled`, `between_tools`; `display`: `summarized`/`omitted` | допустимые значения зависят от модели |
| `output_config` | `effort`: `low`…`max`; `format`: JSON-схема | структурированный вывод — модуль 04 дорожной карты |
| `metadata` | `user_id` до 512 символов | идентификатор пользователя для учёта |
| `service_tier` | `auto` или `standard_only` | уровень обслуживания |
| `cache_control`, `container`, `diagnostics`, `inference_geo`, `speed` | см. документацию | кэширование, регион вывода (`inference_geo`), быстрый режим (`speed`: `standard`/`fast`) |

> На версии API `2026-10-21` параметры `temperature`, `top_p`, `top_k` **удалены** — запрос с ними даёт 400 на любой модели.

### Типы блоков контента

| Направление | Типы |
|---|---|
| Во входных сообщениях | `text`, `image`, `document`, `search_result`, `thinking`, `redacted_thinking`, `tool_use`, `tool_result`, `server_tool_use`, результаты серверных инструментов (`web_search_tool_result`, `web_fetch_tool_result`, `code_execution_tool_result`, `bash_code_execution_tool_result`, `text_editor_code_execution_tool_result`, `tool_search_tool_result`), `container_upload` |
| В ответе | `text`, `thinking`, `tool_use`, `server_tool_use` и блоки результатов серверных инструментов |

### Объект ответа

| Поле | Значение |
|---|---|
| `id` | идентификатор сообщения (в опыте `msg_mock01`) |
| `type` | всегда `message` |
| `role` | всегда `assistant` |
| `content` | массив блоков |
| `model` | модель, обработавшая запрос |
| `stop_reason` | причина остановки (таблица ниже) |
| `stop_sequence` | сработавшая строка или `null` |
| `usage` | `input_tokens`, `output_tokens` (и поля кэша) |
| `stop_details` | только при `stop_reason: "refusal"`: `type`, `category`, `explanation`; иначе `null` |

| `stop_reason` | Когда |
|---|---|
| `end_turn` | естественное завершение |
| `max_tokens` | исчерпан лимит |
| `stop_sequence` | сработала `stop_sequences` |
| `tool_use` | запрошен вызов инструмента |
| `pause_turn` | ход приостановлен |
| `refusal` | отказ по соображениям безопасности |

> ⚠️ Проверить: полный перечень полей `usage` (кэш, итерации, скорость) и набор значений `category` в `stop_details` берутся из актуальной документации и в опыте не проверялись.

### Заголовки

| Заголовок | Направление | Смысл |
|---|---|---|
| `anthropic-version` | запрос | версия API; SDK 2.13.0 отправил `2026-10-21` (в опыте) |
| `x-api-key` | запрос | ключ при прямых HTTP-вызовах (SDK 2.13.0 в опыте передал ключ в `authorization`) |
| `content-type: application/json` | запрос | формат тела |
| `anthropic-beta` | запрос | включение бета-функций (если они нужны) |
| `request-id` | ответ | идентификатор запроса (в опыте `req_mock123`); то же значение в поле `request_id` тела ошибки |
| `retry-after` | ответ | пауза перед повтором при 429 (в опыте читается через `e.response.headers`) |

### Таблица ошибок

| HTTP | `error.type` | Значение |
|---|---|---|
| 400 | `invalid_request_error` | неверная форма запроса; также для других 4xx и при достижении лимита расходов |
| 401 | `authentication_error` | проблема с ключом (или `CORS requests are not allowed…` при запросе из браузера) |
| 402 | `billing_error` | проблема с оплатой |
| 403 | `permission_error` | у ключа нет прав на ресурс |
| 404 | `not_found_error` | ресурс не найден |
| 409 | `conflict_error` | конфликт состояния ресурса |
| 413 | `request_too_large` | превышен размер запроса (Messages API — 128 МБ; Batch API — 256 МБ; Files API — 500 МБ) |
| 429 | `rate_limit_error` | лимит запросов или расходов |
| 500 | `api_error` | внутренняя ошибка |
| 504 | `timeout_error` | таймаут обработки (`timeout_error` бывает и с другими кодами) |
| 529 | `overloaded_error` | API временно перегружен |

Тело ошибки: `{"type": "error", "error": {"type", "message"}, "request_id": "req_…"}`. Новые значения `type` могут появляться без смены версии API: не делайте исчерпывающий `switch` без ветки `default`.

**Ошибки в потоке.** После ответа 200 ошибка приходит событием `event: error` с данными `{"type": "error", "error": {"type": "overloaded_error", "message": "Overloaded"}}`; обычные HTTP-механизмы тут не работают. Для моделей Claude 4.6 и новее документация описывает восстановление после обрыва потока; сверяйтесь с разделом об ошибках потока.

### Протокол SSE потоковых событий

Каждое событие — `event: <тип>` плюс `data: <JSON>` с тем же `type`. Порядок для обычного ответа (из документации; в опыте SDK показал те же типы):

| Событие | Назначение |
|---|---|
| `message_start` | объект `Message` с пустым `content` и начальным `usage` |
| `content_block_start` | начало блока по `index` (например, `{"type": "text", "text": ""}`) |
| `ping` | поддержание соединения; может приходить в любой момент (SDK его не показывает) |
| `content_block_delta` | приращение блока |
| `content_block_stop` | конец блока |
| `message_delta` | итоговые `stop_reason`, `stop_sequence` и накопленный `usage` (`output_tokens` — суммарно) |
| `message_stop` | конец сообщения |
| `error` | ошибка внутри потока |

Типы приращений `delta`: `text_delta` (поле `text`), `input_json_delta` (поле `partial_json`: куски JSON аргументов инструмента, склеиваются и разбираются после `content_block_stop`), `thinking_delta` (`thinking`), `signature_delta` (`signature`, подпись блока рассуждений). Вокруг сообщений между блоками могут появляться новые типы событий: клиент должен игнорировать неизвестные.

## Внутреннее устройство

- **Stateless.** Сервер не хранит диалог. Весь контекст — тело запроса: `system`, `tools`, `messages` в таком порядке образуют префикс, на котором строится кэширование ([модуль 05](../../overview.ru.md) дорожной карты) и подсчёт `input_tokens`.
- **Ответ как набор блоков.** Блоки идут в порядке генерации (рассуждение → текст → вызов инструмента); `index` в потоке связывает приращения с блоками. Итоговое сообщение SDK строит накоплением событий: `get_final_message()` возвращает тот же объект, что и обычный запрос.
- **`usage` в потоке** приходит дважды: начальное значение в `message_start` (в опыте `output_tokens: 1`) и итоговое в `message_delta` (в опыте `6`); использовать нужно итоговое.
- **Версионирование.** Для заданной версии API Anthropic сохраняет входные и выходные параметры, но вправе добавлять необязательные входы, значения в выходах, новые значения перечислений (в том числе типы событий потока) и менять условия некоторых ошибок. Поэтому клиент обязан терпимо относиться к неизвестным полям и значениям.
- **Ограничения.** Размер запроса — до 128 МБ для Messages API, число сообщений — до 100 000; превышение размера даёт 413 ещё до обработки. Не-стриминговые запросы с большим `max_tokens` SDK проверяет на возможное превышение 10-минутного таймаута.
- **Повторы.** SDK повторяет соединение, 408/409/429 и 5xx дважды с экспоненциальной паузой и учётом `retry-after`; идемпотентности на уровне запроса API не гарантирует: повтор после таймаута может привести к двойной генерации и двойной оплате.

> ⚠️ Проверить: поведение API при повторе после таймаута и точный список повторяемых кодов SDK взяты из документации и описаний SDK и в опытах на реальном API не воспроизводились.

## Подвохи

### 1. `content[0]` — не всегда текст

С мышлением или инструментами первым блоком может быть `thinking` или `tool_use`.

### 2. `max_tokens` обрезает ответ молча

`stop_reason == "max_tokens"` — единственный сигнал (в опыте мок вернул `max_tokens`).

### 3. `refusal` приходит с кодом 200

Отказ — не ошибка HTTP; проверяйте `stop_reason`.

### 4. `stop_sequence` не входит в текст

Сработавшая строка отсекается; она в поле `stop_sequence` (в опыте `'4'` → текст `'1, 2, 3, '`).

### 5. Повторы SDK скрывают сбои

В опыте после двух ответов 429 код получил успешный результат; число попыток видно только в логах сервера и по `retry-count` (заголовок `x-stainless-retry-count`).

### 6. `temperature` на новой версии API

Запрос с `temperature`, `top_p` или `top_k` при `anthropic-version: 2026-10-21` даёт 400 на любой модели.

### 7. Блоки `thinking` нельзя править

При продолжении диалога передавайте их без изменений; иначе 400 («блоки … нельзя изменять»).

### 8. Prefill

Заполнение начала ответа сообщением ассистента даёт 400 на новых моделях (4.6 и новее).

### 9. Неизвестные поля и значения

Новые значения `stop_reason`, типов блоков и событий могут появиться без смены версии: обрабатывайте ветку по умолчанию.

### 10. Ошибка после 200 в стриме

Сбой в середине потока приходит событием `error`, а не HTTP-кодом.

## Компромиссы и альтернативы

| Подход | Плюсы | Минусы |
|---|---|---|
| Обычный запрос | проще код | таймауты при больших ответах |
| Стриминг | ранний вывод, нет таймаутов | сложнее код и обработка ошибок |
| Message Batches | дешевле, без сети на ожидании | отложенный результат (модуль 14 дорожной карты) |
| `count_tokens` перед запросом | точная оценка | дополнительный вызов |
| Сырой HTTP | без зависимостей | вручную версии, повторы, разбор SSE |
| SDK | типы, повторы, стриминг | зависимость; версии SDK и API нужно согласовывать |

## Вопросы с собеседований

<details><summary>1. Почему Messages API называют stateless и что из этого следует?</summary>

Сервер не хранит диалог: каждый запрос содержит всю историю. Следствия: растёт стоимость, нужна компактизация, кэширование строится на префиксе.

→ [Базовая теория](basics.ru.md#диалог-api-не-помнит-историю)

</details>

<details><summary>2. Что означают значения <code>stop_reason</code>?</summary>

`end_turn` — закончил сам; `max_tokens` — обрезан лимитом; `stop_sequence` — сработала ваша строка; `tool_use` — просит инструмент; `pause_turn` — пауза хода; `refusal` — отказ.

→ [Справочник](#объект-ответа)

</details>

<details><summary>3. Подвох: <code>message.content[0].text</code> упал с <code>AttributeError</code>. Почему?</summary>

Первым блоком оказался не `text` (например, `thinking`). Нужно проверять `block.type`.

→ [Подвох 1](#1-content0--не-всегда-текст)

</details>

<details><summary>4. Как получить полный ответ, не обрабатывая события потока?</summary>

`with client.messages.stream(...) as s: s.get_final_message()` — возвращает такой же `Message`, как обычный запрос.

→ [Продвинутая теория](advanced.ru.md#потоковая-выдача)

</details>

<details><summary>5. Как устроен поток SSE?</summary>

`message_start` → блоки (`content_block_start` → дельты → `content_block_stop`) → `message_delta` → `message_stop`; `ping` и `error` возможны в любой момент.

→ [Протокол SSE](#протокол-sse-потоковых-событий)

</details>

<details><summary>6. Подвох: запрос с <code>temperature</code> внезапно вернул 400. Что произошло?</summary>

При версии API `2026-10-21` `temperature`, `top_p`, `top_k` удалены.

→ [Подвох 6](#6-temperature-на-новой-версии-api)

</details>

<details><summary>7. Что делает SDK при 429 и 5xx?</summary>

Повторяет запрос дважды с экспоненциальной паузой и учётом `retry-after`; число повторов — `max_retries`.

→ [Базовая теория](basics.ru.md#ошибки)

</details>

<details><summary>8. Как связать ошибку с обращением в поддержку?</summary>

Приложить `request_id` (заголовок `request-id`, поле тела ошибки, `e.request_id`, `message._request_id`).

→ [Продвинутая теория](advanced.ru.md#request-id-и-заголовки-ответа)

</details>

<details><summary>9. Почему клиент должен терпимо относиться к неизвестным значениям?</summary>

Anthropic вправе добавлять поля, значения перечислений и типы событий в рамках одной версии API.

→ [Внутреннее устройство](#внутреннее-устройство)

</details>

<details><summary>10. Подвох: поток вернул 200, но ответ оборвался ошибкой. Как это возможно?</summary>

Ошибки внутри потока приходят событием `error` после ответа 200.

→ [Подвох 10](#10-ошибка-после-200-в-стриме)

</details>

<details><summary>11. Когда нужен <code>count_tokens</code>?</summary>

Для оценки стоимости и проверки, что запрос влезает в окно контекста, без генерации ответа.

→ [Продвинутая теория](advanced.ru.md#подсчёт-токенов)

</details>

<details><summary>12. Чем <code>system</code> отличается от первого сообщения <code>user</code>?</summary>

`system` — инструкции верхнего уровня отдельным параметром (строка или блоки, поддерживает `cache_control`); сообщение `user` — часть диалога.

→ [Базовая теория](basics.ru.md#system-prompt)

</details>

## Шпаргалка

```text
POST /v1/messages  headers: x-api-key, anthropic-version: 2026-10-21, content-type: application/json
body: { model, max_tokens, messages: [{role, content}], system?, stop_sequences?, stream?, thinking?, output_config? }
response: { id, role, content: [blocks], stop_reason, stop_sequence, usage: {input_tokens, output_tokens} }
stop_reason: end_turn | max_tokens | stop_sequence | tool_use | pause_turn | refusal
errors: 400 invalid_request | 401 authentication | 403 permission | 404 not_found | 413 request_too_large | 429 rate_limit | 500 api | 529 overloaded
SDK: client.messages.create / .stream(...).get_final_message() / .count_tokens / .with_raw_response
```

- Проверяйте `stop_reason` и тип блока.
- Большие ответы — стриминг.
- `temperature`, `top_p`, `top_k` удалены в версии `2026-10-21`.

## Связанные материалы

- Предыдущий уровень: [продвинутая теория](advanced.ru.md)
- Выбор модели: [01 · Семейство Claude и выбор модели](../01-models/expert.ru.md)
- Потоки и асинхронность в JavaScript: [12 · Промисы и async/await](../../../javascript/modules/12-promises-and-async-await/expert.ru.md)

## Источники

- [Messages API: справочник](https://platform.claude.com/docs/en/api/messages)
- [Streaming messages](https://platform.claude.com/docs/en/build-with-claude/streaming)
- [Ошибки API](https://platform.claude.com/docs/en/api/errors)
- [Версии API](https://platform.claude.com/docs/en/api/versioning)
- [Rate limits](https://platform.claude.com/docs/en/api/rate-limits)
