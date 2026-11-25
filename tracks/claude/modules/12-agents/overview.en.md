---
title: "Agents and subagents"
lang: en
track: claude
kind: module
doc: overview
slug: 12-agents
part: 2
tags: [claude, agents, subagents, agent-sdk, managed-agents]
versions: "Claude Code 2.1.332; claude-agent-sdk 0.2.189; anthropic 2.14.0; managed-agents-2026-04-01"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-25
---

# Agents and subagents

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

Agents in the Claude ecosystem: **subagents** in Claude Code, the **Agent SDK** in your program and **Managed Agents** hosted by Anthropic.

## Key ideas

- An agent is a model in a loop with tools.
- A subagent has its own context and returns a report.
- Tools and model are set per agent.
- A report is data to be verified.
- Start simple: often no agent is needed.

## Minimal example

```markdown
---
name: reviewer
description: Reviews code for bugs. Use after code changes.
tools: Read, Grep, Glob
model: haiku
---

You are a careful reviewer. Report concrete defects with file and line.
```

## When to use it and when not

Open multi-step tasks with a checkable result. Build fixed pipelines as ordinary code.

## Material levels

| Level | File | Add with |
|---|---|---|
| 1. Fundamentals | [basics.en.md](basics.en.md) | |
| 2. Advanced theory | [advanced.en.md](advanced.en.md) | |
| 3. Expert theory | [expert.en.md](expert.en.md) | |
| 4. Self-check | [quiz.en.md](quiz.en.md), [exercises.en.md](exercises.en.md) | |
| 5. Runnable examples | — | `/learn claude 12-agents level 5` |
| 6. Tests | — | `/learn claude 12-agents level 6` |

## Related materials

- [Claude track](../../overview.en.md)
- [07 · Tool use](../07-tool-use/overview.en.md)
- [11 · Skills and hooks](../11-skills-and-hooks/overview.en.md)
- [LLM](../../../llm/overview.en.md)
- [Claude Agent SDK](../../libraries/agent-sdk/overview.en.md)
