---
title: "Messages API: продвинутый уровень"
lang: ru
track: claude
kind: module
doc: advanced
slug: 02-messages-api
part: 1
tags: [claude, messages-api, streaming, stop-sequences, adaptive-thinking, effort, retries, count-tokens, request-id]
versions: "Claude 5.5 family; Messages API, anthropic-version 2026-10-21; Python SDK anthropic 2.13.0"
status: draft
updated: 2026-11-24
---

# Messages API: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md). Код запускался против локального мок-сервера (см. раздел «Как проверялся код» базового уровня): проверены форма запросов и поведение SDK `anthropic` 2.13.0, но не ответы реальных моделей.

## Потоковая выдача

С `stream: true` ответ приходит по частям как server-sent events (SSE), и текст можно показывать по мере генерации. В Python удобнее всего менеджер `client.messages.stream`:

```python
with client.messages.stream(model="claude-opus-5-5", max_tokens=1024, messages=[{"role": "user", "content": "Capital of France?"}]) as stream:
    chunks = [t for t in stream.text_stream]
    final = stream.get_final_message()
print(chunks, final.content[0].text, final.stop_reason, final.usage.output_tokens)
```

```text
['Par', 'is.'] Paris. end_turn 6
```

- `stream.text_stream` отдаёт только фрагменты текста (в опыте `'Par'`, `'is.'`).
- `stream.get_final_message()` собирает из событий итоговый объект `Message`, **идентичный** результату обычного запроса: так удобно получать готовый ответ, не разбирая события.
- Тело запроса при этом то же, плюс поле `"stream": true` (в опыте).
- Если нужны отдельные события, можно просто перебирать `stream`; в опыте получен такой поток типов: `message_start`, `content_block_start`, `content_block_delta`, `text`, `content_block_delta`, `text`, `content_block_stop`, `message_delta`, `message_stop` (событие `text` — удобство SDK, а сетевое событие `ping` SDK не показывает).
- Сырой поток без помощников даёт `client.messages.create(..., stream=True)`: в нём только сетевые события (`message_start` … `message_stop`).

