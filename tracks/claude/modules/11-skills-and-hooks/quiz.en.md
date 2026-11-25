---
title: "Skills and hooks: quiz"
lang: en
track: claude
kind: module
doc: quiz
slug: 11-skills-and-hooks
part: 2
tags: [claude, claude-code, skills, hooks, automation]
versions: "Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# Skills and hooks: quiz

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What always runs regardless of the model's choice?

- A) a CLAUDE.md line
- B) a tool description
- C) a hook
- D) a skill

<details><summary>Answer</summary>

**C.**
→ [Fundamentals](basics.en.md)

</details>

## 2. 🟢 Where is the project skill `deploy`?

- A) `.claude/skills/deploy/SKILL.md`
- B) `.claude/deploy.skill`
- C) `skills/deploy.json`
- D) `.claude/hooks/deploy`

<details><summary>Answer</summary>

**A.**
→ [Fundamentals](basics.en.md)

</details>

## 3. 🟢 Which exit code makes a hook block a tool call?

- A) 1
- B) 0
- C) -1
- D) 2

<details><summary>Answer</summary>

**D.**
→ [Fundamentals](basics.en.md)

</details>

## 4. 🟢 What part of a skill always costs context?

- A) never anything
- B) its description in the list
- C) the whole body
- D) bundled files

<details><summary>Answer</summary>

**B.**
→ [Fundamentals](basics.en.md)

</details>

## 5. 🟡 How do you make a skill human-only?

- A) `allowed-tools: none`
- B) `disable-model-invocation: true`
- C) `user-invocable: false`
- D) `context: fork`

<details><summary>Answer</summary>

**B.**
→ [Advanced theory](advanced.en.md)

</details>

## 6. 🟡 A hook returned `allow` while a rule says `deny`:

- A) allowed
- B) prompts
- C) a config error
- D) denied

<details><summary>Answer</summary>

**D.**
→ [Advanced theory](advanced.en.md)

</details>

## 7. 🟡 `/greet "Ann Lee" x`. What is `$0`?

- A) `Ann Lee`
- B) `greet`
- C) `"Ann`
- D) `x`

<details><summary>Answer</summary>

**A.**
→ [Advanced theory](advanced.en.md)

</details>

## 8. 🟡 What is true of `UserPromptSubmit`?

- [ ] A) stdout replaces the prompt
- [ ] B) it runs after the reply
- [ ] C) stdout is added to context
- [ ] D) exit 2 rejects the prompt

<details><summary>Answer</summary>

**C, D.**
→ [Advanced theory](advanced.en.md)

</details>

## 9. 🔴 A hook exceeded its `timeout`. The action:

- A) is blocked
- B) is retried
- C) ends the session
- D) proceeds as if not blocked

<details><summary>Answer</summary>

**D.**
→ [Expert theory](expert.en.md)

</details>

## 10. 🔴 Why check `stop_hook_active` in a `Stop` hook?

- A) it is a required response field
- B) to avoid forcing continuation for ever
- C) to learn the model
- D) to return JSON

<details><summary>Answer</summary>

**B.**
→ [Expert theory](expert.en.md)

</details>

## 11. 🔴 How does exit 2 differ between `PreToolUse` and `PostToolUse`?

- A) the second reverts files
- B) the first is ignored
- C) the first prevents the action, the second only reports
- D) no difference

<details><summary>Answer</summary>

**C.**
→ [Expert theory](expert.en.md)

</details>

## 12. 🔴 Open question: "a formatter must run after every code edit, and deploys only on a human command". How do you set it up?

<details><summary>Answer</summary>

A `PostToolUse` hook matching `Edit|Write` that runs the formatter; a `deploy` skill with `disable-model-invocation: true`; an `ask`/`deny` rule or `PreToolUse` hook on the deploy commands so they cannot be run around the skill.
→ [Advanced theory](advanced.en.md)

</details>
