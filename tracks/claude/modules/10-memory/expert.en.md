---
title: "CLAUDE.md and memory: expert"
lang: en
track: claude
kind: module
doc: expert
slug: 10-memory
part: 2
tags: [claude, claude-code, claude-md, memory, context]
versions: "Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# CLAUDE.md and memory: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## Every instruction source

| Level | Location |
|---|---|
| managed | Windows `C:\Program Files\ClaudeCode\CLAUDE.md`; macOS `/Library/Application Support/ClaudeCode/CLAUDE.md`; Linux/WSL `/etc/claude-code/CLAUDE.md` |
| user | `~/.claude/CLAUDE.md`, `~/.claude/rules/` |
| project | `./CLAUDE.md`, `./.claude/CLAUDE.md`, `.claude/rules/` |
| local | `./CLAUDE.local.md` |

## Controlling loading

| Means | Observation |
|---|---|
| `claudeMdExcludes: ["**/proj/CLAUDE.md"]` | the file and its import disappeared; rules and the local file stayed |
| `--setting-sources user` | the user file only |
| `--bare` | nothing loaded |
| `CLAUDE_CODE_DISABLE_AUTO_MEMORY=1` | `MEMORY.md` not loaded |
| `--system-prompt` | `CLAUDE.md` still loaded |

The **Project instructions** setting chooses: `claude-md-or-agents-md` (default), `claude-md-and-agents-md`, `CLAUDE.md` only, or managed files only.

## What it looks like in the request

```text
<system-reminder>
Codebase and user instructions are shown below. Be sure to adhere to these instructions. ...

Contents of .../CLAUDE.md (user's private global instructions for all projects):
Contents of .../CLAUDE.md (project instructions, checked into the codebase):
Contents of .../CLAUDE.local.md (user's private project instructions, not checked in):
</system-reminder>
```

An excerpt of a captured 2.1.332 request with shortened paths; the wording may change between versions.

## Gotchas

1. Adding `CLAUDE.local.md` to a project that had only `AGENTS.md` stops `AGENTS.md` from being read (per the documentation).
2. `MEMORY.md` lines past 200 have no effect, and only the warning says so.
3. Long lines hit the 25 KB limit before the line limit: of 60 lines of 600 characters only part loaded.
4. `claudeMdExcludes` leaves `.claude/rules/` files that the pattern does not match.
5. A `CLAUDE.md` in a session-scratchpad-shaped directory is ignored.

> ⚠️ Verify: the managed file, the external-import approval dialog, compaction and when Claude writes auto memory by itself were not run.

## Interview questions

1. How does CLAUDE.md differ from the system prompt?
<details><summary>Answer</summary>

It is delivered as a user message after the system prompt and is not an enforcement mechanism.
</details>

2. How do you guarantee a prohibition?
<details><summary>Answer</summary>

With permissions or a hook, not a sentence in a file.
</details>

3. How do you keep context small in a monorepo?
<details><summary>Answer</summary>

`paths` rules, nested `CLAUDE.md`, `claudeMdExcludes`, skills for rare procedures.
</details>

4. Do imports save tokens?
<details><summary>Answer</summary>

No, the content is expanded into context.
</details>

5. What goes into CLAUDE.local.md?
<details><summary>Answer</summary>

Personal sandbox URLs and per-project preferences; the file is gitignored.
</details>

6. When is `AGENTS.md` not read?
<details><summary>Answer</summary>

When a `CLAUDE.md` or `CLAUDE.local.md` exists in the working directory or above (default setting).
</details>

7. What is a managed CLAUDE.md?
<details><summary>Answer</summary>

An organisation file in a system directory; it cannot be excluded.
</details>

8. What is the risk of a stranger's repository with a CLAUDE.md?
<details><summary>Answer</summary>

Its instructions are read by an agent running with your rights; read it before running.
</details>

9. How do you check what loaded?
<details><summary>Answer</summary>

`/context` and `/memory`; `--debug-file` shows skipped files.
</details>

10. What belongs there and what does not?
<details><summary>Answer</summary>

Commands, conventions, non-obvious facts yes; what the code shows, big references and secrets no.
</details>

## Related materials

- [Advanced theory](advanced.en.md) · [Quiz](quiz.en.md)
- [05 · Prompt caching](../05-prompt-caching/advanced.en.md)

## Sources

- [Memory](https://code.claude.com/docs/en/memory)
- [Settings](https://code.claude.com/docs/en/settings)
