---
title: "Quiz: gRPC service"
lang: en
track: csharp
kind: recipe
doc: quiz
slug: grpc-service
part: 2
tags: [grpc, protobuf, streaming, deadline]
status: draft
updated: 2026-11-22
---

# Quiz: gRPC service

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [minimal](basics.en.md) · 🟡 [production](advanced.en.md) · 🔴 [scale and edges](expert.en.md).

## 1. 🟢 Where is a gRPC service's contract described?

- A) in JSON Schema
- B) in a `.proto` file (Protocol Buffers)
- C) in XML
- D) in controller attributes

<details><summary>Answer</summary>

**B.** The server base and the client are generated from `.proto`.
→ [Minimal](basics.en.md#the-proto-contract)

</details>

## 2. 🟢 What did `Countdown(3)` (server streaming) return?

- A) `3`
- B) `3,2,1`
- C) `1,2,3`
- D) an error

<details><summary>Answer</summary>

**B.** Verified: `3,2,1`.
→ [Minimal](basics.en.md#four-kinds-of-calls)

</details>

## 3. 🟢 What does the client-streaming `Sum` return for `1,2,3,4`?

- A) `4`
- B) `10`
- C) `24`
- D) `0`

<details><summary>Answer</summary>

**B.** Verified: `sum = 10`; the client must call `CompleteAsync()`.
→ [Minimal](basics.en.md#four-kinds-of-calls)

</details>

## 4. 🟢 What goes over the wire in a protobuf message?

- A) field names
- B) field numbers and values
- C) JSON
- D) XML

<details><summary>Answer</summary>

**B.** That is why field numbers must not change.
→ [Minimal](basics.en.md#the-proto-contract)

</details>

## 5. 🟡 Which status did the client get for an empty name?

- A) `Internal`
- B) `InvalidArgument`
- C) `NotFound`
- D) HTTP 400

<details><summary>Answer</summary>

**B.** Verified: `InvalidArgument / name is required`; gRPC uses its own statuses, not HTTP codes.
→ [Production](advanced.en.md#errors-and-statuses)

</details>

## 6. 🟡 How did a call end where the server waited 2 s and the deadline was 200 ms?

- A) `OK`
- B) `DeadlineExceeded`
- C) `Unavailable`
- D) a hang

<details><summary>Answer</summary>

**B.** Verified.
→ [Production](advanced.en.md#deadlines-and-cancellation)

</details>

## 7. 🟡 Select all correct statements

- [ ] A) metadata is the analogue of HTTP headers
- [ ] B) a `GrpcChannel` should be created per call
- [ ] C) adding a field with a new number is compatible
- [ ] D) changing a field type is compatible

<details><summary>Answer</summary>

**A, C.** B is wrong: a channel is long-lived; D is wrong: changing a number/type breaks compatibility.
→ [Production](advanced.en.md#contract-evolution)

</details>

## 8. 🟡 Open question: how would you organize a call between two microservices over gRPC?

<details><summary>Key points of a good answer</summary>

- a `.proto` contract with a package version, a shared package/generation;
- a long-lived channel, a deadline on every call, retries for `Unavailable` and idempotent calls;
- metadata for tracing and tokens, interceptors for logs/metrics;
- L7 or client-side balancing.

</details>

## 9. 🔴 Why is an L4 balancer poor for gRPC?

- A) it does not support TCP
- B) a long HTTP/2 connection pins all of a client's calls to one server
- C) it encrypts traffic
- D) it does not pass headers

<details><summary>Answer</summary>

**B.** You need L7 or client-side balancing.
→ [Scale and edges](expert.en.md#scale)

</details>

## 10. 🔴 In proto3 the field `int32 age` arrived as 0. What does it mean?

- A) the user entered 0
- B) the field is unset
- C) indistinguishable: you need `optional` or a wrapper
- D) an error

<details><summary>Answer</summary>

**C.** Default values are indistinguishable from absence.
→ [Scale and edges](expert.en.md#edges-and-gotchas)

</details>
