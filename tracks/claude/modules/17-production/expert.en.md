---
title: "Deployment and production: expert"
lang: en
track: claude
kind: module
doc: expert
slug: 17-production
part: 3
tags: [claude, deployment, production, retries, bedrock, vertex-ai, foundry]
versions: "Claude 5.5 family; anthropic 2.14.0; Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# Deployment and production: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## What goes over the wire

Checked with a local server and `anthropic` 2.14.0:

| Client | Default base URL | Path | Auth |
|---|---|---|---|
| `Anthropic` | `api.anthropic.com` | `/v1/messages` | API key |
| `AnthropicAWS` | `aws-external-anthropic.REGION.api.aws` | `/v1/messages` + `anthropic-workspace-id` | SigV4 or a key |
| `AnthropicBedrockMantle` | `bedrock-mantle.REGION.api.aws/anthropic/` | `/v1/messages` | SigV4 or bearer |
| `AnthropicBedrock` (legacy integration) | `bedrock-runtime.REGION.amazonaws.com` | `/model/MODEL/invoke` | SigV4 or bearer |
| `AnthropicVertex`, global | `aiplatform.googleapis.com/v1/` | `…/publishers/anthropic/models/MODEL:rawPredict` | Google token |
| `AnthropicVertex`, `us` / `eu` | `aiplatform.REGION.rep.googleapis.com/v1/` | same | Google token |
| `AnthropicFoundry` | `RESOURCE.services.ai.azure.com/anthropic/` | `/v1/messages` | `x-api-key` / Entra ID |

In legacy Bedrock and Vertex the model is in the path, not the body, and the API version travels as the body field `anthropic_version`.

## Claude Code on cloud platforms

| Platform | Enable | Model requested (no pinning) |
|---|---|---|
| Foundry | `CLAUDE_CODE_USE_FOUNDRY=1` | `claude-sonnet-5-5` |
| Bedrock (mantle) | `CLAUDE_CODE_USE_MANTLE=1` | `anthropic.claude-opus-5-5` |
| Claude Platform on AWS | `CLAUDE_CODE_USE_ANTHROPIC_AWS=1` | `claude-opus-5-5` |
| Bedrock (runtime) | `CLAUDE_CODE_USE_BEDROCK=1` | — |
| Vertex AI | `CLAUDE_CODE_USE_VERTEX=1` | — |

The default model depends on the platform, so pin it for a team: `ANTHROPIC_DEFAULT_OPUS_MODEL`, `ANTHROPIC_DEFAULT_SONNET_MODEL`, `ANTHROPIC_DEFAULT_HAIKU_MODEL`. With a deployment name it does not know, Claude Code warns `unrecognized_model` and assumes a 1M window. Rows with “—” were not run.

## Fast mode

`speed: "fast"` gives up to 2.5x faster output at a premium (Opus 5.5: $8 / $40 per million). Not available with the Batch API; switching speed misses the prompt cache; fast mode has its own limits. `usage.speed` reports what was used. On Foundry it is a separate `-fast` model deployment; Bedrock and Claude Platform on AWS lack it.

## Platform differences

- Claude Platform on AWS: a separate capacity pool; no workspace Admin API, HIPAA readiness, OAuth or fast mode.
- Bedrock: no URL sources for images and documents, no server-side tools or `fallbacks`.
- Vertex AI: a 30 MB request payload limit versus 128 MB for the Claude API's Messages API.
- Foundry: two hosting options (on Azure, on Anthropic) with different features; unsupported ones on Azure return 400.
- Priority Tier is no longer sold; existing commitments run to their end date.

## Gotchas

1. `hasattr(client.messages, "batches")` is `True` even where batches are unsupported.
2. The SDK retries a spend-cap 429 although retrying cannot help.
3. `except anthropic.OverloadedError` misses an overload inside a stream: it is `APIStatusError` with status 200 and `type == "overloaded_error"`.
4. `OverloadedError` subclasses `InternalServerError`: `except` order matters.
5. Foundry takes your deployment name in `model`, not necessarily the model ID.
6. `ANTHROPIC_*` variables from a developer shell change where and how requests go; check the environment in CI.

> ⚠️ Verify: model and feature availability, regions, quotas, prices, limits and dates were checked only in the documentation; no real cloud was called.

## Trade-offs

| Option | Gain | Cost |
|---|---|---|
| Claude API | every feature first | a separate contract and bill |
| A cloud platform | existing IAM, billing, networking | features missing or later |
| Several routes | survives one failing | double evals, cold caches |
| LLM gateway | central keys, accounting, limits | another service in the request path |

## Interview questions

1. How does Claude Platform on AWS differ from Bedrock?
<details><summary>Answer</summary>

Anthropic operates the former, the same Claude API billed through AWS; Bedrock is an AWS service with its own feature schedule and quotas.
</details>

2. What changes in code when moving to Bedrock?
<details><summary>Answer</summary>

The client class, the `anthropic.` model prefix, authentication and missing features.
</details>

3. Which errors does the SDK not retry?
<details><summary>Answer</summary>

Client errors such as 400 and 401, and errors inside a started stream.
</details>

4. Why are retries at several layers dangerous?
<details><summary>Answer</summary>

They multiply: three SDK attempts times three queue attempts is nine requests.
</details>

5. How do you tell a spend cap from a rate limit?
<details><summary>Answer</summary>

On the Messages API by `error.details.error_code` or `EnforcedSpendLimitReachedError`.
</details>

6. When do you need a regional endpoint?
<details><summary>Answer</summary>

When requirements pin processing to a region; it costs 10% more and may narrow the model choice.
</details>

7. What do you check before relying on a fallback route?
<details><summary>Answer</summary>

An eval on it, feature availability, quotas, a cold cache and who pays.
</details>

8. Why keep the `request-id`?
<details><summary>Answer</summary>

It identifies the request for support and log correlation.
</details>

9. How do you change models safely?
<details><summary>Answer</summary>

Configuration, eval, a traffic share, metrics, rollback.
</details>

10. Does failover solve a refusal?
<details><summary>Answer</summary>

Exception-based failover does not: a refusal is a 200. Check `stop_reason`, use the middleware or `fallbacks`.
</details>

## Related materials

- [Advanced theory](advanced.en.md) · [Quiz](quiz.en.md)
- [09 · Claude Code](../09-claude-code/expert.en.md) · [16 · Security](../16-security/expert.en.md)

## Sources

- [Fast mode](https://platform.claude.com/docs/en/build-with-claude/fast-mode)
- [Claude Code third-party integrations](https://code.claude.com/docs/en/third-party-integrations)
- [Claude Code model configuration](https://code.claude.com/docs/en/model-config)
- [Service tiers](https://platform.claude.com/docs/en/api/service-tiers)
