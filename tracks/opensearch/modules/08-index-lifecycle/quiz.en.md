---
title: "Quiz: index lifecycle"
lang: en
track: opensearch
kind: module
doc: quiz
slug: 08-index-lifecycle
part: 2
tags: [index-lifecycle, data-stream, ism, retention, rollover]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Quiz: index lifecycle

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 Which field must data stream documents have?

- A) `date`
- B) `created_at`
- C) `@timestamp`
- D) `_id`

<details><summary>Answer</summary>

**C.**
→ [Fundamentals](basics.en.md)

</details>

## 2. 🟢 Where does a new stream document go?

- A) the current write index
- B) the index matching its date
- C) every backing index
- D) the first index

<details><summary>Answer</summary>

**A.**
→ [Fundamentals](basics.en.md)

</details>

## 3. 🟢 What is allowed on a stream name?

- A) overwrite by `_id`
- B) delete by `_id`
- C) partial `update` by `_id`
- D) adding with `create`

<details><summary>Answer</summary>

**D.**
→ [Fundamentals](basics.en.md)

</details>

## 4. 🟢 Can the stream's current write index be deleted?

- A) yes, but the data stays
- B) no, the request is rejected
- C) yes
- D) yes, the stream falls back to an old one

<details><summary>Answer</summary>

**B.**
→ [Fundamentals](basics.en.md)

</details>

## 5. 🟡 The default `job_interval`?

- A) continuously
- B) 5 minutes
- C) 1 minute
- D) 1 hour

<details><summary>Answer</summary>

**B.**
→ [Advanced theory](advanced.en.md)

</details>

## 6. 🟡 What does an ordinary index need for ISM rollover?

- [ ] A) the `rollover_alias` setting
- [ ] B) an `@timestamp` mapping
- [ ] C) two replicas
- [ ] D) a write alias

<details><summary>Answer</summary>

**A, D.**
→ [Advanced theory](advanced.en.md)

</details>

## 7. 🟡 A policy was changed. An already attached index:

- A) keeps the old version until `change_policy`
- B) switches at once
- C) is deleted
- D) becomes failed

<details><summary>Answer</summary>

**A.**
→ [Advanced theory](advanced.en.md)

</details>

## 8. 🟡 `min_index_age: 7d` in a transition counts from:

- A) the last write
- B) the `@timestamp` value
- C) index creation
- D) entering the state

<details><summary>Answer</summary>

**C.**
→ [Advanced theory](advanced.en.md)

</details>

## 9. 🔴 What retry default does ISM add to actions?

- A) no retries
- B) endless retries
- C) one retry after an hour
- D) 3 retries with exponential backoff

<details><summary>Answer</summary>

**D.**
→ [Expert theory](expert.en.md)

</details>

## 10. 🔴 A repeated `PUT` of an existing policy without sequence numbers:

- A) deletes the old one
- B) 409
- C) updates it
- D) creates a second one

<details><summary>Answer</summary>

**B.**
→ [Expert theory](expert.en.md)

</details>

## 11. 🔴 How do you fix one document in a stream?

- A) delete the stream
- B) `_rollover`
- C) write to the backing index with `if_seq_no` and `if_primary_term`
- D) `PUT stream/_doc/id`

<details><summary>Answer</summary>

**C.**
→ [Expert theory](expert.en.md)

</details>

## 12. 🔴 Open question: 100 GiB of logs a day, keep 30 days. What do you set up?

<details><summary>Answer</summary>

A data stream with a template; rollover by primary shard size and age; shard count for 30–50 GiB shards; `read_only` and `force_merge` after cooling; delete by age; snapshots if a copy is needed; disk sized with replicas and headroom.
→ [Advanced theory](advanced.en.md)

</details>
