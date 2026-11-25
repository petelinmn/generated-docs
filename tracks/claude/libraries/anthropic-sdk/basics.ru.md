---
title: "Anthropic SDK: основы"
lang: ru
track: claude
kind: library
doc: basics
slug: anthropic-sdk
part: 2
tags: [claude, sdk, anthropic-sdk, python, typescript]
versions: "anthropic 2.14.0; @anthropic-ai/sdk 1.14.0"
status: draft
updated: 2026-11-25
---

# Anthropic SDK: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [02 · Messages API](../../modules/02-messages-api/overview.ru.md)

## Как проверялся материал

Код запускался с `anthropic` 2.14.0 (Python) и `@anthropic-ai/sdk` 1.14.0 (TypeScript) против локальной имитации API; настоящих запросов не было.

## Что это

Официальные клиенты Claude API. SDK берёт на себя авторизацию, типы, повторы, стриминг и пагинацию.

| Язык | Пакет | Версия |
|---|---|---|
| Python (3.10+) | `anthropic` | 2.14.0 |
| TypeScript (Node.js 20+) | `@anthropic-ai/sdk` | 1.14.0 |
| C# | `Anthropic` | 13.14.0 |
| Go | `github.com/anthropics/anthropic-sdk-go/v2` | 2.15.0 |
| Java | `com.anthropic:anthropic-java` | 3.14.0 |
| PHP | `anthropic-ai/sdk` | 1.14.0 |
| Ruby | `anthropic` | 2.14.0 |

Версии — по странице SDK на 25 ноября 2026; запускались только Python и TypeScript.

## Первый запрос

```python
from anthropic import Anthropic

client = Anthropic()                      # reads ANTHROPIC_API_KEY
message = client.messages.create(
    model="claude-opus-5-5", max_tokens=1024,
    messages=[{"role": "user", "content": "hi"}],
)
print(message.content[0].text, message._request_id)
```

Ответы — объекты pydantic: `message.to_dict()`, `message.to_json()`. Поле `_request_id` — публичное, несмотря на подчёркивание; пишите его в лог. Без ключа первый запрос падает с `TypeError` «Could not resolve authentication method».

## Стриминг

```python
from anthropic import Anthropic

client = Anthropic()
with client.messages.stream(model="claude-opus-5-5", max_tokens=1024,
                            messages=[{"role": "user", "content": "hi"}]) as stream:
    for text in stream.text_stream:
        print(text, end="", flush=True)
    final = stream.get_final_message()
print()
print(final.stop_reason)
```

`messages.stream(...)` собирает сообщение за вас и даёт `text_stream`. `messages.create(..., stream=True)` возвращает сырые события без накопления.

## Асинхронный клиент

```python
import asyncio

from anthropic import AsyncAnthropic


async def main():
    async with AsyncAnthropic() as client:
        limit = asyncio.Semaphore(5)               # do not fire everything at once

        async def ask(text):
            async with limit:
                return await client.messages.create(model="claude-haiku-5-5", max_tokens=100,
                                                    messages=[{"role": "user", "content": text}])

        replies = await asyncio.gather(*(ask(t) for t in ("a", "b", "c")))
    print(len(replies))


asyncio.run(main())
```

После выхода из `async with` клиент закрыт: запрос на нём завершается `APIConnectionError`. Создавайте один клиент на приложение, а не на запрос: он держит пул соединений.

## TypeScript

```typescript
import Anthropic from "@anthropic-ai/sdk";

const client = new Anthropic();
const stream = client.messages.stream({
  model: "claude-opus-5-5",
  max_tokens: 1024,
  messages: [{ role: "user", content: "hi" }],
});
stream.on("text", (text) => process.stdout.write(text));
const final = await stream.finalMessage();
console.log(final.stop_reason, final._request_id);
```

## Ошибки

| Класс | Когда |
|---|---|
| `BadRequestError` | 400 |
| `AuthenticationError` / `PermissionDeniedError` | 401 / 403 |
| `NotFoundError` | 404 |
| `RateLimitError` | 429 |
| `InternalServerError`, `OverloadedError` | 5xx, 529 |
| `APIConnectionError`, `APITimeoutError` | нет ответа |

У ошибок статуса есть `status_code`, `type`, `request_id`, `body`. Общий предок — `APIError`. Что повторять и как — в [17 · Развёртывание](../../modules/17-production/advanced.ru.md).

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [02 · Messages API](../../modules/02-messages-api/basics.ru.md) · [14 · Batch](../../modules/14-evals/advanced.ru.md)

## Источники

- [Python SDK](https://platform.claude.com/docs/en/cli-sdks-libraries/sdks/python)
- [TypeScript SDK](https://platform.claude.com/docs/en/cli-sdks-libraries/sdks/typescript)
- [Client SDKs](https://platform.claude.com/docs/en/cli-sdks-libraries/overview)
