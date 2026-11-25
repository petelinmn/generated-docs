---
title: "Компактизация и длинный контекст: упражнения"
lang: ru
track: claude
kind: module
doc: exercises
slug: 13-long-context
part: 3
tags: [claude, context-window, compaction, context-editing, token-counting]
versions: "Claude 5.5 family; anthropic 2.14.0; beta compact-2026-09-04, compact-2026-01-12, context-management-2025-06-27; Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# Компактизация и длинный контекст: упражнения

🌐 **Русский** · [English](exercises.en.md)

> Упражнениям 1, 3 и 4 нужен только Python; 2 и 5 — SDK `anthropic` и ключ API.

## 1. Проверка обрезки

Убедитесь, что `trim` не начинает историю с `tool_result`.

<details><summary>Подсказка</summary>

Пара из `tool_use` и `tool_result`.

</details>

<details><summary>Решение</summary>

```python
from history import trim

messages = [
    {"role": "user", "content": "q"},
    {"role": "assistant", "content": [{"type": "tool_use", "id": "t1", "name": "f", "input": {}}]},
    {"role": "user", "content": [{"type": "tool_result", "tool_use_id": "t1", "content": "r"}]},
    {"role": "assistant", "content": "done"},
]
print([m["role"] for m in trim(messages, 2)])
```

</details>

## 2. Бюджет перед отправкой

Не отправляйте запрос длиннее заданного числа токенов.

<details><summary>Подсказка</summary>

`count_tokens`.

</details>

<details><summary>Решение</summary>

```python
def fits(client, messages, limit):
    return client.messages.count_tokens(model="claude-opus-5-5", messages=messages).input_tokens <= limit
```

</details>

## 3. Остаток окна

Посчитайте, сколько остаётся при окне 1M, входе `n` и `max_tokens`.

<details><summary>Подсказка</summary>

Вход плюс вывод.

</details>

<details><summary>Решение</summary>

```python
def headroom(input_tokens, max_tokens, window=1_000_000):
    return window - input_tokens - max_tokens

print(headroom(900_000, 128_000))
```

</details>

## 4. Компактизация с заглушкой

Проверьте `compact` без API.

<details><summary>Подсказка</summary>

Объект с `beta.messages.create`.

</details>

<details><summary>Решение</summary>

```python
from types import SimpleNamespace

from compact import compact


def fake(stop_reason):
    reply = SimpleNamespace(stop_reason=stop_reason, content=["BLOCK"])
    return SimpleNamespace(beta=SimpleNamespace(messages=SimpleNamespace(create=lambda **kw: reply)))


history = [{"role": "user", "content": "hi"}]
print(compact(fake("compaction"), history))
print(compact(fake("max_tokens"), history) is history)
```

</details>

## 5. Очистка

Настройте очистку, сохраняющую 5 последних пар и результаты инструмента `search_docs`.

<details><summary>Подсказка</summary>

`keep`, `exclude_tools`.

</details>

<details><summary>Решение</summary>

```python
edit = {"type": "clear_tool_uses_20250919",
        "keep": {"type": "tool_uses", "value": 5},
        "exclude_tools": ["search_docs"]}
```

</details>
