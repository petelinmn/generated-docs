---
title: "Quiz: structured outputs"
lang: en
track: claude
kind: module
doc: quiz
slug: 04-structured-output
part: 1
tags: [claude, structured-outputs, json-schema, pydantic, strict-tool-use]
status: draft
updated: 2026-11-25
---

# Quiz: structured outputs

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. The label shows the question level: 🟢 [basic](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 In which parameter is the reply's JSON schema passed?

- A) A top-level `output_format`
- B) `output_config.format` with the type `json_schema`
- C) `system`
- D) `metadata`

<details><summary>Answer</summary>

**B.** The old `output_format` is deprecated.
→ [Basic theory](basics.en.md#the-schema-in-output_configformat)

</details>

## 2. 🟢 Where do you read the ready object from with `client.messages.parse(…, output_format=Model)`?

- A) `response.content[0].text`
- B) `response.parsed_output`
- C) `response.output`
- D) `response.json()`

<details><summary>Answer</summary>

**B.** `parse` returns a validated model instance in `parsed_output`. `content[0].text` is the raw JSON string you would parse yourself; `output` and `json()` are not the way to get it.
→ [Basic theory](basics.en.md#messagesparse-and-pydantic)

</details>

## 3. 🟢 What arrives with `messages.create` and a schema?

- A) A ready dictionary in `response.parsed_output`
- B) JSON text in a `text` block, which you must parse
- C) A file
- D) Nothing: you need `parse`

<details><summary>Answer</summary>

**B.** `json.loads(text)` and, better, validation in Pydantic.
→ [Basic theory](basics.en.md#messagescreate-and-jsonloads)

</details>

## 4. 🟢 What must be checked before parsing the JSON?

- A) `model`
- B) `stop_reason` (`max_tokens`, `refusal`)
- C) `id`
- D) Nothing

<details><summary>Answer</summary>

**B.** A truncated or refused reply may not match the schema.
→ [Basic theory](basics.en.md#check-stop_reason)

</details>

## 5. 🟡 What happens to `Field(ge=1, le=5)` in a Pydantic schema?

- A) The model cannot return 9
- B) The constraint goes into `description` as a hint; you must check yourself
- C) The request returns a 400
- D) The SDK removes the field

<details><summary>Answer</summary>

**B.** The schema does not support numeric bounds.
→ [Advanced theory](advanced.en.md#what-the-sdk-does-to-a-schema-transform_schema)

</details>

## 6. 🟡 How do you describe a "dictionary" with unknown keys?

- A) A `dict` field
- B) A list of `{key, value}` pairs
- C) A recursive schema
- D) `oneOf`

<details><summary>Answer</summary>

**B.** A `dict` field gives empty `properties`, and the model returns `{}`.
→ [Advanced theory](advanced.en.md#what-to-do-with-fields)

</details>

## 7. 🟡 What does `strict: true` on a tool guarantee?

- A) That the tool is always called
- B) That the call's arguments match `input_schema` and the tool name is valid
- C) That the reply will be JSON
- D) That the cache will not reset

<details><summary>Answer</summary>

**B.** This differs from JSON outputs, which constrain the reply format.
→ [Advanced theory](advanced.en.md#strict-tool-use)

</details>

## 8. 🟡 What resets the cache of a compiled schema?

- A) Changing `description`
- B) Changing the schema's structure
- C) A new request with the same fields
- D) Changing `max_tokens`

<details><summary>Answer</summary>

**B.** Editing only `name`/`description` does not reset it.
→ [Advanced theory](advanced.en.md#the-schema-cache-and-latency)

</details>

## 9. 🟡 Choose all true statements

- [ ] A) The schema does not support `oneOf` and recursion
- [ ] B) `additionalProperties` must be `false`
- [ ] C) Valid JSON guarantees the right meaning of the values
- [ ] D) Citations together with `output_config.format` give a 400

<details><summary>Answer</summary>

**A, B, D.** C is wrong: the form is guaranteed, not the meaning.
→ [Expert](expert.en.md#reference)

</details>

## 10. 🔴 Find the mistake

> "If the reply came with a 200 code, it can always be safely parsed against the schema."

<details><summary>Answer</summary>

No: `refusal` comes with a 200 code, and `max_tokens` gives truncated JSON. You must check `stop_reason`.
→ [Gotcha 5](expert.en.md#5-a-refusal-arrives-with-a-200-code)

</details>

## 11. 🔴 Why is `tags: dict` in a Pydantic model a bad idea for structured outputs?

- A) Pydantic does not support `dict`
- B) The schema gets empty `properties`, and the model can return only `{}`
- C) The API does not know the `object` type
- D) It slows decoding

<details><summary>Answer</summary>

**B.** In the experiment the result was `{"type": "object", "properties": {}, "additionalProperties": false}`.
→ [Gotcha 3](expert.en.md#3-a-dict-gives-an-empty-schema)

</details>

## 12. 🔴 How does the first request with a new schema differ from the later ones?

- A) Not at all
- B) It is slower: the schema is compiled into a grammar, then the result is cached
- C) It is cheaper
- D) It does not support streaming

<details><summary>Answer</summary>

**B.** Per the documentation the cache lasts up to 24 hours since last use.
→ [Internals](expert.en.md#internals)

</details>

## 13. 🔴 Open question

Design the extraction of fields from invoices (number, date, amount, line items): the schema, failure handling, validation.

<details><summary>Key points of a good answer</summary>

- A Pydantic model with a nested line-item model, `date` as a string with a format, the amount as a number; no `dict` and no recursion.
- `messages.parse`, headroom in `max_tokens`, checking `stop_reason`.
- Validation after parsing (ranges, the sum of line items = total), limited retries, handling of `refusal`.
- A stable schema for the cache; doubtful free text goes to a separate step.

</details>

→ [Advanced theory](advanced.en.md)
