---
title: "Observability: quiz"
lang: en
track: claude
kind: module
doc: quiz
slug: 15-observability
part: 3
tags: [claude, observability, opentelemetry, usage, cost]
versions: "Claude Code 2.1.332; anthropic 2.14.0; Usage and Cost Analytics API"
status: draft
updated: 2026-11-25
---

# Observability: quiz

🌐 [Русский](quiz.ru.md) · **English**

> Answer on your own first, then open the answer. 🟢 [basics](basics.en.md) · 🟡 [advanced](advanced.en.md) · 🔴 [expert](expert.en.md).

## 1. 🟢 What do you give support about a particular request?

- A) all prompt texts
- B) the user's IP
- C) the `request-id`
- D) the API key

<details><summary>Answer</summary>

**C.** The `request-id` identifies the request on Anthropic's side. Prompt texts and IP addresses are unnecessary, and a key must never be shared.
→ [Fundamentals](basics.en.md)

</details>

## 2. 🟢 Where are the token counts in a response?

- A) `usage`
- B) `content`
- C) `stop_details`
- D) `model`

<details><summary>Answer</summary>

**A.** Token consumption is in `usage`. `content` is the reply, `stop_details` explains the stop, `model` names the model.
→ [Fundamentals](basics.en.md)

</details>

## 3. 🟢 Total input is:

- A) `input_tokens` alone
- B) cache reads alone
- C) `output_tokens`
- D) `input_tokens` plus cache writes and reads

<details><summary>Answer</summary>

**D.** With caching `input_tokens` is only the part after the last breakpoint; writes and reads are counted separately and must be added. Output tokens are not input.
→ [Fundamentals](basics.en.md)

</details>

## 4. 🟢 What should not be logged by default?

- A) the model name
- B) prompt texts with personal data
- C) `stop_reason`
- D) latency

<details><summary>Answer</summary>

**B.** Texts may contain personal data and secrets, and logs persist and are widely readable. Model, stop reason and latency are safe and useful.
→ [Fundamentals](basics.en.md)

</details>

## 5. 🟡 What turns on Claude Code telemetry?

- A) nothing, it always reaches your collector
- B) `CLAUDE_CODE_ENABLE_TELEMETRY=1`
- C) `--verbose`
- D) `/cost`

<details><summary>Answer</summary>

**B.** This variable, together with exporter settings, enables the export. Nothing reaches your collector by default, and `--verbose` or `/cost` do not enable it.
→ [Advanced theory](advanced.en.md)

</details>

## 6. 🟡 The prompt text in an event by default:

- A) the full text
- B) the first 500 characters
- C) a hash
- D) `<REDACTED>`

<details><summary>Answer</summary>

**D.** Content is hidden by default; only `OTEL_LOG_USER_PROMPTS` reveals it. Neither a truncated text nor a hash is sent instead.
→ [Advanced theory](advanced.en.md)

</details>

## 7. 🟡 What does the `api_request` event carry?

- [ ] A) tokens and `cost_usd`
- [ ] B) `duration_ms`
- [ ] C) file contents
- [ ] D) the API key

<details><summary>Answer</summary>

**A, B.** The event describes the request with measurements: tokens, cost, duration. It carries no file contents or keys.
→ [Advanced theory](advanced.en.md)

</details>

## 8. 🟡 What does the Analytics API need?

- A) nothing
- B) an OTel collector
- C) an admin credential
- D) an ordinary key

<details><summary>Answer</summary>

**C.** The Analytics API exposes organisation-wide data, so it needs an Admin key. A regular key is not enough, and OTel is a separate mechanism.
→ [Advanced theory](advanced.en.md)

</details>

## 9. 🔴 `DISABLE_TELEMETRY=1` with OTel export configured:

- A) the export stops
- B) a start-up error
- C) only traces are sent
- D) the export continues

<details><summary>Answer</summary>

**D.** In the check `DISABLE_TELEMETRY=1` did not stop the export to your own collector, which is configured separately. There was no startup error, and more than traces arrived.
→ [Expert theory](expert.en.md)

</details>

## 10. 🔴 What does `OTEL_LOG_RAW_API_BODIES` reveal?

- A) the program version
- B) full request and response bodies
- C) status codes only
- D) metrics only

<details><summary>Answer</summary>

**B.** It exports full bodies, meaning the entire conversation. Use it only for debugging and with a protected collector.
→ [Expert theory](expert.en.md)

</details>

## 11. 🔴 Which spans arrived with tracing?

- A) `db.query`
- B) none
- C) `claude_code.llm_request` and `claude_code.tool`
- D) `http.server`

<details><summary>Answer</summary>

**C.** Claude Code tracing produced interaction, model request and tool spans. There were no database or HTTP server spans, and it was not empty.
→ [Expert theory](expert.en.md)

</details>

## 12. 🔴 Open question: costs tripled in a week. How do you find the cause?

<details><summary>Answer</summary>

Break down by model, key or workspace, feature and token type; check cache hits, history length, retries and agent turns, new models and effort; compare cost per task before and after the release.
→ [Advanced theory](advanced.en.md)

</details>
