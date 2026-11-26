---
title: "Quiz: data storage"
lang: en
track: system-design
kind: module
doc: quiz
slug: 04-data-storage
part: 1
tags: [system-design, data-modeling, database-index, b-tree, lsm-tree, query-plan, nosql]
versions: ".NET 11, C# 15; Microsoft.Data.Sqlite 11.0.0"
status: draft
updated: 2026-11-26
---

# Quiz: data storage

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows the level: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 Where does choosing a store start?

- A) With popularity
- B) With access patterns
- C) With the application language
- D) With the disk price

<details><summary>Answer</summary>

**B.** The model and indexes follow from the queries. → [Basics](basics.en.md)

</details>

## 2. 🟢 What does an index do to writes?

- A) Speeds them up
- B) Slows them down
- C) No effect
- D) Always forbids duplicates

<details><summary>Answer</summary>

**B.** It has to be updated too. D holds only for a unique index. → [Basics](basics.en.md)

</details>

## 3. 🟢 What does `SCAN` mean in an SQLite plan?

- A) Going through the whole table or index
- B) A lookup by key
- C) A covering index
- D) An error

<details><summary>Answer</summary>

**A.** Looking up part of the rows is `SEARCH`. → [Basics](basics.en.md)

</details>

## 4. 🟢 Where do video files belong?

- A) In a column of the main table
- B) In object storage, with a link in the database
- C) In a cache
- D) In a search index

<details><summary>Answer</summary>

**B.** → [Basics](basics.en.md)

</details>

## 5. 🟡 Which index is best for `WHERE chat_id = ? ORDER BY sent_at DESC`?

- A) `(sent_at)`
- B) `(sent_at, chat_id)`
- C) `(chat_id, sent_at)`
- D) `(body)`

<details><summary>Answer</summary>

**C.** The filter and the order are served in one pass. → [Advanced](advanced.en.md)

</details>

## 6. 🟡 Select all correct statements about LSM

- [ ] A) On-disk segments are immutable
- [ ] B) A delete erases all old copies immediately
- [ ] C) A read may check several structures
- [ ] D) Compaction removes old versions

<details><summary>Answer</summary>

**A, C, D.** A delete only writes a tombstone. → [Advanced](advanced.en.md)

</details>

## 7. 🟡 What does this code print?

```csharp
var store = new LsmStore(memtableLimit: 2);
store.Put("a", "1"); store.Put("b", "2");
store.Put("a", "10"); store.Delete("b");
Console.WriteLine(store.StoredEntries);
store.Compact();
Console.WriteLine(store.StoredEntries);
```

<details><summary>Answer</summary>

```text
4
1
```

Before compaction two versions of `a`, the value of `b` and its tombstone are stored. → [Advanced](advanced.en.md)

</details>

## 8. 🟡 What is the price of denormalization?

- A) More joins
- B) Copies must be kept in agreement
- C) No indexing
- D) No transactions

<details><summary>Answer</summary>

**B.** → [Advanced](advanced.en.md)

</details>

## 9. 🔴 Find the problem

```sql
CREATE INDEX ix_email ON users (email);
SELECT name FROM users WHERE lower(email) = 'a@b.c';
```

<details><summary>Answer</summary>

The index is not used (`SCAN users`). You need an index on `lower(email)` or store the email normalized. → [Expert](expert.en.md)

</details>

## 10. 🔴 Open question

Design storage for chat messages with text search.

<details><summary>Key points of a good answer</summary>

- Access patterns and volumes.
- The key `(chat_id, sent_at)`, possibly partitioning by chat.
- Search is a separate index updated asynchronously.
- Name the indexing delay and the source of truth.

</details>
