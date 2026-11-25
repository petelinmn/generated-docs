---
title: "Anthropic SDK"
lang: ru
track: claude
kind: library
doc: overview
slug: anthropic-sdk
part: 2
tags: [claude, sdk, anthropic-sdk, python, typescript]
versions: "anthropic 2.14.0; @anthropic-ai/sdk 1.14.0"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-25
---

# Anthropic SDK

🌐 **Русский** · [English](overview.en.md)

## Коротко

**Официальные клиенты Claude API** для Python, TypeScript и других языков: типы, повторы, стриминг, пагинация и помощники для инструментов.

## Ключевые идеи

- Один клиент на приложение; настройки на копии через `with_options`.
- SDK сам повторяет временные ошибки, но не всё.
- `stream()` накапливает сообщение за вас.
- Tool runner по умолчанию использует серверный thread.
- Тестировать можно без сети, подменив транспорт.

## Минимальный пример

```python
from anthropic import Anthropic

client = Anthropic()                      # reads ANTHROPIC_API_KEY
message = client.messages.create(
    model="claude-opus-5-5", max_tokens=1024,
    messages=[{"role": "user", "content": "hi"}],
)
print(message.content[0].text, message._request_id)
```

## Когда применять и когда нет

Почти всегда при вызове API из кода. Для готового агента с инструментами берите Agent SDK; чистый HTTP — если SDK для языка нет.

## Уровни материала

| Уровень | Файл | Добавить |
|---|---|---|
| 1. Базовая теория | [basics.ru.md](basics.ru.md) | |
| 2. Продвинутая теория | [advanced.ru.md](advanced.ru.md) | |
| 3. Экспертная теория | [expert.ru.md](expert.ru.md) | |
| 4. Самопроверка | [quiz.ru.md](quiz.ru.md), [exercises.ru.md](exercises.ru.md) | |
| 5. Запускаемые примеры | — | `/learn claude anthropic-sdk level 5` |
| 6. Тесты | — | `/learn claude anthropic-sdk level 6` |

## Связанные материалы

- [Трек Claude](../../overview.ru.md)
- [02 · Messages API](../../modules/02-messages-api/overview.ru.md)
- [07 · Tool use](../../modules/07-tool-use/overview.ru.md)
- [Claude Agent SDK](../agent-sdk/overview.ru.md)
