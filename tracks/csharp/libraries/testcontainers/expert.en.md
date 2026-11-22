---
title: "Testcontainers: expert"
lang: en
track: csharp
kind: library
doc: expert
slug: testcontainers
part: 2
tags: [testcontainers, docker-api, ryuk, ci, interview-questions]
versions: ".NET 11, C# 15; Testcontainers 4.17.0"
status: draft
updated: 2026-11-22
---

# Testcontainers: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

**Prerequisites:** [basics](basics.en.md) and [advanced](advanced.en.md). **Time:** ~30 min.

> ⚠️ **Verification status.** Docker is unavailable in the preparation environment. Verified by running only: installing the packages (4.17.0), compiling the typed builders and `DockerUnavailableException` on `Build()` without Docker. The rest is per the Testcontainers documentation; the material stays `draft`.

## Internals

Testcontainers for .NET is a client of the **Docker Engine API**: builders accumulate configuration (the image, ports, environment variables, commands, the wait strategy), `Build()` creates the container object, `StartAsync()` pulls the image (if needed), creates and starts the container and waits for readiness by the strategy. Typed modules (`Testcontainers.PostgreSql`, `.Redis`…) are layers over the generic `ContainerBuilder` with a ready image, ports, waiting and a `GetConnectionString()` method.

Verified: `Build()` already contacts Docker (it throws `DockerUnavailableException` after trying to connect to `npipe://./pipe/docker_engine`) — that is, determining the Docker endpoint happens at container build time. The endpoint can be configured by environment variables or the `~/.testcontainers.properties` file (the exception message points straight to this).

For cleanup after crashes Testcontainers usually uses a service "watchdog" container (Ryuk) that removes the session's containers when the test process disappears.

> ⚠️ Verify: the role and settings of Ryuk (disabling, privileges in CI) come from the documentation; they were not reproduced.

## Edges and gotchas

**1. No Docker — a failure on `Build()`.** Verified: `DockerUnavailableException`.

**2. Host ports are random.** Do not hard-code ports; use `GetConnectionString()`/the mapped port.

**3. "Ready" does not mean "accepts connections".** You need the right wait strategy: a port, a log line, a `pg_isready` command; modules have it picked already.

**4. Image versions and architecture.** On ARM machines you need an ARM image (or emulation); the `latest` tag breaks reproducibility.

**5. Docker-in-Docker and rootless.** In CI access to the Docker socket/privileges for Ryuk may be restricted — setup is needed.

**6. Speed.** The first run pulls the image; cache images in CI.

**7. A shared DB across tests.** Parallel tests on one DB interfere — isolate by schema/database per test or run sequentially.

**8. Secrets and data.** Containers are throw-away, but do not put real data/secrets into test images and logs.

**9. Windows containers.** Linux images on Windows work via WSL2/Hyper-V; `Wait.ForUnixContainer()` is for Linux containers.

> ⚠️ Verify: items 4–9 come from the documentation and practice, they were not reproduced on this machine.

## Trade-offs and alternatives

| Approach | Pros | Cons |
|---|---|---|
| Testcontainers | real dependencies, isolation, configuration as code | needs Docker, slower |
| An in-memory provider (EF InMemory) | fast | does not behave like a real DB |
| SQLite in-memory | fast, SQL | a different dialect, not all features |
| A shared test DB | simple | state, races, drift |
| docker-compose in CI | familiar | state outside the test, manual coordination |
| Cloud ephemeral environments | realistic | cost, complexity |

## Interview questions

<details><summary>1. What is Testcontainers?</summary>

A library that starts throw-away Docker containers with real dependencies from test code and removes them afterwards.

</details>

<details><summary>2. How is it better than an in-memory DB?</summary>

A real DB is tested: dialect, transactions, migrations, indexes — behavior that in-memory stand-ins do not reproduce.

</details>

<details><summary>3. How do you learn the connection string?</summary>

`container.GetConnectionString()` — the host and the host port are determined at runtime (the port is random).

</details>

<details><summary>4. What is a wait strategy?</summary>

The condition by which `StartAsync` considers the container ready (the port is open, HTTP responds, a line in the log).

</details>

<details><summary>5. What happens without Docker?</summary>

Verified: `DockerUnavailableException` on `Build()`; tests must be skipped or run in an environment with Docker.

</details>

<details><summary>6. How do you avoid starting a container per test?</summary>

A class/set fixture (`IAsyncLifetime` + `IClassFixture`/`ICollectionFixture`) and cleaning data between tests.

</details>

<details><summary>7. How do you isolate data between tests?</summary>

A transaction rollback, cleaning tables (Respawn), a separate schema/database per test.

</details>

<details><summary>8. What is Ryuk for?</summary>

A service container that removes leftover containers if the test process terminated abnormally.

</details>

<details><summary>9. Trick question: a test passes locally and fails in CI with a timeout when starting the container. The causes?</summary>

No Docker/no socket access, a slow image pull, a wrong endpoint, the image architecture.

</details>

<details><summary>10. Trick question: the container is "started" but the first connection fails. Why?</summary>

The process inside is not ready yet; you need a proper wait strategy (ready modules have one).

</details>

<details><summary>11. When is Testcontainers not suitable?</summary>

When you need a very fast unit test (substitutes are enough there), in environments without Docker or with strict limits on running containers.

</details>

## Related materials

- Back: [advanced](advanced.en.md). Check yourself: [quiz](quiz.en.md) and [exercises](exercises.en.md).
- [17 · Integration testing](../../modules/17-testing-integration/overview.en.md)
- [21 · Application architecture](../../modules/21-architecture/overview.en.md) — test boundaries.
- [Kubernetes](../../../kubernetes/overview.en.md) — containers and kind as a test environment.

## Sources

- [Testcontainers for .NET: documentation](https://dotnet.testcontainers.org/)
- [Testcontainers for .NET: Custom configuration](https://dotnet.testcontainers.org/custom_configuration/)
- [Testcontainers for .NET: Wait strategies](https://dotnet.testcontainers.org/api/wait_strategies/)
