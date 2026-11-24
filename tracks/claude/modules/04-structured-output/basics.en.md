---
title: "Structured outputs: basics"
lang: en
track: claude
kind: module
doc: basics
slug: 04-structured-output
part: 1
tags: [claude, structured-outputs, json-schema, pydantic, output-config, stop-reason]
versions: "Claude 5.5 family; Messages API, anthropic-version 2026-10-21; Python SDK anthropic 2.13.0"
status: draft
updated: 2026-11-24
---

# Structured outputs: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [02 · Messages API](../02-messages-api/overview.en.md), [Python](../../../python/overview.en.md) · **Time:** ~1 h

## How the code was checked

There was no API key during preparation, so the code was run against a **local mock server** (as in [module 02](../02-messages-api/basics.en.md)): it accepts requests from the `anthropic` 2.13.0 SDK, saves the body and returns JSON that matches the schema. What was checked is what the SDK does (building and transforming the schema, the request shape, parsing into Pydantic) and your code; **the replies of real models and the real constrained decoding were not checked**. The facts about schemas and limitations come from the documentation pages "Structured outputs" and "Strict tool use"; part of these pages was read in an abridged summary.

## Why it matters

Asking "return JSON" in a prompt does not always work: the reply may contain an explanation, extra fields or broken JSON. Structured outputs move the requirement from the text into a request parameter and give a reply of the given form that code can parse at once.

## The schema in output_config.format

The reply format is set by the `output_config.format` parameter with the type `json_schema` and the schema itself:

```json
"output_config": {"format": {"type": "json_schema", "schema": {"type": "object", "properties": {...}, "required": [...], "additionalProperties": false}}}
```

The old `output_format` parameter is deprecated: use `output_config.format` (a beta header is no longer needed).

## messages.parse and Pydantic

In the Python SDK it is most convenient to describe the structure with a Pydantic model and call `client.messages.parse(...)`:

```python
from pydantic import BaseModel
import anthropic

class ContactInfo(BaseModel):
    name: str
    email: str
    plan_interest: str
    demo_requested: bool

client = anthropic.Anthropic()
r = client.messages.parse(
    model="claude-opus-5-5", max_tokens=1024,
    messages=[{"role": "user", "content": "Extract: John Smith (john@example.com) wants Enterprise and a demo."}],
    output_format=ContactInfo,
)
print(type(r.parsed_output).__name__, r.parsed_output, r.stop_reason)
```

```text
ContactInfo name='John Smith' email='john@example.com' plan_interest='Enterprise' demo_requested=True end_turn
```

- The class is passed as **`output_format=ContactInfo`** (exactly this argument of `parse`), and the result is read from **`r.parsed_output`** — it is already a `ContactInfo` object, not a dictionary.
- The SDK built the schema itself and sent it in `output_config.format`: in the experiment the request body had `{"format": {"schema": {"type": "object", "title": "ContactInfo", "properties": {…}, "additionalProperties": false, "required": ["name", "email", "plan_interest", "demo_requested"]}, …}}`; there was no `output_format` field in the body.
- `"additionalProperties": false` and a `required` list with all the model's fields were added to the schema: the SDK brings the schema to an acceptable form.

## messages.create and json.loads

Without the helper you pass the schema as a dictionary, and the JSON arrives as **text** in a `text` block; you parse and validate it yourself:

```python
import json
schema = {"type": "object",
          "properties": {"name": {"type": "string"}, "email": {"type": "string"},
                         "plan_interest": {"type": "string"}, "demo_requested": {"type": "boolean"}},
          "required": ["name", "email", "plan_interest", "demo_requested"],
          "additionalProperties": False}
m = client.messages.create(model="claude-opus-5-5", max_tokens=1024,
                           messages=[{"role": "user", "content": "x"}],
                           output_config={"format": {"type": "json_schema", "schema": schema}})
text = next(b.text for b in m.content if b.type == "text")
print(json.loads(text)["name"], ContactInfo.model_validate_json(text))
```

```text
John Smith name='John Smith' email='john@example.com' plan_interest='Enterprise' demo_requested=True
```

`ContactInfo.model_validate_json(text)` not only parses the JSON but also checks the model's types and constraints: it is the second line of defense after the schema.

## Check stop_reason

The guarantee of matching the schema applies to a **finished** reply. If generation is cut by `max_tokens` or the model refused (`refusal`), the text may be incomplete and not fit the schema:

```python
m = client.messages.create(model="claude-opus-5-5", max_tokens=5, messages=[{"role": "user", "content": "x"}],
                           output_config={"format": {"type": "json_schema", "schema": schema}})
print(m.stop_reason)
```

```text
max_tokens
```

In the experiment the mock returned a truncated text `{"name": "John"` for a small limit: `json.loads` gave a `JSONDecodeError`, and `model_validate_json` a `ValidationError`. So the order is: check `stop_reason`, then parse and validate. On `max_tokens` repeat the request with a bigger limit (leave headroom for structured replies), on `refusal` the reply is not fit for parsing although the HTTP code is 200.

## Beginner mistakes

- ❌ Asking for "only JSON" in the prompt instead of a schema → ✅ `output_config.format`.
- ❌ Reading `content[0].text` as a ready object → ✅ `json.loads` or `parse` + `parsed_output`.
- ❌ Not checking `stop_reason` → ✅ `max_tokens` and `refusal`.
- ❌ Considering "valid JSON" to be "the right meaning" → ✅ check the values (Pydantic, domain rules).
- ❌ Passing `output_format` as a dictionary → ✅ `output_config.format` (a dictionary `output_format` is deprecated).
- ❌ Too small a `max_tokens` → ✅ headroom for the full JSON.

## Related materials

- Next: [advanced theory](advanced.en.md) — schema limitations, `enum`, `strict` on tools, the schema cache
- Earlier: [03 · Prompt engineering](../03-prompt-engineering/basics.en.md#output-format)
- Parallels: [Python](../../../python/overview.en.md) · [LLM](../../../llm/overview.en.md)

## Sources

- [Structured outputs](https://platform.claude.com/docs/en/build-with-claude/structured-outputs)
- [Strict tool use](https://platform.claude.com/docs/en/agents-and-tools/tool-use/strict-tool-use)
- [Python SDK](https://platform.claude.com/docs/en/cli-sdks-libraries/sdks/python)
