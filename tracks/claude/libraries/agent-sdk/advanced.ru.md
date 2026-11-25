---
title: "Claude Agent SDK: продвинутый уровень"
lang: ru
track: claude
kind: library
doc: advanced
slug: agent-sdk
part: 2
tags: [claude, agent-sdk, agents, tool-use, hooks]
versions: "claude-agent-sdk 0.2.189"
status: draft
updated: 2026-11-25
---

# Claude Agent SDK: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

## Права

| Средство | Роль |
|---|---|
| `allowed_tools` | разрешить без вопроса |
| `disallowed_tools` | запретить |
| `permission_mode` | общий режим |
| `can_use_tool` | решить в коде то, что не решено правилами |
| `PreToolUse` hook | проверить каждый вызов |

**`allowed_tools` затеняет `can_use_tool`:** для разрешённого целиком инструмента callback не вызывается, и SDK выдаёт `CanUseToolShadowedWarning`. Чтобы проверять каждый вызов, используйте hook.

## Hooks

```python
# guard.py
from claude_agent_sdk import ClaudeAgentOptions, HookMatcher

from shop_tools import NAME, SERVER


async def freeze(input_data, tool_use_id, context):
    if input_data["tool_input"].get("order_id", "").startswith("X-"):
        return {"hookSpecificOutput": {"hookEventName": "PreToolUse", "permissionDecision": "deny",
                                       "permissionDecisionReason": "frozen orders"}}
    return {}


OPTIONS = ClaudeAgentOptions(setting_sources=[], mcp_servers={"shop": SERVER}, allowed_tools=[NAME],
                             hooks={"PreToolUse": [HookMatcher(matcher=NAME, hooks=[freeze])]})
```

Отказ hook попадает в результат инструмента с вашей причиной и в `permission_denials`, даже если инструмент есть в `allowed_tools`.

## Ошибки и пределы

```python
import asyncio

from claude_agent_sdk import ClaudeAgentOptions, ResultError, ResultMessage, query


async def run(prompt, **options):
    result = None
    try:
        async for message in query(prompt=prompt, options=ClaudeAgentOptions(setting_sources=[], **options)):
            if isinstance(message, ResultMessage):
                result = message
    except ResultError:
        if result is None:      # the error result is yielded first, then query() raises
            raise
    return result


result = asyncio.run(run("hello", max_turns=3))
print(result.subtype, result.is_error, len(result.permission_denials or []))
```

| Случай | Что видно |
|---|---|
| исчерпан `max_turns` | `ResultMessage` `error_max_turns`, затем `ResultError` |
| несуществующий `resume` | `ResultError` |
| не найден CLI | `CLINotFoundError` |
| отказ в правах | `success`, запись в `permission_denials` |

`success` не значит «задача выполнена»: проверяйте отказы и сам результат. Для расхода есть `max_budget_usd`.

## Сессии

- `resume=<session_id>` продолжает разговор с тем же ID; `fork_session=True` вместе с ним создаёт ветку с новым ID.
- `continue_conversation=True` берёт последнюю сессию каталога.
- Функции: `list_sessions`, `get_session_messages`, `fork_session`, `rename_session`, `delete_session`.
- Сессии — файлы в `~/.claude/projects/` (или `$CLAUDE_CONFIG_DIR/projects/`), привязанные к рабочему каталогу. На другом сервере `resume` их не найдёт без переноса или `session_store`.

## Структурированный результат и поток

`output_format={"type": "json_schema", "schema": ...}` кладёт объект в `structured_output`. `include_partial_messages=True` добавляет `StreamEvent` с частичными ответами.

## Связанные материалы

- [Базовая теория](basics.ru.md) · [Эксперт](expert.ru.md)
- [11 · Skills и hooks](../../modules/11-skills-and-hooks/advanced.ru.md) · [16 · Безопасность](../../modules/16-security/advanced.ru.md)

## Источники

- [Permissions](https://code.claude.com/docs/en/agent-sdk/permissions)
- [Hooks](https://code.claude.com/docs/en/agent-sdk/hooks)
- [Sessions](https://code.claude.com/docs/en/agent-sdk/sessions)
