---
title: "Security and compliance: expert"
lang: en
track: claude
kind: module
doc: expert
slug: 16-security
part: 3
tags: [claude, security, prompt-injection, secrets, compliance]
versions: "Claude 5.5 family; Claude Code 2.1.332; anthropic 2.14.0"
status: draft
updated: 2026-11-25
---

# Security and compliance: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## Identity

| Means | What it is |
|---|---|
| API key | a static secret; expiry, scope, rotation |
| service account | a non-human organisation identity (`svac_…`) |
| Workload Identity Federation | exchanges your provider's OIDC token for a short-lived one; you configure a service account, an issuer and a rule |
| Admin key | organisation management; not for ordinary requests |

## Sandbox properties

macOS uses Seatbelt, Linux and WSL2 `bubblewrap` and `socat`. Writes go to the working and temp directories, network through a proxy with an allowed-domain list that starts empty. `excludedCommands` run outside; `allowUnsandboxedCommands: false` forbids falling back out.

## Gotchas

1. A string prefix check (`startswith`) lets `workspace-evil` through; compare resolved paths with `is_relative_to`.
2. A `Read` deny rule does not stop a script that opens the file itself.
3. `-p` skips the workspace trust dialog.
4. Settings, hooks, skills and `.mcp.json` from a stranger's repository are executable content.
5. Telemetry with content enabled moves sensitive data into the collector.
6. ZDR does not mean nothing is stored on your side: logs and transcripts remain.

> ⚠️ Verify: ZDR and HIPAA terms, WIF, the sandbox and `inference_geo` on the real API were not checked. Settle legal questions by your contract.

## Interview questions

1. What is indirect prompt injection?
<details><summary>Answer</summary>

Instructions inside data the model reads while working.
</details>

2. Can a prompt solve it?
<details><summary>Answer</summary>

No, only reduce risk; you need privilege limits and checks in code.
</details>

3. Which combination of capabilities is dangerous?
<details><summary>Answer</summary>

Private data, untrusted content and an outbound channel together.
</details>

4. How do you protect a file-reading tool?
<details><summary>Answer</summary>

Fix a root, resolve the path and check containment, mind symlinks, cap size.
</details>

5. Why is WIF better than a static key?
<details><summary>Answer</summary>

Short-lived tokens from your identity provider, nothing to steal from CI or rotate.
</details>

6. Where do you call the API from in a web app?
<details><summary>Answer</summary>

On the server; the key never reaches the browser.
</details>

7. What is ZDR?
<details><summary>Answer</summary>

An arrangement under which prompts and replies are not stored after the response; not for every feature or model.
</details>

8. Why human confirmation?
<details><summary>Answer</summary>

For irreversible or costly actions: a model mistake cannot be undone.
</details>

9. Permission rules vs sandbox?
<details><summary>Answer</summary>

The former decide from text whether to ask; the latter confines the process at OS level.
</details>

10. What do you check before adding a third-party MCP server or plugin?
<details><summary>Answer</summary>

Code and author, permissions, where data goes, a pinned version.
</details>

## Related materials

- [Advanced theory](advanced.en.md) · [Quiz](quiz.en.md)
- [12 · Agents](../12-agents/expert.en.md) · [15 · Observability](../15-observability/expert.en.md)

## Sources

- [Workload Identity Federation](https://platform.claude.com/docs/en/manage-claude/workload-identity-federation)
- [Claude Code sandboxing](https://code.claude.com/docs/en/sandboxing)
- [Claude Code data usage](https://code.claude.com/docs/en/data-usage)
