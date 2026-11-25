---
title: "Claude Code: advanced"
lang: en
track: claude
kind: module
doc: advanced
slug: 09-claude-code
part: 2
tags: [claude, claude-code, permissions, headless, ci]
versions: "Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# Claude Code: advanced

🌐 [Русский](advanced.ru.md) · **English**

**Levels:** [overview](overview.en.md) · [1. Basics](basics.en.md) · **2. Advanced** · [3. Expert](expert.en.md) · [4. Quiz](quiz.en.md)

## Non-interactive mode

```bash
git diff | claude -p "Review this diff"                       # piped input becomes part of the prompt
claude -p "List the TODOs" --output-format json               # result, session_id, num_turns, cost, denials
claude -p "..." --output-format stream-json --verbose         # events as they happen
claude -p "Count the files" --max-turns 3                     # stop after 3 turns
claude -p "Extract the version" --json-schema '{"type":"object","properties":{"n":{"type":"integer"}},"required":["n"]}'
claude -p "Run the tests" --allowedTools "Bash(npm test)" --permission-mode dontAsk
claude -p "Next step" --continue                              # the latest session in this directory
claude -p "Next step" --resume "$SESSION_ID"
```

- `stream-json` with `-p` requires `--verbose`.
- A disallowed action does not stop the run: the tool gets a refusal, the exit code stays 0 and refusals are listed in `permission_denials`.
- Hitting `--max-turns` gives `subtype: error_max_turns` and exit code 1; an unreachable API also gives 1.
- `-p` skips the workspace trust dialog: run it only in repositories you trust.

## Settings and precedence

| File | For |
|---|---|
| `~/.claude/settings.json` | you, in every project |
| `.claude/settings.json` | the team, in git |
| `.claude/settings.local.json` | you, in this project |
| managed settings | organisation policy |

Checked with the `model` key: the project file overrode the user one, the local file overrode the project one, and `--model` overrode all.

## Permission rules

```json
{
  "permissions": {
    "allow": ["Bash(npm run test *)", "Bash(git diff *)"],
    "ask": ["Bash(git push *)"],
    "deny": ["Read(./.env)", "Bash(curl *)"]
  }
}
```

`allow` rules from a file in the repository take effect only after you accept the trust dialog for the directory.

The order is `deny`, then `ask`, then `allow`; specificity does not change it. `deny` applies even in `bypassPermissions`.

- A compound command is checked part by part: with `Bash(touch *)` allowed, `touch a && rm a` still asked about `rm`.
- Bash rules match command text: `/bin/rm` or `sh -c` slips past. Real boundaries need a sandbox or a hook.
- `Read(./.env)` in `deny` stopped both Read and `cat .env`, but does not protect against a script that opens the file itself.

## Sessions and rollback

- `--continue` and `--resume` brought the history back; a new run without them starts empty.
- `/rewind` (or `Esc Esc` on empty input) restores code, conversation or both. A file changed only by Bash commands is not tracked, nor are external changes.

## Editors

- VS Code (1.94.0+): an extension with inline diffs, `@`-mentions and plan review; Claude sees the selected text.
- JetBrains: a plugin over the installed CLI, diffs in the IDE and code diagnostics.

## CI: GitHub Actions

```yaml
name: Claude Code
on:
  issue_comment:
    types: [created]
  pull_request_review_comment:
    types: [created]
jobs:
  claude:
    if: contains(github.event.comment.body, '@claude')
    runs-on: ubuntu-latest
    permissions:
      contents: write
      pull-requests: write
      issues: write
      id-token: write
      actions: read
    steps:
      - uses: actions/checkout@v6
        with:
          fetch-depth: 1
      - uses: anthropics/claude-code-action@v1
        with:
          anthropic_api_key: ${{ secrets.ANTHROPIC_API_KEY }}
```

The easiest setup is `/install-github-app` inside `claude`. With no `prompt` input the action waits for an `@claude` mention.

## Related materials

- [Fundamentals](basics.en.md) · [Expert](expert.en.md)
- [08 · MCP](../08-mcp/advanced.en.md)

## Sources

- [Headless mode](https://code.claude.com/docs/en/headless)
- [Settings](https://code.claude.com/docs/en/settings)
- [Permissions](https://code.claude.com/docs/en/permissions)
- [Checkpointing](https://code.claude.com/docs/en/checkpointing)
- [GitHub Actions](https://code.claude.com/docs/en/github-actions)
