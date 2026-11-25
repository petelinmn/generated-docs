---
title: "Containers and Docker"
lang: en
track: kubernetes
kind: module
doc: overview
slug: 01-containers
part: 1
levels: [1, 2, 3, 4]
tags: [container, docker, image, dockerfile, multi-stage-build, registry, oci, layers]
versions: "Docker Engine 29.6.1 (BuildKit), Go 1.24 for the examples"
status: draft
updated: 2026-11-22
---

# Containers and Docker

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

A container is an ordinary Linux process that the kernel isolates from the others (namespaces) and limits in resources (cgroups). It is not a virtual machine: the kernel is shared with the host. An image is an immutable set of layers with the application files and launch metadata; you can start any number of containers from one image. Kubernetes runs images, so without understanding Dockerfiles, layers and registries you cannot get further.

## Key ideas

- **Image ≠ container.** An image is a read-only template, a container is its running instance with a thin writable layer. Everything the container wrote disappears with it unless moved to a volume.
- **An image consists of layers.** Each `RUN`, `COPY`, `ADD` instruction creates a layer. Layers are cached and reused, so the order of instructions in a Dockerfile affects build speed.
- **A layer cannot be "deleted".** `RUN rm` in the next layer only hides the file: an image with a 20 MB file deleted weighs 20 MB more than its base.
- **Multi-stage builds** separate build tools from the runtime: a Go application image shrank from 516 MB to 12.8 MB.
- **A tag is mutable, a digest is not.** `app:1.0` may point to one image today and another tomorrow; `app@sha256:…` always points to the same one.
- **A registry** stores images; an image name is `registry/repository:tag`. Kubernetes pulls images from a registry onto the nodes.
- **An application for a container** is one foreground process, logs to stdout/stderr, configuration through environment variables, and correct handling of `SIGTERM`.

## Minimal example

```dockerfile
# syntax=docker/dockerfile:1
FROM golang:1.24-alpine AS build
WORKDIR /src
COPY go.mod ./
COPY *.go ./
RUN CGO_ENABLED=0 go build -o /hello .

FROM scratch
COPY --from=build /hello /hello
EXPOSE 8080
ENTRYPOINT ["/hello"]
```

```text
$ docker build -t hello:multi .
$ docker run -d --name h1 -p 18080:8080 hello:multi
$ curl -s localhost:18080
Hello from 74d74856967e
```

In detail, with the application source code, see [basics](basics.en.md).

## When to use and when not

- ✅ Any server application that must start identically on a laptop, in CI and in a cluster.
- ✅ Dependency isolation: different runtime versions on one host.
- ❌ The application needs its own kernel or kernel drivers: a container uses the host kernel.
- ❌ Storing data "inside" a container: it is lost when the container is deleted.
- ❌ A container as a security boundary for untrusted code: isolation is weaker than with a virtual machine.

## Levels of this material

| Level | What's inside |
|---|---|
| [1. Basics](basics.en.md) | image and container, Dockerfile, build and run, ports and variables, layers and cache, volumes, registries |
| [2. Advanced](advanced.en.md) | multi-stage, `.dockerignore`, non-root user, `ENTRYPOINT` and `CMD`, PID 1 and `SIGTERM`, tags and digests, choosing a base image |
| [3. Expert](expert.en.md) | image structure (OCI), the dockerd → containerd → runc chain, Dockerfile and `docker run` reference, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | a quiz and [exercises](exercises.en.md) with solutions |
| 5. Runnable examples | not generated yet — `/learn kubernetes 01-containers level 5` |
| 6. Tests | not generated yet — `/learn kubernetes 01-containers level 6` |

## Related materials

- Prerequisites: the [Linux track](../../../linux/overview.en.md) — processes, namespaces and cgroups that containers are built on
- Next: [02 · Kubernetes architecture](../02-architecture/overview.en.md)
- The same idea in other tracks: [Testcontainers for C#](../../../csharp/libraries/testcontainers/overview.en.md) — running containers from tests
- [Linux 01 · Environment setup](../../../linux/modules/01-setup/overview.en.md)
