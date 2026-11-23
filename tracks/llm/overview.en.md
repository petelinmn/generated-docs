---
title: "LLM"
lang: en
track: llm
kind: track
doc: overview
slug: llm
tags: [llm, ai]
versions: ""
default_level: 4
status: draft
updated: 2026-11-20
---

# LLM

🌐 [Русский](overview.ru.md) · **English**

## In a nutshell

Large language models (LLMs) are neural networks based on the transformer architecture, trained to predict the next token on huge amounts of text. Through an API or locally, they solve text and code tasks: answer questions, extract data, call external tools and act as agents. This track covers how models work and how to build reliable applications on top of them: from the first API request to RAG, agents, quality evaluation and production.

**Prerequisites:** modules 01–08 of the [Python track](../python/overview.en.md).

## Environment

| What | Tool |
|---|---|
| Language | Python 3.12+ and uv, as in the Python track. For .NET — Microsoft.Extensions.AI |
| API access | the provider key in an environment variable or in `.env` (the file is in `.gitignore`, never commit it) |
| Local models | Ollama |
| Module code | as in the Python track: `code/` with `pyproject.toml`, `examples/`, `solutions/`, `tests/` |
| Run all module tests | `uv run pytest` inside `code/` |
| Tests and the API | unit tests never call the API: the client is replaced with a fake. Integration tests are marked `integration` and skipped when there is no key |

## Roadmap

Learning progress is tracked in [PROGRESS.md](../../PROGRESS.md). Default detail level for modules: **4** (theory and self-check, no code), see [detail levels](../../README.en.md#detail-levels).

### Part 1. Fundamentals

- [01 · How LLMs work](modules/01-how-llms-work/overview.en.md) — tokens, next-token prediction, training, context window, temperature
- [02 · Prompt engineering](modules/02-prompt-engineering/overview.en.md) — prompt structure, system instructions, few-shot, reasoning
- [03 · Working with the API](modules/03-llm-apis/overview.en.md) — messages and roles, parameters, streaming, limits, cost
- [04 · Structured output](modules/04-structured-output/overview.en.md) — JSON, schemas, validation with Pydantic
- [05 · Tool use](modules/05-tool-use/overview.en.md) — tool definitions, the call loop, error handling
- [06 · Embeddings and vector search](modules/06-embeddings/overview.en.md) — vectors, cosine similarity, vector databases

### Part 2. Advanced topics

- [07 · RAG](modules/07-rag/overview.en.md) — chunking, indexing, retrieval, reranking, quality evaluation
- [08 · Agents](modules/08-agents/overview.en.md) — the agent loop, planning, memory, multi-agent patterns
- [09 · Model Context Protocol](modules/09-mcp/overview.en.md) — servers and clients, tools and resources, your own MCP server
- [10 · Quality evaluation (evals)](modules/10-evals/overview.en.md) — datasets, metrics, LLM-as-judge, regression testing of prompts
- [11 · Local models](modules/11-local-models/overview.en.md) — Ollama, llama.cpp, quantization, open-weight models
- [12 · Context engineering](modules/12-context-engineering/overview.en.md) — context management, prompt caching, compaction, long conversations

### Part 3. Expert topics

- [13 · The transformer from the inside](modules/13-transformer-internals/overview.en.md) — attention, positional encoding, a mini-GPT from scratch
- [14 · Fine-tuning](modules/14-fine-tuning/overview.en.md) — when you need it, LoRA and QLoRA, data preparation
- [15 · LLMs in production](modules/15-production/overview.en.md) — cost, latency, retries, observability, guardrails
- [16 · Security](modules/16-security/overview.en.md) — prompt injection, data leaks, safe tool use

## Libraries

- [Provider SDKs](libraries/provider-sdks/overview.en.md) — official API clients (Anthropic, OpenAI and others)
- [MCP SDK](libraries/mcp-sdk/overview.en.md) — building MCP servers and clients
- [Ollama](libraries/ollama/overview.en.md) — running models locally
- [sentence-transformers](libraries/sentence-transformers/overview.en.md) — local embeddings
- [Qdrant](libraries/qdrant/overview.en.md) — a vector database
- [pgvector](libraries/pgvector/overview.en.md) — vector search in PostgreSQL
- [Hugging Face Transformers](libraries/hf-transformers/overview.en.md) — loading and running open-weight models
- [Agent frameworks](libraries/agent-frameworks/overview.en.md) — overview and comparison: when to use a framework and when to write your own loop

## Recipes

- [Chatbot with history](recipes/chat-with-history/overview.en.md) — conversation, streaming, context trimming
- [RAG over documentation](recipes/rag-over-docs/overview.en.md) — answering questions from this knowledge base
- [Agent with tools](recipes/agent-with-tools/overview.en.md) — search, files, calculations
- [Your own MCP server](recipes/mcp-server/overview.en.md) — connecting it to Claude Code and other clients
- [Prompt evaluation pipeline](recipes/llm-eval-pipeline/overview.en.md) — dataset, run, version comparison
- [Data extraction from text](recipes/data-extraction/overview.en.md) — structured data from documents and PDFs

## Projects

- [Knowledge base assistant](projects/docs-assistant/overview.en.md) — RAG over this repository: answers with citations and quizzes per module
- [Mini coding agent](projects/coding-agent/overview.en.md) — an agent that reads and edits project files

## Related tracks

- [Python](../python/overview.en.md) — the main language of this track
- [C# / .NET](../csharp/overview.en.md) — LLM applications on .NET
- [Claude](../claude/overview.en.md) — Anthropic models, the Messages API and Claude Code

## Sources

- API documentation of the chosen provider
- [Model Context Protocol](https://modelcontextprotocol.io/)
- [Hugging Face documentation](https://huggingface.co/docs)
