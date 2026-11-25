---
title: "Agents and subagents: quiz"
lang: en
track: claude
kind: module
doc: quiz
slug: 12-agents
part: 2
tags: [claude, agents, subagents, agent-sdk, managed-agents]
versions: "Claude Code 2.1.332; claude-agent-sdk 0.2.189; anthropic 2.14.0; managed-agents-2026-04-01"
status: draft
updated: 2026-11-25
---

# Agents and subagents: quiz

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What does the main agent get back from a subagent?

- A) its system prompt
- B) nothing
- C) a report
- D) its whole history

<details><summary>Answer</summary>

**C.** A subagent works in its own window and returns only a report, which keeps the main context clean. Its history and system prompt do not come back.
→ [Fundamentals](basics.en.md)

</details>

## 2. 🟢 Where does a project subagent live?

- A) `.claude/agents/`
- B) `.claude/skills/`
- C) `.claude/hooks/`
- D) `agents.json`

<details><summary>Answer</summary>

**A.** Project subagents are defined in `.claude/agents/`. `skills` and `hooks` belong to other mechanisms, and an `agents.json` file is not where they live.
→ [Fundamentals](basics.en.md)

</details>

## 3. 🟢 Does a subagent see the main conversation history?

- A) yes, all
- B) only the last 10 messages
- C) only with `model: inherit`
- D) no

<details><summary>Answer</summary>

**D.** A regular subagent starts with a clean window and knows only what its task and definition give it. `model: inherit` picks the model, not the history.
→ [Fundamentals](basics.en.md)

</details>

## 4. 🟢 The built-in agent for searching without edits?

- A) `Deploy`
- B) `Explore`
- C) `reviewer`
- D) `Edit`

<details><summary>Answer</summary>

**B.** `Explore` is the built-in read-only search agent. `reviewer` is a custom example, `Edit` is a tool, and `Deploy` is not built in.
→ [Fundamentals](basics.en.md)

</details>

## 5. 🟡 How do you forbid launching the `reviewer` agent?

- A) `model: none`
- B) `deny: Agent(reviewer)`
- C) `tools: []` on the main one
- D) delete `description`

<details><summary>Answer</summary>

**B.** An `Agent(name)` deny rule blocks that agent. Removing its description or changing the model is not a ban, and emptying the main agent's `tools` would disable everything else.
→ [Advanced theory](advanced.en.md)

</details>

## 6. 🟡 What does `setting_sources=[]` do in the Agent SDK?

- A) disables tools
- B) forbids the network
- C) turns off code hooks
- D) reads no settings or `CLAUDE.md` from disk

<details><summary>Answer</summary>

**D.** An empty source list means no settings or instructions are read from disk. It does not disable tools, the network or hooks defined in code.
→ [Advanced theory](advanced.en.md)

</details>

## 7. 🟡 What lets a custom SDK tool run without a prompt?

- A) list `mcp__<server>__<tool>` in `allowed_tools`
- B) `@tool` is enough
- C) `max_turns`
- D) `setting_sources`

<details><summary>Answer</summary>

**A.** Declaring the tool is not enough: calls need permission until the full name is in `allowed_tools`. `max_turns` and `setting_sources` grant nothing.
→ [Advanced theory](advanced.en.md)

</details>

## 8. 🟡 What is true of Managed Agents?

- [ ] A) they are Zero Data Retention eligible
- [ ] B) an agent is created per request
- [ ] C) Anthropic hosts the loop and sandbox
- [ ] D) they are in beta

<details><summary>Answer</summary>

**C, D.** Managed Agents is an Anthropic-hosted loop and sandbox, in beta. It is not ZDR-eligible, and an agent is created once and reused across sessions.
→ [Advanced theory](advanced.en.md)

</details>

## 9. 🔴 Which built-in agent does not get `CLAUDE.md`?

- A) `general-purpose`
- B) a custom one
- C) all get it
- D) `Explore`

<details><summary>Answer</summary>

**D.** `Explore` (like the built-in `Plan`) does not get `CLAUDE.md`, so pass project rules in the task. `general-purpose` and custom agents receive it.
→ [Expert theory](expert.en.md)

</details>

## 10. 🔴 A subagent was refused permission. Where does it show?

- A) as exit code 2
- B) in the main run's `permission_denials`
- C) nowhere
- D) only in the agent file

<details><summary>Answer</summary>

**B.** A subagent's denial lands in the run's list. It is not signalled by exit code 2, and nothing is written to the agent file.
→ [Expert theory](expert.en.md)

</details>

## 11. 🔴 Default subagent nesting:

- A) unlimited
- B) exactly one
- C) up to three layers
- D) forbidden

<details><summary>Answer</summary>

**C.** Nesting is allowed up to three levels below the main conversation by default: possible, but not unlimited.
→ [Expert theory](expert.en.md)

</details>

## 12. 🔴 Open question: how do you review a large PR with agents?

<details><summary>Answer</summary>

Split by area or aspect; read-only subagents with narrow tools; give each context and criteria; run in parallel; verify findings separately; the main agent aggregates; turn and budget limits; a human decides.
→ [Advanced theory](advanced.en.md)

</details>
