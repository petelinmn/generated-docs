---
title: "Работа с документами и цитаты: основы"
lang: ru
track: claude
kind: module
doc: basics
slug: 06-documents-and-citations
part: 1
tags: [claude, pdf, vision, images, citations, files-api, document-block, base64]
versions: "Claude 5.5 family; Messages API, anthropic-version 2026-10-21; Python SDK anthropic 2.13.0"
status: draft
updated: 2026-11-24
---

# Работа с документами и цитаты: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [02 · Messages API](../02-messages-api/overview.ru.md) · **Время:** ~1.5 ч

## Как проверялся код

Ключа API при подготовке не было, поэтому код запускался против **локального мок-сервера** (как в [модуле 02](../02-messages-api/basics.ru.md)): он принимает запросы SDK `anthropic` 2.13.0, сохраняет тело и возвращает заранее заданные ответы (в том числе с цитатами) и объект файла для загрузки. Проверены форма запросов и разбор ответов; **ответы реальных моделей, фактическая обработка PDF и изображений, точность цитат не проверялись**. Лимиты и правила взяты со страниц документации «PDF support», «Vision», «Citations» и «Files API»; часть первых трёх страниц прочитана в сокращённом пересказе (помечено ниже).

## Зачем это нужно

Реальные данные редко приходят чистым текстом: договоры, отчёты, скриншоты, счета. Блоки `document` и `image` позволяют передать их модели напрямую, а цитаты — получить ответ с указанием, **откуда** взят каждый факт.

## Блок document и источники

PDF передают блоком `document`. У него есть три вида источника (`source`):

| Источник | Форма |
|---|---|
| base64 | `{"type": "base64", "media_type": "application/pdf", "data": "<base64>"}` |
| URL | `{"type": "url", "url": "https://…"}` |
| Files API | `{"type": "file", "file_id": "…"}` |

```python
import base64
pdf = base64.standard_b64encode(open("report.pdf", "rb").read()).decode()
content = [
    {"type": "document", "source": {"type": "base64", "media_type": "application/pdf", "data": pdf}},
    {"type": "text", "text": "Summarize this document."},
]
client.messages.create(model="claude-opus-5-5", max_tokens=1024, messages=[{"role": "user", "content": content}])
```

В опыте тело запроса содержало блоки `['document', 'text']` с источником `base64` и `media_type` `application/pdf`; в строке base64 **нет** переводов строки (так должно быть: `standard_b64encode` их не добавляет). Для URL и `file_id` источники в запросе были `{'type': 'url', 'url': '…'}` и `{'type': 'file', 'file_id': 'file_011mock'}`.

**Правила:**

- **документ — перед текстом** вопроса (рекомендация документации);
- PDF должен быть обычным: без пароля и шифрования;
- платформы различаются: на Amazon Bedrock и Google Cloud (Google managed) доступен только base64.

## Лимиты PDF

| Лимит | Значение |
|---|---|
| Размер запроса | 128 МБ (Claude API) |
| Страниц в запросе | 600; 100 для запросов с окном контекста меньше 1M токенов |
| Формат | стандартный PDF без пароля |

Лимиты относятся ко **всему** запросу, включая другое содержимое. Плотные PDF могут исчерпать окно контекста раньше лимита страниц. Каждая страница обрабатывается и как текст (обычно **1500–3000 токенов** в зависимости от плотности), и как изображение (до 1568 визуальных токенов); отдельной платы за PDF нет. Обычные текстовые документы (`text/plain`, например `.txt`, `.csv`, `.md`) тоже можно передавать блоком `document`; форматы `.xlsx` и `.docx` в `document` не поддерживаются.

## Изображения

Блок `image` принимает `image/jpeg`, `image/png`, `image/gif`, `image/webp` (анимированный GIF — только первый кадр) и те же три вида источника.

```python
content = [
    {"type": "text", "text": "Image 1:"},
    {"type": "image", "source": {"type": "base64", "media_type": "image/png", "data": png}},
    {"type": "text", "text": "Image 2:"},
    {"type": "image", "source": {"type": "url", "url": "https://example.com/b.jpg"}},
    {"type": "text", "text": "How are they different?"},
]
```

В опыте запрос ушёл с блоками `['text', 'image', 'text', 'image', 'text']`.

