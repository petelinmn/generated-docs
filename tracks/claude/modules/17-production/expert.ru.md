---
title: "Развёртывание и production: экспертный уровень"
lang: ru
track: claude
kind: module
doc: expert
slug: 17-production
part: 3
tags: [claude, deployment, production, retries, bedrock, vertex-ai, foundry]
versions: "Claude 5.5 family; anthropic 2.14.0; Claude Code 2.1.332"
status: draft
updated: 2026-11-25
---

# Развёртывание и production: экспертный уровень

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [overview](overview.ru.md) · [1. Основы](basics.ru.md) · [2. Продвинутый](advanced.ru.md) · **3. Эксперт** · [4. Квиз](quiz.ru.md)

## Что уходит по сети

Проверено локальным сервером с `anthropic` 2.14.0:

| Клиент | Адрес по умолчанию | Путь | Авторизация |
|---|---|---|---|
| `Anthropic` | `api.anthropic.com` | `/v1/messages` | ключ API |
| `AnthropicAWS` | `aws-external-anthropic.REGION.api.aws` | `/v1/messages` + `anthropic-workspace-id` | SigV4 или ключ |
| `AnthropicBedrockMantle` | `bedrock-mantle.REGION.api.aws/anthropic/` | `/v1/messages` | SigV4 или bearer |
| `AnthropicBedrock` (прежняя интеграция) | `bedrock-runtime.REGION.amazonaws.com` | `/model/MODEL/invoke` | SigV4 или bearer |
| `AnthropicVertex`, global | `aiplatform.googleapis.com/v1/` | `…/publishers/anthropic/models/MODEL:rawPredict` | токен Google |
| `AnthropicVertex`, `us` / `eu` | `aiplatform.REGION.rep.googleapis.com/v1/` | тот же | токен Google |
| `AnthropicFoundry` | `RESOURCE.services.ai.azure.com/anthropic/` | `/v1/messages` | `x-api-key` / Entra ID |

В прежней Bedrock и в Vertex модель стоит в пути, а не в теле; версия API передаётся полем тела `anthropic_version`.

## Claude Code на облачных платформах

| Платформа | Включение | Модель в запросе (без настроек) |
|---|---|---|
| Foundry | `CLAUDE_CODE_USE_FOUNDRY=1` | `claude-sonnet-5-5` |
| Bedrock (mantle) | `CLAUDE_CODE_USE_MANTLE=1` | `anthropic.claude-opus-5-5` |
| Claude Platform on AWS | `CLAUDE_CODE_USE_ANTHROPIC_AWS=1` | `claude-opus-5-5` |
| Bedrock (runtime) | `CLAUDE_CODE_USE_BEDROCK=1` | — |
| Vertex AI | `CLAUDE_CODE_USE_VERTEX=1` | — |

Модель по умолчанию зависит от платформы, поэтому для команды фиксируйте её: `ANTHROPIC_DEFAULT_OPUS_MODEL`, `ANTHROPIC_DEFAULT_SONNET_MODEL`, `ANTHROPIC_DEFAULT_HAIKU_MODEL`. С именем deployment, которого Claude Code не знает, он предупреждает `unrecognized_model` и предполагает окно 1M. Строки с «—» не запускались.

## Fast mode

`speed: "fast"` — до 2.5 раз быстрее генерация за повышенную цену (Opus 5.5: $8 / $40 за миллион). Не работает с Batch API; смена скорости сбрасывает prompt cache; у fast mode свои лимиты. Что реально применилось, показывает `usage.speed`. В Foundry это отдельный deployment модели с суффиксом `-fast`; в Bedrock и Claude Platform on AWS его нет.

## Отличия платформ

- Claude Platform on AWS: отдельный пул мощности; нет Admin API для workspaces, HIPAA readiness, OAuth и fast mode.
- Bedrock: без URL-источников картинок и документов, server-side инструментов и `fallbacks`.
- Vertex AI: тело запроса ограничено 30 МБ против 128 МБ на Messages API у Claude API.
- Foundry: два вида размещения (на Azure и у Anthropic) с разным набором функций; неподдерживаемое на Azure даёт 400.
- Priority Tier больше не продаётся; действующие обязательства доживают свой срок.

