---
title: "Exercises: structured outputs"
lang: en
track: claude
kind: module
doc: exercises
slug: 04-structured-output
part: 1
tags: [claude, structured-outputs, pydantic, json-schema, stop-reason, validation]
status: draft
updated: 2026-11-24
---

# Exercises: structured outputs

🌐 [Русский](exercises.ru.md) · **English**

> Solve on your own; hints and solutions are under spoilers. The solutions were checked against a local mock server (see "How the code was checked" in the [basic theory](basics.en.md)); the outputs are mock replies. With a real key use `client = anthropic.Anthropic()`. The shared model:

```python
from pydantic import BaseModel
class Contact(BaseModel):
    name: str
    email: str
    plan_interest: str
    demo_requested: bool
```

## 1. 🟢 An `extract` function via `parse`

**Task:** `extract(client, text)` calls `client.messages.parse(…, output_format=Contact)` and returns `parsed_output`; if `stop_reason` is not `end_turn`, it raises `RuntimeError`.

**Expected result:** `name='John Smith' email='john@example.com' plan_interest='Enterprise' demo_requested=True`.

<details><summary>Hint</summary>

Check `stop_reason` before reading `parsed_output`.

</details>

<details><summary>Solution</summary>

```python
def extract(client, text):
    r = client.messages.parse(model="claude-opus-5-5", max_tokens=1024, messages=[{"role": "user", "content": f"Extract the contact: {text}"}], output_format=Contact)
    if r.stop_reason != "end_turn": raise RuntimeError(f"incomplete: {r.stop_reason}")
    return r.parsed_output
```

```text
name='John Smith' email='john@example.com' plan_interest='Enterprise' demo_requested=True
```

</details>

## 2. 🟢 A manual schema and `create`

**Task:** get the schema via `transform_schema(Contact)`, send it in `output_config.format` through `messages.create`, parse the text with `Contact.model_validate_json`. On `max_tokens` raise `Truncated`.

**Expected result:** for a normal request `john@example.com`; for a small `max_tokens` — `Truncated: raise max_tokens`.

<details><summary>Hint</summary>

The text is in the block with `type == "text"`; `from anthropic import transform_schema`.

</details>

<details><summary>Solution</summary>

```python
from anthropic import transform_schema
class Truncated(Exception): pass

def extract_raw(client, text, max_tokens):
    schema = transform_schema(Contact)
    m = client.messages.create(model="claude-opus-5-5", max_tokens=max_tokens, messages=[{"role": "user", "content": text}], output_config={"format": {"type": "json_schema", "schema": schema}})
    if m.stop_reason == "max_tokens": raise Truncated("raise max_tokens")
    return Contact.model_validate_json(next(b.text for b in m.content if b.type == "text"))
```

```text
john@example.com
Truncated: raise max_tokens
```

</details>

## 3. 🟡 An enumeration and an optional field

**Task:** describe `Ticket` with the fields `title: str`, `priority` (an enumeration `low`/`high`) and `assignee: Optional[str] = None`. Print the schema of the `priority` field, the `required` list and the schema of `assignee` via `transform_schema`.

**Expected result:** `{"$ref": "#/$defs/Priority"} ['title', 'priority'] {"anyOf": [{"type": "string"}, {"type": "null"}], "title": "Assignee", "description": "{default: None}"}`.

<details><summary>Hint</summary>

`class Priority(str, Enum)`; an optional field does not go into `required`.

</details>

<details><summary>Solution</summary>

```python
from enum import Enum
from typing import Optional
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

</details>

## 4. 🟡 Constraints do not reach the model

**Task:** a model `Rating` with `score: int = Field(ge=1, le=5)`. Show what the field turns into in the schema, and that the check of the value `9` fires at parse time.

**Expected result:** `{"type": "integer", "title": "Score", "description": "{maximum: 5, minimum: 1}"}` and `rejected: less_than_equal`.

<details><summary>Hint</summary>

`Rating.model_validate_json('{"score": 9}')` inside `try/except ValidationError`; the error type is `e.errors()[0]["type"]`.

</details>

<details><summary>Solution</summary>

```python
from pydantic import Field, ValidationError
class Rating(BaseModel):
    score: int = Field(ge=1, le=5)
print(json.dumps(transform_schema(Rating)["properties"]["score"]))
try: Rating.model_validate_json('{"score": 9}')
except ValidationError as e: print("rejected:", e.errors()[0]["type"])
```

```text
{"type": "integer", "title": "Score", "description": "{maximum: 5, minimum: 1}"}
rejected: less_than_equal
```

</details>

## 5. 🔴 A retry on wrong output

**Task:** `extract_with_retry(call, attempts=2)`: calls `call(i)`, which returns JSON text; parses it with `Contact.model_validate_json`; on `ValidationError` tries again; returns a pair `(object, attempt number)` or raises `ValueError("no valid output")`.

**Expected result:** if the first attempt returned `{"name": "A"` and the second valid JSON — `(Contact(name='A', email='a@b.c', plan_interest='Pro', demo_requested=False), 2)`.

<details><summary>Hint</summary>

A loop over `range(attempts)`, `continue` in `except`.

</details>

<details><summary>Solution</summary>

```python
def extract_with_retry(call, attempts=2):
    for i in range(attempts):
        text = call(i)
        try: return Contact.model_validate_json(text), i + 1
        except ValidationError: continue
    raise ValueError("no valid output")

good = '{"name":"A","email":"a@b.c","plan_interest":"Pro","demo_requested":false}'
print(extract_with_retry(lambda i: '{"name": "A"' if i == 0 else good))
```

```text
(Contact(name='A', email='a@b.c', plan_interest='Pro', demo_requested=False), 2)
```

A retry makes sense for a cut-off and unstable format; for `refusal` it must be replaced by changing the request.

</details>
