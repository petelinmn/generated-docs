---
title: "Claude Agent SDK: expert"
lang: en
track: claude
kind: library
doc: expert
slug: agent-sdk
part: 2
tags: [claude, agent-sdk, agents, tool-use, hooks]
versions: "claude-agent-sdk 0.2.189"
status: draft
updated: 2026-11-25
---

# Claude Agent SDK: expert

🌐 [Русский](expert.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · **3. Expert** · [4. Quiz](quiz.en.md)

## `ClaudeAgentOptions` fields

| Group | Fields |
|---|---|
| Model | `model`, `fallback_model`, `thinking`, `effort`, `betas` |
| Tools | `tools`, `allowed_tools`, `disallowed_tools`, `mcp_servers`, `strict_mcp_config` |
| Permissions | `permission_mode`, `can_use_tool`, `permission_prompt_tool_name`, `sandbox` |
| Limits | `max_turns`, `max_budget_usd`, `task_budget`, `max_buffer_size` |
| Sessions | `resume`, `session_id`, `continue_conversation`, `fork_session`, `session_store` |
| Context | `system_prompt`, `setting_sources`, `skills`, `plugins`, `agents`, `cwd`, `add_dirs` |
| Extensions | `hooks`, `output_format`, `include_partial_messages`, `enable_file_checkpointing` |
| Process | `cli_path`, `env`, `extra_args`, `stderr`, `user`, `settings` |

Field names come from the installed 0.2.189 package; not all were exercised.

## Client methods

`connect`, `disconnect`, `query`, `receive_messages`, `receive_response`, `interrupt`, `set_model`, `set_permission_mode`, `get_context_usage`, `get_mcp_status`, `reconnect_mcp_server`, `toggle_mcp_server`, `rewind_files`, `stop_task`, `get_server_info`

## How it works

The SDK launches the CLI as a subprocess and exchanges messages with it. Permission requests, hooks and your tools are passed back to your process. So a working CLI is needed (the package bundles one), `cli_path` overrides it, and environment variables and settings apply as in Claude Code.

## Gotchas

1. `allowed_tools` is auto-approval, not the list of available tools; availability is set by `tools` and `disallowed_tools`.
2. An uncaught tool exception reaches the model verbatim. The [MCP SDK](../mcp-sdk/advanced.en.md)'s `MCPServer` does the opposite and hides it.
3. `query()` yields the error result and then raises.
4. A permission denial does not make the run fail.
5. Without `setting_sources`, a developer machine's personal settings leak in.
6. Sessions hold the conversation and tool results in plain text.

> ⚠️ Verify: `max_budget_usd`, `session_store`, `sandbox`, `interrupt`, file checkpoints and the TypeScript SDK were not run.

## Trade-offs

| Option | When |
|---|---|
| Agent SDK | an agent in your infrastructure with files and commands |
| `claude -p` | a script or CI step without code |
| Managed Agents | Anthropic hosts the loop and sandbox |
| [Client SDK](../anthropic-sdk/overview.en.md) | full control of the loop |

## Interview questions

1. Agent SDK or client SDK?
<details><summary>Answer</summary>

The Agent SDK is a ready loop with tools in your environment; the client SDK is direct API calls with your own loop.
</details>

2. Where do tools run?
<details><summary>Answer</summary>

On your machine: built-ins in the CLI process, yours in your own process.
</details>

3. `query()` vs `ClaudeSDKClient`?
<details><summary>Answer</summary>

The former is one request; the latter an ongoing conversation with interrupts and setting changes.
</details>

4. Why was `can_use_tool` not called?
<details><summary>Answer</summary>

A rule, the mode or `allowed_tools` had already approved the call.
</details>

5. How do you check every call?
<details><summary>Answer</summary>

A `PreToolUse` hook.
</details>

6. What does the model see when a tool raises?
<details><summary>Answer</summary>

The raw exception text, so compose messages yourself and return `is_error`.
</details>

7. Why `setting_sources=[]`?
<details><summary>Answer</summary>

So settings, hooks and `CLAUDE.md` on disk cannot change behaviour.
</details>

8. How do you share sessions across servers?
<details><summary>Answer</summary>

A `session_store`, copying files, or passing your own state into a new prompt.
</details>

9. How do you bound spending?
<details><summary>Answer</summary>

`max_turns`, `max_budget_usd`, model choice, a narrow tool set.
</details>

10. How do you test an Agent SDK application?
<details><summary>Answer</summary>

Tools and hooks with ordinary unit tests; agent runs against a stand-in or in an isolated environment.
</details>

## Related materials

- [Advanced theory](advanced.en.md) · [Quiz](quiz.en.md)
- [12 · Agents](../../modules/12-agents/expert.en.md) · [17 · Deployment](../../modules/17-production/expert.en.md)

## Sources

- [Python reference](https://code.claude.com/docs/en/agent-sdk/python)
- [Sessions](https://code.claude.com/docs/en/agent-sdk/sessions)