## Подвохи

1. `hasattr(client.messages, "batches")` равно `True` даже там, где batch не поддерживается.
2. Spend-cap 429 повторяется SDK, хотя помочь повтор не может.
3. `except anthropic.OverloadedError` не поймает перегрузку внутри потока: там `APIStatusError` с кодом 200 и `type == "overloaded_error"`.
4. `OverloadedError` — наследник `InternalServerError`: порядок `except` имеет значение.
5. В Foundry в `model` передают имя deployment, а не обязательно ID модели.
6. Переменные `ANTHROPIC_*` из окружения разработчика меняют, куда и с чем идёт запрос; в CI проверяйте окружение.

> ⚠️ Проверить: доступность моделей и функций, регионы, квоты, цены, лимиты и сроки проверены только по документации; настоящие облака не вызывались.

## Компромиссы

| Вариант | Плюс | Минус |
|---|---|---|
| Claude API | все функции первыми | отдельный договор и биллинг |
| Облачная платформа | готовые IAM, биллинг, сети | часть функций отсутствует или позже |
| Несколько маршрутов | переживает сбой одного | двойные eval, холодный кэш |
| LLM gateway | единые ключи, учёт, лимиты | ещё один сервис в пути запроса |

## Вопросы с собеседований

1. Чем Claude Platform on AWS отличается от Bedrock?
<details><summary>Ответ</summary>

Первым управляет Anthropic, это тот же Claude API с оплатой через AWS; Bedrock — сервис AWS со своим расписанием функций и квотами.
</details>

2. Что меняется в коде при переносе на Bedrock?
<details><summary>Ответ</summary>

Класс клиента, префикс `anthropic.` у модели, аутентификация и отсутствующие функции.
</details>

3. Какие ошибки SDK не повторяет?
<details><summary>Ответ</summary>

Ошибки клиента вроде 400 и 401 и ошибки внутри уже начатого потока.
</details>

4. Почему опасны повторы на нескольких уровнях?
<details><summary>Ответ</summary>

Они перемножаются: три попытки SDK на три попытки очереди дают девять запросов.
</details>

5. Как отличить лимит трат от rate limit?
<details><summary>Ответ</summary>

На Messages API по `error.details.error_code` или классу `EnforcedSpendLimitReachedError`.
</details>

6. Когда нужен regional endpoint?
<details><summary>Ответ</summary>

Когда требования обязывают обрабатывать данные в регионе; это дороже на 10% и может сузить выбор моделей.
</details>

7. Что проверить перед переключением на запасной маршрут?
<details><summary>Ответ</summary>

Eval на нём, наличие функций, квоты, холодный кэш и кто платит.
</details>

8. Зачем сохранять `request-id`?
<details><summary>Ответ</summary>

Это идентификатор для поддержки и сопоставления логов.
</details>

9. Как безопасно сменить модель?
<details><summary>Ответ</summary>

Конфигурация, eval, доля трафика, метрики, откат.
</details>

10. Решает ли fallback отказ модели?
<details><summary>Ответ</summary>

Обычный перебор по исключениям — нет: отказ приходит с кодом 200. Нужна проверка `stop_reason`, middleware или `fallbacks`.
</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md) · [Квиз](quiz.ru.md)
- [09 · Claude Code](../09-claude-code/expert.ru.md) · [16 · Безопасность](../16-security/expert.ru.md)

## Источники

- [Fast mode](https://platform.claude.com/docs/en/build-with-claude/fast-mode)
- [Claude Code third-party integrations](https://code.claude.com/docs/en/third-party-integrations)
- [Claude Code model configuration](https://code.claude.com/docs/en/model-config)
- [Service tiers](https://platform.claude.com/docs/en/api/service-tiers)
