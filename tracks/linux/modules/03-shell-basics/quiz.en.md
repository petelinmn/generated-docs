---
title: "Shell basics: quiz"
lang: en
track: linux
kind: module
doc: quiz
slug: 03-shell-basics
part: 1
tags: [linux, bash, shell, redirection, pipeline, quoting, exit-status]
versions: "Ubuntu 26.04 LTS, Bash 5.3, WSL 2"
status: draft
updated: 2026-11-25
---

# Shell basics: quiz

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What does exit status 0 mean?

- A) Command not found
- B) Command interrupted
- C) Success
- D) False

<details><summary>Answer</summary>

**C.** In the shell zero is success and anything else failure. "Not found" is 127.
→ [Fundamentals](basics.en.md)

</details>

## 2. 🟢 What does `echo 'hi $USER'` print?

- A) `hi $USER`
- B) `hi ` and the user name
- C) An empty line
- D) An error

<details><summary>Answer</summary>

**A.** Nothing expands in single quotes; the name would appear with double quotes.
→ [Fundamentals](basics.en.md)

</details>

## 3. 🟢 Which stream goes into `|`?

- A) stdout and stderr
- B) stderr only
- C) stdin
- D) stdout only

<details><summary>Answer</summary>

**D.** For both use `2>&1 |` or `|&`.
→ [Fundamentals](basics.en.md)

</details>

## 4. 🟢 How do you find out whether a command is a builtin?

- A) `echo $?`
- B) `type name`
- C) `man -f name`
- D) `ls name`

<details><summary>Answer</summary>

**B.** `type` knows about aliases, functions and builtins. `man -f` looks up manual pages.
→ [Fundamentals](basics.en.md)

</details>

## 5. 🟢 When does `b` run in `a || b`?

- A) In parallel with `a`
- B) When `a` fails
- C) When `a` succeeds
- D) Always

<details><summary>Answer</summary>

**B.** `&&` runs after success, `||` after failure.
→ [Fundamentals](basics.en.md)

</details>

## 6. 🟡 Where do errors go in `cmd 2>&1 > f`?

- A) Into `f`
- B) Into `/dev/null`
- C) They are lost
- D) Where stdout pointed before (usually the screen)

<details><summary>Answer</summary>

**D.** Redirections apply left to right.
→ [Advanced theory](advanced.en.md)

</details>

## 7. 🟡 Why does `sudo echo x > /etc/f` fail?

- A) The shell opens the file without root rights
- B) `sudo` cannot run `echo`
- C) `/etc` is read-only
- D) Quoting requires `sudo -s`

<details><summary>Answer</summary>

**A.** Fix: `echo x | sudo tee /etc/f`.
→ [Advanced theory](advanced.en.md)

</details>

## 8. 🟡 What does `B=2 true; echo "[$B]"` print (B unset before)?

- A) `[$B]`
- B) An error
- C) `[]`
- D) `[2]`

<details><summary>Answer</summary>

**C.** An assignment before a command affects only that command's environment.
→ [Advanced theory](advanced.en.md)

</details>

## 9. 🔴 What does `x=3; echo {1..$x}` print?

- A) `1 2 3`
- B) `{1..$x}`
- C) Nothing
- D) `{1..3}`

<details><summary>Answer</summary>

**D.** Brace expansion happens before variables: at that point it is not a numeric range.
→ [Expert theory](expert.en.md)

</details>

## 10. 🔴 A counter is incremented inside `cmd | while read ...`. What is it after the loop in a script with no special options?

- A) An error
- B) The old value: the loop ran in a subshell
- C) The line count
- D) Always 0, even if it was 5 before

<details><summary>Answer</summary>

**B.** Changes in a subshell don't come back. Use `done < <(cmd)`.
→ [Expert theory](expert.en.md)

</details>

## 11. 🟡 Select the true statements about `*`.

- [ ] A) Also expands inside double quotes
- [ ] B) Always yields an empty list when nothing matches
- [ ] C) Expanded by the shell before the command starts
- [ ] D) Skips hidden files by default

<details><summary>Answer</summary>

**C, D.** In quotes the pattern is literal, and unmatched it stays as typed (unless `nullglob` is on).
→ [Advanced theory](advanced.en.md)

</details>

## 12. 🔴 Open question: explain step by step what the shell does with `grep -c "$pat" *.log > report 2>&1`.

<details><summary>Answer</summary>

Key points: tokenising with quotes; substituting `$pat` without splitting; expanding `*.log` into names; stdout to `report`, then stderr to the same place; finding `grep` in `PATH` and running it; the status lands in `$?`.
→ [Expert theory](expert.en.md)

</details>
