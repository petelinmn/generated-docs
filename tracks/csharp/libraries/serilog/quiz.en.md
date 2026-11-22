---
title: "Quiz: Serilog"
lang: en
track: csharp
kind: library
doc: quiz
slug: serilog
part: 2
tags: [serilog, structured-logging, sinks, enrichers]
status: draft
updated: 2026-11-22
---

# Quiz: Serilog

🌐 [Русский](quiz.ru.md) · **English**

> Answer first, then open the answer. The mark: 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What is structured logging?

- A) logs in a table
- B) a message template with named properties, the values are kept as data
- C) logs in XML
- D) JSON output only

<details><summary>Answer</summary>

**B.** The values (`User`, `Count`) are separate properties that can be searched.
→ [Basics](basics.en.md#what-it-is-and-why)

</details>

## 2. 🟢 Which statement is correct?

- A) `Log.Information($"user {user}")`
- B) `Log.Information("user {User}", user)`
- C) `Log.Information("user " + user)`
- D) `Log.Information("user {0}")`

<details><summary>Answer</summary>

**B.** Verified: interpolation gives a ready string without properties.
→ [Basics](basics.en.md#beginner-mistakes)

</details>

## 3. 🟢 What does `{@Order}` do?

- A) calls `ToString()`
- B) destructures the object into a structure with properties
- C) ignores the value
- D) encrypts

<details><summary>Answer</summary>

**B.** Verified: `{"Id": 7, "Customer": "Ann", "Total": 9.5, "$type": "Order"}`; without `@` — `ToString()`.
→ [Basics](basics.en.md#message-templates-and-properties)

</details>

## 4. 🟢 Why `Log.CloseAndFlush()`?

- A) for beauty
- B) to flush sink buffers on shutdown
- C) to change the level
- D) for file rotation

<details><summary>Answer</summary>

**B.** Otherwise the last messages may not be written.
→ [Basics](basics.en.md#the-first-configuration)

</details>

## 5. 🟡 What is needed for `LogContext.PushProperty` to work?

- A) nothing
- B) `.Enrich.FromLogContext()`
- C) `.WriteTo.Console()`
- D) `MinimumLevel.Verbose()`

<details><summary>Answer</summary>

**B.** Verified: inside the scope the `RequestId` property is present, outside it is not.
→ [Advanced](advanced.en.md#enrichment-and-context)

</details>

## 6. 🟡 What does `MinimumLevel.Override("Noisy", Warning)` do?

- A) turns logs off
- B) for sources with the `Noisy` prefix only `Warning` and above pass
- C) changes the level of all logs
- D) removes a sink

<details><summary>Answer</summary>

**B.** Verified: info from `Noisy.Thing` was dropped, the warning passed; `Other` was not affected.
→ [Advanced](advanced.en.md#overriding-levels-per-source)

</details>

## 7. 🟡 Select all correct statements

- [ ] A) `CompactJsonFormatter` writes one event per line
- [ ] B) `AddSerilog` lets Serilog receive `ILogger` messages
- [ ] C) numbers in the JSON log are stored as strings
- [ ] D) the properties of a `BeginScope` scope get into the event

<details><summary>Answer</summary>

**A, B, D.** C is wrong: `"Count":3` is a number.
→ [Advanced](advanced.en.md#integration-with-microsoftextensionslogging)

</details>

## 8. 🟡 Open question: how do you configure logs so that all of a request's messages carry `RequestId`?

<details><summary>Key points of a good answer</summary>

- `Enrich.FromLogContext()`;
- in middleware — `using (LogContext.PushProperty("RequestId", id))` around request handling;
- JSON output, search by `RequestId` in the log system.

</details>

## 9. 🔴 Why is a `decimal` `9,5` in text but `9.5` in JSON?

- A) a bug
- B) the text uses the current culture, the structured value is a number
- C) different Serilog versions
- D) because of the encoding

<details><summary>Answer</summary>

**B.** Verified; use the invariant culture/`formatProvider`.
→ [Expert](expert.en.md#edges-and-gotchas)

</details>

## 10. 🔴 How do you avoid computing expensive arguments of a `Debug` log when `Debug` is off?

- A) you cannot
- B) wrap in `Log.IsEnabled(LogEventLevel.Debug)` or use `LoggerMessage`
- C) use interpolation
- D) raise the sink's level

<details><summary>Answer</summary>

**B.** The filter fires before the event is built, but the arguments are computed earlier.
→ [Expert](expert.en.md#edges-and-gotchas)

</details>
