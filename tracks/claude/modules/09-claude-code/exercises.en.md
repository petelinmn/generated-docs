---
title: "Claude Code: exercises"
lang: en
track: claude
kind: module
doc: exercises
slug: 09-claude-code
part: 2
tags: [claude, claude-code, permissions, headless, ci]
versions: "Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# Claude Code: exercises

🌐 [Русский](exercises.ru.md) · **English**

> Use a practice repository. You need Claude Code installed and signed in.

## 1. Guard secrets

Forbid reading `.env` for the whole team.

<details><summary>Hint</summary>

`permissions.deny` in the project file.

</details>

<details><summary>Solution</summary>

```json
{"permissions": {"deny": ["Read(./.env)"]}}
```

</details>

## 2. Tests only

Run non-interactively so that the only command allowed is `npm test`.

<details><summary>Hint</summary>

`--allowedTools` and a mode that refuses the rest.

</details>

<details><summary>Solution</summary>

```bash
claude -p "Run the tests and summarise failures" --allowedTools "Bash(npm test)" --permission-mode dontAsk
```

</details>

## 3. Checking in a script

Make the script fail if anything was refused.

<details><summary>Hint</summary>

Parse the JSON.

</details>

<details><summary>Solution</summary>

```bash
claude -p "Fix the lint errors" --output-format json > out.json
python -c "import json,sys; d=json.load(open('out.json')); sys.exit(1 if d['is_error'] or d['permission_denials'] else 0)"
```

</details>

## 4. Continue a session

Make two runs where the second remembers the first.

<details><summary>Hint</summary>

`session_id` and `--resume`.

</details>

<details><summary>Solution</summary>

```bash
SID=$(claude -p "Remember the number 7" --output-format json | python -c "import json,sys; print(json.load(sys.stdin)['session_id'])")
claude -p "Which number?" --resume "$SID"
```

</details>

## 5. A structured answer

Get an object with `files` (number) and `language` (string).

<details><summary>Hint</summary>

`--json-schema`.

</details>

<details><summary>Solution</summary>

```bash
claude -p "Describe this repository" --output-format json --json-schema '{"type":"object","properties":{"files":{"type":"integer"},"language":{"type":"string"}},"required":["files","language"]}'
# the object is in the structured_output field
```

</details>
