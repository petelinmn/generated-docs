"""Solution: route a task to a Claude model and effort level.

Module: claude/01-models (level 5)
Run: uv run python solutions/sol02-routing-plan.py
"""


def route(task: dict[str, bool]) -> dict[str, str]:
    if task.get("demanding"):
        return {"model": "claude-fable-5-2", "effort": "high"}
    if task.get("complex"):
        return {"model": "claude-opus-5-5", "effort": "medium"}
    if task.get("latency_sensitive") or task.get("cost_sensitive"):
        return {"model": "claude-haiku-5-5", "effort": "medium"}
    return {"model": "claude-sonnet-5-5", "effort": "high"}


tasks = [
    {"latency_sensitive": True},
    {"complex": True},
    {"demanding": True},
    {},
]

for task in tasks:
    print(route(task))
