---
title: "Развёртывание и production: упражнения"
lang: ru
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

# Развёртывание и production: упражнения

🌐 **Русский** · [English](exercises.en.md)

> Нужны `platforms.py`, `complete.py` и `failover.py` из теории и `pip install "anthropic[bedrock,vertex,aws]"`. Ключи и сеть не нужны.

## 1. Фабрика клиентов

Создайте клиентов Bedrock и Foundry с фиктивными значениями и напечатайте ID модели и адрес.

<details><summary>Подсказка</summary>

Передайте `aws_region`, `api_key`, `resource`.

</details>

<details><summary>Решение</summary>

```python
from platforms import make_client

for platform, options in (("bedrock", {"aws_region": "eu-west-1", "api_key": "fake"}),
                          ("foundry", {"resource": "demo", "api_key": "fake"})):
    client, model = make_client(platform, **options)
    print(platform, model, client.base_url)
```

</details>

## 2. Что стоит переключать

Проверьте `worth_another_route` на обрыве соединения и на постороннем исключении.

<details><summary>Подсказка</summary>

`APIConnectionError` требует `request`; SDK 2.14.0 использует пакет `httpx2`.

</details>

<details><summary>Решение</summary>

```python
import anthropic
import httpx2

from failover import worth_another_route

request = httpx2.Request("POST", "https://example.invalid")
print(worth_another_route(anthropic.APIConnectionError(request=request)))
print(worth_another_route(ValueError("bad input")))
```

</details>

## 3. Переключение

Первый маршрут недоступен, второй отвечает. Подмените клиентов простыми объектами.

<details><summary>Подсказка</summary>

`types.SimpleNamespace`.

</details>

<details><summary>Решение</summary>

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

## 4. Цена выбора

Посчитайте запрос ценой $1: global, regional, batch с `inference_geo="us"`.

<details><summary>Подсказка</summary>

Множители перемножаются.

</details>

<details><summary>Решение</summary>

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
