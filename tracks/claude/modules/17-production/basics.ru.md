---
title: "Развёртывание и production: основы"
lang: ru
track: claude
kind: module
doc: basics
slug: 17-production
part: 3
tags: [claude, deployment, production, retries, bedrock, vertex-ai, foundry]
versions: "Claude 5.5 family; anthropic 2.14.0; Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# Развёртывание и production: основы

🌐 **Русский** · [English](basics.en.md)

**Уровни:** [overview](overview.ru.md) · **1. Основы** · [2. Продвинутый](advanced.ru.md) · [3. Эксперт](expert.ru.md) · [4. Квиз](quiz.ru.md)

**Пререквизиты:** [02 · Messages API](../02-messages-api/overview.ru.md), [14 · Batch, evals и надёжность](../14-evals/overview.ru.md)

## Как проверялся материал

Клиенты SDK `anthropic` 2.14.0 для всех платформ запускались против локального сервера с фиктивными ключами: проверены адреса, пути, тела запросов, повторы и ошибки. Claude Code 2.1.332 запускался в режимах Foundry, Bedrock (mantle) и Claude Platform on AWS против локальной имитации. **К настоящим облакам и API запросов не было**: доступность моделей, цены, квоты и регионы взяты из документации на 25 ноября 2026.

## Где можно запустить Claude

| Платформа | Кто управляет | Клиент Python | ID модели |
|---|---|---|---|
| Claude API | Anthropic | `Anthropic` | `claude-opus-5-5` |
| Claude Platform on AWS | Anthropic, оплата через AWS | `AnthropicAWS` | `claude-opus-5-5` |
| Claude in Amazon Bedrock | AWS | `AnthropicBedrockMantle` | `anthropic.claude-opus-5-5` |
| Google Cloud (Google managed, Vertex AI) | Google | `AnthropicVertex` | `claude-opus-5-5` |
| Google Cloud (Anthropic managed) | Anthropic | `AnthropicGoogleCloud` | `claude-opus-5-5` |
| Microsoft Foundry | Anthropic, на Azure или у себя | `AnthropicFoundry` | имя deployment |

Выбор обычно диктуют договор и инфраструктура: где уже есть биллинг и IAM, где должны обрабатываться данные, какие функции нужны.

## Один код, разные платформы

```python
# platforms.py
import os

import anthropic

MODEL = "claude-opus-5-5"


def make_client(platform=None, **options):
    """Return (client, model_id). Credentials and regions come from the environment."""
    platform = platform or os.environ.get("CLAUDE_PLATFORM", "anthropic")
    if platform == "anthropic":
        return anthropic.Anthropic(**options), MODEL
    if platform == "aws":                       # Claude Platform on AWS
        return anthropic.AnthropicAWS(**options), MODEL
    if platform == "bedrock":                   # Claude in Amazon Bedrock
        return anthropic.AnthropicBedrockMantle(**options), f"anthropic.{MODEL}"
    if platform == "vertex":                    # Claude on Google Cloud
        return anthropic.AnthropicVertex(**options), MODEL
    if platform == "foundry":                   # the request names your deployment
        return anthropic.AnthropicFoundry(**options), os.environ.get("CLAUDE_DEPLOYMENT", MODEL)
    raise ValueError(f"unknown platform: {platform}")
```

Расширения SDK: `pip install "anthropic[bedrock,vertex,aws]"`. Переменные окружения, которые клиенты прочитали при проверке:

| Клиент | Переменные |
|---|---|
| `AnthropicAWS` | `AWS_REGION`, `ANTHROPIC_AWS_WORKSPACE_ID`, `ANTHROPIC_AWS_API_KEY` или обычные учётные данные AWS |
| `AnthropicBedrockMantle` | `AWS_REGION`, `AWS_BEARER_TOKEN_BEDROCK` или учётные данные AWS |
| `AnthropicVertex` | `CLOUD_ML_REGION`, `ANTHROPIC_VERTEX_PROJECT_ID`, учётные данные Google |
| `AnthropicFoundry` | `ANTHROPIC_FOUNDRY_API_KEY`, `ANTHROPIC_FOUNDRY_RESOURCE` |

У клиентов AWS и Bedrock региона по умолчанию нет: без него конструктор бросает `AnthropicError`.

## Настройки клиента для production

| Настройка | По умолчанию | Что сделать |
|---|---|---|
| `max_retries` | 2 (три попытки) | согласовать с повторами вашей очереди |
| `timeout` | чтение 600 с, соединение 5 с | задать по SLA вызывающего |
| стриминг | выключен | включать для длинных ответов |
| ID модели | — | в конфигурации, а не в коде |

Нестриминговый запрос с `max_tokens=128000` SDK отклоняет сам (`ValueError`), не отправляя. Сохраняйте `request-id` каждого ответа (`response._request_id`) — он нужен поддержке; в Foundry добавьте `apim-request-id`.

## Перед запуском

- Ключи и роли с минимальными правами ([16 · Безопасность](../16-security/basics.ru.md)).
- Обработка ошибок, отказов и `max_tokens` ([14](../14-evals/advanced.ru.md)).
- Лог расхода и задержек, лимит расходов ([15](../15-observability/basics.ru.md)).
- Eval, который проходит на **той платформе и модели**, что в production.

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [16 · Безопасность](../16-security/basics.ru.md)

## Источники

- [Claude in Amazon Bedrock](https://platform.claude.com/docs/en/build-with-claude/claude-in-amazon-bedrock)
- [Claude Platform on AWS](https://platform.claude.com/docs/en/build-with-claude/claude-platform-on-aws)
- [Claude on Google Cloud](https://platform.claude.com/docs/en/build-with-claude/claude-on-vertex-ai)
- [Claude in Microsoft Foundry](https://platform.claude.com/docs/en/build-with-claude/claude-in-microsoft-foundry)
