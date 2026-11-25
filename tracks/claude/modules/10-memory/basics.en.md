---
title: "CLAUDE.md and memory: fundamentals"
lang: en
track: claude
kind: module
doc: basics
slug: 10-memory
part: 2
tags: [claude, claude-code, claude-md, memory, context]
versions: "Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# CLAUDE.md and memory: fundamentals

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [09 · Claude Code](../09-claude-code/overview.en.md)

## How this was checked

Loading was checked in Claude Code 2.1.332: marker strings were put into the files and a local API stand-in recorded the requests. That shows **exactly what reaches the model**. How well the model follows the instructions is not tested by this.

## Why memory

Every session starts with an empty context. Whatever the agent must always know about the project goes into a file that is added to the start of each conversation.

| | CLAUDE.md | Auto memory |
|---|---|---|
| Written by | you | Claude |
| Holds | instructions and rules | things learned while working |
| Lives in | the repository or home directory | `~/.claude/projects/<project>/memory/` |

## A first CLAUDE.md

`/init` drafts one from the codebase. A good file is short and concrete:

```markdown
# Project

- Build: `npm run build`. Tests: `npm test` (run them before committing).
- Use 2-space indentation.
- API handlers live in `src/api/`.

See @docs/conventions.md
```

- Write checkable things: "2 spaces", not "format nicely".
- The documented target is under 200 lines.
- Leave out what the code already shows.

## Where files live

```text
~/.claude/CLAUDE.md                 yours, every project
repo/CLAUDE.md                      shared through git (or repo/.claude/CLAUDE.md)
repo/CLAUDE.local.md                yours, this project; add to .gitignore
repo/.claude/rules/general.md       loaded at start
repo/.claude/rules/python.md        has `paths:`; loaded when a matching file is opened
repo/sub/CLAUDE.md                  loaded when a file in sub/ is opened
```

## It is not enforcement

`CLAUDE.md` is text in the context, not a rule the program must enforce. Whatever must always hold (never read `.env`, never push) belongs in [permissions](../09-claude-code/advanced.en.md) or hooks.

## Inspecting and editing

- `/memory` — open instruction files and auto memory, switch auto memory on or off.
- `/context` — which files are loaded and what they cost.

## Common beginner mistakes

- A huge file about everything.
- Contradictions between the user and project files.
- Secrets in `CLAUDE.md`: the file goes to the model and into git.

## Related materials

- [Advanced theory](advanced.en.md)
- [05 · Prompt caching](../05-prompt-caching/overview.en.md)

## Sources

- [Memory](https://code.claude.com/docs/en/memory)
