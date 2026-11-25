---
title: "Batch, evals and reliability: advanced"
lang: en
track: claude
kind: module
doc: advanced
slug: 14-evals
part: 3
tags: [claude, evals, batch-api, retries, reliability]
versions: "Claude 5.5 family; anthropic 2.14.0; Message Batches API"
status: draft
updated: 2026-11-25
---

# Batch, evals and reliability: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## SDK retries

The defaults are `max_retries=2` and a 600 s read timeout. Observed:

| Response | Attempts | Outcome |
|---|---|---|
| 429, `retry-after: 1` (twice) | 3, about 2 s | success |
| 529 | 3 | `OverloadedError` |
| 500, `x-should-retry: false` | 1 | `InternalServerError` |
| 400 | 1 | `BadRequestError` |

Network failures, 408, 409, 429 and 5xx are retried; an `x-should-retry` header takes precedence. Without `retry-after` the delay backs off from 0.5 to 8 s. Timeouts are retried too, so wall-clock can reach `timeout × (max_retries + 1)`.

## Handling outcomes

```python
# reliability.py
import anthropic


def ask(client, prompt, model="claude-opus-5-5"):
    try:
        response = client.messages.create(model=model, max_tokens=1024,
                                          messages=[{"role": "user", "content": prompt}])
    except anthropic.BadRequestError as e:
        return ("bug in request", e.request_id)
    except anthropic.RateLimitError as e:
        return ("rate limited", e.request_id)
    except anthropic.APIStatusError as e:
        return (f"api error {e.status_code}", e.request_id)
    except anthropic.APIConnectionError:
        return ("no connection", None)
    if response.stop_reason == "refusal":
        return ("refused", response.stop_details.category if response.stop_details else None)
    if response.stop_reason == "max_tokens":
        return ("truncated", response._request_id)
    return ("ok", "".join(b.text for b in response.content if b.type == "text"))
```

A refusal is a **successful** 200 response with `stop_reason: "refusal"`: `try/except` will not catch it and `content` may be empty. A server-side `fallbacks` option (beta) retries a declined request on another model.

## Message Batches

```python
# batch.py
import time

from anthropic.types.message_create_params import MessageCreateParamsNonStreaming
from anthropic.types.messages.batch_create_params import Request


def run_batch(client, prompts, model="claude-opus-5-5", poll_seconds=60):
    """prompts: {custom_id: text}. Returns ({custom_id: answer}, {custom_id: failure type})."""
    batch = client.messages.batches.create(requests=[
        Request(custom_id=key, params=MessageCreateParamsNonStreaming(
            model=model, max_tokens=1024, messages=[{"role": "user", "content": text}]))
        for key, text in prompts.items()])
    while batch.processing_status != "ended":
        time.sleep(poll_seconds)
        batch = client.messages.batches.retrieve(batch.id)

    answers, failed = {}, {}
    for item in client.messages.batches.results(batch.id):   # any order
        if item.result.type == "succeeded":
            answers[item.custom_id] = "".join(b.text for b in item.result.message.content if b.type == "text")
        else:
            failed[item.custom_id] = item.result.type
    return answers, failed
```

| Fact | Value |
|---|---|
| price | 50% of standard |
| size | up to 100,000 requests or 256 MB |
| time | usually under an hour; expires after 24 hours |
| results | 29 days; JSONL in any order |
| `custom_id` | `^[a-zA-Z0-9_-]{1,64}$` |
| result types | `succeeded`, `errored`, `canceled`, `expired` |

`errored`, `canceled` and `expired` are not billed. `stream` and `max_tokens: 0` are not supported. Batches fit eval runs and background processing, not a user waiting for an answer.

## Grading with a model

```python
RUBRIC = """Grade the reply against the criteria. Answer with one word: pass or fail.
<criteria>{criteria}</criteria>
<reply>{reply}</reply>"""


def judge(client, reply, criteria, model="claude-sonnet-5-5"):
    response = client.messages.create(model=model, max_tokens=2000, messages=[
        {"role": "user", "content": RUBRIC.format(criteria=criteria, reply=reply)}])
    verdict = "".join(b.text for b in response.content if b.type == "text").strip().lower()
    if verdict not in ("pass", "fail"):
        raise ValueError(f"judge did not follow the format: {verdict!r}")
    return verdict == "pass"
```

A strict judge format that raises on deviation stops "Pass, but..." being counted silently. Validate the judge on human-labelled examples.

## Related materials

- [Fundamentals](basics.en.md) · [Expert](expert.en.md)
- [05 · Prompt caching](../05-prompt-caching/advanced.en.md) · [13 · Long context](../13-long-context/basics.en.md)

## Sources

- [Errors](https://platform.claude.com/docs/en/api/errors)
- [Batch processing](https://platform.claude.com/docs/en/build-with-claude/batch-processing)
- [Refusals and fallback](https://platform.claude.com/docs/en/build-with-claude/refusals-and-fallback)