- Несколько изображений подписывайте текстом (`Image 1:`), чтобы потом на них ссылаться.
- Лучше, когда изображения идут **до** текста вопроса.
- Лимиты: до 10 МБ на изображение (5 МБ на Bedrock), до 100 изображений в запросе для моделей с контекстом 200K и до 600 для остальных; максимум 8000×8000 пикселей. Когда в запросе больше 20 изображений, действуют более строгие ограничения на размер.

> ⚠️ Проверить: уровни разрешения (длинная сторона 1568 или 2576 пикселей) и правила изменения размера взяты из сокращённого пересказа страницы «Vision»; перед использованием сверьтесь с ней.

## Files API

Если один файл используется многократно, загрузите его и ссылайтесь по `file_id`: не нужно пересылать base64 в каждом запросе.

```python
f = client.files.upload(file=pathlib.Path("report.pdf"))
print(f.id, f.filename, f.mime_type, f.downloadable, f.expires_at)
```

```text
file_011mock report.pdf application/pdf False None
```

В опыте SDK отправил multipart-запрос `POST /v1/files` с именем файла, а мок вернул объект файла (значения — ответ мока). Поля ответа: `id`, `type`, `filename`, `mime_type`, `size_bytes`, `created_at`, `downloadable`, `expires_at`. Для загруженных файлов `downloadable` равно `false`; срок жизни по умолчанию не задан (`expires_at` — `null`). Загрузка, список, метаданные и удаление бесплатны; содержимое файла в сообщении оплачивается как входные токены. Подробности — в [экспертной теории](expert.ru.md#files-api).

## Цитаты

Чтобы получить ссылки на источник, включите цитаты на **документе**: `"citations": {"enabled": true}`. Необязательные `title` и `context` стоят рядом (`context` передаётся модели, но не цитируется):

```python
doc = {"type": "document",
       "source": {"type": "text", "media_type": "text/plain", "data": "The grass is green. The sky is blue."},
       "title": "My Document", "citations": {"enabled": True}}
r = client.messages.create(model="claude-opus-5-5", max_tokens=1024,
    messages=[{"role": "user", "content": [doc, {"type": "text", "text": "What color is the grass and sky?"}]}])
```

В опыте тело запроса содержало `"citations": {"enabled": true}` у документа. Ответ — **список блоков `text`**: обычные части и части с массивом `citations`. Склеим текст с пометками:

```python
out = []
for b in r.content:
    if b.type == "text":
        out.append(b.text + "".join(f" [{c.document_title}: {c.start_char_index}-{c.end_char_index}]" for c in (b.citations or [])))
print("".join(out))
print([(b.citations[0].type, b.citations[0].cited_text) for b in r.content if b.citations])
```

```text
According to the document, the grass is green [My Document: 0-20] and the sky is blue [My Document: 20-36].
[('char_location', 'The grass is green. '), ('char_location', 'The sky is blue.')]
```

(Ответ вернул мок для проверки разбора.) Цитата содержит `cited_text` (дословный фрагмент), `document_index` (номер документа, с нуля, по всем документам запроса), `document_title` и расположение: у текста `start_char_index` и `end_char_index` (с нуля, конец не включается).

> Включать цитаты нужно **на всех** документах запроса или ни на одном; цитировать можно текст, но не изображения (скан без текстового слоя цитировать нельзя).

## Ошибки новичков

- ❌ Вопрос перед документом → ✅ документ или изображение, затем текст.
- ❌ Base64 в каждом ходе длинного диалога → ✅ `file_id`.
- ❌ Строка base64 с переводами строк → ✅ `base64.standard_b64encode`.
- ❌ Читать только `content[0].text` в ответе с цитатами → ✅ перебирать блоки.
- ❌ Включить цитаты только на части документов → ✅ на всех или ни на одном.
- ❌ Ждать цитаты вместе с `output_config.format` → ✅ несовместимо (400).
- ❌ Excel или Word в `document` → ✅ конвертировать в текст или PDF.

## Связанные материалы

- Дальше: [продвинутая теория](advanced.ru.md) — виды документов и цитат, стоимость, кэширование, стриминг
- Ранее: [05 · Prompt caching](../05-prompt-caching/basics.ru.md), [02 · Messages API](../02-messages-api/basics.ru.md)
- Параллели: [LLM](../../../llm/overview.ru.md)

## Источники

- [PDF support](https://platform.claude.com/docs/en/build-with-claude/pdf-support)
- [Vision](https://platform.claude.com/docs/en/build-with-claude/vision)
- [Citations](https://platform.claude.com/docs/en/build-with-claude/citations)
- [Files API](https://platform.claude.com/docs/en/build-with-claude/files)
