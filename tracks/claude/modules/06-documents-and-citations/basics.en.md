---
title: "Working with documents and citations: basics"
lang: en
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

# Working with documents and citations: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [02 · Messages API](../02-messages-api/overview.en.md) · **Time:** ~1.5 h

## How the code was checked

There was no API key during preparation, so the code was run against a **local mock server** (as in [module 02](../02-messages-api/basics.en.md)): it accepts requests from the `anthropic` 2.13.0 SDK, saves the body and returns predefined replies (including ones with citations) and a file object for an upload. What was checked are the request shapes and the parsing of replies; **the replies of real models, the actual processing of PDFs and images and the accuracy of citations were not checked**. The limits and rules come from the documentation pages "PDF support", "Vision", "Citations" and "Files API"; part of the first three pages was read in an abridged summary (marked below).

## Why it matters

Real data rarely arrives as clean text: contracts, reports, screenshots, invoices. The `document` and `image` blocks let you pass them to the model directly, and citations let you get an answer that says **where** each fact comes from.

## The document block and sources

A PDF is passed in a `document` block. It has three kinds of source (`source`):

| Source | Form |
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

In the experiment the request body had the blocks `['document', 'text']` with a `base64` source and the `media_type` `application/pdf`; the base64 string has **no** line breaks (as it should be: `standard_b64encode` adds none). For a URL and a `file_id` the sources in the request were `{'type': 'url', 'url': '…'}` and `{'type': 'file', 'file_id': 'file_011mock'}`.

**Rules:**

- **the document goes before the text** of the question (the documentation's recommendation);
- the PDF must be an ordinary one: no password or encryption;
- platforms differ: on Amazon Bedrock and Google Cloud (Google managed) only base64 is available.

## PDF limits

| Limit | Value |
|---|---|
| Request size | 128 MB (Claude API) |
| Pages per request | 600; 100 for requests with a context window under 1M tokens |
| Format | a standard PDF without a password |

The limits apply to the **whole** request, including other content. Dense PDFs may exhaust the context window before the page limit. Every page is processed both as text (usually **1500–3000 tokens** depending on density) and as an image (up to 1568 visual tokens); there is no separate charge for a PDF. Plain text documents (`text/plain`, for example `.txt`, `.csv`, `.md`) can also be passed in a `document` block; the formats `.xlsx` and `.docx` are not supported in `document`.

## Images

The `image` block accepts `image/jpeg`, `image/png`, `image/gif`, `image/webp` (an animated GIF — only the first frame) and the same three kinds of source.

```python
content = [
    {"type": "text", "text": "Image 1:"},
    {"type": "image", "source": {"type": "base64", "media_type": "image/png", "data": png}},
    {"type": "text", "text": "Image 2:"},
    {"type": "image", "source": {"type": "url", "url": "https://example.com/b.jpg"}},
    {"type": "text", "text": "How are they different?"},
]
```

In the experiment the request went out with the blocks `['text', 'image', 'text', 'image', 'text']`.

- Label several images with text (`Image 1:`) so you can refer to them later.
- It is better when images come **before** the text of the question.
- Limits: up to 10 MB per image (5 MB on Bedrock), up to 100 images per request for models with a 200K context and up to 600 for the others; a maximum of 8000×8000 pixels. When there are more than 20 images in a request, stricter size limits apply.

> ⚠️ Verify: the resolution tiers (a long side of 1568 or 2576 pixels) and the resizing rules come from an abridged summary of the "Vision" page; check it before use.

## The Files API

If one file is used many times, upload it and refer to it by `file_id`: there is no need to resend base64 in every request.

```python
f = client.files.upload(file=pathlib.Path("report.pdf"))
print(f.id, f.filename, f.mime_type, f.downloadable, f.expires_at)
```

```text
file_011mock report.pdf application/pdf False None
```

In the experiment the SDK sent a multipart request `POST /v1/files` with a file name, and the mock returned a file object (the values are the mock's reply). The response fields: `id`, `type`, `filename`, `mime_type`, `size_bytes`, `created_at`, `downloadable`, `expires_at`. For uploaded files `downloadable` is `false`; no lifetime is set by default (`expires_at` is `null`). Uploading, listing, metadata and deleting are free; a file's content in a message is billed as input tokens. Details are in the [expert theory](expert.en.md#the-files-api).

## Citations

To get references to the source, enable citations on the **document**: `"citations": {"enabled": true}`. The optional `title` and `context` stand next to it (`context` is passed to the model but is not cited):

```python
doc = {"type": "document",
       "source": {"type": "text", "media_type": "text/plain", "data": "The grass is green. The sky is blue."},
       "title": "My Document", "citations": {"enabled": True}}
r = client.messages.create(model="claude-opus-5-5", max_tokens=1024,
    messages=[{"role": "user", "content": [doc, {"type": "text", "text": "What color is the grass and sky?"}]}])
```

In the experiment the request body had `"citations": {"enabled": true}` on the document. The reply is a **list of `text` blocks**: ordinary parts and parts with a `citations` array. Let us join the text with marks:

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

(The reply was returned by the mock to check the parsing.) A citation holds `cited_text` (a verbatim fragment), `document_index` (the document's number, from zero, across all documents of the request), `document_title` and a location: for text `start_char_index` and `end_char_index` (from zero, the end exclusive).

> Citations must be enabled on **all** documents of a request or on none; text can be cited but images cannot (a scan without a text layer cannot be cited).

## Beginner mistakes

- ❌ The question before the document → ✅ the document or image, then the text.
- ❌ Base64 on every turn of a long conversation → ✅ `file_id`.
- ❌ A base64 string with line breaks → ✅ `base64.standard_b64encode`.
- ❌ Reading only `content[0].text` in a reply with citations → ✅ iterate the blocks.
- ❌ Enabling citations on only some documents → ✅ on all or none.
- ❌ Expecting citations together with `output_config.format` → ✅ incompatible (400).
- ❌ Excel or Word in `document` → ✅ convert to text or PDF.

## Related materials

- Next: [advanced theory](advanced.en.md) — kinds of documents and citations, cost, caching, streaming
- Earlier: [05 · Prompt caching](../05-prompt-caching/basics.en.md), [02 · Messages API](../02-messages-api/basics.en.md)
- Parallels: [LLM](../../../llm/overview.en.md)

## Sources

- [PDF support](https://platform.claude.com/docs/en/build-with-claude/pdf-support)
- [Vision](https://platform.claude.com/docs/en/build-with-claude/vision)
- [Citations](https://platform.claude.com/docs/en/build-with-claude/citations)
- [Files API](https://platform.claude.com/docs/en/build-with-claude/files)
