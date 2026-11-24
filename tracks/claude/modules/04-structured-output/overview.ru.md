---
title: "Structured outputs"
lang: ru
track: claude
kind: module
doc: overview
slug: 04-structured-output
part: 1
levels: [1, 2, 3, 4]
tags: [claude, structured-outputs, json-schema, pydantic, strict-tool-use, output-config]
versions: "Claude 5.5 family; Messages API, anthropic-version 2026-10-21; Python SDK anthropic 2.13.0"
status: draft
updated: 2026-11-24
---

# Structured outputs

🌐 **Русский** · [English](overview.en.md)

## Коротко

Structured outputs заставляют Claude возвращать ответ, соответствующий заданной JSON-схеме: вместо просьбы «верни JSON» в промпте вы передаёте схему в `output_config.format`, и ответ приходит в виде валидного JSON. В Python SDK это удобнее всего делать через `client.messages.parse(...)` с моделью Pydantic: SDK строит схему, отправляет запрос и возвращает готовый объект в `parsed_output`. Отдельная возможность — `strict: true` у инструмента, которая гарантирует, что аргументы вызова соответствуют схеме инструмента.

## Ключевые идеи

- **Схема — в параметре, а не в тексте:** `output_config: {"format": {"type": "json_schema", "schema": {...}}}`.
- **`messages.parse` + Pydantic** — рекомендуемый путь в Python: `output_format=Model` → `response.parsed_output`.
- **С `messages.create` JSON не разбирается сам:** он лежит текстом в блоке `text`; вы вызываете `json.loads` и валидируете.
- **У схемы есть ограничения:** `additionalProperties` должен быть `false`, нет рекурсии, `oneOf`, числовых и строковых ограничений (`minimum`, `maxLength`…); SDK переносит такие ограничения в описание.
- **Гарантия касается формата, а не смысла:** значения всё равно нужно проверять.
- **Неполный или отказной ответ может не соответствовать схеме:** проверяйте `stop_reason` (`max_tokens`, `refusal`).
- **`strict: true` у инструмента** ограничивает аргументы вызова схемой; JSON outputs управляют форматом ответа, и их можно сочетать.
- **Первая отправка новой схемы медленнее:** схема компилируется и кэшируется (по документации до 24 часов).
- **Несовместимо с цитатами и prefill.**

## Минимальный пример

```python
from pydantic import BaseModel
import anthropic

class ContactInfo(BaseModel):
    name: str
    email: str
    plan_interest: str
    demo_requested: bool

client = anthropic.Anthropic()
response = client.messages.parse(
    model="claude-opus-5-5",
    max_tokens=1024,
    messages=[{"role": "user", "content": "Extract: John Smith (john@example.com) wants Enterprise and a demo."}],
    output_format=ContactInfo,
)
print(response.parsed_output)
```

```text
name='John Smith' email='john@example.com' plan_interest='Enterprise' demo_requested=True
```

Вывод получен от локального мок-сервера (см. «Как проверялся код» в [базовой теории](basics.ru.md)): проверены построение схемы и разбор, но не ответы реальных моделей.

## Когда применять и когда нет

| Задача | Выбор |
|---|---|
| Извлечение данных в известную структуру | `messages.parse` + модель Pydantic |
| Классификация с фиксированным набором значений | `enum` в схеме |
| Модель вызывает функции | инструменты (модуль 07 дорожной карты), `strict: true` |
| Свободный текст | без схемы |
| Схема с рекурсией или `oneOf` | упростить или валидировать самому |
| Строгие числовые диапазоны | валидация на вашей стороне (Pydantic `Field(ge, le)`) |

- ❌ Не полагайтесь на текст «верни только JSON»: схема надёжнее.
- ❌ Не считайте валидный JSON правильным по смыслу.

## Уровни материала

| Уровень | Что внутри |
|---|---|
| [1. Базовая теория](basics.ru.md) | `output_config.format`, `parse` и Pydantic, `create` и `json.loads`, разбор `stop_reason` |
| [2. Продвинутая теория](advanced.ru.md) | ограничения схемы и `transform_schema`, `enum` и `Optional`, `strict` у инструментов, кэш схемы, повторы |
| [3. Экспертная теория](expert.ru.md) | справочник поддерживаемых ключевых слов, ошибки, совместимость, подвохи, вопросы с собеседований |
| [4. Самопроверка](quiz.ru.md) | квиз и [упражнения](exercises.ru.md) |
| 5. Запускаемые примеры | пока не создан — `/learn claude 04-structured-output level 5` |
| 6. Тесты | пока не создан — `/learn claude 04-structured-output level 6` |

## Связанные материалы

- Пререквизиты: [02 · Messages API](../02-messages-api/overview.ru.md), [03 · Prompt engineering](../03-prompt-engineering/overview.ru.md)
- Дальше: [05 · Prompt caching](../05-prompt-caching/overview.ru.md)
- Аналоги в других треках: [LLM](../../../llm/overview.ru.md), [Python](../../../python/overview.ru.md) (Pydantic), [TypeScript](../../../typescript/overview.ru.md) (схемы и типы)
