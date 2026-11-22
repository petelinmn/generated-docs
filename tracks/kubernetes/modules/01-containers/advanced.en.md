---
title: "Containers and Docker: advanced"
lang: en
track: kubernetes
kind: module
doc: advanced
slug: 01-containers
part: 1
tags: [container, docker, dockerfile, multi-stage-build, dockerignore, non-root, entrypoint, pid1, sigterm, digest]
versions: "Docker Engine 29.6.1 (BuildKit), Go 1.24 for the examples"
status: draft
updated: 2026-11-22
---

# Containers and Docker: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

Builds on [basics](basics.en.md): the test application and the `docker build` and `docker run` commands are not repeated.

## Multi-stage builds

Building Go needs a compiler of hundreds of megabytes; running needs only the finished binary. Multi-stage separates these tasks: one Dockerfile has several `FROM`s, and the final image takes only the needed files from earlier stages through `COPY --from`.

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

`CGO_ENABLED=0` produces a statically linked binary: without it a Go program looks for a system C library, which `scratch` lacks. Comparison with a single-stage build of the same code:

```text
$ docker images hello --format 'table {{.Repository}}:{{.Tag}}\t{{.Size}}'
REPOSITORY:TAG   SIZE
hello:multi      12.8MB
hello:single     516MB
$ docker image inspect hello:multi  --format 'layers={{len .RootFS.Layers}}'
layers=1
$ docker image inspect hello:single --format 'layers={{len .RootFS.Layers}}'
layers=8
```

