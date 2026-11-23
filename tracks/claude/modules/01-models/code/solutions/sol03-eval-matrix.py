"""Solution: build a Claude model and effort eval matrix.

Module: claude/01-models (level 5)
Run: uv run python solutions/sol03-eval-matrix.py
"""


models = ("claude-haiku-5-5", "claude-sonnet-5-5")
efforts = ("low", "medium", "high")

matrix = [
    {
        "model": model,
        "effort": effort,
        "accuracy": None,
        "latency_ms": None,
        "cost_usd": None,
    }
    for model in models
    for effort in efforts
]

print(len(matrix), matrix[0]["model"], matrix[0]["effort"])
