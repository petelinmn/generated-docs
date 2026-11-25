---
title: "Skills and hooks: exercises"
lang: en
track: claude
kind: module
doc: exercises
slug: 11-skills-and-hooks
part: 2
tags: [claude, claude-code, skills, hooks, automation]
versions: "Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# Skills and hooks: exercises

🌐 [Русский](exercises.ru.md) · **English**

> Use a practice repository.

## 1. A commit skill

A skill that reads `git diff --staged` and proposes a commit message.

<details><summary>Hint</summary>

`` !`command` ``.

</details>

<details><summary>Solution</summary>

```markdown
---
description: Propose a commit message for the staged changes.
disable-model-invocation: true
---

Staged changes:
!`git diff --staged`

Propose one commit message.
```

</details>

## 2. Protect a branch

Block `git push` to `main`.

<details><summary>Hint</summary>

`PreToolUse`, exit 2.

</details>

<details><summary>Solution</summary>

```python
import json
import sys

command = json.load(sys.stdin).get("tool_input", {}).get("command", "")
if "git push" in command and "main" in command:
    print("push to main is blocked; open a pull request", file=sys.stderr)
    sys.exit(2)
```

</details>

## 3. Test a hook without Claude

Test the script of exercise 2 from the command line.

<details><summary>Hint</summary>

Feed JSON to stdin.

</details>

<details><summary>Solution</summary>

```bash
echo '{"tool_input":{"command":"git push origin main"}}' | python guard.py; echo $?      # 2
echo '{"tool_input":{"command":"git status"}}' | python guard.py; echo $?                # 0
```

</details>

## 4. No loop

A `Stop` hook asking once to run the tests.

<details><summary>Hint</summary>

`stop_hook_active`.

</details>

<details><summary>Solution</summary>

```python
import json
import sys

if not json.load(sys.stdin).get("stop_hook_active"):
    print("run the tests before finishing", file=sys.stderr)
    sys.exit(2)
```

</details>

## 5. Context at start

Add the current branch to context.

<details><summary>Hint</summary>

stdout on `SessionStart`.

</details>

<details><summary>Solution</summary>

```json
{"hooks": {"SessionStart": [{"hooks": [{"type": "command", "command": "echo Branch: $(git branch --show-current)"}]}]}}
```

</details>
