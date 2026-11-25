---
title: "CLAUDE.md and memory: quiz"
lang: en
track: claude
kind: module
doc: quiz
slug: 10-memory
part: 2
tags: [claude, claude-code, claude-md, memory, context]
versions: "Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# CLAUDE.md and memory: quiz

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 Who writes auto memory?

- A) an administrator
- B) git
- C) Claude
- D) you, at installation

<details><summary>Answer</summary>

**C.** Auto memory is the notes Claude writes for itself while working. Administrators handle managed settings, git writes nothing, and what you write by hand is `CLAUDE.md`.
→ [Fundamentals](basics.en.md)

</details>

## 2. 🟢 Which file should not go into git?

- A) `CLAUDE.local.md`
- B) `CLAUDE.md`
- C) `.claude/rules/general.md`
- D) `AGENTS.md`

<details><summary>Answer</summary>

**A.** `CLAUDE.local.md` holds one developer's private instructions, so it goes into `.gitignore`. The other three are shared with the team and committed.
→ [Fundamentals](basics.en.md)

</details>

## 3. 🟢 How do you reliably forbid reading `.env`?

- A) write it in `CLAUDE.md`
- B) add it to auto memory
- C) say it in the first message
- D) a `deny` permission rule

<details><summary>Answer</summary>

**D.** Only a permission rule is enforced by the program at tool call time. Text in `CLAUDE.md`, memory or a message is guidance to the model, not a guarantee.
→ [Fundamentals](basics.en.md)

</details>

## 4. 🟢 What is the size target for `CLAUDE.md`?

- A) no limit
- B) under 200 lines
- C) under 20 lines
- D) up to 25 MB

<details><summary>Answer</summary>

**B.** Aim for under 200 lines: a long file costs context and is followed less well. There is no need to squeeze into 20, and the 25 KB threshold applies to loading `MEMORY.md`, not to `CLAUDE.md`.
→ [Fundamentals](basics.en.md)

</details>

## 5. 🟡 When does a rule with `paths: ["**/*.py"]` load?

- A) necessarily after compaction
- B) when Claude opens a matching file
- C) always at start
- D) only via `/memory`

<details><summary>Answer</summary>

**B.** A rule with `paths` loads on demand when Claude works with a matching file. Rules without `paths` load at start; it is not guaranteed to return after compaction, and `/memory` is not needed for it.
→ [Advanced theory](advanced.en.md)

</details>

## 6. 🟡 What is true of imports?

- [ ] A) they do not fire inside backticks
- [ ] B) they cost no context
- [ ] C) paths resolve from the launch directory
- [ ] D) at most four hops

<details><summary>Answer</summary>

**A, D.** An `@path` inside code is plain text, and import depth is limited to four hops. Imported content costs context like any text, and relative paths resolve from the importing file.
→ [Advanced theory](advanced.en.md)

</details>

## 7. 🟡 A repository has `CLAUDE.md` and `AGENTS.md` (default settings):

- A) only `CLAUDE.md` is read
- B) both are read
- C) only `AGENTS.md`
- D) neither

<details><summary>Answer</summary>

**A.** By default `AGENTS.md` is read only when there is no `CLAUDE.md` or `CLAUDE.local.md`. Here `CLAUDE.md` exists, so `AGENTS.md` is skipped.
→ [Advanced theory](advanced.en.md)

</details>

## 8. 🟡 How much of `MEMORY.md` loads at start?

- A) the first 20 lines
- B) nothing
- C) 200 lines or 25 KB
- D) the whole file

<details><summary>Answer</summary>

**C.** A session loads the first 200 lines of `MEMORY.md` or 25 KB, whichever comes first. The rest is read when needed.
→ [Advanced theory](advanced.en.md)

</details>

## 9. 🔴 How does `CLAUDE.md` reach the request?

- A) it replaces the system prompt
- B) as a tool
- C) as an HTTP header
- D) as a user message

<details><summary>Answer</summary>

**D.** The content arrives as a message after the system prompt, so it is context rather than enforced configuration. It does not replace the system prompt and is neither a tool nor a header.
→ [Expert theory](expert.en.md)

</details>

## 10. 🔴 An HTML comment in `CLAUDE.md`:

- A) is executed
- B) is stripped before sending
- C) is sent as is
- D) causes an error

<details><summary>Answer</summary>

**B.** Block HTML comments are stripped before sending, which lets you leave notes for people at no token cost. They are not executed and raise no error.
→ [Expert theory](expert.en.md)

</details>

## 11. 🔴 What instruction files does `claude --bare` load?

- A) the project one only
- B) all
- C) none of them
- D) the user one only

<details><summary>Answer</summary>

**C.** `--bare` turns off automatic loading of instructions, project and user alike: nothing was loaded in the check.
→ [Expert theory](expert.en.md)

</details>

## 12. 🔴 Open question: `CLAUDE.md` grew to 900 lines and Claude misses rules. What do you do?

<details><summary>Answer</summary>

Delete the obvious and the stale, remove contradictions; move part-specific rules into `.claude/rules` with `paths`; rare procedures into skills; hard prohibitions into permissions and hooks; check `/context`.
→ [Advanced theory](advanced.en.md)

</details>
