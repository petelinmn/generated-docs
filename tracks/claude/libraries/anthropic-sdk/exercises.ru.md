---
title: "Anthropic SDK: упражнения"
lang: ru
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

# Anthropic SDK: упражнения

🌐 **Русский** · [English](exercises.en.md)

> Нужен `stub.py` из продвинутой теории. Ключ и сеть не нужны.

## 1. Заглушка

Получите ответ через `stub_client` без сети.

<details><summary>Подсказка</summary>

Обычный `messages.create`.

</details>

<details><summary>Решение</summary>

```python
from stub import stub_client

m = stub_client().messages.create(model="claude-opus-5-5", max_tokens=10,
                                  messages=[{"role": "user", "content": "hi"}])
print(m.content[0].text)
```

</details>

## 2. Повторы

Сервер дважды отвечает 529, затем успехом. Покажите число повторов.

<details><summary>Подсказка</summary>

`retry-after: 0` и `with_raw_response`.

</details>

<details><summary>Решение</summary>

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

## 3. Ошибка

Верните 400 и напечатайте тип и `request_id`.

<details><summary>Подсказка</summary>

Заголовок `request-id`.

</details>

<details><summary>Решение</summary>

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

## 4. Что уходит по сети

Проверьте тело запроса: `model` и `max_tokens`.

<details><summary>Подсказка</summary>

`json.loads(request.content)`.

</details>

<details><summary>Решение</summary>

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
