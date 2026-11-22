---
title: "Application architecture: expert"
lang: en
track: csharp
kind: module
doc: expert
slug: 21-architecture
part: 3
tags: [bounded-context, anti-corruption-layer, outbox-pattern, modular-monolith, microservices, event-sourcing, conways-law, interview-questions]
versions: ".NET 11, C# 15"
status: draft
updated: 2026-11-22
---

# Application architecture: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

**Prerequisites:** [basic](basics.en.md) and [advanced](advanced.en.md) theory. **Time:** ~1.5 h.

## Strategic DDD

Tactical DDD (entities, aggregates) describes the structure of one model. **Strategic** DDD is how to divide a large system into models.

- A **bounded context** is a boundary inside which terms have one meaning. A "customer" in sales (contacts, discounts) and a "customer" in delivery (address, time) are different models; one shared model for everything turns into a bloated "god" class.
- A **context map** describes the relationships between contexts: a shared kernel, customer–supplier, conformist, and an **anti-corruption layer** (ACL) — a layer that translates a foreign model into yours so that a legacy or external system does not pollute the domain.
- **Subdomains:** core (the competitive advantage — put your best effort here), supporting and generic (authentication, payments — buy off the shelf).

A context usually corresponds to a module of a monolith or a service, and the code boundaries should coincide with the model boundaries.

## Events and consistency

A **domain event** is a fact inside a context; an **integration event** is a message between contexts/services (a contract, versioned, not exposing internals). The problem: saving an aggregate to the database and **reliably** publishing an event are two operations with a possible failure between them (saved but not published — or vice versa). The solution is the **Outbox** pattern: the event is written to a table in the same transaction as the aggregate change, and a separate process reads the table and publishes messages (at least once, so consumers must be idempotent).

**Eventual consistency:** between aggregates and services data is reconciled not instantly but through events; the user interface and business rules must live with this. Distributed transactions (2PC) are usually avoided; long scenarios use **sagas** (chains of local transactions with compensations).

**Event sourcing** stores not the current state but the sequence of events from which it is restored; it gives an audit trail and a "time machine" but complicates queries and event schema migration. Apply it selectively, not by default.

## Monolith, modular monolith, microservices

| Option | Pros | Cons |
|---|---|---|
| A monolith without structure | starts fast | a "big ball of mud", hard to change |
| **A modular monolith** | module boundaries and one deployment, cheap debugging | needs discipline of boundaries (architecture tests) |
| Microservices | independent deployment and scaling, autonomous teams | distributed complexity: network, consistency, observability, contract versions |

In practice: start with a modular monolith and extract services when there is a real need (different pace of change, load, teams). **Conway's law:** a system's structure mirrors the organization's communication structure; service boundaries should follow contexts and teams, not layers.

## A repository on top of EF Core, and what about the mediator

- **`DbContext` already implements a "unit of work" and repositories** (`DbSet<T>`): an extra `IRepository<T>` layer over it often merely duplicates the API. Justified reasons: a port in the application layer to isolate from the ORM and for testability (like `IOrderRepository` above), queries with domain meaning. A universal `IRepository<T>` with `GetAll()` is usually harmful: queries leak.
- **A mediator hides control flow:** a `SendAsync` call does not show which code will run; finding the handler through `MakeGenericType` and `dynamic` costs time and drags in a dependency (in the verified solution `dynamic` added a reference to `Microsoft.CSharp` to `Application`, and the architecture test caught it). If there are few commands, a direct service call is simpler; if there are many — use a ready library or a generator ([20 · Reflection and source generators](../20-reflection-and-source-generators/overview.en.md)).
- **Validation:** formal (input format) — at the boundary; business invariants — in the domain; do not duplicate the same rule in three layers without need.

## Reference

| Concept | In brief |
|---|---|
| The dependency rule | dependencies point to the domain |
| Port / adapter | an interface at the consumer / an implementation in an outer layer |
| Aggregate | a consistency boundary, changes through the root |
| CQRS | separate write and read models |
| Outbox | the event is written in one transaction with the data |
| Saga | a chain of local transactions with compensation |
| Bounded context | the boundary of the meaning of terms |
| ACL | a translation layer between models |
| Modular monolith | modules with hard boundaries in one deployment |

## Gotchas

**1. An anemic model.** Entities are sets of properties, logic is in services: invariants are lost (a check is a set of `if`s in different places).

**2. A leaky abstraction.** `IRepository<T>` returns `IQueryable<T>` — the consumer depends on the ORM again.

**3. An aggregate that is too big.** Locks and conflicts; reference other aggregates by identifier, not by object.

