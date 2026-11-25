---
title: "Messages API"
lang: en
track: claude
kind: module
doc: overview
slug: 02-messages-api
part: 1
levels: [1, 2, 3, 4]
tags: [claude, messages-api, streaming, stop-reason, anthropic-sdk, api-errors]
versions: "Claude 5.5 family; Messages API, anthropic-version 2026-10-21; Python SDK anthropic 2.13.0"
status: draft
updated: 2026-11-24
---

# Messages API

🌐 [Русский](overview.ru.md) · **English**

## In short

The Messages API is Claude's main interface: one endpoint, `POST /v1/messages`, to which you send a model, the output limit `max_tokens` and a list of messages, and from which you get an assistant message with content blocks, a stop reason (`stop_reason`) and token accounting (`usage`). The API is **stateless**: the client sends the conversation history every time. The other capabilities grow out of the same endpoint: streaming, tools, thinking, structured output.

## Key ideas

- **The request:** `model`, `max_tokens` and `messages` are required; a message role is `user` or `assistant`; `system` is a separate parameter.
- **The response is not a string but a `Message` object:** `content` is a list of blocks (`text`, `tool_use`, `thinking`, …), so you check `block.type` before reading `.text`.
- **The API does not remember the conversation:** every request carries the whole history.
- **`stop_reason` explains why generation stopped:** `end_turn`, `max_tokens`, `stop_sequence`, `tool_use`, `pause_turn`, `refusal`; check it before using `content`.
- **`max_tokens` is a hard output ceiling:** when it is reached the reply is cut off mid-sentence.
- **Streaming** (`stream: true`, SSE events) is a must for large outputs and long requests; the SDK can assemble the final message.
- **Errors are typed:** an HTTP code and an `error.type` field; the SDK automatically retries transient failures twice.
- **The API version** is set by the `anthropic-version` header; the SDK sets it itself.
- **On new models some parameters changed:** `temperature`, `top_p` and `top_k` give a 400 under API version `2026-10-21`, and thinking is controlled through `thinking` and `output_config.effort`.

## Minimal example

```python
import anthropic

client = anthropic.Anthropic()  # the key is taken from ANTHROPIC_API_KEY

message = client.messages.create(
    model="claude-opus-5-5",
    max_tokens=1024,
    messages=[{"role": "user", "content": "What is the capital of France?"}],
)
for block in message.content:
    if block.type == "text":
        print(block.text)
print(message.stop_reason, message.usage.output_tokens)
```

```text
Paris.
end_turn 6
```

The output came from a local mock server (see the "How the code was checked" section of the [basic theory](basics.en.md)), so the reply text is a stand-in; a real model will answer differently.

## When to use and when not

| Task | Choice |
|---|---|
| Q&A, summarization, extraction, classification | one Messages API request |
| A long answer, a chat that shows output as it is generated | streaming |
| Thousands of independent requests with no urgency | Message Batches (module 14 of the roadmap) |
| The model must call your functions | tool use ([module 07](../../overview.en.md) of the roadmap) |
| The answer strictly in JSON | structured outputs (module 04 of the roadmap) |
| An autonomous agent with a sandbox | Managed Agents or Claude Code (modules 09 and 12 of the roadmap) |

- ❌ Do not keep the conversation "on the model's side": without history in `messages` it remembers nothing.
- ❌ Do not read `message.content[0].text` without checking the block type and `stop_reason`.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | request and response, roles, `system`, a conversation, `max_tokens`, `stop_reason`, errors |
| [2. Advanced theory](advanced.en.md) | streaming, `stop_sequences`, thinking and effort, retries and timeouts, `count_tokens`, request ID |
| [3. Expert theory](expert.en.md) | a reference of parameters and fields, the SSE protocol, an error table, pitfalls, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn claude 02-messages-api level 5` |
| 6. Tests | not created yet — `/learn claude 02-messages-api level 6` |

## Related materials

- Prerequisites: [01 · The Claude family and model selection](../01-models/overview.en.md)
- Next: [03 · Prompt engineering](../03-prompt-engineering/overview.en.md)
- Analogues in other tracks: [LLM](../../../llm/overview.en.md), [Python](../../../python/overview.en.md) (SDK examples), [JavaScript](../../../javascript/modules/12-promises-and-async-await/overview.en.md) (asynchronous clients and `fetch`)
- [17 · Deployment and production](../17-production/overview.en.md)
