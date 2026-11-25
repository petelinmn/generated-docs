---
title: "Claude Code: fundamentals"
lang: en
track: claude
kind: module
doc: basics
slug: 09-claude-code
part: 2
tags: [claude, claude-code, permissions, headless, ci]
versions: "Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# Claude Code: fundamentals

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## How this was checked

Commands ran in Claude Code 2.1.332 on Windows with an isolated settings directory. A local stand-in answered instead of the API and requested tools by script. So the behaviour of the program itself (permissions, exit codes, formats) was checked, not model answers. Interactive commands, IDEs and GitHub Actions are described from the documentation.

## What it is

Claude Code is Anthropic's agent that works on your project: it reads files, edits code and runs commands. It is the [tool use](../07-tool-use/overview.en.md) loop with the tools already built.

## Installation

| System | Command |
|---|---|
| macOS, Linux, WSL | `curl -fsSL https://claude.ai/install.sh \| bash` |
| Windows PowerShell | `irm https://claude.ai/install.ps1 \| iex` |
| Homebrew | `brew install --cask claude-code` |
| WinGet | `winget install Anthropic.ClaudeCode` |

Homebrew and WinGet do not auto-update. You need a subscription login or an API key.

## First run

```bash
cd my-project
claude                       # interactive session
claude -p "Explain what this repository does"      # one answer, then exit
```

## Permissions

Before a risky action Claude Code asks for permission. In the default mode reading and safe commands such as `ls` ran without asking, while `touch` and writing a file needed approval.

| Mode | Writing a project file |
|---|---|
| `default` (shown as Manual) | asks |
| `acceptEdits` | allowed |
| `plan` | refused: research and planning only |
| `dontAsk` | refuses anything that would prompt |
| `auto` | a classifier decides |
| `bypassPermissions` | allowed; isolated environments only |

Switch modes with `Shift+Tab` or `--permission-mode`.

## Useful in-session commands

| Command | What it does |
|---|---|
| `/init` | creates `CLAUDE.md` |
| `/clear`, `/compact` | new conversation; compress the current one |
| `/context` | what fills the context |
| `/permissions`, `/model`, `/mcp` | permissions, model, MCP servers |
| `/resume`, `/rewind` | return to a session; roll back edits and conversation |
| `Esc` | interrupt and keep what is done |
| `@file`, `!cmd` | mention a file; run a command yourself |

## How to phrase a task

- Give the goal and a check: "fix test X, run `npm test`".
- Start big changes in plan mode.
- Work in git: everything is visible and easy to revert.

## Common beginner mistakes

- Turning on `bypassPermissions` on a work machine.
- Doing everything in one session: stale context gets in the way.
- Treating `/rewind` as a replacement for git.

## Related materials

- [Advanced theory](advanced.en.md)
- [10 · CLAUDE.md and memory](../10-memory/overview.en.md)

## Sources

- [Overview and installation](https://code.claude.com/docs/en/overview)
- [Permission modes](https://code.claude.com/docs/en/permission-modes)
- [Commands](https://code.claude.com/docs/en/commands)