The image is 40 times smaller, with a single layer. A smaller size means faster download to a node and a smaller attack surface (no compiler, no shell). Other technologies work the same way: [Native AOT in C#](../../../csharp/modules/01-dotnet-platform/advanced.en.md) or a `venv` in Python copy only the build result into the final image.

## Layer cache and .dockerignore

The cache is invalidated from the first changed step, so the order is: first what changes rarely (the dependency manifest and installing them), then the source code. In the example `go.mod` is copied separately from `*.go`: editing sources does not force recomputing the steps before `COPY *.go`.

The second source of needless rebuilds is the build context: the whole content of the folder passed to `docker build`. `COPY . .` without `.dockerignore` pulls both secrets and heavy folders into the image. The experiment: a folder with `.env`, `a.txt` and a 5 MB `node_modules`.

```text
# without .dockerignore
#4 transferring context: 5.00MB 0.4s done
.env
Dockerfile
a.txt
node_modules

# with .dockerignore (containing: .env and node_modules)
#5 transferring context: 111B done
.dockerignore
Dockerfile
a.txt
```

The context shrank from 5 MB to 111 bytes, and `.env` did not reach the image. `.dockerignore` rules are similar to `.gitignore`.

## Do not run as root

By default the process in a container runs as root. If an attacker breaks out of the application, they have maximum rights inside the container. The `USER` instruction switches the user:

```dockerfile
FROM alpine:3.22
RUN adduser -D app
USER app
CMD ["id"]
```

```text
$ docker run --rm t:user
uid=1000(app) gid=1000(app) groups=1000(app)
$ docker run --rm alpine:3.22 id
uid=0(root) gid=0(root) groups=0(root),0(root),1(bin),2(daemon),3(sys),4(adm),6(disk),10(wheel),11(floppy),20(dialout),26(tape),27(video)
```

In Kubernetes the same is set in `securityContext` (module 17); running as root can be forbidden at the cluster level.

## ENTRYPOINT and CMD

- `ENTRYPOINT` is what to run, `CMD` is the default arguments. Arguments after the image name in `docker run` replace `CMD`; `--entrypoint` replaces `ENTRYPOINT`.
- If only `CMD` is set, `docker run image command` replaces the whole command.

```dockerfile
FROM alpine:3.22
ENTRYPOINT ["echo", "entry"]
CMD ["default-arg"]
```

```text
$ docker run --rm t:cmd
entry default-arg
$ docker run --rm t:cmd custom-arg
entry custom-arg
$ docker run --rm --entrypoint echo t:cmd replaced
replaced
```

In Kubernetes the mapping is: `command` in the manifest replaces `ENTRYPOINT`, `args` replaces `CMD`.

## PID 1 and SIGTERM

`docker stop` sends `SIGTERM` to the main process, waits 10 seconds and, if the process is still alive, sends `SIGKILL`. The main process of a container is PID 1 in its namespace, and the kernel does not apply default signal actions to PID 1: if a program has not installed a `SIGTERM` handler, it ignores the signal.

```text
$ docker run -d --name ps2 t:exec        # CMD ["sleep", "300"]
$ time docker stop ps2
real    0m10.392s

$ docker run -d --init --name p3 t:exec
$ docker exec p3 ps -o pid,args
PID   COMMAND
    1 /sbin/docker-init -- sleep 300
    7 sleep 300
$ time docker stop p3
real    0m0.336s
```

`sleep` does not handle `SIGTERM`, so the container waited the full 10 seconds until `SIGKILL`. The `--init` flag starts a tiny init process before the program; it forwards signals and reaps zombies. The Go application from our example stopped in 0.45 s: the Go runtime handles `SIGTERM` itself.

The command form matters too. The shell form (`CMD sleep 300`) runs as `/bin/sh -c "sleep 300"`; in Alpine the shell replaces itself with the last command, so PID 1 is still `sleep`. But with several commands the shell stays PID 1 and the signal does not reach the application:

```text
$ docker run -d --name p4 t:shell2     # CMD sleep 300; echo done
$ docker exec p4 ps -o pid,args
PID   COMMAND
    1 /bin/sh -c sleep 300; echo done
    7 sleep 300
```

The rule: use the exec form (`CMD ["program", "arg"]`) and make the application handle `SIGTERM`. Kubernetes stops the same way: `SIGTERM`, then waiting for `terminationGracePeriodSeconds` (30 s by default), then `SIGKILL`.

## Tags and digests

A tag is a movable reference: `app:1.0` can be overwritten. A digest is a hash of the manifest content and never changes. The digest is visible on `docker push` and in `RepoDigests`:

```text
$ docker image inspect alpine:3.22 --format '{{index .RepoDigests 0}}'
alpine@sha256:49340b346798875b5592e5d420241f2f9cfba5f47e62ebb1446a1b34734e898f
```

Pinning an image by digest (`alpine@sha256:…`) is the only way to guarantee that the same thing starts tomorrow. Practice: a human-readable tag for people (`1.4.2`), a digest for reproducible deployments.

## Choosing a base image

| Base | Size and contents | When |
|---|---|---|
| `scratch` | empty, only your binary | static binaries (Go, Rust, Native AOT) |
| `alpine` | small, uses musl instead of glibc, has `sh` and `apk` | when you need a shell and compactness; incompatibilities with glibc libraries are possible |
| `*-slim` (Debian) | glibc, a trimmed package set | interpreted languages (Python, Node) without compatibility surprises |
| distroless | only the language runtime, no shell or package manager | production where a minimal attack surface is needed |

The less in the image, the fewer vulnerabilities, but the harder the debugging: in `scratch` you cannot run `docker exec … sh`. Debugging such pods in Kubernetes uses ephemeral containers (module 21).

## Best practices

- Pin the base image version (`alpine:3.22`, not `alpine` or `latest`).
- One process per container, logs to stdout/stderr rather than to files.
- Configuration through environment variables, secrets not in the image.
- Multi-stage, `.dockerignore`, a non-root user.
- Combine related commands in one `RUN` so as not to multiply layers when intermediate files must be removed in the same layer.

## Common mistakes

- ❌ `RUN rm big.bin` in a separate layer → ✅ the file stays in the layer beneath. Experiment: a 20 MB `COPY big.bin` followed by `RUN rm` gives a 32.8 MB image against 12.8 MB for the base. Do not add the file at all, or delete it in the same `RUN` that created it.
- ❌ Passing a secret through `ARG` or `RUN echo … && rm` → ✅ the value stays in the layer history: `docker history --no-trunc` shows `RUN |1 TOKEN=abc …`. Use `RUN --mount=type=secret` for build secrets and a Kubernetes Secret for runtime secrets (module 07).
- ❌ Expecting `ARG` to be available at runtime → ✅ `ARG` lives only at build time (`TOKEN=[]` in the container in the experiment), `ENV` stays in the image (`MODE=[prod]`).
- ❌ Relying on a Dockerfile `HEALTHCHECK` in Kubernetes → ✅ Kubernetes ignores it: a pod with an image whose `HEALTHCHECK` fails stays `1/1 Running` while `docker inspect` shows `unhealthy`. Use probes.

## Comparison with other tracks

- [Linux](../../../linux/overview.en.md): a container is namespaces and cgroups; `PID 1` and signals are kernel behavior.
- [C#](../../../csharp/modules/01-dotnet-platform/advanced.en.md): Native AOT produces a static binary for `scratch` or a minimal image.
- From tests: [Testcontainers](../../../csharp/libraries/testcontainers/overview.en.md) starts the same images for the duration of a test.

## Related materials

- Previous level: [basics](basics.en.md)
- Next: [expert](expert.en.md) — image structure and gotchas
- Next module: [02 · Kubernetes architecture](../02-architecture/overview.en.md)

## Sources

- [Multi-stage builds](https://docs.docker.com/build/building/multi-stage/)
- [Build context and .dockerignore](https://docs.docker.com/build/concepts/context/#dockerignore-files)
- [Dockerfile: ENTRYPOINT and CMD](https://docs.docker.com/reference/dockerfile/#understand-how-cmd-and-entrypoint-interact)
- [docker stop](https://docs.docker.com/reference/cli/docker/container/stop/)
- [Build secrets](https://docs.docker.com/build/building/secrets/)
- [Kubernetes: Pod lifecycle, termination](https://kubernetes.io/docs/concepts/workloads/pods/pod-lifecycle/#pod-termination)
- [Distroless](https://github.com/GoogleContainerTools/distroless)
