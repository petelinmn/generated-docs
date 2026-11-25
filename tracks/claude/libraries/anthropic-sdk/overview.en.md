---
title: "Anthropic SDK"
lang: en
track: claude
kind: library
doc: overview
slug: anthropic-sdk
part: 2
tags: [claude, sdk, anthropic-sdk, python, typescript]
versions: "anthropic 2.14.0; @anthropic-ai/sdk 1.14.0"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-25
---

# Anthropic SDK

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

**The official Claude API clients** for Python, TypeScript and other languages: types, retries, streaming, pagination and tool helpers.

## Key ideas

- One client per application; per-call settings on a copy via `with_options`.
- The SDK retries transient errors, but not everything.
- `stream()` accumulates the message for you.
- The tool runner uses a server-side thread by default.
- You can test without a network by swapping the transport.

## Minimal example

```python
from anthropic import Anthropic

client = Anthropic()                      # reads ANTHROPIC_API_KEY
message = client.messages.create(
    model="claude-opus-5-5", max_tokens=1024,
    messages=[{"role": "user", "content": "hi"}],
)
print(message.content[0].text, message._request_id)
```

## When to use it and when not

Almost always when calling the API from code. For a ready-made agent take the Agent SDK; raw HTTP only without an SDK for your language.

## Material levels

| Level | File | Add with |
|---|---|---|
| 1. Fundamentals | [basics.en.md](basics.en.md) | |
| 2. Advanced theory | [advanced.en.md](advanced.en.md) | |
| 3. Expert theory | [expert.en.md](expert.en.md) | |
| 4. Self-check | [quiz.en.md](quiz.en.md), [exercises.en.md](exercises.en.md) | |
| 5. Runnable examples | — | `/learn claude anthropic-sdk level 5` |
| 6. Tests | — | `/learn claude anthropic-sdk level 6` |

## Related materials

- [Claude track](../../overview.en.md)
- [02 · Messages API](../../modules/02-messages-api/overview.en.md)
- [07 · Tool use](../../modules/07-tool-use/overview.en.md)
- [Claude Agent SDK](../agent-sdk/overview.en.md)
