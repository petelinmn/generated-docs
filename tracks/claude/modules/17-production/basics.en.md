---
title: "Deployment and production: fundamentals"
lang: en
track: claude
kind: module
doc: basics
slug: 17-production
part: 3
tags: [claude, deployment, production, retries, bedrock, vertex-ai, foundry]
versions: "Claude 5.5 family; anthropic 2.14.0; Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# Deployment and production: fundamentals

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [02 · Messages API](../02-messages-api/overview.en.md), [14 · Batch, evals and reliability](../14-evals/overview.en.md)

## How this was checked

The `anthropic` 2.14.0 clients for the platforms in the table below (except `AnthropicGoogleCloud`) ran against a local server with fake keys: URLs, paths, request bodies, retries and errors were checked. Claude Code 2.1.332 ran in Foundry, Bedrock (mantle) and Claude Platform on AWS modes against a local stand-in. **No request reached a real cloud or API**: model availability, prices, quotas and regions come from the documentation as of 25 November 2026.

## Where Claude can run

| Platform | Operated by | Python client | Model ID |
|---|---|---|---|
| Claude API | Anthropic | `Anthropic` | `claude-opus-5-5` |
| Claude Platform on AWS | Anthropic, billed through AWS | `AnthropicAWS` | `claude-opus-5-5` |
| Claude in Amazon Bedrock | AWS | `AnthropicBedrockMantle` | `anthropic.claude-opus-5-5` |
| Google Cloud (Google managed, Vertex AI) | Google | `AnthropicVertex` | `claude-opus-5-5` |
| Google Cloud (Anthropic managed) | Anthropic | `AnthropicGoogleCloud` | `claude-opus-5-5` |
| Microsoft Foundry | Anthropic, on Azure or its own infrastructure | `AnthropicFoundry` | your deployment name |

Contracts and infrastructure usually decide: where billing and IAM already exist, where data must be processed, which features you need.

## One code path, several platforms

```python
# platforms.py
import os

import anthropic

MODEL = "claude-opus-5-5"


def make_client(platform=None, **options):
    """Return (client, model_id). Credentials and regions come from the environment."""
    platform = platform or os.environ.get("CLAUDE_PLATFORM", "anthropic")
    if platform == "anthropic":
        return anthropic.Anthropic(**options), MODEL
    if platform == "aws":                       # Claude Platform on AWS
        return anthropic.AnthropicAWS(**options), MODEL
    if platform == "bedrock":                   # Claude in Amazon Bedrock
        return anthropic.AnthropicBedrockMantle(**options), f"anthropic.{MODEL}"
    if platform == "vertex":                    # Claude on Google Cloud
        return anthropic.AnthropicVertex(**options), MODEL
    if platform == "foundry":                   # the request names your deployment
        return anthropic.AnthropicFoundry(**options), os.environ.get("CLAUDE_DEPLOYMENT", MODEL)
    raise ValueError(f"unknown platform: {platform}")
```

SDK extras: `pip install "anthropic[bedrock,vertex,aws]"`. Environment variables the clients picked up in the check:

| Client | Variables |
|---|---|
| `AnthropicAWS` | `AWS_REGION`, `ANTHROPIC_AWS_WORKSPACE_ID`, `ANTHROPIC_AWS_API_KEY` or ordinary AWS credentials |
| `AnthropicBedrockMantle` | `AWS_REGION`, `AWS_BEARER_TOKEN_BEDROCK` or AWS credentials |
| `AnthropicVertex` | `CLOUD_ML_REGION`, `ANTHROPIC_VERTEX_PROJECT_ID`, Google credentials |
| `AnthropicFoundry` | `ANTHROPIC_FOUNDRY_API_KEY`, `ANTHROPIC_FOUNDRY_RESOURCE` |

The AWS and Bedrock clients have no default region: without one the constructor raises `AnthropicError`.

## Client settings for production

| Setting | Default | What to do |
|---|---|---|
| `max_retries` | 2 (three attempts) | align with your queue's retries |
| `timeout` | 600 s read, 5 s connect | set from the caller's SLA |
| streaming | off | use for long replies |
| model ID | — | in configuration, not code |

The SDK itself rejects a non-streaming request with `max_tokens=128000` (`ValueError`) without sending it. Keep each reply's `request-id` (`response._request_id`) for support; on Foundry add `apim-request-id`.

## Before you ship

- Least-privilege keys and roles ([16 · Security](../16-security/basics.en.md)).
- Handling for errors, refusals and `max_tokens` ([14](../14-evals/advanced.en.md)).
- Usage and latency logging, a spend limit ([15](../15-observability/basics.en.md)).
- An eval that passes on the **platform and model** you ship.

## Related materials

- [Advanced theory](advanced.en.md)
- [16 · Security](../16-security/basics.en.md)

## Sources

- [Claude in Amazon Bedrock](https://platform.claude.com/docs/en/build-with-claude/claude-in-amazon-bedrock)
- [Claude Platform on AWS](https://platform.claude.com/docs/en/build-with-claude/claude-platform-on-aws)
- [Claude on Google Cloud](https://platform.claude.com/docs/en/build-with-claude/claude-on-vertex-ai)
- [Claude in Microsoft Foundry](https://platform.claude.com/docs/en/build-with-claude/claude-in-microsoft-foundry)
