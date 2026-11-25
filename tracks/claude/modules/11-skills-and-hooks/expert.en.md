---
title: "Skills and hooks: expert"
lang: en
track: claude
kind: module
doc: expert
slug: 11-skills-and-hooks
part: 2
tags: [claude, claude-code, skills, hooks, automation]
versions: "Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# Skills and hooks: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## All events

Per the documentation: `SessionStart`, `Setup`, `InstructionsLoaded`, `UserPromptSubmit`, `UserPromptExpansion`, `PreSkillShellCommand`, `MessageDisplay`, `PreToolUse`, `PermissionRequest`, `PostToolUse`, `PostToolUseFailure`, `PostToolBatch`, `PermissionDenied`, `Notification`, `SubagentStart`, `SubagentStop`, `TaskCreated`, `TaskCompleted`, `Stop`, `StopFailure`, `TeammateIdle`, `ConfigChange`, `CwdChanged`, `DirectoryAdded`, `FileChanged`, `WorktreeCreate`, `WorktreeRemove`, `PreCompact`, `PostCompact`, `PreModelSwitch`, `PostModelSwitch`, `SessionEnd`, `Elicitation`, `ElicitationResult`.

## Handlers and timeouts

| `type` | Runs | Default timeout, s |
|---|---|---|
| `command` | a command | 600 |
| `http` | an HTTP request | 600 |
| `mcp_tool` | an MCP tool | 600 |
| `prompt` | a model prompt | 30 |
| `agent` | an agent with tools | 60 |

`UserPromptSubmit` and some events have shorter defaults. A command hook can run in the background (`async: true`).

## What a hook receives

A captured `PreToolUse` had `session_id`, `transcript_path`, `cwd`, `prompt_id`, `permission_mode`, `effort`, `hook_event_name`, `tool_name`, `tool_input`, `tool_use_id`. `PostToolUse` adds `tool_response` and `duration_ms`; `Stop` adds `stop_hook_active` and `last_assistant_message`.

## Where they are declared

User, project, local and managed settings; plugins; skill and subagent frontmatter. Hook output is capped at 10,000 characters: longer text is saved to a file and Claude gets the path and a preview.

## Gotchas

1. `exit 1` does not block: the call went on to the normal permission check.
2. A hook with `timeout: 3` that slept 20 s before exiting 2 was cancelled, and the command ran.
3. `matcher: Edit` did not run for `Bash`: a wrong matcher silently disables the guard.
4. A `disable-model-invocation` skill called through the `Skill` tool returned an error forbidding a work-around; `/name` works.
5. Git Bash turns a `/greet` argument into a Windows path: set `MSYS2_ARG_CONV_EXCL='*'` in scripts.
6. `PostToolUse` cannot undo anything: the action already happened.

> ⚠️ Verify: `http`, `mcp_tool`, `prompt` and `agent` handlers, async hooks, plugins and most events in the list were not run.

## Interview questions

1. Skill or CLAUDE.md?
<details><summary>Answer</summary>

CLAUDE.md for what is always needed; a skill for an occasional procedure whose body loads on use.
</details>

2. Hook or instruction?
<details><summary>Answer</summary>

When something must happen regardless of the model.
</details>

3. Which exit code blocks?
<details><summary>Answer</summary>

2.
</details>

4. Can a hook allow what a rule denies?
<details><summary>Answer</summary>

No: `deny` still applies.
</details>

5. How do you rewrite a command?
<details><summary>Answer</summary>

`updatedInput` in the `PreToolUse` JSON.
</details>

6. What if a hook times out?
<details><summary>Answer</summary>

It is cancelled and that is not a block: the action proceeds.
</details>

7. How do hooks of different levels combine?
<details><summary>Answer</summary>

They add up; matching hooks run in parallel.
</details>

8. How do you disable hooks?
<details><summary>Answer</summary>

`disableAllHooks`; managed hooks cannot be disabled that way.
</details>

9. How do you stop the model deploying on its own?
<details><summary>Answer</summary>

`disable-model-invocation: true` on the skill plus permissions or hooks on the commands.
</details>

10. Why are `!` commands in a stranger's skill risky?
<details><summary>Answer</summary>

They run when the skill loads, before the model decides anything.
</details>

## Related materials

- [Advanced theory](advanced.en.md) · [Quiz](quiz.en.md)
- [10 · CLAUDE.md](../10-memory/expert.en.md)

## Sources

- [Hooks reference](https://code.claude.com/docs/en/hooks)
- [Skills](https://code.claude.com/docs/en/skills)
