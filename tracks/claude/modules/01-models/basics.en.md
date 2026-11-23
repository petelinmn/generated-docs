---
title: "The Claude family and model selection: basics"
lang: en
track: claude
kind: module
doc: basics
slug: 01-models
part: 1
tags: [claude, model-selection, context-window, model-ids]
versions: "Claude 5.5 family; 2026-11"
status: verified
updated: 2026-11-23
---

# The Claude family and model selection: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [Overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md) · [5. Runnable examples](code/examples/) · [6. Tests](code/tests/)

**Prerequisites:** [LLM](../../../llm/overview.en.md) · **Time:** ~45 min

**Level 5:** [runnable examples](code/examples/) — routing and eval matrix with no network requests.

## Why it matters

The same text request can be sent to different Claude models, but the cost of a poor choice grows with workload: a weak model may require more iterations, while an excessively powerful model can increase the cost of high-frequency tasks. A basic choice comes down to four variables: capability, latency, cost and effort.

## The Claude 5.5 family

The current 5.5 family has four major profiles:

| Model | API ID | Primary profile | Typical starting workload |
|---|---|---|---|
| Claude Fable 5.2 | `claude-fable-5-2` | Maximum capability | The hardest reasoning tasks and long agentic sessions |
| Claude Opus 5.5 | `claude-opus-5-5` | Agentic coding and knowledge work | Complex development, refactoring and autonomous agents |
| Claude Sonnet 5.5 | `claude-sonnet-5-5` | Balance of speed and capability | Everyday code, analysis, content and tools |
| Claude Haiku 5.5 | `claude-haiku-5-5` | Lowest latency and cost | Classification, extraction, routing and subagents |

All support text and image input, text output, multilingual capabilities, vision and tool use. Verify current IDs, retirement data and partner-platform endpoints on the official models page.

## Two ways to start

With **efficiency-first**, start with Haiku, test quality on a real dataset and upgrade only when you find a capability gap.

With **capability-first**, start with Opus for complex agentic coding or enterprise work, evaluate quality and then reduce effort or move to a cheaper model only where evals remain acceptable. Consider Fable for the hardest reasoning workloads, when even xhigh or max effort does not meet the goal.

If the workload is not yet defined, Anthropic recommends starting with Opus 5.5. Sonnet 5.5 is a reasonable starting point when speed and capability matter for everyday coding, analysis, content and agentic workloads.

## Context and effort

The context window includes the system prompt, all messages, tools, tool results, images and documents. Thinking components are also included in the limit. Current 1M-context 5.5 models have a standard 1M input window and up to 128K synchronous output; no beta header is needed to enable the context.

Effort is set for the Messages API with `output_config.effort`. It changes response thoroughness and volume with one model ID:

```python
effort = "medium"

request = {
    "model": "claude-opus-5-5",
    "max_tokens": 4096,
    "messages": [{"role": "user", "content": "Name the trade-off between two approaches"}],
    "output_config": {"effort": effort},
}

print(request["model"], request["output_config"]["effort"])
```

Output:

```text
claude-opus-5-5 medium
```

By default, Opus 5.5 and Haiku 5.5 use `medium`; other main models use `high`. Verify exact defaults on each model page.

## Model IDs and snapshots

Use:

| Term | Meaning |
|---|---|
| API model ID | ID for the Claude API, for example `claude-opus-5-5` |
| Alias | Convenient pointer to a snapshot for generations before 4.6 |
| Pinned model ID | An identifier for a pinned model version; the weights and configuration for that ID are fixed |
| Deployment name | Name of a deployment on Microsoft Foundry, sent instead of the native ID |
| Inference profile | Deployment/routing ID on a partner platform, for example Bedrock |

For generations 4.6 and later, a dateless ID is the canonical pinned ID, while a short alias for an older model may point to the latest dated snapshot. Serving infrastructure can affect observable responses even when the pinned ID's weights do not change. IDs and lifecycle depend on the provider.

## Beginner mistakes

- ❌ Use Fable for every task → ✅ implement the workflow first and measure typical cost and quality.
- ❌ Assume 1M is useful for every request → ✅ curate context; excess context can reduce recall.
- ❌ Change models by subjective impression → ✅ store prompts, answers, scores and latency in an eval set.
- ❌ Hard-code a partner-specific ID → ✅ keep the provider and model/deployment in separate configuration.

## Related materials

- Prerequisite: [LLM](../../../llm/overview.en.md)
- Next module: Messages API in the [Claude track](../../overview.en.md) roadmap
- Continue with [advanced theory](advanced.en.md)

## Sources

- [Claude models overview](https://platform.claude.com/docs/en/models/overview)
- [Choosing the right model](https://platform.claude.com/docs/en/about-claude/models/choosing-a-model)
- [Effort](https://platform.claude.com/docs/en/build-with-claude/effort)
- [Context windows](https://platform.claude.com/docs/en/build-with-claude/context-windows)
