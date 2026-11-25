---
title: "Batch, evals и надёжность: основы"
lang: ru
track: claude
kind: module
doc: basics
slug: 14-evals
part: 3
tags: [claude, evals, batch-api, retries, reliability]
versions: "Claude 5.5 family; anthropic 2.14.0; Message Batches API"
status: draft
updated: 2026-11-25
---

# Batch, evals и надёжность: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [02 · Messages API](../02-messages-api/overview.ru.md), [03 · Prompt engineering](../03-prompt-engineering/overview.ru.md)

## Как проверялся материал

Ключа API нет. Код запускался с SDK `anthropic` 2.14.0 против локального сервера, имитирующего ответы и ошибки API. Повторы, классы исключений и разбор batch-результатов проверены на настоящем SDK; поведение сервиса, цены и пределы взяты из документации.

## Зачем evals

Изменили промпт или модель — стало лучше? Без набора проверок это угадывание. **Eval** — это примеры входов, способ оценить ответ и порог.

Критерий успеха делают конкретным и измеримым: не «хорошо классифицирует», а «не меньше 90% верных меток на отложенном наборе».

## Минимальный eval

```python
# evals.py
CASES = [
    {"id": "refund", "input": "I want my money back", "expected": "refund"},
    {"id": "delivery", "input": "Where is my parcel?", "expected": "delivery"},
    {"id": "other", "input": "Do you sell gift cards?", "expected": "other"},
]


def grade(output, expected):
    return output.strip().lower() == expected


def run_eval(classify, cases=CASES):
    """classify(text) -> label. Returns (accuracy, failed case ids)."""
    failed = [c["id"] for c in cases if not grade(classify(c["input"]), c["expected"])]
    return (len(cases) - len(failed)) / len(cases), failed
```

```python
from evals import run_eval


def keyword_baseline(text):            # a stand-in for a model call
    if "money" in text:
        return "refund"
    if "parcel" in text:
        return "Delivery"
    return "unknown"


print(run_eval(keyword_baseline))
```

```text
(0.6666666666666666, ['other'])
```

`grade` приводит ответ к нижнему регистру, поэтому `Delivery` засчитан; метка `unknown` — ошибка. Список проваленных случаев важнее среднего: по нему видно, что чинить.

## Три способа оценки

| Способ | Когда |
|---|---|
| кодом | точное совпадение, формат, наличие фактов: быстро и дёшево |
| моделью | тон, полнота, соответствие критериям: сначала проверить самого судью |
| человеком | эталон и спорные случаи: дорого и медленно |

Много автоматически оцениваемых примеров полезнее нескольких разобранных вручную.

## Частые ошибки новичков

- Проверять на тех же примерах, по которым подбирали промпт.
- Только «счастливые» входы: нужны пустые, длинные, враждебные, двусмысленные.
- Один запуск как доказательство: ответы модели различаются между запусками.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [04 · Structured outputs](../04-structured-output/overview.ru.md)

## Источники

- [Define success criteria and build evaluations](https://platform.claude.com/docs/en/test-and-evaluate/develop-tests)
