"""Solution: validate Claude model deployment configuration.

Module: claude/01-models (level 5)
Run: uv run python solutions/sol04-config-validator.py
"""


def validate_config(config: dict[str, object]) -> list[str]:
    partner_providers = {"bedrock", "microsoft-foundry", "google-managed"}
    errors = []
    for field in ("provider", "model", "effort", "environment"):
        value = config.get(field)
        if not isinstance(value, str) or not value.strip():
            errors.append(f"missing or invalid {field}")

    provider = config.get("provider")
    deployment = config.get("deployment")
    if isinstance(provider, str) and provider in partner_providers and (
        not isinstance(deployment, str) or not deployment.strip()
    ):
        errors.append("missing or invalid deployment")
    return errors


config = {
    "provider": "microsoft-foundry",
    "model": "",
    "effort": "medium",
    "environment": "prod",
}
print(validate_config(config))
