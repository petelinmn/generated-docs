---
title: "Quiz: security"
lang: en
track: rabbitmq
kind: module
doc: quiz
slug: 11-security
part: 3
tags: [rabbitmq, security, permissions, tls, vhost]
status: draft
updated: 2026-11-24
---

# Quiz: security

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. The label shows the question level: 🟢 [basic](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What do a user's permissions on a vhost consist of?

- A) Just a password
- B) Three regular expressions: `configure`, `write`, `read`
- C) A list of queues
- D) The `administrator` tag

<details><summary>Answer</summary>

**B.**
→ [Basics](basics.en.md#three-levels-of-access)

</details>

## 2. 🟢 What does a client with a wrong password get?

- A) `403 ACCESS_REFUSED`
- B) `404 NOT_FOUND`
- C) `530 NOT_ALLOWED`
- D) The connection opens but without permissions

<details><summary>Answer</summary>

**A.** An unknown user gets the same error.
→ [Basics](basics.en.md#what-happens-on-a-refusal)

</details>

## 3. 🟢 What closes when a `configure` permission is violated?

- A) The whole connection
- B) The channel
- C) Nothing
- D) The broker

<details><summary>Answer</summary>

**B.** `403 ACCESS_REFUSED`.
→ [Basics](basics.en.md#permissions-on-operations)

</details>

## 4. 🟢 What is a vhost for?

- A) For TLS
- B) For isolating resources and permissions
- C) For speeding up queues
- D) For storing passwords

<details><summary>Answer</summary>

**B.**
→ [Basics](basics.en.md#three-levels-of-access)

</details>

## 5. 🟡 A user has `write` only on `^app\..*`. What happens on `basic_publish("", "app.read", …)`?

- A) The message is published
- B) The channel closes: `write` on `amq.default` is needed
- C) An exception in the call
- D) The message goes to `app.ex`

<details><summary>Answer</summary>

**B.** The refusal shows only as the channel closing.
→ [Basics](basics.en.md#publishing-to-a-queue-through-the-default-exchange)

</details>

## 6. 🟡 Which port listens for AMQP over TLS in this module?

- A) 5672
- B) 5671
- C) 15672
- D) 15692

<details><summary>Answer</summary>

**B.** The `amqp/ssl` listener.
→ [Advanced theory](advanced.en.md#broker-configuration)

</details>

## 7. 🟡 A client verifies the broker's certificate but a wrong host is given (`wrong.example`). Result?

- A) The connection opens
- B) The connection is rejected: the name does not match `subjectAltName`
- C) The broker issues a new certificate
- D) The connection opens unencrypted

<details><summary>Answer</summary>

**B.**
→ [Advanced theory](advanced.en.md#the-client)

</details>

## 8. 🟡 Choose all true statements

- [ ] A) A user tagged `monitoring` can read `/api/overview`
- [ ] B) A user with no tags gets 401 from the management API
- [ ] C) Permissions are checked against the resource name with a regular expression
- [ ] D) TLS replaces the permission system

<details><summary>Answer</summary>

**A, B, C.** D is wrong: you need both.
→ [Basics](basics.en.md#tags-and-management)

</details>

## 9. 🟡 Why did Python reject the teaching CA on the first attempt?

- A) It expired
- B) The CA lacked the `keyUsage` extension
- C) The name was `localhost`
- D) The port was closed

<details><summary>Answer</summary>

**B.** `CA cert does not include key usage extension`.
→ [Gotcha 5](expert.en.md#5-python-rejects-a-ca-without-keyusage)

</details>

## 10. 🔴 Find the mistake

```python
ctx = ssl._create_unverified_context()
conn = pika.Connection(pika.ConnectionParameters("broker.example", 5671, ssl_options=pika.SSLOptions(ctx)))
```

<details><summary>Answer</summary>

Certificate verification is off: the connection is encrypted but you do not know who you are talking to, so impersonation is possible. Use `ssl.create_default_context(cafile=…)` and the host name.
→ [Gotcha 6](expert.en.md#6-tls-without-verification-is-no-protection)

</details>

## 11. 🔴 An application has permissions `.* .* .*` on a shared vhost. What is wrong?

- A) Nothing
- B) The application can create and delete any resource and read any queue in the vhost
- C) The application cannot publish
- D) The broker refuses the connection

<details><summary>Answer</summary>

**B.** Give minimal expressions and a separate vhost.
→ [Gotcha 1](expert.en.md#1-overly-broad-permissions)

</details>

## 12. 🔴 Open question

Design access for three services: an order publisher, an order handler and monitoring.

<details><summary>Key points of a good answer</summary>

- A separate vhost; three users instead of `guest`.
- Publisher: `write` on the orders exchange (and the needed name), no `read`; handler: `read` on its queue and minimal `configure`; monitoring: the `monitoring` tag, no data permissions.
- TLS with verification on all connections; secrets from a store.
- Remember `amq.default` when publishing straight to a queue.
- Not verified here: topic permissions, external account sources.

</details>

→ [Basics](basics.en.md)
