---
title: "CLAUDE.md and memory: exercises"
lang: en
track: claude
kind: module
doc: exercises
slug: 10-memory
part: 2
tags: [claude, claude-code, claude-md, memory, context]
versions: "Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# CLAUDE.md and memory: exercises

🌐 [Русский](exercises.ru.md) · **English**

> You need a practice repository and Claude Code installed.

## 1. A short CLAUDE.md

Write a file with the test command and two conventions.

<details><summary>Hint</summary>

Concrete and checkable.

</details>

<details><summary>Solution</summary>

```markdown
# Project

- Tests: `pytest -q`.
- Public functions have type hints.
- Do not edit files under `vendor/`.
```

</details>

## 2. A path rule

A rule only for tests.

<details><summary>Hint</summary>

`paths` in the front matter.

</details>

<details><summary>Solution</summary>

```markdown
---
paths:
  - "tests/**"
---

- One behaviour per test.
```

</details>

## 3. Share with other agents

You have `AGENTS.md`; add one instruction just for Claude.

<details><summary>Hint</summary>

An import.

</details>

<details><summary>Solution</summary>

```markdown
@AGENTS.md

## Claude Code

- Use plan mode for changes under `src/billing/`.
```

</details>

## 4. A note for humans

Leave a maintainer note that costs no context.

<details><summary>Hint</summary>

An HTML comment.

</details>

<details><summary>Solution</summary>

```markdown
<!-- Owner: platform team. Review quarterly. -->
```

</details>

## 5. Diagnosing

Claude does not see your file. How do you find out why?

<details><summary>Hint</summary>

Two commands and one flag.

</details>

<details><summary>Solution</summary>

`/memory` and `/context` show loaded files; `claude --debug-file debug.log` and a search for `CLAUDE.md` in the log show why one was skipped. Check the launch directory, `claudeMdExcludes` and `--setting-sources`.

</details>
