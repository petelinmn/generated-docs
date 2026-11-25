---
title: "Skills and hooks"
lang: en
track: claude
kind: module
doc: overview
slug: 11-skills-and-hooks
part: 2
tags: [claude, claude-code, skills, hooks, automation]
versions: "Claude Code 2.1.332"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-25
---

# Skills and hooks

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

Two ways to extend Claude Code: **skills**, instructions loaded when needed, and **hooks**, your commands run on events.

## Key ideas

- The model reads a skill; the program runs a hook.
- Only skill descriptions sit in context.
- Exit code 2 blocks, 1 does not.
- A `deny` rule beats a hook's allow.
- Third-party skills and hooks are executable code.

## Minimal example

```markdown
---
description: Greet a person by name. Use when the user asks for a greeting.
---

Greet $0 politely. Mention the current branch: !`git branch --show-current`
```

## When to use it and when not

A skill for a repeatable procedure; a hook for what must always happen. A one-off instruction is simpler to type in chat.

## Material levels

| Level | File | Add with |
|---|---|---|
| 1. Fundamentals | [basics.en.md](basics.en.md) | |
| 2. Advanced theory | [advanced.en.md](advanced.en.md) | |
| 3. Expert theory | [expert.en.md](expert.en.md) | |
| 4. Self-check | [quiz.en.md](quiz.en.md), [exercises.en.md](exercises.en.md) | |
| 5. Runnable examples | — | `/learn claude 11-skills-and-hooks level 5` |
| 6. Tests | — | `/learn claude 11-skills-and-hooks level 6` |

## Related materials

- [Claude track](../../overview.en.md)
- [09 · Claude Code](../09-claude-code/overview.en.md)
- [10 · CLAUDE.md and memory](../10-memory/overview.en.md)
- [12 · Agents](../12-agents/overview.en.md)
