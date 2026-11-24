---
title: "Quiz: Messages API"
lang: en
track: claude
kind: module
doc: quiz
slug: 02-messages-api
part: 1
tags: [claude, messages-api, stop-reason, streaming, api-errors]
status: draft
updated: 2026-11-24
---

# Quiz: Messages API

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. The label shows the question level: 🟢 [basic](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 Which fields are required in a `POST /v1/messages` request?

- A) `model` and `messages`
- B) `model`, `max_tokens`, `messages`
- C) `model`, `system`, `messages`
- D) only `messages`

<details><summary>Answer</summary>

**B.** `model`, `max_tokens` and `messages` are required; `system` is optional.
→ [Basic theory](basics.en.md#request-and-response)

</details>

## 2. 🟢 How do you continue a conversation with the model?

- A) Send only the new message: the server remembers the history
- B) Send the whole message history and the new message
- C) Pass a `conversation_id`
- D) Call `client.messages.continue()`

<details><summary>Answer</summary>

**B.** The API is stateless: the client sends the whole history with every request.
→ [Basic theory](basics.en.md#a-conversation-the-api-does-not-remember-the-history)

</details>

## 3. 🟢 What does `stop_reason == "max_tokens"` mean?

- A) The model finished the reply
- B) The reply was cut by the `max_tokens` limit and is incomplete
- C) The account ran out of tokens
- D) A stop sequence fired

<details><summary>Answer</summary>

**B.** You need to increase the limit or use streaming.
→ [Basic theory](basics.en.md#max_tokens-and-stop_reason)

</details>

## 4. 🟢 How do you safely read the reply text?

- A) `message.content[0].text`
- B) Iterate `message.content` and take `.text` from blocks with `type == "text"`
- C) `str(message)`
- D) `message.text`

<details><summary>Answer</summary>

**B.** `content` is a list of blocks of different types; the first may be `thinking` or `tool_use`.
→ [Basic theory](basics.en.md#request-and-response)

</details>

## 5. 🟡 What does `stream.get_final_message()` return?

- A) Only the text
- B) The final `Message`, as in an ordinary request
- C) The last stream event
- D) A list of events

<details><summary>Answer</summary>

**B.** The SDK assembles the message from the events; handy for big replies without handling events.
→ [Advanced theory](advanced.en.md#streaming)

</details>

## 6. 🟡 What happens with `stop_sequences=["4"]` and a reply "1, 2, 3, 4, 5"?

- A) The reply "1, 2, 3, 4", `stop_reason == "end_turn"`
- B) The reply "1, 2, 3, " without the stop string, `stop_reason == "stop_sequence"`, `stop_sequence == "4"`
- C) A 400 error
- D) The reply "1, 2, 3, 4, 5"

<details><summary>Answer</summary>

**B.** The stop string is cut off and returned in a separate field.
→ [Advanced theory](advanced.en.md#stop_sequences)

</details>

## 7. 🟡 How many times does the SDK retry a request on a 429 or 5xx by default?

- A) Never
- B) Once
- C) Twice
- D) Endlessly

<details><summary>Answer</summary>

**C.** By default `max_retries = 2`, with exponential backoff, respecting `retry-after`.
→ [Advanced theory](advanced.en.md#retries-timeouts-and-client-parameters)

</details>

## 8. 🟡 What happens if you send `temperature` under API version `2026-10-21`?

- A) The parameter is applied
- B) The parameter is ignored
- C) The request returns a 400
- D) The model switches to an old one

<details><summary>Answer</summary>

**C.** `temperature`, `top_p`, `top_k` are removed: a request with them gives a 400 on any model.
→ [Advanced theory](advanced.en.md#parameters-removed-in-api-version-2026-10-21)

</details>

## 9. 🟡 Choose all true statements

- [ ] A) `count_tokens` creates a model reply
- [ ] B) `RateLimitError` inherits `APIStatusError`
- [ ] C) `with_raw_response` gives access to the response headers
- [ ] D) `message._request_id` holds the request identifier

<details><summary>Answer</summary>

**B, C, D.** A is wrong: `count_tokens` only estimates the size of a request.
→ [Advanced theory](advanced.en.md#request-id-and-response-headers)

</details>

## 10. 🔴 Find the mistake

> "If a stream started with HTTP 200, there can be no more errors."

<details><summary>Answer</summary>

An error can arrive as an `error` event inside the stream after the 200 response (for example `overloaded_error`).
→ [Gotcha 10](expert.en.md#10-an-error-after-a-200-in-a-stream)

</details>

## 11. 🔴 In what order do the events of an ordinary stream arrive?

- A) `message_start`, `content_block_start`, `content_block_delta`…, `content_block_stop`, `message_delta`, `message_stop`
- B) `content_block_start`, `message_start`, `message_stop`
- C) `message_delta`, `message_start`, `message_stop`
- D) `message_stop`, then the rest

<details><summary>Answer</summary>

**A.** `ping` and `error` are possible at any time.
→ [The SSE protocol](expert.en.md#the-sse-streaming-protocol)

</details>

## 12. 🔴 Why must a client not crash on an unknown `stop_reason` value or event type?

- A) It is impossible
- B) Within one API version Anthropic may add new enum values and event types
- C) It is an SDK bug
- D) Only with a beta header

<details><summary>Answer</summary>

**B.** The versioning rules allow extending output values.
→ [Internals](expert.en.md#internals)

</details>

## 13. 🔴 Open question

Design a wrapper over the Messages API for a chatbot: what to store, what to check in the reply and how to handle failures.

<details><summary>Key points of a good answer</summary>

- Store the `messages` history (appending the whole `content` of the reply, including `thinking` blocks) and `system` separately; watch the context growth (`count_tokens`, compaction).
- Read the reply block by block; check `stop_reason` (`max_tokens`, `refusal`, `pause_turn`).
- For big replies — streaming and `get_final_message()`.
- Catch typed exceptions, log `request_id`, rely on the SDK's retries, set a timeout; do not use `temperature`.

</details>

→ [Expert theory](expert.en.md)
