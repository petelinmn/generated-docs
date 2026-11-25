---
title: "Anthropic SDK: expert"
lang: en
track: claude
kind: library
doc: expert
slug: anthropic-sdk
part: 2
tags: [claude, sdk, anthropic-sdk, python, typescript]
versions: "anthropic 2.14.0; @anthropic-ai/sdk 1.14.0"
status: draft
updated: 2026-11-25
---

# Anthropic SDK: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## The client surface

| Resource | What is in it |
|---|---|
| `client.messages` | `create`, `stream`, `parse`, `count_tokens`, `batches` |
| `client.models` | list and retrieve models |
| `client.files`, `client.skills` | files and skills (out of beta) |
| `client.beta` | beta features with `betas=[...]`, `tool_runner` |
| `client.get` / `client.post` | endpoints without a typed method |

## What goes into a request

The SDK adds `anthropic-version` (`2026-10-21` in 2.x), a `User-Agent` such as `Anthropic/Python 2.14.0` and `x-stainless-*` headers. The API version changes with an SDK major version. `raw.retries_taken` reports the retries made.

## Details

- `response.model_fields_set` tells a missing field from `null`; unknown fields appear in `model_extra`.
- SemVer mostly: type-only changes may ship in a minor release. Pin the version.
- Set proxies and transports with `DefaultHttpxClient`; a client from the `httpx` package raises `TypeError`.
- The SDK emits OpenTelemetry spans when the application registers a tracer provider.
- In a browser the TypeScript SDK refuses to start by default; `dangerouslyAllowBrowser` exposes your key to users.
- Platform clients and their differences are in [17 · Deployment](../../modules/17-production/expert.en.md).

## Gotchas

1. `page.data` is only the first page; iterate the page itself for everything.
2. The tool runner leaves the conversation on the server (a thread) by default.
3. Your tool's exception text goes to the model: keep secrets out of exceptions.
4. `ANTHROPIC_LOG=debug` logs message content.
5. `extra_body` overrides normal parameters: in the check its `max_tokens` replaced the argument.
6. `ANTHROPIC_*` environment variables silently change the address and credentials.

> ⚠️ Verify: the C#, Go, Java, PHP and Ruby SDKs and their versions come from the documentation; OpenTelemetry spans and proxies were not run.

## Trade-offs

| Option | When |
|---|---|
| Client SDK | you own the loop |
| tool runner | a simple loop over functions |
| [Agent SDK](../agent-sdk/overview.en.md) | you want an agent with built-in tools |
| raw HTTP | a language without an SDK |

## Interview questions

1. Why the SDK over `requests`?
<details><summary>Answer</summary>

Types, header-aware retries, accumulating streams, pagination, error classes, platform clients.
</details>

2. `stream()` vs `create(stream=True)`?
<details><summary>Answer</summary>

The first is a context manager with accumulation and helpers, the second raw events.
</details>

3. What does `with_options` do?
<details><summary>Answer</summary>

Returns a copy of the client with other settings, leaving the original alone.
</details>

4. How do you get response headers?
<details><summary>Answer</summary>

`with_raw_response` or `with_streaming_response`; in TypeScript `.withResponse()`.
</details>

5. How do you send a parameter the types lack?
<details><summary>Answer</summary>

`extra_body`, `extra_headers`, `extra_query`; they override same-named parameters, so never feed them untrusted data.
</details>

6. Why is a large `max_tokens` without streaming risky?
<details><summary>Answer</summary>

A long idle connection drops; the SDK raises `ValueError` upfront when it expects around 10 minutes.
</details>

7. What is the thread in the tool runner?
<details><summary>Answer</summary>

Server-held conversation state so history is not resent; beta, not ZDR-eligible, off with `thread=False`.
</details>

8. How do you test code using the SDK?
<details><summary>Answer</summary>

Swap the transport with `httpx2.MockTransport` or inject a fake client.
</details>

9. When do you need `AsyncAnthropic`?
<details><summary>Answer</summary>

In async applications and for concurrent requests; bound the concurrency.
</details>

10. How many clients should you create?
<details><summary>Answer</summary>

Usually one per application: it reuses connections.
</details>

## Related materials

- [Advanced theory](advanced.en.md) · [Quiz](quiz.en.md)
- [Agent SDK](../agent-sdk/expert.en.md) · [MCP SDK](../mcp-sdk/expert.en.md)

## Sources

- [Python SDK](https://platform.claude.com/docs/en/cli-sdks-libraries/sdks/python)
- [Message threads](https://platform.claude.com/docs/en/build-with-claude/message-threads)
- [anthropic-sdk-python](https://github.com/anthropics/anthropic-sdk-python)
