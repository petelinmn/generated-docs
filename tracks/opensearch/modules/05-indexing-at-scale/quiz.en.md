---
title: "Quiz: indexing at scale"
lang: en
track: opensearch
kind: module
doc: quiz
slug: 05-indexing-at-scale
part: 2
tags: [bulk, index-template, alias, reindex, rollover]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Quiz: indexing at scale

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 How do you load a million documents?

- A) one at a time with `refresh=true`
- B) through `_search`
- C) in `_bulk` batches
- D) one `index` at a time

<details><summary>Answer</summary>

**C.**
→ [Fundamentals](basics.en.md)

</details>

## 2. 🟢 What is an alias?

- A) an extra name for an index or a group
- B) a copy of an index
- C) a replica
- D) an analyzer

<details><summary>Answer</summary>

**A.**
→ [Fundamentals](basics.en.md)

</details>

## 3. 🟢 What does `reindex` not carry over?

- A) documents
- B) field values
- C) ids
- D) mappings and settings

<details><summary>Answer</summary>

**D.**
→ [Fundamentals](basics.en.md)

</details>

## 4. 🟢 `helpers.bulk` raised `BulkIndexError`. What about the other operations of the batch?

- A) the server will retry them
- B) the successful ones are already applied
- C) all are rolled back
- D) they are queued

<details><summary>Answer</summary>

**B.**
→ [Fundamentals](basics.en.md)

</details>

## 5. 🟡 A template was changed. What about existing indexes?

- A) they are recreated
- B) they do not change
- C) they change at once
- D) they change after a refresh

<details><summary>Answer</summary>

**B.**
→ [Advanced theory](advanced.en.md)

</details>

## 6. 🟡 An alias over two indexes with no write index. What is true?

- [ ] A) a write fails
- [ ] B) a write goes to both
- [ ] C) a write goes to the newest
- [ ] D) search works

<details><summary>Answer</summary>

**A, D.**
→ [Advanced theory](advanced.en.md)

</details>

## 7. 🟡 When are `rollover` conditions checked with a manual API call?

- A) only at the moment of the call
- B) continuously in the background
- C) on every write
- D) once a minute

<details><summary>Answer</summary>

**A.**
→ [Advanced theory](advanced.en.md)

</details>

## 8. 🟡 How do you switch an alias with no gap?

- A) two separate calls
- B) with a template
- C) `remove` and `add` in one `_aliases` call
- D) delete the index, then create

<details><summary>Answer</summary>

**C.**
→ [Fundamentals](basics.en.md)

</details>

## 9. 🔴 What is risked by removing replicas during a load?

- A) nothing
- B) losing the mapping
- C) deleting the alias
- D) data loss if a node fails

<details><summary>Answer</summary>

**D.**
→ [Advanced theory](advanced.en.md)

</details>

## 10. 🔴 What is needed before `split`?

- A) nothing
- B) block writes on the source
- C) delete the source
- D) shut down the cluster

<details><summary>Answer</summary>

**B.**
→ [Expert theory](expert.en.md)

</details>

## 11. 🔴 What does a repeated `reindex` with `op_type: create` and `conflicts: proceed` copy?

- A) nothing, ever
- B) only changed ones
- C) only documents missing in the destination
- D) everything again

<details><summary>Answer</summary>

**C.**
→ [Advanced theory](advanced.en.md)

</details>

## 12. 🔴 Open question: change the analyzer of an index under constant writes. The plan?

<details><summary>Answer</summary>

A new index with the analyzer; the application reads through an alias; writes are either duplicated to both indexes or replayed from a queue; `reindex` with `op_type: create` or an external version so fresh data is not overwritten; compare counts and results; switch atomically; keep the old index until confirmed. Deletes must be handled too.
→ [Advanced theory](advanced.en.md)

</details>
