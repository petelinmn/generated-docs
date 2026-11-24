---
title: "Семейство Claude и выбор модели"
lang: ru
track: claude
kind: module
doc: overview
slug: 01-models
part: 1
levels: [1, 2, 3, 4, 5, 6]
tags: [claude, model-selection, context-window, effort, model-ids]
versions: "Claude 5.5 family; 2026-11"
status: verified
updated: 2026-11-23
---

# Семейство Claude и выбор модели

🌐 **Русский** · [English](overview.en.md)

## Коротко

Claude — семейство моделей Anthropic, доступных через Messages API, с разными возможностями и компромиссами скорости, стоимости и качества. Семейство 5.5 включает Fable для максимальной capability, Opus для сложной agentic-работы и программирования, Sonnet для повседневной работы и Haiku для низкой задержки и высоких объёмов.

Выбор модели начинается с критерия задачи, а не с названия линейки: сначала формулируются требования к точности, задержке и бюджету, затем создаётся репрезентативный набор проверочных задач и сравниваются модели и уровни effort. Для одного запроса нужно различать API model ID, alias, pinned model ID и имя развёртывания на partner platform.

## Ключевые идеи

- Модель определяется не только размером семейства: различаются режимы thinking, effort и ограничения контекста.
- Если workload ещё не определён, Anthropic рекомендует начать с Opus; для high-volume задач выбирать Haiku или Sonnet по результатам evals.
- Fable занимает позицию максимальной capability для особенно сложной reasoning или долгих agentic-сессий.
- Контекстное окно определяет весь working memory запроса: system prompt, сообщения, tools, изображения и документы.
- Текущие модели с 1M контекстом имеют стандартный 1M limit; для выбора и lifecycle нужно сверяться с таблицей моделей.
- Эффект effort можно тестировать внутри одной модели до смены модели.
- Multimodel-схема переносит типовые шаги на более дешёвую модель, оставляя сложные решения более мощной.

## Минимальный пример

```python
def recommend(task_type: str) -> str:
    recommendations = {
        "coding": "claude-opus-5-5",
        "high_volume": "claude-haiku-5-5",
        "everyday": "claude-sonnet-5-5",
        "demanding": "claude-fable-5-2",
    }
    return recommendations.get(task_type, "claude-opus-5-5")


print(recommend("coding"))
```

Вывод:

```text
coding claude-opus-5-5
```

## Когда применять, а когда нет

- ✅ Сравнивать модели по реальным промптам и наборам edge cases.
- ✅ Использовать effort для tuning latency и cost до смены линейки.
- ✅ Закреплять model ID или deployment name в конфигурации.
- ❌ Выбирать модель только по бренду или ценнику за модель.
- ❌ Считать размер контекста гарантией качества: при росте контекста возможен context rot.
- ❌ Смешивать versioned model ID и partner-specific inference profile без проверки платформы.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | семейство, модели, model ID, контекст и первый выбор |
| [2. Продвинутая теория](advanced.ru.md) | effort, evals, multimodel, platform IDs и lifecycle |
| [3. Экспертная теория](expert.ru.md) | справочник, breaking changes, подвохи и вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и упражнения по выбору моделей |
| [5. Запускаемые примеры](code/examples/) | детерминированные routing и eval-примеры |
| [6. Тесты](code/tests/) | pytest-проверки routing, eval matrix и конфигурации |

## Связанные материалы

- Пререквизиты: [LLM](../../../llm/overview.ru.md) и основы HTTP/API.
- Дальше: [02 · Messages API](../02-messages-api/overview.ru.md)
- То же в других треках: принципы выбора моделей и evaluation в [LLM](../../../llm/overview.ru.md).
