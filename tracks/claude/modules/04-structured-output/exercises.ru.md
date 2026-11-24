---
title: "Упражнения: structured outputs"
lang: ru
track: claude
kind: module
doc: exercises
slug: 04-structured-output
part: 1
tags: [claude, structured-outputs, pydantic, json-schema, stop-reason, validation]
status: draft
updated: 2026-11-24
---

# Упражнения: structured outputs

🌐 **Русский** · [English](exercises.en.md)

> Решайте самостоятельно, подсказки и решения под спойлерами. Решения проверены против локального мок-сервера (см. «Как проверялся код» в [базовой теории](basics.ru.md)); выводы — ответы мока. С настоящим ключом используйте `client = anthropic.Anthropic()`. Общая модель:

```python
from pydantic import BaseModel
class Contact(BaseModel):
    name: str
    email: str
    plan_interest: str
    demo_requested: bool
```

## 1. 🟢 Функция `extract` через `parse`

**Задача:** `extract(client, text)` вызывает `client.messages.parse(…, output_format=Contact)` и возвращает `parsed_output`; если `stop_reason` не `end_turn`, бросает `RuntimeError`.

**Ожидаемый результат:** `name='John Smith' email='john@example.com' plan_interest='Enterprise' demo_requested=True`.

<details><summary>Подсказка</summary>

Проверка `stop_reason` — до чтения `parsed_output`.

</details>

<details><summary>Решение</summary>

```python
def extract(client, text):
    r = client.messages.parse(model="claude-opus-5-5", max_tokens=1024, messages=[{"role": "user", "content": f"Extract the contact: {text}"}], output_format=Contact)
    if r.stop_reason != "end_turn": raise RuntimeError(f"incomplete: {r.stop_reason}")
    return r.parsed_output
```

```text
name='John Smith' email='john@example.com' plan_interest='Enterprise' demo_requested=True
```

</details>

## 2. 🟢 Ручная схема и `create`

**Задача:** получите схему через `transform_schema(Contact)`, отправьте её в `output_config.format` через `messages.create`, разберите текст через `Contact.model_validate_json`. При `max_tokens` бросайте `Truncated`.

**Ожидаемый результат:** для нормального запроса `john@example.com`; для малого `max_tokens` — `Truncated: raise max_tokens`.

<details><summary>Подсказка</summary>

Текст — в блоке `type == "text"`; `from anthropic import transform_schema`.

</details>

<details><summary>Решение</summary>

```python
from anthropic import transform_schema
class Truncated(Exception): pass

def extract_raw(client, text, max_tokens):
    schema = transform_schema(Contact)
    m = client.messages.create(model="claude-opus-5-5", max_tokens=max_tokens, messages=[{"role": "user", "content": text}], output_config={"format": {"type": "json_schema", "schema": schema}})
    if m.stop_reason == "max_tokens": raise Truncated("raise max_tokens")
    return Contact.model_validate_json(next(b.text for b in m.content if b.type == "text"))
```

```text
john@example.com
Truncated: raise max_tokens
```

</details>

## 3. 🟡 Перечисление и необязательное поле

**Задача:** опишите `Ticket` с полями `title: str`, `priority` (перечисление `low`/`high`) и `assignee: Optional[str] = None`. Выведите схему поля `priority`, список `required` и схему `assignee` через `transform_schema`.

**Ожидаемый результат:** `{"$ref": "#/$defs/Priority"} ['title', 'priority'] {"anyOf": [{"type": "string"}, {"type": "null"}], "title": "Assignee", "description": "{default: None}"}`.

<details><summary>Подсказка</summary>

`class Priority(str, Enum)`; необязательное поле не попадает в `required`.

</details>

<details><summary>Решение</summary>

```python
from enum import Enum
from typing import Optional
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

</details>

## 4. 🟡 Ограничения не доходят до модели

**Задача:** модель `Rating` с `score: int = Field(ge=1, le=5)`. Покажите, во что превращается поле в схеме, и что проверка значения `9` срабатывает при разборе.

**Ожидаемый результат:** `{"type": "integer", "title": "Score", "description": "{maximum: 5, minimum: 1}"}` и `rejected: less_than_equal`.

<details><summary>Подсказка</summary>

`Rating.model_validate_json('{"score": 9}')` внутри `try/except ValidationError`; тип ошибки — `e.errors()[0]["type"]`.

</details>

<details><summary>Решение</summary>

```python
from pydantic import Field, ValidationError
class Rating(BaseModel):
    score: int = Field(ge=1, le=5)
print(json.dumps(transform_schema(Rating)["properties"]["score"]))
try: Rating.model_validate_json('{"score": 9}')
except ValidationError as e: print("rejected:", e.errors()[0]["type"])
```

```text
{"type": "integer", "title": "Score", "description": "{maximum: 5, minimum: 1}"}
rejected: less_than_equal
```

</details>

## 5. 🔴 Повтор при неверном выводе

**Задача:** `extract_with_retry(call, attempts=2)`: вызывает `call(i)`, который возвращает JSON-текст; разбирает через `Contact.model_validate_json`; при `ValidationError` пробует снова; возвращает пару `(объект, номер попытки)` или бросает `ValueError("no valid output")`.

**Ожидаемый результат:** если первая попытка вернула `{"name": "A"`, а вторая валидный JSON, — `(Contact(name='A', email='a@b.c', plan_interest='Pro', demo_requested=False), 2)`.

<details><summary>Подсказка</summary>

Цикл по `range(attempts)`, `continue` в `except`.

</details>

<details><summary>Решение</summary>

```python
def extract_with_retry(call, attempts=2):
    for i in range(attempts):
        text = call(i)
        try: return Contact.model_validate_json(text), i + 1
        except ValidationError: continue
    raise ValueError("no valid output")

good = '{"name":"A","email":"a@b.c","plan_interest":"Pro","demo_requested":false}'
print(extract_with_retry(lambda i: '{"name": "A"' if i == 0 else good))
```

```text
(Contact(name='A', email='a@b.c', plan_interest='Pro', demo_requested=False), 2)
```

Повтор имеет смысл при обрыве и нестабильном формате; при `refusal` его нужно заменить изменением запроса.

</details>
