---
title: "Deployment and production"
lang: en
track: claude
kind: module
doc: overview
slug: 17-production
part: 3
tags: [claude, deployment, production, retries, bedrock, vertex-ai, foundry]
versions: "Claude 5.5 family; anthropic 2.14.0; Claude Code 2.1.332"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-25
---

# Deployment and production

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

Taking a Claude application to production: choosing a **platform** (Claude API, AWS, Google Cloud, Microsoft Foundry), setting up **retries and fallback routes**, regions and cost.

## Key ideas

- Platforms differ in client, model ID and feature set.
- The SDK does not retry everything: mid-stream errors are yours.
- A refusal is not an exception and needs its own path.
- Pinning a region usually costs more.
- Keep model, platform and timeouts in configuration and check them with an eval.

## Minimal example

```python
from platforms import make_client

client, model = make_client("bedrock")
reply = client.messages.create(model=model, max_tokens=1024,
                               messages=[{"role": "user", "content": "hi"}])
```

## When to use it and when not

When a prototype becomes a service with users, a budget and data requirements. A one-off script is fine with the default client.

## Material levels

| Level | File | Add with |
|---|---|---|
| 1. Fundamentals | [basics.en.md](basics.en.md) | |
| 2. Advanced theory | [advanced.en.md](advanced.en.md) | |
| 3. Expert theory | [expert.en.md](expert.en.md) | |
| 4. Self-check | [quiz.en.md](quiz.en.md), [exercises.en.md](exercises.en.md) | |
| 5. Runnable examples | — | `/learn claude 17-production level 5` |
| 6. Tests | — | `/learn claude 17-production level 6` |

## Related materials

- [Claude track](../../overview.en.md)
- [14 · Batch, evals and reliability](../14-evals/overview.en.md)
- [15 · Observability](../15-observability/overview.en.md)
- [16 · Security and compliance](../16-security/overview.en.md)
- [Anthropic SDK](../../libraries/anthropic-sdk/overview.en.md)
