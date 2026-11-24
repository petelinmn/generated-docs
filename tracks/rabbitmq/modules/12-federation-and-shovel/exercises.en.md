---
title: "Exercises: Federation and Shovel"
lang: en
track: rabbitmq
kind: module
doc: exercises
slug: 12-federation-and-shovel
part: 3
tags: [rabbitmq, shovel, federation, upstream]
status: draft
updated: 2026-11-24
---

# Exercises: Federation and Shovel

🌐 [Русский](exercises.ru.md) · **English**

> Solve on your own; hints and solutions are under spoilers. You need two independent disposable brokers `rmqa` and `rmqb` on one Docker network with the plugins `rabbitmq_shovel` and `rabbitmq_federation` enabled (a teaching setup, the `guest` account). At the end remove the containers and the network: `docker rm -f rmqa rmqb; docker network rm rmqnet`.

## 1. 🟢 Start two brokers

**Task:** start two brokers on one network, enable the plugins and make sure they do **not** form a cluster.

**Expected output:** `rabbitmq-plugins enable …` answers `started … plugins`; each `cluster_status` shows only one node.

<details><summary>Hint</summary>

`docker network create rmqnet`, then two `docker run --network rmqnet`, plugins — `rabbitmq-plugins enable rabbitmq_shovel rabbitmq_shovel_management rabbitmq_federation rabbitmq_federation_management`.

</details>

<details><summary>Solution</summary>

See the [basics](basics.en.md#how-the-code-was-verified).

</details>

## 2. 🟢 A queue-to-queue shovel

**Task:** put five messages into `q.src` on `rmqa`, create `q.dst` on `rmqb` and create a shovel on `rmqb`. Print the queue depths and `shovel_status`.

**Expected output:** `q.src` — 0, `q.dst` — 5, state `running`.

<details><summary>Hint</summary>

A `shovel` parameter with the source and destination URIs; see the JSON in the basics.

</details>

<details><summary>Solution</summary>

See the [basics](basics.en.md#shovel-queue-to-queue).

</details>

## 3. 🟡 Order and continuity

**Task:** publish three more messages to `q.src` and read everything from `q.dst`. Check the order.

**Expected output:** `['m0', …, 'm7']` in the original order.

<details><summary>Hint</summary>

Read with `basic_get` until `None`.

</details>

<details><summary>Solution</summary>

See the [basics](basics.en.md#shovel-queue-to-queue).

</details>

## 4. 🟡 A source failure

**Task:** stop `rmqa`, look at `shovel_status`, start it, publish a message and make sure it reaches `q.dst`.

**Expected output:** `terminated` with the reason `failed to connect to source`, then the message in `q.dst`.

<details><summary>Hint</summary>

After starting the container wait for the broker to accept connections, and a few more seconds for the shovel to recover.

</details>

<details><summary>Solution</summary>

See the [advanced theory](advanced.en.md#a-source-failure).

</details>

## 5. 🔴 An exchange federation

**Task:** on `rmqb` set an upstream `up-a` pointing at `rmqa` and a policy for `^fed\.`, declare `fed.ex` on both brokers, bind a queue `fed.q` on `rmqb` and publish to `fed.ex` on `rmqa`. Check `federation_status`.

**Expected output:** one message in `fed.q` on `rmqb`; the link `running`.

<details><summary>Hint</summary>

Wait a few seconds after creating the policy: the link is established asynchronously.

</details>

<details><summary>Solution</summary>

See the [basics](basics.en.md#federation-a-shared-exchange).

</details>
