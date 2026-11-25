---
title: "Развёртывание и production: продвинутый уровень"
lang: ru
track: claude
kind: module
doc: advanced
slug: 17-production
part: 3
tags: [claude, deployment, production, retries, bedrock, vertex-ai, foundry]
versions: "Claude 5.5 family; anthropic 2.14.0; Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# Развёртывание и production: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Чего нет на облачных платформах

| | Claude API | Platform on AWS | Bedrock | Vertex AI | Foundry |
|---|---|---|---|---|---|
| Message Batches | ✓ | ✓ | — | — | — |
| Files API | ✓ | ✓ | — | — | только на Anthropic |
| Models API | ✓ | ✓ | — | — | — |
| web search | ✓ | ✓ | — | базовый | ✓ |
| code execution | ✓ | ✓ | — | — | только на Anthropic |
| Managed Agents | beta | beta | — | — | — |
| `inference_geo` | ✓ | ✓ | — | — | — |
| server-side `fallbacks` | beta | beta | — | — | — |

**Наличие метода в SDK ничего не гарантирует:** у `AnthropicBedrockMantle` и `AnthropicVertex` атрибут `client.messages.batches` есть, хотя платформа batch не поддерживает. Атрибутов `client.models` и `client.files` у них нет.

## Ошибки и повторы

| Ситуация | SDK повторяет | Что делать |
|---|---|---|
| 429 лимит | да | ждать `retry-after`, снизить поток |
| 429 `ramp_limit_exceeded` | да | наращивать нагрузку постепенно |
| 429 `enforced_spend_limit_reached` | **да, хотя бесполезно** | алерт, повышать лимит |
| 500, 529 | да | затем очередь или другой маршрут |
| ошибка внутри потока | **нет** | повторить самим, выбросив часть ответа |
| 400, 401, 403, 404 | нет | чинить запрос или права |

Заголовок `x-should-retry` имеет приоритет над кодом. Исчерпанный лимит трат — это класс `EnforcedSpendLimitReachedError`, наследник `RateLimitError`: при настройках по умолчанию проверка получила три попытки. Лимит, который вы задали сами, возвращает 400.

## Обрыв посреди стрима

Ответ 200 уже получен, часть текста пришла, затем событие `error`. SDK бросает `APIStatusError` со `status_code == 200` и не повторяет запрос.

```python
# complete.py
import time

import anthropic


def complete(client, attempts=3, sleep=time.sleep, **params):
    """Stream a reply; retry only errors that arrive inside the stream (the SDK does not)."""
    for attempt in range(1, attempts + 1):
        try:
            with client.messages.stream(**params) as stream:
                return stream.get_final_message()       # partial text of a failed try is dropped
        except anthropic.APIStatusError as error:
            mid_stream = error.status_code == 200 and error.type in ("overloaded_error", "api_error")
            if not mid_stream or attempt == attempts:
                raise                                   # other errors were already retried by the SDK
            sleep(2 ** attempt)
```

Если текст уже показан пользователю, интерфейс должен уметь стереть его перед повтором. Вызванные инструменты не выполняйте, пока сообщение не получено целиком.

## Запасной маршрут

```python
# failover.py
import anthropic


def worth_another_route(error):
    """True when a different platform or region may succeed where this one failed."""
    if isinstance(error, anthropic.APIConnectionError):          # includes timeouts
        return True
    if isinstance(error, anthropic.APIStatusError):
        return error.status_code == 429 or error.status_code >= 500
    return False                                                 # 400, 401, 403, 404 need a fix


def create_with_failover(routes, **params):
    """routes: list of (client, model_id), best first."""
    last = None
    for client, model in routes:
        try:
            return client.messages.create(model=model, **params)
        except anthropic.APIError as error:
            if not worth_another_route(error):
                raise
            last = error
    raise last
```

Запасной маршрут — это другая платформа или регион с **проверенной** вашим eval моделью. Учтите: prompt cache между ними не общий, функции различаются, а переход на 429 из-за лимита трат уводит расходы на другой счёт.

## Отказ модели

Отказ — это не ошибка, и перебор маршрутов выше его не увидит. SDK middleware повторяет отказавший запрос на другой модели:

```python
from anthropic import Anthropic, BetaFallbackState, BetaRefusalFallbackMiddleware

client = Anthropic(middleware=[BetaRefusalFallbackMiddleware([{"model": "claude-sonnet-5-5"}])])
state = BetaFallbackState()                     # one per conversation
with state:
    response = client.beta.messages.create(model="claude-opus-5-5", max_tokens=1024,
                                           messages=[{"role": "user", "content": "hi"}])
print(response.model, [block.type for block in response.content])
```

В проверке с имитацией отказа первый вызов отправил два запроса и вернул блоки `['fallback', 'text']`; следующий вызов с тем же `state` сразу пошёл на запасную модель. Отказы в части случаев тарифицируются, так что fallback не бесплатен.

## Лимиты

- Лимиты Claude API: RPM, ITPM, OTPM на класс модели, алгоритм token bucket; короткий всплеск тоже может дать 429.
- Чтения из кэша по умолчанию не входят в ITPM, `max_tokens` не входит в OTPM.
- Ramp limit ограничивает скорость роста нагрузки; Batch API ему не подчиняется.
- В Bedrock и Vertex квотами управляет облако; Foundry не отдаёт заголовки `anthropic-ratelimit-*`.

## Регионы и цена

| Выбор | Эффект на цену |
|---|---|
| Bedrock: global → regional | +10% |
| Vertex AI: global → multi-region или regional | +10% |
| `inference_geo: "us"` (Claude API) | ×1.1 |
| Foundry: US Data Zone Standard | ×1.1 |
| Batch API | −50% |

На Vertex AI региональные endpoints обслуживают только Sonnet 4.6 и более ранние модели; для новых берите global или multi-region. Цены Bedrock и Vertex задают AWS и Google. Рычаги снижения расхода — [кэширование](../05-prompt-caching/overview.ru.md), выбор [модели](../01-models/overview.ru.md) и [effort](../01-models/overview.ru.md), batch.

## Смена модели и выкладка

1. ID модели и платформу держите в конфигурации.
2. Перед сменой прогоните eval; сравните качество, цену и задержку.
3. Выкатывайте долей трафика с возможностью отката.
4. Следите за сроками снятия: Anthropic предупреждает минимум за 60 дней, Bedrock и Vertex живут по своим расписаниям.

## Связанные материалы

- [Базовая теория](basics.ru.md) · [Эксперт](expert.ru.md)
- [05 · Prompt caching](../05-prompt-caching/advanced.ru.md) · [15 · Observability](../15-observability/advanced.ru.md)

## Источники

- [Errors](https://platform.claude.com/docs/en/api/errors)
- [Rate limits](https://platform.claude.com/docs/en/api/rate-limits)
- [Refusals and fallback](https://platform.claude.com/docs/en/build-with-claude/refusals-and-fallback)
- [Pricing](https://platform.claude.com/docs/en/about-claude/pricing)
- [Model deprecations](https://platform.claude.com/docs/en/about-claude/model-deprecations)
