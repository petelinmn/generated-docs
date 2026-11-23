"""Tests: deterministic Claude eval matrix.

Module: claude/01-models (level 6)
Run: uv run pytest
"""

import runpy
from pathlib import Path

CODE_DIR = Path(__file__).resolve().parents[1]


def test_eval_matrix_has_six_configurations() -> None:
    namespace = runpy.run_path(str(CODE_DIR / "solutions" / "sol03-eval-matrix.py"))
    matrix = namespace["matrix"]

    assert len(matrix) == 6
    assert matrix[0]["model"] == "claude-haiku-5-5"
    assert matrix[0]["effort"] == "low"
    assert matrix[-1]["model"] == "claude-sonnet-5-5"
    assert matrix[-1]["effort"] == "high"


def test_eval_matrix_rows_contain_metric_fields() -> None:
    namespace = runpy.run_path(str(CODE_DIR / "solutions" / "sol03-eval-matrix.py"))

    for row in namespace["matrix"]:
        assert set(row) == {
            "model",
            "effort",
            "accuracy",
            "latency_ms",
            "cost_usd",
        }
