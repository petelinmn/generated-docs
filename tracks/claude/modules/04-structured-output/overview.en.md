---
title: "Structured outputs"
lang: en
track: claude
kind: module
doc: overview
slug: 04-structured-output
part: 1
levels: [1, 2, 3, 4]
tags: [claude, structured-outputs, json-schema, pydantic, strict-tool-use, output-config]
versions: "Claude 5.5 family; Messages API, anthropic-version 2026-10-21; Python SDK anthropic 2.13.0"
status: draft
updated: 2026-11-24
---

# Structured outputs

🌐 [Русский](overview.ru.md) · **English**

## In short

Structured outputs make Claude return a reply that matches a given JSON schema: instead of asking "return JSON" in the prompt you pass a schema in `output_config.format`, and the reply comes as valid JSON. In the Python SDK this is most convenient through `client.messages.parse(...)` with a Pydantic model: the SDK builds the schema, sends the request and returns a ready object in `parsed_output`. A separate capability is `strict: true` on a tool, which guarantees that the call's arguments match the tool's schema.

## Key ideas

- **The schema goes in a parameter, not in the text:** `output_config: {"format": {"type": "json_schema", "schema": {...}}}`.
- **`messages.parse` + Pydantic** is the recommended path in Python: `output_format=Model` → `response.parsed_output`.
- **With `messages.create` the JSON is not parsed for you:** it lies as text in a `text` block; you call `json.loads` and validate.
- **A schema has limitations:** `additionalProperties` must be `false`, no recursion, `oneOf`, numeric and string constraints (`minimum`, `maxLength`…); the SDK moves such constraints into the description.
- **The guarantee concerns the format, not the meaning:** values still need checking.
- **An incomplete or refused reply may not match the schema:** check `stop_reason` (`max_tokens`, `refusal`).
- **`strict: true` on a tool** constrains the call's arguments to the schema; JSON outputs control the response format, and the two can be combined.
- **The first send of a new schema is slower:** the schema is compiled and cached (up to 24 hours per the documentation).
- **Incompatible with citations and prefill.**

## Minimal example

```python
from pydantic import BaseModel
import anthropic

class ContactInfo(BaseModel):
    name: str
    email: str
    plan_interest: str
    demo_requested: bool

client = anthropic.Anthropic()
response = client.messages.parse(
    model="claude-opus-5-5",
    max_tokens=1024,
    messages=[{"role": "user", "content": "Extract: John Smith (john@example.com) wants Enterprise and a demo."}],
    output_format=ContactInfo,
)
print(response.parsed_output)
```

```text
name='John Smith' email='john@example.com' plan_interest='Enterprise' demo_requested=True
```

The output came from a local mock server (see "How the code was checked" in the [basic theory](basics.en.md)): schema building and parsing were checked, not the replies of real models.

## When to use and when not

| Task | Choice |
|---|---|
| Extracting data into a known structure | `messages.parse` + a Pydantic model |
| Classification with a fixed set of values | an `enum` in the schema |
| The model calls functions | tools (module 07 of the roadmap), `strict: true` |
| Free text | no schema |
| A schema with recursion or `oneOf` | simplify or validate yourself |
| Strict numeric ranges | validation on your side (Pydantic `Field(ge, le)`) |

- ❌ Do not rely on the text "return only JSON": a schema is more reliable.
- ❌ Do not consider valid JSON correct in meaning.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | `output_config.format`, `parse` and Pydantic, `create` and `json.loads`, checking `stop_reason` |
| [2. Advanced theory](advanced.en.md) | schema limitations and `transform_schema`, `enum` and `Optional`, `strict` on tools, the schema cache, retries |
| [3. Expert theory](expert.en.md) | a reference of supported keywords, errors, compatibility, pitfalls, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn claude 04-structured-output level 5` |
| 6. Tests | not created yet — `/learn claude 04-structured-output level 6` |

## Related materials

- Prerequisites: [02 · Messages API](../02-messages-api/overview.en.md), [03 · Prompt engineering](../03-prompt-engineering/overview.en.md)
- Next: [05 · Prompt caching](../05-prompt-caching/overview.en.md)
- Analogues in other tracks: [LLM](../../../llm/overview.en.md), [Python](../../../python/overview.en.md) (Pydantic), [TypeScript](../../../typescript/overview.en.md) (schemas and types)
