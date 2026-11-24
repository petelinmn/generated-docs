---
title: "Quiz: documents and citations"
lang: en
track: claude
kind: module
doc: quiz
slug: 06-documents-and-citations
part: 1
tags: [claude, pdf, vision, citations, files-api]
status: draft
updated: 2026-11-24
---

# Quiz: documents and citations

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. The label shows the question level: 🟢 [basic](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 Which block is used to pass a PDF?

- A) `image`
- B) `document`
- C) `file`
- D) `attachment`

<details><summary>Answer</summary>

**B.** The source is `base64`, `url` or `file` (`file_id`).
→ [Basic theory](basics.en.md#the-document-block-and-sources)

</details>

## 2. 🟢 Where do you put a document relative to the question?

- A) After the question
- B) Before the text of the question
- C) In `system`
- D) It does not matter

<details><summary>Answer</summary>

**B.** The documentation's recommendation: documents and images before the text.
→ [Basic theory](basics.en.md#the-document-block-and-sources)

</details>

## 3. 🟢 How do you enable citations?

- A) `"citations": {"enabled": true}` on the `document` block
- B) `citations=True` at the request level
- C) Add "cite" to the prompt
- D) Turn on a beta header

<details><summary>Answer</summary>

**A.** It must be enabled on all documents of a request or on none.
→ [Basic theory](basics.en.md#citations)

</details>

## 4. 🟢 What is the type of a reply with citations?

- A) A single string
- B) A list of `text` blocks, some with a `citations` array
- C) A dictionary `{answer, sources}`
- D) A `citation` block

<details><summary>Answer</summary>

**B.** Join the blocks, collecting references from `citations`.
→ [Basic theory](basics.en.md#citations)

</details>

## 5. 🟡 What do you choose for a PDF used in twenty requests?

- A) Base64 every time
- B) Upload to the Files API and refer by `file_id`
- C) Put the text in `system`
- D) A URL with no cache

<details><summary>Answer</summary>

**B.** It saves request size; the document can also be cached.
→ [Advanced theory](advanced.en.md#the-files-api-in-conversations)

</details>

## 6. 🟡 How does `page_location` differ from `char_location`?

- A) Not at all
- B) The first is for a PDF (pages from one), the second for text (character indexes from zero)
- C) The first is for images
- D) The second is for a PDF

<details><summary>Answer</summary>

**B.** The ends of the ranges are exclusive.
→ [Advanced theory](advanced.en.md#kinds-of-documents-and-citations)

</details>

## 7. 🟡 What happens with citations and `output_config.format` in one request?

- A) Everything works
- B) The request returns a 400
- C) Citations turn off
- D) The format is ignored

<details><summary>Answer</summary>

**B.** The solution is two steps: a reply with citations, then extracting the structure.
→ [Advanced theory](advanced.en.md#incompatibilities)

</details>

## 8. 🟡 How do you count the tokens of a 1000×1000 image?

- A) 1000
- B) `⌈1000/28⌉ × ⌈1000/28⌉` = 1296
- C) 28
- D) 1000 × 1000

<details><summary>Answer</summary>

**B.** An image is counted in 28×28 pixel patches.
→ [Advanced theory](advanced.en.md#cost-in-tokens)

</details>

## 9. 🟡 Choose all true statements

- [ ] A) `cited_text` is not counted in output tokens
- [ ] B) Images can be cited
- [ ] C) `document_index` counts across all documents of the request
- [ ] D) A file in the Files API can be renamed

<details><summary>Answer</summary>

**A, C.** B is wrong: text is cited; D is wrong: files are immutable.
→ [Expert](expert.en.md#citations-the-structure)

</details>

## 10. 🔴 Find the mistake

> "We take a `file_id` from the user and put it into the request — it is more convenient."

<details><summary>Answer</summary>

Files are tied to the workspace: you must not accept `file_id` values from end users (leakage and substitution). Keep identifiers on the server.
→ [Gotcha 6](expert.en.md#6-files-are-tied-to-the-workspace)

</details>

## 11. 🔴 Why can a scan without a text layer not be cited?

- A) The model does not read scans
- B) Only extracted text is cited; images cannot be cited although the model "sees" the page
- C) The page limit
- D) A beta header is needed

<details><summary>Answer</summary>

**B.** Recognize the text first.
→ [Gotcha 3](expert.en.md#3-a-scan-without-text)

</details>

## 12. 🔴 What must you do before showing a user a reference from a citation?

- A) Nothing
- B) Check that `cited_text` really appears in the source at the given indexes
- C) Translate it into English
- D) Remove `document_title`

<details><summary>Answer</summary>

**B.** In the exercise the check found mismatches when the document text was substituted.
→ [Exercises](exercises.en.md)

</details>

## 13. 🔴 Open question

Design an assistant that answers over a set of PDFs, naming pages, and also returns a structured summary.

<details><summary>Key points of a good answer</summary>

- PDFs uploaded to the Files API (`file_id`), `cache_control` on the documents; documents before the question.
- Citations on all documents; from `page_location` collect page numbers and check `cited_text`.
- The structured summary in a separate second request (citations are incompatible with `output_config.format`).
- Scans — text first; `file_id` only on the server; page and size limits.

</details>

→ [Advanced theory](advanced.en.md)
