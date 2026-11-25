---
title: "Skills и hooks: упражнения"
lang: ru
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

# Skills и hooks: упражнения

🌐 **Русский** · [English](exercises.en.md)

> Выполняйте в учебном репозитории.

## 1. Skill для коммита

Skill, получающий `git diff --staged` и предлагающий сообщение коммита.

<details><summary>Подсказка</summary>

`` !`команда` ``.

</details>

<details><summary>Решение</summary>

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

## 2. Защита ветки

Блокируйте `git push` в `main`.

<details><summary>Подсказка</summary>

`PreToolUse`, код 2.

</details>

<details><summary>Решение</summary>

```python
import json
import sys

command = json.load(sys.stdin).get("tool_input", {}).get("command", "")
if "git push" in command and "main" in command:
    print("push to main is blocked; open a pull request", file=sys.stderr)
    sys.exit(2)
```

</details>

## 3. Проверка hook без Claude

Проверьте скрипт из упражнения 2 из командной строки.

<details><summary>Подсказка</summary>

Подайте JSON на stdin.

</details>

<details><summary>Решение</summary>

```bash
echo '{"tool_input":{"command":"git push origin main"}}' | python guard.py; echo $?      # 2
echo '{"tool_input":{"command":"git status"}}' | python guard.py; echo $?                # 0
```

</details>

## 4. Не зациклиться

`Stop`-hook, один раз требующий запустить тесты.

<details><summary>Подсказка</summary>

`stop_hook_active`.

</details>

<details><summary>Решение</summary>

```python
import json
import sys

if not json.load(sys.stdin).get("stop_hook_active"):
    print("run the tests before finishing", file=sys.stderr)
    sys.exit(2)
```

</details>

## 5. Контекст при старте

Добавляйте в контекст текущую ветку.

<details><summary>Подсказка</summary>

stdout на `SessionStart`.

</details>

<details><summary>Решение</summary>

```json
{"hooks": {"SessionStart": [{"hooks": [{"type": "command", "command": "echo Branch: $(git branch --show-current)"}]}]}}
```

</details>
