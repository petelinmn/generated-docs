---
title: "Anthropic SDK: fundamentals"
lang: en
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

# Anthropic SDK: fundamentals

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [02 · Messages API](../../modules/02-messages-api/overview.en.md)

## How this was checked

The code ran with `anthropic` 2.14.0 (Python) and `@anthropic-ai/sdk` 1.14.0 (TypeScript) against a local stand-in; no real requests were made.

## What it is

The official Claude API clients. The SDK handles auth, types, retries, streaming and pagination.

| Language | Package | Version |
|---|---|---|
| Python (3.10+) | `anthropic` | 2.14.0 |
| TypeScript (Node.js 20+) | `@anthropic-ai/sdk` | 1.14.0 |
| C# | `Anthropic` | 13.14.0 |
| Go | `github.com/anthropics/anthropic-sdk-go/v2` | 2.15.0 |
| Java | `com.anthropic:anthropic-java` | 3.14.0 |
| PHP | `anthropic-ai/sdk` | 1.14.0 |
| Ruby | `anthropic` | 2.14.0 |

Versions as listed on 25 November 2026; only Python and TypeScript were run.

## The first request

```python
from anthropic import Anthropic

client = Anthropic()                      # reads ANTHROPIC_API_KEY
message = client.messages.create(
    model="claude-opus-5-5", max_tokens=1024,
    messages=[{"role": "user", "content": "hi"}],
)
print(message.content[0].text, message._request_id)
```

Replies are pydantic objects: `message.to_dict()`, `message.to_json()`. `_request_id` is public despite the underscore; log it. Without a key the first request raises `TypeError` “Could not resolve authentication method”.

## Streaming

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

`messages.stream(...)` accumulates the message and offers `text_stream`. `messages.create(..., stream=True)` yields raw events without accumulating.

## The async client

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

After `async with` the client is closed: a request on it fails with `APIConnectionError`. Create one client per application, not per request: it holds a connection pool.

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
console.log(final.stop_reason, stream.request_id);
```

## Errors

| Class | When |
|---|---|
| `BadRequestError` | 400 |
| `AuthenticationError` / `PermissionDeniedError` | 401 / 403 |
| `NotFoundError` | 404 |
| `RateLimitError` | 429 |
| `InternalServerError`, `OverloadedError` | 5xx, 529 |
| `APIConnectionError`, `APITimeoutError` | no response |

Status errors carry `status_code`, `type`, `request_id`, `body`. The common base is `APIError`. What to retry is covered in [17 · Deployment](../../modules/17-production/advanced.en.md).

## Related materials

- [Advanced theory](advanced.en.md)
- [02 · Messages API](../../modules/02-messages-api/basics.en.md) · [14 · Batch](../../modules/14-evals/advanced.en.md)

## Sources

- [Python SDK](https://platform.claude.com/docs/en/cli-sdks-libraries/sdks/python)
- [TypeScript SDK](https://platform.claude.com/docs/en/cli-sdks-libraries/sdks/typescript)
- [Client SDKs](https://platform.claude.com/docs/en/cli-sdks-libraries/overview)
