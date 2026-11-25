---
title: "Tool use"
lang: ru
track: claude
kind: module
doc: overview
slug: 07-tool-use
part: 1
levels: [1, 2, 3, 4]
tags: [claude, tool-use, function-calling, tool-choice, parallel-tool-calls, agent-loop]
versions: "Claude 5.5 family; Messages API, anthropic-version 2026-10-21; Python SDK anthropic 2.13.0"
status: draft
updated: 2026-11-24
---

# Tool use

🌐 **Русский** · [English](overview.en.md)

## Коротко

Claude не может сам вызвать ваш код. Вы описываете инструменты (имя, описание и JSON Schema входа), и когда модель решает, что инструмент нужен, она отвечает блоком `tool_use` и `stop_reason: "tool_use"`. Ваша программа выполняет функцию и возвращает результат блоком `tool_result`; Claude продолжает. Повторение этого до обычного `end_turn` — **агентный цикл** (agent loop): основа любого помощника, который делает что-то кроме разговора.

## Ключевые идеи

- **Инструмент — это описание:** `name`, `description`, `input_schema` (JSON Schema); необязательно `strict`, `input_examples`, `cache_control`.
- **Модель только просит.** Блок `tool_use` содержит `id`, `name` и `input`; код выполняет ваше приложение (*клиентский инструмент*) или серверы Anthropic (*серверный инструмент*, например веб-поиск).
- **Цикл:** добавьте ответ как сообщение `assistant`, выполните каждый `tool_use`, отправьте одно сообщение `user` со всеми блоками `tool_result`, повторите.
- **Порядок важен:** блоки `tool_result` идут первыми в сообщении пользователя и сразу за ходом ассистента; текст — после них или вовсе без него.
- **Параллельные вызовы:** в одном ответе может быть несколько `tool_use`; все результаты возвращайте в **одном** сообщении.
- **Ошибки:** возвращайте `is_error: true` с понятным текстом; вызов нельзя просто пропустить.
- **`tool_choice`:** `auto` (по умолчанию), `any`, `tool`, `none`; `disable_parallel_tool_use` ограничивает один вызов. Принудительные `any`/`tool` на перечисленных в документации моделях Claude 5.5 дают 400.
- **Стоимость:** системный промпт для инструментов добавляет около 286 токенов на Opus 5.5 и Sonnet 5.5, плюс сами описания.
- **Всегда ограничивайте число шагов цикла.**

## Минимальный пример

```python
tools = [{"name": "get_weather", "description": "Get the current weather in a city.",
          "input_schema": {"type": "object", "properties": {"location": {"type": "string"}}, "required": ["location"]}}]
r = client.messages.create(model="claude-opus-5-5", max_tokens=1024, tools=tools,
                           messages=[{"role": "user", "content": "Weather in Paris?"}])
print(r.stop_reason)   # tool_use: run the function and send a tool_result
```

Полный цикл и его вывод — в [базовой теории](basics.ru.md). Код проверялся на локальном mock-сервере, а не на реальной модели.

## Когда применять и когда нет

| Задача | Выбор |
|---|---|
| Модели нужны свежие данные или действие (база, API, файлы) | клиентские инструменты |
| Веб-поиск, загрузка страниц, выполнение кода без своей песочницы | серверные инструменты |
| Нужен только структурированный ответ без побочных эффектов | [structured outputs](../04-structured-output/overview.ru.md), а не инструменты |
| Строго вызвать один инструмент | `strict`-инструменты и structured output (принудительный `tool_choice` на моделях 5.5 ограничен) |
| Фиксированная последовательность шагов известна заранее | обычный сценарий в коде; инструменты нужны, когда решает модель |

- ❌ Не выполняйте инструмент по вводу модели без проверки аргументов и прав.
- ❌ Не оставляйте цикл без лимита шагов.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | описание, цикл, `tool_result`, параллельные вызовы, ошибки, `tool_choice` |
| [2. Продвинутая теория](advanced.ru.md) | серверные инструменты, `pause_turn`, описания, принудительный вызов и модели, стоимость, кэширование, tool runner в SDK |
| [3. Экспертная теория](expert.ru.md) | справочник полей, stop reasons и типов инструментов, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | пока не создан — `/learn claude 07-tool-use level 5` |
| 6. Тесты | пока не созданы — `/learn claude 07-tool-use level 6` |

## Связанные материалы

- Пререквизиты: [02 · Messages API](../02-messages-api/overview.ru.md), [04 · Structured outputs](../04-structured-output/overview.ru.md), [05 · Prompt caching](../05-prompt-caching/overview.ru.md)
- Предыдущий: [06 · Документы и цитаты](../06-documents-and-citations/overview.ru.md)
- Дальше: [08 · MCP и connectors](../08-mcp/overview.ru.md)
- Аналоги в других треках: [LLM](../../../llm/overview.ru.md), [Python](../../../python/overview.ru.md)
- [12 · Агенты и subagents](../12-agents/overview.ru.md)
- [16 · Безопасность и compliance](../16-security/overview.ru.md)
