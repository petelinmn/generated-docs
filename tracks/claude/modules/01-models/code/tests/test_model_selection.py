"""Tests: Claude model selection examples.

Module: claude/01-models (level 6)
Run: uv run pytest
"""

import runpy
from pathlib import Path

import pytest

CODE_DIR = Path(__file__).resolve().parents[1]

EXAMPLE_PATH = CODE_DIR / "examples" / "ex01-model-recommendation.py"
recommend_model = runpy.run_path(str(EXAMPLE_PATH))["recommend_model"]
ROUTING_PATH = CODE_DIR / "solutions" / "sol02-routing-plan.py"
route = runpy.run_path(str(ROUTING_PATH))["route"]


@pytest.mark.parametrize(
    ("workload", "expected"),
    [
        ("classification", "claude-haiku-5-5"),
        ("high_volume", "claude-haiku-5-5"),
        ("everyday", "claude-sonnet-5-5"),
        ("coding", "claude-opus-5-5"),
        ("demanding", "claude-fable-5-2"),
        ("unknown", "claude-opus-5-5"),
    ],
)
def test_recommend_model(workload: str, expected: str) -> None:
    assert recommend_model(workload) == expected


@pytest.mark.parametrize(
    ("task", "expected"),
    [
        ({"demanding": True}, {"model": "claude-fable-5-2", "effort": "high"}),
        ({"complex": True}, {"model": "claude-opus-5-5", "effort": "medium"}),
        (
            {"latency_sensitive": True},
            {"model": "claude-haiku-5-5", "effort": "medium"},
        ),
        (
            {"cost_sensitive": True},
            {"model": "claude-haiku-5-5", "effort": "medium"},
        ),
        (
            {"demanding": True, "complex": True, "latency_sensitive": True},
            {"model": "claude-fable-5-2", "effort": "high"},
        ),
        (
            {"complex": True, "latency_sensitive": True},
            {"model": "claude-opus-5-5", "effort": "medium"},
        ),
        ({}, {"model": "claude-sonnet-5-5", "effort": "high"}),
    ],
)
def test_route(task: dict[str, bool], expected: dict[str, str]) -> None:
    assert route(task) == expected
