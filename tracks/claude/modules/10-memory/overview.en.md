---
title: "CLAUDE.md and memory"
lang: en
track: claude
kind: module
doc: overview
slug: 10-memory
part: 2
tags: [claude, claude-code, claude-md, memory, context]
versions: "Claude Code 2.1.332"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-25
---

# CLAUDE.md and memory

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

How Claude Code remembers a project between sessions: **CLAUDE.md** files, rules, imports, **auto memory** and which of it reaches the context.

## Key ideas

- You write `CLAUDE.md`, Claude writes auto memory.
- Everything loaded costs context.
- `paths` rules and nested files load on demand.
- These are instructions, not enforcement.
- The first 200 lines or 25 KB of `MEMORY.md` load.

## Minimal example

```markdown
# Project

- Build: `npm run build`. Tests: `npm test` (run them before committing).
- Use 2-space indentation.
- API handlers live in `src/api/`.

See @docs/conventions.md
```

## When to use it and when not

For lasting project facts and conventions. Say one-off instructions in chat; set hard prohibitions with permissions.

## Material levels

| Level | File | Add with |
|---|---|---|
| 1. Fundamentals | [basics.en.md](basics.en.md) | |
| 2. Advanced theory | [advanced.en.md](advanced.en.md) | |
| 3. Expert theory | [expert.en.md](expert.en.md) | |
| 4. Self-check | [quiz.en.md](quiz.en.md), [exercises.en.md](exercises.en.md) | |
| 5. Runnable examples | — | `/learn claude 10-memory level 5` |
| 6. Tests | — | `/learn claude 10-memory level 6` |

## Related materials

- [Claude track](../../overview.en.md)
- [09 · Claude Code](../09-claude-code/overview.en.md)
- [05 · Prompt caching](../05-prompt-caching/overview.en.md)
