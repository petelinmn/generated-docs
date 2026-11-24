---
title: "Messages API: основы"
lang: ru
track: claude
kind: module
doc: basics
slug: 02-messages-api
part: 1
tags: [claude, messages-api, system-prompt, stop-reason, max-tokens, api-errors, anthropic-sdk]
versions: "Claude 5.5 family; Messages API, anthropic-version 2026-10-21; Python SDK anthropic 2.13.0"
status: draft
updated: 2026-11-24
---

# Messages API: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [01 · Семейство Claude и выбор модели](../01-models/overview.ru.md), [Python](../../../python/overview.ru.md) · **Время:** ~1.5 ч

## Как проверялся код

Ключа API при подготовке материала не было, поэтому код запускался против **локального мок-сервера**: он принимает запросы SDK `anthropic` 2.13.0, сохраняет тело и заголовки и возвращает заранее заданные ответы. Так проверено то, что делает ваш код и SDK: форма запроса, разбор ответа, исключения, повторы, потоковые события. Тексты ответов в выводе (`Paris.` и т. п.) условные: ответы реальных моделей не проверялись.

## Зачем это нужно

Любая интеграция с Claude начинается с вызова Messages API. Если понимать, что именно уходит в запросе и что приходит в ответе, остальные возможности (инструменты, кэширование, структурированный вывод) оказываются добавками к тому же запросу.

## Запрос и ответ

Минимальный запрос содержит три обязательных поля: `model`, `max_tokens`, `messages`.

```python
import anthropic

client = anthropic.Anthropic()  # ключ берётся из переменной ANTHROPIC_API_KEY

message = client.messages.create(
    model="claude-opus-5-5",
    max_tokens=1024,
    messages=[{"role": "user", "content": "What is the capital of France?"}],
)
print(type(message).__name__, message.role, message.stop_reason, [b.type for b in message.content], message.content[0].text, message.usage.input_tokens, message.usage.output_tokens)
```

```text
Message assistant end_turn ['text'] Paris. 14 6
```

SDK отправил такое тело запроса (`POST /v1/messages`):

```json
{"max_tokens": 1024, "messages": [{"role": "user", "content": "What is the capital of France?"}], "model": "claude-opus-5-5"}
```

- **`model`** — идентификатор модели (например, `claude-opus-5-5`, см. [модуль 01](../01-models/basics.ru.md)).
- **`max_tokens`** — максимум токенов в ответе; это потолок, а не цель.
- **`messages`** — список сообщений; у каждого `role` (`user` или `assistant`) и `content` (строка или список блоков).
- Ответ — объект `Message`: `id`, `role` (всегда `assistant`), `content` (список блоков), `model`, `stop_reason`, `usage` (`input_tokens`, `output_tokens`).
- Строка в `content` — сокращённая запись: `"content": "Hi"` равносильно списку из одного блока `{"type": "text", "text": "Hi"}`.

**Чтение ответа.** `content` — список блоков разных типов, поэтому перед обращением к `.text` проверяйте `block.type`:

```python
for block in message.content:
    if block.type == "text":
        print(block.text)
```

В простом запросе без инструментов и мышления блок один, но код, рассчитанный только на `content[0].text`, ломается, как только в ответе появляются блоки `thinking` или `tool_use`.

## Заголовки и версия API

Каждый запрос несёт заголовки `anthropic-version`, `content-type: application/json` и ключ. SDK выставляет их сам: в опыте SDK 2.13.0 отправил `anthropic-version: 2026-10-21` и `content-type: application/json`, а ключ передал в заголовке `authorization` (детали аутентификации SDK берёт на себя). Если вы вызываете API без SDK, используйте заголовки из документации (`x-api-key`, `anthropic-version`, `content-type`):

```bash
curl -sS https://api.anthropic.com/v1/messages \
  -H "x-api-key: $ANTHROPIC_API_KEY" \
  -H "anthropic-version: 2026-10-21" \
  -H "content-type: application/json" \
  -d '{"model": "claude-opus-5-5", "max_tokens": 1024,
       "messages": [{"role": "user", "content": "Hello, Claude"}]}'
```

Против мок-сервера эта команда вернула JSON с теми же полями, что и SDK (`id`, `type: "message"`, `role`, `content`, `stop_reason`, `usage`), а сервер получил все три заголовка.

