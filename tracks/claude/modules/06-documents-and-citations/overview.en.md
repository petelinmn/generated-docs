---
title: "Working with documents and citations"
lang: en
track: claude
kind: module
doc: overview
slug: 06-documents-and-citations
part: 1
levels: [1, 2, 3, 4]
tags: [claude, pdf, vision, images, citations, files-api, document-block]
versions: "Claude 5.5 family; Messages API, anthropic-version 2026-10-21; Python SDK anthropic 2.13.0"
status: draft
updated: 2026-11-24
---

# Working with documents and citations

🌐 [Русский](overview.ru.md) · **English**

## In short

Claude accepts more than text: a user message can hold a PDF and plain-text documents (a `document` block) and images (an `image` block). The source is given in three ways: base64, a URL or a `file_id` from the Files API. If you enable citations (`citations: {enabled: true}`), the reply is split into text blocks with references attached to the exact places in the document: characters, pages or blocks. This is the basis of answers with verifiable sources.

## Key ideas

- **A document or an image is a block in the `content`** of a user message; it is better to put documents and images **before** the text of the question.
- **Three sources:** `base64`, `url`, `file` (`file_id`). On some platforms only base64 is available.
- **Request limits:** a PDF — up to 600 pages (100 for models with a context under 1M); a request — up to 128 MB; images — up to 100 or 600 per request.
- **The price in tokens:** a PDF page is usually 1500–3000 text tokens plus the image tokens; an image is counted in 28×28 patches.
- **The Files API** uploads a file once so you can refer to it by `file_id` and not resend base64 on every turn.
- **Citations:** `citations: {enabled: true}` on a document; the reply is a list of `text` blocks that have a `citations` array with `cited_text` and a location.
- **Three kinds of location:** `char_location` (text), `page_location` (PDF), `content_block_location` (your own blocks).
- **`cited_text` is not counted in output tokens.**
- **Citations are incompatible with `output_config.format`** (structured output): the request returns a 400.
- **Cache the document** whole: `cache_control` on the top-level block.

## Minimal example

```python
import base64
pdf = base64.standard_b64encode(open("report.pdf", "rb").read()).decode()
message = client.messages.create(
    model="claude-opus-5-5", max_tokens=1024,
    messages=[{"role": "user", "content": [
        {"type": "document", "source": {"type": "base64", "media_type": "application/pdf", "data": pdf}},
        {"type": "text", "text": "Summarize this document."},
    ]}],
)
```

The document is the first block, the question the second. The code was checked against a local mock server (see "How the code was checked" in the [basic theory](basics.en.md)).

## When to use and when not

| Task | Choice |
|---|---|
| Questions about a PDF, a report, a contract | a `document` block |
| Analyzing a screenshot, a chart, a photo | an `image` block |
| The same file in several requests | the Files API + `file_id` |
| An answer with references to the source | `citations: {enabled: true}` |
| An answer strictly in JSON and citations | not at once: pick one |
| Scans without a text layer | cannot be cited (text only) |
| Excel tables, Word documents | not supported in `document`: convert to text or PDF |

- ❌ Do not resend a big base64 on every turn of a conversation: use `file_id`.
- ❌ Do not accept `file_id` values from end users: files are tied to the workspace.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | `document` and `image`, three sources, limits, the Files API, citations, parsing the reply |
| [2. Advanced theory](advanced.en.md) | kinds of documents and citations, several images, cost, caching, streaming citations, incompatibilities |
| [3. Expert theory](expert.en.md) | a reference of limits and fields, Files API errors, pitfalls, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn claude 06-documents-and-citations level 5` |
| 6. Tests | not created yet — `/learn claude 06-documents-and-citations level 6` |

## Related materials

- Prerequisites: [02 · Messages API](../02-messages-api/overview.en.md), [05 · Prompt caching](../05-prompt-caching/overview.en.md), [04 · Structured outputs](../04-structured-output/overview.en.md) (incompatibility with citations)
- Next: module 07 · Tool use (in the [roadmap](../../overview.en.md))
- Analogues in other tracks: [LLM](../../../llm/overview.en.md) (RAG), [Python](../../../python/overview.en.md)
