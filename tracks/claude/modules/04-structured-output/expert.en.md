---
title: "Structured outputs: expert"
lang: en
track: claude
kind: module
doc: expert
slug: 04-structured-output
part: 1
tags: [claude, structured-outputs, json-schema, constrained-decoding, strict-tool-use, pydantic, schema-compilation]
versions: "Claude 5.5 family; Messages API, anthropic-version 2026-10-21; Python SDK anthropic 2.13.0"
status: draft
updated: 2026-11-24
---

# Structured outputs: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on the [basic](basics.en.md) and [advanced](advanced.en.md) theory. The reference data comes from the documentation pages "Structured outputs" and "Strict tool use"; the structured outputs page was read only partly (the rest is a summary), so table rows marked as summaries must be checked against the live page. What was run is marked "in the experiment" (a mock server, SDK 2.13.0).

## Reference

### Ways to get a structure

| Way | Parameter | Result | Where to read |
|---|---|---|---|
| JSON outputs (manual) | `output_config={"format": {"type": "json_schema", "schema": …}}` in `messages.create` | JSON text in a `text` block | `json.loads(text)` |
| JSON outputs (helper) | `client.messages.parse(…, output_format=Model)` | a model object | `response.parsed_output` |
| Strict tool use | `strict: true` on a tool | a `tool_use` with `input` per the schema | `block.input` |
| Deprecated | the `output_format` parameter | required the beta header `structured-outputs-2025-11-13`; a dictionary in `output_format` in SDK v1+ gives a `TypeError` | do not use |

### Schema keywords

| Status | Keywords |
|---|---|
| Supported | types (`string`, `number`, `integer`, `boolean`, `object`, `array`, `null`), `enum`, `const`, `anyOf`, `allOf` (with exceptions), `$ref`/`$defs`/`definitions`, `default`, `required`, `additionalProperties: false`, string formats (`date-time`, `time`, `date`, `duration`, `email`, `hostname`, `uri`, `ipv4`, `ipv6`, `uuid`), `minItems` 0 or 1, a limited subset of `pattern` |
| Not supported (400) | recursive schemas, `oneOf`, `not`, `anyOf` together with `type`/`properties`, external `$ref`, `minimum`/`maximum`/`multipleOf`, `minLength`/`maxLength`, arrays with constraints beyond `minItems` 0/1, `additionalProperties` other than `false`, in `pattern` — backreferences, lookaround, `\b`/`\B` |

Required properties come first in the reply; `enum`/`const` values may differ in case. (This table comes from an abridged summary of the page.)

### Behavior when something goes wrong

| Situation | Result |
|---|---|
| `stop_reason: "max_tokens"` | the output is incomplete and does not match the schema (in the experiment — truncated JSON → `JSONDecodeError`) |
| `stop_reason: "refusal"` | HTTP 200, tokens are billed, the output may not match the schema; a schema field asking for "step-by-step reasoning" may cause a `reasoning_extraction` refusal |
| An unsupported keyword | 400 |
| A schema that is too complex | 400 `Schema is too complex for compilation`; compilation is limited to about 180 seconds |
| Citations (`citations`) together with `output_config.format` | 400 |
| Prefilling the reply | incompatible |
| `computer_toolset_20260801`, `browser_toolset_20260801` with `strict: true` | rejected |

### Strict tool use limits

In one request — up to 20 strict tools, up to 24 optional parameters and up to 16 parameters with a union of types (from a summary of the page; check against the live page).

### Support

Claude 5.5 models (`claude-opus-5-5`, `claude-sonnet-5-5`, `claude-haiku-5-5`) and a number of previous ones, on the Claude API and cloud platforms (Bedrock, Google Cloud, Foundry); the endpoint is `/v1/messages`. Managed Agents are not supported. The full list is in the documentation page's metadata.

## Internals

- **Constrained decoding.** The schema is compiled into a grammar, and token generation is constrained so the result matches it. So the guarantee concerns the **syntax and structure** (types, required fields, `enum`), not the meaning of the values.
- **Compilation and the cache.** The first send of a new schema is slower because of compilation; the result is cached for up to 24 hours since last use. A structural change of the schema resets the cache, renaming and editing descriptions does not; changing the tool set also resets it.
- **What the SDK does.** `messages.parse` builds the schema from Pydantic through `transform_schema`, sends it in `output_config.format`, then validates the reply and puts the object into `parsed_output` (in the experiment there was no `output_format` field in the request body). Constraints missing from the schema the SDK moves into `description`; their checking is done on your side at parse time.
- **Order and kinds of reply.** The result is an ordinary `text` block in `content`: reasoning (`thinking`) and structure are separate; with strict tool use the structure arrives as a `tool_use` block.
- **Refusals outside the schema.** Safety classifiers can return `refusal` regardless of the schema, so code always checks `stop_reason`.

> ⚠️ Verify: the details of the grammar and the behavior of constrained decoding are inferred from the documentation and were not checked in experiments on a real model.

## Gotchas

### 1. Valid JSON ≠ a correct answer

A schema does not guarantee meaning: a name may be made up.

### 2. Pydantic constraints do not reach the model

`Field(ge=1, le=5)` turned into a hint in `description`; checking happens only at parse time (in the experiment `less_than_equal`).

