---
title: "Компактизация и длинный контекст"
lang: ru
track: claude
kind: module
doc: overview
slug: 13-long-context
part: 3
tags: [claude, context-window, compaction, context-editing, token-counting]
versions: "Claude 5.5 family; anthropic 2.14.0; beta compact-2026-09-04, compact-2026-01-12, context-management-2025-06-27; Claude Code 2.1.332"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-25
---

# Компактизация и длинный контекст

🌐 **Русский** · [English](overview.en.md)

## Коротко

Что делать, когда разговор или задача агента не помещается в **контекстное окно**: считать токены, обрезать, **сжимать** и очищать.

## Ключевые идеи

- Окно включает и вход, и вывод.
- Большой контекст стоит денег и качества.
- Компактизация заменяет историю сводкой.
- Очистка убирает старые результаты инструментов.
- Всё это — бета-возможности с отдельными заголовками.

## Минимальный пример

```python
# compact.py: on-demand compaction (beta)
def compact(client, history, model="claude-opus-5-5", system=None):
    """Return a new history that starts with the compaction block, or the old one if none came back."""
    extra = {"system": system} if system else {}
    summary = client.beta.messages.create(
        model=model, max_tokens=8192, messages=history,
        betas=["compact-2026-09-04"], compaction={"type": "summarize"}, **extra)
    if summary.stop_reason != "compaction":
        return history
    return [{"role": "assistant", "content": summary.content}]
```

## Когда применять и когда нет

Длинные диалоги и агентные задачи. Короткому запросу это не нужно.

## Уровни материала

| Уровень | Файл | Добавить |
|---|---|---|
| 1. Базовая теория | [basics.ru.md](basics.ru.md) | |
| 2. Продвинутая теория | [advanced.ru.md](advanced.ru.md) | |
| 3. Экспертная теория | [expert.ru.md](expert.ru.md) | |
| 4. Самопроверка | [quiz.ru.md](quiz.ru.md), [exercises.ru.md](exercises.ru.md) | |
| 5. Запускаемые примеры | — | `/learn claude 13-long-context level 5` |
| 6. Тесты | — | `/learn claude 13-long-context level 6` |

## Связанные материалы

- [Трек Claude](../../overview.ru.md)
- [05 · Prompt caching](../05-prompt-caching/overview.ru.md)
- [10 · CLAUDE.md и память](../10-memory/overview.ru.md)
- [12 · Агенты](../12-agents/overview.ru.md)
