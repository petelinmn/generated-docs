---
title: "Quiz: containers and Docker"
lang: en
track: kubernetes
kind: module
doc: quiz
slug: 01-containers
part: 1
tags: [container, docker, image, dockerfile, multi-stage-build, pid1]
status: draft
updated: 2026-11-22
---

# Quiz: containers and Docker

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What happens to a file the container wrote to `/tmp` after `docker rm`?

- A) The file is saved in the image
- B) The file disappears with the container
- C) The file is saved in an anonymous volume
- D) The file is copied to the host

<details><summary>Answer</summary>

**B.** The container writes to a thin writable layer on top of the image, and it is deleted with the container. The image is immutable (A is wrong). A volume appears only if one was declared (C), nothing is copied to the host (D). For data to outlive the container, use a volume.
→ [Basics](basics.en.md#data-the-writable-layer-and-volumes)

</details>

## 2. 🟢 What is true about an image and a container?

- A) An image is a running instance, a container is a template
- B) You can start many containers from one image
- C) A container contains its own Linux kernel
- D) An image can be changed while a container from it is running

<details><summary>Answer</summary>

**B.** An image is an immutable template, a container is its instance (A is swapped). A container shares the host kernel (C is wrong). An image is immutable (D is wrong).
→ [Basics](basics.en.md#image-and-container)

</details>

## 3. 🟢 Will `EXPOSE 8080` in a Dockerfile open the port on the host with a bare `docker run`?

- A) Yes, on port 8080
- B) Yes, on a random port
- C) No, the port is not published
- D) Yes, but only for localhost

<details><summary>Answer</summary>

**C.** `EXPOSE` only documents the port. In the experiment `docker port` for a container without `-p` printed an empty result. A port is published with `-p host:container`; `-P` publishes all `EXPOSE` ports to random host ports.
→ [Expert](expert.en.md#gotcha-expose-does-not-publish-a-port)

</details>

## 4. 🟢 How should `COPY` be ordered in a Dockerfile for fast rebuilds?

- A) First `COPY . .`, then install dependencies
- B) First the dependency manifest and installing them, then the sources
- C) Order does not affect speed
- D) First the most frequently changing files

<details><summary>Answer</summary>

**B.** The cache is invalidated from the first changed step. Rarely changing things go earlier. In the experiment after editing `main.go` the `WORKDIR` and `COPY go.mod` steps came from the cache, only `COPY *.go` and the compilation were rebuilt. Options A and D do the opposite, C is wrong.
→ [Basics](basics.en.md#layers-and-cache)

</details>

## 5. 🟡 What do these commands print?

```dockerfile
FROM alpine:3.22
ENTRYPOINT ["echo", "entry"]
CMD ["default-arg"]
```

```bash
docker run --rm t:cmd
docker run --rm t:cmd custom-arg
```

- A) `entry default-arg` and `entry custom-arg`
- B) `default-arg` and `custom-arg`
- C) `entry default-arg` and `custom-arg`
- D) `entry` and `entry`

<details><summary>Answer</summary>

```text
entry default-arg
entry custom-arg
```

**A.** `ENTRYPOINT` stays, arguments after the image name replace `CMD`. Option C would be right for an image with only `CMD`: then the whole command is replaced.
→ [Advanced](advanced.en.md#entrypoint-and-cmd)

</details>

## 6. 🟡 Why did `docker stop` of a container with `CMD ["sleep", "300"]` take about 10 seconds, but a third of a second with the `--init` flag?

- A) `sleep` takes long to release resources
- B) PID 1 gets no default signal actions, `sleep` ignores `SIGTERM`, and after 10 s `SIGKILL` arrives; `--init` forwards the signal
- C) The `--init` flag speeds up the network
- D) Docker always waits 10 seconds

<details><summary>Answer</summary>

**B.** In the experiment 10.392 s against 0.336 s. The Go application from the example stopped in 0.45 s without `--init`: its runtime handles `SIGTERM`. Option D is wrong.
→ [Advanced](advanced.en.md#pid-1-and-sigterm)

</details>

## 7. 🟡 Select all correct statements about layers

- [ ] A) `RUN rm file` in a separate layer shrinks the image
- [ ] B) Multi-stage lets you leave the compiler out of the final image
- [ ] C) An `ARG` value does not appear in `docker history`
- [ ] D) `.dockerignore` reduces the build context

<details><summary>Answer</summary>

**B, D.** A is wrong: the layer with the file stays (32.8 MB against 12.8 MB). C is wrong: the history shows `RUN |1 TOKEN=abc …`. B is right: 516 MB → 12.8 MB. D is right: context 5 MB → 111 bytes.
→ [Advanced](advanced.en.md)

</details>

## 8. 🟡 What does `docker exec h1 sh -c ls` print if `h1` runs from a `scratch` image?

- A) A file list with `hello`
- B) Empty output
- C) An error: `exec: "sh": executable file not found in $PATH`
- D) The container restarts

<details><summary>Answer</summary>

**C.** The image has one file, `/hello`, and no shell. For debugging an ephemeral container is attached (module 21).
→ [Expert](expert.en.md#gotcha-a-scratch-image-has-no-shell)

</details>

## 9. 🔴 Find the bug

```dockerfile
FROM alpine:3.22
ARG TOKEN
RUN wget --header "Authorization: $TOKEN" https://example.com/pkg.tgz
```

The image is built with `docker build --build-arg TOKEN=abc .` and published. What is the problem?

<details><summary>Answer</summary>

The value `abc` is recorded in the layer history (`RUN |1 TOKEN=abc …`) and available to anyone who downloads the image: `docker history --no-trunc`. The fix: pass the secret as a build secret.

```dockerfile
RUN --mount=type=secret,id=token \
    wget --header "Authorization: $(cat /run/secrets/token)" https://example.com/pkg.tgz
```

```bash
docker build --secret id=token,env=TOKEN .
```

The secret is mounted only for the duration of the instruction and does not enter the layer.
→ [Expert](expert.en.md#gotcha-arg-is-visible-in-image-history)

</details>

## 10. 🔴 Open question

The `app:latest` image is loaded onto a kind node with `kind load docker-image`, but a pod with `image: app:latest` stays in `ErrImagePull`. Explain the cause and suggest two ways to fix it.

<details><summary>Key points of a good answer</summary>

- For the `latest` tag (or no tag) `imagePullPolicy` defaults to `Always`: the kubelet goes to the registry, and the image is not there.
- Fix 1: an explicit tag (`app:1.0`) and `imagePullPolicy: IfNotPresent`, then `kind load docker-image app:1.0`.
- Fix 2: publish the image to a registry reachable from the cluster nodes, and then `Always` works normally.
- A side conclusion: `latest` does not say which version is running; it is bad practice outside kind too.

</details>

→ [Expert](expert.en.md#gotcha-latest-in-kubernetes)
