---
title: "Компактизация и длинный контекст: основы"
lang: ru
track: claude
kind: module
doc: basics
slug: 13-long-context
part: 3
tags: [claude, context-window, compaction, context-editing, token-counting]
versions: "Claude 5.5 family; anthropic 2.14.0; beta compact-2026-09-04, compact-2026-01-12, context-management-2025-06-27; Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# Компактизация и длинный контекст: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [02 · Messages API](../02-messages-api/overview.ru.md), [05 · Prompt caching](../05-prompt-caching/overview.ru.md)

## Как проверялся материал

Ключа API нет. Код запускался с SDK `anthropic` 2.14.0 против локальной заглушки: проверены формы запросов и логика на стороне клиента, но не качество сводок и не реальные счётчики токенов. `/compact` проверен в Claude Code 2.1.332 тем же способом. Пределы и поведение сервиса взяты из документации.

## Контекстное окно

Окно — это всё, что модель видит в одном запросе, вместе с ответом: системный промпт, все сообщения, результаты инструментов, картинки, документы, описания инструментов и вывод, включая thinking. API без состояния: историю вы присылаете заново каждый раз, и она растёт.

| Модели | Окно |
|---|---|
| Fable 5–5.2, Mythos 5–5.2, Mythos Preview, Opus 4.6–5.5, Sonnet 4.6–5.5, Haiku 5.5 | 1M токенов |
| остальные | 200k |

Для моделей с окном 1M оно включено по умолчанию по обычной цене; исключение — Haiku 5.5, где запросы длиннее 100 000 токенов дороже. Больше контекста — не значит лучше: качество падает по мере роста.

## Посчитать заранее

```python
import anthropic

client = anthropic.Anthropic()
count = client.messages.count_tokens(model="claude-opus-5-5", messages=messages, system=system, tools=tools)
print(count.input_tokens)          # an estimate; counting itself is free
```

## Что будет при переполнении

| Ситуация | Результат |
|---|---|
| вход один больше окна | 400 `invalid_request_error`, `prompt is too long` |
| вход + `max_tokens` больше окна (модели 4.5 и новее) | запрос принят; при достижении предела `stop_reason: "model_context_window_exceeded"` |

## Простейший способ: обрезать историю

```python
# history.py
def trim(messages, keep_last):
    """Keep the last `keep_last` messages without starting on an orphaned tool_result."""
    start = max(0, len(messages) - keep_last)
    while start > 0 and is_tool_result(messages[start]):
        start -= 1
    return messages[start:]


def is_tool_result(message):
    content = message["content"]
    return isinstance(content, list) and any(block.get("type") == "tool_result" for block in content)
```

Резать вслепую нельзя: `tool_result` без своего `tool_use` даёт ошибку. Функция отступает назад до начала пары. Обрезка теряет информацию и сбивает кэш — [следующий уровень](advanced.ru.md) показывает лучшие способы.

## Частые ошибки новичков

- Думать, что кэширование освобождает место: кэшированные токены занимают окно так же.
- Складывать в историю огромные результаты инструментов.
- Обрабатывать только `end_turn`.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [06 · Документы](../06-documents-and-citations/overview.ru.md)

## Источники

- [Context windows](https://platform.claude.com/docs/en/build-with-claude/context-windows)
- [Token counting](https://platform.claude.com/docs/en/build-with-claude/token-counting)
