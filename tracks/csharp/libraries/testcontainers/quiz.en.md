---
title: "Quiz: Testcontainers"
lang: en
track: csharp
kind: library
doc: quiz
slug: testcontainers
part: 2
tags: [testcontainers, docker, integration-testing]
status: draft
updated: 2026-11-22
---

# Quiz: Testcontainers

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md). The answers are derived from a material prepared without Docker: only points marked as verified in the text count as verified.

## 1. 🟢 What is Testcontainers for?

- A) starting production containers
- B) starting throw-away Docker containers with real dependencies from a test
- C) replacing Docker
- D) building images

<details><summary>Answer</summary>

**B.** Databases, Redis, brokers — real, removed after the test.
→ [Basics](basics.en.md#what-it-is-and-why)

</details>

## 2. 🟢 Where do you get the connection string?

- A) hard-code `localhost:5432`
- B) `container.GetConnectionString()` — the host port is random
- C) from `appsettings.json`
- D) from the `PATH` variable

<details><summary>Answer</summary>

**B.** The host port is assigned at start.
→ [Basics](basics.en.md#the-postgresql-module)

</details>

## 3. 🟢 What happens on `Build()` without Docker?

- A) an empty container is returned
- B) `DockerUnavailableException`
- C) the test is skipped automatically
- D) Podman starts

<details><summary>Answer</summary>

**B.** Verified by running.
→ [Basics](basics.en.md#what-happens-without-docker)

</details>

## 4. 🟢 Why shouldn't you use the `latest` tag?

- A) it does not exist
- B) it breaks reproducibility: the image version changes
- C) it is slower
- D) not allowed in CI

<details><summary>Answer</summary>

**B.** Pin the version (`postgres:16-alpine`).
→ [Basics](basics.en.md#beginner-mistakes)

</details>

## 5. 🟡 How do you avoid starting a container per test?

- A) call `GC.Collect`
- B) a class/set fixture (`IAsyncLifetime`) and cleaning data between tests
- C) use `static` without disposal
- D) you cannot

<details><summary>Answer</summary>

**B.** A start takes seconds; one container per set.
→ [Advanced](advanced.en.md#container-lifetime)

</details>

## 6. 🟡 What do you do in CI/locally without Docker?

- A) ignore the failures
- B) conditionally skip integration tests
- C) delete the tests
- D) use `latest`

<details><summary>Answer</summary>

**B.** Under the repository's rules integration tests are marked explicitly and skipped when the environment is unavailable.
→ [Advanced](advanced.en.md#skipping-tests-without-docker)

</details>

## 7. 🟡 Select all correct statements

- [ ] A) the wait strategy determines the container's readiness
- [ ] B) host ports are better hard-coded
- [ ] C) data between tests must be isolated
- [ ] D) `latest` is recommended for reproducibility

<details><summary>Answer</summary>

**A, C.** B and D are wrong.
→ [Advanced](advanced.en.md#best-practices)

</details>

## 8. 🟡 Open question: how do you organize integration tests of an API with PostgreSQL?

<details><summary>Key points of a good answer</summary>

- a fixture with a PostgreSQL container per test set, a pinned image version;
- `WebApplicationFactory` with the connection string replaced;
- migrations at startup, cleaning data between tests;
- a conditional skip without Docker, image caching in CI.

</details>

## 9. 🔴 What is the "watchdog" container (Ryuk) for?

- A) for logging
- B) to remove leftover containers if the test process terminated abnormally
- C) for the network
- D) for the image cache

<details><summary>Answer</summary>

**B.** Per the Testcontainers documentation (not reproduced in this environment).
→ [Expert](expert.en.md#internals)

</details>

## 10. 🔴 The container is started but the first connection fails. The cause?

- A) a wrong image
- B) the process inside is not yet ready to accept connections — a wait strategy is needed
- C) no `Dispose`
- D) too few threads

<details><summary>Answer</summary>

**B.** "Started" ≠ "ready".
→ [Expert](expert.en.md#edges-and-gotchas)

</details>