### 3. A `dict` gives an empty schema

The model will return `{}` (in the experiment `properties: {}`).

### 4. Truncation by `max_tokens`

Incomplete JSON does not match the schema (in the experiment `JSONDecodeError`).

### 5. A refusal arrives with a 200 code

Do not try to parse a reply without checking `stop_reason`.

### 6. Citations are incompatible

`citations` enabled together with `output_config.format` → 400.

### 7. Changing the schema resets the cache

Change the schema's structure rarely; do not generate a schema "on the fly" for every request.

### 8. The case of an `enum`

A value may arrive in a different case: normalize when comparing.

### 9. The deprecated `output_format`

A dictionary in `output_format` in new SDKs is an error; use `output_config.format`.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| JSON outputs | a format guarantee | schema limitations, the first delay |
| `strict: true` on tools | correct arguments | limits on the number of tools |
| A request in the prompt + parsing | works everywhere | no guarantee, retries |
| XML tags + `re` | simple | brittle to variations |
| Libraries (Instructor etc.) | convenient wrappers | a dependency; not run |
| A two-step scheme (free text → extraction) | better for complex reasoning | two calls |

## Interview questions

<details><summary>1. How are structured outputs better than asking "return JSON"?</summary>

The schema constrains decoding: the format is guaranteed, not "probable"; no retries due to explanatory text.

→ [Basic theory](basics.en.md#the-schema-in-output_configformat)

</details>

<details><summary>2. How do you get a Pydantic object from a reply?</summary>

`client.messages.parse(…, output_format=Model)` and `response.parsed_output`.

→ [Basic theory](basics.en.md#messagesparse-and-pydantic)

</details>

<details><summary>3. Trick: why does <code>Field(ge=1, le=5)</code> not stop the model from returning 9?</summary>

Numeric bounds are not supported by the schema: the SDK puts them in `description`; checking is on your side.

→ [Gotcha 2](#2-pydantic-constraints-do-not-reach-the-model)

</details>

<details><summary>4. What does <code>strict: true</code> on a tool do?</summary>

The call's arguments strictly match `input_schema`, the tool name is valid.

→ [Advanced theory](advanced.en.md#strict-tool-use)

</details>

<details><summary>5. Trick: the reply came with a 200 code but the JSON does not parse. Causes?</summary>

`max_tokens` (truncation) or `refusal`; check `stop_reason`.

→ [Basic theory](basics.en.md#check-stop_reason)

</details>

<details><summary>6. Which schema keywords are not supported?</summary>

Recursion, `oneOf`, `not`, numeric and string constraints (`minimum`, `maxLength`…) and others.

→ [Reference](#schema-keywords)

</details>

<details><summary>7. Why is the first request with a new schema slower?</summary>

The schema is compiled into a grammar; the result is cached.

→ [Advanced theory](advanced.en.md#the-schema-cache-and-latency)

</details>

<details><summary>8. How do you describe a "dictionary" with arbitrary keys?</summary>

With a list of `{key, value}` pairs: a `dict` field gives empty `properties`.

→ [Gotcha 3](#3-a-dict-gives-an-empty-schema)

</details>

<details><summary>9. How do JSON outputs differ from strict tool use?</summary>

The first constrains the reply format (a `text` block), the second the call's arguments (a `tool_use` block); they can be combined.

→ [Advanced theory](advanced.en.md#strict-tool-use)

</details>

<details><summary>10. What are structured outputs incompatible with?</summary>

Citations (400) and prefill; Managed Agents are not supported.

→ [Reference](#behavior-when-something-goes-wrong)

</details>

<details><summary>11. Trick: the SDK added <code>additionalProperties: false</code> to the schema. Why?</summary>

The API requires `additionalProperties: false` for objects; `transform_schema` brings the schema to an acceptable form.

→ [Advanced theory](advanced.en.md#what-the-sdk-does-to-a-schema-transform_schema)

</details>

<details><summary>12. What do you do about a refusal (<code>refusal</code>)?</summary>

Do not parse the reply as a structure; change the request or show the user a message; a retry is useless.

→ [Gotcha 5](#5-a-refusal-arrives-with-a-200-code)

</details>

## Cheat sheet

```text
parse:  client.messages.parse(model=…, max_tokens=…, messages=[…], output_format=Model) → r.parsed_output
create: output_config={"format": {"type": "json_schema", "schema": {…, "additionalProperties": False}}} → json.loads(text)
tool:   {"name", "description", "strict": True, "input_schema": {…}}
schema: no recursion/oneOf/minimum/maxLength; dict → a list of pairs; enum — compare case-insensitively
check stop_reason (max_tokens, refusal) and the values after parsing
```

- A schema guarantees the form, not the meaning.
- Pydantic constraints are a hint, check them yourself.
- Citations are incompatible.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Request parameters and `stop_reason`: [02 · Messages API](../02-messages-api/expert.en.md)
- Format via the prompt: [03 · Prompt engineering](../03-prompt-engineering/expert.en.md)

## Sources

- [Structured outputs](https://platform.claude.com/docs/en/build-with-claude/structured-outputs)
- [Strict tool use](https://platform.claude.com/docs/en/agents-and-tools/tool-use/strict-tool-use)
- [API errors](https://platform.claude.com/docs/en/api/errors)
