---
title: "Упражнения: tool use"
lang: ru
track: claude
kind: module
doc: exercises
slug: 07-tool-use
part: 1
tags: [claude, tool-use, tool-choice, agent-loop]
status: draft
updated: 2026-11-24
---

# Упражнения: tool use

🌐 **Русский** · [English](exercises.en.md)

> Решайте сами; подсказки и решения — под спойлерами. Решения — локальные функции Python с объектами-заменителями вместо настоящих ответов (см. «Как проверялся код» в [базовой теории](basics.ru.md)); реальные решения модели не проверялись.

Заменители: `NS` — это `types.SimpleNamespace`; блок — `NS(type="tool_use", id="t1", name="add", input={"a": 1, "b": 2})`.

## 1. 🟢 Результаты вызовов

**Задача:** функция `tool_result_blocks(content, handlers)` возвращает `tool_result` для каждого блока `tool_use` ответа (остальные блоки игнорирует). Ошибка обработчика становится `is_error: True` с текстом `ИмяИсключения: сообщение`. Запустите на вызовах `add(1, 2)`, неизвестного `nope` и `add(1)` (нет аргумента).

**Ожидаемый результат:**

```text
{'type': 'tool_result', 'tool_use_id': 't1', 'content': '3'}
{'type': 'tool_result', 'tool_use_id': 't2', 'content': "KeyError: 'nope'", 'is_error': True}
{'type': 'tool_result', 'tool_use_id': 't3', 'content': "TypeError: <lambda>() missing 1 required positional argument: 'b'", 'is_error': True}
```

<details><summary>Подсказка</summary>

Возьмите `handlers[b.name]` и вызовите с `**b.input` внутри `try`; ловите `Exception`.

</details>

<details><summary>Решение</summary>

```python
def tool_result_blocks(content, handlers):
    out = []
    for b in content:
        if b.type != "tool_use":
            continue
        try:
            out.append({"type": "tool_result", "tool_use_id": b.id, "content": str(handlers[b.name](**b.input))})
        except Exception as e:
            out.append({"type": "tool_result", "tool_use_id": b.id, "content": f"{type(e).__name__}: {e}", "is_error": True})
    return out
```

(Обработчики: `{"add": lambda a, b: a + b}`; вывод — как выше.)

</details>

## 2. 🟢 Корректна ли история?

**Задача:** функция `history_ok(messages)` возвращает `False`, если после хода ассистента с блоками `tool_use` нет сообщения пользователя, которое **начинается** с блоков `tool_result` ровно для этих id. Проверьте: верные результаты; текст перед результатом; результат, затем текст; сообщения пользователя нет совсем.

**Ожидаемый результат:** `True False True False`.

<details><summary>Подсказка</summary>

Соберите ведущие блоки `tool_result` следующего сообщения пользователя и сравните множество их `tool_use_id` с id вызовов.

</details>

<details><summary>Решение</summary>

```python
def history_ok(messages):
    for i, m in enumerate(messages):
        if m["role"] != "assistant" or not isinstance(m["content"], list):
            continue
        ids = {b["id"] for b in m["content"] if b.get("type") == "tool_use"}
        if not ids:
            continue
        nxt = messages[i + 1] if i + 1 < len(messages) else None
        if not nxt or nxt["role"] != "user" or not isinstance(nxt["content"], list):
            return False
        head = []
        for b in nxt["content"]:
            if b.get("type") != "tool_result":
                break
            head.append(b["tool_use_id"])
        if set(head) != ids:
            return False
    return True
```

