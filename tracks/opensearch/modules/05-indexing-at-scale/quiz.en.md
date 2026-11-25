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

**C.** `_bulk` sends many operations per round trip. One-by-one requests waste time on overhead, especially with `refresh=true`, and `_search` writes nothing.
→ [Fundamentals](basics.en.md)

</details>

## 2. 🟢 What is an alias?

- A) an extra name for an index or a group
- B) a copy of an index
- C) a replica
- D) an analyzer

<details><summary>Answer</summary>

**A.** An alias is a name pointing at one or more indexes; it copies no data. Replicas and analysers are different things.
→ [Fundamentals](basics.en.md)

</details>

## 3. 🟢 What does `reindex` not carry over?

- A) documents
- B) field values
- C) ids
- D) mappings and settings

<details><summary>Answer</summary>

**D.** `reindex` moves documents with their fields and IDs, while the destination with the right mapping and settings must be created beforehand.
→ [Fundamentals](basics.en.md)

</details>

## 4. 🟢 `helpers.bulk` raised `BulkIndexError`. What about the other operations of the batch?

- A) the server will retry them
- B) the successful ones are already applied
- C) all are rolled back
- D) they are queued

<details><summary>Answer</summary>

**B.** A batch is not atomic: successful operations are already applied and the exception reports the failed ones. The server does not retry, cancel or queue anything.
→ [Fundamentals](basics.en.md)

</details>

## 5. 🟡 A template was changed. What about existing indexes?

- A) they are recreated
- B) they do not change
- C) they change at once
- D) they change after a refresh

<details><summary>Answer</summary>

**B.** A template applies only when an index is created. Existing indexes are not recreated or changed, now or after refresh.
→ [Advanced theory](advanced.en.md)

</details>

## 6. 🟡 An alias over two indexes with no write index. What is true?

- [ ] A) a write fails
- [ ] B) a write goes to both
- [ ] C) a write goes to the newest
- [ ] D) search works

<details><summary>Answer</summary>

**A, D.** With several indexes and no `is_write_index` the target is ambiguous, so writes are rejected while search covers both. The alias does not pick the newest one.
→ [Advanced theory](advanced.en.md)

</details>

## 7. 🟡 When are `rollover` conditions checked with a manual API call?

- A) only at the moment of the call
- B) continuously in the background
- C) on every write
- D) once a minute

<details><summary>Answer</summary>

**A.** A `_rollover` call is a one-off check and creates no background watcher. Automatic checking is the job of ISM.
→ [Advanced theory](advanced.en.md)

</details>

## 8. 🟡 How do you switch an alias with no gap?

- A) two separate calls
- B) with a template
- C) `remove` and `add` in one `_aliases` call
- D) delete the index, then create

<details><summary>Answer</summary>

**C.** Actions in one `_aliases` call apply atomically. Two calls, or delete then create, leave a gap, and a template does not switch aliases.
→ [Fundamentals](basics.en.md)

</details>

## 9. 🔴 What is risked by removing replicas during a load?

- A) nothing
- B) losing the mapping
- C) deleting the alias
- D) data loss if a node fails

<details><summary>Answer</summary>

**D.** Without replicas each shard has one copy, and a node failure loses it. Mappings and aliases live in cluster state and do not depend on replicas.
→ [Advanced theory](advanced.en.md)

</details>

## 10. 🔴 What is needed before `split`?

- A) nothing
- B) block writes on the source
- C) delete the source
- D) shut down the cluster

<details><summary>Answer</summary>

**B.** `split` builds a new index from the source's segments, so writes to the source must be blocked. There is no need to delete the source or close the cluster.
→ [Expert theory](expert.en.md)

</details>

## 11. 🔴 What does a repeated `reindex` with `op_type: create` and `conflicts: proceed` copy?

- A) nothing, ever
- B) only changed ones
- C) only documents missing in the destination
- D) everything again

<details><summary>Answer</summary>

**C.** `create` writes only missing documents, and `proceed` keeps conflicts on existing ones from stopping the task. Changed versions are not updated.
→ [Advanced theory](advanced.en.md)

</details>

## 12. 🔴 Open question: change the analyzer of an index under constant writes. The plan?

<details><summary>Answer</summary>

A new index with the analyzer; the application reads through an alias; writes are either duplicated to both indexes or replayed from a queue; `reindex` with `op_type: create` or an external version so fresh data is not overwritten; compare counts and results; switch atomically; keep the old index until confirmed. Deletes must be handled too.
→ [Advanced theory](advanced.en.md)

</details>
