---
title: "Deployment and production: advanced"
lang: en
track: claude
kind: module
doc: advanced
slug: 17-production
part: 3
tags: [claude, deployment, production, retries, bedrock, vertex-ai, foundry]
versions: "Claude 5.5 family; anthropic 2.14.0; Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# Deployment and production: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## What cloud platforms lack

| | Claude API | Platform on AWS | Bedrock | Vertex AI | Foundry |
|---|---|---|---|---|---|
| Message Batches | ✓ | ✓ | — | — | — |
| Files API | ✓ | ✓ | — | — | Anthropic-hosted only |
| Models API | ✓ | ✓ | — | — | — |
| web search | ✓ | ✓ | — | basic | ✓ |
| code execution | ✓ | ✓ | — | — | Anthropic-hosted only |
| Managed Agents | beta | beta | — | — | — |
| `inference_geo` | ✓ | ✓ | — | — | — |
| server-side `fallbacks` | beta | beta | — | — | — |

**A method in the SDK guarantees nothing:** `AnthropicBedrockMantle` and `AnthropicVertex` have `client.messages.batches` although the platform does not support batches. They have no `client.models` or `client.files`.

## Errors and retries

| Situation | SDK retries | What to do |
|---|---|---|
| 429 rate limit | yes | honour `retry-after`, slow down |
| 429 `ramp_limit_exceeded` | yes | ramp traffic gradually |
| 429 `enforced_spend_limit_reached` | **yes, though pointless** | alert, raise the limit |
| 500, 529 | yes | then queue or another route |
| error inside a stream | **no** | retry yourself, drop the partial reply |
| 400, 401, 403, 404 | no | fix the request or access |

The `x-should-retry` header overrides the status. An exhausted tier cap is `EnforcedSpendLimitReachedError`, a subclass of `RateLimitError`: with default settings the check saw three attempts. A spend limit you set yourself returns a 400.

## Failing mid-stream

The 200 has arrived, some text too, then an `error` event. The SDK raises `APIStatusError` with `status_code == 200` and does not retry.

```python
# complete.py
import time

import anthropic


def complete(client, attempts=3, sleep=time.sleep, **params):
    """Stream a reply; retry only errors that arrive inside the stream (the SDK does not)."""
    for attempt in range(1, attempts + 1):
        try:
            with client.messages.stream(**params) as stream:
                return stream.get_final_message()       # partial text of a failed try is dropped
        except anthropic.APIStatusError as error:
            mid_stream = error.status_code == 200 and error.type in ("overloaded_error", "api_error")
            if not mid_stream or attempt == attempts:
                raise                                   # other errors were already retried by the SDK
            sleep(2 ** attempt)
```

If the text is already on screen, the UI must be able to discard it before the retry. Do not run tools until the whole message has arrived.

## A fallback route

```python
# failover.py
import anthropic


def worth_another_route(error):
    """True when a different platform or region may succeed where this one failed."""
    if isinstance(error, anthropic.APIConnectionError):          # includes timeouts
        return True
    if isinstance(error, anthropic.APIStatusError):
        return error.status_code == 429 or error.status_code >= 500
    return False                                                 # 400, 401, 403, 404 need a fix


def create_with_failover(routes, **params):
    """routes: list of (client, model_id), best first."""
    last = None
    for client, model in routes:
        try:
            return client.messages.create(model=model, **params)
        except anthropic.APIError as error:
            if not worth_another_route(error):
                raise
            last = error
    raise last
```

A fallback route is another platform or region with a model your eval **has passed**. Mind that prompt caches are not shared, features differ, and failing over on a spend-cap 429 moves spending to another bill.

## Model refusals

A refusal is not an error, so the route loop above never sees it. The SDK middleware retries a refused request on another model:

```python
from anthropic import Anthropic, BetaFallbackState, BetaRefusalFallbackMiddleware

client = Anthropic(middleware=[BetaRefusalFallbackMiddleware([{"model": "claude-sonnet-5-5"}])])
state = BetaFallbackState()                     # one per conversation
with state:
    response = client.beta.messages.create(model="claude-opus-5-5", max_tokens=1024,
                                           messages=[{"role": "user", "content": "hi"}])
print(response.model, [block.type for block in response.content])
```

Against a stand-in refusal the first call sent two requests and returned blocks `['fallback', 'text']`; the next call with the same `state` went straight to the fallback model. Some refusals are billed, so a fallback is not free.

## Limits

- Claude API limits: RPM, ITPM, OTPM per model class, a token bucket; a short burst can still get a 429.
- Cache reads do not count toward ITPM by default; `max_tokens` does not count toward OTPM.
- A ramp limit caps how fast usage grows; the Batch API is exempt.
- Bedrock and Vertex quotas belong to the cloud; Foundry omits the `anthropic-ratelimit-*` headers.

## Regions and price

| Choice | Price effect |
|---|---|
| Bedrock: global → regional | +10% |
| Vertex AI: global → multi-region or regional | +10% |
| `inference_geo: "us"` (Claude API) | ×1.1 |
| Foundry: US Data Zone Standard | ×1.1 |
| Batch API | −50% |

On Vertex AI regional endpoints serve only Sonnet 4.6 and earlier; for newer models use global or multi-region. AWS and Google set Bedrock and Vertex prices. To spend less use [caching](../05-prompt-caching/overview.en.md), the right [model](../01-models/overview.en.md) and [effort](../01-models/overview.en.md), and batches.

## Model changes and rollout

1. Keep model ID and platform in configuration.
2. Run the eval before a change; compare quality, cost and latency.
3. Roll out to a share of traffic with a way back.
4. Watch retirement dates: Anthropic gives at least 60 days' notice; Bedrock and Vertex have their own schedules.

## Related materials

- [Fundamentals](basics.en.md) · [Expert](expert.en.md)
- [05 · Prompt caching](../05-prompt-caching/advanced.en.md) · [15 · Observability](../15-observability/advanced.en.md)

## Sources

- [Errors](https://platform.claude.com/docs/en/api/errors)
- [Rate limits](https://platform.claude.com/docs/en/api/rate-limits)
- [Refusals and fallback](https://platform.claude.com/docs/en/build-with-claude/refusals-and-fallback)
- [Pricing](https://platform.claude.com/docs/en/about-claude/pricing)
- [Model deprecations](https://platform.claude.com/docs/en/about-claude/model-deprecations)
