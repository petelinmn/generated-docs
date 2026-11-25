---
title: "Claude Code: expert"
lang: en
track: claude
kind: module
doc: expert
slug: 09-claude-code
part: 2
tags: [claude, claude-code, permissions, headless, ci]
versions: "Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# Claude Code: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## Flags

| Flag | Purpose |
|---|---|
| `-p`, `--print` | answer and exit |
| `--output-format text\|json\|stream-json`, `--input-format` | formats |
| `--json-schema` | structured answer in `structured_output` |
| `--max-turns`, `--max-budget-usd` | limits |
| `--allowedTools`, `--disallowedTools`, `--permission-mode` | permissions |
| `-c`, `-r`, `--fork-session`, `--no-session-persistence` | sessions |
| `--model`, `--effort`, `--fallback-model` | model |
| `--system-prompt`, `--append-system-prompt` | system prompt |
| `--mcp-config`, `--strict-mcp-config` | MCP |
| `--settings`, `--setting-sources`, `--add-dir` | settings and directories |
| `--bare`, `--safe-mode` | start without customisations |
| `--bg` | background session |

Subcommands include `mcp`, `agents`, `attach`, `logs`, `stop`, `plugin`, `doctor`, `auth`, `install`, `update`; `claude --help` lists all. `--max-turns` is not shown there but is accepted.

## Models

Aliases with an API key in 2.1.332: `opus` → `claude-opus-5-5`, `sonnet` → `claude-sonnet-5-5`, `haiku` → `claude-haiku-5-5`, `fable` → `claude-fable-5-2`. An alias moves to newer models over time.

## What goes to the API

The captured request holds the system prompt, the tool list (`Read`, `Edit`, `Write`, `Bash`, `Glob`, `Grep`, `Agent`, `Skill`, `ToolSearch` and others), with git information and `CLAUDE.md` sent as user messages in `<system-reminder>`. MCP tools are deferred by default: names go first and schemas load through `ToolSearch`.

## Gotchas

1. A `-p` run with a permission refusal exits 0 with `subtype: success`.
2. `stream-json` without `--verbose` is an error.
3. `CLAUDE_*` variables inherited from another session change a child run's behaviour; set the environment explicitly in scripts.
4. A `CLAUDE.md` in a directory laid out like a session scratchpad (`claude/<project>/<session>/scratchpad`) is not loaded; `--debug-file` shows why.
5. `allow` cannot carve an exception out of `deny`.
6. In a not yet trusted directory `-p` ignored the `allow` entries of `.claude/settings.json` (warning on stderr); `deny` from the same file applied.

> ⚠️ Verify: `auto` mode, the sandbox, `/rewind`, background sessions, plugins, IDEs and GitHub Actions were not run.

## Interview questions

1. How is Claude Code different from a chat?
<details><summary>Answer</summary>

It is an agent with tools: it reads the project, edits and verifies by itself, within permissions.
</details>

2. How do you run it safely in CI?
<details><summary>Answer</summary>

`-p`, a narrow `--allowedTools`, `dontAsk`, turn and budget limits, minimal token rights, an isolated runner, secrets from a store.
</details>

3. `--allowedTools` vs permission mode?
<details><summary>Answer</summary>

The former adds allow rules, the latter decides what happens to the rest.
</details>

4. Which wins: allow or deny?
<details><summary>Answer</summary>

Deny.
</details>

5. Why is a Bash rule not a security boundary?
<details><summary>Answer</summary>

It matches a string; the same operation can be written another way.
</details>

6. How does a script learn something was refused?
<details><summary>Answer</summary>

`permission_denials` in the JSON: the exit code may be 0.
</details>

7. What do you do when context fills up?
<details><summary>Answer</summary>

Compaction is automatic; `/compact` with a focus, `/clear` between tasks and subagents for reading help.
</details>

8. `--system-prompt` vs `--append-system-prompt`?
<details><summary>Answer</summary>

The first replaces the system prompt, the second adds to it; `CLAUDE.md` loads either way.
</details>

9. What does `--bare` do?
<details><summary>Answer</summary>

A minimal mode with no hooks, auto memory or `CLAUDE.md` discovery; good for reproducible runs.
</details>

10. How do you add your own tools?
<details><summary>Answer</summary>

MCP servers; repeatable workflows are skills and hooks.
</details>

## Related materials

- [Advanced theory](advanced.en.md) · [Quiz](quiz.en.md)
- [10 · CLAUDE.md and memory](../10-memory/expert.en.md)

## Sources

- [CLI reference](https://code.claude.com/docs/en/cli-reference)
- [Permissions](https://code.claude.com/docs/en/permissions)
- [Context window](https://code.claude.com/docs/en/context-window)