Это повторяет документированное правило ([подвохи 1 и 2](expert.ru.md#1-текст-перед-результатами)).

</details>

## 3. 🟡 Накладные расходы инструментов

**Задача:** функция `overhead(model, tc_type)` возвращает документированные токены системного промпта (Opus 5.5 286, Opus 5 286/406, Sonnet 5 354/474, Haiku 5.5 286/406; первое число для `auto`/`none`, второе для `any`/`tool`) и бросает `ValueError` для принудительного вызова на моделях без поддержки. Выведите `overhead("claude-opus-5", "any")`, `overhead("claude-sonnet-5", "auto")`, `overhead("claude-haiku-5-5", "tool")` и ошибку для `claude-opus-5-5` с `any`.

**Ожидаемый результат:**

```text
406 354 406
claude-opus-5-5: forced tool use is not supported
```

<details><summary>Подсказка</summary>

Словарь пар `(auto, forced)`; `None` означает «не поддерживается».

</details>

<details><summary>Решение</summary>

```python
OVERHEAD = {"claude-opus-5-5": (286, None), "claude-opus-5": (286, 406),
            "claude-sonnet-5": (354, 474), "claude-haiku-5-5": (286, 406)}

def overhead(model, tc_type):
    auto, forced = OVERHEAD[model]
    if tc_type in ("auto", "none"):
        return auto
    if forced is None:
        raise ValueError(f"{model}: forced tool use is not supported")
    return forced
```

См. [стоимость в токенах](advanced.ru.md#стоимость-в-токенах).

</details>

## 4. 🟡 Ограниченный цикл

**Задача:** функция `run_loop(call_model, handlers, question, max_steps=5)` реализует агентный цикл: возвращает текст итогового ответа и бросает `RuntimeError("no final answer after N steps")`, если модель продолжает просить инструменты. Используйте `tool_result_blocks` из упражнения 1. Проверьте на сценарной модели (`tool_use` для `add(2, 3)`, затем текст «2 + 3 = 5») и на модели, которая всегда вызывает инструмент, с `max_steps=3`.

**Ожидаемый результат:**

```text
2 + 3 = 5
no final answer after 3 steps
```

<details><summary>Подсказка</summary>

На каждом шаге: вызовите модель, верните текст, если `stop_reason != "tool_use"`, иначе добавьте ход ассистента и ход пользователя с результатами.

</details>

<details><summary>Решение</summary>

```python
def run_loop(call_model, handlers, question, max_steps=5):
    messages = [{"role": "user", "content": question}]
    for _ in range(max_steps):
        r = call_model(messages)
        if r.stop_reason != "tool_use":
            return "".join(b.text for b in r.content if b.type == "text")
        messages.append({"role": "assistant", "content": r.content})
        messages.append({"role": "user", "content": tool_result_blocks(r.content, handlers)})
    raise RuntimeError(f"no final answer after {max_steps} steps")
```

</details>

## 5. 🔴 Запрос к моку

**Задача:** напишите цикл на SDK (как в [базовой теории](basics.ru.md#вызов-и-результат)) для вопроса, на который модель вызывает два инструмента сразу, и выведите значения `tool_use_id` последнего сообщения пользователя. Затем намеренно сломайте: поставьте текстовый блок перед результатом и поймайте `anthropic.BadRequestError`.

**Ожидаемый результат (mock-сервер):**

```text
3 ['toolu_01', 'toolu_02']
BadRequestError 400
```

<details><summary>Подсказка</summary>

В последнем запросе три сообщения: вопрос, ход ассистента, одно сообщение пользователя с двумя результатами.

</details>

<details><summary>Решение</summary>

Цикл — это `ask` из базовой теории; mock-сервер — тот, что описан там. Неверный запрос:

```python
try:
    client.messages.create(model="claude-opus-5-5", max_tokens=100, tools=tools, messages=[
        {"role": "user", "content": "Weather?"},
        {"role": "assistant", "content": [{"type": "tool_use", "id": "toolu_01", "name": "get_weather", "input": {"location": "Paris"}}]},
        {"role": "user", "content": [{"type": "text", "text": "here"},
                                     {"type": "tool_result", "tool_use_id": "toolu_01", "content": "15"}]}])
except anthropic.BadRequestError as e:
    print(type(e).__name__, e.status_code)
```

400 выдаёт мок, реализующий документированное правило; реальный сервис отвечает собственным сообщением.

</details>
