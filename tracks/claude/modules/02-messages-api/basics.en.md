---
title: "Messages API: basics"
lang: en
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

# Messages API: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [01 · The Claude family and model selection](../01-models/overview.en.md), [Python](../../../python/overview.en.md) · **Time:** ~1.5 h

## How the code was checked

There was no API key while this material was prepared, so the code was run against a **local mock server**: it accepts requests from the `anthropic` 2.13.0 SDK, saves the body and headers and returns predefined replies. That checks what your code and the SDK do: the shape of the request, parsing of the response, exceptions, retries, streaming events. The reply texts in the output (`Paris.` and the like) are stand-ins: the replies of real models were not checked.

## Why it matters

Every Claude integration starts with a call to the Messages API. If you understand what exactly goes out in the request and what comes back, the other capabilities (tools, caching, structured output) turn out to be additions to the same request.

## Request and response

A minimal request has three required fields: `model`, `max_tokens`, `messages`.

```python
import anthropic

client = anthropic.Anthropic()  # the key is taken from the ANTHROPIC_API_KEY variable

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

The SDK sent this request body (`POST /v1/messages`):

```json
{"max_tokens": 1024, "messages": [{"role": "user", "content": "What is the capital of France?"}], "model": "claude-opus-5-5"}
```

- **`model`** is the model identifier (for example `claude-opus-5-5`, see [module 01](../01-models/basics.en.md)).
- **`max_tokens`** is the maximum number of tokens in the reply; it is a ceiling, not a target.
- **`messages`** is a list of messages; each has a `role` (`user` or `assistant`) and `content` (a string or a list of blocks).
- The response is a `Message` object: `id`, `role` (always `assistant`), `content` (a list of blocks), `model`, `stop_reason`, `usage` (`input_tokens`, `output_tokens`).
- A string in `content` is shorthand: `"content": "Hi"` is equivalent to a list of one block `{"type": "text", "text": "Hi"}`.

**Reading the response.** `content` is a list of blocks of different types, so check `block.type` before reaching for `.text`:

```python
for block in message.content:
    if block.type == "text":
        print(block.text)
```

In a simple request without tools and thinking there is one block, but code that relies only on `content[0].text` breaks as soon as `thinking` or `tool_use` blocks appear in the reply.

## Headers and the API version

Every request carries the headers `anthropic-version`, `content-type: application/json` and a key. The SDK sets them itself: in the experiment SDK 2.13.0 sent `anthropic-version: 2026-10-21` and `content-type: application/json`, and passed the key in the `authorization` header (the SDK handles the authentication details). If you call the API without an SDK, use the headers from the documentation (`x-api-key`, `anthropic-version`, `content-type`):

```bash
curl -sS https://api.anthropic.com/v1/messages \
  -H "x-api-key: $ANTHROPIC_API_KEY" \
  -H "anthropic-version: 2026-10-21" \
  -H "content-type: application/json" \
  -d '{"model": "claude-opus-5-5", "max_tokens": 1024,
       "messages": [{"role": "user", "content": "Hello, Claude"}]}'
