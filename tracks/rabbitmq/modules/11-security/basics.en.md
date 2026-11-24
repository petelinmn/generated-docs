---
title: "Security: basics"
lang: en
track: rabbitmq
kind: module
doc: basics
slug: 11-security
part: 3
tags: [rabbitmq, security, vhost, users, permissions, access-refused, user-tags]
versions: "RabbitMQ 4.4.1; AMQP 0-9-1; Python pika 1.5.0"
status: draft
updated: 2026-11-24
---

# Security: basics

🌐 [Русский](basics.ru.md) · **English**

**Levels:** [overview](overview.en.md) · **1. Basics** · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

**Prerequisites:** [02 · Exchanges and routing](../02-exchanges-and-routing/basics.en.md), [09 · Monitoring](../09-monitoring/basics.en.md) · **Time:** ~1.5 h

## How the code was verified

One RabbitMQ 4.4.1 node in Docker (`rabbitmq:4-management`) and pika 1.5.0; outputs are copied from the runs. The helper `sh(*cmd)` runs a command inside the container (`docker exec rabbit …`), `conn(user, pw, vhost)` opens a connection with the given credentials. **All accounts are for teaching and are deleted after the experiment.** The passwords in the examples are placeholders.

## Three levels of access

| What | Purpose |
|---|---|
| **vhost** (virtual host) | an isolated namespace: its own exchanges, queues, bindings and permissions |
| **user** | an account with a password |
| **permissions** (`configure`/`write`/`read`) | three regular expressions per "user — vhost" pair |

A regular expression is matched against the **name** of a resource (an exchange or a queue):

| Permission | What it allows |
|---|---|
| `configure` | declare and delete resources |
| `write` | publish to an exchange (verified) |
| `read` | read from a queue (per the documentation; only `configure` and `write` were tested here) |

> ⚠️ Verify: which permissions bindings (`queue_bind`) and reading need was not verified here.

## Setup

```text
$ rabbitmqctl add_vhost shop
$ rabbitmqctl add_user alice pw1
$ rabbitmqctl add_user bob pw2
$ rabbitmqctl set_permissions -p shop alice "^app\..*" "^app\..*" "^app\..*"
$ rabbitmqctl set_permissions -p shop bob "^$" "^$" "^app\.read$"
$ rabbitmqctl set_user_tags alice monitoring
```

```text
$ rabbitmqctl list_permissions -p shop
user    configure   write       read
alice   ^app\..*    ^app\..*    ^app\..*
bob     ^$          ^$          ^app\.read$
```

`alice` works only with `app.*` resources, `bob` can only read the queue `app.read`; the empty expression `^$` allows nothing.

## What happens on a refusal

```python
conn("alice", "nope")              # wrong password
conn("carol", "x")                 # unknown user
conn("alice", "pw1", "/")          # no permissions on that vhost
```

```text
wrong password -> ProbableAuthenticationError ConnectionClosedByBroker: (403) 'ACCESS_REFUSED - Login was refused using authentication mechanism PLAIN. For details se…
unknown user -> ProbableAuthenticationError ConnectionClosedByBroker: (403) 'ACCESS_REFUSED - Login was refused using authentication mechanism PLAIN. For details se…
alice on vhost '/' (no permissions) -> ProbableAccessDeniedError ConnectionClosedByBroker: (530) "NOT_ALLOWED - access to vhost '/' refused for user 'alice'" …
```

- A wrong password and an unknown user give the **same** `403 ACCESS_REFUSED` error: the broker does not reveal whether a user exists.
- No permissions on a vhost is `530 NOT_ALLOWED` and the connection closes.

## Permissions on operations

```text
alice declares app.q -> ok
alice declares other.q (outside regex) -> ChannelClosedByBroker (403, "ACCESS_REFUSED - configure access to queue 'other.q' in vhost 'shop' refused for user 'alice'")
bob declares app.read (no configure) -> ChannelClosedByBroker (403, "ACCESS_REFUSED - configure access to queue 'app.read' in vhost 'shop' refused for user 'bob'")
bob publishes (no write) -> RuntimeError channel closed by broker
```

A permission violation closes the **channel** with `403 ACCESS_REFUSED`; the connection stays.

### Publishing to a queue through the default exchange

`basic_publish("", "app.read", …)` goes to the default exchange (`amq.default`) and needs the `write` permission on it. `alice`'s expression `^app\..*` does not cover it, so the channel closed with no error on the calling side:

```text
publish to default exchange: channel open = False
publish to app.ex: open = True depth = 1
after granting write on amq.default: open = True depth = 2
```

Publishing through `app.ex` (which matches the expression) worked; after the `amq.default` permission was granted, publishing straight to the queue worked too. **Publishing is asynchronous, so a refusal shows only as the channel closing** ([module 05](../05-reliability/advanced.en.md#errors-that-produce-no-confirm)).

## Tags and management

Tags (`monitoring`, `administrator`, …) determine access to the management plugin:

```text
mgmt alice 200
mgmt bob HTTP Error 401: Unauthorized
mgmt guest 200
```

A user tagged `monitoring` reads the API, a user with no tags is not admitted to the API (401), and `guest` has the tag `administrator`.

## The `guest` user

In the `rabbitmq:4-management` image the parameter `loopback_users` is empty (`{loopback_users,[]}`), so `guest`/`guest` is accepted from the host through the published port. **This is a teaching setup:** in a normal installation `guest` is allowed only from `localhost`, and in a production system it is removed or given another password.

> ⚠️ Verify: the default behaviour of `guest` outside the Docker image (the `localhost` restriction) comes from the documentation and was not verified here.

## Common beginner mistakes

- ❌ Leaving `guest`/`guest` → ✅ create your own users and delete `guest`.
- ❌ Granting `.*` everywhere → ✅ minimal regular expressions per application.
- ❌ One shared vhost for every application → ✅ vhosts for isolation.
- ❌ Forgetting the permission on `amq.default` when publishing straight to a queue → ✅ the `write` permission on it, or publish through your own exchange.
- ❌ Expecting an exception on a refused publish → ✅ watch for the channel closing.

## Related materials

- Next: [advanced theory](advanced.en.md) — TLS
- Before: [09 · Monitoring and operations](../09-monitoring/basics.en.md), [02 · Exchanges and routing](../02-exchanges-and-routing/basics.en.md)
- Next module: [12 · Federation and Shovel](../12-federation-and-shovel/overview.en.md)
- Analogue in another track: [Linux](../../../linux/overview.en.md)

## Sources

- [Authentication, Authorisation, Access Control](https://www.rabbitmq.com/docs/access-control)
- [Virtual Hosts](https://www.rabbitmq.com/docs/vhosts)
- [Production Checklist](https://www.rabbitmq.com/docs/production-checklist)
