---
title: "Messages API: advanced"
lang: en
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

# Messages API: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on the [basic theory](basics.en.md). The code was run against a local mock server (see the "How the code was checked" section of the basic level): the shape of the requests and the behavior of the `anthropic` 2.13.0 SDK were checked, not the replies of real models.

## Streaming

With `stream: true` the reply arrives in parts as server-sent events (SSE), and text can be shown as it is generated. In Python the most convenient is the `client.messages.stream` manager:

```python
with client.messages.stream(model="claude-opus-5-5", max_tokens=1024, messages=[{"role": "user", "content": "Capital of France?"}]) as stream:
    chunks = [t for t in stream.text_stream]
    final = stream.get_final_message()
print(chunks, final.content[0].text, final.stop_reason, final.usage.output_tokens)
```

```text
['Par', 'is.'] Paris. end_turn 6
```

- `stream.text_stream` yields only text fragments (in the experiment `'Par'`, `'is.'`).
- `stream.get_final_message()` assembles from the events the final `Message` object, **identical** to the result of an ordinary request: handy for getting a finished reply without handling events.
- The request body is the same plus the field `"stream": true` (in the experiment).
- If you need individual events, just iterate `stream`; in the experiment this stream of types was obtained: `message_start`, `content_block_start`, `content_block_delta`, `text`, `content_block_delta`, `text`, `content_block_stop`, `message_delta`, `message_stop` (the `text` event is an SDK convenience, and the network event `ping` is not shown by the SDK).
- A raw stream without helpers is given by `client.messages.create(..., stream=True)`: it has only network events (`message_start` … `message_stop`).

