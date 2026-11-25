---
title: "Quiz: OpenSearch basics"
lang: en
track: opensearch
kind: module
doc: quiz
slug: 01-opensearch-basics
part: 1
tags: [opensearch, index, document, shard, replica, refresh, bulk, optimistic-concurrency]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Quiz: OpenSearch basics

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What is an index?

- A) A process on a server
- B) A copy of a shard
- C) A set of documents with common settings
- D) A single JSON object

<details><summary>Answer</summary>

**C.** D is a document, A a node, B a replica.
→ [Fundamentals](basics.en.md)

</details>

## 2. 🟢 A document is written without the `refresh` parameter. What is true right after the response?

- A) `get` by `_id` finds it, search may not yet
- B) Neither `get` nor search finds it
- C) Both find it
- D) Search finds it, `get` does not

<details><summary>Answer</summary>

**A.** Search waits for a refresh.
→ [Fundamentals](basics.en.md)

</details>

## 3. 🟢 `index` with an already taken `_id`:

- A) merges the fields
- B) returns 409
- C) creates a second document
- D) replaces the whole document

<details><summary>Answer</summary>

**D.** `update` merges, `create` refuses.
→ [Fundamentals](basics.en.md)

</details>

## 4. 🟢 One node, an index with one replica. The status?

- A) the index is not created
- B) `yellow`
- C) `green`
- D) `red`

<details><summary>Answer</summary>

**B.** A replica is not placed on the same node as its primary.
→ [Fundamentals](basics.en.md)

</details>

## 5. 🟡 What does a second `update` with the same value report?

- A) `created`
- B) `noop`, the version unchanged
- C) `updated`, the version grew
- D) 409

<details><summary>Answer</summary>

**B.**
→ [Fundamentals](basics.en.md)

</details>

## 6. 🟡 Which statements about `_bulk` are true?

- [ ] A) Each operation has its own status
- [ ] B) One error rolls everything back
- [ ] C) It is more efficient than many single requests
- [ ] D) It is not atomic

<details><summary>Answer</summary>

**A, C, D.**
→ [Fundamentals](basics.en.md)

</details>

## 7. 🟡 A document written with `routing=alice` is read without it in a three-shard index. What happened in the experiment?

- A) 404
- B) the document was found
- C) 400
- D) 409

<details><summary>Answer</summary>

**A.** The request went to another shard. If the `_id` hash happens to point at the same shard the document is found — never rely on that.
→ [Advanced theory](advanced.en.md)

</details>

## 8. 🟡 How do you avoid losing someone's change in read → modify → write?

- A) add replicas
- B) `_bulk`
- C) `if_seq_no` and `if_primary_term`
- D) `refresh=true`

<details><summary>Answer</summary>

**C.** A conflict gives 409.
→ [Advanced theory](advanced.en.md)

</details>

## 9. 🟡 What can be changed on an open index?

- A) `number_of_shards`
- B) the type of an existing field
- C) the index name
- D) `number_of_replicas`

<details><summary>Answer</summary>

**D.**
→ [Advanced theory](advanced.en.md)

</details>

## 10. 🔴 `from=9995, size=10` with default settings:

- A) returns an empty list without an error
- B) a 400 error
- C) returns 5 documents
- D) returns 10 documents

<details><summary>Answer</summary>

**B.** `from + size` exceeds `max_result_window` = 10000.
→ [Expert theory](expert.en.md)

</details>

## 11. 🔴 Which operation makes data searchable?

- A) forcemerge
- B) syncing the translog
- C) refresh
- D) flush

<details><summary>Answer</summary>

**C.** flush is a Lucene commit.
→ [Expert theory](expert.en.md)

</details>

## 12. 🔴 Open question: product change events come from a queue and may repeat and arrive out of order. How do you write them to the index?

<details><summary>Answer</summary>

`_id` is the product id (a repeat creates no duplicate); `version_type=external` with the source's version (an old event gets 409 and is skipped); `_bulk` batches with error handling; for deletes consider `gc_deletes`, during which the version of a deleted document is remembered.
→ [Advanced theory](advanced.en.md)

</details>
