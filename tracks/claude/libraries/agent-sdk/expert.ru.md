---
title: "Claude Agent SDK: экспертный уровень"
lang: ru
track: claude
kind: library
doc: expert
slug: agent-sdk
part: 2
tags: [claude, agent-sdk, agents, tool-use, hooks]
versions: "claude-agent-sdk 0.2.189"
status: draft
updated: 2026-11-25
---

# Claude Agent SDK: экспертный уровень

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

## Параметры `ClaudeAgentOptions`

| Группа | Поля |
|---|---|
| Модель | `model`, `fallback_model`, `thinking`, `effort`, `betas` |
| Инструменты | `tools`, `allowed_tools`, `disallowed_tools`, `mcp_servers`, `strict_mcp_config` |
| Права | `permission_mode`, `can_use_tool`, `permission_prompt_tool_name`, `sandbox` |
| Пределы | `max_turns`, `max_budget_usd`, `task_budget`, `max_buffer_size` |
| Сессии | `resume`, `session_id`, `continue_conversation`, `fork_session`, `session_store` |
| Контекст | `system_prompt`, `setting_sources`, `skills`, `plugins`, `agents`, `cwd`, `add_dirs` |
| Расширения | `hooks`, `output_format`, `include_partial_messages`, `enable_file_checkpointing` |
| Процесс | `cli_path`, `env`, `extra_args`, `stderr`, `user`, `settings` |

Имена полей взяты из установленного пакета 0.2.189; не все запускались.

## Методы клиента

`connect`, `disconnect`, `query`, `receive_messages`, `receive_response`, `interrupt`, `set_model`, `set_permission_mode`, `get_context_usage`, `get_mcp_status`, `reconnect_mcp_server`, `toggle_mcp_server`, `rewind_files`, `stop_task`, `get_server_info`

## Устройство

SDK запускает CLI подпроцессом и обменивается с ним сообщениями. Запросы разрешений, hooks и свои инструменты CLI передаёт обратно в ваш процесс. Поэтому нужен работоспособный CLI (в пакете он есть), доступен `cli_path`, а переменные окружения и настройки действуют так же, как у Claude Code.

## Подвохи

1. `allowed_tools` — это автоодобрение, а не список доступных инструментов; доступность задают `tools` и `disallowed_tools`.
2. Непойманное исключение инструмента уходит модели дословно. У [MCP SDK](../mcp-sdk/advanced.ru.md) `MCPServer` поведение обратное: он текст скрывает.
3. `query()` с ошибочным результатом сначала отдаёт сообщение, потом бросает исключение.
4. Отказ в правах не делает запуск неуспешным.
5. Без `setting_sources` на разработческой машине подхватятся личные настройки.
6. Сессии содержат переписку и результаты инструментов открытым текстом.

> ⚠️ Проверить: `max_budget_usd`, `session_store`, `sandbox`, `interrupt`, файловые checkpoints и TypeScript SDK не запускались.

## Компромиссы

| Вариант | Когда |
|---|---|
| Agent SDK | агент в вашей инфраструктуре с файлами и командами |
| `claude -p` | скрипт или CI без своего кода |
| Managed Agents | хостинг цикла и песочницы у Anthropic |
| [Client SDK](../anthropic-sdk/overview.ru.md) | полный контроль цикла |

## Вопросы с собеседований

1. Agent SDK или Client SDK?
<details><summary>Ответ</summary>

Agent SDK — готовый цикл с инструментами в вашем окружении; Client SDK — прямые вызовы API и свой цикл.
</details>

2. Где выполняются инструменты?
<details><summary>Ответ</summary>

На вашей машине: встроенные — в процессе CLI, свои — в вашем процессе.
</details>

3. `query()` против `ClaudeSDKClient`?
<details><summary>Ответ</summary>

Первый — один запрос; второй — длящийся разговор с прерыванием и сменой настроек.
</details>

4. Почему `can_use_tool` не вызвался?
<details><summary>Ответ</summary>

Вызов уже разрешён правилом, режимом или `allowed_tools`.
</details>

5. Как проверить каждый вызов?
<details><summary>Ответ</summary>

`PreToolUse` hook.
</details>

6. Что видит модель, если инструмент бросил исключение?
<details><summary>Ответ</summary>

Исходный текст исключения; поэтому сообщения составляют сами и возвращают `is_error`.
</details>

7. Зачем `setting_sources=[]`?
<details><summary>Ответ</summary>

Чтобы чужие настройки, hooks и `CLAUDE.md` с диска не меняли поведение.
</details>

8. Как развернуть сессии на нескольких серверах?
<details><summary>Ответ</summary>

`session_store`, перенос файлов или передача своего состояния в новый запрос.
</details>

9. Как ограничить расход?
<details><summary>Ответ</summary>

`max_turns`, `max_budget_usd`, выбор модели, узкий набор инструментов.
</details>

10. Как тестировать приложение на Agent SDK?
<details><summary>Ответ</summary>

Инструменты и hooks — обычными юнит-тестами; прогон агента — против заглушки или в изолированном окружении.
</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md) · [Квиз](quiz.ru.md)
- [12 · Агенты](../../modules/12-agents/expert.ru.md) · [17 · Развёртывание](../../modules/17-production/expert.ru.md)

## Источники

- [Python reference](https://code.claude.com/docs/en/agent-sdk/python)
- [Sessions](https://code.claude.com/docs/en/agent-sdk/sessions)
