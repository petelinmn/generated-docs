"""Tests: Claude partner deployment configuration validation.

Module: claude/01-models (level 6)
Run: uv run pytest
"""

import runpy
from pathlib import Path

CODE_DIR = Path(__file__).resolve().parents[1]

VALIDATOR_PATH = CODE_DIR / "solutions" / "sol04-config-validator.py"
validate_config = runpy.run_path(str(VALIDATOR_PATH))["validate_config"]


def test_valid_claude_api_config_has_no_errors() -> None:
    config = {
        "provider": "claude-api",
        "model": "claude-opus-5-5",
        "effort": "medium",
        "environment": "prod",
    }

    assert validate_config(config) == []


def test_partner_config_requires_deployment() -> None:
    config = {
        "provider": "microsoft-foundry",
        "model": "claude-opus-5-5",
        "effort": "medium",
        "environment": "prod",
    }

    assert validate_config(config) == ["missing or invalid deployment"]


def test_missing_core_fields_are_reported() -> None:
    assert validate_config({}) == [
        "missing or invalid provider",
        "missing or invalid model",
        "missing or invalid effort",
        "missing or invalid environment",
    ]


def test_non_string_and_blank_values_are_rejected() -> None:
    config = {
        "provider": "microsoft-foundry",
        "model": 42,
        "effort": "medium",
        "environment": "prod",
        "deployment": "  ",
    }

    assert validate_config(config) == [
        "missing or invalid model",
        "missing or invalid deployment",
    ]


def test_non_string_provider_is_reported_without_type_error() -> None:
    config = {
        "provider": ["microsoft-foundry"],
        "model": "claude-opus-5-5",
        "effort": "medium",
        "environment": "prod",
    }

    assert validate_config(config) == ["missing or invalid provider"]
