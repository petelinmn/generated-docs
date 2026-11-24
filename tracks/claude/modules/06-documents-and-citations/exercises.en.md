---
title: "Exercises: documents and citations"
lang: en
track: claude
kind: module
doc: exercises
slug: 06-documents-and-citations
part: 1
tags: [claude, pdf, vision, citations, files-api]
status: draft
updated: 2026-11-24
---

# Exercises: documents and citations

🌐 [Русский](exercises.ru.md) · **English**

> Solve on your own; hints and solutions are under spoilers. The solutions are local Python functions and requests to a mock server (see "How the code was checked" in the [basic theory](basics.en.md)); real model replies and the accuracy of citations were not checked.

## 1. 🟢 Building the content of a message

**Task:** a function `build_content(question, pdf_bytes=None, images=())` returns the `content` list: first the PDF `document` block (base64), then for each image a label `Image N:` and the `image` block, and the question text last. For a PDF and two images print the types of the blocks.

**Expected result:** `['document', 'text', 'image', 'text', 'image', 'text']`.

<details><summary>Hint</summary>

Documents and images go before the question text; `images` is a list of `(media_type, bytes)` pairs.

</details>

<details><summary>Solution</summary>

```python
import base64

def build_content(question, pdf_bytes=None, images=()):
    blocks = []
    if pdf_bytes is not None:
        blocks.append({"type": "document", "source": {"type": "base64", "media_type": "application/pdf",
                       "data": base64.standard_b64encode(pdf_bytes).decode()}})
    for i, (mt, data) in enumerate(images, 1):
        blocks += [{"type": "text", "text": f"Image {i}:"},
                   {"type": "image", "source": {"type": "base64", "media_type": mt,
                    "data": base64.standard_b64encode(data).decode()}}]
    blocks.append({"type": "text", "text": question})
    return blocks
```

</details>

## 2. 🟢 Base64 without line breaks

**Task:** check which base64 function gives a string with no line breaks: `standard_b64encode` or `encodebytes`. Use `b"x" * 100`.

**Expected result:** `False` for `standard_b64encode`, `True` for `encodebytes`.

<details><summary>Hint</summary>

Look for `"\n"` in the decoded string.

</details>

<details><summary>Solution</summary>

```python
import base64
print("\n" in base64.standard_b64encode(b"x" * 100).decode())
print("\n" in base64.encodebytes(b"x" * 100).decode())
```

```text
False
True
```

Use `standard_b64encode`: `encodebytes` inserts line breaks.

</details>

## 3. 🟡 The tokens of an image

**Task:** a function `visual_tokens(w, h)` counts the image tokens by 28×28 patches. Find the values for 1000×1000 and 1568×1568.

**Expected result:** `1296 3136`.

<details><summary>Hint</summary>

`⌈w/28⌉ × ⌈h/28⌉`, use `math.ceil`.

</details>

<details><summary>Solution</summary>

```python
import math
def visual_tokens(w, h): return math.ceil(w / 28) * math.ceil(h / 28)
print(visual_tokens(1000, 1000), visual_tokens(1568, 1568))
```

```text
1296 3136
```

See [cost in tokens](advanced.en.md#cost-in-tokens).

</details>

## 4. 🟡 Verifying citations

**Task:** a function `verified(message, doc_text)` returns the list of `cited_text` values that do **not** match `doc_text[start_char_index:end_char_index]`. Run it on a mock reply with the real document text and with a different one.

**Expected result:** `[]` for the real text, `['The grass is green. ', 'The sky is blue.']` for the different one.

<details><summary>Hint</summary>

Go through the blocks of `message.content`; read the citations as `getattr(b, "citations", None) or []`.

</details>

<details><summary>Solution</summary>

```python
def verified(message, doc_text):
    bad = []
    for b in message.content:
        for ci in (getattr(b, "citations", None) or []):
            if doc_text[ci.start_char_index:ci.end_char_index] != ci.cited_text:
                bad.append(ci.cited_text)
    return bad
```

```text
[]
['The grass is green. ', 'The sky is blue.']
```

The output is on a mock reply. This is the check from the [advanced theory](advanced.en.md#using-citations-in-an-application).

</details>

## 5. 🔴 Catching an incompatible request

**Task:** a function `has_conflict(body)` returns `True` if the request body has citations enabled on at least one `document` block **and** contains `output_config.format`. Check it on a valid body and on the same body with a format added.

**Expected result:** `False True`.

<details><summary>Hint</summary>

Look through `messages[*].content` for `document` blocks with `citations.enabled`; the format is in `body["output_config"]["format"]`.

</details>

<details><summary>Solution</summary>

```python
def has_conflict(body):
    cited = any(
        isinstance(b, dict) and b.get("type") == "document" and (b.get("citations") or {}).get("enabled")
        for m in body.get("messages", []) if isinstance(m.get("content"), list)
        for b in m["content"]
    )
    return cited and "format" in body.get("output_config", {})
```

```text
False True
```

On a conflict the API returns a 400: split it into two requests ([incompatibilities](advanced.en.md#incompatibilities)).

</details>