**4. An event without reliability.** The order is saved but the event is not sent (or vice versa): you need an Outbox.

**5. Excess architecture.** CQRS/DDD/layers for a CRUD form — cost without gain; complexity must grow with the domain.

**6. Boundaries violated "silently".** Without architecture tests a reference from `Domain` to infrastructure will one day appear (verified: the test found an unexpected `Microsoft.CSharp` reference because of `dynamic`).

**7. One shared "Common/Shared" project.** It becomes a dumping ground and couples contexts; share as little as possible.

**8. Microservices too early.** Distributed complexity before there is a need.

## Trade-offs and alternatives

| Approach | When it fits |
|---|---|
| A simple structure (folders, few layers) | small services, CRUD |
| Layered | typical business applications |
| Clean/Hexagonal | long-lived systems with rich logic and tests |
| Vertical slices | many independent scenarios, a team that likes whole features |
| DDD (tactics) | complex domain rules |
| Microservices | independent teams and load, a mature platform |

## Interview questions

<details><summary>1. What is the dependency rule?</summary>

Dependencies point inward, to the domain: the domain does not know about the database and the web; the application knows the domain but not a concrete database. The inversion is achieved with interfaces at the consumer.

</details>

<details><summary>2. How does Clean/Hexagonal differ from ordinary layered architecture?</summary>

In layered architecture the business layer often depends on the data layer. In Clean/Hexagonal the dependency is inverted: ports (interfaces) belong to the core, adapters implement them from outside.

</details>

<details><summary>3. What are an aggregate and an aggregate root?</summary>

A group of objects kept consistent as a whole (the boundary of invariants and a transaction); outside code changes it only through the root (`Order`), which protects the rules.

</details>

<details><summary>4. Entity and value object: what is the difference?</summary>

An entity is defined by identity (`OrderId`), a value object by its contents and is immutable (`Money`: verified — objects equal by value are equal).

</details>

<details><summary>5. What is an anemic model and why is it bad?</summary>

A model without behavior: logic lives outside entities. Invariants are smeared across services and easily violated.

</details>

<details><summary>6. What is CQRS and when is it needed?</summary>

Separating write models (commands) and read models (queries). It is needed when read and write requirements differ or the logic is complex; for simple CRUD it is excessive.

</details>

<details><summary>7. What is a bounded context?</summary>

A boundary inside which the model and terms are unambiguous. Different contexts may model a "customer" differently; between them are explicit contracts.

</details>

<details><summary>8. How do you reliably publish an event after saving data?</summary>

Outbox: write the event in the same transaction, and a separate process will publish it; consumers are idempotent ("at least once" delivery).

</details>

<details><summary>9. A monolith or microservices?</summary>

Start with a modular monolith and extract services when truly needed; microservices add network and organizational complexity.

</details>

<details><summary>10. How do you protect the architecture from erosion?</summary>

Architecture tests in CI (checking references between assemblies and namespaces); verified: three dependency tests and a reaction to an unexpected reference.

</details>

<details><summary>11. Trick question: is an `IRepository<T>` on top of `DbContext` needed?</summary>

Not necessarily: `DbContext` is already a unit of work. A port is needed for isolation and testability with a domain interface (`FindAsync(OrderId)`), while a universal repository with `IQueryable` leaks.

</details>

<details><summary>12. Trick question: a `Domain` → `Microsoft.CSharp` reference appeared without changing the domain code. Where from?</summary>

From `dynamic` in the project (in our solution — in `Application`): the compiler adds the dependency. The architecture test found it; `dynamic` is replaced with a generator or explicit code.

</details>

## Related materials

- Back: [advanced theory](advanced.en.md). Check yourself: [quiz](quiz.en.md) and [exercises](exercises.en.md).
- [22 · Observability](../22-observability/overview.en.md) — tracing across services.
- [18 · Concurrency](../18-concurrency/overview.en.md) — concurrency of aggregates.
- [15 · EF Core](../15-ef-core/overview.en.md) — concurrency and transactions.
- [Kubernetes](../../../kubernetes/overview.en.md) — deploying microservices.

## Sources

- [.NET microservices architecture](https://learn.microsoft.com/dotnet/architecture/microservices/)
- [The Outbox pattern](https://learn.microsoft.com/azure/architecture/best-practices/transactional-outbox-cosmos)
- [Saga](https://learn.microsoft.com/azure/architecture/patterns/saga)
- [Anti-corruption layer](https://learn.microsoft.com/azure/architecture/patterns/anti-corruption-layer)
