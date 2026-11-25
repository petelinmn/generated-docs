---
title: "Компактизация и длинный контекст: продвинутый уровень"
lang: ru
track: claude
kind: module
doc: advanced
slug: 13-long-context
part: 3
tags: [claude, context-window, compaction, context-editing, token-counting]
versions: "Claude 5.5 family; anthropic 2.14.0; beta compact-2026-09-04, compact-2026-01-12, context-management-2025-06-27; Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# Компактизация и длинный контекст: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Три инструмента

| Средство | Что делает | Кто решает когда |
|---|---|---|
| компактизация по требованию | заменяет историю сводкой | ваш код |
| компактизация по порогу | то же внутри обычного запроса | API |
| очистка контекста (context editing) | убирает старые результаты инструментов или thinking | API |

Все три — в бете. Документация рекомендует компактизацию по требованию там, где она доступна.

## По требованию

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

```python
history.append({"role": "user", "content": question})
response = client.beta.messages.create(model="claude-opus-5-5", max_tokens=8192,
                                       betas=["compact-2026-09-04"], messages=history)
history.append({"role": "assistant", "content": response.content})   # the whole content, not just text

used = response.usage.input_tokens + response.usage.output_tokens    # the next request sends the reply too
if used > COMPACT_AT_TOKENS:
    history = compact(client, history)
```

- Запрос на сводку не даёт ответа: приходит один блок `compaction` и `stop_reason: "compaction"`.
- Блок отправляют первым в `messages` вместо пересказанных сообщений, без изменений, вместе с подписью; ровно один.
- Бета-заголовок нужен и на всех следующих запросах с блоком.
- Передавайте те же `system` и `tools`; `max_tokens` ограничивает весь вызов, включая thinking.

## По порогу

```python
response = client.beta.messages.create(
    model="claude-opus-5-5", max_tokens=4096, messages=messages,
    betas=["compact-2026-01-12"],
    context_management={"edits": [{"type": "compact_20260112",
                                   "trigger": {"type": "input_tokens", "value": 150000}}]})
messages.append({"role": "assistant", "content": response.content})
```

Порог по умолчанию 150 000 входных токенов, минимум 50 000. Добавляйте в историю весь `content`: API сам отбросит то, что было до блока. `pause_after_compaction` останавливает ответ после сводки, `instructions` **заменяет** стандартный промпт пересказа.

## Очистка результатов инструментов

```python
response = client.beta.messages.create(
    model="claude-opus-5-5", max_tokens=4096, messages=messages, tools=tools,
    betas=["context-management-2025-06-27"],
    context_management={"edits": [{
        "type": "clear_tool_uses_20250919",
        "trigger": {"type": "input_tokens", "value": 30000},
        "keep": {"type": "tool_uses", "value": 3},
        "clear_at_least": {"type": "input_tokens", "value": 5000},
        "exclude_tools": ["memory"]}]})
```

| Параметр | По умолчанию |
|---|---|
| `trigger` | 100 000 входных токенов |
| `keep` | 3 пары вызов–результат |
| `clear_at_least` | — |
| `exclude_tools` | — |
| `clear_tool_inputs` | `false` |

Правка происходит на сервере: у себя вы храните полную историю. Очистка сбивает кэш префикса, поэтому `clear_at_least` не даёт делать это ради мелочи. Вторая стратегия — `clear_thinking_20251015`.

## Claude Code

Сжатие происходит автоматически; вручную — `/compact`, можно с указанием, что сохранить: `/compact focus on the auth bug`. В проверке: сработали `PreCompact`, `SessionStart` с источником `compact` и `PostCompact`; история заменилась сводкой, а `CLAUDE.md` был подставлен заново.

## Связанные материалы

- [Базовая теория](basics.ru.md) · [Эксперт](expert.ru.md)
- [10 · CLAUDE.md](../10-memory/advanced.ru.md) · [11 · Hooks](../11-skills-and-hooks/advanced.ru.md)

## Источники

- [Compaction overview](https://platform.claude.com/docs/en/build-with-claude/compaction)
- [Compaction on demand](https://platform.claude.com/docs/en/build-with-claude/compaction-on-demand)
- [Compaction at a token threshold](https://platform.claude.com/docs/en/build-with-claude/compaction-threshold)
- [Context editing](https://platform.claude.com/docs/en/build-with-claude/context-editing)
- [Claude Code: context window](https://code.claude.com/docs/en/context-window)
