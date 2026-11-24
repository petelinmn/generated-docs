---
title: "Exercises: security"
lang: en
track: rabbitmq
kind: module
doc: exercises
slug: 11-security
part: 3
tags: [rabbitmq, security, permissions, tls, vhost]
status: draft
updated: 2026-11-24
---

# Exercises: security

🌐 [Русский](exercises.ru.md) · **English**

> Solve on your own; hints and solutions are under spoilers. Work on a disposable broker (`docker exec rabbit …`), the passwords are for teaching. Delete the users and vhosts after the exercises.

## 1. 🟢 A vhost, a user, permissions

**Task:** create a vhost `shop`, a user `alice` with a password and permissions `^app\..*` for `configure`, `write` and `read`. Print `list_permissions -p shop`.

**Expected output:** the row `alice  ^app\..*  ^app\..*  ^app\..*`.

<details><summary>Hint</summary>

`set_permissions -p shop alice <configure> <write> <read>`.

</details>

<details><summary>Solution</summary>

See the commands in the [basics](basics.en.md#setup).

</details>

## 2. 🟢 Refusals on connect

**Task:** connect (a) with a wrong password, (b) as an unknown user, (c) as a user with no permissions on the vhost `/`. Record each error code.

**Expected output:** (a) and (b) — `403 ACCESS_REFUSED`; (c) — `530 NOT_ALLOWED`.

<details><summary>Hint</summary>

`pika.PlainCredentials(user, password)` and `virtual_host=…`.

</details>

<details><summary>Solution</summary>

See the [basics](basics.en.md#what-happens-on-a-refusal).

</details>

## 3. 🟡 Permissions on resources

**Task:** as `alice` declare `app.q` and `other.q`. What happens in each case and what closes?

**Expected output:** `app.q` is created; `other.q` — `403 ACCESS_REFUSED`, the channel closed.

<details><summary>Hint</summary>

Open a new channel after the error.

</details>

<details><summary>Solution</summary>

See the [basics](basics.en.md#permissions-on-operations).

</details>

## 4. 🟡 The default exchange

**Task:** as `alice` publish to the queue `app.read` via `""`, then via an exchange `app.ex`, then grant `write` on `amq.default` and repeat. Print the channel states and the depth.

**Expected output:** first the channel is closed; via `app.ex` it works; after the grant, publishing to the queue works.

<details><summary>Hint</summary>

The regular expression for `write`: `^(app\..*|amq\.default)$`.

</details>

<details><summary>Solution</summary>

See the [basics](basics.en.md#publishing-to-a-queue-through-the-default-exchange).

</details>

## 5. 🔴 TLS

**Task:** issue a teaching CA and a server certificate for `localhost` (with the `keyUsage` and `subjectAltName` extensions), enable the 5671 listener and connect with a verifying client. Then check the refusals: an unknown CA, a wrong host, plain AMQP to the TLS port.

**Expected output:** a verifying connection opens; three refusals.

<details><summary>Hint</summary>

Put certificates in the container with `docker cp`; configuration in `conf.d/20-tls.conf`; in the client `ssl.create_default_context(cafile=…)`.

</details>

<details><summary>Solution</summary>

The commands and output are in the [advanced theory](advanced.en.md#teaching-certificates). The certificates are disposable; do not use them in a production system.

</details>