**When streaming is required.** Do not use a large `max_tokens` without streaming: idle connections may be dropped, and the SDK checks that a non-streaming request is not expected to exceed a 10-minute timeout. For very large replies (tens of thousands of tokens and more) use `client.messages.stream(...)` and `get_final_message()`. The structure of the events themselves is in the [expert theory](expert.en.md#the-sse-streaming-protocol).

**An error in the middle of a stream.** The response has already started with code 200, so a failure arrives as an `error` event inside the stream, not as an HTTP code; handling such errors differs from the usual ([expert theory](expert.en.md#error-table)).

## stop_sequences

`stop_sequences` are strings at whose appearance the model stops; the string itself is not included in the reply:

```python
m = client.messages.create(model="claude-opus-5-5", max_tokens=100, stop_sequences=["4"], messages=[{"role": "user", "content": "Count from 1 to 5"}])
print(m.stop_reason, repr(m.stop_sequence), repr(m.content[0].text))
```

```text
stop_sequence '4' '1, 2, 3, '
```

The sequence that fired is returned in the `stop_sequence` field, and `stop_reason` is `stop_sequence`. (The reply in the example is a stand-in: the mock produced it to show the parsing.) It is used to stop at delimiters, for example when generating a list up to a marker.

## Thinking and effort

Modern models reason before answering; the control is passed to two parameters: `thinking` and `output_config.effort`.

```python
client.messages.create(
    model="claude-opus-5-5", max_tokens=2000,
    thinking={"type": "adaptive", "display": "summarized"},
    output_config={"effort": "high"},
    messages=[{"role": "user", "content": "hi"}],
)
```

In the experiment the SDK sent `"thinking": {"type": "adaptive", "display": "summarized"}` and `"output_config": {"effort": "high"}`.

- **`thinking: {"type": "adaptive"}`** is the mode in which the model decides itself whether to think and how much. It is the recommended value; on Claude Opus 5.5 thinking is always on: the parameter can be omitted, while `{"type": "disabled"}` and a fixed `budget_tokens` return a 400.
- **`display`** — how to show the reasoning: `"summarized"` returns a short summary, `"omitted"` (the default on these models) leaves the `thinking` field empty. Thinking is billed the same under any `display`.
- **`output_config.effort`** — depth and token spend: `low`, `medium`, `high`, `xhigh`, `max`. For Claude Opus 5.5 the default is `medium`, so set it explicitly.
- `thinking` blocks come back in `content`; when continuing a conversation they are passed back unchanged (otherwise a 400 saying the blocks cannot be modified).
- More on choosing effort — [module 01](../01-models/advanced.en.md); on thinking as a prompting tool — module 03 of the roadmap.

> ⚠️ Verify: the rules for specific models (which `thinking` values each accepts) come from the documentation at the time of writing; they change often when new models ship — check the models page.

## Parameters removed in API version 2026-10-21

Under version `2026-10-21` the parameters `temperature`, `top_p` and `top_k` are **removed**: a request containing any of them returns a 400 on every model. The behavior of the reply is set by the prompt, `effort` and `max_tokens`, not by a "temperature". SDK 2.13.0 sends exactly this version by default (in the experiment `anthropic-version: 2026-10-21`). If you migrate old code, remove these parameters.

What else changed on the new models: **prefill** (starting the reply with an assistant message) returns a 400, and forced `tool_choice` `any`/`tool` is not supported on a number of models — ask for the format in the prompt or use structured outputs (module 04 of the roadmap) instead.

## Retries, timeouts and client parameters

```python
print(client.timeout, client.max_retries)
c2 = client.with_options(timeout=5.0, max_retries=0)
print(c2.timeout, c2.max_retries)
```

```text
Timeout(connect=5.0, read=600, write=600, pool=600) 2
5.0 0
```

- By default the SDK waits for a reply up to 10 minutes (`read=600`) and retries failures twice.
- `client.with_options(...)` creates a copy of the client with other parameters; the `Anthropic(...)` constructor takes the same arguments.
- Network errors, 408/409/429 and 5xx are retried; the pause grows exponentially and respects the `retry-after` header. A timeout is retried too, so the total time can reach `timeout × (retries + 1)`.
- Retrying 4xx (other than those listed) is pointless: fix the request.

## Request ID and response headers

Every response contains a `request-id` header; it can also be read from the response object and attached to a support request:

```python
print(msg._request_id)
raw = client.messages.with_raw_response.create(model="claude-opus-5-5", max_tokens=1, messages=[{"role": "user", "content": "x"}])
print(raw.headers["request-id"], raw.parse().id)
```

```text
req_mock123
req_mock123 msg_mock01
```

`with_raw_response` gives the HTTP headers and an object that is then parsed through `.parse()`; in exceptions the value is in `e.request_id`.

## Counting tokens

`count_tokens` estimates the size of a request before sending and does not create a reply:

```python
ct = client.messages.count_tokens(model="claude-opus-5-5", messages=[{"role": "user", "content": "Hello"}])
print(ct.input_tokens)
```

```text
14
```

(The number was returned by the mock; the real count depends on the model's tokenizer.) Counting is needed to estimate cost in advance and to make sure a request fits the context window. Use it rather than third-party libraries: models' tokenizers differ.

## The async client

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

`AsyncAnthropic` mirrors the interface of the synchronous client, but the methods must be awaited with `await`; this is the basis of parallel requests (`asyncio.gather`), see also [JavaScript: promises and async/await](../../../javascript/modules/12-promises-and-async-await/advanced.en.md).

## Best practices

- Read the reply block by block and check `stop_reason`.
- For big replies — streaming and `get_final_message()`.
- Set `effort` explicitly; do not use `temperature`, `top_p`, `top_k`.
- Do not change or drop the `thinking` blocks received in a reply when continuing a conversation.
- Catch typed exceptions and log `request_id`.
- Rely on the SDK's retries, but set a `timeout` suited to your task.
- Count tokens in advance for long inputs.

## Common mistakes

- ❌ A big `max_tokens` without streaming → ✅ `client.messages.stream(...)`.
- ❌ `{"type": "disabled"}` for thinking on models where it is always on → ✅ omit the parameter and lower the effort.
- ❌ Sending `temperature` in the request → ✅ remove the parameter.
- ❌ Reading only `content[0]` in a reply with thinking → ✅ iterate the blocks.
- ❌ Losing the `request_id` on an error → ✅ log `e.request_id`.
- ❌ Expecting the `stop_sequence` to stay in the text → ✅ it is cut off and arrives in a separate field.

## Related materials

- Next: [expert theory](expert.en.md)
- Model selection and effort: [01 · The Claude family and model selection](../01-models/advanced.en.md)
- Asynchrony: [JavaScript: promises and async/await](../../../javascript/modules/12-promises-and-async-await/advanced.en.md)

## Sources

- [Streaming messages](https://platform.claude.com/docs/en/build-with-claude/streaming)
- [API errors: long requests and validation](https://platform.claude.com/docs/en/api/errors)
- [API versions](https://platform.claude.com/docs/en/api/versioning)
- [Token counting](https://platform.claude.com/docs/en/build-with-claude/token-counting)
- [Python SDK](https://platform.claude.com/docs/en/cli-sdks-libraries/sdks/python)
