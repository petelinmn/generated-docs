---
title: "Anthropic SDK: exercises"
lang: en
track: claude
kind: library
doc: exercises
slug: anthropic-sdk
part: 2
tags: [claude, sdk, anthropic-sdk, python, typescript]
versions: "anthropic 2.14.0; @anthropic-ai/sdk 1.14.0"
status: draft
updated: 2026-11-25
---

# Anthropic SDK: exercises

🌐 [Русский](exercises.ru.md) · **English**

> You need `stub.py` from the advanced theory. No key or network.

## 1. A stub

Get a reply through `stub_client` without a network.

<details><summary>Hint</summary>

A plain `messages.create`.

</details>

<details><summary>Solution</summary>

```python
from stub import stub_client

m = stub_client().messages.create(model="claude-opus-5-5", max_tokens=10,
                                  messages=[{"role": "user", "content": "hi"}])
print(m.content[0].text)
```

</details>

## 2. Retries

The server answers 529 twice, then succeeds. Show the retry count.

<details><summary>Hint</summary>

`retry-after: 0` and `with_raw_response`.

</details>

<details><summary>Solution</summary>

```python
import httpx2

from stub import REPLY, stub_client

calls = []


def handler(request):
    calls.append(1)
    if len(calls) < 3:
        return httpx2.Response(529, headers={"retry-after": "0"},
                               json={"type": "error", "error": {"type": "overloaded_error", "message": "x"}})
    return httpx2.Response(200, json=REPLY)


raw = stub_client(handler).messages.with_raw_response.create(
    model="claude-opus-5-5", max_tokens=10, messages=[{"role": "user", "content": "hi"}])
print(len(calls), raw.retries_taken)
```

</details>

## 3. An error

Return a 400 and print its type and `request_id`.

<details><summary>Hint</summary>

The `request-id` header.

</details>

<details><summary>Solution</summary>

```python
import anthropic
import httpx2

from stub import stub_client


def handler(request):
    return httpx2.Response(400, headers={"request-id": "req_42"},
                           json={"type": "error", "error": {"type": "invalid_request_error", "message": "bad"}})


try:
    stub_client(handler).messages.create(model="claude-opus-5-5", max_tokens=10,
                                         messages=[{"role": "user", "content": "hi"}])
except anthropic.BadRequestError as error:
    print(error.type, error.request_id)
```

</details>

## 4. What goes over the wire

Check the request body: `model` and `max_tokens`.

<details><summary>Hint</summary>

`json.loads(request.content)`.

</details>

<details><summary>Solution</summary>

```python
import json

import httpx2

from stub import REPLY, stub_client

seen = []


def handler(request):
    seen.append(json.loads(request.content))
    return httpx2.Response(200, json=REPLY)


stub_client(handler).messages.create(model="claude-opus-5-5", max_tokens=10,
                                     messages=[{"role": "user", "content": "hi"}])
print(seen[0]["model"], seen[0]["max_tokens"])
```

</details>
