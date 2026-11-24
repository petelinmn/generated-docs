---
title: "The Claude family and model selection"
lang: en
track: claude
kind: module
doc: overview
slug: 01-models
part: 1
levels: [1, 2, 3, 4, 5, 6]
tags: [claude, model-selection, context-window, effort, model-ids]
versions: "Claude 5.5 family; 2026-11"
status: verified
updated: 2026-11-23
---

# The Claude family and model selection

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

Claude is Anthropic's family of models available through the Messages API, with different capabilities and speed, cost and quality trade-offs. The 5.5 family includes Fable for maximum capability, Opus for complex agentic work and coding, Sonnet for everyday workloads and Haiku for low latency and high volume.

Model selection starts with the task criteria, not the name of a model line: define accuracy, latency and budget requirements, create representative test tasks, then compare models and effort levels. For a single request, distinguish the API model ID, alias, pinned model ID and deployment name on a partner platform.

## Key ideas

- A model is defined by more than its family: thinking modes, effort and context limits differ.
- If the workload is not yet defined, Anthropic recommends starting with Opus; for high-volume tasks, select Haiku or Sonnet based on eval results.
- Fable is positioned for maximum capability in especially demanding reasoning or long-running agentic work.
- The context window is the entire working memory of a request: system prompt, messages, tools, images and documents.
- Current 1M-context models have a standard 1M limit; always verify lifecycle and specifications in the model table.
- Effort can be tuned within one model before changing models.
- Multimodel strategies send routine steps to a lower-cost model and reserve difficult decisions for a stronger model.

## Minimal example

```python
def recommend(task_type: str) -> str:
    recommendations = {
        "coding": "claude-opus-5-5",
        "high_volume": "claude-haiku-5-5",
        "everyday": "claude-sonnet-5-5",
        "demanding": "claude-fable-5-2",
    }
    return recommendations.get(task_type, "claude-opus-5-5")


print(recommend("coding"))
```

Output:

```text
coding claude-opus-5-5
```

## When to use and when not

- ✅ Compare models with real prompts and edge-case sets.
- ✅ Tune effort within a model before changing families.
- ✅ Keep model IDs or deployment names in configuration.
- ❌ Choose only by brand or displayed model price.
- ❌ Assume a large context guarantees quality: context rot can reduce recall and accuracy.
- ❌ Mix a versioned model ID with a partner-specific inference profile without checking the platform.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | the family, models, model IDs, context and a first choice |
| [2. Advanced](advanced.en.md) | effort, evals, multimodel, platform IDs and lifecycle |
| [3. Expert](expert.en.md) | reference, breaking changes, gotchas and interview questions |
| [4. Self-check](quiz.en.md) | quiz and exercises for model selection |
| [5. Runnable examples](code/examples/) | deterministic routing and eval examples |
| [6. Tests](code/tests/) | pytest checks for routing, eval matrix and configuration |

## Related materials

- Prerequisites: [LLM](../../../llm/overview.en.md) and HTTP/API fundamentals.
- Next: [02 · Messages API](../02-messages-api/overview.en.md)
- The same idea in other tracks: model selection and evaluation principles in [LLM](../../../llm/overview.en.md).
