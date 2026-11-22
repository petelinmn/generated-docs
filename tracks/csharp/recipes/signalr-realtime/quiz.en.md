---
title: "Quiz: Real-time with SignalR"
lang: en
track: csharp
kind: recipe
doc: quiz
slug: signalr-realtime
part: 2
tags: [signalr, real-time, hub, backplane]
status: draft
updated: 2026-11-22
---

# Quiz: Real-time with SignalR

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark shows which level a question comes from: 🟢 [minimal](basics.en.md) · 🟡 [production](advanced.en.md) · 🔴 [scale and edges](expert.en.md).

## 1. 🟢 What is a hub in SignalR?

- A) an HTTP controller
- B) a class whose methods clients call and through which the server calls client methods
- C) a message queue
- D) a database

<details><summary>Answer</summary>

**B.** `Hub` is the point of two-way communication: `Clients.All/Group(...)` call clients, clients call the hub's public methods.
→ [Minimal](basics.en.md#a-hub)

</details>

## 2. 🟢 Who received the message when `ann` called `Send` through `Clients.All`?

- A) only `ann`
- B) only `bob`
- C) all connected clients (`ann`, `bob`, `cid`)
- D) nobody

<details><summary>Answer</summary>

**C.** Verified: all three.
→ [Minimal](basics.en.md#a-client-net)

</details>

## 3. 🟢 How do you send a message to clients from an endpoint, not from a hub?

- A) create `new ChatHub()`
- B) inject `IHubContext<ChatHub>`
- C) through a static field
- D) you cannot

<details><summary>Answer</summary>

**B.** Verified: a POST endpoint through `IHubContext` delivered a notification to `bob`.
→ [Minimal](basics.en.md#sending-from-outside-a-hub)

</details>

## 4. 🟢 What is true about a hub instance?

- A) one for the whole application
- B) created per method call
- C) one per user
- D) one per group

<details><summary>Answer</summary>

**B.** So hub fields do not keep state; use services and the DB.
→ [Minimal](basics.en.md#beginner-mistakes)

</details>

## 5. 🟡 What does the client see if a hub method threw `InvalidOperationException("secret")`?

- A) the text `secret`
- B) the generic message `An unexpected error occurred invoking '…' on the server.`
- C) nothing
- D) the stack trace

<details><summary>Answer</summary>

**B.** Verified; for visible errors throw `HubException` — its message reaches the client.
→ [Production](advanced.en.md#hub-errors)

</details>

## 6. 🟡 How do you return data as a stream from a hub?

- A) `List<T>`
- B) `IAsyncEnumerable<T>` and `StreamAsync` on the client
- C) only `Clients.All`
- D) you cannot

<details><summary>Answer</summary>

**B.** Verified: the client received `1,2,3,4,5`.
→ [Production](advanced.en.md#streams-from-the-server)

</details>

## 7. 🟡 Select all correct statements

- [ ] A) one user may have several connections
- [ ] B) SignalR picks the transport automatically (WebSockets → SSE → Long Polling)
- [ ] C) the `ConnectionId` is preserved after reconnecting
- [ ] D) `EnableDetailedErrors` is recommended in production

<details><summary>Answer</summary>

**A, B.** C is wrong: after reconnecting the connection is new (and groups must be restored); D is wrong: error details are for development only.
→ [Production](advanced.en.md#reconnecting)

</details>

## 8. 🟡 Open question: how would you implement notifications to a specific user with several tabs?

<details><summary>Key points of a good answer</summary>

- on connect add the connection to a group `user:<id>` (from the authenticated `Context.User`);
- send from anywhere through `IHubContext` to that group;
- authenticate the connection (a token), check permissions;
- a notification history in the DB and catching up after reconnecting.

</details>

## 9. 🔴 Locally `Clients.All` reaches everyone, but in production with three servers only some. Why?

- A) a SignalR bug
- B) connections are spread over servers, and without a backplane `All` is only the current server's clients
- C) a browser limit
- D) WebSockets is off

<details><summary>Answer</summary>

**B.** You need a backplane (Redis) or Azure SignalR Service.
→ [Scale and edges](expert.en.md#scale)

</details>

## 10. 🔴 A client was offline during a broadcast. Will it get the message when it returns?

- A) yes, SignalR stores everything
- B) no: SignalR is not a queue; you need history in a DB and catching up
- C) yes, but only by `ConnectionId`
- D) yes, if a backplane is on

<details><summary>Answer</summary>

**B.** Delivery is not guaranteed; a backplane only connects the servers.
→ [Scale and edges](expert.en.md#edges-and-gotchas)

</details>
