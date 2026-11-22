---
title: "Containers and Docker: basics"
lang: en
track: kubernetes
kind: module
doc: basics
slug: 01-containers
part: 1
tags: [container, docker, image, dockerfile, registry, layers, volume]
versions: "Docker Engine 29.6.1 (BuildKit), Go 1.24 for the examples"
status: draft
updated: 2026-11-22
---

# Containers and Docker: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** the [Linux track](../../../linux/overview.en.md) (processes, file system) · **Time:** ~2 h

All commands were run on Docker Engine 29.6.1. On Windows, run `docker` commands in WSL2 or in PowerShell with Docker Desktop.

## Why it matters

An application depends on a runtime, libraries and environment settings. "It works on my machine" often means "I have a different version". A container packages the application together with its file system, so it starts the same way on a laptop, in CI and on a server. Unlike a virtual machine, a container does not carry its own kernel: it is a host process to which the kernel shows a separate file system, network and process list. That is why it starts in a fraction of a second and weighs megabytes.

## Image and container

- An **image** is an immutable template: the application files plus metadata (the launch command, environment variables, ports).
- A **container** is a running instance of an image. You can start many containers from one image.

A programming analogy: an image is a class, a container is an object.

## The test application

All examples in the module use a tiny HTTP server in Go. The language does not matter: all we need is a single executable file.

```go
package main

import (
	"fmt"
	"net/http"
	"os"
)

func main() {
	greeting := os.Getenv("GREETING")
	if greeting == "" {
		greeting = "Hello"
	}
	http.HandleFunc("/", func(w http.ResponseWriter, r *http.Request) {
		host, _ := os.Hostname()
		fmt.Fprintf(w, "%s from %s\n", greeting, host)
	})
	fmt.Println("listening on :8080")
	http.ListenAndServe(":8080", nil)
}
```

The server reads the greeting from the `GREETING` environment variable and replies with the host name. In a container the host name is the container identifier, so you can see which instance answered. Next to it lies a `go.mod` with `module hello` and `go 1.24`.

## Dockerfile

A Dockerfile is a text recipe for building an image. The simplest variant:

```dockerfile
FROM golang:1.24-alpine
WORKDIR /src
COPY . .
RUN go build -o /hello .
EXPOSE 8080
CMD ["/hello"]
```

| Instruction | What it does |
|---|---|
| `FROM` | the base image the build starts from |
| `WORKDIR` | the working folder for the following instructions |
| `COPY` | copies files from the build context into the image |
| `RUN` | runs a command at build time; the result becomes a layer |
| `EXPOSE` | documents a port; the port itself is not published |
| `CMD` | the default command when the container starts |

## Build and run

```bash
docker build -t hello:single -f Dockerfile.single .   # the simple Dockerfile above
docker build -t hello:multi .                         # the multi-stage Dockerfile (see the overview)
docker run -d --name h1 -p 18080:8080 hello:multi
```

`-t` sets the name and tag, `-f` the Dockerfile (`Dockerfile` by default), `-d` runs in the background, `--name` names the container, `-p host:container` publishes a port. The main management commands:

```bash
docker ps                 # running containers
docker logs h1            # the container's stdout and stderr
docker exec -it h1 sh     # a command inside a running container (the image needs sh)
docker stop h1            # stop (SIGTERM, then SIGKILL)
docker rm h1              # remove a stopped container
```

