---
title: "Claude Code: quiz"
lang: en
track: claude
kind: module
doc: quiz
slug: 09-claude-code
part: 2
tags: [claude, claude-code, permissions, headless, ci]
versions: "Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# Claude Code: quiz

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 How do you get one answer without an interactive session?

- A) `claude --bg`
- B) `claude mcp`
- C) `claude -p`
- D) `claude -c`

<details><summary>Answer</summary>

**C.**
→ [Fundamentals](basics.en.md)

</details>

## 2. 🟢 Which mode forbids file changes and is for planning?

- A) `plan`
- B) `acceptEdits`
- C) `bypassPermissions`
- D) `auto`

<details><summary>Answer</summary>

**A.**
→ [Fundamentals](basics.en.md)

</details>

## 3. 🟢 Which command creates `CLAUDE.md`?

- A) `/clear`
- B) `/context`
- C) `/model`
- D) `/init`

<details><summary>Answer</summary>

**D.**
→ [Fundamentals](basics.en.md)

</details>

## 4. 🟢 What does `Shift+Tab` cycle?

- A) themes
- B) permission modes
- C) models
- D) sessions

<details><summary>Answer</summary>

**B.**
→ [Fundamentals](basics.en.md)

</details>

## 5. 🟡 A command is in both `allow` and `deny`:

- A) depends on file order
- B) denied
- C) allowed
- D) prompts

<details><summary>Answer</summary>

**B.**
→ [Advanced theory](advanced.en.md)

</details>

## 6. 🟡 `claude -p` was refused `Write`. The exit code?

- A) 1
- B) 2
- C) the run hangs
- D) 0, with the refusal in `permission_denials`

<details><summary>Answer</summary>

**D.**
→ [Advanced theory](advanced.en.md)

</details>

## 7. 🟡 What overrides `.claude/settings.json`?

- [ ] A) `.claude/settings.local.json`
- [ ] B) a command-line flag
- [ ] C) `~/.claude/settings.json`
- [ ] D) `README.md`

<details><summary>Answer</summary>

**A, B.**
→ [Advanced theory](advanced.en.md)

</details>

## 8. 🟡 What will `/rewind` not restore?

- A) the conversation
- B) a Write change
- C) a file changed only by Bash commands
- D) an Edit change

<details><summary>Answer</summary>

**C.**
→ [Advanced theory](advanced.en.md)

</details>

## 9. 🔴 `Bash(touch *)` is allowed. The command `touch a && rm a`:

- A) runs entirely
- B) runs only `rm`
- C) is a syntax error
- D) needs permission for `rm`

<details><summary>Answer</summary>

**D.**
→ [Expert theory](expert.en.md)

</details>

## 10. 🔴 `--output-format stream-json` with `-p` requires:

- A) `--model`
- B) `--verbose`
- C) `--bare`
- D) `--continue`

<details><summary>Answer</summary>

**B.**
→ [Expert theory](expert.en.md)

</details>

## 11. 🔴 What does an exhausted `--max-turns 1` return?

- A) empty output
- B) a retry
- C) `error_max_turns` and code 1
- D) `success` and code 0

<details><summary>Answer</summary>

**C.**
→ [Expert theory](expert.en.md)

</details>

## 12. 🔴 Open question: set up a nightly run that fixes lint errors and opens a PR.

<details><summary>Answer</summary>

An isolated runner; `-p` with a precise task; allow only editing and the linter commands, `dontAsk`; `--max-turns` and a budget; a branch and PR instead of pushing to main; tests as a check; a human reviews; inspect `permission_denials` and the exit code.
→ [Advanced theory](advanced.en.md)

</details>