**Когда стриминг обязателен.** Не используйте большой `max_tokens` без стриминга: простаивающие соединения могут обрываться, а SDK проверяет, что не-стриминговый запрос не должен превысить таймаут в 10 минут. Для очень больших ответов (десятки тысяч токенов и более) берите `client.messages.stream(...)` и `get_final_message()`. Структура самих событий описана в [экспертной теории](expert.ru.md#протокол-sse-потоковых-событий).

**Ошибка посреди стрима.** Ответ уже начался с кодом 200, поэтому сбой приходит событием `error` внутри потока, а не HTTP-кодом; обработка таких ошибок отличается от обычной ([экспертная теория](expert.ru.md#таблица-ошибок)).

## stop_sequences

`stop_sequences` — строки, при появлении которых модель останавливается; сама строка в ответ не попадает:

```python
m = client.messages.create(model="claude-opus-5-5", max_tokens=100, stop_sequences=["4"], messages=[{"role": "user", "content": "Count from 1 to 5"}])
print(m.stop_reason, repr(m.stop_sequence), repr(m.content[0].text))
```

```text
stop_sequence '4' '1, 2, 3, '
```

Сработавшая последовательность возвращается в поле `stop_sequence`, а `stop_reason` равен `stop_sequence`. (Ответ в примере условный: его сформировал мок, чтобы показать разбор.) Применяют для остановки на разделителях, например, при генерации списка до маркера.

## Мышление и effort

Современные модели рассуждают перед ответом; управление передаётся двум параметрам: `thinking` и `output_config.effort`.

```python
client.messages.create(
    model="claude-opus-5-5", max_tokens=2000,
    thinking={"type": "adaptive", "display": "summarized"},
    output_config={"effort": "high"},
    messages=[{"role": "user", "content": "hi"}],
)
```

В опыте SDK отправил `"thinking": {"type": "adaptive", "display": "summarized"}` и `"output_config": {"effort": "high"}`.

- **`thinking: {"type": "adaptive"}`** — режим, при котором модель сама решает, думать ли и сколько. Это рекомендуемое значение; на Claude Opus 5.5 мышление включено всегда: параметр можно опустить, а `{"type": "disabled"}` и фиксированный бюджет `budget_tokens` возвращают 400.
- **`display`** — как показывать рассуждения: `"summarized"` возвращает краткое изложение, `"omitted"` (по умолчанию на этих моделях) оставляет поле `thinking` пустым. Мышление оплачивается одинаково при любом `display`.
- **`output_config.effort`** — глубина и расход токенов: `low`, `medium`, `high`, `xhigh`, `max`. Для Claude Opus 5.5 значение по умолчанию — `medium`, поэтому задавайте его явно.
- Блоки `thinking` возвращаются в `content`; при продолжении диалога их передают обратно без изменений (иначе 400 с сообщением, что блоки нельзя изменять).
- Подробнее о выборе effort — [модуль 01](../01-models/advanced.ru.md); о мышлении как инструменте промптинга — модуль 03 дорожной карты.

> ⚠️ Проверить: правила для конкретных моделей (какие значения `thinking` принимает каждая) взяты из документации на дату подготовки; они часто меняются при выходе новых моделей — сверяйте со страницей моделей.

## Параметры, удалённые в версии API 2026-10-21

При версии `2026-10-21` параметры `temperature`, `top_p` и `top_k` **удалены**: запрос, в котором есть любой из них, возвращает 400 на любой модели. Поведение ответа задают промптом, `effort` и `max_tokens`, а не «температурой». SDK 2.13.0 по умолчанию отправляет именно эту версию (в опыте `anthropic-version: 2026-10-21`). Если вы переносите старый код, уберите эти параметры.

Остальное, что изменилось на новых моделях: **prefill** (заполнение начала ответа сообщением ассистента) возвращает 400, а принудительный `tool_choice` `any`/`tool` не поддерживается на ряде моделей — вместо этого просите формат в промпте или используйте structured outputs (модуль 04 дорожной карты).

## Повторы, таймауты и параметры клиента

```python
print(client.timeout, client.max_retries)
c2 = client.with_options(timeout=5.0, max_retries=0)
print(c2.timeout, c2.max_retries)
```

```text
Timeout(connect=5.0, read=600, write=600, pool=600) 2
5.0 0
```

- По умолчанию SDK ждёт ответа до 10 минут (`read=600`) и повторяет сбои дважды.
- `client.with_options(...)` создаёт копию клиента с другими параметрами; те же аргументы принимает конструктор `Anthropic(...)`.
- Повторяются сетевые ошибки, 408/409/429 и 5xx; пауза растёт экспоненциально и учитывает заголовок `retry-after`. Таймаут тоже повторяется, поэтому общее время может достигать `timeout × (повторы + 1)`.
- 4xx (кроме перечисленных) повторять бессмысленно: исправляйте запрос.

## Request ID и заголовки ответа

Каждый ответ содержит заголовок `request-id`; его же можно прочитать у объекта ответа и приложить к обращению в поддержку:

```python
print(msg._request_id)
raw = client.messages.with_raw_response.create(model="claude-opus-5-5", max_tokens=1, messages=[{"role": "user", "content": "x"}])
print(raw.headers["request-id"], raw.parse().id)
```

```text
req_mock123
req_mock123 msg_mock01
```

`with_raw_response` отдаёт заголовки HTTP и объект, который затем разбирается через `.parse()`; в исключениях значение есть в `e.request_id`.

## Подсчёт токенов

`count_tokens` оценивает размер запроса до отправки и не создаёт ответ:

```python
ct = client.messages.count_tokens(model="claude-opus-5-5", messages=[{"role": "user", "content": "Hello"}])
print(ct.input_tokens)
```

```text
14
```

(Число вернул мок; настоящий подсчёт зависит от токенизатора модели.) Подсчёт нужен, чтобы заранее оценить стоимость и убедиться, что запрос влезает в окно контекста. Используйте его, а не сторонние библиотеки: токенизаторы у моделей различаются.

## Асинхронный клиент

```python
import asyncio
async def main():
    ac = anthropic.AsyncAnthropic()
    r = await ac.messages.create(model="claude-opus-5-5", max_tokens=10, messages=[{"role": "user", "content": "x"}])
    return r.content[0].text
print(asyncio.run(main()))
```

```text
Paris.
```

`AsyncAnthropic` повторяет интерфейс синхронного клиента, но методы нужно ждать через `await`; это основа параллельных запросов (`asyncio.gather`), см. также [JavaScript: промисы и async/await](../../../javascript/modules/12-promises-and-async-await/advanced.ru.md).

## Лучшие практики

- Читайте ответ по блокам и проверяйте `stop_reason`.
- Для больших ответов — стриминг и `get_final_message()`.
- Задавайте `effort` явно; не используйте `temperature`, `top_p`, `top_k`.
- Не меняйте и не отбрасывайте блоки `thinking`, полученные в ответе, при продолжении диалога.
- Ловите типизированные исключения и логируйте `request_id`.
- Полагайтесь на повторы SDK, но задавайте `timeout` под вашу задачу.
- Считайте токены заранее для длинных входов.

## Частые ошибки

- ❌ Большой `max_tokens` без стриминга → ✅ `client.messages.stream(...)`.
- ❌ `{"type": "disabled"}` для мышления на моделях, где оно всегда включено → ✅ опустить параметр и снизить effort.
- ❌ Отправка `temperature` в запросе → ✅ убрать параметр.
- ❌ Читать только `content[0]` в ответе с мышлением → ✅ перебирать блоки.
- ❌ Потерять `request_id` при ошибке → ✅ логировать `e.request_id`.
- ❌ Ждать, что `stop_sequence` останется в тексте → ✅ он отсекается и приходит отдельным полем.

## Связанные материалы

- Дальше: [экспертная теория](expert.ru.md)
- Выбор модели и effort: [01 · Семейство Claude и выбор модели](../01-models/advanced.ru.md)
- Асинхронность: [JavaScript: промисы и async/await](../../../javascript/modules/12-promises-and-async-await/advanced.ru.md)

## Источники

- [Streaming messages](https://platform.claude.com/docs/en/build-with-claude/streaming)
- [Ошибки API: длинные запросы и валидация](https://platform.claude.com/docs/en/api/errors)
- [Версии API](https://platform.claude.com/docs/en/api/versioning)
- [Token counting](https://platform.claude.com/docs/en/build-with-claude/token-counting)
- [Python SDK](https://platform.claude.com/docs/en/cli-sdks-libraries/sdks/python)
