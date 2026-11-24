---
title: "Messages API: expert"
lang: en
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

# Messages API: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on the [basic](basics.en.md) and [advanced](advanced.en.md) theory. The reference data comes from the Claude Platform documentation pages (Messages API, Errors, Streaming, Versioning) at the time of writing; what was run is marked "in the experiment" (a local mock server, the `anthropic` 2.13.0 SDK).

## Reference

### Request parameters of `POST /v1/messages`

| Parameter | Type and values | Notes |
|---|---|---|
| `model` | string | the model identifier |
| `max_tokens` | number ≥ 0 | the output ceiling; `0` warms the prompt cache without generating |
| `messages` | an array of `MessageParam` | `role`: `user` or `assistant`; up to 100,000 messages |
| `system` | a string or an array of text blocks | top-level instructions |
| `stop_sequences` | an array of strings | custom stops |
| `stream` | boolean | streaming (SSE) |
| `tools`, `tool_choice` | an array / an object | `auto`, `any`, `tool`, `none` + `disable_parallel_tool_use`; `any`/`tool` are not supported on a number of models |
| `thinking` | `adaptive`, `enabled` (+`budget_tokens` ≥ 1024), `disabled`, `between_tools`; `display`: `summarized`/`omitted` | the accepted values depend on the model |
| `output_config` | `effort`: `low`…`max`; `format`: a JSON schema | structured output — module 04 of the roadmap |
| `metadata` | `user_id` up to 512 characters | a user identifier for accounting |
| `service_tier` | `auto` or `standard_only` | the service tier |
| `cache_control`, `container`, `diagnostics`, `inference_geo`, `speed` | see the documentation | caching, the inference region (`inference_geo`), fast mode (`speed`: `standard`/`fast`) |

> On API version `2026-10-21` the parameters `temperature`, `top_p`, `top_k` are **removed** — a request with them gives a 400 on any model.

### Content block types

| Direction | Types |
|---|---|
| In input messages | `text`, `image`, `document`, `search_result`, `thinking`, `redacted_thinking`, `tool_use`, `tool_result`, `server_tool_use`, results of server tools (`web_search_tool_result`, `web_fetch_tool_result`, `code_execution_tool_result`, `bash_code_execution_tool_result`, `text_editor_code_execution_tool_result`, `tool_search_tool_result`), `container_upload` |
| In the response | `text`, `thinking`, `tool_use`, `server_tool_use` and result blocks of server tools |

### The response object

| Field | Meaning |
|---|---|
| `id` | the message identifier (in the experiment `msg_mock01`) |
| `type` | always `message` |
| `role` | always `assistant` |
| `content` | an array of blocks |
| `model` | the model that handled the request |
| `stop_reason` | the stop reason (table below) |
| `stop_sequence` | the string that fired, or `null` |
| `usage` | `input_tokens`, `output_tokens` (and cache fields) |
| `stop_details` | only with `stop_reason: "refusal"`: `type`, `category`, `explanation`; otherwise `null` |

| `stop_reason` | When |
|---|---|
| `end_turn` | a natural finish |
| `max_tokens` | the limit was exhausted |
| `stop_sequence` | a `stop_sequences` string fired |
| `tool_use` | a tool call was requested |
| `pause_turn` | the turn was paused |
| `refusal` | a refusal for safety reasons |

> ⚠️ Verify: the full list of `usage` fields (cache, iterations, speed) and the set of `category` values in `stop_details` come from the current documentation and were not checked in the experiment.

### Headers

| Header | Direction | Meaning |
|---|---|---|
| `anthropic-version` | request | the API version; SDK 2.13.0 sent `2026-10-21` (in the experiment) |
| `x-api-key` | request | the key for direct HTTP calls (SDK 2.13.0 passed the key in `authorization` in the experiment) |
| `content-type: application/json` | request | the body format |
| `anthropic-beta` | request | enabling beta features (when needed) |
| `request-id` | response | the request identifier (in the experiment `req_mock123`); the same value is in the `request_id` field of an error body |
| `retry-after` | response | the pause before a retry on a 429 (in the experiment read via `e.response.headers`) |

### Error table

| HTTP | `error.type` | Meaning |
|---|---|---|
| 400 | `invalid_request_error` | a malformed request; also for other 4xx and when a spend limit is reached |
| 401 | `authentication_error` | a key problem (or `CORS requests are not allowed…` for a browser request) |
| 402 | `billing_error` | a billing problem |
| 403 | `permission_error` | the key has no permission for the resource |
| 404 | `not_found_error` | the resource was not found |
| 409 | `conflict_error` | a resource state conflict |
| 413 | `request_too_large` | the request size was exceeded (Messages API — 128 MB; Batch API — 256 MB; Files API — 500 MB) |
| 429 | `rate_limit_error` | a rate or spend limit |
| 500 | `api_error` | an internal error |
| 504 | `timeout_error` | a processing timeout (`timeout_error` also occurs with other codes) |
| 529 | `overloaded_error` | the API is temporarily overloaded |

