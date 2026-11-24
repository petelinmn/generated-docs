---
title: "Working with documents and citations: expert"
lang: en
track: claude
kind: module
doc: expert
slug: 06-documents-and-citations
part: 1
tags: [claude, citations, pdf, vision, files-api, document-limits, image-limits]
versions: "Claude 5.5 family; Messages API, anthropic-version 2026-10-21; Python SDK anthropic 2.13.0"
status: draft
updated: 2026-11-24
---

# Working with documents and citations: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on the [basic](basics.en.md) and [advanced](advanced.en.md) theory. The reference data comes from the documentation pages at the time of writing; the Files API page was read in full, while the PDF, Vision and Citations pages only partly (the rest in a summary); items that depend on the summary are marked "check".

## Reference

### The document block

| Field | Value |
|---|---|
| `type` | `document` |
| `source` | `base64` (`media_type`, `data`), `url` (`url`), `file` (`file_id`), `text` (`media_type: text/plain`, `data`), `content` (custom blocks) |
| `title`, `context` | optional; `context` is passed to the model but is not cited |
| `citations` | `{"enabled": true}` |
| `cache_control` | a cache breakpoint for the document |

Supported file types in `document` via the Files API: `application/pdf` and `text/plain`; images (`image/jpeg`, `image/png`, `image/gif`, `image/webp`) go in `image` blocks; other types — `container_upload` for code execution.

### Request limits

| Limit | Value | Source |
|---|---|---|
| Request size (Claude API) | 128 MB | documentation |
| PDF pages | 600; 100 with a context window under 1M | documentation |
| Tokens of a PDF page | 1500–3000 text + up to 1568 visual | documentation |
| An image | up to 10 MB (5 MB on Bedrock); 8000×8000 pixels | documentation |
| Images per request | 100 (a 200K context) or 600 | documentation |
| Stricter limits with more than 20 images | reduce sides (about 3000 pixels) or keep ≤ 20 blocks | check |
| Resolution tiers | a long side of 1568 / 2576 pixels | check |

### The Files API

| Operation | Request |
|---|---|
| Upload | `POST /v1/files`, the multipart field `file`, optional `expires_in_seconds` (3600–7,776,000) |
| List | `GET /v1/files` (`limit` default 20, maximum 1000; `page`; `ids[]` up to 100) |
| Metadata | `GET /v1/files/{file_id}` |
| Download | `GET /v1/files/{file_id}/content` — only for files created by skills or code execution (for uploads — 400) |
| Delete | `DELETE /v1/files/{file_id}` |

Limits: 500 MB per file, 1 TB per organization, a name of 1–255 characters without `< > : " | ? * \ /`. No beta header is needed. Errors: 404 (no file), 400 (the type does not fit the block, not downloadable, storage exceeded), 413 (over 500 MB). Upload, download, list, metadata and delete are free; content in messages is billed as input tokens; ZDR and HIPAA are not supported.

### Citations: the structure

| Citation field | Value |
|---|---|
| `type` | `char_location`, `page_location`, `content_block_location` |
| `cited_text` | a verbatim fragment |
| `document_index` | from zero, across all documents of the request |
| `document_title` | the title |
| the location | characters (`start_char_index`, `end_char_index`), pages (`start_page_number`, `end_page_number`, from one), blocks (`start_block_index`, `end_block_index`) |

In streaming citations arrive as `content_block_delta` events with a `citations_delta` delta (one citation per delta) (check).

## Internals

- **A PDF has two representations.** Every page is turned into an image, and text is extracted from it in parallel; the model gets both. So scans without a text layer are read "by eye" (as an image) but **cannot be cited**.
- **Chunking for citations.** Text and PDFs are split into sentences; custom blocks are not split. The unit boundaries are the basis of the accuracy of references.
- **`document_index` is global.** The numbering runs across all `document` blocks of the request, not within one message.
- **Files and the workspace.** A `file_id` is valid inside a workspace; after `expires_at` a download gives a 404 and a message referring to the file fails, although the metadata stays readable for up to 30 days.
- **Citation tokens.** `cited_text` is not part of the output tokens, but enabling citations increases the input.
- **Cache.** Cited blocks are not cached by themselves; the top-level document is cached.

> ⚠️ Verify: the details of streaming citations, `search_result` blocks, restrictions on batch requests and ZDR for PDFs were not checked in this material; part of the image limits comes from an abridged summary.

## Gotchas

### 1. Citations and structured outputs

Together they give a 400.

### 2. Citations on some documents

Either on all documents of the request or on none.

### 3. A scan without text

There is nothing to cite; recognize the text first.

### 4. `document_index` is global

It counts across all documents of the request, not per message.

