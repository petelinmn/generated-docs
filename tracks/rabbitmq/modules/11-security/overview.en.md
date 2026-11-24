---
title: "Security"
lang: en
track: rabbitmq
kind: module
doc: overview
slug: 11-security
part: 3
levels: [1, 2, 3, 4]
tags: [rabbitmq, security, vhost, permissions, tls, user-tags, access-control]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Security

🌐 [Русский](overview.ru.md) · **English**

## In short

Access to RabbitMQ is built from **vhosts** (isolation), **users** and **permissions** — three regular expressions (`configure`, `write`, `read`) per "user — vhost" pair. Tags determine access to management. Network traffic is protected by **TLS**: the broker listens on a separate port (5671) and the client verifies the certificate and host name.

## Key ideas

- **Permissions are regular expressions on the resource name;** `^$` allows nothing.
- **Refusal at connect:** a wrong password and an unknown user give the same `403 ACCESS_REFUSED`; no permissions on the vhost is `530 NOT_ALLOWED`.
- **A permission violation on a resource closes the channel** (`403`), not the connection.
- **Publishing to a queue via `""` needs `write` on `amq.default`.**
- **A refused publish throws no exception:** you see the channel close.
- **Tags:** `monitoring` reads the management API, no tags means 401.
- **Delete `guest`/`guest`;** in the teaching image it is reachable from outside.
- **TLS with verification:** an unknown CA and a wrong host name are rejected.

## Minimal example

```text
rabbitmqctl add_vhost shop
rabbitmqctl add_user alice pw1
rabbitmqctl set_permissions -p shop alice "^app\..*" "^app\..*" "^app\..*"
```

Everything was run against a real RabbitMQ 4.4.1 in Docker (see the [basic theory](basics.en.md)).

## When to use and when not

| Need | Solution |
|---|---|
| Isolate applications | a vhost per application |
| Limit what a service can do | minimal regular expressions |
| Protect traffic | TLS with certificate verification |
| Give monitoring access without data | the `monitoring` tag |

- ❌ Do not keep `guest` or hand out `.*`.
- ❌ Do not disable certificate verification.

## Material levels

| Level | What is inside |
|---|---|
| [1. Basic theory](basics.en.md) | vhosts, users, permissions, refusals, tags |
| [2. Advanced theory](advanced.en.md) | TLS: certificates, broker setup, client verification |
| [3. Expert theory](expert.en.md) | reference of commands and refusals, gotchas, interview questions |
| [4. Self-check](quiz.en.md) | quiz and [exercises](exercises.en.md) |
| 5. Runnable examples | not created yet — `/learn rabbitmq 11-security level 5` |
| 6. Tests | not created yet — `/learn rabbitmq 11-security level 6` |

## Related materials

- Prerequisites: [02 · Exchanges and routing](../02-exchanges-and-routing/overview.en.md), [09 · Monitoring and operations](../09-monitoring/overview.en.md)
- Next: [12 · Federation and Shovel](../12-federation-and-shovel/overview.en.md)
- Analogues in other tracks: [Linux](../../../linux/overview.en.md)
