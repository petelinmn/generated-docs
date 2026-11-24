---
title: "Security: expert"
lang: en
track: rabbitmq
kind: module
doc: expert
slug: 11-security
part: 3
tags: [rabbitmq, security, permissions, tls, access-control, user-tags, vhost]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Security: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

Builds on the [basic](basics.en.md) and [advanced](advanced.en.md) theory. "Verified" means observed on a single RabbitMQ 4.4.1 node (Docker).

## Reference

### Access-control commands

| Command | Purpose | Verified |
|---|---|---|
| `rabbitmqctl add_vhost` / `delete_vhost` | create/delete a vhost | yes |
| `rabbitmqctl add_user` / `delete_user` | users | yes |
| `rabbitmqctl change_password` | change a password | yes |
| `rabbitmqctl set_permissions -p <vhost> <user> <configure> <write> <read>` | permissions | yes |
| `rabbitmqctl list_permissions -p <vhost>` / `list_users` | inspection | yes |
| `rabbitmqctl set_user_tags` | user tags | yes |
| `rabbitmqctl set_topic_permissions`, `clear_permissions`, `authenticate_user` | topic permissions and clearing | not tested |

### Results of refusals (verified)

| Situation | Code | What closes |
|---|---|---|
| wrong password / unknown user | `403 ACCESS_REFUSED` | connection |
| no permissions on the vhost | `530 NOT_ALLOWED` | connection |
| no `configure` on a resource | `403 ACCESS_REFUSED` | channel |
| no `write` on an exchange | channel closed | channel |
| plain AMQP to the TLS port | `IncompatibleProtocolError` | connection |

### Tags and management access

| User tag | Access to `/api/overview` (verified) |
|---|---|
| `monitoring` | 200 |
| no tags | 401 |
| `administrator` (`guest`) | 200 |

### Defaults read from the broker

| Parameter | Value |
|---|---|
| `loopback_users` | `[]` in the image (by default in the broker itself `guest` is limited to `localhost`) |
| `max_message_size` | 16777216 (16 MiB) |

> ⚠️ Verify: the default `guest` restriction and the effect of `max_message_size` (a refusal when publishing large messages) were not verified here.

## Internals

- **Permissions are checked against a resource name** with a regular expression; the empty expression `^$` allows nothing.
- **The default exchange is named `amq.default`:** publishing to a queue through it requires `write` on that name (verified).
- **A refused publish is not returned to the caller:** the channel closes asynchronously (verified).
- **An identical answer to a wrong password and an unknown user** does not reveal which accounts exist.
- **A TLS listener is a separate port** (5671), the AMQP port remains unless disabled (verified by the listener list).

> ⚠️ Verify: the internals of authentication (external sources: LDAP, OAuth 2), topic permissions, the performance impact of regular expressions and the behaviour when permissions are revoked from an already connected user were not verified here.

## Gotchas

### 1. Overly broad permissions

Permissions `.* .* .*` hand the user the whole vhost. Grant the minimum.

### 2. Publishing to a queue needs a permission on `amq.default`

Without it the channel closes silently (verified).

### 3. A refused publish throws no exception

Asynchronous publishing: you only see the channel close.

### 4. `guest` in the image is open from outside

`loopback_users` is empty in the teaching image: such a user in production is a hole.

### 5. Python rejects a CA without `keyUsage`

Strict certificate verification in Python: `CA cert does not include key usage extension` (verified).

### 6. TLS without verification is no protection

If you disable certificate verification you get encryption without knowing who you are talking to.

### 7. Empty permissions allow nothing

