---
title: "MCP and connectors: advanced"
lang: en
track: claude
kind: module
doc: advanced
slug: 08-mcp
part: 2
tags: [claude, mcp, mcp-connector, tool-use, claude-code]
versions: "MCP specification 2026-07-28; Python SDK mcp 2.3.0; anthropic 2.14.0; anthropic-beta mcp-client-2025-11-20"
status: draft
updated: 2026-11-25
---

# MCP and connectors: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Three ways to connect a server to Claude

| Way | Who is the MCP client | What it supports |
|---|---|---|
| your client + tool use | your code | stdio and HTTP; tools, resources, prompts |
| MCP connector (Messages API) | Anthropic's servers | tool calls only; a public `https` URL |
| Claude Code | Claude Code | local and remote servers |

## Your own client: a bridge to the Messages API

```python
# bridge.py: needs ANTHROPIC_API_KEY
import asyncio

from anthropic import AsyncAnthropic
from anthropic.lib.tools.mcp import async_mcp_tool
from mcp import Client

from server import mcp


async def main():
    claude = AsyncAnthropic()
    async with Client(mcp) as client:  # in-process connection
        tools = [async_mcp_tool(t, client) for t in (await client.list_tools()).tools]
        runner = claude.beta.messages.tool_runner(
            model="claude-opus-5-5", max_tokens=1024, tools=tools,
            messages=[{"role": "user", "content": "Where is order A-1?"}])
        async for message in runner:
            print(message.stop_reason, [b.type for b in message.content])


asyncio.run(main())
```

```text
tool_use ['tool_use']
end_turn ['text']
```

The SDK helper turns an MCP tool into a tool-runner tool. The output comes from the stand-in: it requested `order_status`, the runner called the server and sent the result back. By hand, copying `name`, `description` and `input_schema` is enough.

## MCP connector

```python
response = client.beta.messages.create(
    model="claude-opus-5-5",
    max_tokens=1000,
    messages=[{"role": "user", "content": "Where is order A-1?"}],
    mcp_servers=[{"type": "url", "url": "https://mcp.example.com/mcp", "name": "shop"}],
    tools=[{"type": "mcp_toolset", "mcp_server_name": "shop",
            "default_config": {"enabled": False},
            "configs": {"order_status": {"enabled": True}}}],
    betas=["mcp-client-2025-11-20"],
)
```

- Every server in `mcp_servers` must be referenced by exactly one `mcp_toolset`.
- `default_config` and `configs` make an allowlist; here only `order_status`.
- An OAuth token goes in `authorization_token`; obtaining and refreshing it is your job.
- The connector is not covered by zero data retention (ZDR) arrangements.

> ⚠️ Verify: the request shape was checked against the stand-in; a response with `mcp_tool_use` and `mcp_tool_result` blocks and a real remote server were not exercised.

## Streamable HTTP

```python
# http_server.py
from server import mcp

mcp.run(transport="streamable-http", port=8765)   # http://127.0.0.1:8765/mcp
```

A client only needs the URL: `Client("http://127.0.0.1:8765/mcp")`. A remote server needs TLS and authentication; a local one should not listen on all interfaces.

## Claude Code

```bash
claude mcp add shop -- python server.py                                   # stdio, local scope
claude mcp add --scope project --transport http docs https://mcp.example.com/mcp   # writes .mcp.json
claude mcp list
claude mcp get shop
claude mcp remove shop -s local
```

| Scope | Stored in | Visible to |
|---|---|---|
| `local` (default) | `~/.claude.json` | you, in this project |
| `project` | `.mcp.json` | the team, through git |
| `user` | `~/.claude.json` | you, in every project |

A server from `.mcp.json` does not connect until you approve it: `list` showed `Pending approval`. Tools are named `mcp__<server>__<tool>`; permission rules use that name.

## Common mistakes

- Committing a token in `.mcp.json`: use environment variable expansion.
- Connecting dozens of tools with no filtering.
- Treating a tool result as trusted instructions.

## Related materials

- [Fundamentals](basics.en.md) · [Expert](expert.en.md)
- [09 · Claude Code](../09-claude-code/overview.en.md)

## Sources

- [MCP connector](https://platform.claude.com/docs/en/agents-and-tools/mcp-connector)
- [Claude Code: MCP](https://code.claude.com/docs/en/mcp)
- [Streamable HTTP](https://modelcontextprotocol.io/specification/2026-07-28/basic/transports/streamable-http)
