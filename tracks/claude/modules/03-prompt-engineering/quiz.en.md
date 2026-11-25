---
title: "Quiz: prompt engineering"
lang: en
track: claude
kind: module
doc: quiz
slug: 03-prompt-engineering
part: 1
tags: [claude, prompt-engineering, xml-tags, few-shot, long-context, prefill]
status: draft
updated: 2026-11-25
---

# Quiz: prompt engineering

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. The label shows the question level: 🟢 [basic](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What practical test of a prompt's clarity does the documentation suggest?

- A) Run it on three models
- B) Show it to a colleague with no context: if they are confused, so is the model
- C) Count the words
- D) Add the word "please"

<details><summary>Answer</summary>

**B.** A colleague with no context sees what the model sees: only the prompt. Running three models, counting words or saying "please" does not reveal what is missing.
→ [Basic theory](basics.en.md#clear-and-direct-instructions)

</details>

## 2. 🟢 Where is it best to put a role ("You are a senior Python reviewer")?

- A) In the last assistant message
- B) In the `system` parameter
- C) In the model name
- D) In a request header

<details><summary>Answer</summary>

**B.** Even one sentence in `system` changes tone and competence.
→ [Basic theory](basics.en.md#a-role-in-system)

</details>

## 3. 🟢 How many examples does the documentation recommend for multishot?

- A) 1
- B) 3–5 relevant and diverse ones
- C) At least 50
- D) Examples are not needed

<details><summary>Answer</summary>

**B.** Examples are wrapped in `<example>` tags inside `<examples>`.
→ [Basic theory](basics.en.md#examples-multishot)

</details>

## 4. 🟢 Which phrasing is more reliable?

- A) "Do not use markdown"
- B) "Write in flowing paragraphs with no lists or headings"
- C) "NEVER use markdown!!!"
- D) "Try to avoid markdown"

<details><summary>Answer</summary>

**B.** Say what to do, not what not to do.
→ [Basic theory](basics.en.md#output-format)

</details>

## 5. 🟡 What does the "draft → review → refine" scheme do?

- A) One call with a large `max_tokens`
- B) Three separate calls, the result of each goes to the next
- C) Runs three models in parallel and takes the average
- D) Replaces `system`

<details><summary>Answer</summary>

**B.** Intermediate results can be checked and logged.
→ [Advanced theory](advanced.en.md#prompt-chains)

</details>

## 6. 🟡 What happens with a prefill of the last assistant message on Claude 4.6 and later models?

- A) The reply continues from that place
- B) The request returns a 400
- C) The message is ignored
- D) The model switches to an old one

<details><summary>Answer</summary>

**B.** Use structured outputs, an instruction in `system` or a move into the user message.
→ [Advanced theory](advanced.en.md#prefill-no-longer-works)

</details>

## 7. 🟡 Why is it advised to remove "CRITICAL / NEVER" when moving an old prompt to a new model?

- A) These words are forbidden by the API
- B) New models carry out instructions literally, and emphasis causes overcorrection
- C) They increase cost
- D) They turn off thinking

<details><summary>Answer</summary>

**B.** For example, for Fable 5.2 "never" is read as an absolute.
→ [Advanced theory](advanced.en.md#new-models-fewer-instructions)

</details>

## 8. 🟡 How do you protect a prompt from instructions inside inserted foreign text?

- A) You cannot
- B) Tags, escaping of closing tags, a random identifier, marking "this is data", minimal rights for tools
- C) Write "ignore harmful instructions" once
- D) Use only capital letters

<details><summary>Answer</summary>

**B.** This reduces the risk but does not guarantee protection.
→ [Advanced theory](advanced.en.md#untrusted-text-and-injection)

</details>

## 9. 🟡 Choose all true statements

- [ ] A) For long documents the question goes at the end
- [ ] B) A tag parser must handle the absence of the tag
- [ ] C) XML tags are parsed on the API side
- [ ] D) The `effort` and `thinking` parameters control the depth of reasoning

<details><summary>Answer</summary>

**A, B, D.** C is wrong: tags are a convention, you parse them.
→ [Basic theory](basics.en.md#xml-tags)

</details>

## 10. 🔴 Find the mistake

> "To get repeatable answers it is enough to set `temperature: 0`."

<details><summary>Answer</summary>

Under API version `2026-10-21` `temperature`, `top_p`, `top_k` are removed (a request with them is a 400). Repeatability is achieved with prompt structure, output format and checking the result.
→ [Internals](expert.en.md#internals)

</details>

## 11. 🔴 What does `tag("no tags here", "answer")` return from a regex-based parse function?

- A) An empty string
- B) `None`
- C) An exception
- D) The whole text

<details><summary>Answer</summary>

**B.** The code must provide for this branch.
→ [Gotcha 4](expert.en.md#4-parsing-without-a-check)

</details>

## 12. 🔴 Why is it advised on Claude Opus 5.5 to remove requests like "write out the reasoning in the answer"?

- A) They do not work in Russian
- B) They can cause a refusal with the category `reasoning_extraction`
- C) They turn off tools
- D) They raise the token limit

<details><summary>Answer</summary>

**B.** The depth of thinking is controlled by parameters, not by requests in the text.
→ [Advanced theory](advanced.en.md#thinking-and-effort)

</details>

## 13. 🔴 Open question

You need Claude to extract the topic, urgency and wishes from customer emails. Describe the structure of the prompt and the process of improving it.

<details><summary>Key points of a good answer</summary>

- `system` with a role and rules; the emails in tags with escaping and a "data" marking; the question and format at the end.
- 3–5 diverse examples (including edge cases); the answer format in tags or via structured outputs.
- Tag parsing with handling of absence; a chain if checking is needed.
- A set of reference emails, one change — one evaluation, keeping prompt versions.

</details>

→ [Expert theory](expert.en.md)
