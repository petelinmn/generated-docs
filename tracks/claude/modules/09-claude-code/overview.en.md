---
title: "Claude Code"
lang: en
track: claude
kind: module
doc: overview
slug: 09-claude-code
part: 2
tags: [claude, claude-code, permissions, headless, ci]
versions: "Claude Code 2.1.332"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-25
---

# Claude Code

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

**Claude Code** is an agent for code work in the terminal, the editor and CI. This module: installation, permissions, sessions, non-interactive mode and settings.

## Key ideas

- Permissions decide what the agent may do without you.
- `deny` beats `ask`, `ask` beats `allow`.
- `claude -p` turns the agent into a command for scripts.
- Settings exist per user, per project, local and managed.
- Command rules are not a sandbox.

## Minimal example

```bash
cd my-project
claude                       # interactive session
claude -p "Explain what this repository does"      # one answer, then exit
```

## When to use it and when not

Repository tasks with a checkable result. Do not grant broad rights where a mistake cannot be undone.

## Material levels

| Level | File | Add with |
|---|---|---|
| 1. Fundamentals | [basics.en.md](basics.en.md) | |
| 2. Advanced theory | [advanced.en.md](advanced.en.md) | |
| 3. Expert theory | [expert.en.md](expert.en.md) | |
| 4. Self-check | [quiz.en.md](quiz.en.md), [exercises.en.md](exercises.en.md) | |
| 5. Runnable examples | — | `/learn claude 09-claude-code level 5` |
| 6. Tests | — | `/learn claude 09-claude-code level 6` |

## Related materials

- [Claude track](../../overview.en.md)
- [07 · Tool use](../07-tool-use/overview.en.md)
- [08 · MCP](../08-mcp/overview.en.md)
- [10 · CLAUDE.md and memory](../10-memory/overview.en.md)
