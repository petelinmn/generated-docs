---
title: "CLAUDE.md and memory: advanced"
lang: en
track: claude
kind: module
doc: advanced
slug: 10-memory
part: 2
tags: [claude, claude-code, claude-md, memory, context]
versions: "Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# CLAUDE.md and memory: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## What loads and when

```text
at start            ~/.claude/CLAUDE.md, repo/CLAUDE.md, its import, rules/general.md, CLAUDE.local.md
after reading       sub/CLAUDE.md and rules/python.md are added
sub/a.py
```

Order in the request: the user file, the project file, its imports, rules, the local file. All of it travels as a user message, not as the system prompt. Started from the `sub/` directory, both the parent files and `sub/CLAUDE.md` loaded.

## Path-scoped rules

`.claude/rules/python.md`:

```markdown
---
paths:
  - "**/*.py"
---

- Use type hints in public functions.
```

A rule without `paths` always loads. With `paths` it loads when Claude reads, writes or edits a matching file, so instructions do not occupy context needlessly.

## Imports

- `@path` pulls in a file; a relative path resolves from the importing file.
- At most four hops: in a chain of five imports the fifth file did not load.
- Inside backticks and code blocks `@` is not an import.
- Imports organise but do not save: the content still enters the context.
- An import from project memory pointing outside the project needs approval.

HTML-comments `<!-- ... -->` are stripped before sending: notes for humans.

## AGENTS.md

With no `CLAUDE.md` or `CLAUDE.local.md` in the working directory or above, `AGENTS.md` is loaded. Otherwise `AGENTS.md` is skipped; to use both, put `@AGENTS.md` into `CLAUDE.md`. Your `~/.claude/CLAUDE.md` does not affect this.

## Auto memory

Claude maintains `MEMORY.md` (an index) and topic files. Only `MEMORY.md` loads at start — its first 200 lines or 25 KB; topic files are read on demand. It is plain markdown you can edit and delete.

A 205-line file loaded 200 lines with a warning appended:

```text
> WARNING: MEMORY.md is 205 lines (limit: 200). Only part of it was loaded: 5 of 205 lines were cut off, ...
```

To turn it off: the toggle in `/memory` or `CLAUDE_CODE_DISABLE_AUTO_MEMORY=1`.

## Long sessions and compaction

| What | After compaction (per the documentation) |
|---|---|
| root `CLAUDE.md` and unscoped rules | re-read from disk |
| `paths` rules and nested `CLAUDE.md` | come back when triggered again |
| things said only in chat | survives only as summary |

If an instruction must survive compaction, put it in the root file.

## Related materials

- [Fundamentals](basics.en.md) · [Expert](expert.en.md)
- [09 · Claude Code](../09-claude-code/advanced.en.md)

## Sources

- [Memory](https://code.claude.com/docs/en/memory)
- [Context window](https://code.claude.com/docs/en/context-window)
