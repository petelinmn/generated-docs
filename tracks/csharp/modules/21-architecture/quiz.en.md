---
title: "Quiz: Application architecture"
lang: en
track: csharp
kind: module
doc: quiz
slug: 21-architecture
part: 3
tags: [architecture, dependency-rule, ddd, cqrs, aggregate, architecture-tests]
status: draft
updated: 2026-11-22
---

# Quiz: Application architecture

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What does the dependency rule say?

- A) the domain must depend on the database
- B) dependencies point inward, to the domain: the domain knows nothing about infrastructure and the web
- C) all layers depend on each other
- D) infrastructure must not reference the application

<details><summary>Answer</summary>

**B.** Verified on the solution: `Domain` references nothing but the platform; `Application` — `Domain`; `Infrastructure` — `Application` and `Domain`.
→ [Basics](basics.en.md#layers)

</details>

## 2. 🟢 Where should business rules ("an empty order cannot be placed") live?

- A) in a controller
- B) in an SQL query
- C) in a domain object (`Order.Place()`) that protects its invariants
- D) in the UI

<details><summary>Answer</summary>

**C.** A rich model protects invariants; verified: `new Order("EUR").Place()` throws `InvalidOperationException`, and so does `AddLine` after `Place`.
→ [Basics](basics.en.md#the-domain-a-rich-model)

</details>

## 3. 🟢 Are `new Money(5, "EUR")` and `new Money(5, "EUR")` equal if `Money` is a `readonly record struct`?

- A) no: different objects
- B) yes: a value object is compared by contents
- C) depends on the platform
- D) a compile error

<details><summary>Answer</summary>

**B.** Value objects are equal by value (verified: `Assert.Equal` passes).
→ [Basics](basics.en.md#the-domain-a-rich-model)

</details>

## 4. 🟢 Where are the concrete implementations of dependencies registered?

- A) in every class that needs them
- B) in the composition root (`Program.cs`)
- C) in the domain
- D) in the database

<details><summary>Answer</summary>

**B.** The composition root is the single place where layers are tied together: `AddApplication().AddInfrastructure()`.
→ [Basics](basics.en.md#the-application-and-the-composition-root)

</details>

## 5. 🟡 Who owns the interface `IOrderRepository` in ports-and-adapters architecture?

- A) infrastructure
- B) the application layer (a port); infrastructure implements it (an adapter)
- C) the UI
- D) nobody

<details><summary>Answer</summary>

**B.** The consumer (the application) defines the port, so the domain and use cases are tested with a fake repository without a database (verified).
→ [Advanced theory](advanced.en.md#clean-architecture-and-hexagonal)

</details>

## 6. 🟡 How does a command differ from a query in CQRS?

- A) it does not
- B) a command changes state, a query returns data and does not change state
- C) a query is faster than a command
- D) a command is always asynchronous

<details><summary>Answer</summary>

**B.** `PlaceOrderCommand` creates an order and returns an `OrderId`; `GetOrderSummaryQuery` returns an `OrderSummary` or `null` (verified).
→ [Advanced theory](advanced.en.md#cqrs-commands-and-queries)

</details>

## 7. 🟡 Select all correct statements about an aggregate

- [ ] A) changes inside an aggregate go through the root
- [ ] B) an aggregate is a boundary of consistency and a transaction
- [ ] C) another aggregate is better referenced by identifier
- [ ] D) an aggregate should include the whole database

<details><summary>Answer</summary>

**A, B, C.** D is wrong: large aggregates produce locks and conflicts.
→ [Advanced theory](advanced.en.md#ddd-basics)

</details>

## 8. 🟡 Vertical slices are…

- A) grouping code by technical role
- B) grouping code by capability (feature): the request, handler, validation and entry point together
- C) splitting the database
- D) the same as microservices

<details><summary>Answer</summary>

**B.** Slices gather a feature entirely; layers — by role. The approaches combine: a shared domain and use cases by slices.
→ [Advanced theory](advanced.en.md#vertical-slices)

</details>

## 9. 🟡 Open question: when are CQRS and DDD not needed?

<details><summary>Key points of a good answer</summary>

- a simple CRUD application without complex business rules: a simple structure is enough;
- small services, prototypes, internal utilities;
- DDD pays off with a complex domain and work with experts; CQRS — with different read and write requirements;
- the principle: the complexity of the architecture must match the complexity of the task.

</details>

## 10. 🔴 What is the Outbox pattern and what problem does it solve?

- A) a mail queue
- B) writing the event in the same transaction as the data change and publishing it separately: it solves the "saved but not published" problem
- C) a query cache
- D) a way to compress a database

<details><summary>Answer</summary>

**B.** Saving an aggregate and publishing an event are two operations; Outbox makes the event part of the data transaction and the publication reliable ("at least once" delivery → idempotent consumers).
→ [Expert theory](expert.en.md#events-and-consistency)

</details>

## 11. 🔴 An architecture test found a `Microsoft.CSharp` reference in `Application` that you did not add. What happened?

- A) a bug in the test
- B) `dynamic` is used in the project (the mediator's dispatcher), and the compiler added the dependency
- C) NuGet slipped in a package
- D) it is a reference to the domain

<details><summary>Answer</summary>

**B.** Verified: because of `dynamic` in `Mediator`, a reference to `Microsoft.CSharp` appeared. Such unexpected dependencies are what architecture tests catch; `dynamic` is replaced with a generator or explicit code.
→ [Advanced theory](advanced.en.md#architecture-tests)

</details>
