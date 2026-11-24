---
title: "Structured outputs: advanced"
lang: en
track: claude
kind: module
doc: advanced
slug: 04-structured-output
part: 1
tags: [claude, structured-outputs, json-schema, transform-schema, enum, strict-tool-use, schema-cache, validation]
versions: "Claude 5.5 family; Messages API, anthropic-version 2026-10-21; Python SDK anthropic 2.13.0"
status: draft
updated: 2026-11-24
---

# Structured outputs: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on the [basic theory](basics.en.md). The code was run against a local mock server and local SDK functions; the replies of real models and constrained decoding were not checked. The schema rules come from the documentation (part of it in an abridged summary of the page).

## What the SDK does to a schema: transform_schema

The function `anthropic.transform_schema(...)` returns a schema in a form suitable for the API without sending a request; it accepts both a dictionary and a Pydantic class:

```python
from anthropic import transform_schema
t = transform_schema(ContactInfo)
print(sorted(t), t.get("additionalProperties"), t["required"])
```

```text
['additionalProperties', 'properties', 'required', 'title', 'type'] False ['name', 'email', 'plan_interest', 'demo_requested']
```

- `additionalProperties: false` and `required` for all fields were added to the schema.
- Constraints the API does not support are **moved into the description**: for `score: int = Field(ge=1, le=5, description="rating")` the SDK left `{"type": "integer", "title": "Score", "description": "rating\n\n{maximum: 5, minimum: 1}"}` — there are no numeric bounds in the schema, they are only a hint to the model.
- Hence the rule: **check ranges and lengths yourself** after parsing. In the experiment `Rating.model_validate_json('{"score": 9}')` rejected the value with the error `less_than_equal`.

## What to do with fields

**Enumerations.** An `enum` and a Pydantic `Enum` constrain the set of values:

```python
class Priority(str, Enum):
    low = "low"; high = "high"
class Ticket(BaseModel):
    title: str
    priority: Priority
    assignee: Optional[str] = None
s = transform_schema(Ticket)
print(json.dumps(s["properties"]["priority"]), s["required"], json.dumps(s["properties"]["assignee"]))
```

```text
{"$ref": "#/$defs/Priority"} ['title', 'priority'] {"anyOf": [{"type": "string"}, {"type": "null"}], "title": "Assignee", "description": "{default: None}"}
```

- The enumeration turned into a `$ref` link to a definition in `$defs`; the optional field into an `anyOf` of a string and `null`, and it does not go into `required`, while the default value goes into the description.
- The case of enumeration values in a reply may differ: compare case-insensitively.

**Dictionaries.** A `dict` field has no known keys: the schema gets empty `properties`, and the model can return only `{}`. In the experiment `tags: dict` turned into `{"type": "object", "title": "Tags", "properties": {}, "additionalProperties": false}`. If the keys are known, describe them with a nested model; if not, use a list of `{key, value}` pairs.

## Schema limitations

From the documentation (part in an abridged summary):

| Supported | Not supported |
|---|---|
| basic types, `enum`/`const` (strings, numbers, booleans, `null`), `anyOf`, `$ref`/`$defs`, `required`, string formats (`date-time`, `date`, `email`, `uuid`, `uri` …), `minItems` equal to 0 or 1 | recursive schemas, `oneOf` (use `anyOf`), `not`, external `$ref`, `minimum`/`maximum`/`multipleOf`, `minLength`/`maxLength`, `additionalProperties` other than `false` |

Unsupported things return a 400. A schema that is too complex also gives a 400 (`Schema is too complex for compilation`): compilation is time-limited.

> ⚠️ Verify: the exact list of supported keywords and the complexity limits come from an abridged summary of the page; check the "Structured outputs" page before use.

## Strict tool use

A tool has its own flag `strict: true` — at the top level of the definition, next to `name`, `description`, `input_schema`:

```python
client.messages.create(
    model="claude-opus-5-5", max_tokens=100,
    messages=[{"role": "user", "content": "x"}],
    tools=[{"name": "get_weather", "description": "d", "strict": True,
            "input_schema": {"type": "object", "properties": {"city": {"type": "string"}},
                             "required": ["city"], "additionalProperties": False}}],
)
```

In the experiment the request body contained `strict: true` on the tool (`['name', 'description', 'strict', 'input_schema']`). What `strict` gives: the call's arguments **strictly match** `input_schema`, and the tool name is always valid. The difference between the approaches:

| | JSON outputs | Strict tool use |
|---|---|---|
| What it constrains | the format of the model's **reply** | the **arguments** of a tool call |
| Parameter | `output_config.format` | `strict: true` on the tool |
| Read from | a `text` block | a `tool_use` block (`input`) |

They can be combined in one request. Tools are covered in detail in module 07 of the roadmap.

## The schema cache and latency

- The first request with a new schema is slower: the schema is compiled into a grammar; the result is cached (per the documentation up to 24 hours since last use).
- Changing the **structure** of the schema resets this cache, while changing only `name`/`description` does not.
- Changing `output_config.format` invalidates the prompt cache for this conversation branch ([module 05](../../overview.en.md) of the roadmap): keep the schema stable.

> ⚠️ Verify: the terms and conditions of the schema cache, the compilation limit (about 180 seconds) and the effect on the prompt cache come from a summary of the documentation and were not measured in the experiments.

## Retries on an incomplete reply

A schema-valid reply does not always arrive: there are `max_tokens` and `refusal`. A working technique is a check and a limited number of retries:

```python
def extract_with_retry(call, attempts=2):
    for i in range(attempts):
        text = call(i)
        try:
            return Contact.model_validate_json(text), i + 1
        except ValidationError:
            continue
    raise ValueError("no valid output")
```

In the experiment the first attempt returned truncated JSON, the second a valid one: the result is `(Contact(...), 2)`. A retry is useless on `refusal` (the cause is not the format) and on `max_tokens` without raising the limit.

## Best practices

- Describe the structure with a Pydantic model and take `parsed_output`.
- Use simple schemas: no recursion and `oneOf`.
- Check ranges, lengths and business rules after parsing.
- Leave headroom in `max_tokens` and check `stop_reason`.
- Do not change the schema without need (the cache).
- For function calls use `strict: true`, for the reply format use JSON outputs.

## Common mistakes

- ❌ Expecting `Field(ge=1, le=5)` to constrain the model → ✅ it is only a hint; check yourself.
- ❌ A `dict` field for unknown keys → ✅ a list of pairs or a nested model.
- ❌ `oneOf`/recursion → ✅ `anyOf`, a flat schema.
- ❌ Citations and structured outputs in one request → ✅ they are incompatible (400).
- ❌ Prefill together with a schema → ✅ do not use.
- ❌ Treating a retry as the cure for `refusal` → ✅ change the request.

## Related materials

- Next: [expert theory](expert.en.md)
- Output format via the prompt: [03 · Prompt engineering](../03-prompt-engineering/advanced.en.md)
- Request parameters and `stop_reason`: [02 · Messages API](../02-messages-api/expert.en.md)

## Sources

- [Structured outputs](https://platform.claude.com/docs/en/build-with-claude/structured-outputs)
- [Strict tool use](https://platform.claude.com/docs/en/agents-and-tools/tool-use/strict-tool-use)
