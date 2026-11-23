"""Example: recommend a Claude model for a workload.

Module: claude/01-models (level 5)
Run: uv run python examples/ex01-model-recommendation.py
"""


def recommend_model(workload: str) -> str:
    recommendations = {
        "classification": "claude-haiku-5-5",
        "high_volume": "claude-haiku-5-5",
        "everyday": "claude-sonnet-5-5",
        "coding": "claude-opus-5-5",
        "demanding": "claude-fable-5-2",
    }
    return recommendations.get(workload, "claude-opus-5-5")


for workload in ("classification", "everyday", "coding", "demanding", "unknown"):
    print(workload, recommend_model(workload))