```

Against the mock server this command returned JSON with the same fields as the SDK (`id`, `type: "message"`, `role`, `content`, `stop_reason`, `usage`), and the server received all three headers.

> API version `2026-10-21` is the current one at the time of writing; the list of changes and compatibility rules are on the [API versions](https://platform.claude.com/docs/en/api/versioning) page.

## The system prompt

Top-level instructions — a role, rules, a format — go in a separate `system` parameter, not in a user message:

```python
client.messages.create(
    model="claude-opus-5-5", max_tokens=1024,
    system="You are terse.",
    messages=[{"role": "user", "content": "What is the capital of France?"}],
)
```

`system` can be a string or a list of text blocks (`[{"type": "text", "text": "Answer in French."}]`): the list is needed when `cache_control` is added to the blocks ([module 05](../../overview.en.md) of the roadmap). What to write in `system` is covered in module 03 of the roadmap.

## A conversation: the API does not remember the history

The API is **stateless**. To continue a conversation you send the whole history every time, appending the assistant's reply and the new user message:

```python
history = [
    {"role": "user", "content": "My name is Alice."},
    {"role": "assistant", "content": "Hello Alice!"},
    {"role": "user", "content": "What is my name?"},
]
client.messages.create(model="claude-opus-5-5", max_tokens=1024, system="You are terse.", messages=history)
```

All three messages went out in one request body (in the experiment: `"messages": [{"role": "user", …}, {"role": "assistant", …}, {"role": "user", …}]`). Consequences:

- the cost of a request grows with the history: past messages are also part of `input_tokens`;
- the first message in the list is a `user` message;
- append the **whole** `message.content` of the reply to the history, not only the text: the `thinking` and `tool_use` blocks are needed by the model when it continues (more in module 07 of the roadmap);
- long conversations run into the context window: they need compaction (module 13 of the roadmap).

> ⚠️ Verify: the rule for merging consecutive messages of the same role and the exact restrictions on their order come from the documentation and the SDK guide, and were not run against the real API.

## max_tokens and stop_reason

`stop_reason` shows **why** the model stopped:

| Value | Meaning |
|---|---|
| `end_turn` | the model finished the reply itself |
| `max_tokens` | the `max_tokens` limit was reached: the reply is cut off |
| `stop_sequence` | one of your `stop_sequences` was met |
| `tool_use` | the model asks to call a tool ([module 07](../../overview.en.md) of the roadmap) |
| `pause_turn` | the turn is paused and can be continued |
| `refusal` | the model declined for safety reasons; details are in `stop_details` |

```python
m = client.messages.create(model="claude-opus-5-5", max_tokens=5, messages=[{"role": "user", "content": "Capital of France?"}])
print(m.stop_reason)
```

```text
max_tokens
```

(The mock returned `max_tokens` for a small limit, as a real model would when truncated.) If you see `max_tokens`, the reply is **incomplete**: increase the limit or move to streaming. For ordinary non-streaming requests a sensible limit is on the order of tens of thousands of tokens, not hundreds: too small a value cuts replies off and causes retries.

Always check `stop_reason` before reading `content`. For `refusal` the `stop_details` field holds the category and an explanation; the request still ends with a 200 code, not an error.

## Errors

An API error arrives as an HTTP code and a JSON body with the fields `type: "error"`, `error.type`, `error.message` and `request_id`. The SDK turns it into an exception:

```python
try:
    client.messages.create(model="claude-opus-5-5", max_tokens=1, messages=[{"role": "user", "content": "x"}])
except anthropic.BadRequestError as e:
    print(type(e).__name__, e.status_code, e.body["error"]["type"], e.request_id)
```

```text
BadRequestError 400 invalid_request_error req_mock123
```

(The error was returned by the mock to check the parsing.) The main codes:

| Code | `error.type` | SDK exception (Python) |
|---|---|---|
| 400 | `invalid_request_error` | `BadRequestError` |
| 401 | `authentication_error` | `AuthenticationError` |
| 403 | `permission_error` | `PermissionDeniedError` |
| 404 | `not_found_error` | `NotFoundError` |
| 413 | `request_too_large` | `RequestTooLargeError` |
| 429 | `rate_limit_error` | `RateLimitError` |
| 500 | `api_error` | `InternalServerError` |
| 529 | `overloaded_error` | `OverloadedError` (a subclass of `InternalServerError`) |

They all inherit `anthropic.APIStatusError` ← `anthropic.APIError` (in the experiment `issubclass(RateLimitError, APIStatusError)` and `issubclass(APIStatusError, APIError)` are `True`); network errors are `APIConnectionError`, a timeout is `APITimeoutError`. Catch the specific classes first, then the general ones, and do not parse message text.

The SDK **retries** transient failures (network, 429, 5xx) automatically with exponential backoff, twice by default: in the experiment after two 429 responses the third request went through, and the code noticed nothing (the server saw three calls). The number of retries is set by `max_retries` (in the experiment the client's default was `2`).

## Beginner mistakes

- ❌ `message.content[0].text` without checking the block type → ✅ a loop over blocks with `if block.type == "text"`.
- ❌ Expecting the model to remember a previous request → ✅ send the history in `messages`.
- ❌ Ignoring `stop_reason` → ✅ check `max_tokens` and `refusal`.
- ❌ Too small a `max_tokens` → ✅ headroom, streaming for big replies.
- ❌ Instructions in the user's message → ✅ the `system` parameter.
- ❌ Passing `temperature`/`top_p`/`top_k` in new code → ✅ under API version `2026-10-21` they give a 400 (more in the [advanced theory](advanced.en.md)).
- ❌ Parsing the exception text → ✅ the SDK's typed classes and `status_code`.

## Related materials

- Next: [advanced theory](advanced.en.md) — streaming, `stop_sequences`, thinking, retries, `count_tokens`
- Earlier: [01 · The Claude family and model selection](../01-models/basics.en.md)
- Parallels: [Python](../../../python/overview.en.md) · [JavaScript: promises and async/await](../../../javascript/modules/12-promises-and-async-await/basics.en.md)

## Sources

- [Messages API](https://platform.claude.com/docs/en/api/messages)
- [API errors](https://platform.claude.com/docs/en/api/errors)
- [API versions](https://platform.claude.com/docs/en/api/versioning)
- [Python SDK](https://platform.claude.com/docs/en/cli-sdks-libraries/sdks/python)
