---
title: "Working with documents and citations: advanced"
lang: en
track: claude
kind: module
doc: advanced
slug: 06-documents-and-citations
part: 1
tags: [claude, citations, pdf, vision, files-api, prompt-caching, token-cost, rag]
versions: "Claude 5.5 family; Messages API, anthropic-version 2026-10-21; Python SDK anthropic 2.13.0"
status: draft
updated: 2026-11-24
---

# Working with documents and citations: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on the [basic theory](basics.en.md). The code was run against a local mock server and local functions; processing by real models and the accuracy of citations were not checked. Part of the information about citations and images comes from an abridged summary of the documentation pages (marked).

## Kinds of documents and citations

The type of the document determines how it is split into citable units and which location is returned:

| Document | Source | Splitting | Citation type | Location fields |
|---|---|---|---|---|
| Plain text | `{"type": "text", "media_type": "text/plain", "data": …}` | by sentences | `char_location` | `start_char_index`, `end_char_index` (from zero, the end exclusive) |
| PDF | base64, URL or `file_id` | by sentences, by pages | `page_location` | `start_page_number`, `end_page_number` (pages from one, the end exclusive) |
| Custom blocks | a source of type `content` | every text block is one unit | `content_block_location` | `start_block_index`, `end_block_index` (from zero, the end exclusive) |

In the experiment the mock returned for plain text `char_location` citations with `cited_text`, `document_index`, `document_title`, `start_char_index`, `end_char_index`. For PDFs and custom blocks the location fields come from the documentation.

> ⚠️ Verify: the types `page_location` and `content_block_location`, their fields and numbering come from an abridged summary of the "Citations" page and were not reproduced in the experiment (the mock returned only `char_location`).

**A RAG tip.** Fragments found by search can be passed as documents: as plain text — for sentence-level citation, as custom blocks — for finer control (each block is cited whole). The model itself chooses which fragments to refer to.

## Using citations in an application

It is handy to build a list of sources without repeats from a reply:

```python
def sources(message):
    seen, out = set(), []
    for b in message.content:
        if b.type != "text": continue
        for c in (b.citations or []):
            key = (c.document_index, c.start_char_index, c.end_char_index)
            if key not in seen:
                seen.add(key); out.append((c.document_title, c.cited_text))
    return out
```

```text
[('My Document', 'The grass is green. '), ('My Document', 'The sky is blue.')]
```

(The output is on a mock reply.) For a PDF the key is built from `start_page_number`/`end_page_number`. A check on the application side: make sure that `cited_text` really is in the source document before showing the user a reference.

Properties of citations:

- `cited_text` is **not counted** in output tokens and is not counted in input tokens when the reply is passed back on later turns; but enabling citations itself adds input tokens (system hints and document splitting).
- `document_index` is the document's number **across the whole request** (all messages), from zero.
- Text can be cited but images cannot.
- A request "return only JSON" may yield few citations or none.

## Incompatibilities

| Combination | Result |
|---|---|
| Citations (`citations.enabled`) + `output_config.format` (structured output) | 400 ([module 04](../04-structured-output/advanced.en.md)) |
| Documents with and without citations in one request | not allowed |
| Citations and `cache_control` on the citation blocks themselves | only the top-level document is cached |

To get structured data together with sources, do two steps: first a reply with citations, then extract the structure from that reply with a second request.

## Cost in tokens

- **PDF:** a page is usually 1500–3000 text tokens plus no more than 1568 visual tokens of the image; there is no separate charge for a PDF.
- **Images:** counted in 28×28 pixel patches, tokens = `⌈width/28⌉ × ⌈height/28⌉`:

```python
import math
def visual_tokens(w, h): return math.ceil(w / 28) * math.ceil(h / 28)
print(visual_tokens(1000, 1000), visual_tokens(28, 28), visual_tokens(29, 28))
```

```text
1296 1 2
```

Images that are too large are scaled down to the model's limit (for the "standard" tier — about 1568 pixels on the long side, 1568 visual tokens; for new models — higher), so resize in advance yourself: it is cheaper and keeps control over coordinates.

> ⚠️ Verify: the exact resolution tiers, the token counts and the downscaling rules come from an abridged summary of the "Vision" page.

## Caching documents

A large document that many questions are asked about is worth caching: `cache_control` on the `document` block.

```python
doc = {"type": "document", "source": {"type": "base64", "media_type": "application/pdf", "data": pdf},
       "cache_control": {"type": "ephemeral"}}
```

In the experiment the request contained `{"type": "ephemeral"}` in the document block. For citations cache the **top-level document**, not the citation blocks; empty text blocks cannot be cached. The general rules, TTL and the order of points are in [module 05](../05-prompt-caching/advanced.en.md).

## The Files API in conversations

Every turn of a conversation resends the whole history, so a base64 image or PDF bloats the request on every turn. Upload the file once and refer to it by `file_id` (the `document` and `image` blocks accept `{"type": "file", "file_id": …}`). What to remember:

- files are tied to the **workspace**; never accept `file_id` values from end users;
- a file cannot be modified or renamed: upload a new one and delete the old; deletion is irreversible;
- the lifetime is set on upload (`expires_in_seconds`, from 1 hour to 90 days); after expiry a request that refers to the file fails;
- the Files API is not suitable for ZDR and HIPAA.

## Many images

Every `image` block counts, including images from past turns and inside tool results. When there are more than 20 in one request, stricter size limits apply; on a violation the API returns an `invalid_request_error` mentioning "many-image requests". The ways out: reduce the sides (no more than about 3000 pixels), cut the number of images and documents to 20 or fewer.

## Best practices

- Documents and images go before the question; several — with labels.
- One file in several requests — `file_id` and `cache_control`.
- Citations on all documents of a request; check `cited_text` in the application.
- A complex parse + sources — two requests, not `output_config.format` with citations.
- Scans without text — recognize the text first.
- Resize images in advance.

## Common mistakes

- ❌ Citations on some documents → ✅ on all.
- ❌ Citations and structured outputs in one request → ✅ two steps.
- ❌ Resending large base64 attachments on every turn → ✅ `file_id`.
- ❌ Trusting a reference without checking → ✅ compare `cited_text` with the document.
- ❌ An unsupported format (docx, xlsx) → ✅ convert.
- ❌ Accepting a `file_id` from a user → ✅ keep identifiers on the server.

## Related materials

- Next: [expert theory](expert.en.md)
- Caching: [05 · Prompt caching](../05-prompt-caching/advanced.en.md)
- Structured output: [04 · Structured outputs](../04-structured-output/advanced.en.md)

## Sources

- [Citations](https://platform.claude.com/docs/en/build-with-claude/citations)
- [PDF support](https://platform.claude.com/docs/en/build-with-claude/pdf-support)
- [Vision](https://platform.claude.com/docs/en/build-with-claude/vision)
- [Files API](https://platform.claude.com/docs/en/build-with-claude/files)
