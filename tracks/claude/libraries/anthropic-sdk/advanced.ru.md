---
title: "Anthropic SDK: продвинутый уровень"
lang: ru
track: claude
kind: library
doc: advanced
slug: anthropic-sdk
part: 2
tags: [claude, sdk, anthropic-sdk, python, typescript]
versions: "anthropic 2.14.0; @anthropic-ai/sdk 1.14.0"
status: draft
updated: 2026-11-25
---

# Anthropic SDK: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Настройки на клиент и на запрос

```python
from anthropic import Anthropic

client = Anthropic(max_retries=2, timeout=30.0)
careful = client.with_options(max_retries=5, timeout=120.0)   # a copy; client is unchanged

raw = careful.messages.with_raw_response.create(
    model="claude-opus-5-5", max_tokens=100, messages=[{"role": "user", "content": "hi"}])
print(raw.retries_taken, raw.headers["request-id"])
message = raw.parse()
```

| Параметр | По умолчанию |
|---|---|
| `max_retries` | 2 |
| `timeout` | 600 с (соединение 5 с) |
| `base_url` | `https://api.anthropic.com`, `ANTHROPIC_BASE_URL` |
| `api_key` | `ANTHROPIC_API_KEY` |

TypeScript: `maxRetries` (2), `timeout` в миллисекундах (600000), `.withResponse()` вместо `with_raw_response`, поле ошибки `requestID`.

## Пагинация

Методы `list` возвращают страницу. Цикл `for` по ней сам загружает следующие страницы; `page.data` — только текущую. Есть `has_next_page()` и `get_next_page()`. В проверке три модели при `limit=2` потребовали два запроса.

## Tool runner

```python
from anthropic import Anthropic, beta_tool


@beta_tool
def get_weather(city: str) -> str:
    """Get the weather for a city.

    Args:
        city: City name.
    """
    return f"{city}: 5C"


client = Anthropic()
runner = client.beta.messages.tool_runner(
    model="claude-opus-5-5", max_tokens=1024, max_iterations=5,
    thread=False,                      # send the full history; see below
    tools=[get_weather],
    messages=[{"role": "user", "content": "weather"}],
)
final = runner.until_done()
print(final.content[0].text)
```

- Схема строится из аннотаций типов и docstring.
- Исключение инструмента становится `tool_result` с `is_error` и текстом `repr` исключения — **этот текст видит модель**. Аргументы не по схеме дают такой же результат.
- `max_iterations` ограничивает число запросов; на пределе можно получить сообщение с `stop_reason == "tool_use"`.

**Thread по умолчанию.** Без `thread=False` runner на Claude API и Foundry использует [Message Threads](https://platform.claude.com/docs/en/build-with-claude/message-threads) (бета `message-threads-2026-08-12`): историю держит сервер, а следующий запрос несёт только новые сообщения и `previous_message_id`. Содержимое хранится в памяти сервера не дольше часа после последнего использования, и функция **не подпадает под ZDR**. Клиенты Bedrock и Vertex в проверке отправляли полную историю. В TypeScript — `thread: false`.

## Структурированный вывод

`messages.parse(..., output_format=Model)` возвращает `parsed_output`. Если ответ обрезан (`max_tokens`), разбор JSON бросает `ValidationError` уже из `parse` — см. [модуль 04](../../modules/04-structured-output/overview.ru.md).

## Тесты без сети

```python
# stub.py
import httpx2
from anthropic import Anthropic, DefaultHttpxClient

REPLY = {"id": "msg_1", "type": "message", "role": "assistant", "model": "claude-opus-5-5",
         "content": [{"type": "text", "text": "stubbed"}], "stop_reason": "end_turn",
         "stop_sequence": None, "usage": {"input_tokens": 1, "output_tokens": 1}}


def stub_client(handler=lambda request: httpx2.Response(200, json=REPLY), **options):
    """A real SDK client whose network layer is a function."""
    http = DefaultHttpxClient(transport=httpx2.MockTransport(handler))
    return Anthropic(api_key="test", http_client=http, **options)
```

Так проверяются повторы, ошибки и разбор ответа настоящим кодом SDK. SDK использует пакет `httpx2`, а не `httpx`: инструменты, перехватывающие `httpx` (например, `respx`), его запросов не видят, пока не вызван `httpx2.alias_httpx()`.

## Логирование

`ANTHROPIC_LOG=info` пишет строку на запрос. `ANTHROPIC_LOG=debug` пишет и параметры запроса **вместе с текстом сообщений**; ключ в проверке в лог не попал. Не включайте debug там, где есть персональные данные.

## Связанные материалы

- [Базовая теория](basics.ru.md) · [Эксперт](expert.ru.md)
- [07 · Tool use](../../modules/07-tool-use/advanced.ru.md) · [16 · Безопасность](../../modules/16-security/basics.ru.md)

## Источники

- [Python SDK](https://platform.claude.com/docs/en/cli-sdks-libraries/sdks/python)
- [Message threads](https://platform.claude.com/docs/en/build-with-claude/message-threads)
