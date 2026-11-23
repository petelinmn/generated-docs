---
title: "Упражнения: семейство Claude и выбор модели"
lang: ru
track: claude
kind: module
doc: exercises
slug: 01-models
part: 1
tags: [claude, model-selection, effort, evaluation]
status: verified
updated: 2026-11-23
---

# Упражнения: семейство Claude и выбор модели

🌐 **Русский** · [English](exercises.en.md)

> Решай сам, прежде чем открывать подсказки. Решения приведены под спойлерами.

**Уровни:** [обзор](overview.ru.md) · [1. Базовая теория](basics.ru.md) · [2. Продвинутая теория](advanced.ru.md) · [3. Экспертная теория](expert.ru.md) · [4. Упражнения](exercises.ru.md) · [5. Запускаемые примеры](code/examples/) · [6. Тесты](code/tests/)

Полные решения уровня 5 находятся в [code/solutions/](code/solutions/).

## 1. 🟢 Классификация простого workload

**Задача:** Напиши функцию `suggest_model(workload)`, которая возвращает API ID для типов `classification`, `everyday`, `coding` и `demanding`.

**Ожидаемый результат:**

```text
classification claude-haiku-5-5
everyday claude-sonnet-5-5
coding claude-opus-5-5
demanding claude-fable-5-2
unknown claude-opus-5-5
```

<details><summary>Подсказка</summary>

Используй словарь и значение по умолчанию через `dict.get`.

</details>

<details><summary>Решение</summary>

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

Решение проверено запуском; оно покрывает известные типы и безопасный fallback.

</details>

## 2. 🟡 Построй routing-план

**Задача:** Для задач с полями `cost_sensitive`, `latency_sensitive`, `complex` и `demanding` верни модель и effort. Для простого low-latency или cost-sensitive запроса используй Haiku; для `complex` — Opus; для `demanding` — Fable; для обычного — Sonnet.

**Ожидаемый результат:** словарь конфигураций с model и effort. Если задано несколько признаков, `demanding` имеет приоритет над `complex`, а оба — над latency/cost.

<details><summary>Подсказка</summary>

Проверяй признаки приоритета в фиксированном порядке: demanding перед complex, latency перед everyday.

</details>

<details><summary>Решение</summary>

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

Эта функция иллюстрирует deterministic routing. В production пороги и confidence должны основываться на eval-наборе.

</details>

## 3. 🟡 Сформируй eval-мatrix

**Задача:** Для двух моделей и трех effort-уровней создайте строки результата с полями `model`, `effort`, `accuracy`, `latency_ms` и `cost_usd`.

**Ожидаемый результат:** шесть конфигураций в детерминированном порядке.

<details><summary>Подсказка</summary>

Используй вложенный цикл и фиксированную метрику placeholders. Не подставляй реальные результаты до запуска eval.

</details>

<details><summary>Решение</summary>

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

Вывод при запуске:

```text
6 claude-haiku-5-5 low
```

</details>

## 4. 🔴 Спроектируй migration checklist

**Задача:** Напиши validator конфигурации, который проверяет обязательные поля `provider`, `model`, `effort` и `environment`. Для partner provider поле `deployment` также обязательно.

**Ожидаемый результат:** `list[str]` ошибок для отсутствующих, пустых или некорректно типизированных обязательных строковых значений.

<details><summary>Подсказка</summary>

Сначала проверь обязательные поля как непустые строки, затем добавь provider-specific правило.

</details>

<details><summary>Решение</summary>

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

Вывод при запуске:

```text
['missing or invalid model', 'missing or invalid deployment']
```

Такой validator предотвращает отправку пустого или неверно сопоставленного deployment ID.

</details>
