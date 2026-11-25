---
title: "Развёртывание и production"
lang: ru
track: claude
kind: module
doc: overview
slug: 17-production
part: 3
tags: [claude, deployment, production, retries, bedrock, vertex-ai, foundry]
versions: "Claude 5.5 family; anthropic 2.14.0; Claude Code 2.1.332"
levels: [1, 2, 3, 4]
status: draft
updated: 2026-11-25
---

# Развёртывание и production

🌐 **Русский** · [English](overview.en.md)

## Коротко

Как вывести приложение на Claude в production: выбрать **платформу** (Claude API, AWS, Google Cloud, Microsoft Foundry), настроить **повторы и запасные маршруты**, регионы и стоимость.

## Ключевые идеи

- Платформы различаются клиентом, ID модели и набором функций.
- SDK повторяет не всё: ошибку внутри потока обрабатываете вы.
- Отказ модели — не исключение, ему нужен отдельный путь.
- Привязка к региону обычно стоит дороже.
- Модель, платформу и таймауты держите в конфигурации и проверяйте eval.

## Минимальный пример

```python
from platforms import make_client

client, model = make_client("bedrock")
reply = client.messages.create(model=model, max_tokens=1024,
                               messages=[{"role": "user", "content": "hi"}])
```

## Когда применять и когда нет

Когда прототип становится сервисом с пользователями, бюджетом и требованиями к данным. Для разового скрипта достаточно клиента с настройками по умолчанию.

## Уровни материала

| Уровень | Файл | Добавить |
|---|---|---|
| 1. Базовая теория | [basics.ru.md](basics.ru.md) | |
| 2. Продвинутая теория | [advanced.ru.md](advanced.ru.md) | |
| 3. Экспертная теория | [expert.ru.md](expert.ru.md) | |
| 4. Самопроверка | [quiz.ru.md](quiz.ru.md), [exercises.ru.md](exercises.ru.md) | |
| 5. Запускаемые примеры | — | `/learn claude 17-production level 5` |
| 6. Тесты | — | `/learn claude 17-production level 6` |

## Связанные материалы

- [Трек Claude](../../overview.ru.md)
- [14 · Batch, evals и надёжность](../14-evals/overview.ru.md)
- [15 · Observability](../15-observability/overview.ru.md)
- [16 · Безопасность и compliance](../16-security/overview.ru.md)
- [Anthropic SDK](../../libraries/anthropic-sdk/overview.ru.md)
