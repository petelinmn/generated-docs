---
title: "Exercises: tool use"
lang: en
track: claude
kind: module
doc: exercises
slug: 07-tool-use
part: 1
tags: [claude, tool-use, tool-choice, agent-loop]
status: draft
updated: 2026-11-24
---

# Exercises: tool use

🌐 [Русский](exercises.ru.md) · **English**

> Solve on your own; hints and solutions are under spoilers. The solutions are local Python functions with stand-in objects instead of real replies (see "How the code was checked" in the [basic theory](basics.en.md)); real model decisions were not checked.

The stand-ins: `NS` is `types.SimpleNamespace`; a block is `NS(type="tool_use", id="t1", name="add", input={"a": 1, "b": 2})`.

## 1. 🟢 Results from calls

**Task:** a function `tool_result_blocks(content, handlers)` returns a `tool_result` for every `tool_use` block of the reply (ignoring other blocks). A handler error becomes `is_error: True` with the text `ExceptionName: message`. Run it on the calls `add(1, 2)`, an unknown `nope`, and `add(1)` (a missing argument).

**Expected result:**

```text
{'type': 'tool_result', 'tool_use_id': 't1', 'content': '3'}
{'type': 'tool_result', 'tool_use_id': 't2', 'content': "KeyError: 'nope'", 'is_error': True}
{'type': 'tool_result', 'tool_use_id': 't3', 'content': "TypeError: <lambda>() missing 1 required positional argument: 'b'", 'is_error': True}
```

<details><summary>Hint</summary>

Look up `handlers[b.name]` and call it with `**b.input` inside `try`; catch `Exception`.

</details>

<details><summary>Solution</summary>

```python
def tool_result_blocks(content, handlers):
    out = []
    for b in content:
        if b.type != "tool_use":
            continue
        try:
            out.append({"type": "tool_result", "tool_use_id": b.id, "content": str(handlers[b.name](**b.input))})
        except Exception as e:
            out.append({"type": "tool_result", "tool_use_id": b.id, "content": f"{type(e).__name__}: {e}", "is_error": True})
    return out
```

(The handler was `{"add": lambda a, b: a + b}`; the output is as above.)

</details>

## 2. 🟢 Is the history valid?

**Task:** a function `history_ok(messages)` returns `False` if after an assistant turn with `tool_use` blocks there is no user message that **starts** with `tool_result` blocks for exactly those ids. Check: correct results; text before the result; the result then text; no user message at all.

**Expected result:** `True False True False`.

<details><summary>Hint</summary>

Collect the leading `tool_result` blocks of the next user message and compare their `tool_use_id` set with the call ids.

</details>

<details><summary>Solution</summary>

```python
def history_ok(messages):
    for i, m in enumerate(messages):
        if m["role"] != "assistant" or not isinstance(m["content"], list):
            continue
        ids = {b["id"] for b in m["content"] if b.get("type") == "tool_use"}
        if not ids:
            continue
        nxt = messages[i + 1] if i + 1 < len(messages) else None
        if not nxt or nxt["role"] != "user" or not isinstance(nxt["content"], list):
            return False
        head = []
        for b in nxt["content"]:
            if b.get("type") != "tool_result":
                break
            head.append(b["tool_use_id"])
        if set(head) != ids:
            return False
    return True
```

This mirrors the documented rule ([gotchas 1 and 2](expert.en.md#1-text-before-the-results)).

</details>

## 3. 🟡 The tool overhead

**Task:** a function `overhead(model, tc_type)` returns the documented system-prompt tokens (Opus 5.5 286, Opus 5 286/406, Sonnet 5 354/474, Haiku 5.5 286/406, with `auto`/`none` first and `any`/`tool` second) and raises `ValueError` for forced use on models without support. Print `overhead("claude-opus-5", "any")`, `overhead("claude-sonnet-5", "auto")`, `overhead("claude-haiku-5-5", "tool")` and the error for `claude-opus-5-5` with `any`.

**Expected result:**

```text
406 354 406
claude-opus-5-5: forced tool use is not supported
```

<details><summary>Hint</summary>

A dictionary of pairs `(auto, forced)`; `None` means "not supported".

</details>

<details><summary>Solution</summary>

```python
OVERHEAD = {"claude-opus-5-5": (286, None), "claude-opus-5": (286, 406),
            "claude-sonnet-5": (354, 474), "claude-haiku-5-5": (286, 406)}

def overhead(model, tc_type):
    auto, forced = OVERHEAD[model]
    if tc_type in ("auto", "none"):
        return auto
    if forced is None:
        raise ValueError(f"{model}: forced tool use is not supported")
    return forced
```

See [cost in tokens](advanced.en.md#cost-in-tokens).

</details>

## 4. 🟡 A bounded loop

**Task:** a function `run_loop(call_model, handlers, question, max_steps=5)` implements the agent loop: it returns the text of the final reply and raises `RuntimeError("no final answer after N steps")` when the model keeps asking for tools. Use `tool_result_blocks` from exercise 1. Check it on a scripted model (a `tool_use` for `add(2, 3)`, then the text "2 + 3 = 5") and on a model that always calls a tool, with `max_steps=3`.

**Expected result:**

```text
2 + 3 = 5
no final answer after 3 steps
```

<details><summary>Hint</summary>

On every step: call the model, return the text if `stop_reason != "tool_use"`, otherwise append the assistant turn and the user turn with the results.

</details>

<details><summary>Solution</summary>

```python
def run_loop(call_model, handlers, question, max_steps=5):
    messages = [{"role": "user", "content": question}]
    for _ in range(max_steps):
        r = call_model(messages)
        if r.stop_reason != "tool_use":
            return "".join(b.text for b in r.content if b.type == "text")
        messages.append({"role": "assistant", "content": r.content})
        messages.append({"role": "user", "content": tool_result_blocks(r.content, handlers)})
    raise RuntimeError(f"no final answer after {max_steps} steps")
```

</details>

## 5. 🔴 A request against the mock

**Task:** write a loop with the SDK (as in the [basic theory](basics.en.md#call-and-result)) for a question that makes the model call two tools at once, and print the `tool_use_id` values of the last user message. Then break it on purpose: put a text block before the result and catch `anthropic.BadRequestError`.

**Expected result (mock server):**

```text
3 ['toolu_01', 'toolu_02']
BadRequestError 400
```

<details><summary>Hint</summary>

The last request has three messages: the question, the assistant turn, one user message with two results.

</details>

<details><summary>Solution</summary>

The loop is `ask` from the basic theory; the mock server was the one described there. The wrong request:

```python
try:
    client.messages.create(model="claude-opus-5-5", max_tokens=100, tools=tools, messages=[
        {"role": "user", "content": "Weather?"},
        {"role": "assistant", "content": [{"type": "tool_use", "id": "toolu_01", "name": "get_weather", "input": {"location": "Paris"}}]},
        {"role": "user", "content": [{"type": "text", "text": "here"},
                                     {"type": "tool_result", "tool_use_id": "toolu_01", "content": "15"}]}])
except anthropic.BadRequestError as e:
    print(type(e).__name__, e.status_code)
```

The 400 comes from the mock, which implements the documented rule; a real service answers with its own message.

</details>
