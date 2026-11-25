---
title: "Batch, evals и надёжность"
lang: ru
track: claude
kind: module
doc: overview
slug: 14-evals
part: 3
tags: [claude, evals, batch-api, retries, reliability]
versions: "Claude 5.5 family; anthropic 2.14.0; Message Batches API"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-25
---

# Batch, evals и надёжность

🌐 **Русский** · [English](overview.en.md)

## Коротко

Как проверять качество ответов (**evals**), переживать ошибки и лимиты и дёшево обрабатывать большие объёмы через **Message Batches**.

## Ключевые идеи

- Без eval нет доказательства улучшения.
- SDK повторяет часть ошибок сам.
- Отказ — это ответ 200.
- Batch вдвое дешевле и асинхронен.
- Результаты batch сопоставляют по `custom_id`.

## Минимальный пример

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

## Когда применять и когда нет

Любое приложение, которое увидят пользователи. Для разового эксперимента хватит просмотра глазами.

## Уровни материала

| Уровень | Файл | Добавить |
|---|---|---|
| 1. Базовая теория | [basics.ru.md](basics.ru.md) | |
| 2. Продвинутая теория | [advanced.ru.md](advanced.ru.md) | |
| 3. Экспертная теория | [expert.ru.md](expert.ru.md) | |
| 4. Самопроверка | [quiz.ru.md](quiz.ru.md), [exercises.ru.md](exercises.ru.md) | |
| 5. Запускаемые примеры | — | `/learn claude 14-evals level 5` |
| 6. Тесты | — | `/learn claude 14-evals level 6` |

## Связанные материалы

- [Трек Claude](../../overview.ru.md)
- [03 · Prompt engineering](../03-prompt-engineering/overview.ru.md)
- [13 · Компактизация и длинный контекст](../13-long-context/overview.ru.md)
- [15 · Observability](../15-observability/overview.ru.md)
- [17 · Развёртывание и production](../17-production/overview.ru.md)
