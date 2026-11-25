---
title: "Skills and hooks: fundamentals"
lang: en
track: claude
kind: module
doc: basics
slug: 11-skills-and-hooks
part: 2
tags: [claude, claude-code, skills, hooks, automation]
versions: "Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# Skills and hooks: fundamentals

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [09 · Claude Code](../09-claude-code/overview.en.md), [10 · CLAUDE.md](../10-memory/overview.en.md)

## How this was checked

Skills, hooks and their exit codes ran in Claude Code 2.1.332 in a separate directory; a local stand-in answered instead of the API and requested tools by script. So the program's behaviour was checked, not when a real model decides to use a skill.

## Two ways to extend Claude Code

| | Skill | Hook |
|---|---|---|
| What it is | instructions the model reads | your command the program runs |
| When it runs | when the model decides, or on `/name` | always, on its event |
| For | repeatable procedures and knowledge | checks, prohibitions, automation |

## A first skill

The file `.claude/skills/greet/SKILL.md` (`~/.claude/skills/` for all projects):

```markdown
---
description: Greet a person by name. Use when the user asks for a greeting.
---

Greet $0 politely. Mention the current branch: !`git branch --show-current`
```

- The directory name becomes the command: `/greet Ann`.
- `description` goes into the list of available skills: the model decides by it.
- The body loads only on invocation, so a long procedure does not sit in context.
- `$0` is the first argument; a `` !`command` `` line is replaced by the command's output before the model sees it.

## A first hook

Let us forbid `rm -rf` whatever the model decides. `.claude/settings.json`:

```json
{
  "hooks": {
    "PreToolUse": [
      {
        "matcher": "Bash",
        "hooks": [{"type": "command", "command": "python \"$CLAUDE_PROJECT_DIR/.claude/hooks/guard.py\""}]
      }
    ]
  }
}
```

```python
# .claude/hooks/guard.py
import json
import sys

data = json.load(sys.stdin)
command = data.get("tool_input", {}).get("command", "")

if "rm -rf" in command:
    print("rm -rf is blocked in this project; delete specific files instead", file=sys.stderr)
    sys.exit(2)          # 2 blocks the call; stderr goes to Claude
```

The hook receives JSON on stdin: the event, the tool name and its arguments. Exit code 2 blocks the call and the model sees the stderr text.

This guards against accidents, it is not a security boundary: the same operation can be written differently.

## Common beginner mistakes

- Exiting with 1 to forbid: that does not block.
- A vague skill `description`.
- Running a stranger's hooks and skills unread: it is code with your rights.

## Related materials

- [Advanced theory](advanced.en.md)
- [09 · Claude Code](../09-claude-code/advanced.en.md)

## Sources

- [Skills](https://code.claude.com/docs/en/skills)
- [Hooks guide](https://code.claude.com/docs/en/hooks-guide)
