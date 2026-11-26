---
title: "Quiz: case studies"
lang: en
track: system-design
kind: module
doc: quiz
slug: 17-design-case-studies
part: 3
tags: [system-design, case-studies, url-shortener, news-feed, notifications, id-generation]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-26
---

# Quiz: case studies

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows the level: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 The first step of a walkthrough?

- A) Choose a database
- B) Clarify requirements
- C) Draw a cluster
- D) Choose a language

<details><summary>Answer</summary>

**B.** → [Basics](basics.en.md)

</details>

## 2. 🟢 What does this code print?

```csharp
Console.WriteLine(Base62.Encode(62));
```

<details><summary>Answer</summary>

```text
10
```

→ [Basics](basics.en.md)

</details>

## 3. 🟢 You need click statistics and address changes. Which code?

- A) 301
- B) 302
- C) 200
- D) 404

<details><summary>Answer</summary>

**B.** 301 is heuristically cacheable. → [Basics](basics.en.md)

</details>

## 4. 🟢 Where do you count clicks?

- A) Synchronously in the links database
- B) Asynchronously through an event
- C) In the browser without sending
- D) Nowhere

<details><summary>Answer</summary>

**B.** → [Basics](basics.en.md)

</details>

## 5. 🟡 When is fan-out on write expensive?

- A) Few followers
- B) Millions of followers
- C) No posts
- D) Rare reads of one author

<details><summary>Answer</summary>

**B.** → [Advanced](advanced.en.md)

</details>

## 6. 🟡 What does a "time + node + sequence" generator need?

- [ ] A) A unique node number
- [ ] B) Protection against the clock jumping backwards
- [ ] C) A central counter for every id
- [ ] D) A bounded sequence within a millisecond

<details><summary>Answer</summary>

**A, B, D.** → [Advanced](advanced.en.md)

</details>

## 7. 🟡 A notification with the same id arrives again. What do you do?

- A) Send it again
- B) Treat it as a duplicate
- C) Put it into the dead letter list
- D) Delete the user

<details><summary>Answer</summary>

**B.** → [Advanced](advanced.en.md)

</details>

## 8. 🟡 Where does a notification go after the attempts run out?

- A) It is lost
- B) To the dead letter list
- C) It is sent forever
- D) Into the cache

<details><summary>Answer</summary>

**B.** → [Advanced](advanced.en.md)

</details>

## 9. 🔴 What does this code print?

```csharp
long id = (1L << 53) + 1;
Console.WriteLine((long)(double)id);
```

<details><summary>Answer</summary>

```text
9007199254740992
```

→ [Expert](expert.en.md)

</details>

## 10. 🔴 Open question

Design a chat with history and offline delivery.

<details><summary>Key points of a good answer</summary>

- Requirements and estimates.
- Partition by dialog, message numbers.
- Persistent connections and a queue for offline users.
- Duplicates, ordering, failures.

</details>
