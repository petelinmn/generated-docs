---
title: "Agents and subagents: expert"
lang: en
track: claude
kind: module
doc: expert
slug: 12-agents
part: 2
tags: [claude, agents, subagents, agent-sdk, managed-agents]
versions: "Claude Code 2.1.332; claude-agent-sdk 0.2.189; anthropic 2.14.0; managed-agents-2026-04-01"
status: draft
updated: 2026-11-25
---

# Agents and subagents: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## Subagent fields

| Field | Meaning |
|---|---|
| `name`, `description` | required |
| `tools`, `disallowedTools` | allowed and removed tools |
| `model`, `effort` | `sonnet`, `opus`, `haiku`, `fable`, ID, `inherit` |
| `permissionMode`, `maxTurns` | permission mode and turn limit |
| `skills`, `mcpServers`, `hooks` | preloaded skills, servers, hooks |
| `memory`, `isolation`, `background`, `omitClaudeMd` | memory, worktree, background, no CLAUDE.md |

## What the requests showed

| Agent | Tools | `CLAUDE.md` |
|---|---|---|
| custom `reviewer` | `Read`, `Grep`, `Glob`; haiku model | yes |
| `Explore` | no `Edit`, `Write`, `Agent` | no |
| `general-purpose` | including `Edit`, `Write`, `Agent` | yes |

The agent file's body becomes part of its system prompt. The main agent receives the report as a `tool_result` marked as model output with no user authority, with an `agentId` for follow-up.

## Gotchas

1. An unknown `subagent_type` is a tool error listing the available ones, not a crash.
2. A subagent on another model is billed at its price: both show in the result's `modelUsage`.
3. `Explore` does not see `CLAUDE.md`: pass project rules in the task.
4. In the Agent SDK, without `setting_sources=[]` local settings and `CLAUDE.md` affect your product.
5. A custom SDK tool without `allowed_tools` is refused: declaring it is not enough.

> ⚠️ Verify: background and nested subagents, `isolation`, `memory`, agent teams and resuming by `agentId` are from the documentation.

## Interview questions

1. When do you need an agent?
<details><summary>Answer</summary>

The task is multi-step and hard to specify, the result justifies the cost, and errors are detectable and reversible.
</details>

2. What is a subagent for?
<details><summary>Answer</summary>

Context isolation, restricted tools, a chosen model, parallelism.
</details>

3. What does a subagent not know?
<details><summary>Answer</summary>

The main conversation history.
</details>

4. Subagent vs skill?
<details><summary>Answer</summary>

A skill is instructions in the same (or, forked, a separate) context; a subagent is a separate worker with its own settings.
</details>

5. Tool Runner vs Agent SDK?
<details><summary>Answer</summary>

Tool Runner loops over your tools in the plain API; the Agent SDK is the Claude Code harness with built-in tools.
</details>

6. How do you bound spend?
<details><summary>Answer</summary>

`max_turns`, budgets, cheaper models for simple parts, fewer tools.
</details>

7. How do you avoid trusting a report blindly?
<details><summary>Answer</summary>

Checkable criteria, tests, a second reviewer, file references.
</details>

8. Why is a report from untrusted content risky?
<details><summary>Answer</summary>

It may carry instructions from what was read: it is data, not orders.
</details>

9. What are agent teams?
<details><summary>Answer</summary>

An experimental mode of several sessions exchanging messages; off by default.
</details>

10. Foreground vs background?
<details><summary>Answer</summary>

The former blocks until the report, the latter works concurrently and reports later.
</details>

## Related materials

- [Advanced theory](advanced.en.md) · [Quiz](quiz.en.md)
- [13 · Long context](../13-long-context/basics.en.md)

## Sources

- [Subagents](https://code.claude.com/docs/en/sub-agents)
- [Agent teams](https://code.claude.com/docs/en/agent-teams)
