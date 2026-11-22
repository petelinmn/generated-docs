---
title: "Quiz: Spectre.Console"
lang: en
track: csharp
kind: library
doc: quiz
slug: spectre-console
part: 2
tags: [spectre-console, console, markup, prompts]
status: draft
updated: 2026-11-22
---

# Quiz: Spectre.Console

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 How do you print red bold text?

- A) `Console.WriteLine("red bold")`
- B) `AnsiConsole.MarkupLine("[bold red]text[/]")`
- C) `AnsiConsole.Red("text")`
- D) `Console.ForegroundColor`

<details><summary>Answer</summary>

**B.** `[style]…[/]` tags in markup.
→ [Basics](basics.en.md#markup)

</details>

## 2. 🟢 How do you print literal square brackets?

- A) `\[ \]`
- B) double them: `[[` and `]]`
- C) you cannot
- D) `&#91;`

<details><summary>Answer</summary>

**B.** Verified: `[[not a tag]]` → `[not a tag]`.
→ [Basics](basics.en.md#markup)

</details>

## 3. 🟢 What does `Markup.Escape(input)` do?

- A) removes brackets
- B) escapes markup in user text
- C) makes the text red
- D) encrypts

<details><summary>Answer</summary>

**B.** Verified: `[red]x[/]` is printed literally.
→ [Basics](basics.en.md#markup)

</details>

## 4. 🟢 What happens with an unknown style `[unknown]`?

- A) the tag is ignored
- B) `InvalidOperationException`
- C) red text
- D) an empty string

<details><summary>Answer</summary>

**B.** Verified: "Could not find color or style 'unknown'".
→ [Basics](basics.en.md#markup)

</details>

## 5. 🟡 How do you test output and prompts?

- A) by intercepting `Console.Out`
- B) `TestConsole` from `Spectre.Console.Testing` and `IAnsiConsole` as a dependency
- C) `Process.Start`
- D) you cannot

<details><summary>Answer</summary>

**B.** `Output` and `Input.PushTextWithEnter/PushKey` (verified).
→ [Advanced](advanced.en.md#testing-output-testconsole)

</details>

## 6. 🟡 What is the result of `greet Ann --times 0` with a `Validate()` checking `Times < 1`?

- A) it runs once
- B) the error `times must be >= 1`
- C) silently nothing
- D) a `NullReferenceException`

<details><summary>Answer</summary>

**B.** Verified.
→ [Advanced](advanced.en.md#spectreconsolecli-commands-and-arguments)

</details>

## 7. 🟡 Select all correct statements

- [ ] A) `SelectionPrompt` returned `two` after `↓` and `Enter`
- [ ] B) prompts work in a non-interactive console
- [ ] C) `TextPrompt<int>` repeats the prompt on the input `abc`
- [ ] D) `Panel` and `Table` are `IRenderable`

<details><summary>Answer</summary>

**A, C, D.** B is wrong: verified — `InvalidOperationException`.
→ [Basics](basics.en.md#prompting-the-user)

</details>

## 8. 🟡 Open question: how do you build a CLI utility with testable output?

<details><summary>Key points of a good answer</summary>

- `Spectre.Console.Cli`: settings, commands, `Validate()`;
- `IAnsiConsole` injected into commands (not the static `AnsiConsole`);
- tests on `TestConsole` checking `Output`;
- exit codes, error handling.

</details>

## 9. 🔴 Why is `TestConsole.Output` empty although the command ran?

- A) the test console is broken
- B) the command wrote to the static `AnsiConsole`, not to the configured console
- C) a `Flush` is needed
- D) a `Width` is needed

<details><summary>Answer</summary>

**B.** Verified; inject `IAnsiConsole`.
→ [Expert](expert.en.md#edges-and-gotchas)

</details>

## 10. 🔴 Why does the same code give both colored output and plain text?

- A) two different compile modes
- B) the output is built from `IRenderable` segments and adapts to the terminal's capabilities (`Profile.Capabilities`)
- C) the user chooses by hand
- D) the colors are stored in a file

<details><summary>Answer</summary>

**B.** Verified on `NoColors`/`Ansi = false`.
→ [Expert](expert.en.md#internals)

</details>