The `hello:multi` image is built on `scratch`: it has not even `sh`, so `docker exec h1 sh` fails (see the [gotchas](expert.en.md#gotcha-a-scratch-image-has-no-shell)). `hello:single` has a shell.

The result of requests to two containers from one image:

```text
$ curl -s localhost:18080
Hello from 74d74856967e
$ curl -s localhost:18081
Privet from 53b97f96b876
```

The second container was started with `-e GREETING=Privet`: an environment variable changes behavior without rebuilding the image. The hosts differ: these are two different containers.

Logs are read from stdout:

```text
$ docker logs h1
listening on :8080
```

A container is a process: while it is alive, the container is "running". When the main process ends, the container stops.

## Layers and cache

Each `RUN`, `COPY` or `ADD` instruction creates a layer: a set of file system changes. An image is a stack of layers. Docker (BuildKit) remembers the result of each step and skips unchanged ones on a rebuild. As soon as a step changes, all following ones run again.

The multi-stage Dockerfile from the example (`FROM … AS build`, then `FROM scratch`) builds like this. A rebuild with no changes:

```text
#9 [build 4/5] COPY *.go ./
#9 CACHED
#8 [build 5/5] RUN CGO_ENABLED=0 go build -o /hello .
#8 CACHED
```

After editing `main.go`:

```text
#8 [build 2/5] WORKDIR /src
#8 CACHED
#9 [build 3/5] COPY go.mod ./
#9 CACHED
#10 [build 4/5] COPY *.go ./
#11 [build 5/5] RUN CGO_ENABLED=0 go build -o /hello .
#12 [stage-1 1/1] COPY --from=build /hello /hello
```

Steps before `COPY *.go` come from the cache; starting from the changed file everything runs again. That is why rarely changing things (dependencies) are copied before frequently changing ones (sources). More in [advanced](advanced.en.md#layer-cache-and-dockerignore).

## Data: the writable layer and volumes

A container writes to a thin layer on top of the image. It is deleted together with the container:

```text
$ docker exec h3 sh -c 'echo hi > /tmp/x'
$ docker diff h3
C /tmp
A /tmp/x
```

`A` — a file was added, `C` — a folder changed. For data to outlive the container, use a **volume** — a folder managed by Docker outside the container layer:

```text
$ docker volume create v1
$ docker run --rm -v v1:/data alpine:3.22 sh -c 'echo persisted > /data/f'
$ docker run --rm -v v1:/data alpine:3.22 cat /data/f
persisted
```

The first container is deleted; the data was read by the second one. In Kubernetes the same role is played by volumes and PersistentVolumes.

## Registry and image name

A registry is an image storage service: Docker Hub, GitHub Container Registry, cloud registries. A full image name:

```text
localhost:15000 / team/hello : 1.0
 registry         repository  tag
```

If the registry is omitted, Docker Hub is implied; if the tag is omitted, it is `latest`. A local registry for experiments starts with one command (`docker run -d -p 15000:5000 registry:2`):

```text
$ docker tag hello:multi localhost:15000/team/hello:1.0
$ docker push localhost:15000/team/hello:1.0
1.0: digest: sha256:f3b18a09f5f30b7befd534594e3dc3e6a27b215e42c262233e7c9b115273a60e size: 855
$ curl -s localhost:15000/v2/_catalog
{"repositories":["team/hello"]}
$ curl -s localhost:15000/v2/team/hello/tags/list
{"name":"team/hello","tags":["1.0"]}
```

`docker pull` downloads an image from a registry. Kubernetes does the same on every node when it starts a pod.

## Beginner mistakes

- ❌ Writing data inside the container → ✅ move it to a volume (in Kubernetes, a PersistentVolume): the writable layer disappears with the container.
- ❌ Running two processes in one container "like in a VM" → ✅ one container, one main process; the others go to other containers.
- ❌ Expecting `EXPOSE 8080` to open the port → ✅ a port is published only with the `-p` flag (in Kubernetes, through a Service).
- ❌ Using the `latest` tag → ✅ pin the version: `latest` does not say which version is inside, and in Kubernetes it changes the pull policy (see the [gotchas](expert.en.md#gotcha-latest-in-kubernetes)).
- ❌ Copying everything into the image, including `.env` and `node_modules` → ✅ add a `.dockerignore` ([advanced](advanced.en.md#layer-cache-and-dockerignore)).

## Related materials

- Next: [advanced](advanced.en.md) — multi-stage, non-root, `PID 1`
- Next module: [02 · Kubernetes architecture](../02-architecture/overview.en.md)
- Foundation: the [Linux track](../../../linux/overview.en.md) — processes, namespaces and cgroups
- From C# tests: [Testcontainers](../../../csharp/libraries/testcontainers/overview.en.md)

## Sources

- [Dockerfile reference](https://docs.docker.com/reference/dockerfile/)
- [Docker build: cache](https://docs.docker.com/build/cache/)
- [docker run](https://docs.docker.com/reference/cli/docker/container/run/)
- [Volumes](https://docs.docker.com/engine/storage/volumes/)
- [Registry: deployment](https://distribution.github.io/distribution/about/deploying/)