### 5. Base64 bloats the conversation

Every turn resends the whole history: use `file_id`.

### 6. Files are tied to the workspace

Do not accept `file_id` values from users.

### 7. Files cannot be edited

Upload a new one, delete the old.

### 8. The question before the document

It worsens the result: the document goes before the question.

### 9. Many images

More than 20 — stricter size limits (`many-image requests`).

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| `document` with base64 | simple, no storage | bloats requests |
| The Files API | upload once | lifecycle management, workspace binding |
| A URL source | no client-side upload | the URL must be reachable (not on all platforms) |
| Claude's citations | built-in references with boundaries | incompatible with structured outputs |
| Extracting the text yourself (OCR, a parser) | full control | loss of layout and images |
| RAG with search | scales to large collections | infrastructure ([LLM](../../../llm/overview.en.md)) |

## Interview questions

<details><summary>1. What ways of passing a PDF exist?</summary>

A `document` block with a base64, URL or `file_id` source; plain text — a `text` source.

→ [Basic theory](basics.en.md#the-document-block-and-sources)

</details>

<details><summary>2. What limits does a PDF have?</summary>

Up to 600 pages (100 with a context under 1M) and 128 MB per request; a page is 1500–3000 text tokens plus image tokens.

→ [Basic theory](basics.en.md#pdf-limits)

</details>

<details><summary>3. How do you enable citations and what does the reply look like?</summary>

`"citations": {"enabled": true}` on the document; the reply is a list of `text` blocks, some with a `citations` array with `cited_text` and a location.

→ [Basic theory](basics.en.md#citations)

</details>

<details><summary>4. Trick: why does <code>output_config.format</code> not work together with citations?</summary>

They are incompatible: the request returns a 400; use two steps.

→ [Gotcha 1](#1-citations-and-structured-outputs)

</details>

<details><summary>5. How does <code>char_location</code> differ from <code>page_location</code>?</summary>

The first is for text (character indexes from zero), the second for a PDF (page numbers from one); the ends are exclusive.

→ [Advanced theory](advanced.en.md#kinds-of-documents-and-citations)

</details>

<details><summary>6. What is the Files API for?</summary>

To upload a file once and refer to it by `file_id` without resending base64 on every turn.

→ [Advanced theory](advanced.en.md#the-files-api-in-conversations)

</details>

<details><summary>7. Trick: is <code>cited_text</code> part of the output tokens?</summary>

No, it is not.

→ [Reference](#citations-the-structure)

</details>

<details><summary>8. Can images and scans be cited?</summary>

No: only text is cited.

→ [Gotcha 3](#3-a-scan-without-text)

</details>

<details><summary>9. How do you cache a document?</summary>

`cache_control` on the top-level `document` block.

→ [Advanced theory](advanced.en.md#caching-documents)

</details>

<details><summary>10. Trick: can a user's <code>file_id</code> be passed into a request?</summary>

You must not accept `file_id` values from users: files are tied to the workspace.

→ [Gotcha 6](#6-files-are-tied-to-the-workspace)

</details>

<details><summary>11. How do you count the tokens of an image?</summary>

`⌈width/28⌉ × ⌈height/28⌉`, taking into account the downscaling of images that are too large.

→ [Advanced theory](advanced.en.md#cost-in-tokens)

</details>

<details><summary>12. Where do you put the document relative to the question?</summary>

Before the question: documents and images go before the text.

→ [Gotcha 8](#8-the-question-before-the-document)

</details>

## Cheat sheet

```text
{"type": "document", "source": {"type": "base64"|"url"|"file"|"text"|"content", …}, "title"?, "context"?, "citations": {"enabled": true}?, "cache_control"?}
{"type": "image", "source": {"type": "base64"|"url"|"file", …}}
PDF: ≤600 pages (100 with a context <1M), ≤128 MB; a page ≈1500–3000 tokens + an image
citations: text blocks with citations[]; char_location | page_location | content_block_location; incompatible with output_config.format
Files API: POST/GET/DELETE /v1/files; expires_in_seconds; 500 MB per file
```

- The document goes before the question.
- Citations on all documents or none.
- `file_id` — not from users.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Caching: [05 · Prompt caching](../05-prompt-caching/expert.en.md)
- Structured output: [04 · Structured outputs](../04-structured-output/expert.en.md)

## Sources

- [PDF support](https://platform.claude.com/docs/en/build-with-claude/pdf-support)
- [Vision](https://platform.claude.com/docs/en/build-with-claude/vision)
- [Citations](https://platform.claude.com/docs/en/build-with-claude/citations)
- [Files API](https://platform.claude.com/docs/en/build-with-claude/files)