The empty expression `^$` matches no name, so it allows nothing (verified for `bob`'s `configure` and `write`). Whether at least one non-empty permission is needed to connect to a vhost was not verified here.

## Trade-offs and alternatives

| Choice | Pros | Cons |
|---|---|---|
| One vhost per application | isolation and simple permissions | more vhosts |
| A shared vhost | simpler | harder permissions, risk of name clashes |
| Passwords in the broker | simple | rotation needed |
| An external source (LDAP/OAuth) | one account system | separate setup; not verified here |
| TLS on the broker | encryption, server verification | certificate management |
| TLS on a proxy/load balancer | convenient | an unencrypted stretch inside |

## Interview questions

<details><summary>1. What do access permissions in RabbitMQ consist of?</summary>

Three regular expressions (`configure`, `write`, `read`) per "user — vhost" pair, matched against the resource name.

→ [Basics](basics.en.md#three-levels-of-access)

</details>

<details><summary>2. Trick: does the broker answer a wrong password and an unknown user differently?</summary>

No: identically (`403 ACCESS_REFUSED`), so as not to reveal a user's existence.

→ [Basics](basics.en.md#what-happens-on-a-refusal)

</details>

<details><summary>3. What happens on a `configure` violation?</summary>

The channel closes with `403 ACCESS_REFUSED`; the connection stays.

→ [Basics](basics.en.md#permissions-on-operations)

</details>

<details><summary>4. Trick: a user with `write` on `^app\..*` publishes to a queue via `""`. Will it work?</summary>

No: `write` on `amq.default` is needed; the channel closes.

→ [Gotcha 2](#2-publishing-to-a-queue-needs-a-permission-on-amqdefault)

</details>

<details><summary>5. How do you learn about a refused publish?</summary>

By the channel closing (asynchronously); there is no exception in the call.

→ [Gotcha 3](#3-a-refused-publish-throws-no-exception)

</details>

<details><summary>6. Why a vhost?</summary>

An isolated namespace with its own resources and permissions.

→ [Basics](basics.en.md#three-levels-of-access)

</details>

<details><summary>7. What do user tags do?</summary>

They determine access to the management plugin: `monitoring` reads the API, no tags means refusal (401).

→ [Basics](basics.en.md#tags-and-management)

</details>

<details><summary>8. Trick: is the `guest` user safe?</summary>

No: delete it or change the password; in the teaching image it is reachable from outside.

→ [Basics](basics.en.md#the-guest-user)

</details>

<details><summary>9. How do you enable TLS on the broker?</summary>

A listener `listeners.ssl.default = 5671` and `ssl_options.*` (CA, certificate, key); the client verifies the certificate and host name.

→ [Advanced theory](advanced.en.md#broker-configuration)

</details>

<details><summary>10. Trick: a client connected over TLS without verifying the certificate. Is the connection protected?</summary>

Encrypted, but server impersonation is possible: verification is mandatory.

→ [Gotcha 6](#6-tls-without-verification-is-no-protection)

</details>

<details><summary>11. Why did Python reject the teaching CA?</summary>

It lacked the `keyUsage` extension; strict verification requires it.

→ [Gotcha 5](#5-python-rejects-a-ca-without-keyusage)

</details>

<details><summary>12. What to do after changing a user's password?</summary>

The old password stops being accepted for new connections; open connections were not verified here.

→ [Advanced theory](advanced.en.md#changing-a-password)

</details>

## Cheat sheet

```text
vhost + user + permissions (configure/write/read regexes on resource names)
403 ACCESS_REFUSED : bad login or no configure/write (channel) ; 530 NOT_ALLOWED : no vhost access (connection)
publish to a queue via "" needs write on amq.default
TLS: listeners.ssl.default=5671 + ssl_options.{cacertfile,certfile,keyfile}; client verifies CA and host name
```

- Minimal permissions, your own vhost, your own accounts.
- TLS with verification; secrets stay out of the repository.

## Related materials

- Previous level: [advanced theory](advanced.en.md)
- Before: [10 · Flow control and performance](../10-performance/expert.en.md)
- Next module: [12 · Federation and Shovel](../12-federation-and-shovel/overview.en.md)

## Sources

- [Authentication, Authorisation, Access Control](https://www.rabbitmq.com/docs/access-control)
- [TLS Support](https://www.rabbitmq.com/docs/ssl)
- [Virtual Hosts](https://www.rabbitmq.com/docs/vhosts)
