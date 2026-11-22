---
title: "Exercises: containers and Docker"
lang: en
track: kubernetes
kind: module
doc: exercises
slug: 01-containers
part: 1
tags: [container, docker, dockerfile, multi-stage-build, pid1, dockerignore]
status: draft
updated: 2026-11-22
---

# Exercises: containers and Docker

🌐 [Русский](exercises.ru.md) · **English**

> Solve it yourself before opening the hints. Solutions are under spoilers. The exercises need Docker; the application from [basics](basics.en.md) (`main.go` and `go.mod`) lies in the `app/` folder.

## 1. 🟢 Two containers from one image

**Task:** build the multi-stage image of the application, start two containers with different greetings on ports 18080 and 18081 and make sure the hosts in the replies differ. Then stop and remove both.

**Expected result:**

```text
Hello from 74d74856967e
Privet from 53b97f96b876
```

(your host identifiers will differ)

<details><summary>Hint</summary>

The `GREETING` environment variable is set with the `-e` flag, the host port is the first number in `-p`.

</details>

<details><summary>Solution</summary>

```bash
docker build -t hello:multi .
docker run -d --name h1 -p 18080:8080 hello:multi
docker run -d --name h2 -p 18081:8080 -e GREETING=Privet hello:multi
curl -s localhost:18080
curl -s localhost:18081
docker stop h1 h2 && docker rm h1 h2
```

The containers use one image, but each has its own writable layer, its own variables and its own identifier (also the hostname).

</details>

## 2. 🟡 Cache and .dockerignore

**Task:** a folder contains `a.txt`, `.env` and `node_modules/big.bin` (5 MB). Write a Dockerfile that copies the folder into the image and prints its content, and make sure `.env` and `node_modules` do not enter the image and are not sent in the build context.

**Expected result:** `transferring context` shows about 100 bytes during the build, and the `ls` output has no `.env` and `node_modules`.

<details><summary>Hint</summary>

You need a `.dockerignore` file next to the Dockerfile. Compare the context size before and after: use `docker build --no-cache --progress=plain .`.

</details>

<details><summary>Solution</summary>

```dockerfile
FROM alpine:3.22
COPY . /ctx
RUN ls -A /ctx
```

```text
# .dockerignore
.env
node_modules
```

```text
#5 transferring context: 111B done
.dockerignore
Dockerfile
a.txt
```

Without `.dockerignore` the context was 5.00MB, and `.env` and `node_modules` ended up in the image.

</details>

## 3. 🟡 Fast stop

**Task:** a container with `CMD ["sleep", "300"]` stops in 10 seconds. Show two ways to stop it fast without changing `sleep`: starting with `--init` and a shell wrapper with `trap`. Measure the stop time.

**Expected result:** `docker stop` takes less than a second.

<details><summary>Hint</summary>

Someone inside the container must react to `SIGTERM`: either an init process or a shell with `trap`. For `trap` the shell must wait for `sleep` through `wait`, otherwise the signal is not handled until `sleep` finishes.

</details>

<details><summary>Solution</summary>

Way 1: `--init`.

```bash
docker run -d --init --name p3 t:exec    # t:exec is an image with CMD ["sleep", "300"]
time docker stop p3                      # real 0m0.336s
```

Way 2: a shell with `trap`.

```dockerfile
FROM alpine:3.22
CMD ["sh","-c","trap \"echo bye; exit 0\" TERM; sleep 300 & wait"]
```

```text
$ time docker stop tr
real    0m0.333s
$ docker logs tr
bye
```

In both cases `SIGTERM` is handled by a process that can intercept it, not by a PID 1 without a handler.

</details>

## 4. 🔴 An image without junk and without secrets

**Task:** there is a Dockerfile with `COPY big.bin /big.bin` (20 MB) followed by `RUN rm /big.bin`; the image weighs 32.8 MB. Make two fixed variants so the final image weighs the same as the base `alpine:3.22` (12.8 MB): deletion in the same layer and multi-stage. Then check that a build secret passed through `--secret` is not visible in `docker history`.

**Expected result:**

```text
g:base 12.8MB
g:one  12.8MB
g:ms   12.8MB
```

<details><summary>Hint</summary>

A file cannot be deleted from an already created layer. Either create and delete it in one `RUN`, or use it in an intermediate stage and copy only the result to the final one. For the secret: `RUN --mount=type=secret,id=…` and `docker build --secret id=…,env=…`.

</details>

<details><summary>Solution</summary>

Variant 1: create and delete in one `RUN`:

```dockerfile
FROM alpine:3.22
RUN head -c 20000000 /dev/zero > /big.bin && rm /big.bin
```

Variant 2: multi-stage:

```dockerfile
FROM alpine:3.22 AS build
COPY big.bin /big.bin
RUN echo result > /out.txt

FROM alpine:3.22
COPY --from=build /out.txt /out.txt
```

```text
$ docker images g --format '{{.Repository}}:{{.Tag}} {{.Size}}' | sort
g:base 12.8MB
g:ms 12.8MB
g:one 12.8MB
g:rm 32.8MB
```

The build secret:

```dockerfile
# syntax=docker/dockerfile:1
FROM alpine:3.22
RUN --mount=type=secret,id=token echo "token length: $(wc -c < /run/secrets/token)"
```

```text
$ TOKEN=abc docker build --secret id=token,env=TOKEN -t g:sec .
token length: 3
$ docker history g:sec --no-trunc | grep -c abc
0
$ docker run --rm g:sec ls /run/secrets
ls: /run/secrets: No such file or directory
```

The secret is available only during the `RUN` instruction and does not enter the history or the image.

</details>
