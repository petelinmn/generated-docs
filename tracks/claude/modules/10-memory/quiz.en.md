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

**C.**
→ [Fundamentals](basics.en.md)

</details>

## 2. 🟢 Which file should not go into git?

- A) `CLAUDE.local.md`
- B) `CLAUDE.md`
- C) `.claude/rules/general.md`
- D) `AGENTS.md`

<details><summary>Answer</summary>

**A.**
→ [Fundamentals](basics.en.md)

</details>

## 3. 🟢 How do you reliably forbid reading `.env`?

- A) write it in `CLAUDE.md`
- B) add it to auto memory
- C) say it in the first message
- D) a `deny` permission rule

<details><summary>Answer</summary>

**D.**
→ [Fundamentals](basics.en.md)

</details>

## 4. 🟢 What is the size target for `CLAUDE.md`?

- A) no limit
- B) under 200 lines
- C) under 20 lines
- D) up to 25 MB

<details><summary>Answer</summary>

**B.**
→ [Fundamentals](basics.en.md)

</details>

## 5. 🟡 When does a rule with `paths: ["**/*.py"]` load?

- A) necessarily after compaction
- B) when Claude opens a matching file
- C) always at start
- D) only via `/memory`

<details><summary>Answer</summary>

**B.**
→ [Advanced theory](advanced.en.md)

</details>

## 6. 🟡 What is true of imports?

- [ ] A) they do not fire inside backticks
- [ ] B) they cost no context
- [ ] C) paths resolve from the launch directory
- [ ] D) at most four hops

<details><summary>Answer</summary>

**A, D.**
→ [Advanced theory](advanced.en.md)

</details>

## 7. 🟡 A repository has `CLAUDE.md` and `AGENTS.md` (default settings):

- A) only `CLAUDE.md` is read
- B) both are read
- C) only `AGENTS.md`
- D) neither

<details><summary>Answer</summary>

**A.**
→ [Advanced theory](advanced.en.md)

</details>

## 8. 🟡 How much of `MEMORY.md` loads at start?

- A) the first 20 lines
- B) nothing
- C) 200 lines or 25 KB
- D) the whole file

<details><summary>Answer</summary>

**C.**
→ [Advanced theory](advanced.en.md)

</details>

## 9. 🔴 How does `CLAUDE.md` reach the request?

- A) it replaces the system prompt
- B) as a tool
- C) as an HTTP header
- D) as a user message

<details><summary>Answer</summary>

**D.**
→ [Expert theory](expert.en.md)

</details>

## 10. 🔴 An HTML comment in `CLAUDE.md`:

- A) is executed
- B) is stripped before sending
- C) is sent as is
- D) causes an error

<details><summary>Answer</summary>

**B.**
→ [Expert theory](expert.en.md)

</details>

## 11. 🔴 What instruction files does `claude --bare` load?

- A) the project one only
- B) all
- C) none of them
- D) the user one only

<details><summary>Answer</summary>

**C.**
→ [Expert theory](expert.en.md)

</details>

## 12. 🔴 Open question: `CLAUDE.md` grew to 900 lines and Claude misses rules. What do you do?

<details><summary>Answer</summary>

Delete the obvious and the stale, remove contradictions; move part-specific rules into `.claude/rules` with `paths`; rare procedures into skills; hard prohibitions into permissions and hooks; check `/context`.
→ [Advanced theory](advanced.en.md)

</details>
