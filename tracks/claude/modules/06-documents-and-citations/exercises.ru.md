---
title: "Упражнения: документы и цитаты"
lang: ru
track: claude
kind: module
doc: exercises
slug: 06-documents-and-citations
part: 1
tags: [claude, pdf, vision, citations, files-api]
status: draft
updated: 2026-11-24
---

# Упражнения: документы и цитаты

🌐 **Русский** · [English](exercises.en.md)

> Решайте сами; подсказки и решения — под спойлерами. Решения — локальные функции Python и запросы к mock-серверу (см. «Как проверялся код» в [базовой теории](basics.ru.md)); реальные ответы моделей и точность цитат не проверялись.

## 1. 🟢 Сборка содержимого сообщения

**Задача:** функция `build_content(question, pdf_bytes=None, images=())` возвращает список `content`: сначала блок `document` с PDF (base64), затем для каждого изображения подпись `Image N:` и блок `image`, в конце — текст вопроса. Для PDF и двух изображений выведите типы блоков.

**Ожидаемый результат:** `['document', 'text', 'image', 'text', 'image', 'text']`.

<details><summary>Подсказка</summary>

Документы и изображения — до текста вопроса; `images` — список пар `(media_type, bytes)`.

</details>

<details><summary>Решение</summary>

```python
import base64

def build_content(question, pdf_bytes=None, images=()):
    blocks = []
    if pdf_bytes is not None:
        blocks.append({"type": "document", "source": {"type": "base64", "media_type": "application/pdf",
                       "data": base64.standard_b64encode(pdf_bytes).decode()}})
    for i, (mt, data) in enumerate(images, 1):
        blocks += [{"type": "text", "text": f"Image {i}:"},
                   {"type": "image", "source": {"type": "base64", "media_type": mt,
                    "data": base64.standard_b64encode(data).decode()}}]
    blocks.append({"type": "text", "text": question})
    return blocks
```

</details>

## 2. 🟢 Base64 без переводов строк

**Задача:** проверьте, какая функция base64 даёт строку без переводов строк: `standard_b64encode` или `encodebytes`. Используйте `b"x" * 100`.

**Ожидаемый результат:** `False` для `standard_b64encode`, `True` для `encodebytes`.

<details><summary>Подсказка</summary>

Ищите `"\n"` в декодированной строке.

</details>

<details><summary>Решение</summary>

```python
import base64
print("\n" in base64.standard_b64encode(b"x" * 100).decode())
print("\n" in base64.encodebytes(b"x" * 100).decode())
```

```text
False
True
```

Используйте `standard_b64encode`: `encodebytes` вставляет переводы строк.

</details>

## 3. 🟡 Токены изображения

**Задача:** функция `visual_tokens(w, h)` считает токены изображения патчами 28×28. Найдите значения для 1000×1000 и 1568×1568.

**Ожидаемый результат:** `1296 3136`.

<details><summary>Подсказка</summary>

`⌈w/28⌉ × ⌈h/28⌉`, используйте `math.ceil`.

</details>

<details><summary>Решение</summary>

```python
import math
def visual_tokens(w, h): return math.ceil(w / 28) * math.ceil(h / 28)
print(visual_tokens(1000, 1000), visual_tokens(1568, 1568))
```

```text
1296 3136
```

См. [стоимость в токенах](advanced.ru.md#стоимость-в-токенах).

</details>

## 4. 🟡 Проверка цитат

**Задача:** функция `verified(message, doc_text)` возвращает список `cited_text`, которые **не** совпадают с `doc_text[start_char_index:end_char_index]`. Запустите её на mock-ответе с настоящим текстом документа и с другим.

**Ожидаемый результат:** `[]` для настоящего текста, `['The grass is green. ', 'The sky is blue.']` для другого.

<details><summary>Подсказка</summary>

Обойдите блоки `message.content`; цитаты читайте как `getattr(b, "citations", None) or []`.

</details>

<details><summary>Решение</summary>

```python
def verified(message, doc_text):
    bad = []
    for b in message.content:
        for ci in (getattr(b, "citations", None) or []):
            if doc_text[ci.start_char_index:ci.end_char_index] != ci.cited_text:
                bad.append(ci.cited_text)
    return bad
```

```text
[]
['The grass is green. ', 'The sky is blue.']
```

Вывод — на mock-ответе. Это проверка из [продвинутой теории](advanced.ru.md#как-использовать-цитаты-в-приложении).

</details>

## 5. 🔴 Ловим несовместимый запрос

**Задача:** функция `has_conflict(body)` возвращает `True`, если в теле запроса включены цитаты хотя бы на одном блоке `document` **и** есть `output_config.format`. Проверьте на корректном теле и на нём же с добавленным форматом.

**Ожидаемый результат:** `False True`.

<details><summary>Подсказка</summary>

Просмотрите `messages[*].content` в поисках блоков `document` с `citations.enabled`; формат — в `body["output_config"]["format"]`.

</details>

<details><summary>Решение</summary>

```python
def has_conflict(body):
    cited = any(
        isinstance(b, dict) and b.get("type") == "document" and (b.get("citations") or {}).get("enabled")
        for m in body.get("messages", []) if isinstance(m.get("content"), list)
        for b in m["content"]
    )
    return cited and "format" in body.get("output_config", {})
```

```text
False True
```

При конфликте API вернёт 400: разделите на два запроса ([несовместимости](advanced.ru.md#несовместимости)).

</details>
