---
title: "Structured outputs: основы"
lang: ru
track: claude
kind: module
doc: basics
slug: 04-structured-output
part: 1
tags: [claude, structured-outputs, json-schema, pydantic, output-config, stop-reason]
versions: "Claude 5.5 family; Messages API, anthropic-version 2026-10-21; Python SDK anthropic 2.13.0"
status: draft
updated: 2026-11-24
---

# Structured outputs: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [02 · Messages API](../02-messages-api/overview.ru.md), [Python](../../../python/overview.ru.md) · **Время:** ~1 ч

## Как проверялся код

Ключа API при подготовке не было, поэтому код запускался против **локального мок-сервера** (как в [модуле 02](../02-messages-api/basics.ru.md)): он принимает запросы SDK `anthropic` 2.13.0, сохраняет тело и возвращает JSON, соответствующий схеме. Проверены то, что делает SDK (построение и преобразование схемы, форма запроса, разбор в Pydantic) и ваш код; **ответы реальных моделей и работа настоящего ограничения декодирования не проверялись**. Факты о схемах и ограничениях взяты из страниц документации «Structured outputs» и «Strict tool use»; часть этих страниц прочитана в сокращённом пересказе.

## Зачем это нужно

Просьба «верни JSON» в промпте работает не всегда: ответ может содержать пояснение, лишние поля или битый JSON. Structured outputs переносят требование из текста в параметр запроса и дают ответ в заданной форме, который можно сразу разбирать кодом.

## Схема в output_config.format

Формат ответа задаёт параметр `output_config.format` с типом `json_schema` и самой схемой:

```json
"output_config": {"format": {"type": "json_schema", "schema": {"type": "object", "properties": {...}, "required": [...], "additionalProperties": false}}}
```

Старый параметр `output_format` объявлен устаревшим: используйте `output_config.format` (бета-заголовок больше не нужен).

## messages.parse и Pydantic

В Python SDK удобнее всего описать структуру моделью Pydantic и вызвать `client.messages.parse(...)`:

```python
from pydantic import BaseModel
import anthropic

class ContactInfo(BaseModel):
    name: str
    email: str
    plan_interest: str
    demo_requested: bool

client = anthropic.Anthropic()
r = client.messages.parse(
    model="claude-opus-5-5", max_tokens=1024,
    messages=[{"role": "user", "content": "Extract: John Smith (john@example.com) wants Enterprise and a demo."}],
    output_format=ContactInfo,
)
print(type(r.parsed_output).__name__, r.parsed_output, r.stop_reason)
```

```text
ContactInfo name='John Smith' email='john@example.com' plan_interest='Enterprise' demo_requested=True end_turn
```

- Класс передаётся как **`output_format=ContactInfo`** (именно этот аргумент `parse`), а результат читается из **`r.parsed_output`** — это уже объект `ContactInfo`, а не словарь.
- SDK сам построил схему и отправил её в `output_config.format`: в опыте в теле запроса был `{"format": {"schema": {"type": "object", "title": "ContactInfo", "properties": {…}, "additionalProperties": false, "required": ["name", "email", "plan_interest", "demo_requested"]}, …}}`; поля `output_format` в теле не было.
- В схему добавлены `"additionalProperties": false` и список `required` со всеми полями модели: SDK приводит схему к допустимому виду.

## messages.create и json.loads

Без помощника схему передают словарём, а JSON приходит **текстом** в блоке `text`; его разбирают и проверяют сами:

```python
import json
schema = {"type": "object",
          "properties": {"name": {"type": "string"}, "email": {"type": "string"},
                         "plan_interest": {"type": "string"}, "demo_requested": {"type": "boolean"}},
          "required": ["name", "email", "plan_interest", "demo_requested"],
          "additionalProperties": False}
m = client.messages.create(model="claude-opus-5-5", max_tokens=1024,
                           messages=[{"role": "user", "content": "x"}],
                           output_config={"format": {"type": "json_schema", "schema": schema}})
text = next(b.text for b in m.content if b.type == "text")
print(json.loads(text)["name"], ContactInfo.model_validate_json(text))
```

```text
John Smith name='John Smith' email='john@example.com' plan_interest='Enterprise' demo_requested=True
```

`ContactInfo.model_validate_json(text)` не только разбирает JSON, но и проверяет типы и ограничения модели: это второй рубеж защиты после схемы.

## Проверяйте stop_reason

Гарантия соответствия схеме действует для **завершённого** ответа. Если генерация оборвана `max_tokens` или модель отказала (`refusal`), текст может быть неполным и не подойти схеме:

```python
m = client.messages.create(model="claude-opus-5-5", max_tokens=5, messages=[{"role": "user", "content": "x"}],
                           output_config={"format": {"type": "json_schema", "schema": schema}})
print(m.stop_reason)
```

```text
max_tokens
```

В опыте мок вернул для малого лимита обрезанный текст `{"name": "John"`: `json.loads` дал `JSONDecodeError`, а `model_validate_json` — `ValidationError`. Поэтому порядок такой: проверить `stop_reason`, затем разобрать и провалидировать. При `max_tokens` повторите запрос с большим лимитом (структурированные ответы нужно закладывать с запасом), при `refusal` ответ не годится для разбора, хотя HTTP-код 200.

## Ошибки новичков

- ❌ Просить «только JSON» в промпте вместо схемы → ✅ `output_config.format`.
- ❌ Читать `content[0].text` как готовый объект → ✅ `json.loads` или `parse` + `parsed_output`.
- ❌ Не проверять `stop_reason` → ✅ `max_tokens` и `refusal`.
- ❌ Считать «валидный JSON» «правильным смыслом» → ✅ проверять значения (Pydantic, правила предметной области).
- ❌ Передавать `output_format` словарём → ✅ `output_config.format` (словарь `output_format` устарел).
- ❌ Слишком малый `max_tokens` → ✅ запас под полный JSON.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — ограничения схем, `enum`, `strict` у инструментов, кэш схемы
- Ранее: [03 · Prompt engineering](../03-prompt-engineering/basics.ru.md#формат-вывода)
- Параллели: [Python](../../../python/overview.ru.md) · [LLM](../../../llm/overview.ru.md)

## Источники

- [Structured outputs](https://platform.claude.com/docs/en/build-with-claude/structured-outputs)
- [Strict tool use](https://platform.claude.com/docs/en/agents-and-tools/tool-use/strict-tool-use)
- [Python SDK](https://platform.claude.com/docs/en/cli-sdks-libraries/sdks/python)
