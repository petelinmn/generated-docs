---
title: "Structured outputs: продвинутый уровень"
lang: ru
track: claude
kind: module
doc: advanced
slug: 04-structured-output
part: 1
tags: [claude, structured-outputs, json-schema, transform-schema, enum, strict-tool-use, schema-cache, validation]
versions: "Claude 5.5 family; Messages API, anthropic-version 2026-10-21; Python SDK anthropic 2.13.0"
status: draft
updated: 2026-11-24
---

# Structured outputs: продвинутый уровень

🌐 **Русский** · [English](advanced.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · **2. Продвинутый** · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

Опирается на [базовую теорию](basics.ru.md). Код запускался против локального мок-сервера и локальных функций SDK; ответы настоящих моделей и ограничение декодирования не проверялись. Правила схем взяты из документации (часть — в сокращённом пересказе страницы).

## Что делает SDK со схемой: transform_schema

Функция `anthropic.transform_schema(...)` возвращает схему в виде, пригодном для API, без отправки запроса; принимает и словарь, и класс Pydantic:

```python
from anthropic import transform_schema
t = transform_schema(ContactInfo)
print(sorted(t), t.get("additionalProperties"), t["required"])
```

```text
['additionalProperties', 'properties', 'required', 'title', 'type'] False ['name', 'email', 'plan_interest', 'demo_requested']
```

- В схему добавлены `additionalProperties: false` и `required` для всех полей.
- Ограничения, которые API не поддерживает, **переносятся в описание**: для `score: int = Field(ge=1, le=5, description="rating")` SDK оставил `{"type": "integer", "title": "Score", "description": "rating\n\n{maximum: 5, minimum: 1}"}` — числовых границ в схеме нет, они лишь подсказка модели.
- Отсюда правило: **диапазоны и длины проверяйте сами** после разбора. В опыте `Rating.model_validate_json('{"score": 9}')` отклонил значение с ошибкой `less_than_equal`.

## Что сделать с полями

**Перечисления.** `enum` и `Enum` Pydantic ограничивают набор значений:

```python
class Priority(str, Enum):
    low = "low"; high = "high"
class Ticket(BaseModel):
    title: str
    priority: Priority
    assignee: Optional[str] = None
s = transform_schema(Ticket)
print(json.dumps(s["properties"]["priority"]), s["required"], json.dumps(s["properties"]["assignee"]))
```

```text
{"$ref": "#/$defs/Priority"} ['title', 'priority'] {"anyOf": [{"type": "string"}, {"type": "null"}], "title": "Assignee", "description": "{default: None}"}
```

- Перечисление превратилось в ссылку `$ref` на определение в `$defs`; необязательное поле — в `anyOf` из строки и `null`, при этом оно не попадает в `required`, а значение по умолчанию уходит в описание.
- Регистр значений перечисления в ответе может отличаться: сравнивайте без учёта регистра.

**Словари.** Поле `dict` не имеет известных ключей: схема получает пустые `properties`, и модель сможет вернуть только `{}`. В опыте `tags: dict` превратилось в `{"type": "object", "title": "Tags", "properties": {}, "additionalProperties": false}`. Если ключи известны, опишите их вложенной моделью; если нет, используйте список пар `{key, value}`.

## Ограничения схемы

Из документации (часть — в сокращённом пересказе):

| Поддерживается | Не поддерживается |
|---|---|
| базовые типы, `enum`/`const` (строки, числа, булевы, `null`), `anyOf`, `$ref`/`$defs`, `required`, форматы строк (`date-time`, `date`, `email`, `uuid`, `uri` …), `minItems` равное 0 или 1 | рекурсивные схемы, `oneOf` (используйте `anyOf`), `not`, внешние `$ref`, `minimum`/`maximum`/`multipleOf`, `minLength`/`maxLength`, `additionalProperties` кроме `false` |

Неподдерживаемое возвращает 400. Слишком сложная схема тоже даёт 400 (`Schema is too complex for compilation`): компиляция ограничена по времени.

> ⚠️ Проверить: точный перечень поддерживаемых ключевых слов и пределы сложности взяты из сокращённого пересказа страницы; перед использованием сверьтесь со страницей «Structured outputs».

## Strict tool use

У инструмента есть собственный флаг `strict: true` — на верхнем уровне определения, рядом с `name`, `description`, `input_schema`:

```python
client.messages.create(
    model="claude-opus-5-5", max_tokens=100,
    messages=[{"role": "user", "content": "x"}],
    tools=[{"name": "get_weather", "description": "d", "strict": True,
            "input_schema": {"type": "object", "properties": {"city": {"type": "string"}},
                             "required": ["city"], "additionalProperties": False}}],
)
```

В опыте тело запроса содержало `strict: true` у инструмента (`['name', 'description', 'strict', 'input_schema']`). Что даёт `strict`: аргументы вызова **строго соответствуют** `input_schema`, а имя инструмента всегда допустимо. Различие подходов:

| | JSON outputs | Strict tool use |
|---|---|---|
| Что ограничивает | формат **ответа** модели | **аргументы** вызова инструмента |
| Параметр | `output_config.format` | `strict: true` у инструмента |
| Читается из | блока `text` | блока `tool_use` (`input`) |

Их можно сочетать в одном запросе. Подробно инструменты — в модуле 07 дорожной карты.

## Кэш схемы и задержка

- Первый запрос с новой схемой медленнее: схема компилируется в грамматику; результат кэшируется (по документации до 24 часов с последнего использования).
- Изменение **структуры** схемы сбрасывает этот кэш, а изменение только `name`/`description` — нет.
- Смена `output_config.format` инвалидирует кэш промпта для этой ветки диалога ([модуль 05](../../overview.ru.md) дорожной карты): держите схему стабильной.

> ⚠️ Проверить: сроки и условия кэша схемы, лимит компиляции (около 180 секунд) и влияние на кэш промпта взяты из пересказа документации и в опытах не измерялись.

## Повторы при неполном ответе

Валидный по схеме ответ получается не всегда: бывают `max_tokens` и `refusal`. Рабочий приём — проверка и ограниченное число повторов:

```python
def extract_with_retry(call, attempts=2):
    for i in range(attempts):
        text = call(i)
        try:
            return Contact.model_validate_json(text), i + 1
        except ValidationError:
            continue
    raise ValueError("no valid output")
```

В опыте первая попытка вернула обрезанный JSON, вторая — валидный: результат `(Contact(...), 2)`. Повтор бесполезен при `refusal` (причина не в формате) и при `max_tokens` без увеличения лимита.

## Лучшие практики

- Описывайте структуру моделью Pydantic и берите `parsed_output`.
- Задавайте простые схемы: без рекурсии и `oneOf`.
- Проверяйте диапазоны, длины и бизнес-правила после разбора.
- Закладывайте запас в `max_tokens` и проверяйте `stop_reason`.
- Не меняйте схему без необходимости (кэш).
- Для вызова функций используйте `strict: true`, для формата ответа — JSON outputs.

## Частые ошибки

- ❌ Ждать, что `Field(ge=1, le=5)` ограничит модель → ✅ это только подсказка; проверять самим.
- ❌ Поле `dict` для неизвестных ключей → ✅ список пар или вложенная модель.
- ❌ `oneOf`/рекурсия → ✅ `anyOf`, плоская схема.
- ❌ Цитаты и structured outputs в одном запросе → ✅ они несовместимы (400).
- ❌ Prefill вместе со схемой → ✅ не использовать.
- ❌ Считать повтор лекарством от `refusal` → ✅ изменить запрос.

## Связанные материалы

- Дальше: [экспертная теория](expert.ru.md)
- Формат вывода через промпт: [03 · Prompt engineering](../03-prompt-engineering/advanced.ru.md)
- Параметры запроса и `stop_reason`: [02 · Messages API](../02-messages-api/expert.ru.md)

## Источники

- [Structured outputs](https://platform.claude.com/docs/en/build-with-claude/structured-outputs)
- [Strict tool use](https://platform.claude.com/docs/en/agents-and-tools/tool-use/strict-tool-use)
