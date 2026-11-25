---
title: "Agents and subagents: fundamentals"
lang: en
track: claude
kind: module
doc: basics
slug: 12-agents
part: 2
tags: [claude, agents, subagents, agent-sdk, managed-agents]
versions: "Claude Code 2.1.332; claude-agent-sdk 0.2.189; anthropic 2.14.0; managed-agents-2026-04-01"
status: draft
updated: 2026-11-25
---

# Agents and subagents: fundamentals

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [07 · Tool use](../07-tool-use/overview.en.md), [09 · Claude Code](../09-claude-code/overview.en.md)

## How this was checked

Subagents ran in Claude Code 2.1.332 and the Agent SDK through `claude-agent-sdk` 0.2.189. A local stand-in answered instead of the API: it requested tools by script and recorded requests. Managed Agents need the real API: only the SDK's request shapes were checked.

## What an agent is

An agent is a model in a loop with tools: it picks the next step itself until the task is done. Not everything should be one: when the steps are known, ordinary code is cheaper and more predictable.

## Subagent

A **subagent** is a helper with its own context: the main agent hands it part of the work and gets back only a report. A long search therefore does not clutter the main conversation.

`.claude/agents/reviewer.md`:

```markdown
---
name: reviewer
description: Reviews code for bugs. Use after code changes.
tools: Read, Grep, Glob
model: haiku
---

You are a careful reviewer. Report concrete defects with file and line.
```

- `description` tells the main agent when to call it.
- `tools` limits the tools, `model` picks the model.
- You can ask explicitly: "have the reviewer check the changes".

## What a subagent sees

| Gets | Does not get |
|---|---|
| its own system prompt and the task | the main conversation history |
| `CLAUDE.md` (except the built-in Explore and Plan) | |

So the task itself must contain everything needed.

## Built-in ones

- `Explore` — searches code without editing
- `Plan` — research for a plan
- `general-purpose` — all tools

## Common beginner mistakes

- Delegating trivia: every subagent spends its own tokens.
- A task with no context: "check this".
- Trusting the report unverified.

## Related materials

- [Advanced theory](advanced.en.md)
- [11 · Skills and hooks](../11-skills-and-hooks/basics.en.md)

## Sources

- [Subagents](https://code.claude.com/docs/en/sub-agents)
