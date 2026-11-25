---
title: "Deployment and production: exercises"
lang: en
track: claude
kind: module
doc: exercises
slug: 17-production
part: 3
tags: [claude, deployment, production, retries, bedrock, vertex-ai, foundry]
versions: "Claude 5.5 family; anthropic 2.14.0; Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# Deployment and production: exercises

🌐 [Русский](exercises.ru.md) · **English**

> You need `platforms.py`, `complete.py` and `failover.py` from the theory and `pip install "anthropic[bedrock,vertex,aws]"`. No keys or network.

## 1. The client factory

Create Bedrock and Foundry clients with dummy values and print model ID and base URL.

<details><summary>Hint</summary>

Pass `aws_region`, `api_key`, `resource`.

</details>

<details><summary>Solution</summary>

```python
from platforms import make_client

for platform, options in (("bedrock", {"aws_region": "eu-west-1", "api_key": "fake"}),
                          ("foundry", {"resource": "demo", "api_key": "fake"})):
    client, model = make_client(platform, **options)
    print(platform, model, client.base_url)
```

</details>

## 2. What justifies failover

Check `worth_another_route` on a connection error and on an unrelated exception.

<details><summary>Hint</summary>

`APIConnectionError` needs a `request`; SDK 2.14.0 uses the `httpx2` package.

</details>

<details><summary>Solution</summary>

```python
import anthropic
import httpx2

from failover import worth_another_route

request = httpx2.Request("POST", "https://example.invalid")
print(worth_another_route(anthropic.APIConnectionError(request=request)))
print(worth_another_route(ValueError("bad input")))
```

</details>

## 3. Failing over

The first route is down, the second answers. Fake the clients with plain objects.

<details><summary>Hint</summary>

`types.SimpleNamespace`.

</details>

<details><summary>Solution</summary>

```python
from types import SimpleNamespace

import anthropic
import httpx2

from failover import create_with_failover


def down(**params):
    raise anthropic.APIConnectionError(request=httpx2.Request("POST", "https://example.invalid"))


def up(**params):
    return f"answered by {params['model']}"


routes = [(SimpleNamespace(messages=SimpleNamespace(create=down)), "primary"),
          (SimpleNamespace(messages=SimpleNamespace(create=up)), "backup")]
print(create_with_failover(routes, max_tokens=10, messages=[]))
```

</details>

## 4. The price of a choice

Price a $1 request: global, regional, and a batch with `inference_geo="us"`.

<details><summary>Hint</summary>

Multipliers stack.

</details>

<details><summary>Solution</summary>

```python
# price.py
def request_price(base_cost, endpoint="global", inference_geo="global", batch=False):
    """base_cost: the list-price cost of the request in dollars."""
    cost = base_cost
    if endpoint != "global":                    # Bedrock regional, Google multi-region or regional
        cost *= 1.10
    if inference_geo == "us":                   # Claude API, 4.6 and later models
        cost *= 1.10
    if batch:
        cost *= 0.50
    return round(cost, 6)


print(request_price(1), request_price(1, endpoint="regional"), request_price(1, inference_geo="us", batch=True))
```

</details>
