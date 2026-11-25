---
title: "Batch, evals и надёжность: продвинутый уровень"
lang: ru
track: claude
kind: module
doc: advanced
slug: 14-evals
part: 3
tags: [claude, evals, batch-api, retries, reliability]
versions: "Claude 5.5 family; anthropic 2.14.0; Message Batches API"
status: draft
updated: 2026-11-25
---

# Batch, evals и надёжность: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Повторы SDK

По умолчанию `max_retries=2` и таймаут чтения 600 с. Наблюдения:

| Ответ | Попыток | Итог |
|---|---|---|
| 429, `retry-after: 1` (дважды) | 3, около 2 с | успех |
| 529 | 3 | `OverloadedError` |
| 500, `x-should-retry: false` | 1 | `InternalServerError` |
| 400 | 1 | `BadRequestError` |

Повторяются сетевые сбои, 408, 409, 429 и 5xx; заголовок `x-should-retry` имеет приоритет. Без `retry-after` пауза растёт от 0,5 до 8 с. Таймаут тоже повторяется, поэтому общее время может быть `timeout × (max_retries + 1)`.

## Обработка исходов

```python
# reliability.py
import anthropic


def ask(client, prompt, model="claude-opus-5-5"):
    try:
        response = client.messages.create(model=model, max_tokens=1024,
                                          messages=[{"role": "user", "content": prompt}])
    except anthropic.BadRequestError as e:
        return ("bug in request", e.request_id)
    except anthropic.RateLimitError as e:
        return ("rate limited", e.request_id)
    except anthropic.APIStatusError as e:
        return (f"api error {e.status_code}", e.request_id)
    except anthropic.APIConnectionError:
        return ("no connection", None)
    if response.stop_reason == "refusal":
        return ("refused", response.stop_details.category if response.stop_details else None)
    if response.stop_reason == "max_tokens":
        return ("truncated", response._request_id)
    return ("ok", "".join(b.text for b in response.content if b.type == "text"))
```

Отказ — это **успешный** ответ 200 с `stop_reason: "refusal"`: `try/except` его не поймает, а `content` может быть пустым. Есть серверный `fallbacks` (бета), повторяющий отклонённый запрос на другой модели.

## Message Batches

```python
# batch.py
import time

from anthropic.types.message_create_params import MessageCreateParamsNonStreaming
from anthropic.types.messages.batch_create_params import Request


def run_batch(client, prompts, model="claude-opus-5-5", poll_seconds=60):
    """prompts: {custom_id: text}. Returns ({custom_id: answer}, {custom_id: failure type})."""
    batch = client.messages.batches.create(requests=[
        Request(custom_id=key, params=MessageCreateParamsNonStreaming(
            model=model, max_tokens=1024, messages=[{"role": "user", "content": text}]))
        for key, text in prompts.items()])
    while batch.processing_status != "ended":
        time.sleep(poll_seconds)
        batch = client.messages.batches.retrieve(batch.id)

    answers, failed = {}, {}
    for item in client.messages.batches.results(batch.id):   # any order
        if item.result.type == "succeeded":
            answers[item.custom_id] = "".join(b.text for b in item.result.message.content if b.type == "text")
        else:
            failed[item.custom_id] = item.result.type
    return answers, failed
```

| Факт | Значение |
|---|---|
| цена | 50% от обычной |
| размер | до 100 000 запросов или 256 МБ |
| срок | обычно меньше часа; через 24 часа истекает |
| результаты | 29 дней; JSONL в любом порядке |
| `custom_id` | `^[a-zA-Z0-9_-]{1,64}$` |
| типы результата | `succeeded`, `errored`, `canceled`, `expired` |

За `errored`, `canceled` и `expired` не платят. `stream` и `max_tokens: 0` в batch не поддерживаются. Batch подходит для прогонов eval и фоновой обработки, не для ответа пользователю.

## Оценка моделью

```python
RUBRIC = """Grade the reply against the criteria. Answer with one word: pass or fail.
<criteria>{criteria}</criteria>
<reply>{reply}</reply>"""


def judge(client, reply, criteria, model="claude-sonnet-5-5"):
    response = client.messages.create(model=model, max_tokens=2000, messages=[
        {"role": "user", "content": RUBRIC.format(criteria=criteria, reply=reply)}])
    verdict = "".join(b.text for b in response.content if b.type == "text").strip().lower()
    if verdict not in ("pass", "fail"):
        raise ValueError(f"judge did not follow the format: {verdict!r}")
    return verdict == "pass"
```

Строгий формат ответа судьи и ошибка при отклонении не дают молча засчитать «Pass, but…». Судью проверяют на размеченных людьми примерах.

## Связанные материалы

- [Базовая теория](basics.ru.md) · [Эксперт](expert.ru.md)
- [05 · Prompt caching](../05-prompt-caching/advanced.ru.md) · [13 · Длинный контекст](../13-long-context/basics.ru.md)

## Источники

- [Errors](https://platform.claude.com/docs/en/api/errors)
- [Batch processing](https://platform.claude.com/docs/en/build-with-claude/batch-processing)
- [Refusals and fallback](https://platform.claude.com/docs/en/build-with-claude/refusals-and-fallback)