> Версия API `2026-10-21` — текущая на момент подготовки материала; список изменений и правила совместимости — на странице [версий API](https://platform.claude.com/docs/en/api/versioning).

## System prompt

Инструкции верхнего уровня — роль, правила, формат — передают отдельным параметром `system`, а не сообщением пользователя:

```python
client.messages.create(
    model="claude-opus-5-5", max_tokens=1024,
    system="You are terse.",
    messages=[{"role": "user", "content": "What is the capital of France?"}],
)
```

`system` может быть строкой или списком текстовых блоков (`[{"type": "text", "text": "Answer in French."}]`): список понадобится, когда к блокам добавляют `cache_control` ([модуль 05](../../overview.ru.md) дорожной карты). Подробно о том, что писать в `system`, — в модуле 03 дорожной карты.

## Диалог: API не помнит историю

API **не хранит состояние**. Чтобы продолжить разговор, каждый раз отправляют всю историю, дописывая ответ ассистента и новое сообщение пользователя:

```python
history = [
    {"role": "user", "content": "My name is Alice."},
    {"role": "assistant", "content": "Hello Alice!"},
    {"role": "user", "content": "What is my name?"},
]
client.messages.create(model="claude-opus-5-5", max_tokens=1024, system="You are terse.", messages=history)
```

Все три сообщения ушли в одном теле запроса (в опыте: `"messages": [{"role": "user", …}, {"role": "assistant", …}, {"role": "user", …}]`). Следствия:

- стоимость запроса растёт с историей: прошлые сообщения тоже входят в `input_tokens`;
- первым в списке идёт сообщение `user`;
- в историю добавляют **весь** `message.content` ответа, а не только текст: блоки `thinking` и `tool_use` нужны модели при продолжении (подробнее — в модуле 07 дорожной карты);
- длинные диалоги упираются в окно контекста: для них нужна компактизация (модуль 13 дорожной карты).

> ⚠️ Проверить: правило объединения подряд идущих сообщений одной роли и точные ограничения на их порядок взяты из документации и руководства SDK, на реальном API не запускались.

## max_tokens и stop_reason

`stop_reason` показывает, **почему** модель остановилась:

| Значение | Смысл |
|---|---|
| `end_turn` | модель закончила ответ сама |
| `max_tokens` | достигнут лимит `max_tokens`: ответ обрезан |
| `stop_sequence` | встретилась одна из ваших `stop_sequences` |
| `tool_use` | модель просит вызвать инструмент ([модуль 07](../../overview.ru.md) дорожной карты) |
| `pause_turn` | ход приостановлен и его можно продолжить |
| `refusal` | модель отказалась по соображениям безопасности; подробности в `stop_details` |

```python
m = client.messages.create(model="claude-opus-5-5", max_tokens=5, messages=[{"role": "user", "content": "Capital of France?"}])
print(m.stop_reason)
```

```text
max_tokens
```

(Мок вернул `max_tokens` для малого лимита, как это сделала бы настоящая модель при обрезании.) Если вы видите `max_tokens`, ответ **неполный**: увеличьте лимит или переходите на стриминг. Для обычных запросов без стриминга разумный лимит — порядка десятков тысяч токенов, а не сотни: слишком маленькое значение приводит к обрыву ответа и повторам.

Всегда проверяйте `stop_reason` до чтения `content`. Для `refusal` поле `stop_details` содержит категорию и пояснение; запрос при этом завершается кодом 200, а не ошибкой.

## Ошибки

Ошибка API приходит HTTP-кодом и JSON-телом с полями `type: "error"`, `error.type`, `error.message` и `request_id`. SDK превращает её в исключение:

```python
try:
    client.messages.create(model="claude-opus-5-5", max_tokens=1, messages=[{"role": "user", "content": "x"}])
except anthropic.BadRequestError as e:
    print(type(e).__name__, e.status_code, e.body["error"]["type"], e.request_id)
```

```text
BadRequestError 400 invalid_request_error req_mock123
```

(Ошибку вернул мок для проверки разбора.) Основные коды:

| Код | `error.type` | Исключение SDK (Python) |
|---|---|---|
| 400 | `invalid_request_error` | `BadRequestError` |
| 401 | `authentication_error` | `AuthenticationError` |
| 403 | `permission_error` | `PermissionDeniedError` |
| 404 | `not_found_error` | `NotFoundError` |
| 413 | `request_too_large` | `RequestTooLargeError` |
| 429 | `rate_limit_error` | `RateLimitError` |
| 500 | `api_error` | `InternalServerError` |
| 529 | `overloaded_error` | `OverloadedError` (подкласс `InternalServerError`) |

Все они наследуют `anthropic.APIStatusError` ← `anthropic.APIError` (в опыте `issubclass(RateLimitError, APIStatusError)` и `issubclass(APIStatusError, APIError)` — `True`); ошибки сети — `APIConnectionError`, таймаут — `APITimeoutError`. Ловите сначала частные классы, затем общие, и не разбирайте текст сообщений.

SDK **автоматически повторяет** временные сбои (сеть, 429, 5xx) с экспоненциальной паузой, по умолчанию два раза: в опыте после двух ответов 429 третий запрос прошёл, и код ничего не заметил (сервер увидел три вызова). Число повторов задаёт `max_retries` (в опыте у клиента по умолчанию `2`).

## Ошибки новичков

- ❌ `message.content[0].text` без проверки типа блока → ✅ цикл по блокам с `if block.type == "text"`.
- ❌ Ждать, что модель помнит прошлый запрос → ✅ отправлять историю в `messages`.
- ❌ Не смотреть на `stop_reason` → ✅ проверять `max_tokens` и `refusal`.
- ❌ Слишком маленький `max_tokens` → ✅ запас, стриминг для больших ответов.
- ❌ Система инструкций в сообщении пользователя → ✅ параметр `system`.
- ❌ Передавать `temperature`/`top_p`/`top_k` в новом коде → ✅ при версии API `2026-10-21` они дают 400 (подробнее в [продвинутой теории](advanced.ru.md)).
- ❌ Разбирать текст исключения → ✅ типизированные классы SDK и `status_code`.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — стриминг, `stop_sequences`, мышление, повторы, `count_tokens`
- Ранее: [01 · Семейство Claude и выбор модели](../01-models/basics.ru.md)
- Параллели: [Python](../../../python/overview.ru.md) · [JavaScript: промисы и async/await](../../../javascript/modules/12-promises-and-async-await/basics.ru.md)

## Источники

- [Messages API](https://platform.claude.com/docs/en/api/messages)
- [Ошибки API](https://platform.claude.com/docs/en/api/errors)
- [Версии API](https://platform.claude.com/docs/en/api/versioning)
- [Python SDK](https://platform.claude.com/docs/en/cli-sdks-libraries/sdks/python)
