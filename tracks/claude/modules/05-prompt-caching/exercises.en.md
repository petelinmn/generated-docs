---
title: "Exercises: prompt caching"
lang: en
track: claude
kind: module
doc: exercises
slug: 05-prompt-caching
part: 1
tags: [claude, prompt-caching, cache-ttl, cost-optimization, cache-prewarming]
status: draft
updated: 2026-11-24
---

# Exercises: prompt caching

🌐 [Русский](exercises.ru.md) · **English**

> Solve on your own; hints and solutions are under spoilers. The solutions are local Python functions and requests to a mock server (see "How the code was checked" in the [basic theory](basics.en.md)); the service's real cache was not checked.

## 1. 🟢 The cost of a request by `usage`

**Task:** a function `cost(usage, base=5.0, …)` computes the cost of input tokens from the `usage` fields (a 5-minute write 1.25×, a 1-hour write 2×, a read 0.1×; the price in $ per million tokens). For a write of 2048 tokens and 20 ordinary ones, then a read of 2048 tokens and 20 ordinary ones, find the costs and the ratio "read / without cache".

**Expected result:** `0.0129 0.001124 0.01034 0.109`.

<details><summary>Hint</summary>

`per = base / 1_000_000`; take the write by TTL from `usage.cache_creation`.

</details>

<details><summary>Solution</summary>

See the function `cost` and the output in the [basic theory](basics.en.md#what-it-costs):

```python
def cost(usage, base=5.0, mult_write5=1.25, mult_write1h=2.0, mult_read=0.1):
    per = base / 1_000_000
    cc = usage.cache_creation
    w5 = cc.ephemeral_5m_input_tokens if cc else usage.cache_creation_input_tokens
    w1 = cc.ephemeral_1h_input_tokens if cc else 0
    return per * (usage.input_tokens + w5 * mult_write5 + w1 * mult_write1h + usage.cache_read_input_tokens * mult_read)
```

```text
0.0129 0.001124 0.01034 0.109
```

</details>

## 2. 🟢 Checking the TTL order

**Task:** a function `valid_ttl_order(blocks)` takes a list of `"5m"`/`"1h"` values in the order of the breakpoints and returns `False` if a 1-hour point comes **after** a 5-minute one.

**Expected result:** `True False True` for `["1h","5m"]`, `["5m","1h"]`, `["1h","1h","5m"]`.

<details><summary>Hint</summary>

Remember whether a 5-minute point has already been seen.

</details>

<details><summary>Solution</summary>

```python
def valid_ttl_order(blocks):
    seen_5m = False
    for ttl in blocks:
        if ttl == "5m": seen_5m = True
        elif ttl == "1h" and seen_5m: return False
    return True
print(valid_ttl_order(["1h", "5m"]), valid_ttl_order(["5m", "1h"]), valid_ttl_order(["1h", "1h", "5m"]))
```

```text
True False True
```

</details>

## 3. 🟡 When the cache pays off

**Task:** the total cost of `n` requests with the same prefix as a share of the cost of one ordinary input: `write + read × (n − 1)` against `n` without a cache. Find the minimum `n` at which the cache is profitable, for a 5-minute write (1.25×) and a 1-hour write (2×), if a read is 0.1×.

**Expected result:** `2 3`.

<details><summary>Hint</summary>

Iterate `n` from 1 and compare `total(n) < n`.

</details>

<details><summary>Solution</summary>

```python
def total(n, write=1.25, read=0.1): return write + read * (n - 1)
print(min(n for n in range(1, 50) if total(n) < n), min(n for n in range(1, 50) if total(n, 2.0) < n))
```

```text
2 3
```

For a 5-minute write the cache pays off from the second request (`1.35 < 2`), for a one-hour one from the third (`2.2 < 3`). The calculation is simplified: it counts only the prefix and ignores the variable part and the output.

</details>

## 4. 🟡 A stable prefix

**Task:** show with fingerprints (`sha256`) that a `system` with a changing second gives different prefixes and without it the same; and that JSON with a different key order matches only after `sort_keys=True`.

**Expected result:** `False True` for the fingerprints and `False True` for JSON.

<details><summary>Hint</summary>

`hashlib.sha256(s.encode()).hexdigest()[:8]`; `json.dumps(obj, sort_keys=True)`.

</details>

<details><summary>Solution</summary>

```python
import hashlib, json, datetime
def fp(s): return hashlib.sha256(s.encode()).hexdigest()[:8]
def system_bad(rules, sec): return f"Now: 2026-11-24T10:00:{sec:02d}\n{rules}"
def system_good(rules): return rules
print(fp(system_bad("R", 1)) == fp(system_bad("R", 2)), fp(system_good("R")) == fp(system_good("R")))
print(json.dumps({"b": 1, "a": 2}) == json.dumps({"a": 2, "b": 1}), json.dumps({"b": 1, "a": 2}, sort_keys=True) == json.dumps({"a": 2, "b": 1}, sort_keys=True))
```

```text
False True
False True
```

</details>

## 5. 🔴 A pre-warm request

**Task:** send a request with `max_tokens=0`, automatic `cache_control` and a long `system`; make sure from the reply that `content` is empty, `stop_reason` is `max_tokens` and 2048 tokens were written (a mock reply).

**Expected result:** `[] max_tokens 0 2048` (the content, the stop reason, the output tokens, the written tokens).

<details><summary>Hint</summary>

`cache_control={"type": "ephemeral"}` at the top level; `r.usage.cache_creation_input_tokens`.

</details>

<details><summary>Solution</summary>

```python
r = client.messages.create(model="claude-opus-5-5", max_tokens=0, cache_control={"type": "ephemeral"},
                           system=BIG_SYSTEM, messages=[{"role": "user", "content": "x"}])
print(r.content, r.stop_reason, r.usage.output_tokens, r.usage.cache_creation_input_tokens)
```

```text
[] max_tokens 0 2048
```

On the real API a pre-warm makes sense only with the same thinking and `effort` settings as the working requests and without `stream: true`.

</details>
