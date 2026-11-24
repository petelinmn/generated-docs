---
title: "Упражнения: Messages API"
lang: ru
track: claude
kind: module
doc: exercises
slug: 02-messages-api
part: 1
tags: [claude, messages-api, chat-history, stop-reason, api-errors, streaming]
status: draft
updated: 2026-11-24
---

# Упражнения: Messages API

🌐 **Русский** · [English](exercises.en.md)

> Решайте самостоятельно, подсказки и решения под спойлерами. Решения проверены против локального мок-сервера (см. «Как проверялся код» в [базовой теории](basics.ru.md)); выводы в решениях — ответы мока, поэтому тексты условные. С настоящим ключом используйте `client = anthropic.Anthropic()`.

## 1. 🟢 Функция `ask`

**Задача:** напишите `ask(client, question, system=None, max_tokens=1024)`, которая отправляет вопрос и возвращает склеенный текст из блоков `text`. Параметр `system` добавляйте в запрос только если он задан.

**Ожидаемый результат:** `Paris.`; без `system` в теле запроса нет поля `system`, с `system="Be terse."` оно есть.

<details><summary>Подсказка</summary>

`kwargs = {"system": system} if system else {}` и `"".join(b.text for b in message.content if b.type == "text")`.

</details>

<details><summary>Решение</summary>

```python
def ask(client, question, system=None, max_tokens=1024):
    kwargs = {"system": system} if system else {}
    message = client.messages.create(model="claude-opus-5-5", max_tokens=max_tokens, messages=[{"role": "user", "content": question}], **kwargs)
    return "".join(b.text for b in message.content if b.type == "text")
```

```text
Paris. | False Be terse.
```

(Вторая часть вывода — проверка тела запроса на моке: без `system` поля нет, с `system` — `Be terse.`.)

</details>

## 2. 🟢 Диалог с историей

**Задача:** класс `Chat` хранит список `messages`. Метод `say(text)` добавляет сообщение пользователя, отправляет всю историю, добавляет **весь** `content` ответа как сообщение ассистента и возвращает текст.

**Ожидаемый результат:** после двух вызовов в истории 4 сообщения, а второй запрос содержал роли `user`, `assistant`, `user`.

<details><summary>Подсказка</summary>

`self.messages.append({"role": "assistant", "content": reply.content})` — SDK сериализует блоки сам.

</details>

<details><summary>Решение</summary>

```python
class Chat:
    def __init__(self, client, system=None):
        self.client, self.system, self.messages = client, system, []
    def say(self, text):
        self.messages.append({"role": "user", "content": text})
        kwargs = {"system": self.system} if self.system else {}
        reply = self.client.messages.create(model="claude-opus-5-5", max_tokens=1024, messages=self.messages, **kwargs)
        self.messages.append({"role": "assistant", "content": reply.content})
        return "".join(b.text for b in reply.content if b.type == "text")

chat = Chat(client); chat.say("My name is Alice."); chat.say("What is my name?")
```

```text
4 ['user', 'assistant', 'user'] [{'text': 'Paris.', 'type': 'text'}]
```

Блок ассистента ушёл в запрос как `{'text': 'Paris.', 'type': 'text'}`.

</details>

## 3. 🟡 Контроль `stop_reason`

**Задача:** функция `complete(client, question, max_tokens)` возвращает текст, но бросает `Truncated`, если ответ обрезан по `max_tokens`, и `PermissionError`, если `stop_reason == "refusal"`.

**Ожидаемый результат:** с `max_tokens=1024` — `Paris.`; с `max_tokens=5` — `Truncated: cut after 6 tokens` (число токенов вернул мок).

<details><summary>Подсказка</summary>

Проверьте `m.stop_reason` до чтения `m.content[0].text`.

</details>

<details><summary>Решение</summary>

```python
class Truncated(Exception): pass

def complete(client, question, max_tokens):
    m = client.messages.create(model="claude-opus-5-5", max_tokens=max_tokens, messages=[{"role": "user", "content": question}])
    if m.stop_reason == "max_tokens": raise Truncated(f"cut after {m.usage.output_tokens} tokens")
    if m.stop_reason == "refusal": raise PermissionError("refused")
    return m.content[0].text
```

```text
Paris.
Truncated: cut after 6 tokens
```

</details>

## 4. 🟡 Классификация ошибок

**Задача:** функция `classify(exc)` возвращает строку: для `RateLimitError` — `retry later`, для таймаута — `timeout`, для сетевой ошибки — `network`, для `BadRequestError` — `fix request: <error.type>`, для прочих HTTP-ошибок — `status <код>`. Порядок проверок важен: сначала частные классы.

**Ожидаемый результат:** для ответа 400 — `fix request: invalid_request_error`, для 429 — `retry later`, для недоступного сервера — `network`.

<details><summary>Подсказка</summary>

`APITimeoutError` наследует `APIConnectionError`: проверьте таймаут раньше сети. Для проверки отключите повторы: `max_retries=0`.

</details>

<details><summary>Решение</summary>

```python
import anthropic

def classify(exc):
    if isinstance(exc, anthropic.RateLimitError): return "retry later"
    if isinstance(exc, anthropic.APITimeoutError): return "timeout"
    if isinstance(exc, anthropic.APIConnectionError): return "network"
    if isinstance(exc, anthropic.BadRequestError): return "fix request: " + exc.body["error"]["type"]
    if isinstance(exc, anthropic.APIStatusError): return f"status {exc.status_code}"
    return "unknown"
```

```text
fix request: invalid_request_error
retry later
network
```

</details>

## 5. 🔴 Стриминг с обратным вызовом

**Задача:** функция `stream_text(client, question, on_chunk)` вызывает `on_chunk` для каждого фрагмента текста и возвращает итоговое сообщение.

**Ожидаемый результат:** фрагменты `['Par', 'is.']`, их склейка совпадает с текстом итогового сообщения, `usage.output_tokens` равно `6`.

<details><summary>Подсказка</summary>

`with client.messages.stream(...) as s:`; `for t in s.text_stream`; затем `s.get_final_message()`.

</details>

<details><summary>Решение</summary>

```python
def stream_text(client, question, on_chunk):
    with client.messages.stream(model="claude-opus-5-5", max_tokens=1024, messages=[{"role": "user", "content": question}]) as s:
        for t in s.text_stream:
            on_chunk(t)
        return s.get_final_message()

seen = []; final = stream_text(client, "Capital?", seen.append)
print(seen, "".join(seen) == final.content[0].text, final.usage.output_tokens)
```

```text
['Par', 'is.'] True 6
```

</details>