The error body: `{"type": "error", "error": {"type", "message"}, "request_id": "req_…"}`. New `type` values may appear without an API version change: do not write an exhaustive `switch` without a `default` branch.

**Errors in a stream.** After a 200 response an error arrives as an `event: error` event with the data `{"type": "error", "error": {"type": "overloaded_error", "message": "Overloaded"}}`; ordinary HTTP mechanisms do not apply here. For Claude 4.6 and later models the documentation describes recovery from an interrupted stream; check the section on stream errors.

### The SSE streaming protocol

Each event is `event: <type>` plus `data: <JSON>` with the same `type`. The order for an ordinary reply (from the documentation; in the experiment the SDK showed the same types):

| Event | Purpose |
|---|---|
| `message_start` | a `Message` object with an empty `content` and the initial `usage` |
| `content_block_start` | the start of a block by `index` (for example `{"type": "text", "text": ""}`) |
| `ping` | keeping the connection alive; may arrive at any time (the SDK does not show it) |
| `content_block_delta` | an increment of a block |
| `content_block_stop` | the end of a block |
| `message_delta` | the final `stop_reason`, `stop_sequence` and the accumulated `usage` (`output_tokens` — cumulative) |
| `message_stop` | the end of the message |
| `error` | an error inside the stream |

The `delta` types: `text_delta` (field `text`), `input_json_delta` (field `partial_json`: pieces of a tool's JSON arguments, concatenated and parsed after `content_block_stop`), `thinking_delta` (`thinking`), `signature_delta` (`signature`, the reasoning block's signature). New event types may appear: a client must ignore unknown ones.

## Internals

- **Stateless.** The server does not keep the conversation. All context is the request body: `system`, `tools`, `messages` in this order form the prefix on which caching ([module 05](../../overview.en.md) of the roadmap) and the `input_tokens` count are built.
- **A response as a set of blocks.** Blocks come in generation order (reasoning → text → a tool call); `index` in the stream ties increments to blocks. The SDK builds the final message by accumulating events: `get_final_message()` returns the same object as an ordinary request.
- **`usage` in a stream** arrives twice: an initial value in `message_start` (in the experiment `output_tokens: 1`) and the final one in `message_delta` (in the experiment `6`); use the final one.
- **Versioning.** For a given API version Anthropic preserves input and output parameters, but may add optional inputs, values in outputs, new enum values (including stream event types) and change the conditions of some errors. So a client must tolerate unknown fields and values.
- **Limits.** The request size is up to 128 MB for the Messages API, the number of messages up to 100,000; exceeding the size gives a 413 before processing. For non-streaming requests with a large `max_tokens` the SDK checks for a possible excess of the 10-minute timeout.
- **Retries.** The SDK retries connection errors, 408/409/429 and 5xx twice with exponential backoff, respecting `retry-after`; the API guarantees no request-level idempotency: a retry after a timeout may lead to double generation and double billing.

> ⚠️ Verify: the API's behavior on a retry after a timeout and the exact list of retried codes in the SDK come from the documentation and SDK descriptions and were not reproduced against the real API.

## Gotchas

### 1. `content[0]` is not always text

With thinking or tools the first block may be `thinking` or `tool_use`.

### 2. `max_tokens` truncates silently

`stop_reason == "max_tokens"` is the only signal (in the experiment the mock returned `max_tokens`).

### 3. `refusal` arrives with a 200 code

A refusal is not an HTTP error; check `stop_reason`.

### 4. `stop_sequence` is not part of the text

The string that fired is cut off; it is in the `stop_sequence` field (in the experiment `'4'` → text `'1, 2, 3, '`).

### 5. SDK retries hide failures

In the experiment after two 429 responses the code got a successful result; the number of attempts is visible only in server logs and via the retry count (the `x-stainless-retry-count` header).

### 6. `temperature` on the new API version

A request with `temperature`, `top_p` or `top_k` under `anthropic-version: 2026-10-21` gives a 400 on any model.

### 7. `thinking` blocks cannot be edited

When continuing a conversation pass them back unchanged; otherwise a 400 ("blocks … cannot be modified").

### 8. Prefill

Starting a reply with an assistant message gives a 400 on new models (4.6 and later).

### 9. Unknown fields and values

New values of `stop_reason`, block types and events may appear without a version change: handle a default branch.

### 10. An error after a 200 in a stream

A failure in the middle of a stream arrives as an `error` event, not an HTTP code.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| An ordinary request | simpler code | timeouts on big replies |
| Streaming | early output, no timeouts | more complex code and error handling |
| Message Batches | cheaper, no network while waiting | a deferred result (module 14 of the roadmap) |
| `count_tokens` before a request | an exact estimate | an extra call |
| Raw HTTP | no dependencies | versions, retries, SSE parsing by hand |
| An SDK | types, retries, streaming | a dependency; SDK and API versions must be aligned |

