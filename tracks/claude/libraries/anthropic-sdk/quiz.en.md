---
title: "Anthropic SDK: quiz"
lang: en
track: claude
kind: library
doc: quiz
slug: anthropic-sdk
part: 2
tags: [claude, sdk, anthropic-sdk, python, typescript]
versions: "anthropic 2.14.0; @anthropic-ai/sdk 1.14.0"
status: draft
updated: 2026-11-25
---

# Anthropic SDK: quiz

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What is the Python client package?

- A) `claude-sdk`
- B) `anthropic-api`
- C) `anthropic`
- D) `claude`

<details><summary>Answer</summary>

**C.** The API client installs as `anthropic`. `claude-agent-sdk` is a different package for agents; the other names are not the client.
→ [Fundamentals](basics.en.md)

</details>

## 2. 🟢 Where does the client get the key by default?

- A) `ANTHROPIC_API_KEY`
- B) `.env` automatically
- C) `~/.ssh`
- D) a terminal prompt

<details><summary>Answer</summary>

**A.** The SDK reads the environment variable. It does not load `.env` by itself and never prompts.
→ [Fundamentals](basics.en.md)

</details>

## 3. 🟢 How do you get the request ID from a reply?

- A) `message.id`
- B) `message.usage`
- C) `client.request_id`
- D) `message._request_id`

<details><summary>Answer</summary>

**D.** `_request_id` comes from the `request-id` header. `message.id` identifies the message, `usage` counts tokens, and the client has no such field.
→ [Fundamentals](basics.en.md)

</details>

## 4. 🟢 What does `messages.stream(...)` add over `stream=True`?

- A) it disables retries
- B) it accumulates the final message
- C) it is cheaper
- D) it needs no key

<details><summary>Answer</summary>

**B.** The helper assembles events into a message and offers `text_stream`. It changes neither price, auth nor retries.
→ [Fundamentals](basics.en.md)

</details>

## 5. 🟡 How do you change the timeout for one call only?

- A) the `max_tokens` parameter
- B) `client.with_options(timeout=...)`
- C) mutate `client.timeout` and restore it
- D) `ANTHROPIC_LOG`

<details><summary>Answer</summary>

**B.** `with_options` returns a copy. Mutating a shared client affects concurrent requests, and logging or `max_tokens` set no timeout.
→ [Advanced theory](advanced.en.md)

</details>

## 6. 🟡 What does `for m in client.models.list(limit=2)` do?

- A) returns only two models
- B) raises on the third
- C) always makes one request
- D) walks every page

<details><summary>Answer</summary>

**D.** `limit` is the page size and iteration fetches the rest. Only `page.data` stops at the first page.
→ [Advanced theory](advanced.en.md)

</details>

## 7. 🟡 What happens to an exception inside a `@beta_tool` in the runner?

- [ ] A) it goes to the model as an `is_error` `tool_result`
- [ ] B) the model sees its text
- [ ] C) your code crashes at once
- [ ] D) it is silently ignored

<details><summary>Answer</summary>

**A, B.** The runner turns it into an error result containing the `repr`. The loop continues, so it neither crashes nor vanishes.
→ [Advanced theory](advanced.en.md)

</details>

## 8. 🟡 What does `ANTHROPIC_LOG=debug` write?

- A) nothing without a handler
- B) only the key
- C) message texts among other things
- D) only the status code

<details><summary>Answer</summary>

**C.** Debug prints the full request options. Info gives a line per request; the key was not printed in the check.
→ [Advanced theory](advanced.en.md)

</details>

## 9. 🔴 What does the runner do on the Claude API without `thread=False`?

- A) resends the whole history each time
- B) saves it to the client's disk
- C) disables tools
- D) keeps the conversation server-side in a thread

<details><summary>Answer</summary>

**D.** Later requests carry only new messages and a reference to the previous one. The Bedrock and Vertex clients sent full history.
→ [Expert theory](expert.en.md)

</details>

## 10. 🔴 What if `extra_body` contains `max_tokens`?

- A) the values add up
- B) the `extra_body` value overrides the argument
- C) the argument wins
- D) an SDK error

<details><summary>Answer</summary>

**B.** `extra_*` are applied on top of normal parameters, so never pass untrusted data there.
→ [Expert theory](expert.en.md)

</details>

## 11. 🔴 Why does `respx` not intercept SDK 2.14.0 requests?

- A) requests go through `curl`
- B) `respx` handles only GET
- C) the SDK uses `httpx2`
- D) it uses `requests`

<details><summary>Answer</summary>

**C.** `respx` patches `httpx`, a different package. Use `httpx2.alias_httpx()` or a `MockTransport`.
→ [Expert theory](expert.en.md)

</details>

## 12. 🔴 Open question: a ZDR service uses the tool runner. What do you check?

<details><summary>Answer</summary>

That `thread=False` (TypeScript `thread: false`) is set everywhere; that no other non-ZDR beta features are used; that debug logging is off; that your own logs keep no content; a test asserting the request has no `thread` field.
→ [Advanced theory](advanced.en.md)

</details>
