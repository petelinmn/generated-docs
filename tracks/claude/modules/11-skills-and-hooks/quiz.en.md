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

**C.** The program itself runs a hook on its event. A line in `CLAUDE.md`, a tool description or a skill only influences what the model decides.
→ [Fundamentals](basics.en.md)

</details>

## 2. 🟢 Where is the project skill `deploy`?

- A) `.claude/skills/deploy/SKILL.md`
- B) `.claude/deploy.skill`
- C) `skills/deploy.json`
- D) `.claude/hooks/deploy`

<details><summary>Answer</summary>

**A.** A skill is a directory with `SKILL.md` under `.claude/skills/`. Claude Code does not recognise the other paths as skills.
→ [Fundamentals](basics.en.md)

</details>

## 3. 🟢 Which exit code makes a hook block a tool call?

- A) 1
- B) 0
- C) -1
- D) 2

<details><summary>Answer</summary>

**D.** Exit code 2 blocks: the call is cancelled and the model gets stderr. 0 is success and other codes are non-blocking errors.
→ [Fundamentals](basics.en.md)

</details>

## 4. 🟢 What part of a skill always costs context?

- A) never anything
- B) its description in the list
- C) the whole body
- D) bundled files

<details><summary>Answer</summary>

**B.** Only the listing description stays in context so the model knows when to use the skill. The body is read on invocation, attached files when needed.
→ [Fundamentals](basics.en.md)

</details>

## 5. 🟡 How do you make a skill human-only?

- A) `allowed-tools: none`
- B) `disable-model-invocation: true`
- C) `user-invocable: false`
- D) `context: fork`

<details><summary>Answer</summary>

**B.** This field stops the model from invoking the skill. `user-invocable: false` does the opposite and hides it from people; `allowed-tools` and `context: fork` concern permissions and where it runs.
→ [Advanced theory](advanced.en.md)

</details>

## 6. 🟡 A hook returned `allow` while a rule says `deny`:

- A) allowed
- B) prompts
- C) a config error
- D) denied

<details><summary>Answer</summary>

**D.** A hook's `allow` does not override `deny`: deny rules always apply. It is neither a configuration error nor a reason to prompt.
→ [Advanced theory](advanced.en.md)

</details>

## 7. 🟡 `/greet "Ann Lee" x`. What is `$0`?

- A) `Ann Lee`
- B) `greet`
- C) `"Ann`
- D) `x`

<details><summary>Answer</summary>

**A.** Arguments are zero-based and quotes are honoured, so the first one is the whole `Ann Lee`. The command name is not an argument, and `x` is `$1`.
→ [Advanced theory](advanced.en.md)

</details>

## 8. 🟡 What is true of `UserPromptSubmit`?

- [ ] A) stdout replaces the prompt
- [ ] B) it runs after the reply
- [ ] C) stdout is added to context
- [ ] D) exit 2 rejects the prompt

<details><summary>Answer</summary>

**C, D.** For this event stdout on exit 0 is added to context, and exit 2 rejects the prompt itself. The prompt is not replaced, and the hook fires before processing.
→ [Advanced theory](advanced.en.md)

</details>

## 9. 🔴 A hook exceeded its `timeout`. The action:

- A) is blocked
- B) is retried
- C) ends the session
- D) proceeds as if not blocked

<details><summary>Answer</summary>

**D.** A timeout is a non-blocking failure, so the action proceeds. A guard hook must be fast and cannot rely on a hang to stop anything.
→ [Expert theory](expert.en.md)

</details>

## 10. 🔴 Why check `stop_hook_active` in a `Stop` hook?

- A) it is a required response field
- B) to avoid forcing continuation for ever
- C) to learn the model
- D) to return JSON

<details><summary>Answer</summary>

**B.** The flag says Claude is already continuing because of a `Stop` hook; without the check it can block stopping again and again. It does not identify the model and is not a required reply field.
→ [Expert theory](expert.en.md)

</details>

## 11. 🔴 How does exit 2 differ between `PreToolUse` and `PostToolUse`?

- A) the second reverts files
- B) the first is ignored
- C) the first prevents the action, the second only reports
- D) no difference

<details><summary>Answer</summary>

**C.** `PreToolUse` fires before the call and can prevent it; `PostToolUse` fires after the action has happened. It rolls nothing back.
→ [Expert theory](expert.en.md)

</details>

## 12. 🔴 Open question: "a formatter must run after every code edit, and deploys only on a human command". How do you set it up?

<details><summary>Answer</summary>

A `PostToolUse` hook matching `Edit|Write` that runs the formatter; a `deploy` skill with `disable-model-invocation: true`; an `ask`/`deny` rule or `PreToolUse` hook on the deploy commands so they cannot be run around the skill.
→ [Advanced theory](advanced.en.md)

</details>