## Interview questions

<details><summary>1. Why is the Messages API called stateless and what follows from that?</summary>

The server does not keep the conversation: every request carries the whole history. Consequences: cost grows, compaction is needed, caching is built on the prefix.

→ [Basic theory](basics.en.md#a-conversation-the-api-does-not-remember-the-history)

</details>

<details><summary>2. What do the <code>stop_reason</code> values mean?</summary>

`end_turn` — finished itself; `max_tokens` — cut by the limit; `stop_sequence` — your string fired; `tool_use` — asks for a tool; `pause_turn` — a turn pause; `refusal` — a refusal.

→ [Reference](#the-response-object)

</details>

<details><summary>3. Trick: <code>message.content[0].text</code> failed with an <code>AttributeError</code>. Why?</summary>

The first block was not `text` (for example `thinking`). You must check `block.type`.

→ [Gotcha 1](#1-content0-is-not-always-text)

</details>

<details><summary>4. How do you get a full reply without handling stream events?</summary>

`with client.messages.stream(...) as s: s.get_final_message()` — it returns the same `Message` as an ordinary request.

→ [Advanced theory](advanced.en.md#streaming)

</details>

<details><summary>5. How is the SSE stream structured?</summary>

`message_start` → blocks (`content_block_start` → deltas → `content_block_stop`) → `message_delta` → `message_stop`; `ping` and `error` are possible at any time.

→ [The SSE protocol](#the-sse-streaming-protocol)

</details>

<details><summary>6. Trick: a request with <code>temperature</code> suddenly returned a 400. What happened?</summary>

Under API version `2026-10-21` `temperature`, `top_p`, `top_k` are removed.

→ [Gotcha 6](#6-temperature-on-the-new-api-version)

</details>

<details><summary>7. What does the SDK do on a 429 and 5xx?</summary>

It retries the request twice with exponential backoff, respecting `retry-after`; the number of retries is `max_retries`.

→ [Basic theory](basics.en.md#errors)

</details>

<details><summary>8. How do you tie an error to a support request?</summary>

Attach the `request_id` (the `request-id` header, the error body field, `e.request_id`, `message._request_id`).

→ [Advanced theory](advanced.en.md#request-id-and-response-headers)

</details>

<details><summary>9. Why must a client tolerate unknown values?</summary>

Anthropic may add fields, enum values and event types within one API version.

→ [Internals](#internals)

</details>

<details><summary>10. Trick: a stream returned 200 but the reply broke off with an error. How is that possible?</summary>

Errors inside a stream arrive as an `error` event after a 200 response.

→ [Gotcha 10](#10-an-error-after-a-200-in-a-stream)

</details>

<details><summary>11. When do you need <code>count_tokens</code>?</summary>

To estimate cost and check that a request fits the context window, without generating a reply.

→ [Advanced theory](advanced.en.md#counting-tokens)

</details>

<details><summary>12. How does <code>system</code> differ from a first <code>user</code> message?</summary>

`system` is top-level instructions in a separate parameter (a string or blocks, supports `cache_control`); a `user` message is part of the conversation.

→ [Basic theory](basics.en.md#the-system-prompt)

</details>

## Cheat sheet

```text
POST /v1/messages  headers: x-api-key, anthropic-version: 2026-10-21, content-type: application/json
body: { model, max_tokens, messages: [{role, content}], system?, stop_sequences?, stream?, thinking?, output_config? }
response: { id, role, content: [blocks], stop_reason, stop_sequence, usage: {input_tokens, output_tokens} }
stop_reason: end_turn | max_tokens | stop_sequence | tool_use | pause_turn | refusal
errors: 400 invalid_request | 401 authentication | 403 permission | 404 not_found | 413 request_too_large | 429 rate_limit | 500 api | 529 overloaded
SDK: client.messages.create / .stream(...).get_final_message() / .count_tokens / .with_raw_response
```

- Check `stop_reason` and the block type.
- Big replies — streaming.
- `temperature`, `top_p`, `top_k` were removed in version `2026-10-21`.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Model selection: [01 · The Claude family and model selection](../01-models/expert.en.md)
- Streams and asynchrony in JavaScript: [12 · Promises and async/await](../../../javascript/modules/12-promises-and-async-await/expert.en.md)

## Sources

- [Messages API reference](https://platform.claude.com/docs/en/api/messages)
- [Streaming messages](https://platform.claude.com/docs/en/build-with-claude/streaming)
- [API errors](https://platform.claude.com/docs/en/api/errors)
- [API versions](https://platform.claude.com/docs/en/api/versioning)
- [Rate limits](https://platform.claude.com/docs/en/api/rate-limits)
