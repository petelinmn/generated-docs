---
title: "Exercises: The Claude family and model selection"
lang: en
track: claude
kind: module
doc: exercises
slug: 01-models
part: 1
tags: [claude, model-selection, effort, evaluation]
status: verified
updated: 2026-11-23
---

# Exercises: The Claude family and model selection

🌐 [Русский](exercises.ru.md) · **English**

> Solve each task before opening the hints. Solutions are under spoilers.

**Levels:** [Overview](overview.en.md) · [1. Basics](basics.en.md) · [2. Advanced](advanced.en.md) · [3. Expert](expert.en.md) · [4. Exercises](exercises.en.md) · [5. Runnable examples](code/examples/) · [6. Tests](code/tests/)

Full level 5 solutions are available in [code/solutions/](code/solutions/).

## 1. 🟢 Classify a simple workload

**Task:** Write `suggest_model(workload)` to return an API ID for `classification`, `everyday`, `coding` and `demanding`.

**Expected result:**

```text
classification claude-haiku-5-5
everyday claude-sonnet-5-5
coding claude-opus-5-5
demanding claude-fable-5-2
unknown claude-opus-5-5
```

<details><summary>Hint</summary>

Use a dictionary and a default value with `dict.get`.

</details>

<details><summary>Solution</summary>

```python
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
```

The solution was verified by running it; it covers known types and a safe fallback.

</details>

## 2. 🟡 Build a routing plan

**Task:** For tasks with `cost_sensitive`, `latency_sensitive`, `complex` and `demanding` fields, return a model and effort. Use Haiku for a simple low-latency or cost-sensitive request, Opus for `complex`, Fable for `demanding`, and Sonnet for ordinary work.

**Expected result:** a dictionary of model and effort configurations. If multiple flags are set, `demanding` takes priority over `complex`, and both take priority over latency/cost.

<details><summary>Hint</summary>

Check priority features in a fixed order: demanding before complex, latency before everyday.

</details>

<details><summary>Solution</summary>

```python
def route(task: dict[str, bool]) -> dict[str, str]:
    if task.get("demanding"):
        return {"model": "claude-fable-5-2", "effort": "high"}
    if task.get("complex"):
        return {"model": "claude-opus-5-5", "effort": "medium"}
    if task.get("latency_sensitive") or task.get("cost_sensitive"):
        return {"model": "claude-haiku-5-5", "effort": "medium"}
    return {"model": "claude-sonnet-5-5", "effort": "high"}
```

This illustrates deterministic routing. In production, thresholds and confidence should come from an eval set.

</details>

## 3. 🟡 Build an eval matrix

**Task:** For two models and three effort levels, create result rows with `model`, `effort`, `accuracy`, `latency_ms` and `cost_usd`.

**Expected result:** six configurations in deterministic order.

<details><summary>Hint</summary>

Use nested iteration and fixed placeholder metrics. Do not insert real results until eval runs.

</details>

<details><summary>Solution</summary>

```python
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
```

Output:

```text
6 claude-haiku-5-5 low
```

</details>

## 4. 🔴 Design a migration checklist

**Task:** Write a configuration validator that requires `provider`, `model`, `effort` and `environment`. For a partner provider, `deployment` is also required.

**Expected result:** a `list[str]` of missing, empty or incorrectly typed required string values.

<details><summary>Hint</summary>

First verify required fields are non-empty strings, then add the provider-specific rule.

</details>

<details><summary>Solution</summary>

```python
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
```

Output:

```text
['missing or invalid model', 'missing or invalid deployment']
```

This validator prevents sending an empty or wrongly mapped deployment ID.

</details>
