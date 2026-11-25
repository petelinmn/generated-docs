---
title: "Skills and hooks: advanced"
lang: en
track: claude
kind: module
doc: advanced
slug: 11-skills-and-hooks
part: 2
tags: [claude, claude-code, skills, hooks, automation]
versions: "Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# Skills and hooks: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Skill frontmatter

| Field | Effect |
|---|---|
| `description`, `when_to_use` | when to use it; together cut at 1536 characters |
| `disable-model-invocation: true` | `/name` only; the model does not see it listed |
| `user-invocable: false` | for the model only; `/name` is refused |
| `allowed-tools` | tools pre-approved for the invoking turn |
| `context: fork`, `agent` | run in a separate subagent |
| `model`, `effort`, `paths`, `hooks`, `argument-hint` | set the model, effort, scope, hooks and hint |

Checked: a skill with `allowed-tools: Bash(touch *)` created a file without asking; the same call without it was refused.

Arguments: `$ARGUMENTS` is all of them, `$0`, `$1` are by index with shell-like quoting. `/greet "Ann Lee" tomorrow` gave `$0` = `Ann Lee`. An index without an argument stays the text `$1`. `${{CLAUDE_SKILL_DIR}}` is the skill directory, for bundled scripts and references. Keep `SKILL.md` under 500 lines and put detail into neighbouring files.

Older `.claude/commands/name.md` files are still invoked as `/name`.

## Hook events

| Event | When | What exit 2 does |
|---|---|---|
| `PreToolUse` | before a tool | blocks the call |
| `PostToolUse` | after success | the tool already ran; Claude gets the feedback |
| `UserPromptSubmit` | a prompt is submitted | rejects the prompt: no API request is made |
| `Stop` | Claude finishes | makes it continue |
| `SessionStart` | start or resume | — |
| `SubagentStart`, `SubagentStop`, `PermissionRequest`, `PreCompact`, `PostCompact`, `SessionEnd`, `Notification` | | |

For tool events `matcher` is compared with the tool name: `Bash`, `Edit|Write`, `mcp__server__.*`.

## Exit codes and JSON

| Hook result | Observed for `PreToolUse` |
|---|---|
| exit 0, no output | the normal permission flow |
| exit 2 | blocked, even in `bypassPermissions` |
| exit 1 | does not block |
| `permissionDecision: "deny"` | blocked with the reason |
| `permissionDecision: "allow"` | no prompt, but a `deny` rule still applies |
| `permissionDecision: "ask"` | ask; in `-p` that is a refusal |
| `updatedInput` | replaces the arguments: an `echo` ran instead of `touch` |

```json
{"hookSpecificOutput": {"hookEventName": "PreToolUse", "permissionDecision": "deny",
                        "permissionDecisionReason": "use the deploy script instead"}}
```

The stdout of a `SessionStart` or `UserPromptSubmit` hook is added to context.

## Do not loop on Stop

On the repeat run the input has `stop_hook_active: true`: check it and exit 0.

## Related materials

- [Fundamentals](basics.en.md) · [Expert](expert.en.md)
- [12 · Agents](../12-agents/basics.en.md)

## Sources

- [Skills](https://code.claude.com/docs/en/skills)
- [Hooks reference](https://code.claude.com/docs/en/hooks)
