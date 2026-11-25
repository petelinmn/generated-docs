---
title: "Claude Agent SDK"
lang: ru
track: claude
kind: library
doc: overview
slug: agent-sdk
part: 2
tags: [claude, agent-sdk, agents, tool-use, hooks]
versions: "claude-agent-sdk 0.2.189"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-25
---

# Claude Agent SDK

🌐 **Русский** · [English](overview.en.md)

## Коротко

**Агентный цикл Claude Code как библиотека** для Python и TypeScript: встроенные инструменты, свои инструменты, права, hooks и сессии.

## Ключевые идеи

- `query()` — один запуск, `ClaudeSDKClient` — разговор.
- Права задаются кодом: правила, hooks, callback.
- Проверяйте `ResultMessage`, а не только текст.
- Сообщения об ошибках инструментов видит модель.
- `setting_sources=[]` делает поведение предсказуемым.

## Минимальный пример

```python
import asyncio

from claude_agent_sdk import ClaudeAgentOptions, ResultMessage, query


async def main():
    options = ClaudeAgentOptions(setting_sources=[], max_turns=5)
    async for message in query(prompt="hello", options=options):
        if isinstance(message, ResultMessage):
            print(message.subtype, message.num_turns, message.result)


asyncio.run(main())
```

## Когда применять и когда нет

Когда нужен агент, работающий с файлами и командами в вашем окружении. Для одиночных вызовов модели хватит Anthropic SDK.

## Уровни материала

| Уровень | Файл | Добавить |
|---|---|---|
| 1. Базовая теория | [basics.ru.md](basics.ru.md) | |
| 2. Продвинутая теория | [advanced.ru.md](advanced.ru.md) | |
| 3. Экспертная теория | [expert.ru.md](expert.ru.md) | |
| 4. Самопроверка | [quiz.ru.md](quiz.ru.md), [exercises.ru.md](exercises.ru.md) | |
| 5. Запускаемые примеры | — | `/learn claude agent-sdk level 5` |
| 6. Тесты | — | `/learn claude agent-sdk level 6` |

## Связанные материалы

- [Трек Claude](../../overview.ru.md)
- [12 · Агенты и subagents](../../modules/12-agents/overview.ru.md)
- [Anthropic SDK](../anthropic-sdk/overview.ru.md)
- [MCP SDK](../mcp-sdk/overview.ru.md)
