---
title: "Quiz: cluster and shards"
lang: en
track: opensearch
kind: module
doc: quiz
slug: 07-cluster-and-shards
part: 2
tags: [cluster, shard, replica, shard-allocation, high-availability]
versions: "OpenSearch 3.10.1; opensearch-py 3.2.0"
status: draft
updated: 2026-11-25
---

# Quiz: cluster and shards

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What does yellow mean?

- A) the cluster is read-only
- B) the disk is nearly full
- C) primaries are there, some replicas are unassigned
- D) some data is unavailable

<details><summary>Answer</summary>

**C.** Yellow means all data is available but redundancy is incomplete. Unavailable data is red, and read-only mode is not what the status shows.
→ [Fundamentals](basics.en.md)

</details>

## 2. 🟢 Which can be changed on an existing index with a setting?

- A) the replica count
- B) the primary count
- C) both
- D) neither

<details><summary>Answer</summary>

**A.** Replicas can be added or removed at any time. The primary count is fixed at creation; changing it takes split, shrink or reindex.
→ [Fundamentals](basics.en.md)

</details>

## 3. 🟢 3 primaries and 2 replicas. How many shard copies?

- A) 5
- B) 6
- C) 3
- D) 9

<details><summary>Answer</summary>

**D.** Replicas are per primary shard: 3 × (1 + 2) = 9.
→ [Fundamentals](basics.en.md)

</details>

## 4. 🟢 A node left. When are its replicas rebuilt by default?

- A) never without a command
- B) after 1 minute
- C) immediately
- D) after 30 minutes

<details><summary>Answer</summary>

**B.** By default allocation is delayed by one minute (`delayed_timeout`) so a restarting node can return without copying. No manual command is needed.
→ [Fundamentals](basics.en.md)

</details>

## 5. 🟡 4 copies of a shard on 3 nodes. Result?

- A) red
- B) yellow, one copy unassigned
- C) two copies on one node
- D) the setting change fails

<details><summary>Answer</summary>

**B.** Two copies of a shard never share a node, so the fourth has nowhere to go. Data is available, hence yellow, and the setting is accepted without error.
→ [Advanced theory](advanced.en.md)

</details>

## 6. 🟡 Forced zones a,b,c, 2 replicas, zone c lost. The index?

- A) turns green on two nodes
- B) turns red
- C) is deleted
- D) stays yellow

<details><summary>Answer</summary>

**D.** Forced zone awareness refuses to pile every copy into the remaining zones: the copy meant for zone c waits. Data stays available, so the index is not red.
→ [Advanced theory](advanced.en.md)

</details>

## 7. 🟡 A search with `routing=acme` on a 3-shard index queries:

- A) 1 shard
- B) 3 shards
- C) all nodes
- D) replicas only

<details><summary>Answer</summary>

**A.** Routing computes the single shard holding documents with that value, and search goes only there, to any of its copies.
→ [Advanced theory](advanced.en.md)

</details>

## 8. 🟡 What drains shards off a node before removing it?

- A) `_refresh`
- B) `_close`
- C) `cluster.routing.allocation.exclude._name`
- D) `number_of_replicas: 0`

<details><summary>Answer</summary>

**C.** Excluding the node by name makes the cluster move its shards away. Refresh and close do not, and zero replicas only reduces safety.
→ [Advanced theory](advanced.en.md)

</details>

## 9. 🔴 1 of 3 cluster-manager-eligible nodes remains. What is true?

- [ ] A) searches over local shards may still answer
- [ ] B) the survivor elects itself
- [ ] C) everything works as usual
- [ ] D) the cluster state cannot change

<details><summary>Answer</summary>

**A, D.** Without a voting majority no cluster manager can be elected, so cluster state cannot change, though local shards may still serve reads. A lone node does not elect itself.
→ [Expert theory](expert.en.md)

</details>

## 10. 🔴 `_count` on a red index:

- A) returns 0
- B) may return a short count with no error
- C) always fails
- D) is always exact

<details><summary>Answer</summary>

**B.** Search allows partial results by default: unavailable shards are skipped and the count may be low. Check `_shards.failed` and the health status.
→ [Expert theory](expert.en.md)

</details>

## 11. 🔴 A 4-shard index can be shrunk to:

- A) 8
- B) any smaller number
- C) 2 or 1
- D) 3

<details><summary>Answer</summary>

**C.** Shrink needs a divisor of the original count: for 4 that is 2 or 1. Three does not divide it, and going to 8 is a split.
→ [Expert theory](expert.en.md)

</details>

## 12. 🔴 Open question: the cluster turned red after a node failure. What do you do?

<details><summary>Answer</summary>

Find which indices and shards lack a primary (`_cat/shards`, `allocation/explain`); bring the node and its disk back; with no copy left, restore from a snapshot. Forcing an empty primary loses data: a deliberate last resort only.
→ [Expert theory](expert.en.md)

</details>
