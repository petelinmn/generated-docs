---
title: "Quiz: Federation and Shovel"
lang: en
track: rabbitmq
kind: module
doc: quiz
slug: 12-federation-and-shovel
part: 3
tags: [rabbitmq, shovel, federation, upstream, multi-datacenter]
status: draft
updated: 2026-11-24
---

# Quiz: Federation and Shovel

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. The label shows the question level: 🟢 [basic](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What are Shovel and Federation for?

- A) Linking independent brokers, for example in different data centres
- B) Joining nodes into a cluster
- C) Encrypting traffic
- D) Speeding up queues

<details><summary>Answer</summary>

**A.**
→ [Basics](basics.en.md#why-link-brokers)

</details>

## 2. 🟢 What happens to a message in the source after a shovel transfers it?

- A) It stays (a copy)
- B) It is removed (a move)
- C) It is marked as read
- D) It goes to a DLX

<details><summary>Answer</summary>

**B.** In the test `q.src` became empty.
→ [Basics](basics.en.md#shovel-queue-to-queue)

</details>

## 3. 🟢 How can you see a shovel's state?

- A) `rabbitmqctl shovel_status`
- B) `rabbitmqctl list_exchanges`
- C) `rabbitmq-upgrade drain`
- D) `rabbitmqctl cluster_status`

<details><summary>Answer</summary>

**A.**
→ [Advanced theory](advanced.en.md#managing-a-shovel)

</details>

## 4. 🟢 A cluster or separate brokers between data centres?

- A) A cluster
- B) Independent brokers linked by Shovel/Federation
- C) It does not matter
- D) Only one broker

<details><summary>Answer</summary>

**B.** A cluster is designed for a fast reliable network.
→ [Basics](basics.en.md#why-link-brokers)

</details>

## 5. 🟡 Which state and reason did the shovel show with the source stopped?

- A) `running`
- B) `terminated`, `failed to connect to source`
- C) `paused`
- D) `deleted`

<details><summary>Answer</summary>

**B.** After the source returned the shovel recovered.
→ [Advanced theory](advanced.en.md#a-source-failure)

</details>

## 6. 🟡 How do you enable federation for exchanges `fed.*`?

- A) A `federation-upstream` parameter and a policy with `federation-upstream-set` for the pattern `^fed\.`
- B) `rabbitmqctl join_cluster`
- C) `x-queue-type: federated`
- D) `basic_qos`

<details><summary>Answer</summary>

**A.**
→ [Basics](basics.en.md#federation-a-shared-exchange)

</details>

## 7. 🟡 What delivery guarantees does a shovel have?

- A) Exactly once
- B) At most once
- C) At least once (duplicates possible)
- D) None

<details><summary>Answer</summary>

**C.** (per the documentation; failures between delivery and acknowledgement were not reproduced here).
→ [Advanced theory](advanced.en.md#a-source-failure)

</details>

## 8. 🟡 Choose all true statements

- [ ] A) A shovel moves messages
- [ ] B) Federation is one-directional by default
- [ ] C) Shovel and Federation join brokers into a cluster
- [ ] D) A link should be monitored (`shovel_status`, `federation_status`)

<details><summary>Answer</summary>

**A, B, D.** C is wrong: the brokers stay independent.
→ [Gotchas](expert.en.md#gotchas)

</details>

## 9. 🟡 Which account should the link use?

- A) `guest` with the default password
- B) A separate user with minimal permissions and TLS
- C) An administrator
- D) Anonymous access

<details><summary>Answer</summary>

**B.**
→ [Gotcha 7](expert.en.md#7-guest-for-the-link)

</details>

## 10. 🔴 Find the mistake

> "We stretched a RabbitMQ cluster across two data centres because there is a VPN between them."

<details><summary>Answer</summary>

A cluster is designed for a fast reliable network; to link data centres use independent brokers and Shovel/Federation.
→ [Basics](basics.en.md#why-link-brokers)

</details>

## 11. 🔴 After the source restarted the shovel works again, but you did not notice the downtime. What should you have done?

- A) Nothing
- B) Monitor `shovel_status` and queue depth, alert on `terminated`
- C) Enable prefetch 1
- D) Disable the shovel

<details><summary>Answer</summary>

**B.**
→ [Gotcha 4](expert.en.md#4-the-terminated-state)

</details>

## 12. 🔴 Open question

Two data centres must exchange order events, and losing the link for an hour must not stop either. What do you choose?

<details><summary>Key points of a good answer</summary>

- Independent brokers in each data centre; an exchange Federation or a queue→queue Shovel.
- Queues accumulate messages during a break; after recovery the link continues.
- Idempotent consumers because of duplicates; TLS and a separate user.
- Monitoring of links and queue depth.
- Not verified: behaviour on a real WAN break, bidirectionality.

</details>

→ [Advanced theory](advanced.en.md#what-to-choose-across-data-centres)
