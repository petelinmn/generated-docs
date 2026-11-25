---
title: "Anthropic SDK: advanced"
lang: en
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

# Anthropic SDK: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Client and per-request options

```python
from anthropic import Anthropic

client = Anthropic(max_retries=2, timeout=30.0)
careful = client.with_options(max_retries=5, timeout=120.0)   # a copy; client is unchanged

raw = careful.messages.with_raw_response.create(
    model="claude-opus-5-5", max_tokens=100, messages=[{"role": "user", "content": "hi"}])
print(raw.retries_taken, raw.headers["request-id"])
message = raw.parse()
```

| Option | Default |
|---|---|
| `max_retries` | 2 |
| `timeout` | 600 s (5 s connect) |
| `base_url` | `https://api.anthropic.com`, `ANTHROPIC_BASE_URL` |
| `api_key` | `ANTHROPIC_API_KEY` |

TypeScript: `maxRetries` (2), `timeout` in milliseconds (600000), `.withResponse()` instead of `with_raw_response`, the error field `requestID`.

## Pagination

`list` methods return a page. A `for` loop over it fetches further pages by itself; `page.data` is only the current one. There are `has_next_page()` and `get_next_page()`. In the check three models with `limit=2` took two requests.

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

- The schema comes from type hints and the docstring.
- A tool exception becomes a `tool_result` with `is_error` and the exception's `repr`: **the model sees that text**. Arguments that fail validation do the same.
- `max_iterations` caps the requests; at the cap you can end with `stop_reason == "tool_use"`.

**A thread by default.** Without `thread=False` the runner on the Claude API and Foundry uses [Message Threads](https://platform.claude.com/docs/en/build-with-claude/message-threads) (beta `message-threads-2026-08-12`): the server keeps the history and the next request carries only new messages and `previous_message_id`. Content is held in server memory for at most an hour after last use, and the feature is **not ZDR-eligible**. The Bedrock and Vertex clients sent the full history in the check. In TypeScript use `thread: false`.

## Structured output

`messages.parse(..., output_format=Model)` returns `parsed_output`. If the reply is cut off (`max_tokens`), `parse` itself raises `ValidationError` — see [module 04](../../modules/04-structured-output/overview.en.md).

## Testing without a network

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

This exercises retries, errors and parsing with the real SDK code. The SDK uses the `httpx2` package, not `httpx`: tools patching `httpx` (such as `respx`) do not see its requests unless `httpx2.alias_httpx()` is called.

## Logging

`ANTHROPIC_LOG=info` logs a line per request. `ANTHROPIC_LOG=debug` also logs request options **including message text**; the key did not appear in the check. Keep debug off where personal data flows.

## Related materials

- [Fundamentals](basics.en.md) · [Expert](expert.en.md)
- [07 · Tool use](../../modules/07-tool-use/advanced.en.md) · [16 · Security](../../modules/16-security/basics.en.md)

## Sources

- [Python SDK](https://platform.claude.com/docs/en/cli-sdks-libraries/sdks/python)
- [Message threads](https://platform.claude.com/docs/en/build-with-claude/message-threads)
