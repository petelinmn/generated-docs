---
title: "Quiz: API design"
lang: en
track: system-design
kind: module
doc: quiz
slug: 05-api-design
part: 1
tags: [system-design, api-design, rest, grpc, idempotency, optimistic-concurrency, keyset-pagination, problem-details]
versions: ".NET 11, C# 15"
status: verified
updated: 2026-11-26
---

# Quiz: API design

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows the level: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 Which method is not declared idempotent?

- A) GET
- B) PUT
- C) DELETE
- D) POST

<details><summary>Answer</summary>

**D.** → [Basics](basics.en.md)

</details>

## 2. 🟢 What do you return after creating a resource?

- A) 200 with no headers
- B) 201 with `Location`
- C) 302
- D) 409

<details><summary>Answer</summary>

**B.** → [Basics](basics.en.md)

</details>

## 3. 🟢 What is `Idempotency-Key` for?

- A) Authentication
- B) Recognizing a repeat of the same request
- C) Pagination
- D) Compression

<details><summary>Answer</summary>

**B.** → [Basics](basics.en.md)

</details>

## 4. 🟢 What is the content type of problem details in JSON?

- A) `application/error+json`
- B) `text/problem`
- C) `application/problem+json`
- D) `application/json-patch+json`

<details><summary>Answer</summary>

**C.** → [Basics](basics.en.md)

</details>

## 5. 🟡 A client updated a resource with a stale `If-Match`. The code?

- A) 200
- B) 412
- C) 428
- D) 404

<details><summary>Answer</summary>

**B.** 428 is for a missing precondition. → [Advanced](advanced.en.md)

</details>

## 6. 🟡 Select all compatible protobuf changes

- [ ] A) Adding a field to a response
- [ ] B) Changing a field number
- [ ] C) Adding a method
- [ ] D) Renaming a service

<details><summary>Answer</summary>

**A, C.** → [Advanced](advanced.en.md)

</details>

## 7. 🟡 What does this code print?

```csharp
List<Post> posts = [new(1, "a"), new(2, "b"), new(3, "c"), new(4, "d")];
var first = Pagination.ByOffset(posts, 0, 2);
posts.Add(new Post(5, "e"));
var second = Pagination.ByOffset(posts, 2, 2);
Console.WriteLine(string.Join(" ", second.Select(p => p.Id)));
```

<details><summary>Answer</summary>

```text
3 2
```

Post 3 was already on the first page. → [Advanced](advanced.en.md)

</details>

## 8. 🟡 Find the mistake

"If a request fails, the client generates a new idempotency key and retries."

<details><summary>Answer</summary>

The server will not recognize the repeat and will run the operation a second time. A key is created once per intent. → [Advanced](advanced.en.md)

</details>

## 9. 🔴 What does this code print?

```csharp
Console.WriteLine(JsonSerializer.Serialize(new { Day = DayOfWeek.Monday }));
```

<details><summary>Answer</summary>

```text
{"Day":1}
```

→ [Expert](expert.en.md)

</details>

## 10. 🔴 Open question

Design a payment API that survives a lost response.

<details><summary>Key points of a good answer</summary>

- An idempotency key, a body fingerprint, a retention period.
- An atomic claim and a write together with the effect.
- What to do when an external provider did not answer: an "unknown" state and reconciliation.
- Codes 400, 409, 422 and 202.

</details>
