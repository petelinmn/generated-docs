---
title: "Claude"
lang: en
track: claude
kind: track
doc: overview
slug: claude
tags: [claude, anthropic, messages-api, claude-code, tool-use]
versions: "Claude 5.5 family; Claude Code, 2026-11"
default_level: 4
status: draft
updated: 2026-11-23
---

# Claude

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

Claude is Anthropic's family of language models for text, image and document analysis, software engineering and agentic work. This track covers two related areas: application development through Claude Platform and autonomous work with Claude Code in a terminal or editor.

The track's main interfaces are the Messages API, tool use, structured outputs and Claude Code. Learning progresses from the first synchronous or streaming generation through tools, MCP, skills, hooks and managed agents.

**Prerequisites:** basic programming and HTTP fundamentals; the [Python track](../python/overview.en.md) is sufficient for SDK examples.

## Environment

| What | Tool |
|---|---|
| Example language | Python 3.12+ with the official Anthropic SDK and `uv` |
| Alternative SDKs | TypeScript/JavaScript, C#, Java, Go and other official SDKs |
| API access | `ANTHROPIC_API_KEY` in an environment variable or `.env` |
| Claude Code | terminal, VS Code, JetBrains, desktop or web |
| Module code | as in the Python track: `code/` with `pyproject.toml`, `examples/`, `solutions/`, `tests/` |
| Run all module tests | `uv run pytest` inside `code/` |
| Tests and the API | API calls are replaced with a fake or mock server; integration tests skip without a key |

## Roadmap

Learning progress is tracked in [PROGRESS.md](../../PROGRESS.md). Default detail level for modules: **4** (theory and self-check), see [detail levels](../../README.en.md#detail-levels).

### Part 1. Fundamentals

- [01 · The Claude family and model selection](modules/01-models/overview.en.md) — model, context, output parameters, pricing and lifecycle
- [02 · Messages API](modules/02-messages-api/overview.en.md) — messages, system prompts, content blocks, streaming and stop reasons
- [03 · Prompt engineering](modules/03-prompt-engineering/overview.en.md) — roles, instruction structure, examples, XML markup and thinking
- [04 · Structured outputs](modules/04-structured-output/overview.en.md) — JSON schemas, validation and data extraction
- [05 · Prompt caching](modules/05-prompt-caching/overview.en.md) — cached prefixes, breakpoints and cache-miss diagnostics
- [06 · Working with documents](modules/06-documents-and-citations/overview.en.md) — PDFs, images, files, citations and context limits

### Part 2. Advanced topics

- [07 · Tool use](modules/07-tool-use/overview.en.md) — client tools, server tools, tool_choice and the tool_result loop
- [08 · MCP and connectors](modules/08-mcp/overview.en.md) — MCP servers, tunnels, remote connectors and protocol SDK
- [09 · Claude Code](modules/09-claude-code/overview.en.md) — commands, editors, terminal, CI and repository work
- [10 · CLAUDE.md and memory](modules/10-memory/overview.en.md) — project instructions, auto memory and context management
- [11 · Skills and hooks](modules/11-skills-and-hooks/overview.en.md) — reusable workflows, automatic activation and lifecycle hooks
- [12 · Agents and subagents](modules/12-agents/overview.en.md) — orchestration, Agent SDK, managed agents, budgets and parallelism

### Part 3. Expert topics

- [13 · Compaction and long context](modules/13-long-context/overview.en.md) — 1M context, compaction and message budgets
- [14 · Batch, evals and reliability](modules/14-evals/overview.en.md) — Batches API, regression evals, stop reasons and guardrails
- [15 · Observability](modules/15-observability/overview.en.md) — usage/cost analytics, OpenTelemetry, traces and metrics
- [16 · Security and compliance](modules/16-security/overview.en.md) — permissions, secrets, ZDR, residency and safe tool use
- [17 · Deployment and production](modules/17-production/overview.en.md) — Bedrock, Foundry, regional endpoints, retries and cost

## Libraries

- [Anthropic SDK](libraries/anthropic-sdk/overview.en.md) — clients for Python, TypeScript and other languages
- [Claude Agent SDK](libraries/agent-sdk/overview.en.md) — agents built on Claude Code tools and capabilities
- [MCP SDK](libraries/mcp-sdk/overview.en.md) — Model Context Protocol servers and clients
- [Claude Code](libraries/claude-code/overview.en.md) — CLI, IDE integrations, desktop and web

## Recipes

- [Chat with Claude](recipes/chat-with-history/overview.en.md) — stateless sessions, history and streaming
- [Tool agent](recipes/tool-agent/overview.en.md) — tool_use, tool_result and error handling
- [Structured data extraction](recipes/structured-extraction/overview.en.md) — JSON Schema and citations
- [Repository automation](recipes/claude-code-workflow/overview.en.md) — CLAUDE.md, skills, hooks and CI
- [RAG with Claude](recipes/rag-with-claude/overview.en.md) — retrieval, citations and prompt caching

## Projects

- [Claude API service](projects/api-service/overview.en.md) — a small service with streaming and structured output
- [Coding agent workflow](projects/coding-agent/overview.en.md) — Claude Code and MCP for a typical project
- [Document assistant](projects/document-assistant/overview.en.md) — PDF analysis with citations and quality evaluation

## Related tracks

- [LLM](../llm/overview.en.md) — general language-model, RAG and agent theory
- [Python](../python/overview.en.md) — primary language for SDK examples
- [JavaScript](../javascript/overview.en.md) — JavaScript SDK examples and Node.js integrations
- [TypeScript](../typescript/overview.en.md) — type-safe clients and schemas

## Sources

- [Claude models overview](https://platform.claude.com/docs/en/models/overview)
- [Messages API](https://platform.claude.com/docs/en/api/messages)
- [Tool use with Claude](https://platform.claude.com/docs/en/agents-and-tools/tool-use/overview)
- [Claude Platform features overview](https://platform.claude.com/docs/en/build-with-claude/overview)
- [Claude Code overview](https://code.claude.com/docs/en/overview)
