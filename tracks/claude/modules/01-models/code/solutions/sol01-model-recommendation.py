"""Solution: classify a simple Claude workload.

Module: claude/01-models (level 5)
Run: uv run python solutions/sol01-model-recommendation.py
"""


def suggest_model(workload: str) -> str:
    models = {
        "classification": "claude-haiku-5-5",
        "everyday": "claude-sonnet-5-5",
        "coding": "claude-opus-5-5",
        "demanding": "claude-fable-5-2",
    }
    return models.get(workload, "claude-opus-5-5")


for workload in ("classification", "everyday", "coding", "demanding", "unknown"):
    print(workload, suggest_model(workload))
