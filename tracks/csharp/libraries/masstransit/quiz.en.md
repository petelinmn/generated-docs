---
title: "Quiz: MassTransit"
lang: en
track: csharp
kind: library
doc: quiz
slug: masstransit
part: 3
tags: [masstransit, messaging, consumer, retry, saga, test-harness]
status: draft
updated: 2026-11-24
---

# Quiz: MassTransit

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. The label shows the question level: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What must you do for a message to be publishable?

- A) Declare the type in any (non-global) namespace
- B) Inherit it from `IMessage`
- C) Mark it with a `[Message]` attribute
- D) Nothing: any type will do

<details><summary>Answer</summary>

**A.** A type in the global namespace gives `ArgumentException: Messages types must have a valid namespace`.
→ [Basics](basics.en.md#a-message-contract)

</details>

## 2. 🟢 How does `Publish` differ from `Send`?

- A) `Publish` goes to all subscribers of the type, `Send` to one endpoint
- B) `Publish` is synchronous, `Send` asynchronous
- C) `Send` goes to all subscribers, `Publish` to one
- D) No difference

<details><summary>Answer</summary>

**A.**
→ [Basics](basics.en.md#publish-and-send)

</details>

## 3. 🟢 A container with MassTransit is disposed via `using var sp`. What happens?

- A) All is well
- B) `InvalidOperationException`: you need `await using`
- C) Messages are lost silently
- D) A compile error

<details><summary>Answer</summary>

**B.**
→ [Basics](basics.en.md#an-in-memory-bus)

</details>

## 4. 🟢 What is the endpoint for `OrderSubmittedConsumer` called by default?

- A) `order-submitted`
- B) `OrderSubmitted`
- C) `OrderSubmittedConsumer`
- D) `consumer`

<details><summary>Answer</summary>

**B.** `kebab-case` is enabled by `SetKebabCaseEndpointNameFormatter()`.
→ [Basics](basics.en.md#endpoint-names)

</details>

## 5. 🟡 A consumer fails twice and succeeds on the third time. What must you configure?

- A) `UseMessageRetry` on the endpoint
- B) `ConcurrentMessageLimit`
- C) `AddRequestClient`
- D) Nothing: MassTransit retries by itself

<details><summary>Answer</summary>

**A.** In the experiment: attempts 1, 2, 3 and success.
→ [Advanced](advanced.en.md#retries)

</details>

## 6. 🟡 Retries are exhausted. What about the message?

- A) It is deleted
- B) A `Fault<T>` is published, the message sits in `<queue>_error` with `MT-Fault-*` headers
- C) It returned to the original queue
- D) It went to `_skipped`

<details><summary>Answer</summary>

**B.**
→ [Advanced](advanced.en.md#errors-faultt-and-the-_error-queue)

</details>

## 7. 🟡 What does an `IRequestClient<T>` get if no consumer handles the request?

- A) An empty response
- B) A `RequestTimeoutException` after the timeout
- C) An endless wait
- D) `null`

<details><summary>Answer</summary>

**B.**
→ [Advanced](advanced.en.md#request-and-response)

</details>

## 8. 🟡 Choose all true statements

- [ ] A) In RabbitMQ a fanout exchange `Namespace:Type` is created per message type
- [ ] B) The test harness lets you avoid `Task.Delay` in tests
- [ ] C) `ConcurrentMessageLimit=1` processes messages one at a time
- [ ] D) Retries return the message to the broker's queue between attempts

<details><summary>Answer</summary>

**A, B, C.** D is wrong: retries run in the process.
→ [Advanced](advanced.en.md)

</details>

## 9. 🔴 `OrderShipped` arrives while the saga is in `Submitted` and the transition is not described. What happens?

- A) The saga moves to `Paid`
- B) A `NotAcceptedStateMachineException` and a `Fault`; the state does not change
- C) The message silently vanishes
- D) The saga is deleted

<details><summary>Answer</summary>

**B.**
→ [Sagas](expert.en.md#sagas-and-state-machines)

</details>

## 10. 🔴 Open question

An order service publishes `OrderSubmitted`, three other services react. How do you configure consumers, retries and tests? What must you check before choosing a MassTransit version?

<details><summary>Key points of a good answer</summary>

- An event is `Publish`; each service is a separate consumer with its own queue; the message type in a stable namespace.
- Idempotent consumers; `UseMessageRetry`; monitoring of the `_error` queue and `Fault<T>`.
- Tests on the test harness (`Consumed.Any<T>()`), no `Task.Delay`; `await using`.
- The version: 8.x is open, 9.x commercial — check the licence; usage telemetry.
- Not verified: the outbox, delayed redelivery, persistent saga repositories.

</details>

→ [Expert](expert.en.md)
