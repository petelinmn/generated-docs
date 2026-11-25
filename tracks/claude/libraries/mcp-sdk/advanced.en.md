---
title: "MCP SDK: advanced"
lang: en
track: claude
kind: library
doc: advanced
slug: mcp-sdk
part: 2
tags: [claude, mcp, mcp-sdk, tool-use, json-rpc]
versions: "mcp 2.3.0 (Python)"
status: draft
updated: 2026-11-25
---

# MCP SDK: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Tool errors

| What happened | What the client gets |
|---|---|
| `raise ToolError("…")` | `is_error`, your text |
| any other exception | `is_error`, only `Error executing tool <name>` |
| arguments failing the schema | `is_error`, the validation details |
| an unknown tool | `is_error`, `Unknown tool: …` |
| an unknown resource | an exception `MCPError`, `-32602` |

The hidden exception text still goes to the server log (stderr). Check `result.is_error`: a tool call does not raise.

## Context, progress, lifespan

```python
# jobs.py
from contextlib import asynccontextmanager

from mcp.server import MCPServer
from mcp.server.mcpserver import Context


@asynccontextmanager
async def lifespan(server):
    resources = {"unit": "step"}                 # open pools here, close them after yield
    yield resources


mcp = MCPServer("jobs", lifespan=lifespan)


@mcp.tool()
async def run_job(steps: int, ctx: Context) -> str:
    """Run a job and report progress."""
    unit = ctx.request_context.lifespan_context["unit"]
    for i in range(steps):
        await ctx.report_progress(i + 1, steps)
    return f"done {steps} {unit}s"
```

A `Context` parameter is left out of the input schema. The client receives progress through `progress_callback`.

## URI templates and path safety

`orders://{order_id}` is a template: it shows in `list_resource_templates`, not `list_resources`. By default values with traversal (`..`), absolute paths or null bytes are rejected (`resource_security`). It does not replace path checks in your code.

## stdout with stdio

The protocol owns stdout. A single `print()` in a tool produced an `Invalid JSON` parse error on the client; the call still succeeded in the check, but do not rely on that. Write to stderr or use `logging`.

## HTTP: Origin and Host checks

A local server on `127.0.0.1` rejected a foreign `Origin` (403) and a foreign `Host` (421), protecting against DNS rebinding. That is not authentication: a remote server needs authorisation (OAuth, `token_verifier`, `auth`).

## Mount into an existing app

`mcp.streamable_http_app()` returns a Starlette ASGI app with a `/mcp` route.

## Testing

An in-process `Client(mcp)` tests the whole protocol path without network or subprocesses.

## Protocol versions

By default a 2.x client negotiated `2026-07-28`; `Client(..., mode="legacy")` gave `2025-11-25`.

## Related materials

- [Fundamentals](basics.en.md) · [Expert](expert.en.md)
- [08 · MCP](../../modules/08-mcp/advanced.en.md) · [16 · Security](../../modules/16-security/advanced.en.md)

## Sources

- [Handling errors](https://py.sdk.modelcontextprotocol.io/servers/handling-errors/)
- [The Context](https://py.sdk.modelcontextprotocol.io/handlers/context/)
- [Add to an existing app](https://py.sdk.modelcontextprotocol.io/run/asgi/)
- [Authorization](https://py.sdk.modelcontextprotocol.io/run/authorization/)
