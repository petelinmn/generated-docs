---
title: "Quiz: Integration tests"
lang: en
track: csharp
kind: module
doc: quiz
slug: 17-testing-integration
part: 2
tags: [integration-testing, web-application-factory, test-isolation, sqlite, test-server]
status: draft
updated: 2026-11-22
---

# Quiz: Integration tests

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What does `WebApplicationFactory<Program>` do?

- A) runs the application on a real port in Docker
- B) builds your application's host inside the test on an in-memory server and hands out an `HttpClient`
- C) generates unit tests
- D) replaces the compiler

<details><summary>Answer</summary>

**B.** It uses a `TestServer` with no network; `CreateClient()` gives a client with the address `http://localhost/` (verified).
→ [Basics](basics.en.md#webapplicationfactory)

</details>

## 2. 🟢 Why does an API project need `public partial class Program { }`?

- A) to speed up startup
- B) so that the test project can see the `Program` class of an application with top-level statements
- C) to enable Native AOT
- D) it is an error: extra code

<details><summary>Answer</summary>

**B.** The generated `Program` is internal; the partial declaration makes it accessible to `WebApplicationFactory<Program>`.
→ [Basics](basics.en.md#webapplicationfactory)

</details>

## 3. 🟢 Which codes did the API return in the verified scenarios?

- A) `POST /todos` with a title → `200`; an empty title → `500`
- B) `POST /todos` with a title → `201`; an empty title → `400` (`problem+json`); `GET /todos/999` → `404`
- C) everything returns `200`
- D) `GET /todos/999` → `204`

<details><summary>Answer</summary>

**B.** Creation gives `201` and `Location`, an empty title gives `400` with `application/problem+json`, an unknown `id` gives `404` (verified).
→ [Basics](basics.en.md#checking-the-response)

</details>

## 4. 🟢 Where does the test get a fixed time from?

- A) from `DateTime.Now`
- B) by replacing `IClock` in `ConfigureTestServices`
- C) from the container's system clock
- D) a test cannot control time

<details><summary>Answer</summary>

**B.** The clock service is removed and replaced with `FixedClock`; verified: the `CreatedAt` of the created task equals `ApiFactory.Now`.
→ [Basics](basics.en.md#replacing-dependencies)

</details>

## 5. 🟡 Why can one test's data "leak" into another?

- A) xUnit runs all tests on one thread
- B) a factory in `IClassFixture` is created once per class, and the tests of the class share the database
- C) that is how SQLite works
- D) it does not: data is always isolated

<details><summary>Answer</summary>

**B.** A shared factory means a shared database and singletons. Isolate with a new factory per test (`IAsyncLifetime`), clearing, unique data or transaction rollback.
→ [Advanced theory](advanced.en.md#data-isolation-between-tests)

</details>

## 6. 🟡 What happens if you do not keep the connection to `DataSource=:memory:` open?

- A) nothing: the database is kept
- B) the database vanishes with the connection, each new connection sees an empty database
- C) the application crashes at startup
- D) the test is skipped

<details><summary>Answer</summary>

**B.** An in-memory SQLite database lives while the connection is open. So the factory opens a connection once and passes it to `UseSqlite(_connection)`.
→ [Advanced theory](advanced.en.md#which-database-to-take)

</details>

## 7. 🟡 Select all correct statements about choosing a test database

- [ ] A) EF Core InMemory does not check constraints and does not execute SQL
- [ ] B) in-memory SQLite uses real SQL, but the dialect differs from the production DBMS
- [ ] C) Testcontainers starts a real DBMS in Docker
- [ ] D) the InMemory provider fully reproduces PostgreSQL behavior

<details><summary>Answer</summary>

**A, B, C.** D is wrong: InMemory is not a relational provider. It is better to check logic with constraints on SQLite or a container.
→ [Advanced theory](advanced.en.md#which-database-to-take)

</details>

## 8. 🟡 How do you override configuration in one test?

- A) change `appsettings.json`
- B) `factory.WithWebHostBuilder(b => b.ConfigureAppConfiguration(...))`
- C) set an environment variable by hand in CI
- D) you cannot

<details><summary>Answer</summary>

**B.** A derived factory with its own configuration is created; verified: the value `Feature:Flag = on`.
→ [Advanced theory](advanced.en.md#test-configuration)

</details>

## 9. 🟡 Open question: which scenarios do you check with integration tests and which with unit tests?

<details><summary>Key points of a good answer</summary>

- integration: the main scenarios (create-get-delete), error codes and formats at the boundary, dependency registration, configuration, database work, authorization;
- unit: logic, branching, boundary values, small variations;
- there are fewer integration tests: they are slower and costlier to maintain (the test pyramid);
- replace what is external (time, mail, payments), not the application's real infrastructure.

</details>

## 10. 🔴 You called `new ApiFactory()` and immediately checked the `ConfigureWebHost` call counter. What is it?

- A) 1
- B) 0
- C) 2
- D) depends on the number of tests

<details><summary>Answer</summary>

**B.** The host is built lazily — on the first `CreateClient()`, `Services` or `Server` (verified: `0` before, `1` after).
→ [Expert theory](expert.en.md#how-webapplicationfactory-is-built)

</details>

## 11. 🔴 What is an application's default environment in `WebApplicationFactory` and how do you change it?

- A) `Production`; it cannot be changed
- B) `Development`; `builder.UseEnvironment("Staging")` in `ConfigureWebHost`
- C) `Test`; through `appsettings.json`
- D) `Staging`; through the variable `ASPNETCORE_URLS`

<details><summary>Answer</summary>

**B.** Verified: `Development` by default, after `UseEnvironment("Staging")` — `Staging`. In `Development` the DI scope checks are on.
→ [Expert theory](expert.en.md#how-webapplicationfactory-is-built)

</details>
