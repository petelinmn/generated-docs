---
title: "Messages API"
lang: ru
track: claude
kind: module
doc: overview
slug: 02-messages-api
part: 1
levels: [1, 2, 3, 4]
tags: [claude, messages-api, streaming, stop-reason, anthropic-sdk, api-errors]
versions: "Claude 5.5 family; Messages API, anthropic-version 2026-10-21; Python SDK anthropic 2.13.0"
status: draft
updated: 2026-11-24
---

# Messages API

🌐 **Русский** · [English](overview.en.md)

## Коротко

Messages API — основной интерфейс Claude: один эндпоинт `POST /v1/messages`, в который вы отправляете модель, лимит ответа `max_tokens` и список сообщений, а получаете сообщение ассистента с блоками контента, причиной остановки (`stop_reason`) и учётом токенов (`usage`). API **не хранит состояние**: историю диалога каждый раз присылает клиент. Из этого же эндпоинта растут остальные возможности: потоковая выдача, инструменты, мышление, структурированный вывод.

## Ключевые идеи

- **Запрос:** `model`, `max_tokens` и `messages` обязательны; роль сообщения — `user` или `assistant`; `system` задаётся отдельным параметром.
- **Ответ — не строка, а объект `Message`:** `content` — список блоков (`text`, `tool_use`, `thinking`, …), поэтому перед чтением `.text` проверяют `block.type`.
- **API не помнит диалог:** каждый запрос содержит всю историю.
- **`stop_reason` объясняет, почему остановилась генерация:** `end_turn`, `max_tokens`, `stop_sequence`, `tool_use`, `pause_turn`, `refusal`; его нужно проверять до использования `content`.
- **`max_tokens` — жёсткий потолок вывода:** при его достижении ответ обрывается на полуслове.
- **Стриминг** (`stream: true`, события SSE) обязателен для больших ответов и длинных запросов; SDK умеет собрать итоговое сообщение.
- **Ошибки типизированы:** HTTP-код и поле `error.type`; SDK автоматически повторяет временные сбои дважды.
- **Версия API** задаётся заголовком `anthropic-version`; SDK выставляет его сам.
- **На новых моделях часть параметров изменена:** `temperature`, `top_p` и `top_k` при версии API `2026-10-21` дают 400, а мышление управляется через `thinking` и `output_config.effort`.

## Минимальный пример

```python
import anthropic

client = anthropic.Anthropic()  # ключ берётся из ANTHROPIC_API_KEY

message = client.messages.create(
    model="claude-opus-5-5",
    max_tokens=1024,
    messages=[{"role": "user", "content": "What is the capital of France?"}],
)
for block in message.content:
    if block.type == "text":
        print(block.text)
print(message.stop_reason, message.usage.output_tokens)
```

```text
Paris.
end_turn 6
```

Вывод получен от локального мок-сервера (см. раздел «Как проверялся код» в [базовой теории](basics.ru.md)), поэтому текст ответа условный; настоящая модель ответит иначе.

## Когда применять и когда нет

| Задача | Выбор |
|---|---|
| Вопрос-ответ, суммаризация, извлечение, классификация | один запрос Messages API |
| Длинный ответ, чат с выводом по мере генерации | потоковая выдача |
| Тысячи независимых запросов без срочности | Message Batches (модуль 14 дорожной карты) |
| Модель должна вызывать ваши функции | tool use ([модуль 07](../../overview.ru.md) дорожной карты) |
| Ответ строго в формате JSON | structured outputs (модуль 04 дорожной карты) |
| Автономный агент с песочницей | Managed Agents или Claude Code (модули 09 и 12 дорожной карты) |

- ❌ Не храните диалог «на стороне модели»: без истории в `messages` она ничего не помнит.
- ❌ Не читайте `message.content[0].text` без проверки типа блока и `stop_reason`.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | запрос и ответ, роли, `system`, диалог, `max_tokens`, `stop_reason`, ошибки |
| [2. Продвинутая теория](advanced.ru.md) | стриминг, `stop_sequences`, мышление и effort, повторы и таймауты, `count_tokens`, request ID |
| [3. Экспертная теория](expert.ru.md) | справочник параметров и полей, протокол SSE, таблица ошибок, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | пока не создан — `/learn claude 02-messages-api level 5` |
| 6. Тесты | пока не создан — `/learn claude 02-messages-api level 6` |

## Связанные материалы

- Пререквизиты: [01 · Семейство Claude и выбор модели](../01-models/overview.ru.md)
- Дальше: [03 · Prompt engineering](../03-prompt-engineering/overview.ru.md)
- Аналоги в других треках: [LLM](../../../llm/overview.ru.md), [Python](../../../python/overview.ru.md) (SDK-примеры), [JavaScript](../../../javascript/modules/12-promises-and-async-await/overview.ru.md) (асинхронные клиенты и `fetch`)
- [17 · Развёртывание и production](../17-production/overview.ru.md)
