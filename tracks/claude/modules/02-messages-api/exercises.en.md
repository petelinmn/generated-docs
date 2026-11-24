---
title: "Exercises: Messages API"
lang: en
track: claude
kind: module
doc: exercises
slug: 02-messages-api
part: 1
tags: [claude, messages-api, chat-history, stop-reason, api-errors, streaming]
status: draft
updated: 2026-11-24
---

# Exercises: Messages API

🌐 [Русский](exercises.ru.md) · **English**

> Solve on your own; hints and solutions are under spoilers. The solutions were checked against a local mock server (see "How the code was checked" in the [basic theory](basics.en.md)); the outputs in the solutions are mock replies, so the texts are stand-ins. With a real key use `client = anthropic.Anthropic()`.

## 1. 🟢 The `ask` function

**Task:** write `ask(client, question, system=None, max_tokens=1024)` that sends a question and returns the joined text of the `text` blocks. Add the `system` parameter to the request only if it is set.

**Expected result:** `Paris.`; without `system` the request body has no `system` field, with `system="Be terse."` it does.

<details><summary>Hint</summary>

`kwargs = {"system": system} if system else {}` and `"".join(b.text for b in message.content if b.type == "text")`.

</details>

<details><summary>Solution</summary>

```python
def ask(client, question, system=None, max_tokens=1024):
    kwargs = {"system": system} if system else {}
    message = client.messages.create(model="claude-opus-5-5", max_tokens=max_tokens, messages=[{"role": "user", "content": question}], **kwargs)
    return "".join(b.text for b in message.content if b.type == "text")
```

```text
Paris. | False Be terse.
```

(The second part of the output is a check of the request body on the mock: without `system` there is no field, with `system` it is `Be terse.`.)

</details>

## 2. 🟢 A conversation with history

**Task:** a class `Chat` keeps a list `messages`. The method `say(text)` appends the user's message, sends the whole history, appends the **whole** `content` of the reply as an assistant message and returns the text.

**Expected result:** after two calls the history has 4 messages, and the second request contained the roles `user`, `assistant`, `user`.

<details><summary>Hint</summary>

`self.messages.append({"role": "assistant", "content": reply.content})` — the SDK serializes the blocks itself.

</details>

<details><summary>Solution</summary>

```python
class Chat:
    def __init__(self, client, system=None):
        self.client, self.system, self.messages = client, system, []
    def say(self, text):
        self.messages.append({"role": "user", "content": text})
        kwargs = {"system": self.system} if self.system else {}
        reply = self.client.messages.create(model="claude-opus-5-5", max_tokens=1024, messages=self.messages, **kwargs)
        self.messages.append({"role": "assistant", "content": reply.content})
        return "".join(b.text for b in reply.content if b.type == "text")

chat = Chat(client); chat.say("My name is Alice."); chat.say("What is my name?")
```

```text
4 ['user', 'assistant', 'user'] [{'text': 'Paris.', 'type': 'text'}]
```

The assistant block went into the request as `{'text': 'Paris.', 'type': 'text'}`.

</details>

## 3. 🟡 Controlling `stop_reason`

**Task:** a function `complete(client, question, max_tokens)` returns the text but raises `Truncated` if the reply was cut by `max_tokens`, and `PermissionError` if `stop_reason == "refusal"`.

**Expected result:** with `max_tokens=1024` — `Paris.`; with `max_tokens=5` — `Truncated: cut after 6 tokens` (the token count was returned by the mock).

<details><summary>Hint</summary>

Check `m.stop_reason` before reading `m.content[0].text`.

</details>

<details><summary>Solution</summary>

```python
class Truncated(Exception): pass

def complete(client, question, max_tokens):
    m = client.messages.create(model="claude-opus-5-5", max_tokens=max_tokens, messages=[{"role": "user", "content": question}])
    if m.stop_reason == "max_tokens": raise Truncated(f"cut after {m.usage.output_tokens} tokens")
    if m.stop_reason == "refusal": raise PermissionError("refused")
    return m.content[0].text
```

```text
Paris.
Truncated: cut after 6 tokens
```

</details>

## 4. 🟡 Classifying errors

**Task:** a function `classify(exc)` returns a string: for `RateLimitError` — `retry later`, for a timeout — `timeout`, for a network error — `network`, for `BadRequestError` — `fix request: <error.type>`, for other HTTP errors — `status <code>`. The order of checks matters: specific classes first.

**Expected result:** for a 400 response — `fix request: invalid_request_error`, for a 429 — `retry later`, for an unreachable server — `network`.

<details><summary>Hint</summary>

`APITimeoutError` inherits `APIConnectionError`: check the timeout before the network. To check, turn retries off: `max_retries=0`.

</details>

<details><summary>Solution</summary>

```python
import anthropic

def classify(exc):
    if isinstance(exc, anthropic.RateLimitError): return "retry later"
    if isinstance(exc, anthropic.APITimeoutError): return "timeout"
    if isinstance(exc, anthropic.APIConnectionError): return "network"
    if isinstance(exc, anthropic.BadRequestError): return "fix request: " + exc.body["error"]["type"]
    if isinstance(exc, anthropic.APIStatusError): return f"status {exc.status_code}"
    return "unknown"
```

```text
fix request: invalid_request_error
retry later
network
```

</details>

## 5. 🔴 Streaming with a callback

**Task:** a function `stream_text(client, question, on_chunk)` calls `on_chunk` for each text fragment and returns the final message.

**Expected result:** the fragments `['Par', 'is.']`, their concatenation matches the final message's text, `usage.output_tokens` is `6`.

<details><summary>Hint</summary>

`with client.messages.stream(...) as s:`; `for t in s.text_stream`; then `s.get_final_message()`.

</details>

<details><summary>Solution</summary>

```python
def stream_text(client, question, on_chunk):
    with client.messages.stream(model="claude-opus-5-5", max_tokens=1024, messages=[{"role": "user", "content": question}]) as s:
        for t in s.text_stream:
            on_chunk(t)
        return s.get_final_message()

seen = []; final = stream_text(client, "Capital?", seen.append)
print(seen, "".join(seen) == final.content[0].text, final.usage.output_tokens)
```

```text
['Par', 'is.'] True 6
```

</details>
