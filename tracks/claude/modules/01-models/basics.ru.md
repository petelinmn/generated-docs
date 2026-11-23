---
title: "Семейство Claude и выбор модели: базовая теория"
lang: ru
track: claude
kind: module
doc: basics
slug: 01-models
part: 1
tags: [claude, model-selection, context-window, model-ids]
versions: "Claude 5.5 family; 2026-11"
status: verified
updated: 2026-11-23
---

# Семейство Claude и выбор модели: базовая теория

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [обзор](overview.ru.md) · **1. Базовая теория** · [2. Продвинутая теория](advanced.ru.md) · [3. Экспертная теория](expert.ru.md) · [4. Квиз](quiz.ru.md) · [5. Запускаемые примеры](code/examples/) · [6. Тесты](code/tests/)

**Пререквизиты:** [LLM](../../../llm/overview.ru.md) · **Время:** ~45 мин

**Уровень 5:** [запускаемые примеры](code/examples/) — routing и eval matrix без сетевых запросов.

## Зачем это нужно

Один и тот же текстовый запрос можно отправить разным моделям Claude, но цена ошибки выбора растёт вместе с объёмом работы: слабая модель может требовать больше итераций, а избыточно мощная — увеличивать стоимость задач с высокой частотой. Базовый выбор сводится к четырём переменным: capability, latency, cost и effort.

## Семейство Claude 5.5

Для текущей линии family опубликованы четыре основных профиля:

| Модель | API ID | Основной профиль | Типичный стартовый сценарий |
|---|---|---|---|
| Claude Fable 5.2 | `claude-fable-5-2` | Максимальная capability | Самые сложные reasoning-задачи и долгие agentic-сессии |
| Claude Opus 5.5 | `claude-opus-5-5` | Агентное программирование и длительная knowledge work | Сложная разработка, рефакторинг, автономные агенты |
| Claude Sonnet 5.5 | `claude-sonnet-5-5` | Баланс скорости и capability | Повседневный код, анализ, контент и tools |
| Claude Haiku 5.5 | `claude-haiku-5-5` | Наименьшая latency и cost | Классификация, извлечение, routing и subagents |

Все они поддерживают текстовый и image-ввод, text-output, мультиязычность, vision и tool use. На официальной странице моделей должны сверяться актуальные ID, данные о retirements и partner-platform endpoints.

## Два способа начать

При **efficiency-first** начинают с Haiku, тестируют качество на реальном наборе и повышают модель только при обнаруженном capability-gap.

При **capability-first** начинают с Opus для сложного agentic coding или enterprise work, оценивают качество и затем снижают effort или переходят к более дешёвой модели там, где это не ухудшает evals. Для самых сложных reasoning-задач, где даже xhigh/max effort не достигает результата, рассматривают Fable.

Если задача пока не определена, Anthropic рекомендует начинать с Opus 5.5. Sonnet 5.5 — разумная отправная точка, когда важны скорость и capability для повседневных coding-, analysis-, content- и agentic-workloads.

## Контекст и effort

Контекстное окно включает system prompt, все сообщения, tools, tool results, изображения и документы. Те же компоненты используются при thinking и учитываются в лимите. Текущие 1M-модели 5.5 имеют стандартное 1M входное окно и до 128K синхронного output; отдельный beta-header для включения контекста не нужен.

Effort задаётся для Messages API через `output_config.effort`. Он меняет объём и thoroughness ответа с одним model ID:

```python
effort = "medium"

request = {
    "model": "claude-opus-5-5",
    "max_tokens": 4096,
    "messages": [{"role": "user", "content": "Назови компромисс двух подходов"}],
    "output_config": {"effort": effort},
}

print(request["model"], request["output_config"]["effort"])
```

Вывод:

```text
claude-opus-5-5 medium
```

По умолчанию Opus 5.5 и Haiku 5.5 используют `medium`, остальные основные модели — `high`; точные defaults проверяются на странице каждой модели.

## Model ID и snapshot

Используйте:

| Термин | Значение |
|---|---|
| API model ID | ID для Claude API: например, `claude-opus-5-5` |
| Alias | Удобный указатель на snapshot для поколений до 4.6 |
| Pinned model ID | Идентификатор закреплённой версии модели; веса и конфигурация этого ID фиксированы |
| Deployment name | Имя развёртывания на Microsoft Foundry, которое отправляется вместо native ID |
| Inference profile | ID развёртывания/маршрутизации на partner platform, например Bedrock |

Датeless ID для поколений 4.6+ является каноническим pinned ID, а короткий alias старой модели может указывать на последний датированный snapshot. Поведение serving-инфраструктуры может влиять на наблюдаемые ответы, даже когда веса закреплённого ID не меняются. ID и lifecycle зависят от provider.

## Ошибки новичка

- ❌ Выбирать Fable для всех задач → ✅ сначала реализовать workflow и измерить типовую стоимость и качество.
- ❌ Считать 1M максимально полезным для любого запроса → ✅ курировать context; лишний контекст может ухудшить recall.
- ❌ Менять модель по субъективному впечатлению → ✅ фиксировать промпты, ответы, score и latency в eval-наборе.
- ❌ Жёстко зашивать partner-specific ID → ✅ хранить provider и model/deployment в отдельной конфигурации.

## Связанные материалы

- Пререквизиты: [LLM](../../../llm/overview.ru.md)
- Следующий модуль: Messages API в дорожной карте [трека Claude](../../overview.ru.md)
- Продолжение: [продвинутая теория](advanced.ru.md)

## Источники

- [Claude models overview](https://platform.claude.com/docs/en/models/overview)
- [Choosing the right model](https://platform.claude.com/docs/en/about-claude/models/choosing-a-model)
- [Effort](https://platform.claude.com/docs/en/build-with-claude/effort)
- [Context windows](https://platform.claude.com/docs/en/build-with-claude/context-windows)
